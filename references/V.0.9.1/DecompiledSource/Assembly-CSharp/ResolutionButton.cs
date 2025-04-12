using TMPro;
using UnityEngine;

public class ResolutionButton : MonoBehaviour
{
	public TextMeshProUGUI resolutionLabel;

	private Resolution myRes;

	public GameObject selectedIcon1;

	public GameObject selectedIcon2;

	public void updateButton(Resolution newRes)
	{
		myRes = newRes;
		resolutionLabel.text = myRes.width + " x " + myRes.height;
	}

	public void pressButton()
	{
		OptionsMenu.options.ApplyResolution(myRes);
	}

	private void OnEnable()
	{
		if (myRes.width == Screen.currentResolution.width && myRes.height == Screen.currentResolution.height)
		{
			selectedIcon1.SetActive(value: true);
			selectedIcon2.SetActive(value: true);
		}
		else
		{
			selectedIcon1.SetActive(value: false);
			selectedIcon2.SetActive(value: false);
		}
	}
}
