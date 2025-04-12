using System.Collections.Generic;
using UnityEngine;

public class HintLayoutGroup : MonoBehaviour
{
	public GameObject[] AllHintButtons;

	private List<GameObject> ActiveHintButtons = new List<GameObject>();

	private List<RectTransform> ActiveRects = new List<RectTransform>();

	public RectTransform[] AllPopupSizes;

	public float FixedSpacing = 15f;

	public void Update()
	{
		GetAllActiveHints();
		PositionHintButtons();
	}

	private void GetAllActiveHints()
	{
		ActiveHintButtons.Clear();
		ActiveRects.Clear();
		for (int i = 0; i < AllHintButtons.Length; i++)
		{
			if (AllHintButtons[i].activeSelf)
			{
				ActiveHintButtons.Add(AllHintButtons[i]);
				ActiveRects.Add(AllPopupSizes[i]);
			}
		}
	}

	private void PositionHintButtons()
	{
		_ = Vector3.zero;
		float num = 0f;
		for (int i = 0; i < ActiveHintButtons.Count; i++)
		{
			float width = ActiveRects[i].rect.width;
			num += width;
			ActiveHintButtons[i].transform.localPosition = new Vector3(num, 0f, 0f);
			num += FixedSpacing;
		}
		for (int j = 0; j < ActiveHintButtons.Count; j++)
		{
			ActiveHintButtons[j].transform.localPosition -= new Vector3(num / 2f, 0f, 0f);
		}
	}
}
