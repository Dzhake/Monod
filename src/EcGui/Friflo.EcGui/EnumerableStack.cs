using System.Collections.Generic;

namespace Friflo.EcGui;

internal static class EnumerableStack<T>
{
	internal static void Add(Stack<T> stack, int index)
	{
		T item = ItemUtils.CreateItem<T>();
		T[] array = stack.ToArray();
		stack.Clear();
		if (index == array.Length)
		{
			stack.Push(item);
		}
		for (int num = array.Length - 1; num >= 0; num--)
		{
			stack.Push(array[num]);
			if (num == index)
			{
				stack.Push(item);
			}
		}
	}

	internal static void Remove(Stack<T> stack, int index)
	{
		if (index == -1)
		{
			stack.Clear();
			return;
		}
		T[] array = stack.ToArray();
		stack.Clear();
		for (int num = array.Length - 1; num >= 0; num--)
		{
			if (num != index)
			{
				stack.Push(array[num]);
			}
		}
	}

	internal static void ChangeStackItem(Stack<T> stack, T value, int index)
	{
		T[] array = stack.ToArray();
		stack.Clear();
		for (int num = array.Length - 1; num >= 0; num--)
		{
			if (num == index)
			{
				stack.Push(value);
			}
			else
			{
				stack.Push(array[num]);
			}
		}
	}
}
