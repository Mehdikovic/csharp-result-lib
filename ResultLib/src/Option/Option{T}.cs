// ReSharper disable CheckNamespace
// ReSharper disable ArrangeModifiersOrder
// ReSharper disable MemberCanBePrivate.Global

using System;
using ResultLib.Core;

namespace ResultLib {
    public interface IOption<out T> {
        bool IsSuccess();
        bool IsFailed();
        bool IsCanceled();
        bool IsSuccessOrCanceled() => IsSuccess() || IsCanceled();
        bool IsSuccessOrFailed() => IsSuccess() || IsFailed();
        bool IsFailedOrCanceled() => IsFailed() || IsCanceled();
        IResult<T> UnwrapBoxing();
        Exception GetError();
    }

    public struct Option<T> : IOption<T>, IEquatable<Option<T>>, IComparable<Option<T>> {
        private ActionState _state;
        private Result<T> _value;
        private Exception _error;

        static public Option<T> Success() =>
            new Option<T> { _state = ActionState.Ok, _value = Result<T>.Error() };

        static public Option<T> Success(T value) =>
            new Option<T> { _state = ActionState.Ok, _value = Result<T>.Create(value) };

        static public Option<T> Failed() =>
            new Option<T> { _state = ActionState.Error, _error = Exceptions.Option.Default(), _value = Result<T>.Error() };

        static public Option<T> Failed(string error) =>
            new Option<T> { _state = ActionState.Error, _error = Exceptions.Option.Create(error), _value = Result<T>.Error() };

        static public Option<T> Failed(string error, T value) =>
            new Option<T> { _state = ActionState.Error, _error = Exceptions.Option.Create(error), _value = Result<T>.Create(value) };

        static public Option<T> Failed(Exception exception) =>
            new Option<T> { _state = ActionState.Error, _error = exception ?? Exceptions.Option.Default(), _value = Result<T>.Error() };

        static public Option<T> Failed(Exception exception, T value) =>
            new Option<T> { _state = ActionState.Error, _error = exception ?? Exceptions.Option.Default(), _value = Result<T>.Create(value) };

        static public Option<T> Canceled() =>
            new Option<T> { _state = ActionState.Cancel, _error = Exceptions.Option.Cancel(), _value = Result<T>.Error() };

        static public Option<T> Canceled(T value) =>
            new Option<T> { _state = ActionState.Cancel, _error = Exceptions.Option.Cancel(), _value = Result<T>.Create(value) };

        public bool IsSuccess() => _state == ActionState.Ok;
        public bool IsSuccess(out Result<T> value) {
            value = _value;
            return IsSuccess();
        }

        public bool IsFailed() => _state == ActionState.Error;
        public bool IsFailed(out Result<T> value) {
            value = _value;
            return IsFailed();
        }

        public bool IsCanceled() => _state == ActionState.Cancel;
        public bool IsCanceled(out Result<T> value) {
            value = _value;
            return IsCanceled();
        }

        public bool IsSuccessOrCanceled() => IsSuccess() || IsCanceled();
        public bool IsSuccessOrCanceled(out Result<T> value) {
            value = _value;
            return IsSuccessOrCanceled();
        }

        public bool IsSuccessOrFailed() => IsSuccess() || IsFailed();
        public bool IsSuccessOrFailed(out Result<T> value) {
            value = _value;
            return IsSuccessOrFailed();
        }

        public bool IsFailedOrCanceled() => IsFailed() || IsCanceled();
        public bool IsFailedOrCanceled(out Result<T> value) {
            value = _value;
            return IsFailedOrCanceled();
        }

        public Result<T> GetResult() => _value;
        public Exception GetError() => _error ?? Exceptions.Option.NullUnwrapErr();
        public IResult<T> UnwrapBoxing() => GetResult();

        public void ThrowIfFailed() {
            if (IsFailed()) throw GetError();
        }
        public void ThrowIfCanceled() {
            if (IsCanceled()) throw GetError();
        }
        public void ThrowIfFailedOrCanceled() {
            if (IsFailedOrCanceled()) throw GetError();
        }

