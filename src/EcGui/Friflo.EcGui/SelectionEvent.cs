namespace Friflo.EcGui;

internal delegate void SelectionEvent<in T>(T item, bool selected, SelectionEventType type);
