using System;

namespace XNode.Examples.LogicToy;

public abstract class LogicNode : Node
{
	public Action onStateChange;

	public abstract bool led { get; }

	public void SendSignal(NodePort output)
	{
		int connectionCount = output.ConnectionCount;
		for (int i = 0; i < connectionCount; i++)
		{
			LogicNode logicNode = output.GetConnection(i).node as LogicNode;
			if (logicNode != null)
			{
				logicNode.OnInputChanged();
			}
		}
		if (onStateChange != null)
		{
			onStateChange();
		}
	}

	protected abstract void OnInputChanged();

	public override void OnCreateConnection(NodePort from, NodePort to)
	{
		OnInputChanged();
	}
}
