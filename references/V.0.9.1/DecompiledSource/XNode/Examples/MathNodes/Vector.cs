using UnityEngine;

namespace XNode.Examples.MathNodes;

public class Vector : Node
{
	[Input(ShowBackingValue.Unconnected, ConnectionType.Multiple, TypeConstraint.None, false)]
	public float x;

	[Input(ShowBackingValue.Unconnected, ConnectionType.Multiple, TypeConstraint.None, false)]
	public float y;

	[Input(ShowBackingValue.Unconnected, ConnectionType.Multiple, TypeConstraint.None, false)]
	public float z;

	[Output(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.None, false)]
	public Vector3 vector;

	public override object GetValue(NodePort port)
	{
		vector.x = GetInputValue("x", x);
		vector.y = GetInputValue("y", y);
		vector.z = GetInputValue("z", z);
		return vector;
	}
}
