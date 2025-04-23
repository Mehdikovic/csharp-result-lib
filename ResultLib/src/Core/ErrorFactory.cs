// ReSharper disable CheckNamespace
// ReSharper disable ArrangeModifiersOrder
// ReSharper disable MemberCanBePrivate.Global

using System;

namespace ResultLib.Core {
    internal static class ErrorFactory {
        internal static class Result {
            internal const string Default = "Result:: Something went wrong.";
            internal const string EmptyConstructor = "Result:: Must be instantiated with Static Methods or Factory.";
            internal const string AttemptToCreateOk = "Result:: object value could not be null when called FromRequired";
        }
    }

    public class ResultException : Exception {
        public ResultException(string message) : base(message) { }
        public ResultException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class ResultDefaultConstructorException : ResultException {
        public ResultDefaultConstructorException() : base(ErrorFactory.Result.EmptyConstructor) { }
    };

    public class ResultUnwrapException : ResultException {
        public ResultUnwrapException() : base("Result:: can not unwrap Result with State of [Error]") { }
    };

    public class ResultUnwrapErrorException : ResultException {
        public ResultUnwrapErrorException() : base("Result:: can not unwrap error with State of [Ok]") { }
    };

    public class ResultInvalidSomeOperationException : ResultException {
        public ResultInvalidSomeOperationException() : base("Result:: Some must return a value which is not null and strongly typed") { }
    };

    public class ResultInvalidMatchException : ResultException {
        public ResultInvalidMatchException() : base("Result:: state is not recognized. Should be [Ok] or [Error]") { }
    };

    public class ResultInvalidForwardException : ResultException {
        public ResultInvalidForwardException() : base("Result:: ForwardError is only available for state [Error] to replicate Result.Error() with the error message.") { }
    };

    public class ResultInvalidBoxingCastException : ResultException {
        public ResultInvalidBoxingCastException(Type type) : base("Result:: Cannot create Result from Result<T>. T: {0}".Format(type.Name)) { }
    }

    public class ResultInvalidImplicitCastException : ResultException {
        public ResultInvalidImplicitCastException(Type from, Type to) : base(
            "Result:: Internal Error:: Result value could not be cast from {0} to {1}, possibility of losing data in implicit conversion".Format(from.Name, to.Name)
        ) { }
    }

    public class ResultInvalidExplicitCastException : ResultException {
        public ResultInvalidExplicitCastException(Type from, Type to) : base(
            "Result:: Internal Error:: Result value could not be cast from {0} to {1}, possibility of losing data in explicit conversion.".Format(from.Name, to.Name)
        ) { }
    }
    
    
    // Options
    
    public class OptionException : Exception {
        public OptionException() : base("Result:: Something went wrong.") { }
        public OptionException(string message) : base(message.IsEmpty() ? "Result:: Something went wrong." : $"Option:: {message}") { }
    }

    public class OptionInvalidNullCastException : Exception {
        public OptionInvalidNullCastException() : base("Option:: Option cannot hold a null value when IsOk is true.") { }
    }

    public class OptionInvalidExplicitCastException : Exception {
        public OptionInvalidExplicitCastException(Type from, Type to) : base(
            $"Option:: Internal Error:: value could not be cast from {from.Name} to {to.Name}, possibility of losing data in explicit conversion."
        ) { }
    }

    public class OptionInvalidStateException : Exception {
        public OptionInvalidStateException() : base("Option:: state is not recognized. Should be [Success], [Failed] or [Canceled]") { }
    }
    
    public class OptionInvalidOperationException : Exception {
        public OptionInvalidOperationException() : base("Option:: state [Success] cannot have an Exception when calling GetError.") { }
    }

    public class OptionOperationCanceledException : Exception {
        public OptionOperationCanceledException() : base("Option:: Operation cancelled.") { }
    }
}
