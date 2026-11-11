using Monod.Utils.Exceptions;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Monod.Utils.Enums;

/// <summary>
/// Extensible [Flags]-like enum, where each registered name is associated with a single-bit value.
/// Every newly registered flag gets the next free bit, so flag number <c>i</c> (0-based) has value <c>2^i</c> —
/// each next value is twice as high as the previous one. Because values are sparse, they are never used to
/// index a list, so <typeparamref name="TValue"/> can be an unmanaged type of any size — the only limit is
/// the number of bits it provides (<see cref="Capacity"/>).
/// </summary>
/// <typeparam name="TValue">Type of the flags enum. Must be unmanaged.</typeparam>
/// <remarks>
/// Usage example:
/// <code>
/// public record struct AssetTags(ulong inner)
/// {
///     public static ExtFlagsInfo&lt;AssetTags&gt; Info = new();
/// }
/// </code>
/// Bit 0 (value 1) is assigned to the first registered flag, so <c>default</c> (all bits zero) means
/// "no flags" and cannot be registered. This class is not thread-safe.
/// </remarks>
public sealed class ExtFlagsInfo<TValue> where TValue : unmanaged
{
    private const int BitsPerByte = 8;

    /// <summary>Size of <typeparamref name="TValue"/> in bytes.</summary>
    private static readonly int SizeInBytes = Unsafe.SizeOf<TValue>();

    /// <summary>
    /// List of registered flag names, where the list index is the flag's bit index:
    /// the flag with value <c>2^i</c> is stored at <c>InternalNames[i]</c>.
    /// </summary>
    private readonly List<string> InternalNames;

    /// <summary>
    /// Dictionary of registered flags, where the key is the flag's name, and the value is the flag's single-bit value.
    /// </summary>
    private readonly Dictionary<string, TValue> Values;

    /// <summary>Cached read-only view over <see cref="InternalNames"/>, so that <see cref="Names"/> doesn't allocate on every access.</summary>
    private readonly IReadOnlyList<string> NamesView;

    /// <summary>
    /// Readonly list of all registered flag names, in registration order. Useful for iterating over names/debug purposes.
    /// </summary>
    public IReadOnlyList<string> Names => NamesView;

    /// <summary>
    /// Number of registered flags. The next registered flag will get bit <see cref="Count"/>, i.e. value <c>2^Count</c>.
    /// </summary>
    public int Count => InternalNames.Count;

    /// <summary>
    /// Maximum number of flags that can ever be registered for this <typeparamref name="TValue"/> — the number of bits it has.
    /// </summary>
    public int Capacity => SizeInBytes * BitsPerByte;

    /// <summary>
    /// Value with all currently registered flags set. Equals <c>default</c> if nothing is registered.
    /// </summary>
    public TValue AllFlags => CreateMask(InternalNames.Count);

    /// <summary>
    /// Initialize a new empty instance of the <see cref="ExtFlagsInfo{TValue}"/>.
    /// </summary>
    public ExtFlagsInfo()
    {
        InternalNames = new();
        Values = new(StringComparer.Ordinal);
        NamesView = InternalNames.AsReadOnly();
    }

    /// <summary>
    /// Initialize a new instance of the <see cref="ExtFlagsInfo{TValue}"/> with the specified <paramref name="names"/>.
    /// The <c>i</c>-th name gets value <c>2^i</c>. Same as calling <see cref="AddOrGetValue"/> for each name, but more performant.
    /// </summary>
    /// <param name="names">Names of the flags to register. Must not contain nulls or duplicates.</param>
    public ExtFlagsInfo(IEnumerable<string> names)
    {
        ArgumentNullException.ThrowIfNull(names);

        InternalNames = new(names.TryGetNonEnumeratedCount(out int count) ? count : 4);
        Values = new(InternalNames.Capacity, StringComparer.Ordinal);

        foreach (string name in names)
        {
            ArgumentNullException.ThrowIfNull(name);

            int bit = InternalNames.Count;
            if (bit >= Capacity)
                throw new InvalidOperationException($"Cannot register flag '{name}': all {Capacity} bits of '{typeof(TValue).Name}' are already in use.");

            if (!Values.TryAdd(name, CreateSingleBitValue(bit)))
                throw new ArgumentException($"Flag name '{name}' is specified more than once.", nameof(names));

            InternalNames.Add(name);
        }

        NamesView = InternalNames.AsReadOnly();
    }

