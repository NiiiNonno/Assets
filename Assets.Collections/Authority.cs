#nullable enable

namespace Nonno.Assets.Collections
{
    public class Authority : Context.Index
    {
        readonly string _name;

        public string Name => _name;

        public Authority(string name) : base(Context)
        {
            _name = name;
        }

        public static new Context<Authority> Context { get; } = new();

        public static Authority GetOrCreate(string name)
        {
            foreach (var tag in Context.Indexes) if (tag.Name == name) return tag;
            return new Authority(name);
        }

        public static Authority Unknown { get; } = new("unknown");
        public static Authority Natural { get; } = new("natural");
    }

    public class Authority<I> : Authority
    {
        public Authority(string name) : base(name)
        {
        }
    }
}