        public TRet Match<TRet>(
            Func<Result<T>, TRet> onSuccess,
            Func<Exception, TRet> onFailed,
            Func<Exception, TRet> onCanceled
        ) {
            if (IsSuccess()) return onSuccess != null ? onSuccess.Invoke(GetResult()) : default;
            if (IsFailed()) return onFailed != null ? onFailed.Invoke(_error) : default;
            if (IsCanceled()) return onCanceled != null ? onCanceled.Invoke(_error) : default;
            return default;
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<Result<T>, TRet> onSuccess, Func<Exception, TRet> onFailed)
            => Match(onSuccess, onFailed, null);

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result<T>, TRet> onSuccess, Func<Exception, TRet> onCanceled)
            => Match(onSuccess, null, onCanceled);

        public TRet MatchFailedOrCanceled<TRet>(Func<Exception, TRet> onFailed, Func<Exception, TRet> onCanceled)
            => Match(null, onFailed, onCanceled);

        public TRet Match<TRet>(
            Func<Result<T>, TRet> onSuccess,
            Func<Result<T>, Exception, TRet> onFailed,
            Func<Result<T>, Exception, TRet> onCanceled
        ) {
            if (IsSuccess()) return onSuccess != null ? onSuccess.Invoke(GetResult()) : default;
            if (IsFailed()) return onFailed != null ? onFailed.Invoke(GetResult(), _error) : default;
            if (IsCanceled()) return onCanceled != null ? onCanceled.Invoke(GetResult(), _error) : default;
            return default;
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<Result<T>, TRet> onSuccess, Func<Result<T>, Exception, TRet> onFailed)
            => Match(onSuccess, onFailed, null);

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result<T>, TRet> onSuccess, Func<Result<T>, Exception, TRet> onCanceled)
            => Match(onSuccess, null, onCanceled);

        public TRet MatchFailedOrCanceled<TRet>(Func<Result<T>, Exception, TRet> onFailed, Func<Result<T>, Exception, TRet> onCanceled)
            => Match(null, onFailed, onCanceled);

        public void Match(
            Action<Result<T>> onSuccess,
            Action<Exception> onFailed,
            Action<Exception> onCanceled
        ) {
            if (IsSuccess()) onSuccess?.Invoke(GetResult());
            if (IsFailed()) onFailed?.Invoke(_error);
            if (IsCanceled()) onCanceled?.Invoke(_error);
        }

        public void MatchSuccessOrFailed(Action<Result<T>> onSuccess, Action<Exception> onFailed)
            => Match(onSuccess, onFailed, null);

        public void MatchSuccessOrCanceled(Action<Result<T>> onSuccess, Action<Exception> onCanceled)
            => Match(onSuccess, null, onCanceled);

        public void MatchFailedOrCanceled(Action<Exception> onFailed, Action<Exception> onCanceled)
            => Match(null, onFailed, onCanceled);

        public void Match(
            Action<Result<T>> onSuccess,
            Action<Result<T>, Exception> onFailed,
            Action<Result<T>, Exception> onCanceled
        ) {
            if (IsSuccess()) onSuccess?.Invoke(GetResult());
            if (IsFailed()) onFailed?.Invoke(GetResult(), _error);
            if (IsCanceled()) onCanceled?.Invoke(GetResult(), _error);
        }

        public void MatchSuccessOrFailed(Action<Result<T>> onSuccess, Action<Result<T>, Exception> onFailed)
            => Match(onSuccess, onFailed, null);

        public void MatchSuccessOrCanceled(Action<Result<T>> onSuccess, Action<Result<T>, Exception> onCanceled)
            => Match(onSuccess, null, onCanceled);

        public void MatchFailedOrCanceled(Action<Result<T>, Exception> onFailed, Action<Result<T>, Exception> onCanceled)
            => Match(null, onFailed, onCanceled);

        public bool Equals(Option<T> other) {
            return (_state, other._state) switch {
                (ActionState.Ok, ActionState.Ok) =>
                    _value.Equals(other._value),
                (ActionState.Error, ActionState.Error) =>
                    _value.Equals(other._value) && ExceptionUtility.EqualValue(_error, other._error),
                (ActionState.Cancel, ActionState.Cancel) =>
                    _value.Equals(other._value) && ExceptionUtility.EqualValue(_error, other._error),
                _ => false
            };
        }

        public override bool Equals(object obj)
            => obj is Option<T> other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(
            (int)_state,
            _value.GetHashCode(),
            _error?.GetHashCode() ?? 0
        );

        public override string ToString() {
            if (IsSuccess()) return $"Success; Value: {{{_value}}}";
            if (IsFailed()) return $"Failed = {_error}; Value: {{{_value}}}";
            if (IsCanceled()) return $"Canceled = {_error}; Value: {{{_value}}}";

            return "Unrecognized State";
        }

        public int CompareTo(Option<T> other) {
            return (_state, other._state) switch {
                (ActionState.Ok, ActionState.Ok) => _value.CompareTo(other._value),
                (ActionState.Ok, ActionState.Error) => -1,
                (ActionState.Ok, ActionState.Cancel) => -1,
                (ActionState.Error, ActionState.Error) =>
                    (_value.IsOk() && other._value.IsOk())
                        ? _value.CompareTo(other._value)
                        : _value.IsOk()
                            ? -1
                            : other._value.IsOk()
                                ? 1
                                : 0,
                (ActionState.Error, ActionState.Ok) => 1,
                (ActionState.Error, ActionState.Cancel) => -1,
                (ActionState.Cancel, ActionState.Cancel) =>
                    (_value.IsOk() && other._value.IsOk())
                        ? _value.CompareTo(other._value)
                        : _value.IsOk()
                            ? -1
                            : other._value.IsOk()
                                ? 1
                                : 0,
                (ActionState.Cancel, ActionState.Ok) => 1,
                (ActionState.Cancel, ActionState.Error) => 1,
                _ => 0
            };
        }

        public static bool operator ==(Option<T> left, Option<T> right)
            => left.Equals(right);

        public static bool operator !=(Option<T> left, Option<T> right)
            => !left.Equals(right);

        public static bool operator >(Option<T> left, Option<T> right)
            => left.CompareTo(right) > 0;

        public static bool operator <(Option<T> left, Option<T> right)
            => left.CompareTo(right) < 0;

        public static bool operator >=(Option<T> left, Option<T> right)
            => left.CompareTo(right) >= 0;

        public static bool operator <=(Option<T> left, Option<T> right)
            => left.CompareTo(right) <= 0;

        public Option ToOption() => ToOption(this);

        static public implicit operator Option<T>(Option option) {
            if (option.GetResult().IsOk(out object obj)) {
                if (obj is null) return Failed(Exceptions.Option.InvalidIsOkCastOperation());
                if (obj is not T) return Failed(Exceptions.Option.InvalidImplicitUnboxingCast(obj.GetType(), typeof(T)));
            }

            if (option.IsSuccess(out var result)) {
                return result.Some(out T value)
                    ? Success(value)
                    : Success();
            }

            if (option.IsFailed(out result)) {
                return result.Some(out T value)
                    ? Failed(option.GetError(), value)
                    : Failed(option.GetError());
            }

            if (option.IsCanceled(out result)) {
                return result.Some(out T value)
                    ? Canceled(value)
                    : Canceled();
            }

            // we won't reach here
            return default;
        }

        static public Option ToOption(Option<T> option) {
            if (option.IsSuccess(out var result)) {
                return result.IsOk(out var value)
                    ? Option.Success(value)
                    : Option.Success();
            }

            if (option.IsFailed(out result)) {
                return result.IsOk(out var value)
                    ? Option.Failed(option._error, value)
                    : Option.Failed(option._error);
            }

            if (option.IsCanceled(out result)) {
                return result.IsOk(out var value)
                    ? Option.Canceled(value)
                    : Option.Canceled();
            }

            // we won't reach here
            return default;
        }
    }
}
