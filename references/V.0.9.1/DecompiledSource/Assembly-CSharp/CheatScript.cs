using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CheatScript : MonoBehaviour
{
	public static CheatScript cheat;

	public GameObject cheatWindow;

	public GameObject cheatMenuButton;

	private GameObject[] cheatButtons;

	public Transform cheatScreen;

	public InputField priceField;

	public bool cheatMenuOpen;

	public InputField searchBar;

	public Transform itemSpreadSheetWindow;

	public Transform itemSpeadSheetContent;

	public GameObject itemSpreadSheetEntryPrefab;

	private ItemSpreadSheetEntry[] allItemEntrys;

	public bool cheatsOn;

	public int amountToGive = 1;

	public InventorySlot bin;

	public Sprite selectedIcon;

	public Sprite notSelectedIcon;

	public Image NintyNineSelected;

	public Image OneSelected;

	private bool inOpenOrClose;

	private int itemsAmount = 1222;

	private bool searchingForDeeds;

	private void Awake()
	{
		cheat = this;
	}

	private void Start()
	{
		amountToGive = 1;
		cheatButtons = new GameObject[Inventory.Instance.allItems.Length];
		bin.updateSlotContentsAndRefresh(-1, 0);
		if (PlayerPrefs.HasKey("Cheats"))
		{
			cheatsOn = true;
		}
	}

	private void Update()
	{
		if ((cheatsOn && Input.GetButtonDown("Cheat") && !inOpenOrClose && !MenuButtonsTop.menu.subMenuOpen) || (cheatMenuOpen && InputMaster.input.UICancel() && !inOpenOrClose))
		{
			itemsAmount = Inventory.Instance.allItems.Length;
			cheatMenuOpen = !cheatMenuOpen;
			if (cheatMenuOpen)
			{
				cheatWindow.SetActive(value: true);
				Inventory.Instance.invOpen = true;
				Inventory.Instance.openAndCloseInv();
				searchBar.ActivateInputField();
				StartCoroutine(populateList());
				RenderMap.Instance.ChangeMapWindow();
			}
			else
			{
				StartCoroutine(destroyList());
				RenderMap.Instance.ChangeMapWindow();
				searchingForDeeds = false;
			}
		}
		if (cheatMenuOpen)
		{
			if (bin.itemNo != -1)
			{
				bin.updateSlotContentsAndRefresh(-1, 0);
			}
			if (!Inventory.Instance.invOpen)
			{
				Inventory.Instance.invOpen = true;
				Inventory.Instance.openAndCloseInv();
			}
			if (Input.GetKey(KeyCode.LeftControl) && Inventory.Instance.dragSlot.itemNo != -1)
			{
				Inventory.Instance.dragSlot.updateSlotContentsAndRefresh(-1, -1);
				SoundManager.Instance.play2DSound(SoundManager.Instance.buttonCantPressSound);
			}
		}
	}

	private IEnumerator populateList()
	{
		inOpenOrClose = true;
		int countToSkip = 100;
		for (int i = 0; i < itemsAmount; i++)
		{
			cheatButtons[i] = Object.Instantiate(cheatMenuButton, cheatScreen);
			cheatButtons[i].GetComponent<CheatMenuButton>().setUpButton(i);
			cheatButtons[i].SetActive(ShowIfNotDeed(i));
			float num = 0f;
			if (num > (float)countToSkip)
			{
				yield return null;
				num = 0f;
			}
		}
		inOpenOrClose = false;
	}

	public IEnumerator destroyList()
	{
		inOpenOrClose = true;
		int countToSkip = 50;
		for (int i = 0; i < cheatButtons.Length; i++)
		{
			Object.Destroy(cheatButtons[i]);
			float num = 0f;
			if (num > (float)countToSkip)
			{
				yield return null;
				num = 0f;
			}
		}
		cheatWindow.SetActive(value: false);
		inOpenOrClose = false;
	}

	public void giveAmount(int amount)
	{
		amountToGive = amount;
		if (amountToGive == 99)
		{
			NintyNineSelected.sprite = selectedIcon;
			OneSelected.sprite = notSelectedIcon;
		}
		else
		{
			NintyNineSelected.sprite = notSelectedIcon;
			OneSelected.sprite = selectedIcon;
		}
	}

	public void showAll()
	{
		for (int i = 0; i < itemsAmount; i++)
		{
			cheatButtons[i].gameObject.SetActive(ShowIfNotDeed(i));
		}
	}

	public void showAllHideClothes()
	{
		for (int i = 0; i < itemsAmount; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].equipable && Inventory.Instance.allItems[i].equipable.cloths)
			{
				cheatButtons[i].gameObject.SetActive(value: false);
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(ShowIfNotDeed(i));
			}
		}
	}

	public void showAllWallpaper()
	{
		for (int i = 0; i < itemsAmount; i++)
		{
			if (((bool)Inventory.Instance.allItems[i].equipable && Inventory.Instance.allItems[i].equipable.wallpaper) || ((bool)Inventory.Instance.allItems[i].equipable && Inventory.Instance.allItems[i].equipable.flooring))
			{
				cheatButtons[i].gameObject.SetActive(ShowIfNotDeed(i));
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void showAllFlooring()
	{
		for (int i = 0; i < itemsAmount; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].equipable && Inventory.Instance.allItems[i].equipable.flooring)
			{
				cheatButtons[i].gameObject.SetActive(ShowIfNotDeed(i));
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void showAllVehicles()
	{
		for (int i = 0; i < itemsAmount; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].spawnPlaceable)
			{
				cheatButtons[i].gameObject.SetActive(ShowIfNotDeed(i));
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void showAllTools()
	{
		for (int i = 0; i < itemsAmount; i++)
		{
			if (Inventory.Instance.allItems[i].isATool)
			{
				cheatButtons[i].gameObject.SetActive(ShowIfNotDeed(i));
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void showAllPlaceables()
	{
		for (int i = 0; i < itemsAmount; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].placeable)
			{
				cheatButtons[i].gameObject.SetActive(ShowIfNotDeed(i));
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void showAllClothes()
	{
		for (int i = 0; i < itemsAmount; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].equipable && Inventory.Instance.allItems[i].equipable.cloths)
			{
				cheatButtons[i].gameObject.SetActive(ShowIfNotDeed(i));
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void showAllRequestable()
	{
		for (int i = 0; i < itemsAmount; i++)
		{
			if (Inventory.Instance.allItems[i].isRequestable)
			{
				cheatButtons[i].gameObject.SetActive(ShowIfNotDeed(i));
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void showAllFishAndBugs()
	{
		for (int i = 0; i < itemsAmount; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].fish || (bool)Inventory.Instance.allItems[i].bug || (bool)Inventory.Instance.allItems[i].underwaterCreature)
			{
				cheatButtons[i].gameObject.SetActive(ShowIfNotDeed(i));
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void showMisc()
	{
		for (int i = 0; i < itemsAmount; i++)
		{
			if (!Inventory.Instance.allItems[i].isATool && !Inventory.Instance.allItems[i].placeable && !Inventory.Instance.allItems[i].equipable && !Inventory.Instance.allItems[i].consumeable)
			{
				cheatButtons[i].gameObject.SetActive(ShowIfNotDeed(i));
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void showAllEatable()
	{
		for (int i = 0; i < itemsAmount; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].consumeable)
			{
				cheatButtons[i].gameObject.SetActive(ShowIfNotDeed(i));
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void showAllCraftable()
	{
		for (int i = 0; i < itemsAmount; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].craftable && !Inventory.Instance.allItems[i].isDeed)
			{
				cheatButtons[i].gameObject.SetActive(ShowIfNotDeed(i));
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void searchCheatMenu()
	{
		if (searchBar.text.ToLower() == "t:deed")
		{
			searchingForDeeds = true;
			searchBar.text = "Deed";
		}
		for (int i = 0; i < itemsAmount; i++)
		{
			if (Inventory.Instance.allItems[i].itemName.ToLower().Contains(searchBar.text.ToLower()))
			{
				cheatButtons[i].gameObject.SetActive(ShowIfNotDeed(i));
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void showPlaceableDeeds()
	{
		for (int i = 0; i < itemsAmount; i++)
		{
			if (Inventory.Instance.allItems[i].isDeed && (bool)Inventory.Instance.allItems[i].placeable && (bool)Inventory.Instance.allItems[i].placeable.tileObjectGrowthStages && Inventory.Instance.allItems[i].placeable.tileObjectGrowthStages.NPCMovesInWhenBuilt.Length >= 1)
			{
				cheatButtons[i].gameObject.SetActive(ShowIfNotDeed(i));
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(value: false);
			}
		}
	}

	public bool ShowIfNotDeed(int itemId)
	{
		if (Inventory.Instance.allItems[itemId].isDeed)
		{
			if (searchingForDeeds)
			{
				return true;
			}
			return false;
		}
		return true;
	}
}
