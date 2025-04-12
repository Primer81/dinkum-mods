using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreativeManager : MonoBehaviour
{
	public enum SortingBy
	{
		All,
		Tools,
		Placeables,
		Clothes,
		FishAndBugsAndCritters,
		Food,
		Furniture,
		FloorAndWalls,
		Vehicle,
		Seeds,
		Misc
	}

	public static CreativeManager instance;

	private bool creativeMenuOpen;

	public GameObject creativeButtonPrefab;

	public GameObject creativePageButton;

	public GameObject creativeWindow;

	public Transform buttonWindow;

	public Transform pageButtonWindow;

	private List<CheatMenuButton> allButtons = new List<CheatMenuButton>();

	private int showingFromId;

	public GameObject binButton;

	public GameObject smallBinButton;

	public ASound dropInBinSound;

	private List<int> searchedIds = new List<int>();

	public TMP_InputField searchField;

	private List<CreativePageButton> pageButtons = new List<CreativePageButton>();

	private int defaultButtonsAmount = 60;

	private int currentlyShowingMax = 60;

	public Transform minamiseArrow;

	public GameObject bigScreen;

	public GameObject smallScreen;

	private bool isMinamised;

	private bool firstOpen;

	private bool creativeWindowOpen;

	[Header("Time stuff ----")]
	public GameObject timeWindow;

	public CreativePageButton[] timeButtons;

	private bool timeWindowOpened;

	private bool weatherWindowOpened;

	public GameObject weatherWindow;

	public GameObject windButtonArrow;

	public GameObject windyButtonArrow;

	public GameObject sunnyButtonArrow;

	public GameObject heatwaveButtonArrow;

	public GameObject rainButtonArrow;

	public GameObject stormButtonArrow;

	public GameObject fogButtonArrow;

	public GameObject snowButtonArrow;

	public GameObject meteorButtonArrow;

	private SortingBy currentlySortingBy;

	private bool currentWindy;

	private bool currentHeatWave;

	private bool currentRaining;

	private bool currentStorming;

	private bool currentFoggy;

	private bool currentSnowing;

	private bool currentMeteor;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		for (int i = 0; i < defaultButtonsAmount; i++)
		{
			CheatMenuButton component = Object.Instantiate(creativeButtonPrefab, buttonWindow).GetComponent<CheatMenuButton>();
			component.isCreativeButton = true;
			allButtons.Add(component);
			component.setUpButton(showingFromId + i);
		}
		int num = Mathf.CeilToInt(Inventory.Instance.allItems.Length / defaultButtonsAmount) + 1;
		for (int j = 0; j < num; j++)
		{
			CreativePageButton component2 = Object.Instantiate(creativePageButton, pageButtonWindow).GetComponent<CreativePageButton>();
			component2.pageId = j * defaultButtonsAmount;
			pageButtons.Add(component2);
		}
		creativeWindowOpen = false;
		UpdateButtons();
	}

	private void Update2()
	{
		if (CheatScript.cheat.cheatsOn && Input.GetButtonDown("Cheat"))
		{
			creativeWindowOpen = !creativeWindowOpen;
		}
		if (timeWindowOpened)
		{
			HandleTimeSelection();
		}
		creativeWindow.SetActive(creativeWindowOpen && Inventory.Instance.invOpen && !ChestWindow.chests.chestWindowOpen);
		if (Inventory.Instance.invOpen)
		{
			if (!firstOpen)
			{
				searchField.ActivateInputField();
				firstOpen = true;
			}
			binButton.SetActive(SomethingInDragSlot() && !isMinamised);
			smallBinButton.SetActive(SomethingInDragSlot() && isMinamised);
		}
		else
		{
			firstOpen = false;
		}
	}

	private IEnumerator ScrollControlls()
	{
		float inputDelay = 0f;
		while (true)
		{
			yield return null;
			int num = Mathf.RoundToInt(Mathf.Clamp(InputMaster.input.getScrollWheel(), -1f, 1f));
			if (!Inventory.Instance.usingMouse)
			{
				num = Mathf.RoundToInt(InputMaster.input.VehicleAccelerate());
			}
			if (inputDelay > 0f)
			{
				inputDelay = ((num != 0) ? (inputDelay - Time.deltaTime) : 0f);
			}
			else if (num != 0)
			{
				Scroll(num * 60);
				inputDelay = 0.15f;
			}
		}
	}

	public void Scroll(int rows)
	{
		if (isMinamised)
		{
			rows = Mathf.Clamp(rows, -1, 1);
		}
		showingFromId += rows;
		int num = ((!SearchEmpty()) ? searchedIds.Count : Inventory.Instance.allItems.Length);
		if (isMinamised)
		{
			num = Mathf.Clamp(num - 10, 0, Inventory.Instance.allItems.Length);
		}
		showingFromId = Mathf.Clamp(showingFromId, 0, num);
		if (!isMinamised)
		{
			showingFromId = (int)Mathf.Floor((float)showingFromId / (float)defaultButtonsAmount) * defaultButtonsAmount;
		}
		UpdateButtons();
	}

	public void SkipToShowTo(int newShowingFrom)
	{
		showingFromId = newShowingFrom;
		UpdateButtons();
	}

	public bool IsCreativeMenuOpen()
	{
		if (Inventory.Instance.invOpen)
		{
			return creativeMenuOpen;
		}
		return false;
	}

	public bool IsCreativeSearchWindowOpen()
	{
		if (Inventory.Instance.invOpen)
		{
			return searchField.isFocused;
		}
		return false;
	}

	private void UpdateButtons()
	{
		if (SearchEmpty())
		{
			for (int i = 0; i < allButtons.Count; i++)
			{
				allButtons[i].setUpButton(showingFromId + i);
			}
			GeneratePageButtons();
			return;
		}
		for (int j = 0; j < allButtons.Count; j++)
		{
			if (j + showingFromId < searchedIds.Count)
			{
				allButtons[j].setUpButton(searchedIds[j + showingFromId]);
			}
			else
			{
				allButtons[j].setUpButton(-1);
			}
		}
		GeneratePageButtons();
	}

	private bool SomethingInDragSlot()
	{
		return Inventory.Instance.dragSlot.itemNo != -1;
	}

	public void PlaceInBin()
	{
		if (SomethingInDragSlot())
		{
			Inventory.Instance.dragSlot.updateSlotContentsAndRefresh(-1, -1);
			SoundManager.Instance.play2DSound(dropInBinSound);
		}
	}

	public void UpdateSearch()
	{
		searchedIds.Clear();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if (Inventory.Instance.allItems[i].getInvItemName().ToLower().Contains(searchField.text.ToLower()) && AppearsInMenu(i))
			{
				searchedIds.Add(i);
			}
		}
		showingFromId = 0;
		searchedIds.Sort(SortCreativeMenu);
		UpdateButtons();
	}

	private bool SearchEmpty()
	{
		if (searchedIds.Count == 0)
		{
			return searchField.text == "";
		}
		return false;
	}

	private void GeneratePageButtons()
	{
		float num = -120f;
		float num2 = 120f;
		_ = creativeButtonPrefab.GetComponent<Image>().color;
		int num3 = Mathf.CeilToInt((float)(SearchEmpty() ? Inventory.Instance.allItems.Length : searchedIds.Count) / (float)defaultButtonsAmount);
		int num4 = Mathf.Min(num3, pageButtons.Count);
		int num5 = showingFromId / defaultButtonsAmount;
		if (num4 <= 3)
		{
			num = -50f;
			num2 = 50f;
		}
		for (int i = 0; i < pageButtons.Count; i++)
		{
			if (i < num3)
			{
				pageButtons[i].gameObject.SetActive(value: true);
				if (i == num5)
				{
					pageButtons[i].SetSelected(isSelectedNow: true);
				}
				else
				{
					pageButtons[i].SetSelected(isSelectedNow: false);
				}
				if (num4 > 1)
				{
					float t = (float)i / (float)(num4 - 1);
					float x = Mathf.Lerp(num, num2, t);
					pageButtons[i].transform.localPosition = new Vector3(x, pageButtons[i].transform.localPosition.y, pageButtons[i].transform.localPosition.z);
				}
				else
				{
					pageButtons[i].transform.localPosition = new Vector3((num + num2) / 2f, pageButtons[i].transform.localPosition.y, pageButtons[i].transform.localPosition.z);
				}
			}
			else
			{
				pageButtons[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void PressMiniamised()
	{
		isMinamised = !isMinamised;
		bigScreen.gameObject.SetActive(!isMinamised);
		smallScreen.gameObject.SetActive(isMinamised);
		if (isMinamised)
		{
			minamiseArrow.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}
		else
		{
			minamiseArrow.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
		}
		for (int i = 0; i < allButtons.Count; i++)
		{
			if (i > 9)
			{
				allButtons[i].gameObject.SetActive(!isMinamised);
			}
		}
		showingFromId = Mathf.RoundToInt((float)showingFromId / (float)defaultButtonsAmount * (float)defaultButtonsAmount);
		Scroll(0);
		if (timeWindowOpened)
		{
			OpenTimeWindow();
		}
		if (weatherWindowOpened)
		{
			OpenWeatherWindow();
		}
	}

	public void PressSortingByNumber(int number)
	{
		currentlySortingBy = (SortingBy)number;
		ShowSorted();
	}

	public bool AppearsInMenu(int checkId)
	{
		if (!Inventory.Instance.allItems[checkId].isDeed)
		{
			return Inventory.Instance.allItems[checkId].showInCreativeMenu;
		}
		return false;
	}

	public void ShowSorted()
	{
		searchField.text = "";
		searchedIds.Clear();
		if (currentlySortingBy == SortingBy.All)
		{
			for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
			{
				if (AppearsInMenu(i))
				{
					searchedIds.Add(i);
				}
			}
		}
		else if (currentlySortingBy == SortingBy.Tools)
		{
			for (int j = 0; j < Inventory.Instance.allItems.Length; j++)
			{
				if (IsATool(j))
				{
					searchedIds.Add(j);
				}
			}
		}
		else if (currentlySortingBy == SortingBy.Placeables)
		{
			for (int k = 0; k < Inventory.Instance.allItems.Length; k++)
			{
				if (IsNormalPlaceable(k))
				{
					searchedIds.Add(k);
				}
			}
		}
		else if (currentlySortingBy == SortingBy.Clothes)
		{
			for (int l = 0; l < Inventory.Instance.allItems.Length; l++)
			{
				if (IsClothing(l))
				{
					searchedIds.Add(l);
				}
			}
		}
		else if (currentlySortingBy == SortingBy.FishAndBugsAndCritters)
		{
			for (int m = 0; m < Inventory.Instance.allItems.Length; m++)
			{
				if (IsAFishOrBug(m))
				{
					searchedIds.Add(m);
				}
			}
		}
		else if (currentlySortingBy == SortingBy.Food)
		{
			for (int n = 0; n < Inventory.Instance.allItems.Length; n++)
			{
				if (IsFood(n))
				{
					searchedIds.Add(n);
				}
			}
		}
		else if (currentlySortingBy == SortingBy.Furniture)
		{
			for (int num = 0; num < Inventory.Instance.allItems.Length; num++)
			{
				if (IsFurniturePlaceable(num))
				{
					searchedIds.Add(num);
				}
			}
		}
		else if (currentlySortingBy == SortingBy.FloorAndWalls)
		{
			for (int num2 = 0; num2 < Inventory.Instance.allItems.Length; num2++)
			{
				if (IsWallpaperOrFlooring(num2))
				{
					searchedIds.Add(num2);
				}
			}
		}
		else if (currentlySortingBy == SortingBy.Vehicle)
		{
			for (int num3 = 0; num3 < Inventory.Instance.allItems.Length; num3++)
			{
				if (IsAVehicle(num3))
				{
					searchedIds.Add(num3);
				}
			}
		}
		else if (currentlySortingBy == SortingBy.Seeds)
		{
			for (int num4 = 0; num4 < Inventory.Instance.allItems.Length; num4++)
			{
				if (ShowInSeedCatagory(num4))
				{
					searchedIds.Add(num4);
				}
			}
		}
		showingFromId = 0;
		searchedIds.Sort(SortCreativeMenu);
		UpdateButtons();
	}

	public int SortCreativeMenu(int a, int b)
	{
		if (IsATool(a) && !IsATool(b))
		{
			return -1;
		}
		if (IsATool(b) && !IsATool(a))
		{
			return 1;
		}
		if (IsATool(b) && IsATool(a))
		{
			if ((bool)Inventory.Instance.allItems[a].craftable && !Inventory.Instance.allItems[b].craftable)
			{
				return -1;
			}
			if ((bool)Inventory.Instance.allItems[b].craftable && !Inventory.Instance.allItems[a].craftable)
			{
				return 1;
			}
			if ((bool)Inventory.Instance.allItems[a].craftable && (bool)Inventory.Instance.allItems[b].craftable)
			{
				if (Inventory.Instance.allItems[a].craftable.workPlaceConditions < Inventory.Instance.allItems[b].craftable.workPlaceConditions)
				{
					return -1;
				}
				if (Inventory.Instance.allItems[b].craftable.workPlaceConditions < Inventory.Instance.allItems[a].craftable.workPlaceConditions)
				{
					return 1;
				}
			}
			else
			{
				if (Inventory.Instance.allItems[a].isPowerTool && !Inventory.Instance.allItems[b].isPowerTool)
				{
					return 1;
				}
				if (Inventory.Instance.allItems[b].isPowerTool && !Inventory.Instance.allItems[a].isPowerTool)
				{
					return -1;
				}
				if (Inventory.Instance.allItems[a].isOneOfKindUniqueItem && !Inventory.Instance.allItems[b].isOneOfKindUniqueItem)
				{
					return 1;
				}
				if (Inventory.Instance.allItems[b].isOneOfKindUniqueItem && !Inventory.Instance.allItems[a].isOneOfKindUniqueItem)
				{
					return -1;
				}
			}
		}
		else
		{
			if (IsClothing(a) && !IsClothing(b))
			{
				return -1;
			}
			if (IsClothing(b) && !IsClothing(a))
			{
				return 1;
			}
			if (IsClothing(a) && IsClothing(b))
			{
				if (Inventory.Instance.allItems[a].equipable.hat && !Inventory.Instance.allItems[b].equipable.hat)
				{
					return -1;
				}
				if (Inventory.Instance.allItems[b].equipable.hat && !Inventory.Instance.allItems[a].equipable.hat)
				{
					return 1;
				}
				if (Inventory.Instance.allItems[a].equipable.face && !Inventory.Instance.allItems[b].equipable.face)
				{
					return -1;
				}
				if (Inventory.Instance.allItems[b].equipable.face && !Inventory.Instance.allItems[a].equipable.face)
				{
					return 1;
				}
				if (Inventory.Instance.allItems[a].equipable.dress && !Inventory.Instance.allItems[b].equipable.dress)
				{
					return -1;
				}
				if (Inventory.Instance.allItems[b].equipable.dress && !Inventory.Instance.allItems[a].equipable.dress)
				{
					return 1;
				}
				if (Inventory.Instance.allItems[a].equipable.longDress && !Inventory.Instance.allItems[b].equipable.longDress)
				{
					return -1;
				}
				if (Inventory.Instance.allItems[b].equipable.longDress && !Inventory.Instance.allItems[a].equipable.longDress)
				{
					return 1;
				}
				if (Inventory.Instance.allItems[a].equipable.shirt && !Inventory.Instance.allItems[b].equipable.shirt)
				{
					return -1;
				}
				if (Inventory.Instance.allItems[b].equipable.shirt && !Inventory.Instance.allItems[a].equipable.shirt)
				{
					return 1;
				}
				if (Inventory.Instance.allItems[a].equipable.pants && !Inventory.Instance.allItems[b].equipable.pants)
				{
					return -1;
				}
				if (Inventory.Instance.allItems[b].equipable.pants && !Inventory.Instance.allItems[a].equipable.pants)
				{
					return 1;
				}
				if (Inventory.Instance.allItems[a].equipable.shoes && !Inventory.Instance.allItems[b].equipable.shoes)
				{
					return -1;
				}
				if (Inventory.Instance.allItems[b].equipable.shoes && !Inventory.Instance.allItems[a].equipable.shoes)
				{
					return 1;
				}
			}
			else
			{
				if (ShowInSeedCatagory(a) && !ShowInSeedCatagory(b))
				{
					return -1;
				}
				if (ShowInSeedCatagory(b) && !ShowInSeedCatagory(a))
				{
					return 1;
				}
				if (ShowInSeedCatagory(a) && ShowInSeedCatagory(b))
				{
					if (IsPlacablePlant(a) && !IsPlacablePlant(b))
					{
						return -1;
					}
					if (IsPlacablePlant(b) && !IsPlacablePlant(a))
					{
						return 1;
					}
					if (IsPlacablePlant(b) && IsPlacablePlant(a))
					{
						if (IsFood(a) && !IsFood(b))
						{
							return 1;
						}
						if (IsFood(b) && !IsFood(a))
						{
							return -1;
						}
					}
					else
					{
						if (!IsCropSeed(a) && IsCropSeed(b))
						{
							return -1;
						}
						if (IsCropSeed(a) && !IsCropSeed(b))
						{
							return 1;
						}
					}
				}
				else
				{
					if (IsAFishOrBug(a) && !IsAFishOrBug(b))
					{
						return -1;
					}
					if (IsAFishOrBug(b) && !IsAFishOrBug(a))
					{
						return 1;
					}
					if (IsAFishOrBug(a) && IsAFishOrBug(b))
					{
						if ((bool)Inventory.Instance.allItems[a].bug && !Inventory.Instance.allItems[b].bug)
						{
							return -1;
						}
						if ((bool)Inventory.Instance.allItems[b].bug && !Inventory.Instance.allItems[a].bug)
						{
							return 1;
						}
						if ((bool)Inventory.Instance.allItems[a].fish && !Inventory.Instance.allItems[b].fish)
						{
							return -1;
						}
						if ((bool)Inventory.Instance.allItems[b].fish && !Inventory.Instance.allItems[a].fish)
						{
							return 1;
						}
					}
				}
			}
		}
		return Inventory.Instance.allItems[a].getInvItemName().CompareTo(Inventory.Instance.allItems[b].getInvItemName());
	}

	public bool IsATool(int itemId)
	{
		return Inventory.Instance.allItems[itemId].isATool;
	}

	public bool IsFood(int itemId)
	{
		return Inventory.Instance.allItems[itemId].consumeable;
	}

	public bool IsNormalPlaceable(int itemId)
	{
		if (!IgnoreAsPlaceable(itemId) && !IsSeed(itemId) && (bool)Inventory.Instance.allItems[itemId].placeable && !Inventory.Instance.allItems[itemId].burriedPlaceable && !Inventory.Instance.allItems[itemId].isFurniture)
		{
			return true;
		}
		return false;
	}

	public bool IsFurniturePlaceable(int itemId)
	{
		if (!IgnoreAsPlaceable(itemId) && !IsSeed(itemId) && (bool)Inventory.Instance.allItems[itemId].placeable && !Inventory.Instance.allItems[itemId].burriedPlaceable && Inventory.Instance.allItems[itemId].isFurniture)
		{
			return true;
		}
		return false;
	}

	public bool IsClothing(int itemId)
	{
		if ((bool)Inventory.Instance.allItems[itemId].equipable)
		{
			return Inventory.Instance.allItems[itemId].equipable.cloths;
		}
		return false;
	}

	public bool IsAFishOrBug(int itemId)
	{
		if ((bool)Inventory.Instance.allItems[itemId].fish || (bool)Inventory.Instance.allItems[itemId].bug || (bool)Inventory.Instance.allItems[itemId].underwaterCreature)
		{
			return true;
		}
		return false;
	}

	private bool IgnoreAsPlaceable(int checkId)
	{
		if ((bool)Inventory.Instance.allItems[checkId].fish || (bool)Inventory.Instance.allItems[checkId].bug || (bool)Inventory.Instance.allItems[checkId].underwaterCreature)
		{
			return true;
		}
		if ((bool)Inventory.Instance.allItems[checkId].equipable && Inventory.Instance.allItems[checkId].equipable.cloths)
		{
			return true;
		}
		return false;
	}

	private bool ShowInSeedCatagory(int id)
	{
		if (!IgnoreAsPlaceable(id) && IsSeed(id))
		{
			return true;
		}
		return false;
	}

	private bool IsSeed(int checkId)
	{
		if ((bool)Inventory.Instance.allItems[checkId].placeable && Inventory.Instance.allItems[checkId].burriedPlaceable && !Inventory.Instance.allItems[checkId].consumeable)
		{
			return true;
		}
		if ((bool)Inventory.Instance.allItems[checkId].placeable && (bool)Inventory.Instance.allItems[checkId].placeable.tileObjectGrowthStages)
		{
			if (Inventory.Instance.allItems[checkId].placeable.tileObjectGrowthStages.needsTilledSoil)
			{
				return true;
			}
			if (Inventory.Instance.allItems[checkId].placeable.tileObjectGrowthStages.normalPickUp)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsPlacablePlant(int checkId)
	{
		if ((bool)Inventory.Instance.allItems[checkId].placeable && (bool)Inventory.Instance.allItems[checkId].placeable.tileObjectGrowthStages && Inventory.Instance.allItems[checkId].placeable.tileObjectGrowthStages.normalPickUp)
		{
			return true;
		}
		return false;
	}

	private bool IsCropSeed(int checkId)
	{
		if ((bool)Inventory.Instance.allItems[checkId].placeable && (bool)Inventory.Instance.allItems[checkId].placeable.tileObjectGrowthStages && Inventory.Instance.allItems[checkId].placeable.tileObjectGrowthStages.needsTilledSoil)
		{
			return true;
		}
		return false;
	}

	private bool IsWallpaperOrFlooring(int id)
	{
		if ((bool)Inventory.Instance.allItems[id].equipable && (Inventory.Instance.allItems[id].equipable.wallpaper || Inventory.Instance.allItems[id].equipable.flooring))
		{
			return true;
		}
		return false;
	}

	public bool IsAVehicle(int id)
	{
		if ((bool)Inventory.Instance.allItems[id].spawnPlaceable && (bool)Inventory.Instance.allItems[id].spawnPlaceable.GetComponent<Vehicle>())
		{
			return true;
		}
		return false;
	}

	public void OpenTimeWindow()
	{
		if (weatherWindowOpened)
		{
			OpenWeatherWindow();
		}
		if (!timeWindowOpened && !isMinamised)
		{
			PressMiniamised();
		}
		timeWindowOpened = !timeWindowOpened;
		timeWindow.SetActive(timeWindowOpened);
	}

	private void HandleTimeSelection()
	{
		for (int i = 0; i < timeButtons.Length; i++)
		{
			timeButtons[i].SetSelected(RealWorldTimeLight.time.currentHour == timeButtons[i].pageId);
		}
	}

	public void PressAddTime(int addHour)
	{
		int num = RealWorldTimeLight.time.currentHour + addHour;
		if (num >= 24 || num == 0)
		{
			num = 7;
		}
		if (num < 7)
		{
			num = 0;
		}
		RealWorldTimeLight.time.NetworkcurrentHour = num;
	}

	public void OpenWeatherWindow()
	{
		if (timeWindowOpened)
		{
			OpenTimeWindow();
		}
		if (!weatherWindowOpened && !isMinamised)
		{
			PressMiniamised();
		}
		GetCurrentWeather();
		TurnOnWeatherArrows();
		weatherWindowOpened = !weatherWindowOpened;
		weatherWindow.SetActive(weatherWindowOpened);
	}

	private void GetCurrentWeather()
	{
		currentWindy = WeatherManager.Instance.CurrentWeather.isWindy;
		currentHeatWave = NetworkMapSharer.Instance.todaysWeather[0].isHeatWave;
		currentRaining = WeatherManager.Instance.CurrentWeather.isRainy;
		currentStorming = WeatherManager.Instance.CurrentWeather.isStormy;
		currentFoggy = NetworkMapSharer.Instance.todaysWeather[0].isFoggy;
		currentSnowing = NetworkMapSharer.Instance.todaysWeather[0].isSnowDay;
		currentMeteor = NetworkMapSharer.Instance.todaysWeather[2].isMeteorShower;
	}

	public void PressWindyButton()
	{
		GetCurrentWeather();
		currentWindy = true;
		NetworkMapSharer.Instance.localChar.CmdChangeWeather(currentWindy, currentHeatWave, currentRaining, currentStorming, currentFoggy, currentSnowing, currentMeteor);
		TurnOnWeatherArrows();
	}

	public void PressNoWindButton()
	{
		GetCurrentWeather();
		currentWindy = false;
		NetworkMapSharer.Instance.localChar.CmdChangeWeather(currentWindy, currentHeatWave, currentRaining, currentStorming, currentFoggy, currentSnowing, currentMeteor);
		TurnOnWeatherArrows();
	}

	public void PressSunnyButton()
	{
		GetCurrentWeather();
		currentRaining = false;
		currentStorming = false;
		currentFoggy = false;
		currentSnowing = false;
		NetworkMapSharer.Instance.localChar.CmdChangeWeather(currentWindy, currentHeatWave, currentRaining, currentStorming, currentFoggy, currentSnowing, currentMeteor);
		TurnOnWeatherArrows();
	}

	public void PressRainingButton()
	{
		GetCurrentWeather();
		currentRaining = !currentRaining;
		NetworkMapSharer.Instance.localChar.CmdChangeWeather(currentWindy, currentHeatWave, currentRaining, currentStorming, currentFoggy, currentSnowing, currentMeteor);
		TurnOnWeatherArrows();
	}

	public void PressStormingButton()
	{
		GetCurrentWeather();
		currentStorming = !currentStorming;
		NetworkMapSharer.Instance.localChar.CmdChangeWeather(currentWindy, currentHeatWave, currentRaining, currentStorming, currentFoggy, currentSnowing, currentMeteor);
		TurnOnWeatherArrows();
	}

	public void PressSnowingButton()
	{
		GetCurrentWeather();
		currentSnowing = !currentSnowing;
		NetworkMapSharer.Instance.localChar.CmdChangeWeather(currentWindy, currentHeatWave, currentRaining, currentStorming, currentFoggy, currentSnowing, currentMeteor);
		TurnOnWeatherArrows();
	}

	public void PressMeteorButton()
	{
		GetCurrentWeather();
		currentMeteor = !currentMeteor;
		NetworkMapSharer.Instance.localChar.CmdChangeWeather(currentWindy, currentHeatWave, currentRaining, currentStorming, currentFoggy, currentSnowing, currentMeteor);
		TurnOnWeatherArrows();
	}

	public void PressHeatwaveButton()
	{
		GetCurrentWeather();
		currentHeatWave = !currentHeatWave;
		NetworkMapSharer.Instance.localChar.CmdChangeWeather(currentWindy, currentHeatWave, currentRaining, currentStorming, currentFoggy, currentSnowing, currentMeteor);
		TurnOnWeatherArrows();
	}

	public void PressFoggyButton()
	{
		GetCurrentWeather();
		currentFoggy = !currentFoggy;
		NetworkMapSharer.Instance.localChar.CmdChangeWeather(currentWindy, currentHeatWave, currentRaining, currentStorming, currentFoggy, currentSnowing, currentMeteor);
		TurnOnWeatherArrows();
	}

	private void TurnOnWeatherArrows()
	{
		sunnyButtonArrow.SetActive(CheckIfSunny());
		windyButtonArrow.SetActive(currentWindy);
		windButtonArrow.SetActive(!currentWindy);
		rainButtonArrow.SetActive(currentRaining);
		stormButtonArrow.SetActive(currentStorming);
		fogButtonArrow.SetActive(currentFoggy);
		snowButtonArrow.SetActive(currentSnowing);
		meteorButtonArrow.SetActive(currentMeteor);
		heatwaveButtonArrow.SetActive(currentHeatWave);
	}

	private bool CheckIfSunny()
	{
		if (!currentRaining && !currentStorming && !currentFoggy && !currentSnowing)
		{
			return true;
		}
		return false;
	}
}
