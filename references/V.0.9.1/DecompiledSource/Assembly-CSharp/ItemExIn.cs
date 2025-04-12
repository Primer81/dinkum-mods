using System.IO;
using UnityEngine;

public class ItemExIn : MonoBehaviour
{
	public bool createSheet;

	private void Update()
	{
		if (createSheet)
		{
			createSheet = false;
			ExportFile();
		}
	}

	private void ExportFile()
	{
		StreamWriter streamWriter = new StreamWriter(Application.persistentDataPath + "ItemData.csv");
		streamWriter.WriteLine("ItemName,Sell Price,Buy Price,Price to Craft");
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			int num = 0;
			if ((bool)Inventory.Instance.allItems[i].craftable)
			{
				num = Inventory.Instance.allItems[i].value * 2;
				for (int j = 0; j < Inventory.Instance.allItems[i].craftable.itemsInRecipe.Length; j++)
				{
					num += Inventory.Instance.allItems[i].craftable.itemsInRecipe[j].value * Inventory.Instance.allItems[i].craftable.stackOfItemsInRecipe[j];
				}
				num /= Inventory.Instance.allItems[i].craftable.recipeGiveThisAmount;
			}
			streamWriter.WriteLine(Inventory.Instance.allItems[i].itemName + "," + Inventory.Instance.allItems[i].value + "," + Inventory.Instance.allItems[i].value * 2 + "," + num);
		}
	}
}
