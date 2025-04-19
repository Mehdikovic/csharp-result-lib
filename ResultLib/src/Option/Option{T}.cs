// ReSharper disable CheckNamespace
// ReSharper disable ArrangeModifiersOrder
// ReSharper disable MemberCanBePrivate.Global

using System;

using ResultLib.Core;

using static System.ArgumentNullException;

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

    public struct Option<T>() : IOption<T>, IEquatable<Option<T>>, IComparable<Option<T>> {
        private OptionState _state = OptionState.Failed;
        private Result<T> _value = Result.Error();
        private Exception _error = ErrorFactory.Option.EmptyConstructor();

        static public Option<T> Success() =>
            new Option<T> { _state = OptionState.Success, _value = Result<T>.Error() };

        static public Option<T> Success(T value) =>
            new Option<T> { _state = OptionState.Success, _value = Result<T>.FromRequired(value) };

        static public Option<T> Failed() =>
            new Option<T> { _state = OptionState.Failed, _error = ErrorFactory.Option.Default(), _value = Result<T>.Error() };

        static public Option<T> Failed(string error) =>
            new Option<T> { _state = OptionState.Failed, _error = ErrorFactory.Option.Create(error), _value = Result<T>.Error() };

        static public Option<T> Failed(string error, T value) =>
            new Option<T> { _state = OptionState.Failed, _error = ErrorFactory.Option.Create(error), _value = Result<T>.FromRequired(value) };

        static public Option<T> Failed(Exception exception) =>
            new Option<T> { _state = OptionState.Failed, _error = exception ?? ErrorFactory.Option.Default(), _value = Result<T>.Error() };

        static public Option<T> Failed(Exception exception, T value) =>
            new Option<T> { _state = OptionState.Failed, _error = exception ?? ErrorFactory.Option.Default(), _value = Result<T>.FromRequired(value) };

        static public Option<T> Canceled() =>
            new Option<T> { _state = OptionState.Canceled, _error = ErrorFactory.Option.Cancel(), _value = Result<T>.Error() };

        static public Option<T> Canceled(T value) =>
            new Option<T> { _state = OptionState.Canceled, _error = ErrorFactory.Option.Cancel(), _value = Result<T>.FromRequired(value) };

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
        public Exception GetError() => _error ?? ErrorFactory.Option.NullUnwrapErr();
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

        public TRet MatchSuccessOrFailed<TRet>(Func<Result<T>, TRet> onSuccess, Func<Exception, TRet> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailed.Invoke(GetError()),
                _ => default
            };
        }

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result<T>, TRet> onSuccess, Func<Exception, TRet> onCanceled) {
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
                _ => throw ErrorFactory.Option.InvalidOperationMatch()
            };
        }

        public TRet MatchSuccessOrFailed<TRet>(Func<Result<T>, TRet> onSuccess, Func<Result<T>, Exception, TRet> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Failed => onFailed.Invoke(GetResult(), GetError()),
                _ => default
            };
        }

        public TRet MatchSuccessOrCanceled<TRet>(Func<Result<T>, TRet> onSuccess, Func<Result<T>, Exception, TRet> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Success => onSuccess.Invoke(GetResult()),
                OptionState.Canceled => onCanceled.Invoke(GetResult(), GetError()),
                _ => default
            };
        }

        public TRet MatchFailedOrCanceled<TRet>(Func<Result<T>, Exception, TRet> onFailed, Func<Result<T>, Exception, TRet> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            return _state switch {
                OptionState.Failed => onFailed.Invoke(GetResult(), GetError()),
                OptionState.Canceled => onCanceled.Invoke(GetResult(), GetError()),
                _ => default
            };
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
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchSuccessOrFailed(Action<Result<T>> onSuccess, Action<Exception> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: onFailed.Invoke(GetError()); break;
                case OptionState.Canceled: break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchSuccessOrCanceled(Action<Result<T>> onSuccess, Action<Exception> onCanceled) {
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
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchSuccessOrFailed(Action<Result<T>> onSuccess, Action<Result<T>, Exception> onFailed) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onFailed);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: onFailed.Invoke(GetResult(), GetError()); break;
                case OptionState.Canceled: break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchSuccessOrCanceled(Action<Result<T>> onSuccess, Action<Result<T>, Exception> onCanceled) {
            ThrowIfNull(onSuccess);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: onSuccess.Invoke(GetResult()); break;
                case OptionState.Failed: break;
                case OptionState.Canceled: onCanceled.Invoke(GetResult(), GetError()); break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

        public void MatchFailedOrCanceled(Action<Result<T>, Exception> onFailed, Action<Result<T>, Exception> onCanceled) {
            ThrowIfNull(onFailed);
            ThrowIfNull(onCanceled);

            switch (_state) {
                case OptionState.Success: break;
                case OptionState.Failed: onFailed.Invoke(GetResult(), GetError()); break;
                case OptionState.Canceled: onCanceled.Invoke(GetResult(), GetError()); break;
                default: throw ErrorFactory.Option.InvalidOperationMatch();
            }
        }

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
                OptionState.Success => "Success; Value: {{{0}}}".Format(GetResult()),
                OptionState.Failed => "Failed; Error = {0}; Value: {{{1}}}".Format(GetError(), GetResult()),
                OptionState.Canceled => "Canceled; Error = {0}; Value: {{{1}}}".Format(GetError(), GetResult()),
                _ => "Error:: Unrecognized State"
            };
        }

        public int CompareTo(Option<T> other) {
            return (_state, other._state) switch {
                (OptionState.Success, OptionState.Success) => _value.CompareTo(other._value),
                (OptionState.Success, OptionState.Failed) => -1,
                (OptionState.Success, OptionState.Canceled) => -1,
                (OptionState.Failed, OptionState.Failed) =>
                    (_value.IsOk() && other._value.IsOk())
                        ? _value.CompareTo(other._value)
                        : _value.IsOk()
                            ? -1
                            : other._value.IsOk()
                                ? 1
                                : 0,
                (OptionState.Failed, OptionState.Success) => 1,
                (OptionState.Failed, OptionState.Canceled) => -1,
                (OptionState.Canceled, OptionState.Canceled) =>
                    (_value.IsOk() && other._value.IsOk())
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
                if (obj is null) return Failed(ErrorFactory.Option.InvalidIsOkCastOperation());
                if (obj is not T) return Failed(ErrorFactory.Option.InvalidImplicitUnboxingCast(obj.GetType(), typeof(T)));
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

            // we won't reach here
            return default;
        }
    }
}
