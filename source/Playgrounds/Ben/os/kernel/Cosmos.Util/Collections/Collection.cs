

using System;
using System.Collections.Generic;
using System.Collections;




    public static class Collection
    {
        /// <summary>
        /// Bypasses elements in a collection as long as a specified condition is true
        //  and then returns the remaining elements.
        /// </summary>
        public static IEnumerable<T> SkipWhile<T>(IEnumerable<T> collection, Predicate<T> match)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            foreach (T item in collection)
            {
                if (match(item) == false)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Returns a specified number of contiguous elements from the start of a collection.
        /// </summary>
        public static IEnumerable<T> Take<T>(IEnumerable<T> collection, int count)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (count < 0)
            {
                throw new IndexOutOfRangeException("count");
            }
            int i = 0;
            foreach (T item in collection)
            {
                i++;
                if (i > count)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Returns elements from a collection as long as a specified condition is true.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="match"></param>
        /// <returns>An System.Collections.Generic.IEnumerable<T> that contains the elements from
        //     the input collection that occur before the element at which the test no longer
        //     passes.</returns>
        public static IEnumerable<T> TakeWhile<T>(IEnumerable<T> collection, Predicate<T> match)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            foreach (T item in collection)
            {
                if (match(item))
                {
                    yield break;
                }
                yield return item;
            }
        }
        /// <summary>
        /// Determines whether two collections are equal by comparing the elements by using   
        /// </summary>
        public static bool SequenceEqual<T>(IEnumerable<T> first, IEnumerable<T> second) where T : IEquatable<T>
        {
            if (first == null && second == null)
            {
                return true;
            }
            if (first == null)
            {
                throw new ArgumentNullException("first");
            }
            if (second == null)
            {
                throw new ArgumentNullException("second");
            }
            if (Count(first) != Count(second))
            {
                return false;
            }
            using (IEnumerator<T> enumerator1 = first.GetEnumerator())
            using (IEnumerator<T> enumerator2 = second.GetEnumerator())
            {

                int count = Count(first);

                int index = 1;
                while (enumerator1.Current.Equals(enumerator2.Current))
                {
                    index++;
                    continue;
                }
                return index == count;
            }
        }
        /// <summary>
        /// Bypasses a specified number of elements in a collection and then returns the
        /// </summary>
        public static IEnumerable<T> Skip<T>(IEnumerable<T> collection, int count)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (count < 0)
            {
                throw new IndexOutOfRangeException("count");
            }
            int i = 0;
            foreach (T item in collection)
            {
                i++;
                if (i > count)
                {
                    yield return item;
                }
            }
        }


        /// <summary>
        /// Generates a collection that contains one repeated value.
        /// </summary>
        public static IEnumerable<T> Repeat<T>(T element, int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            for (int i = 0; i < count; i++)
            {
                yield return element;
            }
        }
        /// <summary>
        /// Returns the element at a specified index in a collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">An System.Collections.Generic.IEnumerable<T> to return an element from</param>
        /// <param name="index">The zero-based index of the element to retrieve.</param>
        /// <returns></returns>
        public static T ElementAt<T>(IEnumerable<T> collection, int index)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (index < 0 || index > Count(collection) - 1)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return ToArray(collection)[index];
        }

        /// <summary>
        /// Returns the element at a specified index in a collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">An System.Collections.Generic.IEnumerable<T> to return an element from</param>
        /// <param name="index">The zero-based index of the element to retrieve.</param>
        /// <returns></returns>
        public static T ElementAtOrDefault<T>(IEnumerable<T> collection, int index)
        {
            if (index < 0 || index > Count(collection) - 1 || collection == null)
            {
                return default(T);
            }
            return ElementAt(collection, index);
        }
        /// <summary>
        /// Concatenates two collections.
        /// </summary>
        public static IEnumerable<T> Concat<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null)
            {
                throw new ArgumentNullException("first");
            }
            if (second == null)
            {
                throw new ArgumentNullException("second");
            }
            foreach (T item in first)
            {
                yield return item;
            }
            foreach (T item in second)
            {
                yield return item;
            }
        }
        /// <summary>
        /// Returns true if collection contains the item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool Contains<T>(IEnumerable<T> collection, T item) where T : IEquatable<T>
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            foreach (T t in collection)
            {
                if (t.Equals(item))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Converts all the items of type T in collection to a new collection of type U according to converter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="collection"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        public static IEnumerable<U> ConvertAll<T, U>(IEnumerable<T> collection, Converter<T, U> converter)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (converter == null)
            {
                throw new ArgumentNullException("converter");
            }
            foreach (T item in collection)
            {
                yield return converter(item);
            }
        }

        /// <summary>
        /// Returns true if collection contains an item that satisfies the predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static bool Any<T>(IEnumerable<T> collection, Predicate<T> match)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            foreach (T item in collection)
            {
                if (match(item))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Determines whether a collection contains any elements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static bool Any<T>(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            using (IEnumerator<T> iterator = collection.GetEnumerator())
            {
                return iterator.MoveNext();
            }
        }
        /// <summary>
        /// Finds the only item in the collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static T Single<T>(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (Count(collection) != 1)
            {
                throw new InvalidOperationException("collection");
            }
            return ToList(collection)[0];
        }


        /// <summary>
        /// Finds the only item in the collection that satisfies the predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static T Single<T>(IEnumerable<T> collection, Predicate<T> match)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            if (Count(collection) == 0)
            {
                throw new InvalidOperationException("collection");
            }
            IList<T> list = ToList(FindAll(collection, match));
            if (list.Count != 1)
            {
                throw new InvalidOperationException("Multiple matches found");
            }
            return list[0];
        }



        /// <summary>
        /// Finds the only item in the collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static T SingleOrDefault<T>(IEnumerable<T> collection)
        {
            if (collection == null || Count(collection) != 1)
            {
                return default(T);
            }
            return Single(collection);
        }


        /// <summary>
        /// Finds the only item in the collection that satisfies the predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static T SingleOrDefault<T>(IEnumerable<T> collection, Predicate<T> match)
        {
            if (collection == null || match == null)
            {
                return default(T);
            }
            if (Count(collection) == 0)
            {
                return default(T);
            }
            IList<T> list = ToList(FindAll(collection, match));
            if (list.Count != 1)
            {
                return default(T);
            }
            return list[0];
        }
        /// <summary>
        /// Finds the first item in the collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static T First<T>(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (Count(collection) == 0)
            {
                throw new InvalidOperationException("collection");
            }
            return ToList(collection)[0];
        }

        /// <summary>
        /// Finds the first item in the collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static T FirstOrDefault<T>(IEnumerable<T> collection)
        {
            if (Count(collection) == 0 || collection == null)
            {
                return default(T);
            }
            return ToList(collection)[0];
        }

        /// <summary>
        /// Finds the first occurrence of item in collection that satisfy the predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static T First<T>(IEnumerable<T> collection, Predicate<T> match)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            foreach (T item in collection)
            {
                if (match(item))
                {
                    return item;
                }
            }
            return default(T);
        }
        /// <summary>
        /// Finds all the items in collection that satisfy the predicate match. Same as LINQ's Where
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static IEnumerable<T> FindAll<T>(IEnumerable<T> collection, Predicate<T> match)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            foreach (T item in collection)
            {
                if (match(item))
                {
                    yield return item;
                }
            }
        }
        /// <summary>
        /// Finds all the items in iterator1 that are not in collection2
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iterator1"></param>
        /// <param name="collection2"></param>
        /// <returns></returns>
        public static IEnumerable<T> Complement<T>(IEnumerable<T> collection1, IEnumerable<T> collection2) where T : IEquatable<T>
        {
            foreach (T item in collection1)
            {
                if (Contains(collection2, item) == false)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Finds all the items that are not in the intersection of collection1 and collection2
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection1"></param>
        /// <param name="collection2"></param>
        /// <returns></returns>
        public static IEnumerable<T> Except<T>(IEnumerable<T> collection1, IEnumerable<T> collection2) where T : IEquatable<T>
        {
            IEnumerable<T> complement1 = Complement(collection1, collection2);
            IEnumerable<T> complement2 = Complement(collection2, collection1);
            return Union(complement1, complement2);
        }
        /// <summary>
        /// Returns a collection of all the distinct items in collection (no duplicates)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IEnumerable<T> Distinct<T>(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            IList<T> list = new List<T>();
            foreach (T item in collection)
            {
                if (list.Contains(item) == false)
                {
                    list.Add(item);
                }
            }
            foreach (T item in list)
            {
                yield return item;
            }
        }
        /// <summary>
        /// Find the index of the first occurrence of item in collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int FindIndex<T>(IEnumerable<T> collection, T value) where T : IEquatable<T>
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            int index = 0;

            foreach (T item in collection)
            {
                if (item.Equals(value) == false)
                {
                    index++;
                }
                else
                {
                    return index;
                }
            }
            return -1;
        }
        /// <summary>
        /// Finds the intersection of collection1 and collection2
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection1"></param>
        /// <param name="collection2"></param>
        /// <returns></returns>
        public static IEnumerable<T> Intersect<T>(IEnumerable<T> collection1, IEnumerable<T> collection2) where T : IEquatable<T>
        {
            Predicate<T> existsInCollection2 = delegate(T t)
                                               {
                                                   Predicate<T> exist = delegate(T item)
                                                                        {
                                                                            return item.Equals(t);
                                                                        };
                                                   return Any(collection2, exist);
                                               };
            return FindAll(collection1, existsInCollection2);
        }
        /// <summary>
        /// Find the index of the last occurrence of item in collection that satisfy the predicate match
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static T LastOrDefault<T>(IEnumerable<T> collection, Predicate<T> match)
        {
            if (collection == null)
            {
                return default(T);
            }
            if (Count(collection) == 0)
            {
                return default(T);
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            return Last(collection, match);
        }

        /// <summary>
        /// Find the index of the last occurrence of item in collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static T LastOrDefault<T>(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                return default(T);
            }
            if (Count(collection) == 0)
            {
                return default(T);
            }
            return Last(collection);
        }
        /// <summary>
        /// Find the index of the last occurrence of item in collection 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static T Last<T>(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            return ToArray(collection)[Count(collection) - 1];
        }

        /// <summary>
        /// Find the index of the last occurrence of item in collection that satisfy the predicate match
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static T Last<T>(IEnumerable<T> collection, Predicate<T> match)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            T last = default(T);

            foreach (T item in collection)
            {
                if (match(item))
                {
                    last = item;
                }
            }
            return last;
        }
        /// <summary>
        /// Find the index of the last occurrence of item in collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int FindLastIndex<T>(IEnumerable<T> collection, T value) where T : IEquatable<T>
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            int last = -1;

            foreach (T item in collection)
            {
                if (item.Equals(value))
                {
                    last++;
                }
            }
            if (last >= 0)
            {
                return last;
            }
            else
            {
                return -1;
            }
        }
        /// <summary>
        /// Finds the union of collection1 and collection2
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection1"></param>
        /// <param name="collection2"></param>
        /// <returns></returns>
        public static IEnumerable<T> Union<T>(IEnumerable<T> collection1, IEnumerable<T> collection2) where T : IEquatable<T>
        {
            LinkedList<T> union = new LinkedList<T>(Distinct(collection1));

            Action<T> action = delegate(T t)
                                 {
                                     if (union.Find(t) == null)
                                     {
                                         union.AddLast(t);
                                     }
                                 };
            ForEach(collection2, action);
            foreach (T item in union)
            {
                yield return item;
            }
        }
        /// <summary>
        ///  Performs the action on every item in collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(IEnumerable<T> collection, Action<T> action)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            foreach (T item in collection)
            {
                action(item);
            }
        }
        /// <summary>
        /// Returns the number of items in collection that satisfy a condition
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static int Count<T>(IEnumerable<T> collection, Predicate<T> match)
        {
            int length = 0;
            Action<T> add = delegate(T item)
                            {
                                if (match(item))
                                {
                                    length++;
                                }
                            };
            ForEach(collection, add);
            return length;
        }
        /// <summary>
        /// Returns the number of items in collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static int Count<T>(IEnumerable<T> collection)
        {
            int length = 0;
            Action<T> add = delegate
                            {
                                length++;
                            };
            ForEach(collection, add);
            return length;
        }
        /// <summary>
        /// Returns a collection with a reverse order of items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IEnumerable<T> Reverse<T>(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            List<T> list = new List<T>(collection);
            list.Reverse();

            foreach (T item in list)
            {
                yield return item;
            }
        }
        /// <summary>
        /// Sorts the collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IEnumerable<T> Sort<T>(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            List<T> list = new List<T>(collection);
            list.Sort();

            foreach (T item in list)
            {
                yield return item;
            }
        }
        /// <summary>
        /// Converts collection to an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static T[] ToArray<T>(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            List<T> list = new List<T>();

            foreach (T item in collection)
            {
                list.Add(item);
            }
            return list.ToArray();
        }

        /// <summary>
        /// Converts iterator to an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iterator"></param>
        /// <returns></returns>
        public static T[] ToArray<T>(IEnumerator<T> iterator)
        {
            if (iterator == null)
            {
                throw new ArgumentNullException("iterator");
            }
            List<T> list = new List<T>();

            while (iterator.MoveNext())
            {
                list.Add(iterator.Current);
            }
            return list.ToArray();
        }
        /// <summary>
        /// Converts the items in collection to an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iterator"></param>
        /// <param name="count">Initial size for optimization</param>
        /// <returns></returns>
        public static T[] ToArray<T>(IEnumerable<T> collection, int count)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("iterator");
            }
            return ToArray(collection.GetEnumerator(), count);
        }
        /// <summary>
        /// Converts the items in iterator to an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iterator"></param>
        /// <param name="count">Initial size for optimization</param>
        /// <returns></returns>
        public static T[] ToArray<T>(IEnumerator<T> iterator, int count)
        {
            if (iterator == null)
            {
                throw new ArgumentNullException("iterator");
            }
            List<T> list = new List<T>(count);

            while (iterator.MoveNext())
            {
                list.Add(iterator.Current);
            }

            return list.ToArray();
        }
        /// <summary>
        /// Converts the items in collection to an array of type U according to the converter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="collection"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        static U[] ToArray<T, U>(IEnumerable<T> collection, Converter<T, U> converter)
        {
            int count = Count(collection);
            return ToArray(collection, converter, count);
        }
        /// <summary>
        /// Converts the items in collection to an array  of type U according to the converter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iterator"></param>
        /// <param name="count">Initial size for optimization</param>
        /// <returns></returns>
        static U[] ToArray<T, U>(IEnumerable<T> collection, Converter<T, U> converter, int count)
        {
            List<U> list = new List<U>(count);
            foreach (T t in collection)
            {
                list.Add(converter(t));
            }
            return list.ToArray();
        }
        /// <summary>
        /// Returns a list out of collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IList<T> ToList<T>(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            IList<T> list = new List<T>();
            foreach (T item in collection)
            {
                list.Add(item);
            }
            return list;
        }

        /// <summary>
        /// Returns all the items in collection that satisfy the predicate match
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iterator"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static bool All<T>(IEnumerable<T> collection, Predicate<T> match)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            foreach (T item in collection)
            {
                if (match(item) == false)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Converts all the items in the object-based collection of the type T to a new array of type U according to converter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static U[] UnsafeToArray<T, U>(IEnumerable collection, Converter<T, U> converter)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (converter == null)
            {
                throw new ArgumentNullException("converter");
            }
            IEnumerable<U> newCollection = UnsafeConvertAll(collection, converter);
            return ToArray(newCollection);
        }

        /// <summary>
        /// Converts all the items in the object-based iterator of the type T to a new array of type U according to converter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iterator"></param>
        /// <returns></returns>
        public static U[] UnsafeToArray<T, U>(IEnumerator iterator, Converter<T, U> converter)
        {
            if (iterator == null)
            {
                throw new ArgumentNullException("iterator");
            }
            if (converter == null)
            {
                throw new ArgumentNullException("converter");
            }
            IEnumerator<U> newIterator = UnsafeConvertAll(iterator, converter);
            return ToArray(newIterator);
        }




















        /// <summary>
        /// Converts collection to an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static T[] UnsafeToArray<T>(IEnumerable collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            IEnumerator iterator = collection.GetEnumerator();

            using (iterator as IDisposable)
            {
                return UnsafeToArray<T>(iterator);
            }
        }
        /// <summary>
        /// Converts an iterator to an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iterator"></param>
        /// <returns></returns>
        public static T[] UnsafeToArray<T>(IEnumerator iterator)
        {
            if (iterator == null)
            {
                throw new ArgumentNullException("iterator");
            }
            Converter<object, T> innerConverter = delegate(object item)
                                                 {
                                                     return (T)item;
                                                 };
            return UnsafeToArray(iterator, innerConverter);
        }
        /// <summary>
        /// Converts all the items of type T in the object-based collection to a new collection of type U according to converter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="collection"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        public static IEnumerable<U> UnsafeConvertAll<T, U>(IEnumerable collection, Converter<T, U> converter)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (converter == null)
            {
                throw new ArgumentNullException("converter");
            }
            foreach (object item in collection)
            {
                yield return converter((T)item);
            }
        }
        /// <summary>
        /// Converts all the items of type T in the object-based IEnumerator to a new collection of type U according to converter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="collection"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        public static IEnumerator<U> UnsafeConvertAll<T, U>(IEnumerator iterator, Converter<T, U> converter)
        {
            if (iterator == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (converter == null)
            {
                throw new ArgumentNullException("converter");
            }
            while (iterator.MoveNext())
            {
                yield return converter((T)iterator.Current);
            }
        }
    }
