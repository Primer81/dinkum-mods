using System;
using UnityEngine;

namespace XNode.Examples.StateGraph;

public class StateNode : Node
{
	[Serializable]
	public class Empty
	{
	}

	[Input(ShowBackingValue.Unconnected, ConnectionType.Multiple, TypeConstraint.None, false)]
	public Empty enter;

	[Output(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.None, false)]
	public Empty exit;

	public void MoveNext()
	{
		if ((graph as StateGraph).current != this)
		{
			Debug.LogWarning("Node isn't active");
			return;
		}
		NodePort outputPort = GetOutputPort("exit");
		if (!outputPort.IsConnected)
		{
			Debug.LogWarning("Node isn't connected");
		}
		else
		{
			(outputPort.Connection.node as StateNode).OnEnter();
		}
	}

	public void OnEnter()
	{
		(graph as StateGraph).current = this;
	}
}
