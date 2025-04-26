// ReSharper disable CheckNamespace
// ReSharper disable ArrangeModifiersOrder
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;

using ResultLib.Core;

using static ResultLib.Core.ArgumentNullExceptionExtension;

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

    public readonly struct Option<T> : IOption<T>, IEquatable<Option<T>>, IComparable<Option<T>> {
        private readonly OptionState _state;
        private readonly Result<T> _value;
        private readonly Exception _error;

        private Option(OptionState state, Exception error, Result<T> value) {
            _state = state;
            _error = error;
            _value = value;
        }

        static public Option<T> Success() =>
            new Option<T>(OptionState.Success, error: null, value: Result<T>.Error());

        static public Option<T> Success(T value) =>
            new Option<T>(OptionState.Success, error: null, value: Result<T>.FromRequired(value));

        static public Option<T> Failed() =>
            new Option<T>(OptionState.Failed, error: null, value: Result<T>.Error());

        static public Option<T> Failed(string error) =>
            new Option<T>(OptionState.Failed, error: new OptionException(error), value: Result<T>.Error());

        static public Option<T> Failed(string error, T value) =>
            new Option<T>(OptionState.Failed, error: new OptionException(error), value: Result<T>.FromRequired(value));

        static public Option<T> Failed(Exception exception) =>
            new Option<T>(OptionState.Failed, error: exception, value: Result<T>.Error());

        static public Option<T> Failed(Exception exception, T value) =>
            new Option<T>(OptionState.Failed, error: exception, value: Result<T>.FromRequired(value));

        static public Option<T> Canceled() =>
            new Option<T>(OptionState.Canceled, error: null, value: Result<T>.Error());

        static public Option<T> Canceled(T value) =>
            new Option<T>(OptionState.Canceled, error: null, value: Result<T>.FromRequired(value));

        public bool IsSuccess() => _state == OptionState.Success;
        public bool IsSuccess(out Result<T> value) {
            value = _value;
            return IsSuccess();
        }

        public bool IsFailed() => _state == OptionState.Failed;
        public bool IsFailed(out Result<T> value) {
            value = _value;
            return IsFailed();
        }

        public bool IsCanceled() => _state == OptionState.Canceled;
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
        public IResult<T> UnwrapBoxing() => GetResult();

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
            Func<Result<T>, TRet> onSuccess,
            Func<Result<T>, TRet> onFailed,
            Func<Result<T>, TRet> onCanceled
        ) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailed.Invoke(GetResult()),
                OptionState.Canceled => onCanceled.Invoke(GetResult()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet Match<TRet>(Func<Result<T>, TRet> onSuccess, Func<Result<T>, TRet> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailedOrCanceled.Invoke(GetResult()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetResult()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<Result<T>, TRet> onSuccess, Func<Result<T>, TRet> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailed.Invoke(GetResult()),
                OptionState.Canceled => default,
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result<T>, TRet> onSuccess, Func<Result<T>, TRet> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => default,
                OptionState.Canceled => onCanceled.Invoke(GetResult()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Result<T>, TRet> onFailed, Func<Result<T>, TRet> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailed.Invoke(GetResult()),
                OptionState.Canceled => onCanceled.Invoke(GetResult()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Result<T>, TRet> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailedOrCanceled.Invoke(GetResult()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetResult()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet Match<TRet>(
            Func<Result<T>, TRet> onSuccess,
            Func<Exception, TRet> onFailed,
            Func<Exception, TRet> onCanceled
        ) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailed.Invoke(GetError()),
                OptionState.Canceled => onCanceled.Invoke(GetError()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet Match<TRet>(Func<Result<T>, TRet> onSuccess, Func<Exception, TRet> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailedOrCanceled.Invoke(GetError()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetError()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<Result<T>, TRet> onSuccess, Func<Exception, TRet> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailed.Invoke(GetError()),
                OptionState.Canceled => default,
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result<T>, TRet> onSuccess, Func<Exception, TRet> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
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
            Func<Result<T>, TRet> onSuccess,
            Func<Result<T>, Exception, TRet> onFailed,
            Func<Result<T>, Exception, TRet> onCanceled
        ) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailed.Invoke(GetResult(), GetError()),
                OptionState.Canceled => onCanceled.Invoke(GetResult(), GetError()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet Match<TRet>(Func<Result<T>, TRet> onSuccess, Func<Result<T>, Exception, TRet> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailedOrCanceled.Invoke(GetResult(), GetError()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetResult(), GetError()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<Result<T>, TRet> onSuccess, Func<Result<T>, Exception, TRet> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailed.Invoke(GetResult(), GetError()),
                OptionState.Canceled => default,
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result<T>, TRet> onSuccess, Func<Result<T>, Exception, TRet> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => default,
                OptionState.Canceled => onCanceled.Invoke(GetResult(), GetError()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Result<T>, Exception, TRet> onFailed, Func<Result<T>, Exception, TRet> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailed.Invoke(GetResult(), GetError()),
                OptionState.Canceled => onCanceled.Invoke(GetResult(), GetError()),
                _ => throw new OptionInvalidStateException()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Result<T>, Exception, TRet> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailedOrCanceled.Invoke(GetResult(), GetError()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetResult(), GetError()),
                _ => throw new OptionInvalidStateException()
            };
        }
        #endregion

        #region Match_Action
        public void Match(
            Action onSuccess,
            Action onFailed,
            Action onCanceled
        ) {
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

        public void Match(
            Action<Result<T>> onSuccess,
            Action<Result<T>> onFailed,
            Action<Result<T>> onCanceled
        ) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: onFailed.Invoke(GetResult()); break;
                case OptionState.Canceled: onCanceled.Invoke(GetResult()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void Match(Action<Result<T>> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed:
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetResult()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchSuccessOrFailed(Action<Result<T>> onSuccess, Action<Result<T>> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: onFailed.Invoke(GetResult()); break;
                case OptionState.Canceled: break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchSuccessOrCanceled(Action<Result<T>> onSuccess, Action<Result<T>> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: break;
                case OptionState.Canceled: onCanceled.Invoke(GetResult()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchFailedOrCanceled(Action<Result<T>> onSuccess, Action<Result<T>> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: onFailed.Invoke(GetResult()); break;
                case OptionState.Canceled: break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchSuccessOrFailedCanceled(Action<Result<T>> onSuccess, Action<Result<T>> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed:
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetResult()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void Match(
            Action<Result<T>> onSuccess,
            Action<Exception> onFailed,
            Action<Exception> onCanceled
        ) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: onFailed.Invoke(GetError()); break;
                case OptionState.Canceled: onCanceled.Invoke(GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void Match(Action<Result<T>> onSuccess, Action<Exception> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed:
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchSuccessOrFailed(Action<Result<T>> onSuccess, Action<Exception> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: onFailed.Invoke(GetError()); break;
                case OptionState.Canceled: break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchSuccessOrCanceled(Action<Result<T>> onSuccess, Action<Exception> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
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
            Action<Result<T>> onSuccess,
            Action<Result<T>, Exception> onFailed,
            Action<Result<T>, Exception> onCanceled
        ) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: onFailed.Invoke(GetResult(), GetError()); break;
                case OptionState.Canceled: onCanceled.Invoke(GetResult(), GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void Match(Action<Result<T>> onSuccess, Action<Result<T>, Exception> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed:
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetResult(), GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchSuccessOrFailed(Action<Result<T>> onSuccess, Action<Result<T>, Exception> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: onFailed.Invoke(GetResult(), GetError()); break;
                case OptionState.Canceled: break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchSuccessOrCanceled(Action<Result<T>> onSuccess, Action<Result<T>, Exception> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: break;
                case OptionState.Canceled: onCanceled.Invoke(GetResult(), GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchFailedOrCanceled(Action<Result<T>, Exception> onFailed, Action<Result<T>, Exception> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed: onFailed.Invoke(GetResult(), GetError()); break;
                case OptionState.Canceled: onCanceled.Invoke(GetResult(), GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }

        public void MatchFailedOrCanceled(Action<Result<T>, Exception> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed:
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetResult(), GetError()); break;
                default: throw new OptionInvalidStateException();
            }
        }
        #endregion

        public bool Equals(Option<T> other) {
            return (_state, other._state) switch {
                (OptionState.Success, OptionState.Success) =>
                    _value.Equals(other._value),
                (OptionState.Failed, OptionState.Failed) =>
                    _value.Equals(other._value) && ExceptionUtility.EqualValue(_error, other._error),
                (OptionState.Canceled, OptionState.Canceled) =>
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
            return _state switch {
                OptionState.Success => $"Success; Value: {{{GetResult()}}}",
                OptionState.Failed => $"Failed; Error = {GetError().Message}; Value: {{{GetResult()}}}",
                OptionState.Canceled => $"Canceled; Value: {{{GetResult()}}}",
                _ => throw new OptionInvalidStateException(),
            };
        }

        public int CompareTo(Option<T> other) {
            return (_state, other._state) switch {
                (OptionState.Success, OptionState.Success) => _value.CompareTo(other._value),
                (OptionState.Success, OptionState.Failed) => 1,
                (OptionState.Success, OptionState.Canceled) => 1,
                (OptionState.Failed, OptionState.Failed) =>
                    _value.IsOk() && other._value.IsOk()
                        ? _value.CompareTo(other._value)
                        : _value.IsOk()
                            ? 1
                            : other._value.IsOk()
                                ? -1
                                : Comparer<Exception>.Default.Compare(_error, other._error),
                (OptionState.Failed, OptionState.Success) => -1,
                (OptionState.Failed, OptionState.Canceled) => 1,
                (OptionState.Canceled, OptionState.Canceled) =>
                    _value.IsOk() && other._value.IsOk()
                        ? _value.CompareTo(other._value)
                        : _value.IsOk()
                            ? 1
                            : other._value.IsOk()
                                ? -1
                                : 0,
                (OptionState.Canceled, OptionState.Success) => -1,
                (OptionState.Canceled, OptionState.Failed) => -1,
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

        static private Option ToOption(Option<T> option) {
            if (option.IsSuccess(out Result<T> result)) {
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

            throw new OptionInvalidStateException();
        }
    }
}
