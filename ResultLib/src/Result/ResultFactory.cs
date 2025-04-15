// ReSharper disable CheckNamespace
// ReSharper disable ArrangeModifiersOrder
// ReSharper disable MemberCanBePrivate.Global

using System;

namespace ResultLib {
    static public class ResultFactory {
        static public Result Create(object value) => Result.Create(value);
        static public Result Ok() => Result.Ok();
        static public Result Ok(object value) => Result.Ok(value);

        static public Result Error() => Result.Error();
        static public Result Error(string error) => Result.Error(error);
        static public Result Error(Exception exception) => Result.Error(exception);


        static public Result<T> Create<T>(T value) => Result<T>.Create(value);
        static public Result<T> Ok<T>() => Result<T>.Ok();
        static public Result<T> Ok<T>(T value) => Result<T>.Ok(value);

        static public Result<T> Error<T>() => Result<T>.Error();
        static public Result<T> Error<T>(string error) => Result<T>.Error(error);
        static public Result<T> Error<T>(Exception exception) => Result<T>.Error(exception);
    }
}
