using System;
using System.IO;

namespace Vintagestory.API.Datastructures;

public class StringAttribute : ScalarAttribute<string>, IAttribute
{
	public StringAttribute()
	{
		value = "";
	}

	public StringAttribute(string value)
	{
		base.value = value;
	}

	public void ToBytes(BinaryWriter stream)
	{
		if (value == null)
		{
			value = "";
		}
		stream.Write(value);
	}

	public void FromBytes(BinaryReader stream)
	{
		value = stream.ReadString();
	}

	public int GetAttributeId()
	{
		return 5;
	}

	public override string ToJsonToken()
	{
		return "\"" + value + "\"";
	}

	public IAttribute Clone()
	{
		return new StringAttribute(value);
	}

	Type IAttribute.GetType()
	{
		return GetType();
	}
}
