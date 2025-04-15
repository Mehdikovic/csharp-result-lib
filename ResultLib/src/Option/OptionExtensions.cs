// ReSharper disable CheckNamespace
// ReSharper disable ArrangeModifiersOrder
// ReSharper disable MemberCanBePrivate.Global

using ResultLib.Core;

namespace ResultLib {
    static public class OptionExtensions {
        static public Option<T> ToOption<T>(this Option option) {
            if (option.GetResult().IsOk(out object obj)) {
                if (obj is null) return Option<T>.Failed(Exceptions.Option.InvalidIsOkCastOperation());
                if (obj is not T) return Option<T>.Failed(Exceptions.Option.InvalidExplicitUnboxingCast(obj.GetType(), typeof(T)));
            }

            if (option.IsSuccess(out var result)) {
                return result.Some(out T value)
                    ? Option<T>.Success(value)
                    : Option<T>.Success();
            }

            if (option.IsFailed(out result)) {
                return result.Some(out T value)
                    ? Option<T>.Failed(option.GetError(), value)
                    : Option<T>.Failed(option.GetError());
            }

            if (option.IsCanceled(out result)) {
                return result.Some(out T value)
                    ? Option<T>.Canceled(value)
                    : Option<T>.Canceled();
            }

            // we won't reach here
            return default;
        }

        static public Option<TSuccess, TFailed, TCanceled> ToOption<TSuccess, TFailed, TCanceled>(this Option option) {
            if (option.GetResult().IsOk(out object obj)) {
                if (obj is null) return Option<TSuccess, TFailed, TCanceled>.Failed(Exceptions.Option.InvalidIsOkCastOperation());
                if (obj is not (TSuccess and TFailed and TCanceled)) return Option<TSuccess, TFailed, TCanceled>.Failed(Exceptions.Option.InvalidIsOkCastOperation());
            }

            if (option.IsSuccess(out var result)) {
                if (result.IsError()) return Option<TSuccess, TFailed, TCanceled>.Success();
                return result.Some<TSuccess>(out var some)
                    ? Option<TSuccess, TFailed, TCanceled>.Success(some)
                    : Option<TSuccess, TFailed, TCanceled>.Failed(Exceptions.Option.InvalidExplicitUnboxingCast(obj.GetType(), typeof(TSuccess)));
            }

            if (option.IsFailed(out result)) {
                if (result.IsError()) return Option<TSuccess, TFailed, TCanceled>.Failed();
                return result.Some<TFailed>(out var some)
                    ? Option<TSuccess, TFailed, TCanceled>.Failed(option.GetError(), some)
                    : Option<TSuccess, TFailed, TCanceled>.Failed(Exceptions.Option.InvalidExplicitUnboxingCast(obj.GetType(), typeof(TFailed)));
            }

            if (option.IsCanceled(out result)) {
                if (result.IsError()) return Option<TSuccess, TFailed, TCanceled>.Canceled();
                return result.Some<TCanceled>(out var some)
                    ? Option<TSuccess, TFailed, TCanceled>.Canceled(some)
                    : Option<TSuccess, TFailed, TCanceled>.Failed(Exceptions.Option.InvalidExplicitUnboxingCast(obj.GetType(), typeof(TCanceled)));
            }

            // we won't reach here
            return default;
        }
    }
}
