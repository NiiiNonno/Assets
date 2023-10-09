#nullable enable
using System;
using System.Threading;
using static Nonno.Assets.Utils;

namespace Nonno.Assets
{
    public readonly struct DynamicMask : IDisposable
    {
        readonly int _mask;
        readonly Provider _provider;

        private DynamicMask(Provider provider, int mask)
        {
            _provider = provider;
            _mask = mask;
        }

        public static implicit operator int(DynamicMask v) => v._mask;

        public static bool TryGetNew(Provider provider, out DynamicMask mask, string purpose = "Who got it? Come on out!") => provider.TryGetMask(out mask, purpose);
        public static DynamicMask GetNew(Provider provider, string purpose = "Who got it? Come on out!") => TryGetNew(provider, out var r, purpose) ? r : throw new Exception($"動的マスク生成の上限に達しました。既存のマスクの用途は以下の通りです。\n{provider}");

        public void Dispose()
        {
            var f = _provider.TryReleaseMask(this);
        }

        public class Provider
        {
            const int LEN = 32;

            // 原始性のため。
            // 0 => 未使用, 1 => 使用中, 2 => 不可
            readonly int[] _flags = new int[LEN];
            readonly string[] _purposes = new string[LEN];

            public bool TryGetMask(out DynamicMask mask, string purpose)
            {
                for (var i = 0; i < LEN; i++)
                {
                    if (Interlocked.CompareExchange(ref _flags[i], 1, 0) == 0)
                    {
                        mask = new(this, OneHot(i));
                        _purposes[i] = purpose;
                        return true;
                    }
                }
                mask = default;
                return false;
            }

            public bool TryReleaseMask(DynamicMask mask)
            {
                if (!ReferenceEquals(mask._provider, this)) return false;

                _flags[ShiftOf(mask._mask)] = 0;
                return true;
            }

            public void Preserve(int mask)
            {
                for (var i = 0; i < LEN; i++)
                {
                    if ((mask & OneHot(i)) == 1) _flags[i] = 2;
                }
            }
        }
    }
}