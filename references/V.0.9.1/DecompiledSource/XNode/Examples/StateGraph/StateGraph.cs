using UnityEngine;

namespace XNode.Examples.StateGraph;

[CreateAssetMenu(fileName = "New State Graph", menuName = "xNode Examples/State Graph")]
public class StateGraph : NodeGraph
{
	public StateNode current;

	public void Continue()
	{
		current.MoveNext();
	}
}
