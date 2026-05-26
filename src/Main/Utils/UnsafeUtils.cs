using System.Runtime.CompilerServices;

namespace Monod.Utils;

public static class UnsafeUtils
{
    /// <summary>
    /// Get field of the <paramref name="obj"/> offset by <paramref name="offset"/>.
    /// </summary>
    /// <typeparam name="TStruct">Type of the struct, which contains the field.</typeparam>
    /// <typeparam name="TField">Type of the object stored in field.</typeparam>
    /// <param name="obj">Struct, which contains the field.</param>
    /// <param name="offset">Offset of the field relative to struct.</param>
    /// <returns>Value of the field.</returns>
    public static ref TField GetField<TStruct, TField>(ref TStruct obj, IntPtr offset) where TStruct : struct => ref Unsafe.As<byte, TField>(ref Unsafe.AddByteOffset(ref Unsafe.As<TStruct, byte>(ref obj), offset));

    public static nint GetFieldOffset<TStruct, TField>(ref TStruct obj, ref TField field) => Unsafe.ByteOffset(ref Unsafe.As<TStruct, byte>(ref obj), ref Unsafe.As<TField, byte>(ref field));
}
