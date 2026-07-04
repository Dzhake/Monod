namespace Friflo.EcGui;

internal interface IInspectorComponent
{
	void DrawComponentNode(DrawNode drawNode);

	bool HasFilterMatches(string filter);
}
