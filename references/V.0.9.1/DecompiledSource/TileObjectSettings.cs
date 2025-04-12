using UnityEngine;

public class TileObjectSettings : MonoBehaviour
{
	public int tileObjectId;

	[Header("Death stuff --------------")]
	public float fullHealth = 100f;

	public int changeToTileObjectOnDeath = -1;

	public ASound deathSound;

	[Header("Drops On Death --------------")]
	public bool dropsStatusNumberOnDeath;

	public InventoryItem dropsItemOnDeath;

	public InventoryItemLootTable dropFromLootTable;

	public bool onlyDropWhenGrown;

	[Header("Death Object (Not INV item) --------------")]
	public GameObject dropsObjectOnDeath;

	public GameObject spawnCarryableOnDeath;

	public float carryableChance;

	[Header("Death Particles --------------")]
	public int deathParticle = -1;

	public int particlesPerPositon = 5;

	[Header("Damage Stuff --------------")]
	public ASound damageSound;

	public int damageParticle = -1;

	public int damageParticlesPerPosition;

	[Header("Furniture Settings --------------")]
	public bool canBePlacedOnTopOfFurniture;

	[Header("Settings --------------")]
	public bool getRotationFromMap;

	public bool walkable = true;

	public bool isFence;

	public bool isSpecialFencedObject;

	public bool canBePickedUp;

	public bool pickUpRequiresEmptyPocket;

	public bool hasRandomRotation = true;

	public bool hasRandomScale;

	public bool isMultiTileObject;

	public int xSize;

	public int ySize;

	public bool canPlaceItemsUnderIt;

	[Header("Other Settings --------------")]
	public bool isFlowerBed;

	public bool isWordSign;

	[Header("Damageable type --------------")]
	public bool isGrass;

	public bool isWood;

	public bool isHardWood;

	public bool isMetal;

	public bool isStone;

	public bool isHardStone;

	public bool isSmallPlant;

	public InventoryItem[] statusObjectsPickUpFirst;

	public LoadBuildingInsides tileObjectLoadInside;

	[Header("Town Beauty --------------")]
	public TownManager.TownBeautyType beautyType;

	public float beautyToAdd = 0.1f;

	[Header("Map Icon --------------")]
	public Sprite mapIcon;

	public Color mapIconColor = Color.white;

	public DailyTaskGenerator.genericTaskType TaskType;

	public void addBeauty()
	{
		TownManager.manage.addTownBeauty(beautyToAdd, beautyType);
	}

	public void removeBeauty()
	{
		TownManager.manage.addTownBeauty(0f - beautyToAdd, beautyType);
	}

	public bool canBePlacedOn()
	{
		if (WorldManager.Instance.allObjects[tileObjectId].placedPositions.Length != 0)
		{
			return true;
		}
		return false;
	}

	public bool hasWordsOnIt()
	{
		return isWordSign;
	}

