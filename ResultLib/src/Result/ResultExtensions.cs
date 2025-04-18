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

            throw new Exception(ErrorFactory.Result.CreateExplicitUnboxingCast(result.Unwrap().GetType(), typeof(T)));
        }
    }
}
