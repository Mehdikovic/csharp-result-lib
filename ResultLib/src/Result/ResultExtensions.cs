// ReSharper disable CheckNamespace
// ReSharper disable InvertIf
// ReSharper disable ConvertIfStatementToSwitchStatement

using ResultLib.Core;

namespace ResultLib {
    static public class ResultExtensions {
        static public Result<T> ToResult<T>(this Result result) {
            if (result.IsError(out var exception)) return Result<T>.Error(exception);

            if (result.IsOk(out object obj)) {
                if (obj is null) return Result<T>.Ok();
                if (obj is T value) return Result<T>.Ok(value);
            }

            throw ErrorFactory.Result.InvalidExplicitUnboxingCast(result.Unwrap().GetType(), typeof(T));
        }
    }
}
