using System.Collections.Generic;
using UnityEngine;

public class NPCRequest
{
	public enum requestType
	{
		None,
		BugRequest,
		FishRequest,
		FoodRequest,
		WoodRequest,
		PlankRequest,
		OreBarRequest,
		CookingTableRequest,
		FurnitureRequest,
		ClothingRequest,
		InventoryItemRequest,
		SellItemRequest,
		Birthday
	}

	public int specificDesiredItem = -1;

	public int desiredAmount;

	public requestType myRequestType;

	public bool generatedToday;

	public string itemFoundInLocation = "";

	public int myNPCId;

	public bool acceptedToday;

	public bool completedToday;

	private int priceToPayFor;

	private int sellPrice;

	public void RefreshAcceptedAndCompletedOnNewDay()
	{
		acceptedToday = false;
		completedToday = false;
	}

	public bool CheckIfIDidARequestToday()
	{
		return completedToday;
	}

	public bool CheckIfUnfinishedRequestToday()
	{
		if (acceptedToday && !completedToday)
		{
			return true;
		}
		return false;
	}

	public void GetNewRequest(int newNPCId, bool loadingForSave = false)
	{
		generatedToday = true;
		completedToday = false;
		acceptedToday = false;
		myNPCId = newNPCId;
		clearRequest(newNPCId, loadingForSave);
		desiredAmount = 1;
		RandomiseRequest(newNPCId);
	}

	public void clearRequest(int npcId, bool loadingFromSave = false)
	{
		specificDesiredItem = -1;
		desiredAmount = 1;
		if (!loadingFromSave)
		{
			NPCManager.manage.npcStatus[npcId].completedRequest = false;
			NPCManager.manage.npcStatus[npcId].acceptedRequest = false;
		}
		myRequestType = requestType.None;
	}

	public void completeRequest(int npcId)
	{
		completedToday = true;
		NPCManager.manage.npcStatus[npcId].completedRequest = true;
		QuestTracker.track.UnPinTaskIfCompleted(QuestTracker.typeOfTask.Request, npcId);
		clearRequest(npcId);
	}

	public void failRequest(int npcId)
	{
		completedToday = false;
		NPCManager.manage.npcStatus[npcId].completedRequest = true;
		clearRequest(npcId);
	}

	public void CheckForOtherActionsOnComplete()
	{
		if (myRequestType == requestType.InventoryItemRequest)
		{
			completedToday = true;
			acceptedToday = true;
			Inventory.Instance.removeAmountOfItem(specificDesiredItem, desiredAmount);
			GiftedItemWindow.gifted.addToListToBeGiven(Inventory.Instance.getInvItemId(Inventory.Instance.moneyItem), getDesiredPriceToPay());
			GiftedItemWindow.gifted.openWindowAndGiveItems();
		}
		else if (myRequestType == requestType.SellItemRequest)
		{
			completedToday = true;
			acceptedToday = true;
			Inventory.Instance.changeWallet(-getSellPrice());
			sellPrice = 0;
			GiftedItemWindow.gifted.addToListToBeGiven(specificDesiredItem, desiredAmount);
			GiftedItemWindow.gifted.openWindowAndGiveItems();
		}
	}

