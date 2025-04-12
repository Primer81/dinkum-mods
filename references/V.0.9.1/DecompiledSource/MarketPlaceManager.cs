using System.Collections.Generic;
using UnityEngine;

public class MarketPlaceManager : MonoBehaviour
{
	public static MarketPlaceManager manage;

	public int[] marketPos;

	private int currentShopId = -1;

	private List<int> randomVisitorNPCs = new List<int>();

	public int lastVisitor;

	public bool trapperCanSpawn;

	public bool trapperHasSpawned;

	public void Awake()
	{
		manage = this;
	}

	public string getCurrentVisitorsName()
	{
		if (currentShopId > -1)
		{
			return NPCManager.manage.NPCDetails[WorldManager.Instance.onTileStatusMap[marketPos[0], marketPos[1]] - 1].GetNPCName();
		}
		return "no one";
	}

	public bool someoneVisiting()
	{
		if (currentShopId > -1)
		{
			return true;
		}
		return false;
	}

	public void placeMarketStallAndSpawnNPC()
	{
		if (marketPos.Length == 2)
		{
			int num = currentShopId;
			_ = WorldManager.Instance.onTileMap[marketPos[0], marketPos[1]];
			if (!NPCManager.manage.npcStatus[1].checkIfHasMovedIn())
			{
				num = 1;
			}
			else if (currentShopId == 2 && !NPCManager.manage.npcStatus[2].checkIfHasMovedIn() && CraftsmanManager.manage.craftsmanHasItemReady())
			{
				num = 2;
			}
			else if (TownEventManager.manage.checkEventDay(RealWorldTimeLight.time.getTomorrowsDay(), RealWorldTimeLight.time.getTomorrowsWeek(), RealWorldTimeLight.time.getTomorrowsMonth()) != null)
			{
				num = -1;
			}
			else if (CatchingCompetitionManager.manage.isBugCompDay(RealWorldTimeLight.time.getTomorrowsDay(), RealWorldTimeLight.time.getTomorrowsWeek(), RealWorldTimeLight.time.getTomorrowsMonth()))
			{
				num = 13;
				CatchingCompetitionManager.manage.SetCorrectConversation();
			}
			else if (CatchingCompetitionManager.manage.isFishCompDay(RealWorldTimeLight.time.getTomorrowsDay(), RealWorldTimeLight.time.getTomorrowsWeek(), RealWorldTimeLight.time.getTomorrowsMonth()))
			{
				num = 14;
				CatchingCompetitionManager.manage.SetCorrectConversation();
			}
			else if (currentShopId < 0)
			{
				num = ((!NPCManager.manage.npcStatus[9].hasAskedToMoveIn && !NPCManager.manage.npcStatus[9].checkIfHasMovedIn()) ? 9 : (lastVisitor = getANewVisitor()));
			}
			else if (currentShopId == 9 && NPCManager.manage.npcStatus[currentShopId].hasAskedToMoveIn)
			{
				num = -1;
			}
			else if (NPCManager.manage.npcStatus[currentShopId].checkIfHasMovedIn())
			{
				num = -1;
			}
			else if (TownManager.manage.daysInTent == 0)
			{
				num = -1;
			}
			spawnMarketNPC(num);
			currentShopId = num;
			NetworkMapSharer.Instance.RpcGiveOnTileStatus(num + 1, marketPos[0], marketPos[1]);
			if (num >= 0)
			{
				NetworkMapSharer.Instance.RpcShowOffBuilding(manage.marketPos[0], manage.marketPos[1]);
			}
		}
	}

	private void spawnMarketNPC(int shopId)
	{
		if (shopId >= 0)
		{
			NPCManager.manage.setUpNPCAgent(shopId, 0, 0);
		}
	}

	public void getNpcOnLoad()
	{
		if (marketPos.Length == 2)
		{
			int shopId = (lastVisitor = WorldManager.Instance.onTileStatusMap[marketPos[0], marketPos[1]] - 1);
			currentShopId = WorldManager.Instance.onTileStatusMap[marketPos[0], marketPos[1]] - 1;
			spawnMarketNPC(shopId);
			checkForSpecialVisitors(WorldManager.Instance.day);
		}
	}

	public bool CheckIfRainingInMorning(int checkDay)
	{
		return NetworkMapSharer.Instance.todaysWeather[0].isRainy;
	}

