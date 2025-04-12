using System.Collections.Generic;
using UnityEngine;

public class GenerateLargeDungeon : MonoBehaviour
{
	public DungeonScript[] allConnections;

	public DungeonScript[] dungeonEntrances;

	public DungeonScript[] northConnections;

	public DungeonScript[] eastConnections;

	public DungeonScript[] southConnections;

	public DungeonScript[] westConnections;

	[Header("Treasure connections")]
	public DungeonScript[] northTreasureRoom;

	public DungeonScript[] eastTreasureRoom;

	public DungeonScript[] southTreasureRoom;

	public DungeonScript[] westTreasureRoom;

	private int roomSize = 16;

	public bool CreateDungeon(int xPos, int yPos, List<int[]> otherEntrances)
	{
		if (CheckIfIsCloseToOtherDungeonEntrances(xPos, yPos, otherEntrances))
		{
			return false;
		}
		int underTileType = 60;
		if (CheckIfRoomCanFitInDirection(xPos, yPos, 47))
		{
			DungeonScript randomDungeon = GetRandomDungeon(dungeonEntrances);
			PlaceDungeonRoom(randomDungeon, xPos, yPos, underTileType);
			CheckAllConnectionsAndCreateRoomForDungeon(randomDungeon, xPos, yPos, underTileType, 4);
			return true;
		}
		return false;
	}

	private DungeonScript GetRandomDungeon(DungeonScript[] pool)
	{
		return pool[Random.Range(0, pool.Length)];
	}

	public void CheckAllConnectionsAndCreateRoomForDungeon(DungeonScript connectTo, int xPos, int yPos, int underTileType, int connectionsChecks)
	{
		if (connectionsChecks < 0)
		{
			return;
		}
		if (connectionsChecks == 0)
		{
			if (connectTo.northConnect)
			{
				int yPos2 = yPos + roomSize;
				if (CheckIfRoomCanFitInDirection(xPos, yPos2, underTileType))
				{
					DungeonScript randomDungeon = GetRandomDungeon(southTreasureRoom);
					PlaceDungeonRoom(randomDungeon, xPos, yPos2, underTileType);
					CheckAllConnectionsAndCreateRoomForDungeon(randomDungeon, xPos, yPos2, underTileType, connectionsChecks - 1);
				}
			}
			if (connectTo.southConnect)
			{
				int yPos3 = yPos - roomSize;
				if (CheckIfRoomCanFitInDirection(xPos, yPos3, underTileType))
				{
					DungeonScript randomDungeon2 = GetRandomDungeon(northTreasureRoom);
					PlaceDungeonRoom(randomDungeon2, xPos, yPos3, underTileType);
					CheckAllConnectionsAndCreateRoomForDungeon(randomDungeon2, xPos, yPos3, underTileType, connectionsChecks - 1);
				}
			}
			if (connectTo.eastConnect)
			{
				int xPos2 = xPos + roomSize;
				if (CheckIfRoomCanFitInDirection(xPos2, yPos, underTileType))
				{
					DungeonScript randomDungeon3 = GetRandomDungeon(westTreasureRoom);
					PlaceDungeonRoom(randomDungeon3, xPos2, yPos, underTileType);
					CheckAllConnectionsAndCreateRoomForDungeon(randomDungeon3, xPos2, yPos, underTileType, connectionsChecks - 1);
				}
			}
			if (connectTo.westConnect)
			{
				int xPos3 = xPos - roomSize;
				if (CheckIfRoomCanFitInDirection(xPos3, yPos, underTileType))
				{
					DungeonScript randomDungeon4 = GetRandomDungeon(eastTreasureRoom);
					PlaceDungeonRoom(randomDungeon4, xPos3, yPos, underTileType);
					CheckAllConnectionsAndCreateRoomForDungeon(randomDungeon4, xPos3, yPos, underTileType, connectionsChecks - 1);
				}
			}
			return;
		}
		int num = Random.Range(0, 2);
		bool flag = false;
		if (num == 1 && (connectionsChecks == 4 || connectionsChecks == 2))
		{
			flag = true;
		}
		if (connectTo.northConnect)
		{
			int yPos4 = yPos + roomSize;
			if (CheckIfRoomCanFitInDirection(xPos, yPos4, underTileType))
			{
				DungeonScript randomDungeon5 = GetRandomDungeon(southConnections);
				if (flag)
				{
					randomDungeon5 = GetRandomDungeon(allConnections);
				}
				PlaceDungeonRoom(randomDungeon5, xPos, yPos4, underTileType);
				CheckAllConnectionsAndCreateRoomForDungeon(randomDungeon5, xPos, yPos4, underTileType, connectionsChecks - 1);
			}
		}
		if (connectTo.southConnect)
		{
			int yPos5 = yPos - roomSize;
			if (CheckIfRoomCanFitInDirection(xPos, yPos5, underTileType))
			{
				DungeonScript randomDungeon6 = GetRandomDungeon(northConnections);
				if (flag)
				{
					randomDungeon6 = GetRandomDungeon(allConnections);
				}
				PlaceDungeonRoom(randomDungeon6, xPos, yPos5, underTileType);
				CheckAllConnectionsAndCreateRoomForDungeon(randomDungeon6, xPos, yPos5, underTileType, connectionsChecks - 1);
			}
		}
		if (connectTo.eastConnect)
		{
			int xPos4 = xPos + roomSize;
			if (CheckIfRoomCanFitInDirection(xPos4, yPos, underTileType))
			{
				DungeonScript randomDungeon7 = GetRandomDungeon(westConnections);
				if (flag)
				{
					randomDungeon7 = GetRandomDungeon(allConnections);
				}
				PlaceDungeonRoom(randomDungeon7, xPos4, yPos, underTileType);
				CheckAllConnectionsAndCreateRoomForDungeon(randomDungeon7, xPos4, yPos, underTileType, connectionsChecks - 1);
			}
		}
		if (!connectTo.westConnect)
		{
			return;
		}
		int xPos5 = xPos - roomSize;
		if (CheckIfRoomCanFitInDirection(xPos5, yPos, underTileType))
		{
			DungeonScript randomDungeon8 = GetRandomDungeon(eastConnections);
			if (flag)
			{
				randomDungeon8 = GetRandomDungeon(allConnections);
			}
			PlaceDungeonRoom(randomDungeon8, xPos5, yPos, underTileType);
			CheckAllConnectionsAndCreateRoomForDungeon(randomDungeon8, xPos5, yPos, underTileType, connectionsChecks - 1);
		}
	}

