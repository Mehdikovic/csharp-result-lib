// ReSharper disable CheckNamespace
// ReSharper disable ArrangeModifiersOrder
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;

using ResultLib.Core;

using static ResultLib.Core.ArgumentNullExceptionExtension;

namespace ResultLib {
    public interface IOption<out TSuccess, out TError> {
        bool IsSuccess();
        bool IsFailed();
        bool IsCanceled();
        bool IsSuccessOrCanceled() => IsSuccess() || IsCanceled();
        bool IsSuccessOrFailed() => IsSuccess() || IsFailed();
        bool IsFailedOrCanceled() => IsFailed() || IsCanceled();
        IResult<TSuccess> UnwrapBoxingSuccess();
        IResult<TError> UnwrapBoxingFailed();
        IResult<TError> UnwrapBoxingCanceled();
        Exception GetError();
    }

    public readonly struct Option<TSuccess, TError> :
        IOption<TSuccess, TError>,
        IEquatable<Option<TSuccess, TError>>,
        IComparable<Option<TSuccess, TError>> {
        private readonly OptionState _state;
        private readonly Result<TSuccess> _valueSuccess;
        private readonly Result<TError> _valueFailed;
        private readonly Result<TError> _valueCanceled;
        private readonly Exception _error;

        private Option(OptionState state, Exception error, Result<TSuccess> success, Result<TError> failed, Result<TError> canceled) {
            _state = state;
            _error = error;
            _valueSuccess = success;
            _valueFailed = failed;
            _valueCanceled = canceled;
        }

        static public Option<TSuccess, TError> Success() =>
            new Option<TSuccess, TError>(
                OptionState.Success,
                error: null,
                success: Result<TSuccess>.Error(),
                failed: Result<TError>.Error(),
                canceled: Result<TError>.Error()
            );

        static public Option<TSuccess, TError> Success(TSuccess value) =>
            new Option<TSuccess, TError>(
                OptionState.Success,
                error: null,
                success: Result<TSuccess>.FromRequired(value),
                failed: Result<TError>.Error(),
                canceled: Result<TError>.Error()
            );

        static public Option<TSuccess, TError> Failed() =>
            new Option<TSuccess, TError>(
                OptionState.Failed,
                error: null,
                success: Result<TSuccess>.Error(),
                failed: Result<TError>.Error(),
                canceled: Result<TError>.Error()
            );

        static public Option<TSuccess, TError> Failed(string error) =>
            new Option<TSuccess, TError>(
                OptionState.Failed,
                error: new OptionException(error),
                success: Result<TSuccess>.Error(),
                failed: Result<TError>.Error(),
                canceled: Result<TError>.Error()
            );

        static public Option<TSuccess, TError> Failed(string error, TError value) =>
            new Option<TSuccess, TError>(
                OptionState.Failed,
                error: new OptionException(error),
                success: Result<TSuccess>.Error(),
                failed: Result<TError>.FromRequired(value),
                canceled: Result<TError>.Error()
            );

        static public Option<TSuccess, TError> Failed(Exception exception) =>
            new Option<TSuccess, TError>(
                OptionState.Failed,
                error: exception,
                success: Result<TSuccess>.Error(),
                failed: Result<TError>.Error(),
                canceled: Result<TError>.Error()
            );

        static public Option<TSuccess, TError> Failed(Exception exception, TError value) =>
            new Option<TSuccess, TError>(
                OptionState.Failed,
                error: exception,
                success: Result<TSuccess>.Error(),
                failed: Result<TError>.FromRequired(value),
                canceled: Result<TError>.Error()
            );

        static public Option<TSuccess, TError> Canceled() =>
            new Option<TSuccess, TError>(
                OptionState.Canceled,
                error: null,
                success: Result<TSuccess>.Error(),
                failed: Result<TError>.Error(),
                canceled: Result<TError>.Error()
            );

        static public Option<TSuccess, TError> Canceled(TError value) =>
            new Option<TSuccess, TError>(
                OptionState.Canceled,
                error: null,
                success: Result<TSuccess>.Error(),
                failed: Result<TError>.Error(),
                canceled: Result<TError>.FromRequired(value)
            );

        public bool IsSuccess() => _state == OptionState.Success;
        public bool IsSuccess(out Result<TSuccess> value) {
            value = _valueSuccess;
            return IsSuccess();
        }

        public bool IsFailed() => _state == OptionState.Failed;
        public bool IsFailed(out Result<TError> value) {
            value = _valueFailed;
            return IsFailed();
        }

        public bool IsCanceled() => _state == OptionState.Canceled;
        public bool IsCanceled(out Result<TError> value) {
            value = _valueCanceled;
            return IsCanceled();
        }

        public bool IsSuccessOrCanceled() => IsSuccess() || IsCanceled();
        public bool IsSuccessOrCanceled(out Result<TSuccess> valueSuccess, out Result<TError> valueCanceled) {
            valueSuccess = _valueSuccess;
            valueCanceled = _valueCanceled;
            return IsSuccessOrCanceled();
        }

        public bool IsSuccessOrFailed() => IsSuccess() || IsFailed();
        public bool IsSuccessOrFailed(out Result<TSuccess> valueSuccess, out Result<TError> valueFailed) {
            valueSuccess = _valueSuccess;
            valueFailed = _valueFailed;
            return IsSuccessOrFailed();
        }

        public bool IsFailedOrCanceled() => IsFailed() || IsCanceled();
        public bool IsFailedOrCanceled(out Result<TError> valueFailed, out Result<TError> valueCanceled) {
            valueFailed = _valueFailed;
            valueCanceled = _valueCanceled;
            return IsFailedOrCanceled();
        }

        public Result<TSuccess> GetResultSuccess() => _valueSuccess;
        public IResult<TSuccess> UnwrapBoxingSuccess() => GetResultSuccess();
        public Result<TError> GetResultFailed() => _valueFailed;
        public IResult<TError> UnwrapBoxingFailed() => GetResultFailed();
        public Result<TError> GetResultCanceled() => _valueCanceled;
        public IResult<TError> UnwrapBoxingCanceled() => GetResultCanceled();

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
            Func<Result<TError>, TRet> onFailed,
            Func<Result<TError>, TRet> onCanceled
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

        public TRet Match<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Result<TError>, TRet> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Failed => onFailedOrCanceled.Invoke(GetResultFailed()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetResultCanceled()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Result<TError>, TRet> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Failed => onFailed.Invoke(GetResultFailed()),
                OptionState.Canceled => default,
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Result<TError>, TRet> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Failed => default,
                OptionState.Canceled => onCanceled.Invoke(GetResultCanceled()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Result<TError>, TRet> onFailed, Func<Result<TError>, TRet> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailed.Invoke(GetResultFailed()),
                OptionState.Canceled => onCanceled.Invoke(GetResultCanceled()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Result<TError>, TRet> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailedOrCanceled.Invoke(GetResultFailed()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetResultCanceled()),
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
            Func<Result<TError>, Exception, TRet> onFailed,
            Func<Result<TError>, Exception, TRet> onCanceled
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

        public TRet Match<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Result<TError>, Exception, TRet> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Failed => onFailedOrCanceled.Invoke(GetResultFailed(), GetError()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetResultCanceled(), GetError()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Result<TError>, Exception, TRet> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Failed => onFailed.Invoke(GetResultFailed(), GetError()),
                OptionState.Canceled => default,
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Result<TError>, Exception, TRet> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Failed => default,
                OptionState.Canceled => onCanceled.Invoke(GetResultCanceled(), GetError()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Result<TError>, Exception, TRet> onFailed, Func<Result<TError>, Exception, TRet> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailed.Invoke(GetResultFailed(), GetError()),
                OptionState.Canceled => onCanceled.Invoke(GetResultCanceled(), GetError()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Result<TError>, Exception, TRet> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailedOrCanceled.Invoke(GetResultFailed(), GetError()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetResultCanceled(), GetError()),
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

        public void Match(Action<Result<TSuccess>> onSuccess, Action<Result<TError>> onFailed, Action<Result<TError>> onCanceled) {
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

        public void Match(Action<Result<TSuccess>> onSuccess, Action<Result<TError>> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: onFailedOrCanceled.Invoke(GetResultFailed()); break;
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetResultCanceled()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchSuccessOrFailed(Action<Result<TSuccess>> onSuccess, Action<Result<TError>> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: onFailed.Invoke(GetResultFailed()); break;
                case OptionState.Canceled: break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchSuccessOrCanceled(Action<Result<TSuccess>> onSuccess, Action<Result<TError>> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: break;
                case OptionState.Canceled: onCanceled.Invoke(GetResultCanceled()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchFailedOrCanceled(Action<Result<TSuccess>> onSuccess, Action<Result<TError>> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: onFailed.Invoke(GetResultFailed()); break;
                case OptionState.Canceled: break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchFailedOrCanceled(Action<Result<TError>> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed: onFailedOrCanceled.Invoke(GetResultFailed()); break;
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetResultCanceled()); break;
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
            Action<Result<TError>, Exception> onFailed,
            Action<Result<TError>, Exception> onCanceled
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

        public void Match(Action<Result<TSuccess>> onSuccess, Action<Result<TError>, Exception> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: onFailedOrCanceled.Invoke(GetResultFailed(), GetError()); break;
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetResultCanceled(), GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchSuccessOrFailed(Action<Result<TSuccess>> onSuccess, Action<Result<TError>, Exception> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: onFailed.Invoke(GetResultFailed(), GetError()); break;
                case OptionState.Canceled: break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchSuccessOrCanceled(Action<Result<TSuccess>> onSuccess, Action<Result<TError>, Exception> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: break;
                case OptionState.Canceled: onCanceled.Invoke(GetResultCanceled(), GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchFailedOrCanceled(Action<Result<TError>, Exception> onFailed, Action<Result<TError>, Exception> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed: onFailed.Invoke(GetResultFailed(), GetError()); break;
                case OptionState.Canceled: onCanceled.Invoke(GetResultCanceled(), GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchFailedOrCanceled(Action<Result<TError>, Exception> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed: onFailedOrCanceled.Invoke(GetResultFailed(), GetError()); break;
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetResultCanceled(), GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }
        #endregion

        public bool Equals(Option<TSuccess, TError> other) {
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
            => obj is Option<TSuccess, TError> other && Equals(other);

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

        public int CompareTo(Option<TSuccess, TError> other) {
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

        public static bool operator ==(Option<TSuccess, TError> left, Option<TSuccess, TError> right)
            => left.Equals(right);

        public static bool operator !=(Option<TSuccess, TError> left, Option<TSuccess, TError> right)
            => !left.Equals(right);

        public static bool operator >(Option<TSuccess, TError> left, Option<TSuccess, TError> right)
            => left.CompareTo(right) > 0;

        public static bool operator <(Option<TSuccess, TError> left, Option<TSuccess, TError> right)
            => left.CompareTo(right) < 0;

        public static bool operator >=(Option<TSuccess, TError> left, Option<TSuccess, TError> right)
            => left.CompareTo(right) >= 0;

        public static bool operator <=(Option<TSuccess, TError> left, Option<TSuccess, TError> right)
            => left.CompareTo(right) <= 0;

        public Option ToOption() => ToOption(this);

        static private Option ToOption(Option<TSuccess, TError> option) {
            if (option.IsSuccess(out Result<TSuccess> resultSuccess)) {
                return resultSuccess.IsOk(out var value)
                    ? Option.Success(value)
                    : Option.Success();
            }

            if (option.IsFailed(out Result<TError> resultFailed)) {
                return resultFailed.IsOk(out var value)
                    ? Option.Failed(option._error, value)
                    : Option.Failed(option._error);
            }

            if (option.IsCanceled(out Result<TError> resultCanceled)) {
                return resultCanceled.IsOk(out var value)
                    ? Option.Canceled(value)
                    : Option.Canceled();
            }

            throw new OptionInvalidStateException();
        }
    }
}
