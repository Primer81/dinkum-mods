using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeedManager : MonoBehaviour
{
	public static DeedManager manage;

	public GameObject deedButtonPrefab;

	public GameObject deedWindow;

	[Header("Confirm Window --------")]
	public GameObject confirmWindow;

	public GameObject selectDeedWindow;

	public FillRecipeSlot confirmWindowHeader;

	public TextMeshProUGUI confirmWindowDeedCost;

	public bool deedWindowOpen;

	public Transform deedBuyScrollBox;

	public List<InventoryItem> allDeeds = new List<InventoryItem>();

	public List<DeedStatus> deedDetails = new List<DeedStatus>();

	private List<DeedButton> deedButtons = new List<DeedButton>();

	public FillRecipeSlot[] buildingMaterialsRequired;

	public InventoryItem[] deedsUnlockedOnStart;

	public InventoryItem mineDeed;

	private int lookingAtDeed;

	private void Awake()
	{
		manage = this;
	}

	public void Start()
	{
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if (!Inventory.Instance.allItems[i].isDeed && (!Inventory.Instance.allItems[i].craftable || !Inventory.Instance.allItems[i].craftable.isDeed))
			{
				continue;
			}
			allDeeds.Add(Inventory.Instance.allItems[i]);
			int[] array = new int[0];
			int[] requiredItemsAmount = new int[0];
			if ((bool)Inventory.Instance.allItems[i].craftable)
			{
				array = new int[Inventory.Instance.allItems[i].craftable.itemsInRecipe.Length];
				for (int j = 0; j < Inventory.Instance.allItems[i].craftable.itemsInRecipe.Length; j++)
				{
					array[j] = Inventory.Instance.getInvItemId(Inventory.Instance.allItems[i].craftable.itemsInRecipe[j]);
				}
				requiredItemsAmount = Inventory.Instance.allItems[i].craftable.stackOfItemsInRecipe;
			}
			DeedStatus item = new DeedStatus(array, requiredItemsAmount);
			deedDetails.Add(item);
		}
		unlockStartingDeeds();
		loadDeedIngredients();
	}

	public void unlockStartingDeeds()
	{
		for (int i = 0; i < deedsUnlockedOnStart.Length; i++)
		{
			unlockDeed(deedsUnlockedOnStart[i]);
		}
	}

	public void loadDeedIngredients()
	{
		for (int i = 0; i < allDeeds.Count; i++)
		{
			if ((bool)allDeeds[i].craftable)
			{
				int[] array = new int[allDeeds[i].craftable.itemsInRecipe.Length];
				for (int j = 0; j < allDeeds[i].craftable.itemsInRecipe.Length; j++)
				{
					array[j] = Inventory.Instance.getInvItemId(allDeeds[i].craftable.itemsInRecipe[j]);
				}
				deedDetails[i].fillRequireditems(array, allDeeds[i].craftable.stackOfItemsInRecipe);
			}
		}
	}

	public void openDeedWindow()
	{
		deedWindow.gameObject.SetActive(value: true);
		confirmWindow.SetActive(value: false);
		for (int i = 0; i < deedButtons.Count; i++)
		{
			Object.Destroy(deedButtons[i].gameObject);
		}
		deedButtons.Clear();
		for (int j = 0; j < allDeeds.Count; j++)
		{
			if (deedDetails[j].showInBuyList(allDeeds[j].getItemId()))
			{
				DeedButton component = Object.Instantiate(deedButtonPrefab, deedBuyScrollBox).GetComponent<DeedButton>();
				component.setUpButton(Inventory.Instance.getInvItemId(allDeeds[j]));
				deedButtons.Add(component);
			}
		}
		deedWindowOpen = true;
		TownManager.manage.openTownManager(TownManager.windowType.Deeds);
	}

	public void openConfirmDeedWindow(int deedId)
	{
		selectDeedWindow.SetActive(value: false);
		confirmWindow.SetActive(value: false);
		lookingAtDeed = deedId;
		confirmWindowHeader.fillDeedBuySlot(lookingAtDeed);
		confirmWindowDeedCost.text = "<sprite=11> " + Inventory.Instance.allItems[lookingAtDeed].value.ToString("n0");
		for (int i = 0; i < buildingMaterialsRequired.Length; i++)
		{
			if (i < Inventory.Instance.allItems[lookingAtDeed].craftable.itemsInRecipe.Length)
			{
				buildingMaterialsRequired[i].fillRecipeSlotForQuestReward(Inventory.Instance.getInvItemId(Inventory.Instance.allItems[lookingAtDeed].craftable.itemsInRecipe[i]), Inventory.Instance.allItems[lookingAtDeed].craftable.stackOfItemsInRecipe[i]);
				buildingMaterialsRequired[i].gameObject.SetActive(value: true);
			}
			else
			{
				buildingMaterialsRequired[i].gameObject.SetActive(value: false);
			}
		}
		confirmWindow.SetActive(value: true);
	}

	public void confirmBuyDeedButton()
	{
		ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, ConversationManager.manage.townConvos.confirmDeedConvo);
		TownManager.manage.closeTownManager();
	}

	public void confirmDeedInConvo()
	{
		NetworkMapSharer instance = NetworkMapSharer.Instance;
		instance.NetworktownDebt = instance.townDebt + Inventory.Instance.allItems[lookingAtDeed].value;
		GiftedItemWindow.gifted.addToListToBeGiven(lookingAtDeed, 1);
		GiftedItemWindow.gifted.openWindowAndGiveItems();
		purchaseDeed(lookingAtDeed);
	}

	public int getDeedCost()
	{
		return Inventory.Instance.allItems[lookingAtDeed].value;
	}

	public string getDeedName()
	{
		return Inventory.Instance.allItems[lookingAtDeed].getInvItemName();
	}

	public void closeDeedWindow()
	{
		if (deedWindowOpen)
		{
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, ConversationManager.manage.townConvos.closeDeedWindowConvo);
		}
		deedWindow.gameObject.SetActive(value: false);
		selectDeedWindow.SetActive(value: false);
		confirmWindow.SetActive(value: false);
		deedWindowOpen = false;
	}

	public void closeConfirmWindow()
	{
		selectDeedWindow.SetActive(value: true);
		confirmWindow.SetActive(value: false);
	}

	public void unlockDeed(InventoryItem deedToUnlock)
	{
		for (int i = 0; i < allDeeds.Count; i++)
		{
			if (allDeeds[i] == deedToUnlock)
			{
				deedDetails[i].unlocked = true;
				break;
			}
		}
	}

	public bool isDeedUnlockedAndUnbought(InventoryItem deedToUnlock)
	{
		for (int i = 0; i < allDeeds.Count; i++)
		{
			if (allDeeds[i] == deedToUnlock)
			{
				if (deedDetails[i].unlocked)
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	public void placeDeed(InventoryItem deedToUnlock)
	{
		for (int i = 0; i < allDeeds.Count && !(allDeeds[i] == deedToUnlock); i++)
		{
		}
	}

	public void purchaseDeed(int deedInvId)
	{
		for (int i = 0; i < allDeeds.Count && !(allDeeds[i] == Inventory.Instance.allItems[deedInvId]); i++)
		{
		}
	}

	public bool checkIfDeedsAvaliable()
	{
		for (int i = 0; i < allDeeds.Count; i++)
		{
			if (LicenceManager.manage.allLicences[13].getCurrentLevel() >= 1 && allDeeds[i].getItemId() == mineDeed.getItemId())
			{
				deedDetails[i].unlocked = true;
			}
			if (deedDetails[i].showInBuyList(allDeeds[i].getItemId()))
			{
				return true;
			}
		}
		return false;
	}

	public bool checkIfDeedMaterialsComplete(int xPos, int yPos)
	{
		for (int i = 0; i < allDeeds.Count; i++)
		{
			if (allDeeds[i].placeable.tileObjectId != WorldManager.Instance.onTileMap[xPos, yPos] || !allDeeds[i].craftable)
			{
				continue;
			}
			int num = 0;
			for (int j = 0; j < deedDetails[i].requiredAmount.Length; j++)
			{
				if (deedDetails[i].givenAmounts[j] == deedDetails[i].requiredAmount[j])
				{
					num++;
				}
			}
			if (num == deedDetails[i].requiredAmount.Length)
			{
				checkIfIsATeleporterAndFix(allDeeds[i].placeable.tileObjectId);
				return true;
			}
			return false;
		}
		return false;
	}

	public bool checkIfDeedMaterialsComplete(InventoryItem deedInvItem)
	{
		for (int i = 0; i < allDeeds.Count; i++)
		{
			if (!(allDeeds[i] == deedInvItem) || !allDeeds[i].craftable)
			{
				continue;
			}
			int num = 0;
			for (int j = 0; j < deedDetails[i].requiredAmount.Length; j++)
			{
				if (deedDetails[i].givenAmounts[j] == deedDetails[i].requiredAmount[j])
				{
					num++;
				}
			}
			if (num == deedDetails[i].requiredAmount.Length)
			{
				checkIfIsATeleporterAndFix(allDeeds[i].placeable.tileObjectId);
				return true;
			}
			return false;
		}
		return false;
	}

	public int[] getRequiredItemsForDeed(int xPos, int yPos)
	{
		for (int i = 0; i < allDeeds.Count; i++)
		{
			if (allDeeds[i].placeable.tileObjectId == WorldManager.Instance.onTileMap[xPos, yPos] && (bool)allDeeds[i].craftable)
			{
				return deedDetails[i].requiredItems;
			}
		}
		return new int[0];
	}

	public int[] getRequiredAmountForDeed(int xPos, int yPos)
	{
		for (int i = 0; i < allDeeds.Count; i++)
		{
			if (allDeeds[i].placeable.tileObjectId == WorldManager.Instance.onTileMap[xPos, yPos] && (bool)allDeeds[i].craftable)
			{
				return deedDetails[i].requiredAmount;
			}
		}
		return new int[0];
	}

	public int[] getItemsAlreadyGivenForDeed(int xPos, int yPos)
	{
		for (int i = 0; i < allDeeds.Count; i++)
		{
			if (allDeeds[i].placeable.tileObjectId == WorldManager.Instance.onTileMap[xPos, yPos] && (bool)allDeeds[i].craftable)
			{
				return deedDetails[i].givenAmounts;
			}
		}
		return new int[0];
	}

	public InventoryItem GetDeedForBuilding(int xPos, int yPos)
	{
		for (int i = 0; i < allDeeds.Count; i++)
		{
			if (allDeeds[i].placeable.tileObjectId == WorldManager.Instance.onTileMap[xPos, yPos] && (bool)allDeeds[i].craftable)
			{
				return allDeeds[i];
			}
		}
		return null;
	}

	public int[] getItemsAlreadyGivenForDeed(int buildingId)
	{
		for (int i = 0; i < allDeeds.Count; i++)
		{
			if (allDeeds[i].placeable.tileObjectId == buildingId && (bool)allDeeds[i].craftable)
			{
				return deedDetails[i].givenAmounts;
			}
		}
		return new int[0];
	}

	public int returnStackAndDonateItemToDeed(int itemId, int amount, int xPos, int yPos)
	{
		for (int i = 0; i < allDeeds.Count; i++)
		{
			if (allDeeds[i].placeable.tileObjectId != WorldManager.Instance.onTileMap[xPos, yPos] || !allDeeds[i].craftable)
			{
				continue;
			}
			for (int j = 0; j < deedDetails[i].requiredAmount.Length; j++)
			{
				if (deedDetails[i].requiredItems[j] == itemId && deedDetails[i].givenAmounts[j] < deedDetails[i].requiredAmount[j])
				{
					int num = Mathf.Clamp(amount, 0, deedDetails[i].requiredAmount[j] - deedDetails[i].givenAmounts[j]);
					deedDetails[i].givenAmounts[j] += num;
					amount -= num;
				}
			}
		}
		return amount;
	}

	public void fillItemsAlreadyGivenForClient(int buildingId, int[] alreadyGiven)
	{
		for (int i = 0; i < allDeeds.Count; i++)
		{
			if (allDeeds[i].placeable.tileObjectId != buildingId)
			{
				continue;
			}
			if ((bool)allDeeds[i].craftable)
			{
				int[] array = new int[allDeeds[i].craftable.itemsInRecipe.Length];
				for (int j = 0; j < allDeeds[i].craftable.itemsInRecipe.Length; j++)
				{
					array[j] = Inventory.Instance.getInvItemId(allDeeds[i].craftable.itemsInRecipe[j]);
				}
				deedDetails[i].fillRequireditems(array, allDeeds[i].craftable.stackOfItemsInRecipe);
			}
			for (int k = 0; k < deedDetails[i].requiredAmount.Length; k++)
			{
				deedDetails[i].givenAmounts[k] = alreadyGiven[k];
			}
			GiveNPC.give.updateDeedGive(buildingId);
			break;
		}
	}

	public void checkIfIsATeleporterAndFix(int itemId)
	{
		if (itemId == 288)
		{
			NetworkMapSharer.Instance.localChar.myInteract.CmdFixTeleport("north");
		}
		if (itemId == 289)
		{
			NetworkMapSharer.Instance.localChar.myInteract.CmdFixTeleport("east");
		}
		if (itemId == 290)
		{
			NetworkMapSharer.Instance.localChar.myInteract.CmdFixTeleport("south");
		}
		if (itemId == 291)
		{
			NetworkMapSharer.Instance.localChar.myInteract.CmdFixTeleport("west");
		}
	}

	public bool checkIfDeedHasBeenBought(InventoryItem deedToCheck)
	{
		for (int i = 0; i < allDeeds.Count; i++)
		{
			if (allDeeds[i] == deedToCheck)
			{
				if (CatalogueManager.manage.collectedItem[deedToCheck.getItemId()])
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	public void checkForIngredientsUpdate(int deedId)
	{
	}
}
