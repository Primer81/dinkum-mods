using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

public class NetworkPlayersManager : MonoBehaviour
{
	public static NetworkPlayersManager manage;

	public MultiPlayerButton[] playerButttons;

	public List<CharMovement> connectedChars = new List<CharMovement>();

	public GameObject multiplayerWindow;

	public GameObject LANIPField;

	public TextMeshProUGUI localIp;

	public GameObject singlePlayerOptions;

	public GameObject gamePausedScreen;

	private bool isGamePaused;

	public bool IsPlayingSinglePlayer { get; set; }

	private void Awake()
	{
		manage = this;
	}

	private void Start()
	{
		refreshButtons();
	}

	public void addPlayer(CharMovement newChar)
	{
		if (!connectedChars.Contains(newChar) && !newChar.isLocalPlayer)
		{
			connectedChars.Add(newChar);
		}
		StartCoroutine(waitForNameUpdateAndRefreshName(newChar));
	}

	private IEnumerator waitForNameUpdateAndRefreshName(CharMovement newChar)
	{
		while (!newChar.myEquip.nameHasBeenUpdated)
		{
			yield return null;
		}
		refreshButtons();
	}

	public void removePlayer(CharMovement newChar)
	{
		if (connectedChars.Contains(newChar))
		{
			connectedChars.Remove(newChar);
		}
		refreshButtons();
	}

	public void refreshButtons()
	{
		for (int i = 0; i < playerButttons.Length; i++)
		{
			if (i < connectedChars.Count)
			{
				playerButttons[i].FillSlot(connectedChars[i].myEquip.playerName, i);
			}
			else
			{
				playerButttons[i].EmptySlot();
			}
		}
		if (CustomNetworkManager.manage.checkIfLanGame())
		{
			localIp.text = GetLocalIPAddress();
			if (localIp.text != "")
			{
				LANIPField.SetActive(value: true);
			}
		}
		else
		{
			_ = connectedChars.Count;
			_ = 3;
		}
	}

	public void KickPlayer(int id)
	{
		connectedChars[id].TargetKick(connectedChars[id].connectionToClient);
	}

	public void openMultiplayerOptions()
	{
		multiplayerWindow.SetActive(value: true);
	}

	public void openSinglePlayerOptions()
	{
		singlePlayerOptions.SetActive(value: true);
	}

	public static string GetLocalIPAddress()
	{
		IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
		foreach (IPAddress iPAddress in addressList)
		{
			if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
			{
				return iPAddress.ToString();
			}
		}
		return "";
	}

	public void pauseButton()
	{
		if (!isGamePaused)
		{
			Time.timeScale = 0f;
			isGamePaused = true;
			StartCoroutine(pauseScreen());
		}
		else
		{
			Time.timeScale = 1f;
			isGamePaused = false;
		}
	}

	public IEnumerator pauseScreen()
	{
		gamePausedScreen.SetActive(value: true);
		NPCManager.manage.refreshAllAnimators(on: false);
		while (isGamePaused)
		{
			yield return null;
			if (InputMaster.input.UISelectActiveConfirmButton() || InputMaster.input.UICancel())
			{
				pauseButton();
			}
		}
		NPCManager.manage.refreshAllAnimators(on: true);
		gamePausedScreen.SetActive(value: false);
	}

	public void saveButton()
	{
		if (WeatherManager.Instance.IsMyPlayerInside)
		{
			NotificationManager.manage.createChatNotification("You must be outside to save", specialTip: true);
		}
		else
		{
			StartCoroutine(SaveLoad.saveOrLoad.saveRoutine(NetworkMapSharer.Instance.isServer, takePhoto: true, endOfDaySave: false));
		}
	}
}
