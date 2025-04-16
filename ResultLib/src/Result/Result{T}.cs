// ReSharper disable CheckNamespace
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable InvertIf

using System;
using System.Collections.Generic;

using static System.ArgumentNullException;

using ResultLib.Core;

namespace ResultLib {
    public interface IResult<out T> {
        public bool IsOk();
        public bool IsError();
        public T Unwrap();
        public Exception UnwrapErr();
    }

    public struct Result<T>() : IResult<T>, IEquatable<Result<T>>, IComparable<Result<T>> {
        private ResultState _state = ResultState.Error;
        private Exception _error = ErrorFactory.Result.EmptyConstructor();
        private T _value = default;

        static public Result<T> Ok() =>
            new Result<T> { _state = ResultState.Ok };

        static public Result<T> Ok(T value) =>
            new Result<T> { _state = ResultState.Ok, _value = value };

        static public Result<T> Error() =>
            new Result<T> { _state = ResultState.Error, _error = ErrorFactory.Result.Default() };

        static public Result<T> Error(string error) =>
            new Result<T> { _state = ResultState.Error, _error = ErrorFactory.Result.Create(error) };

        static public Result<T> Error(Exception exception) =>
            new Result<T> { _state = ResultState.Error, _error = exception ?? ErrorFactory.Result.Default() };

        static public Result<T> Create(T value) =>
            value == null ? Error(ErrorFactory.Result.InvalidAttemptToCreateOk()) : Ok(value);

        public bool IsOk() => _state == ResultState.Ok;

        public bool IsOk(out T value) {
            if (_state == ResultState.Ok) {
                value = _value;
                return true;
            }

            value = default;
            return false;
        }

        public bool IsError() => _state == ResultState.Error;

        public bool IsError(out Exception exception) {
            if (_state == ResultState.Error) {
                exception = _error;
                return true;
            }

            exception = null;
            return false;
        }

        public T Unwrap() => IsOk() ? _value : throw ErrorFactory.Result.InvalidOperationUnwrapWhenError();
        public T Unwrap(T defaultValue) => IsOk() ? _value : defaultValue;
        public T Unwrap(Func<T> func) => IsOk() ? _value : func.Invoke();

        public bool Some(out T value) => IsOk(out value) && value != null;

        public T Some(T defaultValue) {
            ThrowIfNull(defaultValue);

            return IsOk(out var value) && value != null
                ? value
                : defaultValue;
        }

        public T Some(Func<T> func) {
            ThrowIfNull(func);

            return IsOk(out var value) && value != null
                ? value
                : (func.Invoke() ?? throw ErrorFactory.Result.InvalidNullSome());
        }

        public Exception UnwrapErr() => IsError() ? _error : throw ErrorFactory.Result.InvalidOperationUnwrapErrWhenOk();
        public void ThrowIfError() {
            if (IsError()) throw _error;
        }

        public TRet Match<TRet>(Func<T, TRet> onOk, Func<Exception, TRet> onError) {
            ThrowIfNull(onOk);
            ThrowIfNull(onError);

            return _state switch {
                ResultState.Ok => onOk.Invoke(Unwrap()),
                ResultState.Error => onError.Invoke(UnwrapErr()),
                _ => throw ErrorFactory.Result.InvalidOperationMatch()
            };
        }

        public void Match(Action<T> onOk, Action<Exception> onError) {
            ThrowIfNull(onOk);
            ThrowIfNull(onError);

            switch (_state) {
                case ResultState.Ok: onOk.Invoke(Unwrap()); break;
                case ResultState.Error: onError.Invoke(UnwrapErr()); break;
                default: throw ErrorFactory.Result.InvalidOperationMatch();
            }
        }

        public Result ToResult() => ToResult(this);

        public bool Equals(Result<T> other) {
            return (_state, other._state) switch {
                (ResultState.Ok, ResultState.Ok) => EqualityComparer<T>.Default.Equals(_value, other._value),
                (ResultState.Error, ResultState.Error) => ExceptionUtility.EqualValue(_error, other._error),
                _ => false
            };
        }

        public override bool Equals(object obj)
            => obj is Result<T> other && Equals(other);

        public override int GetHashCode() =>
            IsOk()
                ? HashCode.Combine((int)_state, _value.GetHashCode())
                : HashCode.Combine((int)_state, _error.GetHashCode());

        public override string ToString() {
            return _state switch {
                ResultState.Ok => "Ok = {0}".Format(Unwrap()),
                ResultState.Error => "Error = {0}".Format(UnwrapErr()),
                _ => "Error:: Unrecognized State"
            };
        }

        public int CompareTo(Result<T> other) {
            return (_state, other._state) switch {
                (ResultState.Ok, ResultState.Ok) => Comparer<T>.Default.Compare(_value, other._value),
                (ResultState.Ok, ResultState.Error) => -1,
                (ResultState.Error, ResultState.Ok) => 1,
                _ => 0
            };
        }

        static public bool operator ==(Result<T> left, Result<T> right)
            => left.Equals(right);

        static public bool operator !=(Result<T> left, Result<T> right)
            => !left.Equals(right);

        static public bool operator >(Result<T> left, Result<T> right)
            => left.CompareTo(right) > 0;

        static public bool operator <(Result<T> left, Result<T> right)
            => left.CompareTo(right) < 0;

        static public bool operator >=(Result<T> left, Result<T> right)
            => left.CompareTo(right) >= 0;

        static public bool operator <=(Result<T> left, Result<T> right)
            => left.CompareTo(right) <= 0;

        static public implicit operator Result<T>(Result result) {
            if (result.IsError(out var exception)) return Result<T>.Error(exception);

            if (result.IsOk(out object obj)) {
                if (obj is null) return Result<T>.Ok();
                if (obj is T value) return Result<T>.Ok(value);
            }

            throw ErrorFactory.Result.InvalidImplicitUnboxingCast(result.Unwrap().GetType(), typeof(T));
        }

        static public Result ToResult(Result<T> result) {
            if (result.IsOk(out var value)) return Result.Ok(value);
            if (result.IsError(out var exception)) return Result.Error(exception);

            throw ErrorFactory.Result.InvalidBoxingCast(typeof(T));
        }
    }
}
