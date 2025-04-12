using System.Collections;
using TMPro;
using UnityEngine;

public class ChestWindow : MonoBehaviour
{
	public static ChestWindow chests;

	public GameObject inventorySlotPrefab;

	public Transform chestWindow;

	public Transform slotWindow;

	public InventorySlot[] chestSlots = new InventorySlot[24];

	private Chest currentlyOpenedChest;

	public bool chestWindowOpen;

	private int[] arrayForCheckingItems = new int[24];

	private int[] arrayForCheckingStacks = new int[24];

	public bool localChangeMade = true;

	public bool isStash;

	public VehicleStorage isVehicleStorage;

	public TextMeshProUGUI titleText;

	public GameObject pondScreen;

	public FillRecipeSlot fishRequestedSlot;

	public Transform fishRequestArea;

	public Transform fishNurseryArea;

	public GameObject noEggsMessage;

	public GameObject fishHungryMessage;

	public GameObject fishHappyMessage;

	public GameObject pondEmptyMessage;

	public TextMeshProUGUI pondEmptySign;

	public TextMeshProUGUI dropSomeAnimalsInSign;

	public TextMeshProUGUI animalStatusSign;

	public TextMeshProUGUI feedAnimalSign;

	public TextMeshProUGUI animalsLookHappy;

	public TextMeshProUGUI animalProductTitleSign;

	public TextMeshProUGUI noAnimalProductSign;

	public ASound feedFishSound;

	public InvButton quickStackButton;

	public InventoryItem swagSack;

	private void Awake()
	{
		chests = this;
	}

	private void Start()
	{
		for (int i = 0; i < 24; i++)
		{
			chestSlots[i] = Object.Instantiate(inventorySlotPrefab, slotWindow).GetComponent<InventorySlot>();
			chestSlots[i].chestSlotNo = i;
		}
	}

	public void makeALocalChange(int chestNo)
	{
		if (!isStash)
		{
			if ((bool)isVehicleStorage)
			{
				NetworkMapSharer.Instance.localChar.myPickUp.CmdChangeOneInVehicleStorage(isVehicleStorage.netId, chestNo, chestSlots[chestNo].itemNo, chestSlots[chestNo].stack);
			}
			else if (localChangeMade)
			{
				NetworkMapSharer.Instance.localChar.myPickUp.CmdChangeOneInChest(currentlyOpenedChest.xPos, currentlyOpenedChest.yPos, chestNo, chestSlots[chestNo].itemNo, chestSlots[chestNo].stack);
			}
		}
	}

	public void refreshOpenWindow(uint vehicleNetId)
	{
		if (!chestWindowOpen || !isVehicleStorage || isVehicleStorage.netId != vehicleNetId)
		{
			return;
		}
		localChangeMade = false;
		for (int i = 0; i < 24; i++)
		{
			if (chestSlots[i].itemNo != isVehicleStorage.invSlots[i] || chestSlots[i].stack != isVehicleStorage.invSlotStacks[i])
			{
				chestSlots[i].chestSlotNo = -1;
				chestSlots[i].updateSlotContentsAndRefresh(isVehicleStorage.invSlots[i], isVehicleStorage.invSlotStacks[i]);
				chestSlots[i].chestSlotNo = i;
			}
		}
		localChangeMade = true;
	}

	public void refreshOpenWindow(int xPos, int yPos, HouseDetails inside)
	{
		if (!chestWindowOpen || !ContainerManager.manage.checkIfChestIsInsideAndInThisHouse(inside, currentlyOpenedChest) || currentlyOpenedChest.xPos != xPos || currentlyOpenedChest.yPos != yPos)
		{
			return;
		}
		localChangeMade = false;
		for (int i = 0; i < 24; i++)
		{
			if (chestSlots[i].itemNo != currentlyOpenedChest.itemIds[i] || chestSlots[i].stack != currentlyOpenedChest.itemStacks[i])
			{
				chestSlots[i].chestSlotNo = -1;
				chestSlots[i].updateSlotContentsAndRefresh(currentlyOpenedChest.itemIds[i], currentlyOpenedChest.itemStacks[i]);
				chestSlots[i].chestSlotNo = i;
			}
		}
		localChangeMade = true;
	}

