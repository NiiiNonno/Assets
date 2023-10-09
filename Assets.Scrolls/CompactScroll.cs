using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Nonno.Assets.Scrolls
{
    public class CompactScroll : SectionScroll<CompactSection>
    {
        public const string EXTENSION = ".cst";

        readonly Dictionary<ulong, CompactSection> _loadeds;
        readonly Random _rand;

        public ZipArchive ZipArchive { get; }
        public long MaxLength { get; }
        protected override CompactSection this[ulong index]
        {
            get
            {
                if (!_loadeds.TryGetValue(index, out var section))
                {
                    section = new(GetEntry(index), MaxLength);
                    section.Init();
                    _loadeds[index] = section;
                }

                return section;
            }
        }

        protected CompactScroll(ZipArchive zipArchive, long maxLength = long.MaxValue) : base(0)
        {
            _loadeds = new Dictionary<ulong, CompactSection>();
            _rand = new Random();

            ZipArchive = zipArchive;
            MaxLength = maxLength;
        }

        public override IScroll Copy() => throw new NotSupportedException();

        protected override void CreateSection(ulong number)
        {
            var entry = ZipArchive.CreateEntry(entryName: GetEntryName(number));
            var sect = new CompactSection(entry, MaxLength);
            sect.Init();
            _loadeds.Add(number, sect);
        }
        protected override void DeleteSection(ulong number)
        {
            var sect = this[number];
            sect.Dispose();
            sect.ZipArchiveEntry.Delete();
        }

        protected override void Dispose(bool disposing)
        {
            Debug.Assert(_loadeds.Count != 0);

            foreach (var item in _loadeds.Values)
            {
                item.Dispose();
            }

            base.Dispose(disposing);
        }
        protected override ValueTask DisposeAsync(bool disposing)
        {
            Debug.Assert(_loadeds.Count != 0);

            foreach (var item in _loadeds.Values)
            {
                item.Dispose();
            }

            return base.DisposeAsync(disposing);
        }

        protected override ulong FindVacantNumber(ulong? previousSectionNumber = null, ulong? nextSectionNumber = null)
        {
            while (true)
            {
                var r = (ulong)_rand.NextInt64();
                if (r == 0) continue;
                var zE = ZipArchive.GetEntry(GetEntryName(r));
                if (zE is null) return r;
            }
        }

        public ZipArchiveEntry GetEntry(ulong number) => ZipArchive.GetEntry(GetEntryName(number)) ?? throw new KeyNotFoundException();

        public static string GetEntryName(ulong number) => number.ToString("X16");
    }

    public class CompactSection : StreamSection
    {
        readonly ZipArchiveEntry _entry;

        public ZipArchiveEntry ZipArchiveEntry => _entry;

        protected override long Length
        {
            get
            {
                if (Stream is null)
                {
                    return _entry.Length;
                }
                else
                {
                    return Stream.Length;
                }
            }
        }

        /// <remarks>
        /// 初期化には別途<see cref="Init"/>の呼び出しが必要です。
        /// </remarks>
        public CompactSection(ZipArchiveEntry entry, long maxLength = long.MaxValue) : base(maxLength)
        {
            _entry = entry;
        }

        protected override Stream GetStream() => _entry.Open();
    }
}
