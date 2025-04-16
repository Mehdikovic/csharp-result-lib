// ReSharper disable CheckNamespace
// ReSharper disable ArrangeModifiersOrder
// ReSharper disable MemberCanBePrivate.Global

using System;

using ResultLib.Core;

using static System.ArgumentNullException;

namespace ResultLib {
    public struct Option() : IEquatable<Option>, IComparable<Option> {
        private OptionState _state = OptionState.Failed;
        private Result _value = Result.Error();
        private Exception _error = ErrorFactory.Option.EmptyConstructor();

        static public Option Success() =>
            new Option { _state = OptionState.Success, _value = Result.Error() };

        static public Option Success(object value) =>
            new Option { _state = OptionState.Success, _value = Result.Create(value) };

        static public Option Failed() =>
            new Option { _state = OptionState.Failed, _error = ErrorFactory.Option.Default(), _value = Result.Error() };

        static public Option Failed(string error) =>
            new Option { _state = OptionState.Failed, _error = ErrorFactory.Option.Create(error), _value = Result.Error() };

        static public Option Failed(string error, object value) =>
            new Option { _state = OptionState.Failed, _error = ErrorFactory.Option.Create(error), _value = Result.Create(value) };

        static public Option Failed(Exception exception) =>
            new Option { _state = OptionState.Failed, _error = exception ?? ErrorFactory.Option.Default(), _value = Result.Error() };

        static public Option Failed(Exception exception, object value) =>
            new Option { _state = OptionState.Failed, _error = exception ?? ErrorFactory.Option.Default(), _value = Result.Create(value) };

        static public Option Canceled() =>
            new Option { _state = OptionState.Canceled, _error = ErrorFactory.Option.Cancel(), _value = Result.Error() };

        static public Option Canceled(object value) =>
            new Option { _state = OptionState.Canceled, _error = ErrorFactory.Option.Cancel(), _value = Result.Create(value) };

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
        public Exception GetError() => _error ?? ErrorFactory.Option.NullUnwrapErr();

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

        public TRet MatchSuccessOrFailed<TRet>(Func<Result, TRet> onSuccess, Func<Exception, TRet> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailed.Invoke(GetError()),
                _ => default
            };
        }

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result, TRet> onSuccess, Func<Exception, TRet> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Canceled => onCanceled.Invoke(GetError()),
                _ => default
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Exception, TRet> onFailed, Func<Exception, TRet> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Failed => onFailed.Invoke(GetError()),
                OptionState.Canceled => onCanceled.Invoke(GetError()),
                _ => default
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

        public TRet MatchSuccessOrFailed<TRet>(Func<Result, TRet> onSuccess, Func<Result, Exception, TRet> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailed.Invoke(GetResult(), GetError()),
                _ => default
            };
        }

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result, TRet> onSuccess, Func<Result, Exception, TRet> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Canceled => onCanceled.Invoke(GetResult(), GetError()),
                _ => default
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Result, Exception, TRet> onFailed, Func<Result, Exception, TRet> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Failed => onFailed.Invoke(GetResult(), GetError()),
                OptionState.Canceled => onCanceled.Invoke(GetResult(), GetError()),
                _ => default
            };
        }

        public void Match(Action<Result> onSuccess, Action<Exception> onFailed, Action<Exception> onCanceled) {
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
