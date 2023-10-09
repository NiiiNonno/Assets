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
        public static DynamicMask GetNew(Provider provider, string purpose = "Who got it? Come on out!") => TryGetNew(provider, out var r, purpose) ? r : throw new Exception($"���I�}�X�N�����̏���ɒB���܂����B�����̃}�X�N�̗p�r�͈ȉ��̒ʂ�ł��B\n{provider}");

        public void Dispose()
        {
            var f = _provider.TryReleaseMask(this);
        }

        public class Provider
        {
            const int LEN = 32;

            // ���n���̂��߁B
            // 0 => ���g�p, 1 => �g�p��, 2 => �s��
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