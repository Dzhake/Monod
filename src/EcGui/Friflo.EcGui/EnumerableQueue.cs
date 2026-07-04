using System.Collections.Generic;

namespace Friflo.EcGui;

internal static class EnumerableQueue<T>
{
	internal static void Add(Queue<T> queue, int index)
	{
		T item = ItemUtils.CreateItem<T>();
		T[] array = queue.ToArray();
		queue.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			if (i == index)
			{
				queue.Enqueue(item);
			}
			queue.Enqueue(array[i]);
		}
		if (index == array.Length)
		{
			queue.Enqueue(item);
		}
	}

	internal static void Remove(Queue<T> queue, int index)
	{
		if (index == -1)
		{
			queue.Clear();
			return;
		}
		T[] array = queue.ToArray();
		queue.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			if (i != index)
			{
				queue.Enqueue(array[i]);
			}
		}
	}

	internal static void ChangeQueueItem(Queue<T> queue, T value, int index)
	{
		T[] array = queue.ToArray();
		queue.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			if (i == index)
			{
				queue.Enqueue(value);
			}
			else
			{
				queue.Enqueue(array[i]);
			}
		}
	}
}
