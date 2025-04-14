// ReSharper disable CheckNamespace
// ReSharper disable ArrangeModifiersOrder
// ReSharper disable MemberCanBePrivate.Global

using System;

using ResultLib.Core;

namespace ResultLib
{
    public interface IOption<out TSuccess, out TFailed, out TCanceled>
    {
        bool IsSuccess();
        bool IsFailed();
        bool IsCanceled();
        bool IsSuccessOrCanceled() => IsSuccess() || IsCanceled();
        bool IsSuccessOrFailed() => IsSuccess() || IsFailed();
        bool IsFailedOrCanceled() => IsFailed() || IsCanceled();
        IResult<TSuccess> UnwrapBoxingSuccess();
        IResult<TFailed> UnwrapBoxingFailed();
        IResult<TCanceled> UnwrapBoxingCanceled();
        Exception GetError();
    }

    public struct Option<TSuccess, TFailed, TCanceled> : IOption<TSuccess, TFailed, TCanceled>, IEquatable<Option<TSuccess, TFailed, TCanceled>>, IComparable<Option<TSuccess, TFailed, TCanceled>>
    {
        private ActionState _state;
        private Result<TSuccess> _valueSuccess;
        private Result<TFailed> _valueFailed;
        private Result<TCanceled> _valueCanceled;
        private Exception _error;

        static public Option<TSuccess, TFailed, TCanceled> Success() =>
            new Option<TSuccess, TFailed, TCanceled>
            {
                _state = ActionState.Ok, 
                _valueSuccess = Result<TSuccess>.Error(),
                _valueFailed = Result<TFailed>.Error(),
                _valueCanceled = Result<TCanceled>.Error(),
            };

        static public Option<TSuccess, TFailed, TCanceled> Success(TSuccess value) =>
            new Option<TSuccess, TFailed, TCanceled>
            {
                _state = ActionState.Ok, 
                _valueSuccess = Result<TSuccess>.Create(value),
                _valueFailed = Result<TFailed>.Error(),
                _valueCanceled = Result<TCanceled>.Error(),
            };

        static public Option<TSuccess, TFailed, TCanceled> Failed() =>
            new Option<TSuccess, TFailed, TCanceled>
            {
                _state = ActionState.Error, 
                _valueSuccess = Result<TSuccess>.Error(),
                _valueFailed = Result<TFailed>.Error(),
                _valueCanceled = Result<TCanceled>.Error(),
                _error = Exceptions.Option.Default(),
            };
        
        static public Option<TSuccess, TFailed, TCanceled> Failed(string error) =>
            new Option<TSuccess, TFailed, TCanceled>
            {
                _state = ActionState.Error, 
                _valueSuccess = Result<TSuccess>.Error(),
                _valueFailed = Result<TFailed>.Error(),
                _valueCanceled = Result<TCanceled>.Error(),
                _error = Exceptions.Option.Create(error),
            };
        
        static public Option<TSuccess, TFailed, TCanceled> Failed(string error, TFailed value) =>
            new Option<TSuccess, TFailed, TCanceled>
            {
                _state = ActionState.Error, 
                _valueSuccess = Result<TSuccess>.Error(),
                _valueFailed = Result<TFailed>.Create(value),
                _valueCanceled = Result<TCanceled>.Error(),
                _error = Exceptions.Option.Create(error),
            };
        
        static public Option<TSuccess, TFailed, TCanceled> Failed(Exception exception) =>
            new Option<TSuccess, TFailed, TCanceled>
            {
                _state = ActionState.Error, 
                _valueSuccess = Result<TSuccess>.Error(),
                _valueFailed = Result<TFailed>.Error(),
                _valueCanceled = Result<TCanceled>.Error(),
                _error = exception ?? Exceptions.Option.Default()
            };
        
        static public Option<TSuccess, TFailed, TCanceled> Failed(Exception exception, TFailed value) =>
            new Option<TSuccess, TFailed, TCanceled>
            {
                _state = ActionState.Error, 
                _valueSuccess = Result<TSuccess>.Error(),
                _valueFailed = Result<TFailed>.Create(value),
                _valueCanceled = Result<TCanceled>.Error(),
                _error = exception ?? Exceptions.Option.Default()
            };
        
