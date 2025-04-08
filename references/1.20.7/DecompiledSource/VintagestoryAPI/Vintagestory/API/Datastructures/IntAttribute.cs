using System;
using System.IO;

namespace Vintagestory.API.Datastructures;

public class IntAttribute : ScalarAttribute<int>, IAttribute
{
	public IntAttribute()
	{
	}

	public IntAttribute(int value)
	{
		base.value = value;
	}

	public void FromBytes(BinaryReader stream)
	{
		value = stream.ReadInt32();
	}

	public void ToBytes(BinaryWriter stream)
	{
		stream.Write(value);
	}

	public int GetAttributeId()
	{
		return 1;
	}

	public IAttribute Clone()
	{
		return new IntAttribute(value);
	}

	Type IAttribute.GetType()
	{
		return GetType();
	}
}
