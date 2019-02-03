namespace Aspect.Models
{
    public struct Dimensions
    {
        public Dimensions(uint width, uint height)
        {
            Width = width;
            Height = height;
        }

        public uint Width { get; }
        public uint Height { get; }
    }
}
