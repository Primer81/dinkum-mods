using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Conversation", menuName = "Conversation", order = 1)]
public class ConversationObject : ScriptableObject
{
	public CONVERSATION_TARGET conversationTarget;

	public ConversationSequence targetOpenings;

	public string[] playerOptions;

	public List<ConversationSequence> targetResponses = new List<ConversationSequence>();

	[Multiline(4)]
	public string newConversationSequence;

	public bool HasOptionsSequence
	{
		get
		{
			if (playerOptions != null)
			{
				return playerOptions.Length != 0;
			}
			return false;
		}
	}

	private void InsertStandardGreetingResponses()
	{
		playerOptions = new string[4] { "<chat>", "<jobCondition>", "<hangOut>", "<cancel>" };
	}

	private void OverrideTargetOpenings()
	{
		LocalizationManager.InitializeIfNeeded();
		string[] sequence = newConversationSequence.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		if (targetOpenings != null)
		{
			targetOpenings.sequence = sequence;
		}
		else
		{
			targetOpenings = new ConversationSequence
			{
				sequence = sequence
			};
		}
		newConversationSequence = string.Empty;
	}

	private void OverridePlayerOptions()
	{
		LocalizationManager.InitializeIfNeeded();
		string[] array = newConversationSequence.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		playerOptions = array;
		newConversationSequence = string.Empty;
	}

	private void AddNewTargetResponse()
	{
		LocalizationManager.InitializeIfNeeded();
		string[] sequence = newConversationSequence.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		targetResponses.Add(new ConversationSequence
		{
			sequence = sequence
		});
		newConversationSequence = string.Empty;
	}

	private void AddToLocDoc()
	{
		LocalizationManager.InitializeIfNeeded();
		SaveTargetOpenings();
		SavePlayerOptions();
		SaveTargetResponses();
		LocalizationManager.Sources[0].UpdateDictionary();
	}

	private void UpdateInLocDoc()
	{
		LocalizationManager.InitializeIfNeeded();
		SaveTargetOpenings(update: true);
		SavePlayerOptions(update: true);
		SaveTargetResponses(update: true);
		LocalizationManager.Sources[0].UpdateDictionary();
	}

	private void AddNotesToLocDoc()
	{
		LocalizationManager.InitializeIfNeeded();
		AddNotesForOpenings();
		AddNotesForResponses();
		LocalizationManager.Sources[0].UpdateDictionary();
	}

	private void SearchForEmptyTags()
	{
		LocalizationManager.InitializeIfNeeded();
		LookForEmptyTags();
	}

	public string GetPlayerOptionTranslated(int index)
	{
		if ((string)(LocalizedString)GetPlayerOptionLocalisationName(index) != null)
		{
			return (LocalizedString)GetPlayerOptionLocalisationName(index);
		}
		return playerOptions[index];
	}

	public string[] GetTargetResponseSequences(int index)
	{
		if ((string)(LocalizedString)GetTargetResponseLocalisationName(index, 0) != null)
		{
			return GetTargetResponsesTranslated(index);
		}
		return targetResponses[index].sequence;
	}

	public CONVERSATION_SPECIAL_ACTION GetSpecialAction(int index)
	{
		if (index == -1)
		{
			return targetOpenings.specialAction;
		}
		return targetResponses[index].specialAction;
	}

	public string[] GetTargetOpeningSequences()
	{
		if ((string)(LocalizedString)GetTargetOpeningLocalisationName(0) != null)
		{
			return GetTargetOpeningsTranslated();
		}
		if (Application.isEditor)
		{
			_ = conversationTarget;
		}
		return targetOpenings.sequence;
	}

