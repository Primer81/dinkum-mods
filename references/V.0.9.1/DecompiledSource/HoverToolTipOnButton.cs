using UnityEngine;

public class HoverToolTipOnButton : MonoBehaviour
{
	private InvButton myInvButton;

	public string hoveringText;

	public string hoveringDesc;

	public bool useHoverColour;

	public Color hoverColour;

	private void Start()
	{
		myInvButton = GetComponent<InvButton>();
	}

	private void Update()
	{
		if (myInvButton.hovering && CursorImageChange.change.buttonHovering != this)
		{
			CursorImageChange.change.setNewHovering(this);
		}
		if (!myInvButton.hovering && CursorImageChange.change.buttonHovering == this)
		{
			CursorImageChange.change.setNewHovering(null);
		}
	}

	private void OnDisable()
	{
		if (CursorImageChange.change.buttonHovering == this)
		{
			CursorImageChange.change.setNewHovering(null);
		}
	}

	public string GetHoverText()
	{
		if (hoveringText.Contains("Tip_"))
		{
			return ConversationGenerator.generate.GetToolTip(hoveringText);
		}
		return hoveringText;
	}
}
