using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimalManager : MonoBehaviour
{
	public static AnimalManager manage;

	public AnimalAI[] allAnimals;

	private List<AnimalAI> animalPool = new List<AnimalAI>();

	public List<Nest> allNests = new List<Nest>();

	public GameObject releasedBug;

	public GameObject releaseFish;

	public UnityEvent lookAtBugBook = new UnityEvent();

	public bool bugBookOpen;

	public UnityEvent lookAtFishBook = new UnityEvent();

	public bool fishBookOpen;

	public List<FencedOffAnimal> fencedOffAnimals = new List<FencedOffAnimal>();

	public List<FencedOffAnimal> alphaAnimals = new List<FencedOffAnimal>();

	public bool crocDay;

	public TileObject crocoBerleyBox;

	public TileObject devilBerleyBox;

	public TileObject sharkBerleyBox;

	public GameObject sittingBugPrefab;

	public TileObject[] bugSitOnTileObjects;

	public LayerMask swimLayerCheck;

	public LayerMask animalGroundedLayer;

	private LayerMask[] defaultLayerMasks;

	private LayerMask[] easyLayerMasks;

	[Header("Fishing Tables --------------")]
	public InventoryLootTableTimeWeatherMaster northernOceanFish;

	public InventoryLootTableTimeWeatherMaster southernOceanFish;

	public InventoryLootTableTimeWeatherMaster riverFish;

	public InventoryLootTableTimeWeatherMaster mangroveFish;

	public InventoryLootTableTimeWeatherMaster billabongFish;

	public InventoryLootTableTimeWeatherMaster undergroundFish;

	public InventoryLootTableTimeWeatherMaster reefIslandFish;

	[Header("Bug Tables --------------")]
	public InventoryLootTableTimeWeatherMaster topicalBugs;

	public InventoryLootTableTimeWeatherMaster desertBugs;

	public InventoryLootTableTimeWeatherMaster bushlandBugs;

	public InventoryLootTableTimeWeatherMaster pineLandBugs;

	public InventoryLootTableTimeWeatherMaster plainsBugs;

	public InventoryLootTableTimeWeatherMaster underGroundBugs;

	public InventoryLootTableTimeWeatherMaster reefIslandBugs;

	[Header("Ocean Creatures Tables --------------")]
	public InventoryLootTableTimeWeatherMaster underWaterOceanCreatures;

	public InventoryLootTableTimeWeatherMaster offIslandUnderWaterCreatures;

	public InventoryLootTableTimeWeatherMaster underWaterRiverCreatures;

	[Header("Animals Spawn by Biome --------------")]
	public AnimalBiomeTable tropicalAnimals;

	public AnimalBiomeTable billabongAnimals;

	public AnimalBiomeTable bushlandAnimals;

	public AnimalBiomeTable desertAnimals;

	public AnimalBiomeTable pineForestAnimals;

	public AnimalBiomeTable plainsAnimals;

	[Header("Underground Animals Spawn by Biome ------")]
	public AnimalBiomeTable undergroundAnimals;

	public List<AnimalChunk> loadedChunks = new List<AnimalChunk>();

	public List<AnimalChunk> loadedUndergroundChunks = new List<AnimalChunk>();

	public List<AnimalChunk> loadedOffIslandChunks = new List<AnimalChunk>();

	public UnityEvent saveFencedOffAnimalsEvent = new UnityEvent();

	private bool changeOverNight;

	private void Awake()
	{
		manage = this;
	}

	private void Start()
	{
		populateFishingTables();
	}

	public AnimalAI spawnFreeAnimal(int animalId, Vector3 spawnPos)
	{
		if (animalId < 0 || animalId >= allAnimals.Length)
		{
			return null;
		}
		AnimalAI animalAI = null;
		Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
		foreach (AnimalAI item in animalPool)
		{
			if (item.animalId == animalId)
			{
				animalAI = item;
				item.transform.position = spawnPos;
				item.transform.rotation = rotation;
				item.gameObject.SetActive(value: true);
				animalPool.Remove(item);
				if (animalId == 1)
				{
					animalAI.GetComponent<FishType>().generateFishForEnviroment();
				}
				return animalAI;
			}
		}
		animalAI = Object.Instantiate(allAnimals[animalId].gameObject, spawnPos, rotation).GetComponent<AnimalAI>();
		animalAI.setUp();
		if (animalId == 1)
		{
			animalAI.GetComponent<FishType>().generateFishForEnviroment();
		}
		return animalAI;
	}

	public void returnAnimalAndSaveToMap(AnimalAI returnMe)
	{
		int xPos = Mathf.RoundToInt(returnMe.transform.position.x / 2f);
		int yPos = Mathf.RoundToInt(returnMe.transform.position.z / 2f);
		safeSaveAnimalOnMap(returnMe, xPos, yPos);
		animalPool.Add(returnMe);
	}

	public void returnAnimalAndDoNotSaveToMap(AnimalAI returnMe)
	{
		animalPool.Add(returnMe);
	}

	private void safeSaveAnimalOnMap(AnimalAI animal, int xPos, int yPos)
	{
		if ((bool)animal.saveAsAlpha)
		{
			saveAnimalToChunk(animal.animalId * 10 + animal.getVariationNo(), xPos, yPos, animal.getSleepPos(), animal.saveAsAlpha.daysRemaining);
		}
		else
		{
			saveAnimalToChunk(animal.animalId * 10 + animal.getVariationNo(), xPos, yPos, animal.getSleepPos());
		}
	}

	public void SaveSittingBugToMap(Vector3 position)
	{
		int xPos = Mathf.RoundToInt(position.x / 2f);
		int yPos = Mathf.RoundToInt(position.z / 2f);
		saveAnimalToChunk(20, xPos, yPos, Vector3.zero);
	}

	public bool checkIfTileIsWalkable(int xPos, int yPos)
	{
		if (!WorldManager.Instance.isPositionOnMap(xPos, yPos))
		{
			return false;
		}
		if (WorldManager.Instance.onTileMap[xPos, yPos] == -1)
		{
			return true;
		}
		if (WorldManager.Instance.onTileMap[xPos, yPos] >= 0 && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].walkable)
		{
			return true;
		}
		return false;
	}

	public string fillAnimalLocation(InventoryItem item)
	{
		if ((bool)item.fish)
		{
			if (northernOceanFish.isBugOrFishInTable(item))
			{
				return northernOceanFish.GetLocationName();
			}
			if (southernOceanFish.isBugOrFishInTable(item))
			{
				return southernOceanFish.GetLocationName();
			}
			if (riverFish.isBugOrFishInTable(item))
			{
				return riverFish.GetLocationName();
			}
			if (mangroveFish.isBugOrFishInTable(item))
			{
				return mangroveFish.GetLocationName();
			}
			if (undergroundFish.isBugOrFishInTable(item))
			{
				return undergroundFish.GetLocationName();
			}
			if (reefIslandFish.isBugOrFishInTable(item))
			{
				return reefIslandFish.GetLocationName();
			}
		}
		if ((bool)item.bug)
		{
			if (bushlandBugs.isBugOrFishInTable(item))
			{
				return bushlandBugs.GetLocationName();
			}
			if (desertBugs.isBugOrFishInTable(item))
			{
				return desertBugs.GetLocationName();
			}
			if (pineLandBugs.isBugOrFishInTable(item))
			{
				return pineLandBugs.GetLocationName();
			}
			if (plainsBugs.isBugOrFishInTable(item))
			{
				return plainsBugs.GetLocationName();
			}
			if (topicalBugs.isBugOrFishInTable(item))
			{
				return plainsBugs.GetLocationName();
			}
			if (underGroundBugs.isBugOrFishInTable(item))
			{
				return underGroundBugs.GetLocationName();
			}
			if (reefIslandBugs.isBugOrFishInTable(item))
			{
				return reefIslandBugs.GetLocationName();
			}
		}
		return ConversationGenerator.generate.GetBiomeNameText("Unknown");
	}

	public string fillAnimalTimeOfDay(InventoryItem item)
	{
		if ((bool)item.fish)
		{
			if (northernOceanFish.isBugOrFishInTable(item))
			{
				return northernOceanFish.getTimeOfDayFound(item);
			}
			if (southernOceanFish.isBugOrFishInTable(item))
			{
				return southernOceanFish.getTimeOfDayFound(item);
			}
			if (riverFish.isBugOrFishInTable(item))
			{
				return riverFish.getTimeOfDayFound(item);
			}
			if (mangroveFish.isBugOrFishInTable(item))
			{
				return mangroveFish.getTimeOfDayFound(item);
			}
		}
		if ((bool)item.bug)
		{
			if (bushlandBugs.isBugOrFishInTable(item))
			{
				return bushlandBugs.getTimeOfDayFound(item);
			}
			if (desertBugs.isBugOrFishInTable(item))
			{
				return desertBugs.getTimeOfDayFound(item);
			}
			if (pineLandBugs.isBugOrFishInTable(item))
			{
				return pineLandBugs.getTimeOfDayFound(item);
			}
			if (plainsBugs.isBugOrFishInTable(item))
			{
				return plainsBugs.getTimeOfDayFound(item);
			}
			if (topicalBugs.isBugOrFishInTable(item))
			{
				return plainsBugs.getTimeOfDayFound(item);
			}
		}
		return "all day";
	}

	public void fillBugPediaEntry(InventoryItem item)
	{
		string text = "";
		if (topicalBugs.isBugOrFishInTable(item) && desertBugs.isBugOrFishInTable(item) && bushlandBugs.isBugOrFishInTable(item) && pineLandBugs.isBugOrFishInTable(item) && plainsBugs.isBugOrFishInTable(item))
		{
			text += capitaliseFirstLetter("Everywhere");
			plainsBugs.getTimeOfDayFound(item);
			plainsBugs.getSeason(item);
		}
		else
		{
			if (topicalBugs.isBugOrFishInTable(item))
			{
				text += capitaliseFirstLetter(topicalBugs.GetLocationName());
				topicalBugs.getTimeOfDayFound(item);
				topicalBugs.getSeason(item);
			}
			if (desertBugs.isBugOrFishInTable(item))
			{
				if (text != "")
				{
					text += " & ";
				}
				text += capitaliseFirstLetter(desertBugs.GetLocationName());
				desertBugs.getTimeOfDayFound(item);
				desertBugs.getSeason(item);
			}
			if (bushlandBugs.isBugOrFishInTable(item))
			{
				if (text != "")
				{
					text += " & ";
				}
				text += capitaliseFirstLetter(bushlandBugs.GetLocationName());
				bushlandBugs.getTimeOfDayFound(item);
				bushlandBugs.getSeason(item);
			}
			if (pineLandBugs.isBugOrFishInTable(item))
			{
				if (text != "")
				{
					text += " & ";
				}
				text += capitaliseFirstLetter(pineLandBugs.GetLocationName());
				pineLandBugs.getTimeOfDayFound(item);
				pineLandBugs.getSeason(item);
			}
			if (plainsBugs.isBugOrFishInTable(item))
			{
				if (text != "")
				{
					text += " & ";
				}
				text += capitaliseFirstLetter(plainsBugs.GetLocationName());
				plainsBugs.getTimeOfDayFound(item);
				plainsBugs.getSeason(item);
			}
		}
		PediaManager.manage.locationText.text = text;
	}

	private string capitaliseFirstLetter(string toChange)
	{
		toChange = char.ToUpper(toChange[0]) + toChange.Substring(1);
		return toChange;
	}

	private void populateChunkNormalUnderground(AnimalChunk toFill, int chunkX, int chunkY)
	{
		for (int i = chunkY; i < chunkY + WorldManager.Instance.getChunkSize(); i++)
		{
			for (int j = chunkX; j < chunkX + WorldManager.Instance.getChunkSize(); j++)
			{
				if (WorldManager.Instance.onTileMap[j, i] != -1)
				{
					continue;
				}
				if (Random.Range(0, 1500) == 2)
				{
					toFill.addAnimalToChunk(200, j, i);
				}
				if (WorldManager.Instance.heightMap[j, i] >= 1 && Random.Range(0, 1600) == 2)
				{
					toFill.addAnimalToChunk(140, j, i);
				}
				if (WorldManager.Instance.heightMap[j, i] >= 1 && Random.Range(0, 200) == 2)
				{
					if (Random.Range(0, 2) == 1)
					{
						toFill.addAnimalToChunk(20, j, i);
					}
					if (Random.Range(0, 5) == 3)
					{
						toFill.addAnimalToChunk(20, j, i);
					}
					if (Random.Range(0, 9) == 7)
					{
						toFill.addAnimalToChunk(20, j, i);
					}
				}
				if (WorldManager.Instance.heightMap[j, i] < 0 && Random.Range(0, 600) == 1)
				{
					toFill.addAnimalToChunk(10, j, i);
				}
				if (Random.Range(0, 800) == 2 && WorldManager.Instance.heightMap[j, i] >= 1)
				{
					toFill.addAnimalToChunk(270, j, i);
					toFill.addAnimalToChunk(270, j, i);
					if (Random.Range(0, 2) == 1)
					{
						toFill.addAnimalToChunk(270, j, i);
					}
					if (Random.Range(0, 5) == 3)
					{
						toFill.addAnimalToChunk(270, j, i);
					}
					if (Random.Range(0, 9) == 7)
					{
						toFill.addAnimalToChunk(270, j, i);
					}
					if (Random.Range(0, 13) == 7)
					{
						toFill.addAnimalToChunk(270, j, i);
					}
					if (Random.Range(0, 20) == 7)
					{
						toFill.addAnimalToChunk(270, j, i);
					}
					if (Random.Range(0, 28) == 7)
					{
						toFill.addAnimalToChunk(270, j, i);
					}
				}
			}
		}
	}

	private void populateChunkLavaUnderground(AnimalChunk toFill, int chunkX, int chunkY)
	{
		int num = 0;
		for (int i = chunkY; i < chunkY + WorldManager.Instance.getChunkSize(); i++)
		{
			for (int j = chunkX; j < chunkX + WorldManager.Instance.getChunkSize(); j++)
			{
				if (WorldManager.Instance.onTileMap[j, i] == 881)
				{
					if (WorldManager.Instance.tileTypeMap[j, i] == 60 && Random.Range(0, 50) == 2)
					{
						toFill.addAnimalToChunk(360, j, i);
						if (Random.Range(0, 2) == 1)
						{
							toFill.addAnimalToChunk(370, j, i);
						}
						if (Random.Range(0, 5) == 3)
						{
							toFill.addAnimalToChunk(370, j, i);
						}
						if (Random.Range(0, 9) == 7)
						{
							toFill.addAnimalToChunk(370, j, i);
						}
					}
					else if (Random.Range(0, 1000) == 2)
					{
						toFill.addAnimalToChunk(360, j, i);
						if (Random.Range(0, 2) == 1)
						{
							toFill.addAnimalToChunk(370, j, i);
						}
						if (Random.Range(0, 5) == 3)
						{
							toFill.addAnimalToChunk(370, j, i);
						}
						if (Random.Range(0, 9) == 7)
						{
							toFill.addAnimalToChunk(370, j, i);
						}
					}
				}
				else if (WorldManager.Instance.onTileMap[j, i] == -1 && WorldManager.Instance.tileTypeMap[j, i] == 60 && Random.Range(0, 12) == 2 && num <= 1)
				{
					if (Random.Range(0, 9) == 1)
					{
						toFill.addAnimalToChunk(410, j, i);
					}
					else
					{
						toFill.addAnimalToChunk(400, j, i);
					}
					num++;
				}
			}
		}
	}

	private void populateChunkForestUnderground(AnimalChunk toFill, int chunkX, int chunkY)
	{
		for (int i = chunkY; i < chunkY + WorldManager.Instance.getChunkSize(); i++)
		{
			for (int j = chunkX; j < chunkX + WorldManager.Instance.getChunkSize(); j++)
			{
				if (WorldManager.Instance.onTileMap[j, i] != -1)
				{
					continue;
				}
				if (WorldManager.Instance.heightMap[j, i] >= 1 && Random.Range(0, 400) == 2)
				{
					toFill.addAnimalToChunk(300, j, i);
				}
				if (WorldManager.Instance.heightMap[j, i] >= 1 && Random.Range(0, 1000) == 2)
				{
					toFill.addAnimalToChunk(310, j, i);
				}
				if (WorldManager.Instance.heightMap[j, i] >= 1 && Random.Range(0, 200) == 2)
				{
					if (Random.Range(0, 2) == 1)
					{
						toFill.addAnimalToChunk(20, j, i);
					}
					if (Random.Range(0, 5) == 3)
					{
						toFill.addAnimalToChunk(20, j, i);
					}
					if (Random.Range(0, 9) == 7)
					{
						toFill.addAnimalToChunk(20, j, i);
					}
				}
				if (WorldManager.Instance.heightMap[j, i] < 0 && Random.Range(0, 600) == 1)
				{
					toFill.addAnimalToChunk(10, j, i);
				}
				if (WorldManager.Instance.tileTypeMap[j, i] == 38 && WorldManager.Instance.heightMap[j, i] >= 1 && Random.Range(0, 50) == 2)
				{
					toFill.addAnimalToChunk(350, j, i);
					if (Random.Range(0, 2) == 1)
					{
						toFill.addAnimalToChunk(340, j, i);
					}
					if (Random.Range(0, 5) == 3)
					{
						toFill.addAnimalToChunk(340, j, i);
					}
					if (Random.Range(0, 9) == 7)
					{
						toFill.addAnimalToChunk(340, j, i);
					}
				}
			}
		}
	}

	private void populateChunkOffIsland(AnimalChunk toFill, int chunkX, int chunkY)
	{
		for (int i = chunkY; i < chunkY + WorldManager.Instance.getChunkSize(); i++)
		{
			for (int j = chunkX; j < chunkX + WorldManager.Instance.getChunkSize(); j++)
			{
				if (WorldManager.Instance.waterMap[j, i] && WorldManager.Instance.heightMap[j, i] < 0 && Random.Range(0, 100) == 1)
				{
					toFill.addAnimalToChunk(10, j, i);
					if (Random.Range(0, 2) == 1)
					{
						toFill.addAnimalToChunk(230, j, i);
					}
				}
				if (WorldManager.Instance.heightMap[j, i] >= 1 && Random.Range(0, 100) == 1)
				{
					toFill.addAnimalToChunk(20, j, i);
				}
			}
		}
	}

	public bool canSpawnOnTile(int xPos, int yPos)
	{
		if (WorldManager.Instance.fencedOffMap[xPos, yPos] > 0)
		{
			return false;
		}
		if (NetworkMapSharer.Instance.wishManager.IsWishActive(WishManager.WishType.SpotlessWish) && (WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].isPath || WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].isMowedGrass))
		{
			return false;
		}
		if (WorldManager.Instance.onTileMap[xPos, yPos] == -1)
		{
			return true;
		}
		if (WorldManager.Instance.onTileMap[xPos, yPos] > -1 && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].walkable)
		{
			return true;
		}
		return false;
	}

	public void checkForWhiteBoomer(AnimalChunk toFill, int x, int y)
	{
		if (RealWorldEventChecker.check.getCurrentEvent() == RealWorldEventChecker.TimedEvent.Chrissy && Random.Range(0, 5500) == 100)
		{
			toFill.addAnimalToChunk(290, x, y);
		}
	}

	private void populateChunk(AnimalChunk toFill, int chunkX, int chunkY)
	{
		int num = 0;
		int num2 = 1;
		if (NetworkMapSharer.Instance.wishManager.IsWishActive(WishManager.WishType.BountifulWish))
		{
			num++;
			num2++;
		}
		if (WorldManager.Instance.month == 4)
		{
			num++;
		}
		if ((bool)WeatherManager.Instance && WeatherManager.Instance.rainMgr.IsActive)
		{
			num2++;
		}
		if (1 == 0)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		List<int[]> list = new List<int[]>();
		for (int i = chunkY; i < chunkY + WorldManager.Instance.getChunkSize(); i++)
		{
			for (int j = chunkX; j < chunkX + WorldManager.Instance.getChunkSize(); j++)
			{
				int num3 = GenerateMap.generate.checkBiomType(j, i);
				if (!flag && num3 == 15)
				{
					if (WorldManager.Instance.onTileMap[j, i] == GenerateMap.generate.denWallObjects.objectsInBiom[0].tileObjectId)
					{
						flag = true;
					}
				}
				else if (!flag2 && num3 == 14 && WorldManager.Instance.onTileMap[j, i] == GenerateMap.generate.denWallObjects.objectsInBiom[0].tileObjectId)
				{
					flag2 = true;
				}
				if (WorldManager.Instance.onTileMap[j, i] == crocoBerleyBox.tileObjectId)
				{
					list.Add(new int[2] { j, i });
				}
				else if (WorldManager.Instance.onTileMap[j, i] == devilBerleyBox.tileObjectId)
				{
					list.Add(new int[2] { j, i });
				}
				else if (WorldManager.Instance.onTileMap[j, i] == sharkBerleyBox.tileObjectId)
				{
					list.Add(new int[2] { j, i });
				}
			}
		}
		for (int k = chunkY; k < chunkY + WorldManager.Instance.getChunkSize(); k++)
		{
			for (int l = chunkX; l < chunkX + WorldManager.Instance.getChunkSize(); l++)
			{
				if (!canSpawnOnTile(l, k))
				{
					continue;
				}
				if (WorldManager.Instance.waterMap[l, k] && WorldManager.Instance.heightMap[l, k] < 0)
				{
					if (WorldManager.Instance.tileTypeMap[l, k] == 2)
					{
						if (Random.Range(0, 100) <= num2)
						{
							toFill.addAnimalToChunk(10, l, k);
						}
						if (Random.Range(0, 250) < 2)
						{
							toFill.addAnimalToChunk(230, l, k);
						}
						if (GenerateMap.generate.checkBiomType(l, k) == 2)
						{
							if (crocDay)
							{
								if (Random.Range(0, 50) == 25)
								{
									toFill.addAnimalToChunk(30, l, k);
								}
							}
							else if (Random.Range(0, 300) == 100)
							{
								toFill.addAnimalToChunk(30, l, k);
							}
						}
						else if (crocDay)
						{
							if (Random.Range(0, 100) == 50)
							{
								toFill.addAnimalToChunk(30, l, k);
							}
						}
						else if (Random.Range(0, 1500) == 100)
						{
							toFill.addAnimalToChunk(30, l, k);
						}
						continue;
					}
					if (Random.Range(0, 100) <= num2 / 2)
					{
						toFill.addAnimalToChunk(10, l, k);
					}
					if (Random.Range(0, 5000) == 1)
					{
						toFill.addAnimalToChunk(50, l, k);
					}
					if (Random.Range(0, 600) < 2)
					{
						toFill.addAnimalToChunk(230, l, k);
					}
					if (Random.Range(0, 260) == 1)
					{
						toFill.addAnimalToChunk(180, l, k);
						if (Random.Range(0, 2) == 1)
						{
							toFill.addAnimalToChunk(180, l, k);
						}
						if (Random.Range(0, 4) == 3)
						{
							toFill.addAnimalToChunk(180, l, k);
						}
					}
					continue;
				}
				int num4 = GenerateMap.generate.checkBiomType(l, k);
				switch (num4)
				{
				case 1:
					toFill.addAnimalToChunk(tropicalAnimals.getBiomeAnimal(), l, k);
					break;
				case 2:
				case 3:
					toFill.addAnimalToChunk(billabongAnimals.getBiomeAnimal(), l, k);
					break;
				case 4:
					toFill.addAnimalToChunk(bushlandAnimals.getBiomeAnimal(), l, k);
					checkForWhiteBoomer(toFill, l, k);
					if (Random.Range(0, 2000) == 2)
					{
						toFill.addAnimalToChunk(40, l, k);
						if (Random.Range(0, 2) == 1)
						{
							toFill.addAnimalToChunk(40, l, k);
						}
						if (Random.Range(0, 5) == 3)
						{
							toFill.addAnimalToChunk(40, l, k);
						}
						if (Random.Range(0, 9) == 7)
						{
							toFill.addAnimalToChunk(40, l, k);
						}
					}
					break;
				case 5:
					toFill.addAnimalToChunk(desertAnimals.getBiomeAnimal(), l, k);
					checkForWhiteBoomer(toFill, l, k);
					break;
				case 6:
					toFill.addAnimalToChunk(pineForestAnimals.getBiomeAnimal(), l, k);
					checkForWhiteBoomer(toFill, l, k);
					break;
				case 7:
					toFill.addAnimalToChunk(plainsAnimals.getBiomeAnimal(), l, k);
					checkForWhiteBoomer(toFill, l, k);
					break;
				case 8:
					if (WorldManager.Instance.month == 3 && Random.Range(0, 300) == 2)
					{
						toFill.addAnimalToChunk(390, l, k);
						toFill.addAnimalToChunk(390, l, k);
					}
					toFill.addAnimalToChunk(pineForestAnimals.getBiomeAnimal(), l, k);
					checkForWhiteBoomer(toFill, l, k);
					break;
				case 13:
					if (WorldManager.Instance.onTileMap[l, k] == GenerateMap.generate.cassowaryNestObjects.getBiomObject())
					{
						addNestToList(l, k);
						toFill.addAnimalToChunk(160, l, k);
					}
					break;
				default:
					if (flag2 && num4 == 14)
					{
						if (toFill.checkAmountAlreadyInChunk(140) < 1 && Random.Range(0, 10) == 2)
						{
							toFill.addAnimalToChunk(140, l, k);
						}
					}
					else
					{
						if (!flag || num4 != 15 || toFill.checkAmountAlreadyInChunk(60) > 1 || Random.Range(0, 15) != 2)
						{
							break;
						}
						int num5 = Random.Range(0, 3000);
						if (num5 != 6 && NetworkMapSharer.Instance.wishManager.IsWishActive(WishManager.WishType.FortuitousWish))
						{
							num5 = Random.Range(0, 1200);
						}
						if (num5 == 6)
						{
							if (NetworkMapSharer.Instance.wishManager.IsWishActive(WishManager.WishType.FortuitousWish))
							{
								toFill.addAnimalToChunk(60 + Random.Range(3, 8), l, k);
							}
							else
							{
								toFill.addAnimalToChunk(60 + Random.Range(3, 7), l, k);
							}
						}
						else
						{
							toFill.addAnimalToChunk(60 + allAnimals[6].getRandomVariationNo(), l, k);
						}
					}
					break;
				case 0:
					break;
				}
				if (WorldManager.Instance.heightMap[l, k] > 0)
				{
					if (Random.Range(0, 500) <= num)
					{
						toFill.addAnimalToChunk(20, l, k);
					}
					else if ((bool)WeatherManager.Instance && WeatherManager.Instance.rainMgr.IsActive && Random.Range(0, 1500) == 1)
					{
						toFill.addAnimalToChunk(150, l, k);
					}
				}
			}
		}
		for (int m = 0; m < list.Count; m++)
		{
			int id = 30;
			int num6 = 0;
			if (WorldManager.Instance.onTileMap[list[m][0], list[m][1]] == crocoBerleyBox.tileObjectId)
			{
				num6 = Random.Range(2, 4);
			}
			else if (WorldManager.Instance.onTileMap[list[m][0], list[m][1]] == devilBerleyBox.tileObjectId)
			{
				num6 = Random.Range(2, 4);
				id = 140;
			}
			else if (WorldManager.Instance.onTileMap[list[m][0], list[m][1]] == sharkBerleyBox.tileObjectId)
			{
				num6 = Random.Range(2, 4);
				id = 50;
			}
			NetworkMapSharer.Instance.RpcClearOnTileObjectNoDrop(list[m][0], list[m][1]);
			while (num6 > 0)
			{
				for (int n = -5; n <= 5; n++)
				{
					for (int num7 = -5; num7 <= 5; num7++)
					{
						if (num6 <= 0)
						{
							break;
						}
						if (Random.Range(0, 5) == 3 && WorldManager.Instance.isPositionOnMap(list[m][0] + num7, list[m][1] + n))
						{
							toFill.addAnimalToChunk(id, list[m][0] + num7, list[m][1] + n);
							num6--;
						}
					}
					if (num6 <= 0)
					{
						break;
					}
				}
			}
		}
	}

	public void checkChunkForAnimals(int chunkX, int chunkY)
	{
		if (RealWorldTimeLight.time.offIsland)
		{
			for (int i = 0; i < loadedOffIslandChunks.Count; i++)
			{
				if (loadedOffIslandChunks[i].chunkX == chunkX && loadedOffIslandChunks[i].chunkY == chunkY)
				{
					loadedOffIslandChunks[i].spawnAnimalsInChunk();
					return;
				}
			}
		}
		else if (RealWorldTimeLight.time.underGround)
		{
			for (int j = 0; j < loadedUndergroundChunks.Count; j++)
			{
				if (loadedUndergroundChunks[j].chunkX == chunkX && loadedUndergroundChunks[j].chunkY == chunkY)
				{
					loadedUndergroundChunks[j].spawnAnimalsInChunk();
					return;
				}
			}
		}
		else
		{
			for (int k = 0; k < loadedChunks.Count; k++)
			{
				if (loadedChunks[k].chunkX == chunkX && loadedChunks[k].chunkY == chunkY)
				{
					if (loadedChunks[k].needsNewDayRefresh)
					{
						populateChunk(loadedChunks[k], chunkX, chunkY);
						loadedChunks[k].needsNewDayRefresh = false;
					}
					loadedChunks[k].spawnAnimalsInChunk();
					return;
				}
			}
		}
		createNewChunkAndPopulateAndSpawn(chunkX, chunkY);
	}

	public void nextDayAnimalChunks()
	{
		getAllFencedOffAnimals();
		refreshNonActiveChunksOnNewDay();
		addEggsToNests();
		List<FencedOffAnimal> list = new List<FencedOffAnimal>();
		for (int i = 0; i < alphaAnimals.Count; i++)
		{
			alphaAnimals[i].daysRemaining--;
			if (alphaAnimals[i].daysRemaining < 0)
			{
				list.Add(alphaAnimals[i]);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			alphaAnimals.Remove(list[j]);
		}
		loadedUndergroundChunks.Clear();
		loadedOffIslandChunks.Clear();
	}

	public void refreshNonActiveChunksOnNewDay()
	{
		for (int i = 0; i < loadedChunks.Count; i++)
		{
			loadedChunks[i].clearNonActiveChunk();
		}
	}

	private void createNewChunkAndPopulateAndSpawn(int chunkX, int chunkY)
	{
		AnimalChunk animalChunk = new AnimalChunk(chunkX, chunkY);
		if (RealWorldTimeLight.time.offIsland)
		{
			populateChunkOffIsland(animalChunk, chunkX, chunkY);
			loadedOffIslandChunks.Add(animalChunk);
		}
		else if (RealWorldTimeLight.time.underGround)
		{
			if (Vector2.Distance(GenerateUndergroundMap.generate.entrancePosition, new Vector2(chunkX, chunkY)) > 30f)
			{
				if (RealWorldTimeLight.time.mineLevel == 2)
				{
					populateChunkLavaUnderground(animalChunk, chunkX, chunkY);
					loadedUndergroundChunks.Add(animalChunk);
				}
				else if (RealWorldTimeLight.time.mineLevel == 1)
				{
					populateChunkForestUnderground(animalChunk, chunkX, chunkY);
					loadedUndergroundChunks.Add(animalChunk);
				}
				else
				{
					populateChunkNormalUnderground(animalChunk, chunkX, chunkY);
					loadedUndergroundChunks.Add(animalChunk);
				}
			}
			else
			{
				loadedUndergroundChunks.Add(animalChunk);
			}
		}
		else
		{
			populateChunk(animalChunk, chunkX, chunkY);
			loadedChunks.Add(animalChunk);
		}
		animalChunk.spawnAnimalsInChunk();
	}

	public void saveAnimalToChunk(int id, int xPos, int yPos, Vector3 sleepPos, int daysRemaining = 1)
	{
		int num = Mathf.RoundToInt(xPos / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
		int num2 = Mathf.RoundToInt(yPos / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
		if (RealWorldTimeLight.time.offIsland)
		{
			for (int i = 0; i < loadedOffIslandChunks.Count; i++)
			{
				if (loadedOffIslandChunks[i].chunkX == num && loadedOffIslandChunks[i].chunkY == num2)
				{
					loadedOffIslandChunks[i].addAnimalToChunkWithSleepPos(id, xPos, yPos, sleepPos, daysRemaining);
					return;
				}
			}
			AnimalChunk animalChunk = new AnimalChunk(num, num2);
			populateChunk(animalChunk, num, num2);
			animalChunk.addAnimalToChunkWithSleepPos(id, xPos, yPos, sleepPos, daysRemaining);
			loadedOffIslandChunks.Add(animalChunk);
			return;
		}
		if (RealWorldTimeLight.time.underGround)
		{
			for (int j = 0; j < loadedUndergroundChunks.Count; j++)
			{
				if (loadedUndergroundChunks[j].chunkX == num && loadedUndergroundChunks[j].chunkY == num2)
				{
					loadedUndergroundChunks[j].addAnimalToChunkWithSleepPos(id, xPos, yPos, sleepPos, daysRemaining);
					return;
				}
			}
			AnimalChunk animalChunk2 = new AnimalChunk(num, num2);
			populateChunk(animalChunk2, num, num2);
			animalChunk2.addAnimalToChunkWithSleepPos(id, xPos, yPos, sleepPos, daysRemaining);
			loadedUndergroundChunks.Add(animalChunk2);
			return;
		}
		for (int k = 0; k < loadedChunks.Count; k++)
		{
			if (loadedChunks[k].chunkX == num && loadedChunks[k].chunkY == num2)
			{
				loadedChunks[k].addAnimalToChunkWithSleepPos(id, xPos, yPos, sleepPos, daysRemaining);
				return;
			}
		}
		AnimalChunk animalChunk3 = new AnimalChunk(num, num2);
		populateChunk(animalChunk3, num, num2);
		animalChunk3.addAnimalToChunkWithSleepPos(id, xPos, yPos, sleepPos, daysRemaining);
		loadedChunks.Add(animalChunk3);
	}

	public void addNestToList(int xPos, int yPos)
	{
		for (int i = 0; i < allNests.Count; i++)
		{
			if (allNests[i].check(xPos, yPos))
			{
				return;
			}
		}
		Nest nest = new Nest(xPos, yPos);
		allNests.Add(nest);
		if (WorldManager.Instance.month == 4 && !nest.isEggNearby() && nest.canHaveEgg() && Random.Range(0, 5) == 3)
		{
			nest.giveEgg();
			NetworkMapSharer.Instance.spawnACarryable(NetworkMapSharer.Instance.cassowaryEgg, new Vector3(nest.xPos * 2, WorldManager.Instance.heightMap[nest.xPos, nest.yPos], nest.yPos * 2));
		}
	}

	public void loadEggsIntoNests()
	{
	}

	public void addEggsToNests()
	{
		if (WorldManager.Instance.month != 4)
		{
			return;
		}
		int biomObject = GenerateMap.generate.cassowaryNestObjects.getBiomObject();
		List<Nest> list = new List<Nest>();
		for (int i = 0; i < allNests.Count; i++)
		{
			if (WorldManager.Instance.onTileMap[allNests[i].xPos, allNests[i].yPos] != biomObject)
			{
				list.Add(allNests[i]);
			}
			else if (!allNests[i].isEggNearby())
			{
				if (allNests[i].canHaveEgg() && Random.Range(0, 5) == 3)
				{
					allNests[i].giveEgg();
					NetworkMapSharer.Instance.spawnACarryable(NetworkMapSharer.Instance.cassowaryEgg, new Vector3(allNests[i].xPos * 2, WorldManager.Instance.heightMap[allNests[i].xPos, allNests[i].yPos], allNests[i].yPos * 2));
				}
				else
				{
					allNests[i].addDaySinceEgg();
				}
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			allNests.Remove(list[j]);
		}
	}

	public void loadEggs()
	{
		_ = WorldManager.Instance.month;
		_ = 3;
	}

	public void getAllFencedOffAnimals()
	{
		fencedOffAnimals.Clear();
		alphaAnimals.Clear();
		saveFencedOffAnimalsEvent.Invoke();
		for (int i = 0; i < loadedChunks.Count; i++)
		{
			loadedChunks[i].fillFencedOffAnimalArray();
		}
	}

	public void placeFencedOffAnimalIntoChunk(FencedOffAnimal animal)
	{
		saveAnimalToChunk(animal.animalId, animal.xPos, animal.yPos, Vector3.zero);
	}

	public void placeHuntingTargetOnMap(int newAnimalId, int xPos, int yPos, int daysRemaining)
	{
		if (NetworkNavMesh.nav.doesPositionHaveNavChunk(xPos, yPos))
		{
			new AnimalsSaved(newAnimalId, xPos, yPos, daysRemaining).spawnAnimal();
		}
		else
		{
			saveAnimalToChunk(newAnimalId, xPos, yPos, Vector3.zero, daysRemaining);
		}
	}

	public void populateFishingTables()
	{
		List<InventoryItem> list = new List<InventoryItem>();
		List<InventoryItem> list2 = new List<InventoryItem>();
		List<InventoryItem> list3 = new List<InventoryItem>();
		List<InventoryItem> list4 = new List<InventoryItem>();
		List<InventoryItem> list5 = new List<InventoryItem>();
		List<InventoryItem> list6 = new List<InventoryItem>();
		List<InventoryItem> list7 = new List<InventoryItem>();
		List<InventoryItem> list8 = new List<InventoryItem>();
		List<InventoryItem> list9 = new List<InventoryItem>();
		List<InventoryItem> list10 = new List<InventoryItem>();
		List<InventoryItem> list11 = new List<InventoryItem>();
		List<InventoryItem> list12 = new List<InventoryItem>();
		List<InventoryItem> list13 = new List<InventoryItem>();
		List<InventoryItem> list14 = new List<InventoryItem>();
		List<InventoryItem> list15 = new List<InventoryItem>();
		List<InventoryItem> list16 = new List<InventoryItem>();
		List<InventoryItem> list17 = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].fish)
			{
				if ((bool)Inventory.Instance.allItems[i].itemChange)
				{
					for (int j = 0; j < Inventory.Instance.allItems[i].itemChange.changesAndTheirChanger.Length; j++)
					{
						Inventory.Instance.allItems[i].itemChange.changesAndTheirChanger[j].cycles = Mathf.RoundToInt(Inventory.Instance.allItems[i].value / Inventory.Instance.allItems[i].itemChange.changesAndTheirChanger[j].changesWhenComplete.value) + 1;
					}
				}
				SeasonAndTime.waterLocation[] myWaterLocation = Inventory.Instance.allItems[i].fish.mySeason.myWaterLocation;
				for (int k = 0; k < myWaterLocation.Length; k++)
				{
					switch (myWaterLocation[k])
					{
					case SeasonAndTime.waterLocation.Billabongs:
						list5.Add(Inventory.Instance.allItems[i]);
						break;
					case SeasonAndTime.waterLocation.NorthOcean:
						list.Add(Inventory.Instance.allItems[i]);
						break;
					case SeasonAndTime.waterLocation.SouthOcean:
						list2.Add(Inventory.Instance.allItems[i]);
						break;
					case SeasonAndTime.waterLocation.Mangroves:
						list3.Add(Inventory.Instance.allItems[i]);
						break;
					case SeasonAndTime.waterLocation.Rivers:
						list4.Add(Inventory.Instance.allItems[i]);
						break;
					case SeasonAndTime.waterLocation.UndergroundLake:
						list6.Add(Inventory.Instance.allItems[i]);
						break;
					case SeasonAndTime.waterLocation.ReefIsland:
						list7.Add(Inventory.Instance.allItems[i]);
						break;
					}
				}
			}
			else if ((bool)Inventory.Instance.allItems[i].bug)
			{
				SeasonAndTime.landLocation[] myLandLocation = Inventory.Instance.allItems[i].bug.mySeason.myLandLocation;
				for (int k = 0; k < myLandLocation.Length; k++)
				{
					switch (myLandLocation[k])
					{
					case SeasonAndTime.landLocation.All:
						list8.Add(Inventory.Instance.allItems[i]);
						list9.Add(Inventory.Instance.allItems[i]);
						list12.Add(Inventory.Instance.allItems[i]);
						list10.Add(Inventory.Instance.allItems[i]);
						list11.Add(Inventory.Instance.allItems[i]);
						break;
					case SeasonAndTime.landLocation.Bushland:
						list8.Add(Inventory.Instance.allItems[i]);
						break;
					case SeasonAndTime.landLocation.Pines:
						list12.Add(Inventory.Instance.allItems[i]);
						break;
					case SeasonAndTime.landLocation.Plains:
						list10.Add(Inventory.Instance.allItems[i]);
						break;
					case SeasonAndTime.landLocation.Tropics:
						list9.Add(Inventory.Instance.allItems[i]);
						break;
					case SeasonAndTime.landLocation.Desert:
						list11.Add(Inventory.Instance.allItems[i]);
						break;
					case SeasonAndTime.landLocation.Underground:
						list13.Add(Inventory.Instance.allItems[i]);
						break;
					case SeasonAndTime.landLocation.ReefIsland:
						list14.Add(Inventory.Instance.allItems[i]);
						break;
					}
				}
			}
			else
			{
				if (!Inventory.Instance.allItems[i].underwaterCreature)
				{
					continue;
				}
				SeasonAndTime.waterLocation[] myWaterLocation = Inventory.Instance.allItems[i].underwaterCreature.mySeason.myWaterLocation;
				for (int k = 0; k < myWaterLocation.Length; k++)
				{
					switch (myWaterLocation[k])
					{
					case SeasonAndTime.waterLocation.ReefIsland:
						list17.Add(Inventory.Instance.allItems[i]);
						break;
					case SeasonAndTime.waterLocation.NorthOcean:
					case SeasonAndTime.waterLocation.SouthOcean:
						list15.Add(Inventory.Instance.allItems[i]);
						break;
					default:
						list16.Add(Inventory.Instance.allItems[i]);
						break;
					}
				}
			}
		}
		topicalBugs.populateTable(list9);
		bushlandBugs.populateTable(list8);
		pineLandBugs.populateTable(list12);
		desertBugs.populateTable(list11);
		plainsBugs.populateTable(list10);
		underGroundBugs.populateTable(list13);
		reefIslandBugs.populateTable(list14);
		northernOceanFish.populateTable(list);
		southernOceanFish.populateTable(list2);
		riverFish.populateTable(list4);
		mangroveFish.populateTable(list3);
		billabongFish.populateTable(list5);
		undergroundFish.populateTable(list6);
		reefIslandFish.populateTable(list7);
		underWaterOceanCreatures.populateTable(list15);
		underWaterRiverCreatures.populateTable(list16);
		offIslandUnderWaterCreatures.populateTable(list17);
	}

	private void OnDestroy()
	{
		lookAtFishBook.RemoveAllListeners();
		lookAtBugBook.RemoveAllListeners();
		saveFencedOffAnimalsEvent.RemoveAllListeners();
	}

	private void CreateEasyLayerMask()
	{
		easyLayerMasks = new LayerMask[allAnimals.Length];
		defaultLayerMasks = new LayerMask[allAnimals.Length];
		int num = LayerMask.NameToLayer("Char");
		for (int i = 0; i < allAnimals.Length; i++)
		{
			AnimalAI_Attack component = allAnimals[i].GetComponent<AnimalAI_Attack>();
			if ((bool)component)
			{
				defaultLayerMasks[i] = component.myPrey;
				easyLayerMasks[i] = component.myPrey;
				if ((easyLayerMasks[i].value & (1 << num)) != 0)
				{
					easyLayerMasks[i].value &= ~(1 << num);
				}
			}
		}
	}

	public bool WasDifficultyChangedOverNight()
	{
		return changeOverNight;
	}

	public void SetChangedOverNight(bool changeTo)
	{
		changeOverNight = changeTo;
	}

	public void MakeAnimalsEasy()
	{
		if (easyLayerMasks == null || easyLayerMasks.Length == 0)
		{
			CreateEasyLayerMask();
		}
		for (int i = 0; i < allAnimals.Length; i++)
		{
			AnimalAI_Attack component = allAnimals[i].GetComponent<AnimalAI_Attack>();
			if (allAnimals[i].isAnimal && (bool)component)
			{
				component.myPrey = easyLayerMasks[i];
			}
		}
		for (int j = 0; j < animalPool.Count; j++)
		{
			animalPool[j].SetDifficultyLayer(easyLayerMasks[animalPool[j].animalId]);
		}
	}

	public void MakeAnimalsNormal(bool activeAnimals = true)
	{
		if (easyLayerMasks != null && defaultLayerMasks.Length != 0)
		{
			for (int i = 0; i < allAnimals.Length; i++)
			{
				AnimalAI_Attack component = allAnimals[i].GetComponent<AnimalAI_Attack>();
				if (allAnimals[i].isAnimal && (bool)component)
				{
					component.myPrey = defaultLayerMasks[i];
				}
			}
		}
		if (activeAnimals)
		{
			for (int j = 0; j < animalPool.Count; j++)
			{
				animalPool[j].SetDifficultyLayer(defaultLayerMasks[animalPool[j].animalId]);
			}
		}
	}

	public LayerMask GetCurrentDifficultyLayer(int animalId)
	{
		if (NetworkMapSharer.Instance.wishManager.IsWishActive(WishManager.WishType.PeacefulWish))
		{
			return easyLayerMasks[animalId];
		}
		return defaultLayerMasks[animalId];
	}
}
