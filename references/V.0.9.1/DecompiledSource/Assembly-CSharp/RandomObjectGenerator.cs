using System;
using System.Collections.Generic;
using UnityEngine;

public class RandomObjectGenerator : MonoBehaviour
{
	public static RandomObjectGenerator generate;

	public InventoryItem[] randomFoodToSell;

	public InventoryItem[] randomOresToSell;

	public InventoryItem[] randomBarToSell;

	public InventoryItem[] randomRelicToSell;

	public InventoryItemLootTable sparklingFishRewards;

	public InventoryItemLootTable sparklingBugRewards;

	public InventoryItemLootTable sparklingUndergroundFishRewards;

	private void Awake()
	{
		generate = this;
	}

	public InventoryItem getRandomShirtForGender(bool feminine)
	{
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].equipable && Inventory.Instance.allItems[i].equipable.shirt)
			{
				list.Add(Inventory.Instance.allItems[i]);
			}
		}
		if (!feminine)
		{
			int index = UnityEngine.Random.Range(0, list.Count);
			while (list[index].equipable.dress)
			{
				index = UnityEngine.Random.Range(0, list.Count);
			}
			return list[index];
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public int getRandomHair(bool feminine)
	{
		int[] array = new int[10] { 0, 1, 2, 3, 4, 8, 9, 12, 13, 15 };
		int num = UnityEngine.Random.Range(0, CharacterCreatorScript.create.allHairStyles.Length);
		bool flag = false;
		if (feminine)
		{
			while (!flag)
			{
				bool flag2 = false;
				for (int i = 0; i < array.Length; i++)
				{
					if (num == array[i])
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					flag = true;
				}
				else
				{
					num = UnityEngine.Random.Range(0, CharacterCreatorScript.create.allHairStyles.Length);
				}
			}
		}
		else
		{
			while (!flag)
			{
				for (int j = 0; j < array.Length; j++)
				{
					if (num == array[j])
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					num = UnityEngine.Random.Range(0, CharacterCreatorScript.create.allHairStyles.Length);
				}
			}
		}
		return num;
	}

	public InventoryItem getRandomPants(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if (!Inventory.Instance.allItems[i].isUniqueItem && (bool)Inventory.Instance.allItems[i].equipable && Inventory.Instance.allItems[i].equipable.cloths && Inventory.Instance.allItems[i].equipable.pants && !Inventory.Instance.allItems[i].equipable.dress && !Inventory.Instance.allItems[i].isOneOfKindUniqueItem)
			{
				list.Add(Inventory.Instance.allItems[i]);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public InventoryItem getRandomFaceItem(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if (!Inventory.Instance.allItems[i].isUniqueItem && (bool)Inventory.Instance.allItems[i].equipable && Inventory.Instance.allItems[i].equipable.cloths && Inventory.Instance.allItems[i].equipable.face && !Inventory.Instance.allItems[i].isOneOfKindUniqueItem)
			{
				list.Add(Inventory.Instance.allItems[i]);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public InventoryItem getRandomHat(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if (!Inventory.Instance.allItems[i].isUniqueItem && (bool)Inventory.Instance.allItems[i].equipable && Inventory.Instance.allItems[i].equipable.cloths && Inventory.Instance.allItems[i].equipable.hat && !Inventory.Instance.allItems[i].isOneOfKindUniqueItem)
			{
				list.Add(Inventory.Instance.allItems[i]);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public InventoryItem getRandomHatNoFaceItems(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if (!Inventory.Instance.allItems[i].isUniqueItem && (bool)Inventory.Instance.allItems[i].equipable && Inventory.Instance.allItems[i].equipable.cloths && Inventory.Instance.allItems[i].equipable.hat && !Inventory.Instance.allItems[i].equipable.face && !Inventory.Instance.allItems[i].isOneOfKindUniqueItem)
			{
				list.Add(Inventory.Instance.allItems[i]);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public InventoryItem getRandomShoes(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if (!Inventory.Instance.allItems[i].isUniqueItem && (bool)Inventory.Instance.allItems[i].equipable && Inventory.Instance.allItems[i].equipable.cloths && Inventory.Instance.allItems[i].equipable.shoes && !Inventory.Instance.allItems[i].isOneOfKindUniqueItem)
			{
				list.Add(Inventory.Instance.allItems[i]);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public InventoryItem getRandomShirtOrDressForShop(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if (!Inventory.Instance.allItems[i].isUniqueItem && (bool)Inventory.Instance.allItems[i].equipable && Inventory.Instance.allItems[i].equipable.cloths && (Inventory.Instance.allItems[i].equipable.shirt || Inventory.Instance.allItems[i].equipable.dress) && !Inventory.Instance.allItems[i].isOneOfKindUniqueItem)
			{
				list.Add(Inventory.Instance.allItems[i]);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public InventoryItem getRandomFurniture(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if (!Inventory.Instance.allItems[i].isUniqueItem && Inventory.Instance.allItems[i].isFurniture && !Inventory.Instance.allItems[i].isOneOfKindUniqueItem)
			{
				list.Add(Inventory.Instance.allItems[i]);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public InventoryItem getRandomFurnitureForShop(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if (!Inventory.Instance.allItems[i].isUniqueItem && Inventory.Instance.allItems[i].isFurniture && (bool)Inventory.Instance.allItems[i].placeable && !WorldManager.Instance.allObjectSettings[Inventory.Instance.allItems[i].placeable.tileObjectId].canBePlacedOnTopOfFurniture && !Inventory.Instance.allItems[i].isOneOfKindUniqueItem)
			{
				list.Add(Inventory.Instance.allItems[i]);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public InventoryItem getRandomOnTopFurniture(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if (!Inventory.Instance.allItems[i].isUniqueItem && Inventory.Instance.allItems[i].isFurniture && (bool)Inventory.Instance.allItems[i].placeable && WorldManager.Instance.allObjectSettings[Inventory.Instance.allItems[i].placeable.tileObjectId].canBePlacedOnTopOfFurniture && !Inventory.Instance.allItems[i].isOneOfKindUniqueItem)
			{
				list.Add(Inventory.Instance.allItems[i]);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public InventoryItem getRandomFlooring(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if (!Inventory.Instance.allItems[i].isUniqueItem && (bool)Inventory.Instance.allItems[i].equipable && Inventory.Instance.allItems[i].equipable.flooring && !Inventory.Instance.allItems[i].isOneOfKindUniqueItem)
			{
				list.Add(Inventory.Instance.allItems[i]);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public InventoryItem getRandomWallPaper(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if (!Inventory.Instance.allItems[i].isUniqueItem && (bool)Inventory.Instance.allItems[i].equipable && Inventory.Instance.allItems[i].equipable.wallpaper && !Inventory.Instance.allItems[i].isOneOfKindUniqueItem)
			{
				list.Add(Inventory.Instance.allItems[i]);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public int getRandomClothing(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		return UnityEngine.Random.Range(0, 5) switch
		{
			0 => getRandomFaceItem().getItemId(), 
			1 => getRandomHat().getItemId(), 
			2 => getRandomShirtOrDressForShop().getItemId(), 
			3 => getRandomPants().getItemId(), 
			4 => getRandomShoes().getItemId(), 
			_ => getRandomShirtOrDressForShop().getItemId(), 
		};
	}

	public void resetTheRandomSeed(bool reset)
	{
		if (reset)
		{
			UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
		}
	}

	public int getRandomCookedDish()
	{
		bool flag = false;
		while (!flag)
		{
			int num = UnityEngine.Random.Range(0, CharLevelManager.manage.recipesUnlockedFromBegining.Length);
			if ((bool)CharLevelManager.manage.recipesUnlockedFromBegining[num].consumeable)
			{
				return CharLevelManager.manage.recipesUnlockedFromBegining[num].getItemId();
			}
		}
		return 0;
	}

	public bool giveRandomRecipeFromBluePrint()
	{
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].craftable && Inventory.Instance.allItems[i].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.CookingTable && Inventory.Instance.allItems[i].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.CraftingShop && !Inventory.Instance.allItems[i].craftable.isDeed && !Inventory.Instance.allItems[i].craftable.learnThroughQuest && !Inventory.Instance.allItems[i].craftable.learnThroughLevels && !Inventory.Instance.allItems[i].craftable.learnThroughLicence && !CharLevelManager.manage.checkIfIsInStartingRecipes(i) && !CharLevelManager.manage.checkIfUnlocked(i) && Inventory.Instance.allItems[i].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.Blocked)
			{
				list.Add(Inventory.Instance.allItems[i]);
			}
		}
		if (list.Count > 0)
		{
			GiftedItemWindow.gifted.addRecipeToUnlock(list[UnityEngine.Random.Range(0, list.Count)].getItemId());
			GiftedItemWindow.gifted.openWindowAndGiveItems();
			return true;
		}
		return false;
	}

	public void getRandomItemForNPCToSell(out int id, out int amount)
	{
		switch (UnityEngine.Random.Range(0, 6))
		{
		case 0:
			id = getRandomClothing();
			amount = 1;
			break;
		case 1:
			id = getRandomFurniture().getItemId();
			amount = 1;
			break;
		case 2:
			id = randomFoodToSell[UnityEngine.Random.Range(0, randomFoodToSell.Length)].getItemId();
			amount = UnityEngine.Random.Range(1, 5);
			break;
		case 3:
			id = randomOresToSell[UnityEngine.Random.Range(0, randomOresToSell.Length)].getItemId();
			amount = UnityEngine.Random.Range(1, 6);
			break;
		case 4:
			id = randomBarToSell[UnityEngine.Random.Range(0, randomBarToSell.Length)].getItemId();
			amount = UnityEngine.Random.Range(2, 4);
			break;
		case 5:
			id = randomRelicToSell[UnityEngine.Random.Range(0, randomRelicToSell.Length)].getItemId();
			amount = UnityEngine.Random.Range(3, 10);
			break;
		default:
			id = getRandomClothing();
			amount = 1;
			break;
		}
	}

	public void GiveRandomFishingReward()
	{
		int itemId = sparklingFishRewards.getRandomDropFromTable().getItemId();
		if (RealWorldTimeLight.time.underGround)
		{
			itemId = sparklingUndergroundFishRewards.getRandomDropFromTable().getItemId();
		}
		BugAndFishCelebration.bugAndFishCel.AddSparklingReward(itemId);
	}

	public void GiveRandomBugReward()
	{
		int itemId = sparklingBugRewards.getRandomDropFromTable().getItemId();
		BugAndFishCelebration.bugAndFishCel.AddSparklingReward(itemId);
	}
}
