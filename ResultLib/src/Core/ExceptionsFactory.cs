// ReSharper disable CheckNamespace
// ReSharper disable ArrangeModifiersOrder
// ReSharper disable MemberCanBePrivate.Global

using System;

namespace ResultLib.Core
{
    internal static class Exceptions
    {
        internal static class Option
        {
            static internal readonly Func<Exception> Default = 
                () => new Exception("Option:: Something went wrong.");
        
            static internal readonly Func<string, Exception> Create = 
                (error) => new Exception($"Option:: {error ?? string.Empty}");
        
            static internal readonly Func<OperationCanceledException> Cancel = 
                () => new OperationCanceledException("Option:: Operation cancelled.");
            
            static internal readonly Func<NullReferenceException> NullUnwrapErr =
                () => new NullReferenceException("Option:: does not have an Exception when calling UnwrapErr.");
            
            static internal readonly Func<NullReferenceException> InvalidIsOkCastOperation =
                () => new NullReferenceException("Option:: Option cannot hold a null value when IsOk is true.");
        
            static internal readonly Func<Type, Type, InvalidCastException> InvalidImplicitUnboxingCast = 
                (from, to) => new InvalidCastException($"Option:: Internal Error:: value could not be cast from {from.Name} to {to.Name}, possibility of losing data in implicit conversion.");
        
            static internal readonly Func<Type, Type, InvalidCastException> InvalidExplicitUnboxingCast = 
                (from, to) => new InvalidCastException($"Option:: Internal Error:: value could not be cast from {from.Name} to {to.Name}, possibility of losing data in explicit conversion.");
        }
        
        internal static class Result
        {
            static internal readonly Func<Exception> Default =
                () => new Exception("Result:: Something went wrong.");
        
            static internal readonly Func<string, Exception> Create =
                (error) => new Exception($"Result:: {error ?? string.Empty}");
            
            static internal readonly Func<Exception> InvalidCreation =
                () => new Exception($"Result:: object value could not be null");
            
            static internal readonly Func<NullReferenceException> InvalidNullSome =
                () => new NullReferenceException("Result:: Some method must return a value which is not null");
            
            static internal readonly Func<InvalidOperationException> InvalidOperationUnwrap =
                () => new InvalidOperationException("Result:: can not unwrap Result with State of [Error]");
            
            static internal readonly Func<InvalidOperationException> InvalidOperationUnwrapErr =
                () => new InvalidOperationException("Result:: can not unwrap Result with State of [Ok]");
            
            static internal readonly Func<InvalidOperationException> InvalidOperationMatch =
                () => new InvalidOperationException("Result:: state is not recognized. Should be [Ok] or [Error]");
            
            static internal readonly Func<Type, Type, InvalidCastException> InvalidImplicitUnboxingCast =
                (from, to) => new InvalidCastException($"Result:: Internal Error:: Result value could not be cast from {from.Name} to {to.Name}, possibility of losing data in implicit conversion.");
            
            static internal readonly Func<Type, Type, InvalidCastException> InvalidExplicitUnboxingCast =
                (from, to) => new InvalidCastException($"Result:: Internal Error:: Result value could not be cast from {from.Name} to {to.Name}, possibility of losing data in explicit conversion.");
            
            static internal readonly Func<Type, InvalidCastException> InvalidBoxingCast = 
                (from) => new InvalidCastException($"Result:: Cannot create Result from Result<T>. T: {from.Name}");
        }
    }
}