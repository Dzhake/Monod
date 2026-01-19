using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Monod.Utils.General;

/// <summary>
/// Struct that represents <see cref="FilePath"/> and it's <see cref="Depth"/> (how deep it's directory is relative to drive's root).
/// </summary>
public readonly struct FileWithDepth : IComparable<FileWithDepth>, IEquatable<FileWithDepth>
{
    /// <summary>
    /// Absolute path to the file.
    /// </summary>
    public readonly string FilePath;
    
    /// <summary>
    /// Depth of the file (how deep it's directory is relative to drive's root).
    /// </summary>
    public readonly int Depth;

    /// <summary>
    /// Initialize a new instance of <see cref="FileWithDepth"/> with the specified <paramref name="filePath"/>.
    /// </summary>
    /// <param name="filePath"></param>
    public FileWithDepth(string filePath)
    {
        FilePath = filePath;
        Depth = CalcDepth(filePath);
    }

    /// <summary>
    /// Calculate depth of a given file path.
    /// </summary>
    /// <param name="path">Absolute path to the file.</param>
    /// <returns>Depth of the file (how deep it's directory is relative to drive's root).</returns>
    public static int CalcDepth(string path)
    {
        char separator = Path.DirectorySeparatorChar;
        char altSeparator = Path.AltDirectorySeparatorChar;
        return path.Count(c => c == separator || c == altSeparator);
    }

    /// <inheritdoc />
    public int CompareTo(FileWithDepth other)
        => other.Depth.CompareTo(Depth);

    /// <inheritdoc />
    public bool Equals(FileWithDepth other) => FilePath == other.FilePath;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is FileWithDepth other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => FilePath.GetHashCode();

    /// <summary>
    /// Indicates whether the one object is equal to another object of the same type.
    /// </summary>
    /// <param name="left">First object.</param>
    /// <param name="right">Second object.</param>
    /// <returns>Whether first and second objects are equal.</returns>
    public static bool operator ==(FileWithDepth left, FileWithDepth right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Indicates whether the one object is not equal to another object of the same type.
    /// </summary>
    /// <param name="left">First object.</param>
    /// <param name="right">Second object.</param>
    /// <returns>Whether first and second objects are not equal.</returns>
    public static bool operator !=(FileWithDepth left, FileWithDepth right)
    {
        return !(left == right);
    }
}