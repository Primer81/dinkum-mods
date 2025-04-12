using UnityEngine;
using UnityEngine.UI;

namespace XNode.Examples.RuntimeMathNodes;

public class UGUITooltip : MonoBehaviour
{
	public CanvasGroup group;

	public Text label;

	private bool show;

	private RuntimeMathGraph graph;

	private void Awake()
	{
		graph = GetComponentInParent<RuntimeMathGraph>();
	}

	private void Start()
	{
		Hide();
	}

	private void Update()
	{
		if (show)
		{
			UpdatePosition();
		}
	}

	public void Show()
	{
		show = true;
		group.alpha = 1f;
		UpdatePosition();
		base.transform.SetAsLastSibling();
	}

	public void Hide()
	{
		show = false;
		group.alpha = 0f;
	}

	private void UpdatePosition()
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(graph.scrollRect.content.transform as RectTransform, cam: graph.gameObject.GetComponentInParent<Canvas>().worldCamera, screenPoint: Input.mousePosition, localPoint: out var localPoint);
		base.transform.localPosition = localPoint;
	}
}
