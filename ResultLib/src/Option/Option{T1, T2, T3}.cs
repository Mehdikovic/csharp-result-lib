// ReSharper disable CheckNamespace
// ReSharper disable ArrangeModifiersOrder
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;

using ResultLib.Core;

using static ResultLib.Core.ArgumentNullExceptionExtension;

namespace ResultLib {
    public interface IOption<out TSuccess, out TFailed, out TCanceled> {
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

    public readonly struct Option<TSuccess, TFailed, TCanceled> :
        IOption<TSuccess, TFailed, TCanceled>,
        IEquatable<Option<TSuccess, TFailed, TCanceled>>,
        IComparable<Option<TSuccess, TFailed, TCanceled>> {
        private readonly OptionState _state;
        private readonly Result<TSuccess> _valueSuccess;
        private readonly Result<TFailed> _valueFailed;
        private readonly Result<TCanceled> _valueCanceled;
        private readonly Exception _error;

        private Option(OptionState state, Exception error, Result<TSuccess> success, Result<TFailed> failed, Result<TCanceled> canceled) {
            _state = state;
            _error = error;
            _valueSuccess = success;
            _valueFailed = failed;
            _valueCanceled = canceled;
        }

        static public Option<TSuccess, TFailed, TCanceled> Success() =>
            new Option<TSuccess, TFailed, TCanceled>(
                OptionState.Success,
                error: null,
                success: Result<TSuccess>.Error(),
                failed: Result<TFailed>.Error(),
                canceled: Result<TCanceled>.Error()
            );

        static public Option<TSuccess, TFailed, TCanceled> Success(TSuccess value) =>
            new Option<TSuccess, TFailed, TCanceled>(
                OptionState.Success,
                error: null,
                success: Result<TSuccess>.FromRequired(value),
                failed: Result<TFailed>.Error(),
                canceled: Result<TCanceled>.Error()
            );

        static public Option<TSuccess, TFailed, TCanceled> Failed() =>
            new Option<TSuccess, TFailed, TCanceled>(
                OptionState.Failed,
                error: null,
                success: Result<TSuccess>.Error(),
                failed: Result<TFailed>.Error(),
                canceled: Result<TCanceled>.Error()
            );

        static public Option<TSuccess, TFailed, TCanceled> FailedValue(TFailed value) =>
            new Option<TSuccess, TFailed, TCanceled>(
                OptionState.Failed,
                error: null,
                success: Result<TSuccess>.Error(),
                failed: Result<TFailed>.FromRequired(value),
                canceled: Result<TCanceled>.Error()
            );

        static public Option<TSuccess, TFailed, TCanceled> Failed(string error) =>
            new Option<TSuccess, TFailed, TCanceled>(
                OptionState.Failed,
                error: new OptionException(error),
                success: Result<TSuccess>.Error(),
                failed: Result<TFailed>.Error(),
                canceled: Result<TCanceled>.Error()
            );

        static public Option<TSuccess, TFailed, TCanceled> Failed(string error, TFailed value) =>
            new Option<TSuccess, TFailed, TCanceled>(
                OptionState.Failed,
                error: new OptionException(error),
                success: Result<TSuccess>.Error(),
                failed: Result<TFailed>.FromRequired(value),
                canceled: Result<TCanceled>.Error()
            );

        static public Option<TSuccess, TFailed, TCanceled> Failed(Exception exception) =>
            new Option<TSuccess, TFailed, TCanceled>(
                OptionState.Failed,
                error: exception,
                success: Result<TSuccess>.Error(),
                failed: Result<TFailed>.Error(),
                canceled: Result<TCanceled>.Error()
            );

        static public Option<TSuccess, TFailed, TCanceled> Failed(Exception exception, TFailed value) =>
            new Option<TSuccess, TFailed, TCanceled>(
                OptionState.Failed,
                error: exception,
                success: Result<TSuccess>.Error(),
                failed: Result<TFailed>.FromRequired(value),
                canceled: Result<TCanceled>.Error()
            );

        static public Option<TSuccess, TFailed, TCanceled> Canceled() =>
            new Option<TSuccess, TFailed, TCanceled>(
                OptionState.Canceled,
                error: null,
                success: Result<TSuccess>.Error(),
                failed: Result<TFailed>.Error(),
                canceled: Result<TCanceled>.Error()
            );

        static public Option<TSuccess, TFailed, TCanceled> Canceled(TCanceled value) =>
            new Option<TSuccess, TFailed, TCanceled>(
                OptionState.Canceled,
                error: null,
                success: Result<TSuccess>.Error(),
                failed: Result<TFailed>.Error(),
                canceled: Result<TCanceled>.FromRequired(value)
            );

        public bool IsSuccess() => _state == OptionState.Success;
        public bool IsSuccess(out Result<TSuccess> value) {
            value = _valueSuccess;
            return IsSuccess();
        }

        public bool IsFailed() => _state == OptionState.Failed;
        public bool IsFailed(out Result<TFailed> value) {
            value = _valueFailed;
            return IsFailed();
        }

        public bool IsCanceled() => _state == OptionState.Canceled;
        public bool IsCanceled(out Result<TCanceled> value) {
            value = _valueCanceled;
            return IsCanceled();
        }

        public bool IsSuccessOrCanceled() => IsSuccess() || IsCanceled();
        public bool IsSuccessOrCanceled(out Result<TSuccess> valueSuccess, out Result<TCanceled> valueCanceled) {
            valueSuccess = _valueSuccess;
            valueCanceled = _valueCanceled;
            return IsSuccessOrCanceled();
        }

        public bool IsSuccessOrFailed() => IsSuccess() || IsFailed();
        public bool IsSuccessOrFailed(out Result<TSuccess> valueSuccess, out Result<TFailed> valueFailed) {
            valueSuccess = _valueSuccess;
            valueFailed = _valueFailed;
            return IsSuccessOrFailed();
        }

        public bool IsFailedOrCanceled() => IsFailed() || IsCanceled();
        public bool IsFailedOrCanceled(out Result<TFailed> valueFailed, out Result<TCanceled> valueCanceled) {
            valueFailed = _valueFailed;
            valueCanceled = _valueCanceled;
            return IsFailedOrCanceled();
        }

        public Result<TSuccess> GetResultSuccess() => _valueSuccess;
        public IResult<TSuccess> UnwrapBoxingSuccess() => GetResultSuccess();
        public Result<TFailed> GetResultFailed() => _valueFailed;
        public IResult<TFailed> UnwrapBoxingFailed() => GetResultFailed();
        public Result<TCanceled> GetResultCanceled() => _valueCanceled;
        public IResult<TCanceled> UnwrapBoxingCanceled() => GetResultCanceled();

        public Exception GetError() {
            if (_error != null) return _error;

            return _state switch {
                OptionState.Success => throw new OptionInvalidGetErrorException(),
                OptionState.Failed => new OptionException(),
                OptionState.Canceled => new OptionOperationCanceledException(),
                _ => throw new OptionInvalidStateException(),
            };
        }

        public void ThrowIfFailed() {
            if (IsFailed()) throw GetError();
        }
        public void ThrowIfCanceled() {
            if (IsCanceled()) throw GetError();
        }
        public void ThrowIfFailedOrCanceled() {
            if (IsFailedOrCanceled()) throw GetError();
        }

        #region Match_Func
        public TRet Match<TRet>(
            Func<TRet> onSuccess,
            Func<TRet> onFailed,
            Func<TRet> onCanceled
        ) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(),
                OptionState.Failed => onFailed.Invoke(),
                OptionState.Canceled => onCanceled.Invoke(),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet Match<TRet>(Func<TRet> onSuccess, Func<TRet> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(),
                OptionState.Failed => onFailedOrCanceled.Invoke(),
                OptionState.Canceled => onFailedOrCanceled.Invoke(),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<TRet> onSuccess, Func<TRet> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(),
                OptionState.Failed => onFailed.Invoke(),
                OptionState.Canceled => default,
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchSuccessOrCanceled<TRet>(Func<TRet> onSuccess, Func<TRet> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(),
                OptionState.Failed => default,
                OptionState.Canceled => onCanceled.Invoke(),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<TRet> onFailed, Func<TRet> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailed.Invoke(),
                OptionState.Canceled => onCanceled.Invoke(),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<TRet> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailedOrCanceled.Invoke(),
                OptionState.Canceled => onFailedOrCanceled.Invoke(),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet Match<TRet>(
            Func<Result<TSuccess>, TRet> onSuccess,
            Func<Result<TFailed>, TRet> onFailed,
            Func<Result<TCanceled>, TRet> onCanceled
        ) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Failed => onFailed.Invoke(GetResultFailed()),
                OptionState.Canceled => onCanceled.Invoke(GetResultCanceled()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet Match<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Result, TRet> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Failed => onFailedOrCanceled.Invoke(GetResultFailed().ToResult()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetResultCanceled().ToResult()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Result<TFailed>, TRet> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Failed => onFailed.Invoke(GetResultFailed()),
                OptionState.Canceled => default,
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Result<TCanceled>, TRet> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Failed => default,
                OptionState.Canceled => onCanceled.Invoke(GetResultCanceled()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Result<TFailed>, TRet> onFailed, Func<Result<TCanceled>, TRet> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailed.Invoke(GetResultFailed()),
                OptionState.Canceled => onCanceled.Invoke(GetResultCanceled()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Result, TRet> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailedOrCanceled.Invoke(GetResultFailed().ToResult()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetResultCanceled().ToResult()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet Match<TRet>(
            Func<Result<TSuccess>, TRet> onSuccess,
            Func<Exception, TRet> onFailed,
            Func<Exception, TRet> onCanceled
        ) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Failed => onFailed.Invoke(GetError()),
                OptionState.Canceled => onCanceled.Invoke(GetError()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet Match<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Exception, TRet> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Failed => onFailedOrCanceled.Invoke(GetError()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetError()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Exception, TRet> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Failed => onFailed.Invoke(GetError()),
                OptionState.Canceled => default,
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Exception, TRet> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Failed => default,
                OptionState.Canceled => onCanceled.Invoke(GetError()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Exception, TRet> onFailed, Func<Exception, TRet> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailed.Invoke(GetError()),
                OptionState.Canceled => onCanceled.Invoke(GetError()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Exception, TRet> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailedOrCanceled.Invoke(GetError()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetError()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet Match<TRet>(
            Func<Result<TSuccess>, TRet> onSuccess,
            Func<Result<TFailed>, Exception, TRet> onFailed,
            Func<Result<TCanceled>, Exception, TRet> onCanceled
        ) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Failed => onFailed.Invoke(GetResultFailed(), GetError()),
                OptionState.Canceled => onCanceled.Invoke(GetResultCanceled(), GetError()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet Match<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Result, Exception, TRet> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Failed => onFailedOrCanceled.Invoke(GetResultFailed().ToResult(), GetError()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetResultCanceled().ToResult(), GetError()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Result<TFailed>, Exception, TRet> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Failed => onFailed.Invoke(GetResultFailed(), GetError()),
                OptionState.Canceled => default,
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Result<TCanceled>, Exception, TRet> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Failed => default,
                OptionState.Canceled => onCanceled.Invoke(GetResultCanceled(), GetError()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Result<TFailed>, Exception, TRet> onFailed, Func<Result<TCanceled>, Exception, TRet> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailed.Invoke(GetResultFailed(), GetError()),
                OptionState.Canceled => onCanceled.Invoke(GetResultCanceled(), GetError()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Result, Exception, TRet> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailedOrCanceled.Invoke(GetResultFailed().ToResult(), GetError()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetResultCanceled().ToResult(), GetError()),
                _ => throw new OptionInvalidStateException()
            };
        }
        #endregion

        #region Match_Action
        public void Match(Action onSuccess, Action onFailed, Action onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(); break;
                case OptionState.Failed: onFailed.Invoke(); break;
                case OptionState.Canceled: onCanceled.Invoke(); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void Match(Action onSuccess, Action onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(); break;
                case OptionState.Failed:
                case OptionState.Canceled: onFailedOrCanceled.Invoke(); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchSuccessOrFailed(Action onSuccess, Action onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(); break;
                case OptionState.Failed: onFailed.Invoke(); break;
                case OptionState.Canceled: break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchSuccessOrCanceled(Action onSuccess, Action onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(); break;
                case OptionState.Failed: onFailed.Invoke(); break;
                case OptionState.Canceled: break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchFailedOrCanceled(Action onFailed, Action onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed: onFailed.Invoke(); break;
                case OptionState.Canceled: onCanceled.Invoke(); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchFailedOrCanceled(Action onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed:
                case OptionState.Canceled: onFailedOrCanceled.Invoke(); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void Match(Action<Result<TSuccess>> onSuccess, Action<Result<TFailed>> onFailed, Action<Result<TCanceled>> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: onFailed.Invoke(GetResultFailed()); break;
                case OptionState.Canceled: onCanceled.Invoke(GetResultCanceled()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void Match(Action<Result<TSuccess>> onSuccess, Action<Result> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: onFailedOrCanceled.Invoke(GetResultFailed().ToResult()); break;
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetResultCanceled().ToResult()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchSuccessOrFailed(Action<Result<TSuccess>> onSuccess, Action<Result<TFailed>> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: onFailed.Invoke(GetResultFailed()); break;
                case OptionState.Canceled: break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchSuccessOrCanceled(Action<Result<TSuccess>> onSuccess, Action<Result<TCanceled>> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: break;
                case OptionState.Canceled: onCanceled.Invoke(GetResultCanceled()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchFailedOrCanceled(Action<Result<TSuccess>> onSuccess, Action<Result<TFailed>> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: onFailed.Invoke(GetResultFailed()); break;
                case OptionState.Canceled: break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchFailedOrCanceled(Action<Result> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed: onFailedOrCanceled.Invoke(GetResultFailed().ToResult()); break;
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetResultCanceled().ToResult()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void Match(
            Action<Result<TSuccess>> onSuccess,
            Action<Exception> onFailed,
            Action<Exception> onCanceled
        ) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: onFailed.Invoke(GetError()); break;
                case OptionState.Canceled: onCanceled.Invoke(GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void Match(Action<Result<TSuccess>> onSuccess, Action<Exception> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed:
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchSuccessOrFailed(Action<Result<TSuccess>> onSuccess, Action<Exception> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: onFailed.Invoke(GetError()); break;
                case OptionState.Canceled: break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchSuccessOrCanceled(Action<Result<TSuccess>> onSuccess, Action<Exception> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: break;
                case OptionState.Canceled: onCanceled.Invoke(GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchFailedOrCanceled(Action<Exception> onFailed, Action<Exception> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed: onFailed.Invoke(GetError()); break;
                case OptionState.Canceled: onCanceled.Invoke(GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchFailedOrCanceled(Action<Exception> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed:
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void Match(
            Action<Result<TSuccess>> onSuccess,
            Action<Result<TFailed>, Exception> onFailed,
            Action<Result<TCanceled>, Exception> onCanceled
        ) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: onFailed.Invoke(GetResultFailed(), GetError()); break;
                case OptionState.Canceled: onCanceled.Invoke(GetResultCanceled(), GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void Match(Action<Result<TSuccess>> onSuccess, Action<Result, Exception> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: onFailedOrCanceled.Invoke(GetResultFailed().ToResult(), GetError()); break;
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetResultCanceled().ToResult(), GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchSuccessOrFailed(Action<Result<TSuccess>> onSuccess, Action<Result<TFailed>, Exception> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: onFailed.Invoke(GetResultFailed(), GetError()); break;
                case OptionState.Canceled: break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchSuccessOrCanceled(Action<Result<TSuccess>> onSuccess, Action<Result<TCanceled>, Exception> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: break;
                case OptionState.Canceled: onCanceled.Invoke(GetResultCanceled(), GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchFailedOrCanceled(Action<Result<TFailed>, Exception> onFailed, Action<Result<TCanceled>, Exception> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed: onFailed.Invoke(GetResultFailed(), GetError()); break;
                case OptionState.Canceled: onCanceled.Invoke(GetResultCanceled(), GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchFailedOrCanceled(Action<Result, Exception> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed: onFailedOrCanceled.Invoke(GetResultFailed().ToResult(), GetError()); break;
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetResultCanceled().ToResult(), GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }
        #endregion

        public bool Equals(Option<TSuccess, TFailed, TCanceled> other) {
            return (_state, other._state) switch {
                (OptionState.Success, OptionState.Success) =>
                    _valueSuccess.Equals(other._valueSuccess),
                (OptionState.Failed, OptionState.Failed) =>
                    _valueFailed.Equals(other._valueFailed) && ExceptionUtility.EqualValue(_error, other._error),
                (OptionState.Canceled, OptionState.Canceled) =>
                    _valueCanceled.Equals(other._valueCanceled) && ExceptionUtility.EqualValue(_error, other._error),
                _ => false
            };
        }

        public override bool Equals(object obj)
            => obj is Option<TSuccess, TFailed, TCanceled> other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(
            (int)_state,
            _state switch {
                OptionState.Success => _valueSuccess.GetHashCode(),
                OptionState.Failed => _valueFailed.GetHashCode(),
                OptionState.Canceled => _valueCanceled.GetHashCode(),
                _ => 0
            },
            _error?.GetHashCode() ?? 0
        );

        public override string ToString() {
            return _state switch {
                OptionState.Success => $"Success; Value: {{{GetResultSuccess()}}}",
                OptionState.Failed => $"Failed; Error = {GetError().Message}; Value: {{{GetResultFailed()}}}",
                OptionState.Canceled => $"Canceled; Value: {{{GetResultCanceled()}}}",
                _ => throw new OptionInvalidStateException(),
            };
        }

        public int CompareTo(Option<TSuccess, TFailed, TCanceled> other) {
            return (_state, other._state) switch {
                (OptionState.Success, OptionState.Success) => _valueSuccess.CompareTo(other._valueSuccess),
                (OptionState.Success, OptionState.Failed) => 1,
                (OptionState.Success, OptionState.Canceled) => 1,
                (OptionState.Failed, OptionState.Failed) =>
                    _valueFailed.IsOk() && other._valueFailed.IsOk()
                        ? _valueFailed.CompareTo(other._valueFailed)
                        : _valueFailed.IsOk()
                            ? 1
                            : other._valueFailed.IsOk()
                                ? -1
                                : Comparer<Exception>.Default.Compare(_error, other._error),
                (OptionState.Failed, OptionState.Success) => -1,
                (OptionState.Failed, OptionState.Canceled) => 1,
                (OptionState.Canceled, OptionState.Canceled) =>
                    _valueCanceled.IsOk() && other._valueCanceled.IsOk()
                        ? _valueCanceled.CompareTo(other._valueCanceled)
                        : _valueCanceled.IsOk()
                            ? 1
                            : other._valueCanceled.IsOk()
                                ? -1
                                : 0,
                (OptionState.Canceled, OptionState.Success) => -1,
                (OptionState.Canceled, OptionState.Failed) => -1,
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

        public Option ToOption() => ToOption(this);
        public Option<TSuccess, TFailed> BuildCompact() => BuildCompact(this);

        static private Option ToOption(Option<TSuccess, TFailed, TCanceled> option) {
            if (option.IsSuccess(out Result<TSuccess> resultSuccess)) {
                return resultSuccess.IsOk(out var value)
                    ? Option.Success(value)
                    : Option.Success();
            }

            if (option.IsFailed(out Result<TFailed> resultFailed)) {
                return resultFailed.IsOk(out var value)
                    ? Option.Failed(option._error, value)
                    : Option.Failed(option._error);
            }

            if (option.IsCanceled(out Result<TCanceled> resultCanceled)) {
                return resultCanceled.IsOk(out var value)
                    ? Option.Canceled(value)
                    : Option.Canceled();
            }

            throw new OptionInvalidStateException();
        }

        static private Option<TSuccess, TFailed> BuildCompact(Option<TSuccess, TFailed, TCanceled> option) {
            if (!option.IsSuccess() && typeof(TFailed) != typeof(TCanceled)) throw new OptionException("TFailed must be of type TCanceled");
            return ToOption(option).ToOption<TSuccess, TFailed>();
        }
    }
}
