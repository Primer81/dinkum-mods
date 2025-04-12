using System;
using UnityEngine;
using UnityEngine.EventSystems;
using XNode.Examples.MathNodes;

namespace XNode.Examples.RuntimeMathNodes;

public class UGUIContextMenu : MonoBehaviour, IPointerExitHandler, IEventSystemHandler
{
	public Action<Type, Vector2> onClickSpawn;

	public CanvasGroup group;

	[HideInInspector]
	public Node selectedNode;

	private Vector2 pos;

	private void Start()
	{
		Close();
	}

	public void OpenAt(Vector2 pos)
	{
		base.transform.position = pos;
		group.alpha = 1f;
		group.interactable = true;
		group.blocksRaycasts = true;
		base.transform.SetAsLastSibling();
	}

	public void Close()
	{
		group.alpha = 0f;
		group.interactable = false;
		group.blocksRaycasts = false;
	}

	public void SpawnMathNode()
	{
		SpawnNode(typeof(MathNode));
	}

	public void SpawnDisplayNode()
	{
		SpawnNode(typeof(DisplayValue));
	}

	public void SpawnVectorNode()
	{
		SpawnNode(typeof(Vector));
	}

	private void SpawnNode(Type nodeType)
	{
		Vector2 arg = new Vector2(base.transform.localPosition.x, 0f - base.transform.localPosition.y);
		onClickSpawn(nodeType, arg);
	}

	public void RemoveNode()
	{
		RuntimeMathGraph componentInParent = GetComponentInParent<RuntimeMathGraph>();
		componentInParent.graph.RemoveNode(selectedNode);
		componentInParent.Refresh();
		Close();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Close();
	}
}
