using System.Linq;
using UnityEngine;

namespace XNode.Examples.LogicToy;

[NodeWidth(140)]
[NodeTint(70, 70, 100)]
public class ToggleNode : LogicNode
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
		bool flag = GetPort("input").GetInputValues<bool>().Any((bool x) => x);
		if (!input && flag)
		{
			input = flag;
			output = !output;
			SendSignal(GetPort("output"));
		}
		else if (input && !flag)
		{
			input = flag;
		}
	}

	public override object GetValue(NodePort port)
	{
		return output;
	}
}
