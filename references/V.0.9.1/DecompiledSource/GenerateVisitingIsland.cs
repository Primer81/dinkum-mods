using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateVisitingIsland : MonoBehaviour
{
	public static GenerateVisitingIsland Instance;

	public bool offIslandGeneratedToday;

	private BiomMap islandBiomeMap = new BiomMap();

	private BiomMap secondIslandBiomeMap = new BiomMap();

	private BiomMap reefObjectsMap = new BiomMap();

	private BiomMap coralMap = new BiomMap();

	private BiomMap underWaterCoralHeight = new BiomMap();

	private BiomMap underWaterHeight = new BiomMap();

	public BiomSpawnTable islandSpawnObjects;

	public BiomSpawnTable highIslandSpawnObjects;

	public BiomSpawnTable underWaterReefObjects;

	public BiomSpawnTable rockyOceanObjects;

	public BiomSpawnTable rockyBeachObjects;

	public BiomSpawnTable beachObjects;

	private List<int[]> multiTiledObjectsPlaceAfterMap = new List<int[]>();

	private MapRand mapRand = new MapRand(0);

	public TileObject shipWreck;

	public TileObject coolerChest;

	public TileObject sharkStatue;

	public TileObject normalSharkStatue;

	public TileObject clothesLine;

	public InventoryItemLootTable coolerLootTable;

	public Vector2 entrancePosition;

	private List<Vector2> sharkStatueLocations = new List<Vector2>();

	private int sharkStatuesUsed;

	private void Awake()
	{
		Instance = this;
	}

	public IEnumerator GenerateReefIslands(bool clientOnConnect = false)
	{
		sharkStatueLocations.Clear();
		sharkStatuesUsed = 0;
		MapStorer.store.CreateOffIslandMapArrays();
		int pause = 0;
		mapRand = new MapRand(NetworkMapSharer.Instance.mineSeed);
		multiTiledObjectsPlaceAfterMap = new List<int[]>();
		islandBiomeMap = new BiomMap(mapRand);
		islandBiomeMap.randomisePosition();
		islandBiomeMap.biomWidth = 35f;
		islandBiomeMap.biomScale = 2.55f;
		secondIslandBiomeMap = new BiomMap(mapRand);
		secondIslandBiomeMap.randomisePosition();
		secondIslandBiomeMap.biomWidth = 89f;
		secondIslandBiomeMap.biomScale = 2.55f;
		reefObjectsMap = new BiomMap(mapRand);
		reefObjectsMap.randomisePosition();
		coralMap = new BiomMap(mapRand);
		coralMap.randomisePosition();
		coralMap.biomWidth = 25f;
		underWaterHeight = new BiomMap(mapRand);
		underWaterHeight.randomisePosition();
		underWaterHeight.biomWidth = 25f;
		underWaterCoralHeight = new BiomMap(mapRand);
		underWaterCoralHeight.randomisePosition();
		underWaterCoralHeight.biomWidth = 25f;
		int xEntrance = -1;
		int yEntrance = -1;
		int mapSize = WorldManager.Instance.GetMapSize();
		MapStorer.store.offIslandChangedMap = new bool[200, 200];
		MapStorer.store.offIslandWaterChangedMap = new bool[200, 200];
		MapStorer.store.offIslandHeightChangedMap = new bool[200, 200];
		int halfMapSize = mapSize / 2;
		for (int y = 0; y < mapSize; y++)
		{
			for (int i = 0; i < mapSize; i++)
			{
				float distanceToCentre = GenerateMap.generate.getDistanceToCentre(i, y, halfMapSize, halfMapSize);
				if (WorldManager.Instance.onTileMap[i, y] == 43)
				{
					xEntrance = i;
					yEntrance = y;
					entrancePosition = new Vector2(xEntrance, yEntrance);
				}
				MapStorer.store.offIslandOnTile[i, y] = -1;
				MapStorer.store.offIslandTileType[i, y] = 3;
				MapStorer.store.offIslandOnTileStatus[i, y] = -1;
				MapStorer.store.offIslandTileTypeStatus[i, y] = 0;
				MapStorer.store.offIslandWaterMap[i, y] = true;
				MapStorer.store.offIslandHeight[i, y] = -4;
				float num = islandBiomeMap.getNoise(i, y) - distanceToCentre;
				float num2 = secondIslandBiomeMap.getNoise(i, y) - distanceToCentre;
				if (num > 0.4f || num2 > 0.45f)
				{
					if (num > 0.68f || num2 > 0.65f)
					{
						MapStorer.store.offIslandTileType[i, y] = 4;
						MapStorer.store.offIslandWaterMap[i, y] = false;
						if (num > 0.8f || num2 > 0.8f)
						{
							MapStorer.store.offIslandHeight[i, y] = 4;
							PlaceWorldObject(i, y, highIslandSpawnObjects);
						}
						else if (num > 0.7f || num2 > 0.7f)
						{
							MapStorer.store.offIslandHeight[i, y] = 3;
							PlaceWorldObject(i, y, highIslandSpawnObjects);
						}
						else
						{
							MapStorer.store.offIslandHeight[i, y] = 2;
							PlaceWorldObject(i, y, islandSpawnObjects);
						}
					}
					else if (num > 0.6f || num2 > 0.6f)
					{
						MapStorer.store.offIslandWaterMap[i, y] = false;
						MapStorer.store.offIslandHeight[i, y] = 1;
						if (mapRand.Range(0, 2) == 1)
						{
							PlaceWorldObject(i, y, beachObjects);
						}
						else
						{
							PlaceWorldObject(i, y, rockyBeachObjects);
						}
					}
					else if (num > 0.55f || num2 > 0.55f)
					{
						MapStorer.store.offIslandHeight[i, y] = 0;
						PlaceWorldObject(i, y, rockyBeachObjects);
					}
					else if (num > 0.45f || num2 > 0.45f)
					{
						MapStorer.store.offIslandHeight[i, y] = -1;
						PlaceWorldObject(i, y, rockyOceanObjects);
					}
					else if (num > 0.42f || num2 > 0.42f)
					{
						MapStorer.store.offIslandHeight[i, y] = -2;
						PlaceWorldObject(i, y, rockyOceanObjects);
					}
					else if (num > 0.4f || num2 > 0.4f)
					{
						MapStorer.store.offIslandHeight[i, y] = -3;
						PlaceWorldObject(i, y, rockyOceanObjects);
					}
				}
				else
				{
					PlaceWorldObject(i, y, underWaterReefObjects);
					MapStorer.store.offIslandTileType[i, y] = 41;
					if (underWaterHeight.getNoise(i, y) > 0.85f)
					{
						MapStorer.store.offIslandHeight[i, y] += 3;
					}
					else if (underWaterHeight.getNoise(i, y) > 0.65f)
					{
						MapStorer.store.offIslandHeight[i, y] += 2;
					}
					else if (underWaterHeight.getNoise(i, y) > 0.5f)
					{
						MapStorer.store.offIslandHeight[i, y]++;
					}
				}
				if (i <= 5 || y <= 5 || i >= mapSize - 5 || y >= mapSize - 5)
				{
					MapStorer.store.offIslandHeight[i, y] = -2;
					MapStorer.store.offIslandTileType[i, y] = 3;
					MapStorer.store.offIslandOnTile[i, y] = -1;
				}
			}
			if (pause < 100)
			{
				pause++;
				continue;
			}
			pause = 0;
			yield return null;
		}
		bool flag = false;
		if (xEntrance != -1 && yEntrance != -1)
		{
			GenerateLandedAirShip(xEntrance, yEntrance);
			flag = true;
		}
		if (flag)
		{
			int num3 = mapRand.Range(1, 4);
			for (int j = 0; j < num3; j++)
			{
				PlaceShipWrecksUnderWater();
			}
			int num4 = mapRand.Range(1, 3);
			for (int k = 0; k < num4; k++)
			{
				PlaceClothesLineUnderWater();
			}
			for (int l = 0; l < 10; l++)
			{
				PlaceCoolerOnRandomLand();
			}
			for (int m = 0; m < 2; m++)
			{
				PlaceSharkStatueRandomLand();
			}
			if (sharkStatueLocations.Count < 2)
			{
				PlaceSharkStatueRandomLand(2, 50000);
			}
			for (int n = 0; n < multiTiledObjectsPlaceAfterMap.Count; n++)
			{
				if (MapStorer.store.offIslandHeight[multiTiledObjectsPlaceAfterMap[n][0], multiTiledObjectsPlaceAfterMap[n][1]] <= -1 && WorldManager.Instance.allObjects[multiTiledObjectsPlaceAfterMap[n][2]].checkIfMultiTileObjectCanBePlacedOnStoredMap(multiTiledObjectsPlaceAfterMap[n][0], multiTiledObjectsPlaceAfterMap[n][1], MapStorer.store.offIslandOnTile, MapStorer.store.offIslandHeight, multiTiledObjectsPlaceAfterMap[n][3], placeOverSingleTiledObjects: true))
				{
					WorldManager.Instance.allObjects[multiTiledObjectsPlaceAfterMap[n][2]].placeMultiTiledObjectOnStoredMap(multiTiledObjectsPlaceAfterMap[n][0], multiTiledObjectsPlaceAfterMap[n][1], MapStorer.LoadMapType.OffIsland, multiTiledObjectsPlaceAfterMap[n][3]);
				}
			}
		}
		if (clientOnConnect)
		{
			WorldManager.Instance.resetAllChunkChangedMaps();
		}
	}

	public IEnumerator GenerateOffIslandForClient(int mineSeedIn)
	{
		mapRand = new MapRand(mineSeedIn);
		yield return StartCoroutine(GenerateReefIslands(clientOnConnect: true));
		MapStorer.store.getStoredOffIslandMapForConnect();
	}

	public void GenerateLandedAirShip(int xPos, int yPos)
	{
		for (int i = -25; i < 25; i++)
		{
			for (int j = -25; j < 25; j++)
			{
				float num = 25f;
				float num2 = (num - (float)(j + 25)) * (num - (float)(j + 25));
				float num3 = (num - (float)(i + 25)) * (num - (float)(i + 25));
				float num4 = Mathf.Sqrt(num2 + num3) / num;
				if (NetworkMapSharer.Instance.isServer)
				{
					int num5 = Mathf.RoundToInt((xPos + j) / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
					int num6 = Mathf.RoundToInt((yPos + i) / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
					MapStorer.store.offIslandChangedMap[num5 / WorldManager.Instance.getChunkSize(), num6 / WorldManager.Instance.getChunkSize()] = true;
					MapStorer.store.offIslandOnTileChangedMap[num5 / WorldManager.Instance.getChunkSize(), num6 / WorldManager.Instance.getChunkSize()] = true;
					MapStorer.store.offIslandHeightChangedMap[num5 / WorldManager.Instance.getChunkSize(), num6 / WorldManager.Instance.getChunkSize()] = true;
					MapStorer.store.offIslandTileTypeChangedMap[num5 / WorldManager.Instance.getChunkSize(), num6 / WorldManager.Instance.getChunkSize()] = true;
					MapStorer.store.offIslandWaterChangedMap[num5 / WorldManager.Instance.getChunkSize(), num6 / WorldManager.Instance.getChunkSize()] = true;
				}
				MapStorer.store.offIslandTileType[xPos + j, yPos + i] = 3;
				if (num4 < 0.5f)
				{
					MapStorer.store.offIslandOnTile[xPos + j, yPos + i] = -1;
					MapStorer.store.offIslandHeight[xPos + j, yPos + i] = 2;
					MapStorer.store.offIslandTileType[xPos + j, yPos + i] = 4;
					MapStorer.store.offIslandWaterMap[xPos + j, yPos + i] = false;
					PlaceWorldObject(xPos + j, yPos + i, islandSpawnObjects);
				}
				else if (num4 < 0.6f)
				{
					MapStorer.store.offIslandOnTile[xPos + j, yPos + i] = -1;
					MapStorer.store.offIslandHeight[xPos + j, yPos + i] = 1;
					MapStorer.store.offIslandWaterMap[xPos + j, yPos + i] = false;
				}
				else if (num4 < 0.7f)
				{
					MapStorer.store.offIslandOnTile[xPos + j, yPos + i] = -1;
					MapStorer.store.offIslandHeight[xPos + j, yPos + i] = 0;
					PlaceWorldObject(xPos + j, yPos + i, rockyBeachObjects);
					MapStorer.store.offIslandWaterMap[xPos + j, yPos + i] = true;
				}
				else if (num4 < 0.8f)
				{
					MapStorer.store.offIslandOnTile[xPos + j, yPos + i] = -1;
					MapStorer.store.offIslandHeight[xPos + j, yPos + i] = -1;
					MapStorer.store.offIslandWaterMap[xPos + j, yPos + i] = true;
					PlaceWorldObject(xPos + j, yPos + i, rockyOceanObjects);
				}
				else if (num4 < 0.9f)
				{
					MapStorer.store.offIslandOnTile[xPos + j, yPos + i] = -1;
					MapStorer.store.offIslandHeight[xPos + j, yPos + i] = -2;
					MapStorer.store.offIslandWaterMap[xPos + j, yPos + i] = true;
					PlaceWorldObject(xPos + j, yPos + i, rockyOceanObjects);
				}
			}
		}
		WorldManager.Instance.allObjects[664].placeMultiTiledObjectOnStoredMap(xPos, yPos, MapStorer.LoadMapType.OffIsland, WorldManager.Instance.rotationMap[xPos, yPos]);
	}

	public void PlaceWorldObject(int xPos, int yPos, BiomSpawnTable spawnFrom)
	{
		int biomObject = spawnFrom.getBiomObject(mapRand);
		if (biomObject == -1 || MapStorer.store.offIslandOnTile[xPos, yPos] != -1)
		{
			return;
		}
		if ((bool)WorldManager.Instance.allObjects[biomObject].tileObjectGrowthStages)
		{
			MapStorer.store.offIslandOnTileStatus[xPos, yPos] = WorldManager.Instance.allObjects[biomObject].tileObjectGrowthStages.objectStages.Length - 1;
		}
		if (WorldManager.Instance.allObjects[biomObject].IsMultiTileObject())
		{
			if (xPos > 10 && xPos < WorldManager.Instance.GetMapSize() - 10 && yPos > 10 && yPos < WorldManager.Instance.GetMapSize() - 10)
			{
				int num = mapRand.Range(1, 4);
				multiTiledObjectsPlaceAfterMap.Add(new int[4] { xPos, yPos, biomObject, num });
			}
			else
			{
				MapStorer.store.offIslandOnTile[xPos, yPos] = -1;
			}
		}
		else
		{
			MapStorer.store.offIslandOnTile[xPos, yPos] = biomObject;
		}
	}

	private void PlaceCoolerOnRandomLand()
	{
		int num = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
		int num2 = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
		bool flag = false;
		int num3 = 0;
		while (!flag)
		{
			if (MapStorer.store.offIslandHeight[num, num2] == 1 && MapStorer.store.offIslandOnTile[num, num2] == -1)
			{
				MapStorer.store.offIslandOnTile[num, num2] = coolerChest.tileObjectId;
				MapStorer.store.offIslandOnTileStatus[num, num2] = 0;
				ContainerManager.manage.generateUndergroundChest(num, num2, coolerLootTable, isOffIsland: true);
				int num4 = Mathf.RoundToInt(num / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
				int num5 = Mathf.RoundToInt(num2 / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
				MapStorer.store.offIslandOnTileChangedMap[num4 / WorldManager.Instance.getChunkSize(), num5 / WorldManager.Instance.getChunkSize()] = true;
				MapStorer.store.offIslandChangedMap[num4 / WorldManager.Instance.getChunkSize(), num5 / WorldManager.Instance.getChunkSize()] = true;
				flag = true;
			}
			else
			{
				num = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
				num2 = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
			}
			num3++;
			if (num3 >= 10000)
			{
				break;
			}
		}
	}

	private void PlaceShipWrecksUnderWater()
	{
		int num = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
		int num2 = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
		int rotation = mapRand.Range(1, 4);
		bool flag = false;
		int num3 = 0;
		while (!flag)
		{
			if (MapStorer.store.offIslandHeight[num, num2] <= -3)
			{
				if (WorldManager.Instance.allObjects[shipWreck.tileObjectId].checkIfMultiTileObjectCanBePlacedOnStoredMap(num, num2, MapStorer.store.offIslandOnTile, MapStorer.store.offIslandHeight, rotation, placeOverSingleTiledObjects: true, ignoreHeight: true))
				{
					WorldManager.Instance.allObjects[shipWreck.tileObjectId].placeMultiTiledObjectOnStoredMap(num, num2, MapStorer.LoadMapType.OffIsland, rotation);
					flag = true;
					LevelTerrainUnderMultitiledObject(MapStorer.store.offIslandHeight[num, num2], num, num2, WorldManager.Instance.allObjects[shipWreck.tileObjectId].GetXSize());
					break;
				}
			}
			else
			{
				num = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
				num2 = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
			}
			num3++;
			if (num3 >= 10000)
			{
				break;
			}
		}
	}

	public void LevelTerrainUnderMultitiledObject(int heightToLevelTo, int startingX, int startingY, int size)
	{
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				MapStorer.store.offIslandHeight[startingX + i, startingY + j] = heightToLevelTo;
				int num = Mathf.RoundToInt((startingX + i) / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
				int num2 = Mathf.RoundToInt((startingY + j) / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
				MapStorer.store.offIslandHeightChangedMap[num / WorldManager.Instance.getChunkSize(), num2 / WorldManager.Instance.getChunkSize()] = true;
				MapStorer.store.offIslandChangedMap[num / WorldManager.Instance.getChunkSize(), num2 / WorldManager.Instance.getChunkSize()] = true;
			}
		}
	}

	private void PlaceClothesLineUnderWater()
	{
		int num = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
		int num2 = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
		bool flag = false;
		int num3 = 0;
		while (!flag)
		{
			if (MapStorer.store.offIslandHeight[num, num2] <= -3)
			{
				if (WorldManager.Instance.allObjects[clothesLine.tileObjectId].checkIfMultiTileObjectCanBePlacedOnStoredMap(num, num2, MapStorer.store.offIslandOnTile, MapStorer.store.offIslandHeight, 1, placeOverSingleTiledObjects: true, ignoreHeight: true))
				{
					WorldManager.Instance.allObjects[clothesLine.tileObjectId].placeMultiTiledObjectOnStoredMap(num, num2, MapStorer.LoadMapType.OffIsland, mapRand.Range(1, 4));
					LevelTerrainUnderMultitiledObject(MapStorer.store.offIslandHeight[num, num2], num, num2, WorldManager.Instance.allObjects[clothesLine.tileObjectId].GetXSize());
					flag = true;
					break;
				}
			}
			else
			{
				num = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
				num2 = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
			}
			num3++;
			if (num3 >= 100000)
			{
				break;
			}
		}
	}

	private void PlaceSharkStatueRandomLand(int height = 3, int totalTries = 25000)
	{
		int num = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
		int num2 = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
		bool flag = false;
		int num3 = 0;
		while (!flag)
		{
			if (MapStorer.store.offIslandHeight[num, num2] >= height)
			{
				if (MapStorer.store.offIslandOnTile[num, num2] == -1 || (MapStorer.store.offIslandOnTile[num, num2] >= 0 && !WorldManager.Instance.allObjectSettings[MapStorer.store.offIslandOnTile[num, num2]].isMultiTileObject))
				{
					MapStorer.store.offIslandOnTile[num, num2] = sharkStatue.tileObjectId;
				}
				MapStorer.store.offIslandOnTileStatus[num, num2] = -2;
				sharkStatueLocations.Add(new Vector2(num, num2));
				int num4 = Mathf.RoundToInt(num / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
				int num5 = Mathf.RoundToInt(num2 / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
				MapStorer.store.offIslandOnTileChangedMap[num4 / WorldManager.Instance.getChunkSize(), num5 / WorldManager.Instance.getChunkSize()] = true;
				MapStorer.store.offIslandChangedMap[num4 / WorldManager.Instance.getChunkSize(), num5 / WorldManager.Instance.getChunkSize()] = true;
				flag = true;
			}
			else
			{
				num = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
				num2 = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
			}
			num3++;
			if (num3 >= totalTries)
			{
				MonoBehaviour.print("No Shark placed");
				break;
			}
		}
	}

	public void UseASharkStatue()
	{
		sharkStatuesUsed++;
		if (sharkStatuesUsed >= 20)
		{
			NetworkMapSharer.Instance.TurnOffAllSharkStatues(sharkStatueLocations);
		}
	}
}
