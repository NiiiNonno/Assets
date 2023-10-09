using vec = System.Numerics.Vector3;

namespace Nonno.Assets.Presentation
{
    public readonly struct Displacement
    {
        readonly vec _l;
        readonly vec _a;

        public vec Linear => _l;
        public vec Angular => _a;

        public Displacement(vec linear, vec angular)
        {
            _l = linear;
            _a = angular;
        }
        public void Deconstruct(out vec linear, out vec angular)
        {
            linear = _l;
            angular = _a;
        }

        public static Displacement operator *(float a, Displacement b) => new(a * b._l, a * b._a);
    }
}