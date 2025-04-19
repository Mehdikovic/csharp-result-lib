// ReSharper disable CheckNamespace

using System;

namespace ResultLib {
    static public class ResultFactory {
        static public Result FromRequired(object value) => Result.FromRequired(value);
        static public Result Ok() => Result.Ok();
        static public Result Ok(object value) => Result.Ok(value);

        static public Result Error() => Result.Error();
        static public Result Error(string error) => Result.Error(error);


        static public Result<T> FromRequired<T>(T value) => Result<T>.FromRequired(value);
        static public Result<T> Ok<T>() => Result<T>.Ok();
        static public Result<T> Ok<T>(T value) => Result<T>.Ok(value);

        static public Result<T> Error<T>() => Result<T>.Error();
        static public Result<T> Error<T>(string error) => Result<T>.Error(error);
    }
}