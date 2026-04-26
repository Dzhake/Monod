using System.Diagnostics.CodeAnalysis;

namespace Monod.AssetsModule.Exceptions;

public class AssetManagerNotFoundException : Exception
{
    public string Prefix;

    public override string Message => $"Could not find asset manager with prefix: \"{Prefix}\". Verify that it was registered, and was not unregistered, or use GetOrDefault when requesting asset.";

    public AssetManagerNotFoundException(string prefix)
    {
        Prefix = prefix;
    }

    [DoesNotReturn]
    public static void Throw(string prefix) => throw new AssetManagerNotFoundException(prefix);
}
