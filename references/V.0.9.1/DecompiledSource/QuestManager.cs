using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
	public static QuestManager manage;

	public GameObject questNotificationPrefab;

	public Quest[] allQuests;

	public bool[] isQuestAccepted;

	public bool[] isQuestCompleted;

	public List<QuestNotification> currentQuestsNotifications;

	public Transform QuestScrollWindow;

	public Text QuestSelectedName;

	public Text QuestSelectedDescription;

	public Transform QuestSelectedRequiredItemsWindow;

	public Text QuestSelectedReward;

	public Quest lastQuestTalkingAbout;

	private void Awake()
	{
		manage = this;
		isQuestCompleted = new bool[allQuests.Length];
		isQuestAccepted = new bool[allQuests.Length];
	}

	public void addAQuest(Quest newQuest)
	{
	}

	public void completeAQuest(Quest completedQuest)
	{
	}

	public ConversationObject checkIfThereIsAQuestToGive(int NPCNo)
	{
		if (!NetworkMapSharer.Instance.isServer)
		{
			return null;
		}
		if (NPCNo < 0)
		{
			return null;
		}
		TownManager.manage.calculateTownScore();
		for (int i = 0; i < isQuestCompleted.Length; i++)
		{
			if (NPCNo == allQuests[i].offeredByNpc && allQuests[i].berkoniumQuest && !isQuestAccepted[i])
			{
				if (CatalogueManager.manage.collectedItem[allQuests[i].requiredItems[0].getItemId()])
				{
					isQuestAccepted[i] = true;
					return allQuests[i].questAcceptedConvo;
				}
			}
			else if (NPCNo == allQuests[i].offeredByNpc && allQuests[i].bandstandQuest && !isQuestCompleted[i])
			{
				if (NPCManager.manage.getNoOfNPCsMovedIn() >= 3)
				{
					isQuestAccepted[i] = true;
					completeQuest(i);
					isQuestAccepted[i] = true;
					return allQuests[i].questAcceptedConvo;
				}
			}
			else if (NPCNo == allQuests[i].offeredByNpc && allQuests[i].airportQuest && !isQuestCompleted[i])
			{
				if (WorldManager.Instance.year >= 2 && NPCManager.manage.getNoOfNPCsMovedIn() >= 11)
				{
					isQuestAccepted[i] = true;
					completeQuest(i);
					isQuestAccepted[i] = true;
					return allQuests[i].questAcceptedConvo;
				}
			}
			else if (NPCNo == allQuests[i].offeredByNpc && allQuests[i].guestHouseQuest && !isQuestCompleted[i])
			{
				if (TownManager.manage.getCurrentHouseStage() >= 1)
				{
					isQuestAccepted[i] = true;
					completeQuest(i);
					isQuestAccepted[i] = true;
					return allQuests[i].questAcceptedConvo;
				}
			}
			else if (NPCNo == allQuests[i].offeredByNpc && allQuests[i].attractResidentsQuest && !isQuestCompleted[i])
			{
				if (WorldManager.Instance.month <= 1 && WorldManager.Instance.year <= 1 && WorldManager.Instance.day + WorldManager.Instance.week * 7 < 16)
				{
					continue;
				}
				if (!isQuestAccepted[i] && NPCManager.manage.getNoOfNPCsMovedIn() < 5)
				{
					isQuestAccepted[i] = true;
					return allQuests[i].questAcceptedConvo;
				}
				if (!isQuestCompleted[i] && NPCManager.manage.getNoOfNPCsMovedIn() >= 5)
				{
					if (BuildingManager.manage.currentlyMoving == -1 || BuildingManager.manage.currentlyMoving != allQuests[i].requiredBuilding[0].tileObjectId)
					{
						completeQuest(i);
						isQuestAccepted[i] = true;
						return allQuests[i].completedConvo;
					}
					return null;
				}
			}
			else if (NPCNo == allQuests[i].offeredByNpc && !isQuestCompleted[i] && allQuests[i].teleporterQuest)
			{
				if (!NPCManager.manage.npcStatus[allQuests[i].offeredByNpc].checkIfHasMovedIn())
				{
					continue;
				}
				int num = 0;
				if (NetworkMapSharer.Instance.northOn)
				{
					num++;
				}
				if (NetworkMapSharer.Instance.eastOn)
				{
					num++;
				}
				if (NetworkMapSharer.Instance.southOn)
				{
					num++;
				}
				if (NetworkMapSharer.Instance.westOn)
				{
					num++;
				}
				if (num >= 2)
				{
					isQuestAccepted[i] = true;
					completeQuest(i);
					if ((bool)allQuests[i].questConvo)
					{
						return allQuests[i].questConvo;
					}
				}
			}
			else if (NPCNo == allQuests[i].offeredByNpc && !isQuestCompleted[i] && allQuests[i].townBeautyLevelRequired > 0f)
			{
				if (TownManager.manage.townBeautyLevel >= allQuests[i].townBeautyLevelRequired)
				{
					isQuestAccepted[i] = true;
					completeQuest(i);
					if ((bool)allQuests[i].questConvo)
					{
						return allQuests[i].questConvo;
					}
				}
			}
			else if (NPCNo == allQuests[i].offeredByNpc && !isQuestCompleted[i] && allQuests[i].townEcconnomyLevelRequired > 0f)
			{
				if (TownManager.manage.townEconomyLevel >= allQuests[i].townEcconnomyLevelRequired)
				{
					isQuestAccepted[i] = true;
					completeQuest(i);
					if ((bool)allQuests[i].questConvo)
					{
						return allQuests[i].questConvo;
					}
				}
			}
			else
			{
				if (NPCNo != allQuests[i].offeredByNpc || isQuestCompleted[i] || NPCManager.manage.npcStatus[NPCNo].relationshipLevel < allQuests[i].relationshipLevelNeeded || NPCNo != allQuests[i].offeredByNpc)
				{
					continue;
				}
				lastQuestTalkingAbout = allQuests[i];
				if (!isQuestAccepted[i])
				{
					if ((bool)allQuests[i].questConvo)
					{
						return allQuests[i].questConvo;
					}
					continue;
				}
				if (!allQuests[i].checkIfComplete())
				{
					if ((bool)allQuests[i].questAcceptedConvo && !allQuests[i].berkoniumQuest)
					{
						return allQuests[i].questAcceptedConvo;
					}
					continue;
				}
				completeQuest(i);
				if ((bool)allQuests[i].completedConvo)
				{
					return allQuests[i].completedConvo;
				}
			}
		}
		return null;
	}

	public void completeQuest(int questNo)
	{
		if (allQuests[questNo].berkoniumQuest)
		{
			CraftsmanManager.manage.craftsmanNowHasBerkonium();
		}
		isQuestCompleted[questNo] = true;
		DailyTaskGenerator.generate.doATask(allQuests[questNo].doTaskOnComplete);
		if (allQuests[questNo].guestHouseQuest)
		{
			NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("GuestHouseAvaliable"), ConversationGenerator.generate.GetNotificationText("TalkToFletchForDeeds"), SoundManager.Instance.notificationSound);
			for (int i = 0; i < allQuests[questNo].giveItemsOnAccept.Length; i++)
			{
				DeedManager.manage.unlockDeed(allQuests[questNo].giveItemsOnAccept[i]);
				CatalogueManager.manage.collectedItem[allQuests[questNo].giveItemsOnAccept[i].getItemId()] = false;
			}
		}
		else if (allQuests[questNo].townHallQuest || allQuests[questNo].attractResidentsQuest)
		{
			allQuests[questNo].upgradeBaseTent();
		}
		else if ((bool)allQuests[questNo].deedUnlockedOnAccept)
		{
			NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NewDeedAvailable"), ConversationGenerator.generate.GetNotificationText("TalkToFletchForDeeds"), SoundManager.Instance.notificationSound);
			DeedManager.manage.unlockDeed(allQuests[questNo].deedUnlockedOnAccept);
		}
		if (allQuests[questNo].acceptQuestOnComplete != -1)
		{
			isQuestAccepted[allQuests[questNo].acceptQuestOnComplete] = true;
		}
		if (!allQuests[questNo].guestHouseQuest)
		{
			for (int j = 0; j < allQuests[questNo].rewardOnComplete.Length; j++)
			{
				GiftedItemWindow.gifted.addToListToBeGiven(Inventory.Instance.getInvItemId(allQuests[questNo].rewardOnComplete[j]), allQuests[questNo].rewardStacksGiven[j]);
			}
			InventoryItem[] unlockRecipeOnComplete = allQuests[questNo].unlockRecipeOnComplete;
			foreach (InventoryItem item in unlockRecipeOnComplete)
			{
				GiftedItemWindow.gifted.addRecipeToUnlock(Inventory.Instance.getInvItemId(item));
			}
		}
		GiftedItemWindow.gifted.openWindowAndGiveItems();
	}

	public void acceptQuestLastTalkedAbout()
	{
		for (int i = 0; i < isQuestAccepted.Length; i++)
		{
			if (!(allQuests[i] == lastQuestTalkingAbout) || isQuestAccepted[i])
			{
				continue;
			}
			isQuestAccepted[i] = true;
			if (!QuestTracker.track.hasPinnedMission())
			{
				QuestTracker.track.pinTheTask(QuestTracker.typeOfTask.Quest, i);
			}
			for (int j = 0; j < lastQuestTalkingAbout.giveItemsOnAccept.Length; j++)
			{
				if (j < lastQuestTalkingAbout.amountGivenOnAccept.Length)
				{
					GiftedItemWindow.gifted.addToListToBeGiven(Inventory.Instance.getInvItemId(lastQuestTalkingAbout.giveItemsOnAccept[j]), lastQuestTalkingAbout.amountGivenOnAccept[j]);
				}
				else
				{
					GiftedItemWindow.gifted.addToListToBeGiven(Inventory.Instance.getInvItemId(lastQuestTalkingAbout.giveItemsOnAccept[j]), 1);
				}
			}
			GiftedItemWindow.gifted.openWindowAndGiveItems();
			if ((bool)lastQuestTalkingAbout.deedUnlockedOnAccept)
			{
				DeedManager.manage.unlockDeed(lastQuestTalkingAbout.deedUnlockedOnAccept);
			}
		}
	}

	public void completeQuestLastTalkedAbout()
	{
		for (int i = 0; i < isQuestAccepted.Length; i++)
		{
			if (allQuests[i] == lastQuestTalkingAbout && !isQuestCompleted[i])
			{
				completeQuest(i);
			}
		}
	}

	public string listRequiredItemsInQuestLastTalkedAbout()
	{
		string text = "";
		for (int i = 0; i < lastQuestTalkingAbout.requiredItems.Length; i++)
		{
			text = ((i == lastQuestTalkingAbout.requiredItems.Length - 1) ? (text + ConversationGenerator.generate.GetItemAmount(lastQuestTalkingAbout.requiredStacks[i], lastQuestTalkingAbout.requiredItems[i].getInvItemName(lastQuestTalkingAbout.requiredStacks[i])) + " ") : (text + ConversationGenerator.generate.GetItemAmount(lastQuestTalkingAbout.requiredStacks[i], lastQuestTalkingAbout.requiredItems[i].getInvItemName(lastQuestTalkingAbout.requiredStacks[i])) + ", "));
		}
		return text;
	}
}
