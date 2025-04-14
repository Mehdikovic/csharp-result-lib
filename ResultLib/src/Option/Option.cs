// ReSharper disable CheckNamespace
// ReSharper disable ArrangeModifiersOrder
// ReSharper disable MemberCanBePrivate.Global

using System;

using ResultLib.Core;

namespace ResultLib
{
    public struct Option : IEquatable<Option>, IComparable<Option>
    {
        private ActionState _state;
        private Result _value;
        private Exception _error;

        static public Option Success() =>
            new Option { _state = ActionState.Ok, _value = Result.Error() };

        static public Option Success(object value) =>
            new Option { _state = ActionState.Ok, _value = Result.Create(value) };

        static public Option Failed() =>
            new Option { _state = ActionState.Error, _error = Exceptions.Option.Default(), _value = Result.Error() };

        static public Option Failed(string error) =>
            new Option { _state = ActionState.Error, _error = Exceptions.Option.Create(error), _value = Result.Error() };

        static public Option Failed(string error, object value) =>
            new Option { _state = ActionState.Error, _error = Exceptions.Option.Create(error), _value = Result.Create(value) };

        static public Option Failed(Exception exception) =>
            new Option { _state = ActionState.Error, _error = exception ?? Exceptions.Option.Default(), _value = Result.Error() };

        static public Option Failed(Exception exception, object value) =>
            new Option { _state = ActionState.Error, _error = exception ?? Exceptions.Option.Default(), _value = Result.Create(value) };

        static public Option Canceled() =>
            new Option { _state = ActionState.Cancel, _error = Exceptions.Option.Cancel(), _value = Result.Error() };

        static public Option Canceled(object value) =>
            new Option { _state = ActionState.Cancel, _error = Exceptions.Option.Cancel(), _value = Result.Create(value) };

        public bool IsSuccess() => _state == ActionState.Ok;
        public bool IsSuccess(out Result value)
        {
            value = _value;
            return IsSuccess();
        }

        public bool IsFailed() => _state == ActionState.Error;
        public bool IsFailed(out Result value)
        {
            value = _value;
            return IsFailed();
        }

        public bool IsCanceled() => _state == ActionState.Cancel;
        public bool IsCanceled(out Result value)
        {
            value = _value;
            return IsCanceled();
        }

        public bool IsSuccessOrCanceled() => IsSuccess() || IsCanceled();
        public bool IsSuccessOrCanceled(out Result value)
        {
            value = _value;
            return IsSuccessOrCanceled();
        }

        public bool IsSuccessOrFailed() => IsSuccess() || IsFailed();
        public bool IsSuccessOrFailed(out Result value)
        {
            value = _value;
            return IsSuccessOrFailed();
        }

        public bool IsFailedOrCanceled() => IsFailed() || IsCanceled();
        public bool IsFailedOrCanceled(out Result value)
        {
            value = _value;
            return IsFailedOrCanceled();
        }
        
        public Result GetResult() => _value;
        public Exception GetError() => _error ?? Exceptions.Option.NullUnwrapErr();

        public void ThrowIfFailed() { if (IsFailed()) throw GetError(); }
        public void ThrowIfCanceled() { if (IsCanceled()) throw GetError(); }
        public void ThrowIfFailedOrCanceled() { if (IsFailedOrCanceled()) throw GetError(); }

        public TRet Match<TRet>(
            Func<Result, TRet> onSuccess,
            Func<Exception, TRet> onFailed,
            Func<Exception, TRet> onCanceled)
        {
            if (IsSuccess()) return onSuccess != null ? onSuccess.Invoke(GetResult()) : default;
            if (IsFailed()) return onFailed != null ? onFailed.Invoke(_error) : default;
            if (IsCanceled()) return onCanceled != null ? onCanceled.Invoke(_error) : default;
            return default;
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<Result, TRet> onSuccess, Func<Exception, TRet> onFailed)
            => Match(onSuccess, onFailed, null);

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result, TRet> onSuccess, Func<Exception, TRet> onCanceled)
            => Match(onSuccess, null, onCanceled);

        public TRet MatchFailedOrCanceled<TRet>(Func<Exception, TRet> onFailed, Func<Exception, TRet> onCanceled)
            => Match(null, onFailed, onCanceled);

