using System;
using UnityEngine;

[Serializable]
public class TownEvent
{
	public int eventDay;

	public int eventMonth;

	public string eventName;

	public Sprite eventSprite;

	public int bandStandStatusOnDay;

	public TownEventManager.TownEventType myEventType;

	public bool isEventToday()
	{
		return isEventDay(WorldManager.Instance.day, WorldManager.Instance.week, WorldManager.Instance.month);
	}

	public bool isEventDay(int checkDay, int checkWeek, int checkMonth)
	{
		if (checkDay + (checkWeek - 1) * 7 == eventDay && checkMonth == eventMonth)
		{
			return true;
		}
		return false;
	}

	public bool isEventDay(int checkCalendarDay)
	{
		if (!TownEventManager.manage.CheckEventPossible())
		{
			return false;
		}
		if (checkCalendarDay == eventDay && WorldManager.Instance.month == eventMonth)
		{
			return true;
		}
		return false;
	}

	public string getEventName()
	{
		return ConversationManager.manage.GetLocByTag(eventName).Replace("<IslandName>", NetworkMapSharer.Instance.islandName);
	}
}
