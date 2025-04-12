using UnityEngine;

public class Task
{
	public int points;

	public int requiredPoints;

	public int taskTypeId;

	public int specificNPC = -1;

	public int reward = 25;

	public TileObject tileObjectToInteract;

	public string missionText = "";

	public bool completed;

	public Task(int taskIdMax)
	{
		generateTask(taskIdMax);
	}

	public Task(int firstDailyTaskNo, int taskIdMax)
	{
		if (firstDailyTaskNo == 0)
		{
			taskTypeId = 1;
			tileObjectToInteract = DailyTaskGenerator.generate.harvestables[0];
			requiredPoints = 3;
			if ((bool)tileObjectToInteract.tileObjectGrowthStages.harvestDrop)
			{
				missionText = ConversationGenerator.generate.GetDailyChallengeByTag("HarvestAmountName", requiredPoints, tileObjectToInteract.tileObjectGrowthStages.harvestDrop.getInvItemName(requiredPoints));
			}
			else
			{
				missionText = ConversationGenerator.generate.GetDailyChallengeByTag("HarvestName", 1, tileObjectToInteract.name);
			}
			reward = 10 * requiredPoints;
		}
		if (firstDailyTaskNo == 1)
		{
			taskTypeId = 3;
			requiredPoints = 2;
			missionText = ConversationGenerator.generate.GetDailyChallengeByTag("CatchBugsAmount", requiredPoints);
			reward = 25 * requiredPoints;
		}
		if (firstDailyTaskNo == 2)
		{
			taskTypeId = 4;
			requiredPoints = 1;
			missionText = ConversationGenerator.generate.GetDailyChallengeByTag("CraftSomethingAmount", requiredPoints);
			reward = 25 * requiredPoints;
		}
	}

	public string GetMissionText()
	{
		if (1 == taskTypeId)
		{
			if ((bool)tileObjectToInteract && (bool)tileObjectToInteract.tileObjectGrowthStages.harvestDrop)
			{
				return ConversationGenerator.generate.GetDailyChallengeByTag("HarvestAmountName", requiredPoints, tileObjectToInteract.tileObjectGrowthStages.harvestDrop.getInvItemName(requiredPoints));
			}
			return ConversationGenerator.generate.GetDailyChallengeByTag("HarvestName", 1, tileObjectToInteract.name);
		}
		if (2 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("ChatWithAmountResidents", requiredPoints);
		}
		if (37 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("BuryAmountFruit", requiredPoints);
		}
		if (73 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("CollectShellsAmount", requiredPoints);
		}
		if (34 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("SellShellsAmount", requiredPoints);
		}
		if (87 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("SellFruitAmount", requiredPoints);
		}
		if (42 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("DoAJob", requiredPoints);
		}
		if (59 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("PlantWildSeedAmount", requiredPoints);
		}
		if (60 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("DigUpDirtAmount", requiredPoints);
		}
		if (3 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("CatchBugsAmount", requiredPoints);
		}
		if (33 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("SellBugsAmount", requiredPoints);
		}
		if (4 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("CraftSomethingAmount", requiredPoints);
		}
		if (5 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("EatSomething", requiredPoints);
		}
		if (8 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("MakeDinks", requiredPoints, Inventory.Instance.moneyItem.getInvItemName(requiredPoints));
		}
		if (7 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("SpendDinks", requiredPoints, Inventory.Instance.moneyItem.getInvItemName(requiredPoints));
		}
		if (6 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("TravelAmountFoot", requiredPoints);
		}
		if (62 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("TravelAmountVehicle", requiredPoints);
		}
		if (9 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("CookMeatAmount", requiredPoints);
		}
		if (29 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("CookFruitAmount", requiredPoints);
		}
		if (30 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("CookAtCookingTable", requiredPoints);
		}
		if (31 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("PlantTreeSeedAmount", requiredPoints);
		}
		if (10 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("PlantCropSeedAmount", requiredPoints);
		}
		if (11 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("WaterCropAmount", requiredPoints);
		}
		if (12 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("SmashRockAmount", requiredPoints);
		}
		if (13 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("SmashOreRockAmount", requiredPoints);
		}
		if (14 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("SmeltOreAmount", requiredPoints);
		}
		if (15 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("GringStoneAmount", requiredPoints);
		}
		if (16 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("CutDownTreeAmount", requiredPoints);
		}
		if (17 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("CutStumpTreeAmount", requiredPoints);
		}
		if (18 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("SawPlanksAmount", requiredPoints);
		}
		if (19 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("CatchFishAmount", requiredPoints);
		}
		if (32 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("SellFishAmount", requiredPoints);
		}
		if (20 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("ClearGrassAmount", requiredPoints);
		}
		if (39 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("PetAnAnimal", requiredPoints);
		}
		if (28 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("BuyNewClothes", requiredPoints);
		}
		if (23 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("BuyNewFurniture", requiredPoints);
		}
		if (24 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("BuyNewWallpaper", requiredPoints);
		}
		if (25 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("BuyNewFlooring", requiredPoints);
		}
		if (27 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("HarvestCropAmount", requiredPoints);
		}
		if (35 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("SellCropAmount", requiredPoints);
		}
		if (76 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("CompostSomething", requiredPoints);
		}
		if (61 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("CraftNewTool", requiredPoints);
		}
		if (26 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("SellCropSeedsAmount", requiredPoints);
		}
		if (22 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("TrapAnAnimal", requiredPoints);
		}
		if (21 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("HuntAnimalAmount", requiredPoints);
		}
		if (51 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("BuyANewTool", requiredPoints);
		}
		if (52 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("BreakATool", requiredPoints);
		}
		if (36 == taskTypeId)
		{
			return ConversationGenerator.generate.GetDailyChallengeByTag("FindSomeTreasure", requiredPoints);
		}
		return "No mission text";
	}