	public bool HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION action, int optionIndex)
	{
		if (targetOpenings != null && targetOpenings.specialAction == action)
		{
			return true;
		}
		if (targetResponses.Count == 0)
		{
			return false;
		}
		if (targetResponses != null && targetResponses.Count - 1 >= optionIndex && targetResponses[optionIndex].specialAction == action)
		{
			return true;
		}
		return false;
	}

	public bool IsConversationGoingToBranchOut(int optionIndex)
	{
		if (!HasOptionsSequence)
		{
			return false;
		}
		if (targetResponses[optionIndex].branchToConversation != null)
		{
			return true;
		}
		return false;
	}

	private string GetTargetOpeningLocalisationName(int i)
	{
		return conversationTarget.ToString() + "/" + base.name + "_Intro_" + i.ToString("D3");
	}

	private string GetPlayerOptionLocalisationName(int i)
	{
		return conversationTarget.ToString() + "/" + base.name + "_Option_" + i.ToString("D3");
	}

	private string GetTargetResponseLocalisationName(int i, int r)
	{
		return conversationTarget.ToString() + "/" + base.name + "_Response_" + i.ToString("D3") + "_" + r.ToString("D3");
	}

	private void SaveTargetOpenings(bool update = false)
	{
		string categoryName = GetCategoryName();
		for (int i = 0; i < targetOpenings.sequence.Length; i++)
		{
			if (targetOpenings.sequence[i] == null)
			{
				continue;
			}
			if (!update && !LocalizationManager.Sources[0].ContainsTerm($"{conversationTarget}/{categoryName}_Intro_{i:D3}"))
			{
				LocalizationManager.Sources[0].AddTerm($"{conversationTarget}/{categoryName}_Intro_{i:D3}").Languages[0] = targetOpenings.sequence[i];
			}
			else if (update && LocalizationManager.Sources[0].ContainsTerm($"{conversationTarget}/{categoryName}_Intro_{i:D3}"))
			{
				TermData termData = LocalizationManager.Sources[0].GetTermData($"{conversationTarget}/{categoryName}_Intro_{i:D3}");
				if (termData != null)
				{
					termData.Languages[0] = targetOpenings.sequence[i];
				}
			}
		}
	}

	private void AddNotesForOpenings()
	{
		string categoryName = GetCategoryName();
		for (int i = 0; i < targetOpenings.sequence.Length; i++)
		{
			if (targetOpenings.sequence[i] == null || !LocalizationManager.Sources[0].ContainsTerm($"{conversationTarget}/{categoryName}_Intro_{i:D3}") || (!targetOpenings.talkingAboutAnimal && !targetOpenings.talkingAboutItem && !targetOpenings.talkingAboutNPC))
			{
				continue;
			}
			TermData termData = LocalizationManager.Sources[0].GetTermData($"{conversationTarget}/{categoryName}_Intro_{i:D3}");
			string text = "";
			if ((bool)targetOpenings.talkingAboutNPC && targetOpenings.sequence[i].Contains("<NPCName>"))
			{
				text = text + "<NPCName> = " + targetOpenings.talkingAboutNPC.NPCName;
			}
			if ((bool)targetOpenings.talkingAboutItem && targetOpenings.sequence[i].Contains("<itemName>"))
			{
				if (text != "")
				{
					text += "\n";
				}
				text = text + "<itemName> = " + targetOpenings.talkingAboutItem.itemName;
			}
			if ((bool)targetOpenings.talkingAboutAnimal && targetOpenings.sequence[i].Contains("<animalName>"))
			{
				if (text != "")
				{
					text += "\n";
				}
				text = text + "<animalName> = " + targetOpenings.talkingAboutAnimal.animalName;
			}
			termData.Languages[14] = text;
		}
	}

	private void LookForEmptyTags()
	{
		string categoryName = GetCategoryName();
		for (int i = 0; i < targetOpenings.sequence.Length; i++)
		{
			if (targetOpenings.sequence[i] != null && LocalizationManager.Sources[0].ContainsTerm($"{conversationTarget}/{categoryName}_Intro_{i:D3}"))
			{
				string text = "";
				if (!targetOpenings.talkingAboutNPC && targetOpenings.sequence[i].Contains("<NPCName>"))
				{
					text += "<NPCName> No Object";
				}
				if (!targetOpenings.talkingAboutItem && targetOpenings.sequence[i].Contains("<itemName>"))
				{
					text += "<NPCName> No Object";
				}
				if (!targetOpenings.talkingAboutAnimal && targetOpenings.sequence[i].Contains("<animalName>"))
				{
					text += "<animalName> No Object";
				}
				if (text != "")
				{
					Debug.Log("Without Tag: " + base.name + " " + text);
				}
			}
		}
	}

	private void SavePlayerOptions(bool update = false)
	{
		string categoryName = GetCategoryName();
		for (int i = 0; i < playerOptions.Length; i++)
		{
			if (playerOptions[i] == null || playerOptions[i].Contains("<"))
			{
				continue;
			}
			if (!update && !LocalizationManager.Sources[0].ContainsTerm($"{conversationTarget}/{categoryName}_Option_{i:D3}"))
			{
				LocalizationManager.Sources[0].AddTerm($"{conversationTarget}/{categoryName}_Option_{i:D3}").Languages[0] = playerOptions[i];
			}
			else if (update && LocalizationManager.Sources[0].ContainsTerm($"{conversationTarget}/{categoryName}_Option_{i:D3}"))
			{
				TermData termData = LocalizationManager.Sources[0].GetTermData($"{conversationTarget}/{categoryName}_Option_{i:D3}");
				if (termData != null)
				{
					termData.Languages[0] = playerOptions[i];
				}
			}
		}
	}

	private void SaveTargetResponses(bool update = false)
	{
		string categoryName = GetCategoryName();
		for (int i = 0; i < targetResponses.Count; i++)
		{
			if (targetResponses[i] == null || targetResponses[i].sequence == null)
			{
				continue;
			}
			for (int j = 0; j < targetResponses[i].sequence.Length; j++)
			{
				if (targetResponses[i].sequence[j] == null)
				{
					continue;
				}
				if (!update && !LocalizationManager.Sources[0].ContainsTerm($"{conversationTarget}/{categoryName}_Response_{i:D3}_{j:D3}"))
				{
					LocalizationManager.Sources[0].AddTerm($"{conversationTarget}/{categoryName}_Response_{i:D3}_{j:D3}").Languages[0] = targetResponses[i].sequence[j];
				}
				else if (update && LocalizationManager.Sources[0].ContainsTerm($"{conversationTarget}/{categoryName}_Response_{i:D3}_{j:D3}"))
				{
					TermData termData = LocalizationManager.Sources[0].GetTermData($"{conversationTarget}/{categoryName}_Response_{i:D3}_{j:D3}");
					if (termData != null)
					{
						termData.Languages[0] = targetResponses[i].sequence[j];
					}
				}
			}
		}
	}

	private void AddNotesForResponses()
	{
		string categoryName = GetCategoryName();
		for (int i = 0; i < targetResponses.Count; i++)
		{
			if (targetResponses[i] == null || targetResponses[i].sequence == null)
			{
				continue;
			}
			for (int j = 0; j < targetResponses[i].sequence.Length; j++)
			{
				if (targetResponses[i].sequence[j] == null || !LocalizationManager.Sources[0].ContainsTerm($"{conversationTarget}/{categoryName}_Response_{i:D3}_{j:D3}"))
				{
					continue;
				}
				TermData termData = LocalizationManager.Sources[0].GetTermData($"{conversationTarget}/{categoryName}_Response_{i:D3}_{j:D3}");
				if (termData == null)
				{
					continue;
				}
				string text = "";
				if ((bool)targetResponses[i].talkingAboutNPC && targetResponses[i].sequence[j].Contains("<NPCName>"))
				{
					text = text + "<NPCName> = " + targetResponses[i].talkingAboutNPC.NPCName;
				}
				if ((bool)targetResponses[i].talkingAboutItem && targetResponses[i].sequence[j].Contains("<itemName>"))
				{
					if (text != "")
					{
						text += "\n";
					}
					text = text + "<itemName> = " + targetResponses[i].talkingAboutItem.itemName;
				}
				if ((bool)targetResponses[i].talkingAboutAnimal && targetResponses[i].sequence[j].Contains("<animalName>"))
				{
					if (text != "")
					{
						text += "\n";
					}
					text = text + "<animalName> = " + targetResponses[i].talkingAboutAnimal.animalName;
				}
				termData.Languages[14] = text;
			}
		}
	}

	private void Translate(TermData termData, string text)
	{
		for (int i = 1; i < LocalizationManager.Sources[0].GetLanguagesCode().Count; i++)
		{
			termData.Languages[i] = GoogleTranslation.ForceTranslate(text, "en", LocalizationManager.Sources[0].GetLanguagesCode()[i]);
		}
	}

	private string[] GetTargetResponsesTranslated(int index)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < targetResponses[index].sequence.Length; i++)
		{
			list.Add((LocalizedString)GetTargetResponseLocalisationName(index, i));
			if (list[i] == null)
			{
				Debug.LogWarning("Got a line from untranslated text box");
				list[i] = targetResponses[index].sequence[i];
			}
		}
		return list.ToArray();
	}

	private string GetCategoryName()
	{
		return base.name;
	}

	private string[] GetTargetOpeningsTranslated()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < targetOpenings.sequence.Length; i++)
		{
			list.Add((LocalizedString)GetTargetOpeningLocalisationName(i));
			if (list[i] == null)
			{
				Debug.LogWarning("Got a line from untranslated text box");
				list[i] = targetOpenings.sequence[i];
			}
		}
		return list.ToArray();
	}
}
