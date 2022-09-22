// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Runtime.CompilerServices
{
    // When applied to a type this custom attribute cause the type to be treated as reflection blocked.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Delegate | AttributeTargets.Enum | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    [DependencyReductionRoot]
#if !MONO
    public
#endif
    class ReflectionBlockedAttribute : Attribute
    {
    }
}