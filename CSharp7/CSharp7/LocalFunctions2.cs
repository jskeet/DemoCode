// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharp7
{
    static class LocalFunctions2
    {
        static void Main()
        {
            try
            {
                int[] source = null;
                Select1(source, x => x * 2);
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Caught!");
            }
            Console.WriteLine(new[] { 1, 2, 3 }.Select1(x => x * 2).Count());
            Console.WriteLine(new[] { 1, 2, 3 }.Select2(x => x * 2).Count());
        }

        static IEnumerable<TResult> SelectBroken<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }
            foreach (var item in source)
            {
                yield return selector(item);
            }
        }

        static IEnumerable<TResult> Select1<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }
            IEnumerable<TResult> impl()
            {
                foreach (var item in source)
                {
                    yield return selector(item);
                }
            }
            return impl();
        }

        static IEnumerable<TResult> Select2<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }
            IEnumerable<TResult> impl(
                IEnumerable<TSource> realSource,
                Func<TSource, TResult> realSelector)
            {
                foreach (var item in realSource)
                {
                    yield return realSelector(item);
                }
            }
            return impl(source, selector);
        }
    }
}