	public bool CheckIfRoomCanFitInDirection(int xPos, int yPos, int underTileType)
	{
		if (xPos <= 40 || xPos >= 960 || yPos <= 40 || xPos >= 960)
		{
			return false;
		}
		if (MapStorer.store.underWorldTileType[xPos, yPos] == underTileType)
		{
			return false;
		}
		return true;
	}

	private void PlaceDungeonRoom(DungeonScript toPlace, int xPos, int yPos, int underTileType)
	{
		int[,] array = toPlace.convertTo2dArray();
		for (int i = 0; i < array.GetLength(1); i++)
		{
			for (int j = 0; j < array.GetLength(0); j++)
			{
				if (NetworkMapSharer.Instance.isServer)
				{
					int num = Mathf.RoundToInt((xPos + j) / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
					int num2 = Mathf.RoundToInt((yPos + i) / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
					MapStorer.store.underWorldChangedMap[num / WorldManager.Instance.getChunkSize(), num2 / WorldManager.Instance.getChunkSize()] = true;
					MapStorer.store.underworldOnTileChangedMap[num / WorldManager.Instance.getChunkSize(), num2 / WorldManager.Instance.getChunkSize()] = true;
					MapStorer.store.underWorldHeightChangedMap[num / WorldManager.Instance.getChunkSize(), num2 / WorldManager.Instance.getChunkSize()] = true;
					MapStorer.store.underworldTileTypeChangedMap[num / WorldManager.Instance.getChunkSize(), num2 / WorldManager.Instance.getChunkSize()] = true;
				}
				int num3 = array[j, i];
				if (num3 != -3)
				{
					CheckTileForSpecialPlaceable(num3, j, i, xPos, yPos, underTileType);
					if (num3 == 890)
					{
						if (toPlace.northConnect || toPlace.southConnect)
						{
							MapStorer.store.underWorldRotationMap[xPos + j, yPos + i] = 1;
						}
						else
						{
							MapStorer.store.underWorldRotationMap[xPos + j, yPos + i] = 4;
						}
					}
				}
				if (num3 == -5)
				{
					MapStorer.store.underWorldOnTile[xPos + j, yPos + i] = -3;
				}
			}
		}
	}

	private void CheckTileForSpecialPlaceable(int thisTile, int x, int y, int xPos, int yPos, int underTileType)
	{
		if (thisTile != -3)
		{
			MapStorer.store.underWorldHeight[xPos + x, yPos + y] = 1;
			MapStorer.store.underWorldOnTile[xPos + x, yPos + y] = thisTile;
			if (thisTile == 429)
			{
				MapStorer.store.underWorldOnTileStatus[xPos + x, yPos + y] = 0;
				ContainerManager.manage.generateUndergroundChest(xPos + x, yPos + y, ContainerManager.manage.lavaDungeonChestLootTable);
				MapStorer.store.underWorldRotationMap[xPos + x, yPos + y] = Random.Range(1, 5);
			}
			MapStorer.store.underWorldTileType[xPos + x, yPos + y] = underTileType;
			if (thisTile == 563)
			{
				MapStorer.store.underWorldOnTileStatus[xPos + x, yPos + y] = 0;
				MapStorer.store.underWorldRotationMap[xPos + x, yPos + y] = 1;
			}
			if (thisTile == 364 || thisTile == 435)
			{
				MapStorer.store.underWorldRotationMap[xPos + x, yPos + y] = Random.Range(1, 5);
			}
			if (thisTile == 887)
			{
				MapStorer.store.underWorldOnTileStatus[xPos + x, yPos + y] = 0;
			}
			if (thisTile == 881)
			{
				MapStorer.store.underWorldHeight[xPos + x, yPos + y] = 0;
			}
		}
	}

	public bool CheckIfIsCloseToOtherDungeonEntrances(int checkPosX, int checkPosY, List<int[]> otherEntrances)
	{
		for (int i = 0; i < otherEntrances.Count; i++)
		{
			int num = otherEntrances[i][0];
			int num2 = otherEntrances[i][1];
			int num3 = 150;
			if (Mathf.Abs(num - checkPosX) < num3 && Mathf.Abs(num2 - checkPosY) < num3)
			{
				return true;
			}
		}
		return false;
	}
}
