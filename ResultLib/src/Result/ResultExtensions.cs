// ReSharper disable CheckNamespace
// ReSharper disable InvertIf
// ReSharper disable ConvertIfStatementToSwitchStatement

using ResultLib.Core;

namespace ResultLib {
    static public class ResultExtensions {
        static public Result<T> ToResult<T>(this Result result) {
            if (result.IsError(out string error)) {
                return result.HasInnerException(out var innerException) ? Result<T>.Error(error, innerException) : Result<T>.Error(error);
            }

            if (result.IsOk(out object obj)) {
                if (obj is null) return Result<T>.Ok();
                if (obj is T value) return Result<T>.Ok(value);
            }

            throw new ResultInvalidExplicitCastException(result.Unwrap().GetType(), typeof(T));
        }

        static public Result ForwardError(this Result result) {
            if (result.IsError(out string error)) {
                return result.HasInnerException(out var innerException) ? Result.Error(error, innerException) : Result.Error(error);
            }
            throw new ResultInvalidForwardException();
        }

        static public Result<T> ForwardError<T>(this Result result) {
            if (result.IsError(out string error)) {
                return result.HasInnerException(out var innerException) ? Result<T>.Error(error, innerException) : Result<T>.Error(error);
            }
            throw new ResultInvalidForwardException();
        }

        static public Result<T> ForwardError<T>(this Result<T> result) {
            if (result.IsError(out string error)) {
                return result.HasInnerException(out var innerException) ? Result<T>.Error(error, innerException) : Result<T>.Error(error);
            }
            throw new ResultInvalidForwardException();
        }

        static public Result<TTo> ForwardError<TFrom, TTo>(this Result<TFrom> result) {
            if (result.IsError(out string error)) {
                return result.HasInnerException(out var innerException) ? Result<TTo>.Error(error, innerException) : Result<TTo>.Error(error);
            }
            throw new ResultInvalidForwardException();
        }
    }
}
