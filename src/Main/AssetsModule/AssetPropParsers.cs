using Monod.AssetsModule.Utils;
using Monod.Utils.Extensions;
using Monod.Utils.General;
using OneOf;
using OneOf.Types;

namespace Monod.AssetsModule;

public static class DefaultAssetPropParsers
{
    /// <summary>
    /// Parse the specified <see cref="AssetProps.Parser"/>. Format is "FullNamespace.ClassName:MethodName".
    /// </summary>
    /// <param name="value">String to parse.</param>
    /// <returns>Error message (string) on failure, </returns>
    public static OneOf<object, Error<string>> Parser(string value)
    {
        int colonIndex = value.IndexOf(':');
        if (colonIndex == -1)
            return new Error<string>("No colon found. Use colon to separate full class name and name of the static method, e.g. \"Monod.AssetsModule.AssetParser:Binary\".");

        string className = value[..colonIndex];
        string methodName = value[(colonIndex + 1)..];
        var type = Type.GetType(className);
        if (type is null)
            return new Error<string>($"Could not find type \"{type}\"");

        var methodInfo = type.GetMethod(methodName, ReflectionUtils.StaticAnyAccess);
        if (methodInfo is null)
            return new Error<string>($"Could not find method \"{methodName}\" in type {type}");

        var parser = methodInfo.TryCreateDelegate<AssetParser>();
        if (parser is null)
            return new Error<string>("Signature of the method doesn't match expected signature.");
        return parser;
    }
}
