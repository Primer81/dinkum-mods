using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using Steamworks;
using UnityEngine;

public class ProximityChat : NetworkBehaviour
{
	public EyesScript myEyes;

	public AudioSource audioSource;

	private bool localSpeaking;

	private Animator myAnim;

	public AudioEchoFilter megaphoneEffect;

	public InventoryItem megaphoneItem;

	private EquipItemToChar myEquip;

	private WaitForSeconds mouthFlapWait = new WaitForSeconds(0.45f);

	private void Start()
	{
		if (NetworkPlayersManager.manage.IsPlayingSinglePlayer || !SteamManager.Initialized || CustomNetworkManager.manage.checkIfLanGame())
		{
			base.enabled = false;
			return;
		}
		onChangeMasterVoiceVolume();
		myAnim = GetComponent<Animator>();
		StartCoroutine(charMovesMouthOnTalk());
		myEquip = GetComponent<EquipItemToChar>();
	}

	private void OnEnable()
	{
		ProximityChatManager.manage.volumeChangeEvent.AddListener(onChangeMasterVoiceVolume);
	}

	private void OnDisable()
	{
		ProximityChatManager.manage.volumeChangeEvent.RemoveListener(onChangeMasterVoiceVolume);
	}

	private void onChangeMasterVoiceVolume()
	{
		audioSource.volume = ProximityChatManager.manage.voiceVolumeMaster;
	}

	private void Update()
	{
		if (base.isLocalPlayer)
		{
			if (!ProximityChatManager.manage.voiceChatOnForSever)
			{
				SteamUser.StopVoiceRecording();
				return;
			}
			if (Inventory.Instance.CanMoveCharacter() && OptionsMenu.options.voiceChatButtonIsToggle && InputMaster.input.VoiceChatToggle())
			{
				ProximityChatManager.manage.voiceToggleLocal();
			}
			if (!OptionsMenu.options.voiceChatButtonIsToggle)
			{
				if (Inventory.Instance.CanMoveCharacter() && InputMaster.input.VoiceChatHeld())
				{
					if (!ProximityChatManager.manage.isVoiceOnLocal())
					{
						ProximityChatManager.manage.voiceToggleLocal();
					}
				}
				else if (ProximityChatManager.manage.isVoiceOnLocal())
				{
					ProximityChatManager.manage.voiceToggleLocal();
				}
			}
			if (ProximityChatManager.manage.isVoiceOnLocal())
			{
				SteamUser.StartVoiceRecording();
				if (SteamUser.GetAvailableVoice(out var pcbCompressed) == EVoiceResult.k_EVoiceResultOK && pcbCompressed > 2048)
				{
					byte[] array = new byte[2048];
					if (SteamUser.GetVoice(bWantCompressed: true, array, 2048u, out var nBytesWritten) == EVoiceResult.k_EVoiceResultOK && nBytesWritten != 0)
					{
						localSpeaking = true;
						Cmd_SendData(array, nBytesWritten);
					}
				}
			}
			else
			{
				SteamUser.StopVoiceRecording();
			}
		}
		else if (ProximityChatManager.manage.voiceChatOnForSever)
		{
			if (myEquip.usingItem && myEquip.currentlyHoldingItemId == megaphoneItem.getItemId())
			{
				megaphoneEffect.enabled = true;
				audioSource.maxDistance = 195f;
				audioSource.minDistance = 35f;
			}
			else
			{
				megaphoneEffect.enabled = false;
				audioSource.maxDistance = 52f;
				audioSource.minDistance = 12f;
			}
		}
	}