	public bool checkIfMultiTileObjectCanBePlaced(int startingXPos, int startingYPos, int rotation)
	{
		if (startingXPos < 5 || startingXPos > WorldManager.Instance.GetMapSize() - 5 || startingYPos < 5 || startingYPos > WorldManager.Instance.GetMapSize() - 5)
		{
			return false;
		}
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		for (int i = 0; i < NetworkNavMesh.nav.charsConnected.Count; i++)
		{
			Vector3 position = NetworkNavMesh.nav.charsConnected[i].transform.position;
			if (position.x > (float)(startingXPos * 2) && position.x < (float)((startingXPos + num) * 2) && position.z > (float)(startingYPos * 2) && position.z < (float)((startingYPos + num2) * 2))
			{
				return false;
			}
		}
		int num3 = WorldManager.Instance.heightMap[startingXPos, startingYPos];
		for (int j = 0; j < num; j++)
		{
			for (int k = 0; k < num2; k++)
			{
				if (WorldManager.Instance.heightMap[startingXPos + j, startingYPos + k] != num3)
				{
					return false;
				}
				if (WorldManager.Instance.onTileMap[startingXPos + j, startingYPos + k] < -1)
				{
					return false;
				}
				if (WorldManager.Instance.onTileMap[startingXPos + j, startingYPos + k] != -1 && WorldManager.Instance.onTileMap[startingXPos + j, startingYPos + k] > -1 && WorldManager.Instance.onTileMap[startingXPos + j, startingYPos + k] != 30 && !WorldManager.Instance.getTileObjectSettings(startingXPos + j, startingYPos + k).isGrass)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool checkIfCanPlaceUnderMultiTileObjectCanBePlaced(int startingXPos, int startingYPos, int rotation)
	{
		if (startingXPos < 5 || startingXPos > WorldManager.Instance.GetMapSize() - 5 || startingYPos < 5 || startingYPos > WorldManager.Instance.GetMapSize() - 5)
		{
			return false;
		}
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		int num3 = WorldManager.Instance.heightMap[startingXPos, startingYPos];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if ((i == 0 && j == 0) || (i == num - 1 && j == 0) || (i == 0 && j == num2 - 1) || (i == num - 1 && j == num2 - 1))
				{
					if (WorldManager.Instance.heightMap[startingXPos + i, startingYPos + j] != num3)
					{
						return false;
					}
					if (WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] < -1)
					{
						return false;
					}
					if (WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] != -1 && WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] > -1 && WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] != 30 && !WorldManager.Instance.getTileObjectSettings(startingXPos + i, startingYPos + j).isGrass)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public bool checkIfMultiTileObjectIsUnderWater(int startingXPos, int startingYPos, int rotation)
	{
		if (startingXPos < 5 || startingXPos > WorldManager.Instance.GetMapSize() - 5 || startingYPos < 5 || startingYPos > WorldManager.Instance.GetMapSize() - 5)
		{
			return false;
		}
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		_ = WorldManager.Instance.heightMap[startingXPos, startingYPos];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (!WorldManager.Instance.waterMap[startingXPos + i, startingYPos + j])
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool checkIfMultiTileObjectCanBePlacedMapGenerationOnly(int startingXPos, int startingYPos, int rotation)
	{
		if (startingXPos < 5 || startingXPos > WorldManager.Instance.GetMapSize() - 5 || startingYPos < 5 || startingYPos > WorldManager.Instance.GetMapSize() - 5)
		{
			return false;
		}
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		int num3 = WorldManager.Instance.heightMap[startingXPos, startingYPos];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (WorldManager.Instance.heightMap[startingXPos + i, startingYPos + j] != num3)
				{
					return false;
				}
				if (WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] != -1 && WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] != 30)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool checkIfDeedCanBePlaced(int startingXPos, int startingYPos, int rotation)
	{
		if (startingXPos < 5 || startingXPos > WorldManager.Instance.GetMapSize() - 5 || startingYPos < 5 || startingYPos > WorldManager.Instance.GetMapSize() - 5)
		{
			return false;
		}
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		for (int i = 0; i < NetworkNavMesh.nav.charsConnected.Count; i++)
		{
			Vector3 position = NetworkNavMesh.nav.charsConnected[i].transform.position;
			if (position.x > (float)(startingXPos * 2) && position.x < (float)((startingXPos + num) * 2) && position.z > (float)(startingYPos * 2) && position.z < (float)((startingYPos + num2) * 2))
			{
				return false;
			}
		}
		int num3 = WorldManager.Instance.heightMap[startingXPos, startingYPos];
		for (int j = 0; j < num; j++)
		{
			for (int k = 0; k < num2; k++)
			{
				if (WorldManager.Instance.heightMap[startingXPos + j, startingYPos + k] != num3)
				{
					return false;
				}
				if (WorldManager.Instance.heightMap[startingXPos + j, startingYPos + k] < 0 && WorldManager.Instance.waterMap[startingXPos + j, startingYPos + k])
				{
					return false;
				}
				if (WorldManager.Instance.onTileMap[startingXPos + j, startingYPos + k] > -1)
				{
					if (!WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[startingXPos + j, startingYPos + k]].isWood && !WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[startingXPos + j, startingYPos + k]].isHardWood && !WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[startingXPos + j, startingYPos + k]].isSmallPlant && !WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[startingXPos + j, startingYPos + k]].isStone && !WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[startingXPos + j, startingYPos + k]].isHardStone)
					{
						return false;
					}
				}
				else if (WorldManager.Instance.onTileMap[startingXPos + j, startingYPos + k] < -1)
				{
					return false;
				}
			}
		}
		return true;
	}

	public string getWhyCantPlaceDeedText(int startingXPos, int startingYPos, int rotation)
	{
		if (startingXPos < 5 || startingXPos > WorldManager.Instance.GetMapSize() - 5 || startingYPos < 5 || startingYPos > WorldManager.Instance.GetMapSize() - 5)
		{
			return ConversationGenerator.generate.GetToolTip("Tip_CantPlaceHere");
		}
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		for (int i = 0; i < NetworkNavMesh.nav.charsConnected.Count; i++)
		{
			Vector3 position = NetworkNavMesh.nav.charsConnected[i].transform.position;
			if (position.x > (float)(startingXPos * 2) && position.x < (float)((startingXPos + num) * 2) && position.z > (float)(startingYPos * 2) && position.z < (float)((startingYPos + num2) * 2))
			{
				return ConversationGenerator.generate.GetToolTip("Tip_SomeoneInWay");
			}
		}
		int num3 = WorldManager.Instance.heightMap[startingXPos, startingYPos];
		for (int j = 0; j < num; j++)
		{
			for (int k = 0; k < num2; k++)
			{
				if (WorldManager.Instance.heightMap[startingXPos + j, startingYPos + k] != num3)
				{
					return ConversationGenerator.generate.GetToolTip("Tip_NotOnLevelGround");
				}
				if (WorldManager.Instance.heightMap[startingXPos + j, startingYPos + k] < 0 && WorldManager.Instance.waterMap[startingXPos + j, startingYPos + k])
				{
					return ConversationGenerator.generate.GetToolTip("Tip_CantPlaceInWater");
				}
				if (WorldManager.Instance.onTileMap[startingXPos + j, startingYPos + k] > -1)
				{
					if (!WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[startingXPos + j, startingYPos + k]].isWood && !WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[startingXPos + j, startingYPos + k]].isHardWood && !WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[startingXPos + j, startingYPos + k]].isSmallPlant && !WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[startingXPos + j, startingYPos + k]].isStone && !WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[startingXPos + j, startingYPos + k]].isHardStone)
					{
						return ConversationGenerator.generate.GetToolTip("Tip_SomethingInWay");
					}
				}
				else if (WorldManager.Instance.onTileMap[startingXPos + j, startingYPos + k] < -1)
				{
					return ConversationGenerator.generate.GetToolTip("Tip_SomethingInWay");
				}
			}
		}
		return "";
	}

	public int CheckBridgeLength(int startX, int startY, int xCheck = 0, int yCheck = 0)
	{
		int i;
		for (i = 1; i <= 15; i++)
		{
			int num = startX + xCheck * i;
			int num2 = startY + yCheck * i;
			if (num < 5 || num > WorldManager.Instance.GetMapSize() - 5 || num2 < 5 || num2 > WorldManager.Instance.GetMapSize() - 5)
			{
				i = 20;
				break;
			}
			if (WorldManager.Instance.heightMap[startX, startY] == WorldManager.Instance.heightMap[num, num2] && i >= 2)
			{
				i++;
				break;
			}
		}
		return i;
	}

	public bool checkIfBridgeCanBePlaced(int startingXPos, int startingYPos, int rotation)
	{
		if (startingXPos < 5 || startingXPos > WorldManager.Instance.GetMapSize() - 5 || startingYPos < 5 || startingYPos > WorldManager.Instance.GetMapSize() - 5)
		{
			return false;
		}
		if (WorldManager.Instance.waterMap[startingXPos, startingYPos] && WorldManager.Instance.heightMap[startingXPos, startingYPos] <= -1)
		{
			return false;
		}
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		int num3 = 2;
		switch (rotation)
		{
		case 1:
			num3 = CheckBridgeLength(startingXPos, startingYPos, 0, -1);
			if (num3 <= 15)
			{
				num2 = num3;
				startingYPos -= num3 - 1;
				break;
			}
			return false;
		case 2:
			num3 = CheckBridgeLength(startingXPos, startingYPos, -1);
			if (num3 <= 15)
			{
				num = num3;
				startingXPos -= num3 - 1;
				break;
			}
			return false;
		case 3:
			num3 = CheckBridgeLength(startingXPos, startingYPos, 0, 1);
			if (num3 <= 15)
			{
				num2 = num3;
				break;
			}
			return false;
		case 4:
			num3 = CheckBridgeLength(startingXPos, startingYPos, 1);
			if (num3 <= 15)
			{
				num = num3;
				break;
			}
			return false;
		}
		int num4 = WorldManager.Instance.heightMap[startingXPos, startingYPos];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (WorldManager.Instance.heightMap[startingXPos + i, startingYPos + j] > num4)
				{
					return false;
				}
				if (WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] > -1)
				{
					if (!WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j]].isWood && !WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j]].isHardWood && !WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j]].isSmallPlant && !WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j]].isStone && !WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j]].isHardStone)
					{
						return false;
					}
					if ((bool)WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j]].dropsItemOnDeath && (bool)WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j]].dropsItemOnDeath && (bool)WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j]].dropsItemOnDeath.placeable && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j]].dropsItemOnDeath.placeable.tileObjectId == WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j])
					{
						return false;
					}
				}
				else if (WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] < -1)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool checkIfMultiTileObjectCanBePlacedOnStoredMap(int startingXPos, int startingYPos, int[,] storedOnTileArray, int[,] storedHeightMap, int rotation, bool allowPlacementOnSingleTiledObjects = false, bool ignoreHeight = false)
	{
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		if (startingXPos < 5 || startingXPos > WorldManager.Instance.GetMapSize() - 5 || startingYPos < 5 || startingYPos > WorldManager.Instance.GetMapSize() - 5)
		{
			return false;
		}
		int num3 = storedHeightMap[startingXPos, startingYPos];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (!ignoreHeight && storedHeightMap[startingXPos + i, startingYPos + j] != num3)
				{
					return false;
				}
				if (allowPlacementOnSingleTiledObjects)
				{
					int num4 = storedOnTileArray[startingXPos + i, startingYPos + j];
					if (num4 >= 0 && WorldManager.Instance.allObjectSettings[num4].isMultiTileObject)
					{
						return false;
					}
					if (num4 < -1)
					{
						return false;
					}
				}
				else if (!allowPlacementOnSingleTiledObjects && storedOnTileArray[startingXPos + i, startingYPos + j] != -1 && storedOnTileArray[startingXPos + i, startingYPos + j] != 30)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool CheckIfMultiTileObjectCanBePlacedInside(int startingXPos, int startingYPos, int rotation, HouseDetails houseDetails)
	{
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (houseDetails.houseMapOnTile[startingXPos + i, startingYPos + j] != -1)
				{
					return false;
				}
			}
		}
		return true;
	}

	public int[] placeBridgeTiledObject(int startingXPos, int startingYPos, int rotation = 4, int length = 2)
	{
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		switch (rotation)
		{
		case 1:
			num2 = length;
			startingYPos -= length - 1;
			rotation = 3;
			break;
		case 2:
			num = length;
			startingXPos -= length - 1;
			rotation = 4;
			break;
		case 3:
			num2 = length;
			break;
		case 4:
			num = length;
			break;
		}
		WorldManager.Instance.rotationMap[startingXPos, startingYPos] = rotation;
		WorldManager.Instance.onTileStatusMap[startingXPos, startingYPos] = length;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (i == 0 && j == 0)
				{
					WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] = tileObjectId;
				}
				else if (i == 0)
				{
					WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] = -3;
				}
				else if (j == 0)
				{
					WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] = -4;
				}
				else
				{
					WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] = -2;
				}
				WorldManager.Instance.addToChunksToRefreshList(startingXPos + i, startingYPos + j);
				Vector3 position = new Vector3((float)(startingXPos + i) * 2f, WorldManager.Instance.heightMap[startingXPos + i, startingYPos + j], (float)(startingYPos + j) * 2f);
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], position, 2);
				WorldManager.Instance.onTileChunkHasChanged(startingXPos + i, startingYPos + j);
				if (NetworkMapSharer.Instance.isServer)
				{
					WorldManager.Instance.findSpaceForDropAfterTileObjectChange(startingXPos + i, startingYPos + j);
				}
			}
		}
		return new int[2] { startingXPos, startingYPos };
	}

	public void placeMultiTiledObject(int startingXPos, int startingYPos, int rotation = 4)
	{
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		WorldManager.Instance.rotationMap[startingXPos, startingYPos] = rotation;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] != -1 && WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] != 30)
				{
					TileObject tileObject = WorldManager.Instance.findTileObjectInUse(startingXPos + i, startingYPos + j);
					if ((bool)tileObject)
					{
						tileObject.onDeath();
					}
				}
				if (i == 0 && j == 0)
				{
					WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] = tileObjectId;
				}
				else if (i == 0)
				{
					WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] = -3;
				}
				else if (j == 0)
				{
					WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] = -4;
				}
				else
				{
					WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] = -2;
				}
				WorldManager.Instance.placeFenceInChunk(startingXPos + i, startingYPos + j);
				WorldManager.Instance.addToChunksToRefreshList(startingXPos + i, startingYPos + j);
				Vector3 position = new Vector3((float)(startingXPos + i) * 2f, WorldManager.Instance.heightMap[startingXPos + i, startingYPos + j], (float)(startingYPos + j) * 2f);
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], position, 2);
				WorldManager.Instance.onTileChunkHasChanged(startingXPos + i, startingYPos + j);
				if (CustomNetworkManager.manage.isNetworkActive)
				{
					WorldManager.Instance.findSpaceForDropAfterTileObjectChange(startingXPos + i, startingYPos + j);
				}
			}
		}
	}

	public void placeMultiTiledObjectPlaceUnder(int startingXPos, int startingYPos, int rotation = 4)
	{
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		WorldManager.Instance.rotationMap[startingXPos, startingYPos] = rotation;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] != -1)
				{
					_ = WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j];
					_ = 30;
				}
				if (i == 0 && j == 0)
				{
					WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] = tileObjectId;
				}
				else if (i == num - 1 && j == 0)
				{
					WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] = -200 - num;
				}
				else if (i == 0 && j == num2 - 1)
				{
					WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] = -100 - num2;
				}
				else if (i == num - 1 && j == num2 - 1)
				{
					WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] = -100 - num2;
				}
				WorldManager.Instance.placeFenceInChunk(startingXPos + i, startingYPos + j);
				WorldManager.Instance.addToChunksToRefreshList(startingXPos + i, startingYPos + j);
				Vector3 position = new Vector3((float)(startingXPos + i) * 2f, WorldManager.Instance.heightMap[startingXPos + i, startingYPos + j], (float)(startingYPos + j) * 2f);
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], position, 2);
				WorldManager.Instance.onTileChunkHasChanged(startingXPos + i, startingYPos + j);
				if (CustomNetworkManager.manage.isNetworkActive)
				{
					WorldManager.Instance.findSpaceForDropAfterTileObjectChange(startingXPos + i, startingYPos + j);
				}
			}
		}
	}

	public void flattenPosUnderMultitiledObject(int startingXPos, int startingYPos, int height, int rotation = 4)
	{
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				WorldManager.Instance.heightMap[startingXPos + i, startingYPos + j] = height;
			}
		}
	}

	public void placeMultiTiledObjectOnStoredMap(int startingXPos, int startingYPos, MapStorer.LoadMapType storedMapType, int rotation = 4)
	{
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		switch (storedMapType)
		{
		case MapStorer.LoadMapType.Underworld:
		{
			MapStorer.store.underWorldRotationMap[startingXPos, startingYPos] = rotation;
			for (int k = 0; k < num; k++)
			{
				for (int l = 0; l < num2; l++)
				{
					if (k == 0 && l == 0)
					{
						MapStorer.store.underWorldOnTile[startingXPos + k, startingYPos + l] = tileObjectId;
					}
					else if (k == 0)
					{
						MapStorer.store.underWorldOnTile[startingXPos + k, startingYPos + l] = -3;
					}
					else if (l == 0)
					{
						MapStorer.store.underWorldOnTile[startingXPos + k, startingYPos + l] = -4;
					}
					else
					{
						MapStorer.store.underWorldOnTile[startingXPos + k, startingYPos + l] = -2;
					}
				}
			}
			break;
		}
		case MapStorer.LoadMapType.OffIsland:
		{
			MapStorer.store.offIslandRotationMap[startingXPos, startingYPos] = rotation;
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					int num3 = Mathf.RoundToInt((startingXPos + i) / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
					int num4 = Mathf.RoundToInt((startingYPos + j) / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
					MapStorer.store.offIslandOnTileChangedMap[num3 / WorldManager.Instance.getChunkSize(), num4 / WorldManager.Instance.getChunkSize()] = true;
					MapStorer.store.offIslandChangedMap[num3 / WorldManager.Instance.getChunkSize(), num4 / WorldManager.Instance.getChunkSize()] = true;
					if (i == 0 && j == 0)
					{
						MapStorer.store.offIslandOnTile[startingXPos + i, startingYPos + j] = tileObjectId;
					}
					else if (i == 0)
					{
						MapStorer.store.offIslandOnTile[startingXPos + i, startingYPos + j] = -3;
					}
					else if (j == 0)
					{
						MapStorer.store.offIslandOnTile[startingXPos + i, startingYPos + j] = -4;
					}
					else
					{
						MapStorer.store.offIslandOnTile[startingXPos + i, startingYPos + j] = -2;
					}
				}
			}
			break;
		}
		}
	}

	public void PlaceMultiTiledObjectInside(int startingXPos, int startingYPos, int rotation, HouseDetails placeInside)
	{
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		placeInside.houseMapRotation[startingXPos, startingYPos] = rotation;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (i == 0 && j == 0)
				{
					placeInside.houseMapOnTile[startingXPos + i, startingYPos + j] = tileObjectId;
				}
				else if (i == 0)
				{
					placeInside.houseMapOnTile[startingXPos + i, startingYPos + j] = -3;
				}
				else if (j == 0)
				{
					placeInside.houseMapOnTile[startingXPos + i, startingYPos + j] = -4;
				}
				else
				{
					placeInside.houseMapOnTile[startingXPos + i, startingYPos + j] = -2;
				}
			}
		}
	}

	public void removeMultiTiledObject(int startingXPos, int startingYPos, int rotation)
	{
		int num = xSize;
		int num2 = ySize;
		if (WorldManager.Instance.onTileMap[startingXPos, startingYPos] > -1 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[startingXPos, startingYPos]].tileObjectBridge)
		{
			num2 = WorldManager.Instance.onTileStatusMap[startingXPos, startingYPos];
			if (rotation == 2 || rotation == 4)
			{
				num = num2;
				num2 = xSize;
			}
		}
		else if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] = -1;
				Vector3 position = new Vector3((float)(startingXPos + i) * 2f, WorldManager.Instance.heightMap[startingXPos + i, startingYPos + j], (float)(startingYPos + j) * 2f);
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], position, 2);
				WorldManager.Instance.onTileChunkHasChanged(startingXPos + i, startingYPos + j);
				WorldManager.Instance.placeFenceInChunk(startingXPos + i, startingYPos + j);
			}
		}
	}

	public void removeMultiTiledObjectPlaceUnder(int startingXPos, int startingYPos, int rotation)
	{
		int num = xSize;
		int num2 = ySize;
		if (WorldManager.Instance.onTileMap[startingXPos, startingYPos] > -1 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[startingXPos, startingYPos]].tileObjectBridge)
		{
			num2 = WorldManager.Instance.onTileStatusMap[startingXPos, startingYPos];
			if (rotation == 2 || rotation == 4)
			{
				num = num2;
				num2 = xSize;
			}
		}
		else if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (tileObjectId == WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] || WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] <= -100)
				{
					WorldManager.Instance.onTileMap[startingXPos + i, startingYPos + j] = -1;
					Vector3 position = new Vector3((float)(startingXPos + i) * 2f, WorldManager.Instance.heightMap[startingXPos + i, startingYPos + j], (float)(startingYPos + j) * 2f);
					ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], position, 2);
					WorldManager.Instance.onTileChunkHasChanged(startingXPos + i, startingYPos + j);
					WorldManager.Instance.placeFenceInChunk(startingXPos + i, startingYPos + j);
				}
			}
		}
	}

	public void removeMultiTiledObjectInside(int startingXPos, int startingYPos, int rotation, HouseDetails removeFrom)
	{
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				removeFrom.houseMapOnTile[startingXPos + i, startingYPos + j] = -1;
			}
		}
	}

	public void onDeathServer(int xPos, int yPos, HouseDetails inside, TileObjectGrowthStages tileObjectGrowthStages, Transform _transform, Transform[] dropObjectFromPositions)
	{
		int num = 0;
		int num2 = WorldManager.Instance.onTileStatusMap[xPos, yPos];
		if (inside != null)
		{
			num2 = inside.houseMapOnTileStatus[xPos, yPos];
		}
		if (dropsStatusNumberOnDeath && !pickUpRequiresEmptyPocket)
		{
			NetworkMapSharer.Instance.spawnAServerDrop(num2, 1, _transform.position, inside);
		}
		else if (((bool)WorldManager.Instance.allObjectSettings[tileObjectId].dropsItemOnDeath || (bool)WorldManager.Instance.allObjectSettings[tileObjectId].dropFromLootTable) && (!onlyDropWhenGrown || (onlyDropWhenGrown && WorldManager.Instance.onTileStatusMap[xPos, yPos] >= WorldManager.Instance.allObjects[tileObjectId].tileObjectGrowthStages.objectStages.Length + WorldManager.Instance.allObjects[tileObjectId].tileObjectGrowthStages.takeOrAddFromStateOnHarvest - 1)))
		{
			if (dropObjectFromPositions.Length == 0)
			{
				if (!tileObjectGrowthStages || ((bool)tileObjectGrowthStages && tileObjectGrowthStages.dropsForStages.Length == 0) || num < tileObjectGrowthStages.dropsForStages[num2])
				{
					num++;
					if ((bool)WorldManager.Instance.allObjectSettings[tileObjectId].dropFromLootTable)
					{
						InventoryItem randomDropFromTable = WorldManager.Instance.allObjectSettings[tileObjectId].dropFromLootTable.getRandomDropFromTable();
						if (randomDropFromTable != null)
						{
							if (randomDropFromTable.hasFuel)
							{
								NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(randomDropFromTable), Random.Range(10, (int)((float)randomDropFromTable.fuelMax / 1.5f)), _transform.position, inside, tryNotToStack: true, WorldManager.Instance.allObjects[tileObjectId].getXpTallyType());
							}
							else
							{
								NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(randomDropFromTable), 1, _transform.position, inside, tryNotToStack: true, WorldManager.Instance.allObjects[tileObjectId].getXpTallyType());
							}
						}
					}
					else if (WorldManager.Instance.allObjectSettings[tileObjectId].canBePickedUp)
					{
						NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(WorldManager.Instance.allObjectSettings[tileObjectId].dropsItemOnDeath), 1, _transform.position + Vector3.up, inside, tryNotToStack: false, WorldManager.Instance.allObjects[tileObjectId].getXpTallyType());
					}
					else
					{
						NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(WorldManager.Instance.allObjectSettings[tileObjectId].dropsItemOnDeath), 1, _transform.position + Vector3.up, inside, tryNotToStack: true, WorldManager.Instance.allObjects[tileObjectId].getXpTallyType());
					}
				}
			}
			else
			{
				foreach (Transform transform in dropObjectFromPositions)
				{
					if ((bool)tileObjectGrowthStages && (!tileObjectGrowthStages || tileObjectGrowthStages.dropsForStages.Length != 0) && num >= tileObjectGrowthStages.dropsForStages[num2])
					{
						continue;
					}
					num++;
					if ((bool)WorldManager.Instance.allObjectSettings[tileObjectId].dropFromLootTable)
					{
						InventoryItem randomDropFromTable2 = WorldManager.Instance.allObjectSettings[tileObjectId].dropFromLootTable.getRandomDropFromTable();
						if (randomDropFromTable2 != null)
						{
							if (randomDropFromTable2.hasFuel)
							{
								NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(randomDropFromTable2), Random.Range(10, (int)((float)randomDropFromTable2.fuelMax / 1.5f)), transform.position, inside, tryNotToStack: true, WorldManager.Instance.allObjects[tileObjectId].getXpTallyType());
							}
							else
							{
								NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(randomDropFromTable2), 1, transform.position, inside, tryNotToStack: true, WorldManager.Instance.allObjects[tileObjectId].getXpTallyType());
							}
						}
					}
					else if (WorldManager.Instance.allObjectSettings[tileObjectId].canBePickedUp)
					{
						NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(WorldManager.Instance.allObjectSettings[tileObjectId].dropsItemOnDeath), 1, transform.position, inside, tryNotToStack: false, WorldManager.Instance.allObjects[tileObjectId].getXpTallyType());
					}
					else
					{
						NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(WorldManager.Instance.allObjectSettings[tileObjectId].dropsItemOnDeath), 1, transform.position, inside, tryNotToStack: true, WorldManager.Instance.allObjects[tileObjectId].getXpTallyType());
					}
				}
			}
		}
		if ((bool)tileObjectGrowthStages && tileObjectGrowthStages.canBeHarvested(WorldManager.Instance.onTileStatusMap[xPos, yPos]))
		{
			tileObjectGrowthStages.harvest(xPos, yPos);
		}
		if ((bool)WorldManager.Instance.allObjectSettings[tileObjectId].dropsObjectOnDeath)
		{
			NetworkMapSharer.Instance.RpcSpawnATileObjectDrop(tileObjectId, xPos, yPos, num2);
		}
		if ((bool)WorldManager.Instance.allObjectSettings[tileObjectId].spawnCarryableOnDeath && WorldManager.Instance.allObjectSettings[tileObjectId].carryableChance >= Random.Range(0f, 100f))
		{
			Vector3 pos = new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2);
			NetworkMapSharer.Instance.spawnACarryable(WorldManager.Instance.allObjectSettings[tileObjectId].spawnCarryableOnDeath, pos);
		}
		if (statusObjectsPickUpFirst.Length != 0 && num2 > 0)
		{
			if ((bool)statusObjectsPickUpFirst[num2].placeable)
			{
				WorldManager.Instance.allObjectSettings[statusObjectsPickUpFirst[num2].placeable.tileObjectId].removeBeauty();
			}
			NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(statusObjectsPickUpFirst[num2]), 1, _transform.position + Vector3.up, inside, tryNotToStack: true);
		}
	}
}
