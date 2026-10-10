using Monod.AssetsModule.Utils;
using Monod.Utils.Extensions;
using Monod.Utils.General;
using OneOf;
using System.Reflection;

namespace Monod.AssetsModule;

public static class DefaultAssetPropParsers
{
    public static OneOf<object, string> Parser(string value)
    {
        int colonIndex = value.IndexOf(':');
        if (colonIndex == -1)
            return "No colon found. Use colon to separate full class name and name of the static method, e.g. \"Monod.AssetsModule.AssetParser:Binary\".";

        string className = value[..colonIndex];
        string methodName = value[(colonIndex + 1)..];
        var type = Type.GetType(className);
        if (type is null)
            return $"Could not find type \"{type}\"";

        MethodInfo? methodInfo = type.GetMethod(methodName, ReflectionUtils.StaticAnyAccess);
        if (methodInfo is null)
            return $"Could not find method \"{methodName}\" in type {type}";

        AssetParser? parser = methodInfo.TryCreateDelegate<AssetParser>();
        if (parser is null)
            return "Signature of the method doesn't match expected signature.";
        return parser;
    }
}
