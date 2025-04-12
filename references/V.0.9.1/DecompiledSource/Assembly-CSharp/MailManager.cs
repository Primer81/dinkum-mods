using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MailManager : MonoBehaviour
{
	public static MailManager manage;

	public GameObject mailButtonPrefab;

	public Transform LettersWindow;

	public GameObject letterListWindow;

	public GameObject showLetterWindow;

	public GameObject deleteButtonGO;

	public bool mailWindowOpen;

	public TextMeshProUGUI letterText;

	public FillRecipeSlot attachmentSlot;

	public GameObject mailWindow;

	public GameObject takeRewardButton;

	public List<Letter> mailInBox = new List<Letter>();

	public List<Letter> tomorrowsLetters = new List<Letter>();

	public LetterTemplate animalResearchLetter;

	public LetterTemplate animalResearchChrissy;

	public LetterTemplate animalResearchSpecialDrop;

	public LetterTemplate returnTrapLetter;

	public LetterTemplate devLetter;

	public LetterTemplate catalogueItemLetter;

	public LetterTemplate craftmanDayOff;

	public LetterTemplate soldFarmAnimalWithItem;

	public LetterTemplate[] randomLetters;

	public LetterTemplate[] thankYouLetters;

	public LetterTemplate[] didNotFitInInvLetter;

	public LetterTemplate[] fishingTips;

	public LetterTemplate[] bugTips;

	public LetterTemplate[] licenceLevelUp;

	public LetterTemplate[] bugCompPositions;

	public LetterTemplate[] fishingCompPositions;

	public UnityEvent newMailEvent = new UnityEvent();

	public Image mailBorder;

	private List<MailButton> showingButtons = new List<MailButton>();

	public GameObject noMailScreen;

	public Animator windowBounceAnim;

	public TextMeshProUGUI fromText;

	public GameObject unopenedWindow;

	public ASound openLetterSound;

	public Image letterSprite;

	public WindowAnimator changeLetterWindowMask;

	public GameObject deleteAllButton;

	public Image saveButtonStarIcon;

	public TextMeshProUGUI saveButtonText;

	private int showingLetterId;

	private List<Letter> deleteOnClose = new List<Letter>();

	private void Awake()
	{
		manage = this;
	}

	private void Start()
	{
	}

	public void sendDailyMail()
	{
		for (int i = 0; i < tomorrowsLetters.Count; i++)
		{
			mailInBox.Add(tomorrowsLetters[i]);
		}
		if (WorldManager.Instance.day == 6 && WorldManager.Instance.week == 3 && LicenceManager.manage.allLicences[3].getCurrentLevel() >= 1)
		{
			int undiscoveredFish = PediaManager.manage.getUndiscoveredFish();
			if (undiscoveredFish != -1)
			{
				mailInBox.Add(new Letter(-3, Letter.LetterType.FishingTips, undiscoveredFish));
			}
		}
		if (WorldManager.Instance.day == 3 && WorldManager.Instance.week == 2)
		{
			int undiscoveredBug = PediaManager.manage.getUndiscoveredBug();
			if (undiscoveredBug != -1)
			{
				mailInBox.Add(new Letter(-4, Letter.LetterType.BugTips, undiscoveredBug));
			}
		}
		tomorrowsLetters.Clear();
		for (int j = 0; j < NPCManager.manage.NPCDetails.Length; j++)
		{
			if (NPCManager.manage.npcStatus[j].checkIfHasMovedIn() && Random.Range(0, 109 - NPCManager.manage.npcStatus[j].relationshipLevel) == 1)
			{
				mailInBox.Add(new Letter(j, Letter.LetterType.randomLetter));
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.GetLetters);
			}
		}
		newMailEvent.Invoke();
	}

	public void sendAnInvFullLetter(int itemId, int stack)
	{
		mailInBox.Add(new Letter(ConversationManager.manage.lastConversationTarget.myId.NPCNo, Letter.LetterType.fullInvLetter, itemId, stack));
		newMailEvent.Invoke();
	}

	public void sendAnAnimalCapturedLetter(int rewardAmount, int trapType)
	{
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.TrapAnAnimal);
		tomorrowsLetters.Add(new Letter(-1, Letter.LetterType.AnimalResearchLetter, Inventory.Instance.getInvItemId(Inventory.Instance.moneyItem), rewardAmount));
		tomorrowsLetters.Add(new Letter(-1, Letter.LetterType.AnimalTrapReturn, trapType, 1));
	}

	public void sendAnimalSoldLetter(int itemId, int amount)
	{
		mailInBox.Add(new Letter(7, Letter.LetterType.SoldAnimalLetter, itemId, amount));
	}

	public void sendAChrissyAnimalCapturedLetter(int trapType)
	{
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.TrapAnAnimal);
		tomorrowsLetters.Add(new Letter(-1, Letter.LetterType.ChrissyTrapDelivered));
		tomorrowsLetters.Add(new Letter(-1, Letter.LetterType.AnimalTrapReturn, trapType, 1));
	}

	public void SendASpecialDropAnimalCapturedLetter(int specialItem)
	{
		tomorrowsLetters.Add(new Letter(-1, Letter.LetterType.SpecialItemTrapDelivery, specialItem, 1));
	}

	public void checkForUpdateLetter()
	{
		mailInBox.Add(new Letter(-2, Letter.LetterType.DevLetter));
	}

	public void openMailWindow()
	{
		mailWindowOpen = true;
		mailWindow.gameObject.SetActive(value: true);
		showLetterWindow.gameObject.SetActive(value: false);
		letterListWindow.gameObject.SetActive(value: true);
		fillButtons();
		Inventory.Instance.checkIfWindowIsNeeded();
		if ((bool)MailBoxShowsMail.showsMail)
		{
			MailBoxShowsMail.showsMail.refresh();
		}
		if ((bool)Inventory.Instance.activeScrollBar)
		{
			Inventory.Instance.activeScrollBar.resetToTop();
		}
		MenuButtonsTop.menu.closed = false;
		letterListWindow.SetActive(value: true);
	}

	public void closeMailWindow()
	{
		mailWindowOpen = false;
		mailWindow.gameObject.SetActive(value: false);
		showLetterWindow.gameObject.SetActive(value: false);
		letterListWindow.gameObject.SetActive(value: true);
		if ((bool)MailBoxShowsMail.showsMail)
		{
			MailBoxShowsMail.showsMail.refresh();
		}
		for (int i = 0; i < deleteOnClose.Count; i++)
		{
			mailInBox.Remove(deleteOnClose[i]);
		}
		deleteOnClose.Clear();
		newMailEvent.Invoke();
		Inventory.Instance.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closeButtonDelay();
	}

	private void fillButtons()
	{
		for (int i = 0; i < showingButtons.Count; i++)
		{
			Object.Destroy(showingButtons[i].gameObject);
		}
		showingButtons.Clear();
		for (int j = 0; j < mailInBox.Count; j++)
		{
			MailButton component = Object.Instantiate(mailButtonPrefab, LettersWindow).GetComponent<MailButton>();
			component.letterId = j;
			component.buttonText.text = getSentByName(mailInBox[j].sentById);
			component.iconColor = getLetterColour(mailInBox[j].sentById);
			component.showOpen(mailInBox[j].hasBeenRead, mailInBox[j].itemAttached, mailInBox[j].saved);
			showingButtons.Add(component);
			component.transform.SetAsFirstSibling();
		}
		if (mailInBox.Count <= 0)
		{
			noMailScreen.SetActive(value: true);
		}
		else
		{
			noMailScreen.SetActive(value: false);
		}
	}

	private void selectorDeselectButtons()
	{
		for (int i = 0; i < showingButtons.Count; i++)
		{
			if (showingLetterId == i)
			{
				showingButtons[i].showOpen(mailInBox[i].hasBeenRead, mailInBox[i].itemAttached, mailInBox[i].saved);
			}
		}
	}

	public void closeShowLetterWindow()
	{
		showLetterWindow.SetActive(value: false);
		letterListWindow.SetActive(value: true);
		if (mailInBox.Count <= 0)
		{
			noMailScreen.SetActive(value: true);
		}
		else
		{
			noMailScreen.SetActive(value: false);
		}
		deleteAllButton.SetActive(value: true);
	}

	public void showLetter(int letterId)
	{
		deleteAllButton.SetActive(value: false);
		showSavedLetterButton(letterId);
		letterListWindow.SetActive(value: false);
		changeLetterWindowMask.refreshAnimation();
		letterSprite.color = getLetterColour(mailInBox[letterId].sentById);
		showingLetterId = letterId;
		mailBorder.color = getLetterColour(mailInBox[letterId].sentById);
		if (mailInBox[showingLetterId].hasBeenRead)
		{
			unopenedWindow.SetActive(value: false);
		}
		else
		{
			fromText.text = string.Format(ConversationGenerator.generate.GetLetterText("FromName"), getSentByName(mailInBox[letterId].sentById));
			unopenedWindow.SetActive(value: true);
		}
		showLetterWindow.gameObject.SetActive(value: true);
		SoundManager.Instance.play2DSound(BulletinBoard.board.pageTurnSound);
		if (mailInBox[letterId].itemOriginallAttached != -1)
		{
			letterText.text = "<size=18><b>" + string.Format(ConversationGenerator.generate.GetLetterText("AddressToName"), Inventory.Instance.playerName) + "</size></b>,\n\n" + mailInBox[letterId].getMyTemplate().GetLetterText().Replace("<itemAttachedName>", Inventory.Instance.allItems[mailInBox[letterId].itemOriginallAttached].getInvItemName()) + "\n\n<size=18><b>" + string.Format(ConversationGenerator.generate.GetLetterText("FromName"), getSentByName(mailInBox[letterId].sentById)) + "</size></b>.";
			letterText.text = letterText.text.Replace("<season>", RealWorldTimeLight.time.getSeasonName(mailInBox[letterId].seasonSent - 1));
			letterText.text = letterText.text.Replace("<biomeName>", AnimalManager.manage.fillAnimalLocation(Inventory.Instance.allItems[mailInBox[letterId].itemOriginallAttached]));
			letterText.text = letterText.text.Replace("<timeOfDay>", AnimalManager.manage.fillAnimalTimeOfDay(Inventory.Instance.allItems[mailInBox[letterId].itemOriginallAttached]));
			letterText.text = letterText.text.Replace("<licenceName>", LicenceManager.manage.getLicenceName((LicenceManager.LicenceTypes)mailInBox[letterId].itemOriginallAttached));
		}
		else
		{
			letterText.text = "<size=18><b>" + string.Format(ConversationGenerator.generate.GetLetterText("AddressToName"), Inventory.Instance.playerName) + "</size></b>,\n\n" + mailInBox[letterId].getMyTemplate().GetLetterText() + "\n\n<size=18><b>" + string.Format(ConversationGenerator.generate.GetLetterText("FromName"), getSentByName(mailInBox[letterId].sentById)) + "</b>.";
		}
		if (mailInBox[letterId].itemAttached != -1)
		{
			takeRewardButton.SetActive(value: true);
			Inventory.Instance.setCurrentlySelectedAndMoveCursor(takeRewardButton.GetComponent<RectTransform>());
			deleteButtonGO.SetActive(value: false);
			attachmentSlot.gameObject.SetActive(value: true);
			attachmentSlot.fillRecipeSlotForQuestReward(mailInBox[letterId].itemAttached, mailInBox[letterId].stackOfItemAttached);
			if (Inventory.Instance.allItems[mailInBox[letterId].itemAttached].hasFuel)
			{
				attachmentSlot.itemAmounts.text = "";
			}
		}
		else
		{
			takeRewardButton.SetActive(value: false);
			deleteButtonGO.SetActive(value: true);
			Inventory.Instance.setCurrentlySelectedAndMoveCursor(deleteButtonGO.GetComponent<RectTransform>());
			attachmentSlot.gameObject.SetActive(value: false);
		}
		selectorDeselectButtons();
	}

	public void takeAttachment()
	{
		if (Inventory.Instance.moneyItem == Inventory.Instance.allItems[mailInBox[showingLetterId].itemAttached])
		{
			takeRewardButton.SetActive(value: false);
			deleteButtonGO.SetActive(value: true);
			Inventory.Instance.setCurrentlySelectedAndMoveCursor(deleteButtonGO.GetComponent<RectTransform>());
			attachmentSlot.gameObject.SetActive(value: false);
			mailInBox[showingLetterId].itemAttached = -1;
			showingButtons[showingLetterId].showOpen(isOpened: true, mailInBox[showingLetterId].itemAttached, mailInBox[showingLetterId].saved);
			Inventory.Instance.changeWallet(mailInBox[showingLetterId].stackOfItemAttached);
		}
		else if (Inventory.Instance.addItemToInventory(mailInBox[showingLetterId].itemAttached, mailInBox[showingLetterId].stackOfItemAttached))
		{
			takeRewardButton.SetActive(value: false);
			deleteButtonGO.SetActive(value: true);
			Inventory.Instance.setCurrentlySelectedAndMoveCursor(deleteButtonGO.GetComponent<RectTransform>());
			attachmentSlot.gameObject.SetActive(value: false);
			mailInBox[showingLetterId].itemAttached = -1;
			showingButtons[showingLetterId].showOpen(isOpened: true, mailInBox[showingLetterId].itemAttached, mailInBox[showingLetterId].saved);
		}
		else
		{
			NotificationManager.manage.createChatNotification(ConversationGenerator.generate.GetToolTip("Tip_PocketsFull"), specialTip: true);
		}
	}

	public void takeAttachment(int mailId)
	{
		if (Inventory.Instance.moneyItem == Inventory.Instance.allItems[mailInBox[mailId].itemAttached])
		{
			takeRewardButton.SetActive(value: false);
			deleteButtonGO.SetActive(value: true);
			Inventory.Instance.setCurrentlySelectedAndMoveCursor(deleteButtonGO.GetComponent<RectTransform>());
			attachmentSlot.gameObject.SetActive(value: false);
			mailInBox[mailId].itemAttached = -1;
			showingButtons[mailId].showOpen(isOpened: true, mailInBox[mailId].itemAttached, mailInBox[mailId].saved);
			Inventory.Instance.changeWallet(mailInBox[mailId].stackOfItemAttached);
		}
		else if (Inventory.Instance.addItemToInventory(mailInBox[mailId].itemAttached, mailInBox[mailId].stackOfItemAttached))
		{
			takeRewardButton.SetActive(value: false);
			deleteButtonGO.SetActive(value: true);
			Inventory.Instance.setCurrentlySelectedAndMoveCursor(deleteButtonGO.GetComponent<RectTransform>());
			attachmentSlot.gameObject.SetActive(value: false);
			mailInBox[mailId].itemAttached = -1;
			showingButtons[mailId].showOpen(isOpened: true, mailInBox[mailId].itemAttached, mailInBox[mailId].saved);
		}
		else
		{
			NotificationManager.manage.createChatNotification(ConversationGenerator.generate.GetToolTip("Tip_PocketsFull"), specialTip: true);
		}
	}

	public void collectAndDeleteAll()
	{
		for (int i = 0; i < mailInBox.Count; i++)
		{
			if (!mailInBox[i].saved)
			{
				if (mailInBox[i].itemAttached != -1)
				{
					takeAttachment(i);
				}
				if (mailInBox[i].itemAttached == -1)
				{
					deleteLetter(i);
				}
			}
		}
	}

	public void openLetter()
	{
		SoundManager.Instance.play2DSound(openLetterSound);
		windowBounceAnim.SetTrigger("Open");
		Invoke("openLetterDelay", 0.35f);
	}

	private void openLetterDelay()
	{
		mailInBox[showingLetterId].hasBeenRead = true;
		showLetter(showingLetterId);
	}

	public void deleteButton()
	{
		deleteOnClose.Add(mailInBox[showingLetterId]);
		showingButtons[showingLetterId].gameObject.SetActive(value: false);
		closeShowLetterWindow();
		for (int num = showingLetterId; num >= 0; num--)
		{
			if (showingButtons[num].gameObject.activeSelf)
			{
				Inventory.Instance.setCurrentlySelectedAndMoveCursor(showingButtons[num].GetComponent<RectTransform>());
				return;
			}
		}
		for (int i = 0; i < showingButtons.Count; i++)
		{
			if (showingButtons[i].gameObject.activeSelf)
			{
				Inventory.Instance.setCurrentlySelectedAndMoveCursor(showingButtons[i].GetComponent<RectTransform>());
				return;
			}
		}
		noMailScreen.SetActive(value: true);
	}

	public void deleteLetter(int letterId)
	{
		deleteOnClose.Add(mailInBox[letterId]);
		showingButtons[letterId].gameObject.SetActive(value: false);
		closeShowLetterWindow();
		for (int num = letterId; num >= 0; num--)
		{
			if (showingButtons[num].gameObject.activeSelf)
			{
				Inventory.Instance.setCurrentlySelectedAndMoveCursor(showingButtons[num].GetComponent<RectTransform>());
				return;
			}
		}
		for (int i = 0; i < showingButtons.Count; i++)
		{
			if (showingButtons[i].gameObject.activeSelf)
			{
				Inventory.Instance.setCurrentlySelectedAndMoveCursor(showingButtons[i].GetComponent<RectTransform>());
				return;
			}
		}
		noMailScreen.SetActive(value: true);
	}

	public string getSentByName(int id)
	{
		if (id >= 0)
		{
			return NPCManager.manage.NPCDetails[id].GetNPCName();
		}
		return id switch
		{
			-1 => ConversationGenerator.generate.GetBulletinBoardText("Animal Research Centre_Name"), 
			-2 => ConversationGenerator.generate.GetLetterText("DinkumDevName"), 
			-3 => ConversationGenerator.generate.GetLetterText("FishTipsterName"), 
			-4 => ConversationGenerator.generate.GetLetterText("BugTipsterName"), 
			_ => "???", 
		};
	}

	public Color getLetterColour(int id)
	{
		if (id >= 0)
		{
			return NPCManager.manage.NPCDetails[id].npcColor;
		}
		return Color.white;
	}

	public bool checkIfAnyUndreadLetters()
	{
		for (int i = 0; i < mailInBox.Count; i++)
		{
			if (!mailInBox[i].hasBeenRead)
			{
				return true;
			}
		}
		return false;
	}

	public void sendLicenceUnlockMail(int licenceId)
	{
		mailInBox.Add(new Letter(6, Letter.LetterType.LicenceUnlock, licenceId));
	}

	public void setLetterSave()
	{
		mailInBox[showingLetterId].saved = !mailInBox[showingLetterId].saved;
		showingButtons[showingLetterId].showOpen(mailInBox[showingLetterId].hasBeenRead, mailInBox[showingLetterId].itemAttached, mailInBox[showingLetterId].saved);
		showSavedLetterButton(showingLetterId);
	}

	public void showSavedLetterButton(int letterId)
	{
		if (showingLetterId < mailInBox.Count)
		{
			if (mailInBox[showingLetterId].saved)
			{
				saveButtonText.text = ConversationGenerator.generate.GetJournalNameByTag("MailSaved");
				saveButtonStarIcon.color = Color.Lerp(Color.yellow, Color.red, 0.5f);
			}
			else
			{
				saveButtonText.text = ConversationGenerator.generate.GetJournalNameByTag("MailSave");
				saveButtonStarIcon.color = Color.white;
			}
		}
	}
}
