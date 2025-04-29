using CommonUtils.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtils.Utils
{
    public static class EnumerableExtension
    {
            public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> Action)
            {
                foreach (var item in enumerable)
                {
                    Action(item);
                }
            }

            public static IEnumerable<T> Repeat<T>(this int times, Func<int, T> selector) => Enumerable.Range(0, times).Select(selector).ToList();

            public static IEnumerable<T> Repeat<T>(this int times, Func<T> selector) => times.Repeat(_ => selector());

            public static void Repeat(this int times, Action<int> action)
            {
                for (var i = 0; i < times; i++) action(i);
            }

            public static void Repeat(this int times, Action action) => times.Repeat(i => action());

            public static Task RepeatAsync<N>(this int times, Func<int, Task<N>> asyncAction) => Enumerable.Range(0, times).ToList().ForEachSequentialAsync(asyncAction);

            public static Task RepeatAsync<N>(this int times, Func<Task<N>> asyncAction) => times.RepeatAsync(i => asyncAction());

            public static IEnumerable<int> PairNumbers(this (int From, int To) values) => Enumerable.Range(values.From, values.To).Where(n => n % 2 == 0);
            public static IEnumerable<int> OddNumbers(this (int From, int To) values) => Enumerable.Range(values.From, values.To).Where(n => n % 2 == 1);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static IEnumerable<T> AsEnum<T>([AllowNull] this T? obj) where T : class => obj?.AsEnum(1) ?? Enumerable.Empty<T>();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static IEnumerable<T> AsNotNullEnum<T>(this T obj) where T : class => obj.AsEnum(1);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static IEnumerable<T> AsStructEnum<T>(this T obj) where T : struct => obj.AsEnum(1);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static IEnumerable<T> AsEnum<T>(this T obj, int repeat = 1) => Enumerable.Repeat(obj, repeat);

            public static IEnumerable<T> ToEnum<T>(this (T _1, T _2) t) => new T[] { t._1, t._2 };

#if NET6_0_OR_GREATER
            public static IEnumerable<(T?, S?)> Zip<T, S>(this (IEnumerable<T> first, IEnumerable<S> second) source)
                => Math.Max(source.first.Count(), source.second.Count()).Repeat(n => (source.first.ElementAtOrDefault(n), source.second.ElementAtOrDefault(n)));
#else
    public static IEnumerable<(T, S)> Zip<T, S>(this (IEnumerable<T> first, IEnumerable<S> second) source)
        => Math.Max(source.first.Count(), source.second.Count()).Repeat(n => (source.first.ElementAtOrDefault(n), source.second.ElementAtOrDefault(n)));
#endif
            public static IEnumerable<(T, S)> PermutationCouples<T, S>(this (IEnumerable<T> First, IEnumerable<S> Second) source) => source.First.SelectMany(x => source.Second, (first, second) => (first, second));

            public static IEnumerable<(T, T)> PermutationCouples<T>(this IEnumerable<T> source) => (source, source).PermutationCouples().Distinct();

            public static TSource? GetSingleCommonValueOrDefault<TSource>(this IEnumerable<IEnumerable<TSource>> sources, TSource? @default = default)
                => sources.IsEmpty() ? @default : sources.Aggregate(seed: sources.First(), (s1, s2) => s1.Intersect(s2)).SingleOrDefault() ?? @default;

            public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<IEnumerable<TSource>> sources) => sources.First().Intersect(sources.Skip(1));

            public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<IEnumerable<TSource>> nextSources)
                => nextSources.Any() ? first.Intersect(nextSources.First()).Intersect(nextSources.Skip(1)) : first;

            public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source, Equals<TSource> Equals, GetHashCode<TSource> GetHashCode)
                => source.Distinct(comparer: (Equals, GetHashCode).AsEqualityComparer());

            public static IEqualityComparer<IEnumerable<T>> AsEnumerableEqualityComparer<T>(this (Equals<T> equals, GetHashCode<T> getHashCode) value) where T : notnull
                => (value.equals, value.getHashCode).AsEqualityComparer().AsEnumerableEqualityComparer();

            public static IEqualityComparer<IEnumerable<T>> AsEnumerableEqualityComparer<T>(this IEqualityComparer<T> elementEqualityComparer) where T : notnull
            {
                bool Equals(IEnumerable<T>? e1, IEnumerable<T>? e2) => e2 == null ? e1 == e2 : e1?.SequenceEqual(e2, elementEqualityComparer) ?? e1 == e2;
                int GetHashCode(IEnumerable<T> e) => !e.Any() ? 0 : elementEqualityComparer.GetHashCode(e.ElementAt(0)) * e.Count();
                return ((Equals<IEnumerable<T>>)Equals, (GetHashCode<IEnumerable<T>>)GetHashCode).AsEqualityComparer();
            }

