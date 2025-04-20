// ReSharper disable CheckNamespace
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable InvertIf

using System;
using System.Collections.Generic;

using ResultLib.Core;

using static ResultLib.Core.ArgumentNullExceptionExtension;

namespace ResultLib {
    public interface IResult<out T> {
        public bool IsOk();
        public bool IsError();
        public T Unwrap();
        public Exception UnwrapErr();
    }

    public struct Result<T> : IResult<T>, IEquatable<Result<T>>, IComparable<Result<T>> {
        private ResultState _state;
        private string _error;
        private T _value;

        static public Result<T> Ok() =>
            new Result<T> { _state = ResultState.Ok };

        static public Result<T> Ok(T value) =>
            new Result<T> { _state = ResultState.Ok, _value = value };

        static public Result<T> Error() =>
            new Result<T> { _state = ResultState.Error, _error = ErrorFactory.Result.Default };

        static public Result<T> Error(string error) =>
            new Result<T> { _state = ResultState.Error, _error = error ?? string.Empty };

        static public Result<T> FromRequired(T value) =>
            value == null ? Error(ErrorFactory.Result.AttemptToCreateOk) : Ok(value);

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

        public bool IsError(out string error) {
            if (_state == ResultState.Error) {
                error = _error ?? ErrorFactory.Result.EmptyConstructor;
                return true;
            }

            error = null;
            return false;
        }

        public T Unwrap() => IsOk() ? _value : throw new Exception(ErrorFactory.Result.OperationUnwrapWhenError);
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
                : (func.Invoke() ?? throw new Exception(ErrorFactory.Result.SomeReturnNull));
        }

        public Exception UnwrapErr() {
            if (!IsError()) throw new Exception(ErrorFactory.Result.OperationUnwrapErrWhenOk);
            if (_error == null) throw new Exception(ErrorFactory.Result.EmptyConstructor);
            return new Exception(_error);
        }

        public void ThrowIfError() {
            if (IsError()) throw new Exception(_error ?? ErrorFactory.Result.EmptyConstructor);
        }

        public TRet Match<TRet>(Func<T, TRet> onOk, Func<Exception, TRet> onError) {
            ThrowIfNull(onOk);
            ThrowIfNull(onError);

            return _state switch {
                ResultState.Ok => onOk.Invoke(Unwrap()),
                ResultState.Error => onError.Invoke(UnwrapErr()),
                _ => throw new Exception(ErrorFactory.Result.OperationMatch)
            };
        }

        public void Match(Action<T> onOk, Action<Exception> onError) {
            ThrowIfNull(onOk);
            ThrowIfNull(onError);

            switch (_state) {
                case ResultState.Ok: onOk.Invoke(Unwrap()); break;
                case ResultState.Error: onError.Invoke(UnwrapErr()); break;
                default: throw new Exception(ErrorFactory.Result.OperationMatch);
            }
        }

        public Result ToResult() => ToResult(this);

        public bool Equals(Result<T> other) {
            return (_state, other._state) switch {
                (ResultState.Ok, ResultState.Ok) => EqualityComparer<T>.Default.Equals(_value, other._value),
                (ResultState.Error, ResultState.Error) => string.Equals(_error, other._error, StringComparison.InvariantCultureIgnoreCase),
                _ => false
            };
        }

        public override bool Equals(object obj)
            => obj is Result<T> other && Equals(other);

        public override int GetHashCode() =>
            IsOk()
                ? HashCode.Combine((int)_state, _value?.GetHashCode() ?? 0)
                : HashCode.Combine((int)_state, _error?.GetHashCode() ?? 0);

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
            if (result.IsError(out string error)) return Result<T>.Error(error);

            if (result.IsOk(out object obj)) {
                if (obj is null) return Result<T>.Ok();
                if (obj is T value) return Result<T>.Ok(value);
            }

            throw new Exception(ErrorFactory.Result.CreateImplicitUnboxingCast(result.Unwrap().GetType(), typeof(T)));
        }

        static private Result ToResult(Result<T> result) {
            if (result.IsError(out string error)) return Result.Error(error);
            if (result.IsOk(out var value)) return Result.Ok(value);

            throw new Exception(ErrorFactory.Result.CreateBoxingCast(typeof(T)));
        }
    }
}
