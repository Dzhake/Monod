namespace Monod.AssetsModule;

/// <summary>
/// Class for various fields and methods related to asset properties.
/// </summary>
public static class AssetProps
{
    public static void Initialize()
    {
        Parser = AssetProp.Info.AddOrGetValue("Parser");
    }

    /// <summary>
    /// Parsers for <see cref="AssetProp"/>. The key is property's id, the value is the parser. Parser accepts the value of the property, and returns some <see cref="object"/> based on it.
    /// </summary>
    public static readonly Dictionary<AssetProp, Func<string, object>> AssetPropParsers = new(); //TODO somewhere add parser for        parsers   uh

    /// <summary>
    /// Parse asset's property form the given string based on property's id/name.
    /// </summary>
    /// <param name="property">Property of the asset from the json, as a string.</param>
    /// <param name="propertyId">Id/name of the property.</param>
    /// <returns>Parsed result.</returns>
    public static object ParseAssetProp(string property, AssetProp propertyId) => AssetPropParsers[propertyId](property);

    /// <summary>
    /// Parser used to parse the asset from stream into the asset as an object.
    /// </summary>
    /// <remarks>
    /// String. Full class name + static method, separated by semicolon. Example: "Monod.Assets.AssetParsers;ParseTexture".<br/>
    /// Method's signature must be "object (AssetInfo)", aka Func&lt;AssetInfo, object&gt;.
    /// </remarks>
    public static AssetProp Parser;
}