// ReSharper disable CheckNamespace
// ReSharper disable InvertIf
// ReSharper disable ConvertIfStatementToSwitchStatement

using System;

using ResultLib.Core;

namespace ResultLib {
    static public class ResultExtensions {
        static public Result<T> ToResult<T>(this Result result) {
            if (result.IsError(out string error)) return Result<T>.Error(error);

            if (result.IsOk(out object obj)) {
                if (obj is null) return Result<T>.Ok();
                if (obj is T value) return Result<T>.Ok(value);
            }

            throw new ResultInvalidExplicitCastException(result.Unwrap().GetType(), typeof(T));
        }

        static public Result ForwardError(this Result result) {
            if (result.IsError(out string error)) return Result.Error(error);
            throw new ResultInvalidForwardException();
        }
        
        static public Result<TTo> ForwardError<TTo>(this Result result) {
            if (result.IsError(out string error)) return Result<TTo>.Error(error);
            throw new ResultInvalidForwardException();
        }

        static public Result ForwardError<TFrom>(this Result<TFrom> result) {
            if (result.IsError(out string error)) return Result.Error(error);
            throw new ResultInvalidForwardException();
        }

        static public Result<TTo> ForwardError<TFrom, TTo>(this Result<TFrom> result) {
            if (result.IsError(out string error)) return Result<TTo>.Error(error);
            throw new ResultInvalidForwardException();
        }
    }
}