        static public Option<TSuccess, TFailed, TCanceled> Canceled() =>
            new Option<TSuccess, TFailed, TCanceled>
            {
                _state = ActionState.Cancel, 
                _valueSuccess = Result<TSuccess>.Error(),
                _valueFailed = Result<TFailed>.Error(),
                _valueCanceled = Result<TCanceled>.Error(),
                _error = Exceptions.Option.Cancel()
            };
        
        static public Option<TSuccess, TFailed, TCanceled> Canceled(TCanceled value) =>
            new Option<TSuccess, TFailed, TCanceled>
            {
                _state = ActionState.Cancel, 
                _valueSuccess = Result<TSuccess>.Error(),
                _valueFailed = Result<TFailed>.Error(),
                _valueCanceled = Result<TCanceled>.Create(value),
                _error = Exceptions.Option.Cancel()
            };

        public bool IsSuccess() => _state == ActionState.Ok;
        public bool IsSuccess(out Result<TSuccess> value)
        {
            value = _valueSuccess;
            return IsSuccess();
        }

        public bool IsFailed() => _state == ActionState.Error;
        public bool IsFailed(out Result<TFailed> value)
        {
            value = _valueFailed;
            return IsFailed();
        }

        public bool IsCanceled() => _state == ActionState.Cancel;
        public bool IsCanceled(out Result<TCanceled> value)
        {
            value = _valueCanceled;
            return IsCanceled();
        }

        public bool IsSuccessOrCanceled() => IsSuccess() || IsCanceled();
        public bool IsSuccessOrCanceled(out Result<TSuccess> valueSuccess, out Result<TCanceled> valueCanceled)
        {
            valueSuccess = _valueSuccess;
            valueCanceled = _valueCanceled;
            return IsSuccessOrCanceled();
        }

        public bool IsSuccessOrFailed() => IsSuccess() || IsFailed();
        public bool IsSuccessOrFailed(out Result<TSuccess> valueSuccess, out Result<TFailed> valueFailed)
        {
            valueSuccess = _valueSuccess;
            valueFailed = _valueFailed;
            return IsSuccessOrFailed();
        }

        public bool IsFailedOrCanceled() => IsFailed() || IsCanceled();
        public bool IsFailedOrCanceled(out Result<TFailed> valueFailed, out Result<TCanceled> valueCanceled)
        {
            valueFailed = _valueFailed;
            valueCanceled = _valueCanceled;
            return IsFailedOrCanceled();
        }

        public Result<TSuccess> GetResultSuccess() => _valueSuccess;
        public Result<TFailed> GetResultFailed() => _valueFailed;
        public Result<TCanceled> GetResultCanceled() => _valueCanceled;
        public Exception GetError() => _error ?? Exceptions.Option.NullUnwrapErr();
        public IResult<TSuccess> UnwrapBoxingSuccess() => GetResultSuccess();
        public IResult<TFailed> UnwrapBoxingFailed() => GetResultFailed();
        public IResult<TCanceled> UnwrapBoxingCanceled() => GetResultCanceled();

        public TRet Match<TRet>(
            Func<Result<TSuccess>, TRet> onSuccess,
            Func<Exception, TRet> onFailed,
            Func<Exception, TRet> onCanceled)
        {
            if (IsSuccess()) return onSuccess != null ? onSuccess.Invoke(GetResultSuccess()) : default;
            if (IsFailed()) return onFailed != null ? onFailed.Invoke(_error) : default;
            if (IsCanceled()) return onCanceled != null ? onCanceled.Invoke(_error) : default;
            return default;
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Exception, TRet> onFailed)
            => Match(onSuccess, onFailed, null);

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Exception, TRet> onCanceled)
            => Match(onSuccess, null, onCanceled);

        public TRet MatchFailedOrCanceled<TRet>(Func<Exception, TRet> onFailed, Func<Exception, TRet> onCanceled)
            => Match(null, onFailed, onCanceled);

