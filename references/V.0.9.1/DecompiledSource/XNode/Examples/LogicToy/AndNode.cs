using System.Linq;
using UnityEngine;

namespace XNode.Examples.LogicToy;

[NodeWidth(140)]
[NodeTint(100, 70, 70)]
public class AndNode : LogicNode
{
	[Input(ShowBackingValue.Unconnected, ConnectionType.Multiple, TypeConstraint.None, false)]
	[HideInInspector]
	public bool input;

	[Output(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.None, false)]
	[HideInInspector]
	public bool output;

	public override bool led => output;

	protected override void OnInputChanged()
	{
		bool flag = GetPort("input").GetInputValues<bool>().All((bool x) => x);
		if (input != flag)
		{
			input = flag;
			output = flag;
			SendSignal(GetPort("output"));
		}
	}

	public override object GetValue(NodePort port)
	{
		return output;
	}
}
