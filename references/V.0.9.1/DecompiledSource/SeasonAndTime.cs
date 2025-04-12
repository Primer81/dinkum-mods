using UnityEngine;

public class SeasonAndTime : MonoBehaviour
{
	public enum landLocation
	{
		All,
		Bushland,
		Tropics,
		Pines,
		Plains,
		Desert,
		Underground,
		ReefIsland
	}

	public enum waterLocation
	{
		NorthOcean,
		SouthOcean,
		Rivers,
		Mangroves,
		Billabongs,
		UndergroundLake,
		ReefIsland
	}

	public enum seasonFound
	{
		All,
		Summer,
		Autum,
		Winter,
		Spring
	}

	public enum timeOfDay
	{
		All,
		Morning,
		Day,
		Night
	}

	public enum rarity
	{
		Common,
		Uncommon,
		Rare,
		VeryRare,
		SuperRare
	}

	public landLocation[] myLandLocation;

	public waterLocation[] myWaterLocation;

	public seasonFound[] mySeasons;

	public timeOfDay[] myTimeOfDay;

	public rarity myRarity;

	public string getLocationName()
	{
		string text = "";
		if (myWaterLocation.Length != 0)
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			waterLocation[] array = myWaterLocation;
			foreach (waterLocation num in array)
			{
				if (num == waterLocation.NorthOcean)
				{
					flag2 = true;
				}
				if (num == waterLocation.SouthOcean)
				{
					flag3 = true;
				}
			}
			if (flag2 && flag3)
			{
				flag = true;
				text += ConversationGenerator.generate.GetBiomeNameText("Ocean");
			}
			array = myWaterLocation;
			foreach (waterLocation waterLocation in array)
			{
				switch (waterLocation)
				{
				case waterLocation.Billabongs:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.billabongFish.GetLocationName());
					continue;
				case waterLocation.NorthOcean:
					if (!flag)
					{
						if (text != "")
						{
							text += " & ";
						}
						text += capitaliseFirstLetter(AnimalManager.manage.northernOceanFish.GetLocationName());
						continue;
					}
					break;
				}
				if (waterLocation == waterLocation.SouthOcean && !flag)
				{
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.southernOceanFish.GetLocationName());
					continue;
				}
				switch (waterLocation)
				{
				case waterLocation.Rivers:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.riverFish.GetLocationName());
					break;
				case waterLocation.Mangroves:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.mangroveFish.GetLocationName());
					break;
				case waterLocation.UndergroundLake:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.undergroundFish.GetLocationName());
					break;
				case waterLocation.ReefIsland:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.reefIslandFish.GetLocationName());
					break;
				}
			}
		}
		if (myLandLocation.Length != 0)
		{
			landLocation[] array2 = myLandLocation;
			for (int i = 0; i < array2.Length; i++)
			{
				switch (array2[i])
				{
				case landLocation.All:
					return ConversationGenerator.generate.GetBiomeNameText("Everywhere");
				case landLocation.Bushland:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.bushlandBugs.GetLocationName());
					break;
				case landLocation.Desert:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.desertBugs.GetLocationName());
					break;
				case landLocation.Pines:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.pineLandBugs.GetLocationName());
					break;
				case landLocation.Plains:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.plainsBugs.GetLocationName());
					break;
				case landLocation.Tropics:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.topicalBugs.GetLocationName());
					break;
				case landLocation.Underground:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.underGroundBugs.GetLocationName());
					break;
				case landLocation.ReefIsland:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.reefIslandBugs.GetLocationName());
					break;
				}
			}
		}
		return text;
	}

	public void getSeasonName()
	{
		PediaManager.manage.seasonIcons[0].color = PediaManager.manage.iconOffColor;
		PediaManager.manage.seasonIcons[1].color = PediaManager.manage.iconOffColor;
		PediaManager.manage.seasonIcons[2].color = PediaManager.manage.iconOffColor;
		PediaManager.manage.seasonIcons[3].color = PediaManager.manage.iconOffColor;
		seasonFound[] array = mySeasons;
		foreach (seasonFound seasonFound in array)
		{
			if (seasonFound == seasonFound.All)
			{
				PediaManager.manage.seasonIcons[0].color = PediaManager.manage.iconOnColor;
				PediaManager.manage.seasonIcons[1].color = PediaManager.manage.iconOnColor;
				PediaManager.manage.seasonIcons[2].color = PediaManager.manage.iconOnColor;
				PediaManager.manage.seasonIcons[3].color = PediaManager.manage.iconOnColor;
			}
			else
			{
				PediaManager.manage.seasonIcons[(int)(seasonFound - 1)].color = PediaManager.manage.iconOnColor;
			}
		}
	}

	public void getTime()
	{
		PediaManager.manage.timeIcons[0].color = PediaManager.manage.iconOffColor;
		PediaManager.manage.timeIcons[1].color = PediaManager.manage.iconOffColor;
		PediaManager.manage.timeIcons[2].color = PediaManager.manage.iconOffColor;
		timeOfDay[] array = myTimeOfDay;
		foreach (timeOfDay timeOfDay in array)
		{
			switch (timeOfDay)
			{
			case timeOfDay.All:
				PediaManager.manage.timeIcons[0].color = PediaManager.manage.iconOnColor;
				PediaManager.manage.timeIcons[1].color = PediaManager.manage.iconOnColor;
				PediaManager.manage.timeIcons[2].color = PediaManager.manage.iconOnColor;
				continue;
			case timeOfDay.Morning:
				PediaManager.manage.timeIcons[0].color = PediaManager.manage.iconOnColor;
				break;
			}
			if (timeOfDay == timeOfDay.Day)
			{
				PediaManager.manage.timeIcons[1].color = PediaManager.manage.iconOnColor;
			}
			if (timeOfDay == timeOfDay.Night)
			{
				PediaManager.manage.timeIcons[2].color = PediaManager.manage.iconOnColor;
			}
		}
	}

	private string capitaliseFirstLetter(string toChange)
	{
		return toChange;
	}
}
