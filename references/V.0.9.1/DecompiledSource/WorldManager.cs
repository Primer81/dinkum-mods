using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WorldManager : MonoBehaviour
{
	public enum MapType
	{
		OnTileMap,
		TileTypeMap,
		HeightMap
	}

	public static WorldManager Instance;

	public int versionNumber;

	public int masterVersionNumber = 4;

	public NetworkMapSharer netMapSharer;

	public NetworkNavMesh netNavMesh;

	public RealWorldTimeLight netTime;

	private static int mapSize = 1000;

	public int chunkSize = 10;

	public int tileSize = 2;

	public float testSize;

	public List<TileObject>[] allObjectsSorted;

	public List<TileObject> freeObjects = new List<TileObject>();

	public int[,] heightMap;

	public int[,] tileTypeMap;

	public int[,] onTileMap;

	public int[,] onTileStatusMap;

	public int[,] tileTypeStatusMap;

	public int[,] rotationMap;

	public bool[,] waterMap;

	public int[,] fencedOffMap;

	public bool[,] clientRequestedMap;

	public bool[,] chunkChangedMap;

	public bool[,] changedMapOnTile;

	public bool[,] changedMapHeight;

	public bool[,] changedMapWater;

	public bool[,] changedMapTileType;

	public bool[,] chunkHasChangedToday;

	public bool[,] chunkWithFenceInIt;

	public Transform spawnPos;

	public TileObject[] allObjects;

	public TileObjectSettings[] allObjectSettings;

	public List<Chunk> chunksInUse;

	public List<DroppedItem> itemsOnGround;

	public List<PickUpAndCarry> allCarriables;

	public GameObject ChunkPrefab;

	public GameObject ChunkLoaderPrfab;

	public GameObject droppedItemPrefab;

	public bool firstChunkLayed;

	public Material stoneSide;

	public TileTypes fallBackTileType;

	public TileTypes[] tileTypes;

	public UnityEvent changeDayEvent = new UnityEvent();

	public int day = 1;

	public int week = 1;

	public int month = 1;

	public int year = 1;

	public List<int[]> chunksToRefresh = new List<int[]>();

	public GameObject firstConnectAirShip;

	public ReadableSign confirmSleepSign;

	public ConversationObject confirmSleepConvo;

	public ConversationObject sleepUndergroundConvo;

	public ConversationObject sleepHouseMovingConvo;

	public ConversationObject sleepOffIsland;

	private List<int[]> clientLock = new List<int[]>();

	private List<int[]> clientLockHouse = new List<int[]>();

	public bool chunkRefreshCompleted;

	private int completedCropChecker;

	public List<CurrentChanger> allChangers = new List<CurrentChanger>();

	private WaitForSeconds waterSec = new WaitForSeconds(0.25f);

	private WaitForSeconds sec = new WaitForSeconds(1f);

	public LayerMask pickUpLayer;

	private void Awake()
	{
		Instance = this;
		heightMap = new int[mapSize, mapSize];
		tileTypeMap = new int[mapSize, mapSize];
		onTileMap = new int[mapSize, mapSize];
		onTileStatusMap = new int[mapSize, mapSize];
		tileTypeStatusMap = new int[mapSize, mapSize];
		rotationMap = new int[mapSize, mapSize];
		waterMap = new bool[mapSize, mapSize];
		fencedOffMap = new int[mapSize, mapSize];
		clientRequestedMap = new bool[mapSize / chunkSize, mapSize / chunkSize];
		chunkChangedMap = new bool[mapSize / chunkSize, mapSize / chunkSize];
		changedMapOnTile = new bool[mapSize / chunkSize, mapSize / chunkSize];
		changedMapHeight = new bool[mapSize / chunkSize, mapSize / chunkSize];
		changedMapWater = new bool[mapSize / chunkSize, mapSize / chunkSize];
		changedMapTileType = new bool[mapSize / chunkSize, mapSize / chunkSize];
		chunkHasChangedToday = new bool[mapSize / chunkSize, mapSize / chunkSize];
		chunkWithFenceInIt = new bool[mapSize / chunkSize, mapSize / chunkSize];
		allObjectsSorted = new List<TileObject>[allObjects.Length];
		for (int i = 0; i < allObjectsSorted.Length; i++)
		{
			allObjectsSorted[i] = new List<TileObject>();
		}
		NetworkMapSharer.Instance = netMapSharer;
		NetworkNavMesh.nav = netNavMesh;
		RealWorldTimeLight.time = netTime;
		_ = Application.isEditor;
	}

	public ConversationObject GetSleepText()
	{
		if (RealWorldTimeLight.time.underGround)
		{
			return sleepUndergroundConvo;
		}
		if (RealWorldTimeLight.time.offIsland)
		{
			return sleepOffIsland;
		}
		if (TownManager.manage.checkIfInMovingBuildingForSleep())
		{
			return sleepHouseMovingConvo;
		}
		return confirmSleepConvo;
	}

	private void Start()
	{
		heightMap = new int[mapSize, mapSize];
		tileTypeMap = new int[mapSize, mapSize];
		onTileMap = new int[mapSize, mapSize];
		onTileStatusMap = new int[mapSize, mapSize];
		tileTypeStatusMap = new int[mapSize, mapSize];
		rotationMap = new int[mapSize, mapSize];
		waterMap = new bool[mapSize, mapSize];
		fencedOffMap = new int[mapSize, mapSize];
		clientRequestedMap = new bool[mapSize / chunkSize, mapSize / chunkSize];
		chunkChangedMap = new bool[mapSize / chunkSize, mapSize / chunkSize];
		changedMapOnTile = new bool[mapSize / chunkSize, mapSize / chunkSize];
		changedMapHeight = new bool[mapSize / chunkSize, mapSize / chunkSize];
		changedMapWater = new bool[mapSize / chunkSize, mapSize / chunkSize];
		changedMapTileType = new bool[mapSize / chunkSize, mapSize / chunkSize];
		chunkHasChangedToday = new bool[mapSize / chunkSize, mapSize / chunkSize];
		TileObject[] array = allObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].checkForAllExtensions();
		}
	}

	public bool CheckTileClientLock(int xPos, int yPos)
	{
		for (int i = 0; i < clientLock.Count; i++)
		{
			if (clientLock[i][0] == xPos && clientLock[i][1] == yPos)
			{
				return true;
			}
		}
		return false;
	}

	public void lockTileClient(int xPos, int yPos)
	{
		for (int i = 0; i < clientLock.Count; i++)
		{
			if (clientLock[i][0] == xPos && clientLock[i][1] == yPos)
			{
				return;
			}
		}
		clientLock.Add(new int[2] { xPos, yPos });
	}

	public void unlockClientTile(int xPos, int yPos)
	{
		for (int i = 0; i < clientLock.Count; i++)
		{
			if (clientLock[i][0] == xPos && clientLock[i][1] == yPos)
			{
				clientLock.RemoveAt(i);
				break;
			}
		}
	}

	public bool checkTileClientLockHouse(int xPos, int yPos, int houseX, int houseY)
	{
		for (int i = 0; i < clientLockHouse.Count; i++)
		{
			if (clientLockHouse[i][2] == houseX && clientLockHouse[i][3] == houseY && clientLockHouse[i][0] == xPos && clientLockHouse[i][1] == yPos)
			{
				return true;
			}
		}
		return false;
	}

	public void lockTileHouseClient(int xPos, int yPos, int houseX, int houseY)
	{
		for (int i = 0; i < clientLockHouse.Count; i++)
		{
			if (clientLockHouse[i][2] == houseX && clientLockHouse[i][3] == houseY && clientLockHouse[i][0] == xPos && clientLockHouse[i][1] == yPos)
			{
				return;
			}
		}
		clientLockHouse.Add(new int[4] { xPos, yPos, houseX, houseY });
	}

	public void unlockClientTileHouse(int xPos, int yPos, int houseX, int houseY)
	{
		for (int i = 0; i < clientLockHouse.Count; i++)
		{
			if (clientLockHouse[i][2] == houseX && clientLockHouse[i][3] == houseY && clientLockHouse[i][0] == xPos && clientLockHouse[i][1] == yPos)
			{
				clientLockHouse.RemoveAt(i);
				break;
			}
		}
	}

	public DateSave getDateSave()
	{
		return new DateSave
		{
			day = day,
			week = week,
			month = month,
			year = year,
			minute = RealWorldTimeLight.time.currentMinute
		};
	}

	public void loadDateFromSave(DateSave loadFrom)
	{
		day = loadFrom.day;
		week = loadFrom.week;
		month = loadFrom.month;
		year = loadFrom.year;
		SeasonManager.manage.checkSeasonAndChangeMaterials();
	}

	private bool checkIfDropCanDrop(int xPos, int yPos, HouseDetails inside = null)
	{
		return true;
	}

	public bool tryAndStackItem(int itemId, int stack, int xPos, int yPos, HouseDetails inside)
	{
		if (Inventory.Instance.allItems[itemId].checkIfStackable() && inside == null)
		{
			List<DroppedItem> allDropsOnTile = getAllDropsOnTile(xPos, yPos);
			for (int i = 0; i < allDropsOnTile.Count; i++)
			{
				if (allDropsOnTile[i].myItemId == itemId)
				{
					DroppedItem droppedItem = allDropsOnTile[i];
					droppedItem.NetworkstackAmount = droppedItem.stackAmount + stack;
					return true;
				}
			}
		}
		return false;
	}

	public bool checkIfFishCanBeDropped(Vector3 positionToDrop)
	{
		if (waterMap[Mathf.RoundToInt(positionToDrop.x / 2f), Mathf.RoundToInt(positionToDrop.z / 2f)])
		{
			return true;
		}
		return false;
	}

	public bool IsFishPondInPos(Vector3 positionToDrop)
	{
		Vector2 vector = Instance.findMultiTileObjectPos(Mathf.RoundToInt(positionToDrop.x / 2f), Mathf.RoundToInt(positionToDrop.z / 2f));
		if (onTileMap[(int)vector.x, (int)vector.y] >= 0 && (bool)allObjects[onTileMap[(int)vector.x, (int)vector.y]].tileObjectChest)
		{
			return allObjects[onTileMap[(int)vector.x, (int)vector.y]].tileObjectChest.isFishPond;
		}
		return false;
	}

	public bool IsBugTerrariumInPos(Vector3 positionToDrop)
	{
		Vector2 vector = Instance.findMultiTileObjectPos(Mathf.RoundToInt(positionToDrop.x / 2f), Mathf.RoundToInt(positionToDrop.z / 2f));
		if (onTileMap[(int)vector.x, (int)vector.y] >= 0 && (bool)allObjects[onTileMap[(int)vector.x, (int)vector.y]].tileObjectChest)
		{
			return allObjects[onTileMap[(int)vector.x, (int)vector.y]].tileObjectChest.isBugTerrarium;
		}
		return false;
	}

	public bool checkIfDropCanFitOnGround(int itemId, int stackAmount, Vector3 positionToDrop, HouseDetails inside)
	{
		if (!WeatherManager.Instance.IsMyPlayerInside || (WeatherManager.Instance.IsMyPlayerInside && NetworkMapSharer.Instance.localChar.myInteract.IsInsidePlayerHouse))
		{
			if (Inventory.Instance.allItems[itemId].isDeed)
			{
				return false;
			}
			if ((bool)Inventory.Instance.allItems[itemId].fish && !checkIfFishCanBeDropped(positionToDrop))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public List<DroppedItem> getAllDropsOnTile(int xPos, int yPos)
	{
		Vector2 other = new Vector2(xPos, yPos);
		List<DroppedItem> list = new List<DroppedItem>();
		for (int i = 0; i < itemsOnGround.Count; i++)
		{
			if (itemsOnGround[i].IsDropOnCurrentLevel() && itemsOnGround[i].inside == null && itemsOnGround[i].onTile.Equals(other))
			{
				list.Add(itemsOnGround[i]);
			}
		}
		return list;
	}

	public List<DroppedItem> getDropsToSave()
	{
		List<DroppedItem> list = new List<DroppedItem>();
		for (int i = 0; i < itemsOnGround.Count; i++)
		{
			if (itemsOnGround[i] != null && itemsOnGround[i].IsDropOnCurrentLevel() && itemsOnGround[i].saveDrop)
			{
				list.Add(itemsOnGround[i]);
			}
		}
		return list;
	}

	public bool checkIfDropIsTooCloseToEachOther(Vector3 positionToCheck)
	{
		for (int i = 0; i < itemsOnGround.Count; i++)
		{
			if (Vector3.Distance(itemsOnGround[i].transform.position, positionToCheck) < 0.2f)
			{
				return true;
			}
		}
		return false;
	}

	public void updateDropsOnTileHeight(int xPos, int yPos)
	{
		List<DroppedItem> allDropsOnTile = getAllDropsOnTile(xPos, yPos);
		for (int i = 0; i < allDropsOnTile.Count; i++)
		{
			allDropsOnTile[i].NetworkdesiredPos = new Vector3(allDropsOnTile[i].desiredPos.x, heightMap[xPos, yPos], allDropsOnTile[i].desiredPos.z);
		}
	}

	public bool isChecking()
	{
		return true;
	}

	public Vector3 getClosestTileToDropPos(Vector3 startingPos)
	{
		float num = 100f;
		Vector3 result = Vector3.zero;
		for (int i = -8; i <= 8; i++)
		{
			for (int j = -8; j <= 8; j++)
			{
				if (spaceCanBeDroppedOn(startingPos + new Vector3(j * 2, 0f, i * 2)))
				{
					Vector3 b = startingPos + new Vector3(j * 2, 0f, i * 2);
					b.y = heightMap[(int)b.x / 2, (int)b.z / 2];
					float num2 = Vector3.Distance(startingPos, b);
					if (num2 < num || (num2 == num && UnityEngine.Random.Range(0, 4) == 2))
					{
						num = num2;
						result = startingPos + new Vector3(j * 2, 0f, i * 2);
					}
				}
			}
		}
		return result;
	}

	public Vector3 moveDropPosToSafeOutside(Vector3 pos, bool useNavMesh = true)
	{
		if (!isPositionOnMap((int)pos.x / 2, (int)pos.z / 2))
		{
			pos.y = -2f;
			return pos;
		}
		bool flag = false;
		if (spaceCanBeDroppedOn(pos))
		{
			flag = true;
		}
		else
		{
			if (useNavMesh && NetworkNavMesh.nav.checkIfPlaceOnNavMeshForDrop(pos) != Vector3.zero)
			{
				return NetworkNavMesh.nav.checkIfPlaceOnNavMeshForDrop(pos);
			}
			Vector3 closestTileToDropPos = getClosestTileToDropPos(pos);
			if (closestTileToDropPos != Vector3.zero)
			{
				pos = closestTileToDropPos;
				flag = true;
			}
			else if (spaceCanBeDroppedOn(pos + new Vector3(-2f, 0f, 0f)))
			{
				pos.x -= 2f;
				flag = true;
			}
			else if (spaceCanBeDroppedOn(pos + new Vector3(2f, 0f, 0f)))
			{
				pos.x += 2f;
				flag = true;
			}
			else if (spaceCanBeDroppedOn(pos + new Vector3(0f, 0f, -2f)))
			{
				pos.z -= 2f;
				flag = true;
			}
			else if (spaceCanBeDroppedOn(pos + new Vector3(0f, 0f, 2f)))
			{
				pos.z += 2f;
				flag = true;
			}
			else
			{
				Vector3 vector = NetworkNavMesh.nav.checkIfPlaceOnNavMeshForDrop(pos);
				if (vector != Vector3.zero)
				{
					return vector;
				}
			}
		}
		int num = 300;
		int num2 = (int)pos.x;
		int num3 = (int)pos.z;
		while (!flag)
		{
			num--;
			if (num <= 0)
			{
				pos.x += num2;
				pos.z += num3;
				flag = true;
				break;
			}
			if (spaceCanBeDroppedOn(pos))
			{
				flag = true;
				continue;
			}
			pos.x += UnityEngine.Random.Range(-2, 2);
			pos.z += UnityEngine.Random.Range(-2, 2);
		}
		if (isPositionOnMap(pos))
		{
			if (onTileMap[Mathf.RoundToInt(pos.x) / 2, Mathf.RoundToInt(pos.z) / 2] == 15)
			{
				pos.y = onTileStatusMap[Mathf.RoundToInt(pos.x) / 2, Mathf.RoundToInt(pos.z) / 2];
			}
			else
			{
				pos.y = heightMap[Mathf.RoundToInt(pos.x) / 2, Mathf.RoundToInt(pos.z) / 2];
			}
		}
		return pos;
	}

	public Vector3 moveDropPosToSafeInside(Vector3 pos, HouseDetails inside, DisplayPlayerHouseTiles display)
	{
		bool flag = false;
		if (spaceCanBeDroppedInside(pos, inside, display))
		{
			flag = true;
		}
		else if (spaceCanBeDroppedInside(pos + new Vector3(-2f, 0f, 0f), inside, display))
		{
			pos.x -= 2f;
			flag = true;
		}
		else if (spaceCanBeDroppedInside(pos + new Vector3(2f, 0f, 0f), inside, display))
		{
			pos.x += 2f;
			flag = true;
		}
		else if (spaceCanBeDroppedInside(pos + new Vector3(0f, 0f, -2f), inside, display))
		{
			pos.z -= 2f;
			flag = true;
		}
		else if (spaceCanBeDroppedInside(pos + new Vector3(0f, 0f, 2f), inside, display))
		{
			pos.z += 2f;
			flag = true;
		}
		int num = 200;
		while (!flag)
		{
			num--;
			if (spaceCanBeDroppedInside(pos, inside, display) || num <= 0)
			{
				flag = true;
				continue;
			}
			pos.x += UnityEngine.Random.Range(-1, 2) * 2;
			pos.x = Mathf.Clamp(pos.x, display.getStartingPosTransform().position.x, display.getStartingPosTransform().position.x + 50f);
			pos.z += UnityEngine.Random.Range(-1, 2) * 2;
			pos.z = Mathf.Clamp(pos.z, display.getStartingPosTransform().position.z, display.getStartingPosTransform().position.z + 50f);
		}
		return pos;
	}

	private bool spaceCanBeDroppedOn(Vector3 pos)
	{
		if (isPositionOnMap(Mathf.RoundToInt(pos.x) / 2, Mathf.RoundToInt(pos.z) / 2) && (onTileMap[Mathf.RoundToInt(pos.x) / 2, Mathf.RoundToInt(pos.z) / 2] == -1 || (onTileMap[Mathf.RoundToInt(pos.x) / 2, Mathf.RoundToInt(pos.z) / 2] > -1 && allObjectSettings[onTileMap[Mathf.RoundToInt(pos.x) / 2, Mathf.RoundToInt(pos.z) / 2]].walkable)))
		{
			return true;
		}
		return false;
	}

	private bool spaceCanBeDroppedInside(Vector3 pos, HouseDetails details, DisplayPlayerHouseTiles inside)
	{
		int num = Mathf.RoundToInt((pos.x - inside.getStartingPosTransform().position.x) / 2f);
		int num2 = Mathf.RoundToInt((pos.z - inside.getStartingPosTransform().position.z) / 2f);
		if (checkIfOnMap(num, inside: true) && checkIfOnMap(num2, inside: true) && (details.houseMapOnTile[num, num2] == -1 || (details.houseMapOnTile[num, num2] > -1 && allObjectSettings[details.houseMapOnTile[num, num2]].walkable)))
		{
			return true;
		}
		return false;
	}

	public GameObject dropAnItem(int itemId, int stackAmount, Vector3 positionToDrop, HouseDetails inside, bool tryNotToStack)
	{
		if (!tryNotToStack && tryAndStackItem(itemId, stackAmount, Mathf.RoundToInt(positionToDrop.x / 2f), Mathf.RoundToInt(positionToDrop.z / 2f), inside))
		{
			return null;
		}
		if (inside == null)
		{
			GameObject obj = UnityEngine.Object.Instantiate(droppedItemPrefab, positionToDrop, Quaternion.identity);
			DroppedItem component = obj.GetComponent<DroppedItem>();
			positionToDrop = moveDropPosToSafeOutside(positionToDrop);
			component.setDesiredPos(positionToDrop.y, positionToDrop.x, positionToDrop.z);
			component.NetworkstackAmount = stackAmount;
			component.NetworkmyItemId = itemId;
			itemsOnGround.Add(component);
			return obj;
		}
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(inside.xPos, inside.yPos);
		positionToDrop = moveDropPosToSafeInside(positionToDrop, inside, displayPlayerHouseTiles);
		positionToDrop.y = displayPlayerHouseTiles.getStartingPosTransform().position.y;
		GameObject obj2 = UnityEngine.Object.Instantiate(droppedItemPrefab, positionToDrop, Quaternion.identity);
		DroppedItem component2 = obj2.GetComponent<DroppedItem>();
		component2.inside = inside;
		component2.setDesiredPos(displayPlayerHouseTiles.getStartingPosTransform().position.y, positionToDrop.x, positionToDrop.z);
		component2.NetworkstackAmount = stackAmount;
		component2.NetworkmyItemId = itemId;
		itemsOnGround.Add(component2);
		return obj2;
	}

	public void getFreeChunkAndSetInPos(int xPos, int yPos)
	{
		for (int i = 0; i < chunksInUse.Count; i++)
		{
			if (!chunksInUse[i].isActiveAndEnabled)
			{
				chunksInUse[i]._transform.position = Vector3.zero;
				chunksInUse[i].setChunkAndRefresh(xPos, yPos);
				return;
			}
		}
		Chunk component = UnityEngine.Object.Instantiate(ChunkPrefab).GetComponent<Chunk>();
		component.transform.position = Vector3.zero;
		component.setChunkAndRefresh(xPos, yPos);
		chunksInUse.Add(component);
	}

	public void giveBackChunk(Chunk giveBackChunk)
	{
		giveBackChunk.returnAllTileObjects();
		giveBackChunk.gameObject.SetActive(value: false);
	}

	public void refreshAllChunksForSwitch(Vector3 mineEntranceExitPos)
	{
		chunkRefreshCompleted = false;
		StartCoroutine(refreshChunkDelay(mineEntranceExitPos));
	}

	private IEnumerator refreshChunkDelay(Vector3 mineEntranceExitPos)
	{
		Chunk mineEntranceOrExitChunk = null;
		Chunk mineEntranceLeft = null;
		Chunk mineEntranceRight = null;
		Chunk mineEntranceUp = null;
		Chunk mineEntranceDown = null;
		int entranceChunkX = (int)(Mathf.Round(mineEntranceExitPos.x) / 2f) / chunkSize * chunkSize;
		int entranceChunkY = (int)(Mathf.Round(mineEntranceExitPos.z) / 2f) / chunkSize * chunkSize;
		for (int j = 0; j < chunksInUse.Count; j++)
		{
			if (chunksInUse[j].gameObject.activeInHierarchy)
			{
				if (chunksInUse[j].showingChunkX == entranceChunkX && chunksInUse[j].showingChunkY == entranceChunkY)
				{
					chunksInUse[j].setChunkAndRefresh(chunksInUse[j].showingChunkX, chunksInUse[j].showingChunkY, fullRefresh: true);
					mineEntranceOrExitChunk = chunksInUse[j];
				}
				else if (chunksInUse[j].showingChunkX == entranceChunkX + 10 && chunksInUse[j].showingChunkY == entranceChunkY)
				{
					chunksInUse[j].setChunkAndRefresh(chunksInUse[j].showingChunkX, chunksInUse[j].showingChunkY, fullRefresh: true);
					mineEntranceRight = chunksInUse[j];
					yield return null;
				}
				else if (chunksInUse[j].showingChunkX == entranceChunkX - 10 && chunksInUse[j].showingChunkY == entranceChunkY)
				{
					chunksInUse[j].setChunkAndRefresh(chunksInUse[j].showingChunkX, chunksInUse[j].showingChunkY, fullRefresh: true);
					mineEntranceLeft = chunksInUse[j];
					yield return null;
				}
				else if (chunksInUse[j].showingChunkX == entranceChunkX && chunksInUse[j].showingChunkY == entranceChunkY - 10)
				{
					chunksInUse[j].setChunkAndRefresh(chunksInUse[j].showingChunkX, chunksInUse[j].showingChunkY, fullRefresh: true);
					mineEntranceDown = chunksInUse[j];
					yield return null;
				}
				else if (chunksInUse[j].showingChunkX == entranceChunkX && chunksInUse[j].showingChunkY == entranceChunkY + 10)
				{
					chunksInUse[j].setChunkAndRefresh(chunksInUse[j].showingChunkX, chunksInUse[j].showingChunkY, fullRefresh: true);
					mineEntranceUp = chunksInUse[j];
					yield return null;
				}
			}
		}
		int chunkCounter = 0;
		for (int j = 0; j < chunksInUse.Count; j++)
		{
			if (chunksInUse[j].gameObject.activeInHierarchy && chunksInUse[j] != mineEntranceOrExitChunk && chunksInUse[j] != mineEntranceUp && chunksInUse[j] != mineEntranceDown && chunksInUse[j] != mineEntranceLeft && chunksInUse[j] != mineEntranceRight)
			{
				chunksInUse[j].setChunkAndRefresh(chunksInUse[j].showingChunkX, chunksInUse[j].showingChunkY, fullRefresh: true);
				chunkCounter++;
				if (chunkCounter >= 4)
				{
					chunkCounter = 0;
					yield return null;
				}
			}
		}
		chunkRefreshCompleted = true;
	}

	public void refreshAllChunksForConnect()
	{
		for (int i = 0; i < chunksInUse.Count; i++)
		{
			if (chunksInUse[i].gameObject.activeInHierarchy)
			{
				giveBackChunk(chunksInUse[i]);
			}
		}
		NewChunkLoader.loader.resetChunksViewing();
	}

	public IEnumerator refreshAllChunksNewDay()
	{
		for (int i = 0; i < chunksInUse.Count; i++)
		{
			if (chunksInUse[i].gameObject.activeInHierarchy && chunkChangedMap[chunksInUse[i].showingChunkX / 10, chunksInUse[i].showingChunkY / 10] && chunkHasChangedToday[chunksInUse[i].showingChunkX / 10, chunksInUse[i].showingChunkY / 10])
			{
				chunksInUse[i].refreshChunk();
				yield return null;
			}
		}
	}

	public bool clientHasRequestedChunk(int changeXPos, int changeYPos)
	{
		changeXPos = Mathf.RoundToInt(changeXPos / chunkSize) * chunkSize;
		changeYPos = Mathf.RoundToInt(changeYPos / chunkSize) * chunkSize;
		return clientRequestedMap[changeXPos / chunkSize, changeYPos / chunkSize];
	}

	public void waterChunkHasChanged(int changeXPos, int changeYPos)
	{
		changeXPos = Mathf.RoundToInt(changeXPos / chunkSize) * chunkSize;
		changeYPos = Mathf.RoundToInt(changeYPos / chunkSize) * chunkSize;
		changedMapWater[changeXPos / chunkSize, changeYPos / chunkSize] = true;
		chunkChangedMap[changeXPos / chunkSize, changeYPos / chunkSize] = true;
	}

	public void setChunkHasChangedToday(int changeXPos, int changeYPos)
	{
		changeXPos = Mathf.RoundToInt(changeXPos / chunkSize) * chunkSize;
		changeYPos = Mathf.RoundToInt(changeYPos / chunkSize) * chunkSize;
		chunkHasChangedToday[changeXPos / chunkSize, changeYPos / chunkSize] = true;
	}

	public void heightChunkHasChanged(int changeXPos, int changeYPos)
	{
		changeXPos = Mathf.RoundToInt(changeXPos / chunkSize) * chunkSize;
		changeYPos = Mathf.RoundToInt(changeYPos / chunkSize) * chunkSize;
		changedMapHeight[changeXPos / chunkSize, changeYPos / chunkSize] = true;
		chunkChangedMap[changeXPos / chunkSize, changeYPos / chunkSize] = true;
	}

	public void onTileChunkHasChanged(int changeXPos, int changeYPos)
	{
		changeXPos = Mathf.RoundToInt(changeXPos / chunkSize) * chunkSize;
		changeYPos = Mathf.RoundToInt(changeYPos / chunkSize) * chunkSize;
		changedMapOnTile[changeXPos / chunkSize, changeYPos / chunkSize] = true;
		chunkChangedMap[changeXPos / chunkSize, changeYPos / chunkSize] = true;
	}

	public void tileTypeChunkHasChanged(int changeXPos, int changeYPos)
	{
		changeXPos = Mathf.RoundToInt(changeXPos / chunkSize) * chunkSize;
		changeYPos = Mathf.RoundToInt(changeYPos / chunkSize) * chunkSize;
		changedMapTileType[changeXPos / chunkSize, changeYPos / chunkSize] = true;
		chunkChangedMap[changeXPos / chunkSize, changeYPos / chunkSize] = true;
	}

	public void placeFenceInChunk(int changeXPos, int changeYPos)
	{
		if (onTileMap[changeXPos, changeYPos] >= -1 && (onTileMap[changeXPos, changeYPos] <= -1 || allObjectSettings[onTileMap[changeXPos, changeYPos]].walkable))
		{
			return;
		}
		if (onTileMap[changeXPos, changeYPos] > -1 && allObjectSettings[onTileMap[changeXPos, changeYPos]].isSpecialFencedObject)
		{
			if (fencedOffMap[changeXPos, changeYPos] <= 1)
			{
				fencedOffMap[changeXPos, changeYPos] = 2;
			}
		}
		else
		{
			fencedOffMap[changeXPos, changeYPos] = 1;
		}
		changeXPos = Mathf.RoundToInt(changeXPos / chunkSize) * chunkSize;
		changeYPos = Mathf.RoundToInt(changeYPos / chunkSize) * chunkSize;
		chunkWithFenceInIt[changeXPos / chunkSize, changeYPos / chunkSize] = true;
	}

	public void refreshAllChunksInUse(int changeXPos, int changeYPos, bool networkRefresh = false)
	{
		changeXPos = Mathf.RoundToInt(changeXPos / chunkSize) * chunkSize;
		changeYPos = Mathf.RoundToInt(changeYPos / chunkSize) * chunkSize;
		for (int i = 0; i < chunksInUse.Count; i++)
		{
			if (chunksInUse[i].gameObject.activeInHierarchy)
			{
				if (chunksInUse[i].showingChunkY == changeYPos && chunksInUse[i].showingChunkX == changeXPos)
				{
					chunksInUse[i].refreshChunk();
				}
				if ((chunksInUse[i].showingChunkX == changeXPos || chunksInUse[i].showingChunkX == changeXPos + chunkSize || chunksInUse[i].showingChunkX == changeXPos - chunkSize) && (chunksInUse[i].showingChunkY == changeYPos || chunksInUse[i].showingChunkY == changeYPos + chunkSize || chunksInUse[i].showingChunkY == changeYPos - chunkSize))
				{
					chunksInUse[i].refreshChunk(fullRefresh: false);
				}
			}
		}
	}

	public void refreshTileObjectsOnChunksInUse(int changeXPos, int changeYPos, bool networkRefresh = false)
	{
		changeXPos = Mathf.RoundToInt(changeXPos / chunkSize) * chunkSize;
		changeYPos = Mathf.RoundToInt(changeYPos / chunkSize) * chunkSize;
		for (int i = 0; i < chunksInUse.Count; i++)
		{
			if (chunksInUse[i].gameObject.activeInHierarchy)
			{
				if (chunksInUse[i].showingChunkY == changeYPos && chunksInUse[i].showingChunkX == changeXPos)
				{
					chunksInUse[i].refreshChunksOnTileObjects();
				}
				if ((chunksInUse[i].showingChunkX == changeXPos || chunksInUse[i].showingChunkX == changeXPos + chunkSize || chunksInUse[i].showingChunkX == changeXPos - chunkSize) && (chunksInUse[i].showingChunkY == changeYPos || chunksInUse[i].showingChunkY == changeYPos + chunkSize || chunksInUse[i].showingChunkY == changeYPos - chunkSize))
				{
					chunksInUse[i].refreshChunksOnTileObjects(neighbourCheck: true);
				}
			}
		}
	}

	public int[] getChunkDetails(int chunkX, int chunkY, MapType fromMap)
	{
		int[] array = new int[chunkSize * chunkSize];
		switch (fromMap)
		{
		case MapType.OnTileMap:
		{
			for (int m = 0; m < chunkSize; m++)
			{
				for (int n = 0; n < chunkSize; n++)
				{
					array[m * chunkSize + n] = onTileMap[chunkX + n, chunkY + m];
				}
			}
			return array;
		}
		case MapType.TileTypeMap:
		{
			for (int k = 0; k < chunkSize; k++)
			{
				for (int l = 0; l < chunkSize; l++)
				{
					array[k * chunkSize + l] = tileTypeMap[chunkX + l, chunkY + k];
				}
			}
			return array;
		}
		case MapType.HeightMap:
		{
			for (int i = 0; i < chunkSize; i++)
			{
				for (int j = 0; j < chunkSize; j++)
				{
					array[i * chunkSize + j] = heightMap[chunkX + j, chunkY + i];
				}
			}
			return array;
		}
		default:
			return null;
		}
	}

	public bool chunkHasItemsOnTop(int chunkX, int chunkY)
	{
		for (int i = 0; i < chunkSize; i++)
		{
			for (int j = 0; j < chunkSize; j++)
			{
				if (onTileMap[chunkX + j, chunkY + i] > -1 && allObjects[onTileMap[chunkX + j, chunkY + i]].canBePlaceOn() && ItemOnTopManager.manage.hasItemsOnTop(chunkX + j, chunkY + i))
				{
					return true;
				}
			}
		}
		return false;
	}

	public ItemOnTop[] getItemsOnTopInChunk(int chunkX, int chunkY)
	{
		List<ItemOnTop> list = new List<ItemOnTop>();
		for (int i = 0; i < chunkSize; i++)
		{
			for (int j = 0; j < chunkSize; j++)
			{
				if (onTileMap[chunkX + j, chunkY + i] > -1 && allObjects[onTileMap[chunkX + j, chunkY + i]].canBePlaceOn() && ItemOnTopManager.manage.hasItemsOnTop(chunkX + j, chunkY + i))
				{
					ItemOnTop[] allItemsOnTop = ItemOnTopManager.manage.getAllItemsOnTop(chunkX + j, chunkY + i, null);
					for (int k = 0; k < allItemsOnTop.Length; k++)
					{
						list.Add(allItemsOnTop[k]);
					}
				}
			}
		}
		return list.ToArray();
	}

	public int[] getChunkStatusDetails(int chunkX, int chunkY)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < chunkSize; i++)
		{
			for (int j = 0; j < chunkSize; j++)
			{
				if (Instance.onTileMap[chunkX + j, chunkY + i] > -1)
				{
					if (allObjectSettings[Instance.onTileMap[chunkX + j, chunkY + i]].getRotationFromMap || allObjectSettings[Instance.onTileMap[chunkX + j, chunkY + i]].isMultiTileObject)
					{
						list.Add(Instance.rotationMap[chunkX + j, chunkY + i]);
					}
					if (allObjects[Instance.onTileMap[chunkX + j, chunkY + i]].hasExtensions)
					{
						list.Add(Instance.onTileStatusMap[chunkX + j, chunkY + i]);
					}
				}
			}
		}
		int[] array = new int[list.Count];
		for (int k = 0; k < list.Count; k++)
		{
			array[k] = list[k];
		}
		return array;
	}

	public bool[] getWaterChunkDetails(int chunkX, int chunkY)
	{
		bool[] array = new bool[chunkSize * chunkSize];
		for (int i = 0; i < chunkSize; i++)
		{
			for (int j = 0; j < chunkSize; j++)
			{
				array[i * chunkSize + j] = Instance.waterMap[chunkX + j, chunkY + i];
			}
		}
		return array;
	}

	public int[] getHouseDetailsArray(int[,] requestedMap)
	{
		int[] array = new int[625];
		for (int i = 0; i < 25; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				array[i * 25 + j] = requestedMap[j, i];
			}
		}
		return array;
	}

	public int[,] fillHouseDetailsArray(int[] convertMap)
	{
		int[,] array = new int[25, 25];
		for (int i = 0; i < 25; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				array[j, i] = convertMap[i * 25 + j];
			}
		}
		return array;
	}

	public void fillOnTileChunkDetails(int chunkX, int chunkY, int[] onTileDetails, int[] otherDetails)
	{
		for (int i = 0; i < chunkSize; i++)
		{
			for (int j = 0; j < chunkSize; j++)
			{
				Instance.onTileMap[chunkX + j, chunkY + i] = onTileDetails[i * chunkSize + j];
			}
		}
		bool flag = false;
		int num = 0;
		for (int k = 0; k < chunkSize; k++)
		{
			for (int l = 0; l < chunkSize; l++)
			{
				if (Instance.onTileMap[chunkX + l, chunkY + k] > -1)
				{
					if (!flag && allObjectSettings[Instance.onTileMap[chunkX + l, chunkY + k]].canBePlacedOn())
					{
						flag = true;
					}
					if (allObjectSettings[Instance.onTileMap[chunkX + l, chunkY + k]].getRotationFromMap || allObjectSettings[Instance.onTileMap[chunkX + l, chunkY + k]].isMultiTileObject)
					{
						Instance.rotationMap[chunkX + l, chunkY + k] = otherDetails[num];
						num++;
					}
					if (allObjects[Instance.onTileMap[chunkX + l, chunkY + k]].hasExtensions)
					{
						Instance.onTileStatusMap[chunkX + l, chunkY + k] = otherDetails[num];
						num++;
					}
				}
			}
		}
		if (flag)
		{
			NetworkMapSharer.Instance.localChar.CmdRequestItemOnTopForChunk(chunkX, chunkY);
		}
	}

	public void fillTileTypeChunkDetails(int chunkX, int chunkY, int[] tileTypeDetails)
	{
		for (int i = 0; i < chunkSize; i++)
		{
			for (int j = 0; j < chunkSize; j++)
			{
				Instance.tileTypeMap[chunkX + j, chunkY + i] = tileTypeDetails[i * chunkSize + j];
			}
		}
	}

	public void fillWaterChunkDetails(int chunkX, int chunkY, bool[] waterDetails)
	{
		for (int i = 0; i < chunkSize; i++)
		{
			for (int j = 0; j < chunkSize; j++)
			{
				Instance.waterMap[chunkX + j, chunkY + i] = waterDetails[i * chunkSize + j];
			}
		}
	}

	public void fillHeightChunkDetails(int chunkX, int chunkY, int[] heightTileDetails)
	{
		for (int i = 0; i < chunkSize; i++)
		{
			for (int j = 0; j < chunkSize; j++)
			{
				Instance.heightMap[chunkX + j, chunkY + i] = heightTileDetails[i * chunkSize + j];
			}
		}
	}

	public bool doesPositionNeedsChunk(int changeXPos, int changeYPos)
	{
		for (int i = 0; i < chunksInUse.Count; i++)
		{
			if (chunksInUse[i].isActiveAndEnabled && chunksInUse[i].showingChunkY == changeYPos && chunksInUse[i].showingChunkX == changeXPos)
			{
				return false;
			}
		}
		return true;
	}

	public void getNoOfWaterTilesClose(int changeXPos, int changeYPos)
	{
		NewChunkLoader.loader.oceanTilesNearChar = 0;
		NewChunkLoader.loader.waterTilesNearChar = 0;
		for (int i = 0; i < chunksInUse.Count; i++)
		{
			if (chunksInUse[i].gameObject.activeInHierarchy)
			{
				if (chunksInUse[i].showingChunkX == changeXPos && chunksInUse[i].showingChunkY == changeYPos)
				{
					NewChunkLoader.loader.riverTilesInCharChunk = chunksInUse[i].waterTilesOnChunk;
				}
				int num = chunkSize + chunkSize;
				if (chunksInUse[i].showingChunkY < changeYPos + num && chunksInUse[i].showingChunkY > changeYPos - num && chunksInUse[i].showingChunkX < changeXPos + num && chunksInUse[i].showingChunkX > changeXPos - num)
				{
					NewChunkLoader.loader.waterTilesNearChar += chunksInUse[i].waterTilesOnChunk;
				}
				int num2 = 2 * chunkSize + chunkSize;
				if (chunksInUse[i].showingChunkY < changeYPos + num2 && chunksInUse[i].showingChunkY > changeYPos - num2 && chunksInUse[i].showingChunkX < changeXPos + num2 && chunksInUse[i].showingChunkX > changeXPos - num2)
				{
					NewChunkLoader.loader.oceanTilesNearChar += chunksInUse[i].oceanTilesOnChunk;
				}
			}
		}
	}

	public void returnChunksNotCloseEnough(int changeXPos, int changeYPos, int amountOfChunksCloseToChar)
	{
		for (int i = 0; i < chunksInUse.Count; i++)
		{
			if (chunksInUse[i].isActiveAndEnabled)
			{
				int num = amountOfChunksCloseToChar * chunkSize + chunkSize;
				if (chunksInUse[i].showingChunkY >= changeYPos + num || chunksInUse[i].showingChunkY <= changeYPos - num || chunksInUse[i].showingChunkX >= changeXPos + num || chunksInUse[i].showingChunkX <= changeXPos - num)
				{
					giveBackChunk(chunksInUse[i]);
				}
			}
		}
	}

	public TileObject findTileObjectInUse(int xPos, int yPos)
	{
		if ((bool)netMapSharer && netMapSharer.isActiveAndEnabled && !netMapSharer.isServer && !clientHasRequestedChunk(xPos, yPos))
		{
			return null;
		}
		TileObject result = null;
		if (onTileMap[xPos, yPos] == -1 || onTileMap[xPos, yPos] == 30)
		{
			return result;
		}
		int num = Mathf.RoundToInt(xPos / chunkSize) * chunkSize;
		int num2 = Mathf.RoundToInt(yPos / chunkSize) * chunkSize;
		foreach (Chunk item in chunksInUse)
		{
			if (item.gameObject.activeInHierarchy && item.showingChunkX == num && item.showingChunkY == num2)
			{
				int num3 = xPos - num;
				int num4 = yPos - num2;
				result = item.chunksTiles[num3, num4].onThisTile;
			}
		}
		return result;
	}

	public TileObject getObjectFromAllObjectsSorted(int objectId)
	{
		int count = allObjectsSorted[objectId].Count;
		for (int i = 0; i < count; i++)
		{
			if (!allObjectsSorted[objectId][i].active)
			{
				return allObjectsSorted[objectId][i];
			}
		}
		return null;
	}

	public void addObjectToAllObjectsSorted(TileObject toAdd)
	{
		allObjectsSorted[toAdd.tileObjectId].Add(toAdd);
	}

	public TileObject getTileObject(int desiredObject, int xPos, int yPos)
	{
		TileObject tileObject = getObjectFromAllObjectsSorted(desiredObject);
		if (tileObject == null)
		{
			tileObject = UnityEngine.Object.Instantiate(allObjects[desiredObject].gameObject, new Vector3(xPos * 2, Instance.heightMap[xPos, yPos], yPos * 2), Quaternion.identity).GetComponent<TileObject>();
			tileObject._transform = tileObject.transform;
			tileObject._gameObject = tileObject.gameObject;
			tileObject.currentHealth = allObjectSettings[desiredObject].fullHealth;
			addObjectToAllObjectsSorted(tileObject);
		}
		tileObject._transform.localPosition = new Vector3(xPos * 2, Instance.heightMap[xPos, yPos], yPos * 2);
		tileObject.getRotation(xPos, yPos);
		if (!tileObject.active)
		{
			tileObject.active = true;
			tileObject._gameObject.SetActive(value: true);
		}
		return tileObject;
	}

	private IEnumerator delayActivateObject(GameObject dObject)
	{
		yield return null;
		if (UnityEngine.Random.Range(0, 2) == 1)
		{
			yield return null;
		}
		dObject.SetActive(value: true);
	}

	public TileObject getTileObjectForHouse(int desiredObject, Vector3 moveTo, int xPos, int yPos, HouseDetails thisHouse)
	{
		TileObject tileObject = getObjectFromAllObjectsSorted(desiredObject);
		if (tileObject == null)
		{
			tileObject = UnityEngine.Object.Instantiate(allObjects[desiredObject].gameObject, moveTo, Quaternion.identity).GetComponent<TileObject>();
			tileObject._transform = tileObject.transform;
			tileObject._gameObject = tileObject.gameObject;
			addObjectToAllObjectsSorted(tileObject);
		}
		tileObject._transform.localPosition = moveTo;
		tileObject.getRotationInside(xPos, yPos, thisHouse);
		if (!tileObject.active)
		{
			tileObject.active = true;
			tileObject._gameObject.SetActive(value: true);
		}
		return tileObject;
	}

	public TileObject getTileObjectForOnTop(int desiredObject, Vector3 pos)
	{
		TileObject component = UnityEngine.Object.Instantiate(allObjects[desiredObject].gameObject, pos, Quaternion.identity).GetComponent<TileObject>();
		component._transform = component.transform;
		component._gameObject = component.gameObject;
		return component;
	}

	public TileObject getTileObjectForServerDrop(int desiredObject, Vector3 position)
	{
		TileObject tileObject = getObjectFromAllObjectsSorted(desiredObject);
		if (tileObject == null)
		{
			tileObject = UnityEngine.Object.Instantiate(allObjects[desiredObject].gameObject, position, Quaternion.identity).GetComponent<TileObject>();
			tileObject._transform = tileObject.transform;
			tileObject._gameObject = tileObject.gameObject;
			addObjectToAllObjectsSorted(tileObject);
		}
		tileObject._transform.localPosition = position;
		tileObject.getRotation((int)position.x / 2, (int)position.z / 2);
		UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
		return tileObject;
	}

	public TileObject getTileObjectForShopInterior(int desiredObject, Vector3 position)
	{
		TileObject component = UnityEngine.Object.Instantiate(allObjects[desiredObject].gameObject, position, Quaternion.identity).GetComponent<TileObject>();
		UnityEngine.Object.Destroy(component.GetComponentInChildren<MineEnterExit>());
		component._transform = component.transform;
		component._transform.localPosition = position;
		component.getRotation((int)position.x / 2, (int)position.z / 2);
		return component;
	}

	public void returnTileObject(TileObject returnedObject)
	{
		returnedObject.currentHealth = allObjectSettings[returnedObject.tileObjectId].fullHealth;
		returnedObject.active = false;
		returnedObject.gameObject.SetActive(value: false);
	}

	public void destroyTileObject(TileObject returnedObject)
	{
	}

	public void nextDay()
	{
		GenerateUndergroundMap.generate.generateMineSeedForNewDay();
		NetworkMapSharer.Instance.NetworkcraftsmanWorking = false;
		NetworkMapSharer.Instance.syncLicenceLevels();
		NetworkMapSharer.Instance.NetworknextDayIsReady = false;
		CatchingCompetitionManager.manage.FinishComp();
		updateAllChangers();
		WeatherManager.Instance.CreateNewWeatherPatterns();
		if (WeatherManager.Instance.IsSnowDay)
		{
			WeatherManager.Instance.PlaceSnowBallsOnSnowDay();
		}
		else
		{
			WeatherManager.Instance.RemoveSnowBallsAndMenForNoSnowDay();
		}
		NetworkMapSharer.Instance.RpcAddADay(NetworkMapSharer.Instance.mineSeed);
		TuckshopManager.manage.setSpecialItem();
		FarmAnimalManager.manage.RemoveAllAnimalHousesNotOnMainIsland();
	}

	public void updateAllChangers()
	{
		for (int i = 0; i < allChangers.Count; i++)
		{
			if (allChangers[i].counterDays == 0)
			{
				if (RealWorldTimeLight.time.currentHour == 0)
				{
					allChangers[i].counterSeconds -= 420;
				}
				else
				{
					allChangers[i].counterSeconds -= (24 - RealWorldTimeLight.time.currentHour) * 120 + 840;
				}
			}
			else
			{
				allChangers[i].counterDays--;
			}
		}
	}

	public void addToCropChecker()
	{
		completedCropChecker++;
	}

	public int getNoOfCompletedCrops()
	{
		return completedCropChecker;
	}

	public void WetTilledTilesWhenRainStarts()
	{
		StartCoroutine(WetTilesForRain(refreshLiveLoadedChunk: true));
	}

	private IEnumerator WetTilesForRain(bool refreshLiveLoadedChunk)
	{
		int chunkCounter = 0;
		for (int chunkY = 0; chunkY < mapSize / 10; chunkY++)
		{
			for (int chunkX = 0; chunkX < mapSize / 10; chunkX++)
			{
				if (!chunkChangedMap[chunkX, chunkY])
				{
					continue;
				}
				for (int i = chunkY * 10; i < chunkY * 10 + 10; i++)
				{
					for (int j = chunkX * 10; j < chunkX * 10 + 10; j++)
					{
						if (tileTypes[tileTypeMap[j, i]].wetVersion != -1)
						{
							tileTypeMap[j, i] = tileTypes[tileTypeMap[j, i]].wetVersion;
							chunkHasChangedToday[chunkX, chunkY] = true;
						}
					}
				}
				if (chunkCounter >= 10)
				{
					chunkCounter = 0;
					yield return null;
				}
				else
				{
					chunkCounter++;
				}
			}
		}
		if (refreshLiveLoadedChunk)
		{
			StartCoroutine(refreshAllChunksNewDay());
		}
	}

	public void doNextDayChange()
	{
		StartCoroutine(nextDayChanges(raining: false, UnityEngine.Random.Range(-200000, 200000)));
	}

	public IEnumerator nextDayChanges(bool raining, int mineSeed)
	{
		List<int[]> sprinklerPos = new List<int[]>();
		List<int[]> waterTankPos = new List<int[]>();
		if (WeatherManager.Instance.IsItRainingToday())
		{
			yield return StartCoroutine(WetTilesForRain(refreshLiveLoadedChunk: false));
		}
		chunkHasChangedToday = new bool[mapSize / 10, mapSize / 10];
		int grassType = 1;
		int tropicalGrassType = 4;
		int pineGrassType = 15;
		int chunkCounter = 0;
		completedCropChecker = 0;
		for (int chunkY = 0; chunkY < mapSize / 10; chunkY++)
		{
			for (int chunkX = 0; chunkX < mapSize / 10; chunkX++)
			{
				if (!chunkChangedMap[chunkX, chunkY])
				{
					continue;
				}
				UnityEngine.Random.InitState(mineSeed + chunkX * chunkY);
				for (int i = chunkY * 10; i < chunkY * 10 + 10; i++)
				{
					for (int j = chunkX * 10; j < chunkX * 10 + 10; j++)
					{
						if (onTileMap[j, i] >= -1)
						{
							if (onTileMap[j, i] == -1)
							{
								if (raining && tileTypeMap[j, i] == 5 && (!waterMap[j, i] || (waterMap[j, i] && heightMap[j, i] >= -1)))
								{
									GenerateMap.generate.desertRainGrowBack.getRandomObjectAndPlaceWithGrowth(j, i);
									if (onTileMap[j, i] != -1)
									{
										chunkHasChangedToday[chunkX, chunkY] = true;
									}
								}
								if (tileTypeMap[j, i] == 14 && (!waterMap[j, i] || (waterMap[j, i] && heightMap[j, i] >= -1)))
								{
									GenerateMap.generate.mangroveGrowback.getRandomObjectAndPlaceWithGrowth(j, i);
									if (onTileMap[j, i] != -1)
									{
										chunkHasChangedToday[chunkX, chunkY] = true;
									}
								}
								if (tileTypeMap[j, i] == 3 && GenerateMap.generate.checkBiomType(j, i) == 16)
								{
									GenerateMap.generate.beachGrowBack.getRandomObjectAndPlaceWithGrowth(j, i);
									if (onTileMap[j, i] != -1)
									{
										chunkHasChangedToday[chunkX, chunkY] = true;
									}
								}
								if (GenerateMap.generate.checkBiomType(j, i) == 12 && tileTypeMap[j, i] == 18 && checkAllNeighboursAreEmpty(j, i))
								{
									if (NetworkMapSharer.Instance.miningLevel == 2)
									{
										onTileMap[j, i] = GenerateMap.generate.quaryGrowBack1.getBiomObject();
									}
									else if (NetworkMapSharer.Instance.miningLevel == 3)
									{
										onTileMap[j, i] = GenerateMap.generate.quaryGrowBack2.getBiomObject();
									}
									else
									{
										onTileMap[j, i] = GenerateMap.generate.quaryGrowBack0.getBiomObject();
									}
									if (onTileMap[j, i] != -1)
									{
										chunkHasChangedToday[chunkX, chunkY] = true;
									}
								}
								if (tileTypeMap[j, i] == grassType || tileTypeMap[j, i] == tropicalGrassType || tileTypeMap[j, i] == pineGrassType)
								{
									if (tileTypeMap[j, i] == grassType)
									{
										GenerateMap.generate.bushLandGrowBack.getRandomObjectAndPlaceWithGrowth(j, i);
									}
									else if (tileTypeMap[j, i] == tropicalGrassType)
									{
										onTileMap[j, i] = GenerateMap.generate.tropicalGrowBack.getBiomObject();
									}
									else if (tileTypeMap[j, i] == pineGrassType)
									{
										onTileMap[j, i] = GenerateMap.generate.coldLandGrowBack.getBiomObject();
									}
									if (onTileMap[j, i] > -1)
									{
										if ((bool)allObjects[onTileMap[j, i]].tileObjectGrowthStages)
										{
											onTileStatusMap[j, i] = 0;
										}
										chunkHasChangedToday[chunkX, chunkY] = true;
									}
								}
							}
							else if (allObjectSettings[onTileMap[j, i]].isFlowerBed && onTileStatusMap[j, i] <= 0)
							{
								if (j != 0 && i != 0 && j < mapSize && i < mapSize)
								{
									switch (UnityEngine.Random.Range(0, 7))
									{
									case 0:
										if (onTileMap[j + 1, i] > -1 && allObjectSettings[onTileMap[j + 1, i]].isFlowerBed && onTileStatusMap[j + 1, i] > 0)
										{
											onTileStatusMap[j, i] = onTileStatusMap[j + 1, i];
											chunkHasChangedToday[chunkX, chunkY] = true;
										}
										break;
									case 1:
										if (onTileMap[j - 1, i] > -1 && allObjectSettings[onTileMap[j - 1, i]].isFlowerBed && onTileStatusMap[j - 1, i] > 0)
										{
											onTileStatusMap[j, i] = onTileStatusMap[j - 1, i];
											chunkHasChangedToday[chunkX, chunkY] = true;
										}
										break;
									case 2:
										if (onTileMap[j, i + 1] > -1 && allObjectSettings[onTileMap[j, i + 1]].isFlowerBed && onTileStatusMap[j, i + 1] > 0)
										{
											onTileStatusMap[j, i] = onTileStatusMap[j, i + 1];
											chunkHasChangedToday[chunkX, chunkY] = true;
										}
										break;
									case 3:
										if (onTileMap[j, i - 1] > -1 && allObjectSettings[onTileMap[j, i - 1]].isFlowerBed && onTileStatusMap[j, i - 1] > 0)
										{
											onTileStatusMap[j, i] = onTileStatusMap[j, i - 1];
											chunkHasChangedToday[chunkX, chunkY] = true;
										}
										break;
									}
								}
							}
							else if ((bool)allObjects[onTileMap[j, i]].sprinklerTile)
							{
								int[] item = new int[2] { j, i };
								if (allObjects[onTileMap[j, i]].sprinklerTile.isTank)
								{
									waterTankPos.Add(item);
								}
								else
								{
									sprinklerPos.Add(item);
								}
							}
							else if ((bool)allObjects[onTileMap[j, i]].tileObjectChest)
							{
								if (allObjects[onTileMap[j, i]].tileObjectChest.isFishPond)
								{
									ContainerManager.manage.fishPondManager.AddToPondToEndOfDayList(j, i);
								}
								else if (allObjects[onTileMap[j, i]].tileObjectChest.isBugTerrarium)
								{
									ContainerManager.manage.fishPondManager.AddBugTerrariumToEndOfDayList(j, i);
								}
							}
							else if (allObjects[onTileMap[j, i]].hasExtensions)
							{
								if (onTileMap[j, i] == 28)
								{
									onTileMap[j, i] = -1;
									chunkHasChangedToday[chunkX, chunkY] = true;
								}
								else if ((bool)allObjects[onTileMap[j, i]].tileObjectGrowthStages)
								{
									allObjects[onTileMap[j, i]].tileObjectGrowthStages.checkIfShouldGrow(j, i);
									if ((bool)allObjects[onTileMap[j, i]].tileObjectGrowthStages && allObjects[onTileMap[j, i]].tileObjectGrowthStages.checkIfShouldDie(j, i))
									{
										if ((bool)allObjects[onTileMap[j, i]].tileObjectGrowthStages.changeToWhenDead)
										{
											onTileMap[j, i] = allObjects[onTileMap[j, i]].tileObjectGrowthStages.changeToWhenDead.tileObjectId;
											onTileStatusMap[j, i] = Mathf.Clamp(onTileStatusMap[j, i], 0, allObjects[onTileMap[j, i]].tileObjectGrowthStages.objectStages.Length);
											if (tileTypeMap[j, i] == 12 || tileTypeMap[j, i] == 13)
											{
												tileTypeMap[j, i] = 7;
											}
										}
										else
										{
											onTileMap[j, i] = -1;
											onTileStatusMap[j, i] = -1;
										}
									}
									chunkHasChangedToday[chunkX, chunkY] = true;
								}
							}
						}
						if (RealWorldTimeLight.time.getTomorrowsMonth() == 2 && !waterMap[j, i] && !tileTypes[tileTypeMap[j, i]].isPath && UnityEngine.Random.Range(0, 5) == 1 && j > 10 && j < 990 && i > 10 && i < 990 && (onTileMap[j, i] == -1 || (onTileMap[j, i] >= 0 && allObjectSettings[onTileMap[j, i]].isGrass)))
						{
							int num = UnityEngine.Random.Range(-1, 2);
							int num2 = UnityEngine.Random.Range(-1, 2);
							if (onTileMap[j + num, i + num2] >= 0 && (bool)allObjectSettings[onTileMap[j + num, i + num2]].dropsObjectOnDeath)
							{
								int randomMushroomId = SeasonManager.manage.GetRandomMushroomId(tileTypeMap[j, i]);
								if (randomMushroomId != -1)
								{
									onTileMap[j, i] = randomMushroomId;
									onTileStatusMap[j, i] = 0;
								}
								chunkHasChangedToday[chunkX, chunkY] = true;
							}
						}
						if (!raining)
						{
							if (tileTypes[tileTypeMap[j, i]].dryVersion != -1)
							{
								tileTypeMap[j, i] = tileTypes[tileTypeMap[j, i]].dryVersion;
								chunkHasChangedToday[chunkX, chunkY] = true;
							}
							if (tileTypeMap[j, i] == 12 || tileTypeMap[j, i] == 13 || tileTypeMap[j, i] == 43 || tileTypeMap[j, i] == 44)
							{
								if ((onTileMap[j, i] <= -1 || !allObjects[onTileMap[j, i]].tileObjectGrowthStages || !allObjects[onTileMap[j, i]].tileObjectGrowthStages.needsTilledSoil) && UnityEngine.Random.Range(0, 4) == 2)
								{
									tileTypeMap[j, i] = 7;
									chunkHasChangedToday[chunkX, chunkY] = true;
								}
							}
							else if ((tileTypeMap[j, i] == 7 || tileTypeMap[j, i] == 8) && (onTileMap[j, i] <= -1 || !allObjects[onTileMap[j, i]].tileObjectGrowthStages || !allObjects[onTileMap[j, i]].tileObjectGrowthStages.needsTilledSoil) && UnityEngine.Random.Range(0, 3) == 2 && NetworkMapSharer.Instance.isServer)
							{
								NetworkMapSharer.Instance.RpcUpdateTileType(tileTypeStatusMap[j, i], j, i);
							}
						}
						else if (tileTypes[tileTypeMap[j, i]].wetVersion != -1)
						{
							tileTypeMap[j, i] = tileTypes[tileTypeMap[j, i]].wetVersion;
							chunkHasChangedToday[chunkX, chunkY] = true;
						}
					}
				}
				if (chunkCounter >= 10)
				{
					chunkCounter = 0;
					yield return null;
				}
				else
				{
					chunkCounter++;
				}
			}
		}
		foreach (int[] item2 in sprinklerPos)
		{
			allObjects[onTileMap[item2[0], item2[1]]].sprinklerTile.waterTiles(item2[0], item2[1], waterTankPos);
		}
		ContainerManager.manage.fishPondManager.DoFishPondNextDay();
		ContainerManager.manage.fishPondManager.DoBugTerrariumNextDay();
		StartCoroutine(refreshAllChunksNewDay());
	}

	public bool checkAllNeighboursAreEmpty(int x, int y)
	{
		if (onTileMap[Mathf.Clamp(x, 0, mapSize - 1), Mathf.Clamp(y + 1, 0, mapSize - 1)] == -1 && onTileMap[Mathf.Clamp(x, 0, mapSize - 1), Mathf.Clamp(y - 1, 0, mapSize - 1)] == -1 && onTileMap[Mathf.Clamp(x + 1, 0, mapSize - 1), Mathf.Clamp(y, 0, mapSize - 1)] == -1 && onTileMap[Mathf.Clamp(x - 1, 0, mapSize - 1), Mathf.Clamp(y, 0, mapSize - 1)] == -1)
		{
			return true;
		}
		return false;
	}

	public void sprinkerContinuesToWater(int xPos, int yPos)
	{
		StartCoroutine(continueWateringSprinkler(xPos, yPos));
	}

	private IEnumerator continueWateringSprinkler(int xPos, int yPos)
	{
		if (!allObjects[onTileMap[xPos, yPos]].sprinklerTile)
		{
			yield break;
		}
		while (onTileMap[xPos, yPos] > -1 && onTileStatusMap[xPos, yPos] != 0 && RealWorldTimeLight.time.currentHour >= 1 && RealWorldTimeLight.time.currentHour < 9)
		{
			yield return new WaitForSeconds(0.25f);
			if (onTileMap[xPos, yPos] <= -1 || !allObjects[onTileMap[xPos, yPos]].sprinklerTile)
			{
				continue;
			}
			for (int i = -allObjects[onTileMap[xPos, yPos]].sprinklerTile.horizontalSize; i < allObjects[onTileMap[xPos, yPos]].sprinklerTile.horizontalSize + 1; i++)
			{
				for (int j = -allObjects[onTileMap[xPos, yPos]].sprinklerTile.verticlSize; j < allObjects[onTileMap[xPos, yPos]].sprinklerTile.verticlSize + 1; j++)
				{
					if (tileTypes[tileTypeMap[xPos + i, yPos + j]].wetVersion != -1)
					{
						NetworkMapSharer.Instance.RpcUpdateTileType(tileTypes[tileTypeMap[xPos + i, yPos + j]].wetVersion, xPos + i, yPos + j);
					}
				}
			}
		}
	}

	public int getChunkSize()
	{
		return chunkSize;
	}

	public int getTileSize()
	{
		return tileSize;
	}

	public int GetMapSize()
	{
		return mapSize;
	}

	public void startCountDownForTile(int itemId, int xPos, int yPos, HouseDetails inside = null)
	{
		CurrentChanger currentChanger = new CurrentChanger(xPos, yPos);
		ItemChange itemChange = Inventory.Instance.allItems[itemId].itemChange;
		if (itemChange == null)
		{
			return;
		}
		if (inside != null)
		{
			currentChanger.houseX = inside.xPos;
			currentChanger.houseY = inside.yPos;
			currentChanger.counterSeconds = itemChange.getChangeTime(inside.houseMapOnTile[xPos, yPos]);
			currentChanger.counterDays = itemChange.getChangeDays(inside.houseMapOnTile[xPos, yPos]);
			currentChanger.cycles = itemChange.getCycles(inside.houseMapOnTile[xPos, yPos]);
			currentChanger.timePerCycles = currentChanger.counterSeconds;
			if (currentChanger.counterDays <= 0)
			{
			}
		}
		else
		{
			currentChanger.houseX = -1;
			currentChanger.houseY = -1;
			currentChanger.counterSeconds = itemChange.getChangeTime(onTileMap[xPos, yPos]);
			currentChanger.counterDays = itemChange.getChangeDays(onTileMap[xPos, yPos]);
			currentChanger.cycles = itemChange.getCycles(onTileMap[xPos, yPos]);
			currentChanger.timePerCycles = currentChanger.counterSeconds;
		}
		allChangers.Add(currentChanger);
		StartCoroutine(countDownPos(currentChanger));
	}

	public void loadCountDownForTile(CurrentChanger thisChanger)
	{
		allChangers.Add(thisChanger);
		StartCoroutine(countDownPos(thisChanger));
	}

	public bool checkIfTileHasChanger(int xPos, int yPos, HouseDetails house = null)
	{
		if (house == null && onTileMap[xPos, yPos] > -1 && (!allObjects[onTileMap[xPos, yPos]].tileObjectItemChanger || ((bool)allObjects[onTileMap[xPos, yPos]].tileObjectItemChanger && onTileStatusMap[xPos, yPos] <= 0)))
		{
			return false;
		}
		if (house != null && house.houseMapOnTile[xPos, yPos] > -1 && (!allObjects[house.houseMapOnTile[xPos, yPos]].tileObjectItemChanger || ((bool)allObjects[house.houseMapOnTile[xPos, yPos]].tileObjectItemChanger && house.houseMapOnTileStatus[xPos, yPos] <= 0)))
		{
			return false;
		}
		for (int i = 0; i < allChangers.Count; i++)
		{
			if (house == null)
			{
				if (allChangers[i].xPos == xPos && allChangers[i].yPos == yPos && allChangers[i].houseX == -1 && allChangers[i].houseY == -1)
				{
					return true;
				}
			}
			else if (allChangers[i].xPos == xPos && allChangers[i].yPos == yPos && allChangers[i].houseX == house.xPos && allChangers[i].houseY == house.yPos)
			{
				return true;
			}
		}
		if (house == null)
		{
			NetworkMapSharer.Instance.RpcGiveOnTileStatus(-2, xPos, yPos);
		}
		else
		{
			NetworkMapSharer.Instance.RpcGiveOnTileStatusInside(-2, xPos, yPos, house.xPos, house.yPos);
		}
		return false;
	}

	private bool checkNeighbourIsWater(int xPos, int yPos)
	{
		if (waterMap[xPos + 1, yPos])
		{
			return true;
		}
		if (waterMap[xPos - 1, yPos])
		{
			return true;
		}
		if (waterMap[xPos, yPos + 1])
		{
			return true;
		}
		if (waterMap[xPos, yPos - 1])
		{
			return true;
		}
		return false;
	}

	public void doWaterCheckOnHeightChange(int xPos, int yPos)
	{
		StartCoroutine(checkWaterAndFlow(xPos, yPos));
	}

	private IEnumerator checkWaterAndFlow(int xPos, int yPos)
	{
		yield return waterSec;
		if (heightMap[xPos, yPos] <= 0 && !waterMap[xPos, yPos] && xPos != 0 && yPos != 0 && yPos != mapSize - 1 && xPos != mapSize - 1 && checkNeighbourIsWater(xPos, yPos))
		{
			NetworkMapSharer.Instance.RpcFillWithWater(xPos, yPos);
			waterMap[xPos, yPos] = true;
			if (heightMap[xPos + 1, yPos] <= 0 && !waterMap[xPos + 1, yPos])
			{
				StartCoroutine(checkWaterAndFlow(xPos + 1, yPos));
			}
			if (heightMap[xPos - 1, yPos] <= 0 && !waterMap[xPos - 1, yPos])
			{
				StartCoroutine(checkWaterAndFlow(xPos - 1, yPos));
			}
			if (heightMap[xPos, yPos + 1] <= 0 && !waterMap[xPos, yPos + 1])
			{
				StartCoroutine(checkWaterAndFlow(xPos, yPos + 1));
			}
			if (heightMap[xPos, yPos - 1] <= 0 && !waterMap[xPos, yPos - 1])
			{
				StartCoroutine(checkWaterAndFlow(xPos, yPos - 1));
			}
		}
	}

	private IEnumerator countDownPos(CurrentChanger change)
	{
		HouseDetails inside = null;
		if (change.houseX != -1 && change.houseY != -1)
		{
			inside = HouseManager.manage.getHouseInfo(change.houseX, change.houseY);
		}
		bool doubleSpeed = false;
		if (change.cycles == 0)
		{
			change.cycles = 1;
		}
		while (change.cycles > 0)
		{
			change.counterSeconds = change.timePerCycles;
			if (inside == null && onTileMap[change.xPos, change.yPos] > -1 && allObjects[onTileMap[change.xPos, change.yPos]].tileObjectItemChanger.useWindMill)
			{
				for (int i = -14; i <= 14; i++)
				{
					for (int j = -14; j <= 14; j++)
					{
						if (!isPositionOnMap(change.xPos + i, change.yPos + j))
						{
							continue;
						}
						if (onTileMap[change.xPos + i, change.yPos + j] < -1)
						{
							Vector2 vector = findMultiTileObjectPos(change.xPos + i, change.yPos + j);
							if (onTileMap[(int)vector.x, (int)vector.y] == 16)
							{
								doubleSpeed = true;
								break;
							}
						}
						if (onTileMap[change.xPos + i, change.yPos + j] == 16)
						{
							doubleSpeed = true;
							break;
						}
					}
				}
			}
			if (inside == null && onTileMap[change.xPos, change.yPos] > -1 && allObjects[onTileMap[change.xPos, change.yPos]].tileObjectItemChanger.useSolar)
			{
				for (int k = -8; k <= 8; k++)
				{
					for (int l = -8; l <= 8; l++)
					{
						if (!isPositionOnMap(change.xPos + k, change.yPos + l))
						{
							continue;
						}
						if (onTileMap[change.xPos + k, change.yPos + l] < -1)
						{
							Vector2 vector2 = findMultiTileObjectPos(change.xPos + k, change.yPos + l);
							if (onTileMap[(int)vector2.x, (int)vector2.y] == 703)
							{
								doubleSpeed = true;
								break;
							}
						}
						if (onTileMap[change.xPos + k, change.yPos + l] == 703)
						{
							doubleSpeed = true;
							break;
						}
					}
				}
			}
			while (change.counterSeconds > 0 || change.counterDays > 0)
			{
				yield return sec;
				if (change.counterDays <= 0)
				{
					if (doubleSpeed)
					{
						change.counterSeconds -= 2;
					}
					else
					{
						change.counterSeconds--;
					}
				}
			}
			while (change.startedUnderground != RealWorldTimeLight.time.underGround)
			{
				yield return sec;
			}
			while (change.startedOffIsland != RealWorldTimeLight.time.offIsland)
			{
				yield return sec;
			}
			while (!NetworkMapSharer.Instance.serverActive())
			{
				yield return sec;
			}
			TileObject tileObject = null;
			if (inside != null)
			{
				DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(inside.xPos, inside.yPos);
				tileObject = Instance.getTileObjectForHouse(inside.houseMapOnTile[change.xPos, change.yPos], displayPlayerHouseTiles.getStartingPosTransform().position + new Vector3(change.xPos * 2, 0f, change.yPos * 2), change.xPos, change.yPos, inside);
			}
			else if (onTileMap[change.xPos, change.yPos] > 0)
			{
				tileObject = getTileObjectForServerDrop(onTileMap[change.xPos, change.yPos], new Vector3(change.xPos * 2, heightMap[change.xPos, change.yPos], change.yPos * 2));
			}
			if ((bool)tileObject)
			{
				if ((bool)tileObject.tileObjectItemChanger)
				{
					tileObject.tileObjectItemChanger.ejectItemOnCycle(change.xPos, change.yPos, inside);
				}
				returnTileObject(tileObject);
			}
			change.cycles--;
		}
		if (inside != null)
		{
			NetworkMapSharer.Instance.RpcEjectItemFromChangerInside(change.xPos, change.yPos, inside.xPos, inside.yPos);
		}
		else
		{
			NetworkMapSharer.Instance.RpcEjectItemFromChanger(change.xPos, change.yPos);
		}
		if (RealWorldTimeLight.time.offIsland && onTileMap[change.xPos, change.yPos] == GenerateVisitingIsland.Instance.sharkStatue.tileObjectId)
		{
			GenerateVisitingIsland.Instance.UseASharkStatue();
		}
		yield return null;
		allChangers.Remove(change);
	}

	private bool checkIfOnMap(int intToCheck, bool inside)
	{
		if (inside)
		{
			if (intToCheck >= 0 && intToCheck < 25)
			{
				return true;
			}
			return false;
		}
		if (intToCheck >= 0 && intToCheck < Instance.GetMapSize())
		{
			return true;
		}
		return false;
	}

	public Vector2 findMultiTileObjectPos(int xPos, int yPos, HouseDetails house = null)
	{
		if (house == null)
		{
			if (onTileMap[xPos, yPos] >= -1)
			{
				return new Vector2(xPos, yPos);
			}
			if (onTileMap[xPos, yPos] <= -200)
			{
				int num = onTileMap[xPos, yPos] + 201;
				return new Vector2(xPos + num, yPos);
			}
			if (onTileMap[xPos, yPos] <= -100)
			{
				int num2 = onTileMap[xPos, yPos] + 101;
				int num3 = 0;
				if (onTileMap[xPos, yPos + num2] <= -200)
				{
					num3 = onTileMap[xPos, yPos + num2] + 201;
				}
				return new Vector2(xPos + num3, yPos + num2);
			}
			bool flag = false;
			bool flag2 = false;
			int num4 = 0;
			int num5 = 0;
			bool inside = false;
			int num6 = 1000;
			while (!flag || !flag2)
			{
				num6--;
				if (num6 <= 0)
				{
					Debug.Log("Search size reached - This should never be called.");
					return new Vector2(xPos, yPos);
				}
				if (!flag2)
				{
					if (checkIfOnMap(xPos + num4, inside))
					{
						if (onTileMap[xPos + num4, yPos] == -3)
						{
							flag2 = true;
						}
						else if (onTileMap[xPos + num4, yPos] == -4 && checkIfOnMap(xPos + (num4 - 1), inside) && onTileMap[xPos + (num4 - 1), yPos] != -4)
						{
							num4--;
							flag2 = true;
						}
						else
						{
							num4--;
						}
					}
					else
					{
						num4 = 0;
						flag2 = true;
					}
				}
				if (!flag2)
				{
					continue;
				}
				if (checkIfOnMap(yPos + num5, inside))
				{
					if (onTileMap[xPos + num4, yPos + num5] != -3)
					{
						flag = true;
					}
					else if (onTileMap[xPos + num4, yPos + num5] == -3 && checkIfOnMap(yPos + (num5 - 1), inside) && onTileMap[xPos + num4, yPos + (num5 - 1)] != -3)
					{
						num5--;
						flag = true;
					}
					else
					{
						num5--;
					}
				}
				else
				{
					num5 = 0;
					flag = true;
				}
			}
			xPos += num4;
			yPos += num5;
			return new Vector2(xPos, yPos);
		}
		if (house.houseMapOnTile[xPos, yPos] >= -1)
		{
			return new Vector2(xPos, yPos);
		}
		bool flag3 = false;
		bool flag4 = false;
		int num7 = 0;
		int num8 = 0;
		bool inside2 = true;
		int num9 = 1000;
		while (!flag3 || !flag4)
		{
			num9--;
			if (num9 <= 0)
			{
				Debug.Log("Search size reached - This should never be called.");
				return new Vector2(xPos, yPos);
			}
			if (!flag4)
			{
				if (checkIfOnMap(xPos + num7, inside2))
				{
					if (house.houseMapOnTile[xPos + num7, yPos] == -3)
					{
						flag4 = true;
					}
					else if (house.houseMapOnTile[xPos + num7, yPos] == -4 && checkIfOnMap(xPos + (num7 - 1), inside2) && house.houseMapOnTile[xPos + (num7 - 1), yPos] != -4)
					{
						num7--;
						flag4 = true;
					}
					else
					{
						num7--;
					}
				}
				else
				{
					num7 = 0;
					flag4 = true;
				}
			}
			if (!flag4)
			{
				continue;
			}
			if (checkIfOnMap(yPos + num8, inside2))
			{
				if (house.houseMapOnTile[xPos + num7, yPos + num8] != -3)
				{
					flag3 = true;
				}
				else if (house.houseMapOnTile[xPos + num7, yPos + num8] == -3 && checkIfOnMap(yPos + (num8 - 1), inside2) && house.houseMapOnTile[xPos + num7, yPos + (num8 - 1)] != -3)
				{
					num8--;
					flag3 = true;
				}
				else
				{
					num8--;
				}
			}
			else
			{
				num8 = 0;
				flag3 = true;
			}
		}
		xPos += num7;
		yPos += num8;
		return new Vector2(xPos, yPos);
	}

	public Vector3 findTileObjectAround(Vector3 position, TileObject[] lookingForObjects, int distance = 5, bool checkIfFencedOff = false)
	{
		int num = (int)position.x / 2;
		int num2 = (int)position.z / 2;
		Vector3 result = Vector3.zero;
		int num3 = fencedOffMap[num, num2];
		for (int i = -distance; i < distance; i++)
		{
			for (int j = -distance; j < distance; j++)
			{
				for (int k = 0; k < lookingForObjects.Length; k++)
				{
					if (isPositionOnMap(num + i, num2 + j) && onTileMap[num + i, num2 + j] == lookingForObjects[k].tileObjectId && (!allObjects[onTileMap[num + i, num2 + j]].tileOnOff || ((bool)allObjects[onTileMap[num + i, num2 + j]].tileOnOff && onTileStatusMap[num + i, num2 + j] == 1)) && (!checkIfFencedOff || (checkIfFencedOff && num3 == fencedOffMap[num + i, num2 + j])))
					{
						if (checkIfFencedOff)
						{
							return new Vector3((num + i) * 2, heightMap[num + i, num2 + j], (num2 + j) * 2);
						}
						result = new Vector3((num + i) * 2, heightMap[num + i, num2 + j], (num2 + j) * 2);
					}
				}
			}
		}
		return result;
	}

	public Vector3 findClosestTileObjectToPosition(Vector3 position, TileObject[] lookingForObjects, int distance = 5, bool checkIfFencedOff = false)
	{
		int num = (int)position.x / 2;
		int num2 = (int)position.z / 2;
		Vector3 result = Vector3.zero;
		int num3 = fencedOffMap[num, num2];
		float num4 = (float)distance * 3.5f;
		for (int i = -distance; i < distance; i++)
		{
			for (int j = -distance; j < distance; j++)
			{
				for (int k = 0; k < lookingForObjects.Length; k++)
				{
					if (onTileMap[num + i, num2 + j] == lookingForObjects[k].tileObjectId && (!allObjects[onTileMap[num + i, num2 + j]].tileOnOff || ((bool)allObjects[onTileMap[num + i, num2 + j]].tileOnOff && onTileStatusMap[num + i, num2 + j] == 1)) && (!checkIfFencedOff || (checkIfFencedOff && num3 == fencedOffMap[num + i, num2 + j])))
					{
						float num5 = Vector3.Distance(position, new Vector3((num + i) * 2, heightMap[num + i, num2 + j], (num2 + j) * 2));
						if (num5 <= num4)
						{
							num4 = num5;
							result = new Vector3((num + i) * 2, heightMap[num + i, num2 + j], (num2 + j) * 2);
						}
					}
				}
			}
		}
		return result;
	}

	public Vector3 findDroppedObjectAround(Vector3 position, InventoryItem[] lookingForObjects, float distance = 5f, bool checkIfFencedOff = false)
	{
		Vector3 result = Vector3.zero;
		int num = fencedOffMap[(int)position.x / 2, (int)position.z / 2];
		if (Physics.CheckSphere(position, distance, pickUpLayer))
		{
			Collider[] array = Physics.OverlapSphere(position, distance, pickUpLayer);
			if (array.Length != 0)
			{
				float num2 = distance + 2f;
				for (int i = 0; i < array.Length; i++)
				{
					if (!(Vector3.Distance(position, array[i].transform.position) < num2) || (checkIfFencedOff && (!checkIfFencedOff || fencedOffMap[(int)array[i].transform.position.x / 2, (int)array[i].transform.position.z / 2] != num)))
					{
						continue;
					}
					DroppedItem componentInParent = array[i].GetComponentInParent<DroppedItem>();
					if (!componentInParent)
					{
						continue;
					}
					for (int j = 0; j < lookingForObjects.Length; j++)
					{
						if (componentInParent.myItemId == Inventory.Instance.getInvItemId(lookingForObjects[j]))
						{
							num2 = Vector3.Distance(position, componentInParent.transform.position);
							result = new Vector3(componentInParent.onTile.x * 2f, heightMap[Mathf.RoundToInt(componentInParent.transform.position.x) / 2, Mathf.RoundToInt(componentInParent.transform.position.z) / 2], componentInParent.onTile.y * 2f);
						}
					}
				}
			}
		}
		return result;
	}

	public bool isSeatTaken(Vector3 seatPos, int desiredPos = -1)
	{
		int num = onTileMap[(int)seatPos.x / 2, (int)seatPos.z / 2];
		if (onTileStatusMap[(int)seatPos.x / 2, (int)seatPos.z / 2] == 3)
		{
			return true;
		}
		if (num < 0 || !allObjects[num].tileObjectFurniture)
		{
			return true;
		}
		if (!allObjects[num].tileObjectFurniture.seatPosition2 && desiredPos == 2)
		{
			return true;
		}
		switch (desiredPos)
		{
		case -1:
			if ((bool)allObjects[num].tileObjectFurniture.seatPosition2)
			{
				if (onTileStatusMap[(int)seatPos.x / 2, (int)seatPos.z / 2] == 3)
				{
					return true;
				}
				return false;
			}
			if (onTileStatusMap[(int)seatPos.x / 2, (int)seatPos.z / 2] == 1)
			{
				return true;
			}
			return false;
		case 1:
			if (onTileStatusMap[(int)seatPos.x / 2, (int)seatPos.z / 2] == 2 || onTileStatusMap[(int)seatPos.x / 2, (int)seatPos.z / 2] <= 0)
			{
				return false;
			}
			return true;
		case 2:
			if (onTileStatusMap[(int)seatPos.x / 2, (int)seatPos.z / 2] == 1 || onTileStatusMap[(int)seatPos.x / 2, (int)seatPos.z / 2] <= 0)
			{
				return false;
			}
			return true;
		default:
			return false;
		}
	}

	public bool hasSquareBeenWatered(Vector3 cropPos)
	{
		int num = (int)cropPos.x / 2;
		int num2 = (int)cropPos.z / 2;
		if (Instance.tileTypeMap[num, num2] == 8 || Instance.tileTypeMap[num, num2] == 13)
		{
			return true;
		}
		return false;
	}

	public Vector3 findClosestTileObjectAround(Vector3 position, TileObject[] lookingForObjects, int distance = 5, bool checkIfWatered = false, bool checkIfSeatEmpty = false)
	{
		int num = (int)position.x / 2;
		int num2 = (int)position.z / 2;
		float num3 = (float)distance * 2f;
		float num4 = num3;
		Vector3 result = Vector3.zero;
		_ = fencedOffMap[num, num2];
		for (int i = -distance; i < distance; i++)
		{
			for (int j = -distance; j < distance; j++)
			{
				for (int k = 0; k < lookingForObjects.Length; k++)
				{
					if (onTileMap[num + i, num2 + j] == lookingForObjects[k].tileObjectId && (!lookingForObjects[k].tileObjectFurniture || !lookingForObjects[k].tileObjectFurniture.isToilet || UnityEngine.Random.Range(0, 10) >= 8) && ((!checkIfSeatEmpty && !checkIfWatered) || (checkIfWatered && !hasSquareBeenWatered(new Vector3((num + i) * 2, heightMap[num + i, num2 + j], (num2 + j) * 2))) || (checkIfSeatEmpty && !isSeatTaken(new Vector3((num + i) * 2, heightMap[num + i, num2 + j], (num2 + j) * 2)))))
					{
						num4 = Vector3.Distance(new Vector3((num + i) * 2, heightMap[num + i, num2 + j], (num2 + j) * 2), position);
						if (num4 < num3)
						{
							num3 = num4;
							result = new Vector3((num + i) * 2, heightMap[num + i, num2 + j], (num2 + j) * 2);
						}
					}
				}
			}
		}
		return result;
	}

	public Vector3 findClosestWaterTile(Vector3 position, int distance = 5, bool checkIfFencedOff = false, int depth = 0)
	{
		int num = (int)position.x / 2;
		int num2 = (int)position.z / 2;
		Vector3 result = Vector3.zero;
		int num3 = fencedOffMap[num, num2];
		float num4 = (float)distance * 0.1f;
		float num5 = num4;
		for (int i = -distance; i < distance; i++)
		{
			for (int j = -distance; j < distance; j++)
			{
				if (isPositionOnMap(num + i, num2 + j) && waterMap[num + i, num2 + j] && heightMap[num + i, num2 + j] <= depth && (!checkIfFencedOff || (checkIfFencedOff && num3 == fencedOffMap[num + i, num2 + j])))
				{
					num5 = Vector3.Distance(new Vector3((num + i) * 2, heightMap[num + i, num2 + j], (num2 + j) * 2), position);
					if (num5 < num4)
					{
						num4 = num5;
						result = new Vector3((num + i) * 2, heightMap[num + i, num2 + j], (num2 + j) * 2);
					}
				}
			}
		}
		return result;
	}

	private bool chunkNeedsFenceCheck(bool[,] hadMapCheck, int x, int y)
	{
		if (x == 0 || y == 0)
		{
			return false;
		}
		if (hadMapCheck[x - 1, y] || hadMapCheck[x, y - 1])
		{
			return true;
		}
		return false;
	}

	public void resetAllChunkChangedMaps()
	{
		clientRequestedMap = new bool[mapSize / 10, mapSize / 10];
		chunkChangedMap = new bool[mapSize / 10, mapSize / 10];
		changedMapWater = new bool[mapSize / 10, mapSize / 10];
		changedMapHeight = new bool[mapSize / 10, mapSize / 10];
		changedMapOnTile = new bool[mapSize / 10, mapSize / 10];
		changedMapTileType = new bool[mapSize / 10, mapSize / 10];
	}

	public bool isPositionOnMap(int xPos, int yPos)
	{
		if (xPos < 0 || xPos >= Instance.GetMapSize() || yPos < 0 || yPos >= Instance.GetMapSize())
		{
			return false;
		}
		return true;
	}

	public bool isPositionOnMap(Vector3 position)
	{
		int num = (int)position.x / 2;
		int num2 = (int)position.z / 2;
		if (num < 0 || num >= Instance.GetMapSize() || num2 < 0 || num2 >= Instance.GetMapSize())
		{
			return false;
		}
		return true;
	}

	public bool isPositionInWater(Vector3 position)
	{
		if (isPositionOnMap(position))
		{
			int num = (int)position.x / 2;
			int num2 = (int)position.z / 2;
			return waterMap[num, num2];
		}
		return true;
	}

	public bool isPositionChest(int xPos, int yPos)
	{
		if (xPos < 0 || xPos >= Instance.GetMapSize() || yPos < 0 || yPos >= Instance.GetMapSize())
		{
			return false;
		}
		if (onTileMap[xPos, yPos] > 0)
		{
			return allObjects[onTileMap[xPos, yPos]].tileObjectChest;
		}
		return false;
	}

	private bool isAFence(int xPos, int yPos)
	{
		return fencedOffMap[xPos, yPos] == 1;
	}

	public IEnumerator fenceCheck()
	{
		int size = mapSize / chunkSize;
		bool[,] hadMapCheck = new bool[size, size];
		int yieldAmount = 0;
		for (int y = 1; y < size; y++)
		{
			for (int x = 1; x < size; x++)
			{
				if (chunkWithFenceInIt[x, y] || chunkNeedsFenceCheck(hadMapCheck, x, y))
				{
					if (!changedMapOnTile[x, y] && !chunkNeedsFenceCheck(hadMapCheck, x, y))
					{
						continue;
					}
					hadMapCheck[x, y] = true;
					for (int i = 0; i < 10; i++)
					{
						for (int j = 0; j < 10; j++)
						{
							if (fencedOffMap[x * 10 + j, y * 10 + i] == 1)
							{
								continue;
							}
							if (fencedOffMap[x * 10 + j - 1, y * 10 + i] > 0 && fencedOffMap[x * 10 + j, y * 10 + i - 1] > 0)
							{
								fencedOffMap[x * 10 + j, y * 10 + i] = 2;
								continue;
							}
							fencedOffMap[x * 10 + j, y * 10 + i] = 0;
							int num = i - 1;
							int num2 = j - 1;
							while (fencedOffMap[x * 10 + num2, y * 10 + i] > 1)
							{
								fencedOffMap[x * 10 + num2, y * 10 + i] = 0;
								for (int k = i + 1; isPositionOnMap(x * 10 + num2, y * 10 + k) && fencedOffMap[x * 10 + num2, y * 10 + k] > 1; k++)
								{
									fencedOffMap[x * 10 + num2, y * 10 + k] = 0;
								}
								int num3 = i - 1;
								while (isPositionOnMap(x * 10 + num2, y * 10 + num3) && fencedOffMap[x * 10 + num2, y * 10 + num3] > 1)
								{
									fencedOffMap[x * 10 + num2, y * 10 + num3] = 0;
									num3--;
								}
								num2--;
							}
							while (fencedOffMap[x * 10 + j, y * 10 + num] > 1)
							{
								fencedOffMap[x * 10 + j, y * 10 + num] = 0;
								for (int l = j + 1; isPositionOnMap(x * 10 + l, y * 10 + num) && fencedOffMap[x * 10 + l, y * 10 + num] > 1; l++)
								{
									fencedOffMap[x * 10 + l, y * 10 + num] = 0;
								}
								num2 = j - 1;
								while (isPositionOnMap(x * 10 + num2, y * 10 + num) && fencedOffMap[x * 10 + num2, y * 10 + num] > 1)
								{
									fencedOffMap[x * 10 + num2, y * 10 + num] = 0;
									num2--;
								}
								num--;
							}
						}
					}
					yieldAmount++;
					if (yieldAmount >= 100)
					{
						yieldAmount = 0;
						yield return null;
					}
				}
				else
				{
					yieldAmount++;
					if (yieldAmount >= 500)
					{
						yieldAmount = 0;
						yield return null;
					}
				}
			}
		}
		yield return StartCoroutine(labelFencedOffAreas());
	}

	public IEnumerator labelFencedOffAreas()
	{
		int size = mapSize / chunkSize;
		bool[,] hadMapCheck = new bool[size, size];
		int fenceGroup = 3;
		int yieldAmount = 0;
		for (int y = 0; y < size; y++)
		{
			for (int x = 0; x < size; x++)
			{
				if (chunkWithFenceInIt[x, y])
				{
					if (!changedMapOnTile[x, y] && !chunkNeedsFenceCheck(hadMapCheck, x, y))
					{
						continue;
					}
					for (int i = 0; i < 10; i++)
					{
						for (int j = 0; j < 10; j++)
						{
							if (isPositionOnMap(x * 10 + j, y * 10 + i) && fencedOffMap[x * 10 + j, y * 10 + i] == 2)
							{
								fenceGroup = ((isPositionOnMap(x * 10 + j, y * 10 + i - 1) && fencedOffMap[x * 10 + j, y * 10 + i - 1] > 2) ? fencedOffMap[x * 10 + j, y * 10 + i - 1] : ((isPositionOnMap(x * 10 + j, y * 10 + i + 1) && fencedOffMap[x * 10 + j, y * 10 + i + 1] > 2) ? fencedOffMap[x * 10 + j, y * 10 + i + 1] : ((isPositionOnMap(x * 10 + j - 1, y * 10 + i) && fencedOffMap[x * 10 + j - 1, y * 10 + i] > 2) ? fencedOffMap[x * 10 + j - 1, y * 10 + i] : ((!isPositionOnMap(x * 10 + j + 1, y * 10 + i) || fencedOffMap[x * 10 + j + 1, y * 10 + i] <= 2) ? (fenceGroup + 1) : fencedOffMap[x * 10 + j + 1, y * 10 + i]))));
								fencedOffMap[x * 10 + j, y * 10 + i] = fenceGroup;
								int num = j - 1;
								while (isPositionOnMap(x * 10 + num, y * 10 + i) && fencedOffMap[x * 10 + num, y * 10 + i] != fenceGroup && fencedOffMap[x * 10 + num, y * 10 + i] > 1)
								{
									fencedOffMap[x * 10 + num, y * 10 + i] = fenceGroup;
									num--;
								}
							}
						}
					}
					yieldAmount++;
					if (yieldAmount >= 100)
					{
						yieldAmount = 0;
						yield return null;
					}
				}
				else
				{
					yieldAmount++;
					if (yieldAmount >= 500)
					{
						yieldAmount = 0;
						yield return null;
					}
				}
			}
		}
	}

	public void findSpaceForDropAfterTileObjectChange(int xPos, int yPos)
	{
		List<DroppedItem> allDropsOnTile = getAllDropsOnTile(xPos, yPos);
		for (int i = 0; i < allDropsOnTile.Count; i++)
		{
			Vector3 vector = moveDropPosToSafeOutside(allDropsOnTile[i].transform.position, useNavMesh: false);
			allDropsOnTile[i].setDesiredPos(vector.y, vector.x, vector.z);
		}
	}

	public bool isOnTileEmpty(int xPos, int yPos)
	{
		return onTileMap[xPos, yPos] == -1;
	}

	public TileObjectSettings getTileObjectSettings(int xPos, int yPos)
	{
		if (onTileMap[xPos, yPos] > -1)
		{
			return allObjectSettings[onTileMap[xPos, yPos]];
		}
		return null;
	}

	public void addToChunksToRefreshList(int xPos, int yPos)
	{
		if (!chunksToRefresh.Contains(new int[2] { xPos, yPos }))
		{
			chunksToRefresh.Add(new int[2] { xPos, yPos });
		}
	}

	public void refreshChunksInChunksToRefreshList()
	{
		for (int i = 0; i < chunksToRefresh.Count; i++)
		{
			refreshTileObjectsOnChunksInUse(chunksToRefresh[i][0], chunksToRefresh[i][1]);
		}
		chunksToRefresh.Clear();
	}

	public bool canReleaseTrapHere(int xPos, int yPos, float height)
	{
		if ((int)height < heightMap[xPos, yPos] || (float)(int)height > (float)heightMap[xPos, yPos] + 2f)
		{
			return false;
		}
		if (onTileMap[xPos, yPos] == -1 || onTileMap[xPos, yPos] == 30)
		{
			return true;
		}
		if (onTileMap[xPos, yPos] >= 0 && allObjectSettings[onTileMap[xPos, yPos]].walkable)
		{
			return true;
		}
		return false;
	}

	public void spawnFirstConnectAirShip()
	{
		Vector3 position = spawnPos.position + new Vector3(-160f, 0f, -10f);
		position.y = 20f;
		UnityEngine.Object.Instantiate(firstConnectAirShip, position, Quaternion.identity);
	}

	public void checkAllCarryHeight(int xPos, int yPos)
	{
		int num = heightMap[xPos, yPos];
		for (int i = 0; i < allCarriables.Count; i++)
		{
			if (allCarriables[i].gameObject.activeInHierarchy && Mathf.RoundToInt(allCarriables[i].transform.position.x / 2f) == xPos && Mathf.RoundToInt(allCarriables[i].transform.position.z / 2f) == yPos && !(allCarriables[i].transform.position.y <= -12f) && (allCarriables[i].dropToPosY < (float)num || Mathf.Abs(allCarriables[i].dropToPosY - (float)num) <= 1f || (Instance.onTileMap[xPos, yPos] == -1 && allCarriables[i].dropToPosY > (float)num)))
			{
				allCarriables[i].MoveToNewDropPos(num);
			}
		}
	}

	public void MoveAllCarriablesInsideHouseToSurfaceOnMove(Vector3 centerPos, float houseSize)
	{
		for (int i = 0; i < allCarriables.Count; i++)
		{
			if (allCarriables[i].gameObject.activeInHierarchy && allCarriables[i].transform.position.y <= -12f && allCarriables[i].transform.position.y >= centerPos.y - 1f && Vector3.Distance(allCarriables[i].transform.position, centerPos) <= houseSize * 2f)
			{
				int num = Mathf.RoundToInt(allCarriables[i].transform.position.x / 2f);
				int num2 = Mathf.RoundToInt(allCarriables[i].transform.position.z / 2f);
				allCarriables[i].RemoveAuthorityBeforeBeforeServerDestroy();
				allCarriables[i].transform.position = new Vector3(allCarriables[i].transform.position.x, heightMap[num, num2], allCarriables[i].transform.position.z);
			}
		}
	}

	public void moveAllCarriablesToSpawn()
	{
		int prefabId = NetworkMapSharer.Instance.cassowaryEgg.GetComponent<PickUpAndCarry>().prefabId;
		for (int i = 0; i < allCarriables.Count; i++)
		{
			if ((bool)allCarriables[i].gameObject && allCarriables[i].gameObject.activeInHierarchy && !allCarriables[i].investigationItem && allCarriables[i].prefabId != prefabId)
			{
				allCarriables[i].dropToPosY = Instance.spawnPos.position.y;
				allCarriables[i].transform.position = Instance.spawnPos.position;
			}
		}
	}

	public bool isPositionInSameFencedArea(Vector3 pos1, Vector3 pos2)
	{
		if (isPositionOnMap(pos1) && isPositionOnMap(pos2))
		{
			return fencedOffMap[Mathf.RoundToInt(pos1.x / 2f), Mathf.RoundToInt(pos1.z / 2f)] == fencedOffMap[Mathf.RoundToInt(pos2.x / 2f), Mathf.RoundToInt(pos2.z / 2f)];
		}
		return false;
	}

	public bool checkIfUnderWater(Vector3 position)
	{
		if (position.y < -1f && isPositionOnMap(position) && position.y > (float)(heightMap[Mathf.RoundToInt(position.x / 2f), Mathf.RoundToInt(position.z / 2f)] - 1) && waterMap[Mathf.RoundToInt(position.x / 2f), Mathf.RoundToInt(position.z / 2f)] && heightMap[Mathf.RoundToInt(position.x / 2f), Mathf.RoundToInt(position.z / 2f)] <= -1)
		{
			return true;
		}
		return false;
	}

	public void cleanOutObjects()
	{
		StartCoroutine(DestroyOverFrames());
	}

	private IEnumerator DestroyOverFrames()
	{
		for (int i = 0; i < allObjects.Length; i++)
		{
			if (allObjectsSorted[i].Count < 500)
			{
				continue;
			}
			for (int num = allObjectsSorted[i].Count - 1; num >= 0; num--)
			{
				if (!allObjectsSorted[i][num].active)
				{
					UnityEngine.Object.Destroy(allObjectsSorted[i][num].gameObject);
					allObjectsSorted[i].RemoveAt(num);
				}
			}
			yield return null;
		}
	}

	private void OnDestroy()
	{
		allObjectsSorted = null;
		allObjectsSorted = new List<TileObject>[0];
	}
}
