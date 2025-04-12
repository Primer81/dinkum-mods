using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class DailyTaskGenerator : MonoBehaviour
{
	public enum genericTaskType
	{
		None,
		Harvest,
		TalkToPeople,
		CatchBugs,
		CraftAnything,
		EatSomething,
		TravelDistance,
		BuyItems,
		SellItems,
		CookMeat,
		PlantSeeds,
		WaterCrops,
		SmashRocks,
		CollectOre,
		SmeltOre,
		GrindObjects,
		ChopDownTree,
		ChopDownStump,
		CutPlanks,
		CatchFish,
		CutGrass,
		HuntAnimals,
		TrapAnAnimal,
		BuyFurniture,
		BuyWallpaper,
		BuyFlooring,
		BuySeeds,
		HarvestCrops,
		BuyShirt,
		CookSomeFruit,
		CookAtCookingTable,
		PlantTreeSeed,
		SellFish,
		SellBugs,
		SellShells,
		SellCrops,
		DigUpTreasure,
		BuryFruit,
		DiveForTreasure,
		PetAnimals,
		FeedAnimals,
		EnterMines,
		DoAFavourSomeone,
		GetPoisoned,
		TakeDamage,
		Faint,
		GetLetters,
		UpgradeHouse,
		PlaceBaseTent,
		PlaceOwnTent,
		CompleteABuilding,
		BuyATool,
		BreakATool,
		CompleteABulletinBoardRequest,
		AddNewBugToPedia,
		AddNewFishToPedia,
		AddNewAnimalToPedia,
		ProcessAnimalProduct,
		SellAnEgg,
		PlantWildSeeds,
		SoilMover,
		CraftATool,
		TravelDistanceOnVehicle,
		GetAHairCut,
		DepositMoneyIntoBank,
		BuyAnAnimal,
		EmptyCrabPot,
		FossileFinder,
		EggTheif,
		HiveFinder,
		CollectHoney,
		PaintVehicle,
		SharkHunter,
		CollectShells,
		BrewSomething,
		TeleportSomewhere,
		UseCompostBin,
		Wheelbarrow,
		LawnMower,
		FlyAHelicopter,
		GetATractor,
		PutBucketOnHead,
		Photographer,
		HangGlide,
		UnlockTreasure,
		MilkAnimal,
		ShearAnimal,
		SellFruit,
		SellAmber,
		GiveBirthdayGift,
		SellRuby,
		GetHitByLightning,
		SellThunderEgg,
		HuntAlphaAnimal,
		CookFish,
		CatchACritter,
		PlaceInBugComp,
		RideAnAnimal,
		PlaceInFishingComp,
		UseTheDunny,
		SellMeteorite,
		MakeAWish
	}

	public static DailyTaskGenerator generate;

	public TaskIcon[] taskIcons;

	public Task[] currentTasks;

	public TileObject[] harvestables;

	private InventoryItem[] seeds;

	public Sprite[] taskSprites;

	private int[] loadedTaskCompletion;

	public List<int> doublesCheck = new List<int>();

	private void Awake()
	{
		generate = this;
	}

	private void Start()
	{
		LocalizationManager.OnLocalizeEvent += RefreshTaskOnLanguageChange;
	}

	public void startDistanceChecker()
	{
		StartCoroutine(distanceTracker());
	}

	public void generateFirstDailyTasks()
	{
		doublesCheck.Clear();
		currentTasks = new Task[3];
		for (int i = 0; i < 3; i++)
		{
			currentTasks[i] = new Task(i, 1);
			doublesCheck.Add(currentTasks[i].taskTypeId);
			taskIcons[i].fillWithDetails(currentTasks[i]);
			taskIcons[i].gameObject.SetActive(value: true);
		}
		loadDailyTaskCompletion();
	}

	private void loadDailyTaskCompletion()
	{
		if (loadedTaskCompletion != null)
		{
			for (int i = 0; i < loadedTaskCompletion.Length; i++)
			{
				currentTasks[i].points = loadedTaskCompletion[i];
				taskIcons[i].fillWithDetails(currentTasks[i]);
			}
		}
		if (loadedTaskCompletion != null)
		{
			for (int j = 0; j < loadedTaskCompletion.Length; j++)
			{
				loadedTaskCompletion[j] = 0;
			}
		}
	}

	public void fillDailyTasksFromLoad(int[] newLoadedPoints)
	{
		loadedTaskCompletion = newLoadedPoints;
	}

	public void generateNewDailyTasks()
	{
		UnityEngine.Random.InitState(NetworkMapSharer.Instance.mineSeed + NetworkMapSharer.Instance.tomorrowsMineSeed);
		doublesCheck.Clear();
		currentTasks = new Task[3];
		int taskIdMax = Enum.GetNames(typeof(genericTaskType)).Length;
		for (int i = 0; i < 3; i++)
		{
			currentTasks[i] = new Task(taskIdMax);
			doublesCheck.Add(currentTasks[i].taskTypeId);
			taskIcons[i].fillWithDetails(currentTasks[i]);
			taskIcons[i].gameObject.SetActive(value: true);
		}
		CurrencyWindows.currency.closeJournal();
		loadDailyTaskCompletion();
	}

	public void RefreshTaskOnLanguageChange()
	{
		if (currentTasks != null)
		{
			for (int i = 0; i < 3; i++)
			{
				taskIcons[i].fillWithDetails(currentTasks[i]);
			}
		}
	}

	public bool checkIfTaskDoubled(int toCheck)
	{
		return doublesCheck.Contains(toCheck);
	}

	public TileObject generateRandomHarvestable()
	{
		return harvestables[UnityEngine.Random.Range(0, harvestables.Length)];
	}

	public void doATask(genericTaskType taskType, int addAmount = 1)
	{
		if (taskType == genericTaskType.None)
		{
			return;
		}
		MilestoneManager.manage.doATaskAndCountToMilestone(taskType, addAmount);
		if (currentTasks == null)
		{
			return;
		}
		for (int i = 0; i < currentTasks.Length; i++)
		{
			if (currentTasks[i].taskTypeId == (int)taskType)
			{
				currentTasks[i].points += addAmount;
				if (currentTasks[i].points >= currentTasks[i].requiredPoints && !currentTasks[i].completed)
				{
					currentTasks[i].completed = true;
					taskIcons[i].completeTask();
				}
				taskIcons[i].fillWithDetails(currentTasks[i]);
			}
		}
	}

	public void doATaskTileObject(genericTaskType taskType, int tileId, int addAmount = 1)
	{
		if (taskType == genericTaskType.None)
		{
			return;
		}
		MilestoneManager.manage.doATaskAndCountToMilestone(taskType, addAmount);
		if (currentTasks == null)
		{
			return;
		}
		for (int i = 0; i < currentTasks.Length; i++)
		{
			if (currentTasks[i].taskTypeId == (int)taskType && currentTasks[i].tileObjectToInteract.tileObjectId == tileId)
			{
				currentTasks[i].points += addAmount;
				if (currentTasks[i].points >= currentTasks[i].requiredPoints && !currentTasks[i].completed)
				{
					currentTasks[i].completed = true;
					taskIcons[i].completeTask();
				}
				taskIcons[i].fillWithDetails(currentTasks[i]);
			}
		}
	}

	public IEnumerator distanceTracker()
	{
		while (!NetworkMapSharer.Instance.localChar)
		{
			yield return null;
		}
		Transform charTrans = NetworkMapSharer.Instance.localChar.transform;
		Vector3 lastPos = charTrans.position;
		float distance = 0f;
		while (true)
		{
			if (charTrans == null)
			{
				yield return null;
			}
			float num = Vector3.Distance(lastPos, charTrans.position);
			if (num > 0.1f && num < 3f)
			{
				distance += num;
			}
			lastPos = charTrans.position;
			if (distance >= 1f)
			{
				if (NetworkMapSharer.Instance.localChar.myPickUp.drivingVehicle || charTrans.parent != null)
				{
					if ((bool)NetworkMapSharer.Instance.localChar.myPickUp.currentlyDriving && (bool)NetworkMapSharer.Instance.localChar.myPickUp.currentlyDriving.myAnimalAnimations)
					{
						doATask(genericTaskType.RideAnAnimal);
					}
					else
					{
						doATask(genericTaskType.TravelDistanceOnVehicle);
					}
					if ((bool)NetworkMapSharer.Instance.localChar.myPickUp.currentlyDriving && NetworkMapSharer.Instance.localChar.myPickUp.currentlyDriving.saveId == 6)
					{
						doATask(genericTaskType.FlyAHelicopter);
					}
				}
				else if (NetworkMapSharer.Instance.localChar.usingHangGlider)
				{
					doATask(genericTaskType.HangGlide);
				}
				else
				{
					doATask(genericTaskType.TravelDistance);
				}
				distance = 0f;
			}
			yield return null;
		}
	}

	private void OnDestroy()
	{
		LocalizationManager.OnLocalizeEvent -= RefreshTaskOnLanguageChange;
	}
}
