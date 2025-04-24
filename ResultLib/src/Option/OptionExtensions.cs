// ReSharper disable CheckNamespace
// ReSharper disable ArrangeModifiersOrder
// ReSharper disable MemberCanBePrivate.Global

using ResultLib.Core;

namespace ResultLib {
    static public class OptionExtensions {
        static public Option<T> ToOption<T>(this Option option) {
            if (option.GetResult().IsOk(out object obj)) {
                if (obj is null) throw new OptionInvalidNullCastException();
                if (obj is not T) throw new OptionInvalidExplicitCastException(obj.GetType(), typeof(T));
            }

            if (option.IsSuccess(out var result)) {
                return result.IsError() ? Option<T>.Success() : Option<T>.Success(result.Some<T>());
            }

            if (option.IsFailed(out result)) {
                return result.IsError() ? Option<T>.Failed(option.GetErrorInternal()) : Option<T>.Failed(option.GetErrorInternal(), result.Some<T>());
            }

            if (option.IsCanceled(out result)) {
                return result.IsError() ? Option<T>.Canceled() : Option<T>.Canceled(result.Some<T>());
            }

            throw new OptionInvalidStateException();
        }

        static public Option<TSuccess, TFailed, TCanceled> ToOption<TSuccess, TFailed, TCanceled>(this Option option) {
            if (option.GetResult().IsOk(out object obj)) {
                if (obj is null) throw new OptionInvalidNullCastException();
            }

            if (option.IsSuccess(out var result)) {
                if (result.IsError()) return Option<TSuccess, TFailed, TCanceled>.Success();
                return result.Some<TSuccess>(out var some)
                    ? Option<TSuccess, TFailed, TCanceled>.Success(some)
                    : throw new OptionInvalidExplicitCastException(obj.GetType(), typeof(TSuccess));
            }

            if (option.IsFailed(out result)) {
                if (result.IsError()) return Option<TSuccess, TFailed, TCanceled>.Failed();
                return result.Some<TFailed>(out var some)
                    ? Option<TSuccess, TFailed, TCanceled>.Failed(option.GetErrorInternal(), some)
                    : throw new OptionInvalidExplicitCastException(obj.GetType(), typeof(TFailed));
            }

            if (option.IsCanceled(out result)) {
                if (result.IsError()) return Option<TSuccess, TFailed, TCanceled>.Canceled();
                return result.Some<TCanceled>(out var some)
                    ? Option<TSuccess, TFailed, TCanceled>.Canceled(some)
                    : throw new OptionInvalidExplicitCastException(obj.GetType(), typeof(TCanceled));
            }

            throw new OptionInvalidStateException();
        }
    }
}
