using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Mirror;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomNetworkManager : NetworkManager
{
	public enum lobbyType
	{
		Friends,
		Invite,
		Public,
		Lan
	}

	public static CustomNetworkManager manage;

	public TMP_InputField ipAddressField;

	public UnityEvent onClientDisconnect = new UnityEvent();

	public UnityEvent onClientConnect = new UnityEvent();

	private bool usingSteam = true;

	private bool connected;

	private bool hosting;

	public Transport steamTransport;

	public Transport lanTransport;

	public Text usingSteamOrLanText;

	public GameObject steamManager;

	private object myConfig;

	public SteamLobby lobby;

	public GameObject disconectedScreen;

	[Header("Lobby Settings-----")]
	private lobbyType selectedLobbyType;

	public TextMeshProUGUI friendGameText;

	public TextMeshProUGUI inviteOnlyText;

	public TextMeshProUGUI publicGameText;

	public TextMeshProUGUI lanGameText;

	public GameObject[] hostingExplainations;

	private void OnEnable()
	{
		manage = this;
		refreshLobbyTypeButtons();
		if (steamManager == null)
		{
			steamManager = GameObject.Find("Steam_Manager");
		}
		LocalizationManager.OnLocalizeEvent += refreshLobbyTypeButtons;
	}

	public void setLobbyTypeButton(int newType)
	{
		selectedLobbyType = (lobbyType)newType;
		refreshLobbyTypeButtons();
		for (int i = 0; i < hostingExplainations.Length; i++)
		{
			hostingExplainations[i].SetActive(i == newType);
		}
		if (selectedLobbyType == lobbyType.Lan)
		{
			changeTransport(newIsSteam: false);
		}
		else
		{
			changeTransport(newIsSteam: true);
		}
	}

	public void refreshLobbyTypeButtons()
	{
		friendGameText.text = "<sprite=16> " + ConversationGenerator.generate.GetMenuText("Multiplayer_FriendsOnlyButton");
		inviteOnlyText.text = "<sprite=16> " + ConversationGenerator.generate.GetMenuText("Multiplayer_InviteOnlyButton");
		publicGameText.text = "<sprite=16> " + ConversationGenerator.generate.GetMenuText("Multiplayer_PublicGameButton");
		lanGameText.text = "<sprite=16> " + ConversationGenerator.generate.GetMenuText("Multiplayer_LanButton");
		if (selectedLobbyType == lobbyType.Friends)
		{
			friendGameText.text = friendGameText.text.Replace("<sprite=16>", "<sprite=17>");
		}
		if (selectedLobbyType == lobbyType.Invite)
		{
			inviteOnlyText.text = inviteOnlyText.text.Replace("<sprite=16>", "<sprite=17>");
		}
		if (selectedLobbyType == lobbyType.Public)
		{
			publicGameText.text = publicGameText.text.Replace("<sprite=16>", "<sprite=17>");
		}
		if (selectedLobbyType == lobbyType.Lan)
		{
			lanGameText.text = lanGameText.text.Replace("<sprite=16>", "<sprite=17>");
		}
	}

	public override void Start()
	{
		if (!Application.isEditor)
		{
			changeTransport(newIsSteam: true);
			steamManager.GetComponent<SteamManager>().enabled = true;
			lobby.gameObject.SetActive(value: true);
		}
		else
		{
			changeTransport(newIsSteam: false);
		}
	}

	public void turnSteamOnOrOff()
	{
		changeTransport(!usingSteam);
	}

	public void swapBackToHostButton()
	{
		setLobbyTypeButton((int)selectedLobbyType);
	}

	public void changeTransport(bool newIsSteam)
	{
		usingSteam = newIsSteam;
		lanTransport.enabled = !usingSteam;
		steamTransport.enabled = usingSteam;
		if (usingSteam)
		{
			transport = steamTransport;
			Transport.activeTransport = steamTransport;
		}
		else
		{
			transport = lanTransport;
			Transport.activeTransport = lanTransport;
		}
	}

	public void StartUpHost()
	{
		hosting = true;
		SetPort();
		NetworkManager.singleton.StartHost();
	}

	public void JoinGame()
	{
		SetIpAddress();
		SetPort();
		NetworkManager.singleton.StartClient();
	}

	public void JoinSteamGame(string steamId)
	{
		NetworkManager.singleton.networkAddress = steamId;
		NetworkManager.singleton.StartClient();
	}

	public void disconectClient()
	{
	}

	public void SetPort()
	{
	}

	public void SetIpAddress()
	{
		if (ipAddressField.text == "")
		{
			NetworkManager.singleton.networkAddress = "localhost";
		}
		else
		{
			NetworkManager.singleton.networkAddress = ipAddressField.text;
		}
	}

	public override void OnStartHost()
	{
		MusicManager.manage.changeFromMenu();
	}

	public bool checkIfLanGame()
	{
		return selectedLobbyType == lobbyType.Lan;
	}

	public void createLobbyBeforeConnection()
	{
		if (usingSteam)
		{
			if (selectedLobbyType == lobbyType.Friends)
			{
				SteamLobby.Instance.CreateLobbyWithSettings(manage.maxConnections);
			}
			else if (selectedLobbyType == lobbyType.Invite)
			{
				SteamLobby.Instance.CreateLobbyWithSettings(manage.maxConnections, ELobbyType.k_ELobbyTypePrivate);
			}
			else
			{
				SteamLobby.Instance.CreateLobbyWithSettings(manage.maxConnections, ELobbyType.k_ELobbyTypePublic);
			}
		}
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		SceneManager.LoadScene(1);
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		onClientDisconnect.Invoke();
		base.OnClientDisconnect(conn);
		if (base.mode != NetworkManagerMode.Host)
		{
			if (connected && !NetworkMapSharer.Instance.isServer)
			{
				_ = conn.connectionId;
				Inventory.Instance.menuOpen = true;
				Inventory.Instance.checkIfWindowIsNeeded();
				disconectedScreen.SetActive(value: true);
			}
			if ((bool)lobby)
			{
				lobby.LeaveGameLobby();
			}
		}
	}

	public void disconectionScreenButton()
	{
		connected = false;
		SceneManager.LoadScene(1);
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		connected = true;
		if (!hosting)
		{
			SaveLoad.saveOrLoad.LoadInvForMultiplayer();
			StartCoroutine(loadingOnConnect());
		}
		base.OnClientConnect(conn);
	}

	private IEnumerator loadingOnConnect()
	{
		SaveLoad.saveOrLoad.loadingScreen.appear("Tip_Connecting");
		cameraWonderOnMenu.wonder.enabled = false;
		yield return new WaitForSeconds(0.5f);
		onClientConnect.Invoke();
		MusicManager.manage.changeFromMenu();
		float time = 0f;
		while (time < 2f)
		{
			SaveLoad.saveOrLoad.loadingScreen.showPercentage(time / 2f);
			time += Time.deltaTime;
			yield return null;
		}
		SaveLoad.saveOrLoad.loadingScreen.completed();
		yield return new WaitForSeconds(0.5f);
		SaveLoad.saveOrLoad.loadingScreen.disappear();
	}

	public void leaveLobbyOnPressMultiplayer()
	{
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		foreach (NetworkIdentity item in new List<NetworkIdentity>(conn.clientOwnedObjects))
		{
			if (!NetworkIdentity.spawned.ContainsKey(item.netId))
			{
				continue;
			}
			PickUpAndCarry componentInParent = NetworkIdentity.spawned[item.netId].GetComponentInParent<PickUpAndCarry>();
			if (componentInParent != null)
			{
				if (componentInParent.beingCarriedBy == conn.identity.netId)
				{
					componentInParent.CarriedByHoldPos = null;
					componentInParent.NetworkbeingCarriedBy = 0u;
					componentInParent.MyRigidBody.isKinematic = true;
					componentInParent.ResetAfterPlayerWithAuthorityDisconnected = true;
				}
				NetworkIdentity component = componentInParent.GetComponent<NetworkIdentity>();
				component.RemoveClientAuthority();
				component.AssignClientAuthority(NetworkMapSharer.Instance.localChar.connectionToClient);
				continue;
			}
			NetworkBall componentInParent2 = NetworkIdentity.spawned[item.netId].GetComponentInParent<NetworkBall>();
			if (componentInParent2 != null)
			{
				NetworkIdentity component2 = componentInParent2.GetComponent<NetworkIdentity>();
				component2.RemoveClientAuthority();
				component2.AssignClientAuthority(NetworkMapSharer.Instance.localChar.connectionToClient);
				continue;
			}
			Vehicle componentInParent3 = NetworkIdentity.spawned[item.netId].GetComponentInParent<Vehicle>();
			if (componentInParent3 != null)
			{
				NetworkIdentity component3 = componentInParent3.GetComponent<NetworkIdentity>();
				component3.RemoveClientAuthority();
				componentInParent3.StopDriving();
				component3.AssignClientAuthority(NetworkMapSharer.Instance.localChar.connectionToClient);
			}
			PickUpAndCarry componentInParent4 = NetworkIdentity.spawned[item.netId].GetComponentInParent<PickUpAndCarry>();
			if ((bool)componentInParent4)
			{
				componentInParent4.GetComponent<NetworkIdentity>().RemoveClientAuthority();
				componentInParent4.transform.position = new Vector3(componentInParent4.transform.position.x, WorldManager.Instance.heightMap[Mathf.RoundToInt(componentInParent4.transform.position.x / 2f), Mathf.RoundToInt(componentInParent4.transform.position.z / 2f)], componentInParent4.transform.position.x);
			}
		}
		base.OnServerDisconnect(conn);
	}

	public bool isServerRunning()
	{
		return NetworkServer.active;
	}

	private void OnDisable()
	{
		LocalizationManager.OnLocalizeEvent -= refreshLobbyTypeButtons;
		onClientDisconnect.RemoveAllListeners();
		onClientConnect.RemoveAllListeners();
	}
}
