using System.Text.RegularExpressions;
using UnityEngine;

public class LocalisationMarkUp : MonoBehaviour
{
	public enum LanguageGender
	{
		masculine,
		feminine,
		neutral
	}

	public static string RunMarkupCheck(string input, InventoryItem item, int count = 1, InventoryItem rewardItem = null, int rewardCount = 0)
	{
		if (count <= 0)
		{
			count = 1;
		}
		if (input.Contains("{"))
		{
			if (item != null && input.Contains("{gender|"))
			{
				input = CheckGender(input, item.GetLanguageGender(), count > 1);
			}
			if (rewardItem != null && input.Contains("{rewardGender|"))
			{
				input = CheckGender(input.Replace("{rewardGender|", "{gender|"), rewardItem.GetLanguageGender(), rewardCount > 1);
			}
			if (input.Contains("{count|"))
			{
				input = CheckCount(input, count > 1);
			}
			if (input.Contains("{a}"))
			{
				char c = item.getInvItemName().ToUpper()[0];
				input = ((c != 'A' && c != 'E' && c != 'I' && !(c == 'O' || c == 'U')) ? input.Replace("{a}", "a") : input.Replace("{a}", "an"));
			}
		}
		return input;
	}

	public static string RunMarkupCheck(string input, int itemId, int count = 1, int rewardItemId = -1, int rewardCount = 0)
	{
		if (itemId >= 0)
		{
			if (rewardItemId >= 0)
			{
				return RunMarkupCheck(input, Inventory.Instance.allItems[itemId], count, Inventory.Instance.allItems[rewardItemId], rewardCount);
			}
			return RunMarkupCheck(input, Inventory.Instance.allItems[itemId], count);
		}
		if (rewardItemId >= 0)
		{
			return RunMarkupCheckForRewardOnly(input, Inventory.Instance.allItems[rewardItemId], rewardCount);
		}
		return input;
	}

	public static string RunMarkupCheckAnimal(string input, AnimalAI myAnimal, int count = 1)
	{
		if (input.Contains("{") && myAnimal != null && input.Contains("{animalGender|"))
		{
			input = CheckAnimalGender(input, myAnimal.GetLanguageGender(), count > 1);
		}
		if (input.Contains("{a}"))
		{
			char c = myAnimal.GetAnimalName()[0];
			input = ((c != 'A' && c != 'E' && c != 'I' && !(c == 'O' || c == 'U')) ? input.Replace("{a}", "a") : input.Replace("{a}", "an"));
		}
		return input;
	}

	public static string RunMarkupCheckAnimal(string input, int animalId, int count = 1)
	{
		if (animalId < 0 || animalId >= AnimalManager.manage.allAnimals.Length)
		{
			return input;
		}
		AnimalAI animalAI = AnimalManager.manage.allAnimals[count];
		if (input.Contains("{") && animalAI != null && input.Contains("{animalGender|"))
		{
			input = CheckAnimalGender(input, animalAI.GetLanguageGender(), count > 1);
		}
		return input;
	}

	public static string RunMarkupCheckForRewardOnly(string input, InventoryItem rewardItem, int rewardCount = 0)
	{
		if (input.Contains("{"))
		{
			if (input.Contains("{gender|"))
			{
				input = input.Replace("{gender|", "{ignore|");
			}
			if (rewardItem != null && input.Contains("{rewardGender|"))
			{
				input = CheckGender(input.Replace("{rewardGender|", "{gender|"), rewardItem.GetLanguageGender(), rewardCount > 1);
			}
			if (input.Contains("{ignore|"))
			{
				input = input.Replace("{ignore|", "{gender|");
			}
		}
		return input;
	}

