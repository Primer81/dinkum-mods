using TMPro;
using UnityEngine;

public class RemapButton : MonoBehaviour
{
	public TextMeshProUGUI currentKey;

	public TextMeshProUGUI altKey;

	public Input_Rebind.RebindType myType;

	public int myTypeId;

	public int myAltTypeId;

	private WindowAnimator textBounce;

	private WindowAnimator altTextBounce;

	private void Start()
	{
		textBounce = currentKey.gameObject.AddComponent<WindowAnimator>();
		altTextBounce = altKey.gameObject.AddComponent<WindowAnimator>();
		textBounce.playOpenAndCloseSound = false;
		altTextBounce.playOpenAndCloseSound = false;
	}

	public void startRebind()
	{
		Input_Rebind.rebind.startRebind(myType, myTypeId);
	}

	public void startRebindAlt()
	{
		Input_Rebind.rebind.startRebind(myType, myAltTypeId, alt: true);
	}

	public void resetRebind()
	{
		Input_Rebind.rebind.resetToDefault(myType, myTypeId, myAltTypeId);
	}

	public void checkListening(Input_Rebind.RebindType checkType, bool alt = false)
	{
		if (myType == checkType)
		{
			if (alt)
			{
				altKey.text = " . . . ";
				altTextBounce.refreshAnimation();
			}
			else
			{
				currentKey.text = " . . . ";
				altTextBounce.refreshAnimation();
			}
		}
	}

	public void finishListening(string newText, string altText = "")
	{
		if (currentKey.text != newText && (bool)textBounce)
		{
			textBounce.refreshAnimation();
		}
		if ((bool)altKey && altText != "" && altKey.text != altText && (bool)altTextBounce)
		{
			altTextBounce.refreshAnimation();
		}
		currentKey.text = newText;
		altKey.text = altText;
	}
}
