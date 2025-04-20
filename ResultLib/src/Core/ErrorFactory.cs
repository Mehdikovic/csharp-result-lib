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
            private const string ImplicitUnboxingCast = "Result:: Internal Error:: Result value could not be cast from {0} to {1}, possibility of losing data in implicit conversion";
            private const string ExplicitUnboxingCast = "Result:: Internal Error:: Result value could not be cast from {0} to {1}, possibility of losing data in explicit conversion.";
            private const string BoxingCast = "Result:: Cannot create Result from Result<T>. T: {0}";

            internal const string Default = "Result:: Something went wrong.";
            internal const string EmptyConstructor = "Result:: Must be instantiated with Static Methods or Factory.";
            internal const string AttemptToCreateOk = "Result:: object value could not be null";
            internal const string AttemptToForwardError = "Result:: ForwardError is only available for state [Error] to replicate Result.Error() with the error message.";
            internal const string SomeReturnNull = "Result:: Some method must return a value which is not null";
            internal const string OperationUnwrapWhenError = "Result:: can not unwrap Result with State of [Error]";
            internal const string OperationUnwrapErrWhenOk = "Result:: can not unwrap Result with State of [Ok]";
            internal const string OperationMatch ="Result:: state is not recognized. Should be [Ok] or [Error]";


            static internal readonly Func<Type, Type, string> CreateImplicitUnboxingCast = (from, to) => ImplicitUnboxingCast.Format(from.Name, to.Name);
            static internal readonly Func<Type, Type, string> CreateExplicitUnboxingCast = (from, to) => ExplicitUnboxingCast.Format(from.Name, to.Name);
            static internal readonly Func<Type, string> CreateBoxingCast = (from) => BoxingCast.Format(from.Name);
        }
    }
}
