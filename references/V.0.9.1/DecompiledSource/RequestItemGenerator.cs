using System.Collections.Generic;
using UnityEngine;

public class RequestItemGenerator : MonoBehaviour
{
	public static RequestItemGenerator request;

	private List<InventoryItem> allMeat = new List<InventoryItem>();

	private List<InventoryItem> animalProduct = new List<InventoryItem>();

	private List<InventoryItem> allFruit = new List<InventoryItem>();

	private List<InventoryItem> cookedRecipe = new List<InventoryItem>();

	public InventoryItem[] woodLogs;

	public InventoryItem[] woodPlanks;

	public InventoryItem[] oreBars;

	private void Awake()
	{
		request = this;
	}

	private void Start()
	{
		fillRandomTables();
	}

	private void fillRandomTables()
	{
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].consumeable && !Inventory.Instance.allItems[i].isOneOfKindUniqueItem)
			{
				if (Inventory.Instance.allItems[i].consumeable.isAnimalProduct)
				{
					animalProduct.Add(Inventory.Instance.allItems[i]);
				}
				if (Inventory.Instance.allItems[i].consumeable.isMeat)
				{
					allMeat.Add(Inventory.Instance.allItems[i]);
				}
				if (Inventory.Instance.allItems[i].consumeable.isFruit)
				{
					allFruit.Add(Inventory.Instance.allItems[i]);
				}
				if ((bool)Inventory.Instance.allItems[i].craftable && Inventory.Instance.allItems[i].craftable.workPlaceConditions == CraftingManager.CraftingMenuType.CookingTable)
				{
					cookedRecipe.Add(Inventory.Instance.allItems[i]);
				}
			}
		}
	}

	public int getRandomMeatInt()
	{
		return Inventory.Instance.getInvItemId(allMeat[Random.Range(0, allMeat.Count)]);
	}

	public int getRandomAnimalProduct()
	{
		return Inventory.Instance.getInvItemId(animalProduct[Random.Range(0, animalProduct.Count)]);
	}

	public int getRandomFruit()
	{
		return Inventory.Instance.getInvItemId(allFruit[Random.Range(0, allFruit.Count)]);
	}

	public int getRandomWood()
	{
		return Inventory.Instance.getInvItemId(woodLogs[Random.Range(0, woodLogs.Length)]);
	}

	public int getRandomPlank()
	{
		return Inventory.Instance.getInvItemId(woodPlanks[Random.Range(0, woodPlanks.Length)]);
	}

	public int getRandomOreBar()
	{
		return Inventory.Instance.getInvItemId(oreBars[Random.Range(0, oreBars.Length)]);
	}

	public int getRandomCookedDish()
	{
		return Inventory.Instance.getInvItemId(cookedRecipe[Random.Range(0, cookedRecipe.Count)]);
	}
}
