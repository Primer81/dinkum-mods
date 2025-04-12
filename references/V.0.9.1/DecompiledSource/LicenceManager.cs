using System;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LicenceManager : MonoBehaviour
{
	public enum LicenceTypes
	{
		None,
		Mining,
		Logging,
		Fishing,
		Hunting,
		LandScaping,
		MetalDetecting,
		Vehicle,
		Commerce,
		AnimalHandling,
		Bridge,
		Farming,
		Cargo,
		DeepMining,
		ToolBelt,
		AnimalTrapping,
		Excavation,
		Irrigation,
		AgriculturalVehicle,
		SignWriting,
		WaterBed,
		Brewing,
		AnimalProccessing,
		FishFarming
	}

	public enum LicenceSortingGroup
	{
		None,
		Mining,
		Logging,
		Fishing,
		Digging,
		MetalDetecting,
		Hunting,
		Farming,
		AnimalFarming,
		Inventory,
		Building,
		Vechile,
		Bonus
	}

	public static LicenceManager manage;

	public GameObject licenceButtonPrefab;

	public Licence[] allLicences;

	public Sprite[] licenceIcons;

	public Color[] licenceColours;

	public bool windowOpen;

	public GameObject licenceWindow;

	public GameObject confirmWindow;

	public GameObject licenceListWindow;

	public GameObject completedLicenceButton;

	public Transform licenceButtonParent;

	public Image licenceIcon;

	public TextMeshProUGUI confirmWindowTitle;

	public TextMeshProUGUI confirmWindowLevel;

	public TextMeshProUGUI confirmWindowDescription;

	public TextMeshProUGUI confirmWindowCost;

	public InvButton confirmWindowConfirmButton;

	private List<LicenceButton> licenceButtons = new List<LicenceButton>();

	private List<LicenceButton> journalButtons = new List<LicenceButton>();

	[Header("Journal Details")]
	public Transform journalWindow;

	public GameObject journalButtonPrefab;

	private Licence licenceToConfirm;

	private void Awake()
	{
		manage = this;
		allLicences = new Licence[Enum.GetNames(typeof(LicenceTypes)).Length];
	}

	private void Start()
	{
		for (int i = 1; i < allLicences.Length; i++)
		{
			allLicences[i] = new Licence((LicenceTypes)i);
			LicenceButton component = UnityEngine.Object.Instantiate(licenceButtonPrefab, licenceButtonParent).GetComponent<LicenceButton>();
			component.fillButton(i);
			licenceButtons.Add(component);
			LicenceButton component2 = UnityEngine.Object.Instantiate(journalButtonPrefab, journalWindow).GetComponent<LicenceButton>();
			component2.fillDetailsForJournal(i);
			journalButtons.Add(component2);
		}
		setLicenceLevelsAndPrice();
		for (int j = 0; j < licenceButtons.Count; j++)
		{
			licenceButtons[j].updateButton();
			journalButtons[j].updateJournalButton();
		}
		sortLicenceList(licenceButtons);
		sortLicenceList(journalButtons);
		LocalizationManager.OnLocalizeEvent += OnChangeLanguage;
	}

	private void OnDestroy()
	{
		LocalizationManager.OnLocalizeEvent -= OnChangeLanguage;
	}

	private void OnChangeLanguage()
	{
		for (int i = 1; i < licenceButtons.Count; i++)
		{
			licenceButtons[i].updateButton();
		}
	}

	public void setLicenceLevelsAndPrice()
	{
		allLicences[1].connectToSkillLevel(CharLevelManager.SkillTypes.Mining, 10);
		allLicences[11].connectToSkillLevel(CharLevelManager.SkillTypes.Farming, 10);
		allLicences[3].connectToSkillLevel(CharLevelManager.SkillTypes.Fishing, 5);
		allLicences[2].connectToSkillLevel(CharLevelManager.SkillTypes.Foraging, 10);
		allLicences[4].connectToSkillLevel(CharLevelManager.SkillTypes.Hunting, 5);
		allLicences[16].isUnlocked = true;
		allLicences[16].setLevelCost(500);
		allLicences[16].maxLevel = 1;
		allLicences[17].setLevelCost(1000);
		allLicences[17].maxLevel = 2;
		allLicences[18].setLevelCost(2000, 0);
		allLicences[18].maxLevel = 3;
		allLicences[5].setLevelCost(250, 1);
		allLicences[5].maxLevel = 2;
		allLicences[19].setLevelCost(500, 1);
		allLicences[19].maxLevel = 2;
		allLicences[6].maxLevel = 2;
		allLicences[6].setLevelCost(500);
		allLicences[8].setLevelCost(750);
		allLicences[13].setLevelCost(3500);
		allLicences[13].maxLevel = 1;
		allLicences[15].maxLevel = 2;
		allLicences[15].setLevelCost(500, 1);
		allLicences[12].setLevelCost(500);
		allLicences[8].setLevelCost(750);
		allLicences[7].setLevelCost(1200, 1);
		allLicences[7].isUnlocked = true;
		allLicences[21].maxLevel = 2;
		allLicences[21].setLevelCost(500);
		allLicences[20].maxLevel = 1;
		allLicences[20].setLevelCost(1000);
		allLicences[22].maxLevel = 2;
		allLicences[22].setLevelCost(500, 1);
		allLicences[23].isUnlocked = false;
		allLicences[23].maxLevel = 1;
		allLicences[23].setLevelCost(2500, 1);
		allLicences[1].sortingNumber = 1;
		allLicences[2].sortingNumber = 2;
		allLicences[3].sortingNumber = 3;
		allLicences[23].sortingNumber = 3;
		allLicences[4].sortingNumber = 6;
		allLicences[5].sortingNumber = 10;
		allLicences[19].sortingNumber = 10;
		allLicences[6].sortingNumber = 5;
		allLicences[7].sortingNumber = 11;
		allLicences[8].sortingNumber = 12;
		allLicences[9].sortingNumber = 8;
		allLicences[10].sortingNumber = 10;
		allLicences[11].sortingNumber = 7;
		allLicences[12].sortingNumber = 9;
		allLicences[13].sortingNumber = 1;
		allLicences[14].sortingNumber = 9;
		allLicences[15].sortingNumber = 6;
		allLicences[16].sortingNumber = 4;
		allLicences[17].sortingNumber = 7;
		allLicences[18].sortingNumber = 11;
		allLicences[22].sortingNumber = 8;
		allLicences[21].sortingNumber = 12;
		allLicences[20].sortingNumber = 10;
	}

	public void checkAllLicenceRewardsOnLoad()
	{
		for (int i = 0; i < allLicences.Length; i++)
		{
			checkForUnlocksOnLevelUp(allLicences[i], loadCheck: true);
		}
	}

	public void sortLicenceList(List<LicenceButton> listToSort)
	{
		listToSort.Sort(sortButtons);
		for (int i = 0; i < listToSort.Count; i++)
		{
			listToSort[i].transform.SetSiblingIndex(i);
		}
	}

	public void sortAndMoveCompletedLicencesToBottom(List<LicenceButton> listToSort)
	{
		listToSort.Sort(sortAndMoveCompletedLicencesToBottom);
		for (int i = 0; i < listToSort.Count; i++)
		{
			listToSort[i].transform.SetSiblingIndex(i);
		}
		int num = 0;
		bool active = false;
		for (int j = 0; j < listToSort.Count; j++)
		{
			if (allLicences[listToSort[j].myLicenceId].isMaxLevelAndCompleted())
			{
				num++;
			}
			else
			{
				active = true;
			}
		}
		completedLicenceButton.SetActive(active);
		completedLicenceButton.transform.SetSiblingIndex(licenceButtonParent.childCount - num - 2);
	}

	public int sortButtons(LicenceButton a, LicenceButton b)
	{
		if (allLicences[a.myLicenceId].sortingNumber < allLicences[b.myLicenceId].sortingNumber)
		{
			return -1;
		}
		if (allLicences[a.myLicenceId].sortingNumber > allLicences[b.myLicenceId].sortingNumber)
		{
			return 1;
		}
		if (a.myLicenceId < b.myLicenceId)
		{
			return -1;
		}
		if (a.myLicenceId > b.myLicenceId)
		{
			return 1;
		}
		return 0;
	}

	public int sortAndMoveCompletedLicencesToBottom(LicenceButton a, LicenceButton b)
	{
		if (allLicences[a.myLicenceId].isMaxLevelAndCompleted() && !allLicences[b.myLicenceId].isMaxLevelAndCompleted())
		{
			return 1;
		}
		if (!allLicences[a.myLicenceId].isMaxLevelAndCompleted() && allLicences[b.myLicenceId].isMaxLevelAndCompleted())
		{
			return -1;
		}
		if (allLicences[a.myLicenceId].sortingNumber < allLicences[b.myLicenceId].sortingNumber)
		{
			return -1;
		}
		if (allLicences[a.myLicenceId].sortingNumber > allLicences[b.myLicenceId].sortingNumber)
		{
			return 1;
		}
		if (a.myLicenceId < b.myLicenceId)
		{
			return -1;
		}
		if (a.myLicenceId > b.myLicenceId)
		{
			return 1;
		}
		return 0;
	}

	public void openLicenceWindow()
	{
		for (int i = 0; i < licenceButtons.Count; i++)
		{
			licenceButtons[i].updateButton();
		}
		closeConfirmWindow();
		windowOpen = true;
		licenceWindow.SetActive(value: true);
		confirmWindow.gameObject.SetActive(value: false);
		MenuButtonsTop.menu.closed = false;
		Inventory.Instance.checkIfWindowIsNeeded();
		CurrencyWindows.currency.openJournal();
		sortAndMoveCompletedLicencesToBottom(licenceButtons);
	}

	public void refreshCharacterJournalTab()
	{
		for (int i = 0; i < licenceButtons.Count; i++)
		{
			journalButtons[i].updateJournalButton();
		}
	}

	public void closeLicenceWindow()
	{
		windowOpen = false;
		licenceWindow.SetActive(value: false);
		confirmWindow.gameObject.SetActive(value: false);
		Inventory.Instance.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closeButtonDelay();
		CurrencyWindows.currency.closeJournal();
		refreshCharacterJournalTab();
		ConversationManager.manage.CheckIfLocalPlayerWasTalkingToNPCAndSetNetworkStopTalkingAfterConversationEnds();
	}

	public void openConfirmWindow(LicenceTypes type)
	{
		confirmWindow.gameObject.SetActive(value: false);
		licenceIcon.sprite = licenceIcons[(int)type];
		licenceToConfirm = allLicences[(int)type];
		confirmWindowTitle.text = getLicenceName(type);
		confirmWindowLevel.text = string.Format(GetLicenceStatusDesc("LicenceLevel"), allLicences[(int)type].getCurrentLevel() + 1);
		confirmWindowDescription.text = getLicenceLevelDescription(type, allLicences[(int)type].getCurrentLevel() + 1);
		confirmWindowCost.text = "<sprite=15> " + allLicences[(int)type].getNextLevelPrice().ToString("n0");
		if (allLicences[(int)type].getCurrentLevel() == allLicences[(int)type].getCurrentMaxLevel())
		{
			confirmWindowConfirmButton.gameObject.SetActive(value: false);
			confirmWindowConfirmButton.enabled = false;
			confirmWindowCost.text = "";
			if (allLicences[(int)type].getCurrentLevel() == allLicences[(int)type].getMaxLevel())
			{
				confirmWindowLevel.text = string.Format(GetLicenceStatusDesc("LicenceLevel"), allLicences[(int)type].getCurrentLevel());
				confirmWindowDescription.text = string.Format(GetLicenceStatusDesc("HoldAllLevels"), getLicenceName(type));
			}
			else
			{
				confirmWindowLevel.text = string.Format(GetLicenceStatusDesc("LicenceLevel"), allLicences[(int)type].getCurrentLevel());
				if (allLicences[(int)type].isConnectedWithSkillLevel())
				{
					confirmWindowDescription.text = string.Format(GetLicenceStatusDesc("LevelUpSkillToUnlock"), allLicences[(int)type].getConnectedSkillName());
				}
				else
				{
					confirmWindowDescription.text = string.Format(GetLicenceStatusDesc("HoldAllLevels"), getLicenceName(type));
				}
			}
		}
		else if (allLicences[(int)type].canAffordNextLevel())
		{
			confirmWindowConfirmButton.gameObject.SetActive(value: true);
			confirmWindowConfirmButton.enabled = true;
		}
		else
		{
			confirmWindowConfirmButton.gameObject.SetActive(value: false);
			confirmWindowConfirmButton.enabled = false;
		}
		licenceListWindow.gameObject.SetActive(value: false);
		confirmWindow.gameObject.SetActive(value: true);
	}

	public void closeConfirmWindow()
	{
		licenceListWindow.SetActive(value: true);
		confirmWindow.SetActive(value: false);
		sortAndMoveCompletedLicencesToBottom(licenceButtons);
	}

	public void confirmAndBuy()
	{
		licenceToConfirm.buyNextLevel();
		for (int i = 0; i < licenceButtons.Count; i++)
		{
			licenceButtons[i].updateButton();
		}
		GiftedItemWindow.gifted.addLicenceToBeGiven((int)licenceToConfirm.type);
		closeLicenceWindow();
		GiftedItemWindow.gifted.openWindowAndGiveItems();
	}

	public string getLicenceName(LicenceTypes type)
	{
		int num = (int)type;
		return (LocalizedString)("LicenceNames/Licence_" + num);
	}

	public void checkForUnlocksOnLevelUp(Licence check, bool loadCheck = false)
	{
		if (check == null)
		{
			return;
		}
		giveRecipeOnLicenceLevelUp(check);
		if (check.type == LicenceTypes.Logging)
		{
			if (check.getCurrentLevel() >= 1 && !allLicences[5].isUnlocked)
			{
				allLicences[5].isUnlocked = true;
				NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NewLicenceAvailable"), getLicenceName(LicenceTypes.LandScaping));
			}
			if (check.getCurrentLevel() >= 2 && !allLicences[21].isUnlocked)
			{
				allLicences[21].isUnlocked = true;
				NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NewLicenceAvailable"), getLicenceName(LicenceTypes.Brewing));
			}
		}
		if (check.type == LicenceTypes.LandScaping)
		{
			if (check.getCurrentLevel() >= 2 && !allLicences[19].isUnlocked)
			{
				allLicences[19].isUnlocked = true;
				NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NewLicenceAvailable"), getLicenceName(LicenceTypes.SignWriting));
			}
			if (check.getCurrentLevel() >= 2 && !allLicences[20].isUnlocked)
			{
				allLicences[20].isUnlocked = true;
				NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NewLicenceAvailable"), getLicenceName(LicenceTypes.WaterBed));
			}
		}
		if (check.type == LicenceTypes.Hunting && check.getCurrentLevel() == 1 && !allLicences[15].isUnlocked)
		{
			allLicences[15].isUnlocked = true;
			NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NewLicenceAvailable"), getLicenceName(LicenceTypes.AnimalTrapping));
		}
		if (check.type == LicenceTypes.Mining && check.getCurrentLevel() >= 2 && !allLicences[13].isUnlocked)
		{
			allLicences[13].isUnlocked = true;
			NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NewLicenceAvailable"), getLicenceName(LicenceTypes.DeepMining));
		}
		if (check.type == LicenceTypes.Excavation && check.getCurrentLevel() >= 1 && !allLicences[6].isUnlocked)
		{
			allLicences[6].isUnlocked = true;
			NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NewLicenceAvailable"), getLicenceName(LicenceTypes.MetalDetecting));
		}
		if (check.type == LicenceTypes.DeepMining)
		{
			if (!loadCheck && check.getCurrentLevel() >= 1 && !DeedManager.manage.isDeedUnlockedAndUnbought(DeedManager.manage.mineDeed))
			{
				DeedManager.manage.unlockDeed(DeedManager.manage.mineDeed);
				NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NewDeedAvailable"), ConversationGenerator.generate.GetNotificationText("TalkToFletchForDeeds"), SoundManager.Instance.notificationSound);
			}
			if (check.getCurrentLevel() >= 1)
			{
				DeedManager.manage.unlockDeed(DeedManager.manage.mineDeed);
			}
		}
		if (check.type == LicenceTypes.Farming && check.getCurrentLevel() >= 3 && !allLicences[17].isUnlocked)
		{
			allLicences[17].isUnlocked = true;
			NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NewLicenceAvailable"), getLicenceName(LicenceTypes.Irrigation));
		}
		if (check.type == LicenceTypes.Farming && check.getCurrentLevel() >= 1 && !allLicences[9].isUnlocked)
		{
			NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NewLicenceAvailable"), getLicenceName(LicenceTypes.AnimalHandling));
			allLicences[9].isUnlocked = true;
		}
		if (check.type == LicenceTypes.Logging && check.getCurrentLevel() >= 1 && !allLicences[10].isUnlocked)
		{
			NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NewLicenceAvailable"), getLicenceName(LicenceTypes.Bridge));
			allLicences[10].isUnlocked = true;
		}
		if (check.type == LicenceTypes.AnimalHandling && check.getCurrentLevel() >= 2 && !allLicences[22].isUnlocked)
		{
			NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NewLicenceAvailable"), getLicenceName(LicenceTypes.AnimalProccessing));
			allLicences[22].isUnlocked = true;
		}
		if ((check.type == LicenceTypes.Fishing || check.type == LicenceTypes.WaterBed) && allLicences[3].isMaxLevelAndCompleted() && allLicences[20].isMaxLevelAndCompleted() && !allLicences[23].isUnlocked)
		{
			if (!loadCheck)
			{
				NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NewLicenceAvailable"), getLicenceName(LicenceTypes.FishFarming));
			}
			allLicences[23].isUnlocked = true;
		}
		if (allLicences[1].getCurrentLevel() >= 1 && allLicences[2].getCurrentLevel() >= 1 && allLicences[3].getCurrentLevel() >= 1 && allLicences[4].getCurrentLevel() >= 1 && !allLicences[14].isUnlocked)
		{
			allLicences[14].isUnlocked = true;
			NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NewLicenceAvailable"), getLicenceName(LicenceTypes.ToolBelt));
		}
		if (allLicences[1].getCurrentLevel() >= 2 && allLicences[2].getCurrentLevel() >= 2 && allLicences[3].getCurrentLevel() >= 2 && allLicences[4].getCurrentLevel() >= 2 && !allLicences[8].isUnlocked)
		{
			allLicences[8].isUnlocked = true;
			NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NewLicenceAvailable"), getLicenceName(LicenceTypes.Commerce));
		}
		if (allLicences[11].getCurrentLevel() >= 3 && allLicences[7].getCurrentLevel() >= 2 && allLicences[17].getCurrentLevel() >= 2 && !allLicences[18].isUnlocked)
		{
			allLicences[18].isUnlocked = true;
			NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NewLicenceAvailable"), getLicenceName(LicenceTypes.AgriculturalVehicle));
		}
		if (check.type == LicenceTypes.ToolBelt)
		{
			if (check.getCurrentLevel() >= 1 && !allLicences[12].isUnlocked)
			{
				allLicences[12].isUnlocked = true;
				NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NewLicenceAvailable"), getLicenceName(LicenceTypes.Cargo));
			}
			Inventory.Instance.setSlotsUnlocked(updateBelt: true);
		}
		if (check.type == LicenceTypes.Cargo)
		{
			Inventory.Instance.setSlotsUnlocked(updateBelt: true);
		}
	}

	public string getLicenceLevelDescription(LicenceTypes type, int level)
	{
		return type switch
		{
			LicenceTypes.Logging => (LocalizedString)("LicenceDesc/Licence_Logging_" + level + "_Desc"), 
			LicenceTypes.Mining => (LocalizedString)("LicenceDesc/Licence_Mining_" + level + "_Desc"), 
			LicenceTypes.LandScaping => (LocalizedString)("LicenceDesc/Licence_Landscaping_" + level + "_Desc"), 
			LicenceTypes.Fishing => (LocalizedString)("LicenceDesc/Licence_Fishing_" + level + "_Desc"), 
			LicenceTypes.Hunting => (LocalizedString)("LicenceDesc/Licence_Hunting_" + level + "_Desc"), 
			LicenceTypes.MetalDetecting => (LocalizedString)("LicenceDesc/Licence_MetalDetecting_" + level + "_Desc"), 
			LicenceTypes.Vehicle => (LocalizedString)("LicenceDesc/Licence_Vehicle_" + level + "_Desc"), 
			LicenceTypes.Farming => (LocalizedString)("LicenceDesc/Licence_Farming_" + level + "_Desc"), 
			LicenceTypes.Commerce => (LocalizedString)("LicenceDesc/Licence_Commerce_" + level + "_Desc"), 
			LicenceTypes.Cargo => (LocalizedString)"LicenceDesc/Licence_Cargo_1_Desc", 
			LicenceTypes.ToolBelt => (LocalizedString)"LicenceDesc/Licence_ToolBelt_Desc", 
			LicenceTypes.AnimalHandling => (LocalizedString)("LicenceDesc/Licence_AnimalHandling_" + level + "_Desc"), 
			LicenceTypes.Bridge => (LocalizedString)("LicenceDesc/Licence_Building_" + level + "_Desc"), 
			LicenceTypes.AnimalTrapping => (LocalizedString)("LicenceDesc/Licence_AnimalTrapping_" + level + "_Desc"), 
			LicenceTypes.Irrigation => (LocalizedString)("LicenceDesc/Licence_Irrigation_" + level + "_Desc"), 
			LicenceTypes.Excavation => (LocalizedString)"LicenceDesc/Licence_Excavation_Desc", 
			LicenceTypes.AgriculturalVehicle => (LocalizedString)("LicenceDesc/Licence_AgriculturalVehicle_" + level + "_Desc"), 
			LicenceTypes.DeepMining => (LocalizedString)"LicenceDesc/Licence_DeepMining_Desc", 
			LicenceTypes.SignWriting => (LocalizedString)("LicenceDesc/Licence_SignWriting_" + level + "_Desc"), 
			LicenceTypes.Brewing => (LocalizedString)("LicenceDesc/Licence_Brewing_" + level + "_Desc"), 
			LicenceTypes.WaterBed => (LocalizedString)("LicenceDesc/Licence_WaterScaping_" + level + "_Desc"), 
			LicenceTypes.AnimalProccessing => (LocalizedString)("LicenceDesc/Licence_AnimalProcessingLicence_" + level + "_Desc"), 
			LicenceTypes.FishFarming => (LocalizedString)("LicenceDesc/Licence_FishFarming_" + level + "_Desc"), 
			_ => "", 
		};
	}

	public string GetLicenceStatusDesc(string tag)
	{
		return (LocalizedString)("LicenceDesc/" + tag);
	}

	public void giveRecipeOnLicenceLevelUp(Licence check)
	{
		if (check == null)
		{
			return;
		}
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].craftable && Inventory.Instance.allItems[i].craftable.checkIfMeetsLicenceRequirement(check.type, check.getCurrentLevel()) && Inventory.Instance.allItems[i].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.TrapperShop && !CharLevelManager.manage.checkIfUnlocked(i))
			{
				GiftedItemWindow.gifted.addRecipeToUnlock(Inventory.Instance.getInvItemId(Inventory.Instance.allItems[i]));
			}
		}
		GiftedItemWindow.gifted.openWindowAndGiveItems();
	}

	public void unlockRecipesAlreadyLearntFromAllLicences()
	{
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if (!Inventory.Instance.allItems[i].craftable || Inventory.Instance.allItems[i].isDeed || Inventory.Instance.allItems[i].craftable.workPlaceConditions == CraftingManager.CraftingMenuType.CraftingShop)
			{
				continue;
			}
			if (Inventory.Instance.allItems[i].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.TrapperShop && Inventory.Instance.allItems[i].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.Blocked && Inventory.Instance.allItems[i].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.AgentCrafting)
			{
				if (Inventory.Instance.allItems[i].craftable.workPlaceConditions == CraftingManager.CraftingMenuType.RaffleBox || Inventory.Instance.allItems[i].craftable.workPlaceConditions == CraftingManager.CraftingMenuType.SkyFestRaffleBox)
				{
					Inventory.Instance.allItems[i].value = Inventory.Instance.allItems[i].craftable.stackOfItemsInRecipe[0] * 500 / Inventory.Instance.allItems[i].craftable.recipeGiveThisAmount;
				}
				else
				{
					int num = 0;
					for (int j = 0; j < Inventory.Instance.allItems[i].craftable.itemsInRecipe.Length; j++)
					{
						num += Inventory.Instance.allItems[i].craftable.itemsInRecipe[j].value * Inventory.Instance.allItems[i].craftable.stackOfItemsInRecipe[j];
					}
					if (Inventory.Instance.allItems[i].craftable.workPlaceConditions == CraftingManager.CraftingMenuType.CookingTable)
					{
						Inventory.Instance.allItems[i].value = Mathf.RoundToInt((float)(num / Inventory.Instance.allItems[i].craftable.recipeGiveThisAmount) * 1f);
					}
					else
					{
						Inventory.Instance.allItems[i].value = Mathf.RoundToInt((float)(num / Inventory.Instance.allItems[i].craftable.recipeGiveThisAmount) * 1.25f);
					}
				}
			}
			if (CharLevelManager.manage.checkIfUnlocked(i))
			{
				continue;
			}
			for (int k = 1; k < allLicences.Length; k++)
			{
				if (Inventory.Instance.allItems[i].craftable.checkIfMeetsLicenceRequirement(allLicences[k].type, allLicences[k].getCurrentLevel()))
				{
					CharLevelManager.manage.unlockRecipe(Inventory.Instance.allItems[i]);
				}
			}
		}
		if (CatalogueManager.manage.collectedItem[DeedManager.manage.deedsUnlockedOnStart[0].getItemId()])
		{
			CharLevelManager.manage.unlockRecipe(WorldManager.Instance.allObjectSettings[DeedManager.manage.deedsUnlockedOnStart[0].placeable.tileObjectId].dropsItemOnDeath);
		}
	}
}
