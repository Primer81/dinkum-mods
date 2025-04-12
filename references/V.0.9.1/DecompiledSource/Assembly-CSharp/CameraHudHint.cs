using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraHudHint : MonoBehaviour
{
	public Sprite normalCamera;

	public Sprite topDownCamera;

	public Sprite arrowsAllDir;

	public Sprite arrowsInAndOut;

	public Image cameraImage;

	public Image arrowImage;

	public Image buttonImage;

	public TextMeshProUGUI buttonOverrideText;

	public Sprite controllerButtonIcon;

	public WindowAnimator myAnim;

	public void updateIcon(bool freeCamOn)
	{
		myAnim.refreshAnimation();
		if (freeCamOn)
		{
			cameraImage.sprite = normalCamera;
			arrowImage.sprite = arrowsAllDir;
		}
		else
		{
			cameraImage.sprite = topDownCamera;
			arrowImage.sprite = arrowsInAndOut;
		}
		if (Inventory.Instance.usingMouse)
		{
			if (ButtonIcons.icons.isOverridden(Input_Rebind.RebindType.SwapCameraMode))
			{
				buttonImage.sprite = ButtonIcons.icons.isAMouseButton(Input_Rebind.rebind.getKeyBindingForInGame(Input_Rebind.RebindType.SwapCameraMode));
				if (buttonImage.sprite != null)
				{
					buttonOverrideText.gameObject.SetActive(value: false);
					buttonImage.enabled = true;
					return;
				}
				buttonOverrideText.text = Input_Rebind.rebind.getKeyBindingForInGame(Input_Rebind.RebindType.SwapCameraMode);
				if (buttonOverrideText.text.Length >= 3)
				{
					buttonOverrideText.text = buttonOverrideText.text.Replace("Left", "");
					buttonImage.sprite = ButtonIcons.icons.genericLong.keyBoardIcon;
				}
				else
				{
					buttonImage.sprite = ButtonIcons.icons.genericSmall.keyBoardIcon;
				}
				buttonImage.enabled = true;
				buttonOverrideText.gameObject.SetActive(value: true);
			}
			else
			{
				Sprite spriteForType = ButtonIcons.icons.getSpriteForType(Input_Rebind.RebindType.SwapCameraMode);
				if (spriteForType == null)
				{
					buttonImage.enabled = false;
				}
				else
				{
					buttonImage.enabled = true;
				}
				buttonOverrideText.gameObject.SetActive(value: false);
				buttonImage.sprite = spriteForType;
			}
		}
		else
		{
			buttonOverrideText.gameObject.SetActive(value: false);
			buttonImage.sprite = controllerButtonIcon;
		}
	}
}
