using UnityEngine;

public class ResetInputCaretPos : MonoBehaviour
{
	public RectTransform textRectTransform;

	private RectTransform caretRectTransform;

	private void OnEnable()
	{
		if (caretRectTransform == null)
		{
			Transform transform = textRectTransform.parent.Find("Caret");
			if ((bool)transform)
			{
				caretRectTransform = transform.GetComponent<RectTransform>();
			}
		}
		else
		{
			ResetLocalPositionOfRects(caretRectTransform);
		}
		ResetLocalPositionOfRects(textRectTransform);
	}

	private void ResetLocalPositionOfRects(RectTransform toReset)
	{
		toReset.offsetMax = new Vector2(0f, 0f);
		toReset.offsetMin = new Vector2(0f, 0f);
	}
}
