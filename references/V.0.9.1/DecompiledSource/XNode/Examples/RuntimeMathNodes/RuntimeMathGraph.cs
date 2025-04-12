using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Examples.RuntimeMathNodes;

public class RuntimeMathGraph : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[Header("Graph")]
	public MathGraph graph;

	[Header("Prefabs")]
	public UGUIMathNode runtimeMathNodePrefab;

	public UGUIVector runtimeVectorPrefab;

	public UGUIDisplayValue runtimeDisplayValuePrefab;

	public Connection runtimeConnectionPrefab;

	[Header("References")]
	public UGUIContextMenu graphContextMenu;

	public UGUIContextMenu nodeContextMenu;

	public UGUITooltip tooltip;

	private List<UGUIMathBaseNode> nodes;

	public ScrollRect scrollRect { get; private set; }

	private void Awake()
	{
		graph = graph.Copy() as MathGraph;
		scrollRect = GetComponentInChildren<ScrollRect>();
		UGUIContextMenu uGUIContextMenu = graphContextMenu;
		uGUIContextMenu.onClickSpawn = (Action<Type, Vector2>)Delegate.Remove(uGUIContextMenu.onClickSpawn, new Action<Type, Vector2>(SpawnNode));
		UGUIContextMenu uGUIContextMenu2 = graphContextMenu;
		uGUIContextMenu2.onClickSpawn = (Action<Type, Vector2>)Delegate.Combine(uGUIContextMenu2.onClickSpawn, new Action<Type, Vector2>(SpawnNode));
	}

	private void Start()
	{
		SpawnGraph();
	}

	public void Refresh()
	{
		Clear();
		SpawnGraph();
	}

	public void Clear()
	{
		for (int num = nodes.Count - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(nodes[num].gameObject);
		}
		nodes.Clear();
	}

	public void SpawnGraph()
	{
		if (nodes != null)
		{
			nodes.Clear();
		}
		else
		{
			nodes = new List<UGUIMathBaseNode>();
		}
		for (int i = 0; i < graph.nodes.Count; i++)
		{
			Node node = graph.nodes[i];
			UGUIMathBaseNode uGUIMathBaseNode = null;
			if (node is MathNode)
			{
				uGUIMathBaseNode = UnityEngine.Object.Instantiate(runtimeMathNodePrefab);
			}
			else if (node is Vector)
			{
				uGUIMathBaseNode = UnityEngine.Object.Instantiate(runtimeVectorPrefab);
			}
			else if (node is DisplayValue)
			{
				uGUIMathBaseNode = UnityEngine.Object.Instantiate(runtimeDisplayValuePrefab);
			}
			uGUIMathBaseNode.transform.SetParent(scrollRect.content);
			uGUIMathBaseNode.node = node;
			uGUIMathBaseNode.graph = this;
			nodes.Add(uGUIMathBaseNode);
		}
	}

	public UGUIMathBaseNode GetRuntimeNode(Node node)
	{
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i].node == node)
			{
				return nodes[i];
			}
		}
		return null;
	}

	public void SpawnNode(Type type, Vector2 position)
	{
		Node node = graph.AddNode(type);
		node.name = type.Name;
		node.position = position;
		Refresh();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right)
		{
			graphContextMenu.OpenAt(eventData.position);
		}
	}
}
