using System;

namespace XNode.Examples.MathNodes;

[Serializable]
public class MathNode : Node
{
	public enum MathType
	{
		Add,
		Subtract,
		Multiply,
		Divide
	}

	[Input(ShowBackingValue.Unconnected, ConnectionType.Multiple, TypeConstraint.None, false)]
	public float a;

	[Input(ShowBackingValue.Unconnected, ConnectionType.Multiple, TypeConstraint.None, false)]
	public float b;

	[Output(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.None, false)]
	public float result;

	public MathType mathType;

	public override object GetValue(NodePort port)
	{
		float inputValue = GetInputValue("a", a);
		float inputValue2 = GetInputValue("b", b);
		result = 0f;
		if (port.fieldName == "result")
		{
			switch (mathType)
			{
			default:
				result = inputValue + inputValue2;
				break;
			case MathType.Subtract:
				result = inputValue - inputValue2;
				break;
			case MathType.Multiply:
				result = inputValue * inputValue2;
				break;
			case MathType.Divide:
				result = inputValue / inputValue2;
				break;
			}
		}
		return result;
	}
}
