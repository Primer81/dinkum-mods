using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HouseEditor : MonoBehaviour
{
	public static HouseEditor edit;

	public GameObject housePartButtonPrefab;

	public bool windowOpen;

	public GameObject window;

	public GameObject houseWithCamera;

	public GameObject mainButtonsWindow;

	public GameObject subButtonWindow;

	public GameObject paintSubMenu;

	public GameObject colorSubMenu;

	public GameObject paintOptionButtons;

	public Transform subButtonParent;

	public Transform textureButtonParent;

	public PlayerHouseExterior dummyHouse;

	public HouseExterior dummyExterior = new HouseExterior(-2, -2);

	public PlayerHouseExterior.houseParts currentlyPainting;

	public HouseExterior currentlyEditing;

	public InventoryItem houseKitItem;

	public GameObject confirmWindow;

	public ASound changeHouseSound;

	public TMP_InputField nameField;

	public TextMeshProUGUI confirmUseHouseCustomisationKit;

	private List<HousePartButton> buttonsCurrentlyShown = new List<HousePartButton>();

	private List<HousePartButton> colorButtonsShown = new List<HousePartButton>();

	public ConversationObject noUpgradeAvaliable;

	public ConversationObject upgradeAvaliableWithEnoughMoney;

	public ConversationObject upgradeAvaliableNotEnoughMoney;

	public ConversationObject beingMoved;

	public TileObject[] interiorWallObjects;

	private void Awake()
	{
		edit = this;
	}

	private void Start()
	{
		textureButtonParent.gameObject.SetActive(value: false);
		buttonsCurrentlyShown = new List<HousePartButton>();
		colorButtonsShown = new List<HousePartButton>();
	}

	public void openWindow()
	{
		if (currentlyEditing == null)
		{
			currentlyEditing = HouseManager.manage.getPlayerHouseExterior();
		}
		if (currentlyEditing != null)
		{
			confirmUseHouseCustomisationKit.text = string.Format(ConversationGenerator.generate.GetToolTip("Tip_ConfirmHouseCustomisation"), "<b>" + houseKitItem.getInvItemName() + "</b>");
			dummyExterior.copyFromAnotherHouseExterior(currentlyEditing);
			dummyHouse.setExterior(dummyExterior);
			dummyHouse.gameObject.SetActive(value: true);
			windowOpen = true;
			window.SetActive(value: true);
			houseWithCamera.SetActive(value: true);
			subButtonWindow.SetActive(value: false);
			openSubButtonWindow_House();
			paintHouse_Color1();
			CurrencyWindows.currency.openJournal();
			nameField.text = dummyExterior.houseName;
		}
	}

	public void applyButton()
	{
		dummyExterior.houseName = nameField.text;
		dummyExterior.copyToAnotherHouseExterior(currentlyEditing);
		NetworkMapSharer.Instance.localChar.CmdUpdateHouseExterior(currentlyEditing);
		Inventory.Instance.removeAmountOfItem(houseKitItem.getItemId(), 1);
		Inventory.Instance.equipNewSelectedSlot();
		closeWindow();
	}

	public void closeWindow()
	{
		windowOpen = false;
		window.SetActive(value: false);
		dummyHouse.gameObject.SetActive(value: true);
		MenuButtonsTop.menu.closeButtonDelay();
		currentlyEditing = null;
		CurrencyWindows.currency.closeJournal();
		dummyHouse.gameObject.SetActive(value: false);
	}

	public void closeSubWindow()
	{
		subButtonWindow.SetActive(value: false);
		paintSubMenu.SetActive(value: false);
		colorSubMenu.SetActive(value: false);
		paintOptionButtons.SetActive(value: false);
	}

	public void updateDummy()
	{
		dummyHouse.setExterior(dummyExterior);
	}

	public void paintHouse_Color1()
	{
		currentlyPainting = PlayerHouseExterior.houseParts.houseBase;
		fillButtonsTextureArray(dummyHouse.wallMaterials);
		paintOptionButtons.SetActive(value: true);
	}

	public void paintHouse_Color2()
	{
		currentlyPainting = PlayerHouseExterior.houseParts.houseDetailsColor;
		fillButtonsTextureArray(dummyHouse.houseMaterials);
		paintOptionButtons.SetActive(value: true);
	}

	public void paintHouse_Roof()
	{
		currentlyPainting = PlayerHouseExterior.houseParts.roof;
		fillButtonsTextureArray(dummyHouse.roofMaterials);
		paintOptionButtons.SetActive(value: false);
	}

	public void openPaintSubMenu()
	{
	}

	public void openColorSubMenu()
	{
		colorSubMenu.SetActive(value: true);
	}

	public void openSubButtonWindow_House()
	{
		fillButtonsForArray(dummyHouse.bases[0].baseParts, PlayerHouseExterior.houseParts.houseBase);
		colorSubMenu.SetActive(value: true);
	}

	public void openSubButtonWindow_Roof()
	{
		fillButtonsForArray(dummyHouse.roofs[0].baseParts, PlayerHouseExterior.houseParts.roof);
		colorSubMenu.SetActive(value: true);
		paintOptionButtons.SetActive(value: false);
	}

	public void openSubButtonWindow_Door()
	{
		fillButtonsForArray(dummyHouse.doors[0].baseParts, PlayerHouseExterior.houseParts.door);
		colorSubMenu.SetActive(value: false);
		paintOptionButtons.SetActive(value: false);
	}

	public void openSubButtonWindow_Windows()
	{
		fillButtonsForArray(dummyHouse.windows[0].baseParts, PlayerHouseExterior.houseParts.window);
		colorSubMenu.SetActive(value: false);
		paintOptionButtons.SetActive(value: false);
	}

	public void openSubButtonWindow_Fence()
	{
		fillButtonsForArray(dummyHouse.fenceOptions, PlayerHouseExterior.houseParts.fence);
		colorSubMenu.SetActive(value: false);
		paintOptionButtons.SetActive(value: false);
	}

	public void openSubButtonWindow_Confirm()
	{
		subButtonWindow.SetActive(value: false);
		confirmWindow.SetActive(value: true);
		colorSubMenu.SetActive(value: false);
		paintOptionButtons.SetActive(value: false);
	}

	public void fillButtonsForArray(GameObject[] array, PlayerHouseExterior.houseParts partsToShow)
	{
		for (int i = 0; i < buttonsCurrentlyShown.Count; i++)
		{
			Object.Destroy(buttonsCurrentlyShown[i].gameObject);
		}
		buttonsCurrentlyShown.Clear();
		for (int j = 0; j < array.Length; j++)
		{
			buttonsCurrentlyShown.Add(Object.Instantiate(housePartButtonPrefab, subButtonParent.transform).GetComponent<HousePartButton>());
			buttonsCurrentlyShown[j].setUpButton(dummyExterior, j, partsToShow);
		}
		subButtonWindow.SetActive(value: true);
		confirmWindow.SetActive(value: false);
	}

	public void tintColorButtons(Color tintColor)
	{
		foreach (HousePartButton item in colorButtonsShown)
		{
			item.textureImage.color = tintColor;
		}
	}

	public void fillButtonsTextureArray(Material[] array)
	{
		for (int i = 0; i < colorButtonsShown.Count; i++)
		{
			Object.Destroy(colorButtonsShown[i].gameObject);
		}
		colorButtonsShown.Clear();
		for (int j = 0; j < array.Length; j++)
		{
			colorButtonsShown.Add(Object.Instantiate(housePartButtonPrefab, textureButtonParent.transform).GetComponent<HousePartButton>());
			colorButtonsShown[j].setUpTextureButton(dummyExterior, j, currentlyPainting, array[j]);
		}
	}

	public ConversationObject ReturnUpgradeText()
	{
		if (BuildingManager.manage.currentlyMoving == WorldManager.Instance.onTileMap[currentlyEditing.xPos, currentlyEditing.yPos])
		{
			return beingMoved;
		}
		if ((bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[currentlyEditing.xPos, currentlyEditing.yPos]].tileObjectGrowthStages)
		{
			if (Inventory.Instance.wallet >= GetCurrentGuestHouseUpdgradeCost())
			{
				return upgradeAvaliableWithEnoughMoney;
			}
			return upgradeAvaliableNotEnoughMoney;
		}
		return noUpgradeAvaliable;
	}

	public int GetCurrentGuestHouseUpdgradeCost()
	{
		return 220000;
	}

	public void setCurrentlyEditing(int xPos, int yPos)
	{
		currentlyEditing = HouseManager.manage.getHouseExterior(xPos, yPos);
	}

	public void TakePaymentAndSetBuildingToUpgrade()
	{
		Inventory.Instance.changeWallet(-GetCurrentGuestHouseUpdgradeCost());
		NetworkMapSharer.Instance.localChar.CmdUpgradeGuestHouse(currentlyEditing.xPos, currentlyEditing.yPos);
	}

	public bool IsWallBehindThisObject(HouseDetails house, int xPos, int yPos, int xSize, int ySize)
	{
		if (house.houseMapOnTile[xPos, yPos] >= 0)
		{
			if (IsTileObjectWallObject(house.houseMapOnTile[xPos, yPos]))
			{
				return false;
			}
			if (WorldManager.Instance.allObjectSettings[house.houseMapOnTile[xPos, yPos]].getRotationFromMap || WorldManager.Instance.allObjectSettings[house.houseMapOnTile[xPos, yPos]].isMultiTileObject)
			{
				if (house.houseMapRotation[xPos, yPos] == 0)
				{
					return false;
				}
				if (house.houseMapRotation[xPos, yPos] == 1)
				{
					if (IsOnHouseMap(xPos, yPos + 1, xSize, ySize) && IsTileObjectWallObject(house.houseMapOnTile[xPos, yPos + 1]))
					{
						return true;
					}
					return false;
				}
				if (house.houseMapRotation[xPos, yPos] == 2)
				{
					if (IsOnHouseMap(xPos + 1, yPos, xSize, ySize) && IsTileObjectWallObject(house.houseMapOnTile[xPos + 1, yPos]))
					{
						return true;
					}
					return false;
				}
				if (house.houseMapRotation[xPos, yPos] == 3)
				{
					if (IsOnHouseMap(xPos, yPos - 1, xSize, ySize) && IsTileObjectWallObject(house.houseMapOnTile[xPos - 1, yPos]))
					{
						return true;
					}
					return false;
				}
				if (house.houseMapRotation[xPos, yPos] == 4)
				{
					if (IsOnHouseMap(xPos - 1, yPos, xSize, ySize) && IsTileObjectWallObject(house.houseMapOnTile[xPos, yPos - 1]))
					{
						return true;
					}
					return false;
				}
			}
		}
		return false;
	}

	public bool IsTileObjectWallObject(int tileObjectId)
	{
		for (int i = 0; i < interiorWallObjects.Length; i++)
		{
			if (tileObjectId == interiorWallObjects[i].tileObjectId)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsOnHouseMap(int xPos, int yPos, int xSize, int ySize)
	{
		if (xPos < 0 || xPos >= xSize || yPos < 0 || yPos >= ySize)
		{
			return false;
		}
		return true;
	}
}
