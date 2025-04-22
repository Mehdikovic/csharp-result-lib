//ReSharper disable CheckNamespace

using System;
using System.Collections.Generic;

using ResultLib.Core;

using static ResultLib.Core.ArgumentNullExceptionExtension;

namespace ResultLib {
    public readonly struct Result : IEquatable<Result>, IComparable<Result> {
        private readonly ResultState _state;
        private readonly string _error;
        private readonly object _value;
        private readonly Exception _innerException;

        private Result(ResultState state, string error, object value) {
            _state = state;
            _error = error.IsEmpty() ? ErrorFactory.Result.Default : error;
            _value = value;
            _innerException = null;
        }

        private Result(ResultState state, Exception innerException, object value) {
            _state = state;
            _error = ErrorFactory.Result.Default;
            _value = value;
            _innerException = innerException;
        }

        static public Result Ok() =>
            new Result(ResultState.Ok, error: null, value: null);

        static public Result Ok(object value) =>
            new Result(ResultState.Ok, error: null, value: value);

        static public Result Error() =>
            new Result(ResultState.Error, error: null, value: null);

        static public Result Error(string error) =>
            new Result(ResultState.Error, error: error, value: null);

        static public Result Error(Exception exception) =>
            new Result(ResultState.Error, innerException: exception, value: null);

        static public Result FromRequired(object value) =>
            value == null ? Error(ErrorFactory.Result.AttemptToCreateOk) : Ok(value);

        public bool IsOk() => _state == ResultState.Ok;

        public bool IsOk(out object value) {
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

        public object Unwrap() => IsOk() ? _value : throw new ResultUnwrapException();
        public object Unwrap(object defaultValue) => IsOk() ? _value : defaultValue;
        public object Unwrap(Func<object> func) {
            if (IsOk()) return _value;
            ThrowIfNull(func);
            return func.Invoke();
        }

        public bool Some(out object value) => IsOk(out value) && value != null;

        public object Some(object defaultValue) {
            if (IsOk(out object value) && value != null) return value;
            ThrowIfNull(defaultValue);
            return defaultValue;
        }

        public object Some(Func<object> func) {
            if (IsOk(out object value) && value != null) return value;
            ThrowIfNull(func);
            object newValueFromSomeFunc = func.Invoke();
            if (newValueFromSomeFunc == null) throw new ResultInvalidSomeOperationException();
            return newValueFromSomeFunc;
        }

        public bool Some<T>(out T value) => IsOk(out value);

        public T Some<T>(T defaultValue) {
            if (IsOk(out T value)) return value;
            ThrowIfNull(defaultValue);
            return defaultValue;
        }

        public T Some<T>(Func<T> func) {
            if (IsOk(out T value)) return value;
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

        public TRet Match<TRet>(Func<object, TRet> onOk, Func<ResultException, TRet> onError) {
            ThrowIfNull(onOk);
            ThrowIfNull(onError);

            return _state switch {
                ResultState.Ok => onOk.Invoke(Unwrap()),
                ResultState.Error => onError.Invoke(UnwrapErr()),
                _ => throw new ResultInvalidMatchException()
            };
        }

        public TRet Match<TRet>(Func<TRet> onOk, Func<TRet> onError) {
            ThrowIfNull(onOk);
            ThrowIfNull(onError);

            return _state switch {
                ResultState.Ok => onOk.Invoke(),
                ResultState.Error => onError.Invoke(),
                _ => throw new ResultInvalidMatchException()
            };
        }

        public void Match(Action<object> onOk, Action<ResultException> onError) {
            ThrowIfNull(onOk);
            ThrowIfNull(onError);

            switch (_state) {
                case ResultState.Ok: onOk.Invoke(Unwrap()); break;
                case ResultState.Error: onError.Invoke(UnwrapErr()); break;
                default: throw new ResultInvalidMatchException();
            }
        }

        public void Match(Action onOk, Action onError) {
            ThrowIfNull(onOk);
            ThrowIfNull(onError);

            switch (_state) {
                case ResultState.Ok: onOk.Invoke(); break;
                case ResultState.Error: onError.Invoke(); break;
                default: throw new ResultInvalidMatchException();
            }
        }

        public bool Equals(Result other) {
            return (_state, other._state) switch {
                (ResultState.Ok, ResultState.Ok) => EqualityComparer<object>.Default.Equals(_value, other._value),
                (ResultState.Error, ResultState.Error) => string.Equals(_error, other._error, StringComparison.InvariantCultureIgnoreCase),
                _ => false
            };
        }

        public override bool Equals(object obj)
            => obj is Result other && Equals(other);

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

        public int CompareTo(Result other) {
            return (_state, other._state) switch {
                (ResultState.Ok, ResultState.Ok) => Comparer<object>.Default.Compare(_value, other._value),
                (ResultState.Ok, ResultState.Error) => 1,
                (ResultState.Error, ResultState.Ok) => -1,
                _ => 0
            };
        }

        static public bool operator ==(Result left, Result right)
            => left.Equals(right);

        static public bool operator !=(Result left, Result right)
            => !left.Equals(right);

        static public bool operator >(Result left, Result right)
            => left.CompareTo(right) > 0;

        static public bool operator <(Result left, Result right)
            => left.CompareTo(right) < 0;

        static public bool operator >=(Result left, Result right)
            => left.CompareTo(right) >= 0;

        static public bool operator <=(Result left, Result right)
            => left.CompareTo(right) <= 0;

        private bool IsOk<T>(out T value) {
            if (_state == ResultState.Ok && _value is T nValue) {
                value = nValue;
                return true;
            }

            value = default;
            return false;
        }
    }
}
