using Monod.Shared.Enums;

namespace Monod.AssetsModule;

/// <summary>
/// Class for various fields and methods related to asset properties.
/// </summary>
public static class AssetProps
{
    public static NamedExtEnum DefaultValues()
    {
        return new([nameof(Parser)]);
    }

    /// <summary>
    /// <see cref="NamedExtEnum"/> with one value being a name of a property an asset can have and it's unique id. Used to speed up search/parse operations.
    /// </summary>
    public static readonly NamedExtEnum Values = DefaultValues();

    /// <summary>
    /// Get id of the property by it's name.
    /// </summary>
    /// <param name="propertyName">Name of the asset's property.</param>
    /// <returns>Id associated with that property.</returns>
    public static int NameToId(string propertyName) => Values.GetValue(propertyName);

    /// <summary>
    /// Get name of the property by it's id.
    /// </summary>
    /// <param name="id">Id of the asset's property.</param>
    /// <returns>Name of the property associated with that id.</returns>
    public static string IdToName(int id) => Values.GetName(id);

    /// <summary>
    /// Parsers for <see cref="Values"/>. The key is property's id, the value is the parser. Parser accepts the value of the property, and produces some <see cref="object"/> based on it.
    /// </summary>
    public static readonly Dictionary<int, Func<string, object>> AssetPropParsers = new(); //TODO somewhere add parser for        parsers   uh

    /// <summary>
    /// Parse asset's property form the given string based on property's id/name.
    /// </summary>
    /// <param name="property">Property of the asset from the json, as a string.</param>
    /// <param name="propertyId">Id/name of the property.</param>
    /// <returns>Parsed result.</returns>
    public static object ParseAssetProp(string property, int propertyId) => AssetPropParsers[propertyId](property);



#pragma warning disable CA1805
    /// <summary>
    /// Parser used to parse the asset from stream into the asset as an object.
    /// </summary>
    /// <remarks>
    /// Format: string: full class name + static method, separated by semicolon. Example: "Monod.Assets.AssetParsers;ParseTexture".<br/>
    /// Static method's signature must be "object (AssetInfo)", aka Func&lt;AssetInfo, object&gt;.
    /// </remarks>
    public static readonly int Parser = 0; //For clarity
#pragma warning restore CA1805
}