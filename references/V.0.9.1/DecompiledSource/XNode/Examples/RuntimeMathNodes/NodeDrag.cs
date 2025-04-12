using UnityEngine;
using UnityEngine.EventSystems;

namespace XNode.Examples.RuntimeMathNodes;

public class NodeDrag : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	private Vector3 offset;

	private UGUIMathBaseNode node;

	private void Awake()
	{
		node = GetComponentInParent<UGUIMathBaseNode>();
	}

	public void OnDrag(PointerEventData eventData)
	{
		node.transform.localPosition = node.graph.scrollRect.content.InverseTransformPoint(eventData.position) - offset;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		Vector2 vector = node.graph.scrollRect.content.InverseTransformPoint(eventData.position);
		Vector2 vector2 = node.transform.localPosition;
		offset = vector - vector2;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		node.transform.localPosition = node.graph.scrollRect.content.InverseTransformPoint(eventData.position) - offset;
		Vector2 position = node.transform.localPosition;
		position.y = 0f - position.y;
		node.node.position = position;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right)
		{
			node.graph.nodeContextMenu.selectedNode = node.node;
			node.graph.nodeContextMenu.OpenAt(eventData.position);
		}
	}
}
