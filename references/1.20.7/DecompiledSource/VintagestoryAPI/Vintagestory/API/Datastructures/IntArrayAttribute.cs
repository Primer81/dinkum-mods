using System;
using System.IO;
using Vintagestory.API.Util;

namespace Vintagestory.API.Datastructures;

public class IntArrayAttribute : ArrayAttribute<int>, IAttribute
{
	public ushort[] AsUShort
	{
		get
		{
			ushort[] vals = new ushort[value.Length];
			for (int i = 0; i < vals.Length; i++)
			{
				vals[i] = (ushort)value[i];
			}
			return vals;
		}
	}

	public uint[] AsUint
	{
		get
		{
			uint[] vals = new uint[value.Length];
			for (int i = 0; i < vals.Length; i++)
			{
				vals[i] = (uint)value[i];
			}
			return vals;
		}
	}

	public IntArrayAttribute()
	{
	}

	public IntArrayAttribute(int[] value)
	{
		base.value = value;
	}

	public IntArrayAttribute(uint[] value)
	{
		base.value = new int[value.Length];
		for (int i = 0; i < value.Length; i++)
		{
			base.value[i] = (int)value[i];
		}
	}

	public IntArrayAttribute(ushort[] value)
	{
		base.value = new int[value.Length];
		for (int i = 0; i < value.Length; i++)
		{
			base.value[i] = value[i];
		}
	}

	public void ToBytes(BinaryWriter stream)
	{
		stream.Write(value.Length);
		for (int i = 0; i < value.Length; i++)
		{
			stream.Write(value[i]);
		}
	}

	public void FromBytes(BinaryReader stream)
	{
		int quantity = stream.ReadInt32();
		value = new int[quantity];
		for (int i = 0; i < quantity; i++)
		{
			value[i] = stream.ReadInt32();
		}
	}

	public int GetAttributeId()
	{
		return 11;
	}

	public void AddInt(params int[] val)
	{
		if (value == null || value.Length == 0)
		{
			value = val;
		}
		else
		{
			value = value.Append(val);
		}
	}

	public void RemoveInt(int val)
	{
		value = value.Remove(val);
	}

	public IAttribute Clone()
	{
		return new IntArrayAttribute((int[])value.Clone());
	}

	Type IAttribute.GetType()
	{
		return GetType();
	}
}
