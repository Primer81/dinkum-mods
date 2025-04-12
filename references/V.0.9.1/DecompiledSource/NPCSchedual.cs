using System.Collections.Generic;
using UnityEngine;

public class NPCSchedual : MonoBehaviour
{
	public enum Locations
	{
		Wonder,
		Exit,
		Market_place,
		Johns_Goods,
		Craft_Workshop,
		Post_Office,
		Cloth_Shop,
		Weapon_Shop,
		Museum,
		Animal_Shop,
		Furniture_Shop,
		Plant_Shop,
		Town_Hall,
		Harbour_House,
		Bank,
		Hair_Dresser,
		NPCHouse1,
		NPCHouse2,
		NPCHouse3,
		NPCHouse4,
		JimmysBoat,
		Telepad,
		Mine,
		Barber,
		Tuckshop,
		BandStand,
		Airport,
		WishingWell
	}

	public string[] visitorTimeTable;

	public Locations[] dailySchedual;

	public Locations[] todaysSchedual;

	public bool[] dayOff = new bool[7];

	public Locations[] dayOffSchedule;

	public Locations getDesiredLocation(int npcNo, int hour, int dayToCheck)
	{
		if (NPCManager.manage.npcStatus[npcNo].checkIfHasMovedIn() || NPCManager.manage.NPCDetails[npcNo].isSpecialGuest)
		{
			if (!dayOff[dayToCheck - 1])
			{
				return todaysSchedual[hour];
			}
			return dayOffSchedule[hour];
		}
		return NPCManager.manage.visitingSchedual[hour];
	}

	public string getNextDayOffName()
	{
		for (int i = WorldManager.Instance.day - 1; i < dayOff.Length; i++)
		{
			if (dayOff[i])
			{
				return RealWorldTimeLight.time.getDayName(i);
			}
		}
		for (int j = 0; j < dayOff.Length; j++)
		{
			if (dayOff[j])
			{
				return RealWorldTimeLight.time.getDayName(j);
			}
		}
		return ConversationGenerator.generate.GetTimeNameByTag("NoDayOff");
	}

	public bool checkIfOpen(int npcId)
	{
		if (RealWorldTimeLight.time.currentHour == 0 || RealWorldTimeLight.time.currentHour == 24)
		{
			return false;
		}
		if (dayOff[WorldManager.Instance.day - 1])
		{
			return false;
		}
		if (todaysSchedual[RealWorldTimeLight.time.currentHour] == NPCManager.manage.NPCDetails[npcId].workLocation)
		{
			return true;
		}
		return false;
	}

	public bool checkIfLate(int npcId)
	{
		if (RealWorldTimeLight.time.currentHour == 0 || RealWorldTimeLight.time.currentHour == 24)
		{
			return false;
		}
		if (dayOff[WorldManager.Instance.day - 1])
		{
			return false;
		}
		if (todaysSchedual[RealWorldTimeLight.time.currentHour] == NPCManager.manage.NPCDetails[npcId].workLocation && todaysSchedual[RealWorldTimeLight.time.currentHour] != 0 && todaysSchedual[RealWorldTimeLight.time.currentHour] != Locations.Exit && (todaysSchedual[RealWorldTimeLight.time.currentHour - 1] != 0 || RealWorldTimeLight.time.currentMinute >= 15))
		{
			return true;
		}
		return false;
	}

	public bool checkIfShopIsOpen(int npcId, int hourToCheck, int dayToCheck)
	{
		if (dayOff[dayToCheck - 1])
		{
			return false;
		}
		if (dailySchedual[hourToCheck] == NPCManager.manage.NPCDetails[npcId].workLocation)
		{
			return true;
		}
		return false;
	}

	public string getOpeningHours()
	{
		string text = ConversationManager.manage.GetLocByTag("OpeningHours") + " ";
		bool flag = false;
		for (int i = 8; i < dailySchedual.Length; i++)
		{
			if (dailySchedual[i] != 0 && !flag)
			{
				flag = true;
				text = ((!OptionsMenu.options.use24HourTime) ? ((i >= 12) ? ((i <= 12) ? (text + (i - 12) + ":00PM - ") : (text + i + ":00PM - ")) : (text + i + ":00AM - ")) : (text + i + ":00 - "));
			}
			else if (dailySchedual[i] == Locations.Exit)
			{
				text = ((!OptionsMenu.options.use24HourTime) ? ((i >= 12) ? ((i != 12) ? (text + (i - 12) + ":00PM") : (text + i + ":00PM")) : (text + i + ":00AM")) : (text + i + ":00"));
				break;
			}
		}
		return text;
	}

	public string getDaysClosed()
	{
		TownEvent townEvent = TownEventManager.manage.checkEventDay(WorldManager.Instance.day, WorldManager.Instance.week, WorldManager.Instance.month);
		if (townEvent != null)
		{
			return string.Format(ConversationManager.manage.GetLocByTag("ClosedDueToEventName"), townEvent.getEventName());
		}
		string text = ConversationManager.manage.GetLocByTag("ClosedOn") + " ";
		List<string> list = new List<string>();
		for (int i = 0; i < dayOff.Length; i++)
		{
			if (dayOff[i])
			{
				list.Add(RealWorldTimeLight.time.getDayName(i));
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			text = ((j != 0) ? ((j != list.Count - 1) ? (text + ", " + list[j]) : (text + " & " + list[j])) : (text + list[j]));
		}
		return text;
	}
}
