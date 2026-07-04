using System;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal sealed class DrawContext
{
	internal Entity entity;

	internal bool multiLine;

	internal Rect rect;

	internal Rect drawRect;

	internal OnEdit? onEdit;

	internal OnError? onError;

	internal bool showTooltips;

	internal int contextItemRow;

	internal int maxRowCount = 10;

	internal bool fixedTabledHeight;

	internal readonly bool syncTables;

	internal void Edit()
	{
		onEdit?.Invoke();
	}

	internal void Error(string message, Exception? exception)
	{
		onError?.Invoke(message, exception);
	}

	internal DrawContext(bool syncTables)
	{
		this.syncTables = syncTables;
	}
}
