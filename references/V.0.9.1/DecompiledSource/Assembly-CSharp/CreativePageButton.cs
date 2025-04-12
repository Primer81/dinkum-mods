using UnityEngine;
using UnityEngine.UI;

public class CreativePageButton : MonoBehaviour
{
	public int pageId;

	public Image dotImage;

	public InvButton myButton;

	public GameObject selectedArrow;

	private bool isSelected;

	public Color selectedColor;

	public Color notSelectedColor;

	public void PressButton()
	{
		CreativeManager.instance.SkipToShowTo(pageId);
	}

	private void Update()
	{
		if (myButton.hovering && InputMaster.input.UISelectHeld())
		{
			myButton.onButtonPress.Invoke();
		}
	}

	public void SetSelected(bool isSelectedNow)
	{
		if (isSelected != isSelectedNow)
		{
			selectedArrow.SetActive(isSelectedNow);
			if (isSelectedNow)
			{
				dotImage.color = selectedColor;
			}
			else
			{
				dotImage.color = notSelectedColor;
			}
		}
		isSelected = isSelectedNow;
	}

	public void PressTimeButton()
	{
		RealWorldTimeLight.time.NetworkcurrentHour = pageId;
	}
}
