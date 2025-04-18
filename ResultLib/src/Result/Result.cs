//ReSharper disable CheckNamespace

using System;
using System.Collections.Generic;

using ResultLib.Core;

namespace ResultLib {
    public struct Result : IEquatable<Result>, IComparable<Result> {
        private ResultState _state;
        private string _error;
        private object _value;

        static public Result Ok() =>
            new Result { _state = ResultState.Ok };

        static public Result Ok(object value) =>
            new Result { _state = ResultState.Ok, _value = value };

        static public Result Error() =>
            new Result { _state = ResultState.Error, _error = ErrorFactory.Result.Default };

        static public Result Error(string error) =>
            new Result { _state = ResultState.Error, _error = error ?? string.Empty };

        static public Result Create(object value) =>
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

        public object Unwrap() => IsOk() ? _value : throw new Exception(ErrorFactory.Result.OperationUnwrapWhenError);
        public object Unwrap(object defaultValue) => IsOk() ? _value : defaultValue;
        public object Unwrap(Func<object> defaultGetter) => IsOk() ? _value : defaultGetter.Invoke();

        public bool Some(out object value) => IsOk(out value) && value != null;

        public object Some(object defaultValue) {
            if (defaultValue == null) throw new Exception(ErrorFactory.Result.SomeDefaultValueOfNull);

            return IsOk(out object value) && value != null
                ? value
                : defaultValue;
        }

        public object Some(Func<object> func) {
            return IsOk(out object value) && value != null
                ? value
                : (func.Invoke() ?? throw new Exception(ErrorFactory.Result.SomeReturnNull));
        }

        public bool Some<T>(out T value) => IsOk(out value);

        public T Some<T>(T defaultValue) {
            if (defaultValue == null) throw new Exception(ErrorFactory.Result.SomeDefaultValueOfNull);

            return IsOk(out T value)
                ? value
                : defaultValue;
        }

        public T Some<T>(Func<T> func) {
            return IsOk(out T value)
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

        public TRet Match<TRet>(Func<object, TRet> onOk, Func<Exception, TRet> onError) {
            return _state switch {
                ResultState.Ok => onOk.Invoke(Unwrap()),
                ResultState.Error => onError.Invoke(UnwrapErr()),
                _ => throw new Exception(ErrorFactory.Result.OperationMatch)
            };
        }

        public void Match(Action<object> onOk, Action<Exception> onError) {
            switch (_state) {
                case ResultState.Ok: onOk.Invoke(Unwrap()); break;
                case ResultState.Error: onError.Invoke(UnwrapErr()); break;
                default: throw new Exception(ErrorFactory.Result.OperationMatch);
            }
        }

        public bool Equals(Result other) {
            return (_state, other._state) switch {
                (ResultState.Ok, ResultState.Ok) => EqualityComparer<object>.Default.Equals(_value, other._value),
                (ResultState.Error, ResultState.Error) => string.Equals(_error, other._error, StringComparison.OrdinalIgnoreCase),
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
