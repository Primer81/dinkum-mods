using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CatalogueManager : MonoBehaviour
{
	public enum EntryType
	{
		Clothing,
		Furniture
	}

	public static CatalogueManager manage;

	public bool[] collectedItem;

	public GameObject catalogueWindow;

	public bool catalogueOpen;

	public GameObject catalogueButton;

	public Camera previewCamera;

	public Transform previewPos;

	public Transform buttonWindow;

	private List<CatalogueButton> allButtons = new List<CatalogueButton>();

	public bool openWindow;

	public GameObject cameraPreview;

	public int furnitureOnOrder;

	public int clothingOnOrder;

	public GameObject infoScreen;

	public GameObject orderButton;

	public GameObject orderCompleteWindow;

	public TextMeshProUGUI itemTitleText;

	public TextMeshProUGUI itemPrice;

	public TextMeshProUGUI unavailableButtonText;

	public GameObject orderClothesButtons;

	public GameObject orderFurnitureButtons;

	public GameObject showUnavailableButton;

	private bool showingUnavailable;

	private int showingItemId = -1;

	[Header("Clothing Select Tabs")]
	private GameObject clothingTabs;

	private int currentFilterType;

	private EntryType showingType;

	private GameObject previewObject;

	private bool resetPreviewRot;

	private void Awake()
	{
		manage = this;
	}

	private void Start()
	{
		collectedItem = new bool[Inventory.Instance.allItems.Length];
	}

	private void Update()
	{
		if (openWindow)
		{
			openWindow = false;
			openCatalogue(EntryType.Furniture);
		}
	}

	public void openCatalogue(EntryType filterType)
	{
		currentFilterType = 0;
		showingType = filterType;
		orderCompleteWindow.SetActive(value: false);
		infoScreen.SetActive(value: false);
		catalogueOpen = true;
		clearButtons();
		catalogueWindow.SetActive(value: true);
		if (filterType == EntryType.Clothing)
		{
			orderClothesButtons.SetActive(value: true);
			orderFurnitureButtons.SetActive(value: false);
			for (int i = 0; i < collectedItem.Length; i++)
			{
				if (collectedItem[i] && (bool)Inventory.Instance.allItems[i].equipable && Inventory.Instance.allItems[i].equipable.cloths)
				{
					createButtonAndFill(i);
				}
			}
		}
		else
		{
			orderClothesButtons.SetActive(value: false);
			orderFurnitureButtons.SetActive(value: true);
			for (int j = 0; j < collectedItem.Length; j++)
			{
				if (collectedItem[j] && Inventory.Instance.allItems[j].isFurniture)
				{
					createButtonAndFill(j);
				}
			}
		}
		allButtons.Sort(CompareListByName);
		setButtonsToIndex();
		StartCoroutine(rotatePreviewPos());
		Inventory.Instance.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closed = false;
		if (showingType == EntryType.Clothing)
		{
			sortCatalogueByClothType(currentFilterType);
		}
		else
		{
			sortCatalogueByFurnitureType(currentFilterType);
		}
		showUnavailableButton.transform.SetAsFirstSibling();
		SetButtonText();
	}

	private static int CompareListByName(CatalogueButton i1, CatalogueButton i2)
	{
		return i1.getShowingInvItemName().CompareTo(i2.getShowingInvItemName());
	}

	public void setButtonsToIndex()
	{
		for (int i = 0; i < allButtons.Count; i++)
		{
			allButtons[i].transform.SetSiblingIndex(i);
		}
	}

	public void closeCatalogue()
	{
		orderCompleteWindow.SetActive(value: false);
		infoScreen.SetActive(value: false);
		catalogueOpen = false;
		catalogueWindow.gameObject.SetActive(value: false);
		cameraPreview.gameObject.SetActive(value: false);
		Inventory.Instance.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closeButtonDelay();
	}

	public void showItemInfo(int itemToShow)
	{
		resetPreviewRot = true;
		previewPos.rotation = Quaternion.identity;
		showingItemId = itemToShow;
		cameraPreview.gameObject.SetActive(value: true);
		Object.Destroy(previewObject);
		itemTitleText.text = Inventory.Instance.allItems[itemToShow].getInvItemName();
		if (Inventory.Instance.allItems[showingItemId].isUniqueItem)
		{
			itemPrice.text = ConversationGenerator.generate.GetJournalNameByTag("Unavailable");
		}
		else
		{
			itemPrice.text = "<sprite=11> " + ((float)Inventory.Instance.allItems[itemToShow].value * 2.5f).ToString("n0");
		}
		if (Inventory.Instance.allItems[itemToShow].isFurniture)
		{
			previewCamera.transform.localPosition = new Vector3(0f, 2.58f, -5.2f);
			previewObject = Object.Instantiate(Inventory.Instance.allItems[itemToShow].placeable, previewPos).gameObject;
			previewObject.transform.localPosition = Vector3.zero;
			previewObject.transform.rotation = Quaternion.identity;
			if (WorldManager.Instance.allObjectSettings[Inventory.Instance.allItems[itemToShow].placeable.tileObjectId].isMultiTileObject)
			{
				previewObject.transform.localPosition -= new Vector3(WorldManager.Instance.allObjectSettings[Inventory.Instance.allItems[itemToShow].placeable.tileObjectId].xSize / 2, 0f, WorldManager.Instance.allObjectSettings[Inventory.Instance.allItems[itemToShow].placeable.tileObjectId].ySize / 2);
			}
		}
		else if ((bool)Inventory.Instance.allItems[itemToShow].equipable && Inventory.Instance.allItems[itemToShow].equipable.cloths)
		{
			if (Inventory.Instance.allItems[itemToShow].equipable.hat)
			{
				previewObject = Object.Instantiate(EquipWindow.equip.hatPlaceable, previewPos).gameObject;
			}
			else if (Inventory.Instance.allItems[itemToShow].equipable.face)
			{
				previewObject = Object.Instantiate(EquipWindow.equip.hatPlaceable, previewPos).gameObject;
			}
			else if (Inventory.Instance.allItems[itemToShow].equipable.shirt)
			{
				previewObject = Object.Instantiate(EquipWindow.equip.shirtPlaceable, previewPos).gameObject;
			}
			else if (Inventory.Instance.allItems[itemToShow].equipable.pants)
			{
				previewObject = Object.Instantiate(EquipWindow.equip.pantsPlaceable, previewPos).gameObject;
			}
			else if (Inventory.Instance.allItems[itemToShow].equipable.shoes)
			{
				previewObject = Object.Instantiate(EquipWindow.equip.shoePlaceable, previewPos).gameObject;
			}
			previewCamera.transform.localPosition = new Vector3(0f, 1.4f, -2.63f);
			previewObject.GetComponentInChildren<ClothingDisplay>().updateStatus(itemToShow);
			previewObject.transform.localPosition = Vector3.zero;
			previewObject.transform.rotation = Quaternion.identity;
		}
		infoScreen.SetActive(value: false);
		infoScreen.SetActive(value: true);
		if (Inventory.Instance.allItems[showingItemId].isUniqueItem)
		{
			orderButton.SetActive(value: false);
		}
		else if (Inventory.Instance.wallet < (int)((float)Inventory.Instance.allItems[showingItemId].value * 2.5f))
		{
			orderButton.SetActive(value: false);
		}
		else
		{
			orderButton.SetActive(value: true);
		}
	}

	public void sortCatalogueByFurnitureType(int type)
	{
		currentFilterType = type;
		switch (type)
		{
		case 0:
		{
			for (int j = 0; j < allButtons.Count; j++)
			{
				allButtons[j].gameObject.SetActive(ShouldBeShownDependingOnUnavailableSetting(allButtons[j]));
			}
			break;
		}
		case 1:
		{
			for (int l = 0; l < allButtons.Count; l++)
			{
				allButtons[l].gameObject.SetActive((bool)allButtons[l].getShowingInvItem().placeable.tileObjectFurniture && !allButtons[l].getShowingInvItem().placeable.tileObjectFurniture.isSeat && ShouldBeShownDependingOnUnavailableSetting(allButtons[l]));
			}
			break;
		}
		case 2:
		{
			for (int m = 0; m < allButtons.Count; m++)
			{
				allButtons[m].gameObject.SetActive((bool)allButtons[m].getShowingInvItem().placeable.tileObjectFurniture && allButtons[m].getShowingInvItem().placeable.tileObjectFurniture.isSeat && ShouldBeShownDependingOnUnavailableSetting(allButtons[m]));
			}
			break;
		}
		case 3:
		{
			for (int k = 0; k < allButtons.Count; k++)
			{
				allButtons[k].gameObject.SetActive((bool)allButtons[k].getShowingInvItem().placeable.tileObjectChest && ShouldBeShownDependingOnUnavailableSetting(allButtons[k]));
			}
			break;
		}
		case 4:
		{
			for (int i = 0; i < allButtons.Count; i++)
			{
				allButtons[i].gameObject.SetActive((bool)allButtons[i].getShowingInvItem().placeable && WorldManager.Instance.allObjectSettings[allButtons[i].getShowingInvItem().placeable.tileObjectId].canBePlacedOnTopOfFurniture && ShouldBeShownDependingOnUnavailableSetting(allButtons[i]));
			}
			break;
		}
		}
	}

	public void sortCatalogueByClothType(int type)
	{
		currentFilterType = type;
		switch (type)
		{
		case 0:
		{
			for (int n = 0; n < allButtons.Count; n++)
			{
				allButtons[n].gameObject.SetActive(ShouldBeShownDependingOnUnavailableSetting(allButtons[n]));
			}
			break;
		}
		case 1:
		{
			for (int j = 0; j < allButtons.Count; j++)
			{
				allButtons[j].gameObject.SetActive(allButtons[j].getShowingInvItem().equipable.hat && ShouldBeShownDependingOnUnavailableSetting(allButtons[j]));
			}
			break;
		}
		case 2:
		{
			for (int l = 0; l < allButtons.Count; l++)
			{
				allButtons[l].gameObject.SetActive(allButtons[l].getShowingInvItem().equipable.face && ShouldBeShownDependingOnUnavailableSetting(allButtons[l]));
			}
			break;
		}
		case 3:
		{
			for (int num = 0; num < allButtons.Count; num++)
			{
				allButtons[num].gameObject.SetActive(allButtons[num].getShowingInvItem().equipable.shirt && !allButtons[num].getShowingInvItem().equipable.dress && ShouldBeShownDependingOnUnavailableSetting(allButtons[num]));
			}
			break;
		}
		case 4:
		{
			for (int m = 0; m < allButtons.Count; m++)
			{
				allButtons[m].gameObject.SetActive(allButtons[m].getShowingInvItem().equipable.dress && ShouldBeShownDependingOnUnavailableSetting(allButtons[m]));
			}
			break;
		}
		case 5:
		{
			for (int k = 0; k < allButtons.Count; k++)
			{
				allButtons[k].gameObject.SetActive(allButtons[k].getShowingInvItem().equipable.pants && !allButtons[k].getShowingInvItem().equipable.dress && ShouldBeShownDependingOnUnavailableSetting(allButtons[k]));
			}
			break;
		}
		case 6:
		{
			for (int i = 0; i < allButtons.Count; i++)
			{
				allButtons[i].gameObject.SetActive(allButtons[i].getShowingInvItem().equipable.shoes && ShouldBeShownDependingOnUnavailableSetting(allButtons[i]));
			}
			break;
		}
		}
	}

	public void pickUpItem(int itemId)
	{
		collectedItem[itemId] = true;
	}

	private void clearButtons()
	{
		for (int i = 0; i < allButtons.Count; i++)
		{
			Object.Destroy(allButtons[i].gameObject);
		}
		allButtons.Clear();
	}

	private void createButtonAndFill(int itemId)
	{
		CatalogueButton component = Object.Instantiate(catalogueButton, buttonWindow).GetComponent<CatalogueButton>();
		component.setUpButton(itemId);
		allButtons.Add(component);
	}

	private IEnumerator rotatePreviewPos()
	{
		while (catalogueOpen)
		{
			if (resetPreviewRot)
			{
				previewPos.rotation = Quaternion.identity;
				resetPreviewRot = false;
			}
			previewPos.Rotate(Vector3.up, Time.deltaTime * 5f);
			yield return null;
		}
	}

	public void orderItem()
	{
		if (Inventory.Instance.allItems[showingItemId].isUniqueItem)
		{
			SoundManager.Instance.play2DSound(SoundManager.Instance.buttonCantPressSound);
			return;
		}
		Inventory.Instance.changeWallet(-(int)((float)Inventory.Instance.allItems[showingItemId].value * 2.5f));
		int nPCFrom = 4;
		if (Inventory.Instance.allItems[showingItemId].isFurniture)
		{
			nPCFrom = 3;
		}
		MailManager.manage.tomorrowsLetters.Add(new Letter(nPCFrom, Letter.LetterType.CatalogueOrder, showingItemId, 1));
		StartCoroutine(openOrderCompleteWindow());
		if (Inventory.Instance.wallet < (int)((float)Inventory.Instance.allItems[showingItemId].value * 2.5f))
		{
			orderButton.SetActive(value: false);
		}
		else
		{
			orderButton.SetActive(value: true);
		}
	}

	public IEnumerator openOrderCompleteWindow()
	{
		orderCompleteWindow.SetActive(value: true);
		yield return new WaitForSeconds(0.5f);
		bool holdWindow = true;
		while (holdWindow)
		{
			yield return null;
			if (InputMaster.input.Interact() || InputMaster.input.Use() || InputMaster.input.UICancel())
			{
				holdWindow = false;
			}
		}
		orderCompleteWindow.SetActive(value: false);
	}

	private bool ShouldBeShownDependingOnUnavailableSetting(CatalogueButton button)
	{
		if (Inventory.Instance.allItems[button.getShowingInvItem().getItemId()].isUniqueItem)
		{
			if (showingUnavailable)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public void PressHideUnavailableButton()
	{
		showingUnavailable = !showingUnavailable;
		SetButtonText();
		if (showingType == EntryType.Clothing)
		{
			sortCatalogueByClothType(currentFilterType);
		}
		else
		{
			sortCatalogueByFurnitureType(currentFilterType);
		}
	}

	public void SetButtonText()
	{
		if (showingUnavailable)
		{
			unavailableButtonText.text = "<sprite=16> " + ConversationGenerator.generate.GetJournalNameByTag("Show Available Only");
		}
		else
		{
			unavailableButtonText.text = "<sprite=17> " + ConversationGenerator.generate.GetJournalNameByTag("Show Available Only");
		}
	}
}
