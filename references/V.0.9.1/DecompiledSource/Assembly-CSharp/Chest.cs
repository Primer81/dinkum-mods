using System;

[Serializable]
public class Chest
{
	public bool inside;

	public int insideX = -1;

	public int insideY = -1;

	public int xPos;

	public int yPos;

	public int[] itemIds = new int[24];

	public int[] itemStacks = new int[24];

	public int playingLookingInside;

	public int placedInWorldLevel;

	public Chest(int xPosIn, int yPosIn)
	{
		xPos = xPosIn;
		yPos = yPosIn;
	}

	public bool IsOnCorrectLevel()
	{
		if (RealWorldTimeLight.time.underGround && placedInWorldLevel == 1)
		{
			return true;
		}
		if (RealWorldTimeLight.time.offIsland && placedInWorldLevel == 2)
		{
			return true;
		}
		return false;
	}

	public void SetCorrectLevel()
	{
		if (RealWorldTimeLight.time.underGround)
		{
			placedInWorldLevel = 1;
		}
		else if (RealWorldTimeLight.time.offIsland)
		{
			placedInWorldLevel = 2;
		}
		else
		{
			placedInWorldLevel = 0;
		}
	}

	public void SetToUnderGround()
	{
		placedInWorldLevel = 1;
	}

	public void SetToOffIsland()
	{
		placedInWorldLevel = 2;
	}

	public int GetAmountOfItemInside(int checkingId)
	{
		int num = 0;
		for (int i = 0; i < itemIds.Length; i++)
		{
			if (itemIds[i] == checkingId)
			{
				num = ((!Inventory.Instance.allItems[itemIds[i]].isATool && !Inventory.Instance.allItems[itemIds[i]].hasFuel) ? (num + itemStacks[i]) : (num + 1));
			}
		}
		return num;
	}

	public void SetNewChestPosition(int newX, int newY, int newHouseX, int newHouseY)
	{
		playingLookingInside = 0;
		xPos = newX;
		yPos = newY;
		insideX = newHouseX;
		insideY = newHouseY;
	}

	public bool IsAutoSorter()
	{
		if (insideX == -1 && insideY == -1)
		{
			if (WorldManager.Instance.onTileMap[xPos, yPos] >= 0 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest)
			{
				return WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest.isAutoSorter;
			}
		}
		else
		{
			HouseDetails houseInfoIfExists = HouseManager.manage.getHouseInfoIfExists(insideX, insideY);
			if (houseInfoIfExists != null && houseInfoIfExists.houseMapOnTile[xPos, yPos] >= 0 && (bool)WorldManager.Instance.allObjects[houseInfoIfExists.houseMapOnTile[xPos, yPos]].tileObjectChest)
			{
				return WorldManager.Instance.allObjects[houseInfoIfExists.houseMapOnTile[xPos, yPos]].tileObjectChest.isAutoSorter;
			}
		}
		return false;
	}

	public bool IsMannequin()
	{
		if (insideX == -1 && insideY == -1)
		{
			if (WorldManager.Instance.onTileMap[xPos, yPos] >= 0 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest)
			{
				return WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest.isMannequin;
			}
		}
		else
		{
			HouseDetails houseInfoIfExists = HouseManager.manage.getHouseInfoIfExists(insideX, insideY);
			if (houseInfoIfExists != null && houseInfoIfExists.houseMapOnTile[xPos, yPos] >= 0 && (bool)WorldManager.Instance.allObjects[houseInfoIfExists.houseMapOnTile[xPos, yPos]].tileObjectChest)
			{
				return WorldManager.Instance.allObjects[houseInfoIfExists.houseMapOnTile[xPos, yPos]].tileObjectChest.isMannequin;
			}
		}
		return false;
	}

	public bool IsToolRack()
	{
		if (insideX == -1 && insideY == -1)
		{
			if (WorldManager.Instance.onTileMap[xPos, yPos] >= 0 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest)
			{
				return WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest.isToolRack;
			}
		}
		else
		{
			HouseDetails houseInfoIfExists = HouseManager.manage.getHouseInfoIfExists(insideX, insideY);
			if (houseInfoIfExists != null && houseInfoIfExists.houseMapOnTile[xPos, yPos] >= 0 && (bool)WorldManager.Instance.allObjects[houseInfoIfExists.houseMapOnTile[xPos, yPos]].tileObjectChest)
			{
				return WorldManager.Instance.allObjects[houseInfoIfExists.houseMapOnTile[xPos, yPos]].tileObjectChest.isToolRack;
			}
		}
		return false;
	}

	public bool IsDisplayStand()
	{
		if (insideX == -1 && insideY == -1)
		{
			if (WorldManager.Instance.onTileMap[xPos, yPos] >= 0 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest)
			{
				return WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest.isDisplayStand;
			}
		}
		else
		{
			HouseDetails houseInfoIfExists = HouseManager.manage.getHouseInfoIfExists(insideX, insideY);
			if (houseInfoIfExists != null && houseInfoIfExists.houseMapOnTile[xPos, yPos] >= 0 && (bool)WorldManager.Instance.allObjects[houseInfoIfExists.houseMapOnTile[xPos, yPos]].tileObjectChest)
			{
				return WorldManager.Instance.allObjects[houseInfoIfExists.houseMapOnTile[xPos, yPos]].tileObjectChest.isDisplayStand;
			}
		}
		return false;
	}

	public bool IsFishPond()
	{
		if (insideX == -1 && insideY == -1 && WorldManager.Instance.onTileMap[xPos, yPos] >= 0 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest)
		{
			return WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest.isFishPond;
		}
		return false;
	}

	public bool IsBugTerrarium()
	{
		if (insideX == -1 && insideY == -1 && WorldManager.Instance.onTileMap[xPos, yPos] >= 0 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest)
		{
			return WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest.isBugTerrarium;
		}
		return false;
	}
}