    /// <summary>
    /// Register a new flag with the specified <paramref name="name"/> and return its value (the next free single-bit
    /// value, i.e. the next power of two), or return the value of the already registered flag with this name.
    /// </summary>
    /// <param name="name">Name of the flag. Must be unique among registered flags.</param>
    /// <returns>Value associated with the name.</returns>
    public TValue AddOrGetValue(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        // Single hash probe for both the hit and the miss path.
        ref var slot = ref CollectionsMarshal.GetValueRefOrAddDefault(Values, name, out bool exists);
        if (exists)
            return slot;

        int bit = InternalNames.Count;
        if (bit >= Capacity)
        {
            // GetValueRefOrAddDefault already inserted the key with a default value — roll it back,
            // so that a failed registration leaves the enum unchanged.
            Values.Remove(name);
            throw new InvalidOperationException($"Cannot register flag '{name}': all {Capacity} bits of '{typeof(TValue).Name}' are already in use.");
        }

        var value = CreateSingleBitValue(bit);
        slot = value;
        InternalNames.Add(name);
        return value;
    }

    /// <summary>
    /// Get a flag's value based on its name.
    /// </summary>
    /// <param name="name">Name that was used when registering the flag.</param>
    /// <returns>Value associated with the specified name.</returns>
    public TValue GetValue(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (Values.TryGetValue(name, out var value))
            return value;

        Guard.KeyNotFoundException(name);
        return default; // unreachable
    }

    /// <summary>
    /// Get a flag's value based on its name in an error-safe way.
    /// </summary>
    /// <param name="name">The name of the flag to get.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified name, if the name is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
    /// <returns>Whether the value was found.</returns>
    public bool TryGetValue(string name, out TValue value)
    {
        ArgumentNullException.ThrowIfNull(name);
        return Values.TryGetValue(name, out value);
    }

    /// <summary>
    /// Get the names of all registered flags present in <paramref name="value"/>. Names are returned in flag
    /// registration order (i.e. ordered by value). Bits that are set but were never registered are silently skipped.
    /// </summary>
    /// <param name="value">Flags value to translate to names. May be any combination of bits.</param>
    /// <returns>Names of all registered flags present in the value; empty, if no registered flags are set.</returns>
    public string[] GetNames(TValue value)
    {
        var bytes = AsBytes(value);

        CollectNames(bytes, null, out int count, out _);
        if (count == 0)
            return Array.Empty<string>();

        string[] names = new string[count];
        CollectNames(bytes, names, out _, out _);
        return names;
    }

    /// <summary>
    /// Get the names of all registered flags present in <paramref name="value"/> in an error-safe way.
    /// Unlike <see cref="GetNames"/>, this is strict: returns <c>false</c> if the value contains any bit that was
    /// never registered. A value with no bits set returns <c>true</c> with an empty array.
    /// </summary>
    /// <param name="value">Flags value to translate to names.</param>
    /// <param name="names">When this method returns, contains names of all registered flags present in the value, if every set bit is registered; otherwise, <c>null</c>.</param>
    /// <returns>Whether every set bit of the value belongs to a registered flag.</returns>
    public bool TryGetNames(TValue value, [NotNullWhen(true)] out string[]? names)
    {
        var bytes = AsBytes(value);

        CollectNames(bytes, null, out int count, out bool allBitsRegistered);
        if (!allBitsRegistered)
        {
            names = null;
            return false;
        }

        if (count == 0)
        {
            names = Array.Empty<string>();
            return true;
        }

        names = new string[count];
        CollectNames(bytes, names, out _, out _);
        return true;
    }

