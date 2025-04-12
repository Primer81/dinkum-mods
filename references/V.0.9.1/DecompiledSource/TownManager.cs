using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TownManager : MonoBehaviour
{
	public enum TownBeautyType
	{
		None,
		Path,
		Fence,
		Flowers,
		Farms,
		Decorations
	}

	public enum windowType
	{
		Awards,
		Deeds,
		Move
	}

	public static TownManager manage;

	[Header("Town Stuff")]
	public int currentHouseVersion;

	public int[] recyclingBoxPos = new int[2];

	public int[] startingDockPosition = new int[2];

	public int[] northTowerPos = new int[2];

	public int[] southTowerPos = new int[2];

	public int[] eastTowerPos = new int[2];

	public int[] westTowerPos = new int[2];

	public InventoryItem[] playerHouseStages;

	public List<int[]> buildingsRequested = new List<int[]>();

	public NPCBuildingDoors[] allShopFloors;

	public TileObject[] allCropsTypes;

	public TileObject[] allTownSeats;

	public TileObject[] fenceObjects;

	public bool baseTentFirstPlace;

	public bool firstConnect = true;

	public bool journalUnlocked;

	public bool mapUnlocked;

	public int lastNPCInTent = -1;

	public int daysInTent;

	public InventoryItem baseTentItem;

	public HouseDetails sleepInsideHouse;

	public float savedRot;

	public Vector3 saveCameraRot;

	public int[] savedInside = new int[2];

	public Vector3 lastSleptPos;

	public Vector3 lastSavedPos;

	public bool calculateNow;

	[Header("Conversations")]
	public ConversationObject debtCompleteConvo;

	[Header("Town AwardsStuff ------")]
	public GameObject deedButtonPrefab;

	public GameObject townManagerWindow;

	public GameObject awardWindow;

	public bool townManagerOpen;

	public Transform deedBuyScrollBox;

	public TownAward beautyAward;

	public TownAward amanitiesAward;

	public TownAward economyAward;

	public TownAward happinessAward;

	public TownAward museumAward;

	public TownAward licenceAward;

	public Image townDebtFill;

	public TextMeshProUGUI debtAmount;

	public GameObject debtWindow;

	public HeartContainer[] stars;

	private List<int> itemsCanSpawnInBin = new List<int>();

	public float[] beautyLevels = new float[6];

	public float townBeautyLevel;

	public float townAmenitiesLevel;

	public float townHappynessLevel;

	public float townEconomyLevel;

	public int moneySpentInTownTotal;

	private float npcsThatCanMoveIn;

	private int npcsThatHaveMovedIn;

	private float museumAwardAmount;

	private float licenceAwardAmount;

	private void Awake()
	{
		manage = this;
		recyclingBoxPos = new int[2] { -1, -1 };
		allShopFloors = new NPCBuildingDoors[Enum.GetNames(typeof(NPCSchedual.Locations)).Length];
	}

	public void getAllSeatsAndCrops()
	{
		List<TileObject> list = new List<TileObject>();
		List<TileObject> list2 = new List<TileObject>();
		for (int i = 0; i < WorldManager.Instance.allObjects.Length; i++)
		{
			if ((bool)WorldManager.Instance.allObjects[i].tileObjectFurniture && WorldManager.Instance.allObjects[i].tileObjectFurniture.isSeat)
			{
				list.Add(WorldManager.Instance.allObjects[i]);
			}
			if ((bool)WorldManager.Instance.allObjects[i].tileObjectGrowthStages && WorldManager.Instance.allObjects[i].tileObjectGrowthStages.needsTilledSoil)
			{
				list2.Add(WorldManager.Instance.allObjects[i]);
			}
		}
		allCropsTypes = list2.ToArray();
		allTownSeats = list.ToArray();
	}

	private void Start()
	{
		getAllSeatsAndCrops();
	}

	public int getCurrentHouseStage()
	{
		int num = HouseManager.manage.getPlayersHousePos()[0];
		int num2 = HouseManager.manage.getPlayersHousePos()[1];
		if (num != -1 && num2 != -1)
		{
			for (int i = 0; i < playerHouseStages.Length; i++)
			{
				if (playerHouseStages[i].placeable.tileObjectId == WorldManager.Instance.onTileMap[num, num2])
				{
					return i;
				}
			}
		}
		else
		{
			Debug.LogWarning("No player House here");
		}
		return -1;
	}

	public int getNextHouseCost()
	{
		currentHouseVersion = getCurrentHouseStage();
		return playerHouseStages[currentHouseVersion + 1].value;
	}

	public void payForUpgradeAndSetBuildForTomorrow()
	{
		int xPos = HouseManager.manage.getPlayersHousePos()[0];
		int yPos = HouseManager.manage.getPlayersHousePos()[1];
		Inventory.Instance.changeWallet(-getNextHouseCost());
		NetworkMapSharer.Instance.RpcGiveOnTileStatus(1, xPos, yPos);
	}

	public bool checkIfHouseIsBeingUpgraded()
	{
		int num = HouseManager.manage.getPlayersHousePos()[0];
		int num2 = HouseManager.manage.getPlayersHousePos()[1];
		if (WorldManager.Instance.onTileStatusMap[num, num2] == 1)
		{
			return true;
		}
		return false;
	}

	public bool checkIfHouseIsBeingMoved()
	{
		int num = HouseManager.manage.getPlayersHousePos()[0];
		int num2 = HouseManager.manage.getPlayersHousePos()[1];
		if (WorldManager.Instance.onTileMap[num, num2] == BuildingManager.manage.currentlyMoving)
		{
			return true;
		}
		return false;
	}

	public void townMembersDonate()
	{
		if (NetworkMapSharer.Instance.townDebt <= 0)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < NPCManager.manage.npcStatus.Count; i++)
		{
			if (NPCManager.manage.npcStatus[i].checkIfHasMovedIn())
			{
				num += NetworkMapSharer.Instance.townDebt / UnityEngine.Random.Range(26 * NPCManager.manage.npcStatus.Count, 38 * NPCManager.manage.npcStatus.Count);
			}
		}
		manage.payTownDebt(num);
	}

	public bool checkIfBuildingInteriorHasBeenRequested(int xPos, int yPos)
	{
		for (int i = 0; i < buildingsRequested.Count; i++)
		{
			if (buildingsRequested[i][0] == xPos && buildingsRequested[i][1] == yPos)
			{
				return true;
			}
		}
		return false;
	}

	public void checkIfBaseTentFirstPlace()
	{
		if ((bool)NetworkMapSharer.Instance && (bool)NetworkMapSharer.Instance.localChar && !baseTentFirstPlace)
		{
			baseTentFirstPlace = true;
			StartCoroutine(doBaseTentCheckDelay());
		}
	}

	public IEnumerator doBaseTentCheckDelay()
	{
		NetworkMapSharer.Instance.fadeToBlack.fadeTime = 0.5f;
		NetworkMapSharer.Instance.fadeToBlack.fadeIn();
		yield return new WaitForSeconds(1f);
		NPCManager.manage.moveNpcToPlayerAndStartTalking(6, turnplayer: true);
		NetworkMapSharer.Instance.fadeToBlack.fadeOut();
	}

	public void checkIfFirstConnect()
	{
		if (firstConnect && (bool)NetworkMapSharer.Instance && NetworkMapSharer.Instance.isServer)
		{
			WorldManager.Instance.spawnFirstConnectAirShip();
		}
	}

	public void addBuildingAlreadyRequested(int xPos, int yPos)
	{
		if (WorldManager.Instance.onTileMap[xPos, yPos] == baseTentItem.placeable.tileObjectId)
		{
			checkIfBaseTentFirstPlace();
		}
		buildingsRequested.Add(new int[2] { xPos, yPos });
	}

	public void removeBuildingAlreadyRequestedOnUpgrade(int xPos, int yPos)
	{
		for (int i = 0; i < buildingsRequested.Count; i++)
		{
			if (buildingsRequested[i][0] == xPos && buildingsRequested[i][1] == yPos)
			{
				buildingsRequested.RemoveAt(i);
				break;
			}
		}
	}

	public void randomiseRecyclingBox()
	{
		if (WorldManager.Instance.day == 4)
		{
			ContainerManager.manage.clearWholeContainer(recyclingBoxPos[0], recyclingBoxPos[1], null);
		}
		else if (recyclingBoxPos[0] != -1 && recyclingBoxPos[1] != -1 && UnityEngine.Random.Range(0, 11) <= 6)
		{
			Chest chestForRecycling = ContainerManager.manage.getChestForRecycling(recyclingBoxPos[0], recyclingBoxPos[1], null);
			int num = UnityEngine.Random.Range(0, chestForRecycling.itemIds.Length - 1);
			int num2 = 0;
			while (chestForRecycling.itemIds[num] != -1 && num2 < 24)
			{
				num = UnityEngine.Random.Range(0, chestForRecycling.itemIds.Length - 1);
				num2++;
			}
			if (chestForRecycling.itemIds[num] == -1)
			{
				fillListPossibleList();
				ContainerManager.manage.changeSlotInChest(recyclingBoxPos[0], recyclingBoxPos[1], num, itemsCanSpawnInBin[UnityEngine.Random.Range(0, itemsCanSpawnInBin.Count)], 1, null);
			}
		}
	}

	private void fillListPossibleList()
	{
		if (itemsCanSpawnInBin.Count != 0)
		{
			return;
		}
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if ((Inventory.Instance.allItems[i].isFurniture || ((bool)Inventory.Instance.allItems[i].equipable && Inventory.Instance.allItems[i].equipable.cloths) || ((bool)Inventory.Instance.allItems[i].equipable && Inventory.Instance.allItems[i].equipable.wallpaper) || ((bool)Inventory.Instance.allItems[i].equipable && Inventory.Instance.allItems[i].equipable.flooring) || (bool)Inventory.Instance.allItems[i].consumeable || (bool)Inventory.Instance.allItems[i].itemChange || (Inventory.Instance.allItems[i].isRequestable && !Inventory.Instance.allItems[i].fish && !Inventory.Instance.allItems[i].bug)) && !Inventory.Instance.allItems[i].isUniqueItem && !Inventory.Instance.allItems[i].isOneOfKindUniqueItem)
			{
				itemsCanSpawnInBin.Add(i);
			}
		}
	}

	public void addTownBeauty(float amount, TownBeautyType type)
	{
		beautyLevels[(int)type] += amount;
		if (beautyLevels[(int)type] < 0f)
		{
			beautyLevels[(int)type] = 0f;
		}
	}

	public void payTownDebt(int amountToPay)
	{
		NetworkMapSharer instance = NetworkMapSharer.Instance;
		instance.NetworktownDebt = instance.townDebt - amountToPay;
		if (NetworkMapSharer.Instance.townDebt < 0)
		{
			NetworkMapSharer.Instance.NetworktownDebt = 0;
		}
	}

	public void calculateTownScore()
	{
		townBeautyLevel = Mathf.Clamp(Mathf.Clamp(beautyLevels[1] / 2f, 0f, 25f) + Mathf.Clamp(beautyLevels[2] / 2f, 0f, 25f) + Mathf.Clamp(beautyLevels[3], 0f, 40f) + Mathf.Clamp(beautyLevels[4], 0f, 30f) + Mathf.Clamp(beautyLevels[5], 0f, 90f), 0f, 100f);
		townHappynessLevel = 0f;
		npcsThatCanMoveIn = 0f;
		npcsThatHaveMovedIn = 0;
		for (int i = 0; i < NPCManager.manage.npcStatus.Count; i++)
		{
			if ((bool)NPCManager.manage.NPCDetails[i].deedOnMoveRequest)
			{
				npcsThatCanMoveIn += 1f;
				if (NPCManager.manage.npcStatus[i].checkIfHasMovedIn())
				{
					npcsThatHaveMovedIn++;
				}
				townHappynessLevel += NPCManager.manage.npcStatus[i].relationshipLevel;
			}
		}
		townEconomyLevel = (float)moneySpentInTownTotal / 2500000f * 100f;
		int num = 0;
		for (int j = 0; j < allShopFloors.Length; j++)
		{
			if (allShopFloors[j] != null)
			{
				num++;
			}
		}
		townAmenitiesLevel = (float)npcsThatHaveMovedIn / npcsThatCanMoveIn * 100f;
		calculateMuseumAward();
		calculateLicenceAward();
		float num2 = (townBeautyLevel + townHappynessLevel / npcsThatCanMoveIn + townEconomyLevel + townAmenitiesLevel + museumAwardAmount + licenceAwardAmount) / 6f;
		for (int k = 0; k < stars.Length; k++)
		{
			stars[k].updateHealth((int)num2);
		}
	}

	public void openTownManager(windowType typeToOpen)
	{
		if (typeToOpen == windowType.Awards)
		{
			awardWindow.SetActive(value: true);
		}
		else
		{
			awardWindow.SetActive(value: false);
		}
		townManagerOpen = true;
		calculateTownScore();
		museumAward.fillAwardPercent(museumAwardAmount);
		happinessAward.fillAwardPercent(townHappynessLevel / npcsThatCanMoveIn);
		economyAward.fillAwardPercent(townEconomyLevel);
		beautyAward.fillAwardPercent(townBeautyLevel);
		amanitiesAward.fillAwardPercent(townAmenitiesLevel);
		licenceAward.fillAwardPercent(licenceAwardAmount);
		townManagerWindow.SetActive(value: true);
	}

	public void calculateMuseumAward()
	{
		int num = 0;
		for (int i = 0; i < MuseumManager.manage.fishDonated.Length; i++)
		{
			if (MuseumManager.manage.fishDonated[i])
			{
				num++;
			}
		}
		for (int j = 0; j < MuseumManager.manage.bugsDonated.Length; j++)
		{
			if (MuseumManager.manage.bugsDonated[j])
			{
				num++;
			}
		}
		museumAwardAmount = (float)num / (float)(MuseumManager.manage.bugsDonated.Length + MuseumManager.manage.fishDonated.Length) * 100f;
	}

	public void calculateLicenceAward()
	{
		int num = 0;
		int num2 = 0;
		for (int i = 1; i < LicenceManager.manage.allLicences.Length; i++)
		{
			num += LicenceManager.manage.allLicences[i].maxLevel;
			num2 += LicenceManager.manage.allLicences[i].getCurrentLevel();
		}
		licenceAwardAmount = (float)num2 / (float)num * 100f;
	}

	public void closeTownManager()
	{
		townManagerWindow.SetActive(value: false);
		townManagerOpen = false;
		BuildingManager.manage.closeWindow();
		DeedManager.manage.closeDeedWindow();
		MenuButtonsTop.menu.closeButtonDelay();
	}

	public void unlockJournalAndStartTime()
	{
		journalUnlocked = true;
		MilestoneManager.manage.checkIfNeedNotification();
		StartCoroutine(talkToFeltchWhenJournalUnlocked());
		DailyTaskGenerator.generate.generateFirstDailyTasks();
		RealWorldTimeLight.time.startTimeOnFirstDay();
		CurrencyWindows.currency.sideTaskBarSmall.gameObject.SetActive(value: true);
	}

	private IEnumerator talkToFeltchWhenJournalUnlocked()
	{
		while (GiftedItemWindow.gifted.windowOpen)
		{
			yield return null;
		}
		ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget);
	}

	public bool checkIfInMovingBuildingForSleep()
	{
		if (NetworkMapSharer.Instance.movingBuilding != -1 && savedInside[0] != -1 && savedInside[1] != -1 && WorldManager.Instance.onTileMap[savedInside[0], savedInside[1]] == NetworkMapSharer.Instance.movingBuilding)
		{
			return true;
		}
		return false;
	}
}
