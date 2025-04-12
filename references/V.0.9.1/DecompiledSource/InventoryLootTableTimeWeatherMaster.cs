using System.Collections.Generic;
using UnityEngine;

public class InventoryLootTableTimeWeatherMaster : MonoBehaviour
{
	[Header("Location Name --------------")]
	public string locationName = "river";

	[Header("Sumer --------------")]
	public InventoryItemLootTable summerMorning;

	public InventoryItemLootTable summerDay;

	public InventoryItemLootTable summerNight;

	[Header("Winter --------------")]
	public InventoryItemLootTable winterMorning;

	public InventoryItemLootTable winterDay;

	public InventoryItemLootTable winterNight;

	[Header("Autum Days --------------")]
	public InventoryItemLootTable autumMorning;

	public InventoryItemLootTable autumDay;

	public InventoryItemLootTable autumNight;

	[Header("Spring Days --------------")]
	public InventoryItemLootTable springMorning;

	public InventoryItemLootTable springDay;

	public InventoryItemLootTable springNight;

	public int getSomethingUndiscovered()
	{
		int num = -1;
		if (WorldManager.Instance.month == 1)
		{
			num = summerMorning.getSomethingUnDiscovered();
			if (num != -1)
			{
				return num;
			}
			num = summerDay.getSomethingUnDiscovered();
			if (num != -1)
			{
				return num;
			}
			num = summerNight.getSomethingUnDiscovered();
			if (num != -1)
			{
				return num;
			}
		}
		else if (WorldManager.Instance.month == 2)
		{
			num = autumMorning.getSomethingUnDiscovered();
			if (num != -1)
			{
				return num;
			}
			num = autumDay.getSomethingUnDiscovered();
			if (num != -1)
			{
				return num;
			}
			num = autumNight.getSomethingUnDiscovered();
			if (num != -1)
			{
				return num;
			}
		}
		else if (WorldManager.Instance.month == 3)
		{
			num = winterMorning.getSomethingUnDiscovered();
			if (num != -1)
			{
				return num;
			}
			num = winterDay.getSomethingUnDiscovered();
			if (num != -1)
			{
				return num;
			}
			num = winterNight.getSomethingUnDiscovered();
			if (num != -1)
			{
				return num;
			}
		}
		else if (WorldManager.Instance.month == 4)
		{
			num = springMorning.getSomethingUnDiscovered();
			if (num != -1)
			{
				return num;
			}
			num = springDay.getSomethingUnDiscovered();
			if (num != -1)
			{
				return num;
			}
			num = springNight.getSomethingUnDiscovered();
			if (num != -1)
			{
				return num;
			}
		}
		_ = -1;
		return num;
	}

