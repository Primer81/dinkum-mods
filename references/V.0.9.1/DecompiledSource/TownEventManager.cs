using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class TownEventManager : NetworkBehaviour
{
	public enum TownEventType
	{
		None,
		IslandDay,
		SkyFest
	}

	public static TownEventManager manage;

	public TownEvent[] townEvents;

	public NPCSchedual.Locations specialEventLocation;

	public TownEventType townEventOn;

	public ConversationObject defaultFletchKeeperConvo;

	public ConversationObject islandDay_MorningConvo;

	public ConversationObject islandDay_GetSnagConvo;

	public ConversationObject islandDay_SnagWontFitConvo;

	public ConversationObject islandDay_NoonConvo;

	public ConversationObject islandDay_AfterNoonConvo;

	public ConversationObject islandDay_EveningConvo;

	public GameObject fireworks;

	public TileObject bandStand;

	public ASound specialEventChime;

	public bool snagAvaliable;

	public InventoryItem snag;

	[SyncVar]
	public bool eventPossible;

	public GameObject PaperLanternGo;

	public ConversationObject skyFest_MorningConvo;

	public ConversationObject skyFest_GetKiteConvo;

	public ConversationObject skyFest_KiteWontFitConvo;

	public ConversationObject skyFest_NoonConvo;

	public ConversationObject skyFest_AfterNoonConvo;

	public ConversationObject skyFest_EveningConvo;

	public InventoryItem kiteKit;

	public GameObject Aurora;

	public Material AuroramMat;

	public bool laternsActive;

	private GameObject eventSpecialEffect;

	private List<PaperLantern> allLanterns = new List<PaperLantern>();

	public bool NetworkeventPossible
	{
		get
		{
			return eventPossible;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref eventPossible))
			{
				bool flag = eventPossible;
				SetSyncVar(value, ref eventPossible, 1uL);
			}
		}
	}

	private void Awake()
	{
		manage = this;
	}

	public override void OnStartServer()
	{
		CheckEventPossible();
		ScheduleManager.manage.giveNpcsNewDaySchedual(WorldManager.Instance.day, WorldManager.Instance.week, WorldManager.Instance.month);
		checkForTownEventAndSetUp(WorldManager.Instance.day, WorldManager.Instance.week, WorldManager.Instance.month);
	}

	public bool CheckEventPossible()
	{
		if (base.isServer)
		{
			NetworkeventPossible = TownManager.manage.allShopFloors[25] != null;
		}
		return eventPossible;
	}

	public void checkForTownEventAndSetUp(int day, int week, int month)
	{
		TownEvent townEvent = checkEventDay(day, week, month);
		if (townEvent != null)
		{
			specialEventLocation = NPCSchedual.Locations.BandStand;
			townEventOn = townEvent.myEventType;
			NPCManager.manage.NPCDetails[6].workLocation = NPCSchedual.Locations.BandStand;
			if (townEventOn == TownEventType.IslandDay)
			{
				StartCoroutine(RunIslandDay());
			}
			if (townEventOn == TownEventType.SkyFest)
			{
				StartCoroutine(RunSkyFest());
			}
			setBandStandStatus(townEvent.bandStandStatusOnDay);
		}
		else
		{
			NPCManager.manage.NPCDetails[6].keeperConvos = defaultFletchKeeperConvo;
			NPCManager.manage.NPCDetails[6].workLocation = NPCSchedual.Locations.Post_Office;
			specialEventLocation = NPCSchedual.Locations.Wonder;
			townEventOn = TownEventType.None;
			setBandStandStatus();
			snagAvaliable = false;
		}
	}

	public TownEvent checkEventForToday(int day)
	{
		CheckEventPossible();
		for (int i = 0; i < townEvents.Length; i++)
		{
			if (townEvents[i].isEventDay(day))
			{
				return townEvents[i];
			}
		}
		return null;
	}

	public TownEvent checkEventDay(int day, int week, int month)
	{
		if (!CheckEventPossible())
		{
			return null;
		}
		for (int i = 0; i < townEvents.Length; i++)
		{
			if (townEvents[i].isEventDay(day, week, month))
			{
				return townEvents[i];
			}
		}
		return null;
	}

	public bool isEventToday(TownEventType checkType)
	{
		if (checkEventDay(WorldManager.Instance.day, WorldManager.Instance.week, WorldManager.Instance.month) == null)
		{
			return false;
		}
		return checkEventDay(WorldManager.Instance.day, WorldManager.Instance.week, WorldManager.Instance.month).myEventType == checkType;
	}

	public string AddOrdinal(int num)
	{
		if (num <= 0)
		{
			return num.ToString();
		}
		int num2 = num % 100;
		if ((uint)(num2 - 11) <= 2u)
		{
			return string.Format(ConversationGenerator.generate.GetDescriptionDetails("Placing_Fourth"), num.ToString());
		}
		return (num % 10) switch
		{
			1 => string.Format(ConversationGenerator.generate.GetDescriptionDetails("Placing_First"), num.ToString()), 
			2 => string.Format(ConversationGenerator.generate.GetDescriptionDetails("Placing_Second"), num.ToString()), 
			3 => string.Format(ConversationGenerator.generate.GetDescriptionDetails("Placing_Third"), num.ToString()), 
			_ => string.Format(ConversationGenerator.generate.GetDescriptionDetails("Placing_Fourth"), num.ToString()), 
		};
	}

	private IEnumerator RunIslandDay()
	{
		WaitForSeconds wait = new WaitForSeconds(1f);
		int arrivalHour = RealWorldTimeLight.time.currentHour;
		if (townEventOn == TownEventType.IslandDay)
		{
			NPCManager.manage.NPCDetails[6].keeperConvos = islandDay_MorningConvo;
		}
		while (townEventOn == TownEventType.IslandDay && RealWorldTimeLight.time.currentHour == 7 && RealWorldTimeLight.time.currentMinute < 3)
		{
			yield return null;
		}
		if (townEventOn == TownEventType.IslandDay)
		{
			NotificationManager.manage.makeTopNotification(string.Format(ConversationGenerator.generate.GetNotificationText("IslandDayAnnouncement"), townEvents[0].getEventName()), ConversationGenerator.generate.GetNotificationText("CallToBandStand"), specialEventChime, 10f);
			if (base.isServer)
			{
				NPCManager.manage.clearAllActiveNPCWantToWalkIntoForBandStand();
			}
		}
		while (townEventOn == TownEventType.IslandDay && RealWorldTimeLight.time.currentHour < 12)
		{
			yield return wait;
		}
		if (townEventOn == TownEventType.IslandDay && arrivalHour < 12)
		{
			snagAvaliable = true;
		}
		while (townEventOn == TownEventType.IslandDay && RealWorldTimeLight.time.currentHour < 15 && snagAvaliable)
		{
			yield return null;
			if (Inventory.Instance.checkIfItemCanFit(snag.getItemId(), 1))
			{
				NPCManager.manage.NPCDetails[6].keeperConvos = islandDay_GetSnagConvo;
			}
			else
			{
				NPCManager.manage.NPCDetails[6].keeperConvos = islandDay_SnagWontFitConvo;
			}
		}
		if (townEventOn == TownEventType.IslandDay)
		{
			NPCManager.manage.NPCDetails[6].keeperConvos = islandDay_NoonConvo;
		}
		while (townEventOn == TownEventType.IslandDay && RealWorldTimeLight.time.currentHour < 15)
		{
			yield return wait;
		}
		if (townEventOn == TownEventType.IslandDay)
		{
			NPCManager.manage.NPCDetails[6].keeperConvos = islandDay_AfterNoonConvo;
		}
		while (townEventOn == TownEventType.IslandDay && RealWorldTimeLight.time.currentHour < 20)
		{
			yield return wait;
		}
		if ((bool)eventSpecialEffect)
		{
			Object.Destroy(eventSpecialEffect);
		}
		eventSpecialEffect = Object.Instantiate(fireworks, CameraController.control.transform);
		if (townEventOn == TownEventType.IslandDay)
		{
			NPCManager.manage.NPCDetails[6].keeperConvos = islandDay_EveningConvo;
		}
		MusicManager.manage.playCompMusic();
		while (townEventOn == TownEventType.IslandDay)
		{
			yield return wait;
		}
		if ((bool)eventSpecialEffect)
		{
			Object.Destroy(eventSpecialEffect);
		}
		MusicManager.manage.stopCompMusic();
	}

	private IEnumerator RunSkyFest()
	{
		WaitForSeconds wait = new WaitForSeconds(1f);
		int arrivalHour = RealWorldTimeLight.time.currentHour;
		if (townEventOn == TownEventType.SkyFest)
		{
			NPCManager.manage.NPCDetails[6].keeperConvos = skyFest_MorningConvo;
		}
		while (townEventOn == TownEventType.SkyFest && RealWorldTimeLight.time.currentHour == 7 && RealWorldTimeLight.time.currentMinute < 3)
		{
			yield return null;
		}
		if (townEventOn == TownEventType.SkyFest)
		{
			NotificationManager.manage.makeTopNotification(string.Format(ConversationGenerator.generate.GetNotificationText("SkyfestAnnouncement"), townEvents[1].getEventName()), ConversationGenerator.generate.GetNotificationText("CallToBandStand"), specialEventChime, 10f);
			if (base.isServer)
			{
				NPCManager.manage.clearAllActiveNPCWantToWalkIntoForBandStand();
			}
		}
		while (townEventOn == TownEventType.SkyFest && RealWorldTimeLight.time.currentHour < 11)
		{
			yield return wait;
		}
		if (townEventOn == TownEventType.SkyFest && arrivalHour < 12)
		{
			snagAvaliable = true;
		}
		while (townEventOn == TownEventType.SkyFest && RealWorldTimeLight.time.currentHour < 15 && snagAvaliable)
		{
			yield return null;
			if (Inventory.Instance.checkIfItemCanFit(kiteKit.getItemId(), 1))
			{
				NPCManager.manage.NPCDetails[6].keeperConvos = skyFest_GetKiteConvo;
			}
			else
			{
				NPCManager.manage.NPCDetails[6].keeperConvos = skyFest_KiteWontFitConvo;
			}
		}
		if (townEventOn == TownEventType.SkyFest)
		{
			NPCManager.manage.NPCDetails[6].keeperConvos = skyFest_NoonConvo;
		}
		while (townEventOn == TownEventType.SkyFest && RealWorldTimeLight.time.currentHour < 15)
		{
			yield return wait;
		}
		if (townEventOn == TownEventType.SkyFest)
		{
			NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("FlyingLanternStarted"), ConversationGenerator.generate.GetNotificationText("LearnTheRules"), CatchingCompetitionManager.manage.compWhistleSound);
		}
		if (townEventOn == TownEventType.SkyFest)
		{
			NPCManager.manage.NPCDetails[6].keeperConvos = skyFest_AfterNoonConvo;
		}
		StartCoroutine(HandleSkyFestLanterns());
		while (townEventOn == TownEventType.SkyFest && RealWorldTimeLight.time.currentHour < 20)
		{
			yield return wait;
		}
		if ((bool)eventSpecialEffect)
		{
			Object.Destroy(eventSpecialEffect);
		}
		StartCoroutine(FadeInAurora());
		if (townEventOn == TownEventType.SkyFest)
		{
			NPCManager.manage.NPCDetails[6].keeperConvos = skyFest_EveningConvo;
		}
		MusicManager.manage.playCompMusic();
		while (townEventOn == TownEventType.SkyFest)
		{
			yield return wait;
		}
		if ((bool)eventSpecialEffect)
		{
			Object.Destroy(eventSpecialEffect);
		}
		Aurora.gameObject.SetActive(value: false);
		MusicManager.manage.stopCompMusic();
	}

	private IEnumerator FadeInAurora()
	{
		float timer = 0f;
		Color currentColour = AuroramMat.color;
		currentColour.a = 0f;
		AuroramMat.color = currentColour;
		Aurora.gameObject.SetActive(value: true);
		while (timer <= 1f)
		{
			AuroramMat.color = currentColour;
			yield return null;
			timer += Time.deltaTime / 25f;
			currentColour.a = Mathf.Lerp(0f, 1f, timer);
		}
		currentColour.a = 1f;
		AuroramMat.color = currentColour;
	}

	public void PlaceLanterns()
	{
		for (int i = 200; i < 800; i++)
		{
			for (int j = 200; j < 800; j++)
			{
				if (Random.Range(0, 100) == 50 && (WorldManager.Instance.onTileMap[j, i] == -1 || (WorldManager.Instance.onTileMap[j, i] >= 0 && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[j, i]].isGrass)))
				{
					WorldManager.Instance.onTileMap[j, i] = 864;
				}
			}
		}
	}

	public void setBandStandStatus(int tileStatus = 0)
	{
		if (!base.isServer)
		{
			return;
		}
		for (int i = 0; i < WorldManager.Instance.GetMapSize() / 10; i++)
		{
			for (int j = 0; j < WorldManager.Instance.GetMapSize() / 10; j++)
			{
				if (!WorldManager.Instance.chunkChangedMap[j, i])
				{
					continue;
				}
				for (int k = i * 10; k < i * 10 + 10; k++)
				{
					for (int l = j * 10; l < j * 10 + 10; l++)
					{
						if (WorldManager.Instance.onTileMap[l, k] != -1 && bandStand.tileObjectId == WorldManager.Instance.onTileMap[l, k])
						{
							WorldManager.Instance.onTileStatusMap[l, k] = tileStatus;
							NetworkMapSharer.Instance.RpcGiveOnTileStatus(tileStatus, l, k);
							return;
						}
					}
				}
			}
		}
	}

	public bool IsJohnsAnniversary()
	{
		if (WorldManager.Instance.year >= 2 && WorldManager.Instance.month == 1 && WorldManager.Instance.week == 1)
		{
			return true;
		}
		return false;
	}

	public bool IsJohnsAnniversary(int date)
	{
		if (WorldManager.Instance.year >= 2 && WorldManager.Instance.month == 1 && date > 1 && date <= 7)
		{
			return true;
		}
		return false;
	}

	public Vector3 GetRandomLanternLocation()
	{
		return NetworkNavMesh.nav.GetRandomPlayerPosition() + WeatherManager.Instance.windMgr.WindDirection * -20f + new Vector3(Random.Range(-100f, 100f), 0f, Random.Range(-100f, 100f)) + Vector3.up * 45f;
	}

	private IEnumerator HandleSkyFestLanterns()
	{
		if (!base.isServer)
		{
			yield break;
		}
		laternsActive = true;
		WaitForSeconds paperWait = new WaitForSeconds(2f);
		while (townEventOn == TownEventType.SkyFest && RealWorldTimeLight.time.currentHour >= 15)
		{
			yield return paperWait;
			if (allLanterns.Count < 50)
			{
				int num = Random.Range(0, 5);
				for (int i = 0; i < num; i++)
				{
					PaperLantern component = Object.Instantiate(PaperLanternGo, GetRandomLanternLocation(), Quaternion.identity).GetComponent<PaperLantern>();
					allLanterns.Add(component);
					NetworkServer.Spawn(component.gameObject);
				}
			}
		}
		laternsActive = false;
		while (townEventOn == TownEventType.SkyFest)
		{
			yield return paperWait;
		}
		for (int j = 0; j < allLanterns.Count; j++)
		{
			if (allLanterns[j] != null && (bool)allLanterns[j].gameObject)
			{
				NetworkServer.Destroy(allLanterns[j].gameObject);
			}
		}
		allLanterns.Clear();
	}

	private void MirrorProcessed()
	{
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(eventPossible);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(eventPossible);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			bool flag = eventPossible;
			NetworkeventPossible = reader.ReadBool();
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			bool flag2 = eventPossible;
			NetworkeventPossible = reader.ReadBool();
		}
	}
}
