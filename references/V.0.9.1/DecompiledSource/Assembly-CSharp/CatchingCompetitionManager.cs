using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class CatchingCompetitionManager : NetworkBehaviour
{
	public static CatchingCompetitionManager manage;

	public bool inCompetition;

	[SyncVar]
	public bool compPossible;

	[SyncVar]
	public string currentHighScoreHolders;

	public CompPlayerScore[] npcScores;

	public int bugCompDay = 14;

	public InventoryItem[] bugCompTrophies;

	public int fishCompDay = 22;

	public InventoryItem[] fishCompTrophies;

	public InventoryItem desiredFishItem;

	public ConversationObject loggedScoreInBookConvo;

	public ConversationObject loggedScoreTooLateConvo;

	public ConversationObject notInCompConvo_BugConvo;

	public ConversationObject inCompConvo_BugConvo;

	public ConversationObject signUpForCompBook_BugConvo;

	public ConversationObject signedUpForCompBook_BugConvo;

	public ConversationObject compClosedBook_BugConvo;

	public ConversationObject signUpButNotStartedBook_BugConvo;

	public ConversationObject CompEnded_BugConvo;

	public ConversationObject inCompButNotStarted_BugConvo;

	public ConversationObject notInCompConvo_FishConvo;

	public ConversationObject inCompConvo_FishConvo;

	public ConversationObject signUpForCompBook_FishConvo;

	public ConversationObject signedUpForCompBook_FishConvo;

	public ConversationObject compClosedBook_FishConvo;

	public ConversationObject signUpButNotStartedBook_FishConvo;

	public ConversationObject CompEnded_FishConvo;

	public ConversationObject inCompButNotStarted_FishConvo;

	public ASound compWhistleSound;

	public ASound compEndsWhistleSound;

	private int compStartTime = 9;

	private int compEndTime = 16;

	private int currentHourScoring;

	private bool compFinished;

	public bool NetworkcompPossible
	{
		get
		{
			return compPossible;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref compPossible))
			{
				bool flag = compPossible;
				SetSyncVar(value, ref compPossible, 1uL);
			}
		}
	}

	public string NetworkcurrentHighScoreHolders
	{
		get
		{
			return currentHighScoreHolders;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref currentHighScoreHolders))
			{
				string text = currentHighScoreHolders;
				SetSyncVar(value, ref currentHighScoreHolders, 2uL);
			}
		}
	}

	public string getStartTime()
	{
		return compStartTime + ":00 am";
	}

	public string getEndTime()
	{
		return compEndTime - 12 + ":00 pm";
	}

	private void Awake()
	{
		manage = this;
	}

	public override void OnStartServer()
	{
		npcScores = new CompPlayerScore[NPCManager.manage.NPCDetails.Length];
		for (int i = 0; i < npcScores.Length; i++)
		{
			npcScores[i] = new CompPlayerScore(i);
		}
	}

	public override void OnStartClient()
	{
		UpdateCompDetails();
	}

	public bool competitionActive()
	{
		if (RealWorldTimeLight.time.currentHour >= compStartTime)
		{
			return RealWorldTimeLight.time.currentHour < compEndTime;
		}
		return false;
	}

	private ConversationObject GetBugKeeperConversation()
	{
		if (inCompetition && competitionActive())
		{
			return inCompConvo_BugConvo;
		}
		if (inCompetition && !competitionActive() && RealWorldTimeLight.time.currentHour < compStartTime)
		{
			return inCompButNotStarted_BugConvo;
		}
		if (!competitionActive() && RealWorldTimeLight.time.currentHour >= compEndTime)
		{
			return CompEnded_BugConvo;
		}
		return notInCompConvo_BugConvo;
	}

	private ConversationObject GetFishKeeperConversation()
	{
		if (inCompetition && competitionActive())
		{
			return inCompConvo_FishConvo;
		}
		if (inCompetition && !competitionActive() && RealWorldTimeLight.time.currentHour < compStartTime)
		{
			return inCompButNotStarted_FishConvo;
		}
		if (!competitionActive() && RealWorldTimeLight.time.currentHour >= compEndTime)
		{
			return CompEnded_FishConvo;
		}
		return notInCompConvo_FishConvo;
	}

	public string getHighestScores()
	{
		return currentHighScoreHolders;
	}

	public void SetCorrectConversation()
	{
		NPCManager.manage.NPCDetails[13].keeperConvos = GetBugKeeperConversation();
		NPCManager.manage.NPCDetails[14].keeperConvos = GetFishKeeperConversation();
	}

	public ConversationObject getBookConvo()
	{
		if (isBugCompDay(WorldManager.Instance.day, WorldManager.Instance.week, WorldManager.Instance.month))
		{
			if (RealWorldTimeLight.time.currentHour == 0 || RealWorldTimeLight.time.currentHour >= compEndTime)
			{
				return compClosedBook_BugConvo;
			}
			if (inCompetition && RealWorldTimeLight.time.currentHour < compStartTime)
			{
				return signUpButNotStartedBook_BugConvo;
			}
			if (inCompetition)
			{
				return signedUpForCompBook_BugConvo;
			}
			return signUpForCompBook_BugConvo;
		}
		if (isFishCompDay(WorldManager.Instance.day, WorldManager.Instance.week, WorldManager.Instance.month))
		{
			if (RealWorldTimeLight.time.currentHour == 0 || RealWorldTimeLight.time.currentHour >= compEndTime)
			{
				return compClosedBook_FishConvo;
			}
			if (inCompetition && RealWorldTimeLight.time.currentHour < compStartTime)
			{
				return signUpButNotStartedBook_FishConvo;
			}
			if (inCompetition)
			{
				return signedUpForCompBook_FishConvo;
			}
			return signUpForCompBook_FishConvo;
		}
		return null;
	}

	public bool IsBugCompToday()
	{
		return isBugCompDay(WorldManager.Instance.day, WorldManager.Instance.week, WorldManager.Instance.month);
	}

	public bool IsFishCompToday()
	{
		return isFishCompDay(WorldManager.Instance.day, WorldManager.Instance.week, WorldManager.Instance.month);
	}

	public bool isBugCompDay(int checkDay, int checkWeek, int checkMonth)
	{
		if (!compPossible)
		{
			return false;
		}
		if (checkDay + (checkWeek - 1) * 7 == bugCompDay && (checkMonth == 4 || checkMonth == 2))
		{
			return true;
		}
		return false;
	}

	public bool isBugCompDay(int calendarDay)
	{
		if (!compPossible)
		{
			return false;
		}
		if (calendarDay == bugCompDay && (WorldManager.Instance.month == 4 || WorldManager.Instance.month == 2))
		{
			return true;
		}
		return false;
	}

	public bool isFishCompDay(int checkDay, int checkWeek, int checkMonth)
	{
		if (!compPossible)
		{
			return false;
		}
		if (checkDay + (checkWeek - 1) * 7 == fishCompDay && (checkMonth == 1 || checkMonth == 3))
		{
			return true;
		}
		return false;
	}

	public bool isFishCompDay(int calendarDay)
	{
		if (!compPossible)
		{
			return false;
		}
		if (calendarDay == fishCompDay && (WorldManager.Instance.month == 1 || WorldManager.Instance.month == 3))
		{
			return true;
		}
		return false;
	}

	public void signUpForComp()
	{
		inCompetition = true;
		StartCoroutine(compTimerAndPointCounter());
		SetCorrectConversation();
	}

	private IEnumerator compTimerAndPointCounter()
	{
		while (RealWorldTimeLight.time.currentHour < compStartTime)
		{
			yield return null;
		}
		if (NetworkMapSharer.Instance.nextDayIsReady)
		{
			if (IsFishCompToday())
			{
				NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("FishingCompHasStarted"), ConversationGenerator.generate.GetNotificationText("GoodLuck"), compWhistleSound);
				MusicManager.manage.playCompMusic();
			}
			else
			{
				NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("BugCompHasStarted"), ConversationGenerator.generate.GetNotificationText("GoodLuck"), compWhistleSound);
				MusicManager.manage.playCompMusic();
			}
		}
		SetCorrectConversation();
		while (RealWorldTimeLight.time.currentHour < compEndTime - 1)
		{
			yield return null;
		}
		if (RealWorldTimeLight.time.currentHour == compEndTime - 1)
		{
			NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("OneHourRemaining"), ConversationGenerator.generate.GetNotificationText("LogYourScore"));
		}
		while (RealWorldTimeLight.time.currentHour < compEndTime)
		{
			yield return null;
		}
		if (NetworkMapSharer.Instance.nextDayIsReady)
		{
			if (IsFishCompToday())
			{
				NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("FishingCompHasEnded"), ConversationGenerator.generate.GetNotificationText("LetsSeeWhoWon"), compEndsWhistleSound);
			}
			else
			{
				NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("BugCompHasEnded"), ConversationGenerator.generate.GetNotificationText("LetsSeeWhoWon"), compEndsWhistleSound);
			}
		}
		MusicManager.manage.stopCompMusic();
		SetCorrectConversation();
	}

	public void UpdateCompDetails()
	{
		inCompetition = false;
		if (base.isServer)
		{
			if (!compPossible && NPCManager.manage.npcStatus[9].hasMovedIn)
			{
				NetworkcompPossible = true;
			}
			for (int i = 0; i < NetworkNavMesh.nav.charsConnected.Count; i++)
			{
				NetworkNavMesh.nav.charsConnected[i].GetComponent<EquipItemToChar>().NetworkcompScore = 0f;
			}
		}
		if (IsBugCompToday() || IsFishCompToday())
		{
			if (base.isServer)
			{
				StartCoroutine(RunNPCsForComp());
			}
			if ((bool)NetworkMapSharer.Instance.localChar)
			{
				NetworkMapSharer.Instance.localChar.myEquip.localCompScore = 0f;
			}
			SetCorrectConversation();
		}
	}

	private IEnumerator RunNPCsForComp()
	{
		int runningOnDay = WorldManager.Instance.day;
		WaitForSeconds npcUpdateWait = new WaitForSeconds(1f);
		currentHourScoring = compStartTime;
		compFinished = false;
		while (runningOnDay == WorldManager.Instance.day && RealWorldTimeLight.time.currentHour <= compEndTime)
		{
			yield return npcUpdateWait;
			if (!compFinished)
			{
				if (currentHourScoring != RealWorldTimeLight.time.currentHour)
				{
					currentHourScoring = RealWorldTimeLight.time.currentHour;
					UpdateNPCsScores(currentHourScoring);
					updateCurrentLeader();
				}
				continue;
			}
			break;
		}
	}

	public void FinishComp()
	{
		if (IsBugCompToday())
		{
			compFinished = true;
			while (currentHourScoring <= compEndTime)
			{
				currentHourScoring++;
				UpdateNPCsScores(currentHourScoring);
			}
			SendWinnersLetters();
		}
		if (IsFishCompToday())
		{
			compFinished = true;
			while (currentHourScoring <= compEndTime)
			{
				currentHourScoring++;
				UpdateNPCsScores(currentHourScoring);
			}
			SendWinnersLetters();
		}
	}

	public void SendWinnersLetters()
	{
		List<CompPlayerScore> list = new List<CompPlayerScore>();
		for (int i = 0; i < NetworkNavMesh.nav.charsConnected.Count; i++)
		{
			EquipItemToChar component = NetworkNavMesh.nav.charsConnected[i].GetComponent<EquipItemToChar>();
			if ((bool)component)
			{
				list.Add(new CompPlayerScore(component));
			}
		}
		for (int j = 0; j < npcScores.Length; j++)
		{
			list.Add(npcScores[j]);
		}
		list = giverCompetitorsTheirPlace(list);
		for (int k = 0; k < list.Count; k++)
		{
			if (list[k].id == -1 && list[k].currentPlace == 0)
			{
				if (IsBugCompToday())
				{
					NetworkMapSharer.Instance.TargetSendBugCompLetter(NetworkIdentity.spawned[list[k].playerNetId].connectionToClient, 0);
				}
				else
				{
					NetworkMapSharer.Instance.TargetSendFishingCompLetter(NetworkIdentity.spawned[list[k].playerNetId].connectionToClient, 0);
				}
			}
			else if (list[k].id == -1 && list[k].currentPlace == 1)
			{
				if (IsBugCompToday())
				{
					NetworkMapSharer.Instance.TargetSendBugCompLetter(NetworkIdentity.spawned[list[k].playerNetId].connectionToClient, 1);
				}
				else
				{
					NetworkMapSharer.Instance.TargetSendFishingCompLetter(NetworkIdentity.spawned[list[k].playerNetId].connectionToClient, 1);
				}
			}
			else if (list[k].id == -1 && list[k].currentPlace == 2)
			{
				if (IsBugCompToday())
				{
					NetworkMapSharer.Instance.TargetSendBugCompLetter(NetworkIdentity.spawned[list[k].playerNetId].connectionToClient, 2);
				}
				else
				{
					NetworkMapSharer.Instance.TargetSendFishingCompLetter(NetworkIdentity.spawned[list[k].playerNetId].connectionToClient, 2);
				}
			}
		}
	}

	public List<CompPlayerScore> giverCompetitorsTheirPlace(List<CompPlayerScore> allScores)
	{
		allScores.Sort(sortPlayersScores);
		float[] array = new float[3] { -1f, -1f, -1f };
		array[0] = allScores[0].score;
		for (int i = 0; i < allScores.Count; i++)
		{
			if (allScores[i].score == 0f)
			{
				allScores[i].currentPlace = 5;
			}
			else if (allScores[i].score >= array[0])
			{
				allScores[i].currentPlace = 0;
			}
			else if (allScores[i].score >= array[1])
			{
				array[1] = allScores[i].score;
				allScores[i].currentPlace = 1;
			}
			else if (allScores[i].score >= array[2])
			{
				array[2] = allScores[i].score;
				allScores[i].currentPlace = 2;
			}
			else
			{
				allScores[i].currentPlace = 5;
			}
		}
		return allScores;
	}

	public float getScoreForBug(int bugInvId)
	{
		float num = 0.5f + (float)Inventory.Instance.allItems[bugInvId].bug.mySeason.myRarity + (float)Mathf.RoundToInt(Inventory.Instance.allItems[bugInvId].value / 800);
		if (!Inventory.Instance.allItems[bugInvId].bug.isFlyingBug)
		{
			num *= 1.8f;
		}
		return Mathf.Clamp(Mathf.RoundToInt(num), 1, 15);
	}

	public float getScoreForFish(int fishInvId)
	{
		if (desiredFishItem.getItemId() == fishInvId)
		{
			return 1f;
		}
		return 0f;
	}

	public void UpdateNPCsScores(int currentHourScoringFor)
	{
		for (int i = 0; i < npcScores.Length; i++)
		{
			if (!NPCManager.manage.npcStatus[i].checkIfHasMovedIn() || ((NPCManager.manage.NPCDetails[i].mySchedual.dayOff[WorldManager.Instance.day - 1] || NPCManager.manage.NPCDetails[i].mySchedual.todaysSchedual[currentHourScoringFor] != 0) && (!NPCManager.manage.NPCDetails[i].mySchedual.dayOff[WorldManager.Instance.day - 1] || NPCManager.manage.NPCDetails[i].mySchedual.dayOffSchedule[currentHourScoringFor] != 0)) || currentHourScoringFor <= compStartTime || currentHourScoringFor > compEndTime)
			{
				continue;
			}
			if (IsBugCompToday())
			{
				int num = Random.Range(1, 15) + Random.Range(1, 5);
				if (currentHourScoringFor == compEndTime)
				{
					num = Random.Range(1, 7) + Random.Range(1, 4);
				}
				while (num > 0)
				{
					npcScores[i].addToScore(getScoreForBug(getARandomBugForVillager()));
					num--;
				}
			}
			else if (IsFishCompToday())
			{
				int num2 = Random.Range(0, 3);
				npcScores[i].addToScore(num2);
			}
		}
	}

	public void updateCurrentLeader()
	{
		List<CompPlayerScore> list = new List<CompPlayerScore>();
		for (int i = 0; i < NetworkNavMesh.nav.charsConnected.Count; i++)
		{
			EquipItemToChar component = NetworkNavMesh.nav.charsConnected[i].GetComponent<EquipItemToChar>();
			if ((bool)component)
			{
				list.Add(new CompPlayerScore(component));
			}
		}
		for (int j = 0; j < npcScores.Length; j++)
		{
			list.Add(npcScores[j]);
		}
		list = giverCompetitorsTheirPlace(list);
		NetworkcurrentHighScoreHolders = getCurrentLeaderText(list);
	}

	public string getCurrentLeaderText(List<CompPlayerScore> allScores)
	{
		string text = "";
		string text2 = getPosText(0);
		string text3 = getPosText(1);
		string text4 = getPosText(2);
		string text5 = "\n";
		string text6 = "\n";
		string text7 = "\n";
		for (int i = 0; i < allScores.Count; i++)
		{
			if (allScores[i].currentPlace == -1)
			{
				continue;
			}
			if (allScores[i].currentPlace == 0 && allScores[i].score != 0f)
			{
				if (text5 == "\n")
				{
					text5 = " - " + string.Format(ConversationGenerator.generate.GetDescriptionDetails("CompPoints"), allScores[i].score) + "\n";
				}
				text2 = ((!(text2 == getPosText(0))) ? (text2 + " & " + getPlayerNameForScore(allScores[i])) : (text2 + getPlayerNameForScore(allScores[i])));
			}
			if (allScores[i].currentPlace == 1 && allScores[i].score != 0f)
			{
				if (text6 == "\n")
				{
					text6 = " - " + string.Format(ConversationGenerator.generate.GetDescriptionDetails("CompPoints"), allScores[i].score) + "\n";
				}
				text3 = ((!(text3 == getPosText(1))) ? (text3 + " & " + getPlayerNameForScore(allScores[i])) : (text3 + getPlayerNameForScore(allScores[i])));
			}
			if (allScores[i].currentPlace == 2 && allScores[i].score != 0f)
			{
				if (text7 == "\n")
				{
					text7 = " - " + string.Format(ConversationGenerator.generate.GetDescriptionDetails("CompPoints"), allScores[i].score) + "\n";
				}
				text4 = ((!(text4 == getPosText(2))) ? (text4 + " & " + getPlayerNameForScore(allScores[i])) : (text4 + getPlayerNameForScore(allScores[i])));
			}
		}
		text = text2 + text5 + text3 + text6 + text4 + text7;
		if (text == "")
		{
			text = ConversationGenerator.generate.GetDescriptionDetails("CompNoScoreLogged");
		}
		return text;
	}

	public string getPosText(int positionOnList)
	{
		return positionOnList switch
		{
			0 => string.Format(ConversationGenerator.generate.GetDescriptionDetails("Placing_First"), "1") + ": ", 
			1 => string.Format(ConversationGenerator.generate.GetDescriptionDetails("Placing_Second"), "2") + ": ", 
			_ => string.Format(ConversationGenerator.generate.GetDescriptionDetails("Placing_Third"), "3") + ": ", 
		};
	}

	public string getPlayerNameForScore(CompPlayerScore scorer)
	{
		if (scorer.id != -1)
		{
			return UIAnimationManager.manage.GetItemColorTag(NPCManager.manage.NPCDetails[scorer.id].GetNPCName());
		}
		return UIAnimationManager.manage.GetCharacterNameTag(scorer.playerName);
	}

	public int sortPlayersScores(CompPlayerScore a, CompPlayerScore b)
	{
		if (a.score < b.score)
		{
			return 1;
		}
		if (a.score > b.score)
		{
			return -1;
		}
		return 0;
	}

	public int getARandomBugForVillager()
	{
		return Random.Range(0, 5) switch
		{
			0 => AnimalManager.manage.bushlandBugs.getInventoryItem().getItemId(), 
			1 => AnimalManager.manage.desertBugs.getInventoryItem().getItemId(), 
			2 => AnimalManager.manage.topicalBugs.getInventoryItem().getItemId(), 
			3 => AnimalManager.manage.plainsBugs.getInventoryItem().getItemId(), 
			_ => AnimalManager.manage.pineLandBugs.getInventoryItem().getItemId(), 
		};
	}

	private void MirrorProcessed()
	{
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(compPossible);
			writer.WriteString(currentHighScoreHolders);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(compPossible);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteString(currentHighScoreHolders);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			bool flag = compPossible;
			NetworkcompPossible = reader.ReadBool();
			string text = currentHighScoreHolders;
			NetworkcurrentHighScoreHolders = reader.ReadString();
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			bool flag2 = compPossible;
			NetworkcompPossible = reader.ReadBool();
		}
		if ((num & 2L) != 0L)
		{
			string text2 = currentHighScoreHolders;
			NetworkcurrentHighScoreHolders = reader.ReadString();
		}
	}
}
