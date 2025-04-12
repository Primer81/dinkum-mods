using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QuestTracker : MonoBehaviour
{
	public enum typeOfTask
	{
		None,
		Quest,
		BulletinBoard,
		Request,
		DeedItems,
		Recipe
	}

	public static QuestTracker track;

	public GameObject requiredItemPrefab;

	public GameObject QuestWindow;

	public GameObject noTaskAvaliableWindow;

	public GameObject questButtonPrefab;

	public GameObject requiredItemsBox;

	public Transform questListWindow;

	public TextMeshProUGUI questTitle;

	public TextMeshProUGUI questDesc;

	public TextMeshProUGUI questGiverName;

	public TextMeshProUGUI questDateGiven;

	public TextMeshProUGUI questMission;

	public FillRecipeSlot rewardInfo;

	public bool trackerOpen;

	private List<InvButton> questButtons = new List<InvButton>();

	private int currentlyDisplayingQuestNo = -1;

	private Color defualtColor;

	private Color defualtHoverColor;

	public Image givenByCharImage;

	public GameObject questDescriptionWindow;

	public TextMeshProUGUI pinMissionText;

	public TextMeshProUGUI recipePinChest;

	public bool pinnedMissionTextOn;

	public UnityEvent updateTasksEvent = new UnityEvent();

	public TextMeshProUGUI pinTaskButtonText;

	public TextMeshProUGUI pinRecipeButtonText;

	public Sprite bulletinBoardMissionIcon;

	public QuestButton trackingRecipeButton;

	private List<GameObject> currentRequiredItemIcons = new List<GameObject>();

	private typeOfTask pinnedType;

	private int pinnedId = -1;

	private typeOfTask lookingAtTask;

	private int lookingAtId = -1;

	private int pinnedRecipeId = -1;

	private PostOnBoard pinnedBuletingBoardPost;

	private void Awake()
	{
		track = this;
	}

	private void Start()
	{
		updateTasksEvent.AddListener(updatePinnedTask);
		updatePinnedTask();
	}

	private void OnEnable()
	{
		LocalizationManager.OnLocalizeEvent += OnLanguageChange;
	}

	private void OnDisable()
	{
		LocalizationManager.OnLocalizeEvent -= OnLanguageChange;
	}

	private void OnLanguageChange()
	{
		if (pinnedRecipeId != -1)
		{
			if (Inventory.Instance.allItems[pinnedRecipeId].isDeed)
			{
				trackingRecipeButton.buttonText.text = ConversationGenerator.generate.GetQuestTrackerText("TrackingDeedRequirements").Replace("<itemName>", Inventory.Instance.allItems[pinnedRecipeId].getInvItemName());
			}
			else
			{
				trackingRecipeButton.buttonText.text = ConversationGenerator.generate.GetQuestTrackerText("TrackingItemRecipe").Replace("<itemName>", Inventory.Instance.allItems[pinnedRecipeId].getInvItemName());
			}
		}
	}

	public void openQuestWindow()
	{
		QuestWindow.SetActive(value: true);
		defualtColor = questButtonPrefab.GetComponent<InvButton>().defaultColor;
		defualtHoverColor = questButtonPrefab.GetComponent<InvButton>().hoverColor;
		trackerOpen = true;
		fillQuestList();
	}

	public void closeQuestWindow()
	{
		QuestWindow.SetActive(value: false);
		trackerOpen = false;
	}

	private void fillQuestList()
	{
		for (int i = 0; i < questButtons.Count; i++)
		{
			Object.Destroy(questButtons[i].gameObject);
		}
		questButtons.Clear();
		for (int j = 0; j < QuestManager.manage.allQuests.Length; j++)
		{
			if (QuestManager.manage.isQuestAccepted[j] && !QuestManager.manage.isQuestCompleted[j])
			{
				InvButton component = Object.Instantiate(questButtonPrefab, questListWindow).GetComponent<InvButton>();
				component.craftRecipeNumber = j;
				questButtons.Add(component);
				component.GetComponent<QuestButton>().setUpMainQuest(j);
			}
		}
		for (int k = 0; k < BulletinBoard.board.attachedPosts.Count; k++)
		{
			if (BulletinBoard.board.attachedPosts[k].checkIfAccepted() && !BulletinBoard.board.attachedPosts[k].completed && !BulletinBoard.board.attachedPosts[k].checkIfExpired())
			{
				InvButton component2 = Object.Instantiate(questButtonPrefab, questListWindow).GetComponent<InvButton>();
				component2.craftRecipeNumber = k;
				questButtons.Add(component2);
				component2.GetComponent<QuestButton>().setUp(questButton: true, k);
			}
		}
		for (int l = 0; l < NPCManager.manage.NPCRequests.Length; l++)
		{
			if (NPCManager.manage.npcStatus[l].acceptedRequest && !NPCManager.manage.npcStatus[l].completedRequest)
			{
				InvButton component3 = Object.Instantiate(questButtonPrefab, questListWindow).GetComponent<InvButton>();
				component3.craftRecipeNumber = l;
				questButtons.Add(component3);
				component3.GetComponent<QuestButton>().setUp(questButton: false, l);
			}
		}
		if (questButtons.Count > 0)
		{
			if (currentlyDisplayingQuestNo == -1)
			{
				questButtons[0].PressButtonDelay();
			}
			else
			{
				displayQuest(currentlyDisplayingQuestNo);
			}
			noTaskAvaliableWindow.gameObject.SetActive(value: false);
		}
		else
		{
			currentlyDisplayingQuestNo = -1;
			trackingRecipeButton.onPress();
			if (pinnedRecipeId <= -1)
			{
				noTaskAvaliableWindow.gameObject.SetActive(value: true);
			}
			else
			{
				noTaskAvaliableWindow.gameObject.SetActive(value: false);
			}
		}
		refreshButtonSelection();
	}

	private void refreshButtonSelection()
	{
	}

	public void displayTrackingRecipe()
	{
		currentlyDisplayingQuestNo = -1;
		refreshButtonSelection();
		if (pinnedRecipeId > -1)
		{
			if (Inventory.Instance.allItems[pinnedRecipeId].isDeed)
			{
				questTitle.text = ConversationGenerator.generate.GetQuestTrackerText("TrackingDeedRequirements").Replace("<itemName>", Inventory.Instance.allItems[pinnedRecipeId].getInvItemName());
				questDesc.text = ConversationGenerator.generate.GetQuestTrackerText("ItemsRequiredToStartConscruction").Replace("<itemName>", Inventory.Instance.allItems[pinnedRecipeId].getInvItemName()) + "\n" + ConversationGenerator.generate.GetQuestTrackerText("UnpinToRemove");
			}
			else
			{
				questTitle.text = ConversationGenerator.generate.GetQuestTrackerText("TrackingItemRecipe").Replace("<itemName>", Inventory.Instance.allItems[pinnedRecipeId].getInvItemName());
				questDesc.text = ConversationGenerator.generate.GetQuestTrackerText("ItemsRequiredToCraft").Replace("<itemName>", Inventory.Instance.allItems[pinnedRecipeId].getInvItemName()) + "\n" + ConversationGenerator.generate.GetQuestTrackerText("UnpinToRemove");
			}
			questGiverName.text = "";
			questDateGiven.text = "";
			givenByCharImage.sprite = Inventory.Instance.allItems[pinnedRecipeId].getSprite();
			fillRequiredItemsBox(Inventory.Instance.allItems[pinnedRecipeId]);
			questMission.text = "";
			rewardInfo.gameObject.SetActive(value: false);
			updateLookingAtTask(typeOfTask.Recipe, 0);
		}
	}

	public void displayMainQuest(int questNo)
	{
		currentlyDisplayingQuestNo = -1;
		refreshButtonSelection();
		questTitle.text = QuestManager.manage.allQuests[questNo].GetQuestTitleText();
		questDesc.text = QuestManager.manage.allQuests[questNo].GetQuestDescription().Replace("<IslandName>", Inventory.Instance.islandName);
		questGiverName.text = NPCManager.manage.NPCDetails[QuestManager.manage.allQuests[questNo].offeredByNpc].GetNPCName();
		questDateGiven.text = "";
		fillRequiredItemsBox(QuestManager.manage.allQuests[questNo]);
		questMission.text = QuestManager.manage.allQuests[questNo].getMissionObjText();
		rewardInfo.gameObject.SetActive(value: false);
		if (QuestManager.manage.allQuests[questNo].offeredByNpc <= 11)
		{
			givenByCharImage.sprite = NPCManager.manage.NPCDetails[QuestManager.manage.allQuests[questNo].offeredByNpc].npcSprite;
		}
		else
		{
			givenByCharImage.sprite = NPCManager.manage.NPCDetails[QuestManager.manage.allQuests[questNo].offeredByNpc].GetNPCSprite(QuestManager.manage.allQuests[questNo].offeredByNpc);
		}
		updateLookingAtTask(typeOfTask.Quest, questNo);
		replaceMissionTextSprites();
	}

	public void displayRequest(int requestNo)
	{
		currentlyDisplayingQuestNo = -1;
		refreshButtonSelection();
		questTitle.text = ConversationGenerator.generate.GetQuestTrackerText("RequestForNPCName").Replace("<npcName>", NPCManager.manage.NPCDetails[requestNo].GetNPCName());
		questDesc.text = ConversationGenerator.generate.GetQuestTrackerText("RequestForNPCName").Replace("<npcName>", NPCManager.manage.NPCDetails[requestNo].GetNPCName()).Replace("<itemName>", NPCManager.manage.NPCRequests[requestNo].getDesiredItemName());
		questGiverName.text = NPCManager.manage.NPCDetails[requestNo].GetNPCName();
		questDateGiven.text = ConversationGenerator.generate.GetQuestTrackerText("ByTheEndOfTheDay");
		questMission.text = NPCManager.manage.NPCRequests[requestNo].getMissionText(requestNo);
		rewardInfo.gameObject.SetActive(value: false);
		fillRequiredItemsBox(NPCManager.manage.NPCRequests[requestNo]);
		if (requestNo <= 11)
		{
			givenByCharImage.sprite = NPCManager.manage.NPCDetails[requestNo].npcSprite;
		}
		else
		{
			givenByCharImage.sprite = NPCManager.manage.NPCDetails[requestNo].GetNPCSprite(requestNo);
		}
		updateLookingAtTask(typeOfTask.Request, requestNo);
		replaceMissionTextSprites();
	}

	public void displayQuest(int boardId)
	{
		currentlyDisplayingQuestNo = boardId;
		refreshButtonSelection();
		if (boardId >= BulletinBoard.board.attachedPosts.Count)
		{
			currentlyDisplayingQuestNo = -1;
			fillQuestList();
			return;
		}
		questTitle.text = LocalisationMarkUp.RunMarkupCheck(BulletinBoard.board.attachedPosts[boardId].getTitleText(boardId), BulletinBoard.board.attachedPosts[boardId].requiredItem, BulletinBoard.board.attachedPosts[boardId].requireItemAmount, BulletinBoard.board.attachedPosts[boardId].rewardId, BulletinBoard.board.attachedPosts[boardId].rewardAmount);
		questDesc.text = LocalisationMarkUp.RunMarkupCheck(BulletinBoard.board.attachedPosts[boardId].getContentText(boardId), BulletinBoard.board.attachedPosts[boardId].requiredItem, BulletinBoard.board.attachedPosts[boardId].requireItemAmount, BulletinBoard.board.attachedPosts[boardId].rewardId, BulletinBoard.board.attachedPosts[boardId].rewardAmount);
		questGiverName.text = BulletinBoard.board.attachedPosts[boardId].getPostedByName();
		if (BulletinBoard.board.attachedPosts[boardId].isHuntingTask)
		{
			questTitle.text = LocalisationMarkUp.RunMarkupCheckAnimal(questTitle.text, BulletinBoard.board.attachedPosts[boardId].myHuntingChallenge.challengeAnimalId);
			questDesc.text = LocalisationMarkUp.RunMarkupCheckAnimal(questDesc.text, BulletinBoard.board.attachedPosts[boardId].myHuntingChallenge.challengeAnimalId);
		}
		else if (BulletinBoard.board.attachedPosts[boardId].isCaptureTask)
		{
			questTitle.text = LocalisationMarkUp.RunMarkupCheckAnimal(questTitle.text, BulletinBoard.board.attachedPosts[boardId].animalToCapture);
			questDesc.text = LocalisationMarkUp.RunMarkupCheckAnimal(questDesc.text, BulletinBoard.board.attachedPosts[boardId].animalToCapture);
		}
		if (BulletinBoard.board.attachedPosts[boardId].getDaysUntilExpired() == 0)
		{
			questDateGiven.text = ConversationGenerator.generate.GetTimeNameByTag("Last_Day");
		}
		else
		{
			questDateGiven.text = string.Format(ConversationGenerator.generate.GetTimeNameByTag("Amount_Days_Remaining"), BulletinBoard.board.attachedPosts[boardId].getDaysUntilExpired());
		}
		questMission.text = BulletinBoard.board.getMissionText(boardId);
		fillRequiredItemsBox(BulletinBoard.board.attachedPosts[boardId]);
		if (BulletinBoard.board.attachedPosts[boardId].rewardId > -1)
		{
			rewardInfo.fillRecipeSlotForQuestReward(BulletinBoard.board.attachedPosts[boardId].rewardId, BulletinBoard.board.attachedPosts[boardId].rewardAmount);
			rewardInfo.gameObject.SetActive(value: false);
		}
		else
		{
			rewardInfo.gameObject.SetActive(value: false);
		}
		questGiverName.text = BulletinBoard.board.attachedPosts[boardId].getPostedByName();
		if (BulletinBoard.board.attachedPosts[boardId].postedByNpcId < 0)
		{
			givenByCharImage.sprite = bulletinBoardMissionIcon;
		}
		else if (BulletinBoard.board.attachedPosts[boardId].postedByNpcId > 0 && BulletinBoard.board.attachedPosts[boardId].postedByNpcId <= 11)
		{
			givenByCharImage.sprite = NPCManager.manage.NPCDetails[BulletinBoard.board.attachedPosts[boardId].postedByNpcId].npcSprite;
		}
		else
		{
			givenByCharImage.sprite = NPCManager.manage.NPCDetails[BulletinBoard.board.attachedPosts[boardId].postedByNpcId].GetNPCSprite(BulletinBoard.board.attachedPosts[boardId].postedByNpcId);
		}
		updateLookingAtTask(typeOfTask.BulletinBoard, boardId);
		replaceMissionTextSprites();
	}

	public void fillRequiredItemsBox(Quest questToFillFrom)
	{
		foreach (GameObject currentRequiredItemIcon in currentRequiredItemIcons)
		{
			Object.Destroy(currentRequiredItemIcon);
		}
		currentRequiredItemIcons.Clear();
		requiredItemsBox.SetActive(questToFillFrom.requiredItems.Length != 0);
		for (int i = 0; i < questToFillFrom.requiredItems.Length; i++)
		{
			int invItemId = Inventory.Instance.getInvItemId(questToFillFrom.requiredItems[i]);
			currentRequiredItemIcons.Add(Object.Instantiate(requiredItemPrefab, requiredItemsBox.transform));
			currentRequiredItemIcons[currentRequiredItemIcons.Count - 1].GetComponent<FillRecipeSlot>().fillRecipeSlotWithAmounts(invItemId, Inventory.Instance.getAmountOfItemInAllSlots(invItemId), questToFillFrom.requiredStacks[i]);
		}
	}

	public void fillRequiredItemsBox(InventoryItem craftable)
	{
		foreach (GameObject currentRequiredItemIcon in currentRequiredItemIcons)
		{
			Object.Destroy(currentRequiredItemIcon);
		}
		currentRequiredItemIcons.Clear();
		requiredItemsBox.SetActive(value: true);
		for (int i = 0; i < craftable.craftable.itemsInRecipe.Length; i++)
		{
			int invItemId = Inventory.Instance.getInvItemId(craftable.craftable.itemsInRecipe[i]);
			currentRequiredItemIcons.Add(Object.Instantiate(requiredItemPrefab, requiredItemsBox.transform));
			currentRequiredItemIcons[currentRequiredItemIcons.Count - 1].GetComponent<FillRecipeSlot>().fillRecipeSlotWithAmounts(invItemId, Inventory.Instance.getAmountOfItemInAllSlots(invItemId), craftable.craftable.stackOfItemsInRecipe[i]);
		}
	}

	public void fillRequiredItemsBox(NPCRequest request)
	{
		foreach (GameObject currentRequiredItemIcon in currentRequiredItemIcons)
		{
			Object.Destroy(currentRequiredItemIcon);
		}
		currentRequiredItemIcons.Clear();
		requiredItemsBox.SetActive(value: true);
		if (request.specificDesiredItem == -1)
		{
			requiredItemsBox.SetActive(value: false);
			return;
		}
		int specificDesiredItem = request.specificDesiredItem;
		currentRequiredItemIcons.Add(Object.Instantiate(requiredItemPrefab, requiredItemsBox.transform));
		currentRequiredItemIcons[0].GetComponent<FillRecipeSlot>().fillRecipeSlotWithAmounts(specificDesiredItem, Inventory.Instance.getAmountOfItemInAllSlots(specificDesiredItem), request.desiredAmount);
	}

	public void fillRequiredItemsBox(PostOnBoard boardPost)
	{
		foreach (GameObject currentRequiredItemIcon in currentRequiredItemIcons)
		{
			Object.Destroy(currentRequiredItemIcon);
		}
		currentRequiredItemIcons.Clear();
		if (boardPost.requiredItem <= -1)
		{
			requiredItemsBox.SetActive(value: false);
			return;
		}
		requiredItemsBox.SetActive(value: true);
		currentRequiredItemIcons.Add(Object.Instantiate(requiredItemPrefab, requiredItemsBox.transform));
		currentRequiredItemIcons[0].GetComponent<FillRecipeSlot>().fillRecipeSlotWithAmounts(boardPost.requiredItem, Inventory.Instance.getAmountOfItemInAllSlots(boardPost.requiredItem), boardPost.requireItemAmount);
	}

	public bool hasPinnedMission()
	{
		if (pinnedId != -1)
		{
			return true;
		}
		return false;
	}

	public void pressPinTaskButton()
	{
		pinnedRecipeId = -1;
		trackingRecipeButton.gameObject.SetActive(value: false);
		if (pinnedType == typeOfTask.Recipe)
		{
			pinnedType = typeOfTask.None;
			pinnedRecipeId = -1;
			openQuestWindow();
		}
		else if (pinnedType == lookingAtTask && pinnedId == lookingAtId)
		{
			unpinTask();
		}
		else if (lookingAtId == -1 || lookingAtTask == typeOfTask.None)
		{
			pinTheTask(typeOfTask.None, -1);
		}
		else if (lookingAtTask == typeOfTask.BulletinBoard)
		{
			if (lookingAtId < BulletinBoard.board.attachedPosts.Count)
			{
				pinnedBuletingBoardPost = BulletinBoard.board.attachedPosts[lookingAtId];
				pinTheTask(lookingAtTask, lookingAtId);
			}
			else
			{
				pinnedBuletingBoardPost = null;
			}
		}
		else
		{
			pinnedBuletingBoardPost = null;
			pinTheTask(lookingAtTask, lookingAtId);
		}
	}

	public void pressPinRecipeButton()
	{
		if (pinnedRecipeId == CraftingManager.manage.craftableItemId)
		{
			pinnedRecipeId = -1;
			pinTheTask(typeOfTask.None, -1);
		}
		else
		{
			pinnedRecipeId = CraftingManager.manage.craftableItemId;
			if (pinnedRecipeId != -1)
			{
				pinTheTask(typeOfTask.Recipe, lookingAtId);
			}
		}
		if (pinnedRecipeId != -1)
		{
			trackingRecipeButton.gameObject.SetActive(value: true);
			trackingRecipeButton.npcIcon.sprite = Inventory.Instance.allItems[pinnedRecipeId].getSprite();
			if (Inventory.Instance.allItems[pinnedRecipeId].isDeed)
			{
				trackingRecipeButton.buttonText.text = ConversationGenerator.generate.GetQuestTrackerText("TrackingDeedRequirements").Replace("<itemName>", Inventory.Instance.allItems[pinnedRecipeId].getInvItemName());
			}
			else
			{
				trackingRecipeButton.buttonText.text = ConversationGenerator.generate.GetQuestTrackerText("TrackingItemRecipe").Replace("<itemName>", Inventory.Instance.allItems[pinnedRecipeId].getInvItemName());
			}
		}
		else
		{
			trackingRecipeButton.gameObject.SetActive(value: false);
		}
		updatePinnedRecipeButton();
	}

	public void PressPinDeedButton()
	{
		if (!(GiveNPC.give.GetDeedForCurrentBuilding() != null))
		{
			return;
		}
		int itemId = GiveNPC.give.GetDeedForCurrentBuilding().getItemId();
		if (pinnedRecipeId == itemId)
		{
			pinnedRecipeId = -1;
			pinTheTask(typeOfTask.None, -1);
		}
		else
		{
			pinnedRecipeId = itemId;
			if (pinnedRecipeId != -1)
			{
				pinTheTask(typeOfTask.Recipe, lookingAtId);
			}
		}
		if (pinnedRecipeId != -1)
		{
			trackingRecipeButton.gameObject.SetActive(value: true);
			trackingRecipeButton.npcIcon.sprite = Inventory.Instance.allItems[pinnedRecipeId].getSprite();
			trackingRecipeButton.buttonText.text = ConversationGenerator.generate.GetQuestTrackerText("TrackingItemRecipe").Replace("<itemName>", Inventory.Instance.allItems[pinnedRecipeId].getInvItemName());
		}
		else
		{
			trackingRecipeButton.gameObject.SetActive(value: false);
		}
		updatePinnedRecipeButton();
	}

	public void unpinTask()
	{
		pinTheTask(typeOfTask.None, -1);
	}

	public void updateLookingAtTask(typeOfTask type, int id)
	{
		if (type == typeOfTask.Recipe)
		{
			pinTaskButtonText.text = "<sprite=17> " + ConversationGenerator.generate.GetToolTip("Tip_Pinned");
			return;
		}
		lookingAtId = id;
		lookingAtTask = type;
		if (pinnedId == id && pinnedType == type)
		{
			pinTaskButtonText.text = "<sprite=17> " + ConversationGenerator.generate.GetToolTip("Tip_Pinned");
		}
		else
		{
			pinTaskButtonText.text = "<sprite=16> " + ConversationGenerator.generate.GetToolTip("Tip_Pinned");
		}
	}

	public void pinTheTask(typeOfTask type, int id)
	{
		pinnedType = type;
		pinnedId = id;
		if (type != typeOfTask.BulletinBoard || (type == typeOfTask.BulletinBoard && id >= BulletinBoard.board.attachedPosts.Count))
		{
			pinnedBuletingBoardPost = null;
		}
		updatePinnedTask();
		updateLookingAtTask(lookingAtTask, lookingAtId);
	}

	public void updatePinnedTask()
	{
		if (pinnedType == typeOfTask.None)
		{
			pinMissionText.gameObject.SetActive(value: false);
		}
		else if (!pinnedMissionTextOn)
		{
			pinMissionText.gameObject.SetActive(value: false);
		}
		else
		{
			pinMissionText.gameObject.SetActive(value: true);
		}
		if (pinnedType == typeOfTask.Recipe)
		{
			fillMissionTextForRecipe(pinnedRecipeId);
		}
		else if (pinnedType == typeOfTask.Quest)
		{
			if (QuestManager.manage.isQuestCompleted[pinnedId])
			{
				if (QuestManager.manage.allQuests[pinnedId].acceptQuestOnComplete != -1)
				{
					pinnedId = QuestManager.manage.allQuests[pinnedId].acceptQuestOnComplete;
					updatePinnedTask();
				}
				else
				{
					unpinTask();
				}
			}
			else
			{
				pinMissionText.text = QuestManager.manage.allQuests[pinnedId].GetQuestTitleText() + "\n<size=11>" + QuestManager.manage.allQuests[pinnedId].getMissionObjText();
			}
		}
		else if (pinnedType == typeOfTask.BulletinBoard)
		{
			if (pinnedBuletingBoardPost == null)
			{
				unpinTask();
				return;
			}
			if (!BulletinBoard.board.attachedPosts.Contains(pinnedBuletingBoardPost))
			{
				unpinTask();
				return;
			}
			pinnedId = BulletinBoard.board.attachedPosts.IndexOf(pinnedBuletingBoardPost);
			if (BulletinBoard.board.attachedPosts[pinnedId].checkIfExpired() || BulletinBoard.board.attachedPosts[pinnedId].completed)
			{
				unpinTask();
			}
			else
			{
				pinMissionText.text = BulletinBoard.board.attachedPosts[pinnedId].getTitleText(pinnedId) + "\n<size=11>" + BulletinBoard.board.getMissionText(pinnedId);
			}
		}
		else if (pinnedType == typeOfTask.Request)
		{
			if (NPCManager.manage.npcStatus[pinnedId].completedRequest)
			{
				unpinTask();
			}
			else
			{
				pinMissionText.text = ConversationGenerator.generate.GetQuestTrackerText("RequestForNPCName").Replace("<npcName>", NPCManager.manage.NPCDetails[pinnedId].GetNPCName()) + "\n<size=11>" + NPCManager.manage.NPCRequests[pinnedId].getMissionText(pinnedId);
			}
		}
		else if (pinnedType != typeOfTask.DeedItems)
		{
			pinMissionText.text = "";
			pinMissionText.gameObject.SetActive(value: false);
		}
		if (pinnedType != typeOfTask.Recipe)
		{
			recipePinChest.gameObject.SetActive(value: false);
			return;
		}
		recipePinChest.text = pinMissionText.text;
		recipePinChest.gameObject.SetActive(value: true);
	}

	public void UnPinTaskIfCompleted(typeOfTask taskType, int npcId)
	{
		if (taskType == pinnedType && npcId == pinnedId)
		{
			unpinTask();
		}
	}

	public void replaceMissionTextSprites()
	{
		questMission.text = questMission.text.Replace("<sprite=12>", "<sprite=16>");
		questMission.text = questMission.text.Replace("<sprite=13>", "<sprite=17>");
	}

	public void updatePinnedRecipeButton()
	{
		if (pinnedRecipeId == CraftingManager.manage.craftableItemId)
		{
			pinRecipeButtonText.text = "<sprite=13> " + ConversationGenerator.generate.GetToolTip("Tip_TrackRecipe");
		}
		else
		{
			pinRecipeButtonText.text = "<sprite=12> " + ConversationGenerator.generate.GetToolTip("Tip_TrackRecipe");
		}
		if (GiveNPC.give.GetDeedForCurrentBuilding() == null || pinnedRecipeId != GiveNPC.give.GetDeedForCurrentBuilding().getItemId())
		{
			GiveNPC.give.trackDeedRequirementsButtonText.text = "<sprite=12> " + ConversationGenerator.generate.GetToolTip("Tip_TrackRequirements");
		}
		else
		{
			GiveNPC.give.trackDeedRequirementsButtonText.text = "<sprite=13> " + ConversationGenerator.generate.GetToolTip("Tip_TrackRequirements");
		}
	}

	public void fillMissionTextForRecipe(int recipeId)
	{
		if (recipeId < 0)
		{
			return;
		}
		Recipe craftable = Inventory.Instance.allItems[recipeId].craftable;
		if (Inventory.Instance.allItems[pinnedRecipeId].isDeed)
		{
			pinMissionText.text = ConversationGenerator.generate.GetQuestTrackerText("TrackingDeedRequirements").Replace("<itemName>", Inventory.Instance.allItems[pinnedRecipeId].getInvItemName()) + "\n<size=11>";
		}
		else
		{
			pinMissionText.text = ConversationGenerator.generate.GetQuestTrackerText("TrackingItemRecipe").Replace("<itemName>", Inventory.Instance.allItems[pinnedRecipeId].getInvItemName()) + "\n<size=11>";
		}
		for (int i = 0; i < craftable.itemsInRecipe.Length; i++)
		{
			int amountOfItemInAllSlots = Inventory.Instance.getAmountOfItemInAllSlots(craftable.itemsInRecipe[i].getItemId());
			string text = " [" + amountOfItemInAllSlots + "/" + craftable.stackOfItemsInRecipe[i] + "]";
			if (amountOfItemInAllSlots >= craftable.stackOfItemsInRecipe[i])
			{
				TextMeshProUGUI textMeshProUGUI = pinMissionText;
				textMeshProUGUI.text = textMeshProUGUI.text + "<sprite=13> " + craftable.itemsInRecipe[i].getInvItemName() + text + "\n";
			}
			else
			{
				TextMeshProUGUI textMeshProUGUI = pinMissionText;
				textMeshProUGUI.text = textMeshProUGUI.text + "<sprite=12> " + craftable.itemsInRecipe[i].getInvItemName() + text + "\n";
			}
		}
	}
}
