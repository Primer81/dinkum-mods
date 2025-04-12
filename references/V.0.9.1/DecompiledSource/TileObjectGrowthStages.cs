using UnityEngine;

public class TileObjectGrowthStages : MonoBehaviour
{
	[Header("Details----------------")]
	public bool isPlant = true;

	public bool diesOnHarvest;

	public bool needsTilledSoil = true;

	public bool normalPickUp;

	public bool mustBeInWater;

	public bool canGrowInTilledWater;

	public int onlyGrowIfAboveStage;

	[Header("Growth Stages--------------")]
	public GameObject[] objectStages;

	public GameObject[] objectStagesAnim2;

	public Animator stagesAnim;

	private int showingStage;

	[Header("Harvest Settings--------------")]
	public int takeOrAddFromStateOnHarvest = -1;

	public Transform[] harvestSpots;

	public InventoryItem harvestDrop;

	public InventoryItemLootTable dropsFromLootTable;

	public bool isCrabPot;

	public bool harvestableByHand = true;

	public bool autoPickUpOnHarvest;

	[Header("Once grown or dead change to-----")]
	public TileObject changeToWhenGrown;

	public TileObject changeToWhenDead;

	public ASound harvestSound;

	public int[] dropsForStages;

	[Header("Steams out or grows out--------------")]
	public TileObject steamsOutInto;

	public BiomSpawnTable steamsOutIntoTable;

	[Header("Growing Conditions--------------")]
	public bool diesOutOfSeason;

	public bool growsInSummer = true;

	public bool growsInWinter = true;

	public bool growsInAutum = true;

	public bool growsInSpring = true;

	public int minTemp = 20;

	public int maxTemp = 25;

	public bool canAppearInShops = true;

	[Header("Place Item On To Grow Instant")]
	public InventoryItem[] itemsToPlace;

	public int maxStageToReachByPlacing;

	[Header("Deeds and Building--------------")]
	public bool isADeed;

	public NPCDetails[] NPCMovesInWhenBuilt;

	public DailyTaskGenerator.genericTaskType milestoneOnHarvest;

	public int getShowingStage()
	{
		return showingStage;
	}

	public void showOnlyFirstStageForPreview()
	{
		if (!stagesAnim)
		{
			for (int i = 0; i < objectStages.Length; i++)
			{
				objectStages[i].SetActive(value: false);
			}
			objectStages[0].SetActive(value: true);
		}
	}

	public void setStage(int xPos, int yPos)
	{
		int num = (showingStage = WorldManager.Instance.onTileStatusMap[xPos, yPos]);
		if (!stagesAnim)
		{
			for (int i = 0; i < objectStages.Length; i++)
			{
				if (i != num)
				{
					objectStages[i].SetActive(value: false);
				}
			}
			for (int j = 0; j < objectStages.Length; j++)
			{
				if (j == num)
				{
					objectStages[j].SetActive(value: true);
				}
			}
			for (int k = 0; k < objectStagesAnim2.Length; k++)
			{
				if (k != num)
				{
					objectStagesAnim2[k].SetActive(value: false);
				}
			}
			for (int l = 0; l < objectStagesAnim2.Length; l++)
			{
				if (l == num)
				{
					objectStagesAnim2[l].SetActive(value: true);
				}
			}
			if (num > objectStages.Length)
			{
				objectStages[objectStages.Length - 1].SetActive(value: true);
				if (objectStagesAnim2.Length != 0)
				{
					objectStagesAnim2[objectStagesAnim2.Length - 1].SetActive(value: true);
				}
			}
			return;
		}
		if (mustBeInWater)
		{
			if (WorldManager.Instance.heightMap[xPos, yPos] < 0 && WorldManager.Instance.waterMap[xPos, yPos])
			{
				stagesAnim.SetBool("InWater", value: true);
			}
			else
			{
				stagesAnim.SetBool("InWater", value: false);
			}
		}
		stagesAnim.SetInteger("Stage", num);
	}

	public void setStageForHands(int currentStage)
	{
		for (int i = 0; i < objectStages.Length; i++)
		{
			if (i != currentStage)
			{
				objectStages[i].SetActive(value: false);
			}
		}
		for (int j = 0; j < objectStages.Length; j++)
		{
			if (j == currentStage)
			{
				objectStages[j].SetActive(value: true);
			}
		}
		for (int k = 0; k < objectStagesAnim2.Length; k++)
		{
			if (k != currentStage)
			{
				objectStagesAnim2[k].SetActive(value: false);
			}
		}
		for (int l = 0; l < objectStagesAnim2.Length; l++)
		{
			if (l == currentStage)
			{
				objectStagesAnim2[l].SetActive(value: true);
			}
		}
		if (currentStage > objectStages.Length)
		{
			objectStages[objectStages.Length - 1].SetActive(value: true);
			if (objectStagesAnim2.Length != 0)
			{
				objectStagesAnim2[objectStagesAnim2.Length - 1].SetActive(value: true);
			}
		}
	}