	public void checkForSpecialVisitors(int checkDay)
	{
		if (CheckIfRainingInMorning(checkDay) && NPCManager.manage.npcStatus[1].checkIfHasMovedIn() && BankMenu.menu.accountBalance >= 1000000)
		{
			spawnJimmysBoat();
		}
		else
		{
			despawnJimmiesBoat();
		}
		if (NetworkMapSharer.Instance.wishManager.currentWishType != 1)
		{
			if (checkDay == 7 && LicenceManager.manage.allLicences[4].getCurrentLevel() >= 2)
			{
				trapperCanSpawn = true;
			}
			else if ((NetworkMapSharer.Instance.wishManager.currentWishType == 2 && checkDay == 1) || (NetworkMapSharer.Instance.wishManager.currentWishType == 2 && checkDay == 4))
			{
				trapperCanSpawn = true;
			}
			else
			{
				trapperCanSpawn = false;
			}
		}
		else
		{
			trapperCanSpawn = false;
		}
		if ((bool)TownManager.manage.allShopFloors[26] && checkDay == 1)
		{
			NPCManager.manage.setUpNPCAgent(15, Mathf.RoundToInt(TownManager.manage.allShopFloors[26].transform.position.x / 2f), Mathf.RoundToInt(TownManager.manage.allShopFloors[26].transform.position.z / 2f));
			NPCManager.manage.getMapAgentForNPC(15);
		}
		if (NPCManager.manage.npcStatus[1].checkIfHasMovedIn() && RealWorldEventChecker.check.getCurrentEvent() == RealWorldEventChecker.TimedEvent.Chrissy && !CheckIfRainingInMorning(checkDay))
		{
			bool flag = false;
			int num = 30000;
			while (!flag && num > 0)
			{
				int num2 = Random.Range(200, 800);
				int num3 = Random.Range(200, 800);
				if (!WorldManager.Instance.waterMap[num2, num3] && WorldManager.Instance.tileTypeMap[num2, num3] == 3 && AnimalManager.manage.canSpawnOnTile(num2, num3))
				{
					Vector3 vector = new Vector3(num2 * 2, 0f, num3 * 2);
					if (NetworkNavMesh.nav.farAwayFromAllNPCs(vector))
					{
						NPCManager.manage.setUpNPCAgent(16, num2, num3);
						NPCManager.manage.getMapAgentForNPC(16).setNewDayDesire();
						flag = true;
						NetworkMapSharer.Instance.PlaceNickMarker(vector);
					}
				}
				num--;
			}
			if (!flag)
			{
				NetworkMapSharer.Instance.RemoveNickMarker();
			}
		}
		else
		{
			NetworkMapSharer.Instance.RemoveNickMarker();
		}
	}

	public void SpawnNed()
	{
		if (!trapperCanSpawn)
		{
			return;
		}
		trapperHasSpawned = false;
		bool flag = false;
		int num = 8000;
		while (!flag)
		{
			int num2 = Random.Range(200, 800);
			int num3 = Random.Range(200, 800);
			if (NetworkNavMesh.nav.farAwayFromAllNPCs(new Vector3(num2 * 2, WorldManager.Instance.heightMap[num2, num3], num3 * 2)) && AnimalManager.manage.checkIfTileIsWalkable(num2, num3))
			{
				if (!WorldManager.Instance.waterMap[num2, num3])
				{
					NPCManager.manage.setUpNPCAgent(5, num2, num3);
					flag = true;
				}
			}
			else
			{
				num--;
				if (num < 0)
				{
					break;
				}
			}
		}
	}

	public void SpawnGoGoAgent()
	{
		if (!AgentCanSpawn())
		{
			return;
		}
		bool flag = false;
		int num = 8000;
		while (!flag)
		{
			int num2 = Random.Range(200, 800);
			int num3 = Random.Range(200, 800);
			if (NetworkNavMesh.nav.farAwayFromAllNPCs(new Vector3(num2 * 2, WorldManager.Instance.heightMap[num2, num3], num3 * 2), 100f) && AnimalManager.manage.checkIfTileIsWalkable(num2, num3))
			{
				if (!WorldManager.Instance.waterMap[num2, num3])
				{
					MonoBehaviour.print("Agent Spawned at " + (float)num2 * 2f + "," + (float)num3 * 2f);
					NPCManager.manage.setUpNPCAgent(17, num2, num3);
					flag = true;
				}
			}
			else
			{
				num--;
				if (num < 0)
				{
					break;
				}
			}
		}
	}

	public bool AgentCanSpawn()
	{
		if (WorldManager.Instance.week >= 4 && WorldManager.Instance.day == 2 && NPCManager.manage.npcStatus[1].checkIfHasMovedIn() && currentShopId == -1)
		{
			return true;
		}
		return false;
	}

	public int howManyNPCsCanVisit()
	{
		int num = 0;
		for (int i = 0; i < randomVisitorNPCs.Count; i++)
		{
			if (!NPCManager.manage.npcStatus[i].checkIfHasMovedIn() && i != 5)
			{
				num++;
			}
		}
		return num;
	}