	public InventoryItem getInventoryItem()
	{
		int num = 0;
		if (WeatherManager.Instance.rainMgr.IsActive)
		{
			num = 4;
		}
		if (WeatherManager.Instance.stormMgr.IsActive)
		{
			num = 6;
		}
		if (WeatherManager.Instance.fogMgr.IsActive)
		{
			num = 1;
		}
		if (WorldManager.Instance.day + (WorldManager.Instance.week - 1) * 7 == 13)
		{
			num += -10;
		}
		if (WorldManager.Instance.month == 1)
		{
			if ((RealWorldTimeLight.time.currentHour >= 5 && RealWorldTimeLight.time.currentHour < 11) || (RealWorldTimeLight.time.currentHour == 11 && Random.Range(0, 6) == 1))
			{
				return summerMorning.getRandomDropWithAddedLuck(num);
			}
			if (RealWorldTimeLight.time.currentHour >= 5 && RealWorldTimeLight.time.currentHour < RealWorldTimeLight.time.getSunSetTime() + 1)
			{
				return summerDay.getRandomDropWithAddedLuck(num);
			}
			return summerNight.getRandomDropWithAddedLuck(num);
		}
		if (WorldManager.Instance.month == 2)
		{
			if ((RealWorldTimeLight.time.currentHour >= 5 && RealWorldTimeLight.time.currentHour < 11) || (RealWorldTimeLight.time.currentHour == 11 && Random.Range(0, 6) == 1))
			{
				return autumMorning.getRandomDropWithAddedLuck(num);
			}
			if (RealWorldTimeLight.time.currentHour >= 5 && RealWorldTimeLight.time.currentHour < RealWorldTimeLight.time.getSunSetTime() + 1)
			{
				return autumDay.getRandomDropWithAddedLuck(num);
			}
			return autumNight.getRandomDropWithAddedLuck(num);
		}
		if (WorldManager.Instance.month == 3)
		{
			if ((RealWorldTimeLight.time.currentHour >= 5 && RealWorldTimeLight.time.currentHour < 11) || (RealWorldTimeLight.time.currentHour == 11 && Random.Range(0, 6) == 1))
			{
				return winterMorning.getRandomDropWithAddedLuck(num);
			}
			if (RealWorldTimeLight.time.currentHour >= 5 && RealWorldTimeLight.time.currentHour < RealWorldTimeLight.time.getSunSetTime() + 1)
			{
				return winterDay.getRandomDropWithAddedLuck(num);
			}
			return winterNight.getRandomDropWithAddedLuck(num);
		}
		if ((RealWorldTimeLight.time.currentHour >= 5 && RealWorldTimeLight.time.currentHour < 11) || (RealWorldTimeLight.time.currentHour == 11 && Random.Range(0, 6) == 1))
		{
			return springMorning.getRandomDropWithAddedLuck(num);
		}
		if (RealWorldTimeLight.time.currentHour >= 5 && RealWorldTimeLight.time.currentHour < RealWorldTimeLight.time.getSunSetTime() + 1)
		{
			return springDay.getRandomDropWithAddedLuck(num);
		}
		return springNight.getRandomDropWithAddedLuck(num);
	}

	public bool isBugOrFishInTable(InventoryItem checkIfIsInTable)
	{
		if (summerMorning.isInTable(checkIfIsInTable))
		{
			return true;
		}
		if (summerDay.isInTable(checkIfIsInTable))
		{
			return true;
		}
		if (summerNight.isInTable(checkIfIsInTable))
		{
			return true;
		}
		if (winterMorning.isInTable(checkIfIsInTable))
		{
			return true;
		}
		if (winterDay.isInTable(checkIfIsInTable))
		{
			return true;
		}
		if (winterNight.isInTable(checkIfIsInTable))
		{
			return true;
		}
		if (autumMorning.isInTable(checkIfIsInTable))
		{
			return true;
		}
		if (autumDay.isInTable(checkIfIsInTable))
		{
			return true;
		}
		if (autumNight.isInTable(checkIfIsInTable))
		{
			return true;
		}
		if (springMorning.isInTable(checkIfIsInTable))
		{
			return true;
		}
		if (springDay.isInTable(checkIfIsInTable))
		{
			return true;
		}
		if (springNight.isInTable(checkIfIsInTable))
		{
			return true;
		}
		return false;
	}