	public void openStashInWindow(int stashId)
	{
		isStash = true;
		isVehicleStorage = null;
		currentlyOpenedChest = ContainerManager.manage.privateStashes[stashId];
		switch (stashId)
		{
		case 0:
			titleText.text = ConversationGenerator.generate.GetJournalNameByTag("My Travel Bag");
			break;
		case 1:
			titleText.text = swagSack.getInvItemName();
			break;
		}
		CloseFishPond();
		if (currentlyOpenedChest != null)
		{
			for (int i = 0; i < 24; i++)
			{
				chestSlots[i].chestSlotNo = -1;
				chestSlots[i].updateSlotContentsAndRefresh(currentlyOpenedChest.itemIds[i], currentlyOpenedChest.itemStacks[i]);
				chestSlots[i].chestSlotNo = i;
				arrayForCheckingItems[i] = currentlyOpenedChest.itemIds[i];
				arrayForCheckingStacks[i] = currentlyOpenedChest.itemStacks[i];
			}
			chestWindow.gameObject.SetActive(value: true);
			chestWindowOpen = true;
			lockBugsAndFishFromChest();
			Inventory.Instance.invOpen = true;
			Inventory.Instance.openAndCloseInv();
		}
		Inventory.Instance.checkIfWindowIsNeeded();
		CurrencyWindows.currency.openJournal();
	}

