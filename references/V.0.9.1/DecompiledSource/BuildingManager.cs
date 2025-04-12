using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
	public static BuildingManager manage;

	public bool windowOpen;

	public GameObject moveBuilingButtonPrefab;

	public Transform moveBuildingWindow;

	public Transform buttonScrollArea;

	private List<MoveableBuilding> moveableBuildings = new List<MoveableBuilding>();

	public int currentlyMoving = -1;

	[Header("Conversations---------")]
	public ConversationObject alreadyMovingABuildingConversation;

	public ConversationObject houseIsBeingUpgradedConversation;

	public ConversationObject noRoomInInvConversation;

	public ConversationObject wantToMoveBuildingConversation;

	public ConversationObject beingUpgradedConvoConversation;

	public ConversationObject alreadyInDebtConversation;

	public ConversationObject wantToMoveBuildingNonLocalConversation;

	public ConversationObject wantToMovePlayerHouseConversation;

	public ConversationObject wantToMovePlayerHouseNotEnoughMoneyConversation;

	public List<GameObject> buttons = new List<GameObject>();

	public InventoryItem houseMoveDeed;

	private bool movingHouse;

	private int talkingAboutMovingBuilding = -1;

	private void Awake()
	{
		manage = this;
	}

	public void openWindow()
	{
		movingHouse = false;
		if (!stopAtStartConvoChecks())
		{
			TownManager.manage.openTownManager(TownManager.windowType.Move);
			windowOpen = true;
			moveBuildingWindow.gameObject.SetActive(value: true);
			fillListWithButtons();
			DeedManager.manage.closeDeedWindow();
			Inventory.Instance.checkIfWindowIsNeeded();
		}
	}

	public void fillListWithButtons()
	{
		for (int i = 0; i < moveableBuildings.Count; i++)
		{
			InventoryItem inventoryItem = findDeedForBuilding(moveableBuildings[i].getBuildingId());
			if (inventoryItem != null && !inventoryItem.placeable.GetComponent<DisplayPlayerHouseTiles>())
			{
				DeedButton component = Object.Instantiate(moveBuilingButtonPrefab, buttonScrollArea).GetComponent<DeedButton>();
				component.moveABuildingButton = true;
				component.setUpBuildingButton(Inventory.Instance.getInvItemId(inventoryItem), i);
				buttons.Add(component.gameObject);
			}
		}
	}

	public int findPlayersHouseNumberInMovableBuildings()
	{
		for (int i = 0; i < moveableBuildings.Count; i++)
		{
			if ((bool)WorldManager.Instance.allObjects[moveableBuildings[i].getBuildingId()].displayPlayerHouseTiles && moveableBuildings[i].checkIfBuildingIsPlayerHouse())
			{
				return i;
			}
		}
		return -1;
	}

	public void closeWindow()
	{
		moveBuildingWindow.gameObject.SetActive(value: false);
		windowOpen = false;
		for (int i = 0; i < buttons.Count; i++)
		{
			Object.Destroy(buttons[i]);
		}
		buttons.Clear();
	}

	public void addBuildingToMoveList(int xPos, int yPos)
	{
		if (findBuildingToMoveById(WorldManager.Instance.onTileMap[xPos, yPos]) == null)
		{
			moveableBuildings.Add(new MoveableBuilding(xPos, yPos));
		}
	}

	public void askToMoveBuilding(int buildingNo)
	{
		if (moveableBuildings[buildingNo].isBeingUpgraded())
		{
			TownManager.manage.closeTownManager();
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, beingUpgradedConvoConversation);
			return;
		}
		wantToMoveBuildingConversation.targetResponses[0].talkingAboutItem = findDeedForBuilding(moveableBuildings[buildingNo].getBuildingId());
		talkingAboutMovingBuilding = moveableBuildings[buildingNo].getBuildingId();
		TownManager.manage.closeTownManager();
		ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, wantToMoveBuildingConversation);
	}

	public string getTalkingAboutBuildingName()
	{
		return getBuildingName(talkingAboutMovingBuilding);
	}

	public string getBuildingName(int buildingId)
	{
		if (buildingId < -1)
		{
			return "no building";
		}
		if ((bool)WorldManager.Instance.allObjects[buildingId].displayPlayerHouseTiles)
		{
			for (int i = 0; i < HouseManager.manage.allExteriors.Count; i++)
			{
				if (WorldManager.Instance.onTileMap[HouseManager.manage.allExteriors[i].xPos, HouseManager.manage.allExteriors[i].yPos] == buildingId)
				{
					HouseExterior houseExteriorIfItExists = HouseManager.manage.getHouseExteriorIfItExists(HouseManager.manage.allExteriors[i].xPos, HouseManager.manage.allExteriors[i].yPos);
					if (houseExteriorIfItExists != null)
					{
						return houseExteriorIfItExists.houseName;
					}
				}
			}
			return "House";
		}
		if ((bool)WorldManager.Instance.allObjectSettings[buildingId].tileObjectLoadInside)
		{
			return WorldManager.Instance.allObjectSettings[buildingId].tileObjectLoadInside.GetBuildingName();
		}
		if ((bool)WorldManager.Instance.allObjects[buildingId].displayPlayerHouseTiles)
		{
			if (WorldManager.Instance.allObjects[buildingId].displayPlayerHouseTiles.isPlayerHouse)
			{
				return ConversationGenerator.generate.GetBuildingName("House");
			}
			if (!WorldManager.Instance.allObjects[buildingId].displayPlayerHouseTiles.isPlayerHouse)
			{
				return WorldManager.Instance.allObjects[buildingId].displayPlayerHouseTiles.buildingName;
			}
			return ConversationGenerator.generate.GetBuildingName("House");
		}
		return WorldManager.Instance.allObjects[buildingId].name;
	}

	public void confirmWantToMoveBuilding()
	{
		if (!movingHouse)
		{
			NetworkMapSharer instance = NetworkMapSharer.Instance;
			instance.NetworktownDebt = instance.townDebt + 25000;
			currentlyMoving = talkingAboutMovingBuilding;
			NetworkMapSharer.Instance.NetworkmovingBuilding = talkingAboutMovingBuilding;
			giveDeedForBuildingToBeMoved(talkingAboutMovingBuilding);
		}
		else
		{
			Inventory.Instance.changeWallet(-25000);
			giveDeedForHouseToMove();
			currentlyMoving = talkingAboutMovingBuilding;
			NetworkMapSharer.Instance.NetworkmovingBuilding = talkingAboutMovingBuilding;
		}
		movingHouse = false;
	}

	private InventoryItem findDeedForBuilding(int movingBuildingId)
	{
		for (int i = 0; i < DeedManager.manage.allDeeds.Count; i++)
		{
			if ((bool)DeedManager.manage.allDeeds[i].placeable && (bool)DeedManager.manage.allDeeds[i].placeable.tileObjectGrowthStages && (bool)DeedManager.manage.allDeeds[i].placeable.tileObjectGrowthStages.changeToWhenGrown && DeedManager.manage.allDeeds[i].placeable.tileObjectGrowthStages.changeToWhenGrown.tileObjectId == movingBuildingId && !DeedManager.manage.allDeeds[i].placeable.displayPlayerHouseTiles)
			{
				return DeedManager.manage.allDeeds[i];
			}
		}
		return null;
	}

	public void giveDeedForBuildingToBeMoved(int movingBuildingId)
	{
		GiftedItemWindow.gifted.addToListToBeGiven(Inventory.Instance.getInvItemId(findDeedForBuilding(movingBuildingId)), 1);
		GiftedItemWindow.gifted.openWindowAndGiveItems();
	}

	public void giveDeedForHouseToMove()
	{
		int itemId = houseMoveDeed.getItemId();
		talkingAboutMovingBuilding = moveableBuildings[findPlayersHouseNumberInMovableBuildings()].getBuildingId();
		Inventory.Instance.allItems[itemId].placeable.tileObjectGrowthStages.changeToWhenGrown = WorldManager.Instance.allObjects[talkingAboutMovingBuilding];
		GiftedItemWindow.gifted.addToListToBeGiven(itemId, 1);
		GiftedItemWindow.gifted.openWindowAndGiveItems();
	}

	public void moveBuildingToNewSite(int movingBuildingId, int newXPos, int newYPos)
	{
		for (int i = 0; i < TownManager.manage.allShopFloors.Length; i++)
		{
			if ((bool)TownManager.manage.allShopFloors[i] && TownManager.manage.allShopFloors[i].connectedToBuilingId == movingBuildingId)
			{
				Object.Destroy(TownManager.manage.allShopFloors[i]);
				TownManager.manage.allShopFloors[i] = null;
			}
		}
		findBuildingToMoveById(movingBuildingId)?.moveBuildingToNewPos(newXPos, newYPos);
	}

	private MoveableBuilding findBuildingToMoveById(int id)
	{
		for (int i = 0; i < moveableBuildings.Count; i++)
		{
			if (moveableBuildings[i].getBuildingId() == id)
			{
				return moveableBuildings[i];
			}
		}
		return null;
	}

	public void getWantToMovePlayerHouseConvo()
	{
		if (TownManager.manage.checkIfHouseIsBeingUpgraded())
		{
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, houseIsBeingUpgradedConversation);
			return;
		}
		if (currentlyMoving != -1)
		{
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, alreadyMovingABuildingConversation);
			return;
		}
		if (!Inventory.Instance.checkIfItemCanFit(0, 1))
		{
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, noRoomInInvConversation);
			return;
		}
		if (Inventory.Instance.wallet < 25000)
		{
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, wantToMovePlayerHouseNotEnoughMoneyConversation);
			return;
		}
		movingHouse = true;
		ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, wantToMovePlayerHouseConversation);
	}

	public bool stopAtStartConvoChecks()
	{
		if (NetworkMapSharer.Instance.isServer)
		{
			if (currentlyMoving != -1)
			{
				ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, alreadyMovingABuildingConversation);
				return true;
			}
			if (!Inventory.Instance.checkIfItemCanFit(0, 1))
			{
				ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, noRoomInInvConversation);
				return true;
			}
			if (NetworkMapSharer.Instance.townDebt > 0)
			{
				ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, alreadyInDebtConversation);
				return true;
			}
			return false;
		}
		ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, wantToMoveBuildingNonLocalConversation);
		return true;
	}

	public void loadCurrentlyMoving(int newCurrentlyMoving)
	{
		currentlyMoving = newCurrentlyMoving;
		NetworkMapSharer.Instance.NetworkmovingBuilding = newCurrentlyMoving;
		if ((bool)WorldManager.Instance.allObjects[currentlyMoving].displayPlayerHouseTiles && WorldManager.Instance.allObjects[currentlyMoving].displayPlayerHouseTiles.isPlayerHouse)
		{
			houseMoveDeed.placeable.tileObjectGrowthStages.changeToWhenGrown = WorldManager.Instance.allObjects[currentlyMoving];
		}
	}
}
