namespace Friflo.EcGui;

internal interface IMember
{
	internal void DrawMemberNode(DrawNode drawNode);

	internal void AddExplorerColumns(QueryExplorer explorer);

	internal bool HasFilterMatches(string filter);
}
