using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestButton : MonoBehaviour
{
	public RectTransform buttonSize;

	public TextMeshProUGUI buttonText;

	public TextMeshProUGUI missionSteps;

	public bool isQuestButton;

	public bool isMainQuestButton;

	public bool trackingRecipeButton;

	private int representingNo;

	public Sprite bulletinBoardSprite;

	public Sprite requestSprite;

	public Sprite mainQuestSprite;

	public Image npcIcon;

	public Image missionTypeIcon;

	public void setUpMainQuest(int questNo)
	{
		representingNo = questNo;
		isMainQuestButton = true;
		if (isMainQuestButton)
		{
			setUp(questButton: true, questNo);
		}
	}

	public void setUp(bool questButton, int questNo)
	{
		representingNo = questNo;
		isQuestButton = questButton;
		if (isMainQuestButton)
		{
			missionTypeIcon.sprite = mainQuestSprite;
			buttonText.text = QuestManager.manage.allQuests[questNo].GetQuestTitleText();
			missionSteps.text = QuestManager.manage.allQuests[questNo].getMissionObjText();
			if (QuestManager.manage.allQuests[questNo].offeredByNpc > 0 && QuestManager.manage.allQuests[questNo].offeredByNpc <= 11)
			{
				npcIcon.sprite = NPCManager.manage.NPCDetails[QuestManager.manage.allQuests[questNo].offeredByNpc].npcSprite;
			}
			else
			{
				npcIcon.sprite = NPCManager.manage.NPCDetails[QuestManager.manage.allQuests[questNo].offeredByNpc].GetNPCSprite(QuestManager.manage.allQuests[questNo].offeredByNpc);
			}
		}
		else if (questButton)
		{
			missionTypeIcon.sprite = bulletinBoardSprite;
			buttonText.text = BulletinBoard.board.attachedPosts[questNo].getTitleText(questNo);
			missionSteps.text = BulletinBoard.board.getMissionText(questNo);
			if (BulletinBoard.board.attachedPosts[questNo].postedByNpcId < 0)
			{
				npcIcon.sprite = QuestTracker.track.bulletinBoardMissionIcon;
			}
			else if (BulletinBoard.board.attachedPosts[questNo].postedByNpcId > 0 && BulletinBoard.board.attachedPosts[questNo].postedByNpcId <= 11)
			{
				npcIcon.sprite = NPCManager.manage.NPCDetails[BulletinBoard.board.attachedPosts[questNo].postedByNpcId].npcSprite;
			}
			else
			{
				npcIcon.sprite = NPCManager.manage.NPCDetails[BulletinBoard.board.attachedPosts[questNo].postedByNpcId].GetNPCSprite(BulletinBoard.board.attachedPosts[questNo].postedByNpcId);
			}
		}
		else
		{
			missionTypeIcon.sprite = requestSprite;
			buttonText.text = ConversationGenerator.generate.GetQuestTrackerText("RequestForNPCName").Replace("<npcName>", NPCManager.manage.NPCDetails[questNo].GetNPCName());
			missionSteps.text = NPCManager.manage.NPCRequests[questNo].getMissionText(questNo);
			if (questNo > 0 && questNo <= 11)
			{
				npcIcon.sprite = NPCManager.manage.NPCDetails[questNo].npcSprite;
			}
			else
			{
				npcIcon.sprite = NPCManager.manage.NPCDetails[questNo].GetNPCSprite(questNo);
			}
		}
		missionSteps.text = missionSteps.text.Replace("<sprite=12>", "<sprite=16>");
		missionSteps.text = missionSteps.text.Replace("<sprite=13>", "<sprite=17>");
		missionSteps.ForceMeshUpdate();
		float preferredHeight = missionSteps.preferredHeight;
		float num = 65f;
		float y = preferredHeight + num;
		buttonSize.sizeDelta = new Vector2(buttonSize.sizeDelta.x, y);
	}

	public void onPress()
	{
		if (trackingRecipeButton)
		{
			QuestTracker.track.displayTrackingRecipe();
		}
		else if (isMainQuestButton)
		{
			QuestTracker.track.displayMainQuest(representingNo);
		}
		else if (isQuestButton)
		{
			QuestTracker.track.displayQuest(representingNo);
		}
		else
		{
			QuestTracker.track.displayRequest(representingNo);
		}
		QuestTracker.track.questDescriptionWindow.SetActive(value: false);
		QuestTracker.track.questDescriptionWindow.SetActive(value: true);
	}
}
