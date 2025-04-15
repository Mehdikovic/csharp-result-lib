// ReSharper disable CheckNamespace
// ReSharper disable ArrangeModifiersOrder
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ResultLib.Core;

namespace ResultLib {
    public interface IResult<out T> {
        public bool IsOk();
        public bool IsError();
        public T Unwrap();
        public Exception UnwrapErr();
    }

    public struct Result<T> : IResult<T>, IEquatable<Result<T>>, IComparable<Result<T>> {
        private ActionState _state;
        private Exception _error;
        private T _value;

        static public Result<T> Ok() =>
            new Result<T> { _state = ActionState.Ok };

        static public Result<T> Ok(T value) =>
            new Result<T> { _state = ActionState.Ok, _value = value };

        static public Result<T> Error() =>
            new Result<T> { _state = ActionState.Error, _error = Exceptions.Result.Default() };

        static public Result<T> Error(string error) =>
            new Result<T> { _state = ActionState.Error, _error = Exceptions.Result.Create(error) };

        static public Result<T> Error(Exception exception) =>
            new Result<T> { _state = ActionState.Error, _error = exception ?? Exceptions.Result.Default() };

        static public Result<T> Create(T value) =>
            value == null ? Error(Exceptions.Result.InvalidCreation()) : Ok(value);

        public bool IsOk() => _state == ActionState.Ok;

        public bool IsOk(out T value) {
            if (_state == ActionState.Ok) {
                value = _value;
                return true;
            }

            value = default;
            return false;
        }

        public bool IsError() => _state == ActionState.Error;

        public bool IsError(out Exception exception) {
            if (_state == ActionState.Error) {
                exception = _error;
                return true;
            }

            exception = null;
            return false;
        }

        public T Unwrap() => IsOk() ? _value : throw Exceptions.Result.InvalidOperationUnwrap();
        public T Unwrap(T defaultValue) => IsOk() ? _value : defaultValue;
        public T Unwrap([NotNull] Func<T> func) => IsOk() ? _value : func.Invoke();

        public bool Some(out T value) => IsOk(out value) && value != null;
        public T Some(T defaultValue) =>
            IsOk(out var value) && value != null
                ? value
                : (defaultValue ?? throw Exceptions.Result.InvalidNullSome());
        public T Some([NotNull] Func<T> func) =>
            IsOk(out var value) && value != null
                ? value
                : (func.Invoke() ?? throw Exceptions.Result.InvalidNullSome());

        public Exception UnwrapErr() => IsError() ? _error : throw Exceptions.Result.InvalidOperationUnwrapErr();
        public void ThrowIfError() {
            if (IsError()) throw _error;
        }

        public TRet Match<TRet>([NotNull] Func<T, TRet> onOk, [NotNull] Func<Exception, TRet> onError) {
            if (IsOk(out var value)) return onOk.Invoke(value);
            if (IsError(out var exception)) return onError.Invoke(exception);
            throw Exceptions.Result.InvalidOperationMatch();
        }

        public void Match([NotNull] Action<T> onOk, [NotNull] Action<Exception> onError) {
            if (IsOk(out var value)) onOk.Invoke(value);
            if (IsError(out var exception)) onError.Invoke(exception);
            throw Exceptions.Result.InvalidOperationMatch();
        }

        public Result ToResult() => ToResult(this);

        public bool Equals(Result<T> other) {
            return (_state, other._state) switch {
                (ActionState.Ok, ActionState.Ok) => EqualityComparer<T>.Default.Equals(_value, other._value),
                (ActionState.Error, ActionState.Error) => ExceptionUtility.EqualValue(_error, other._error),
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
            if (IsOk(out var value)) return $"Ok = {value}";
            if (IsError(out var exception)) return $"Error = {exception}";
            return "Unrecognized State";
        }

        public int CompareTo(Result<T> other) {
            return (_state, other._state) switch {
                (ActionState.Ok, ActionState.Ok) => Comparer<T>.Default.Compare(_value, other._value),
                (ActionState.Ok, ActionState.Error) => -1,
                (ActionState.Error, ActionState.Ok) => 1,
                _ => 0
            };
        }

        public static bool operator ==(Result<T> left, Result<T> right)
            => left.Equals(right);

        public static bool operator !=(Result<T> left, Result<T> right)
            => !left.Equals(right);

        public static bool operator >(Result<T> left, Result<T> right)
            => left.CompareTo(right) > 0;

        public static bool operator <(Result<T> left, Result<T> right)
            => left.CompareTo(right) < 0;

        public static bool operator >=(Result<T> left, Result<T> right)
            => left.CompareTo(right) >= 0;

        public static bool operator <=(Result<T> left, Result<T> right)
            => left.CompareTo(right) <= 0;

        static public implicit operator Result<T>(Result result) {
            if (result.IsError(out var exception)) return Result<T>.Error(exception);

            if (result.IsOk(out object obj)) {
                if (obj is null) return Result<T>.Ok();
                if (obj is T value) return Result<T>.Ok(value);
            }

            throw Exceptions.Result.InvalidImplicitUnboxingCast(result.Unwrap().GetType(), typeof(T));
        }

        static public Result ToResult(Result<T> result) {
            if (result.IsOk(out var value)) return Result.Ok(value);
            if (result.IsError(out var exception)) return Result.Error(exception);

            throw Exceptions.Result.InvalidBoxingCast(typeof(T));
        }
    }
}
