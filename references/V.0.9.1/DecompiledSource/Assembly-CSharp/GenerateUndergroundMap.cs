using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateUndergroundMap : MonoBehaviour
{
	public static GenerateUndergroundMap generate;

	private BiomMap biomGrass = new BiomMap();

	private BiomMap heightBooshLand = new BiomMap();

	private BiomMap minesMap = new BiomMap();

	private BiomMap minesWalls = new BiomMap();

	private BiomMap undergroundPonds = new BiomMap();

	private BiomMap undergroundForest = new BiomMap();

	private BiomMap undergroundForestHeight = new BiomMap();

	private BiomMap undergroundIronVien = new BiomMap();

	public BiomSpawnTable normalUnderGroundBiome;

	public BiomSpawnTable undergroundPondBiome;

	public BiomSpawnTable undergroundIronVienBiom;

	[Header("Forest Biome Stuff")]
	public BiomSpawnTable undergroundForestBiom;

	public BiomSpawnTable undergroundForestRiver;

	public BiomSpawnTable undergroundIronForestVienBiom;

	public BiomSpawnTable undergroundIronForestVienBiom_Grass;

	public BiomSpawnTable undergroundForestStoneFloorBiome;

	[Header("Lava Biome Stuff")]
	public BiomSpawnTable undergroundLavaBiome;

	public BiomSpawnTable islandLavaBiome;

	public bool mineGeneratedToday;

	private List<int[]> multiTiledObjectsPlaceAfterMap = new List<int[]>();

	private MapRand mapRand = new MapRand(0);

	public MeshRenderer mineRoofRen;

	public Material mineRoofDefault;

	public Material mineRoofVines;

	public Material mineRoofLava;

	public GameObject lavaLight;

	public Vector2 entrancePosition;

	public GenerateLargeDungeon largeDungeonCreator;

	[Header("Normal Dungeons")]
	public DungeonScript basicDungeon;

	public DungeonScript basicDungeon2DoorUp;

	public DungeonScript basicDungeon2DoorSide;

	public DungeonScript windingWoodPath;

	public DungeonScript woodPathWithBarrels;

	public DungeonScript barrelRound;

	public DungeonScript batDungeon;

	public DungeonScript rubyDungeon;

	[Header("Forest Dungeons")]
	public DungeonScript forestDungeon;

	public DungeonScript multiRoomForestDunegeon1;

	public DungeonScript multiRoomForestDunegeon2;

	public DungeonScript strangeForestDungeon;

	public DungeonScript emeralForestDungeon;

	public DungeonScript bridgeDungeon;

	public DungeonScript bridgeDungeon2;

	public DungeonScript torchDungeon;

	public DungeonScript ruins1;

	public DungeonScript ruins2;

	public DungeonScript ruins3;

	[Header("Lava Dungeons")]
	public DungeonScript waterCircle1;

	public DungeonScript waterCircle2;

	public DungeonScript waterCircle3;

	private void Awake()
	{
		generate = this;
	}

	private void Start()
	{
	}

	public void setUpMineSeedFirstTime()
	{
		if (NetworkMapSharer.Instance.mineSeed == 0 && NetworkMapSharer.Instance.tomorrowsMineSeed == 0)
		{
			UnityEngine.Random.InitState(Environment.TickCount);
			NetworkMapSharer.Instance.NetworkmineSeed = UnityEngine.Random.Range(-32000, 32000) + UnityEngine.Random.Range(-32000, 32000);
			NetworkMapSharer.Instance.tomorrowsMineSeed = UnityEngine.Random.Range(-32000, 32000) + UnityEngine.Random.Range(-32000, 32000);
		}
	}

	public void generateMineSeedForNewDay()
	{
		NetworkMapSharer.Instance.NetworkmineSeed = NetworkMapSharer.Instance.tomorrowsMineSeed;
		NetworkMapSharer.Instance.tomorrowsMineSeed = UnityEngine.Random.Range(-32000, 32000) + UnityEngine.Random.Range(-32000, 32000);
	}

	public IEnumerator generateNewMinesForDay()
	{
		if (RealWorldTimeLight.time.mineLevel < 0)
		{
			RealWorldTimeLight.time.NetworkmineLevel = 0;
		}
		mapRand = new MapRand(NetworkMapSharer.Instance.mineSeed);
		yield return StartCoroutine(generateMines());
	}

	public IEnumerator generateMineForClient(int mineSeedIn)
	{
		mapRand = new MapRand(mineSeedIn);
		yield return StartCoroutine(generateMines(clientOnConnect: true));
		MapStorer.store.getStoredMineMapForConnect();
	}

	private IEnumerator generateMines(bool clientOnConnect = false)
	{
		MapStorer.store.CreateUnderworldMapArrays();
		if (RealWorldTimeLight.time.mineLevel == 2)
		{
			yield return StartCoroutine(generateLavaMines(clientOnConnect));
		}
		else if (RealWorldTimeLight.time.mineLevel == 1)
		{
			yield return StartCoroutine(generateUndergroundForest(clientOnConnect));
		}
		else
		{
			yield return StartCoroutine(generateNormalMines(clientOnConnect));
		}
	}

	private IEnumerator generateUndergroundForest(bool clientOnConnect = false)
	{
		mineRoofRen.sharedMaterial = mineRoofVines;
		int pause = 0;
		multiTiledObjectsPlaceAfterMap = new List<int[]>();
		biomGrass = new BiomMap(mapRand);
		biomGrass.randomisePosition();
		heightBooshLand = new BiomMap(mapRand);
		heightBooshLand.randomisePosition();
		minesMap = new BiomMap(mapRand);
		minesMap.randomisePosition();
		minesWalls = new BiomMap(mapRand);
		minesWalls.randomisePosition();
		undergroundPonds = new BiomMap(mapRand);
		undergroundPonds.randomisePosition();
		undergroundForest = new BiomMap(mapRand);
		undergroundForest.randomisePosition();
		undergroundForestHeight = new BiomMap(mapRand);
		undergroundForestHeight.randomisePosition();
		undergroundIronVien = new BiomMap(mapRand);
		undergroundIronVien.randomisePosition();
		minesMap.biomWidth = 18f;
		minesWalls.biomWidth = 130f;
		biomGrass.biomWidth = 15f;
		heightBooshLand.biomWidth = 40f;
		undergroundPonds.biomWidth = 213.86665f;
		undergroundPonds.biomScale = 2.35f;
		undergroundIronVien.biomWidth = 80f;
		int xEntrance = -1;
		int yEntrance = -1;
		int mapSize = WorldManager.Instance.GetMapSize();
		MapStorer.store.underWorldChangedMap = new bool[200, 200];
		MapStorer.store.underWorldWaterChangedMap = new bool[200, 200];
		MapStorer.store.underWorldHeightChangedMap = new bool[200, 200];
		for (int y = 0; y < mapSize; y++)
		{
			for (int i = 0; i < mapSize; i++)
			{
				if (WorldManager.Instance.onTileMap[i, y] == 54)
				{
					xEntrance = i;
					yEntrance = y;
					entrancePosition = new Vector2(xEntrance, yEntrance);
				}
				MapStorer.store.underWorldOnTile[i, y] = -1;
				MapStorer.store.underWorldTileType[i, y] = 0;
				MapStorer.store.underWorldOnTileStatus[i, y] = -1;
				MapStorer.store.underWorldTileTypeStatus[i, y] = 9;
				MapStorer.store.underWorldWaterMap[i, y] = false;
				MapStorer.store.underWorldHeight[i, y] = 1;
				MapStorer.store.underWorldTileType[i, y] = 36;
				if (biomGrass.getNoise(i, y) < 0.4f)
				{
					MapStorer.store.underWorldTileType[i, y] = 9;
				}
				if (i <= 5 || y <= 5 || i >= mapSize - 5 || y >= mapSize - 5)
				{
					MapStorer.store.underWorldOnTile[i, y] = 344;
				}
				else if (undergroundPonds.getNoise(i, y) > 0.5f && undergroundPonds.getNoise(i, y) < 0.6f)
				{
					MapStorer.store.underWorldWaterMap[i, y] = true;
					if (undergroundPonds.getNoise(i, y) > 0.51f && undergroundPonds.getNoise(i, y) < 0.59f)
					{
						MapStorer.store.underWorldHeight[i, y] = -2;
					}
					else if (undergroundPonds.getNoise(i, y) > 0.53f && undergroundPonds.getNoise(i, y) < 0.57f)
					{
						MapStorer.store.underWorldHeight[i, y] = -3;
					}
					else
					{
						MapStorer.store.underWorldHeight[i, y] = -1;
					}
					if (MapStorer.store.underWorldHeight[i, y] < 0)
					{
						placeUnderWorldObject(i, y, undergroundForestRiver);
					}
				}
				else if (undergroundIronVien.getNoise(i, y) > 0.4f)
				{
					if (undergroundIronVien.getNoise(i, y) > 0.55f)
					{
						if (biomGrass.getNoise(i, y) < 0.2f)
						{
							MapStorer.store.underWorldTileType[i, y] = 9;
						}
						else
						{
							MapStorer.store.underWorldTileType[i, y] = 36;
						}
						if (undergroundForestHeight.getNoise(i, y) > 0.7f)
						{
							MapStorer.store.underWorldOnTile[i, y] = 508;
						}
						else if (MapStorer.store.underWorldTileType[i, y] == 36)
						{
							placeUnderWorldObject(i, y, undergroundIronForestVienBiom_Grass);
						}
						else
						{
							placeUnderWorldObject(i, y, undergroundIronForestVienBiom);
						}
					}
					else if (mapRand.Range(0, 5) >= 4)
					{
						MapStorer.store.underWorldOnTile[i, y] = 508;
						MapStorer.store.underWorldTileType[i, y] = 9;
					}
				}
				else if (minesMap.getNoise(i, y) < 0.48f)
				{
					if (minesMap.getNoise(i, y) < 0.4f)
					{
						MapStorer.store.underWorldOnTile[i, y] = 508;
					}
					else if (mapRand.Range(0, 5) == 2)
					{
						MapStorer.store.underWorldOnTile[i, y] = 508;
					}
				}
				else if (minesWalls.getNoise(i, y) > 0.55f && (double)minesWalls.getNoise(i, y) < 0.6)
				{
					MapStorer.store.underWorldHeight[i, y] = 1;
					if (mapRand.Range(0, 3) <= 1)
					{
						MapStorer.store.underWorldOnTile[i, y] = 508;
					}
					else
					{
						MapStorer.store.underWorldOnTile[i, y] = -1;
					}
				}
				else if (MapStorer.store.underWorldTileType[i, y] == 9)
				{
					placeUnderWorldObject(i, y, undergroundForestStoneFloorBiome);
					if (undergroundForestHeight.getNoise(i, y) < 0.25f)
					{
						MapStorer.store.underWorldTileType[i, y] = 39;
						MapStorer.store.underWorldTileTypeStatus[i, y] = 9;
					}
				}
				else
				{
					placeUnderWorldObject(i, y, undergroundForestBiom);
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
		for (int j = 0; j < 450; j++)
		{
			int xPos = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
			int yPos = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
			generateForestDungeon(xPos, yPos);
		}
		if (xEntrance != -1 && yEntrance != -1)
		{
			generateMineEntrance(xEntrance, yEntrance);
		}
		for (int k = 0; k < multiTiledObjectsPlaceAfterMap.Count; k++)
		{
			if (WorldManager.Instance.allObjects[multiTiledObjectsPlaceAfterMap[k][2]].checkIfMultiTileObjectCanBePlacedOnStoredMap(multiTiledObjectsPlaceAfterMap[k][0], multiTiledObjectsPlaceAfterMap[k][1], MapStorer.store.underWorldOnTile, MapStorer.store.underWorldHeight, multiTiledObjectsPlaceAfterMap[k][3]))
			{
				WorldManager.Instance.allObjects[multiTiledObjectsPlaceAfterMap[k][2]].placeMultiTiledObjectOnStoredMap(multiTiledObjectsPlaceAfterMap[k][0], multiTiledObjectsPlaceAfterMap[k][1], MapStorer.LoadMapType.Underworld, multiTiledObjectsPlaceAfterMap[k][3]);
			}
		}
		if (clientOnConnect)
		{
			WorldManager.Instance.resetAllChunkChangedMaps();
		}
	}

	private IEnumerator generateLavaMines(bool clientOnConnect = false)
	{
		mineRoofRen.sharedMaterial = mineRoofLava;
		int pause = 0;
		multiTiledObjectsPlaceAfterMap = new List<int[]>();
		biomGrass = new BiomMap(mapRand);
		biomGrass.randomisePosition();
		heightBooshLand = new BiomMap(mapRand);
		heightBooshLand.randomisePosition();
		minesMap = new BiomMap(mapRand);
		minesMap.randomisePosition();
		minesWalls = new BiomMap(mapRand);
		minesWalls.randomisePosition();
		undergroundPonds = new BiomMap(mapRand);
		undergroundPonds.randomisePosition();
		undergroundForest = new BiomMap(mapRand);
		undergroundForest.randomisePosition();
		undergroundForestHeight = new BiomMap(mapRand);
		undergroundForestHeight.randomisePosition();
		undergroundIronVien = new BiomMap(mapRand);
		undergroundIronVien.randomisePosition();
		minesMap.biomWidth = 10f;
		undergroundForestHeight.biomWidth = 10f;
		minesWalls.biomWidth = 130f;
		biomGrass.biomWidth = 10f;
		heightBooshLand.biomWidth = 15f;
		undergroundPonds.biomWidth = 213.86665f;
		undergroundPonds.biomScale = 2.35f;
		undergroundIronVien.biomWidth = 80f;
		int xEntrance = -1;
		int yEntrance = -1;
		int mapSize = WorldManager.Instance.GetMapSize();
		MapStorer.store.underWorldChangedMap = new bool[200, 200];
		MapStorer.store.underWorldWaterChangedMap = new bool[200, 200];
		MapStorer.store.underWorldHeightChangedMap = new bool[200, 200];
		for (int y = 0; y < mapSize; y++)
		{
			for (int i = 0; i < mapSize; i++)
			{
				if (WorldManager.Instance.onTileMap[i, y] == 54)
				{
					xEntrance = i;
					yEntrance = y;
					entrancePosition = new Vector2(xEntrance, yEntrance);
				}
				MapStorer.store.underWorldOnTile[i, y] = -1;
				MapStorer.store.underWorldTileType[i, y] = 58;
				MapStorer.store.underWorldOnTileStatus[i, y] = -1;
				MapStorer.store.underWorldTileTypeStatus[i, y] = 58;
				MapStorer.store.underWorldWaterMap[i, y] = false;
				MapStorer.store.underWorldHeight[i, y] = 1;
				if (biomGrass.getNoise(i, y) <= 0.3f || biomGrass.getNoise(i, y) > 0.8f)
				{
					MapStorer.store.underWorldTileType[i, y] = 59;
				}
				if (i <= 5 || y <= 5 || i >= mapSize - 5 || y >= mapSize - 5)
				{
					MapStorer.store.underWorldOnTile[i, y] = 344;
				}
				else if (undergroundPonds.getNoise(i, y) > 0.46f && undergroundPonds.getNoise(i, y) < 0.64f)
				{
					if (minesMap.getNoise(i, y) >= 0.7f)
					{
						placeUnderWorldObject(i, y, islandLavaBiome);
						continue;
					}
					if (undergroundForestHeight.getNoise(i, y) >= 0.7f)
					{
						MapStorer.store.underWorldHeight[i, y] = 0;
						continue;
					}
					MapStorer.store.underWorldHeight[i, y] = -2;
					MapStorer.store.underWorldOnTile[i, y] = 881;
					if (mapRand.Range(0, 300) == 2)
					{
						MapStorer.store.underWorldHeight[i, y] = 8;
					}
				}
				else if (undergroundIronVien.getNoise(i, y) > 0.4f)
				{
					if (heightBooshLand.getNoise(i, y) > 0.45f && heightBooshLand.getNoise(i, y) < 0.65f)
					{
						MapStorer.store.underWorldHeight[i, y] = 0;
						MapStorer.store.underWorldOnTile[i, y] = 881;
					}
					else if (undergroundIronVien.getNoise(i, y) > 0.55f)
					{
						biomGrass.getNoise(i, y);
						_ = 0.2f;
						placeUnderWorldObject(i, y, undergroundLavaBiome);
					}
					else if (mapRand.Range(0, 5) >= 4)
					{
						MapStorer.store.underWorldOnTile[i, y] = 880;
					}
				}
				else if (minesMap.getNoise(i, y) < 0.6f)
				{
					if (undergroundForestHeight.getNoise(i, y) >= 0.7f)
					{
						MapStorer.store.underWorldHeight[i, y] = 2;
						placeUnderWorldObject(i, y, islandLavaBiome);
					}
					else
					{
						MapStorer.store.underWorldHeight[i, y] = -2;
						MapStorer.store.underWorldOnTile[i, y] = 881;
					}
				}
				else if (minesWalls.getNoise(i, y) > 0.55f && (double)minesWalls.getNoise(i, y) < 0.6)
				{
					MapStorer.store.underWorldHeight[i, y] = 1;
					if (mapRand.Range(0, 3) <= 1)
					{
						MapStorer.store.underWorldOnTile[i, y] = 880;
					}
					else
					{
						MapStorer.store.underWorldOnTile[i, y] = -1;
					}
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
		for (int j = 0; j < 200; j++)
		{
			int xPos = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
			int yPos = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
			generateLavaDungeon(xPos, yPos);
		}
		if (NetworkMapSharer.Instance.isServer)
		{
			int num = 10;
			int num2 = 18000;
			List<int[]> list = new List<int[]>();
			while (num >= 0)
			{
				int num3 = UnityEngine.Random.Range(100, WorldManager.Instance.GetMapSize() - 100);
				int num4 = UnityEngine.Random.Range(100, WorldManager.Instance.GetMapSize() - 100);
				num3 = Mathf.RoundToInt((float)num3 / 16f) * 16;
				num4 = Mathf.RoundToInt((float)num4 / 16f) * 16;
				if (Mathf.Abs(num3 - xEntrance) > 80 && Mathf.Abs(num4 - yEntrance) > 80 && largeDungeonCreator.CreateDungeon(num3, num4, list))
				{
					list.Add(new int[2] { num3, num4 });
					num--;
				}
				num2--;
				if (num2 <= 0)
				{
					break;
				}
			}
		}
		if (xEntrance != -1 && yEntrance != -1)
		{
			generateMineEntrance(xEntrance, yEntrance);
		}
		for (int k = 0; k < multiTiledObjectsPlaceAfterMap.Count; k++)
		{
			if (WorldManager.Instance.allObjects[multiTiledObjectsPlaceAfterMap[k][2]].checkIfMultiTileObjectCanBePlacedOnStoredMap(multiTiledObjectsPlaceAfterMap[k][0], multiTiledObjectsPlaceAfterMap[k][1], MapStorer.store.underWorldOnTile, MapStorer.store.underWorldHeight, multiTiledObjectsPlaceAfterMap[k][3]))
			{
				WorldManager.Instance.allObjects[multiTiledObjectsPlaceAfterMap[k][2]].placeMultiTiledObjectOnStoredMap(multiTiledObjectsPlaceAfterMap[k][0], multiTiledObjectsPlaceAfterMap[k][1], MapStorer.LoadMapType.Underworld, multiTiledObjectsPlaceAfterMap[k][3]);
			}
		}
		if (clientOnConnect)
		{
			WorldManager.Instance.resetAllChunkChangedMaps();
		}
	}

	private IEnumerator generateNormalMines(bool clientOnConnect = false)
	{
		int pause = 0;
		multiTiledObjectsPlaceAfterMap = new List<int[]>();
		biomGrass = new BiomMap(mapRand);
		biomGrass.randomisePosition();
		heightBooshLand = new BiomMap(mapRand);
		heightBooshLand.randomisePosition();
		minesMap = new BiomMap(mapRand);
		minesMap.randomisePosition();
		minesWalls = new BiomMap(mapRand);
		minesWalls.randomisePosition();
		undergroundPonds = new BiomMap(mapRand);
		undergroundPonds.randomisePosition();
		undergroundForest = new BiomMap(mapRand);
		undergroundForest.randomisePosition();
		undergroundForestHeight = new BiomMap(mapRand);
		undergroundForestHeight.randomisePosition();
		undergroundIronVien = new BiomMap(mapRand);
		undergroundIronVien.randomisePosition();
		minesMap.biomWidth = 15f;
		minesWalls.biomWidth = 25f;
		biomGrass.biomWidth = 4f;
		heightBooshLand.biomWidth = 20f;
		undergroundPonds.biomWidth = 25f;
		undergroundIronVien.biomWidth = 10f;
		int xEntrance = -1;
		int yEntrance = -1;
		int mapSize = WorldManager.Instance.GetMapSize();
		MapStorer.store.underWorldChangedMap = new bool[200, 200];
		MapStorer.store.underWorldWaterChangedMap = new bool[200, 200];
		MapStorer.store.underWorldHeightChangedMap = new bool[200, 200];
		for (int y = 0; y < mapSize; y++)
		{
			for (int i = 0; i < mapSize; i++)
			{
				if (WorldManager.Instance.onTileMap[i, y] == 54)
				{
					xEntrance = i;
					yEntrance = y;
					entrancePosition = new Vector2(xEntrance, yEntrance);
				}
				MapStorer.store.underWorldOnTile[i, y] = -1;
				MapStorer.store.underWorldTileType[i, y] = 0;
				MapStorer.store.underWorldOnTileStatus[i, y] = -1;
				MapStorer.store.underWorldTileTypeStatus[i, y] = 9;
				MapStorer.store.underWorldWaterMap[i, y] = false;
				MapStorer.store.underWorldHeight[i, y] = 1;
				MapStorer.store.underWorldTileType[i, y] = 10;
				if (biomGrass.getNoise(i, y) < 0.4f)
				{
					MapStorer.store.underWorldTileType[i, y] = 9;
				}
				if (i <= 5 || y <= 5 || i >= mapSize - 5 || y >= mapSize - 5)
				{
					MapStorer.store.underWorldOnTile[i, y] = 344;
				}
				else if (undergroundPonds.getNoise(i, y) > 0.7f)
				{
					MapStorer.store.underWorldWaterMap[i, y] = true;
					if (undergroundPonds.getNoise(i, y) > 0.75f)
					{
						MapStorer.store.underWorldHeight[i, y] = -1;
					}
					else if (undergroundPonds.getNoise(i, y) > 0.85f)
					{
						MapStorer.store.underWorldHeight[i, y] = -2;
					}
					else if (undergroundPonds.getNoise(i, y) > 0.91f)
					{
						placeUnderWorldObject(i, y, undergroundIronVienBiom);
						MapStorer.store.underWorldWaterMap[i, y] = false;
					}
					else
					{
						MapStorer.store.underWorldHeight[i, y] = 0;
					}
					if (MapStorer.store.underWorldHeight[i, y] < 0)
					{
						placeUnderWorldObject(i, y, undergroundPondBiome);
					}
				}
				else if (undergroundIronVien.getNoise(i, y) > 0.75f)
				{
					if (undergroundIronVien.getNoise(i, y) > 0.8f)
					{
						placeUnderWorldObject(i, y, undergroundIronVienBiom);
					}
					else if (mapRand.Range(0, 5) >= 4)
					{
						MapStorer.store.underWorldOnTile[i, y] = 29;
						MapStorer.store.underWorldTileType[i, y] = 9;
					}
				}
				else if (minesMap.getNoise(i, y) < 0.48f)
				{
					if (minesMap.getNoise(i, y) < 0.4f)
					{
						MapStorer.store.underWorldOnTile[i, y] = 29;
					}
					else if (mapRand.Range(0, 5) == 2)
					{
						MapStorer.store.underWorldOnTile[i, y] = 29;
					}
				}
				else if (minesWalls.getNoise(i, y) > 0.55f && (double)minesWalls.getNoise(i, y) < 0.6)
				{
					MapStorer.store.underWorldHeight[i, y] = 1;
					if (mapRand.Range(0, 3) <= 1)
					{
						MapStorer.store.underWorldOnTile[i, y] = 29;
					}
					else
					{
						MapStorer.store.underWorldOnTile[i, y] = -1;
					}
				}
				else
				{
					placeUnderWorldObject(i, y, normalUnderGroundBiome);
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
		for (int j = 0; j < 450; j++)
		{
			int xPos = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
			int yPos = mapRand.Range(100, WorldManager.Instance.GetMapSize() - 100);
			generateRoundDungeon(xPos, yPos);
		}
		if (xEntrance != -1 && yEntrance != -1)
		{
			generateMineEntrance(xEntrance, yEntrance);
		}
		for (int k = 0; k < multiTiledObjectsPlaceAfterMap.Count; k++)
		{
			if (WorldManager.Instance.allObjects[multiTiledObjectsPlaceAfterMap[k][2]].checkIfMultiTileObjectCanBePlacedOnStoredMap(multiTiledObjectsPlaceAfterMap[k][0], multiTiledObjectsPlaceAfterMap[k][1], MapStorer.store.underWorldOnTile, MapStorer.store.underWorldHeight, multiTiledObjectsPlaceAfterMap[k][3]))
			{
				WorldManager.Instance.allObjects[multiTiledObjectsPlaceAfterMap[k][2]].placeMultiTiledObjectOnStoredMap(multiTiledObjectsPlaceAfterMap[k][0], multiTiledObjectsPlaceAfterMap[k][1], MapStorer.LoadMapType.Underworld, multiTiledObjectsPlaceAfterMap[k][3]);
			}
		}
		if (clientOnConnect)
		{
			WorldManager.Instance.resetAllChunkChangedMaps();
		}
	}

	public void generateMineEntrance(int xPos, int yPos)
	{
		for (int i = -10; i < 10; i++)
		{
			for (int j = -10; j < 10; j++)
			{
				float num = 10f;
				float num2 = (num - (float)(j + 10)) * (num - (float)(j + 10));
				float num3 = (num - (float)(i + 10)) * (num - (float)(i + 10));
				if (Mathf.Sqrt(num2 + num3) / num < 0.8f)
				{
					MapStorer.store.underWorldOnTile[xPos + j, yPos + i] = -1;
					MapStorer.store.underWorldHeight[xPos + j, yPos + i] = 1;
				}
				if (NetworkMapSharer.Instance.isServer)
				{
					int num4 = Mathf.RoundToInt((xPos + j) / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
					int num5 = Mathf.RoundToInt((yPos + i) / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
					MapStorer.store.underWorldChangedMap[num4 / WorldManager.Instance.getChunkSize(), num5 / WorldManager.Instance.getChunkSize()] = true;
					MapStorer.store.underworldOnTileChangedMap[num4 / WorldManager.Instance.getChunkSize(), num5 / WorldManager.Instance.getChunkSize()] = true;
					MapStorer.store.underWorldHeightChangedMap[num4 / WorldManager.Instance.getChunkSize(), num5 / WorldManager.Instance.getChunkSize()] = true;
					MapStorer.store.underworldTileTypeChangedMap[num4 / WorldManager.Instance.getChunkSize(), num5 / WorldManager.Instance.getChunkSize()] = true;
				}
			}
		}
		WorldManager.Instance.allObjects[55].placeMultiTiledObjectOnStoredMap(xPos, yPos, MapStorer.LoadMapType.Underworld, WorldManager.Instance.rotationMap[xPos, yPos]);
		if (WorldManager.Instance.rotationMap[xPos, yPos] == 1 || WorldManager.Instance.rotationMap[xPos, yPos] == 3)
		{
			WorldManager.Instance.allObjects[891].placeMultiTiledObjectOnStoredMap(xPos - 3, yPos, MapStorer.LoadMapType.Underworld, WorldManager.Instance.rotationMap[xPos, yPos]);
			StartCoroutine(DelayStoneCrusherFloor(xPos - 3, yPos));
		}
		else if (WorldManager.Instance.rotationMap[xPos, yPos] == 2 || WorldManager.Instance.rotationMap[xPos, yPos] == 4)
		{
			WorldManager.Instance.allObjects[891].placeMultiTiledObjectOnStoredMap(xPos, yPos - 3, MapStorer.LoadMapType.Underworld, WorldManager.Instance.rotationMap[xPos, yPos]);
			StartCoroutine(DelayStoneCrusherFloor(xPos, yPos - 3));
		}
	}

	private IEnumerator DelayStoneCrusherFloor(int xPos, int yPos)
	{
		while (!RealWorldTimeLight.time.underGround)
		{
			yield return null;
		}
		FarmAnimalManager.manage.placeNavmeshOnHouseFloor(xPos, yPos);
	}

	public void TurnOnSpecialUndergroundEffects(bool underground, int mineLevel)
	{
		if (mineLevel == 2)
		{
			lavaLight.SetActive(underground);
		}
	}

	public void generateRandomDungeon(int xPos, int yPos)
	{
		for (int i = 0; i < 9; i++)
		{
			for (int j = 0; j < 9; j++)
			{
				if (MapStorer.store.underWorldTileType[xPos + j, yPos + i] == 11)
				{
					return;
				}
			}
		}
		for (int k = 0; k < 9; k++)
		{
			for (int l = 0; l < 9; l++)
			{
				if (NetworkMapSharer.Instance.isServer)
				{
					int num = Mathf.RoundToInt((xPos + l) / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
					int num2 = Mathf.RoundToInt((yPos + k) / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
					MapStorer.store.underWorldChangedMap[num / WorldManager.Instance.getChunkSize(), num2 / WorldManager.Instance.getChunkSize()] = true;
					MapStorer.store.underWorldHeightChangedMap[num / WorldManager.Instance.getChunkSize(), num2 / WorldManager.Instance.getChunkSize()] = true;
				}
				MapStorer.store.underWorldHeight[xPos + l, yPos + k] = 1;
				if (l == 0 || k == 0 || l == 8 || k == 8)
				{
					if (k == 4 || l == 4)
					{
						MapStorer.store.underWorldOnTile[xPos + l, yPos + k] = 186;
						MapStorer.store.underWorldOnTileStatus[xPos + l, yPos + k] = 0;
						MapStorer.store.underWorldRotationMap[xPos + l, yPos + k] = 1;
					}
					else
					{
						MapStorer.store.underWorldOnTile[xPos + l, yPos + k] = 185;
					}
				}
				else if ((l == 1 && k == 1) || (l == 7 && k == 1) || (l == 1 && k == 7) || (l == 1 && k == 7))
				{
					MapStorer.store.underWorldOnTile[xPos + l, yPos + k] = 28;
				}
				else
				{
					MapStorer.store.underWorldOnTile[xPos + l, yPos + k] = -1;
				}
				MapStorer.store.underWorldTileType[xPos + l, yPos + k] = 11;
				MapStorer.store.underWorldTileTypeStatus[xPos + l, yPos + k] = 10;
			}
		}
	}

	public void generateForestDungeon(int xPos, int yPos)
	{
		int[,] array = new int[0, 0];
		int num = mapRand.Range(0, 17);
		int num2 = 38;
		if (num <= 1)
		{
			array = forestDungeon.convertTo2dArray();
		}
		else if (num <= 3)
		{
			array = forestDungeon.convertTo2dArray();
		}
		else if (num > 5)
		{
			array = ((num <= 7) ? bridgeDungeon.convertTo2dArray() : ((num <= 9) ? bridgeDungeon2.convertTo2dArray() : ((num <= 10) ? ruins1.convertTo2dArray() : ((num == 11) ? emeralForestDungeon.convertTo2dArray() : ((num <= 13) ? ruins2.convertTo2dArray() : ((num > 15) ? strangeForestDungeon.convertTo2dArray() : ruins3.convertTo2dArray()))))));
		}
		else
		{
			num2 = 39;
			array = torchDungeon.convertTo2dArray();
		}
		for (int i = 0; i < array.GetLength(1); i++)
		{
			for (int j = 0; j < array.GetLength(0); j++)
			{
				if (MapStorer.store.underWorldTileType[xPos + j, yPos + i] == 38)
				{
					return;
				}
			}
		}
		for (int k = 0; k < array.GetLength(1); k++)
		{
			for (int l = 0; l < array.GetLength(0); l++)
			{
				if (NetworkMapSharer.Instance.isServer)
				{
					int num3 = Mathf.RoundToInt((xPos + l) / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
					int num4 = Mathf.RoundToInt((yPos + k) / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
					MapStorer.store.underWorldChangedMap[num3 / WorldManager.Instance.getChunkSize(), num4 / WorldManager.Instance.getChunkSize()] = true;
					MapStorer.store.underWorldHeightChangedMap[num3 / WorldManager.Instance.getChunkSize(), num4 / WorldManager.Instance.getChunkSize()] = true;
				}
				int num5 = array[l, k];
				if (num5 != -3)
				{
					if (num5 == -1 && UnityEngine.Random.Range(0, 6) == 3)
					{
						num5 = 503;
					}
					MapStorer.store.underWorldHeight[xPos + l, yPos + k] = 1;
					MapStorer.store.underWorldOnTile[xPos + l, yPos + k] = num5;
					if (NetworkMapSharer.Instance.isServer && num5 == 200)
					{
						ContainerManager.manage.generateUndergroundChest(xPos + l, yPos + k, ContainerManager.manage.undergroundCrateTable);
						MapStorer.store.underWorldRotationMap[xPos + l, yPos + k] = UnityEngine.Random.Range(1, 5);
					}
					if (NetworkMapSharer.Instance.isServer && num5 == 425)
					{
						MapStorer.store.underWorldOnTileStatus[xPos + l, yPos + k] = 0;
						ContainerManager.manage.generateUndergroundChest(xPos + l, yPos + k, ContainerManager.manage.undergroundForestChestTable);
						MapStorer.store.underWorldRotationMap[xPos + l, yPos + k] = UnityEngine.Random.Range(1, 5);
					}
					MapStorer.store.underWorldTileType[xPos + l, yPos + k] = num2;
					MapStorer.store.underWorldTileTypeStatus[xPos + l, yPos + k] = 9;
					if (num5 == 563)
					{
						MapStorer.store.underWorldOnTileStatus[xPos + l, yPos + k] = 0;
						MapStorer.store.underWorldRotationMap[xPos + l, yPos + k] = 1;
					}
					if (num5 == 364 || num5 == 435)
					{
						MapStorer.store.underWorldRotationMap[xPos + l, yPos + k] = UnityEngine.Random.Range(1, 5);
					}
				}
			}
		}
	}

	public void generateLavaDungeon(int xPos, int yPos)
	{
		int[,] array = new int[0, 0];
		int num = mapRand.Range(0, 3);
		int num2 = 60;
		switch (num)
		{
		case 0:
			array = waterCircle1.convertTo2dArray();
			break;
		case 1:
			array = waterCircle2.convertTo2dArray();
			break;
		case 2:
			array = waterCircle3.convertTo2dArray();
			break;
		}
		for (int i = 0; i < array.GetLength(1); i++)
		{
			for (int j = 0; j < array.GetLength(0); j++)
			{
				if (NetworkMapSharer.Instance.isServer)
				{
					int num3 = Mathf.RoundToInt((xPos + j) / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
					int num4 = Mathf.RoundToInt((yPos + i) / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
					MapStorer.store.underWorldChangedMap[num3 / WorldManager.Instance.getChunkSize(), num4 / WorldManager.Instance.getChunkSize()] = true;
					MapStorer.store.underWorldHeightChangedMap[num3 / WorldManager.Instance.getChunkSize(), num4 / WorldManager.Instance.getChunkSize()] = true;
					MapStorer.store.underworldOnTileChangedMap[num3 / WorldManager.Instance.getChunkSize(), num4 / WorldManager.Instance.getChunkSize()] = true;
					MapStorer.store.underworldTileTypeChangedMap[num3 / WorldManager.Instance.getChunkSize(), num4 / WorldManager.Instance.getChunkSize()] = true;
				}
				int num5 = array[j, i];
				if (num5 != -3)
				{
					MapStorer.store.underWorldHeight[xPos + j, yPos + i] = 1;
					MapStorer.store.underWorldOnTile[xPos + j, yPos + i] = num5;
					if (NetworkMapSharer.Instance.isServer && num5 == 425)
					{
						MapStorer.store.underWorldOnTileStatus[xPos + j, yPos + i] = 0;
						ContainerManager.manage.generateUndergroundChest(xPos + j, yPos + i, ContainerManager.manage.undergroundForestChestTable);
						MapStorer.store.underWorldRotationMap[xPos + j, yPos + i] = UnityEngine.Random.Range(1, 5);
					}
					MapStorer.store.underWorldTileType[xPos + j, yPos + i] = num2;
				}
			}
		}
	}

	public void generateRoundDungeon(int xPos, int yPos)
	{
		int[,] array = new int[0, 0];
		int num = 11;
		int num2 = mapRand.Range(0, 23);
		if (num2 == 1)
		{
			array = basicDungeon.convertTo2dArray();
		}
		else if (num2 <= 8)
		{
			num = 10;
			array = new int[1, 1] { { -1 } };
		}
		else if (num2 <= 10)
		{
			num = 10;
			array = new int[3, 5]
			{
				{ -1, -1, -1, -1, -1 },
				{ -1, 57, -1, -1, -1 },
				{ -1, -1, -1, 57, -1 }
			};
		}
		else if (num2 <= 11)
		{
			num = 10;
			array = new int[3, 5]
			{
				{ -1, -1, -1, 119, -1 },
				{ -1, -1, 119, -1, -1 },
				{ -1, -1, -1, -1, -1 }
			};
		}
		else
		{
			switch (num2)
			{
			case 12:
				array = basicDungeon2DoorSide.convertTo2dArray();
				break;
			case 13:
				array = basicDungeon2DoorUp.convertTo2dArray();
				break;
			case 14:
				num = 16;
				array = woodPathWithBarrels.convertTo2dArray();
				break;
			case 15:
				num = 16;
				array = windingWoodPath.convertTo2dArray();
				break;
			case 16:
				num = 16;
				array = barrelRound.convertTo2dArray();
				array = getRandomDungeonRot(array);
				break;
			case 18:
				num = 10;
				array = new int[1, 1] { { 317 } };
				break;
			case 19:
				num = 11;
				array = batDungeon.convertTo2dArray();
				array = getRandomDungeonRot(array);
				break;
			case 20:
				num = 11;
				array = rubyDungeon.convertTo2dArray();
				array = getRandomDungeonRot(array);
				break;
			default:
				num = 10;
				array = new int[2, 1]
				{
					{ -1 },
					{ -1 }
				};
				break;
			}
		}
		for (int i = 0; i < array.GetLength(1); i++)
		{
			for (int j = 0; j < array.GetLength(0); j++)
			{
				if (MapStorer.store.underWorldTileType[xPos + j, yPos + i] == 11 || MapStorer.store.underWorldTileType[xPos + j, yPos + i] == 16)
				{
					return;
				}
			}
		}
		for (int k = 0; k < array.GetLength(1); k++)
		{
			for (int l = 0; l < array.GetLength(0); l++)
			{
				if (NetworkMapSharer.Instance.isServer)
				{
					int num3 = Mathf.RoundToInt((xPos + l) / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
					int num4 = Mathf.RoundToInt((yPos + k) / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
					MapStorer.store.underWorldChangedMap[num3 / WorldManager.Instance.getChunkSize(), num4 / WorldManager.Instance.getChunkSize()] = true;
					MapStorer.store.underWorldHeightChangedMap[num3 / WorldManager.Instance.getChunkSize(), num4 / WorldManager.Instance.getChunkSize()] = true;
				}
				int num5 = array[l, k];
				if (num5 != -3)
				{
					MapStorer.store.underWorldHeight[xPos + l, yPos + k] = 1;
					MapStorer.store.underWorldOnTile[xPos + l, yPos + k] = num5;
					if (NetworkMapSharer.Instance.isServer && num5 == 200)
					{
						MapStorer.store.underWorldOnTileStatus[xPos + l, yPos + k] = 0;
						ContainerManager.manage.generateUndergroundChest(xPos + l, yPos + k, ContainerManager.manage.undergroundCrateTable);
						MapStorer.store.underWorldRotationMap[xPos + l, yPos + k] = UnityEngine.Random.Range(1, 5);
					}
					MapStorer.store.underWorldTileType[xPos + l, yPos + k] = num;
					MapStorer.store.underWorldTileTypeStatus[xPos + l, yPos + k] = 10;
					if (num5 == 186)
					{
						MapStorer.store.underWorldOnTileStatus[xPos + l, yPos + k] = 0;
						MapStorer.store.underWorldRotationMap[xPos + l, yPos + k] = 1;
					}
					if (num5 == 364 || num5 == 435)
					{
						MapStorer.store.underWorldRotationMap[xPos + l, yPos + k] = UnityEngine.Random.Range(1, 5);
					}
				}
			}
		}
	}

	public void placeUnderWorldObject(int xPos, int yPos, BiomSpawnTable spawnFrom)
	{
		int biomObject = spawnFrom.getBiomObject(mapRand);
		if (biomObject == -1 || MapStorer.store.underWorldOnTile[xPos, yPos] != -1)
		{
			return;
		}
		if ((bool)WorldManager.Instance.allObjects[biomObject].tileObjectGrowthStages)
		{
			MapStorer.store.underWorldOnTileStatus[xPos, yPos] = WorldManager.Instance.allObjects[biomObject].tileObjectGrowthStages.objectStages.Length - 1;
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
				MapStorer.store.underWorldOnTile[xPos, yPos] = -1;
			}
		}
		else
		{
			MapStorer.store.underWorldOnTile[xPos, yPos] = biomObject;
		}
	}

	private int[,] getRandomDungeonRot(int[,] dungeonArray)
	{
		if (mapRand.Range(1, 4) < 2)
		{
			int[,] array = new int[dungeonArray.GetLength(1), dungeonArray.GetLength(0)];
			for (int i = 0; i < array.GetLength(1); i++)
			{
				for (int j = 0; j < array.GetLength(0); j++)
				{
					array[j, i] = dungeonArray[i, j];
				}
			}
			return array;
		}
		return dungeonArray;
	}
}
