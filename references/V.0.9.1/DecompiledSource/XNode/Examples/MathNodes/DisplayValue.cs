using System;

namespace XNode.Examples.MathNodes;

public class DisplayValue : Node
{
	[Serializable]
	public class Anything
	{
	}

	[Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.None, false)]
	public Anything input;

	public object GetValue()
	{
		return GetInputValue<object>("input");
	}
}
