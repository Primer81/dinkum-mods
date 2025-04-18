using System;
using UnityEngine;

[Serializable]
public class FarmAnimalDetails
{
	public int animalId;

	public string animalName = "Bob";

	public int agentListId;

	public int animalRelationShip;

	public int animalAge;

	public int animalVariation = -1;

	public bool hasBeenPatted;

	public bool hasEaten;

	public bool ateYesterDay;

	public bool hasDoneDrop;

	public float[] currentPosition = new float[3];

	public int houseX = -1;

	public int houseY = -1;

	public FarmAnimalDetails()
	{
	}

	public FarmAnimalDetails(int id, int variation, string name)
	{
		animalId = id;
		animalVariation = variation;
		animalName = name;
	}

	public void endOfDayCheck()
	{
		animalAge++;
		if (!hasEaten)
		{
			setPosition(FarmAnimalManager.manage.activeAnimalAgents[agentListId].transform.position);
			Vector3 vector = WorldManager.Instance.findTileObjectAround(new Vector3(currentPosition[0], currentPosition[1], currentPosition[2]), AnimalManager.manage.allAnimals[animalId].GetComponent<AnimalAILookForFood>().eatsTiles, 25);
			if (vector != Vector3.zero)
			{
				if ((bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[(int)vector.x / 2, (int)vector.z / 2]].tileOnOff)
				{
					NetworkMapSharer.Instance.RpcOpenCloseTile((int)vector.x / 2, (int)vector.z / 2, 0);
					WorldManager.Instance.onTileStatusMap[(int)vector.x / 2, (int)vector.z / 2] = 0;
				}
				else
				{
					NetworkMapSharer.Instance.RpcUpdateOnTileObject(-1, (int)vector.x / 2, (int)vector.z / 2);
				}
				WorldManager.Instance.setChunkHasChangedToday((int)vector.x / 2, (int)vector.z / 2);
				hasEaten = true;
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.FeedAnimals);
			}
		}
		ateYesterDay = hasEaten;
		int num = 50;
		int num2 = 0;
		if (hasBeenPatted)
		{
			num += 25;
		}
		if (hasHouse())
		{
			num += 25;
		}
		num2 = ((!hasEaten) ? (num2 - 2) : (num2 + 3));
		if (hasBeenPatted)
		{
			num2 += 2;
		}
		if (WeatherManager.Instance.rainMgr.IsActive && !hasHouse())
		{
			num2 -= 5;
		}
		if (animalRelationShip < num)
		{
			animalRelationShip += num2;
			if (animalRelationShip > num)
			{
				animalRelationShip = num;
			}
		}
		else
		{
			animalRelationShip += num2;
		}
		animalRelationShip = Mathf.Clamp(animalRelationShip, 0, 100);
		hasEaten = false;
		hasBeenPatted = false;
		FarmAnimal component = AnimalManager.manage.allAnimals[animalId].GetComponent<FarmAnimal>();
		if ((bool)component.growsInto && animalAge >= component.growUpAge)
		{
			FarmAnimal farmAnimal = FarmAnimalManager.manage.activeAnimalAgents[agentListId];
			NetworkNavMesh.nav.UnSpawnAnAnimal(farmAnimal.GetComponent<AnimalAI>(), saveToMap: false);
			animalId = component.growsInto.GetComponent<AnimalAI>().animalId;
			if ((bool)AnimalManager.manage.allAnimals[animalId].hasVariation)
			{
				animalVariation = AnimalManager.manage.allAnimals[animalId].getRandomVariationNo();
			}
			NetworkNavMesh.nav.SpawnFarmAnimal(this, animalGrew: true);
		}
		FarmAnimalManager.manage.checkLiveAgentAfterFeed(this);
		FarmAnimalManager.manage.activeAnimalAgents[agentListId].updateAnimalDetails(this);
		hasDoneDrop = false;
	}

	public void setPosition(Vector3 pos)
	{
		currentPosition[0] = pos.x;
		currentPosition[1] = pos.y;
		currentPosition[2] = pos.z;
	}

	public void onAnimalDeath()
	{
		FarmAnimalManager.manage.activeAnimalAgents[agentListId] = null;
		FarmAnimalManager.manage.farmAnimalDetails.Remove(this);
		clearFromHouse();
	}

	public void clearFromHouse()
	{
		if (houseX != -1 && houseY != -1)
		{
			AnimalHouse animalHouse = FarmAnimalManager.manage.findHouseById(houseX, houseY);
			if (animalHouse != null)
			{
				animalHouse.belongsToAnimal = false;
			}
			houseX = -1;
			houseY = -1;
		}
	}

	public bool isHouseAtPos(int xPos, int yPos)
	{
		if (xPos == houseX)
		{
			return yPos == houseY;
		}
		return false;
	}

	public Vector2 getHousePos()
	{
		return new Vector2(houseX, houseY);
	}

	public void moveIntoAnimalHouse(int newHouseX, int newHouseY)
	{
		AnimalHouse animalHouse = FarmAnimalManager.manage.findHouseById(newHouseX, newHouseY);
		if (animalHouse != null)
		{
			animalHouse.belongsToAnimal = true;
		}
		houseX = newHouseX;
		houseY = newHouseY;
	}

	public void sell()
	{
		if (FarmAnimalManager.manage.activeAnimalAgents[agentListId] != null)
		{
			NetworkNavMesh.nav.UnSpawnAnAnimal(FarmAnimalManager.manage.activeAnimalAgents[agentListId].GetComponent<AnimalAI>(), saveToMap: false);
		}
		FarmAnimalManager.manage.activeAnimalAgents[agentListId] = null;
		clearFromHouse();
		FarmAnimalManager.manage.farmAnimalDetails.Remove(this);
	}

	public bool hasHouse()
	{
		if (houseX == -1 && houseY == -1)
		{
			return false;
		}
		return true;
	}

	public bool isCurrentlyOnChunk(int chunkX, int chunkY)
	{
		if (chunkX == (int)(Mathf.Round(currentPosition[0]) / 2f) / WorldManager.Instance.chunkSize * WorldManager.Instance.chunkSize && chunkY == (int)(Mathf.Round(currentPosition[2]) / 2f) / WorldManager.Instance.chunkSize * WorldManager.Instance.chunkSize)
		{
			return true;
		}
		return false;
	}
}
