using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XNode.Examples.RuntimeMathNodes;

public class UGUIMathBaseNode : MonoBehaviour, IDragHandler, IEventSystemHandler
{
	[HideInInspector]
	public Node node;

	[HideInInspector]
	public RuntimeMathGraph graph;

	public Text header;

	private UGUIPort[] ports;

	public virtual void Start()
	{
		ports = GetComponentsInChildren<UGUIPort>();
		UGUIPort[] array = ports;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].node = node;
		}
		header.text = node.name;
		SetPosition(node.position);
	}

	public virtual void UpdateGUI()
	{
	}

	private void LateUpdate()
	{
		UGUIPort[] array = ports;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateConnectionTransforms();
		}
	}

	public UGUIPort GetPort(string name)
	{
		for (int i = 0; i < ports.Length; i++)
		{
			if (ports[i].name == name)
			{
				return ports[i];
			}
		}
		return null;
	}

	public void SetPosition(Vector2 pos)
	{
		pos.y = 0f - pos.y;
		base.transform.localPosition = pos;
	}

	public void SetName(string name)
	{
		header.text = name;
	}

	public void OnDrag(PointerEventData eventData)
	{
	}
}