        public TRet Match<TRet>(
            Func<Result, TRet> onSuccess,
            Func<Result, Exception, TRet> onFailed,
            Func<Result, Exception, TRet> onCanceled)
        {
            if (IsSuccess()) return onSuccess != null ? onSuccess.Invoke(GetResult()) : default;
            if (IsFailed()) return onFailed != null ? onFailed.Invoke(GetResult(), _error) : default;
            if (IsCanceled()) return onCanceled != null ? onCanceled.Invoke(GetResult(), _error) : default;
            return default;
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<Result, TRet> onSuccess, Func<Result, Exception, TRet> onFailed)
            => Match(onSuccess, onFailed, null);

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result, TRet> onSuccess, Func<Result, Exception, TRet> onCanceled)
            => Match(onSuccess, null, onCanceled);

        public TRet MatchFailedOrCanceled<TRet>(Func<Result, Exception, TRet> onFailed, Func<Result, Exception, TRet> onCanceled)
            => Match(null, onFailed, onCanceled);

        public void Match(Action<Result> onSuccess, Action<Exception> onFailed, Action<Exception> onCanceled)
        {
            if (IsSuccess()) onSuccess?.Invoke(GetResult());
            if (IsFailed()) onFailed?.Invoke(_error);
            if (IsCanceled()) onCanceled?.Invoke(_error);
        }

        public void MatchSuccessOrFailed(Action<Result> onSuccess, Action<Exception> onFailed)
            => Match(onSuccess, onFailed, null);

        public void MatchSuccessOrCanceled(Action<Result> onSuccess, Action<Exception> onCanceled)
            => Match(onSuccess, null, onCanceled);

        public void MatchFailedOrCanceled(Action<Exception> onFailed, Action<Exception> onCanceled)
            => Match(null, onFailed, onCanceled);

        public void Match(
            Action<Result> onSuccess,
            Action<Result, Exception> onFailed,
            Action<Result, Exception> onCanceled)
        {
            if (IsSuccess()) onSuccess?.Invoke(GetResult());
            if (IsFailed()) onFailed?.Invoke(GetResult(), _error);
            if (IsCanceled()) onCanceled?.Invoke(GetResult(), _error);
        }

        public void MatchSuccessOrFailed(Action<Result> onSuccess, Action<Result, Exception> onFailed)
            => Match(onSuccess, onFailed, null);

        public void MatchSuccessOrCanceled(Action<Result> onSuccess, Action<Result, Exception> onCanceled)
            => Match(onSuccess, null, onCanceled);

        public void MatchFailedOrCanceled(Action<Result, Exception> onFailed, Action<Result, Exception> onCanceled)
            => Match(null, onFailed, onCanceled);

        public bool Equals(Option other)
        {
            return (_state, other._state) switch
            {
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
            => obj is Option other && Equals(other);

        public override int GetHashCode()
        {
            return HashCode.Combine(
                (int)_state,
                _value.GetHashCode(),
                _error?.GetHashCode() ?? 0
            );
        }

        public override string ToString()
        {
            if (IsSuccess()) return $"Success; Value: {{{_value}}}";
            if (IsFailed()) return $"Failed = {_error}; Value: {{{_value}}}";
            if (IsCanceled()) return $"Canceled = {_error}; Value: {{{_value}}}";

            return "Unrecognized State";
        }

        public int CompareTo(Option other)
        {
            return (_state, other._state) switch
            {
                (ActionState.Ok, ActionState.Ok) => _value.CompareTo(other._value),
                (ActionState.Ok, ActionState.Error) => -1,
                (ActionState.Ok, ActionState.Cancel) => -1,
                (ActionState.Error, ActionState.Error) =>
                    _value.IsOk() && other._value.IsOk()
                        ? _value.CompareTo(other._value)
                        : _value.IsOk()
                            ? -1
                            : other._value.IsOk()
                                ? 1
                                : 0,
                (ActionState.Error, ActionState.Ok) => 1,
                (ActionState.Error, ActionState.Cancel) => -1,
                (ActionState.Cancel, ActionState.Cancel) =>
                    _value.IsOk() && other._value.IsOk()
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

        public static bool operator ==(Option left, Option right)
            => left.Equals(right);

        public static bool operator !=(Option left, Option right)
            => !left.Equals(right);

        public static bool operator >(Option left, Option right)
            => left.CompareTo(right) > 0;

        public static bool operator <(Option left, Option right)
            => left.CompareTo(right) < 0;

        public static bool operator >=(Option left, Option right)
            => left.CompareTo(right) >= 0;

        public static bool operator <=(Option left, Option right)
            => left.CompareTo(right) <= 0;
    }
}