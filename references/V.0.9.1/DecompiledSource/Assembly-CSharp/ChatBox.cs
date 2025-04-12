using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatBox : MonoBehaviour
{
	public static ChatBox chat;

	public TMP_InputField chatBox;

	public GameObject enterChatTextWindow;

	public GameObject chatLogWindow;

	public bool chatOpen;

	private List<string> history = new List<string>();

	private int showingHistoryNo = -1;

	public bool chatLogOpen = true;

	public ASound chatSend;

	private uint lastPersonToTalk;

	private bool showingHud = true;

	public GameObject chatBubble;

	public Transform chatBubbleWindow;

	public UIScrollBar myScrollbar;

	public List<ChatBubble> chatLog;

	public float chatSpeed = 1f;

	public GameObject whistleButton;

	private bool commandsOn;

	public RectTransform inputTextPos;

	private bool canOpen = true;

	private bool closeChatOnEmote;

	public Sprite pvpOnIcon;

	public Sprite pvpOffIcon;

	public Image pvpIconImage;

	public GameObject pvpButton;

	public GameObject pvpLoadingButton;

	public TextMeshProUGUI pvpLoadingCountdown;

	private void Awake()
	{
		chat = this;
		StartCoroutine(chatBubbleWindowScroll());
		if (PlayerPrefs.HasKey("DevCommandOn"))
		{
			if (PlayerPrefs.GetInt("DevCommandOn") == 1)
			{
				commandsOn = true;
			}
			else
			{
				commandsOn = false;
			}
		}
	}

	public void chatLogWindowToggle()
	{
		chatLogOpen = chatLogWindow.activeSelf;
	}

	private IEnumerator chatBubbleWindowScroll()
	{
		while (true)
		{
			float num = 0f;
			for (int num2 = chatLog.Count - 1; num2 > -1; num2--)
			{
				float b = num;
				num += chatLog[num2].getHeight() + 4f;
				float y = Mathf.Lerp(chatLog[num2].transform.localPosition.y, b, Time.deltaTime * 10f);
				chatLog[num2].transform.localPosition = new Vector3(chatLog[num2].transform.localPosition.x, y, chatLog[num2].transform.localPosition.z);
			}
			if (chatLog.Count <= 10)
			{
				chatSpeed = 1f;
			}
			else if (chatLog.Count <= 20)
			{
				chatSpeed = 2f;
			}
			else if (chatLog.Count < 30)
			{
				chatSpeed = 4f;
			}
			else if (chatLog.Count < 40)
			{
				chatSpeed = 6f;
			}
			else
			{
				chatSpeed = 8f;
			}
			yield return null;
		}
	}

	public void addToChatBox(EquipItemToChar charThatTalked, string message, bool specialMessage = false)
	{
		ChatBubble component = Object.Instantiate(chatBubble, chatBubbleWindow).GetComponent<ChatBubble>();
		component.fillBubble(charThatTalked.playerName, message);
		chatLog.Add(component);
		lastPersonToTalk = charThatTalked.netId;
	}

	private IEnumerator switchCanOpenTimer()
	{
		canOpen = false;
		yield return new WaitForSeconds(0.25f);
		canOpen = true;
	}

	private void Update()
	{
		if (!NetworkMapSharer.Instance || !NetworkMapSharer.Instance.localChar || Inventory.Instance.menuOpen)
		{
			return;
		}
		if (chatOpen)
		{
			if (NetworkMapSharer.Instance.localChar.myPickUp.drivingVehicle)
			{
				whistleButton.SetActive(value: false);
			}
			else
			{
				whistleButton.SetActive(value: true);
			}
			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				showingHistoryNo = Mathf.Clamp(showingHistoryNo - 1, 0, history.Count);
				if (showingHistoryNo == history.Count)
				{
					chatBox.text = "";
				}
				else
				{
					chatBox.text = history[showingHistoryNo];
				}
			}
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				showingHistoryNo = Mathf.Clamp(showingHistoryNo + 1, 0, history.Count);
				if (showingHistoryNo == history.Count)
				{
					chatBox.text = "";
				}
				else
				{
					chatBox.text = history[showingHistoryNo];
				}
			}
		}
		if (!chatOpen && !Inventory.Instance.CanMoveCharacter())
		{
			return;
		}
		if (InputMaster.input.OpenChat())
		{
			Inventory.Instance.usingMouse = true;
		}
		if (!InputMaster.input.OpenChat() && (!chatOpen || !InputMaster.input.UICancel()) && (!chatOpen || !InputMaster.input.UISelectActiveConfirmButton()) && !closeChatOnEmote)
		{
			return;
		}
		closeChatOnEmote = false;
		StartCoroutine(switchCanOpenTimer());
		enterChatTextWindow.gameObject.SetActive(!chatOpen);
		chatOpen = !chatOpen;
		Inventory.Instance.checkIfWindowIsNeeded();
		if (chatOpen)
		{
			MenuButtonsTop.menu.closed = false;
		}
		else
		{
			MenuButtonsTop.menu.closeButtonDelay();
		}
		if (!showingHud)
		{
			showingHud = !Inventory.Instance.casters[0].GetComponent<Canvas>().enabled;
			GraphicRaycaster[] casters = Inventory.Instance.casters;
			for (int i = 0; i < casters.Length; i++)
			{
				casters[i].GetComponent<Canvas>().enabled = showingHud;
			}
		}
		if (chatOpen)
		{
			chatBox.ActivateInputField();
			return;
		}
		if (chatBox.text != "")
		{
			history.Add(chatBox.text);
		}
		showingHistoryNo = history.Count;
		string[] array = chatBox.text.Split(' ');
		if (array[0] == "devCommandsOn")
		{
			PlayerPrefs.SetInt("DevCommandOn", 1);
			string message = "<color=red> Dev Commands are not currently recommended for players and can cause problems that might ruin your save.\n\n Use at your own risk.</color>";
			addToChatBox(NetworkMapSharer.Instance.localChar.myEquip, message);
			commandsOn = true;
		}
		else if (array[0] == "devCommandsOff")
		{
			PlayerPrefs.DeleteKey("DevCommandOn");
			PlayerPrefs.DeleteKey("Cheats");
			CheatScript.cheat.cheatsOn = false;
			commandsOn = false;
		}
		else if (commandsOn && array[0] == "giveMilestone")
		{
			MilestoneManager.manage.doATaskAndCountToMilestone((DailyTaskGenerator.genericTaskType)Random.Range(1, MilestoneManager.manage.milestones.Count), 100);
		}
		else if (commandsOn && array[0] == "EasyAnimals")
		{
			AnimalManager.manage.MakeAnimalsEasy();
		}
		else if (commandsOn && array[0] == "warpNPCs")
		{
			for (int j = 0; j < NPCManager.manage.npcsOnMap.Count; j++)
			{
				NetworkNavMesh.nav.SpawnAnNPCAtPosition(NPCManager.manage.npcsOnMap[j].npcId, NetworkMapSharer.Instance.localChar.transform.position);
			}
		}
		else if (commandsOn && array[0] == "placeXMarker")
		{
			MonoBehaviour.print("Placing X Marker");
		}
		else if (commandsOn && array[0] == "korean")
		{
			LocalizationManager.CurrentLanguage = "Korean";
		}
		else if (commandsOn && array[0] == "playNewSong")
		{
			MusicManager.manage.playNewSong();
		}
		else if (commandsOn && array[0] == "changeWeather")
		{
			if (array[1] == "rain")
			{
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isRainy = true;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isStormy = false;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isOvercast = true;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isFoggy = false;
			}
			if (array[1] == "heatWave")
			{
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isRainy = false;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isStormy = false;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isOvercast = false;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isFoggy = false;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isHeatWave = true;
			}
			if (array[1] == "meteor")
			{
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isRainy = false;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isStormy = false;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isOvercast = false;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isFoggy = false;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isHeatWave = false;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isMeteorShower = true;
			}
			if (array[1] == "overcast")
			{
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isRainy = false;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isStormy = false;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isOvercast = true;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isFoggy = false;
			}
			if (array[1] == "storming")
			{
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isRainy = true;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isStormy = true;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isOvercast = true;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isFoggy = false;
			}
			if (array[1] == "foggy")
			{
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isRainy = false;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isStormy = false;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isOvercast = true;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isFoggy = true;
			}
			if (array[1] == "clear")
			{
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isRainy = false;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isStormy = false;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isOvercast = false;
				NetworkMapSharer.Instance.todaysWeather[WeatherManager.Instance.GetDayOfTimeWeatherIndex()].isFoggy = false;
			}
		}
		else if (!commandsOn || !(array[0] == "giveGift"))
		{
			if (commandsOn && array[0] == "lockRotation")
			{
				NetworkMapSharer.Instance.localChar.MyRigidBody.freezeRotation = !NetworkMapSharer.Instance.localChar.MyRigidBody.freezeRotation;
			}
			else if (commandsOn && array[0] == "spawnBoat")
			{
				MarketPlaceManager.manage.spawnJimmysBoat();
			}
			else if (commandsOn && array[0] == "dropAllFurniture")
			{
				for (int k = 0; k < Inventory.Instance.allItems.Length; k++)
				{
					if (Inventory.Instance.allItems[k].isFurniture)
					{
						NetworkMapSharer.Instance.spawnAServerDrop(k, 1, CameraController.control.transform.position);
					}
				}
			}
			else if (commandsOn && array[0] == "nextDayChange")
			{
				WorldManager.Instance.doNextDayChange();
			}
			else if (commandsOn && array[0] == "renameIsland")
			{
				NetworkMapSharer.Instance.NetworkislandName = array[1];
				Inventory.Instance.islandName = array[1];
			}
			else if (commandsOn && array[0] == "spawnCarry")
			{
				NetworkMapSharer.Instance.spawnACarryable(SaveLoad.saveOrLoad.carryablePrefabs[int.Parse(array[1])], NetworkMapSharer.Instance.localChar.transform.position);
			}
			else if (commandsOn && array[0] == "spawnMeteor")
			{
				NetworkMapSharer.Instance.SpawnMeteor();
			}
			else if (commandsOn && array[0] == "resetHouse")
			{
				if (NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails != null)
				{
					NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails.resetHouseMap();
				}
			}
			else if (commandsOn && array[0] == "revealMap")
			{
				RenderMap.Instance.ScanTheMap();
			}
			else if (commandsOn && array[0] == "resetWeather")
			{
				WeatherManager.Instance.CreateNewWeatherPatterns();
			}
			else if (commandsOn && array[0] == "resetHouseExteriors")
			{
				HouseManager.manage.clearHouseExteriors();
			}
			else if (commandsOn && array[0] == "refreshInside")
			{
				if (NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails != null)
				{
					NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails.upgradeHouseSize();
				}
			}
			else if (commandsOn && array[0] == "cropsGrowAllSeasons")
			{
				for (int l = 0; l < WorldManager.Instance.allObjects.Length; l++)
				{
					if ((bool)WorldManager.Instance.allObjects[l].tileObjectGrowthStages && WorldManager.Instance.allObjects[l].tileObjectGrowthStages.needsTilledSoil)
					{
						WorldManager.Instance.allObjects[l].tileObjectGrowthStages.growsInWinter = true;
						WorldManager.Instance.allObjects[l].tileObjectGrowthStages.growsInSummer = true;
						WorldManager.Instance.allObjects[l].tileObjectGrowthStages.growsInAutum = true;
						WorldManager.Instance.allObjects[l].tileObjectGrowthStages.growsInSpring = true;
					}
				}
			}
			else if (!commandsOn || !(array[0] == "compassLock"))
			{
				if (commandsOn && array[0] == "randomiseWeather")
				{
					WeatherManager.Instance.CreateNewWeatherPatterns();
					WeatherManager.Instance.CreateNewWeatherPatterns();
				}
				else if (commandsOn && array[0] == "cheatsOn")
				{
					PlayerPrefs.SetInt("Cheats", 1);
					CheatScript.cheat.cheatsOn = true;
				}
				else if (commandsOn && array[0] == "npcPhoto")
				{
					CharacterCreatorScript.create.takeNPCPhoto(int.Parse(array[1]));
				}
				else if (commandsOn && array[0] == "setTired")
				{
					StatusManager.manage.changeStamina(-40f);
				}
				else if (commandsOn && array[0] == "givePoints")
				{
					PermitPointsManager.manage.addPoints(int.Parse(array[1]));
				}
				else if (commandsOn && array[0] == "setDisguise")
				{
					NetworkMapSharer.Instance.localChar.myEquip.CmdSetDisguise(int.Parse(array[1]));
				}
				else if (commandsOn && array[0] == "/e")
				{
					NetworkMapSharer.Instance.localChar.CmdSendEmote(int.Parse(array[1]));
				}
				else if (commandsOn && array[0] == "skipSong")
				{
					MusicManager.manage.outsideMusic.time = MusicManager.manage.outsideMusic.clip.length - 30f;
				}
				else if (commandsOn && array[0] == "setAnimalRel")
				{
					foreach (FarmAnimalDetails farmAnimalDetail in FarmAnimalManager.manage.farmAnimalDetails)
					{
						farmAnimalDetail.animalRelationShip = int.Parse(array[1]);
					}
				}
				else if (commandsOn && array[0] == "fullPedia")
				{
					foreach (PediaEntry allEntry in PediaManager.manage.allEntries)
					{
						allEntry.amountCaught = 1;
					}
				}
				else if (commandsOn && array[0] == "randomBoard")
				{
					BulletinBoard.board.RandomisePostsForTesting();
				}
				else if (commandsOn && array[0] == "randomRequests")
				{
					for (int m = 0; m < NPCManager.manage.npcStatus.Count; m++)
					{
						if (NPCManager.manage.npcStatus[m].hasMovedIn)
						{
							NPCManager.manage.npcStatus[m].acceptedRequest = false;
							NPCManager.manage.npcStatus[m].completedRequest = false;
							NPCManager.manage.NPCRequests[m].RefreshAcceptedAndCompletedOnNewDay();
							NPCManager.manage.NPCRequests[m].GetNewRequest(m);
							NPCManager.manage.NPCRequests[m].acceptRequest(m, makeNotification: false);
						}
					}
				}
				else if (commandsOn && array[0] == "clearNpcDay")
				{
					for (int n = 0; n < NPCManager.manage.npcStatus.Count; n++)
					{
						if (NPCManager.manage.npcStatus[n].hasMovedIn)
						{
							NPCManager.manage.npcStatus[n].hasBeenTalkedToToday = false;
							NPCManager.manage.npcStatus[n].hasGossipedToday = false;
							NPCManager.manage.npcStatus[n].acceptedRequest = false;
							NPCManager.manage.npcStatus[n].completedRequest = false;
							NPCManager.manage.NPCRequests[n].generatedToday = false;
							NPCManager.manage.NPCRequests[n].RefreshAcceptedAndCompletedOnNewDay();
						}
					}
				}
				else if (commandsOn && array[0] == "giveHats")
				{
					for (int num = 0; num < NPCManager.manage.npcsOnMap.Count; num++)
					{
						if ((bool)NPCManager.manage.npcsOnMap[num].activeNPC)
						{
							NPCManager.manage.npcsOnMap[num].activeNPC.GetComponent<NPCIdentity>().SetHatOnHead(-1, RandomObjectGenerator.generate.getRandomHat().getItemId());
							NPCManager.manage.npcsOnMap[num].activeNPC.GetComponent<NPCIdentity>().OnChangeTop(-1, RandomObjectGenerator.generate.getRandomShirtOrDressForShop().getItemId());
							NPCManager.manage.npcsOnMap[num].activeNPC.GetComponent<NPCIdentity>().SetFaceOnHead(-1, RandomObjectGenerator.generate.getRandomFaceItem().getItemId());
						}
					}
				}
				else if (commandsOn && array[0] == "placeItem")
				{
					bool flag = true;
					if (WorldManager.Instance.onTileMap[(int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.x, (int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.y] >= 0 && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[(int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.x, (int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.y]].isMultiTileObject)
					{
						if ((bool)WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[(int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.x, (int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.y]].tileObjectLoadInside || (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[(int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.x, (int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.y]].displayPlayerHouseTiles)
						{
							string message2 = "<color=red> Can not move buildings this way.</color>";
							addToChatBox(NetworkMapSharer.Instance.localChar.myEquip, message2);
							flag = false;
						}
						else
						{
							NetworkMapSharer.Instance.RpcRemoveMultiTiledObject(WorldManager.Instance.onTileMap[(int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.x, (int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.y], (int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.x, (int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.y, WorldManager.Instance.rotationMap[(int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.x, (int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.y]);
						}
					}
					if (flag)
					{
						NetworkMapSharer.Instance.RpcUpdateOnTileObject(int.Parse(array[1]), (int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.x, (int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.y);
					}
				}
				else if (commandsOn && array[0] == "placeItemFix")
				{
					NetworkMapSharer.Instance.RpcUpdateOnTileObject(int.Parse(array[1]), Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.position.x / 2f), Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.position.z / 2f));
				}
				else if (commandsOn && array[0] == "setRotation")
				{
					WorldManager.Instance.rotationMap[Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.position.x / 2f), Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.position.z / 2f)] = int.Parse(array[1]);
				}
				else if (commandsOn && array[0] == "setDate")
				{
					WorldManager.Instance.day = int.Parse(array[1]);
					WorldManager.Instance.week = int.Parse(array[2]);
					WorldManager.Instance.month = int.Parse(array[3]);
					WorldManager.Instance.year = int.Parse(array[4]);
				}
				else if (commandsOn && array[0] == "makeAutumn")
				{
					if ((bool)NetworkMapSharer.Instance && NetworkMapSharer.Instance.isServer)
					{
						WorldManager.Instance.day = 1;
						WorldManager.Instance.week = 1;
						WorldManager.Instance.month = 2;
						NetworkMapSharer.Instance.RpcSyncDate(1, 1, 2, WorldManager.Instance.year, 0);
						RealWorldTimeLight.time.NetworkcurrentHour = 7;
						WorldManager.Instance.nextDay();
					}
				}
				else if (commandsOn && array[0] == "makeSummer")
				{
					if ((bool)NetworkMapSharer.Instance && NetworkMapSharer.Instance.isServer)
					{
						WorldManager.Instance.day = 1;
						WorldManager.Instance.week = 1;
						WorldManager.Instance.month = 1;
						NetworkMapSharer.Instance.RpcSyncDate(1, 1, 1, WorldManager.Instance.year, 0);
						RealWorldTimeLight.time.NetworkcurrentHour = 7;
						WorldManager.Instance.nextDay();
					}
				}
				else if (commandsOn && array[0] == "makeWinter")
				{
					if ((bool)NetworkMapSharer.Instance && NetworkMapSharer.Instance.isServer)
					{
						WorldManager.Instance.day = 1;
						WorldManager.Instance.week = 1;
						WorldManager.Instance.month = 3;
						NetworkMapSharer.Instance.RpcSyncDate(1, 1, 3, WorldManager.Instance.year, 0);
						RealWorldTimeLight.time.NetworkcurrentHour = 7;
						WorldManager.Instance.nextDay();
					}
				}
				else if (commandsOn && array[0] == "makeSpring")
				{
					if ((bool)NetworkMapSharer.Instance && NetworkMapSharer.Instance.isServer)
					{
						WorldManager.Instance.day = 1;
						WorldManager.Instance.week = 1;
						WorldManager.Instance.month = 4;
						NetworkMapSharer.Instance.RpcSyncDate(1, 1, 4, WorldManager.Instance.year, 0);
						RealWorldTimeLight.time.NetworkcurrentHour = 7;
						WorldManager.Instance.nextDay();
					}
				}
				else if (commandsOn && array[0] == "makeSkyFest")
				{
					if ((bool)NetworkMapSharer.Instance && NetworkMapSharer.Instance.isServer)
					{
						WorldManager.Instance.day = 7;
						WorldManager.Instance.week = 1;
						WorldManager.Instance.month = 2;
						NetworkMapSharer.Instance.RpcSyncDate(7, 1, 2, WorldManager.Instance.year, 0);
						RealWorldTimeLight.time.NetworkcurrentHour = 7;
						WorldManager.Instance.nextDay();
					}
				}
				else if (commandsOn && array[0] == "makeIslandDay")
				{
					if ((bool)NetworkMapSharer.Instance && NetworkMapSharer.Instance.isServer)
					{
						WorldManager.Instance.day = 6;
						WorldManager.Instance.week = 4;
						WorldManager.Instance.month = 4;
						NetworkMapSharer.Instance.RpcSyncDate(6, 4, 4, WorldManager.Instance.year, 0);
						RealWorldTimeLight.time.NetworkcurrentHour = 7;
						WorldManager.Instance.nextDay();
					}
				}
				else if (commandsOn && array[0] == "makeBugComp")
				{
					if ((bool)NetworkMapSharer.Instance && NetworkMapSharer.Instance.isServer)
					{
						WorldManager.Instance.day = 7;
						WorldManager.Instance.week = 2;
						WorldManager.Instance.month = 2;
						NetworkMapSharer.Instance.RpcSyncDate(7, 2, 2, WorldManager.Instance.year, 0);
						RealWorldTimeLight.time.NetworkcurrentHour = 7;
						WorldManager.Instance.nextDay();
					}
				}
				else if (commandsOn && array[0] == "makeFishComp")
				{
					if ((bool)NetworkMapSharer.Instance && NetworkMapSharer.Instance.isServer)
					{
						WorldManager.Instance.day = 7;
						WorldManager.Instance.week = 3;
						WorldManager.Instance.month = 3;
						NetworkMapSharer.Instance.RpcSyncDate(7, 3, 3, WorldManager.Instance.year, 0);
						RealWorldTimeLight.time.NetworkcurrentHour = 7;
						WorldManager.Instance.nextDay();
					}
				}
				else if (commandsOn && array[0] == "maxBugs")
				{
					for (int num2 = 0; num2 < 100; num2++)
					{
						int num3 = Random.Range(0, Inventory.Instance.allItems.Length);
						while (!Inventory.Instance.allItems[num3].bug)
						{
							num3 = Random.Range(0, Inventory.Instance.allItems.Length);
						}
						CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.BugCatching, (int)Mathf.Clamp((float)Inventory.Instance.allItems[num3].value / 200f, 1f, 100f));
						CharLevelManager.manage.addToDayTally(num3, 1, 4);
					}
				}
				else if (commandsOn && array[0] == "scanMap")
				{
					StartCoroutine(RenderMap.Instance.ScanTheMap());
				}
				else if (commandsOn && array[0] == "spawnFish")
				{
					NetworkNavMesh.nav.spawnAFishBreakFree(int.Parse(array[1]), NetworkMapSharer.Instance.localChar.transform.position, NetworkMapSharer.Instance.localChar.transform);
				}
				else if (commandsOn && array[0] == "spawnBug")
				{
					NetworkNavMesh.nav.spawnSpecificBug(int.Parse(array[1]), NetworkMapSharer.Instance.localChar.transform.position);
				}
				else if (commandsOn && array[0] == "hairStyle")
				{
					if (int.TryParse(array[1], out var result))
					{
						NetworkMapSharer.Instance.localChar.myEquip.CmdChangeHairId(result);
					}
				}
				else if (commandsOn && array[0] == "strikeLightning")
				{
					NetworkMapSharer.Instance.RpcThunderStrike(new Vector2(int.Parse(array[1]), int.Parse(array[2])));
				}
				else if (commandsOn && array[0] == "hairColour")
				{
					if (int.TryParse(array[1], out var result2))
					{
						NetworkMapSharer.Instance.localChar.myEquip.CmdChangeHairColour(result2);
					}
				}
				else if (commandsOn && array[0] == "setStatus")
				{
					NetworkMapSharer.Instance.RpcGiveOnTileStatus(int.Parse(array[1]), Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.position.x / 2f), Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.position.z / 2f));
				}
				else if (commandsOn && array[0] == "setStatusDirty")
				{
					WorldManager.Instance.onTileStatusMap[Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.position.x / 2f), Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.position.z / 2f)] = int.Parse(array[1]);
				}
				else if (commandsOn && array[0] == "skinTone")
				{
					if (int.TryParse(array[1], out var result3))
					{
						NetworkMapSharer.Instance.localChar.myEquip.CmdChangeSkin(result3);
					}
				}
				else if (commandsOn && array[0] == "hideGuide")
				{
					NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.gameObject.SetActive(!NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.gameObject.activeSelf);
					TileHighlighter.highlight.off = !TileHighlighter.highlight.off;
				}
				else if (commandsOn && array[0] == "noClip")
				{
					CameraController.control.swapFlyCam();
				}
				else if (commandsOn && array[0] == "noClipNoFollow")
				{
					CameraController.control.swapFlyCam(followCam: false);
				}
				else if (commandsOn && array[0] == "saveFreeCam")
				{
					CameraController.control.saveFreeCam();
				}
				else if (commandsOn && array[0] == "loadFreeCam")
				{
					CameraController.control.loadFreeCam();
				}
				else if (commandsOn && array[0] == "clearFreeCam")
				{
					CameraController.control.clearFreeCam();
				}
				else if (commandsOn && array[0] == "hideHud")
				{
					showingHud = !Inventory.Instance.casters[0].GetComponent<Canvas>().enabled;
					GraphicRaycaster[] casters = Inventory.Instance.casters;
					for (int i = 0; i < casters.Length; i++)
					{
						casters[i].GetComponent<Canvas>().enabled = showingHud;
					}
				}
				else if (commandsOn && array[0] == "moveInNPC")
				{
					if (int.TryParse(array[1], out var result4) && result4 < NPCManager.manage.npcStatus.Count)
					{
						NPCManager.manage.moveInNPC(result4);
						NPCManager.manage.npcStatus[result4].hasAskedToMoveIn = true;
					}
				}
				else if (!commandsOn || !(array[0] == "makeWindy"))
				{
					if (commandsOn && array[0] == "completeNPC")
					{
						if (int.TryParse(array[1], out var result5) && result5 < NPCManager.manage.npcStatus.Count)
						{
							NPCManager.manage.npcStatus[result5].hasMet = true;
							NPCManager.manage.npcStatus[result5].relationshipLevel = NPCManager.manage.NPCDetails[result5].relationshipBeforeMove;
							NPCManager.manage.npcStatus[result5].moneySpentAtStore = NPCManager.manage.NPCDetails[result5].spendBeforeMoveIn;
						}
					}
					else if (commandsOn && array[0] == "maxRelation")
					{
						if (int.TryParse(array[1], out var result6) && result6 < NPCManager.manage.npcStatus.Count)
						{
							NPCManager.manage.npcStatus[result6].hasMet = true;
							NPCManager.manage.npcStatus[result6].relationshipLevel = 100;
						}
					}
					else if (commandsOn && array[0] == "spawnFarmAnimal")
					{
						FarmAnimalManager.manage.spawnNewFarmAnimalWithDetails(int.Parse(array[1]), int.Parse(array[2]), array[3], NetworkMapSharer.Instance.localChar.transform.position);
					}
					else if (commandsOn && array[0] == "spawnLanterns")
					{
						TownEventManager.manage.PlaceLanterns();
					}
					else if (commandsOn && array[0] == "moveAllCarry")
					{
						WorldManager.Instance.moveAllCarriablesToSpawn();
					}
					else if (commandsOn && array[0] == "save")
					{
						SaveLoad.saveOrLoad.newFileSaver.SaveGame(NetworkMapSharer.Instance.isServer, takePhoto: true, endOfDaySave: false);
					}
					else if (commandsOn && array[0] == "crocDay")
					{
						AnimalManager.manage.crocDay = !AnimalManager.manage.crocDay;
					}
					else if (commandsOn && array[0] == "randomClothing")
					{
						StartCoroutine(EquipWindow.equip.randomClothes());
					}
					else if (commandsOn && array[0] == "randomiseCharacter")
					{
						EquipWindow.equip.randomiseCharacter();
					}
					else if (commandsOn && array[0] == "stopRandom")
					{
						EquipWindow.equip.keepChanging = false;
					}
					else if (commandsOn && array[0] == "nextDay")
					{
						if ((bool)NetworkMapSharer.Instance && NetworkMapSharer.Instance.isServer)
						{
							RealWorldTimeLight.time.NetworkcurrentHour = 7;
							WorldManager.Instance.nextDay();
						}
					}
					else if (commandsOn && array[0] == "chunkDistance")
					{
						NewChunkLoader.loader.setChunkDistance(int.Parse(array[1]));
					}
					else if (commandsOn && array[0] == "freeCam")
					{
						CameraController.control.swapFreeCam();
					}
					else if (commandsOn && array[0] == "spawnAnimal")
					{
						Vector3 position = NetworkMapSharer.Instance.localChar.transform.position;
						NetworkNavMesh.nav.SpawnAnAnimalOnTile(int.Parse(array[1]), (int)(position.x / 2f), (int)(position.z / 2f));
					}
					else if (chatBox.text == "debug")
					{
						bool flag2 = !base.transform.Find("Debug").gameObject.activeSelf;
						base.transform.Find("Debug").gameObject.SetActive(flag2);
						if (flag2)
						{
							NPCManager.manage.turnOnNpcDebugMarkers();
						}
						else
						{
							NPCManager.manage.removeDebugMarkers();
						}
					}
					else if (commandsOn && array[0] == "giveMoney")
					{
						Inventory.Instance.changeWallet(int.Parse(array[1]));
					}
					else if (commandsOn && array[0] == "changeSize")
					{
						NetworkMapSharer.Instance.localChar.myEquip.CmdChangeSize(new Vector3(float.Parse(array[1]), float.Parse(array[1]), float.Parse(array[1])));
					}
					else if (commandsOn && array[0] == "clearFood")
					{
						for (int num4 = 0; num4 < StatusManager.manage.eatenFoods.Length; num4++)
						{
							StatusManager.manage.eatenFoods[num4].ClearFood();
						}
					}
					else if (commandsOn && array[0] == "teleport")
					{
						NetworkMapSharer.Instance.localChar.transform.position = new Vector3(int.Parse(array[1]), 10f, int.Parse(array[2]));
						CameraController.control.transform.position = NetworkMapSharer.Instance.localChar.transform.position;
						NewChunkLoader.loader.forceInstantUpdateAtPos();
					}
					else if (commandsOn && array[0] == "checkPath")
					{
						NPCManager.manage.pathFinder.CanReach(new Vector2Int(int.Parse(array[1]), int.Parse(array[2])), new Vector2Int(int.Parse(array[3]), int.Parse(array[4])));
					}
					else if (commandsOn && array[0] == "changeSpeed")
					{
						RealWorldTimeLight.time.changeSpeed(float.Parse(array[1]));
					}
					else if (commandsOn && array[0] == "setTime")
					{
						RealWorldTimeLight.time.useTime = false;
						RealWorldTimeLight.time.NetworkcurrentHour = int.Parse(array[1]);
					}
					else if (commandsOn && array[0] == "spawnNpc")
					{
						if (int.Parse(array[1]) >= 0 && int.Parse(array[1]) < NPCManager.manage.NPCDetails.Length)
						{
							int xPos = (int)NetworkMapSharer.Instance.localChar.transform.position.x / 2;
							int yPos = (int)NetworkMapSharer.Instance.localChar.transform.position.z / 2;
							NPCManager.manage.setUpNPCAgent(int.Parse(array[1]), xPos, yPos);
							NetworkNavMesh.nav.SpawnAnNPCAtPosition(int.Parse(array[1]), NetworkMapSharer.Instance.localChar.transform.position);
						}
					}
					else if (commandsOn && chatBox.text == "completeQuests")
					{
						for (int num5 = 0; num5 < QuestManager.manage.allQuests.Length; num5++)
						{
							QuestManager.manage.isQuestAccepted[num5] = true;
							QuestManager.manage.isQuestCompleted[num5] = true;
							for (int num6 = 0; num6 < QuestManager.manage.allQuests[num5].unlockRecipeOnComplete.Length; num6++)
							{
								GiftedItemWindow.gifted.addRecipeToUnlock(Inventory.Instance.getInvItemId(QuestManager.manage.allQuests[num5].unlockRecipeOnComplete[num6]));
							}
						}
						GiftedItemWindow.gifted.openWindowAndGiveItems();
					}
					else if (commandsOn && chatBox.text == "levelUp")
					{
						for (int num7 = 0; num7 < CharLevelManager.manage.currentLevels.Length; num7++)
						{
							CharLevelManager.manage.currentLevels[num7] = 20;
						}
						CharLevelManager.manage.currentLevels[4] = 9;
					}
					else if (commandsOn && chatBox.text == "unlockRecipes")
					{
						for (int num8 = 0; num8 < Inventory.Instance.allItems.Length; num8++)
						{
							if ((bool)Inventory.Instance.allItems[num8].craftable)
							{
								CharLevelManager.manage.unlockRecipe(Inventory.Instance.allItems[num8]);
							}
						}
					}
					else if (!commandsOn || !(chatBox.text == "changeRain"))
					{
						if (commandsOn && chatBox.text == "setTimeDay")
						{
							RealWorldTimeLight.time.useTime = false;
							RealWorldTimeLight.time.NetworkcurrentHour = 10;
						}
						else if (commandsOn && chatBox.text == "setTimeNight")
						{
							RealWorldTimeLight.time.useTime = false;
							RealWorldTimeLight.time.NetworkcurrentHour = RealWorldTimeLight.time.getSunSetTime() + 1;
						}
						else if (commandsOn && chatBox.text == "setTimeReal")
						{
							RealWorldTimeLight.time.useTime = true;
						}
						else if (chatBox.text != "")
						{
							NetworkMapSharer.Instance.localChar.CmdSendChatMessage(chatBox.text);
						}
					}
				}
			}
		}
		chatBox.text = "";
	}

	public void sendEmote(int emoteNo)
	{
		NetworkMapSharer.Instance.localChar.CmdSendEmote(emoteNo);
		closeChatOnEmote = true;
	}

	public void whistle()
	{
		if (!NetworkMapSharer.Instance.localChar.myEquip.isWhistling())
		{
			NetworkMapSharer.Instance.localChar.myEquip.CharWhistles();
			closeChatOnEmote = true;
		}
	}

	public void SitOnFloor()
	{
		NetworkMapSharer.Instance.localChar.myPickUp.PressSitOnFloor();
		closeChatOnEmote = true;
	}

	public void pressPVPButton()
	{
		CharNetworkOptions component = NetworkMapSharer.Instance.localChar.GetComponent<CharNetworkOptions>();
		if (component.pvpOn)
		{
			pvpIconImage.sprite = pvpOffIcon;
		}
		else
		{
			pvpIconImage.sprite = pvpOnIcon;
		}
		component.CmdSwapPvpOn();
		StartCoroutine(doPVPButtonCountDown(10));
	}

	private IEnumerator doPVPButtonCountDown(int setTimer)
	{
		pvpLoadingButton.SetActive(value: true);
		pvpButton.SetActive(value: false);
		int timer = setTimer;
		WaitForSeconds wait = new WaitForSeconds(1f);
		while (timer > 0)
		{
			pvpLoadingCountdown.text = timer + "s";
			yield return wait;
			timer--;
		}
		pvpLoadingButton.SetActive(value: false);
		pvpButton.SetActive(value: true);
	}
}