	public void openChestInWindow(int xPos, int yPos)
	{
		isStash = false;
		isVehicleStorage = null;
		currentlyOpenedChest = ContainerManager.manage.getChestForWindow(xPos, yPos, NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails);
		bool flag = false;
		bool flag2 = false;
		if (NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails != null)
		{
			if (NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails.houseMapOnTile[xPos, yPos] > -1)
			{
				titleText.text = WorldManager.Instance.allObjectSettings[NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails.houseMapOnTile[xPos, yPos]].dropsItemOnDeath.getInvItemName();
			}
			else
			{
				titleText.text = "Container";
			}
			CloseFishPond();
		}
		else
		{
			if (WorldManager.Instance.onTileMap[xPos, yPos] > -1)
			{
				titleText.text = WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].dropsItemOnDeath.getInvItemName();
			}
			else
			{
				titleText.text = "Container";
			}
			if ((bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest && WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest.isFishPond)
			{
				SetUpForFishPond();
				flag = true;
			}
			else if ((bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest && WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest.isBugTerrarium)
			{
				SetUpForBugTerrarium();
				flag2 = true;
			}
			else
			{
				CloseFishPond();
			}
		}
		if (currentlyOpenedChest != null)
		{
			for (int i = 0; i < 24; i++)
			{
				chestSlots[i].chestSlotNo = -1;
				chestSlots[i].updateSlotContentsAndRefresh(currentlyOpenedChest.itemIds[i], currentlyOpenedChest.itemStacks[i]);
				chestSlots[i].chestSlotNo = i;
				arrayForCheckingItems[i] = currentlyOpenedChest.itemIds[i];
				arrayForCheckingStacks[i] = currentlyOpenedChest.itemStacks[i];
			}
			chestWindow.gameObject.SetActive(value: true);
			chestWindowOpen = true;
			lockBugsAndFishFromChest();
			Inventory.Instance.invOpen = true;
			Inventory.Instance.openAndCloseInv();
			if (flag || flag2)
			{
				StartCoroutine(ShowAppropriateFishPondInfo(flag2));
			}
		}
		Inventory.Instance.checkIfWindowIsNeeded();
		CurrencyWindows.currency.openJournal();
	}

	public void openVehicleStorage(VehicleStorage vehicleToOpen)
	{
		isStash = false;
		isVehicleStorage = vehicleToOpen;
		currentlyOpenedChest = null;
		titleText.text = "Vehicle Storage";
		CloseFishPond();
		if (isVehicleStorage != null)
		{
			for (int i = 0; i < 24; i++)
			{
				chestSlots[i].chestSlotNo = -1;
				chestSlots[i].updateSlotContentsAndRefresh(isVehicleStorage.invSlots[i], isVehicleStorage.invSlotStacks[i]);
				chestSlots[i].chestSlotNo = i;
				arrayForCheckingItems[i] = isVehicleStorage.invSlots[i];
				arrayForCheckingStacks[i] = isVehicleStorage.invSlots[i];
			}
			chestWindow.gameObject.SetActive(value: true);
			chestWindowOpen = true;
			lockBugsAndFishFromChest();
			Inventory.Instance.invOpen = true;
			Inventory.Instance.openAndCloseInv();
			StartCoroutine(vehicleWindowOpen());
		}
		Inventory.Instance.checkIfWindowIsNeeded();
		CurrencyWindows.currency.openJournal();
	}

	private IEnumerator vehicleWindowOpen()
	{
		while ((bool)isVehicleStorage)
		{
			yield return null;
			if (isVehicleStorage == null)
			{
				closeChestInWindow();
				MenuButtonsTop.menu.closeButtonDelay();
			}
			else if (Vector3.Distance(isVehicleStorage.transform.position, NetworkMapSharer.Instance.localChar.transform.position) > 8f)
			{
				closeChestInWindow();
				MenuButtonsTop.menu.closeButtonDelay();
			}
		}
	}

	public void autoStackIntoChest()
	{
		if (currentlyOpenedChest != null)
		{
			bool flag = false;
			if (isStash)
			{
				for (int i = 0; i < 24; i++)
				{
					currentlyOpenedChest.itemIds[i] = chests.chestSlots[i].itemNo;
					currentlyOpenedChest.itemStacks[i] = chests.chestSlots[i].stack;
				}
			}
			for (int j = 0; j < 24; j++)
			{
				if (currentlyOpenedChest.itemIds[j] != -1 && Inventory.Instance.allItems[currentlyOpenedChest.itemIds[j]].checkIfStackable())
				{
					int amountOfItemInAllSlots = Inventory.Instance.getAmountOfItemInAllSlots(currentlyOpenedChest.itemIds[j]);
					if (amountOfItemInAllSlots > 0)
					{
						chestSlots[j].updateSlotContentsAndRefresh(currentlyOpenedChest.itemIds[j], currentlyOpenedChest.itemStacks[j] + amountOfItemInAllSlots);
						Inventory.Instance.removeAmountOfItem(currentlyOpenedChest.itemIds[j], amountOfItemInAllSlots);
						flag = true;
					}
				}
				arrayForCheckingItems[j] = currentlyOpenedChest.itemIds[j];
				arrayForCheckingStacks[j] = currentlyOpenedChest.itemStacks[j];
			}
			if (!flag)
			{
				SoundManager.Instance.play2DSound(SoundManager.Instance.buttonCantPressSound);
			}
		}
		else
		{
			if (!isVehicleStorage)
			{
				return;
			}
			bool flag2 = false;
			for (int k = 0; k < 24; k++)
			{
				if (isVehicleStorage.invSlots[k] != -1 && Inventory.Instance.allItems[isVehicleStorage.invSlots[k]].checkIfStackable())
				{
					int amountOfItemInAllSlots2 = Inventory.Instance.getAmountOfItemInAllSlots(isVehicleStorage.invSlots[k]);
					if (amountOfItemInAllSlots2 > 0)
					{
						chestSlots[k].updateSlotContentsAndRefresh(isVehicleStorage.invSlots[k], isVehicleStorage.invSlotStacks[k] + amountOfItemInAllSlots2);
						Inventory.Instance.removeAmountOfItem(isVehicleStorage.invSlots[k], amountOfItemInAllSlots2);
						flag2 = true;
					}
				}
				arrayForCheckingItems[k] = isVehicleStorage.invSlots[k];
				arrayForCheckingStacks[k] = isVehicleStorage.invSlotStacks[k];
			}
			if (!flag2)
			{
				SoundManager.Instance.play2DSound(SoundManager.Instance.buttonCantPressSound);
			}
		}
	}

	public void closeChestInWindow()
	{
		if (!isStash && !isVehicleStorage && currentlyOpenedChest != null)
		{
			NetworkMapSharer.Instance.localChar.CmdCloseChest(currentlyOpenedChest.xPos, currentlyOpenedChest.yPos);
		}
		isVehicleStorage = null;
		isStash = false;
		StartCoroutine(clearClosedChestDelay());
		Inventory.Instance.invOpen = false;
		Inventory.Instance.openAndCloseInv();
		chestWindow.gameObject.SetActive(value: false);
		chestWindowOpen = false;
		unlockAllSlots();
		Inventory.Instance.checkIfWindowIsNeeded();
		CurrencyWindows.currency.closeJournal();
	}

	private IEnumerator clearClosedChestDelay()
	{
		yield return null;
		if (currentlyOpenedChest != null)
		{
			for (int i = 0; i < 24; i++)
			{
				currentlyOpenedChest.itemIds[i] = chestSlots[i].itemNo;
				currentlyOpenedChest.itemStacks[i] = chestSlots[i].stack;
			}
			currentlyOpenedChest = null;
		}
		Inventory.Instance.CheckIfBagInInventory();
	}

	public void lockBugsAndFishFromChest()
	{
		for (int i = 0; i < Inventory.Instance.invSlots.Length; i++)
		{
			if (Inventory.Instance.invSlots[i].itemNo >= 0 && ((bool)Inventory.Instance.allItems[Inventory.Instance.invSlots[i].itemNo].bug || (bool)Inventory.Instance.allItems[Inventory.Instance.invSlots[i].itemNo].fish || Inventory.Instance.allItems[Inventory.Instance.invSlots[i].itemNo].isDeed || (isStash && Inventory.Instance.invSlots[i].itemNo == swagSack.getItemId())))
			{
				Inventory.Instance.invSlots[i].disableForGive();
			}
		}
	}

	public void SetUpForFishPond()
	{
		pondEmptySign.text = ConversationGenerator.generate.GetToolTip("Tip_FishPondEmpty");
		dropSomeAnimalsInSign.text = ConversationGenerator.generate.GetToolTip("Tip_DropFishIntoPond");
		animalStatusSign.text = ConversationGenerator.generate.GetToolTip("Tip_FishStatus");
		feedAnimalSign.text = ConversationGenerator.generate.GetToolTip("Tip_FeedTheFishACritter");
		animalsLookHappy.text = ConversationGenerator.generate.GetToolTip("Tip_TheFishLookHappy");
		animalProductTitleSign.text = ConversationGenerator.generate.GetToolTip("Tip_FishNursery");
		noAnimalProductSign.text = ConversationGenerator.generate.GetToolTip("Tip_Nothinghere");
		OpenFishPond();
	}

	public void SetUpForBugTerrarium()
	{
		pondEmptySign.text = "Terrarium Empty";
		dropSomeAnimalsInSign.text = "I should release some bugs into it...";
		animalStatusSign.text = "Bug Status";
		feedAnimalSign.text = "Feed the bugs some honey?";
		ConversationGenerator.generate.GetToolTip("Tip_FeedTheFishACritter");
		animalsLookHappy.text = "The bugs are happy";
		ConversationGenerator.generate.GetToolTip("Tip_TheFishLookHappy");
		animalProductTitleSign.text = "Silk Nest";
		ConversationGenerator.generate.GetToolTip("Tip_FishNursery");
		noAnimalProductSign.text = "Nothing in here...";
		OpenBugTerrarium();
	}

	public void LockSlotsForFishPondContainer()
	{
		if (Inventory.Instance.dragSlot.itemNo == ContainerManager.manage.fishPondManager.fishRoe.getItemId())
		{
			if (!chestSlots[22].isDisabledForGive())
			{
				chestSlots[22].disableForGive();
			}
			else if (chestSlots[23].isDisabledForGive())
			{
				chestSlots[23].clearDisable();
			}
			for (int i = 0; i < Inventory.Instance.invSlots.Length; i++)
			{
				if (Inventory.Instance.invSlots[i].itemNo >= 0)
				{
					if (!Inventory.Instance.invSlots[i].isDisabledForGive() && Inventory.Instance.invSlots[i].itemNo != ContainerManager.manage.fishPondManager.fishRoe.getItemId())
					{
						Inventory.Instance.invSlots[i].disableForGive();
					}
					else if (Inventory.Instance.invSlots[i].isDisabledForGive() && Inventory.Instance.invSlots[i].itemNo == ContainerManager.manage.fishPondManager.fishRoe.getItemId())
					{
						Inventory.Instance.invSlots[i].clearDisable();
					}
				}
			}
			return;
		}
		if (Inventory.Instance.dragSlot.itemNo >= 0 && !Inventory.Instance.allItems[Inventory.Instance.dragSlot.itemNo].underwaterCreature)
		{
			if (chestSlots[22].isDisabledForGive())
			{
				chestSlots[22].clearDisable();
			}
			else if (!chestSlots[23].isDisabledForGive())
			{
				chestSlots[23].disableForGive();
			}
			return;
		}
		if (chestSlots[22].isDisabledForGive() || chestSlots[23].isDisabledForGive())
		{
			chestSlots[22].clearDisable();
			chestSlots[23].clearDisable();
		}
		for (int j = 0; j < Inventory.Instance.invSlots.Length; j++)
		{
			if (Inventory.Instance.invSlots[j].itemNo >= 0)
			{
				if (!Inventory.Instance.invSlots[j].isDisabledForGive() && !Inventory.Instance.allItems[Inventory.Instance.invSlots[j].itemNo].underwaterCreature)
				{
					Inventory.Instance.invSlots[j].disableForGive();
				}
				else if (Inventory.Instance.invSlots[j].isDisabledForGive() && (bool)Inventory.Instance.allItems[Inventory.Instance.invSlots[j].itemNo].underwaterCreature)
				{
					Inventory.Instance.invSlots[j].clearDisable();
				}
			}
		}
	}

	public void LockSlotsForBugContainer()
	{
		if (Inventory.Instance.dragSlot.itemNo == ContainerManager.manage.fishPondManager.silkItem.getItemId())
		{
			if (!chestSlots[22].isDisabledForGive())
			{
				chestSlots[22].disableForGive();
			}
			else if (chestSlots[23].isDisabledForGive())
			{
				chestSlots[23].clearDisable();
			}
			for (int i = 0; i < Inventory.Instance.invSlots.Length; i++)
			{
				if (Inventory.Instance.invSlots[i].itemNo >= 0)
				{
					if (!Inventory.Instance.invSlots[i].isDisabledForGive() && Inventory.Instance.invSlots[i].itemNo != ContainerManager.manage.fishPondManager.silkItem.getItemId())
					{
						Inventory.Instance.invSlots[i].disableForGive();
					}
					else if (Inventory.Instance.invSlots[i].isDisabledForGive() && Inventory.Instance.invSlots[i].itemNo == ContainerManager.manage.fishPondManager.silkItem.getItemId())
					{
						Inventory.Instance.invSlots[i].clearDisable();
					}
				}
			}
			return;
		}
		if (Inventory.Instance.dragSlot.itemNo >= 0 && Inventory.Instance.dragSlot.itemNo != ContainerManager.manage.fishPondManager.honeyItem.getItemId())
		{
			if (chestSlots[22].isDisabledForGive())
			{
				chestSlots[22].clearDisable();
			}
			else if (!chestSlots[23].isDisabledForGive())
			{
				chestSlots[23].disableForGive();
			}
			return;
		}
		if (chestSlots[22].isDisabledForGive() || chestSlots[23].isDisabledForGive())
		{
			chestSlots[22].clearDisable();
			chestSlots[23].clearDisable();
		}
		for (int j = 0; j < Inventory.Instance.invSlots.Length; j++)
		{
			if (Inventory.Instance.invSlots[j].itemNo >= 0)
			{
				if (!Inventory.Instance.invSlots[j].isDisabledForGive() && Inventory.Instance.invSlots[j].itemNo != ContainerManager.manage.fishPondManager.honeyItem.getItemId())
				{
					Inventory.Instance.invSlots[j].disableForGive();
				}
				else if (Inventory.Instance.invSlots[j].isDisabledForGive() && Inventory.Instance.invSlots[j].itemNo == ContainerManager.manage.fishPondManager.honeyItem.getItemId())
				{
					Inventory.Instance.invSlots[j].clearDisable();
				}
			}
		}
	}

	public IEnumerator ShowAppropriateFishPondInfo(bool isTerrarium)
	{
		bool foodIsInside = false;
		if (chestSlots[22].itemNo != -1)
		{
			foodIsInside = true;
		}
		while (chestWindowOpen)
		{
			if (!isTerrarium)
			{
				LockSlotsForFishPondContainer();
			}
			else
			{
				LockSlotsForBugContainer();
			}
			if (chestSlots[0].itemNo == -1 && chestSlots[1].itemNo == -1 && chestSlots[2].itemNo == -1 && chestSlots[3].itemNo == -1 && chestSlots[4].itemNo == -1)
			{
				if (chestSlots[22].itemNo != -1)
				{
					fishRequestArea.gameObject.SetActive(value: true);
				}
				else
				{
					fishRequestArea.gameObject.SetActive(value: false);
				}
				if (chestSlots[23].itemNo != -1)
				{
					fishNurseryArea.gameObject.SetActive(value: true);
				}
				else
				{
					fishNurseryArea.gameObject.SetActive(value: false);
				}
				fishHappyMessage.SetActive(value: false);
				fishHungryMessage.SetActive(value: false);
				noEggsMessage.SetActive(value: false);
				if (chestSlots[22].itemNo == -1 && chestSlots[23].itemNo == -1)
				{
					pondEmptyMessage.SetActive(value: true);
				}
				else
				{
					pondEmptyMessage.SetActive(value: false);
				}
			}
			else
			{
				pondEmptyMessage.SetActive(value: false);
				if (chestSlots[22].itemNo != -1)
				{
					if (!foodIsInside)
					{
						foodIsInside = true;
						NetworkMapSharer.Instance.RpcFeedFishSound(NetworkMapSharer.Instance.localChar.transform.position);
					}
					fishHappyMessage.SetActive(value: true);
					fishHungryMessage.SetActive(value: false);
					fishRequestArea.gameObject.SetActive(value: false);
				}
				else
				{
					fishHappyMessage.SetActive(value: false);
					fishHungryMessage.SetActive(value: true);
					fishRequestArea.gameObject.SetActive(value: true);
				}
				if (chestSlots[23].itemNo == -1)
				{
					fishNurseryArea.gameObject.SetActive(value: false);
					noEggsMessage.SetActive(value: true);
				}
				else
				{
					fishNurseryArea.gameObject.SetActive(value: true);
					noEggsMessage.SetActive(value: false);
				}
			}
			yield return true;
		}
	}

	private void OpenFishPond()
	{
		pondScreen.SetActive(value: true);
		chestSlots[22].transform.SetParent(fishRequestArea);
		chestSlots[23].transform.SetParent(fishNurseryArea);
		slotWindow.gameObject.SetActive(value: false);
		quickStackButton.gameObject.SetActive(value: false);
	}

	private void OpenBugTerrarium()
	{
		pondScreen.SetActive(value: true);
		chestSlots[22].transform.SetParent(fishRequestArea);
		chestSlots[23].transform.SetParent(fishNurseryArea);
		slotWindow.gameObject.SetActive(value: false);
		quickStackButton.gameObject.SetActive(value: false);
	}

	public void CloseFishPond()
	{
		pondScreen.SetActive(value: false);
		chestSlots[22].transform.SetParent(slotWindow);
		chestSlots[22].transform.SetSiblingIndex(22);
		chestSlots[23].transform.SetParent(slotWindow);
		chestSlots[23].transform.SetSiblingIndex(23);
		chestSlots[22].clearDisable();
		chestSlots[23].clearDisable();
		slotWindow.gameObject.SetActive(value: true);
		quickStackButton.gameObject.SetActive(value: true);
	}

	public void unlockAllSlots()
	{
		for (int i = 0; i < Inventory.Instance.invSlots.Length; i++)
		{
			Inventory.Instance.invSlots[i].clearDisable();
		}
	}

	public void PressQuickStackButtonWithControl()
	{
		if (quickStackButton.gameObject.activeSelf)
		{
			quickStackButton.PressButtonDelay();
		}
	}

	public int GetCurrentBagColour()
	{
		if (ContainerManager.manage.privateStashes.Count < 2)
		{
			return 0;
		}
		for (int i = 0; i < ContainerManager.manage.privateStashes[1].itemIds.Length; i++)
		{
			if (ContainerManager.manage.privateStashes[1].itemIds[i] > -1 && Inventory.Instance.allItems[ContainerManager.manage.privateStashes[1].itemIds[i]].tag == "paint")
			{
				return (int)Inventory.Instance.allItems[ContainerManager.manage.privateStashes[1].itemIds[i]].GetComponent<PaintCan>().colourId;
			}
		}
		return 0;
	}
}