	[Command(channel = 2)]
	private void Cmd_SendData(byte[] data, uint size)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBytesAndSize(data);
		writer.WriteUInt(size);
		SendCommandInternal(typeof(ProximityChat), "Cmd_SendData", writer, 2);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc(channel = 2)]
	private void Target_PlaySound(NetworkConnection connection, byte[] DestBuffer, uint BytesWritten)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBytesAndSize(DestBuffer);
		writer.WriteUInt(BytesWritten);
		SendTargetRPCInternal(connection, typeof(ProximityChat), "Target_PlaySound", writer, 2);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator charMovesMouthOnTalk()
	{
		bool playingTalkingAnimation = false;
		while (true)
		{
			yield return mouthFlapWait;
			if (!base.isLocalPlayer)
			{
				if (!isSilent())
				{
					myEyes.sayWord();
					myEyes.speakingWordsPart();
					if (!playingTalkingAnimation)
					{
						playingTalkingAnimation = true;
						setTalkingAnimation(shouldPlayTalkingAnimation: true);
					}
				}
				else if (playingTalkingAnimation)
				{
					playingTalkingAnimation = false;
					setTalkingAnimation(shouldPlayTalkingAnimation: false);
				}
				continue;
			}
			if (localSpeaking)
			{
				myEyes.sayWord();
				myEyes.speakingWordsPart();
				ProximityChatManager.manage.moveTalkingWindowAnimator();
				if (!playingTalkingAnimation)
				{
					playingTalkingAnimation = true;
					setTalkingAnimation(shouldPlayTalkingAnimation: true);
				}
			}
			else if (playingTalkingAnimation)
			{
				playingTalkingAnimation = false;
				setTalkingAnimation(shouldPlayTalkingAnimation: false);
			}
			localSpeaking = false;
		}
	}

	public void setTalkingAnimation(bool shouldPlayTalkingAnimation)
	{
		myAnim.SetBool("Talking", shouldPlayTalkingAnimation);
	}

	private bool isSilent()
	{
		if (audioSource.isPlaying)
		{
			float[] array = new float[audioSource.clip.samples * audioSource.clip.channels];
			audioSource.clip.GetData(array, 0);
			float num = 0.01f;
			for (int i = 0; i < array.Length; i++)
			{
				if (Mathf.Abs(array[i]) > num)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_Cmd_SendData(byte[] data, uint size)
	{
		if (!ProximityChatManager.manage.voiceChatOnForSever)
		{
			return;
		}
		int num = 55;
		if (myEquip.usingItem && myEquip.currentlyHoldingItemId == megaphoneItem.getItemId())
		{
			num = 200;
		}
		for (int i = 0; i < NetworkNavMesh.nav.charsConnected.Count; i++)
		{
			if (NetworkNavMesh.nav.charsConnected[i] != base.transform && Vector3.Distance(NetworkNavMesh.nav.charsConnected[i].position, base.transform.position) <= (float)num)
			{
				Target_PlaySound(NetworkNavMesh.nav.charNetConn[i].connectionToClient, data, size);
			}
		}
	}

	protected static void InvokeUserCode_Cmd_SendData(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command Cmd_SendData called on client.");
		}
		else
		{
			((ProximityChat)obj).UserCode_Cmd_SendData(reader.ReadBytesAndSize(), reader.ReadUInt());
		}
	}

	protected void UserCode_Target_PlaySound(NetworkConnection connection, byte[] DestBuffer, uint BytesWritten)
	{
		byte[] array = new byte[44100];
		if (SteamUser.DecompressVoice(DestBuffer, BytesWritten, array, (uint)array.Length, out var nBytesWritten, 22050u) != 0 || nBytesWritten == 0)
		{
			return;
		}
		int num = (int)nBytesWritten / 2;
		audioSource.clip = AudioClip.Create(Random.Range(100, 1000000).ToString(), num, 1, 22050, stream: false);
		float[] array2 = new float[num];
		for (int i = 0; i < num; i++)
		{
			if (OptionsMenu.options.savedVoiceChatVolume > 1f)
			{
				array2[i] = OptionsMenu.options.savedVoiceChatVolume * (float)(short)(array[i * 2] | (array[i * 2 + 1] << 8)) / 32768f;
			}
			else
			{
				array2[i] = (float)(short)(array[i * 2] | (array[i * 2 + 1] << 8)) / 32768f;
			}
		}
		audioSource.clip.SetData(array2, 0);
		audioSource.Play();
	}

	protected static void InvokeUserCode_Target_PlaySound(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC Target_PlaySound called on server.");
		}
		else
		{
			((ProximityChat)obj).UserCode_Target_PlaySound(NetworkClient.readyConnection, reader.ReadBytesAndSize(), reader.ReadUInt());
		}
	}

	static ProximityChat()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(ProximityChat), "Cmd_SendData", InvokeUserCode_Cmd_SendData, requiresAuthority: true);
		RemoteCallHelper.RegisterRpcDelegate(typeof(ProximityChat), "Target_PlaySound", InvokeUserCode_Target_PlaySound);
	}
}
