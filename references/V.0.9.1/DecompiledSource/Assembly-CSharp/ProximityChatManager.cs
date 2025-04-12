using System.Runtime.InteropServices;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ProximityChatManager : NetworkBehaviour
{
	public static ProximityChatManager manage;

	[SyncVar(hook = "onVoiceServerChange")]
	public bool voiceChatOnForSever = true;

	public GameObject voiceChatWindow;

	public WindowAnimator voiceChatWindowAnimator;

	public WindowAnimator voiceComingThroughWindowAnimator;

	public Image voiceImage;

	public Sprite voiceOnSprite;

	public Sprite voiceOffSprite;

	public float voiceVolumeMaster = 1f;

	private bool localVoiceOn;

	public UnityEvent volumeChangeEvent = new UnityEvent();

	[Header("Button Stuff")]
	public Image buttonImage;

	public Sprite controllerButtonIcon;

	public TextMeshProUGUI buttonOverrideText;

	public bool NetworkvoiceChatOnForSever
	{
		get
		{
			return voiceChatOnForSever;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref voiceChatOnForSever))
			{
				bool oldVoiceOn = voiceChatOnForSever;
				SetSyncVar(value, ref voiceChatOnForSever, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					onVoiceServerChange(oldVoiceOn, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	private void Awake()
	{
		manage = this;
	}

	public override void OnStartServer()
	{
		if (CustomNetworkManager.manage.checkIfLanGame() || NetworkPlayersManager.manage.IsPlayingSinglePlayer)
		{
			NetworkvoiceChatOnForSever = false;
		}
		else
		{
			NetworkvoiceChatOnForSever = OptionsMenu.options.hostAllowVoiceChat;
		}
	}

	public override void OnStartClient()
	{
		voiceVolumeMaster = OptionsMenu.options.savedVoiceChatVolume;
		checkVoiceButtons();
		onVoiceServerChange(voiceChatOnForSever, voiceChatOnForSever);
	}

	public void onVoiceServerChange(bool oldVoiceOn, bool newVoiceOn)
	{
		NetworkvoiceChatOnForSever = newVoiceOn;
		voiceChatWindow.SetActive(voiceChatOnForSever);
	}

	public bool isVoiceOnLocal()
	{
		return localVoiceOn;
	}

	public void voiceToggleLocal()
	{
		localVoiceOn = !localVoiceOn;
		voiceChatWindowAnimator.refreshAnimation();
		if (localVoiceOn)
		{
			voiceImage.sprite = voiceOnSprite;
		}
		else
		{
			voiceImage.sprite = voiceOffSprite;
		}
	}

	public void moveTalkingWindowAnimator()
	{
		voiceComingThroughWindowAnimator.refreshAnimation();
	}

	public void checkVoiceButtons()
	{
		if (Inventory.Instance.usingMouse)
		{
			if (ButtonIcons.icons.isOverridden(Input_Rebind.RebindType.SwapCameraMode))
			{
				buttonImage.sprite = ButtonIcons.icons.isAMouseButton(Input_Rebind.rebind.getKeyBindingForInGame(Input_Rebind.RebindType.VoiceChat));
				if (buttonImage.sprite != null)
				{
					buttonOverrideText.gameObject.SetActive(value: false);
					buttonImage.enabled = true;
					return;
				}
				buttonOverrideText.text = Input_Rebind.rebind.getKeyBindingForInGame(Input_Rebind.RebindType.VoiceChat);
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
				Sprite spriteForType = ButtonIcons.icons.getSpriteForType(Input_Rebind.RebindType.VoiceChat);
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

	private void MirrorProcessed()
	{
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(voiceChatOnForSever);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(voiceChatOnForSever);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			bool flag = voiceChatOnForSever;
			NetworkvoiceChatOnForSever = reader.ReadBool();
			if (!SyncVarEqual(flag, ref voiceChatOnForSever))
			{
				onVoiceServerChange(flag, voiceChatOnForSever);
			}
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			bool flag2 = voiceChatOnForSever;
			NetworkvoiceChatOnForSever = reader.ReadBool();
			if (!SyncVarEqual(flag2, ref voiceChatOnForSever))
			{
				onVoiceServerChange(flag2, voiceChatOnForSever);
			}
		}
	}
}
