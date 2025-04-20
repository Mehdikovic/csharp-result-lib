// ReSharper disable CheckNamespace
// ReSharper disable ArrangeModifiersOrder
// ReSharper disable MemberCanBePrivate.Global

using System;

using ResultLib.Core;

using static ResultLib.Core.ArgumentNullExceptionExtension;

namespace ResultLib {
    public readonly struct Option : IEquatable<Option>, IComparable<Option> {
        private readonly OptionState _state;
        private readonly Result _value;
        private readonly Exception _error;

        private Option(OptionState state, Exception error, Result value) {
            _state = state;
            _error = error;
            _value = value;
        }

        static public Option Success() =>
            new Option(OptionState.Success, error: null, value: Result.Error());

        static public Option Success(object value) =>
            new Option(OptionState.Success, error: null, value: Result.FromRequired(value));

        static public Option Failed() =>
            new Option(OptionState.Failed, error: null, value: Result.Error());

        static public Option Failed(string error) =>
            new Option(OptionState.Failed, error: ErrorFactory.Option.Create(error), value: Result.Error());

        static public Option Failed(string error, object value) =>
            new Option(OptionState.Failed, error: ErrorFactory.Option.Create(error), value: Result.FromRequired(value));

        static public Option Failed(Exception exception) =>
            new Option(OptionState.Failed, error: exception, value: Result.Error());

        static public Option Failed(Exception exception, object value) =>
            new Option(OptionState.Failed, error: exception, value: Result.FromRequired(value));

        static public Option Canceled() =>
            new Option(OptionState.Canceled, error: null, value: Result.Error());

        static public Option Canceled(object value) =>
            new Option(OptionState.Canceled, error: null, value: Result.FromRequired(value));

        public bool IsSuccess() => _state == OptionState.Success;
        public bool IsSuccess(out Result value) {
            value = _value;
            return IsSuccess();
        }

        public bool IsFailed() => _state == OptionState.Failed;
        public bool IsFailed(out Result value) {
            value = _value;
            return IsFailed();
        }

        public bool IsCanceled() => _state == OptionState.Canceled;
        public bool IsCanceled(out Result value) {
            value = _value;
            return IsCanceled();
        }

        public bool IsSuccessOrCanceled() => IsSuccess() || IsCanceled();
        public bool IsSuccessOrCanceled(out Result value) {
            value = _value;
            return IsSuccessOrCanceled();
        }

        public bool IsSuccessOrFailed() => IsSuccess() || IsFailed();
        public bool IsSuccessOrFailed(out Result value) {
            value = _value;
            return IsSuccessOrFailed();
        }

        public bool IsFailedOrCanceled() => IsFailed() || IsCanceled();
        public bool IsFailedOrCanceled(out Result value) {
            value = _value;
            return IsFailedOrCanceled();
        }

        public Result GetResult() => _value;

        internal Exception GetErrorInternal() => _error;
        public Exception GetError() {
            if (_error != null) return _error;

            return _state switch {
                OptionState.Success => ErrorFactory.Option.NullUnwrapErr(),
                OptionState.Failed => ErrorFactory.Option.Default(),
                OptionState.Canceled => ErrorFactory.Option.Cancel(),
                _ => throw ErrorFactory.Option.InvalidStateWhenGettingError(),
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
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet Match<TRet>(Func<TRet> onSuccess, Func<TRet> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(),
                OptionState.Failed => onFailedOrCanceled.Invoke(),
                OptionState.Canceled => onFailedOrCanceled.Invoke(),
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<TRet> onSuccess, Func<TRet> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(),
                OptionState.Failed => onFailed.Invoke(),
                OptionState.Canceled => default,
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet MatchSuccessOrCanceled<TRet>(Func<TRet> onSuccess, Func<TRet> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(),
                OptionState.Failed => default,
                OptionState.Canceled => onCanceled.Invoke(),
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<TRet> onFailed, Func<TRet> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailed.Invoke(),
                OptionState.Canceled => onCanceled.Invoke(),
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<TRet> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailedOrCanceled.Invoke(),
                OptionState.Canceled => onFailedOrCanceled.Invoke(),
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet Match<TRet>(
            Func<Result, TRet> onSuccess,
            Func<Result, TRet> onFailed,
            Func<Result, TRet> onCanceled
        ) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailed.Invoke(GetResult()),
                OptionState.Canceled => onCanceled.Invoke(GetResult()),
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet Match<TRet>(Func<Result, TRet> onSuccess, Func<Result, TRet> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailedOrCanceled.Invoke(GetResult()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetResult()),
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<Result, TRet> onSuccess, Func<Result, TRet> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailed.Invoke(GetResult()),
                OptionState.Canceled => default,
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result, TRet> onSuccess, Func<Result, TRet> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => default,
                OptionState.Canceled => onCanceled.Invoke(GetResult()),
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Result, TRet> onFailed, Func<Result, TRet> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailed.Invoke(GetResult()),
                OptionState.Canceled => onCanceled.Invoke(GetResult()),
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Result, TRet> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailedOrCanceled.Invoke(GetResult()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetResult()),
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet Match<TRet>(
            Func<Result, TRet> onSuccess,
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
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet Match<TRet>(Func<Result, TRet> onSuccess, Func<Exception, TRet> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailedOrCanceled.Invoke(GetError()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetError()),
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<Result, TRet> onSuccess, Func<Exception, TRet> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailed.Invoke(GetError()),
                OptionState.Canceled => default,
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result, TRet> onSuccess, Func<Exception, TRet> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => default,
                OptionState.Canceled => onCanceled.Invoke(GetError()),
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Exception, TRet> onFailed, Func<Exception, TRet> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailed.Invoke(GetError()),
                OptionState.Canceled => onCanceled.Invoke(GetError()),
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Exception, TRet> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailedOrCanceled.Invoke(GetError()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetError()),
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet Match<TRet>(
            Func<Result, TRet> onSuccess,
            Func<Result, Exception, TRet> onFailed,
            Func<Result, Exception, TRet> onCanceled
        ) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailed.Invoke(GetResult(), GetError()),
                OptionState.Canceled => onCanceled.Invoke(GetResult(), GetError()),
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet Match<TRet>(Func<Result, TRet> onSuccess, Func<Result, Exception, TRet> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailedOrCanceled.Invoke(GetResult(), GetError()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetResult(), GetError()),
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<Result, TRet> onSuccess, Func<Result, Exception, TRet> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailed.Invoke(GetResult(), GetError()),
                OptionState.Canceled => default,
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result, TRet> onSuccess, Func<Result, Exception, TRet> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => default,
                OptionState.Canceled => onCanceled.Invoke(GetResult(), GetError()),
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Result, Exception, TRet> onFailed, Func<Result, Exception, TRet> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailed.Invoke(GetResult(), GetError()),
                OptionState.Canceled => onCanceled.Invoke(GetResult(), GetError()),
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Result, Exception, TRet> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            return _state switch {
                OptionState.Success => default,
                OptionState.Failed => onFailedOrCanceled.Invoke(GetResult(), GetError()),
                OptionState.Canceled => onFailedOrCanceled.Invoke(GetResult(), GetError()),
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
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
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }


        public void Match(Action onSuccess, Action onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(); break;
                case OptionState.Failed:
                case OptionState.Canceled: onFailedOrCanceled.Invoke(); break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchSuccessOrFailed(Action onSuccess, Action onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(); break;
                case OptionState.Failed: onFailed.Invoke(); break;
                case OptionState.Canceled: break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchSuccessOrCanceled(Action onSuccess, Action onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(); break;
                case OptionState.Failed: onFailed.Invoke(); break;
                case OptionState.Canceled: break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchFailedOrCanceled(Action onFailed, Action onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed: onFailed.Invoke(); break;
                case OptionState.Canceled: onCanceled.Invoke(); break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchFailedOrCanceled(Action onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed:
                case OptionState.Canceled: onFailedOrCanceled.Invoke(); break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void Match(
            Action<Result> onSuccess,
            Action<Result> onFailed,
            Action<Result> onCanceled
        ) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: onFailed.Invoke(GetResult()); break;
                case OptionState.Canceled: onCanceled.Invoke(GetResult()); break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void Match(Action<Result> onSuccess, Action<Result> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed:
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetResult()); break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchSuccessOrFailed(Action<Result> onSuccess, Action<Result> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: onFailed.Invoke(GetResult()); break;
                case OptionState.Canceled: break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchSuccessOrCanceled(Action<Result> onSuccess, Action<Result> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: break;
                case OptionState.Canceled: onCanceled.Invoke(GetResult()); break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchFailedOrCanceled(Action<Result> onSuccess, Action<Result> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: onFailed.Invoke(GetResult()); break;
                case OptionState.Canceled: break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchFailedOrCanceled(Action<Result> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed:
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetResult()); break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void Match(
            Action<Result> onSuccess,
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
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void Match(Action<Result> onSuccess, Action<Exception> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed:
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetError()); break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchSuccessOrFailed(Action<Result> onSuccess, Action<Exception> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: onFailed.Invoke(GetError()); break;
                case OptionState.Canceled: break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchSuccessOrCanceled(Action<Result> onSuccess, Action<Exception> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: break;
                case OptionState.Canceled: onCanceled.Invoke(GetError()); break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchFailedOrCanceled(Action<Exception> onFailed, Action<Exception> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed: onFailed.Invoke(GetError()); break;
                case OptionState.Canceled: onCanceled.Invoke(GetError()); break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchFailedOrCanceled(Action<Exception> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed:
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetError()); break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void Match(
            Action<Result> onSuccess,
            Action<Result, Exception> onFailed,
            Action<Result, Exception> onCanceled
        ) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: onFailed.Invoke(GetResult(), GetError()); break;
                case OptionState.Canceled: onCanceled.Invoke(GetResult(), GetError()); break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void Match(Action<Result> onSuccess, Action<Result, Exception> onFailedOrCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed:
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetResult(), GetError()); break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchSuccessOrFailed(Action<Result> onSuccess, Action<Result, Exception> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: onFailed.Invoke(GetResult(), GetError()); break;
                case OptionState.Canceled: break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchSuccessOrCanceled(Action<Result> onSuccess, Action<Result, Exception> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: break;
                case OptionState.Canceled: onCanceled.Invoke(GetResult(), GetError()); break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchFailedOrCanceled(Action<Result, Exception> onFailed, Action<Result, Exception> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed: onFailed.Invoke(GetResult(), GetError()); break;
                case OptionState.Canceled: onCanceled.Invoke(GetResult(), GetError()); break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchFailedOrCanceled(Action<Result, Exception> onFailedOrCanceled) {
            ThrowIfNull(onFailedOrCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed:
                case OptionState.Canceled: onFailedOrCanceled.Invoke(GetResult(), GetError()); break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }
        #endregion

        public bool Equals(Option other) {
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
            => obj is Option other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(
            (int)_state,
            _value.GetHashCode(),
            _error?.GetHashCode() ?? 0
        );

        public override string ToString() {
            return _state switch {
                OptionState.Success => "Success; Value: {{{0}}}".Format(GetResult()),
                OptionState.Failed => "Failed; Error = {0}; Value: {{{1}}}".Format(GetError(), GetResult()),
                OptionState.Canceled => "Canceled; Error = {0}; Value: {{{1}}}".Format(GetError(), GetResult()),
                _ => "Error:: Unrecognized State"
            };
        }

        public int CompareTo(Option other) {
            return (_state, other._state) switch {
                (OptionState.Success, OptionState.Success) => _value.CompareTo(other._value),
                (OptionState.Success, OptionState.Failed) => -1,
                (OptionState.Success, OptionState.Canceled) => -1,
                (OptionState.Failed, OptionState.Failed) =>
                    _value.IsOk() && other._value.IsOk()
                        ? _value.CompareTo(other._value)
                        : _value.IsOk()
                            ? -1
                            : other._value.IsOk()
                                ? 1
                                : 0,
                (OptionState.Failed, OptionState.Success) => 1,
                (OptionState.Failed, OptionState.Canceled) => -1,
                (OptionState.Canceled, OptionState.Canceled) =>
                    _value.IsOk() && other._value.IsOk()
                        ? _value.CompareTo(other._value)
                        : _value.IsOk()
                            ? -1
                            : other._value.IsOk()
                                ? 1
                                : 0,
                (OptionState.Canceled, OptionState.Success) => 1,
                (OptionState.Canceled, OptionState.Failed) => 1,
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
