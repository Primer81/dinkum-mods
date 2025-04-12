using System.Collections;
using I2.Loc;
using UnityEngine;

public class NpcBubbleTalk : MonoBehaviour
{
	[Header("Rayne")]
	public NpcBackAndForth[] npc0Convo;

	[Header("John")]
	public NpcBackAndForth[] npc1Convo;

	[Header("Franklyn")]
	public NpcBackAndForth[] npc2Convo;

	[Header("Melvin")]
	public NpcBackAndForth[] npc3Convo;

	[Header("Clover")]
	public NpcBackAndForth[] npc4Convo;

	[Header("Ned")]
	public NpcBackAndForth[] npc5Convo;

	[Header("Fletch")]
	public NpcBackAndForth[] npc6Convo;

	[Header("Irwin")]
	public NpcBackAndForth[] npc7Convo;

	[Header("Sally")]
	public NpcBackAndForth[] npc8Convo;

	[Header("Theodore")]
	public NpcBackAndForth[] npc9Convo;

	[Header("Milburn")]
	public NpcBackAndForth[] npc10Convo;

	[Header("Jimmy")]
	public NpcBackAndForth[] npc11Convo;

	[Header("Sheila")]
	public NpcBackAndForth[] npc12Convo;

	[Header("Random Fallbacks")]
	public NpcBackAndForth[] morningFallbacks;

	public NpcBackAndForth[] dayFallbacks;

	public NpcBackAndForth[] nightFallbacks;

	public NpcBackAndForth[] rainingFallbacks;

	public string SpeakersName = "SpeakersName";

	private string conversationTarget = "NPCBubbleDialogue";

	public NpcBackAndForth getAConvoToSay(NPCAI startingConvo, NPCAI talkingTo)
	{
		if (startingConvo.myId.NPCNo <= 4 && Random.Range(0, 20) <= 10)
		{
			if (talkingTo.myId.NPCNo == 0)
			{
				return npc0Convo[Random.Range(0, npc0Convo.Length)];
			}
			if (talkingTo.myId.NPCNo == 1)
			{
				return npc1Convo[Random.Range(0, npc1Convo.Length)];
			}
			if (talkingTo.myId.NPCNo == 2)
			{
				return npc2Convo[Random.Range(0, npc2Convo.Length)];
			}
			if (talkingTo.myId.NPCNo == 3)
			{
				return npc3Convo[Random.Range(0, npc3Convo.Length)];
			}
			if (talkingTo.myId.NPCNo == 4)
			{
				return npc4Convo[Random.Range(0, npc4Convo.Length)];
			}
			if (talkingTo.myId.NPCNo == 5)
			{
				return npc5Convo[Random.Range(0, npc5Convo.Length)];
			}
			if (talkingTo.myId.NPCNo == 6)
			{
				return npc6Convo[Random.Range(0, npc6Convo.Length)];
			}
			if (talkingTo.myId.NPCNo == 7)
			{
				return npc7Convo[Random.Range(0, npc7Convo.Length)];
			}
			if (talkingTo.myId.NPCNo == 8)
			{
				return npc8Convo[Random.Range(0, npc8Convo.Length)];
			}
			if (talkingTo.myId.NPCNo == 9)
			{
				return npc9Convo[Random.Range(0, npc9Convo.Length)];
			}
			if (talkingTo.myId.NPCNo == 10)
			{
				return npc10Convo[Random.Range(0, npc10Convo.Length)];
			}
			if (talkingTo.myId.NPCNo == 11)
			{
				return npc11Convo[Random.Range(0, npc11Convo.Length)];
			}
			if (talkingTo.myId.NPCNo == 12)
			{
				return npc12Convo[Random.Range(0, npc12Convo.Length)];
			}
		}
		if (WeatherManager.Instance.rainMgr.IsActive)
		{
			return rainingFallbacks[Random.Range(0, rainingFallbacks.Length)];
		}
		if (RealWorldTimeLight.time.currentHour < 11)
		{
			return morningFallbacks[Random.Range(0, morningFallbacks.Length)];
		}
		if (RealWorldTimeLight.time.currentHour < RealWorldTimeLight.time.getSunSetTime())
		{
			return dayFallbacks[Random.Range(0, dayFallbacks.Length)];
		}
		return nightFallbacks[Random.Range(0, nightFallbacks.Length)];
	}

