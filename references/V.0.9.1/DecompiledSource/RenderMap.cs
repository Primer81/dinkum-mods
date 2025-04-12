using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RenderMap : MonoBehaviour
{
	public static RenderMap Instance;

	public static RenderMap undergroundMap;

	public RectTransform mapParent;

	public bool refreshMap;

	[SerializeField]
	private RectTransform mapWindow;

	[SerializeField]
	private GameObject mapSubWindow;

	[SerializeField]
	private RawImage mapImage;

	[SerializeField]
	private Transform mapMask;

	[SerializeField]
	private Image mapWindowShape;

	[SerializeField]
	private Sprite mapWindowCircle;

	[SerializeField]
	private Sprite mapWindowSquare;

	public bool selectTeleWindowOpen;

	public GameObject teleSelectWindow;

	public bool canTele;

	public bool debugTeleport;

	[SerializeField]
	public float mapScale;

	[SerializeField]
	public float scale = 5f;

	[SerializeField]
	public float desiredScale = 5f;

	public Material mapMaterial;

	[SerializeField]
	private Color water;

	[SerializeField]
	private Color deepWater;

	[SerializeField]
	private Color mapBackColor;

	[SerializeField]
	private Color heightLineColour;

	[SerializeField]
	private Color[] tileObjectsShownOnMapColor;

	[SerializeField]
	private RectTransform charPointer;

	[SerializeField]
	private RectTransform charDirPointer;

	[SerializeField]
	private RectTransform compass;

	[SerializeField]
	private RectTransform buttonPrompt;

	public Transform charToPointTo;

	public RectTransform mapCircle;

	public MapCursor mapCursor;

	[SerializeField]
	private mapIcon mapIconPrefab;

	[SerializeField]
	private mapIcon npcMapIconPrefab;

	public Image turnOnMapButtonIcon;

	public Sprite nickMarker;

	public mapIcon nickIcon;

	public Sprite[] icons;

	[SerializeField]
	private Image[] iconButtons;

	[SerializeField]
	private GameObject iconSelectorWindow;

	public bool iconSelectorOpen;

	public List<mapIcon> mapIcons = new List<mapIcon>();

	public Sprite mineSprite;

	public Sprite airportSprite;

	public TileObject[] tileObjectShowOnMap;

	public GameObject otherCharPointerPrefab;

	public GraphicRaycaster mapCaster;

	public TextMeshProUGUI mapKeybindText;

	public TextMeshProUGUI biomeName;

	public bool mapOpen;

	[SerializeField]
	private ASound placeMarkerSound;

	[SerializeField]
	public ASound removeMarkerSound;

	private readonly List<Transform> otherPlayersToTrack = new List<Transform>();

	private readonly List<RectTransform> otherPlayerIcons = new List<RectTransform>();

	private Texture2D noiseTex;

	private Texture2D undergroundTex;

	private readonly WaitForSeconds mapWait = new WaitForSeconds(0.25f);

	private readonly List<int[]> bridgePositions = new List<int[]>();

	private Color[] pix;

	private Color[] undergroundPix;

	private Color defaultMapMaskBackgroundColour;

	private string teleDir = string.Empty;

	private float openedScale = 5f;

	private float mapXPosDif;

	private float mapYPosDif;

	private int selectedCustomIconIndex;

	private bool firstOpen = true;

	private bool[,] litSquares;

	private Coroutine clearNPCRoutine;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		defaultMapMaskBackgroundColour = mapWindowShape.color;
		desiredScale = scale;
		noiseTex = new Texture2D(WorldManager.Instance.GetMapSize(), WorldManager.Instance.GetMapSize())
		{
			filterMode = FilterMode.Point
		};
		undergroundTex = new Texture2D(WorldManager.Instance.GetMapSize(), WorldManager.Instance.GetMapSize())
		{
			filterMode = FilterMode.Point
		};
		pix = new Color[noiseTex.width * noiseTex.height];
		undergroundPix = new Color[noiseTex.width * noiseTex.height];
		mapScale = WorldManager.Instance.GetMapSize() / 500;
	}

	public void ScanAndUpdateScanAMapIconHighlights(SyncList<MapPoint>.Operation op, int index, MapPoint oldItem, MapPoint newItem)
	{
		ScanAndUpdateScanAMapIconHighlights();
	}

	public void ScanAndUpdateScanAMapIconHighlights()
	{
		for (int i = 0; i < mapIcons.Count; i++)
		{
			mapIcon mapIcon2 = mapIcons[i];
			if (mapIcons[i] == null || mapIcons[i].CurrentIconType == mapIcon.iconType.PlayerPlaced || mapIcons[i].CurrentIconType == mapIcon.iconType.Vehicle)
			{
				continue;
			}
			if (NetworkMapSharer.Instance.mapPoints.Contains(mapIcon2.MyMapPoint))
			{
				if (!mapIcon2.IconShouldBeHighlighted)
				{
					mapIcon2.ChangeHighlightValue(newValue: true);
				}
			}
			else if (mapIcon2.IconShouldBeHighlighted)
			{
				mapIcon2.ChangeHighlightValue(newValue: false);
			}
		}
	}

	public void Update()
	{
		if (!mapOpen)
		{
			return;
		}
		UpdateBioNameLabel();
		if (selectTeleWindowOpen)
		{
			HandleSelectTeleportDestinationWindowIsOpen();
			return;
		}
		HandleMapInput();
		if (!iconSelectorOpen)
		{
			HandleMouseScrolls();
		}
	}

	private void HandleMapInput()
	{
		Cursor.lockState = CursorLockMode.Locked;
		if ((!selectTeleWindowOpen && !iconSelectorOpen && InputMaster.input.UICancel()) || (!selectTeleWindowOpen && !iconSelectorOpen && InputMaster.input.OpenMap()))
		{
			MenuButtonsTop.menu.closeWindow();
			return;
		}
		if (InputMaster.input.UISelect())
		{
			if ((bool)IsMouseHoveringMapIcon())
			{
				HandleClickedOnMapIcon();
			}
			else
			{
				OpenIconSelectionBox();
			}
		}
		if (!iconSelectorOpen)
		{
			CheckToRemoveCustomMarker();
		}
		float num = 0f - InputMaster.input.getLeftStick().x;
		float num2 = 0f - InputMaster.input.getLeftStick().y;
		if (Inventory.Instance.usingMouse)
		{
			num = 0f - InputMaster.input.getMousePosOld().x;
			num2 = 0f - InputMaster.input.getMousePosOld().y;
		}
		if ((bool)IsMouseHoveringMapIcon())
		{
			mapCursor.SetHovering(IsMouseHoveringMapIcon().GetComponentInParent<mapIcon>());
			num /= 2f;
			num2 /= 2f;
		}
		else
		{
			mapCursor.SetHovering(null);
		}
		if ((!Inventory.Instance.usingMouse && InputMaster.input.drop()) || (Inventory.Instance.usingMouse && InputMaster.input.TriggerLook()))
		{
			RecenterToCharacterPosition();
		}
		if (!iconSelectorOpen)
		{
			mapXPosDif += num * 2f / (scale / 5f);
			mapYPosDif += num2 * 2f / (scale / 5f);
		}
	}

	private void HandleClickedOnMapIcon()
	{
		mapIcon mapIconMouseIsHovering = GetMapIconMouseIsHovering();
		if (!(mapIconMouseIsHovering == null))
		{
			if (canTele)
			{
				mapIconMouseIsHovering.OnPressedIcon();
				mapCursor.PressDownOnButton();
			}
			else
			{
				mapIconMouseIsHovering.SetHighlightValueNetworkChange(!mapIconMouseIsHovering.IconShouldBeHighlighted);
				SoundManager.Instance.play2DSound(placeMarkerSound);
				mapCursor.PressDownOnButton();
			}
		}
	}

	private void CheckToRemoveCustomMarker()
	{
		if (InputMaster.input.UIAlt())
		{
			mapIcon mapIconMouseIsHovering = GetMapIconMouseIsHovering();
			if (!(mapIconMouseIsHovering == null) && mapIconMouseIsHovering.CurrentIconType == mapIcon.iconType.PlayerPlaced)
			{
				mapIconMouseIsHovering.RemoveMapMarkerFromMap();
				SoundManager.Instance.play2DSound(removeMarkerSound);
				mapCursor.PlaceButtonPing();
			}
		}
	}

	private void OpenIconSelectionBox()
	{
		iconSelectorOpen = true;
		ChangeSelectedCustomIconIndex(0);
		StartCoroutine(RunIconSelector());
	}

	private void HandleSelectTeleportDestinationWindowIsOpen()
	{
		Cursor.lockState = CursorLockMode.None;
		if (InputMaster.input.UICancel() && selectTeleWindowOpen)
		{
			CloseTeleSelectWindow();
		}
	}

	private void HandleMouseScrolls()
	{
		float scrollWheel = InputMaster.input.getScrollWheel();
		if (scrollWheel == 0f)
		{
			changeScale(InputMaster.input.getRightStick().y / 2f);
		}
		else
		{
			changeScale(scrollWheel * 3f);
		}
	}

	public void OpenMap()
	{
		if (!mapOpen)
		{
			NetworkMapSharer.Instance.localChar.myEquip.setNewLookingAtMap(newLookingAtMap: true);
			mapCaster.enabled = true;
		}
		mapOpen = true;
	}

	public void CloseMap()
	{
		if (mapOpen)
		{
			NetworkMapSharer.Instance.localChar.myEquip.setNewLookingAtMap(newLookingAtMap: false);
			mapCaster.enabled = false;
			Instance.canTele = false;
		}
		Instance.mapOpen = false;
	}

	private IEnumerator RunIconSelector()
	{
		float changeTimer = 0.2f;
		iconSelectorWindow.SetActive(value: true);
		mapCursor.setPressing(isPressing: true);
		while (iconSelectorOpen)
		{
			yield return null;
			if (changeTimer >= 0.2f)
			{
				if (!Inventory.Instance.usingMouse)
				{
					float f = 0f - InputMaster.input.getLeftStick().y;
					float x = InputMaster.input.getLeftStick().x;
					if (Inventory.Instance.usingMouse && Mathf.CeilToInt(x) != 0)
					{
						if ((Mathf.CeilToInt(x) == 1 && selectedCustomIconIndex != 3 && selectedCustomIconIndex != 7) || (Mathf.CeilToInt(x) == -1 && selectedCustomIconIndex != 0 && selectedCustomIconIndex != 4))
						{
							ChangeSelectedCustomIconIndex(Mathf.CeilToInt(x));
							yield return new WaitForSeconds(0.15f);
						}
					}
					else if (Inventory.Instance.usingMouse && Mathf.CeilToInt(f) != 0)
					{
						int num = Mathf.CeilToInt(f);
						if ((num == 1 && selectedCustomIconIndex < 4) || (num == -1 && selectedCustomIconIndex >= 4))
						{
							ChangeSelectedCustomIconIndex(num * 4);
							yield return new WaitForSeconds(0.15f);
						}
					}
					else if (Mathf.RoundToInt(InputMaster.input.UINavigation().x) != 0)
					{
						if ((Mathf.RoundToInt(InputMaster.input.UINavigation().x) == 1 && selectedCustomIconIndex != 3 && selectedCustomIconIndex != 7) || (Mathf.RoundToInt(InputMaster.input.UINavigation().x) == -1 && selectedCustomIconIndex != 0 && selectedCustomIconIndex != 4))
						{
							ChangeSelectedCustomIconIndex(Mathf.RoundToInt(InputMaster.input.UINavigation().x));
							yield return new WaitForSeconds(0.15f);
						}
					}
					else if (Mathf.RoundToInt(Mathf.RoundToInt(InputMaster.input.UINavigation().y)) != 0)
					{
						int num2 = -Mathf.RoundToInt(InputMaster.input.UINavigation().y);
						if ((num2 == 1 && selectedCustomIconIndex < 4) || (num2 == -1 && selectedCustomIconIndex >= 4))
						{
							ChangeSelectedCustomIconIndex(num2 * 4);
							yield return new WaitForSeconds(0.15f);
						}
					}
				}
			}
			else
			{
				changeTimer += Time.deltaTime;
			}
			if (Inventory.Instance.usingMouse)
			{
				if (InputMaster.input.getScrollWheel() / 20f > 0f)
				{
					ChangeSelectedCustomIconIndex(-1);
					SoundManager.Instance.play2DSound(SoundManager.Instance.inventorySound);
				}
				if (InputMaster.input.getScrollWheel() / 20f < 0f)
				{
					ChangeSelectedCustomIconIndex(1);
					SoundManager.Instance.play2DSound(SoundManager.Instance.inventorySound);
				}
				float y = InputMaster.input.getLeftStick().y;
				float x2 = InputMaster.input.getLeftStick().x;
				if (Mathf.CeilToInt(x2) != 0)
				{
					if ((Mathf.CeilToInt(x2) == 1 && selectedCustomIconIndex != 3 && selectedCustomIconIndex != 7) || (Mathf.CeilToInt(x2) == -1 && selectedCustomIconIndex != 0 && selectedCustomIconIndex != 4))
					{
						ChangeSelectedCustomIconIndex(Mathf.CeilToInt(x2));
						yield return new WaitForSeconds(0.15f);
					}
				}
				else if (Mathf.CeilToInt(y) != 0)
				{
					int num3 = -Mathf.CeilToInt(y);
					if ((num3 == 1 && selectedCustomIconIndex < 4) || (num3 == -1 && selectedCustomIconIndex >= 4))
					{
						ChangeSelectedCustomIconIndex(num3 * 4);
						yield return new WaitForSeconds(0.15f);
					}
				}
			}
			if (InputMaster.input.UISelect())
			{
				RectTransformUtility.ScreenPointToLocalPointInRectangle(mapImage.rectTransform, mapCursor.transform.position, null, out var localPoint);
				NetworkMapSharer.Instance.localChar.CmdPlacePlayerPlacedIconOnMap(localPoint / 2f, selectedCustomIconIndex);
				SoundManager.Instance.play2DSound(placeMarkerSound);
				mapCursor.PlaceButtonPing();
				ClosePlayerPlacedMapIconsSelector();
			}
			if (iconSelectorOpen && (InputMaster.input.UICancel() || (Inventory.Instance.usingMouse && InputMaster.input.Interact())))
			{
				ClosePlayerPlacedMapIconsSelector();
			}
		}
		mapCursor.setPressing(isPressing: false);
	}

	public void ClosePlayerPlacedMapIconsSelector()
	{
		if (selectTeleWindowOpen)
		{
			CloseTeleSelectWindow();
		}
		else if (iconSelectorOpen)
		{
			iconSelectorOpen = false;
			iconSelectorWindow.SetActive(value: false);
		}
	}

	public void ChangeSelectedCustomIconIndex(int changeBy)
	{
		selectedCustomIconIndex += changeBy;
		if (selectedCustomIconIndex >= iconButtons.Length)
		{
			selectedCustomIconIndex = 0;
		}
		if (selectedCustomIconIndex < 0)
		{
			selectedCustomIconIndex = iconButtons.Length - 1;
		}
		for (int i = 0; i < iconButtons.Length; i++)
		{
			iconButtons[i].enabled = false;
		}
		iconButtons[selectedCustomIconIndex].enabled = true;
		SoundManager.Instance.play2DSound(SoundManager.Instance.inventorySound);
	}

	public void changeScale(float dif)
	{
		dif = Mathf.Clamp(dif, -2f, 2f);
		scale += dif * scale / 5f;
	}

	public void RecenterToCharacterPosition()
	{
		mapXPosDif = 0f;
		mapYPosDif = 0f;
	}

	public void openTeleSelectWindow(string dirSelected)
	{
		teleDir = dirSelected;
		teleSelectWindow.SetActive(value: true);
		selectTeleWindowOpen = true;
	}

	public void CloseTeleSelectWindow()
	{
		teleSelectWindow.SetActive(value: false);
		selectTeleWindowOpen = false;
	}

	public void ConfirmTele()
	{
		CloseTeleSelectWindow();
		MenuButtonsTop.menu.closeWindow();
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.TeleportSomewhere);
		NetworkMapSharer.Instance.localChar.CmdTeleport(teleDir);
	}

	public void ChangeMapWindow()
	{
		if (mapOpen)
		{
			mapSubWindow.gameObject.SetActive(value: true);
			mapWindow.SetParent(mapSubWindow.transform.Find("MapPos"));
			mapWindow.SetSiblingIndex(0);
			mapWindow.anchoredPosition = Vector3.zero;
			Cursor.lockState = CursorLockMode.Locked;
			mapWindow.sizeDelta = mapWindow.GetComponentInParent<RectTransform>().sizeDelta;
			mapMask.localScale = new Vector3(1f, 1f, 1f);
			mapWindowShape.sprite = mapWindowSquare;
			mapWindowShape.type = Image.Type.Sliced;
			mapCircle.gameObject.SetActive(value: false);
			scale = openedScale;
			desiredScale = openedScale;
			mapMask.localRotation = Quaternion.Euler(0f, 0f, 0f - CameraController.control.transform.eulerAngles.y);
			compass.gameObject.SetActive(value: false);
			buttonPrompt.gameObject.SetActive(value: false);
			CurrencyWindows.currency.windowOn(enabled: false);
			return;
		}
		mapSubWindow.gameObject.SetActive(value: false);
		mapMask.localRotation = Quaternion.Euler(0f, 0f, CameraController.control.transform.eulerAngles.y);
		compass.localRotation = Quaternion.Euler(0f, 0f, CameraController.control.transform.eulerAngles.y);
		charPointer.localRotation = Quaternion.Euler(0f, 0f, 0f - CameraController.control.transform.eulerAngles.y);
		Cursor.lockState = CursorLockMode.None;
		mapMask.localScale = new Vector3(0.285f, 0.285f, 0.285f);
		mapWindowShape.sprite = mapWindowCircle;
		mapWindowShape.type = Image.Type.Simple;
		mapCircle.anchoredPosition = new Vector3(-100f, -100f, 0f);
		mapWindow.SetParent(mapCircle);
		mapWindow.anchoredPosition = Vector3.zero;
		mapWindow.sizeDelta = mapCircle.sizeDelta * 4f;
		compass.gameObject.SetActive(value: true);
		buttonPrompt.gameObject.SetActive(value: true);
		openedScale = scale;
		scale = 20f;
		desiredScale = 20f;
		if (!TownManager.manage.mapUnlocked)
		{
			mapCircle.gameObject.SetActive(value: false);
			CurrencyWindows.currency.windowOn(enabled: false);
		}
		else if (MenuButtonsTop.menu.subMenuOpen || WeatherManager.Instance.IsMyPlayerInside || ChestWindow.chests.chestWindowOpen || CraftingManager.manage.craftMenuOpen || CheatScript.cheat.cheatMenuOpen || HouseEditor.edit.windowOpen || BulletinBoard.board.windowOpen)
		{
			mapCircle.gameObject.SetActive(value: false);
			CurrencyWindows.currency.windowOn(enabled: true);
		}
		else
		{
			mapCircle.gameObject.SetActive(value: true);
			CurrencyWindows.currency.windowOn(enabled: false);
		}
	}

	public void RunMapFollow()
	{
		if ((bool)charToPointTo)
		{
			if (!mapOpen)
			{
				mapXPosDif = 0f;
				mapYPosDif = 0f;
			}
			if (firstOpen)
			{
				mapWindow.gameObject.SetActive(value: true);
				firstOpen = false;
			}
			scale = Mathf.Clamp(scale, 0.75f, 25f);
			if (Mathf.Abs(desiredScale - scale) < 0.005f)
			{
				desiredScale = scale;
			}
			else
			{
				desiredScale = Mathf.Lerp(desiredScale, scale, Time.deltaTime * 5f);
			}
			mapParent.localScale = new Vector3(desiredScale, desiredScale, 1f);
			float num = mapXPosDif + 250f * (0f - desiredScale) + (mapXPosDif + 250f - charPointer.localPosition.x) * desiredScale;
			float num2 = mapYPosDif + 250f * (0f - desiredScale) + (mapYPosDif + 250f - charPointer.localPosition.y) * desiredScale;
			mapParent.localPosition = new Vector3(num - mapXPosDif, num2 - mapYPosDif, 0f);
			if (!mapOpen && OptionsMenu.options.mapFacesNorth)
			{
				charDirPointer.localRotation = Quaternion.Euler(0f, 0f, 0f - charToPointTo.eulerAngles.y);
			}
			else if (mapOpen)
			{
				charDirPointer.localRotation = Quaternion.Euler(0f, 0f, 0f - charToPointTo.eulerAngles.y);
			}
			else
			{
				charDirPointer.localRotation = Quaternion.Euler(0f, 0f, CameraController.control.transform.eulerAngles.y - charToPointTo.eulerAngles.y);
			}
			Vector3 vector = new Vector3(charToPointTo.position.x / 2f / mapScale, charToPointTo.position.z / 2f / mapScale, 1f);
			charPointer.localPosition = new Vector3(vector.x, vector.y, 1f);
			TrackOtherPlayers();
			if (!mapOpen && OptionsMenu.options.mapFacesNorth)
			{
				compass.localRotation = Quaternion.Euler(0f, 0f, 0f);
				mapMask.localRotation = Quaternion.Euler(0f, 0f, 0f);
				charPointer.localRotation = Quaternion.Euler(0f, 0f, 0f);
				charPointer.localRotation = Quaternion.Lerp(charPointer.localRotation, Quaternion.Euler(0f, 0f, 0f - CameraController.control.transform.eulerAngles.y), Time.deltaTime * 3f);
				charPointer.localScale = new Vector3(3f / desiredScale, 3f / desiredScale, 1f);
			}
			else if (!mapOpen)
			{
				mapMask.localRotation = Quaternion.Lerp(mapMask.localRotation, Quaternion.Euler(0f, 0f, CameraController.control.transform.eulerAngles.y), Time.deltaTime * 3f);
				compass.localRotation = Quaternion.Lerp(mapMask.localRotation, Quaternion.Euler(0f, 0f, CameraController.control.transform.eulerAngles.y), Time.deltaTime * 3f);
				charPointer.localRotation = Quaternion.Lerp(charPointer.localRotation, Quaternion.Euler(0f, 0f, 0f - CameraController.control.transform.eulerAngles.y), Time.deltaTime * 3f);
				charPointer.localScale = new Vector3(3f / desiredScale, 3f / desiredScale, 1f);
			}
			else
			{
				mapMask.localRotation = Quaternion.Euler(0f, 0f, 0f);
				charPointer.localRotation = Quaternion.Euler(0f, 0f, 0f);
				charPointer.localScale = new Vector3(1f / desiredScale, 1f / desiredScale, 1f);
			}
			if (refreshMap)
			{
				refreshMap = !refreshMap;
			}
		}
	}

	public IEnumerator ClearMapForUnderground()
	{
		mapWindowShape.color = Color.black;
		mapImage.texture = undergroundTex;
		int mapCounter = 0;
		int y = 0;
		yield return null;
		for (; y < WorldManager.Instance.GetMapSize(); y++)
		{
			for (int i = 0; i < WorldManager.Instance.GetMapSize(); i++)
			{
				undergroundPix[y * WorldManager.Instance.GetMapSize() + i] = Color.black;
			}
			if ((float)mapCounter < 50f)
			{
				mapCounter++;
				continue;
			}
			mapCounter = 0;
			yield return null;
		}
		undergroundTex.SetPixels(undergroundPix);
		undergroundTex.Apply();
		mapMaterial.SetTexture("_MainTex", undergroundTex);
		ChangeWorldArea(WorldArea.UNDERGROUND);
		litSquares = new bool[WorldManager.Instance.GetMapSize(), WorldManager.Instance.GetMapSize()];
		while (RealWorldTimeLight.time.underGround)
		{
			checkIfChunkIsOnUndergroundMap(charToPointTo.position);
			yield return mapWait;
		}
		litSquares = null;
	}

	public IEnumerator ClearMapForOffIsland()
	{
		mapWindowShape.color = Color.blue;
		mapImage.texture = undergroundTex;
		int mapCounter = 0;
		int y = 0;
		yield return null;
		for (; y < WorldManager.Instance.GetMapSize(); y++)
		{
			for (int i = 0; i < WorldManager.Instance.GetMapSize(); i++)
			{
				undergroundPix[y * WorldManager.Instance.GetMapSize() + i] = Color.blue;
			}
			if ((float)mapCounter < 50f)
			{
				mapCounter++;
				continue;
			}
			mapCounter = 0;
			yield return null;
		}
		undergroundTex.SetPixels(undergroundPix);
		undergroundTex.Apply();
		mapMaterial.SetTexture("_MainTex", undergroundTex);
		ChangeWorldArea(WorldArea.OFF_ISLAND);
		while (RealWorldTimeLight.time.offIsland)
		{
			checkIfChunkIsOnOffIslandMap(charToPointTo.position);
			yield return mapWait;
		}
	}

	public IEnumerator ScanTheMap()
	{
		mapWindowShape.color = defaultMapMaskBackgroundColour;
		mapImage.texture = noiseTex;
		int mapCounter = 0;
		int y = 0;
		yield return null;
		for (; y < WorldManager.Instance.GetMapSize(); y++)
		{
			for (int i = 0; i < WorldManager.Instance.GetMapSize(); i++)
			{
				CheckIfNeedsIcon(i, y);
				if (WorldManager.Instance.heightMap[i, y] < 1)
				{
					if (GetTileObjectId(WorldManager.Instance.onTileMap[i, y]) != -1)
					{
						pix[y * WorldManager.Instance.GetMapSize() + i] = tileObjectsShownOnMapColor[GetTileObjectId(WorldManager.Instance.onTileMap[i, y])];
					}
					else if (WorldManager.Instance.heightMap[i, y] < -1)
					{
						pix[y * WorldManager.Instance.GetMapSize() + i] = deepWater;
					}
					else
					{
						pix[y * WorldManager.Instance.GetMapSize() + i] = water;
					}
				}
				else if (GetTileObjectId(WorldManager.Instance.onTileMap[i, y]) != -1)
				{
					pix[y * WorldManager.Instance.GetMapSize() + i] = tileObjectsShownOnMapColor[GetTileObjectId(WorldManager.Instance.onTileMap[i, y])];
				}
				else
				{
					pix[y * WorldManager.Instance.GetMapSize() + i] = Color.Lerp(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[i, y]].tileColorOnMap, mapBackColor, (float)WorldManager.Instance.heightMap[i, y] / 12f);
					if (checkIfShouldBeHeightLine(i, y))
					{
						pix[y * WorldManager.Instance.GetMapSize() + i] = Color.Lerp(pix[y * WorldManager.Instance.GetMapSize() + i], heightLineColour, 0.075f);
					}
				}
			}
			if ((float)mapCounter < 50f)
			{
				mapCounter++;
				continue;
			}
			mapCounter = 0;
			yield return null;
		}
		DrawBridgesOnMap();
		noiseTex.SetPixels(pix);
		noiseTex.Apply();
		mapMaterial.SetTexture("_MainTex", noiseTex);
		ScanAndUpdateScanAMapIconHighlights();
	}

	private bool checkIfShouldBeHeightLine(int xPos, int yPos)
	{
		if (xPos < 2 || xPos >= WorldManager.Instance.GetMapSize() - 2 || yPos < 2 || yPos >= WorldManager.Instance.GetMapSize() - 2)
		{
			return false;
		}
		if (WorldManager.Instance.heightMap[xPos - 1, yPos] < WorldManager.Instance.heightMap[xPos, yPos] || WorldManager.Instance.heightMap[xPos + 1, yPos] < WorldManager.Instance.heightMap[xPos, yPos] || WorldManager.Instance.heightMap[xPos, yPos + 1] < WorldManager.Instance.heightMap[xPos, yPos] || WorldManager.Instance.heightMap[xPos, yPos - 1] < WorldManager.Instance.heightMap[xPos, yPos])
		{
			return true;
		}
		return false;
	}

	public void ChangeWorldArea(WorldArea area)
	{
		for (int i = 0; i < mapIcons.Count; i++)
		{
			if (mapIcons[i].Icon.sprite == mineSprite && area == WorldArea.UNDERGROUND)
			{
				mapIcons[i].NetworkmapIconLevelIndex = (int)area;
			}
			if (mapIcons[i].Icon.sprite == airportSprite && area == WorldArea.OFF_ISLAND)
			{
				mapIcons[i].NetworkmapIconLevelIndex = (int)area;
			}
			if (mapIcons[i].VehicleFollowingId != 0)
			{
				mapIcons[i].NetworkmapIconLevelIndex = (int)area;
			}
			bool flag = mapIcons[i].mapIconLevelIndex == (int)area;
			mapIcons[i].NetworkIsVisible = flag;
			mapIcons[i].container.SetActive(flag);
		}
	}

	public void updateMapOnPlaced()
	{
		StartCoroutine(updateMap());
	}

	public void checkIfChunkIsOnUndergroundMap(Vector3 playerPos)
	{
		int num = Mathf.RoundToInt(playerPos.x / 2f / (float)WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
		int num2 = Mathf.RoundToInt(playerPos.z / 2f / (float)WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
		int num3 = num;
		int num4 = num2;
		int chunkSize = WorldManager.Instance.getChunkSize();
		Color color = Color.Lerp(Color.red, Color.yellow, 0.45f);
		for (int i = -2; i < 3; i++)
		{
			for (int j = -2; j < 3; j++)
			{
				num3 = num + j * chunkSize;
				num4 = num2 + i * chunkSize;
				for (int k = 0; k < 10; k++)
				{
					for (int l = 0; l < 10; l++)
					{
						if (!(getDistanceFromCharacter(num3 + l, num4 + k) > 0f) || !WorldManager.Instance.isPositionOnMap(num3 + l, num4 + k) || litSquares[num3 + l, num4 + k])
						{
							continue;
						}
						Color color2 = ((WorldManager.Instance.onTileMap[num3 + l, num4 + k] == 881) ? color : ((WorldManager.Instance.waterMap[num3 + l, num4 + k] && WorldManager.Instance.heightMap[num3 + l, num4 + k] < 1) ? ((WorldManager.Instance.onTileMap[num3 + l, num4 + k] == 881) ? Color.Lerp(Color.red, Color.yellow, 0.45f) : ((GetTileObjectId(WorldManager.Instance.onTileMap[num3 + l, num4 + k]) != -1) ? tileObjectsShownOnMapColor[GetTileObjectId(WorldManager.Instance.onTileMap[num3 + l, num4 + k])] : ((WorldManager.Instance.heightMap[num3 + l, num4 + k] >= -1) ? Color.Lerp(Color.blue, Color.white, 0.65f) : Color.Lerp(Color.blue, Color.white, 0.85f)))) : ((WorldManager.Instance.onTileMap[num3 + l, num4 + k] == 29 || WorldManager.Instance.onTileMap[num3 + l, num4 + k] == 508 || WorldManager.Instance.onTileMap[num3 + l, num4 + k] == 880) ? Color.Lerp(Color.grey, Color.black, 0.65f) : ((WorldManager.Instance.onTileMap[num3 + l, num4 + k] == 881) ? Color.Lerp(Color.red, Color.yellow, 0.45f) : ((GetTileObjectId(WorldManager.Instance.onTileMap[num3 + l, num4 + k]) <= -1) ? Color.Lerp(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num3 + l, num4 + k]].tileColorOnMap, mapBackColor, (float)WorldManager.Instance.heightMap[num3 + l, num4 + k] / 12f) : tileObjectsShownOnMapColor[GetTileObjectId(WorldManager.Instance.onTileMap[num3 + l, num4 + k])])))));
						if (!litSquares[num3 + l, num4 + k] && undergroundPix[(num4 + k) * WorldManager.Instance.GetMapSize() + (num3 + l)] != color2 && (undergroundPix[(num4 + k) * WorldManager.Instance.GetMapSize() + (num3 + l)] == Color.black || undergroundPix[(num4 + k) * WorldManager.Instance.GetMapSize() + (num3 + l)].a < getDistanceFromCharacter(num3 + l, num4 + k)))
						{
							undergroundPix[(num4 + k) * WorldManager.Instance.GetMapSize() + (num3 + l)] = color2;
							undergroundPix[(num4 + k) * WorldManager.Instance.GetMapSize() + (num3 + l)].a = getDistanceFromCharacter(num3 + l, num4 + k);
							if (undergroundPix[(num4 + k) * WorldManager.Instance.GetMapSize() + (num3 + l)].a == 1f)
							{
								litSquares[num3 + l, num4 + k] = true;
							}
						}
					}
				}
			}
		}
		undergroundTex.SetPixels(undergroundPix);
		undergroundTex.Apply();
		mapMaterial.SetTexture("_MainTex", undergroundTex);
	}

	public void checkIfChunkIsOnOffIslandMap(Vector3 playerPos)
	{
		int num = Mathf.RoundToInt(playerPos.x / 2f / (float)WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
		int num2 = Mathf.RoundToInt(playerPos.z / 2f / (float)WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
		int num3 = num;
		int num4 = num2;
		for (int i = -2; i < 3; i++)
		{
			for (int j = -2; j < 3; j++)
			{
				num3 = num + j * WorldManager.Instance.getChunkSize();
				num4 = num2 + i * WorldManager.Instance.getChunkSize();
				for (int k = 0; k < 10; k++)
				{
					for (int l = 0; l < 10; l++)
					{
						if (getDistanceFromCharacter(num3 + l, num4 + k) > 0f && WorldManager.Instance.isPositionOnMap(num3 + l, num4 + k))
						{
							Color color = ((WorldManager.Instance.heightMap[num3 + l, num4 + k] < 1) ? ((GetTileObjectId(WorldManager.Instance.onTileMap[num3 + l, num4 + k]) != -1) ? tileObjectsShownOnMapColor[GetTileObjectId(WorldManager.Instance.onTileMap[num3 + l, num4 + k])] : ((WorldManager.Instance.heightMap[num3 + l, num4 + k] >= -1) ? Color.Lerp(Color.blue, Color.white, 0.65f) : Color.Lerp(Color.blue, Color.white, 0.85f))) : ((WorldManager.Instance.onTileMap[num3 + l, num4 + k] == 29 || WorldManager.Instance.onTileMap[num3 + l, num4 + k] == 508) ? Color.Lerp(Color.grey, Color.black, 0.65f) : ((GetTileObjectId(WorldManager.Instance.onTileMap[num3 + l, num4 + k]) <= -1) ? Color.Lerp(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num3 + l, num4 + k]].tileColorOnMap, mapBackColor, (float)WorldManager.Instance.heightMap[num3 + l, num4 + k] / 12f) : tileObjectsShownOnMapColor[GetTileObjectId(WorldManager.Instance.onTileMap[num3 + l, num4 + k])])));
							if (undergroundPix[(num4 + k) * WorldManager.Instance.GetMapSize() + (num3 + l)] != color && (undergroundPix[(num4 + k) * WorldManager.Instance.GetMapSize() + (num3 + l)] == Color.blue || undergroundPix[(num4 + k) * WorldManager.Instance.GetMapSize() + (num3 + l)].a < getDistanceFromCharacter(num3 + l, num4 + k)))
							{
								undergroundPix[(num4 + k) * WorldManager.Instance.GetMapSize() + (num3 + l)] = color;
								undergroundPix[(num4 + k) * WorldManager.Instance.GetMapSize() + (num3 + l)].a = getDistanceFromCharacter(num3 + l, num4 + k);
							}
						}
					}
				}
			}
		}
		undergroundTex.SetPixels(undergroundPix);
		undergroundTex.Apply();
		mapMaterial.SetTexture("_MainTex", undergroundTex);
	}

	public void ReturnMapToMainIslandView()
	{
		mapImage.texture = noiseTex;
		mapWindowShape.color = defaultMapMaskBackgroundColour;
		noiseTex.SetPixels(pix);
		noiseTex.Apply();
		mapMaterial.SetTexture("_MainTex", noiseTex);
		ChangeWorldArea(WorldArea.MAIN_ISLAND);
		StartCoroutine(ScanTheMap());
	}

	public float getDistanceFromCharacter(int x, int y)
	{
		float num = Vector3.Distance(new Vector3(charToPointTo.transform.position.x, 0f, charToPointTo.transform.position.z), new Vector3(x * 2, 0f, y * 2));
		if (num <= 18f)
		{
			return 1f;
		}
		return 1f - Mathf.Clamp(num, 18f, 40f) / 40f;
	}

	public IEnumerator updateMap()
	{
		mapImage.texture = noiseTex;
		for (int y = 0; y < WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize(); y++)
		{
			for (int x = 0; x < WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize(); x++)
			{
				if (!WorldManager.Instance.chunkChangedMap[x, y])
				{
					continue;
				}
				for (int i = 0; i < 10; i++)
				{
					for (int j = 0; j < 10; j++)
					{
						CheckIfNeedsIcon(x * 10 + j, y * 10 + i);
						if (WorldManager.Instance.heightMap[x * 10 + j, y * 10 + i] < 1)
						{
							if (GetTileObjectId(WorldManager.Instance.onTileMap[x * 10 + j, y * 10 + i]) != -1)
							{
								pix[(y * 10 + i) * WorldManager.Instance.GetMapSize() + (x * 10 + j)] = tileObjectsShownOnMapColor[GetTileObjectId(WorldManager.Instance.onTileMap[x * 10 + j, y * 10 + i])];
							}
							else if (WorldManager.Instance.heightMap[x * 10 + j, y * 10 + i] < -1)
							{
								pix[(y * 10 + i) * WorldManager.Instance.GetMapSize() + (x * 10 + j)] = deepWater;
							}
							else
							{
								pix[(y * 10 + i) * WorldManager.Instance.GetMapSize() + (x * 10 + j)] = water;
							}
						}
						else if (GetTileObjectId(WorldManager.Instance.onTileMap[x * 10 + j, y * 10 + i]) > -1)
						{
							pix[(y * 10 + i) * WorldManager.Instance.GetMapSize() + (x * 10 + j)] = tileObjectsShownOnMapColor[GetTileObjectId(WorldManager.Instance.onTileMap[x * 10 + j, y * 10 + i])];
						}
						else
						{
							pix[(y * 10 + i) * WorldManager.Instance.GetMapSize() + (x * 10 + j)] = Color.Lerp(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[x * 10 + j, y * 10 + i]].tileColorOnMap, mapBackColor, (float)WorldManager.Instance.heightMap[x * 10 + j, y * 10 + i] / 12f);
							if (checkIfShouldBeHeightLine(x * 10 + j, y * 10 + i))
							{
								pix[(y * 10 + i) * WorldManager.Instance.GetMapSize() + (x * 10 + j)] = Color.Lerp(pix[(y * 10 + i) * WorldManager.Instance.GetMapSize() + (x * 10 + j)], heightLineColour, 0.075f);
							}
						}
					}
				}
				yield return null;
			}
		}
		DrawBridgesOnMap();
		noiseTex.SetPixels(pix);
		noiseTex.Apply();
		mapMaterial.SetTexture("_MainTex", noiseTex);
		ScanAndUpdateScanAMapIconHighlights();
	}

	public void trackOtherPlayers(Transform trackMe)
	{
		otherPlayersToTrack.Add(trackMe);
		RectTransform component = Object.Instantiate(otherCharPointerPrefab, mapParent).GetComponent<RectTransform>();
		otherPlayerIcons.Add(component);
		component.GetComponent<OtherPlayerIcon>().setName(trackMe.GetComponent<EquipItemToChar>().playerName);
		PlayerMarkersOnTop();
	}

	public void changeMapIconName(Transform changeMe, string newName)
	{
		for (int i = 0; i < otherPlayersToTrack.Count; i++)
		{
			if (otherPlayersToTrack[i] == changeMe)
			{
				otherPlayerIcons[i].GetComponent<OtherPlayerIcon>().setName(newName);
			}
		}
	}

	public void unTrackOtherPlayers(Transform unTrackMe)
	{
		if (otherPlayersToTrack.Contains(unTrackMe))
		{
			int index = otherPlayersToTrack.IndexOf(unTrackMe);
			otherPlayersToTrack.RemoveAt(index);
			Object.Destroy(otherPlayerIcons[index].gameObject);
			otherPlayerIcons.RemoveAt(index);
		}
	}

	public void UpdateIconName(int xPos, int yPos, string newName)
	{
		for (int i = 0; i < mapIcons.Count; i++)
		{
			if (mapIcons[i].TileObjectId == WorldManager.Instance.onTileMap[xPos, yPos])
			{
				mapIcons[i].SetUp(WorldManager.Instance.onTileMap[xPos, yPos], xPos, yPos);
				mapIcons[i].IconName = newName;
				break;
			}
		}
	}

	public void CheckIfNeedsIcon(int xPos, int yPos)
	{
		if (WorldManager.Instance.onTileMap[xPos, yPos] > -1 && (bool)WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].mapIcon)
		{
			for (int i = 0; i < mapIcons.Count; i++)
			{
				if (mapIcons[i].CurrentIconType != 0 && mapIcons[i].TileObjectId == WorldManager.Instance.onTileMap[xPos, yPos])
				{
					mapIcons[i].SetUp(WorldManager.Instance.onTileMap[xPos, yPos], xPos, yPos);
					return;
				}
			}
			mapIcon component = Object.Instantiate(mapIconPrefab, mapParent).GetComponent<mapIcon>();
			component.SetUp(WorldManager.Instance.onTileMap[xPos, yPos], xPos, yPos);
			mapIcons.Add(component);
			PlayerMarkersOnTop();
		}
		else if (WorldManager.Instance.onTileMap[xPos, yPos] > -1 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectBridge)
		{
			bridgePositions.Add(new int[2] { xPos, yPos });
		}
	}

	public void RenameIcon(int tileObjectId, string newName)
	{
		for (int i = 0; i < mapIcons.Count; i++)
		{
			if (mapIcons[i].CurrentIconType != 0 && mapIcons[i].TileObjectId == tileObjectId)
			{
				mapIcons[i].IconName = newName;
				break;
			}
		}
	}

	public void createTeleIcons(string dir)
	{
		mapIcon component = Object.Instantiate(mapIconPrefab, mapParent).GetComponent<mapIcon>();
		component.SetUpTelePoint(dir);
		mapIcons.Add(component);
		PlayerMarkersOnTop();
	}

	public void CreateTaskIcon(PostOnBoard postToTrack)
	{
		if (postToTrack.getRequiredLocation() != Vector3.zero && !GetTaskAlreadyHasIcon(postToTrack))
		{
			mapIcon component = Object.Instantiate(mapIconPrefab, mapParent).GetComponent<mapIcon>();
			component.SetUpQuestIcon(postToTrack);
			mapIcons.Add(component);
			PlayerMarkersOnTop();
		}
	}

	public void RemoveTaskIcon(mapIcon toRemove)
	{
		Object.Destroy(toRemove.gameObject);
	}

	private void UpdateBioNameLabel()
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(mapImage.rectTransform, mapCursor.transform.position, null, out var localPoint);
		biomeName.text = ConversationGenerator.generate.GetBiomeNameText(GenerateMap.generate.getBiomeNameUnderMapCursor((int)(localPoint.x * 2f), (int)(localPoint.y * 2f)));
	}

	private bool GetTaskAlreadyHasIcon(PostOnBoard postToTrack)
	{
		for (int i = 0; i < mapIcons.Count; i++)
		{
			if (mapIcons[i].isConnectedToTask(postToTrack))
			{
				return true;
			}
		}
		return false;
	}

	public mapIcon CreateNewNetworkedPlayerSetMarker(Vector2 position, int iconSpriteIndex)
	{
		mapIcon mapIcon2 = Object.Instantiate(mapIconPrefab, position, Quaternion.identity);
		mapIcon2.transform.localScale = Vector3.zero;
		mapIcon2.SetUpPlayerPlacedMarker(new Vector3(position.x * 8f, 0f, position.y * 8f), iconSpriteIndex);
		mapIcons.Add(mapIcon2);
		PlayerMarkersOnTop();
		return mapIcon2;
	}

	public mapIcon CreateNewNetworkedNpcMarker(Vector2 position, int npcId)
	{
		mapIcon mapIcon2 = Object.Instantiate(npcMapIconPrefab, position, Quaternion.identity);
		mapIcon2.transform.localScale = Vector3.zero;
		mapIcon2.isNpcMarker = true;
		mapIcon2.SetUpPlayerPlacedMarker(new Vector3(position.x * 8f, 0f, position.y * 8f), npcId);
		mapIcons.Add(mapIcon2);
		PlayerMarkersOnTop();
		return mapIcon2;
	}

	public void ClearAllNPCMarkers()
	{
		for (int num = mapIcons.Count - 1; num >= 0; num--)
		{
			if (mapIcons[num].isNpcMarker)
			{
				NetworkServer.Destroy(mapIcons[num].gameObject);
				mapIcons.RemoveAt(num);
			}
		}
		if (clearNPCRoutine != null)
		{
			StopCoroutine(clearNPCRoutine);
			clearNPCRoutine = null;
		}
	}

	public void StartNPCMarkerCountdown()
	{
		if (clearNPCRoutine != null)
		{
			StopCoroutine(clearNPCRoutine);
			clearNPCRoutine = null;
		}
		clearNPCRoutine = StartCoroutine(NPCMarkerCountdown());
	}

	private IEnumerator NPCMarkerCountdown()
	{
		float timer = 35f;
		while (timer > 0f)
		{
			timer -= Time.deltaTime;
			yield return null;
		}
		ClearAllNPCMarkers();
	}

	public mapIcon CreateMapIconForVehicle(uint netId, int vehicleSaveId)
	{
		if (vehicleSaveId < 0)
		{
			return null;
		}
		NetworkIdentity.spawned[netId].gameObject.GetComponent<Vehicle>();
		return Object.Instantiate(mapIconPrefab, Vector3.zero, Quaternion.identity);
	}

	public mapIcon CreateSpecialMapMarker(Vector3 position, int specialId)
	{
		mapIcon component = Object.Instantiate(mapIconPrefab).GetComponent<mapIcon>();
		component.SetUpAsSpecialIcon(position, specialId);
		mapIcons.Add(component);
		return component;
	}

	public void removeSpecialMapMarker(mapIcon removeIcon)
	{
		Object.Destroy(removeIcon.gameObject);
	}

	public void TrackOtherPlayers()
	{
		for (int i = 0; i < otherPlayerIcons.Count; i++)
		{
			Vector3 vector = new Vector3(otherPlayersToTrack[i].position.x / 2f / mapScale, otherPlayersToTrack[i].position.z / 2f / mapScale, 1f);
			otherPlayerIcons[i].localPosition = new Vector3(vector.x, vector.y, 1f);
			if (!mapOpen)
			{
				if (OptionsMenu.options.mapFacesNorth)
				{
					otherPlayerIcons[i].localRotation = Quaternion.Euler(0f, 0f, 0f);
				}
				else
				{
					otherPlayerIcons[i].localRotation = Quaternion.Lerp(otherPlayerIcons[i].localRotation, Quaternion.Euler(0f, 0f, 0f - CameraController.control.transform.eulerAngles.y), Time.deltaTime * 3f);
				}
				otherPlayerIcons[i].localScale = new Vector3(2f / desiredScale, 2f / desiredScale, 1f);
			}
			else
			{
				otherPlayerIcons[i].localRotation = Quaternion.Euler(0f, 0f, 0f);
				otherPlayerIcons[i].localScale = new Vector3(1f / desiredScale, 1f / desiredScale, 1f);
			}
		}
	}

	public void ConnectMainChar(Transform mainChar)
	{
		charToPointTo = mainChar;
		Instance.ChangeMapWindow();
		if (RealWorldTimeLight.time.underGround)
		{
			StartCoroutine(ClearMapForUnderground());
		}
		else if (RealWorldTimeLight.time.offIsland)
		{
			StartCoroutine(ClearMapForOffIsland());
		}
		else
		{
			StartCoroutine(ScanTheMap());
		}
	}

	public int GetTileObjectId(int onThisTile)
	{
		if (tileObjectShowOnMap.Length != 0)
		{
			for (int i = 0; i < tileObjectShowOnMap.Length; i++)
			{
				if (tileObjectShowOnMap[i].tileObjectId == onThisTile)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public void handlePointerSizeAndPos(RectTransform pointerName, Vector3 pointToPosition)
	{
		if (!(pointToPosition != Vector3.zero))
		{
			return;
		}
		if (mapOpen)
		{
			pointerName.localPosition = new Vector3(pointToPosition.x / 2f / mapScale, pointToPosition.z / 2f / mapScale, 1f);
			pointerName.localRotation = Quaternion.Euler(0f, 0f, 0f);
			pointerName.localScale = new Vector3(2f / desiredScale, 2f / desiredScale, 1f);
			return;
		}
		pointerName.localRotation = Quaternion.Lerp(pointerName.localRotation, Quaternion.Euler(0f, 0f, 0f - CameraController.control.transform.eulerAngles.y), Time.deltaTime * 3f);
		Vector3 a = new Vector3(charToPointTo.position.x, 0f, charToPointTo.position.z);
		Vector3 b = new Vector3(pointToPosition.x, 0f, pointToPosition.z);
		if (Vector3.Distance(a, b) < 100f)
		{
			pointerName.localPosition = Vector3.Lerp(pointerName.localPosition, new Vector3(pointToPosition.x / 2f / mapScale, pointToPosition.z / 2f / mapScale, 1f), Time.deltaTime * 3f);
			pointerName.localScale = Vector3.Lerp(pointerName.localScale, new Vector3(2f / desiredScale, 2f / desiredScale, 1f), Time.deltaTime * 2f);
		}
		else
		{
			Vector3 vector = charToPointTo.position + (pointToPosition - charToPointTo.position).normalized * 115f;
			pointerName.localPosition = Vector3.Lerp(pointerName.localPosition, new Vector3(vector.x / 2f / mapScale, vector.z / 2f / mapScale, 1f), Time.deltaTime * 3f);
			pointerName.localScale = Vector3.Lerp(pointerName.localScale, new Vector3(1.25f / desiredScale, 1.25f / desiredScale, 1f), Time.deltaTime * 2f);
		}
	}

	public void PlayerMarkersOnTop()
	{
		for (int i = 0; i < otherPlayerIcons.Count; i++)
		{
			otherPlayerIcons[i].SetAsLastSibling();
		}
		charPointer.SetAsLastSibling();
	}

	private mapIcon IsMouseHoveringMapIcon()
	{
		PointerEventData pointerEventData = new PointerEventData(null);
		pointerEventData.position = mapCursor.transform.position;
		List<RaycastResult> list = new List<RaycastResult>();
		mapCaster.Raycast(pointerEventData, list);
		for (int i = 0; i < list.Count; i++)
		{
			mapIcon componentInParent = list[i].gameObject.GetComponentInParent<mapIcon>();
			if (componentInParent != null)
			{
				return componentInParent;
			}
		}
		return null;
	}

	private mapIcon GetMapIconMouseIsHovering()
	{
		PointerEventData pointerEventData = new PointerEventData(null);
		pointerEventData.position = mapCursor.transform.position;
		List<RaycastResult> list = new List<RaycastResult>();
		mapCaster.Raycast(pointerEventData, list);
		for (int i = 0; i < list.Count; i++)
		{
			mapIcon componentInParent = list[i].gameObject.GetComponentInParent<mapIcon>();
			if (componentInParent != null)
			{
				return componentInParent;
			}
		}
		return null;
	}

	public void DrawBridgesOnMap()
	{
		for (int i = 0; i < bridgePositions.Count; i++)
		{
			int num = bridgePositions[i][0];
			int num2 = bridgePositions[i][1];
			Color bridgeColour = WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[num, num2]].tileObjectBridge.bridgeColour;
			pix[num2 * WorldManager.Instance.GetMapSize() + num] = bridgeColour;
			for (int j = 1; WorldManager.Instance.onTileMap[num + j, num2] < -1; j++)
			{
				pix[num2 * WorldManager.Instance.GetMapSize() + (num + j)] = bridgeColour;
			}
			for (int k = 1; WorldManager.Instance.onTileMap[num, num2 + k] < -1; k++)
			{
				pix[(num2 + k) * WorldManager.Instance.GetMapSize() + num] = bridgeColour;
				for (int l = 1; WorldManager.Instance.onTileMap[num + l, num2 + k] < -1; l++)
				{
					pix[(num2 + k) * WorldManager.Instance.GetMapSize() + (num + l)] = bridgeColour;
				}
			}
		}
		bridgePositions.Clear();
	}

	private void OnDestroy()
	{
		Object.Destroy(noiseTex);
		noiseTex = null;
		Object.Destroy(undergroundTex);
		undergroundTex = null;
		Instance = null;
	}
}
