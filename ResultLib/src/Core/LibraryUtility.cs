// ReSharper disable ArrangeModifiersOrder

using System;

namespace ResultLib.Core {
    static internal class ExceptionUtility {
        static internal bool EqualValue(Exception left, Exception right) {
            if (left == null && right == null) return true;
            if (ReferenceEquals(left, right)) return true;
            return left != null
                   && left.GetType() == right.GetType()
                   && left.Message.Equals(right.Message, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    static internal class StringExtensions {
        static public string Format(this string format, params object[] args) => string.Format(format, args);
        static public bool IsEmpty(this string str) => string.IsNullOrEmpty(str);
    }
}
