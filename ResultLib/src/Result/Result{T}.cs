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
        public ResultException UnwrapErr();
    }

    public readonly struct Result<T> : IResult<T>, IEquatable<Result<T>>, IComparable<Result<T>> {
        private readonly ResultState _state;
        private readonly string _error;
        private readonly T _value;
        private readonly Exception _innerException;

        private Result(ResultState state, string error, Exception innerException, T value) {
            _state = state;
            _error = error.IsEmpty() ? ErrorFactory.Result.Default : error;
            _value = value;
            _innerException = innerException;
        }

        private Result(ResultState state, T value) : this(state, null, null, value) { }
        private Result(ResultState state, string error, T value) : this(state, error, null, value) { }
        private Result(ResultState state, Exception innerException, T value) : this(state, null, innerException, value) { }


        static public Result<T> Ok() =>
            new Result<T>(ResultState.Ok, value: default);

        static public Result<T> Ok(T value) =>
            new Result<T>(ResultState.Ok, value: value);

        static public Result<T> Error() =>
            new Result<T>(ResultState.Error, value: default);

        static public Result<T> Error(string error) =>
            new Result<T>(ResultState.Error, error, value: default);

        static public Result<T> Error(Exception innerException) =>
            new Result<T>(ResultState.Error, innerException, value: default);

        static internal Result<T> Error(string error, Exception innerException) =>
            new Result<T>(ResultState.Error, error, innerException, value: default);

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

        public bool IsError(out ResultException exception) {
            if (_state == ResultState.Error) {
                exception = new ResultException(_error ?? ErrorFactory.Result.EmptyConstructor, _innerException);
                return true;
            }

            exception = null;
            return false;
        }

        internal bool HasInnerException(out Exception innerException) {
            innerException = _innerException;
            return innerException != null;
        }

        public T Unwrap() => IsOk() ? _value : throw new ResultUnwrapException();
        public T Unwrap(T defaultValue) => IsOk() ? _value : defaultValue;
        public T Unwrap(Func<T> func) {
            if (IsOk()) return _value;
            ThrowIfNull(func);
            return func.Invoke();
        }

        public bool Some(out T value) => IsOk(out value) && value != null;

        public T Some() {
            if (IsOk(out var value) && value != null) return value;
            throw new ResultInvalidSomeOperationException();
        }

        public T Some(T defaultValue) {
            if (IsOk(out var value) && value != null) return value;
            ThrowIfNull(defaultValue);
            return defaultValue;
        }

        public T Some(Func<T> func) {
            if (IsOk(out var value) && value != null) return value;
            ThrowIfNull(func);
            var newValueFromSomeFunc = func.Invoke();
            if (newValueFromSomeFunc == null) throw new ResultInvalidSomeOperationException();
            return newValueFromSomeFunc;
        }

        public ResultException UnwrapErr() {
            if (!IsError()) throw new ResultUnwrapErrorException();
            if (_error == null) throw new ResultDefaultConstructorException();
            return new ResultException(_error, _innerException);
        }

        public void ThrowIfError() {
            if (!IsError()) return;
            if (_error == null) throw new ResultDefaultConstructorException();
            throw new ResultException(_error, _innerException);
        }

        public TRet Match<TRet>(Func<T, TRet> onOk, Func<ResultException, TRet> onError) {
            ThrowIfNull(onOk);
            ThrowIfNull(onError);

            return _state switch {
                ResultState.Ok => onOk.Invoke(Unwrap()),
                ResultState.Error => onError.Invoke(UnwrapErr()),
                _ => throw new ResultInvalidStateException()
            };
        }

        public TRet Match<TRet>(Func<TRet> onOk, Func<TRet> onError) {
            ThrowIfNull(onOk);
            ThrowIfNull(onError);

            return _state switch {
                ResultState.Ok => onOk.Invoke(),
                ResultState.Error => onError.Invoke(),
                _ => throw new ResultInvalidStateException()
            };
        }

        public void Match(Action<T> onOk, Action<ResultException> onError) {
            ThrowIfNull(onOk);
            ThrowIfNull(onError);

            switch (_state) {
                case ResultState.Ok: onOk.Invoke(Unwrap()); break;
                case ResultState.Error: onError.Invoke(UnwrapErr()); break;
                default: throw new ResultInvalidStateException();
            }
        }

        public void Match(Action onOk, Action onError) {
            ThrowIfNull(onOk);
            ThrowIfNull(onError);

            switch (_state) {
                case ResultState.Ok: onOk.Invoke(); break;
                case ResultState.Error: onError.Invoke(); break;
                default: throw new ResultInvalidStateException();
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
                ResultState.Ok => $"Ok = {(_value == null ? "null" : _value)}",
                ResultState.Error => $"Error = {_error}",
                _ => throw new ResultInvalidStateException()
            };
        }

        public int CompareTo(Result<T> other) {
            return (_state, other._state) switch {
                (ResultState.Ok, ResultState.Ok) => Comparer<T>.Default.Compare(_value, other._value),
                (ResultState.Ok, ResultState.Error) => 1,
                (ResultState.Error, ResultState.Ok) => -1,
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
            if (result.IsError(out string error)) {
                return result.HasInnerException(out var innerException) ? Error(error, innerException) : Error(error);
            }

            if (result.IsOk(out object obj)) {
                if (obj is null) return Ok();
                if (obj is T value) return Ok(value);
            }

            throw new ResultInvalidImplicitCastException(result.Unwrap().GetType(), typeof(T));
        }

        static private Result ToResult(Result<T> result) {
            if (result.IsError(out string error)) {
                return result.HasInnerException(out var innerException) ? Result.Error(error, innerException) : Result.Error(error);
            }
            if (result.IsOk(out var value)) return Result.Ok(value);

            throw new ResultInvalidBoxingCastException(typeof(T));
        }
    }
}