	public bool canBeHarvested(int stage, bool deathCheck = false)
	{
		if (!harvestableByHand && !deathCheck)
		{
			return false;
		}
		if (stage >= objectStages.Length - 1)
		{
			return true;
		}
		return false;
	}

	public bool canTractorHarvest(int stage)
	{
		if (stage == objectStages.Length - 1)
		{
			return true;
		}
		return false;
	}

	public bool checkIfShouldDie(int xPos, int yPos)
	{
		if (needsTilledSoil || diesOutOfSeason)
		{
			if (WorldManager.Instance.tileTypeMap[xPos, yPos] == 43 || WorldManager.Instance.tileTypeMap[xPos, yPos] == 44)
			{
				return false;
			}
			if (WorldManager.Instance.month == 1 && growsInSummer)
			{
				return false;
			}
			if (WorldManager.Instance.month == 2 && growsInAutum)
			{
				return false;
			}
			if (WorldManager.Instance.month == 3 && growsInWinter)
			{
				return false;
			}
			if (WorldManager.Instance.month == 4 && growsInSpring)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool growsAllYear()
	{
		if (growsInAutum && growsInSpring && growsInSummer && growsInWinter)
		{
			return true;
		}
		return false;
	}

	public bool IsTilledTile(int xPos, int yPos)
	{
		if (WorldManager.Instance.tileTypeMap[xPos, yPos] == 8 || WorldManager.Instance.tileTypeMap[xPos, yPos] == 7 || WorldManager.Instance.tileTypeMap[xPos, yPos] == 43 || WorldManager.Instance.tileTypeMap[xPos, yPos] == 44)
		{
			return true;
		}
		return false;
	}

	public bool IsFertilisedTile(int xPos, int yPos)
	{
		if (WorldManager.Instance.tileTypeMap[xPos, yPos] == 12 || WorldManager.Instance.tileTypeMap[xPos, yPos] == 13)
		{
			return true;
		}
		return false;
	}

	public bool IsOnNormalTilledDirtInWater(int xPos, int yPos)
	{
		if (canGrowInTilledWater && IsTilledTile(xPos, yPos) && WorldManager.Instance.heightMap[xPos, yPos] == 0 && WorldManager.Instance.waterMap[xPos, yPos])
		{
			return true;
		}
		return false;
	}

	public bool IsOnFertillisedTilledDirtInWater(int xPos, int yPos)
	{
		if (canGrowInTilledWater && IsFertilisedTile(xPos, yPos) && WorldManager.Instance.heightMap[xPos, yPos] == 0 && WorldManager.Instance.waterMap[xPos, yPos])
		{
			return true;
		}
		return false;
	}

	public void checkIfShouldGrow(int xPos, int yPos)
	{
		GenerateMap.generate.getTileTemperature(xPos, yPos);
		if (WorldManager.Instance.onTileStatusMap[xPos, yPos] >= 0 && WorldManager.Instance.onTileStatusMap[xPos, yPos] < onlyGrowIfAboveStage)
		{
			return;
		}
		if (isADeed)
		{
			if ((bool)WorldManager.Instance.allObjects[GetComponent<TileObject>().tileObjectId].tileObjectGrowthStages.changeToWhenGrown && BuildingManager.manage.currentlyMoving == WorldManager.Instance.allObjects[GetComponent<TileObject>().tileObjectId].tileObjectGrowthStages.changeToWhenGrown.tileObjectId)
			{
				BuildingManager.manage.moveBuildingToNewSite(changeToWhenGrown.tileObjectId, xPos, yPos);
			}
			else
			{
				if (!NetworkMapSharer.Instance.isServer || !DeedManager.manage.checkIfDeedMaterialsComplete(xPos, yPos))
				{
					return;
				}
				if ((bool)GetComponent<DisplayPlayerHouseTiles>())
				{
					int tileObjectId = GetComponent<TileObject>().tileObjectId;
					for (int i = 0; i < TownManager.manage.playerHouseStages.Length; i++)
					{
						if (TownManager.manage.playerHouseStages[i].placeable.tileObjectId == tileObjectId)
						{
							DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.UpgradeHouse);
							break;
						}
					}
					NetworkMapSharer.Instance.RpcUpgradeHouse(changeToWhenGrown.tileObjectId, xPos, yPos);
					if (NPCMovesInWhenBuilt.Length == 0)
					{
						return;
					}
					for (int j = 0; j < NPCManager.manage.NPCDetails.Length; j++)
					{
						for (int k = 0; k < NPCMovesInWhenBuilt.Length; k++)
						{
							if (NPCManager.manage.NPCDetails[j] == NPCMovesInWhenBuilt[k])
							{
								NPCManager.manage.moveInNPC(j);
							}
						}
					}
					return;
				}
				WorldManager.Instance.onTileStatusMap[xPos, yPos] = Mathf.Clamp(WorldManager.Instance.onTileStatusMap[xPos, yPos] + 1, 0, objectStages.Length - 1);
				if ((bool)changeToWhenGrown && WorldManager.Instance.onTileStatusMap[xPos, yPos] >= objectStages.Length - 1)
				{
					DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.CompleteABuilding);
					WorldManager.Instance.onTileStatusMap[xPos, yPos] = -1;
					NetworkMapSharer.Instance.RpcUpdateOnTileObject(changeToWhenGrown.tileObjectId, xPos, yPos);
					WorldManager.Instance.onTileMap[xPos, yPos] = changeToWhenGrown.tileObjectId;
					if ((bool)WorldManager.Instance.allObjectSettings[changeToWhenGrown.tileObjectId].tileObjectLoadInside)
					{
						NetworkMapSharer.Instance.overideOldFloor(xPos, yPos);
						WorldManager.Instance.allObjectSettings[changeToWhenGrown.tileObjectId].tileObjectLoadInside.checkForInterior(xPos, yPos);
					}
					if (NPCMovesInWhenBuilt.Length == 0)
					{
						return;
					}
					for (int l = 0; l < NPCManager.manage.NPCDetails.Length; l++)
					{
						for (int m = 0; m < NPCMovesInWhenBuilt.Length; m++)
						{
							if (NPCManager.manage.NPCDetails[l] == NPCMovesInWhenBuilt[m])
							{
								NPCManager.manage.moveInNPC(l);
							}
						}
					}
				}
				else
				{
					NetworkMapSharer.Instance.RpcGiveOnTileStatus(WorldManager.Instance.onTileStatusMap[xPos, yPos], xPos, yPos);
				}
			}
		}
		else if (!needsTilledSoil)
		{
			if (mustBeInWater && (WorldManager.Instance.heightMap[xPos, yPos] >= 0 || !WorldManager.Instance.waterMap[xPos, yPos]))
			{
				return;
			}
			WorldManager.Instance.onTileStatusMap[xPos, yPos] = Mathf.Clamp(WorldManager.Instance.onTileStatusMap[xPos, yPos] + 1, 0, objectStages.Length - 1);
			if (((bool)steamsOutInto && WorldManager.Instance.onTileStatusMap[xPos, yPos] == objectStages.Length - 1) || ((bool)steamsOutIntoTable && WorldManager.Instance.onTileStatusMap[xPos, yPos] == objectStages.Length - 1))
			{
				tryAndSpoutOut(xPos, yPos);
			}
			if (!changeToWhenGrown || WorldManager.Instance.onTileStatusMap[xPos, yPos] < objectStages.Length - 1)
			{
				return;
			}
			WorldManager.Instance.onTileMap[xPos, yPos] = changeToWhenGrown.tileObjectId;
			if ((bool)changeToWhenGrown.tileObjectGrowthStages)
			{
				WorldManager.Instance.onTileStatusMap[xPos, yPos] = 0;
			}
			else
			{
				WorldManager.Instance.onTileStatusMap[xPos, yPos] = -1;
			}
			if (!NetworkMapSharer.Instance.isServer)
			{
				return;
			}
			if ((bool)WorldManager.Instance.allObjectSettings[changeToWhenGrown.tileObjectId].tileObjectLoadInside)
			{
				WorldManager.Instance.allObjectSettings[changeToWhenGrown.tileObjectId].tileObjectLoadInside.checkForInterior(xPos, yPos);
			}
			if (NPCMovesInWhenBuilt.Length == 0)
			{
				return;
			}
			for (int n = 0; n < NPCManager.manage.NPCDetails.Length; n++)
			{
				for (int num = 0; num < NPCMovesInWhenBuilt.Length; num++)
				{
					if (NPCManager.manage.NPCDetails[n] == NPCMovesInWhenBuilt[num])
					{
						NPCManager.manage.moveInNPC(n);
					}
				}
			}
		}
		else if (WorldManager.Instance.tileTypeMap[xPos, yPos] == 8 || WorldManager.Instance.tileTypeMap[xPos, yPos] == 44 || IsOnNormalTilledDirtInWater(xPos, yPos))
		{
			WorldManager.Instance.onTileStatusMap[xPos, yPos] = Mathf.Clamp(WorldManager.Instance.onTileStatusMap[xPos, yPos] + 1, 0, objectStages.Length - 1);
			if (((bool)steamsOutInto && WorldManager.Instance.onTileStatusMap[xPos, yPos] == objectStages.Length - 1) || ((bool)steamsOutIntoTable && WorldManager.Instance.onTileStatusMap[xPos, yPos] == objectStages.Length - 1))
			{
				tryAndSpoutOut(xPos, yPos);
			}
		}
		else if (WorldManager.Instance.tileTypeMap[xPos, yPos] == 13 || IsOnFertillisedTilledDirtInWater(xPos, yPos))
		{
			if (WorldManager.Instance.onTileStatusMap[xPos, yPos] % 3 == 0)
			{
				WorldManager.Instance.onTileStatusMap[xPos, yPos] = Mathf.Clamp(WorldManager.Instance.onTileStatusMap[xPos, yPos] + 2, 0, objectStages.Length - 1);
			}
			else
			{
				WorldManager.Instance.onTileStatusMap[xPos, yPos] = Mathf.Clamp(WorldManager.Instance.onTileStatusMap[xPos, yPos] + 1, 0, objectStages.Length - 1);
			}
			if (WorldManager.Instance.onTileStatusMap[xPos, yPos] == objectStages.Length - 1)
			{
				WorldManager.Instance.addToCropChecker();
			}
			if (((bool)steamsOutInto && WorldManager.Instance.onTileStatusMap[xPos, yPos] == objectStages.Length - 1) || ((bool)steamsOutIntoTable && WorldManager.Instance.onTileStatusMap[xPos, yPos] == objectStages.Length - 1))
			{
				tryAndSpoutOut(xPos, yPos);
				tryAndSpoutOut(xPos, yPos);
			}
		}
	}

