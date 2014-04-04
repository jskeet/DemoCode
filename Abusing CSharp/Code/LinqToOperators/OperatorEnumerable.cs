// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LinqToOperators
{
    public sealed class OperatorEnumerable : IEnumerable
    {
        internal enum UnaryMinusBehaviour
        {
            Reverse,
            ElementwiseNegation,
            Pop
        }

        private UnaryMinusBehaviour unaryMinusBehaviour;

        private IEnumerable<object> source;

        private OperatorEnumerable(IEnumerable<object> source, UnaryMinusBehaviour unaryMinusBehaviour)
        {
            this.source = source;
            this.unaryMinusBehaviour = unaryMinusBehaviour;
        }

        internal OperatorEnumerable(IEnumerable<object> source) : this(source, UnaryMinusBehaviour.Reverse)
        {
        }

        internal OperatorEnumerable(IEnumerable source) : this(source.Cast<object>())
        {
        }

        public static DarkEnumerable operator !(OperatorEnumerable operand)
        {
            return new DarkEnumerable(operand.source);
        }

        /// <summary>
        /// This operator is dedicated to Eric Lippert.
        /// </summary>
        public static OperatorEnumerable operator +(OperatorEnumerable operand)
        {
            var newBehaviour = (UnaryMinusBehaviour) (((int) operand.unaryMinusBehaviour + 1) % 3);
            return new OperatorEnumerable(operand.source, newBehaviour);
        }

        public static OperatorEnumerable operator +(OperatorEnumerable lhs, object rhs)
        {
            return new OperatorEnumerable(lhs.source.Concat(new object[] { rhs }));
        }

        public static OperatorEnumerable operator +(OperatorEnumerable lhs, DarkEnumerable rhs)
        {
            return new OperatorEnumerable(lhs.source.Except(rhs.Source));
        }

        public static OperatorEnumerable operator ++(OperatorEnumerable lhs)
        {
            return +(+lhs);
        }        

        public static OperatorEnumerable operator -(OperatorEnumerable lhs, object rhs)
        {
            return new OperatorEnumerable(lhs.source.Except(new object[] { rhs }));
        }

        public static OperatorEnumerable operator --(OperatorEnumerable lhs)
        {
            return lhs - "-";
        }
        
        public static OperatorEnumerable operator *(OperatorEnumerable lhs, int rhs)
        {
            return new OperatorEnumerable(RepeatSequence(lhs.source, rhs));
        }

        public static OperatorEnumerable operator *(OperatorEnumerable lhs, IEnumerable rhs)
        {
            return new OperatorEnumerable(lhs.source.SelectMany(x => rhs.Cast<object>(), Tuple.Create));
        }

        public static OperatorEnumerable operator /(OperatorEnumerable lhs, Func<dynamic, dynamic> rhs)
        {
            return new OperatorEnumerable(lhs.source.GroupBy(rhs));
        }

        public static OperatorEnumerable operator /(OperatorEnumerable lhs, int rhs)
        {
            return new OperatorEnumerable(Batch(lhs, rhs));
        }

        private static IEnumerable<OperatorEnumerable> Batch(OperatorEnumerable source, int size)
        {
            List<object> currentList = new List<object>(size);
            foreach (var item in source)
            {
                currentList.Add(item);
                if (currentList.Count == size)
                {
                    yield return new OperatorEnumerable(currentList);
                    currentList = new List<object>(size);
                }
            }
        }

        public static OperatorEnumerable operator %(OperatorEnumerable lhs, int rhs)
        {
            List<object> list = new List<object>(rhs);
            foreach (var item in lhs.source)
            {
                list.Add(item);
                if (list.Count == rhs)
                {
                    list.Clear();
                }
            }
            return new OperatorEnumerable(list);
        }

        public static OperatorEnumerable operator ^(OperatorEnumerable lhs, IEnumerable rhs)
        {
            List<object> left = lhs.source.ToList();
            List<object> right = rhs.Cast<object>().ToList();
            return new OperatorEnumerable(left.Except(right).Union(right.Except(left)));
        }

        public static OperatorEnumerable operator ^(OperatorEnumerable lhs, uint rhs)
        {
            if (rhs == 0)
            {
                return Enumerable.Empty<object>().Evil();
            }
            OperatorEnumerable result = lhs;
            for (int i = 1; i < rhs; i++)
            {
                result = result * lhs;
            }
            return result | FlattenTuples;
        }

        private static object FlattenTuples(object item)
        {
            // Avoid converting x => {x}, even if we possibly should...
            if (!item.GetType().Name.StartsWith("Tuple`"))
            {
                return item;
            }
            object[] items = ExtractTupleElements(item).ToArray();
            Type openType = Type.GetType("System.Tuple`" + items.Length);
            Type constructedType = openType.MakeGenericType(items.Select(x => x.GetType()).ToArray());
            return constructedType.GetConstructors()[0].Invoke(items);
        }

        private static IEnumerable<object> ExtractTupleElements(object item)
        {
            Type type = item.GetType();
            if (!type.Name.StartsWith("Tuple`"))
            {
                return new[] { item };
            }
            int arity = int.Parse(type.Name.Substring(6));
            // First
            return Enumerable.Range(1, arity).Select(index => type.GetProperty("Item" + index)
                                                                  .GetValue(item))
                                             .SelectMany(ExtractTupleElements);
        }

        public static OperatorEnumerable operator ~(OperatorEnumerable operand)
        {
            return new OperatorEnumerable(Shuffle(operand.source));
        }

        public static bool operator true(OperatorEnumerable operand)
        {
            return operand.source.Any();
        }

        public static bool operator false(OperatorEnumerable operand)
        {
            return !operand.source.Any();
        }

        public static OperatorEnumerable operator <<(OperatorEnumerable lhs, int rhs)
        {
            return new OperatorEnumerable(lhs.source.Skip(rhs).Concat(lhs.source.Take(rhs)));
        }

        public static OperatorEnumerable operator >>(OperatorEnumerable lhs, int rhs)
        {
            int count = lhs.source.Count();
            return lhs << (count - rhs);            
        }

        public static bool operator ==(OperatorEnumerable lhs, IEnumerable rhs)
        {
            return lhs.source.SequenceEqual(rhs.Cast<object>());
        }

        public static bool operator !=(OperatorEnumerable lhs, IEnumerable rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator >(OperatorEnumerable lhs, IEnumerable rhs)
        {
            return Compare(lhs, rhs) > 0;
        }

        public static bool operator <(OperatorEnumerable lhs, IEnumerable rhs)
        {
            return Compare(lhs, rhs) < 0;
        }

        public static bool operator >=(OperatorEnumerable lhs, IEnumerable rhs)
        {
            return Compare(lhs, rhs) >= 0;
        }

        public static bool operator <=(OperatorEnumerable lhs, IEnumerable rhs)
        {
            return Compare(lhs, rhs) <= 0;
        }

        private static int Compare(OperatorEnumerable lhs, IEnumerable rhs)
        {
            using (var leftIterator = lhs.source.GetEnumerator())
            {
                using (var rightIterator = rhs.Cast<object>().GetEnumerator())
                {
                    while (true)
                    {
                        bool leftHasNext = leftIterator.MoveNext();
                        bool rightHasNext = rightIterator.MoveNext();
                        if (!leftHasNext && !rightHasNext)
                        {
                            return 0;
                        }
                        if (!rightHasNext)
                        {
                            return 1;
                        }
                        if (!leftHasNext)
                        {
                            return -1;
                        }
                        int comparison = Comparer<object>.Default.Compare(leftIterator.Current, rightIterator.Current);
                        if (comparison != 0)
                        {
                            return comparison;
                        }
                    }
                }
            }
        }

        public override bool Equals(object obj)
        {
            return this == obj as IEnumerable;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static dynamic operator -(OperatorEnumerable operand)
        {
            switch (operand.unaryMinusBehaviour)
            {
                case UnaryMinusBehaviour.Reverse:
                    return new OperatorEnumerable(operand.source.Reverse());
                case UnaryMinusBehaviour.ElementwiseNegation:
                    return new OperatorEnumerable(operand.source.Cast<dynamic>().Select(x => -x));
                case UnaryMinusBehaviour.Pop:
                {
                    var ret = operand.source.First();
                    operand.source = operand.source.Skip(1);
                    return ret;
                }
            }
            throw new InvalidOperationException();
        }

        private static IEnumerable<object> RepeatSequence(IEnumerable<object> source, int times)
        {
            for (int i = 0; i < times; i++)
            {
                foreach (var item in source)
                {
                    yield return item;
                }
            }
        }

        private static IEnumerable<object> Shuffle(IEnumerable<object> source)
        {
            // Never do this!
            Random rng = new Random();
            var array = source.ToArray();
            // Note i > 0 to avoid final pointless iteration
            for (int i = array.Length - 1; i > 0; i--)
            {
                // Swap element "i" with a random earlier element it (or itself)
                int swapIndex = rng.Next(i + 1);
                object tmp = array[i];
                array[i] = array[swapIndex];
                array[swapIndex] = tmp;
            }
            // Lazily yield (avoiding aliasing issues etc)
            foreach (object element in array)
            {
                yield return element;
            }
        }

        public static OperatorEnumerable operator &(OperatorEnumerable lhs, Func<dynamic, bool> rhs)
        {
            return new OperatorEnumerable(lhs.source.Where(rhs));
        }

        public static OperatorEnumerable operator |(OperatorEnumerable lhs, Func<dynamic, dynamic> rhs)
        {
            return new OperatorEnumerable(lhs.source.Select(rhs));
        }

        public IEnumerator GetEnumerator()
        {
            return source.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join(", ", source);
        }
    }
}