	public IEnumerator startRandomConvo(NPCAI startingConvo, NPCAI talkingTo)
	{
		NpcBackAndForth selectedConvo = getAConvoToSay(startingConvo, talkingTo);
		if (selectedConvo == null)
		{
			yield break;
		}
		bool startersTurn = true;
		for (int i = 0; i < selectedConvo.backAndForthText.Length; i++)
		{
			if (startersTurn)
			{
				string returnString = selectedConvo.backAndForthText[i];
				if (selectedConvo.LocalTags != null && i < selectedConvo.LocalTags.Length)
				{
					string nPCChattingBubbleText = ConversationGenerator.generate.GetNPCChattingBubbleText(selectedConvo.LocalTags[i]);
					if (nPCChattingBubbleText != null)
					{
						returnString = nPCChattingBubbleText;
					}
				}
				startingConvo.chatBubble.tryAndTalk(convertSpecialNames(returnString), 2.5f);
			}
			else
			{
				string returnString2 = selectedConvo.backAndForthText[i];
				if (selectedConvo.LocalTags != null && i < selectedConvo.LocalTags.Length)
				{
					string nPCChattingBubbleText2 = ConversationGenerator.generate.GetNPCChattingBubbleText(selectedConvo.LocalTags[i]);
					if (nPCChattingBubbleText2 != null)
					{
						returnString2 = nPCChattingBubbleText2;
					}
				}
				talkingTo.chatBubble.tryAndTalk(convertSpecialNames(returnString2), 2.5f);
			}
			startersTurn = !startersTurn;
			yield return new WaitForSeconds(2.7f);
		}
	}

	public string convertSpecialNames(string returnString)
	{
		if (returnString.Contains("<PlayerName>"))
		{
			returnString.Replace("<PlayerName>", UIAnimationManager.manage.GetCharacterNameTag(Inventory.Instance.playerName));
		}
		if (returnString.Contains("<IslandName>"))
		{
			returnString = returnString.Replace("<IslandName>", UIAnimationManager.manage.GetCharacterNameTag(NetworkMapSharer.Instance.islandName));
		}
		if (returnString.Contains("<SouthCity>"))
		{
			returnString = returnString.Replace("<SouthCity>", UIAnimationManager.manage.GetCharacterNameTag(ConversationManager.manage.GetLocByTag("SouthCity")));
		}
		if (returnString.Contains("<Dinks>"))
		{
			returnString = returnString.Replace("<Dinks>", UIAnimationManager.manage.MoneyAmountColorTag(ConversationManager.manage.GetLocByTag("Dinks")));
		}
		if (returnString.Contains("<Dink>"))
		{
			returnString = returnString.Replace("<Dink>", UIAnimationManager.manage.MoneyAmountColorTag(ConversationManager.manage.GetLocByTag("Dink")));
		}
		returnString = returnString.Replace("\r", "");
		return returnString;
	}

	public void AddChatBubblesToLocDoc()
	{
		SpeakersName = GetComponent<NPCDetails>().NPCName;
		AddToLocDoc(npc0Convo, "Rayne");
		AddToLocDoc(npc1Convo, "John");
		AddToLocDoc(npc2Convo, "Franklyn");
		AddToLocDoc(npc3Convo, "Melvin");
		AddToLocDoc(npc4Convo, "Clover");
		AddToLocDoc(npc5Convo, "Ned");
		AddToLocDoc(npc6Convo, "Fletch");
		AddToLocDoc(npc7Convo, "Irwin");
		AddToLocDoc(npc8Convo, "Sally");
		AddToLocDoc(npc9Convo, "Theodore");
		AddToLocDoc(npc10Convo, "Milburn");
		AddToLocDoc(npc11Convo, "Jimmy");
		AddToLocDoc(npc12Convo, "Shiela");
		AddToLocDoc(morningFallbacks, "Anyone_Morning");
		AddToLocDoc(dayFallbacks, "Anyone_DayTime");
		AddToLocDoc(nightFallbacks, "Anyone_NightTime");
		AddToLocDoc(rainingFallbacks, "Anyone_NightTime");
		LocalizationManager.Sources[0].UpdateDictionary();
	}

	public void AddToLocDoc(NpcBackAndForth[] backAndForth, string speakingTo)
	{
		if (SpeakersName == speakingTo)
		{
			return;
		}
		for (int i = 0; i < backAndForth.Length; i++)
		{
			backAndForth[i].LocalTags = new string[backAndForth[i].backAndForthText.Length];
			for (int j = 0; j < backAndForth[i].backAndForthText.Length; j++)
			{
				MonoBehaviour.print($"{conversationTarget}/{SpeakersName}_ChatBubbleTo_{speakingTo}_Convo{i}_Part{j}");
				backAndForth[i].LocalTags[j] = $"{conversationTarget}/{SpeakersName}_ChatBubbleTo_{speakingTo}_Convo{i}_Part{j}";
				if (LocalizationManager.Sources[0].ContainsTerm($"{conversationTarget}/{SpeakersName}_ChatBubbleTo_{speakingTo}_Convo{i}_Part{j}"))
				{
					LocalizationManager.Sources[0].GetTermData($"{conversationTarget}/{SpeakersName}_ChatBubbleTo_{speakingTo}_Convo{i}_Part{j}").Languages[0] = backAndForth[i].backAndForthText[j];
					continue;
				}
				LocalizationManager.Sources[0].AddTerm($"{conversationTarget}/{SpeakersName}_ChatBubbleTo_{speakingTo}_Convo{i}_Part{j}").Languages[0] = backAndForth[i].backAndForthText[j];
				backAndForth[i].LocalTags[j] = $"{conversationTarget}/{SpeakersName}_ChatBubbleTo_{speakingTo}_Convo{i}_Part{j}";
			}
		}
	}
}
