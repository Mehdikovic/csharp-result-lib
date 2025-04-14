// ReSharper disable CheckNamespace
// ReSharper disable ArrangeModifiersOrder
// ReSharper disable MemberCanBePrivate.Global

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using System;

using ResultLib.Core;

namespace ResultLib
{
    public struct Result : IEquatable<Result>, IComparable<Result>
    {
        private ActionState _state;
        private Exception _error;
        private object _value;

        static public Result Ok() =>
            new Result { _state = ActionState.Ok };
        
        static public Result Ok(object value) =>
            new Result { _state = ActionState.Ok, _value = value };

        static public Result Error() =>
            new Result { _state = ActionState.Error, _error = Exceptions.Result.Default() };

        static public Result Error(string error) =>
            new Result { _state = ActionState.Error, _error = Exceptions.Result.Create(error) };

        static public Result Error(Exception exception) =>
            new Result { _state = ActionState.Error, _error = exception ?? Exceptions.Result.Default() };

        static public Result Create(object value) => 
            value == null ? Error(Exceptions.Result.InvalidCreation()) : Ok(value);

        public bool IsOk() => _state == ActionState.Ok;
        
        public bool IsOk(out object value)
        {
            if (_state == ActionState.Ok)
            {
                value = _value;
                return true;
            }

            value = default;
            return false;
        }
        
        public bool IsError() => _state == ActionState.Error;

        public bool IsError(out Exception exception)
        {
            if (_state == ActionState.Error)
            {
                exception = _error;
                return true;
            }

            exception = null;
            return false;
        }

        public object Unwrap() => IsOk() ? _value : throw new InvalidOperationException();
        public object Unwrap(object defaultValue) => IsOk() ? _value : defaultValue;
        public object Unwrap(Func<object> defaultGetter) => IsOk() ? _value : defaultGetter.Invoke();
        
        public T Unwrap<T>() => IsOk() && _value is T nValue ? nValue : throw new InvalidOperationException();
        public T Unwrap<T>(T defaultValue) => IsOk() && _value is T nValue ? nValue : defaultValue;
        public T Unwrap<T>(Func<T> func) => IsOk() && _value is T nValue ? nValue : func.Invoke();
        
        public bool Some(out object value) => IsOk(out value) && value != null;
        public object Some(object defaultValue) => IsOk(out var value) && value != null
            ? value
            : (defaultValue ?? throw Exceptions.Result.InvalidNullSome());
        public object Some([NotNull] Func<object> func) => IsOk(out var value) && value != null 
            ? value
            : (func.Invoke() ?? throw Exceptions.Result.InvalidNullSome());

        public bool Some<T>(out T value) => IsOk(out value);
        public T Some<T>(T defaultValue) => IsOk(out T value)
            ? value
            : (defaultValue ?? throw Exceptions.Result.InvalidNullSome());
        public T Some<T>([NotNull] Func<T> func) => IsOk(out T value) 
            ? value
            : (func.Invoke() ?? throw Exceptions.Result.InvalidNullSome());

        public Exception UnwrapErr() => IsError() ? _error : throw Exceptions.Result.InvalidOperationUnwrapErr();
        public void ThrowIfError() { if (IsError()) throw _error; }

        public TRet Match<TRet>(Func<object, TRet> onOk, Func<Exception, TRet> onError)
        {
            if (IsOk(out var value)) return onOk.Invoke(value);
            if (IsError(out var exception)) return onError.Invoke(exception);
            throw new InvalidOperationException();
        }

        public void Match(Action<object> onOk, Action<Exception> onError)
        {
            if (IsOk(out var value)) onOk.Invoke(value);
            if (IsError(out var exception)) onError.Invoke(exception);
            throw new InvalidOperationException();
        }
        
        private bool IsOk<T>(out T value)
        {
            if (_state == ActionState.Ok && _value is T nValue)
            {
                value = nValue;
                return true;
            }

            value = default;
            return false;
        }

        public bool Equals(Result other)
        {
            return (_state, other._state) switch
            {
                (ActionState.Ok, ActionState.Ok) => EqualityComparer<object>.Default.Equals(_value, other._value),
                (ActionState.Error, ActionState.Error) => ExceptionUtility.EqualValue(_error, other._error),
                _ => false
            };
        }

        public override bool Equals(object obj)
            => obj is Result other && Equals(other);

        public override int GetHashCode()
        {
            return IsOk()
                ? HashCode.Combine((int)_state, _value.GetHashCode())
                : HashCode.Combine((int)_state, _error.GetHashCode());
        }

        public override string ToString()
        {
            if (IsOk(out var value)) return $"Ok = {value}";
            if (IsError(out var exception)) return $"Error = {exception}";
            return "Unrecognized State";
        }

        public int CompareTo(Result other)
        {
            return (_state, other._state) switch
            {
                (ActionState.Ok, ActionState.Ok) => Comparer<object>.Default.Compare(_value, other._value),
                (ActionState.Ok, ActionState.Error) => -1,
                (ActionState.Error, ActionState.Ok) => 1,
                _ => 0
            };
        }

        public static bool operator ==(Result left, Result right)
            => left.Equals(right);

        public static bool operator !=(Result left, Result right)
            => !left.Equals(right);

        public static bool operator >(Result left, Result right)
            => left.CompareTo(right) > 0;

        public static bool operator <(Result left, Result right)
            => left.CompareTo(right) < 0;

        public static bool operator >=(Result left, Result right)
            => left.CompareTo(right) >= 0;

        public static bool operator <=(Result left, Result right)
            => left.CompareTo(right) <= 0;
    }
}