using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace Vintagestory.API.Datastructures;

public abstract class ScalarAttribute<T>
{
	public T value;

	public virtual bool Equals(IWorldAccessor worldForResolve, IAttribute attr)
	{
		if (!attr.GetValue().Equals(value))
		{
			return EqualityUtil.NumberEquals(value, attr.GetValue());
		}
		return true;
	}

	public override bool Equals(object b)
	{
		return value.Equals(b);
	}

	public virtual object GetValue()
	{
		return value;
	}

	public virtual void SetValue(T newval)
	{
		value = newval;
	}

	public override string ToString()
	{
		return value.ToString();
	}

	public virtual string ToJsonToken()
	{
		return value.ToString();
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}
}
