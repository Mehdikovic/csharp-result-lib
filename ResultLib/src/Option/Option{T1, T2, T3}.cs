// ReSharper disable CheckNamespace
// ReSharper disable ArrangeModifiersOrder
// ReSharper disable MemberCanBePrivate.Global

using System;

using ResultLib.Core;

using static System.ArgumentNullException;

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

    public struct Option<TSuccess, TFailed, TCanceled>() :
        IOption<TSuccess, TFailed, TCanceled>,
        IEquatable<Option<TSuccess, TFailed, TCanceled>>,
        IComparable<Option<TSuccess, TFailed, TCanceled>> {
        private OptionState _state = OptionState.Failed;
        private Result<TSuccess> _valueSuccess = Result<TSuccess>.Error();
        private Result<TFailed> _valueFailed = Result<TFailed>.Error();
        private Result<TCanceled> _valueCanceled = Result<TCanceled>.Error();
        private Exception _error = ErrorFactory.Option.EmptyConstructor();


        static public Option<TSuccess, TFailed, TCanceled> Success() =>
            new Option<TSuccess, TFailed, TCanceled> {
                _state = OptionState.Success,
                _valueSuccess = Result<TSuccess>.Error(),
                _valueFailed = Result<TFailed>.Error(),
                _valueCanceled = Result<TCanceled>.Error(),
            };

        static public Option<TSuccess, TFailed, TCanceled> Success(TSuccess value) =>
            new Option<TSuccess, TFailed, TCanceled> {
                _state = OptionState.Success,
                _valueSuccess = Result<TSuccess>.Create(value),
                _valueFailed = Result<TFailed>.Error(),
                _valueCanceled = Result<TCanceled>.Error(),
            };

        static public Option<TSuccess, TFailed, TCanceled> Failed() =>
            new Option<TSuccess, TFailed, TCanceled> {
                _state = OptionState.Failed,
                _valueSuccess = Result<TSuccess>.Error(),
                _valueFailed = Result<TFailed>.Error(),
                _valueCanceled = Result<TCanceled>.Error(),
                _error = ErrorFactory.Option.Default(),
            };

        static public Option<TSuccess, TFailed, TCanceled> Failed(string error) =>
            new Option<TSuccess, TFailed, TCanceled> {
                _state = OptionState.Failed,
                _valueSuccess = Result<TSuccess>.Error(),
                _valueFailed = Result<TFailed>.Error(),
                _valueCanceled = Result<TCanceled>.Error(),
                _error = ErrorFactory.Option.Create(error),
            };

        static public Option<TSuccess, TFailed, TCanceled> Failed(string error, TFailed value) =>
            new Option<TSuccess, TFailed, TCanceled> {
                _state = OptionState.Failed,
                _valueSuccess = Result<TSuccess>.Error(),
                _valueFailed = Result<TFailed>.Create(value),
                _valueCanceled = Result<TCanceled>.Error(),
                _error = ErrorFactory.Option.Create(error),
            };

        static public Option<TSuccess, TFailed, TCanceled> Failed(Exception exception) =>
            new Option<TSuccess, TFailed, TCanceled> {
                _state = OptionState.Failed,
                _valueSuccess = Result<TSuccess>.Error(),
                _valueFailed = Result<TFailed>.Error(),
                _valueCanceled = Result<TCanceled>.Error(),
                _error = exception ?? ErrorFactory.Option.Default()
            };

        static public Option<TSuccess, TFailed, TCanceled> Failed(Exception exception, TFailed value) =>
            new Option<TSuccess, TFailed, TCanceled> {
                _state = OptionState.Failed,
                _valueSuccess = Result<TSuccess>.Error(),
                _valueFailed = Result<TFailed>.Create(value),
                _valueCanceled = Result<TCanceled>.Error(),
                _error = exception ?? ErrorFactory.Option.Default()
            };

        static public Option<TSuccess, TFailed, TCanceled> Canceled() =>
            new Option<TSuccess, TFailed, TCanceled> {
                _state = OptionState.Canceled,
                _valueSuccess = Result<TSuccess>.Error(),
                _valueFailed = Result<TFailed>.Error(),
                _valueCanceled = Result<TCanceled>.Error(),
                _error = ErrorFactory.Option.Cancel()
            };

        static public Option<TSuccess, TFailed, TCanceled> Canceled(TCanceled value) =>
            new Option<TSuccess, TFailed, TCanceled> {
                _state = OptionState.Canceled,
                _valueSuccess = Result<TSuccess>.Error(),
                _valueFailed = Result<TFailed>.Error(),
                _valueCanceled = Result<TCanceled>.Create(value),
                _error = ErrorFactory.Option.Cancel()
            };

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
        public Result<TFailed> GetResultFailed() => _valueFailed;
        public Result<TCanceled> GetResultCanceled() => _valueCanceled;
        public Exception GetError() => _error ?? ErrorFactory.Option.NullUnwrapErr();
        public IResult<TSuccess> UnwrapBoxingSuccess() => GetResultSuccess();
        public IResult<TFailed> UnwrapBoxingFailed() => GetResultFailed();
        public IResult<TCanceled> UnwrapBoxingCanceled() => GetResultCanceled();

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
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Exception, TRet> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Failed => onFailed.Invoke(GetError()),
                _ => default
            };
        }

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Exception, TRet> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
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
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Result<TFailed>, Exception, TRet> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Failed => onFailed.Invoke(GetResultFailed(), GetError()),
                _ => default
            };
        }

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result<TSuccess>, TRet> onSuccess, Func<Result<TCanceled>, Exception, TRet> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResultSuccess()),
                OptionState.Canceled => onCanceled.Invoke(GetResultCanceled(), GetError()),
                _ => default
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Result<TFailed>, Exception, TRet> onFailed, Func<Result<TCanceled>, Exception, TRet> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Failed => onFailed.Invoke(GetResultFailed(), GetError()),
                OptionState.Canceled => onCanceled.Invoke(GetResultCanceled(), GetError()),
                _ => default
            };
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
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchSuccessOrFailed(Action<Result<TSuccess>> onSuccess, Action<Exception> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: onFailed.Invoke(GetError()); break;
                case OptionState.Canceled: break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchSuccessOrCanceled(Action<Result<TSuccess>> onSuccess, Action<Exception> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
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
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchSuccessOrFailed(Action<Result<TSuccess>> onSuccess, Action<Result<TFailed>, Exception> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: onFailed.Invoke(GetResultFailed(), GetError()); break;
                case OptionState.Canceled: break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchSuccessOrCanceled(Action<Result<TSuccess>> onSuccess, Action<Result<TCanceled>, Exception> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResultSuccess()); break;
                case OptionState.Failed: break;
                case OptionState.Canceled: onCanceled.Invoke(GetResultCanceled(), GetError()); break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchFailedOrCanceled(Action<Result<TFailed>, Exception> onFailed, Action<Result<TCanceled>, Exception> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed: onFailed.Invoke(GetResultFailed(), GetError()); break;
                case OptionState.Canceled: onCanceled.Invoke(GetResultCanceled(), GetError()); break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

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
                OptionState.Success => "Success; Value: {{{0}}}".Format(GetResultSuccess()),
                OptionState.Failed => "Failed; Error = {0}; Value: {{{1}}}".Format(GetError(), GetResultFailed()),
                OptionState.Canceled => "Canceled; Error = {0}; Value: {{{1}}}".Format(GetError(), GetResultCanceled()),
                _ => "Error:: Unrecognized State"
            };
        }

        public int CompareTo(Option<TSuccess, TFailed, TCanceled> other) {
            return (_state, other._state) switch {
                (OptionState.Success, OptionState.Success) => _valueSuccess.CompareTo(other._valueSuccess),
                (OptionState.Success, OptionState.Failed) => -1,
                (OptionState.Success, OptionState.Canceled) => -1,
                (OptionState.Failed, OptionState.Failed) =>
                    (_valueFailed.IsOk() && other._valueFailed.IsOk())
                        ? _valueFailed.CompareTo(other._valueFailed)
                        : _valueFailed.IsOk()
                            ? -1
                            : other._valueFailed.IsOk()
                                ? 1
                                : 0,
                (OptionState.Failed, OptionState.Success) => 1,
                (OptionState.Failed, OptionState.Canceled) => -1,
                (OptionState.Canceled, OptionState.Canceled) =>
                    (_valueCanceled.IsOk() && other._valueCanceled.IsOk())
                        ? _valueCanceled.CompareTo(other._valueCanceled)
                        : _valueCanceled.IsOk()
                            ? -1
                            : other._valueCanceled.IsOk()
                                ? 1
                                : 0,
                (OptionState.Canceled, OptionState.Success) => 1,
                (OptionState.Canceled, OptionState.Failed) => 1,
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

        static public implicit operator Option<TSuccess, TFailed, TCanceled>(Option option) {
            if (option.GetResult().IsOk(out object obj)) {
                if (obj is null) return Failed(ErrorFactory.Option.InvalidIsOkCastOperation());
                if (obj is not (TSuccess and TFailed and TCanceled)) return Failed(ErrorFactory.Option.InvalidIsOkCastOperation());
            }

            if (option.IsSuccess(out var result)) {
                if (result.IsError()) return Success();
                return result.Some<TSuccess>(out var some)
                    ? Success(some)
                    : Failed(ErrorFactory.Option.InvalidImplicitUnboxingCast(obj.GetType(), typeof(TSuccess)));
            }

            if (option.IsFailed(out result)) {
                if (result.IsError()) return Failed();
                return result.Some<TFailed>(out var some)
                    ? Failed(option.GetError(), some)
                    : Failed(ErrorFactory.Option.InvalidImplicitUnboxingCast(obj.GetType(), typeof(TFailed)));
            }

            if (option.IsCanceled(out result)) {
                if (result.IsError()) return Canceled();
                return result.Some<TCanceled>(out var some)
                    ? Canceled(some)
                    : Failed(ErrorFactory.Option.InvalidImplicitUnboxingCast(obj.GetType(), typeof(TCanceled)));
            }

            // we won't reach here
            return default;
        }

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

            // we won't reach here
            return default;
        }
    }
}
