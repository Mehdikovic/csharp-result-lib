// ReSharper disable CheckNamespace
// ReSharper disable ArrangeModifiersOrder
// ReSharper disable MemberCanBePrivate.Global

using System;

namespace ResultLib {
    static public class OptionFactory {
        // Option {T}
        static public Option Success() => Option.Success();
        static public Option Success(object value) => Option.Success(value);

        static public Option Failed() => Option.Failed();
        static public Option Failed(string error) => Option.Failed(error);
        static public Option Failed(string error, object value) => Option.Failed(error, value);
        static public Option Failed(Exception exception) => Option.Failed(exception);
        static public Option Failed(Exception exception, object value) => Option.Failed(exception, value);

        static public Option Canceled() => Option.Canceled();
        static public Option Canceled(object value) => Option.Canceled(value);

        static public Option<T> Success<T>() => Option<T>.Success();
        static public Option<T> Success<T>(T value) => Option<T>.Success(value);

        static public Option<T> Failed<T>() => Option<T>.Failed();
        static public Option<T> Failed<T>(string error) => Option<T>.Failed(error);
        static public Option<T> Failed<T>(string error, T value) => Option<T>.Failed(error, value);
        static public Option<T> Failed<T>(Exception exception) => Option<T>.Failed(exception);
        static public Option<T> Failed<T>(Exception exception, T value) => Option<T>.Failed(exception, value);

        static public Option<T> Canceled<T>() => Option<T>.Canceled();
        static public Option<T> Canceled<T>(T value) => Option<T>.Canceled(value);

        // Option {T1, T2}
        static public Option<TSuccess, TError> Success<TSuccess, TError>() => Option<TSuccess, TError>.Success();
        static public Option<TSuccess, TError> Success<TSuccess, TError>(TSuccess value) => Option<TSuccess, TError>.Success(value);

        static public Option<TSuccess, TError> Failed<TSuccess, TError>() => Option<TSuccess, TError>.Failed();
        static public Option<TSuccess, TError> Failed<TSuccess, TError>(string error) => Option<TSuccess, TError>.Failed(error);
        static public Option<TSuccess, TError> Failed<TSuccess, TError>(string error, TError value) => Option<TSuccess, TError>.Failed(error, value);

        static public Option<TSuccess, TError> Failed<TSuccess, TError>(Exception exception) => Option<TSuccess, TError>.Failed(exception);
        static public Option<TSuccess, TError> Failed<TSuccess, TError>(Exception exception, TError value) => Option<TSuccess, TError>.Failed(exception, value);

        static public Option<TSuccess, TError> Canceled<TSuccess, TError>() => Option<TSuccess, TError>.Canceled();
        static public Option<TSuccess, TError> Canceled<TSuccess, TError>(TError value) => Option<TSuccess, TError>.Canceled(value);


        // Option {T1, T2, T3}
        static public Option<TSuccess, TFailed, TCanceled> Success<TSuccess, TFailed, TCanceled>() =>
            Option<TSuccess, TFailed, TCanceled>.Success();
        static public Option<TSuccess, TFailed, TCanceled> Success<TSuccess, TFailed, TCanceled>(TSuccess value) =>
            Option<TSuccess, TFailed, TCanceled>.Success(value);

        static public Option<TSuccess, TFailed, TCanceled> Failed<TSuccess, TFailed, TCanceled>() =>
            Option<TSuccess, TFailed, TCanceled>.Failed();
        static public Option<TSuccess, TFailed, TCanceled> Failed<TSuccess, TFailed, TCanceled>(string error) =>
            Option<TSuccess, TFailed, TCanceled>.Failed(error);
        static public Option<TSuccess, TFailed, TCanceled> Failed<TSuccess, TFailed, TCanceled>(string error, TFailed value) =>
            Option<TSuccess, TFailed, TCanceled>.Failed(error, value);

        static public Option<TSuccess, TFailed, TCanceled> Failed<TSuccess, TFailed, TCanceled>(Exception exception) =>
            Option<TSuccess, TFailed, TCanceled>.Failed(exception);
        static public Option<TSuccess, TFailed, TCanceled> Failed<TSuccess, TFailed, TCanceled>(Exception exception, TFailed value) =>
            Option<TSuccess, TFailed, TCanceled>.Failed(exception, value);

        static public Option<TSuccess, TFailed, TCanceled> Canceled<TSuccess, TFailed, TCanceled>() =>
            Option<TSuccess, TFailed, TCanceled>.Canceled();
        static public Option<TSuccess, TFailed, TCanceled> Canceled<TSuccess, TFailed, TCanceled>(TCanceled value) =>
            Option<TSuccess, TFailed, TCanceled>.Canceled(value);
    }
}