	public void generateTask(int taskIdMax)
	{
		specificNPC = -1;
		bool flag = false;
		while (!flag)
		{
			taskTypeId = Random.Range(1, taskIdMax);
			if (1 == taskTypeId)
			{
				requiredPoints = Random.Range(3, 6);
				tileObjectToInteract = DailyTaskGenerator.generate.generateRandomHarvestable();
				requiredPoints *= tileObjectToInteract.tileObjectGrowthStages.harvestSpots.Length;
				_ = (bool)tileObjectToInteract.tileObjectGrowthStages.harvestDrop;
				reward = 10 * requiredPoints;
				flag = true;
			}
			else if (2 == taskTypeId && NPCManager.manage.getNoOfNPCsMovedIn() > 0)
			{
				int noOfNPCsMovedIn = NPCManager.manage.getNoOfNPCsMovedIn();
				requiredPoints = Random.Range(1, 4);
				Mathf.Clamp(requiredPoints, 1, noOfNPCsMovedIn);
				reward = 10 * requiredPoints;
				flag = true;
			}
			else if (37 == taskTypeId)
			{
				requiredPoints = Random.Range(3, 5);
				reward = 10 * requiredPoints;
				flag = true;
			}
			else if (73 == taskTypeId)
			{
				requiredPoints = Random.Range(5, 10);
				reward = 10 * requiredPoints;
				flag = true;
			}
			else if (WorldManager.Instance.day != 1 && 34 == taskTypeId)
			{
				if (!NPCManager.manage.NPCDetails[3].mySchedual.dayOff[WorldManager.Instance.day - 1])
				{
					requiredPoints = Random.Range(5, 10);
					reward = 10 * requiredPoints;
					flag = true;
				}
			}
			else if (WorldManager.Instance.day != 1 && 87 == taskTypeId)
			{
				if (!NPCManager.manage.NPCDetails[3].mySchedual.dayOff[WorldManager.Instance.day - 1])
				{
					requiredPoints = Random.Range(3, 5);
					reward = 10 * requiredPoints;
					flag = true;
				}
			}
			else if (42 == taskTypeId)
			{
				requiredPoints = 1;
				reward = 100;
				flag = true;
			}
			else if (59 == taskTypeId)
			{
				requiredPoints = Random.Range(3, 5);
				reward = 10 * requiredPoints;
				flag = true;
			}
			else if (60 == taskTypeId && LicenceManager.manage.allLicences[16].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(3, 5);
				reward = 10 * requiredPoints;
				flag = true;
			}
			else if (3 == taskTypeId)
			{
				requiredPoints = Random.Range(3, 5);
				reward = 20 * requiredPoints;
				flag = true;
			}
			else if (WorldManager.Instance.day != 1 && 33 == taskTypeId)
			{
				if (!NPCManager.manage.NPCDetails[3].mySchedual.dayOff[WorldManager.Instance.day - 1])
				{
					requiredPoints = Random.Range(3, 5);
					reward = 20 * requiredPoints;
					flag = true;
				}
			}
			else if (4 == taskTypeId)
			{
				requiredPoints = Random.Range(2, 4);
				reward = 10 * requiredPoints;
				flag = true;
			}
			else if (5 == taskTypeId)
			{
				requiredPoints = 1;
				reward = 10;
				flag = true;
			}
			else if (WorldManager.Instance.day != 1 && 8 == taskTypeId && !NPCManager.manage.NPCDetails[1].mySchedual.dayOff[WorldManager.Instance.day - 1])
			{
				requiredPoints = Random.Range(2, 6);
				requiredPoints *= 1000;
				reward = 100 * (requiredPoints / 1000);
				flag = true;
			}
			else if (7 == taskTypeId)
			{
				requiredPoints = Random.Range(1, 4);
				requiredPoints *= 1000;
				reward = 100 * (requiredPoints / 1000);
				flag = true;
			}
			else if (6 == taskTypeId)
			{
				requiredPoints = Random.Range(1, 3);
				requiredPoints *= 500 / Random.Range(1, 2);
				reward = 50;
				flag = true;
			}
			else if (62 == taskTypeId && LicenceManager.manage.allLicences[7].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(1, 3);
				requiredPoints *= 500 / Random.Range(1, 2);
				reward = 50;
				flag = true;
			}
			else if (9 == taskTypeId)
			{
				requiredPoints = Random.Range(1, 3);
				reward = 20;
				flag = true;
			}
			else if (29 == taskTypeId)
			{
				requiredPoints = Random.Range(1, 3);
				reward = 20;
				flag = true;
			}
			else if (30 == taskTypeId)
			{
				requiredPoints = 1;
				reward = 100;
				flag = true;
			}
			else if (31 == taskTypeId)
			{
				requiredPoints = Random.Range(1, 4);
				reward = 25 * requiredPoints;
				flag = true;
			}
			else if (10 == taskTypeId && LicenceManager.manage.allLicences[11].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(3, 7);
				reward = 45 * requiredPoints;
				flag = true;
			}
			else if (11 == taskTypeId && LicenceManager.manage.allLicences[11].hasALevelOneOrHigher() && !WeatherManager.Instance.rainMgr.IsActive)
			{
				requiredPoints = Random.Range(3, 7);
				reward = 25 * requiredPoints;
				flag = true;
			}
			else if (12 == taskTypeId && LicenceManager.manage.allLicences[1].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(3, 7);
				reward = 15 * requiredPoints;
				flag = true;
			}
			else if (13 == taskTypeId && LicenceManager.manage.allLicences[1].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(3, 7);
				reward = 25 * requiredPoints;
				flag = true;
			}
			else if (14 == taskTypeId && LicenceManager.manage.allLicences[1].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(1, 3);
				reward = 50 * requiredPoints;
				flag = true;
			}
			else if (15 == taskTypeId && NPCManager.manage.npcStatus[1].checkIfHasMovedIn() && LicenceManager.manage.allLicences[1].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(2, 4);
				reward = 15 * requiredPoints;
				flag = true;
			}
			else if (16 == taskTypeId && LicenceManager.manage.allLicences[2].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(2, 5);
				reward = 15 * requiredPoints;
				flag = true;
			}
			else if (17 == taskTypeId && LicenceManager.manage.allLicences[2].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(2, 5);
				reward = 10 * requiredPoints;
				flag = true;
			}
			else if (18 == taskTypeId && LicenceManager.manage.allLicences[2].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(5, 9);
				reward = 5 * requiredPoints;
				flag = true;
			}
			else if (19 == taskTypeId && LicenceManager.manage.allLicences[3].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(3, 5);
				reward = 35 * requiredPoints;
				flag = true;
			}
			else if (WorldManager.Instance.day != 1 && 32 == taskTypeId && LicenceManager.manage.allLicences[3].hasALevelOneOrHigher())
			{
				if (!NPCManager.manage.NPCDetails[3].mySchedual.dayOff[WorldManager.Instance.day - 1])
				{
					requiredPoints = Random.Range(3, 5);
					reward = 35 * requiredPoints;
					flag = true;
				}
			}
			else if (20 == taskTypeId && LicenceManager.manage.allLicences[2].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(8, 15);
				reward = 25;
				flag = true;
			}
			else if (39 == taskTypeId && FarmAnimalManager.manage.isThereAtleastOneActiveAgent())
			{
				requiredPoints = 1;
				reward = 10;
				flag = true;
			}
			else if (28 == taskTypeId && (bool)TownManager.manage.allShopFloors[6])
			{
				if (!NPCManager.manage.NPCDetails[4].mySchedual.dayOff[WorldManager.Instance.day - 1])
				{
					requiredPoints = 1;
					reward = 25;
					flag = true;
				}
			}
			else if (23 == taskTypeId && (bool)TownManager.manage.allShopFloors[10])
			{
				if (!NPCManager.manage.NPCDetails[3].mySchedual.dayOff[WorldManager.Instance.day - 1])
				{
					requiredPoints = 1;
					reward = 25;
					flag = true;
				}
			}
			else if (24 == taskTypeId && (bool)TownManager.manage.allShopFloors[10])
			{
				if (!NPCManager.manage.NPCDetails[3].mySchedual.dayOff[WorldManager.Instance.day - 1])
				{
					requiredPoints = 1;
					reward = 25;
					flag = true;
				}
			}
			else if (25 == taskTypeId && (bool)TownManager.manage.allShopFloors[10])
			{
				if (!NPCManager.manage.NPCDetails[3].mySchedual.dayOff[WorldManager.Instance.day - 1])
				{
					requiredPoints = 1;
					reward = 25;
					flag = true;
				}
			}
			else if (WorldManager.Instance.getNoOfCompletedCrops() >= 4 && 27 == taskTypeId && LicenceManager.manage.allLicences[11].hasALevelOneOrHigher())
			{
				requiredPoints = Mathf.Clamp(Random.Range(4, 11), 4, WorldManager.Instance.getNoOfCompletedCrops());
				reward = 45 * requiredPoints;
				flag = true;
			}
			else if (WorldManager.Instance.day != 1 && WorldManager.Instance.getNoOfCompletedCrops() >= 4 && 35 == taskTypeId && LicenceManager.manage.allLicences[11].hasALevelOneOrHigher())
			{
				requiredPoints = Mathf.Clamp(Random.Range(4, 11), 4, WorldManager.Instance.getNoOfCompletedCrops());
				reward = 25 * requiredPoints;
				flag = true;
			}
			else if (76 == taskTypeId && LicenceManager.manage.allLicences[11].getCurrentLevel() >= 2)
			{
				requiredPoints = 1;
				reward = 25;
				flag = true;
			}
			else if (61 == taskTypeId)
			{
				requiredPoints = 1;
				reward = 100;
				flag = true;
			}
			else if (26 == taskTypeId && (bool)TownManager.manage.allShopFloors[11])
			{
				if (!NPCManager.manage.NPCDetails[0].mySchedual.dayOff[WorldManager.Instance.day - 1])
				{
					requiredPoints = Random.Range(3, 7);
					reward = 5 * requiredPoints;
					flag = true;
				}
			}
			else if (22 == taskTypeId && LicenceManager.manage.allLicences[15].hasALevelOneOrHigher())
			{
				requiredPoints = 1;
				reward = 100;
				flag = true;
			}
			else if (21 == taskTypeId && LicenceManager.manage.allLicences[4].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(2, 6);
				reward = 25 * requiredPoints;
				flag = true;
			}
			else if (51 == taskTypeId && !NPCManager.manage.NPCDetails[1].mySchedual.dayOff[WorldManager.Instance.day - 1])
			{
				requiredPoints = 1;
				reward = 10 * requiredPoints;
				flag = true;
			}
			else if (Inventory.Instance.checkIfToolNearlyBroken() && 52 == taskTypeId)
			{
				requiredPoints = 1;
				reward = 25 * requiredPoints;
				flag = true;
			}
			else if (36 == taskTypeId && LicenceManager.manage.allLicences[6].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(1, 3);
				reward = 30 * requiredPoints;
				flag = true;
			}
			else
			{
				flag = false;
			}
			missionText = "Mission Text";
			if (flag && DailyTaskGenerator.generate.checkIfTaskDoubled(taskTypeId))
			{
				flag = false;
			}
		}
	}
}
