namespace NScript.CommonApi;

public ref struct Payload
{
    public static Payload Empty => new Payload(IntPtr.Zero, 0);

    public IntPtr DataPointer;
    public int Length;

    public Payload(IntPtr dataPointer, int length)
    {
        DataPointer = dataPointer;
        Length = length;
    }

    public unsafe Span<byte> AsSpan()
    {
        return new Span<byte>(DataPointer.ToPointer(), Length);
    }

    public unsafe byte[] ToArray()
    {
        byte[] array = new byte[Length];
        fixed (byte* pArray = array)
        {
            Buffer.MemoryCopy(DataPointer.ToPointer(), pArray, Length, Length);
        }
        return array;
    }
}
