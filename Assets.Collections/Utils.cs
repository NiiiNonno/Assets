using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Nonno.Assets.Scrolls;

namespace Nonno.Assets.Collections
{
    public static partial class Utils
    {
        public static WordList AsList(this string @this) => new(@this);

        public static WordDictionary AsDisctionary(this string @this) => new(@this);

        public static IEnumerable<T> Slice<T>(this IReadOnlyList<T> @this, Range range)
        {
            int length = @this.Count;
            int end = range.End.GetOffset(length);
            for (int i = range.Start.GetOffset(length); i < end; i++)
            {
                yield return @this[i];
            }
        }

        public static T AddNew<T>(this System.Collections.Generic.ICollection<T> @this, T neoItem)
        {
            @this.Add(neoItem);
            return neoItem;
        }
        [return: NotNull]
        public static TValue AddNew<TKey, TValue>(this System.Collections.Generic.IDictionary<TKey, TValue> @this, TKey key, TValue neoValue)
        {
            @this.Add(key, neoValue);
            return neoValue!;
        }

        public static TValue? RemoveOrNull<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key)
        {
            _ = @this.TryRemove(key, out var r);
            return r;
        }

        public static WordList GetList(this MarkAttribute @this) => new(@this.Text);

        public static WordDictionary GetDictionary(this MarkAttribute @this) => new(@this.Text);

        public static Task Read(this IScroll @this, out WordDictionary wordDictionary)
        {
            var task = @this.Remove(out string? value_);
            wordDictionary = new(value_ ?? throw new NullReferenceException("語典の内部文字列が`null`でした。"));
            return task;
        }
        public static Task Read(this IScroll @this, out WordDictionary? wordDictionaryOrNull)
        {
            var task = @this.Remove(out string? value_);
            wordDictionaryOrNull = value_ == null ? null : new(value_);
            return task;
        }
        public static Task Read(this IScroll @this, out WordList wordList)
        {
            var task = @this.Remove(out string? value_);
            wordList = new(value_ ?? throw new NullReferenceException("語列の内部文字列が`null`でした。"));
            return task;
        }
        public static Task Read(this IScroll @this, out WordList? wordListOrNull)
        {
            var task = @this.Remove(out string? value_);
            wordListOrNull = value_ == null ? null : new(value_);
            return task;
        }

        public static Task Write(this IScroll @this, WordDictionary wordDictionary)
        {
            return @this.Insert(@string: wordDictionary.ToString());
        }
        public static Task Write(this IScroll @this, WordDictionary? wordDictionaryOrNull)
        {
            return @this.Insert(@string: wordDictionaryOrNull?.ToString());
        }
        public static Task Write(this IScroll @this, WordList wordList)
        {
            return @this.Insert(@string: wordList.ToString());
        }
        public static Task Write(this IScroll @this, WordList? wordListOrNull)
        {
            return @this.Insert(@string: wordListOrNull?.ToString());
        }

        [Obsolete("`IBuiltInTypeAccessor`は廃止されます。")]
        public static Task Read(this IBuiltinTypeAccessor @this, out WordDictionary wordDictionary)
        {
            var task = @this.Read(out string? value_);
            wordDictionary = new(value_ ?? throw new NullReferenceException("語典の内部文字列が`null`でした。"));
            return task;
        }
        [Obsolete("`IBuiltInTypeAccessor`は廃止されます。")]
        public static Task Read(this IBuiltinTypeAccessor @this, out WordDictionary? wordDictionaryOrNull)
        {
            var task = @this.Read(out string? value_);
            wordDictionaryOrNull = value_ == null ? null : new(value_);
            return task;
        }
        [Obsolete("`IBuiltInTypeAccessor`は廃止されます。")]
        public static Task Read(this IBuiltinTypeAccessor @this, out WordList wordList)
        {
            var task = @this.Read(out string? value_);
            wordList = new(value_ ?? throw new NullReferenceException("語列の内部文字列が`null`でした。"));
            return task;
        }
        [Obsolete("`IBuiltInTypeAccessor`は廃止されます。")]
        public static Task Read(this IBuiltinTypeAccessor @this, out WordList? wordListOrNull)
        {
            var task = @this.Read(out string? value_);
            wordListOrNull = value_ == null ? null : new(value_);
            return task;
        }

        [Obsolete("`IBuiltInTypeAccessor`は廃止されます。")]
        public static Task Write(this IBuiltinTypeAccessor @this, WordDictionary wordDictionary)
        {
            return @this.Write(wordDictionary.ToString());
        }
        [Obsolete("`IBuiltInTypeAccessor`は廃止されます。")]
        public static Task Write(this IBuiltinTypeAccessor @this, WordDictionary? wordDictionaryOrNull)
        {
            return @this.Write(wordDictionaryOrNull?.ToString());
        }
        [Obsolete("`IBuiltInTypeAccessor`は廃止されます。")]
        public static Task Write(this IBuiltinTypeAccessor @this, WordList wordList)
        {
            return @this.Write(wordList.ToString());
        }
        [Obsolete("`IBuiltInTypeAccessor`は廃止されます。")]
        public static Task Write(this IBuiltinTypeAccessor @this, WordList? wordListOrNull)
        {
            return @this.Write(wordListOrNull?.ToString());
        }

        public static TKey GetFirstKey<TKey, TValue>(this ICollection<KeyValuePair<TKey, TValue>> @this, TValue value) where TValue : class?
        {
            foreach (var (key, aEValue) in @this)
            {
                if (Equals(aEValue, value)) return key;
            }
            throw new Exception("指定された値を持つキーが見つかりませんでした。");
        }

        public static void QuickSort<T>(this Span<T> span, bool descending = false, IComparer<T>? comparer = null)
        {
            comparer ??= Comparer<T>.Default;

            var pivot = span[span.Length >> 1];
            int c_l = 0;
            int c_r = span.Length - 1;

            while (true)
            {
                if (descending)
                {
                    for (; comparer.Compare(span[c_l], pivot) > 0; c_l++) ;
                    for (; comparer.Compare(span[c_r], pivot) < 0; c_r--) ;
                }
                else
                {
                    for (; comparer.Compare(span[c_l], pivot) < 0; c_l++) ;
                    for (; comparer.Compare(span[c_r], pivot) > 0; c_r--) ;
                }

                if (c_l < c_r)
                {
                    Switch(ref span[c_l], ref span[c_r]);
                }
                else if (c_l == c_r)
                {
                    span[..(c_r - 1)].QuickSort(descending, comparer);
                    span[(c_l + 1)..].QuickSort(descending, comparer);
                    break;
                }
                else
                {
                    span[..c_r].QuickSort(descending, comparer);
                    span[c_l..].QuickSort(descending, comparer);
                    break;
                }

                c_l++;
                c_r--;
            }

            void Switch(ref T left, ref T right) => (left, right) = (right, left);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IndexInRange(int index, int length) => unchecked((uint)index < (uint)length);
    }

    namespace RangeExtentions
    {
        public static class RangeExtentions
        {
            public static RangeEnumerator GetEnumerator(this Range r) => new(r);

            public struct RangeEnumerator : IEnumerator<int>
            {
                private int _i;
                readonly private int _end;

                public int Current => _i;
                object IEnumerator.Current => _i;

                public RangeEnumerator(Range r)
                {
                    _i = r.Start.Value - 1;
                    _end = r.End.Value;
                }

                public bool MoveNext() => ++_i < _end;

                void IEnumerator.Reset() => throw new NotSupportedException();

                void IDisposable.Dispose() {}
            }
        }
    }
}
