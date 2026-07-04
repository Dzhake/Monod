namespace Friflo.EcGui;

internal struct MatchInfo
{
	internal string component;

	internal string field;

	internal string filter;

	public override string ToString()
	{
		return $"{component}.{field} filter: {filter}";
	}
}
