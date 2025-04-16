//ReSharper disable CheckNamespace

using System;
using System.Collections.Generic;

using ResultLib.Core;

using static System.ArgumentNullException;

namespace ResultLib {
    public struct Result() : IEquatable<Result>, IComparable<Result> {
        private ResultState _state = ResultState.Error;
        private Exception _error = ErrorFactory.Result.EmptyConstructor();
        private object _value = default;

        static public Result Ok() =>
            new Result { _state = ResultState.Ok };

        static public Result Ok(object value) =>
            new Result { _state = ResultState.Ok, _value = value };

        static public Result Error() =>
            new Result { _state = ResultState.Error, _error = ErrorFactory.Result.Default() };

        static public Result Error(string error) =>
            new Result { _state = ResultState.Error, _error = ErrorFactory.Result.Create(error) };

        static public Result Error(Exception exception) =>
            new Result { _state = ResultState.Error, _error = exception ?? ErrorFactory.Result.Default() };

        static public Result Create(object value) =>
            value == null ? Error(ErrorFactory.Result.InvalidAttemptToCreateOk()) : Ok(value);

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

        public bool IsError(out Exception exception) {
            if (_state == ResultState.Error) {
                exception = _error ?? ErrorFactory.Result.EmptyConstructor();
                return true;
            }

            exception = null;
            return false;
        }

        public object Unwrap() => IsOk() ? _value : throw ErrorFactory.Result.InvalidOperationUnwrapWhenError();
        public object Unwrap(object defaultValue) => IsOk() ? _value : defaultValue;
        public object Unwrap(Func<object> defaultGetter) => IsOk() ? _value : defaultGetter.Invoke();

        public T Unwrap<T>() => IsOk() && _value is T nValue ? nValue : throw ErrorFactory.Result.InvalidOperationUnwrapWhenError();
        public T Unwrap<T>(T defaultValue) => IsOk() && _value is T nValue ? nValue : defaultValue;
        public T Unwrap<T>(Func<T> func) => IsOk() && _value is T nValue ? nValue : func.Invoke();

        public bool Some(out object value) => IsOk(out value) && value != null;

        public object Some(object defaultValue) {
            ThrowIfNull(defaultValue);

            return IsOk(out object value) && value != null
                ? value
                : defaultValue;
        }

        public object Some(Func<object> func) {
            ThrowIfNull(func);

            return IsOk(out object value) && value != null
                ? value
                : (func.Invoke() ?? throw ErrorFactory.Result.InvalidNullSome());
        }

        public bool Some<T>(out T value) => IsOk(out value);

        public T Some<T>(T defaultValue) {
            ThrowIfNull(defaultValue);

            return IsOk(out T value)
                ? value
                : defaultValue;
        }

        public T Some<T>(Func<T> func) {
            ThrowIfNull(func);

            return IsOk(out T value)
                ? value
                : (func.Invoke() ?? throw ErrorFactory.Result.InvalidNullSome());
        }


        public Exception UnwrapErr() {
            if (!IsError()) throw ErrorFactory.Result.InvalidOperationUnwrapErrWhenOk();
            if (_error == null) throw ErrorFactory.Result.EmptyConstructor();
            return _error;
        }

        public void ThrowIfError() {
            if (IsError()) throw _error ?? ErrorFactory.Result.EmptyConstructor();
        }

        public TRet Match<TRet>(Func<object, TRet> onOk, Func<Exception, TRet> onError) {
            ThrowIfNull(onOk);
            ThrowIfNull(onError);

            return _state switch {
                ResultState.Ok => onOk.Invoke(Unwrap()),
                ResultState.Error => onError.Invoke(UnwrapErr()),
                _ => throw ErrorFactory.Result.InvalidOperationMatch()
            };
        }

        public void Match(Action<object> onOk, Action<Exception> onError) {
            ThrowIfNull(onOk);
            ThrowIfNull(onError);

            switch (_state) {
                case ResultState.Ok: onOk.Invoke(Unwrap()); break;
                case ResultState.Error: onError.Invoke(UnwrapErr()); break;
                default: throw ErrorFactory.Result.InvalidOperationMatch();
            }
        }

        public bool Equals(Result other) {
            return (_state, other._state) switch {
                (ResultState.Ok, ResultState.Ok) => EqualityComparer<object>.Default.Equals(_value, other._value),
                (ResultState.Error, ResultState.Error) => ExceptionUtility.EqualValue(_error, other._error),
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
                (ResultState.Ok, ResultState.Error) => -1,
                (ResultState.Error, ResultState.Ok) => 1,
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