    /// <summary>
    /// Combine several registered flags into a single value (bitwise OR of their values). Exists because
    /// the <c>|</c> operator is not available on an arbitrary <typeparamref name="TValue"/>.
    /// </summary>
    /// <param name="names">Names of the flags to combine. All must be registered.</param>
    /// <exception cref="KeyNotFoundException">One of the names is not registered.</exception>
    public TValue Combine(ReadOnlySpan<string> names)
    {
        TValue result = default;
        foreach (string name in names)
        {
            ArgumentNullException.ThrowIfNull(name);

            if (!Values.TryGetValue(name, out var flag))
            {
                Guard.KeyNotFoundException(name);
                return default; // unreachable
            }

            OrInto(ref result, flag);
        }
        return result;
    }

    /// <inheritdoc cref="Combine(ReadOnlySpan{string})"/>
    public TValue Combine(params string[] names)
    {
        ArgumentNullException.ThrowIfNull(names);
        return Combine(names.AsSpan());
    }

    /// <summary>
    /// Create a value with only bit <paramref name="bitIndex"/> set. Assumes little-endian layout
    /// (bit <c>i</c> is bit <c>i % 8</c> of byte <c>i / 8</c>), which holds on every platform .NET supports today.
    /// </summary>
    private static TValue CreateSingleBitValue(int bitIndex)
    {
        TValue value = default;
        ref byte firstByte = ref Unsafe.As<TValue, byte>(ref value);
        Unsafe.Add(ref firstByte, bitIndex >>> 3) |= (byte)(1 << (bitIndex & 7));
        return value;
    }

    /// <summary>
    /// Create a value with the low <paramref name="bitCount"/> bits set. <paramref name="bitCount"/> must not exceed <see cref="Capacity"/>.
    /// </summary>
    private static TValue CreateMask(int bitCount)
    {
        TValue value = default;
        ref byte firstByte = ref Unsafe.As<TValue, byte>(ref value);

        int fullBytes = bitCount >>> 3;
        for (int i = 0; i < fullBytes; i++)
            Unsafe.Add(ref firstByte, i) = 0xFF;

        int remainder = bitCount & 7;
        if (remainder != 0)
            Unsafe.Add(ref firstByte, fullBytes) |= (byte)(0xFF >>> (8 - remainder));

        return value;
    }

    /// <summary>Bitwise OR of two unmanaged values, byte by byte.</summary>
    private static void OrInto(ref TValue destination, TValue source)
    {
        var destinationBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref destination, 1));
        var sourceBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref source, 1));
        for (int i = 0; i < destinationBytes.Length; i++)
            destinationBytes[i] |= sourceBytes[i];
    }

    private static ReadOnlySpan<byte> AsBytes(TValue value)
        => MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref value, 1));

    /// <summary>
    /// Iterate over all set bits in <paramref name="bytes"/>. When <paramref name="destination"/> is <c>null</c>,
    /// only counts the registered flags; otherwise also writes their names into it. Additionally reports whether
    /// every set bit belongs to a registered flag.
    /// </summary>
    private void CollectNames(ReadOnlySpan<byte> bytes, string?[]? destination, out int count, out bool allBitsRegistered)
    {
        int registered = InternalNames.Count;
        int found = 0;
        bool allRegistered = true;

        for (int byteIndex = 0; byteIndex < bytes.Length; byteIndex++)
        {
            int bits = bytes[byteIndex];
            while (bits != 0)
            {
                int bitIndex = (byteIndex * BitsPerByte) + BitOperations.TrailingZeroCount(bits);
                if (bitIndex < registered)
                {
                    destination?[found] = InternalNames[bitIndex];
                    found++;
                }
                else
                {
                    allRegistered = false;
                }

                bits &= bits - 1; // clear the lowest set bit
            }
        }

        count = found;
        allBitsRegistered = allRegistered;
    }
}
