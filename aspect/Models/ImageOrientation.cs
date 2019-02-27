namespace Aspect.Models
{
    // https://docs.microsoft.com/en-us/windows/desktop/properties/props-system-photo-orientation
    // NOTE: in practice Rotate90 and 270 seem to be switched from the official documentation
    public enum ImageOrientation : ushort
    {
        Normal = 1,
        FlipHorizontal = 2,
        Rotate180 = 3,
        FlipVertical = 4,
        Transpose = 5,
        Rotate90 = 6,
        Transverse = 7,
        Rotate270 = 8
    }
}
