﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    /// <summary>
    /// Represents a .NET event invocation consisting of the strongly typed object that raised the event and the data that was generated by the event.
    /// </summary>
    /// <typeparam name="TSender">
    /// The type of the sender that raised the event.
    /// This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.
    /// </typeparam>
    /// <typeparam name="TEventArgs">
    /// The type of the event data generated by the event.
    /// This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.
    /// </typeparam>
    public interface IEventPattern<
#if !NO_VARIANCE
        out TSender, out TEventArgs
#else
        TSender, TEventArgs
#endif
    >
#if !NO_EVENTARGS_CONSTRAINT
        where TEventArgs : EventArgs
#endif
    {
        /// <summary>
        /// Gets the sender object that raised the event.
        /// </summary>
        TSender Sender { get; }

        /// <summary>
        /// Gets the event data that was generated by the event.
        /// </summary>
        TEventArgs EventArgs { get; }
    }
}
