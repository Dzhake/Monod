using System;

namespace Friflo.EcGui;

internal interface IContainer : IDisposable
{
	int Count { get; }

	bool IsNull { get; }

	Type ItemType { get; }

	void StartIterator();

	bool MoveNext();

	void SeekCurrent(int offset);

	void Add(int index);

	void Remove(int index);
}
