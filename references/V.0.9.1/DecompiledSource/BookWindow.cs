using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookWindow : MonoBehaviour
{
	public static BookWindow book;

	public FillRecipeSlot[] slots;

	public GameObject bookWindow;

	public TextMeshProUGUI objectTitle;

	public bool open;

	public bool weatherForecastOpen;

	private int showingTileObjectId = -1;

	public GridLayoutGroup myGrid;

	public GameObject plantBook;

	public GameObject machineBook;

	public TextMeshProUGUI plantBookText;

	public GameObject nothingFound;

	public GameObject weatherForcast;

	private void Awake()
	{
		book = this;
	}

	public void openBook()
	{
		open = true;
		machineBook.SetActive(value: true);
		plantBook.SetActive(value: false);
		weatherForcast.SetActive(value: false);
		StartCoroutine(runWhileBookOpen());
	}

	public void closeBook()
	{
		open = false;
		bookWindow.SetActive(value: false);
	}

	public void openPlantBook(string textToShow)
	{
		bookWindow.SetActive(value: true);
		machineBook.SetActive(value: false);
		plantBook.SetActive(value: true);
		weatherForcast.SetActive(value: false);
		plantBookText.text = textToShow;
	}

	private void whileBookOpen()
	{
		int num = WorldManager.Instance.onTileMap[(int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.x, (int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.y];
		if ((num > 0 && (bool)WorldManager.Instance.allObjects[num].tileObjectItemChanger) || num == 16 || num == 703)
		{
			bookWindow.SetActive(value: true);
			if (num != showingTileObjectId)
			{
				bookWindow.SetActive(value: false);
				showingTileObjectId = num;
				getAllItemsForChanger(num);
				bookWindow.SetActive(value: true);
			}
		}
		else
		{
			bookWindow.SetActive(value: false);
		}
	}

	private IEnumerator runWhileBookOpen()
	{
		while (open)
		{
			yield return null;
			whileBookOpen();
		}
		bookWindow.SetActive(value: false);
	}

	public void getAllItemsForChanger(int tileObject)
	{
		objectTitle.text = WorldManager.Instance.allObjectSettings[tileObject].dropsItemOnDeath.getInvItemName();
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if (CatalogueManager.manage.collectedItem[i] && (bool)Inventory.Instance.allItems[i].itemChange && Inventory.Instance.allItems[i].itemChange.getAmountNeeded(tileObject) > 0 && !Inventory.Instance.allItems[i].fish)
			{
				list.Add(Inventory.Instance.allItems[i]);
			}
		}
		if (list.Count == 0)
		{
			if (tileObject == 16)
			{
				list = GetAllItemsForWindMill();
			}
			if (tileObject == 703)
			{
				list = GetAllItemsForSolarPanel();
			}
		}
		if (list.Count == 0)
		{
			myGrid.constraintCount = 2;
			nothingFound.SetActive(value: true);
			for (int j = 0; j < slots.Length; j++)
			{
				slots[j].gameObject.SetActive(value: false);
			}
			return;
		}
		nothingFound.SetActive(value: false);
		list.Sort(sortIngredients);
		if (list.Count < 8)
		{
			myGrid.constraintCount = list.Count;
		}
		else if (list.Count > 16)
		{
			myGrid.constraintCount = 12;
		}
		else
		{
			myGrid.constraintCount = 8;
		}
		for (int k = 0; k < slots.Length; k++)
		{
			slots[k].gameObject.SetActive(value: false);
			if (k < list.Count)
			{
				slots[k].gameObject.SetActive(value: true);
				slots[k].fillDeedBuySlot(list[k].getItemId());
				slots[k].itemAmounts.text = "";
			}
			else
			{
				slots[k].gameObject.SetActive(value: false);
			}
		}
	}

	public List<InventoryItem> GetAllItemsForWindMill()
	{
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if (CatalogueManager.manage.collectedItem[i] && (bool)Inventory.Instance.allItems[i].placeable && (bool)Inventory.Instance.allItems[i].placeable.tileObjectItemChanger && Inventory.Instance.allItems[i].placeable.tileObjectItemChanger.useWindMill)
			{
				list.Add(Inventory.Instance.allItems[i]);
			}
		}
		return list;
	}

	public List<InventoryItem> GetAllItemsForSolarPanel()
	{
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if (CatalogueManager.manage.collectedItem[i] && (bool)Inventory.Instance.allItems[i].placeable && (bool)Inventory.Instance.allItems[i].placeable.tileObjectItemChanger && Inventory.Instance.allItems[i].placeable.tileObjectItemChanger.useSolar)
			{
				list.Add(Inventory.Instance.allItems[i]);
			}
		}
		return list;
	}

	public void GetAllItemsForWaterTank()
	{
	}

	public int sortIngredients(InventoryItem a, InventoryItem b)
	{
		if (a.value < b.value)
		{
			return -1;
		}
		if (a.value > b.value)
		{
			return 1;
		}
		return 0;
	}

	public void OpenWeatherForecast()
	{
		weatherForecastOpen = true;
		bookWindow.SetActive(value: true);
		machineBook.SetActive(value: false);
		plantBook.SetActive(value: false);
		weatherForcast.SetActive(value: true);
		objectTitle.text = ConversationGenerator.generate.GetToolTip("Tip_WeatherForecast");
		Inventory.Instance.checkIfWindowIsNeeded();
	}

	public void CloseWeatherForecast()
	{
		weatherForecastOpen = false;
		weatherForcast.SetActive(value: false);
		bookWindow.SetActive(value: false);
		Inventory.Instance.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closeButtonDelay();
	}

	private IEnumerator closeWeatherForecastWhenConversationEnds()
	{
		Vector3 charPos = NetworkMapSharer.Instance.localChar.transform.position;
		while (Vector3.Distance(charPos, NetworkMapSharer.Instance.localChar.transform.position) < 0.25f)
		{
			yield return null;
			if (InputMaster.input.OpenInventory() || InputMaster.input.Journal())
			{
				break;
			}
		}
		weatherForcast.SetActive(value: false);
		bookWindow.SetActive(value: false);
	}
}
