using UnityEngine;
using UnityEngine.UI;

public class ButtonIcons : MonoBehaviour
{
	public static ButtonIcons icons;

	public ButtonIconSet interactIcon;

	public ButtonIconSet useIcon;

	public ButtonIconSet otherIcon;

	public ButtonIconSet swapCameraIcon;

	public ButtonIconSet openJournalIcon;

	public ButtonIconSet mapIcon;

	public ButtonIconSet voiceChatIcon;

	public ButtonIconSet genericSmall;

	public ButtonIconSet genericLong;

	public Sprite mouseLeft;

	public Sprite mouseRight;

	public Sprite mouseMiddle;

	public Image whistleButtonImage;

	public Sprite whistleController;

	public Sprite whistleKeyboard;

	private void Awake()
	{
		icons = this;
	}

	public bool isOverridden(Input_Rebind.RebindType type)
	{
		return Input_Rebind.rebind.checkIfIsOverridden(type);
	}

	public Sprite getSpriteForType(Input_Rebind.RebindType type)
	{
		return type switch
		{
			Input_Rebind.RebindType.Interact => interactIcon.keyBoardIcon, 
			Input_Rebind.RebindType.Use => useIcon.keyBoardIcon, 
			Input_Rebind.RebindType.Other => otherIcon.keyBoardIcon, 
			Input_Rebind.RebindType.SwapCameraMode => swapCameraIcon.keyBoardIcon, 
			_ => type switch
			{
				Input_Rebind.RebindType.SwapCameraMode => swapCameraIcon.keyBoardIcon, 
				Input_Rebind.RebindType.OpenMap => mapIcon.keyBoardIcon, 
				Input_Rebind.RebindType.VoiceChat => voiceChatIcon.keyBoardIcon, 
				_ => null, 
			}, 
		};
	}

	public Sprite isAMouseButton(string overrideName)
	{
		if (overrideName.Contains("ouse"))
		{
			if (overrideName.Contains("RMB"))
			{
				return mouseRight;
			}
			if (overrideName.Contains("LMB"))
			{
				return mouseLeft;
			}
			if (overrideName.Contains("MMB"))
			{
				return mouseMiddle;
			}
		}
		return null;
	}
}
