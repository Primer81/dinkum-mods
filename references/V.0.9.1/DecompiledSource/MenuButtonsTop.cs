using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuButtonsTop : MonoBehaviour
{
	public static MenuButtonsTop menu;

	public GameObject subMenuWindow;

	public GameObject subMenuButtonsWindow;

	public GameObject hud;

	public GameObject charWindow;

	public GameObject invCamera;

	public GameObject closeButton;

	public GameObject optionsPage;

	public GameObject confirmQuitWindow;

	public GameObject optionButtonNoJournal;

	private Vector2 originalSize;

	private Vector2 originalPos;

	public bool closed = true;

	private bool windowOpenCheck;

	public bool windowNoTop = true;

	public string UpAxis = "";

	public string B_Button = "B_1";

	public string Y_Button = "Y_1";

	public string A_Button = "A_1";

	public string X_Button = "X_1";

	public string RB_Button = "RB_1";

	public string LB_Button = "LB_1";

	public string StartButton = "Start_1";

	public string Select = "Back_1";

	public string Cancel = "Cancel";

	public string MoveX = "L_XAxis_1";

	public string MoveY = "L_YAxis_1";

	public string Chat = "Chat";

	public string Drop = "Drop";

	public string QuickSlot1 = "QS_1";

	public string QuickSlot2 = "QS_2";

	public string QuickSlot3 = "QS_3";

	public string QuickSlot4 = "QS_4";

	public string DpadUPDown = "DPadUpDown";

	public string DpadLeftRight = "DpadLeftRight";

	public string RightTrigger = "RightTrigger";

	public string LeftTrigger = "LeftTrigger";

	public bool subMenuOpen;

	public bool subMenuJustOpened;

	public Transform questTrackerWindow;

	public Transform pages;

	public GameObject[] disabledOnOpenWhenNoJournal;

	public Sprite controllerMapButton;

	public Sprite keyboardMapButton;

	public Sprite controllerJournal;

	public Sprite keyboardJournal;

	private bool quitToDesktop;

	private void Awake()
	{
		menu = this;
	}

	public void openSubMenu()
	{
		hud.gameObject.SetActive(value: false);
		subMenuWindow.gameObject.SetActive(value: true);
		closed = false;
		subMenuOpen = true;
		Inventory.Instance.checkIfWindowIsNeeded();
		subMenuJustOpened = true;
		StartCoroutine(subMenuDelay());
		MilestoneManager.manage.updateMilestoneList(firstOpen: true);
		subMenuButtonsWindow.gameObject.SetActive(value: true);
		CurrencyWindows.currency.openJournal();
		GameObject[] array;
		if (!TownManager.manage.journalUnlocked)
		{
			questTrackerWindow.SetParent(subMenuWindow.transform);
			questTrackerWindow.GetComponent<Image>().enabled = true;
			switchToQuests();
			array = disabledOnOpenWhenNoJournal;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
			optionButtonNoJournal.SetActive(value: true);
			return;
		}
		NetworkMapSharer.Instance.localChar.myEquip.setNewLookingAtJournal(isLookingAtJournalNow: true);
		questTrackerWindow.GetComponent<Image>().enabled = false;
		questTrackerWindow.SetParent(pages.transform);
		questTrackerWindow.SetSiblingIndex(2);
		questTrackerWindow.gameObject.SetActive(value: false);
		optionButtonNoJournal.SetActive(value: false);
		array = disabledOnOpenWhenNoJournal;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
	}

	public void closeSubMenu()
	{
		NetworkMapSharer.Instance.localChar.myEquip.setNewLookingAtJournal(isLookingAtJournalNow: false);
		if (MilestoneManager.manage.milestoneClaimWindowOpen)
		{
			MilestoneManager.manage.closeMilestoneClaimWindow();
			return;
		}
		if (PediaManager.manage.entryFullScreenShown)
		{
			PediaManager.manage.closeEntryDetails();
			return;
		}
		if (PhotoManager.manage.photoTabOpen && PhotoManager.manage.blownUpWindow.activeInHierarchy)
		{
			PhotoManager.manage.closeBlownUpWindow();
			return;
		}
		hud.gameObject.SetActive(value: true);
		subMenuWindow.gameObject.SetActive(value: false);
		closeWindow();
		closeButtonDelay();
		subMenuOpen = false;
		Inventory.Instance.checkIfWindowIsNeeded();
		subMenuJustOpened = true;
		StartCoroutine(subMenuDelay());
		CurrencyWindows.currency.closeJournal();
	}

	public IEnumerator subMenuDelay()
	{
		yield return null;
		subMenuJustOpened = false;
	}

	public void Update()
	{
		if (!NetworkMapSharer.Instance || !NetworkMapSharer.Instance.localChar || Inventory.Instance.menuOpen || StatusManager.manage.dead)
		{
			return;
		}
		if (GiveNPC.give.giveWindowOpen || ChestWindow.chests.chestWindowOpen || FarmAnimalMenu.menu.farmAnimalMenuOpen || HairDresserMenu.menu.hairMenuOpen || (PhotoManager.manage.photoTabOpen && PhotoManager.manage.isGivingToNPC()) || (CraftingManager.manage.craftMenuOpen && CraftingManager.manage.specialCraftMenu) || CharLevelManager.manage.unlockWindowOpen)
		{
			if (!windowNoTop)
			{
				charWindow.SetActive(value: false);
				if ((PhotoManager.manage.photoTabOpen && PhotoManager.manage.isGivingToNPC()) || (CraftingManager.manage.craftMenuOpen && CraftingManager.manage.specialCraftMenu) || CharLevelManager.manage.unlockWindowOpen)
				{
					closeButton.SetActive(value: true);
				}
				else
				{
					closeButton.SetActive(value: false);
				}
				closed = false;
				windowNoTop = true;
			}
			if (!ChestWindow.chests.chestWindowOpen)
			{
				return;
			}
		}
		else if (windowNoTop)
		{
			closeButton.SetActive(value: true);
			windowNoTop = false;
		}
		if (InputMaster.input.OpenInventory() && !CreativeManager.instance.IsCreativeSearchWindowOpen())
		{
			if (ChestWindow.chests.chestWindowOpen)
			{
				Inventory.Instance.invOpen = false;
				Inventory.Instance.openAndCloseInv();
				Inventory.Instance.scanPocketsForCatalogueUpdate();
				ChestWindow.chests.closeChestInWindow();
				closeButtonDelay();
			}
			else if (!Inventory.Instance.invOpen && Inventory.Instance.CanMoveCharacter())
			{
				switchToInv();
			}
			else if (Inventory.Instance.invOpen)
			{
				closeButtonDelay();
				closeCamera();
				Inventory.Instance.invOpen = false;
				Inventory.Instance.openAndCloseInv();
			}
		}
		if (!ChatBox.chat.chatOpen && !CheatScript.cheat.cheatMenuOpen && !CreativeManager.instance.IsCreativeMenuOpen())
		{
			if (InputMaster.input.OpenMap() && !RenderMap.Instance.mapOpen && !subMenuOpen && !subMenuJustOpened && Inventory.Instance.CanMoveCharacter())
			{
				switchToMap();
			}
			else if (InputMaster.input.OpenMap() && RenderMap.Instance.mapOpen && !RenderMap.Instance.iconSelectorOpen && !RenderMap.Instance.selectTeleWindowOpen)
			{
				Inventory.Instance.pressActiveBackButton();
			}
			else if (InputMaster.input.Journal())
			{
				if (subMenuOpen && !Inventory.Instance.usingMouse && !RenderMap.Instance.mapOpen && !QuestTracker.track.trackerOpen && !PhotoManager.manage.photoTabOpen)
				{
					closeSubMenu();
				}
				else if (!subMenuOpen && !subMenuJustOpened && Inventory.Instance.CanMoveCharacter() && closed)
				{
					openSubMenu();
				}
			}
		}
		RenderMap.Instance.RunMapFollow();
	}

	public void switchToInv()
	{
		Inventory.Instance.invOpen = true;
		Inventory.Instance.openAndCloseInv();
		RenderMap.Instance.CloseMap();
		QuestTracker.track.closeQuestWindow();
		PhotoManager.manage.closePhotoTab();
		moveCameraToInvPosition();
		Inventory.Instance.checkIfWindowIsNeeded();
		closed = false;
	}

	public void switchToCraft()
	{
		Inventory.Instance.invOpen = false;
		Inventory.Instance.openAndCloseInv();
		RenderMap.Instance.CloseMap();
		QuestTracker.track.closeQuestWindow();
		PhotoManager.manage.closePhotoTab();
		closeCamera();
		closed = false;
	}

	public void switchToQuests()
	{
		swapWindow();
		QuestTracker.track.openQuestWindow();
		closeCamera();
		Inventory.Instance.checkIfWindowIsNeeded();
		closed = false;
		subMenuButtonsWindow.gameObject.SetActive(value: false);
	}

	public void switchToAnimals()
	{
		swapWindow();
		FarmAnimalMenu.menu.openJournalTab();
		closeCamera();
		Inventory.Instance.checkIfWindowIsNeeded();
		closed = false;
		subMenuButtonsWindow.gameObject.SetActive(value: false);
	}

	public void switchToMyDetails()
	{
		swapWindow();
		PlayerDetailManager.manage.openTab();
		closeCamera();
		Inventory.Instance.checkIfWindowIsNeeded();
		closed = false;
		subMenuButtonsWindow.gameObject.SetActive(value: false);
	}

	public void switchToPedia()
	{
		swapWindow();
		PediaManager.manage.openPedia();
		closeCamera();
		Inventory.Instance.checkIfWindowIsNeeded();
		closed = false;
		subMenuButtonsWindow.gameObject.SetActive(value: false);
	}

	public void switchToPhotos()
	{
		swapWindow();
		PhotoManager.manage.openPhotoTab();
		closeCamera();
		Inventory.Instance.checkIfWindowIsNeeded();
		closed = false;
		subMenuButtonsWindow.gameObject.SetActive(value: false);
	}

	public void switchToMap()
	{
		RenderMap.Instance.refreshMap = true;
		swapWindow();
		RenderMap.Instance.OpenMap();
		RenderMap.Instance.ChangeMapWindow();
		PhotoManager.manage.closePhotoTab();
		Inventory.Instance.checkIfWindowIsNeeded();
		closeCamera();
		closed = false;
		subMenuButtonsWindow.gameObject.SetActive(value: false);
	}

	public void switchToOptions()
	{
		swapWindow();
		optionsPage.SetActive(value: true);
		confirmQuitWindow.SetActive(value: false);
		PhotoManager.manage.closePhotoTab();
		Inventory.Instance.checkIfWindowIsNeeded();
		closeCamera();
		closed = false;
		subMenuButtonsWindow.gameObject.SetActive(value: false);
	}

	public void quitToDesktopButton()
	{
		confirmQuitWindow.SetActive(value: true);
		quitToDesktop = true;
	}

	public void quitToMenuButton()
	{
		confirmQuitWindow.SetActive(value: true);
		quitToDesktop = false;
	}

	public void ConfirmQuitButton()
	{
		if (quitToDesktop)
		{
			SaveLoad.saveOrLoad.quitGame();
		}
		else
		{
			SaveLoad.saveOrLoad.returnToMenu();
		}
	}

	public void moveCameraToInvPosition()
	{
	}

	public void closeCamera()
	{
	}

	public void swapWindow()
	{
		PlayerDetailManager.manage.closeTab();
		FarmAnimalMenu.menu.closeJournalTab();
		RenderMap.Instance.CloseMap();
		closeCamera();
		Inventory.Instance.invOpen = false;
		Inventory.Instance.openAndCloseInv();
		QuestTracker.track.closeQuestWindow();
		PhotoManager.manage.closePhotoTab();
		CharLevelManager.manage.closeUnlockScreen();
		RenderMap.Instance.CloseMap();
		PediaManager.manage.closePedia();
		Inventory.Instance.checkIfWindowIsNeeded();
		subMenuButtonsWindow.gameObject.SetActive(value: true);
		MilestoneManager.manage.closeMilestoneClaimWindow();
		optionsPage.SetActive(value: false);
		confirmQuitWindow.SetActive(value: false);
	}

	public void closeWindow()
	{
		PlayerDetailManager.manage.closeTab();
		FarmAnimalMenu.menu.closeJournalTab();
		RenderMap.Instance.CloseMap();
		closeCamera();
		Inventory.Instance.invOpen = false;
		Inventory.Instance.openAndCloseInv();
		QuestTracker.track.closeQuestWindow();
		PhotoManager.manage.closePhotoTab();
		CharLevelManager.manage.closeUnlockScreen();
		RenderMap.Instance.CloseMap();
		RenderMap.Instance.ChangeMapWindow();
		Inventory.Instance.checkIfWindowIsNeeded();
		PediaManager.manage.closePedia();
		closeButtonDelay();
		subMenuButtonsWindow.gameObject.SetActive(value: true);
		MilestoneManager.manage.closeMilestoneClaimWindow();
		optionsPage.gameObject.SetActive(value: false);
	}

	public void closeCraftWindow()
	{
		RenderMap.Instance.CloseMap();
		closeCamera();
		Inventory.Instance.invOpen = false;
		Inventory.Instance.openAndCloseInv();
		CraftingManager.manage.openCloseCraftMenu(isMenuOpen: false);
		QuestTracker.track.closeQuestWindow();
		PhotoManager.manage.closePhotoTab();
		CharLevelManager.manage.closeUnlockScreen();
		RenderMap.Instance.CloseMap();
		RenderMap.Instance.ChangeMapWindow();
		Inventory.Instance.checkIfWindowIsNeeded();
		closeButtonDelay();
		subMenuButtonsWindow.gameObject.SetActive(value: true);
		MilestoneManager.manage.closeMilestoneClaimWindow();
	}

	public void closeButtonDelay(float delayTime = 0.15f)
	{
		Inventory.Instance.checkIfWindowIsNeeded();
		Invoke("closeDelay", delayTime);
	}

	private void closeDelay()
	{
		if (!Inventory.Instance.isMenuOpen())
		{
			closed = true;
		}
	}
}
