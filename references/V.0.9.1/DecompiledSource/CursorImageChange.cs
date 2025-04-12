using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CursorImageChange : MonoBehaviour
{
	public static CursorImageChange change;

	public Image cursorImage;

	public Sprite normalCursor;

	public Sprite dragCursor;

	public Sprite startDragCursor;

	public Vector3 dragPos;

	[Header("Special Hover Text")]
	public bool isMainCursor;

	public HoverToolTipOnButton buttonHovering;

	public TextMeshProUGUI specialHoverText;

	public TextMeshProUGUI specialHoverDesc;

	public Image specialHoverImage;

	public GameObject specialHoverBox;

	public Color defaultHoverColour;

	private Coroutine specialHoveringRoutine;

	private void Awake()
	{
		if (isMainCursor)
		{
			change = this;
		}
	}

	private void OnEnable()
	{
		StartCoroutine(checkCursor());
	}

	private IEnumerator checkCursor()
	{
		cursorImage.transform.localPosition = Vector3.zero;
		cursorImage.sprite = normalCursor;
		while (!Inventory.Instance)
		{
			yield return null;
		}
		while (true)
		{
			if (Inventory.Instance.dragSlot.itemNo != -1)
			{
				int inDragSlot = Inventory.Instance.dragSlot.itemNo;
				yield return StartCoroutine(moveCursorToPos(dragPos, startDragCursor, dragCursor));
				while (Inventory.Instance.dragSlot.itemNo == inDragSlot)
				{
					yield return null;
				}
				if (Inventory.Instance.dragSlot.itemNo != -1)
				{
					yield return StartCoroutine(moveCursorToPos(dragPos / 2f, startDragCursor, dragCursor));
				}
				else
				{
					yield return StartCoroutine(moveCursorToPos(Vector3.zero, startDragCursor, normalCursor));
				}
			}
			yield return null;
		}
	}

	private IEnumerator moveCursorToPos(Vector3 desiredPos, Sprite startingSprite, Sprite endingSprite)
	{
		float timer = 0f;
		cursorImage.sprite = startingSprite;
		while (timer < 1f)
		{
			cursorImage.transform.localPosition = Vector3.Lerp(cursorImage.transform.localPosition, desiredPos, timer);
			timer += Time.deltaTime * 10f;
			yield return null;
		}
		cursorImage.sprite = endingSprite;
	}

	public void setNewHovering(HoverToolTipOnButton newHovering)
	{
		if (buttonHovering != newHovering)
		{
			buttonHovering = newHovering;
			specialHoverBox.SetActive(value: false);
			if (specialHoveringRoutine != null)
			{
				StopCoroutine(specialHoveringRoutine);
			}
			if ((bool)newHovering)
			{
				specialHoveringRoutine = StartCoroutine(runSpecialHovering());
			}
		}
	}

	private IEnumerator runSpecialHovering()
	{
		specialHoverBox.transform.localPosition = Inventory.Instance.InvDescription.transform.localPosition + new Vector3(0f, 20f, 0f);
		specialHoverText.text = buttonHovering.GetHoverText();
		if (buttonHovering.hoveringDesc != "")
		{
			specialHoverDesc.text = buttonHovering.hoveringDesc;
			specialHoverDesc.gameObject.SetActive(value: true);
		}
		else
		{
			specialHoverDesc.text = "";
			specialHoverDesc.gameObject.SetActive(value: false);
		}
		if (buttonHovering.useHoverColour)
		{
			specialHoverImage.color = buttonHovering.hoverColour;
		}
		else
		{
			specialHoverImage.color = defaultHoverColour;
		}
		specialHoverBox.SetActive(value: true);
		while ((bool)buttonHovering)
		{
			yield return null;
		}
		specialHoveringRoutine = null;
	}
}