#if !NET8_0_OR_GREATER
    public static Dictionary<K, V> ToDictionary<K, V>(this IEnumerable<KeyValuePair<K, V>> entries) where K : notnull => entries.ToDictionary(entry => entry.Key, entry => entry.Value);

    public static Dictionary<K, V> ToDictionary<K, V>(this IEnumerable<(K, V)> entries) where K : notnull => entries.ToDictionary(entry => entry.Item1, entry => entry.Item2);
#endif

            public static Dictionary<K, IEnumerable<V>> ToDictionary<K, V>(this IEnumerable<IGrouping<K, V>> groupedEntries) where K : notnull
                => groupedEntries.ToDictionary(entry => entry.Key, entry => entry as IEnumerable<V>);

            public static IEnumerable<TResult> SelectNotNull<TSource, TResult>(this IEnumerable<TSource>? source, Func<TSource, TResult?> selector)
                => source?.Select(selector).ExcludeNull() ?? Enumerable.Empty<TResult>();

            public static IEnumerable<TResult> SelectNotNullValues<TSource, TResult>(this IEnumerable<TSource>? source, Func<TSource, TResult?> selector) where TResult : struct
                => source?.Select(selector).ExcludeNullValues() ?? Enumerable.Empty<TResult>();

            public static IEnumerable<TSource> ExcludeNull<TSource>(this IEnumerable<TSource?> source) => source.Where(r => r != null).Cast<TSource>();

            public static IEnumerable<TSource> ExcludeNullValues<TSource>(this IEnumerable<TSource?> source) where TSource : struct => source.Where(r => r != null).Cast<TSource>();

            public static IEnumerable<string> ExcludeNullOrEmpty(this IEnumerable<string?> source) => source.Where(s => !string.IsNullOrEmpty(s)).Cast<string>();

            public static V Single<T, V>(this IEnumerable<T> source, Func<T, V> selector) where V : class => source.Select(selector).Single();

            public static IEnumerable<TResult> SelectManyNotNull<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult?>?> selector)
                => source.SelectNotNull(selector).SelectMany(r => r).ExcludeNull();

            public static IEnumerable<TResult> SelectManyNotNullValues<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult?>?> selector) where TResult : struct
                => source.SelectNotNull(selector).SelectMany(r => r).ExcludeNullValues();

            public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, (TResult, TResult)> selector)
                => source.Select(selector).SelectMany(r => new TResult[] { r.Item1, r.Item2 });

            public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source) => source.SelectMany(t => t);

            public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> source, Func<T, bool> predicate) => source.Where(t => !predicate(t));

            public static List<T> AsList<T>(this T obj) => new List<T>() { obj };

            public static List<T> AddValue<T>(this List<T> list, T obj)
            {
                list.Add(obj);
                return list;
            }

            public static T[] AsArray<T>(this T obj) => new T[] { obj };

            public static T[] AsNotNullArray<T>(this T? obj) => obj == null ? Array.Empty<T>() : new T[] { obj };

            public static T[] Duplicate<T>(this T[] array)
            {
                T[] newArray = new T[array.Length];
                array.CopyTo(newArray, 0);
                return newArray;
            }

            public static IEnumerable<T> Duplicate<T>(this IEnumerable<T> enumerable)
            {
                T[] newArray = new T[enumerable.Count()];
                enumerable.ToArray().CopyTo(newArray, 0);
                return newArray;
            }
        public static IEnumerable<T> GetDuplicates<T, TKey>(this IEnumerable<T> source,  Func<T, TKey> keySelector)
        {
                    return source
                        .GroupBy(keySelector)
                        .Where(group => group.Count() > 1)
                        .SelectMany(group => group);
        }
        public static IEnumerable<T> FirstNotEmpty<T>(this (IEnumerable<T> First, IEnumerable<T> Second) enumerable) => enumerable.First.Any() ? enumerable.First : enumerable.Second;

            public static IEnumerable<T> FirstNotEmpty<T>(this IEnumerable<T> firstEnum, params IEnumerable<T>[] enums) => firstEnum.Any() ? firstEnum : enums.First(e => e.Any());

            public static bool IsEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? enumerable) => enumerable == null || !enumerable.Any();

            public static bool TryGetIndexOf<T>(this IEnumerable<T> enumerable, T element, out int? index, IEqualityComparer<T>? comparer = null)
                => (index = enumerable.IndexOf(element, comparer)) > -1;

            public static int IndexOf<T>(this IEnumerable<T> enumerable, T element, IEqualityComparer<T>? comparer = null)
            {
                int i = 0;
                comparer ??= EqualityComparer<T>.Default;
                foreach (var currentElement in enumerable)
                {
                    if (comparer.Equals(currentElement, element)) return i;
                    else i++;
                }
                return -1;
            }

            // Least Common Multiple
            public static decimal LCM<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) => source.Select(selector).Distinct().Aggregate(MathUtils.LCM);
            public static long LCM<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) => source.Select(selector).Distinct().Aggregate(MathUtils.LCM);
            public static int LCM<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) => source.Select(selector).Distinct().Aggregate(MathUtils.LCM);

            public static IEnumerable<T> OnlyIf<T>(this IEnumerable<T> enumerable, Predicate<T>? predicate) => predicate == null ? enumerable : enumerable.Where(predicate.Invoke);

            public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, params T[] elements) => enumerable.Except((IEnumerable<T>)elements);

            public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, Func<T, bool> Predicate) => enumerable.Where(t => !Predicate(t));

            public static IEnumerable<T> Sort<T>(this IEnumerable<T> enumerable) => enumerable.OrderBy(i => i);

            public static T Unique<T>(this IEnumerable<T> enumerable) => enumerable.Distinct().Single();

            [return: MaybeNull]
            public static T UniqueOrDefault<T>(this IEnumerable<T> enumerable) => enumerable.Distinct() is var distinct && distinct.Count() == 1 ? distinct.ElementAt(0) : default;

            public static R Unique<T, R>(this IEnumerable<T> enumerable, Func<T, R> selector) => enumerable.Select(selector).Unique();

            public static IEnumerable<IEnumerable<TSource>> GroupsWith<TSource>(this IEnumerable<TSource> enumerable, int maxElements) => enumerable.Select((item, index) => new { item, index }).GroupBy(g => g.index / maxElements, i => i.item);

            public static IEnumerable<IEnumerable<TSource>> GroupBy<TSource>(this IEnumerable<TSource> enumerable, int maxCount) => enumerable.GroupsWith(maxElements: Math.Max(1, (int)Math.Ceiling(enumerable.Count() / (decimal)maxCount)));

            public static bool HasUniqueElements<T, K>(this IEnumerable<T> enumerable, Func<T, K> keySelector) => enumerable.GroupBy(keySelector).All(g => g.Count() != 1);
            public static IEnumerable<T> GetUniqueElements<T, K>(this IEnumerable<T> enumerable, Func<T, K> keySelector) => enumerable.GroupBy(keySelector).Where(g => g.Count() == 1).SelectMany(t => t);
            public static IEnumerable<IGrouping<K, T>> GetNotUniqueElements<T, K>(this IEnumerable<T> enumerable, Func<T, K> keySelector) => enumerable.GroupBy(keySelector).Where(g => g.Count() > 1);

            public static bool IsOrderedBy<T, Q>(this IEnumerable<T> enumerable, Func<T, Q> keySelector) => enumerable.OrderBy(keySelector).SequenceEqual(enumerable);
            public static bool IsOrderedByDescending<T, Q>(this IEnumerable<T> enumerable, Func<T, Q> keySelector) => enumerable.OrderByDescending(keySelector).SequenceEqual(enumerable);

            [return: MaybeNull]
            public static T LastButOneOrDefault<T>(this IEnumerable<T> enumerable)
            {
                var count = enumerable.Count();
                return count > 1 ? enumerable.ElementAt(count - 2) : default;
            }

            public static bool AreDistinct<T>(this IEnumerable<T> enumerable) => enumerable?.Distinct().Count() == enumerable?.Count();

            public static IEnumerable<T> GetCommonStart<T>(this IEnumerable<T>? firstEnumerable, IEnumerable<T>? secondEnumerable)
                => firstEnumerable.IsEmpty() || secondEnumerable.IsEmpty() ? new T[] { } :
                    firstEnumerable.TakeWhile((c, i) => secondEnumerable.Count() > i && (secondEnumerable.ElementAt(i)?.Equals(c) ?? c == null));

            public static bool ContainsElements<T>(this IEnumerable<T> firstEnumerable, IEnumerable<T> secondEnumerable)
                => secondEnumerable.Select(firstEnumerable.Contains).All(true.Equals);

            public static bool Contains<T>(this IEnumerable<T> enumerable, Func<T, bool> Predicate) => enumerable.Where(Predicate).Any();

            public static IEnumerable<IEnumerable<T>> GetSubEnumerables<T>(this IEnumerable<T> enumerable, int subEnumerablesSize)
                => enumerable.Count() < subEnumerablesSize || subEnumerablesSize <= 0 ? Enumerable.Empty<IEnumerable<T>>() :
                Enumerable.Range(0, enumerable.Count() - subEnumerablesSize + 1).Select(i => enumerable.Skip(i).Take(subEnumerablesSize));

            public static bool StartsWith<T>(this IEnumerable<T> firstEnumerable, IEnumerable<T> secondEnumerable)
                => firstEnumerable.Count() >= secondEnumerable.Count() && firstEnumerable.Take(secondEnumerable.Count()).SequenceEqual(secondEnumerable);

            public static string ToHexString(this IEnumerable<byte>? enumerable, string separator = ",")
                => enumerable == null ? "" : "[" + BitConverter.ToString(enumerable.ToArray()).Replace("-", separator) + "]";

            public static string ConcatAsString<T>(this IEnumerable<T>? enumerable, char separator) => enumerable.ConcatAsString(separator.ToString());
            public static string ConcatAsString<T>(this IEnumerable<T>? enumerable, string separator = ",")
                => enumerable.ToString(separator, prefix: "", suffix: "");

            public static string ToString<T>(this IEnumerable<T>? enumerable, string separator, string prefix = "[", string suffix = "]")
                => enumerable == null ? "" : prefix + string.Join(separator, enumerable) + suffix;

            public static string ToString<T>(this IEnumerable<T>? enumerable, string separator, string defaultValue)
                => enumerable?.Select(p => p?.ToString()).Select(p => p == defaultValue ? $"{p}(default)" : p).ToString(separator) ?? "";

            public static T Second<T>(this IEnumerable<T> enumerable) => enumerable.ElementAt(1);

            public static T Second<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate) => enumerable.Where(predicate).ElementAt(1);

            public static (T First, T Second) FirstAndSecond<T>(this IEnumerable<T> enumerable) => (enumerable.First(), enumerable.Second());

            [return: MaybeNull]
            public static T SecondOrDefault<T>(this IEnumerable<T> enumerable) => enumerable.ElementAtOrDefault(1);

            public static T SecondLast<T>(this IEnumerable<T> enumerable) => enumerable.ElementAt(enumerable.Count() - 2);

            public static IEnumerable<TIn> IgnoreExceptions<TIn>(this IEnumerable<TIn> values, Action<TIn?, Exception>? action = null) => values.SelectIgnoringExceptions(i => i, action);

            public static IEnumerable<TOut> SelectIgnoringExceptions<TIn, TOut>(this IEnumerable<TIn> values, Func<TIn, TOut> selector, Action<TIn?, Exception>? action = null)
            {
                action ??= (i, e) => Console.Error.WriteLine(i == null ? $"Error executing foreach on values" : $"Error applying selector on item: {i}", e);
                try
                {
                    return values.SelectIgnoringItemsExceptions(selector, action).ToArray();
                }
                catch (Exception ex)
                {
                    action?.Invoke(default, ex);
                }
                return Enumerable.Empty<TOut>();
            }

            private static IEnumerable<TOut> SelectIgnoringItemsExceptions<TIn, TOut>(this IEnumerable<TIn> values, Func<TIn, TOut> selector, Action<TIn, Exception> action)
            {
                foreach (var item in values)
                {
                    TOut? output = default;
                    try
                    {
                        output = selector(item);
                    }
                    catch (Exception ex)
                    {

                        action?.Invoke(item, ex);
                        continue;
                    }
                    yield return output;
                }
            }

            public static void DisposeElements<T>(this IEnumerable<T> elements) where T : IDisposable => elements.ForEach(element => element.Dispose());

        }
    }
