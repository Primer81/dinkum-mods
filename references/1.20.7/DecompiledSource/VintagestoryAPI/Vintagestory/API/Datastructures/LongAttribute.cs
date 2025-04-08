using System;
using System.IO;

namespace Vintagestory.API.Datastructures;

public class LongAttribute : ScalarAttribute<long>, IAttribute
{
	public LongAttribute()
	{
	}

	public LongAttribute(long value)
	{
		base.value = value;
	}

	public void FromBytes(BinaryReader stream)
	{
		value = stream.ReadInt64();
	}

	public void ToBytes(BinaryWriter stream)
	{
		stream.Write(value);
	}

	public int GetAttributeId()
	{
		return 2;
	}

	public IAttribute Clone()
	{
		return new LongAttribute(value);
	}

	Type IAttribute.GetType()
	{
		return GetType();
	}
}
