#if !NET6_0_OR_GREATER
namespace ForVersionCompatibility
{
    internal class Array
    {
        public void Clear(System.Array array) => System.Array.Clear(array, 0, array.Length);
    }
}
#endif