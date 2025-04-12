using UnityEngine;

namespace XNode.Examples.LogicToy;

[NodeWidth(140)]
[NodeTint(70, 100, 70)]
public class PulseNode : LogicNode, ITimerTick
{
	[Space(-18f)]
	public float interval = 1f;

	[Output(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.None, false)]
	[HideInInspector]
	public bool output;

	private float timer;

	public override bool led => output;

	public void Tick(float deltaTime)
	{
		timer += deltaTime;
		if (!output && timer > interval)
		{
			timer -= interval;
			output = true;
			SendSignal(GetPort("output"));
		}
		else if (output)
		{
			output = false;
			SendSignal(GetPort("output"));
		}
	}

	protected override void OnInputChanged()
	{
	}

	public override object GetValue(NodePort port)
	{
		return output;
	}
}
