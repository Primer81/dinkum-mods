using UnityEngine;

public class Quest : MonoBehaviour
{
	public bool canBeCompletedInMultiplayer;

	public string QuestName;

	[TextArea]
	public string QuestDescription;

	[Header("Offered By --------------")]
	public int offeredByNpc;

	public bool townHallQuest;

	public bool guestHouseQuest;

	public bool bandstandQuest;

	public bool berkoniumQuest;

	public bool airportQuest;

	public bool attractResidentsQuest;

	public int relationshipLevelNeeded;

	public float townBeautyLevelRequired;

	public float townEcconnomyLevelRequired;

	public ConversationObject questConvo;

	public ConversationObject questAcceptedConvo;

	public ConversationObject completedConvo;

	[Header("Items Given On Quest Accepted ------")]
	public InventoryItem[] giveItemsOnAccept;

	public int[] amountGivenOnAccept;

	public InventoryItem deedUnlockedOnAccept;

	[Header("Items Given As Reward ------")]
	public InventoryItem[] rewardOnComplete;

	public int[] rewardStacksGiven;

	public InventoryItem[] unlockRecipeOnComplete;

	[Header("Buildings Required by Quest ------")]
	public TileObject[] requiredBuilding;

	public InventoryItem placeableToPlace;

	[Header("Items Required by Quest ------")]
	public InventoryItem[] requiredItems;

	public int[] requiredStacks;

	[Header("NPC Required by Quest ------")]
	public NPCDetails npcToConvinceToMoveAIn;

	[Header("Other ------")]
	public DailyTaskGenerator.genericTaskType doTaskOnComplete;

	public bool questForFood;

	public TileObject changerItem;

	public int acceptQuestOnComplete = -1;

	public bool questToUseChanger;

	public bool autoCompletesOnDate;

	public bool placeOrHaveItem;

	public bool teleporterQuest;

	public int[] dateCompleted = new int[4];

	public InventoryItem deedToApplyFor;

	public bool checkIfComplete()
	{
		if (autoCompletesOnDate)
		{
			return isPastDate();
		}
		if (placeOrHaveItem)
		{
			return checkIfHasInInvOrHasBeenPlaced();
		}
		if (npcToConvinceToMoveAIn != null)
		{
			for (int i = 0; i < NPCManager.manage.NPCDetails.Length; i++)
			{
				if (NPCManager.manage.NPCDetails[i] == npcToConvinceToMoveAIn)
				{
					return NPCManager.manage.npcStatus[i].checkIfHasMovedIn();
				}
			}
		}
		if (requiredItems.Length != 0)
		{
			return checkIfHasAllRequiredItems();
		}
		if (requiredBuilding.Length != 0)
		{
			return checkIfHasBeenPlaced();
		}
		if (questForFood)
		{
			return checkIfFoodInInv();
		}
		return false;
	}