	public int getANewVisitor()
	{
		int num = -1;
		generateAvaliableVisitors();
		if (randomVisitorNPCs.Count == 0)
		{
			return -1;
		}
		bool flag = false;
		int num2 = 0;
		while (!flag)
		{
			num = randomVisitorNPCs[Random.Range(0, randomVisitorNPCs.Count)];
			if (NPCManager.manage.npcStatus[num].checkIfHasMovedIn())
			{
				continue;
			}
			if (num != lastVisitor && num != 5)
			{
				flag = true;
				continue;
			}
			num2++;
			if (num2 >= 500)
			{
				num = randomVisitorNPCs[Random.Range(0, randomVisitorNPCs.Count)];
				break;
			}
		}
		return num;
	}

	public bool noOneVisiting()
	{
		return currentShopId == -1;
	}

	public void spawnJimmysBoat()
	{
		for (int i = 0; i < SaveLoad.saveOrLoad.vehiclesToSave.Count; i++)
		{
			if (SaveLoad.saveOrLoad.vehiclesToSave[i].saveId == 10)
			{
				NPCManager.manage.setUpNPCAgent(11, 0, 0);
				NPCMapAgent mapAgentForNPC = NPCManager.manage.getMapAgentForNPC(11);
				mapAgentForNPC.setNewDayDesire();
				mapAgentForNPC.warpNpcInside();
				return;
			}
		}
		int num = 50 + Random.Range(-35, 35);
		int num2 = 50 + Random.Range(-35, 35);
		bool flag = checkIfChunkCanHaveBoat(num, num2);
		int num3 = 50000;
		while (!flag)
		{
			num = 50 + Random.Range(-35, 35);
			num2 = 50 + Random.Range(-35, 35);
			flag = checkIfChunkCanHaveBoat(num, num2);
			num3--;
			if (num3 < 0)
			{
				break;
			}
		}
		if (flag)
		{
			GameObject spawnMe = Object.Instantiate(SaveLoad.saveOrLoad.vehiclePrefabs[10], new Vector3(num * 20, 0.4f, num2 * 20), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
			NetworkMapSharer.Instance.spawnGameObject(spawnMe);
			NPCManager.manage.setUpNPCAgent(11, 0, 0);
			NPCMapAgent mapAgentForNPC2 = NPCManager.manage.getMapAgentForNPC(11);
			mapAgentForNPC2.setNewDayDesire();
			mapAgentForNPC2.warpNpcInside();
		}
	}

	public void despawnJimmiesBoat()
	{
		for (int i = 0; i < SaveLoad.saveOrLoad.vehiclesToSave.Count; i++)
		{
			if (SaveLoad.saveOrLoad.vehiclesToSave[i].saveId == 10)
			{
				SaveLoad.saveOrLoad.vehiclesToSave[i].destroyServerSelf();
			}
		}
	}

	private bool checkIfChunkCanHaveBoat(int chunkX, int chunkY)
	{
		for (int i = 0; i < WorldManager.Instance.getChunkSize(); i++)
		{
			for (int j = 0; j < WorldManager.Instance.getChunkSize(); j++)
			{
				if (!WorldManager.Instance.waterMap[chunkX * 10 + j, chunkY * 10 + i] || WorldManager.Instance.heightMap[chunkX * 10 + j, chunkY * 10 + i] >= 0)
				{
					return false;
				}
			}
		}
		return true;
	}

	public void generateAvaliableVisitors()
	{
		randomVisitorNPCs.Clear();
		addNPCIfNotMovedIn(2);
		addNPCIfNotMovedIn(4);
		if (TownManager.manage.getCurrentHouseStage() >= 1)
		{
			addNPCIfNotMovedIn(3);
		}
		if (LicenceManager.manage.allLicences[11].hasALevelOneOrHigher())
		{
			addNPCIfNotMovedIn(0);
			addNPCIfNotMovedIn(0);
			addNPCIfNotMovedIn(0);
			addNPCIfNotMovedIn(0);
		}
		if (LicenceManager.manage.allLicences[9].hasALevelOneOrHigher())
		{
			addNPCIfNotMovedIn(7);
			addNPCIfNotMovedIn(7);
		}
		if (NPCManager.manage.getNoOfNPCsMovedIn() >= 5)
		{
			addNPCIfNotMovedIn(12);
		}
		if (NPCManager.manage.getNoOfNPCsMovedIn() >= 5)
		{
			addNPCIfNotMovedIn(8);
		}
	}

	public void addNPCIfNotMovedIn(int npcId)
	{
		if (!NPCManager.manage.npcStatus[npcId].hasMovedIn)
		{
			randomVisitorNPCs.Add(npcId);
		}
	}
}
