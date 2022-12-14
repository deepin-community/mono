using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mono.Linker.Steps {
	public class CodeRewriterStep : BaseStep
	{
		AssemblyDefinition assembly;

		protected override void ProcessAssembly (AssemblyDefinition assembly)
		{
			if (Annotations.GetAction (assembly) != AssemblyAction.Link)
				return;

			this.assembly = assembly;

			foreach (var type in assembly.MainModule.Types)
				ProcessType (type);
		}

		void ProcessType (TypeDefinition type)
		{
			foreach (var method in type.Methods) {
				if (method.HasBody)
					ProcessMethod (method);
			}

			foreach (var nested in type.NestedTypes)
				ProcessType (nested);
		}

		void ProcessMethod (MethodDefinition method)
		{
			switch (Annotations.GetAction (method)) {
			case MethodAction.ConvertToStub:
				RewriteBodyToStub (method);
				break;
			case MethodAction.ConvertToThrow:
				RewriteBodyToLinkedAway (method);
				break;
			case MethodAction.ConvertToFalse:
				RewriteBodyToFalse (method);
				break;
			}
		}

		protected virtual void RewriteBodyToLinkedAway (MethodDefinition method)
		{
			method.ImplAttributes &= ~(MethodImplAttributes.AggressiveInlining | MethodImplAttributes.Synchronized);
			method.ImplAttributes |= MethodImplAttributes.NoInlining;

			method.Body = CreateThrowLinkedAwayBody (method);

			method.ClearDebugInformation();
		}

		protected virtual void RewriteBodyToStub (MethodDefinition method)
		{
			if (!method.IsIL)
				throw new NotImplementedException ();

			method.Body = CreateStubBody (method);

			method.ClearDebugInformation();
		}

		protected virtual void RewriteBodyToFalse (MethodDefinition method)
		{
			if (!method.IsIL)
				throw new NotImplementedException ();

			method.Body = CreateReturnFalseBody (method);

			method.ClearDebugInformation();
		}

		MethodBody CreateThrowLinkedAwayBody (MethodDefinition method)
		{
			var body = new MethodBody (method);
			var il = body.GetILProcessor ();

			// import the method into the current assembly
			MethodReference ctor = Context.MarkedKnownMembers.NotSupportedExceptionCtorString;
			ctor = assembly.MainModule.ImportReference (ctor);

			il.Emit (OpCodes.Ldstr, "Linked away");
			il.Emit (OpCodes.Newobj, ctor);
			il.Emit (OpCodes.Throw);
			return body;
		}

		MethodBody CreateStubBody (MethodDefinition method)
		{
			var body = new MethodBody (method);

			if (method.HasParameters && method.Parameters.Any (l => l.IsOut))
				throw new NotImplementedException ();

			var il = body.GetILProcessor ();
			if (method.IsInstanceConstructor ()) {
				var base_ctor = method.DeclaringType.BaseType.GetDefaultInstanceConstructor();
				base_ctor = assembly.MainModule.ImportReference (base_ctor);

				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Call, base_ctor);
			}

			switch (method.ReturnType.MetadataType) {
			case MetadataType.Void:
				break;
			case MetadataType.Boolean:
				il.Emit (OpCodes.Ldc_I4_0);
				break;
			default:
				throw new NotImplementedException (method.ReturnType.FullName);
			}

			il.Emit (OpCodes.Ret);
			return body;
		}

		MethodBody CreateReturnFalseBody (MethodDefinition method)
		{
			if (method.ReturnType.MetadataType != MetadataType.Boolean)
				throw new NotImplementedException ();

			var body = new MethodBody (method);
			var il = body.GetILProcessor ();
			il.Emit (OpCodes.Ldc_I4_0);
			il.Emit (OpCodes.Ret);
			return body;
		}
	}
}
