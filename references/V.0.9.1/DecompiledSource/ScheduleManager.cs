using UnityEngine;

public class ScheduleManager : MonoBehaviour
{
	public static ScheduleManager manage;

	private void Awake()
	{
		manage = this;
	}

	public void giveNpcsNewDaySchedual(int newDay, int newWeek, int newMonth)
	{
		for (int i = 0; i < NPCManager.manage.NPCDetails.Length; i++)
		{
			fillTodaysSchedual(i, newDay, newWeek, newMonth);
			if (!NPCManager.manage.npcStatus[i].hasMovedIn || TownEventManager.manage.checkEventDay(newDay, newWeek, newMonth) != null || (newDay < 6 && Random.Range(0, 3) != 1))
			{
				continue;
			}
			for (int j = 18; j < 23; j++)
			{
				if (NPCManager.manage.NPCDetails[i].mySchedual.dailySchedual[j] != NPCManager.manage.NPCDetails[i].workLocation)
				{
					NPCManager.manage.NPCDetails[i].mySchedual.todaysSchedual[j] = NPCSchedual.Locations.Tuckshop;
					NPCManager.manage.NPCDetails[i].mySchedual.dayOffSchedule[j] = NPCSchedual.Locations.Tuckshop;
				}
			}
		}
	}

	public void fillTodaysSchedual(int npcId, int dayToFill, int weekToFill, int monthToFill)
	{
		if (TownEventManager.manage.checkEventDay(dayToFill, weekToFill, monthToFill) != null)
		{
			if (NPCManager.manage.npcStatus[npcId].hasMovedIn || ((bool)NetworkMapSharer.Instance && !NetworkMapSharer.Instance.isServer))
			{
				NPCManager.manage.NPCDetails[npcId].mySchedual.todaysSchedual = new NPCSchedual.Locations[NPCManager.manage.NPCDetails[npcId].mySchedual.dailySchedual.Length];
				NPCManager.manage.NPCDetails[npcId].mySchedual.dayOffSchedule = new NPCSchedual.Locations[NPCManager.manage.NPCDetails[npcId].mySchedual.dailySchedual.Length];
				fillDayForEvent(npcId);
			}
			return;
		}
		NPCManager.manage.NPCDetails[npcId].mySchedual.todaysSchedual = new NPCSchedual.Locations[NPCManager.manage.NPCDetails[npcId].mySchedual.dailySchedual.Length];
		for (int i = 0; i < NPCManager.manage.NPCDetails[npcId].mySchedual.dailySchedual.Length; i++)
		{
			NPCManager.manage.NPCDetails[npcId].mySchedual.todaysSchedual[i] = NPCManager.manage.NPCDetails[npcId].mySchedual.dailySchedual[i];
		}
		NPCManager.manage.NPCDetails[npcId].mySchedual.dayOffSchedule = new NPCSchedual.Locations[NPCManager.manage.NPCDetails[npcId].mySchedual.dailySchedual.Length];
		if (NPCManager.manage.NPCDetails[npcId].mySchedual.dayOff[dayToFill - 1])
		{
			fillDayOffSchedule(npcId, dayToFill);
		}
	}

	public void fillDayOffSchedule(int npcId, int dayToFill)
	{
		int num = Random.Range(8, 16);
		int num2 = Random.Range(2, 5);
		int randomNPCToVisit = getRandomNPCToVisit(npcId, num, dayToFill);
		NPCManager.manage.NPCDetails[npcId].mySchedual.dayOffSchedule[6] = NPCManager.manage.NPCDetails[npcId].mySchedual.dailySchedual[6];
		if (randomNPCToVisit != -1)
		{
			for (int i = 0; i < num2; i++)
			{
				if (NPCManager.manage.NPCDetails[randomNPCToVisit].mySchedual.checkIfShopIsOpen(randomNPCToVisit, num + i, dayToFill))
				{
					NPCManager.manage.NPCDetails[npcId].mySchedual.dayOffSchedule[num + i] = NPCManager.manage.NPCDetails[randomNPCToVisit].workLocation;
				}
			}
		}
		NPCManager.manage.NPCDetails[npcId].mySchedual.dayOffSchedule[0] = NPCManager.manage.NPCDetails[npcId].mySchedual.dailySchedual[0];
	}

	public void fillDayForEvent(int npcId)
	{
		for (int i = 0; i < NPCManager.manage.NPCDetails[npcId].mySchedual.dailySchedual.Length; i++)
		{
			if (npcId == 6 && i != 0)
			{
				NPCManager.manage.NPCDetails[npcId].mySchedual.todaysSchedual[i] = NPCSchedual.Locations.BandStand;
				NPCManager.manage.NPCDetails[npcId].mySchedual.dayOffSchedule[i] = NPCSchedual.Locations.BandStand;
			}
			else
			{
				NPCManager.manage.NPCDetails[npcId].mySchedual.todaysSchedual[i] = NPCSchedual.Locations.Wonder;
				NPCManager.manage.NPCDetails[npcId].mySchedual.dayOffSchedule[i] = NPCSchedual.Locations.Wonder;
			}
		}
		NPCManager.manage.NPCDetails[npcId].mySchedual.dayOffSchedule[6] = NPCSchedual.Locations.BandStand;
		NPCManager.manage.NPCDetails[npcId].mySchedual.todaysSchedual[6] = NPCSchedual.Locations.BandStand;
	}

	public int getRandomNPCToVisit(int myNpcId, int visitingTime, int visitingDay)
	{
		int i = 0;
		int num = Random.Range(0, NPCManager.manage.NPCDetails.Length);
		for (; i < 25; i++)
		{
			if (num != myNpcId && NPCManager.manage.npcStatus[num].hasMovedIn && NPCManager.manage.NPCDetails[num].mySchedual.checkIfShopIsOpen(num, visitingTime, visitingDay))
			{
				return num;
			}
			num = Random.Range(0, NPCManager.manage.NPCDetails.Length);
		}
		return -1;
	}
}
