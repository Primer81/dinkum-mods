using UnityEngine;

namespace XNode.Examples.RuntimeMathNodes;

public class Connection : MonoBehaviour
{
	private RectTransform rectTransform;

	public void SetPosition(Vector2 start, Vector2 end)
	{
		if (!rectTransform)
		{
			rectTransform = (RectTransform)base.transform;
		}
		base.transform.position = (start + end) * 0.5f;
		float z = Mathf.Atan2(start.y - end.y, start.x - end.x) * 57.29578f;
		base.transform.rotation = Quaternion.Euler(0f, 0f, z);
		rectTransform.sizeDelta = new Vector2(Vector2.Distance(start, end), rectTransform.sizeDelta.y);
	}
}