	public string getTimeOfDayFound(InventoryItem checkIfIsInTable)
	{
		if (summerMorning.isInTable(checkIfIsInTable) && summerDay.isInTable(checkIfIsInTable) && summerNight.isInTable(checkIfIsInTable))
		{
			return ConversationGenerator.generate.GetTimeNameByTag("all day");
		}
		if (summerMorning.isInTable(checkIfIsInTable) && summerDay.isInTable(checkIfIsInTable))
		{
			return ConversationGenerator.generate.GetTimeNameByTag("during the day");
		}
		if (summerMorning.isInTable(checkIfIsInTable))
		{
			return ConversationGenerator.generate.GetTimeNameByTag("early mornings");
		}
		if (summerDay.isInTable(checkIfIsInTable))
		{
			return ConversationGenerator.generate.GetTimeNameByTag("around noon");
		}
		if (summerNight.isInTable(checkIfIsInTable))
		{
			return ConversationGenerator.generate.GetTimeNameByTag("after dark");
		}
		if (winterMorning.isInTable(checkIfIsInTable) && winterDay.isInTable(checkIfIsInTable) && winterNight.isInTable(checkIfIsInTable))
		{
			return ConversationGenerator.generate.GetTimeNameByTag("all day");
		}
		if (winterMorning.isInTable(checkIfIsInTable) && winterDay.isInTable(checkIfIsInTable))
		{
			return ConversationGenerator.generate.GetTimeNameByTag("during the day");
		}
		if (winterMorning.isInTable(checkIfIsInTable))
		{
			return ConversationGenerator.generate.GetTimeNameByTag("early mornings");
		}
		if (winterDay.isInTable(checkIfIsInTable))
		{
			return ConversationGenerator.generate.GetTimeNameByTag("around noon");
		}
		if (winterNight.isInTable(checkIfIsInTable))
		{
			return ConversationGenerator.generate.GetTimeNameByTag("after dark");
		}
		if (autumMorning.isInTable(checkIfIsInTable) && autumDay.isInTable(checkIfIsInTable) && autumNight.isInTable(checkIfIsInTable))
		{
			return ConversationGenerator.generate.GetTimeNameByTag("all day");
		}
		if (autumMorning.isInTable(checkIfIsInTable) && autumDay.isInTable(checkIfIsInTable))
		{
			return ConversationGenerator.generate.GetTimeNameByTag("during the day");
		}
		if (autumMorning.isInTable(checkIfIsInTable))
		{
			return ConversationGenerator.generate.GetTimeNameByTag("early mornings");
		}
		if (autumDay.isInTable(checkIfIsInTable))
		{
			return ConversationGenerator.generate.GetTimeNameByTag("around noon");
		}
		if (autumNight.isInTable(checkIfIsInTable))
		{
			return ConversationGenerator.generate.GetTimeNameByTag("after dark");
		}
		if (springMorning.isInTable(checkIfIsInTable) && springDay.isInTable(checkIfIsInTable) && springNight.isInTable(checkIfIsInTable))
		{
			return ConversationGenerator.generate.GetTimeNameByTag("all day");
		}
		if (springMorning.isInTable(checkIfIsInTable) && springDay.isInTable(checkIfIsInTable))
		{
			return ConversationGenerator.generate.GetTimeNameByTag("during the day");
		}
		if (springMorning.isInTable(checkIfIsInTable))
		{
			return ConversationGenerator.generate.GetTimeNameByTag("early mornings");
		}
		if (springDay.isInTable(checkIfIsInTable))
		{
			return ConversationGenerator.generate.GetTimeNameByTag("around noon");
		}
		if (springNight.isInTable(checkIfIsInTable))
		{
			return ConversationGenerator.generate.GetTimeNameByTag("after dark");
		}
		return ConversationGenerator.generate.GetTimeNameByTag("all day");
	}

