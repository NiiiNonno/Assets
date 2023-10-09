using System.Numerics;
using vec = System.Numerics.Vector3;
using qtn = System.Numerics.Quaternion;

namespace Nonno.Assets.Presentation
{
    public readonly struct Transform
    {
        readonly vec _p;
        readonly qtn _r;

        public vec Position => _p;
        public qtn Rotation => _r;

        public Transform(vec position, qtn rotation)
        {
            _p = position;
            _r = rotation;
        }
        public void Deconstruct(out vec position, out qtn rotation)
        {
            position = _p;
            rotation = _r;
        }

        public static Transform Identity { get; } = new(vec.Zero, qtn.Identity);

        public static Transform operator +(Transform a, Displacement b)
        {
            var p = a.Position + b.Linear;
            var eular = b.Angular;
            var r = a.Rotation * qtn.CreateFromYawPitchRoll(eular.X, eular.Y, eular.Z);
            return new(p, r);
        }
        public static Transform operator -(Transform a, Displacement b)
        {
            var p = a.Position - b.Linear;
            var eular = -b.Angular;
            var r = a.Rotation * qtn.CreateFromYawPitchRoll(eular.X, eular.Y, eular.Z);
            return new(p, r);
        }
        public static Transform operator -(Transform a) => new(-a.Position, qtn.Inverse(a.Rotation));
    }
}