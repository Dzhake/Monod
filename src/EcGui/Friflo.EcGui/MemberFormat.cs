using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

public abstract class MemberFormat
{
	protected Entity currentEntity;

	protected MemberPath? currentMember;

	internal abstract string WriteAsString(Column[] columns, IReadOnlyList<Entity> entities);

	public void GetValue<TMember>(out TMember value, out Exception? exception)
	{
		EntityUtils.GetEntityComponentMember<TMember>(currentEntity, currentMember, out value, out exception);
	}

	public abstract void NotImplemented();

	public abstract void Append(sbyte value, Exception? exception);

	public abstract void Append(short value, Exception? exception);

	public abstract void Append(int value, Exception? exception);

	public abstract void Append(long value, Exception? exception);

	public abstract void Append(byte value, Exception? exception);

	public abstract void Append(ushort value, Exception? exception);

	public abstract void Append(uint value, Exception? exception);

	public abstract void Append(ulong value, Exception? exception);

	public abstract void Append(float value, Exception? exception);

	public abstract void Append(double value, Exception? exception);

	public abstract void Append(decimal value, Exception? exception);

	public abstract void Append(char value, Exception? exception);

	public abstract void Append(bool value, Exception? exception);

	public abstract void Append(string value, Exception? exception);

	public abstract void Append(DateTime value, Exception? exception);

	public abstract void Append(object value, Exception? exception);
}