	public string getSeason(InventoryItem checkIfIsInTable)
	{
		string text = "";
		if (summerMorning.isInTable(checkIfIsInTable) || summerDay.isInTable(checkIfIsInTable) || (summerNight.isInTable(checkIfIsInTable) && winterMorning.isInTable(checkIfIsInTable)) || winterDay.isInTable(checkIfIsInTable) || (winterNight.isInTable(checkIfIsInTable) && autumMorning.isInTable(checkIfIsInTable)) || autumDay.isInTable(checkIfIsInTable) || (autumNight.isInTable(checkIfIsInTable) && springMorning.isInTable(checkIfIsInTable)) || springDay.isInTable(checkIfIsInTable) || springNight.isInTable(checkIfIsInTable))
		{
			return "All";
		}
		if (summerMorning.isInTable(checkIfIsInTable) || summerDay.isInTable(checkIfIsInTable) || summerNight.isInTable(checkIfIsInTable))
		{
			text += RealWorldTimeLight.time.getSeasonName(0);
		}
		if (winterMorning.isInTable(checkIfIsInTable) || winterDay.isInTable(checkIfIsInTable) || winterNight.isInTable(checkIfIsInTable))
		{
			if (text != "")
			{
				text += " & ";
			}
			text += RealWorldTimeLight.time.getSeasonName(2);
		}
		if (autumMorning.isInTable(checkIfIsInTable) || autumDay.isInTable(checkIfIsInTable) || autumNight.isInTable(checkIfIsInTable))
		{
			if (text != "")
			{
				text += " & ";
			}
			text += RealWorldTimeLight.time.getSeasonName(1);
		}
		if (springMorning.isInTable(checkIfIsInTable) || springDay.isInTable(checkIfIsInTable) || springNight.isInTable(checkIfIsInTable))
		{
			if (text != "")
			{
				text += " & ";
			}
			text += RealWorldTimeLight.time.getSeasonName(3);
		}
		return text;
	}

	public void populateTable(List<InventoryItem> allItems)
	{
		SeasonTable seasonTable = new SeasonTable();
		SeasonTable seasonTable2 = new SeasonTable();
		SeasonTable seasonTable3 = new SeasonTable();
		SeasonTable seasonTable4 = new SeasonTable();
		for (int i = 0; i < allItems.Count; i++)
		{
			SeasonAndTime seasonAndTime = (allItems[i].fish ? allItems[i].fish.mySeason : ((!allItems[i].bug) ? allItems[i].underwaterCreature.mySeason : allItems[i].bug.mySeason));
			SeasonAndTime.seasonFound[] mySeasons = seasonAndTime.mySeasons;
			for (int j = 0; j < mySeasons.Length; j++)
			{
				switch (mySeasons[j])
				{
				case SeasonAndTime.seasonFound.All:
					seasonTable.addToTable(allItems[i], seasonAndTime.myTimeOfDay);
					seasonTable2.addToTable(allItems[i], seasonAndTime.myTimeOfDay);
					seasonTable3.addToTable(allItems[i], seasonAndTime.myTimeOfDay);
					seasonTable4.addToTable(allItems[i], seasonAndTime.myTimeOfDay);
					break;
				case SeasonAndTime.seasonFound.Summer:
					seasonTable.addToTable(allItems[i], seasonAndTime.myTimeOfDay);
					break;
				case SeasonAndTime.seasonFound.Winter:
					seasonTable2.addToTable(allItems[i], seasonAndTime.myTimeOfDay);
					break;
				case SeasonAndTime.seasonFound.Autum:
					seasonTable3.addToTable(allItems[i], seasonAndTime.myTimeOfDay);
					break;
				case SeasonAndTime.seasonFound.Spring:
					seasonTable4.addToTable(allItems[i], seasonAndTime.myTimeOfDay);
					break;
				}
			}
		}
		summerMorning.autoFillFromArray(seasonTable.morning.ToArray());
		summerDay.autoFillFromArray(seasonTable.day.ToArray());
		summerNight.autoFillFromArray(seasonTable.night.ToArray());
		winterMorning.autoFillFromArray(seasonTable2.morning.ToArray());
		winterDay.autoFillFromArray(seasonTable2.day.ToArray());
		winterNight.autoFillFromArray(seasonTable2.night.ToArray());
		autumMorning.autoFillFromArray(seasonTable3.morning.ToArray());
		autumDay.autoFillFromArray(seasonTable3.day.ToArray());
		autumNight.autoFillFromArray(seasonTable3.night.ToArray());
		springMorning.autoFillFromArray(seasonTable4.morning.ToArray());
		springDay.autoFillFromArray(seasonTable4.day.ToArray());
		springNight.autoFillFromArray(seasonTable4.night.ToArray());
	}

	public string GetLocationName()
	{
		return ConversationGenerator.generate.GetBiomeNameText(locationName);
	}
}
