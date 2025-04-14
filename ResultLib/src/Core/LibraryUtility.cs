// ReSharper disable ArrangeModifiersOrder

using System;

namespace ResultLib.Core
{
    static public class ExceptionUtility
    {
        static internal bool EqualValue(Exception left, Exception right)
        {
            if (left == null && right == null) return true;
            if (ReferenceEquals(left, right)) return true;
            return left != null
                   && left.GetType() == right.GetType()
                   && left.Message.Equals(right.Message, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}