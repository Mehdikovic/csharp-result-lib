// ReSharper disable CheckNamespace
// ReSharper disable ArrangeModifiersOrder
// ReSharper disable MemberCanBePrivate.Global

using System;

namespace ResultLib.Core {
    internal static class ErrorFactory {
        internal static class Option {
            static internal readonly Func<Exception> Default =
                () => new Exception("Option:: Something went wrong.");

            static internal readonly Func<string, Exception> Create =
                (error) => new Exception($"Option:: {error ?? string.Empty}");

            static internal readonly Func<InvalidOperationException> InvalidStateWhenGettingError =
                () => new InvalidOperationException("Option:: state is invalid. Must be [Success] | [Failed] | [Canceled]");

            static internal readonly Func<OperationCanceledException> Cancel =
                () => new OperationCanceledException("Option:: Operation cancelled.");

            static internal readonly Func<NullReferenceException> NullUnwrapErr =
                () => new NullReferenceException("Option:: does not have an Exception when calling UnwrapErr in state [Success].");

            static internal readonly Func<InvalidOperationException> InvalidOperationMatch =
                () => new InvalidOperationException("Option:: state is not recognized. Should be [Success], [Failed] or [Canceled]");

            static internal readonly Func<NullReferenceException> InvalidIsOkCastOperation =
                () => new NullReferenceException("Option:: Option cannot hold a null value when IsOk is true.");

            static internal readonly Func<Type, Type, InvalidCastException> InvalidImplicitUnboxingCast =
                (from, to) => new InvalidCastException($"Option:: Internal Error:: value could not be cast from {from.Name} to {to.Name}, possibility of losing data in implicit conversion.");

            static internal readonly Func<Type, Type, InvalidCastException> InvalidExplicitUnboxingCast =
                (from, to) => new InvalidCastException($"Option:: Internal Error:: value could not be cast from {from.Name} to {to.Name}, possibility of losing data in explicit conversion.");
        }

        internal static class Result {
            internal const string Default = "Result:: Something went wrong.";
            internal const string EmptyConstructor = "Result:: Must be instantiated with Static Methods or Factory.";
            internal const string AttemptToCreateOk = "Result:: object value could not be null";
        }
    }

    public class ResultException(string message) : Exception(message) {}
    public class ResultDefaultConstructorException() : ResultException(ErrorFactory.Result.EmptyConstructor);
    
    public class ResultUnwrapException() : ResultException("Result:: can not unwrap Result with State of [Error]");
    public class ResultUnwrapErrorException() : ResultException("Result:: can not unwrap Result with State of [Ok]");
    public class ResultInvalidSomeOperationException() : ResultException("Result:: Some method must return a value which is not null");
    public class ResultInvalidMatchException() : ResultException("Result:: state is not recognized. Should be [Ok] or [Error]");
    public class ResultInvalidForwardException() : ResultException("Result:: ForwardError is only available for state [Error] to replicate Result.Error() with the error message.");
    public class ResultInvalidBoxingCastException(Type type) 
        : ResultException("Result:: Cannot create Result from Result<T>. T: {0}".Format(type.Name));
    public class ResultInvalidImplicitCastException(Type from, Type to) 
        : ResultException("Result:: Internal Error:: Result value could not be cast from {0} to {1}, possibility of losing data in implicit conversion".Format(from.Name, to.Name));
    public class ResultInvalidExplicitCastException(Type from, Type to)
        : ResultException("Result:: Internal Error:: Result value could not be cast from {0} to {1}, possibility of losing data in explicit conversion.".Format(from.Name, to.Name));
}
