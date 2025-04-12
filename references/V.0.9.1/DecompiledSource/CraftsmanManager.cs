using UnityEngine;

public class CraftsmanManager : MonoBehaviour
{
	public static CraftsmanManager manage;

	public int currentPoints;

	public int currentLevel;

	public bool craftsmanHasBerkonium;

	public InventoryItem shinyDiscItem;

	private InventoryItem itemAskingAbout;

	public int itemCurrentlyCrafting = -1;

	public ConversationObject hasLearnedANewRecipeIconConvos;

	public ConversationObject canCraftItemConvos;

	public ConversationObject canCraftItemNoMoneyConvos;

	public ConversationObject doesNotHaveLicenceConvos;

	public ConversationObject agreeToCraftingConvo;

	public ConversationObject lookingForTechConvo;

	public ConversationObject normalWorkConvo;

	public ConversationObject currentlyCraftingConvo;

	public ConversationObject itemIsCompletedConvo;

	public ConversationObject giveItemOnCompleteConvo;

	public ConversationObject itemCompletedNoSpaceConvo;

	public ConversationObject trapperCraftingCompletedConvo;

	private void Awake()
	{
		manage = this;
	}

	public void giveCraftsmanXp()
	{
		currentPoints += Mathf.RoundToInt(GiveNPC.give.moneyOffer / 6 / shinyDiscItem.value);
		if (NetworkMapSharer.Instance.isServer)
		{
			NPCManager.manage.npcStatus[2].moneySpentAtStore += GiveNPC.give.moneyOffer;
		}
		while (currentPoints >= getPointsForNextLevel(currentLevel + 1))
		{
			currentPoints -= getPointsForNextLevel(currentLevel + 1);
			currentLevel++;
		}
	}

	public int getPointsForNextLevel(int levelToCheck)
	{
		if (levelToCheck == 1)
		{
			return 1;
		}
		if (levelToCheck <= 4)
		{
			return 2;
		}
		if (levelToCheck <= 7)
		{
			return 3;
		}
		return 4;
	}

	public void askAboutCraftingItem(InventoryItem item)
	{
		itemAskingAbout = item;
		doesNotHaveLicenceConvos.targetOpenings.talkingAboutItem = item;
		canCraftItemConvos.targetOpenings.talkingAboutItem = item;
		canCraftItemNoMoneyConvos.targetOpenings.talkingAboutItem = item;
		if (item.requiredToBuy != 0 && LicenceManager.manage.allLicences[(int)item.requiredToBuy].getCurrentLevel() < item.requiredLicenceLevel)
		{
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, doesNotHaveLicenceConvos);
		}
		else if (Inventory.Instance.wallet >= getCraftingPrice())
		{
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, canCraftItemConvos);
		}
		else
		{
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, canCraftItemNoMoneyConvos);
		}
	}

	public void agreeToCrafting()
	{
		CraftingManager.manage.takeItemsForRecipe(Inventory.Instance.getInvItemId(itemAskingAbout));
		Inventory.Instance.changeWallet(-itemAskingAbout.value * 2);
		NPCManager.manage.npcStatus[NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Craft_Workshop).myId.NPCNo].moneySpentAtStore += itemAskingAbout.value * 2;
		ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, agreeToCraftingConvo);
		NetworkMapSharer.Instance.localChar.CmdAgreeToCraftsmanCrafting();
		itemCurrentlyCrafting = Inventory.Instance.getInvItemId(itemAskingAbout);
		MailManager.manage.tomorrowsLetters.Add(new Letter(2, Letter.LetterType.CraftsmanClosedLetter, manage.itemCurrentlyCrafting, manage.getAmountOnCraft()));
		manage.itemCurrentlyCrafting = -1;
		manage.switchCrafterConvo();
	}

	public int getCraftingPrice()
	{
		return itemAskingAbout.value * 2;
	}

	public int getAmountOnCraft()
	{
		int result = 1;
		if ((bool)Inventory.Instance.allItems[itemCurrentlyCrafting].craftable)
		{
			result = Inventory.Instance.allItems[itemCurrentlyCrafting].craftable.recipeGiveThisAmount;
		}
		if (Inventory.Instance.allItems[itemCurrentlyCrafting].hasFuel)
		{
			result = Inventory.Instance.allItems[itemCurrentlyCrafting].fuelMax;
		}
		return result;
	}

	public void tryAndGiveCompletedItem()
	{
		int stackAmount = 1;
		if ((bool)Inventory.Instance.allItems[itemCurrentlyCrafting].craftable)
		{
			stackAmount = Inventory.Instance.allItems[itemCurrentlyCrafting].craftable.recipeGiveThisAmount;
		}
		if (Inventory.Instance.allItems[itemCurrentlyCrafting].hasFuel)
		{
			stackAmount = Inventory.Instance.allItems[itemCurrentlyCrafting].fuelMax;
		}
		if (Inventory.Instance.addItemToInventory(itemCurrentlyCrafting, stackAmount))
		{
			giveItemOnCompleteConvo.targetOpenings.talkingAboutItem = Inventory.Instance.allItems[itemCurrentlyCrafting];
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, giveItemOnCompleteConvo);
			itemCurrentlyCrafting = -1;
			switchCrafterConvo();
		}
		else
		{
			itemCompletedNoSpaceConvo.targetOpenings.talkingAboutItem = Inventory.Instance.allItems[itemCurrentlyCrafting];
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, itemCompletedNoSpaceConvo);
		}
	}

	public void switchCrafterConvo()
	{
		if (NetworkMapSharer.Instance.craftsmanWorking)
		{
			NPCManager.manage.NPCDetails[2].keeperConvos = currentlyCraftingConvo;
		}
		else if (NetworkMapSharer.Instance.isServer && itemCurrentlyCrafting != -1)
		{
			NPCManager.manage.NPCDetails[2].keeperConvos = itemIsCompletedConvo;
		}
		else
		{
			NPCManager.manage.NPCDetails[2].keeperConvos = normalWorkConvo;
		}
	}

	public bool craftsmanHasItemReady()
	{
		return itemCurrentlyCrafting != -1;
	}

	public void craftsmanNowHasBerkonium()
	{
		craftsmanHasBerkonium = true;
		NetworkMapSharer.Instance.NetworkcraftsmanHasBerkonium = true;
		CharLevelManager.manage.isCraftsmanRecipeUnlockedThisLevel();
	}
}
