using System;
using System.Runtime.CompilerServices;

namespace ResultLib.Core {
    static internal class ArgumentNullExceptionExtension {
#if NET5_0_OR_GREATER
        static internal void ThrowIfNull(object argument, [CallerArgumentExpression(nameof(argument))] string paramName = null) {
            if (argument is null) throw new ArgumentNullException(paramName ?? string.Empty);
        }
#else
        static internal void ThrowIfNull(object argument) {
            if (argument is null) throw new System.ArgumentNullException();
        }
#endif
    }
}
