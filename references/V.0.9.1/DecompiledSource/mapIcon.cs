using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class mapIcon : NetworkBehaviour
{
	public enum iconType
	{
		PlayerPlaced,
		TileObject,
		Vehicle,
		Teletower,
		CameraQuest,
		HuntingQuest,
		InvestigationQuest,
		Special
	}

	public enum negativeIconId
	{
		NotUsed,
		NotUsed2,
		CameraQuestComplete,
		CameraQuest,
		HuntingQuestComplete,
		HuntingQuest,
		InvestigationQuest,
		NickMarker
	}

	public Image Icon;

	[SyncVar(hook = "OnMyMapIconMapLevelChanged")]
	public int mapIconLevelIndex;

	private iconType _currentIconType;

	[SyncVar(hook = "OnHighlightChange")]
	public bool IconShouldBeHighlighted;

	[SyncVar(hook = "OnPositionChanged")]
	public Vector3 PointingAtPosition;

	public MapPoint MyMapPoint = new MapPoint
	{
		X = -1,
		Y = -1
	};

	public Vector3 localPointingAtPosition;

	[SyncVar(hook = "OnSpriteIndexChanged")]
	public int IconId;

	[SyncVar(hook = "OnIsVisibleChanged")]
	public bool IsVisible;

	[SyncVar(hook = "OnVehicleFollowingChanged")]
	public uint VehicleFollowingId;

	[SerializeField]
	private Sprite teleTowerIcon;

	[SerializeField]
	private Sprite cameraIcon;

	[SerializeField]
	private Sprite cameraCompleteIcon;

	[SerializeField]
	private Sprite huntingIcon;

	[SerializeField]
	private Sprite huntingCompleIcon;

	[SerializeField]
	private Sprite investigationIcon;

	public GameObject container;

	[SerializeField]
	private GameObject ping;

	private RectTransform myRectTransform;

	private Transform followTransform;

	private PostOnBoard myPost;

	public bool isNpcMarker;

	public iconType CurrentIconType
	{
		get
		{
			return _currentIconType;
		}
		set
		{
			_currentIconType = value;
		}
	}

	public string TelePointName { get; set; } = string.Empty;


	public string IconName { get; set; } = string.Empty;


	public int TileObjectId { get; set; }

	public uint PlacedByNetworkedPlayerId { get; set; }

	public int NetworkmapIconLevelIndex
	{
		get
		{
			return mapIconLevelIndex;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref mapIconLevelIndex))
			{
				int oldValue = mapIconLevelIndex;
				SetSyncVar(value, ref mapIconLevelIndex, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					OnMyMapIconMapLevelChanged(oldValue, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public bool NetworkIconShouldBeHighlighted
	{
		get
		{
			return IconShouldBeHighlighted;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref IconShouldBeHighlighted))
			{
				bool iconShouldBeHighlighted = IconShouldBeHighlighted;
				SetSyncVar(value, ref IconShouldBeHighlighted, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, value: true);
					OnHighlightChange(iconShouldBeHighlighted, value);
					setSyncVarHookGuard(2uL, value: false);
				}
			}
		}
	}

	public Vector3 NetworkPointingAtPosition
	{
		get
		{
			return PointingAtPosition;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref PointingAtPosition))
			{
				Vector3 pointingAtPosition = PointingAtPosition;
				SetSyncVar(value, ref PointingAtPosition, 4uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(4uL))
				{
					setSyncVarHookGuard(4uL, value: true);
					OnPositionChanged(pointingAtPosition, value);
					setSyncVarHookGuard(4uL, value: false);
				}
			}
		}
	}

	public int NetworkIconId
	{
		get
		{
			return IconId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref IconId))
			{
				int iconId = IconId;
				SetSyncVar(value, ref IconId, 8uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(8uL))
				{
					setSyncVarHookGuard(8uL, value: true);
					OnSpriteIndexChanged(iconId, value);
					setSyncVarHookGuard(8uL, value: false);
				}
			}
		}
	}

	public bool NetworkIsVisible
	{
		get
		{
			return IsVisible;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref IsVisible))
			{
				bool isVisible = IsVisible;
				SetSyncVar(value, ref IsVisible, 16uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(16uL))
				{
					setSyncVarHookGuard(16uL, value: true);
					OnIsVisibleChanged(isVisible, value);
					setSyncVarHookGuard(16uL, value: false);
				}
			}
		}
	}

	public uint NetworkVehicleFollowingId
	{
		get
		{
			return VehicleFollowingId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref VehicleFollowingId))
			{
				uint vehicleFollowingId = VehicleFollowingId;
				SetSyncVar(value, ref VehicleFollowingId, 32uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(32uL))
				{
					setSyncVarHookGuard(32uL, value: true);
					OnVehicleFollowingChanged(vehicleFollowingId, value);
					setSyncVarHookGuard(32uL, value: false);
				}
			}
		}
	}

	private void Awake()
	{
		myRectTransform = GetComponent<RectTransform>();
		myRectTransform.pivot = new Vector2(0.5f, 0.5f);
	}

	private void Start()
	{
		base.transform.SetParent(RenderMap.Instance.mapParent.transform);
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		OnPositionChanged(Vector3.zero, PointingAtPosition);
		OnSpriteIndexChanged(-1, IconId);
		OnVehicleFollowingChanged(0u, VehicleFollowingId);
		OnMyMapIconMapLevelChanged(-1, mapIconLevelIndex);
		OnIsVisibleChanged(oldValue: false, IsVisible);
	}

	public void SetUp(int showingTileObjectId, int tileX, int tileY)
	{
		SetMyMapLevel();
		TileObjectId = showingTileObjectId;
		SetPosition(tileX, tileY);
		Icon.sprite = WorldManager.Instance.allObjectSettings[showingTileObjectId].mapIcon;
		Icon.color = WorldManager.Instance.allObjectSettings[showingTileObjectId].mapIconColor;
		if ((bool)WorldManager.Instance.allObjects[showingTileObjectId].displayPlayerHouseTiles)
		{
			HouseExterior houseExteriorIfItExists = HouseManager.manage.getHouseExteriorIfItExists(tileX, tileY);
			if (houseExteriorIfItExists != null)
			{
				IconName = houseExteriorIfItExists.houseName;
			}
		}
		else if ((bool)WorldManager.Instance.allObjectSettings[showingTileObjectId].tileObjectLoadInside)
		{
			IconName = "<buildingName>" + WorldManager.Instance.allObjectSettings[showingTileObjectId].tileObjectLoadInside.buildingName;
		}
		else if ((bool)WorldManager.Instance.allObjects[showingTileObjectId].displayPlayerHouseTiles && WorldManager.Instance.allObjects[showingTileObjectId].displayPlayerHouseTiles.isPlayerHouse)
		{
			IconName = Inventory.Instance.playerName + "'s House";
		}
		else if ((bool)WorldManager.Instance.allObjects[showingTileObjectId].displayPlayerHouseTiles && !WorldManager.Instance.allObjects[showingTileObjectId].displayPlayerHouseTiles.isPlayerHouse)
		{
			IconName = WorldManager.Instance.allObjects[showingTileObjectId].displayPlayerHouseTiles.buildingName;
		}
		CurrentIconType = iconType.TileObject;
		NetworkIconId = showingTileObjectId;
	}

	public void SetUpPlayerPlacedMarker(Vector3 pointingPosition, int customIconSpriteIndex)
	{
		if (isNpcMarker)
		{
			Icon.sprite = NPCManager.manage.NPCDetails[customIconSpriteIndex].GetNPCSprite(customIconSpriteIndex);
		}
		else if (customIconSpriteIndex < 0 || customIconSpriteIndex >= RenderMap.Instance.icons.Length)
		{
			Debug.LogWarning("An icon was loaded with a sprite out of the index. That shouldn't happen.");
			customIconSpriteIndex = Mathf.Clamp(customIconSpriteIndex, 0, RenderMap.Instance.icons.Length - 1);
		}
		SetPointingAtPositionAndLocalPointingAtPosition(pointingPosition);
		if (isNpcMarker)
		{
			Icon.sprite = NPCManager.manage.NPCDetails[customIconSpriteIndex].GetNPCSprite(customIconSpriteIndex);
		}
		else
		{
			Icon.sprite = RenderMap.Instance.icons[customIconSpriteIndex];
		}
		CurrentIconType = iconType.PlayerPlaced;
		NetworkIconId = customIconSpriteIndex;
		SetMyMapLevel();
	}

	public void SetUpTelePoint(string dir)
	{
		TelePointName = dir;
		switch (dir)
		{
		case "private":
			SetPosition((int)NetworkMapSharer.Instance.privateTowerPos.x, (int)NetworkMapSharer.Instance.privateTowerPos.y);
			break;
		case "north":
			SetPosition(TownManager.manage.northTowerPos[0], TownManager.manage.northTowerPos[1]);
			break;
		case "east":
			SetPosition(TownManager.manage.eastTowerPos[0], TownManager.manage.eastTowerPos[1]);
			break;
		case "south":
			SetPosition(TownManager.manage.southTowerPos[0], TownManager.manage.southTowerPos[1]);
			break;
		case "west":
			SetPosition(TownManager.manage.westTowerPos[0], TownManager.manage.westTowerPos[1]);
			break;
		}
		Icon.sprite = teleTowerIcon;
		Icon.color = Color.Lerp(Color.yellow, Color.red, 0.35f);
		if (dir == "private")
		{
			IconName = "<buildingName>Tele Pad";
		}
		else
		{
			IconName = "<buildingName>Tele-Tower";
		}
		CurrentIconType = iconType.Teletower;
		SetMyMapLevelToMainIsland();
	}

	public void SetUpQuestIcon(PostOnBoard newPost)
	{
		myPost = newPost;
		QuestTracker.track.updateTasksEvent.AddListener(SetUpTaskIcon);
		SetUpTaskIcon();
		SetMyMapLevel();
	}

	public void SetUpTaskIcon()
	{
		if (BulletinBoard.board.attachedPosts.Contains(myPost) && !myPost.checkIfExpired() && !myPost.completed)
		{
			SetPointingAtPositionAndLocalPointingAtPosition(myPost.getRequiredLocation());
			if (myPost.isPhotoTask)
			{
				CurrentIconType = iconType.CameraQuest;
				if (myPost.readyForNPC)
				{
					IconName = string.Format(ConversationGenerator.generate.GetToolTip("Tip_CompletedQuestIcon"), myPost.getTitleText(myPost.getPostIdOnBoard()));
					Icon.sprite = cameraCompleteIcon;
					NetworkIconId = -2;
				}
				else
				{
					IconName = myPost.getTitleText(myPost.getPostIdOnBoard());
					Icon.sprite = cameraIcon;
					NetworkIconId = -3;
				}
			}
			else if (myPost.isHuntingTask)
			{
				CurrentIconType = iconType.HuntingQuest;
				if (myPost.readyForNPC)
				{
					IconName = string.Format(ConversationGenerator.generate.GetToolTip("Tip_CompletedQuestIcon"), myPost.getTitleText(myPost.getPostIdOnBoard()));
					Icon.sprite = huntingCompleIcon;
					NetworkIconId = -4;
				}
				else
				{
					IconName = myPost.getTitleText(myPost.getPostIdOnBoard());
					Icon.sprite = huntingIcon;
					NetworkIconId = -5;
				}
			}
			else if (myPost.isInvestigation)
			{
				IconName = myPost.getTitleText(myPost.getPostIdOnBoard());
				Icon.sprite = investigationIcon;
				NetworkIconId = -6;
				CurrentIconType = iconType.InvestigationQuest;
			}
		}
		else
		{
			RenderMap.Instance.RemoveTaskIcon(this);
		}
	}

	public bool isConnectedToTask(PostOnBoard newPost)
	{
		if (myPost == newPost)
		{
			return true;
		}
		return false;
	}

	public void OnPressedIcon()
	{
		if (RenderMap.Instance.debugTeleport)
		{
			NetworkMapSharer.Instance.localChar.transform.position = new Vector3(PointingAtPosition.x, (float)WorldManager.Instance.heightMap[(int)PointingAtPosition.x / 2, (int)PointingAtPosition.z / 2] + 2f, PointingAtPosition.z);
			CameraController.control.moveToFollowing();
			NewChunkLoader.loader.forceInstantUpdateAtPos();
			RenderMap.Instance.debugTeleport = false;
		}
		if (!TelePointName.Equals(string.Empty) && Vector3.Distance(NetworkMapSharer.Instance.localChar.transform.position, PointingAtPosition) > 25f && !RenderMap.Instance.selectTeleWindowOpen && RenderMap.Instance.canTele)
		{
			RenderMap.Instance.openTeleSelectWindow(TelePointName);
		}
	}

	private void SetMyMapLevel()
	{
		NetworkmapIconLevelIndex = (int)RealWorldTimeLight.time.CurrentWorldArea;
		NetworkIsVisible = true;
	}

	private void SetMyMapLevelToMainIsland()
	{
		NetworkmapIconLevelIndex = 0;
		if (!RealWorldTimeLight.time.underGround && !RealWorldTimeLight.time.offIsland)
		{
			NetworkIsVisible = true;
		}
		else
		{
			NetworkIsVisible = false;
		}
		container.SetActive(IsVisible);
	}

	private void SetPosition(int tileX, int tileY)
	{
		MyMapPoint.X = tileX;
		MyMapPoint.Y = tileY;
		SetPointingAtPositionAndLocalPointingAtPosition(new Vector3(tileX * 2, 1f, tileY * 2));
	}

	private void Update()
	{
		if ((bool)followTransform)
		{
			localPointingAtPosition = followTransform.position;
		}
		if (isNpcMarker)
		{
			if (base.isServer)
			{
				for (int i = 0; i < NPCManager.manage.npcsOnMap.Count; i++)
				{
					if (NPCManager.manage.npcsOnMap[i].npcId == IconId)
					{
						localPointingAtPosition = Vector3.Lerp(localPointingAtPosition, NPCManager.manage.npcsOnMap[i].currentPosition, Time.deltaTime / 3f);
					}
				}
			}
			IconName = NPCManager.manage.NPCDetails[IconId].GetNPCName();
		}
		if (RenderMap.Instance.mapOpen)
		{
			if (RenderMap.Instance.canTele)
			{
				if (CurrentIconType == iconType.Teletower)
				{
					myRectTransform.localPosition = new Vector3(localPointingAtPosition.x / 2f / RenderMap.Instance.mapScale, localPointingAtPosition.z / 2f / RenderMap.Instance.mapScale, 1f);
					myRectTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
					myRectTransform.localScale = new Vector3(2f / RenderMap.Instance.desiredScale, 2f / RenderMap.Instance.desiredScale, 1f);
				}
				else
				{
					myRectTransform.localPosition = new Vector3(localPointingAtPosition.x / 2f / RenderMap.Instance.mapScale, localPointingAtPosition.z / 2f / RenderMap.Instance.mapScale, 1f);
					myRectTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
					myRectTransform.localScale = new Vector3(2f / RenderMap.Instance.desiredScale / 1.5f, 2f / RenderMap.Instance.desiredScale / 1.5f, 1f);
				}
			}
			else
			{
				myRectTransform.localPosition = new Vector3(localPointingAtPosition.x / 2f / RenderMap.Instance.mapScale, localPointingAtPosition.z / 2f / RenderMap.Instance.mapScale, 1f);
				myRectTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
				myRectTransform.localScale = new Vector3(2f / RenderMap.Instance.desiredScale, 2f / RenderMap.Instance.desiredScale, 1f);
			}
			return;
		}
		if (!OptionsMenu.options.mapFacesNorth)
		{
			myRectTransform.localRotation = Quaternion.Lerp(myRectTransform.localRotation, Quaternion.Euler(0f, 0f, 0f - CameraController.control.transform.eulerAngles.y), Time.deltaTime * 3f);
		}
		else
		{
			myRectTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}
		if (!IconShouldBeHighlighted)
		{
			myRectTransform.localPosition = new Vector3(localPointingAtPosition.x / 2f / RenderMap.Instance.mapScale, localPointingAtPosition.z / 2f / RenderMap.Instance.mapScale, 1f);
			myRectTransform.localScale = new Vector3(4.5f / RenderMap.Instance.desiredScale, 4.5f / RenderMap.Instance.desiredScale, 1f);
		}
		else if (Vector3.Distance(RenderMap.Instance.charToPointTo.position, localPointingAtPosition) < 45f)
		{
			myRectTransform.localPosition = new Vector3(localPointingAtPosition.x / 2f / RenderMap.Instance.mapScale, localPointingAtPosition.z / 2f / RenderMap.Instance.mapScale, 1f);
			myRectTransform.localScale = new Vector3(5.5f / RenderMap.Instance.desiredScale, 5.5f / RenderMap.Instance.desiredScale, 1f);
		}
		else
		{
			Vector3 vector = RenderMap.Instance.charToPointTo.position + (localPointingAtPosition - RenderMap.Instance.charToPointTo.position).normalized * 45f;
			myRectTransform.localPosition = new Vector3(vector.x / 2f / RenderMap.Instance.mapScale, vector.z / 2f / RenderMap.Instance.mapScale, 1f);
			myRectTransform.localScale = new Vector3(5.5f / RenderMap.Instance.desiredScale, 5.5f / RenderMap.Instance.desiredScale, 1f);
		}
	}

	public void SetUpAsSpecialIcon(Vector3 pointingPosition, int specialId)
	{
		NetworkIconId = specialId;
		SetPointingAtPositionAndLocalPointingAtPosition(pointingPosition);
		SetMyMapLevel();
	}

	public void SetPointingAtPositionAndLocalPointingAtPosition(Vector3 newPointingAtPosition)
	{
		NetworkPointingAtPosition = newPointingAtPosition;
		localPointingAtPosition = PointingAtPosition;
	}

	public void SetHighlightValueNetworkChange(bool value)
	{
		if (CurrentIconType == iconType.PlayerPlaced || CurrentIconType == iconType.Vehicle)
		{
			if (!base.isServer)
			{
				NetworkMapSharer.Instance.localChar.CmdSetPlayerPlacedMapIconHighlightValue(base.netId, value);
			}
			else
			{
				NetworkIconShouldBeHighlighted = value;
			}
		}
		else if (!base.isServer)
		{
			NetworkMapSharer.Instance.localChar.CmdToggleHighlightForAutomaticallySetMapIcon(MyMapPoint.X, MyMapPoint.Y);
		}
		else
		{
			NetworkMapSharer.Instance.localChar.ToggleHighlightForAutomaticallySetMapIcon(MyMapPoint.X, MyMapPoint.Y);
		}
	}

	private void OnEnable()
	{
		if (VehicleFollowingId != 0)
		{
			OnVehicleFollowingChanged(VehicleFollowingId, VehicleFollowingId);
		}
	}

	public void RemoveMapMarkerFromMap()
	{
		if (!base.isServer)
		{
			NetworkMapSharer.Instance.localChar.CommandRemovePlayerPlacedMapIcon(base.netId);
		}
		else
		{
			NetworkServer.Destroy(base.gameObject);
		}
	}

	public void ChangeHighlightValue(bool newValue)
	{
		NetworkIconShouldBeHighlighted = newValue;
		ping.SetActive(newValue);
	}

	public void UpdateVisibility()
	{
		NetworkIsVisible = RealWorldTimeLight.time.CurrentWorldArea == (WorldArea)mapIconLevelIndex;
	}

	private void OnHighlightChange(bool oldValue, bool newValue)
	{
		ChangeHighlightValue(newValue);
	}

	private void OnPositionChanged(Vector3 oldValue, Vector3 newValue)
	{
		base.transform.SetParent(RenderMap.Instance.mapParent.transform);
		SetPointingAtPositionAndLocalPointingAtPosition(newValue);
		RenderMap.Instance.PlayerMarkersOnTop();
	}

	private void OnVehicleFollowingChanged(uint oldValue, uint newValue)
	{
		NetworkVehicleFollowingId = newValue;
		if (newValue == 0 || !NetworkIdentity.spawned.ContainsKey(VehicleFollowingId))
		{
			return;
		}
		Vehicle component = NetworkIdentity.spawned[VehicleFollowingId].GetComponent<Vehicle>();
		followTransform = component.transform;
		Icon.sprite = component.mapIconSprite;
		CurrentIconType = iconType.Vehicle;
		if (SaveLoad.saveOrLoad.vehiclePrefabs[component.saveId].GetComponent<Vehicle>().canBePainted)
		{
			Icon.color = EquipWindow.equip.vehicleColoursUI[SaveLoad.saveOrLoad.vehiclePrefabs[component.saveId].GetComponent<Vehicle>().getVariation()];
		}
		IconName = "???";
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].spawnPlaceable && (bool)Inventory.Instance.allItems[i].spawnPlaceable.GetComponent<Vehicle>() && Inventory.Instance.allItems[i].spawnPlaceable.GetComponent<Vehicle>().saveId == component.saveId)
			{
				IconName = Inventory.Instance.allItems[i].getInvItemName();
				break;
			}
		}
		if (component.canBePainted)
		{
			Icon.color = EquipWindow.equip.vehicleColoursUI[component.colourVaration];
		}
		RenderMap.Instance.PlayerMarkersOnTop();
		SetMyMapLevel();
	}

	private IEnumerator DelayPlacementForNewlySpawnedVehicleIcons()
	{
		yield return null;
		yield return null;
		yield return null;
		OnVehicleFollowingChanged(VehicleFollowingId, VehicleFollowingId);
	}

	private void OnSpriteIndexChanged(int oldValue, int newValue)
	{
		NetworkIconId = newValue;
		if (IconId < -1)
		{
			if (IconId == -2)
			{
				CurrentIconType = iconType.CameraQuest;
				Icon.sprite = cameraCompleteIcon;
			}
			else if (IconId == -3)
			{
				CurrentIconType = iconType.CameraQuest;
				Icon.sprite = cameraIcon;
			}
			else if (IconId == -4)
			{
				CurrentIconType = iconType.HuntingQuest;
				Icon.sprite = huntingCompleIcon;
			}
			else if (IconId == -5)
			{
				CurrentIconType = iconType.HuntingQuest;
				Icon.sprite = huntingIcon;
			}
			else if (IconId == -6)
			{
				CurrentIconType = iconType.InvestigationQuest;
				Icon.sprite = investigationIcon;
			}
			else if (IconId == -7)
			{
				CurrentIconType = iconType.Special;
				IconName = "?";
				Icon.sprite = RenderMap.Instance.nickMarker;
			}
		}
		if (CurrentIconType == iconType.PlayerPlaced)
		{
			if (isNpcMarker)
			{
				Icon.sprite = NPCManager.manage.NPCDetails[IconId].GetNPCSprite(IconId);
			}
			else
			{
				Icon.sprite = RenderMap.Instance.icons[IconId];
			}
		}
	}

	private void OnMyMapIconMapLevelChanged(int oldValue, int newValue)
	{
		NetworkmapIconLevelIndex = newValue;
	}

	private void OnIsVisibleChanged(bool oldValue, bool newValue)
	{
		container.SetActive(newValue);
	}

	private void OnDestroy()
	{
		if (!(RenderMap.Instance == null))
		{
			RenderMap.Instance.mapIcons.Remove(this);
			if (base.isServer && NetworkServer.active)
			{
				NetworkMapSharer.Instance.RemoveMapPoint(MyMapPoint);
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(mapIconLevelIndex);
			writer.WriteBool(IconShouldBeHighlighted);
			writer.WriteVector3(PointingAtPosition);
			writer.WriteInt(IconId);
			writer.WriteBool(IsVisible);
			writer.WriteUInt(VehicleFollowingId);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(mapIconLevelIndex);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteBool(IconShouldBeHighlighted);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteVector3(PointingAtPosition);
			result = true;
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteInt(IconId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			writer.WriteBool(IsVisible);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteUInt(VehicleFollowingId);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = mapIconLevelIndex;
			NetworkmapIconLevelIndex = reader.ReadInt();
			if (!SyncVarEqual(num, ref mapIconLevelIndex))
			{
				OnMyMapIconMapLevelChanged(num, mapIconLevelIndex);
			}
			bool iconShouldBeHighlighted = IconShouldBeHighlighted;
			NetworkIconShouldBeHighlighted = reader.ReadBool();
			if (!SyncVarEqual(iconShouldBeHighlighted, ref IconShouldBeHighlighted))
			{
				OnHighlightChange(iconShouldBeHighlighted, IconShouldBeHighlighted);
			}
			Vector3 pointingAtPosition = PointingAtPosition;
			NetworkPointingAtPosition = reader.ReadVector3();
			if (!SyncVarEqual(pointingAtPosition, ref PointingAtPosition))
			{
				OnPositionChanged(pointingAtPosition, PointingAtPosition);
			}
			int iconId = IconId;
			NetworkIconId = reader.ReadInt();
			if (!SyncVarEqual(iconId, ref IconId))
			{
				OnSpriteIndexChanged(iconId, IconId);
			}
			bool isVisible = IsVisible;
			NetworkIsVisible = reader.ReadBool();
			if (!SyncVarEqual(isVisible, ref IsVisible))
			{
				OnIsVisibleChanged(isVisible, IsVisible);
			}
			uint vehicleFollowingId = VehicleFollowingId;
			NetworkVehicleFollowingId = reader.ReadUInt();
			if (!SyncVarEqual(vehicleFollowingId, ref VehicleFollowingId))
			{
				OnVehicleFollowingChanged(vehicleFollowingId, VehicleFollowingId);
			}
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			int num3 = mapIconLevelIndex;
			NetworkmapIconLevelIndex = reader.ReadInt();
			if (!SyncVarEqual(num3, ref mapIconLevelIndex))
			{
				OnMyMapIconMapLevelChanged(num3, mapIconLevelIndex);
			}
		}
		if ((num2 & 2L) != 0L)
		{
			bool iconShouldBeHighlighted2 = IconShouldBeHighlighted;
			NetworkIconShouldBeHighlighted = reader.ReadBool();
			if (!SyncVarEqual(iconShouldBeHighlighted2, ref IconShouldBeHighlighted))
			{
				OnHighlightChange(iconShouldBeHighlighted2, IconShouldBeHighlighted);
			}
		}
		if ((num2 & 4L) != 0L)
		{
			Vector3 pointingAtPosition2 = PointingAtPosition;
			NetworkPointingAtPosition = reader.ReadVector3();
			if (!SyncVarEqual(pointingAtPosition2, ref PointingAtPosition))
			{
				OnPositionChanged(pointingAtPosition2, PointingAtPosition);
			}
		}
		if ((num2 & 8L) != 0L)
		{
			int iconId2 = IconId;
			NetworkIconId = reader.ReadInt();
			if (!SyncVarEqual(iconId2, ref IconId))
			{
				OnSpriteIndexChanged(iconId2, IconId);
			}
		}
		if ((num2 & 0x10L) != 0L)
		{
			bool isVisible2 = IsVisible;
			NetworkIsVisible = reader.ReadBool();
			if (!SyncVarEqual(isVisible2, ref IsVisible))
			{
				OnIsVisibleChanged(isVisible2, IsVisible);
			}
		}
		if ((num2 & 0x20L) != 0L)
		{
			uint vehicleFollowingId2 = VehicleFollowingId;
			NetworkVehicleFollowingId = reader.ReadUInt();
			if (!SyncVarEqual(vehicleFollowingId2, ref VehicleFollowingId))
			{
				OnVehicleFollowingChanged(vehicleFollowingId2, VehicleFollowingId);
			}
		}
	}
}