	public static string CheckGender(string input, LanguageGender genderToReplace, bool isPlural)
	{
		string gender = genderToReplace.ToString().ToLower();
		string pattern = "\\{gender\\|m:([^/|}]*)/?([^|}]*)?\\|?(f:([^/|}]*)/?([^|}]*)?)?\\|?(n:([^/|}]*)/?([^|}]*)?)?\\}";
		return Regex.Replace(input, pattern, delegate(Match match)
		{
			string text = (match.Groups[1].Success ? match.Groups[1].Value : string.Empty);
			string text2 = ((match.Groups[2].Success && !string.IsNullOrEmpty(match.Groups[2].Value)) ? match.Groups[2].Value : text);
			string text3 = (match.Groups[4].Success ? match.Groups[4].Value : string.Empty);
			string text4 = ((match.Groups[5].Success && !string.IsNullOrEmpty(match.Groups[5].Value)) ? match.Groups[5].Value : text3);
			string text5 = (match.Groups[7].Success ? match.Groups[7].Value : string.Empty);
			string text6 = ((match.Groups[8].Success && !string.IsNullOrEmpty(match.Groups[8].Value)) ? match.Groups[8].Value : text5);
			if (gender != null)
			{
				switch (gender)
				{
				case "masculine":
					return isPlural ? text2 : text;
				case "feminine":
					return isPlural ? text4 : text3;
				case "neutral":
					return isPlural ? text6 : text5;
				}
			}
			return string.Empty;
		});
	}

	public static string CheckAnimalGender(string input, LanguageGender genderToReplace, bool isPlural)
	{
		string gender = genderToReplace.ToString().ToLower();
		string pattern = "\\{animalGender\\|m:([^/|}]*)/?([^|}]*)?\\|?(f:([^/|}]*)/?([^|}]*)?)?\\|?(n:([^/|}]*)/?([^|}]*)?)?\\}";
		return Regex.Replace(input, pattern, delegate(Match match)
		{
			string text = (match.Groups[1].Success ? match.Groups[1].Value : string.Empty);
			string text2 = ((match.Groups[2].Success && !string.IsNullOrEmpty(match.Groups[2].Value)) ? match.Groups[2].Value : text);
			string text3 = (match.Groups[4].Success ? match.Groups[4].Value : string.Empty);
			string text4 = ((match.Groups[5].Success && !string.IsNullOrEmpty(match.Groups[5].Value)) ? match.Groups[5].Value : text3);
			string text5 = (match.Groups[7].Success ? match.Groups[7].Value : string.Empty);
			string text6 = ((match.Groups[8].Success && !string.IsNullOrEmpty(match.Groups[8].Value)) ? match.Groups[8].Value : text5);
			if (gender != null)
			{
				switch (gender)
				{
				case "masculine":
					return isPlural ? text2 : text;
				case "feminine":
					return isPlural ? text4 : text3;
				case "neutral":
					return isPlural ? text6 : text5;
				}
			}
			return string.Empty;
		});
	}

	public static string CheckCount(string input, bool isPlural)
	{
		string pattern = "\\{isPlural\\|([^/|}]*)/([^|}]*)\\}";
		return Regex.Replace(input, pattern, delegate(Match match)
		{
			string value = match.Groups[1].Value;
			string value2 = match.Groups[2].Value;
			return (!isPlural) ? value : value2;
		});
	}

	public static string RemoveAllTags(string input)
	{
		return Regex.Replace(input, "\\{.*?\\}", "").Trim();
	}

	public static string ProcessNameTag(string input, int count)
	{
		string text = Regex.Replace(input, "\\{(M|F|N)\\}", "");
		if (count >= 1)
		{
			if (count > 1)
			{
				Match match = Regex.Match(text, "\\{pl:(.+?)\\}");
				if (match.Success)
				{
					return match.Groups[1].Value;
				}
				Match match2 = Regex.Match(text, "\\{(.+?)\\}");
				if (match2.Success)
				{
					return Regex.Replace(text, "\\{.*?\\}", "") + match2.Groups[1].Value;
				}
				return Regex.Replace(text, "\\{.*?\\}", "").Trim();
			}
			return Regex.Replace(text, "\\{.*?\\}", "").Trim();
		}
		return text.Trim();
	}

	public static string ScrubGenderPlaceholders(string input)
	{
		string pattern = "\\{gender\\|m:[^/|}]*\\/?[^|}]*\\|?(f:[^/|}]*\\/?[^|}]*)?\\|?(n:[^/|}]*\\/?[^|}]*)?\\}";
		return Regex.Replace(input, pattern, "");
	}
}
