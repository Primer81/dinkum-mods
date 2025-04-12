using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class Conversation : MonoBehaviour
{
	public CONVERSATION_TARGET saidBy;

	[Header("Opening Sentences --------------------------------")]
	public ConversationSequence startLineAlt;

	[Header("Response options ---------------------------------")]
	public string[] optionNames;

	[Header("Response Sentences -------------------------------")]
	public ConversationSequence[] responesAlt;

	private static string[] fullstop = new string[1] { ". " };

	private static string[] questionmark = new string[1] { "? " };

	private static string[] exclaim = new string[1] { "! " };

	private static string[] elipse = new string[1] { "... " };

	public bool HasOptionsSequence
	{
		get
		{
			if (optionNames != null)
			{
				return optionNames.Length != 0;
			}
			return false;
		}
	}

	public string[] GetTargetOpeningSequences()
	{
		if ((bool)GetComponent<ConverstationSequence>())
		{
			Debug.LogWarning(base.name + "has an old conversation componenet");
		}
		if ((string)(LocalizedString)getIntroName(0) != null)
		{
			return getTranslatedIntro();
		}
		if (Application.isEditor && saidBy != 0)
		{
			fillConvoTranslations();
		}
		return startLineAlt.sequence;
	}

	public string[] GetTargetResponseSequences(int responseNo)
	{
		if ((bool)GetComponent<ConverstationSequence>())
		{
			Debug.LogWarning(base.name + "has an old conversation componenet");
		}
		if ((string)(LocalizedString)getResponseName(responseNo, 0) != null)
		{
			return getTranslatedResponse(responseNo);
		}
		return responesAlt[responseNo].sequence;
	}

	public string getOption(int optionNo)
	{
		if ((string)(LocalizedString)getOptionName(optionNo) != null)
		{
			return (LocalizedString)getOptionName(optionNo);
		}
		return optionNames[optionNo];
	}

	public string[] getTranslatedIntro()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < startLineAlt.sequence.Length; i++)
		{
			list.Add((LocalizedString)getIntroName(i));
			if (list[i] == null)
			{
				Debug.LogWarning("Got a line from untranslated text box");
				list[i] = startLineAlt.sequence[i];
			}
		}
		return list.ToArray();
	}

	public string[] getTranslatedResponse(int responseNo)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < responesAlt[responseNo].sequence.Length; i++)
		{
			list.Add((LocalizedString)getResponseName(responseNo, i));
			if (list[i] == null)
			{
				Debug.LogWarning("Got a line from untranslated text box");
				list[i] = responesAlt[responseNo].sequence[i];
			}
		}
		return list.ToArray();
	}

	public CONVERSATION_SPECIAL_ACTION checkSpecialAction(int optionNo)
	{
		if (optionNo == -1)
		{
			return startLineAlt.specialAction;
		}
		return responesAlt[optionNo].specialAction;
	}

	public bool HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION action, int optionIndex)
	{
		if (startLineAlt != null && startLineAlt.specialAction == action)
		{
			return true;
		}
		if (responesAlt != null && responesAlt.Length >= optionIndex - 1 && responesAlt[optionIndex].specialAction == action)
		{
			return true;
		}
		return false;
	}

	private string[] splitUpIntoSentences(ConversationSequence splitMeUp)
	{
		string[] toBeSplitUp = returnSplitConvo(splitMeUp.sequence, fullstop);
		toBeSplitUp = returnSplitConvo(toBeSplitUp, questionmark, "?");
		toBeSplitUp = returnSplitConvo(toBeSplitUp, exclaim, "!");
		return returnSplitConvo(toBeSplitUp, elipse, "...");
	}

	private string[] returnSplitConvo(string[] toBeSplitUp, string[] splitAt, string endOfSentenceCharacter = ".")
	{
		List<string> list = new List<string>();
		for (int i = 0; i < toBeSplitUp.Length; i++)
		{
			string[] array = toBeSplitUp[i].Split(splitAt, StringSplitOptions.RemoveEmptyEntries);
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].LastIndexOf('.') != array[j].Length - 1 && array[j].LastIndexOf('?') != array[j].Length - 1 && array[j].LastIndexOf('!') != array[j].Length - 1)
				{
					array[j] += endOfSentenceCharacter;
				}
				list.Add(array[j]);
			}
		}
		return list.ToArray();
	}

	public void fillConvoTranslations()
	{
		_ = saidBy.ToString() + "/";
		for (int i = 0; i < startLineAlt.sequence.Length; i++)
		{
			LocalizationManager.Sources[0].AddTerm(getIntroName(i)).Languages[0] = startLineAlt.sequence[i];
		}
		for (int j = 0; j < optionNames.Length; j++)
		{
			if (!optionNames[j].Contains("<"))
			{
				LocalizationManager.Sources[0].AddTerm(getOptionName(j)).Languages[0] = optionNames[j];
			}
		}
		for (int k = 0; k < responesAlt.Length; k++)
		{
			for (int l = 0; l < responesAlt[k].sequence.Length; l++)
			{
				LocalizationManager.Sources[0].AddTerm(getResponseName(k, l)).Languages[0] = responesAlt[k].sequence[l];
			}
		}
		LocalizationManager.Sources[0].UpdateDictionary();
	}

	public void forceNewFillConvoTranslations()
	{
		_ = saidBy.ToString() + "/";
		for (int i = 0; i < startLineAlt.sequence.Length; i++)
		{
			LocalizationManager.Sources[0].RemoveTerm(getIntroName(i));
			LocalizationManager.Sources[0].AddTerm(getIntroName(i)).Languages[0] = startLineAlt.sequence[i];
		}
		for (int j = 0; j < optionNames.Length; j++)
		{
			if (!optionNames[j].Contains("<"))
			{
				LocalizationManager.Sources[0].RemoveTerm(getOptionName(j));
				LocalizationManager.Sources[0].AddTerm(getOptionName(j)).Languages[0] = optionNames[j];
			}
		}
		for (int k = 0; k < responesAlt.Length; k++)
		{
			for (int l = 0; l < responesAlt[k].sequence.Length; l++)
			{
				LocalizationManager.Sources[0].RemoveTerm(getResponseName(k, l));
				LocalizationManager.Sources[0].AddTerm(getResponseName(k, l)).Languages[0] = responesAlt[k].sequence[l];
			}
		}
		LocalizationManager.Sources[0].UpdateDictionary();
	}

	private void Translate(TermData termData, string text)
	{
		for (int i = 1; i < LocalizationManager.Sources[0].GetLanguagesCode().Count; i++)
		{
			termData.Languages[i] = GoogleTranslation.ForceTranslate(text, "en", LocalizationManager.Sources[0].GetLanguagesCode()[i]);
		}
	}

	public string getIntroName(int i)
	{
		return saidBy.ToString() + "/" + base.name + "_Intro_" + i.ToString("D3");
	}

	public string getOptionName(int i)
	{
		return saidBy.ToString() + "/" + base.name + "_Option_" + i.ToString("D3");
	}

	public string getResponseName(int i, int r)
	{
		return saidBy.ToString() + "/" + base.name + "_Response_" + i.ToString("D3") + "_" + r.ToString("D3");
	}

	public string GetUntilOrEmpty(string text, char stopAt = '_')
	{
		string text2 = text;
		if (!string.IsNullOrWhiteSpace(text))
		{
			text2 = text2.Split(' ')[0];
			text2 = text2.Split('_')[0];
		}
		return text2;
	}
}