	public int getCrabTrapDrop(int xPos, int yPos)
	{
		int num = GenerateMap.generate.checkBiomType(xPos, yPos);
		if (Random.Range(0, 3) == 2)
		{
			if (WorldManager.Instance.tileTypeMap[xPos, yPos] == 3 || num == 8 || num == 9 || num == 10 || num == 11)
			{
				return AnimalManager.manage.underWaterOceanCreatures.getInventoryItem().getItemId();
			}
			return AnimalManager.manage.underWaterRiverCreatures.getInventoryItem().getItemId();
		}
		if (Random.Range(0, 20) == 15)
		{
			return BuriedManager.manage.wheelieBinDrops.getRandomDropFromTable().getItemId();
		}
		if (Random.Range(0, 8) == 1 && num >= 8 && num <= 11)
		{
			return BuriedManager.manage.shellDrops.getRandomDropFromTable().getItemId();
		}
		return Inventory.Instance.getInvItemId(dropsFromLootTable.getRandomDropFromTable());
	}

	public void harvest(int xPos, int yPos)
	{
		Transform[] array = harvestSpots;
		foreach (Transform transform in array)
		{
			_ = transform == null;
			if (isCrabPot)
			{
				NetworkMapSharer.Instance.spawnAServerDrop(getCrabTrapDrop(xPos, yPos), 1, transform.position, null, tryNotToStack: false, WorldManager.Instance.allObjects[GetComponent<TileObject>().tileObjectId].getXpTallyType());
				continue;
			}
			if ((bool)harvestDrop && Inventory.Instance.getInvItemId(harvestDrop) != -1)
			{
				NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(harvestDrop), 1, transform.position, null, tryNotToStack: false, WorldManager.Instance.allObjects[GetComponent<TileObject>().tileObjectId].getXpTallyType());
			}
			if ((bool)dropsFromLootTable)
			{
				int invItemId = Inventory.Instance.getInvItemId(dropsFromLootTable.getRandomDropFromTable());
				if (invItemId != -1)
				{
					NetworkMapSharer.Instance.spawnAServerDrop(invItemId, 1, transform.position, null, tryNotToStack: false, WorldManager.Instance.allObjects[GetComponent<TileObject>().tileObjectId].getXpTallyType());
				}
			}
		}
	}

	private bool checkIfCanSproutToTile(int xPos, int yPos, int centreX, int centreY)
	{
		if (WorldManager.Instance.heightMap[xPos, yPos] == WorldManager.Instance.heightMap[centreX, centreY] && (WorldManager.Instance.onTileMap[xPos, yPos] == -1 || (WorldManager.Instance.onTileMap[xPos, yPos] >= 0 && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].isGrass)))
		{
			return true;
		}
		return false;
	}

	public bool isAPlantSproutFromAFarmPlant(int tileObjectId)
	{
		TileObject tileObject = WorldManager.Instance.allObjects[tileObjectId];
		if ((bool)tileObject.tileObjectConnect && (bool)tileObject.tileObjectConnect.canConnectTo[0].tileObjectGrowthStages && tileObject.tileObjectConnect.canConnectTo[0].tileObjectGrowthStages.needsTilledSoil)
		{
			return true;
		}
		return false;
	}

	private int countSprouts(int xPos, int yPos)
	{
		int num = 0;
		if ((bool)steamsOutIntoTable)
		{
			return 0;
		}
		if (WorldManager.Instance.onTileMap[xPos - 1, yPos] == steamsOutInto.tileObjectId)
		{
			num++;
		}
		if (WorldManager.Instance.onTileMap[xPos + 1, yPos] == steamsOutInto.tileObjectId)
		{
			num++;
		}
		if (WorldManager.Instance.onTileMap[xPos, yPos - 1] == steamsOutInto.tileObjectId)
		{
			num++;
		}
		if (WorldManager.Instance.onTileMap[xPos, yPos + 1] == steamsOutInto.tileObjectId)
		{
			num++;
		}
		return num;
	}

	public void tryAndSpoutOut(int xPos, int yPos)
	{
		if (!NetworkMapSharer.Instance.isServer || xPos <= 1 || yPos <= 1 || yPos >= WorldManager.Instance.GetMapSize() - 1 || xPos >= WorldManager.Instance.GetMapSize() - 1)
		{
			return;
		}
		int num = -1;
		num = ((!steamsOutInto) ? steamsOutIntoTable.getBiomObject() : steamsOutInto.tileObjectId);
		if (countSprouts(xPos, yPos) == 0)
		{
			bool flag = false;
			if (!checkIfCanSproutToTile(xPos - 1, yPos, xPos, yPos) && !checkIfCanSproutToTile(xPos + 1, yPos, xPos, yPos) && !checkIfCanSproutToTile(xPos, yPos - 1, xPos, yPos) && !checkIfCanSproutToTile(xPos, yPos + 1, xPos, yPos))
			{
				return;
			}
			int num2 = -1;
			while (!flag)
			{
				switch (Random.Range(0, 4))
				{
				case 0:
					if (checkIfCanSproutToTile(xPos - 1, yPos, xPos, yPos))
					{
						flag = true;
						NetworkMapSharer.Instance.RpcUpdateOnTileObject(num, xPos - 1, yPos);
					}
					break;
				case 1:
					if (checkIfCanSproutToTile(xPos + 1, yPos, xPos, yPos))
					{
						flag = true;
						NetworkMapSharer.Instance.RpcUpdateOnTileObject(num, xPos + 1, yPos);
					}
					break;
				case 2:
					if (checkIfCanSproutToTile(xPos, yPos - 1, xPos, yPos))
					{
						flag = true;
						NetworkMapSharer.Instance.RpcUpdateOnTileObject(num, xPos, yPos - 1);
					}
					break;
				default:
					if (checkIfCanSproutToTile(xPos, yPos + 1, xPos, yPos))
					{
						flag = true;
						NetworkMapSharer.Instance.RpcUpdateOnTileObject(num, xPos, yPos + 1);
					}
					break;
				}
			}
		}
		else
		{
			if (countSprouts(xPos, yPos) == 4 || Random.Range(0, 3 * countSprouts(xPos, yPos)) != 1)
			{
				return;
			}
			switch (Random.Range(0, 3))
			{
			case 0:
				if (checkIfCanSproutToTile(xPos - 1, yPos, xPos, yPos))
				{
					NetworkMapSharer.Instance.RpcUpdateOnTileObject(num, xPos - 1, yPos);
				}
				break;
			case 1:
				if (checkIfCanSproutToTile(xPos + 1, yPos, xPos, yPos))
				{
					NetworkMapSharer.Instance.RpcUpdateOnTileObject(num, xPos + 1, yPos);
				}
				break;
			case 2:
				if (checkIfCanSproutToTile(xPos, yPos - 1, xPos, yPos))
				{
					NetworkMapSharer.Instance.RpcUpdateOnTileObject(num, xPos, yPos - 1);
				}
				break;
			default:
				if (checkIfCanSproutToTile(xPos, yPos + 1, xPos, yPos))
				{
					NetworkMapSharer.Instance.RpcUpdateOnTileObject(num, xPos, yPos + 1);
				}
				break;
			}
		}
	}
}
