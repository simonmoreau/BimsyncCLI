using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace BimsyncCLI.Services
{
    public static class PredicateBuilder
    {
        public static Func<T, bool> True<T>() { return f => true; }
        public static Func<T, bool> False<T>() { return f => false; }

        public static Func<T, bool> Or<T>(this Func<T, bool> predicate1, Func<T, bool> predicate2)
        {
            Func<T, bool> predicate3 = order => predicate1(order) || predicate2(order);
            return predicate3;
        }

        public static Func<T, bool> And<T>(this Func<T, bool> predicate1, Func<T, bool> predicate2)
        {
            Func<T, bool> predicate3 = order => predicate1(order) && predicate2(order);
            return predicate3;
        }
    }

}