	public void acceptRequest(int npcId, bool makeNotification = true)
	{
		acceptedToday = true;
		if (makeNotification)
		{
			NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("RequestAddedToJournal"), ConversationGenerator.generate.GetNotificationText("RequestToBeCompletedEndOfDay"), SoundManager.Instance.taskAcceptedSound);
		}
		NPCManager.manage.npcStatus[npcId].acceptedRequest = true;
	}

	public string getDesiredItemName()
	{
		if (specificDesiredItem != -1)
		{
			return getDesiredItemNameByNumber(specificDesiredItem, desiredAmount);
		}
		if (myRequestType == requestType.BugRequest)
		{
			return ConversationGenerator.generate.GetQuestTrackerText("AnyBug");
		}
		if (myRequestType == requestType.FishRequest)
		{
			return ConversationGenerator.generate.GetQuestTrackerText("AnyFish");
		}
		if (myRequestType == requestType.FoodRequest)
		{
			return ConversationGenerator.generate.GetQuestTrackerText("SomethingToEat");
		}
		if (myRequestType == requestType.CookingTableRequest)
		{
			return ConversationGenerator.generate.GetQuestTrackerText("SomethingFromCookingTable");
		}
		if (myRequestType == requestType.FurnitureRequest)
		{
			return ConversationGenerator.generate.GetQuestTrackerText("AnyFurniture");
		}
		if (myRequestType == requestType.ClothingRequest)
		{
			return ConversationGenerator.generate.GetQuestTrackerText("AnyClothing");
		}
		return specificDesiredItem.ToString();
	}

	public string getDesiredItemNameByNumber(int invId, int amount)
	{
		if (amount == 1)
		{
			return Inventory.Instance.allItems[invId].getInvItemName(amount) ?? "";
		}
		return ConversationGenerator.generate.GetItemAmount(amount, Inventory.Instance.allItems[invId].getInvItemName(amount));
	}

	public bool checkIfItemMatchesRequest(int itemGivenId)
	{
		if (specificDesiredItem != -1)
		{
			if (itemGivenId == specificDesiredItem)
			{
				return true;
			}
			return false;
		}
		if ((myRequestType == requestType.BugRequest && (bool)Inventory.Instance.allItems[itemGivenId].bug) || (myRequestType == requestType.Birthday && (bool)Inventory.Instance.allItems[itemGivenId].bug))
		{
			return true;
		}
		if ((myRequestType == requestType.FishRequest && (bool)Inventory.Instance.allItems[itemGivenId].fish) || (myRequestType == requestType.Birthday && (bool)Inventory.Instance.allItems[itemGivenId].fish))
		{
			return true;
		}
		if ((myRequestType == requestType.FoodRequest && (bool)Inventory.Instance.allItems[itemGivenId].consumeable) || (myRequestType == requestType.Birthday && (bool)Inventory.Instance.allItems[itemGivenId].consumeable))
		{
			return true;
		}
		if ((myRequestType == requestType.CookingTableRequest && (bool)Inventory.Instance.allItems[itemGivenId].consumeable && (bool)Inventory.Instance.allItems[itemGivenId].craftable) || (myRequestType == requestType.Birthday && (bool)Inventory.Instance.allItems[itemGivenId].consumeable && (bool)Inventory.Instance.allItems[itemGivenId].craftable))
		{
			return true;
		}
		if ((myRequestType == requestType.FurnitureRequest && Inventory.Instance.allItems[itemGivenId].isFurniture) || (myRequestType == requestType.Birthday && Inventory.Instance.allItems[itemGivenId].isFurniture))
		{
			return true;
		}
		if ((myRequestType == requestType.ClothingRequest && (bool)Inventory.Instance.allItems[itemGivenId].equipable && Inventory.Instance.allItems[itemGivenId].equipable.cloths) || (myRequestType == requestType.Birthday && (bool)Inventory.Instance.allItems[itemGivenId].equipable && Inventory.Instance.allItems[itemGivenId].equipable.cloths))
		{
			return true;
		}
		return false;
	}

	public bool wantsCertainFishOrBug()
	{
		if ((myRequestType == requestType.BugRequest || myRequestType == requestType.FishRequest) && specificDesiredItem != -1)
		{
			return true;
		}
		return false;
	}

	public bool wantsSomethingToEat()
	{
		return myRequestType == requestType.FoodRequest;
	}

	public bool wantToSellSomething()
	{
		return myRequestType == requestType.SellItemRequest;
	}

	public bool wantsSomeLogging()
	{
		if (myRequestType == requestType.WoodRequest || myRequestType == requestType.PlankRequest)
		{
			return true;
		}
		return false;
	}

	public bool wantsSomeMining()
	{
		return myRequestType == requestType.OreBarRequest;
	}

	public bool wantsSomeFurniture()
	{
		return myRequestType == requestType.FurnitureRequest;
	}

	public bool wantsSomeClothing()
	{
		return myRequestType == requestType.ClothingRequest;
	}

	public bool isMyBirthday()
	{
		return myRequestType == requestType.Birthday;
	}

	public bool wantsAnItemInYourInv()
	{
		if (myRequestType == requestType.InventoryItemRequest)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < Inventory.Instance.invSlots.Length; i++)
			{
				if (Inventory.Instance.invSlots[i].itemNo != -1 && ((bool)Inventory.Instance.allItems[Inventory.Instance.invSlots[i].itemNo].fish || (bool)Inventory.Instance.allItems[Inventory.Instance.invSlots[i].itemNo].bug || Inventory.Instance.allItems[Inventory.Instance.invSlots[i].itemNo].isFurniture || ((bool)Inventory.Instance.allItems[Inventory.Instance.invSlots[i].itemNo].equipable && Inventory.Instance.allItems[Inventory.Instance.invSlots[i].itemNo].equipable.cloths) || ((bool)Inventory.Instance.allItems[Inventory.Instance.invSlots[i].itemNo].equipable && Inventory.Instance.allItems[Inventory.Instance.invSlots[i].itemNo].equipable.wallpaper) || ((bool)Inventory.Instance.allItems[Inventory.Instance.invSlots[i].itemNo].equipable && Inventory.Instance.allItems[Inventory.Instance.invSlots[i].itemNo].equipable.flooring) || (bool)Inventory.Instance.allItems[Inventory.Instance.invSlots[i].itemNo].consumeable || (bool)Inventory.Instance.allItems[Inventory.Instance.invSlots[i].itemNo].itemChange) && !Inventory.Instance.allItems[Inventory.Instance.invSlots[i].itemNo].isATool && !Inventory.Instance.allItems[Inventory.Instance.invSlots[i].itemNo].hasFuel && !Inventory.Instance.allItems[Inventory.Instance.invSlots[i].itemNo].hasColourVariation && Inventory.Instance.allItems[Inventory.Instance.invSlots[i].itemNo] != Inventory.Instance.moneyItem)
				{
					list.Add(i);
				}
			}
			if (list.Count > 0)
			{
				int num = list[Random.Range(0, list.Count)];
				int num2 = 0;
				while (!checkIfNPCAccepts(num) && num2 < 25)
				{
					num = list[Random.Range(0, list.Count)];
					num2++;
				}
				if (num2 < 25)
				{
					specificDesiredItem = Inventory.Instance.invSlots[num].itemNo;
					desiredAmount = Mathf.Clamp(Inventory.Instance.invSlots[num].stack, 1, 5);
					priceToPayFor = (int)((float)Inventory.Instance.allItems[specificDesiredItem].value * Random.Range(1.1f, 3.5f)) * desiredAmount;
					return true;
				}
			}
			while (myRequestType == requestType.InventoryItemRequest)
			{
				completeRequest(myNPCId);
				GetNewRequest(myNPCId);
			}
		}
		return false;
	}

	public int getDesiredPriceToPay()
	{
		return priceToPayFor;
	}

	public void setRandomFishNoAndLocation()
	{
		switch (Random.Range(0, 5))
		{
		case 0:
			specificDesiredItem = getFishFromBiomWithLowChanceOfCommon(AnimalManager.manage.northernOceanFish);
			itemFoundInLocation = UIAnimationManager.manage.GetCharacterNameTag(AnimalManager.manage.northernOceanFish.GetLocationName());
			break;
		case 1:
			specificDesiredItem = getFishFromBiomWithLowChanceOfCommon(AnimalManager.manage.southernOceanFish);
			itemFoundInLocation = UIAnimationManager.manage.GetCharacterNameTag(AnimalManager.manage.southernOceanFish.GetLocationName());
			break;
		case 2:
			specificDesiredItem = getFishFromBiomWithLowChanceOfCommon(AnimalManager.manage.riverFish);
			itemFoundInLocation = UIAnimationManager.manage.GetCharacterNameTag(AnimalManager.manage.riverFish.GetLocationName());
			break;
		case 3:
			specificDesiredItem = getFishFromBiomWithLowChanceOfCommon(AnimalManager.manage.billabongFish);
			itemFoundInLocation = UIAnimationManager.manage.GetCharacterNameTag(AnimalManager.manage.billabongFish.GetLocationName());
			break;
		default:
			specificDesiredItem = getFishFromBiomWithLowChanceOfCommon(AnimalManager.manage.mangroveFish);
			itemFoundInLocation = UIAnimationManager.manage.GetCharacterNameTag(AnimalManager.manage.mangroveFish.GetLocationName());
			break;
		}
	}

	public int getFishFromBiomWithLowChanceOfCommon(InventoryLootTableTimeWeatherMaster biome)
	{
		int i = 0;
		InventoryItem inventoryItem = biome.getInventoryItem();
		for (; i < 15; i++)
		{
			if (inventoryItem.fish.mySeason.myRarity != 0)
			{
				break;
			}
		}
		return inventoryItem.getItemId();
	}

	public int getBugFromBiomWithLowChanceOfCommon(InventoryLootTableTimeWeatherMaster biome)
	{
		int i = 0;
		InventoryItem inventoryItem = biome.getInventoryItem();
		for (; i < 15; i++)
		{
			if (inventoryItem.bug.mySeason.myRarity != 0)
			{
				break;
			}
		}
		return inventoryItem.getItemId();
	}

	public void setRandomBugNoAndLocation()
	{
		switch (Random.Range(0, 5))
		{
		case 0:
			specificDesiredItem = getBugFromBiomWithLowChanceOfCommon(AnimalManager.manage.bushlandBugs);
			itemFoundInLocation = UIAnimationManager.manage.GetCharacterNameTag(AnimalManager.manage.bushlandBugs.GetLocationName());
			break;
		case 1:
			specificDesiredItem = getBugFromBiomWithLowChanceOfCommon(AnimalManager.manage.desertBugs);
			itemFoundInLocation = UIAnimationManager.manage.GetCharacterNameTag(AnimalManager.manage.desertBugs.GetLocationName());
			break;
		case 2:
			specificDesiredItem = getBugFromBiomWithLowChanceOfCommon(AnimalManager.manage.topicalBugs);
			itemFoundInLocation = UIAnimationManager.manage.GetCharacterNameTag(AnimalManager.manage.topicalBugs.GetLocationName());
			break;
		case 3:
			specificDesiredItem = getBugFromBiomWithLowChanceOfCommon(AnimalManager.manage.plainsBugs);
			itemFoundInLocation = UIAnimationManager.manage.GetCharacterNameTag(AnimalManager.manage.plainsBugs.GetLocationName());
			break;
		default:
			specificDesiredItem = getBugFromBiomWithLowChanceOfCommon(AnimalManager.manage.pineLandBugs);
			itemFoundInLocation = UIAnimationManager.manage.GetCharacterNameTag(AnimalManager.manage.pineLandBugs.GetLocationName());
			break;
		}
	}

	public void getRandomItemToSell()
	{
		int id = specificDesiredItem;
		int amount = desiredAmount;
		RandomObjectGenerator.generate.getRandomItemForNPCToSell(out id, out amount);
		specificDesiredItem = id;
		desiredAmount = amount;
		sellPrice = (int)((float)Inventory.Instance.allItems[specificDesiredItem].value * Random.Range(2f, 3.5f) * (float)desiredAmount);
	}

	public int getSellPrice()
	{
		return sellPrice;
	}

	public int checkAmountOfItemsInInv()
	{
		return Inventory.Instance.getAmountOfItemInAllSlots(specificDesiredItem);
	}

	public string getMissionText(int npcId)
	{
		if (specificDesiredItem != -1)
		{
			string text = " [" + checkAmountOfItemsInInv() + "/" + desiredAmount + "]";
			if (checkAmountOfItemsInInv() >= desiredAmount)
			{
				return ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("Collect_ItemName", completed: true).Replace("<itemName>", getDesiredItemName()) + text + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("Bring_ItemName_To_NPCName", completed: false), getDesiredItemName(), NPCManager.manage.NPCDetails[npcId].GetNPCName());
			}
			return ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("Collect_ItemName", completed: false).Replace("<itemName>", getDesiredItemName()) + text + "\n" + string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("Bring_ItemName_To_NPCName", completed: false), getDesiredItemName(), NPCManager.manage.NPCDetails[npcId].GetNPCName());
		}
		return string.Format(ConversationGenerator.generate.GetQuestTrackerTextWithCompletedCheck("Bring_ItemName_To_NPCName", completed: false), getDesiredItemName(), NPCManager.manage.NPCDetails[npcId].GetNPCName());
	}

	public bool checkIfNPCAccepts(int itemId)
	{
		InventoryItem inventoryItem = Inventory.Instance.allItems[itemId];
		if (specificDesiredItem != -1 && itemId == specificDesiredItem)
		{
			return true;
		}
		if (inventoryItem == NPCManager.manage.NPCDetails[myNPCId].hatedFood)
		{
			return false;
		}
		if (inventoryItem == NPCManager.manage.NPCDetails[myNPCId].favouriteFood)
		{
			return true;
		}
		if ((bool)inventoryItem.consumeable)
		{
			if (inventoryItem.consumeable.isMeat && NPCManager.manage.NPCDetails[myNPCId].hatesMeat)
			{
				return false;
			}
			if (inventoryItem.consumeable.isAnimalProduct && NPCManager.manage.NPCDetails[myNPCId].hatesAnimalProducts)
			{
				return false;
			}
			if (inventoryItem.consumeable.isFruit && NPCManager.manage.NPCDetails[myNPCId].hatesFruits)
			{
				return false;
			}
			if (inventoryItem.consumeable.isVegitable && NPCManager.manage.NPCDetails[myNPCId].hatesVegitables)
			{
				return false;
			}
		}
		return true;
	}

	public void RandomiseRequest(int npcId = -1)
	{
		bool flag = false;
		if (NPCManager.manage.NPCDetails[npcId].IsTodayMyBirthday())
		{
			myRequestType = requestType.Birthday;
			NPCManager.manage.npcStatus[npcId].acceptedRequest = true;
			flag = true;
			return;
		}
		while (!flag)
		{
			int num = Random.Range(0, 16);
			if (npcId == 12)
			{
				num = 6;
			}
			if (num <= 2)
			{
				myRequestType = requestType.BugRequest;
				flag = true;
				setRandomBugNoAndLocation();
				continue;
			}
			if (num <= 4 && LicenceManager.manage.allLicences[3].getCurrentLevel() >= 1)
			{
				myRequestType = requestType.FishRequest;
				flag = true;
				setRandomFishNoAndLocation();
				continue;
			}
			if (num <= 6)
			{
				myRequestType = requestType.FoodRequest;
				if (npcId == 12)
				{
					specificDesiredItem = RequestItemGenerator.request.getRandomCookedDish();
				}
				else if (Random.Range(0, 11) < 5 && !NPCManager.manage.NPCDetails[myNPCId].hatesMeat && LicenceManager.manage.allLicences[4].getCurrentLevel() >= 1)
				{
					specificDesiredItem = RequestItemGenerator.request.getRandomMeatInt();
					while ((bool)NPCManager.manage.NPCDetails[myNPCId].hatedFood && specificDesiredItem == Inventory.Instance.getInvItemId(NPCManager.manage.NPCDetails[myNPCId].hatedFood))
					{
						specificDesiredItem = RequestItemGenerator.request.getRandomMeatInt();
					}
				}
				else if (Random.Range(0, 11) < 5 && !NPCManager.manage.NPCDetails[myNPCId].hatesAnimalProducts && LicenceManager.manage.allLicences[9].getCurrentLevel() >= 1)
				{
					specificDesiredItem = RequestItemGenerator.request.getRandomAnimalProduct();
					while ((bool)NPCManager.manage.NPCDetails[myNPCId].hatedFood && specificDesiredItem == Inventory.Instance.getInvItemId(NPCManager.manage.NPCDetails[myNPCId].hatedFood))
					{
						specificDesiredItem = RequestItemGenerator.request.getRandomAnimalProduct();
					}
				}
				else if (Random.Range(0, 11) < 5 && !NPCManager.manage.NPCDetails[myNPCId].hatesFruits)
				{
					specificDesiredItem = RequestItemGenerator.request.getRandomFruit();
					while ((bool)NPCManager.manage.NPCDetails[myNPCId].hatedFood && specificDesiredItem == Inventory.Instance.getInvItemId(NPCManager.manage.NPCDetails[myNPCId].hatedFood))
					{
						specificDesiredItem = RequestItemGenerator.request.getRandomFruit();
					}
				}
				flag = true;
				continue;
			}
			switch (num)
			{
			case 7:
				myRequestType = requestType.InventoryItemRequest;
				flag = true;
				continue;
			case 8:
				if ((WorldManager.Instance.year != 1 || WorldManager.Instance.month > 2) && Random.Range(0, 11) <= 5)
				{
					myRequestType = requestType.FurnitureRequest;
					flag = true;
				}
				continue;
			case 9:
				if ((WorldManager.Instance.year != 1 || WorldManager.Instance.month != 1) && Random.Range(0, 11) <= 5)
				{
					myRequestType = requestType.ClothingRequest;
					flag = true;
				}
				continue;
			case 10:
				if (Random.Range(0, 4) == 2)
				{
					myRequestType = requestType.InventoryItemRequest;
					flag = true;
					continue;
				}
				break;
			}
			if (num == 11 && (float)NPCManager.manage.npcStatus[myNPCId].relationshipLevel < 25f && LicenceManager.manage.allLicences[2].hasALevelOneOrHigher())
			{
				myRequestType = requestType.WoodRequest;
				desiredAmount = Random.Range(2, 7);
				specificDesiredItem = RequestItemGenerator.request.getRandomWood();
				flag = true;
			}
			else if (num == 12 && (float)NPCManager.manage.npcStatus[myNPCId].relationshipLevel < 25f && LicenceManager.manage.allLicences[2].hasALevelOneOrHigher())
			{
				myRequestType = requestType.PlankRequest;
				desiredAmount = Random.Range(2, 7);
				specificDesiredItem = RequestItemGenerator.request.getRandomPlank();
				flag = true;
			}
			else if (num == 13 && (float)NPCManager.manage.npcStatus[myNPCId].relationshipLevel < 35f && LicenceManager.manage.allLicences[1].hasALevelOneOrHigher())
			{
				myRequestType = requestType.OreBarRequest;
				desiredAmount = Random.Range(1, 3);
				specificDesiredItem = RequestItemGenerator.request.getRandomOreBar();
				flag = true;
			}
			else if (num >= 15)
			{
				myRequestType = requestType.SellItemRequest;
				getRandomItemToSell();
				flag = true;
			}
		}
	}
}
