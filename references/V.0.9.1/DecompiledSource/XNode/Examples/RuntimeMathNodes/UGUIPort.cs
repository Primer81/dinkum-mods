using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XNode.Examples.RuntimeMathNodes;

public class UGUIPort : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
	public string fieldName;

	[HideInInspector]
	public Node node;

	private NodePort port;

	private Connection tempConnection;

	private NodePort startPort;

	private UGUIPort tempHovered;

	private RuntimeMathGraph graph;

	private Vector2 startPos;

	private List<Connection> connections = new List<Connection>();

	private void Start()
	{
		port = node.GetPort(fieldName);
		graph = GetComponentInParent<RuntimeMathGraph>();
		if (port.IsOutput && port.IsConnected)
		{
			for (int i = 0; i < port.ConnectionCount; i++)
			{
				AddConnection();
			}
		}
	}

	private void Reset()
	{
		fieldName = base.name;
	}

	private void OnDestroy()
	{
		for (int num = connections.Count - 1; num >= 0; num--)
		{
			Object.Destroy(connections[num].gameObject);
		}
		connections.Clear();
	}

	public void UpdateConnectionTransforms()
	{
		if (port.IsInput)
		{
			return;
		}
		while (connections.Count < port.ConnectionCount)
		{
			AddConnection();
		}
		while (connections.Count > port.ConnectionCount)
		{
			Object.Destroy(connections[0].gameObject);
			connections.RemoveAt(0);
		}
		for (int i = 0; i < port.ConnectionCount; i++)
		{
			NodePort connection = port.GetConnection(i);
			UGUIMathBaseNode runtimeNode = graph.GetRuntimeNode(connection.node);
			if (!runtimeNode)
			{
				Debug.LogWarning(connection.node.name + " node not found", this);
			}
			Transform transform = runtimeNode.GetPort(connection.fieldName).transform;
			if (!transform)
			{
				Debug.LogWarning(connection.fieldName + " not found", this);
			}
			connections[i].SetPosition(base.transform.position, transform.position);
		}
	}

	private void AddConnection()
	{
		Connection connection = Object.Instantiate(graph.runtimeConnectionPrefab);
		connection.transform.SetParent(graph.scrollRect.content);
		connections.Add(connection);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (port.IsOutput)
		{
			tempConnection = Object.Instantiate(graph.runtimeConnectionPrefab);
			tempConnection.transform.SetParent(graph.scrollRect.content);
			tempConnection.SetPosition(base.transform.position, eventData.position);
			startPos = base.transform.position;
			startPort = port;
		}
		else if (port.IsConnected)
		{
			NodePort connection = port.Connection;
			Debug.Log("has " + port.ConnectionCount + " connections");
			Debug.Log(port.GetConnection(0));
			UGUIPort uGUIPort = graph.GetRuntimeNode(connection.node).GetPort(connection.fieldName);
			Debug.Log("Disconnect");
			connection.Disconnect(port);
			tempConnection = Object.Instantiate(graph.runtimeConnectionPrefab);
			tempConnection.transform.SetParent(graph.scrollRect.content);
			tempConnection.SetPosition(uGUIPort.transform.position, eventData.position);
			startPos = uGUIPort.transform.position;
			startPort = uGUIPort.port;
			graph.GetRuntimeNode(node).UpdateGUI();
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (!(tempConnection == null))
		{
			UGUIPort uGUIPort = FindPortInStack(eventData.hovered);
			tempHovered = uGUIPort;
			tempConnection.SetPosition(startPos, eventData.position);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (!(tempConnection == null))
		{
			if ((bool)tempHovered)
			{
				startPort.Connect(tempHovered.port);
				graph.GetRuntimeNode(tempHovered.node).UpdateGUI();
			}
			Object.Destroy(tempConnection.gameObject);
		}
	}

	public UGUIPort FindPortInStack(List<GameObject> stack)
	{
		for (int i = 0; i < stack.Count; i++)
		{
			UGUIPort component = stack[i].GetComponent<UGUIPort>();
			if ((bool)component)
			{
				return component;
			}
		}
		return null;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		graph.tooltip.Show();
		object inputValue = node.GetInputValue<object>(port.fieldName);
		if (inputValue != null)
		{
			graph.tooltip.label.text = inputValue.ToString();
		}
		else
		{
			graph.tooltip.label.text = "n/a";
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		graph.tooltip.Hide();
	}
}