	public bool checkIfFoodInInv()
	{
		InventorySlot[] invSlots = Inventory.Instance.invSlots;
		foreach (InventorySlot inventorySlot in invSlots)
		{
			if (inventorySlot.itemNo == -1)
			{
				continue;
			}
			if ((bool)Inventory.Instance.allItems[inventorySlot.itemNo].consumeable)
			{
				return true;
			}
			if ((bool)Inventory.Instance.allItems[inventorySlot.itemNo].itemChange)
			{
				int changerResultId = Inventory.Instance.allItems[inventorySlot.itemNo].itemChange.getChangerResultId(changerItem.tileObjectId);
				if (changerResultId != -1 && (bool)Inventory.Instance.allItems[changerResultId].consumeable)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool checkIfHasBeenPlaced()
	{
		if (requiredBuilding.Length != 0)
		{
			for (int i = 0; i < WorldManager.Instance.GetMapSize() / 10; i++)
			{
				for (int j = 0; j < WorldManager.Instance.GetMapSize() / 10; j++)
				{
					if (!WorldManager.Instance.chunkChangedMap[j, i])
					{
						continue;
					}
					for (int k = i * 10; k < i * 10 + 10; k++)
					{
						for (int l = j * 10; l < j * 10 + 10; l++)
						{
							if (WorldManager.Instance.onTileMap[l, k] == -1)
							{
								continue;
							}
							for (int m = 0; m < requiredBuilding.Length; m++)
							{
								if (requiredBuilding[m].tileObjectId == WorldManager.Instance.onTileMap[l, k])
								{
									return true;
								}
							}
						}
					}
				}
			}
		}
		return false;
	}

	public bool upgradeBaseTent()
	{
		if (requiredBuilding.Length != 0)
		{
			for (int i = 0; i < WorldManager.Instance.GetMapSize() / 10; i++)
			{
				for (int j = 0; j < WorldManager.Instance.GetMapSize() / 10; j++)
				{
					if (!WorldManager.Instance.chunkChangedMap[j, i])
					{
						continue;
					}
					for (int k = i * 10; k < i * 10 + 10; k++)
					{
						for (int l = j * 10; l < j * 10 + 10; l++)
						{
							if (WorldManager.Instance.onTileMap[l, k] == -1)
							{
								continue;
							}
							for (int m = 0; m < requiredBuilding.Length; m++)
							{
								if (requiredBuilding[m].tileObjectId == WorldManager.Instance.onTileMap[l, k])
								{
									NetworkMapSharer.Instance.RpcGiveOnTileStatus(1, l, k);
									return true;
								}
							}
						}
					}
				}
			}
		}
		return false;
	}

	public bool checkIfHasAllRequiredItems()
	{
		for (int i = 0; i < requiredItems.Length; i++)
		{
			if (Inventory.Instance.getAmountOfItemInAllSlots(Inventory.Instance.getInvItemId(requiredItems[i])) < requiredStacks[i])
			{
				return false;
			}
		}
		return true;
	}

	public bool isPastDate()
	{
		if (WorldManager.Instance.day >= dateCompleted[0])
		{
			return true;
		}
		return false;
	}

	public bool checkIfHasInInvOrHasBeenPlaced()
	{
		if (requiredItems.Length != 0 && checkIfHasAllRequiredItems())
		{
			return true;
		}
		if (requiredBuilding.Length != 0)
		{
			return checkIfHasBeenPlaced();
		}
		return false;
	}

	public string getMissionObjText()
	{
		if (attractResidentsQuest)
		{
			if (NPCManager.manage.getNoOfNPCsMovedIn() < 5)
			{
				return string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("Attract5ResidentsToIslandName", completed: false), Inventory.Instance.islandName) + " [ " + NPCManager.manage.getNoOfNPCsMovedIn() + "/5]";
			}
			if (BuildingManager.manage.currentlyMoving == requiredBuilding[0].tileObjectId)
			{
				return string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("TalkTo_Name_OnceBaseTentIsMoved", completed: false), NPCManager.manage.NPCDetails[offeredByNpc].GetNPCName());
			}
			return string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("TalkTo_Name", completed: false), NPCManager.manage.NPCDetails[offeredByNpc].GetNPCName());
		}
		if (questToUseChanger)
		{
			if (checkIfHasAllRequiredItems())
			{
				return string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("CraftCrudeFurnace", completed: true), placeableToPlace.getInvItemName()) + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("Place_ItemName", completed: true), placeableToPlace.getInvItemName()) + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("PlaceTinOreInsideAndWaitToBecome", completed: true), placeableToPlace.getInvItemName(), requiredItems[0].getInvItemName()) + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("Bring_ItemName_To_NPCName", completed: false), requiredItems[0].getInvItemName(), NPCManager.manage.NPCDetails[offeredByNpc].GetNPCName());
			}
			if (checkIfHasBeenPlaced())
			{
				return string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("CraftCrudeFurnace", completed: true), placeableToPlace.getInvItemName()) + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("Place_ItemName", completed: true), placeableToPlace.getInvItemName()) + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("PlaceTinOreInsideAndWaitToBecome", completed: false), placeableToPlace.getInvItemName(), requiredItems[0].getInvItemName()) + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("Bring_ItemName_To_NPCName", completed: false), requiredItems[0].getInvItemName(), NPCManager.manage.NPCDetails[offeredByNpc].GetNPCName());
			}
			if (Inventory.Instance.getAmountOfItemInAllSlots(Inventory.Instance.getInvItemId(placeableToPlace)) >= 1)
			{
				return string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("CraftCrudeFurnace", completed: true), placeableToPlace.getInvItemName()) + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("Place_ItemName", completed: false), placeableToPlace.getInvItemName()) + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("PlaceTinOreInsideAndWaitToBecome", completed: false), placeableToPlace.getInvItemName(), requiredItems[0].getInvItemName()) + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("Bring_ItemName_To_NPCName", completed: false), requiredItems[0].getInvItemName(), NPCManager.manage.NPCDetails[offeredByNpc].GetNPCName());
			}
			return string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("CraftCrudeFurnace", completed: false), placeableToPlace.getInvItemName()) + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("Place_ItemName", completed: false), placeableToPlace.getInvItemName()) + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("PlaceTinOreInsideAndWaitToBecome", completed: false), placeableToPlace.getInvItemName(), requiredItems[0].getInvItemName()) + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("Bring_ItemName_To_NPCName", completed: false), requiredItems[0].getInvItemName(), NPCManager.manage.NPCDetails[offeredByNpc].GetNPCName());
		}
		if (placeOrHaveItem)
		{
			if (checkIfHasInInvOrHasBeenPlaced())
			{
				return string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("BuyThe_ItemName", completed: true), requiredItems[0].getInvItemName()) + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("TalkTo_Name", completed: false), NPCManager.manage.NPCDetails[offeredByNpc].GetNPCName());
			}
			return string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("BuyThe_ItemName", completed: false), requiredItems[0].getInvItemName()) + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("TalkTo_Name", completed: false), NPCManager.manage.NPCDetails[offeredByNpc].GetNPCName());
		}
		if (autoCompletesOnDate)
		{
			if (!isPastDate())
			{
				return ConversationGenerator.generate.GetQuestTrackerText("FirstDayOptionalText") + "\n" + ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("PlaceSleepingBag", completed: false);
			}
			return string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("TalkTo_Name", completed: false), NPCManager.manage.NPCDetails[offeredByNpc].GetNPCName());
		}
		if (questForFood)
		{
			if (checkIfFoodInInv())
			{
				return ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("FindSomethingToEat", completed: true) + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("TalkTo_Name", completed: false), NPCManager.manage.NPCDetails[offeredByNpc].GetNPCName());
			}
			return ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("FindSomethingToEat", completed: false) + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("TalkTo_Name", completed: false), NPCManager.manage.NPCDetails[offeredByNpc].GetNPCName());
		}
		if (requiredItems.Length != 0)
		{
			string text = "\t";
			for (int i = 0; i < requiredItems.Length; i++)
			{
				text = text + "\n[" + Inventory.Instance.getAmountOfItemInAllSlots(Inventory.Instance.getInvItemId(requiredItems[i])) + "/" + requiredStacks[i] + "] " + requiredItems[i].getInvItemName(requiredStacks[i]);
			}
			if (checkIfHasAllRequiredItems())
			{
				return ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("CollectRequestedItems", completed: true) + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("BringItemsTo_Name", completed: false), NPCManager.manage.NPCDetails[offeredByNpc].GetNPCName());
			}
			return ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("CollectRequestedItems", completed: false) + text + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("BringItemsTo_Name", completed: false), NPCManager.manage.NPCDetails[offeredByNpc].GetNPCName());
		}
		if ((bool)deedToApplyFor)
		{
			if (npcToConvinceToMoveAIn != null)
			{
				for (int j = 0; j < NPCManager.manage.NPCDetails.Length; j++)
				{
					if (NPCManager.manage.NPCDetails[j] == npcToConvinceToMoveAIn && !NPCManager.manage.npcStatus[j].hasAskedToMoveIn)
					{
						string text2 = "";
						text2 = ((NPCManager.manage.npcStatus[j].relationshipLevel >= NPCManager.manage.NPCDetails[j].relationshipBeforeMove) ? (text2 + ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("DoFavoursForJohn", completed: true)) : (text2 + ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("DoFavoursForJohn", completed: false)));
						text2 = ((NPCManager.manage.npcStatus[j].moneySpentAtStore >= NPCManager.manage.NPCDetails[j].spendBeforeMoveIn) ? (text2 + "\n" + ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("SpendMoneyAtJohns", completed: true)) : (text2 + "\n" + ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("SpendMoneyAtJohns", completed: false)));
						return text2 + "\n" + ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("ConvinceJohnToMoveIn", completed: false);
					}
				}
			}
			if (!DeedManager.manage.checkIfDeedHasBeenBought(deedToApplyFor))
			{
				return string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("Ask_NPCName_AboutTheTownToApplyFor_ItemName", completed: false), NPCManager.manage.NPCDetails[offeredByNpc].GetNPCName(), deedToApplyFor.getInvItemName());
			}
			if (!checkIfHasBeenPlaced())
			{
				if (DeedManager.manage.checkIfDeedMaterialsComplete(deedToApplyFor))
				{
					return string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("TalkTo_Name", completed: false), NPCManager.manage.NPCDetails[6].GetNPCName());
				}
				return string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("Place_ItemName", completed: false), deedToApplyFor.getInvItemName());
			}
			if (DeedManager.manage.checkIfDeedMaterialsComplete(deedToApplyFor))
			{
				return string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("WaitFor_ItemName_Consruction", completed: false), deedToApplyFor.getInvItemName());
			}
			return ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("PlaceRequiredItemsIntoConsructionBox", completed: false);
		}
		if (requiredBuilding != null && placeableToPlace != null)
		{
			if (!checkIfHasBeenPlaced())
			{
				return string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("Place_ItemName", completed: false), placeableToPlace.getInvItemName()) + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("TalkTo_Name", completed: false), NPCManager.manage.NPCDetails[6].GetNPCName());
			}
			return string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("Place_ItemName", completed: true), placeableToPlace.getInvItemName()) + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("TalkTo_Name", completed: false), NPCManager.manage.NPCDetails[6].GetNPCName());
		}
		return "";
	}

	public string GetQuestTitleText()
	{
		string questTrackerText = ConversationGenerator.generate.GetQuestTrackerText(base.name + "_Title");
		if (questTrackerText != null && questTrackerText != "")
		{
			return questTrackerText;
		}
		return QuestName;
	}

	public string GetQuestDescription()
	{
		string questTrackerText = ConversationGenerator.generate.GetQuestTrackerText(base.name);
		if (questTrackerText != null && questTrackerText != "")
		{
			return questTrackerText;
		}
		return QuestDescription;
	}
}