        public TRet Match<TRet>(
            Func<Result<TSuccess>, TRet> onSuccess,
            Func<Result<TFailed>, Exception, TRet> onFailed,
            Func<Result<TCanceled>, Exception, TRet> onCanceled)
        {
            if (IsSuccess()) return onSuccess != null ? onSuccess.Invoke(GetResultSuccess()) : default;
            if (IsFailed()) return onFailed != null ? onFailed.Invoke(GetResultFailed(), _error) : default;
            if (IsCanceled()) return onCanceled != null ? onCanceled.Invoke(GetResultCanceled(), _error) : default;
            return default;
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Result<TFailed>, Exception, TRet> onFailed)
            => Match(onSuccess, onFailed, null);

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Result<TCanceled>, Exception, TRet> onCanceled)
            => Match(onSuccess, null, onCanceled);

        public TRet MatchFailedOrCanceled<TRet>(Func<Result<TFailed>, Exception, TRet> onFailed, Func<Result<TCanceled>, Exception, TRet> onCanceled)
            => Match(null, onFailed, onCanceled);

        public void Match(
            Action<Result<TSuccess>> onSuccess,
            Action<Exception> onFailed,
            Action<Exception> onCanceled)
        {
            if (IsSuccess()) onSuccess?.Invoke(GetResultSuccess());
            if (IsFailed()) onFailed?.Invoke(_error);
            if (IsCanceled()) onCanceled?.Invoke(_error);
        }

        public void MatchSuccessOrFailed(Action<Result<TSuccess>> onSuccess, Action<Exception> onFailed)
            => Match(onSuccess, onFailed, null);

        public void MatchSuccessOrCanceled(Action<Result<TSuccess>> onSuccess, Action<Exception> onCanceled)
            => Match(onSuccess, null, onCanceled);

        public void MatchFailedOrCanceled(Action<Exception> onFailed, Action<Exception> onCanceled)
            => Match(null, onFailed, onCanceled);

        public void Match(
            Action<Result<TSuccess>> onSuccess,
            Action<Result<TFailed>, Exception> onFailed,
            Action<Result<TCanceled>, Exception> onCanceled)
        {
            if (IsSuccess()) onSuccess?.Invoke(GetResultSuccess());
            if (IsFailed()) onFailed?.Invoke(GetResultFailed(), _error);
            if (IsCanceled()) onCanceled?.Invoke(GetResultCanceled(), _error);
        }

        public void MatchSuccessOrFailed(Action<Result<TSuccess>> onSuccess, Action<Result<TFailed>, Exception> onFailed)
            => Match(onSuccess, onFailed, null);

        public void MatchSuccessOrCanceled(Action<Result<TSuccess>> onSuccess, Action<Result<TCanceled>, Exception> onCanceled)
            => Match(onSuccess, null, onCanceled);

        public void MatchFailedOrCanceled(Action<Result<TFailed>, Exception> onFailed, Action<Result<TCanceled>, Exception> onCanceled)
            => Match(null, onFailed, onCanceled);

        public bool Equals(Option<TSuccess, TFailed, TCanceled> other)
        {
            return (_state, other._state) switch
            {
                (ActionState.Ok, ActionState.Ok) => 
                    _valueSuccess.Equals(other._valueSuccess),
                (ActionState.Error, ActionState.Error) =>
                    _valueFailed.Equals(other._valueFailed) && ExceptionUtility.EqualValue(_error, other._error),
                (ActionState.Cancel, ActionState.Cancel) =>
                    _valueCanceled.Equals(other._valueCanceled) && ExceptionUtility.EqualValue(_error, other._error),
                _ => false
            };
        }

        public override bool Equals(object obj)
            => obj is Option<TSuccess, TFailed, TCanceled> other && Equals(other);

        public override int GetHashCode()
        {
            return HashCode.Combine(
                (int)_state,
                _state switch
                {
                    ActionState.Ok => _valueSuccess.GetHashCode(),
                    ActionState.Error => _valueFailed.GetHashCode(),
                    ActionState.Cancel => _valueCanceled.GetHashCode(),
                    _ => 0
                },
                _error?.GetHashCode() ?? 0
            );
        }

        public override string ToString()
        {
            if (IsSuccess()) return $"Success; Value: {{{_valueSuccess}}}";
            if (IsFailed()) return $"Failed = {_error}; Value: {{{_valueFailed}}}";
            if (IsCanceled()) return $"Canceled = {_error}; Value: {{{_valueCanceled}}}";

            return "Unrecognized State";
        }

        public int CompareTo(Option<TSuccess, TFailed, TCanceled> other)
        {
            return (_state, other._state) switch
            {
                (ActionState.Ok, ActionState.Ok) => _valueSuccess.CompareTo(other._valueSuccess),
                (ActionState.Ok, ActionState.Error) => -1,
                (ActionState.Ok, ActionState.Cancel) => -1,
                (ActionState.Error, ActionState.Error) =>
                    (_valueFailed.IsOk() && other._valueFailed.IsOk())
                        ? _valueFailed.CompareTo(other._valueFailed)
                        : _valueFailed.IsOk()
                            ? -1
                            : other._valueFailed.IsOk()
                                ? 1
                                : 0,
                (ActionState.Error, ActionState.Ok) => 1,
                (ActionState.Error, ActionState.Cancel) => -1,
                (ActionState.Cancel, ActionState.Cancel) =>
                    (_valueCanceled.IsOk() && other._valueCanceled.IsOk())
                        ? _valueCanceled.CompareTo(other._valueCanceled)
                        : _valueCanceled.IsOk()
                            ? -1
                            : other._valueCanceled.IsOk()
                                ? 1
                                : 0,
                (ActionState.Cancel, ActionState.Ok) => 1,
                (ActionState.Cancel, ActionState.Error) => 1,
                _ => 0
            };
        }

        public static bool operator ==(Option<TSuccess, TFailed, TCanceled> left, Option<TSuccess, TFailed, TCanceled> right)
            => left.Equals(right);

        public static bool operator !=(Option<TSuccess, TFailed, TCanceled> left, Option<TSuccess, TFailed, TCanceled> right)
            => !left.Equals(right);

        public static bool operator >(Option<TSuccess, TFailed, TCanceled> left, Option<TSuccess, TFailed, TCanceled> right)
            => left.CompareTo(right) > 0;

        public static bool operator <(Option<TSuccess, TFailed, TCanceled> left, Option<TSuccess, TFailed, TCanceled> right)
            => left.CompareTo(right) < 0;

        public static bool operator >=(Option<TSuccess, TFailed, TCanceled> left, Option<TSuccess, TFailed, TCanceled> right)
            => left.CompareTo(right) >= 0;

        public static bool operator <=(Option<TSuccess, TFailed, TCanceled> left, Option<TSuccess, TFailed, TCanceled> right)
            => left.CompareTo(right) <= 0;

        public Option ToOption()
        {
            return ToOption(this);
        }

        static public implicit operator Option<TSuccess, TFailed, TCanceled>(Option option)
        {
            if (option.GetResult().IsOk(out var obj))
            {
                if (obj is null) return Failed(Exceptions.Option.InvalidIsOkCastOperation());
                if (obj is not (TSuccess and TFailed and TCanceled)) return Failed(Exceptions.Option.InvalidIsOkCastOperation()); 
            }
            
            if (option.IsSuccess(out var result))
            {
                if (result.IsError()) return Success();
                return result.Some<TSuccess>(out var some) 
                    ? Success(some) 
                    : Failed(Exceptions.Option.InvalidImplicitUnboxingCast(obj.GetType(), typeof(TSuccess)));
            }

            if (option.IsFailed(out result))
            {
                if (result.IsError()) return Failed();
                return result.Some<TFailed>(out var some)
                    ? Failed(option.GetError(), some)
                    : Failed(Exceptions.Option.InvalidImplicitUnboxingCast(obj.GetType(), typeof(TFailed)));
            }
            
            if (option.IsCanceled(out result))
            {
                if (result.IsError()) return Canceled();
                return result.Some<TCanceled>(out var some)
                    ? Canceled(some)
                    : Failed(Exceptions.Option.InvalidImplicitUnboxingCast(obj.GetType(), typeof(TCanceled)));
            }

            // we won't reach here
            return default;
        }

        static public Option ToOption(Option<TSuccess, TFailed, TCanceled> option)
        {
            if (option.IsSuccess(out var resultSuccess))
            {
                return resultSuccess.IsOk(out var value)
                    ? Option.Success(value)
                    : Option.Success();
            }

            if (option.IsFailed(out var resultFailed))
            {
                return resultFailed.IsOk(out var value)
                    ? Option.Failed(option._error, value)
                    : Option.Failed(option._error);
            }

            if (option.IsCanceled(out var resultCanceled))
            {
                return resultCanceled.IsOk(out var value)
                    ? Option.Canceled(value)
                    : Option.Canceled();
            }
            
            // we won't reach here
            return default;
        }
    }
}