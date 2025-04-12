using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BulletinBoard : MonoBehaviour
{
	public enum BoardPostType
	{
		Quest,
		Announcement,
		Reminder
	}

	public enum TemplateType
	{
		huntingTemplate,
		captureTemplate,
		tradeTemplate,
		photoTemplate,
		cookingTemplate,
		smeltingTemplate,
		compostTemplate,
		sateliteTemplate,
		craftingRequest,
		shippingRequest,
		presentDrop
	}

	public static BulletinBoard board;

	public GameObject bullitenBoardWindow;

	[Header("RealPostBox --------")]
	public TextMeshProUGUI postTitleTextBox;

	public TextMeshProUGUI postContentText;

	public TextMeshProUGUI postedByName;

	public TextMeshProUGUI missionObjectiveText;

	public TextMeshProUGUI timeLimitText;

	public FillRecipeSlot rewardInfo;

	public bool windowOpen;

	private int showingPost;

	public List<PostOnBoard> attachedPosts = new List<PostOnBoard>();

	public Image fullPostBorder;

	public GameObject acceptButton;

	public GameObject completedStamp;

	public GameObject expiredStamp;

	public GameObject newStamp;

	public BullitenBoardPost huntingTemplate;

	public BullitenBoardPost captureTemplate;

	public BullitenBoardPost tradeTemplate;

	public BullitenBoardPost photoTemplate;

	public BullitenBoardPost cookingTemplate;

	public BullitenBoardPost smeltingTemplate;

	public BullitenBoardPost compostTemplate;

	public BullitenBoardPost sateliteTemplate;

	public BullitenBoardPost presentTemplate;

	public BullitenBoardPost craftingTemplate;

	public BullitenBoardPost shippingRequestTemplate;

	public List<BullitenBoardPost> randomPosts;

	public BullitenBoardPost[] reminderPosts;

	public BullitenBoardPost[] announcementPosts;

	public ASound pageTurnSound;

	public bool clientLoaded;

	public BulletinBoardTaskButton[] taskButtons;

	public UnityEvent closeBoardEvent = new UnityEvent();

	private void Awake()
	{
		board = this;
	}

	public void onLocalConnect()
	{
		attachedPosts.Clear();
		randomPosts.Clear();
	}

	public void openWindow()
	{
		windowOpen = true;
		bullitenBoardWindow.gameObject.SetActive(value: true);
		showingPost = attachedPosts.Count - 1;
		Inventory.Instance.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closed = false;
		updateTaskButtons();
		Calendar.calendar.updateCalendar();
	}

	public void updateTaskButtons()
	{
		for (int i = 0; i < taskButtons.Length; i++)
		{
			taskButtons[i].attachToPost(attachedPosts.Count - 1 - i);
		}
	}

	public void closeWindow()
	{
		windowOpen = false;
		bullitenBoardWindow.gameObject.SetActive(value: false);
		Inventory.Instance.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closeButtonDelay();
		closeBoardEvent.Invoke();
	}

	public void setSelectedSlotAndShow(int postId)
	{
		showingPost = postId;
		showSelectedPost();
	}

	public void showSelectedPost()
	{
		if (!windowOpen)
		{
			return;
		}
		SoundManager.Instance.play2DSound(pageTurnSound);
		if (attachedPosts[showingPost].hasBeenRead)
		{
			newStamp.SetActive(value: false);
		}
		else
		{
			newStamp.SetActive(value: true);
		}
		if (board.attachedPosts[showingPost].postedByNpcId > 0)
		{
			fullPostBorder.color = NPCManager.manage.NPCDetails[board.attachedPosts[showingPost].postedByNpcId].npcColor;
		}
		else
		{
			fullPostBorder.color = Color.grey;
		}
		attachedPosts[showingPost].hasBeenRead = true;
		if ((bool)BulletinBoardShowNewMessage.showMessage)
		{
			BulletinBoardShowNewMessage.showMessage.showIfNewMessage();
		}
		postTitleTextBox.text = LocalisationMarkUp.RunMarkupCheck(attachedPosts[showingPost].getTitleText(showingPost), attachedPosts[showingPost].requiredItem, attachedPosts[showingPost].requireItemAmount, attachedPosts[showingPost].rewardId, attachedPosts[showingPost].rewardAmount);
		postContentText.text = LocalisationMarkUp.RunMarkupCheck(attachedPosts[showingPost].getContentText(showingPost), attachedPosts[showingPost].requiredItem, attachedPosts[showingPost].requireItemAmount, attachedPosts[showingPost].rewardId, attachedPosts[showingPost].rewardAmount);
		postedByName.text = LocalisationMarkUp.RunMarkupCheck(attachedPosts[showingPost].getPostedByName(), attachedPosts[showingPost].requiredItem, attachedPosts[showingPost].requireItemAmount, attachedPosts[showingPost].rewardId, attachedPosts[showingPost].rewardAmount);
		if (attachedPosts[showingPost].isHuntingTask)
		{
			postTitleTextBox.text = LocalisationMarkUp.RunMarkupCheckAnimal(postTitleTextBox.text, attachedPosts[showingPost].myHuntingChallenge.challengeAnimalId);
			postContentText.text = LocalisationMarkUp.RunMarkupCheckAnimal(postContentText.text, attachedPosts[showingPost].myHuntingChallenge.challengeAnimalId);
		}
		else if (attachedPosts[showingPost].isCaptureTask)
		{
			postTitleTextBox.text = LocalisationMarkUp.RunMarkupCheckAnimal(postTitleTextBox.text, attachedPosts[showingPost].animalToCapture);
			postContentText.text = LocalisationMarkUp.RunMarkupCheckAnimal(postContentText.text, attachedPosts[showingPost].animalToCapture);
		}
		if (attachedPosts[showingPost].completed)
		{
			missionObjectiveText.text = "";
			completedStamp.SetActive(value: true);
		}
		else
		{
			completedStamp.SetActive(value: false);
			missionObjectiveText.text = getMissionText(showingPost).Replace("<sprite=12>", "<sprite=16>").Replace("<sprite=13>", "<sprite=17>");
		}
		if (attachedPosts[showingPost].getPostTypeId() == BoardPostType.Quest)
		{
			if (attachedPosts[showingPost].completed)
			{
				timeLimitText.text = "";
				expiredStamp.SetActive(value: false);
			}
			else if (attachedPosts[showingPost].checkIfExpired())
			{
				expiredStamp.SetActive(value: true);
				timeLimitText.text = "EXPIRED";
			}
			else
			{
				expiredStamp.SetActive(value: false);
				if (attachedPosts[showingPost].getDaysUntilExpired() == 0)
				{
					timeLimitText.text = ConversationGenerator.generate.GetTimeNameByTag("Last_Day");
				}
				else
				{
					timeLimitText.text = string.Format(ConversationGenerator.generate.GetTimeNameByTag("Amount_Days_Remaining"), (attachedPosts[showingPost].getDaysUntilExpired() + 1).ToString() ?? "");
				}
			}
		}
		else
		{
			expiredStamp.SetActive(value: false);
			timeLimitText.text = "";
		}
		if (attachedPosts[showingPost].checkIfCanBeAccepted())
		{
			if (attachedPosts[showingPost].checkIfAccepted())
			{
				acceptButton.SetActive(value: false);
			}
			else
			{
				acceptButton.SetActive(value: true);
			}
		}
		else
		{
			acceptButton.SetActive(value: false);
		}
		if (attachedPosts[showingPost].rewardId > -1)
		{
			rewardInfo.fillRecipeSlotForQuestReward(attachedPosts[showingPost].rewardId, attachedPosts[showingPost].rewardAmount);
			rewardInfo.gameObject.SetActive(value: true);
		}
		else
		{
			rewardInfo.gameObject.SetActive(value: false);
		}
	}

	public void checkForInvestigationPostAndComplete(Vector3 position)
	{
		for (int i = 0; i < attachedPosts.Count; i++)
		{
			if (attachedPosts[i].isInvestigation && Vector3.Distance(attachedPosts[i].getRequiredLocation(), position) <= 2f)
			{
				NetworkMapSharer.Instance.RpcCompleteBulletinBoard(i);
			}
		}
	}

	public void acceptQuest()
	{
		NetworkMapSharer.Instance.localChar.CmdAcceptBulletinBoardPost(showingPost);
	}

	public void selectRandomPost(int mineSeed)
	{
		int num = Random.Range(1, 3);
		for (int i = 0; i < num; i++)
		{
			if (attachedPosts.Count < 4 && (Random.Range(0, 8) < 4 || attachedPosts.Count == 0))
			{
				int num2 = Random.Range(0, NPCManager.manage.npcStatus.Count - 1);
				while (!NPCManager.manage.npcStatus[num2].checkIfHasMovedIn())
				{
					num2 = Random.Range(0, NPCManager.manage.npcStatus.Count - 1);
				}
				PostOnBoard postOnBoard = new PostOnBoard(Random.Range(-999999, 999999), num2, BoardPostType.Quest);
				attachedPosts.Add(postOnBoard);
				NetworkMapSharer.Instance.RpcAddNewTaskToClientBoard(postOnBoard);
			}
		}
	}

	public void RandomisePostsForTesting()
	{
		int num = 4;
		randomPosts.Clear();
		attachedPosts.Clear();
		for (int i = 0; i < num; i++)
		{
			int num2 = Random.Range(0, NPCManager.manage.npcStatus.Count - 1);
			while (!NPCManager.manage.npcStatus[num2].checkIfHasMovedIn())
			{
				num2 = Random.Range(0, NPCManager.manage.npcStatus.Count - 1);
			}
			PostOnBoard item = new PostOnBoard(Random.Range(-999999, 999999), num2, BoardPostType.Quest);
			attachedPosts.Add(item);
		}
	}

	public void checkExpiredAndRemove()
	{
		List<PostOnBoard> list = new List<PostOnBoard>();
		for (int i = 0; i < attachedPosts.Count; i++)
		{
			attachedPosts[i].daysToComplete--;
			attachedPosts[i].checkIfExpired();
			if ((attachedPosts[i].checkIfExpired() && attachedPosts[i].daysToComplete <= -1) || (attachedPosts[i].checkIfExpired() && attachedPosts[i].getPostTypeId() == BoardPostType.Quest))
			{
				list.Add(attachedPosts[i]);
				_ = attachedPosts[i].isInvestigation;
			}
		}
		foreach (PostOnBoard item in list)
		{
			randomPosts.RemoveAt(attachedPosts.IndexOf(item));
			attachedPosts.RemoveAt(attachedPosts.IndexOf(item));
		}
		QuestTracker.track.updateTasksEvent.Invoke();
	}

	public string getMissionText(int missionNo)
	{
		BullitenBoardPost postPostsById = attachedPosts[missionNo].getPostPostsById();
		if (attachedPosts[missionNo].requiredItem > -1)
		{
			string text = string.Concat("<sprite=12> ", (LocalizedString)"Bulletinboard/Collect", "\n<sprite=12> ", (LocalizedString)"Bulletinboard/Bring");
			if (attachedPosts[missionNo].checkAmountOfItemsInInv() >= attachedPosts[missionNo].requireItemAmount)
			{
				text = string.Concat("<sprite=13> ", (LocalizedString)"Bulletinboard/Collect", "\n<sprite=12> ", (LocalizedString)"Bulletinboard/Bring");
			}
			return text.Replace("<itemAmount>", attachedPosts[missionNo].requireItemAmount.ToString()).Replace("<itemName>", Inventory.Instance.allItems[attachedPosts[missionNo].requiredItem].getInvItemName()).Replace("<amountCollected>", "[" + attachedPosts[missionNo].checkAmountOfItemsInInv() + "/" + attachedPosts[missionNo].requireItemAmount + "]")
				.Replace("<npcName>", NPCManager.manage.NPCDetails[attachedPosts[missionNo].postedByNpcId].GetNPCName());
		}
		if (attachedPosts[missionNo].isTrade)
		{
			return ("<sprite=12> " + (LocalizedString)"Bulletinboard/Trade").Replace("<itemName>", attachedPosts[missionNo].getPostPostsById().getBoardRequestItem(missionNo, 1)).Replace("<npcName>", attachedPosts[missionNo].getPostedByName());
		}
		if (attachedPosts[missionNo].isCaptureTask)
		{
			return LocalisationMarkUp.RunMarkupCheckAnimal(("<sprite=12> " + (LocalizedString)"Bulletinboard/Capture").Replace("<animalName>", postPostsById.getBoardHuntRequestAnimal(missionNo)), attachedPosts[missionNo].animalToCapture);
		}
		if (attachedPosts[missionNo].isHuntingTask)
		{
			if (attachedPosts[missionNo].readyForNPC)
			{
				return LocalisationMarkUp.RunMarkupCheckAnimal(((string)(LocalizedString)"Bulletinboard/TalkTo").Replace("<npcName>", attachedPosts[missionNo].getPostedByName()), attachedPosts[missionNo].myHuntingChallenge.getAnimalId());
			}
			return LocalisationMarkUp.RunMarkupCheckAnimal(("<sprite=12> " + (LocalizedString)"Bulletinboard/Hunt").Replace("<animalName>", postPostsById.getBoardHuntRequestAnimal(missionNo)), attachedPosts[missionNo].myHuntingChallenge.getAnimalId());
		}
		if (attachedPosts[missionNo].isPhotoTask)
		{
			string requirementsNeededInPhoto = postPostsById.getRequirementsNeededInPhoto(missionNo, isMissionText: true);
			if (attachedPosts[missionNo].readyForNPC)
			{
				return ("<sprite=13> " + postPostsById.GetPhotoRequestMissionText(missionNo) + "\n<sprite=12> " + (LocalizedString)"Bulletinboard/Show").Replace("<animalsInPhoto>", requirementsNeededInPhoto).Replace("<npcName>", attachedPosts[missionNo].getPostedByName());
			}
			return string.Concat("<sprite=12> ", (LocalizedString)"Bulletinboard/Photograph", "\n<sprite=12> ", (LocalizedString)"Bulletinboard/Show").Replace("<animalsInPhoto>", requirementsNeededInPhoto).Replace("<npcName>", attachedPosts[missionNo].getPostedByName());
		}
		if (attachedPosts[missionNo].isInvestigation)
		{
			return "<sprite=12>" + (LocalizedString)"Bulletinboard/Investigate";
		}
		return "";
	}

	public void checkAllMissionsForAnimalKill(int animalKilled, Vector3 killedPosition)
	{
		float num = 3000f;
		int num2 = -1;
		for (int i = 0; i < attachedPosts.Count; i++)
		{
			if (!attachedPosts[i].checkIfExpired() && attachedPosts[i].checkIfAccepted() && !attachedPosts[i].completed && attachedPosts[i].checkIfHuntingQuestAndHasSameAnimalId(animalKilled) && attachedPosts[i].returnDistanceFromKilledPos(killedPosition) < num)
			{
				num = attachedPosts[i].returnDistanceFromKilledPos(killedPosition);
				num2 = i;
			}
		}
		if (num2 != -1)
		{
			attachedPosts[num2].readyForNPC = true;
			QuestTracker.track.updateTasksEvent.Invoke();
		}
	}

	public int getRewardForCapturingAnimalIncludingBulletinBoards(int animalDelivered, int variationDelivered)
	{
		foreach (PostOnBoard attachedPost in attachedPosts)
		{
			if (!attachedPost.checkIfExpired() && attachedPost.checkIfAnimalCapturedQuest(animalDelivered, variationDelivered))
			{
				return attachedPost.rewardAmount;
			}
		}
		return AnimalManager.manage.allAnimals[animalDelivered].dangerValue * 8;
	}

	public bool isInvestigationRequestOnBoard()
	{
		foreach (PostOnBoard attachedPost in attachedPosts)
		{
			if (attachedPost.isInvestigation)
			{
				return true;
			}
		}
		return false;
	}

	public void checkAllMissionsForPhotoRequestsOnNewPhoto(PhotoDetails latestPhoto)
	{
		board.checkAllMissionsForPhotosOnChange();
	}

	public void checkAllMissionsForPhotosOnChange()
	{
		foreach (PostOnBoard attachedPost in attachedPosts)
		{
			if (!attachedPost.checkIfAccepted() || !attachedPost.isPhotoTask)
			{
				continue;
			}
			attachedPost.readyForNPC = false;
			for (int i = 0; i < PhotoManager.manage.savedPhotos.Count; i++)
			{
				if (attachedPost.myPhotoChallenge.checkIfPhotoMeetsRequirements(PhotoManager.manage.savedPhotos[i]))
				{
					attachedPost.readyForNPC = true;
					break;
				}
			}
		}
		QuestTracker.track.updateTasksEvent.Invoke();
	}

	public void checkIfHuntingTargetNeedsToBeSpawned(int acceptedId)
	{
		if (attachedPosts[acceptedId].isHuntingTask && attachedPosts[acceptedId].myHuntingChallenge != null)
		{
			attachedPosts[acceptedId].myHuntingChallenge.addToChunkOnAccept(attachedPosts[acceptedId].daysToComplete);
		}
	}

	public void checkAllMissionsForItems()
	{
		foreach (PostOnBoard attachedPost in attachedPosts)
		{
			if (attachedPost.checkIfAccepted())
			{
				attachedPost.checkIfHasEnoughItems();
			}
		}
	}

	public PostOnBoard checkMissionsCompletedForNPC(int npcId)
	{
		foreach (PostOnBoard attachedPost in attachedPosts)
		{
			if (!attachedPost.checkIfExpired() && attachedPost.postedByNpcId == npcId && attachedPost.readyForNPC && !attachedPost.completed)
			{
				return attachedPost;
			}
		}
		return null;
	}

	public bool checkIfAnyUnread()
	{
		foreach (PostOnBoard attachedPost in attachedPosts)
		{
			if (!attachedPost.hasBeenRead)
			{
				return true;
			}
		}
		return false;
	}

	public bool huntingMissionAccepted()
	{
		for (int i = 0; i < attachedPosts.Count; i++)
		{
			if (attachedPosts[i].isHuntingTask && attachedPosts[i].checkIfAccepted() && !attachedPosts[i].completed && !attachedPosts[i].checkIfExpired())
			{
				return true;
			}
		}
		return false;
	}
}
