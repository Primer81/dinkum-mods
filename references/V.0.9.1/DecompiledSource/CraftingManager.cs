using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingManager : MonoBehaviour
{
	public enum CraftingMenuType
	{
		None,
		CraftingTable,
		CookingTable,
		PostOffice,
		CraftingShop,
		TrapperShop,
		Blocked,
		NickShop,
		SignWritingTable,
		RaffleBox,
		AdvancedCraftingTable,
		AdvancedCookingTable,
		KiteTable,
		SkyFestRaffleBox,
		IceCraftingTable,
		AgentCrafting
	}

	public static CraftingManager manage;

	public Transform CraftWindow;

	public Transform RecipeList;

	public WindowAnimator recipeWindowAnim;

	private RectTransform recipeListTrans;

	public RectTransform recipeMask;

	public Transform RecipeWindow;

	public Transform RecipeIngredients;

	public Transform CraftButton;

	public GameObject craftWindowPopup;

	public GameObject craftProgressionBar;

	public GameObject scrollBar;

	public Image craftBarFill;

	public GameObject completedItemWindow;

	public FillRecipeSlot completedItemIcon;

	public List<FillRecipeSlot> recipeButtons;

	public List<GameObject> currentRecipeObjects;

	public GameObject recipeButton;

	public GameObject craftsmanRecipeButton;

	public GameObject recipeSlot;

	public Text craftCostText;

	public TextMeshProUGUI craftButtonText;

	public TextMeshProUGUI craftingText;

	public int craftableItemId = -1;

	public bool craftMenuOpen;

	private Vector2 desiredPos;

	public GameObject upButton;

	public GameObject downButton;

	private InventoryItem[] craftableOnceItems;

	public InventoryItem[] deedsCraftableAtStart;

	public SnapSelectionForWindow snapBack;

	public GameObject pinRecipeButton;

	public int[] craftableRecipeIds;

	public InventoryItem repairKit;

	public GameObject[] topButtons;

	public Image[] craftableBoxColours;

	private bool craftingFromChests;

	private int tableXPos = -1;

	private int tableYPos = -1;

	public ReplacableIngredient[] allReplaceables;

	private CraftingMenuType showingRecipesFromMenu;

	private Recipe.CraftingCatagory sortingBy = Recipe.CraftingCatagory.All;

	public bool specialCraftMenu;

	private bool crafting;

	private CraftingMenuType menuTypeOpen = CraftingMenuType.CraftingTable;

	private int currentVariation = -1;

	public GameObject variationLeftButton;

	public GameObject variationRightButton;

	private Coroutine currentRefreshOnTimer;

	private void Awake()
	{
		manage = this;
		desiredPos = new Vector2(0f, -5f);
	}

	private void Start()
	{
		recipeListTrans = RecipeList.GetComponent<RectTransform>();
		int num = 0;
		List<int> list = new List<int>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].craftable)
			{
				list.Add(i);
			}
			if ((bool)Inventory.Instance.allItems[i].craftable && Inventory.Instance.allItems[i].craftable.buildOnce)
			{
				num++;
			}
		}
		craftableRecipeIds = list.ToArray();
		craftableOnceItems = new InventoryItem[num];
		num = 0;
		for (int j = 0; j < Inventory.Instance.allItems.Length; j++)
		{
			if ((bool)Inventory.Instance.allItems[j].craftable && Inventory.Instance.allItems[j].craftable.buildOnce)
			{
				craftableOnceItems[num] = Inventory.Instance.allItems[j];
				num++;
			}
		}
		InventoryItem[] array = deedsCraftableAtStart;
		foreach (InventoryItem itemToMakeAvaliable in array)
		{
			makeRecipeAvaliable(itemToMakeAvaliable);
		}
	}

	private bool checkIfCanBeenCrafted(int itemId)
	{
		_ = Inventory.Instance.allItems[itemId].craftable.buildOnce;
		return true;
	}

	public void setCraftOnlyOnceToFalse(int itemId)
	{
		for (int i = 0; i < craftableOnceItems.Length; i++)
		{
			_ = craftableOnceItems[i] == Inventory.Instance.allItems[itemId];
		}
	}

	public void makeRecipeAvaliable(InventoryItem itemToMakeAvaliable)
	{
		for (int i = 0; i < craftableOnceItems.Length; i++)
		{
			_ = craftableOnceItems[i] == itemToMakeAvaliable;
		}
	}

	public bool isRecipeAvaliable(InventoryItem itemToCheck)
	{
		for (int i = 0; i < craftableOnceItems.Length; i++)
		{
			_ = craftableOnceItems[i] == itemToCheck;
		}
		return false;
	}

	public void changeListSort(Recipe.CraftingCatagory sortBy)
	{
		if (sortingBy != sortBy)
		{
			sortingBy = sortBy;
			populateCraftList(showingRecipesFromMenu);
			Inventory.Instance.activeScrollBar.resetToTop();
			recipeListTrans.anchoredPosition = desiredPos;
			snapBack.reselectDelay();
		}
	}

	public IEnumerator startCrafting(int currentlyCrafting)
	{
		if (showingRecipesFromMenu == CraftingMenuType.CraftingShop)
		{
			CraftsmanManager.manage.askAboutCraftingItem(Inventory.Instance.allItems[currentlyCrafting]);
			openCloseCraftMenu(isMenuOpen: false);
			yield break;
		}
		if (showingRecipesFromMenu == CraftingMenuType.TrapperShop)
		{
			craftItem(currentlyCrafting);
			openCloseCraftMenu(isMenuOpen: false);
			CraftsmanManager.manage.trapperCraftingCompletedConvo.targetOpenings.talkingAboutItem = Inventory.Instance.allItems[currentlyCrafting];
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, CraftsmanManager.manage.trapperCraftingCompletedConvo);
			yield break;
		}
		if (showingRecipesFromMenu == CraftingMenuType.RaffleBox || showingRecipesFromMenu == CraftingMenuType.SkyFestRaffleBox)
		{
			craftItem(currentlyCrafting);
			pinRecipeButton.SetActive(value: false);
			yield break;
		}
		crafting = true;
		CraftButton.gameObject.SetActive(value: false);
		pinRecipeButton.SetActive(value: false);
		craftProgressionBar.SetActive(value: true);
		variationLeftButton.SetActive(value: false);
		variationRightButton.SetActive(value: false);
		if (menuTypeOpen != CraftingMenuType.CookingTable && menuTypeOpen != CraftingMenuType.AdvancedCookingTable)
		{
			NetworkMapSharer.Instance.localChar.myEquip.startCrafting();
		}
		else
		{
			NetworkMapSharer.Instance.localChar.myEquip.startCooking();
		}
		float timer = 0f;
		while (timer < 1.5f)
		{
			timer += Time.deltaTime;
			craftBarFill.fillAmount = timer / 1.5f;
			yield return null;
		}
		crafting = false;
		CraftButton.gameObject.SetActive(value: true);
		pinRecipeButton.SetActive(value: true);
		craftProgressionBar.SetActive(value: false);
		craftItem(currentlyCrafting);
		if (Inventory.Instance.allItems[currentlyCrafting].craftable.completeTaskOnCraft != 0)
		{
			DailyTaskGenerator.generate.doATask(Inventory.Instance.allItems[currentlyCrafting].craftable.completeTaskOnCraft);
		}
		if (showingRecipesFromMenu == CraftingMenuType.CookingTable)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.CookAtCookingTable);
		}
		else
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.CraftAnything);
		}
		if (Inventory.Instance.allItems[craftableItemId].craftable.altRecipes.Length != 0)
		{
			variationLeftButton.SetActive(value: true);
			variationRightButton.SetActive(value: true);
		}
		else
		{
			variationLeftButton.SetActive(value: false);
			variationRightButton.SetActive(value: false);
		}
		updateCanBeCraftedOnAllRecipeButtons();
	}

	public void checkIfNeedTopButtons()
	{
		if (craftWindowPopup.activeSelf)
		{
			topButtons[0].SetActive(value: false);
			topButtons[1].SetActive(value: false);
			topButtons[0].GetComponent<ButtonTabs>().selectFirstButtonOnEnable = false;
		}
		else if (menuTypeOpen == CraftingMenuType.CraftingTable)
		{
			topButtons[0].SetActive(value: true);
			topButtons[1].SetActive(value: true);
			topButtons[0].GetComponent<ButtonTabs>().selectFirstButtonOnEnable = true;
		}
		else
		{
			topButtons[0].SetActive(value: false);
			topButtons[1].SetActive(value: false);
		}
	}

	private void populateCraftList(CraftingMenuType listType = CraftingMenuType.CraftingTable)
	{
		menuTypeOpen = listType;
		checkIfNeedTopButtons();
		switch (listType)
		{
		case CraftingMenuType.CookingTable:
			craftButtonText.text = ConversationGenerator.generate.GetJournalNameByTag("COOK");
			craftingText.text = ConversationGenerator.generate.GetJournalNameByTag("COOKING");
			break;
		case CraftingMenuType.CraftingShop:
			craftButtonText.text = ConversationGenerator.generate.GetJournalNameByTag("COMMISSION");
			craftingText.text = ConversationGenerator.generate.GetJournalNameByTag("CRAFTING");
			break;
		case CraftingMenuType.RaffleBox:
		case CraftingMenuType.SkyFestRaffleBox:
			craftButtonText.text = ConversationGenerator.generate.GetJournalNameByTag("EXCHANGE");
			craftingText.text = ConversationGenerator.generate.GetJournalNameByTag("EXCHANGING");
			break;
		default:
			craftButtonText.text = ConversationGenerator.generate.GetJournalNameByTag("CRAFT");
			craftingText.text = ConversationGenerator.generate.GetJournalNameByTag("CRAFTING");
			break;
		}
		specialCraftMenu = true;
		GameObject original = recipeButton;
		GridLayoutGroup component = RecipeList.GetComponent<GridLayoutGroup>();
		if (listType == CraftingMenuType.CraftingShop || listType == CraftingMenuType.TrapperShop || listType == CraftingMenuType.NickShop || listType == CraftingMenuType.RaffleBox || listType == CraftingMenuType.SkyFestRaffleBox || listType == CraftingMenuType.AgentCrafting)
		{
			original = craftsmanRecipeButton;
			component.cellSize = new Vector2(688f, 70f);
			component.constraintCount = 1;
		}
		else
		{
			component.cellSize = new Vector2(76.8f, 105.600006f);
			component.constraintCount = 8;
		}
		foreach (FillRecipeSlot recipeButton in recipeButtons)
		{
			Object.Destroy(recipeButton.gameObject);
		}
		recipeButtons.Clear();
		showingRecipesFromMenu = listType;
		for (int i = 0; i < craftableRecipeIds.Length; i++)
		{
			int num = craftableRecipeIds[i];
			if (((!Inventory.Instance.allItems[num].craftable.isDeed || Inventory.Instance.allItems[num].craftable.workPlaceConditions != 0 || listType != CraftingMenuType.PostOffice) && (!CharLevelManager.manage.checkIfUnlocked(num) || Inventory.Instance.allItems[num].craftable.workPlaceConditions != 0 || listType != CraftingMenuType.CraftingTable) && (Inventory.Instance.allItems[num].craftable.isDeed || !CharLevelManager.manage.checkIfUnlocked(num) || Inventory.Instance.allItems[num].craftable.workPlaceConditions != CraftingMenuType.CraftingShop || listType != CraftingMenuType.CraftingShop) && (Inventory.Instance.allItems[num].craftable.isDeed || !CharLevelManager.manage.checkIfUnlocked(num) || Inventory.Instance.allItems[num].craftable.workPlaceConditions != CraftingMenuType.TrapperShop || listType != CraftingMenuType.TrapperShop) && (Inventory.Instance.allItems[num].craftable.isDeed || !CharLevelManager.manage.checkIfUnlocked(num) || Inventory.Instance.allItems[num].craftable.workPlaceConditions != CraftingMenuType.NickShop || listType != CraftingMenuType.NickShop) && (Inventory.Instance.allItems[num].craftable.isDeed || !CharLevelManager.manage.checkIfUnlocked(num) || Inventory.Instance.allItems[num].craftable.workPlaceConditions != CraftingMenuType.RaffleBox || listType != CraftingMenuType.RaffleBox) && (Inventory.Instance.allItems[num].craftable.isDeed || !CharLevelManager.manage.checkIfUnlocked(num) || Inventory.Instance.allItems[num].craftable.workPlaceConditions != CraftingMenuType.KiteTable || listType != CraftingMenuType.KiteTable) && (Inventory.Instance.allItems[num].craftable.isDeed || !CharLevelManager.manage.checkIfUnlocked(num) || Inventory.Instance.allItems[num].craftable.workPlaceConditions != CraftingMenuType.SkyFestRaffleBox || listType != CraftingMenuType.SkyFestRaffleBox) && (Inventory.Instance.allItems[num].craftable.isDeed || !CharLevelManager.manage.checkIfUnlocked(num) || Inventory.Instance.allItems[num].craftable.workPlaceConditions != CraftingMenuType.IceCraftingTable || listType != CraftingMenuType.IceCraftingTable) && (Inventory.Instance.allItems[num].craftable.isDeed || !CharLevelManager.manage.checkIfUnlocked(num) || Inventory.Instance.allItems[num].craftable.workPlaceConditions != CraftingMenuType.AgentCrafting || listType != CraftingMenuType.AgentCrafting) && (Inventory.Instance.allItems[num].craftable.isDeed || !CharLevelManager.manage.checkIfUnlocked(num) || Inventory.Instance.allItems[num].craftable.workPlaceConditions != listType)) || !checkIfCanBeenCrafted(num) || (sortingBy != Recipe.CraftingCatagory.All && Inventory.Instance.allItems[num].craftable.catagory != sortingBy && (Inventory.Instance.allItems[num].craftable.catagory != 0 || sortingBy != Recipe.CraftingCatagory.Misc)))
			{
				continue;
			}
			recipeButtons.Add(Object.Instantiate(original, RecipeList).GetComponent<FillRecipeSlot>());
			recipeButtons[recipeButtons.Count - 1].GetComponent<InvButton>().craftRecipeNumber = num;
			recipeButtons[recipeButtons.Count - 1].fillRecipeSlot(num);
			if (showingRecipesFromMenu == CraftingMenuType.CraftingShop || showingRecipesFromMenu == CraftingMenuType.TrapperShop || showingRecipesFromMenu == CraftingMenuType.NickShop || showingRecipesFromMenu == CraftingMenuType.RaffleBox || showingRecipesFromMenu == CraftingMenuType.SkyFestRaffleBox || showingRecipesFromMenu == CraftingMenuType.AgentCrafting)
			{
				if (showingRecipesFromMenu == CraftingMenuType.NickShop || showingRecipesFromMenu == CraftingMenuType.RaffleBox || showingRecipesFromMenu == CraftingMenuType.SkyFestRaffleBox || showingRecipesFromMenu == CraftingMenuType.AgentCrafting)
				{
					recipeButtons[recipeButtons.Count - 1].transform.Find("Price").GetComponent<TextMeshProUGUI>().text = "";
				}
				else
				{
					recipeButtons[recipeButtons.Count - 1].transform.Find("Price").GetComponent<TextMeshProUGUI>().text = "<sprite=11> " + (Inventory.Instance.allItems[num].value * 2).ToString("n0");
				}
				recipeButtons[recipeButtons.Count - 1].transform.Find("Titlebox").GetComponent<Image>().color = UIAnimationManager.manage.getSlotColour(num);
			}
		}
		sortRecipeList();
	}

	public void sortRecipeList()
	{
		recipeButtons.Sort(sortButtons);
		for (int i = 0; i < recipeButtons.Count; i++)
		{
			recipeButtons[i].transform.SetSiblingIndex(i);
		}
	}

	public void closeCraftPopup()
	{
		RecipeList.parent.gameObject.SetActive(value: true);
		craftWindowPopup.SetActive(value: false);
		checkIfNeedTopButtons();
		scrollBar.SetActive(value: true);
	}

	public int sortButtons(FillRecipeSlot a, FillRecipeSlot b)
	{
		if (a.itemInSlot.craftable.catagory < b.itemInSlot.craftable.catagory)
		{
			return -1;
		}
		if (a.itemInSlot.craftable.catagory > b.itemInSlot.craftable.catagory)
		{
			return 1;
		}
		if (a.itemInSlot.craftable.subCatagory < b.itemInSlot.craftable.subCatagory)
		{
			return -1;
		}
		if (a.itemInSlot.craftable.subCatagory > b.itemInSlot.craftable.subCatagory)
		{
			return 1;
		}
		if (a.itemInSlot.craftable.tierLevel < b.itemInSlot.craftable.tierLevel)
		{
			return -1;
		}
		if (a.itemInSlot.craftable.tierLevel > b.itemInSlot.craftable.tierLevel)
		{
			return 1;
		}
		if (a.itemInSlot.getItemId() < b.itemInSlot.getItemId())
		{
			return -1;
		}
		if (a.itemInSlot.getItemId() > b.itemInSlot.getItemId())
		{
			return 1;
		}
		return 0;
	}

	private void fillRecipeIngredients(int recipeNo, int variation)
	{
		if (variation == -1)
		{
			for (int i = 0; i < Inventory.Instance.allItems[recipeNo].craftable.itemsInRecipe.Length; i++)
			{
				int invItemId = Inventory.Instance.getInvItemId(Inventory.Instance.allItems[recipeNo].craftable.itemsInRecipe[i]);
				currentRecipeObjects.Add(Object.Instantiate(recipeSlot, RecipeIngredients));
				currentRecipeObjects[currentRecipeObjects.Count - 1].GetComponent<FillRecipeSlot>().fillRecipeSlotWithAmounts(invItemId, GetAmountOfItemsFromAllReleventSources(invItemId), Inventory.Instance.allItems[recipeNo].craftable.stackOfItemsInRecipe[i]);
			}
		}
		else
		{
			for (int j = 0; j < Inventory.Instance.allItems[recipeNo].craftable.altRecipes[variation].itemsInRecipe.Length; j++)
			{
				int invItemId2 = Inventory.Instance.getInvItemId(Inventory.Instance.allItems[recipeNo].craftable.altRecipes[variation].itemsInRecipe[j]);
				currentRecipeObjects.Add(Object.Instantiate(recipeSlot, RecipeIngredients));
				currentRecipeObjects[currentRecipeObjects.Count - 1].GetComponent<FillRecipeSlot>().fillRecipeSlotWithAmounts(invItemId2, GetAmountOfItemsFromAllReleventSources(invItemId2), Inventory.Instance.allItems[recipeNo].craftable.altRecipes[variation].stackOfItemsInRecipe[j]);
			}
		}
	}

	private void RefreshCurrentRecipeIngredients()
	{
		for (int i = 0; i < currentRecipeObjects.Count; i++)
		{
			FillRecipeSlot component = currentRecipeObjects[i].GetComponent<FillRecipeSlot>();
			component.fillRecipeSlotWithAmounts(component.itemInSlot.getItemId(), GetAmountOfItemsFromAllReleventSources(component.itemInSlot.getItemId()), component.GetAmountNeededForRefresh());
		}
	}

	public void changeVariation(int dif)
	{
		currentVariation += dif;
		if (currentVariation < -1)
		{
			currentVariation = Inventory.Instance.allItems[craftableItemId].craftable.altRecipes.Length - 1;
		}
		else if (currentVariation > Inventory.Instance.allItems[craftableItemId].craftable.altRecipes.Length - 1)
		{
			currentVariation = -1;
		}
		showRecipeForItem(craftableItemId, currentVariation, moveToAvaliableRecipe: false);
	}

	public void updateCanBeCraftedOnAllRecipeButtons()
	{
		for (int i = 0; i < recipeButtons.Count; i++)
		{
			recipeButtons[i].updateIfCanBeCrafted();
		}
	}

	public void showRecipeForItem(int recipeNo, int recipeVariation = -1, bool moveToAvaliableRecipe = true)
	{
		craftWindowPopup.SetActive(value: true);
		RecipeList.parent.gameObject.SetActive(value: false);
		scrollBar.SetActive(value: false);
		checkIfNeedTopButtons();
		currentVariation = recipeVariation;
		int num = craftableItemId;
		if (recipeNo != craftableItemId)
		{
			RecipeWindow.gameObject.SetActive(value: false);
		}
		craftableItemId = recipeNo;
		if (Inventory.Instance.allItems[craftableItemId].craftable.altRecipes.Length != 0)
		{
			variationLeftButton.SetActive(value: true);
			variationRightButton.SetActive(value: true);
		}
		else
		{
			variationLeftButton.SetActive(value: false);
			variationRightButton.SetActive(value: false);
		}
		foreach (GameObject currentRecipeObject in currentRecipeObjects)
		{
			Object.Destroy(currentRecipeObject);
		}
		currentRecipeObjects.Clear();
		if (moveToAvaliableRecipe && recipeVariation == -1 && !canBeCrafted(recipeNo))
		{
			for (int i = 0; i < Inventory.Instance.allItems[recipeNo].craftable.altRecipes.Length; i++)
			{
				currentVariation = i;
				if (canBeCrafted(recipeNo))
				{
					break;
				}
				currentVariation = recipeVariation;
			}
		}
		fillRecipeIngredients(recipeNo, currentVariation);
		RecipeWindow.gameObject.SetActive(value: true);
		Invoke("delaySizeRefresh", 0.001f);
		if (num != craftableItemId)
		{
			currentRecipeObjects[currentRecipeObjects.Count - 1].GetComponent<WindowAnimator>().enabled = true;
		}
		if (currentVariation == -1)
		{
			completedItemIcon.fillRecipeSlotWithCraftAmount(recipeNo, Inventory.Instance.allItems[recipeNo].craftable.recipeGiveThisAmount);
		}
		else
		{
			completedItemIcon.fillRecipeSlotWithCraftAmount(recipeNo, Inventory.Instance.allItems[recipeNo].craftable.altRecipes[currentVariation].recipeGiveThisAmount);
		}
		int num2 = Inventory.Instance.allItems[craftableItemId].value * 2;
		if (CharLevelManager.manage.checkIfUnlocked(craftableItemId))
		{
			num2 = 0;
		}
		if (num2 == 0)
		{
			craftCostText.gameObject.SetActive(value: false);
		}
		else
		{
			craftCostText.gameObject.SetActive(value: true);
			craftCostText.text = "$" + num2;
		}
		if (Inventory.Instance.wallet < num2)
		{
			craftCostText.GetComponent<FadeImagesAndText>().isFaded(isFaded: true);
		}
		else
		{
			craftCostText.GetComponent<FadeImagesAndText>().isFaded(isFaded: false);
		}
		completedItemWindow.SetActive(value: true);
		if (!crafting)
		{
			CraftButton.gameObject.SetActive(value: true);
			if (menuTypeOpen == CraftingMenuType.RaffleBox || menuTypeOpen == CraftingMenuType.SkyFestRaffleBox)
			{
				pinRecipeButton.SetActive(value: false);
			}
			else
			{
				pinRecipeButton.SetActive(value: true);
			}
			pinRecipeButton.transform.SetAsLastSibling();
		}
		if (!canBeCrafted(recipeNo))
		{
			CraftButton.GetComponent<Image>().color = UIAnimationManager.manage.noColor;
		}
		else
		{
			CraftButton.GetComponent<Image>().color = UIAnimationManager.manage.yesColor;
		}
		fillRecipeColourBoxes();
		QuestTracker.track.updatePinnedRecipeButton();
	}

	private void fillRecipeColourBoxes()
	{
		for (int i = 0; i < craftableBoxColours.Length; i++)
		{
			craftableBoxColours[i].color = UIAnimationManager.manage.getSlotColour(craftableItemId);
		}
	}

	private void delaySizeRefresh()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(RecipeIngredients.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(RecipeWindow.GetComponent<RectTransform>());
	}

	public bool canBeCraftedInAVariation(int recipeId)
	{
		int num = currentVariation;
		currentVariation = -1;
		if (canBeCrafted(recipeId))
		{
			currentVariation = num;
			return true;
		}
		for (int i = 0; i < Inventory.Instance.allItems[recipeId].craftable.altRecipes.Length; i++)
		{
			currentVariation = i;
			if (canBeCrafted(recipeId))
			{
				currentVariation = num;
				return true;
			}
		}
		currentVariation = num;
		return false;
	}

	public bool canBeCrafted(int itemId)
	{
		bool result = true;
		int num = Inventory.Instance.allItems[itemId].value * 2;
		if (CharLevelManager.manage.checkIfUnlocked(craftableItemId) && Inventory.Instance.allItems[itemId].craftable.workPlaceConditions != CraftingMenuType.TrapperShop)
		{
			num = 0;
		}
		if (Inventory.Instance.wallet < num)
		{
			return false;
		}
		if (currentVariation == -1 || Inventory.Instance.allItems[itemId].craftable.altRecipes.Length == 0)
		{
			for (int i = 0; i < Inventory.Instance.allItems[itemId].craftable.itemsInRecipe.Length; i++)
			{
				int invItemId = Inventory.Instance.getInvItemId(Inventory.Instance.allItems[itemId].craftable.itemsInRecipe[i]);
				int num2 = Inventory.Instance.allItems[itemId].craftable.stackOfItemsInRecipe[i];
				if (GetAmountOfItemsFromAllReleventSources(invItemId) < num2)
				{
					result = false;
					break;
				}
			}
		}
		else
		{
			for (int j = 0; j < Inventory.Instance.allItems[itemId].craftable.altRecipes[currentVariation].itemsInRecipe.Length; j++)
			{
				int invItemId2 = Inventory.Instance.getInvItemId(Inventory.Instance.allItems[itemId].craftable.altRecipes[currentVariation].itemsInRecipe[j]);
				int num3 = Inventory.Instance.allItems[itemId].craftable.altRecipes[currentVariation].stackOfItemsInRecipe[j];
				if (GetAmountOfItemsFromAllReleventSources(invItemId2) < num3)
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	public void pressCraftButton()
	{
		if (!canBeCrafted(craftableItemId))
		{
			SoundManager.Instance.play2DSound(SoundManager.Instance.buttonCantPressSound);
		}
		else if (showingRecipesFromMenu != CraftingMenuType.CraftingShop && !Inventory.Instance.checkIfItemCanFit(craftableItemId, Inventory.Instance.allItems[craftableItemId].craftable.recipeGiveThisAmount))
		{
			SoundManager.Instance.play2DSound(SoundManager.Instance.pocketsFull);
			NotificationManager.manage.createChatNotification(ConversationGenerator.generate.GetToolTip("Tip_PocketsFull"), specialTip: true);
		}
		else
		{
			StartCoroutine(startCrafting(craftableItemId));
		}
	}

	public void takeItemsForRecipe(int currentlyCrafting)
	{
		if (currentVariation == -1)
		{
			for (int i = 0; i < Inventory.Instance.allItems[currentlyCrafting].craftable.itemsInRecipe.Length; i++)
			{
				int invItemId = Inventory.Instance.getInvItemId(Inventory.Instance.allItems[currentlyCrafting].craftable.itemsInRecipe[i]);
				int amountToRemove = Inventory.Instance.allItems[currentlyCrafting].craftable.stackOfItemsInRecipe[i];
				RemoveAmountOfItemsFromAllReleventSources(invItemId, amountToRemove);
			}
		}
		else
		{
			for (int j = 0; j < Inventory.Instance.allItems[currentlyCrafting].craftable.altRecipes[currentVariation].itemsInRecipe.Length; j++)
			{
				int invItemId2 = Inventory.Instance.getInvItemId(Inventory.Instance.allItems[currentlyCrafting].craftable.altRecipes[currentVariation].itemsInRecipe[j]);
				int amountToRemove2 = Inventory.Instance.allItems[currentlyCrafting].craftable.altRecipes[currentVariation].stackOfItemsInRecipe[j];
				RemoveAmountOfItemsFromAllReleventSources(invItemId2, amountToRemove2);
			}
		}
	}

	public void craftItem(int currentlyCrafting)
	{
		int num = Inventory.Instance.allItems[currentlyCrafting].value * 2;
		if (CharLevelManager.manage.checkIfUnlocked(currentlyCrafting) && showingRecipesFromMenu != CraftingMenuType.TrapperShop)
		{
			num = 0;
		}
		if (showingRecipesFromMenu == CraftingMenuType.CraftingShop)
		{
			if ((bool)NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Craft_Workshop))
			{
				NPCManager.manage.npcStatus[NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Craft_Workshop).myId.NPCNo].moneySpentAtStore += Inventory.Instance.allItems[currentlyCrafting].value;
			}
			return;
		}
		_ = showingRecipesFromMenu;
		_ = 5;
		takeItemsForRecipe(currentlyCrafting);
		Inventory.Instance.changeWallet(-num);
		showRecipeForItem(craftableItemId, currentVariation);
		if (Inventory.Instance.allItems[currentlyCrafting].craftable.buildOnce)
		{
			setCraftOnlyOnceToFalse(currentlyCrafting);
			populateCraftList(CraftingMenuType.PostOffice);
			RecipeWindow.gameObject.SetActive(value: false);
		}
		else
		{
			foreach (FillRecipeSlot recipeButton in recipeButtons)
			{
				recipeButton.refreshRecipeSlot();
			}
		}
		if (Inventory.Instance.allItems[currentlyCrafting].hasFuel)
		{
			Inventory.Instance.addItemToInventory(currentlyCrafting, Inventory.Instance.allItems[currentlyCrafting].fuelMax);
		}
		else if (currentVariation == -1)
		{
			Inventory.Instance.addItemToInventory(currentlyCrafting, Inventory.Instance.allItems[currentlyCrafting].craftable.recipeGiveThisAmount);
		}
		else
		{
			Inventory.Instance.addItemToInventory(currentlyCrafting, Inventory.Instance.allItems[currentlyCrafting].craftable.altRecipes[currentVariation].recipeGiveThisAmount);
		}
		SoundManager.Instance.play2DSound(SoundManager.Instance.craftingComplete);
	}

	public void openCloseCraftMenuWithTableCoords(bool isMenuOpen, int tableX, int tableY, CraftingMenuType optionsType = CraftingMenuType.None)
	{
		tableXPos = tableX;
		tableYPos = tableY;
		openCloseCraftMenu(isMenuOpen, optionsType);
	}

	public void openCloseCraftMenu(bool isMenuOpen, CraftingMenuType optionsType = CraftingMenuType.None)
	{
		switch (optionsType)
		{
		case CraftingMenuType.AdvancedCraftingTable:
			craftingFromChests = true;
			optionsType = CraftingMenuType.CraftingTable;
			break;
		case CraftingMenuType.AdvancedCookingTable:
			optionsType = CraftingMenuType.CookingTable;
			craftingFromChests = true;
			break;
		default:
			craftingFromChests = false;
			break;
		}
		CraftButton.gameObject.SetActive(value: false);
		pinRecipeButton.SetActive(value: false);
		completedItemWindow.SetActive(value: false);
		craftCostText.text = "";
		craftMenuOpen = isMenuOpen;
		CraftWindow.gameObject.SetActive(isMenuOpen);
		desiredPos = new Vector2(0f, -5f);
		sortingBy = Recipe.CraftingCatagory.All;
		closeCraftPopup();
		if (!isMenuOpen && (showingRecipesFromMenu == CraftingMenuType.CraftingShop || showingRecipesFromMenu == CraftingMenuType.TrapperShop || showingRecipesFromMenu == CraftingMenuType.NickShop || showingRecipesFromMenu == CraftingMenuType.AgentCrafting))
		{
			ConversationManager.manage.CheckIfLocalPlayerWasTalkingToNPCAndSetNetworkStopTalkingAfterConversationEnds();
		}
		if (!isMenuOpen)
		{
			RecipeWindow.gameObject.SetActive(isMenuOpen);
		}
		else
		{
			populateCraftList(optionsType);
		}
		Inventory.Instance.checkIfWindowIsNeeded();
		if (isMenuOpen)
		{
			CurrencyWindows.currency.openJournal();
		}
		else
		{
			CurrencyWindows.currency.closeJournal();
		}
	}

	public void repairItemsInPockets()
	{
		StartCoroutine(delayRepair());
		NetworkMapSharer.Instance.localChar.myEquip.startCrafting();
		Inventory.Instance.removeAmountOfItem(repairKit.getItemId(), 1);
	}

	private IEnumerator delayRepair()
	{
		yield return new WaitForSeconds(1f);
		for (int i = 0; i < Inventory.Instance.invSlots.Length; i++)
		{
			if ((bool)Inventory.Instance.invSlots[i].itemInSlot && Inventory.Instance.invSlots[i].itemInSlot.hasFuel && Inventory.Instance.invSlots[i].itemInSlot.isRepairable)
			{
				Inventory.Instance.invSlots[i].updateSlotContentsAndRefresh(Inventory.Instance.invSlots[i].itemNo, Inventory.Instance.invSlots[i].itemInSlot.fuelMax);
			}
		}
		SoundManager.Instance.play2DSound(SoundManager.Instance.craftingComplete);
	}

	public bool IsAReplaceableItem(int itemId)
	{
		for (int i = 0; i < allReplaceables.Length; i++)
		{
			if (allReplaceables[i].replaceableItem.getItemId() == itemId)
			{
				return true;
			}
		}
		return false;
	}

	public int GetAmountOfItemFromChestForReplaceables(int replaceableId)
	{
		int num = GetAmountOfItemFromChestsNearby(replaceableId) + Inventory.Instance.getAmountOfItemInAllSlots(replaceableId);
		for (int i = 0; i < allReplaceables.Length; i++)
		{
			if (allReplaceables[i].replaceableItem.getItemId() == replaceableId)
			{
				for (int j = 0; j < allReplaceables[i].replaceableWith.Length; j++)
				{
					num += GetAmountOfItemFromChestsNearby(allReplaceables[i].replaceableWith[j].getItemId()) + Inventory.Instance.getAmountOfItemInAllSlots(allReplaceables[i].replaceableWith[j].getItemId());
				}
			}
		}
		return num;
	}

	public int GetAmountOfItemFromInvForReplaceables(int replaceableId)
	{
		int num = Inventory.Instance.getAmountOfItemInAllSlots(replaceableId);
		for (int i = 0; i < allReplaceables.Length; i++)
		{
			if (allReplaceables[i].replaceableItem.getItemId() == replaceableId)
			{
				for (int j = 0; j < allReplaceables[i].replaceableWith.Length; j++)
				{
					num += Inventory.Instance.getAmountOfItemInAllSlots(allReplaceables[i].replaceableWith[j].getItemId());
				}
			}
		}
		return num;
	}

	public int GetAmountOfItemsFromAllReleventSources(int itemId)
	{
		if (craftingFromChests)
		{
			return GetAmountOfItemFromChestsNearby(itemId) + Inventory.Instance.getAmountOfItemInAllSlots(itemId);
		}
		return Inventory.Instance.getAmountOfItemInAllSlots(itemId);
	}

	public void RemoveAmountOfItemsFromAllReleventSources(int itemId, int amountToRemove)
	{
		if (craftingFromChests)
		{
			HouseDetails insideHouseDetails = NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails;
			int num = Mathf.Clamp(amountToRemove - Inventory.Instance.getAmountOfItemInAllSlots(itemId), 0, amountToRemove);
			Inventory.Instance.removeAmountOfItem(itemId, amountToRemove);
			if (num > 0)
			{
				if (insideHouseDetails == null)
				{
					NetworkMapSharer.Instance.localChar.myPickUp.CmdTakeItemsFromChestInChest(itemId, num, tableXPos, tableYPos, -1, -1);
				}
				else
				{
					NetworkMapSharer.Instance.localChar.myPickUp.CmdTakeItemsFromChestInChest(itemId, num, tableXPos, tableYPos, insideHouseDetails.xPos, insideHouseDetails.yPos);
				}
			}
		}
		else
		{
			Inventory.Instance.removeAmountOfItem(itemId, amountToRemove);
		}
	}

	private IEnumerator RefreshOnTimer()
	{
		yield return null;
		yield return null;
		updateCanBeCraftedOnAllRecipeButtons();
		RefreshCurrentRecipeIngredients();
		currentRefreshOnTimer = null;
	}

	public void RefreshIfCraftingFromChest()
	{
		if (craftMenuOpen && craftingFromChests && currentRefreshOnTimer == null)
		{
			currentRefreshOnTimer = StartCoroutine(RefreshOnTimer());
		}
	}

	public int GetAmountOfItemFromChestsNearby(int itemId)
	{
		int num = 0;
		HouseDetails insideHouseDetails = NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails;
		int num2 = WorldManager.Instance.rotationMap[tableXPos, tableYPos];
		int num3 = 5;
		int num4 = 5;
		if (num2 == 1 || num2 == 3)
		{
			num3 = 6;
		}
		else
		{
			num4 = 6;
		}
		if (insideHouseDetails == null)
		{
			for (int i = tableYPos - 5; i <= tableYPos + num4; i++)
			{
				for (int j = tableXPos - 5; j <= tableXPos + num3; j++)
				{
					if (WorldManager.Instance.isPositionChest(j, i))
					{
						if (!NetworkMapSharer.Instance.isServer && ContainerManager.manage.CheckIfClientNeedsToRequestChest(j, i))
						{
							NetworkMapSharer.Instance.localChar.myPickUp.CmdRequestChestForCrafting(j, i);
						}
						num += ContainerManager.manage.GetAmountOfItemsInChestForTable(itemId, j, i);
					}
				}
			}
		}
		else
		{
			for (int k = 0; k < 25; k++)
			{
				for (int l = 0; l < 25; l++)
				{
					if (insideHouseDetails.houseMapOnTile[l, k] >= 0 && (bool)WorldManager.Instance.allObjects[insideHouseDetails.houseMapOnTile[l, k]].tileObjectChest)
					{
						if (ContainerManager.manage.CheckIfClientNeedsToRequestChest(l, k))
						{
							NetworkMapSharer.Instance.localChar.myPickUp.CmdRequestChestForCrafting(l, k);
						}
						num += ContainerManager.manage.GetAmountOfItemsInChestForTable(itemId, l, k);
					}
				}
			}
		}
		return num;
	}

	public void RemoveAmountOfItemFromChestsNearby(int itemId, int amountToRemove, int remoteTablePosX, int remoteTablePosY, int houseX, int houseY)
	{
		int num = amountToRemove;
		if (houseX == -1 && houseY == -1)
		{
			int num2 = WorldManager.Instance.rotationMap[remoteTablePosX, remoteTablePosY];
			int num3 = 5;
			int num4 = 5;
			if (num2 == 1 || num2 == 3)
			{
				num3 = 6;
			}
			else
			{
				num4 = 6;
			}
			for (int i = remoteTablePosY - 5; i <= remoteTablePosY + num4; i++)
			{
				for (int j = remoteTablePosX - 5; j <= remoteTablePosX + num3; j++)
				{
					if (WorldManager.Instance.isPositionChest(j, i))
					{
						int amountOfItemsInChestForTable = ContainerManager.manage.GetAmountOfItemsInChestForTable(itemId, j, i);
						if (amountOfItemsInChestForTable > 0)
						{
							int removeAmount = Mathf.Clamp(amountToRemove, 0, amountOfItemsInChestForTable);
							num = Mathf.Clamp(num - amountOfItemsInChestForTable, 0, amountToRemove);
							ContainerManager.manage.RemoveAmountOfItemsInChestForTable(itemId, removeAmount, j, i, -1, -1);
						}
						if (num == 0)
						{
							break;
						}
					}
				}
				if (num == 0)
				{
					break;
				}
			}
			return;
		}
		HouseDetails houseInfo = HouseManager.manage.getHouseInfo(houseX, houseY);
		for (int k = 0; k < 25; k++)
		{
			for (int l = 0; l < 25; l++)
			{
				if (houseInfo.houseMapOnTile[l, k] >= 0 && (bool)WorldManager.Instance.allObjects[houseInfo.houseMapOnTile[l, k]].tileObjectChest)
				{
					int amountOfItemsInChestForTable2 = ContainerManager.manage.GetAmountOfItemsInChestForTable(itemId, l, k);
					if (amountOfItemsInChestForTable2 > 0)
					{
						int removeAmount2 = Mathf.Clamp(amountToRemove, 0, amountOfItemsInChestForTable2);
						num = Mathf.Clamp(num - amountOfItemsInChestForTable2, 0, amountToRemove);
						MonoBehaviour.print("Removing " + itemId + " From Chests in house " + l + "," + k + "," + houseX + "," + houseY + ",");
						ContainerManager.manage.RemoveAmountOfItemsInChestForTable(itemId, removeAmount2, l, k, houseX, houseY);
					}
					if (num == 0)
					{
						break;
					}
				}
			}
		}
	}
}
