using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Events;

public class NetworkMapSharer : NetworkBehaviour
{
	public static NetworkMapSharer Instance;

	public GameObject southCityCutscene;

	public GameObject stickTrapObject;

	public GameObject trapObject;

	public GameObject projectile;

	public GameObject cassowaryEgg;

	public GameObject vehicleBox;

	public GameObject vehicleBoxPreview;

	public GenerateMap mapGenerator;

	public CharMovement localChar;

	public InventoryItem trapInvItem;

	public bool canUseMineControls = true;

	public UnityEvent onChangeMaps = new UnityEvent();

	public UnityEvent returnAgents = new UnityEvent();

	public FadeBlackness fadeToBlack;

	public GameObject farmAnimalChecker;

	[SyncVar]
	public int seed;

	[SyncVar(hook = "onMineSeedChange")]
	public int mineSeed;

	public int tomorrowsMineSeed;

	[SyncVar(hook = "onHairDresserSeatChange")]
	public bool hairDresserSeatOccupied;

	[SyncVar]
	private bool serverUndergroundIsLoaded;

	[SyncVar]
	private bool serverOffIslandIsLoaded;

	[SyncVar(hook = "northCheck")]
	public bool northOn;

	[SyncVar(hook = "eastCheck")]
	public bool eastOn;

	[SyncVar(hook = "southCheck")]
	public bool southOn;

	[SyncVar(hook = "westCheck")]
	public bool westOn;

	[SyncVar(hook = "privateTowerCheck")]
	public Vector2 privateTowerPos;

	[SyncVar]
	public int miningLevel;

	[SyncVar]
	public int loggingLevel;

	[SyncVar(hook = "craftsmanWorkingChange")]
	public bool craftsmanWorking;

	[SyncVar]
	public bool craftsmanHasBerkonium;

	[SyncVar]
	public string islandName = "Dinkum";

	public GameObject multiplayerWindow;

	[SyncVar]
	public bool nextDayIsReady = true;

	[SyncVar]
	public int movingBuilding = -1;

	public Vector3 personalSpawnPoint = Vector3.zero;

	public Transform nonLocalSpawnPos;

	[SyncVar]
	public int townDebt;

	public List<ChunkUpdateDelay> chunkRequested = new List<ChunkUpdateDelay>();

	public readonly SyncList<MapPoint> mapPoints = new SyncList<MapPoint>();

	public readonly SyncList<WeatherData> todaysWeather = new SyncList<WeatherData>();

	public readonly SyncList<WeatherData> tomorrowsWeather = new SyncList<WeatherData>();

	public WishManager wishManager;

	public GameObject teleSignalObject;

	public bool sleeping;

	private GameObject signal;

	private Vector3 todaysSignalPos = Vector3.zero;

	public int Networkseed
	{
		get
		{
			return seed;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref seed))
			{
				int num = seed;
				SetSyncVar(value, ref seed, 1uL);
			}
		}
	}

	public int NetworkmineSeed
	{
		get
		{
			return mineSeed;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref mineSeed))
			{
				int oldSeed = mineSeed;
				SetSyncVar(value, ref mineSeed, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, value: true);
					onMineSeedChange(oldSeed, value);
					setSyncVarHookGuard(2uL, value: false);
				}
			}
		}
	}

	public bool NetworkhairDresserSeatOccupied
	{
		get
		{
			return hairDresserSeatOccupied;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref hairDresserSeatOccupied))
			{
				bool old = hairDresserSeatOccupied;
				SetSyncVar(value, ref hairDresserSeatOccupied, 4uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(4uL))
				{
					setSyncVarHookGuard(4uL, value: true);
					onHairDresserSeatChange(old, value);
					setSyncVarHookGuard(4uL, value: false);
				}
			}
		}
	}

	public bool NetworkserverUndergroundIsLoaded
	{
		get
		{
			return serverUndergroundIsLoaded;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref serverUndergroundIsLoaded))
			{
				bool flag = serverUndergroundIsLoaded;
				SetSyncVar(value, ref serverUndergroundIsLoaded, 8uL);
			}
		}
	}

	public bool NetworkserverOffIslandIsLoaded
	{
		get
		{
			return serverOffIslandIsLoaded;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref serverOffIslandIsLoaded))
			{
				bool flag = serverOffIslandIsLoaded;
				SetSyncVar(value, ref serverOffIslandIsLoaded, 16uL);
			}
		}
	}

	public bool NetworknorthOn
	{
		get
		{
			return northOn;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref northOn))
			{
				bool old = northOn;
				SetSyncVar(value, ref northOn, 32uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(32uL))
				{
					setSyncVarHookGuard(32uL, value: true);
					northCheck(old, value);
					setSyncVarHookGuard(32uL, value: false);
				}
			}
		}
	}

	public bool NetworkeastOn
	{
		get
		{
			return eastOn;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref eastOn))
			{
				bool old = eastOn;
				SetSyncVar(value, ref eastOn, 64uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(64uL))
				{
					setSyncVarHookGuard(64uL, value: true);
					eastCheck(old, value);
					setSyncVarHookGuard(64uL, value: false);
				}
			}
		}
	}

	public bool NetworksouthOn
	{
		get
		{
			return southOn;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref southOn))
			{
				bool old = southOn;
				SetSyncVar(value, ref southOn, 128uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(128uL))
				{
					setSyncVarHookGuard(128uL, value: true);
					southCheck(old, value);
					setSyncVarHookGuard(128uL, value: false);
				}
			}
		}
	}

	public bool NetworkwestOn
	{
		get
		{
			return westOn;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref westOn))
			{
				bool old = westOn;
				SetSyncVar(value, ref westOn, 256uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(256uL))
				{
					setSyncVarHookGuard(256uL, value: true);
					westCheck(old, value);
					setSyncVarHookGuard(256uL, value: false);
				}
			}
		}
	}

	public Vector2 NetworkprivateTowerPos
	{
		get
		{
			return privateTowerPos;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref privateTowerPos))
			{
				Vector2 old = privateTowerPos;
				SetSyncVar(value, ref privateTowerPos, 512uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(512uL))
				{
					setSyncVarHookGuard(512uL, value: true);
					privateTowerCheck(old, value);
					setSyncVarHookGuard(512uL, value: false);
				}
			}
		}
	}

	public int NetworkminingLevel
	{
		get
		{
			return miningLevel;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref miningLevel))
			{
				int num = miningLevel;
				SetSyncVar(value, ref miningLevel, 1024uL);
			}
		}
	}

	public int NetworkloggingLevel
	{
		get
		{
			return loggingLevel;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref loggingLevel))
			{
				int num = loggingLevel;
				SetSyncVar(value, ref loggingLevel, 2048uL);
			}
		}
	}

	public bool NetworkcraftsmanWorking
	{
		get
		{
			return craftsmanWorking;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref craftsmanWorking))
			{
				bool old = craftsmanWorking;
				SetSyncVar(value, ref craftsmanWorking, 4096uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(4096uL))
				{
					setSyncVarHookGuard(4096uL, value: true);
					craftsmanWorkingChange(old, value);
					setSyncVarHookGuard(4096uL, value: false);
				}
			}
		}
	}

	public bool NetworkcraftsmanHasBerkonium
	{
		get
		{
			return craftsmanHasBerkonium;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref craftsmanHasBerkonium))
			{
				bool flag = craftsmanHasBerkonium;
				SetSyncVar(value, ref craftsmanHasBerkonium, 8192uL);
			}
		}
	}

	public string NetworkislandName
	{
		get
		{
			return islandName;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref islandName))
			{
				string text = islandName;
				SetSyncVar(value, ref islandName, 16384uL);
			}
		}
	}

	public bool NetworknextDayIsReady
	{
		get
		{
			return nextDayIsReady;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref nextDayIsReady))
			{
				bool flag = nextDayIsReady;
				SetSyncVar(value, ref nextDayIsReady, 32768uL);
			}
		}
	}

	public int NetworkmovingBuilding
	{
		get
		{
			return movingBuilding;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref movingBuilding))
			{
				int num = movingBuilding;
				SetSyncVar(value, ref movingBuilding, 65536uL);
			}
		}
	}

	public int NetworktownDebt
	{
		get
		{
			return townDebt;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref townDebt))
			{
				int num = townDebt;
				SetSyncVar(value, ref townDebt, 131072uL);
			}
		}
	}

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		mapPoints.Callback += RenderMap.Instance.ScanAndUpdateScanAMapIconHighlights;
	}

	public override void OnStartServer()
	{
		Networkseed = GenerateMap.generate.seed;
		GenerateUndergroundMap.generate.setUpMineSeedFirstTime();
		SaveLoad.saveOrLoad.loadVehicles();
		if (!TownManager.manage.firstConnect && TownManager.manage.allShopFloors[13] != null)
		{
			NPCBuildingDoors obj = TownManager.manage.allShopFloors[13];
			obj.removeSelfFromNavMesh();
			Object.Destroy(obj.gameObject);
		}
		GenerateMap.generate.placeAllBuildings();
		SaveLoad.saveOrLoad.loadBulletinBoard();
		checkTeleportsOn();
		syncLicenceLevels();
		Invoke("nameIslandDelay", 0.5f);
		NPCManager.manage.resetNPCRequestsForSave();
		AnimalManager.manage.loadEggsIntoNests();
		SaveLoad.saveOrLoad.loadDrops();
		SaveLoad.saveOrLoad.loadCarriables();
		if (privateTowerPos != Vector2.zero)
		{
			privateTowerCheck(privateTowerPos, privateTowerPos);
		}
		SaveLoad.saveOrLoad.loadMapIcons();
		SaveLoad.saveOrLoad.LoadWeather();
	}

	private void nameIslandDelay()
	{
		NetworkislandName = Inventory.Instance.islandName;
	}

	public void onMineSeedChange(int oldSeed, int newMineSeed)
	{
		NetworkmineSeed = newMineSeed;
	}

	public void syncLicenceLevels()
	{
		NetworkminingLevel = LicenceManager.manage.allLicences[1].getCurrentLevel();
		NetworkloggingLevel = LicenceManager.manage.allLicences[2].getCurrentLevel();
	}

	public override void OnStartClient()
	{
		StartCoroutine(onClientConnect());
		NPCManager.manage.resetNPCRequestsForSave();
		if (!base.isServer)
		{
			BulletinBoard.board.onLocalConnect();
		}
		multiplayerWindow.SetActive(value: false);
		Object.Destroy(southCityCutscene);
		todaysWeather.Callback += WeatherManager.Instance.ChangeWeather;
		tomorrowsWeather.Callback += WeatherManager.Instance.ChangeWeather;
		farmAnimalChecker.SetActive(value: true);
		SaveLoad.saveOrLoad.newFileSaver.LoadStash();
	}

	public bool serverActive()
	{
		return NetworkServer.active;
	}

	private IEnumerator onClientConnect()
	{
		NewChunkLoader.loader.staggerChunkDistanceOnConnect();
		if (!base.isServer)
		{
			if (RealWorldTimeLight.time.offIsland)
			{
				yield return StartCoroutine(GenerateVisitingIsland.Instance.GenerateOffIslandForClient(mineSeed));
			}
			else if (RealWorldTimeLight.time.underGround)
			{
				yield return StartCoroutine(GenerateUndergroundMap.generate.generateMineForClient(mineSeed));
			}
			else
			{
				NewChunkLoader.loader.inside = true;
				yield return StartCoroutine(mapGenerator.generateNewMap(seed));
				MuseumManager.manage.clearForClient();
				NewChunkLoader.loader.inside = false;
				onMineSeedChange(mineSeed, mineSeed);
			}
			WorldManager.Instance.refreshAllChunksForConnect();
			TownManager.manage.journalUnlocked = true;
			TownManager.manage.mapUnlocked = true;
			SaveLoad.saveOrLoad.loadPhotos(isClient: true);
		}
		else
		{
			WorldManager.Instance.refreshAllChunksForConnect();
			onMineSeedChange(mineSeed, mineSeed);
		}
		RealWorldTimeLight.time.getDaySkyBox();
		while (!localChar)
		{
			yield return null;
		}
		if (!base.isServer)
		{
			NPCManager.manage.requestNPCInvs();
			if (!CameraController.control.isFreeCamOn())
			{
				CameraController.control.swapFreeCam();
			}
		}
		WeatherManager.Instance.ChangeWeather();
		SaveLoad.saveOrLoad.loadMail();
		DailyTaskGenerator.generate.generateNewDailyTasks();
		DailyTaskGenerator.generate.startDistanceChecker();
		craftsmanWorkingChange(craftsmanWorking, craftsmanWorking);
		while (TownManager.manage.firstConnect)
		{
			yield return true;
		}
		float timer = 0f;
		RaycastHit hitInfo;
		while (!Physics.Raycast(localChar.transform.position + Vector3.up * 12f, Vector3.down, out hitInfo, 17f, localChar.jumpLayers))
		{
			if (localChar.transform.position.y < (float)WorldManager.Instance.heightMap[Mathf.RoundToInt(localChar.transform.position.x / 2f), Mathf.RoundToInt(localChar.transform.position.z / 2f)] || localChar.transform.position.y > (float)WorldManager.Instance.heightMap[Mathf.RoundToInt(localChar.transform.position.x / 2f), Mathf.RoundToInt(localChar.transform.position.z / 2f)] + 3f)
			{
				timer += Time.deltaTime;
				if (base.isServer && timer > 10f)
				{
					if (localChar.myInteract.IsInsidePlayerHouse)
					{
						localChar.myInteract.ChangeInsideOut(isEntry: false);
						WeatherManager.Instance.ChangeToOutsideEnvironment();
						RealWorldTimeLight.time.goOutside();
					}
					localChar.transform.position = new Vector3(localChar.transform.position.x, (float)WorldManager.Instance.heightMap[Mathf.RoundToInt(localChar.transform.position.x / 2f), Mathf.RoundToInt(localChar.transform.position.z / 2f)] + 2f, localChar.transform.position.z);
				}
				else if (timer > 15f)
				{
					if (localChar.myInteract.IsInsidePlayerHouse)
					{
						localChar.myInteract.ChangeInsideOut(isEntry: false);
						WeatherManager.Instance.ChangeToOutsideEnvironment();
						RealWorldTimeLight.time.goOutside();
					}
					localChar.transform.position = new Vector3(localChar.transform.position.x, (float)WorldManager.Instance.heightMap[Mathf.RoundToInt(localChar.transform.position.x / 2f), Mathf.RoundToInt(localChar.transform.position.z / 2f)] + 2f, localChar.transform.position.z);
				}
			}
			yield return null;
		}
		localChar.unlockClientOnLoad();
		if (base.isServer && (bool)nonLocalSpawnPos)
		{
			WorldManager.Instance.spawnPos.position = nonLocalSpawnPos.position;
		}
	}

	public void setNonLocalSpawnPos(Transform newPos)
	{
		nonLocalSpawnPos = newPos;
		if (base.isServer)
		{
			personalSpawnPoint = nonLocalSpawnPos.position;
		}
	}

	public void spawnGameObject(GameObject spawnMe)
	{
		NetworkServer.Spawn(spawnMe);
	}

	public void unSpawnGameObject(GameObject despawn)
	{
		NetworkServer.UnSpawn(despawn);
	}

	public void callRequest(NetworkConnection con, int chunkX, int chunkY)
	{
		if (!base.isServer)
		{
			return;
		}
		if (WorldManager.Instance.chunkChangedMap[chunkX / 10, chunkY / 10])
		{
			bool waitForOnTile = false;
			bool waitForType = false;
			bool waitForHeight = false;
			bool waitForWater = false;
			if (WorldManager.Instance.changedMapTileType[chunkX / 10, chunkY / 10])
			{
				int[] chunkDetails = WorldManager.Instance.getChunkDetails(chunkX, chunkY, WorldManager.MapType.TileTypeMap);
				TargetGiveChunkTileTypeDetails(con, chunkX, chunkY, chunkDetails);
				waitForType = true;
			}
			if (WorldManager.Instance.changedMapOnTile[chunkX / 10, chunkY / 10])
			{
				int[] chunkDetails2 = WorldManager.Instance.getChunkDetails(chunkX, chunkY, WorldManager.MapType.OnTileMap);
				int[] chunkStatusDetails = WorldManager.Instance.getChunkStatusDetails(chunkX, chunkY);
				TargetGiveChunkOnTileDetails(con, chunkX, chunkY, chunkDetails2, chunkStatusDetails);
				waitForOnTile = true;
				if (SignManager.manage.areThereSignsInThisChunk(chunkX, chunkY))
				{
					TargetGiveSignDetailsForChunk(con, SignManager.manage.collectSignsInChunk(chunkX, chunkY));
				}
			}
			if (WorldManager.Instance.changedMapHeight[chunkX / 10, chunkY / 10])
			{
				int[] chunkDetails3 = WorldManager.Instance.getChunkDetails(chunkX, chunkY, WorldManager.MapType.HeightMap);
				TargetGiveChunkHeightDetails(con, chunkX, chunkY, chunkDetails3);
				waitForHeight = true;
			}
			if (WorldManager.Instance.changedMapWater[chunkX / 10, chunkY / 10])
			{
				bool[] waterChunkDetails = WorldManager.Instance.getWaterChunkDetails(chunkX, chunkY);
				TargetGiveChunkWaterDetails(con, chunkX, chunkY, waterChunkDetails);
				waitForWater = true;
			}
			TargetRefreshChunkAfterSent(con, chunkX, chunkY, waitForOnTile, waitForType, waitForHeight, waitForWater);
		}
		else
		{
			TargetRefreshNotNeeded(con, chunkX, chunkY);
		}
	}

	public void spawnAServerDrop(int itemId, int stackAmount, Vector3 position, HouseDetails inside = null, bool tryNotToStack = false, int xPType = -1)
	{
		if (!base.isServer)
		{
			return;
		}
		GameObject gameObject = WorldManager.Instance.dropAnItem(itemId, stackAmount, position, inside, tryNotToStack);
		if (gameObject != null)
		{
			if (inside == null && tryNotToStack)
			{
				float x = position.x + (float)Random.Range(-1, 1);
				float z = position.z + (float)Random.Range(-1, 1);
				int num = (int)position.y;
				Vector3 vector = WorldManager.Instance.moveDropPosToSafeOutside(new Vector3(x, num, z));
				gameObject.GetComponent<DroppedItem>().setDesiredPos(vector.y, vector.x, vector.z);
			}
			if (xPType != -1)
			{
				gameObject.GetComponent<DroppedItem>().NetworkendOfDayTallyType = xPType;
			}
			NetworkServer.Spawn(gameObject);
		}
	}

	public void spawnAServerDropToSave(int itemId, int stackAmount, Vector3 position, HouseDetails inside = null, bool tryNotToStack = false, int xPType = -1)
	{
		if (!base.isServer)
		{
			return;
		}
		GameObject gameObject = WorldManager.Instance.dropAnItem(itemId, stackAmount, position, inside, tryNotToStack);
		if (gameObject != null)
		{
			gameObject.GetComponent<DroppedItem>().saveDrop = true;
			if (inside == null && tryNotToStack)
			{
				float x = position.x + (float)Random.Range(-1, 1);
				float z = position.z + (float)Random.Range(-1, 1);
				int num = (int)position.y;
				Vector3 vector = WorldManager.Instance.moveDropPosToSafeOutside(new Vector3(x, num, z));
				gameObject.GetComponent<DroppedItem>().setDesiredPos(vector.y, vector.x, vector.z);
			}
			if (xPType != -1)
			{
				gameObject.GetComponent<DroppedItem>().NetworkendOfDayTallyType = xPType;
			}
			NetworkServer.Spawn(gameObject);
		}
	}

	public void spawnAServerDrop(int itemId, int stackAmount, Vector3 position, Vector3 desiredPos, HouseDetails inside = null, bool tryNotToStack = false, int xPType = -1)
	{
		if (!base.isServer || (!tryNotToStack && WorldManager.Instance.tryAndStackItem(itemId, stackAmount, Mathf.RoundToInt(desiredPos.x / 2f), Mathf.RoundToInt(desiredPos.z / 2f), inside)))
		{
			return;
		}
		GameObject gameObject = WorldManager.Instance.dropAnItem(itemId, stackAmount, position, inside, tryNotToStack);
		if (gameObject != null)
		{
			if (inside == null)
			{
				Vector3 vector = WorldManager.Instance.moveDropPosToSafeOutside(desiredPos);
				gameObject.GetComponent<DroppedItem>().setDesiredPos(vector.y, vector.x, vector.z);
			}
			if (xPType != -1)
			{
				gameObject.GetComponent<DroppedItem>().NetworkendOfDayTallyType = xPType;
			}
			NetworkServer.Spawn(gameObject);
		}
	}

	public void CharDropsAServerDrop(int itemId, int stackAmount, Vector3 position, Vector3 desiredPos, HouseDetails inside = null, bool tryNotToStack = false, int xPType = -1)
	{
		if (!base.isServer || (!tryNotToStack && WorldManager.Instance.tryAndStackItem(itemId, stackAmount, Mathf.RoundToInt(desiredPos.x / 2f), Mathf.RoundToInt(desiredPos.z / 2f), inside)))
		{
			return;
		}
		GameObject gameObject = WorldManager.Instance.dropAnItem(itemId, stackAmount, position, inside, tryNotToStack);
		if (gameObject != null)
		{
			if (inside == null)
			{
				Vector3 vector = WorldManager.Instance.moveDropPosToSafeOutside(desiredPos);
				gameObject.GetComponent<DroppedItem>().setDesiredPos(vector.y, vector.x, vector.z);
			}
			if (xPType != -1)
			{
				gameObject.GetComponent<DroppedItem>().NetworkendOfDayTallyType = xPType;
			}
			NetworkServer.Spawn(gameObject);
		}
	}

	public PickUpAndCarry spawnACarryable(GameObject go, Vector3 pos, bool moveToGroundLevel = true)
	{
		if (base.isServer)
		{
			if (moveToGroundLevel)
			{
				pos.y = WorldManager.Instance.heightMap[(int)pos.x / 2, (int)pos.z / 2];
			}
			GameObject obj = Object.Instantiate(go, pos, Quaternion.identity);
			obj.GetComponent<PickUpAndCarry>().dropToPosY = pos.y;
			NetworkServer.Spawn(obj);
			return obj.GetComponent<PickUpAndCarry>();
		}
		return null;
	}

	public void DestroyCarryable(PickUpAndCarry carry)
	{
		carry.RemoveAuthorityBeforeBeforeServerDestroy();
		NetworkServer.Destroy(carry.gameObject);
	}

	[ClientRpc]
	public void RpcPlayCarryDeathPart(int carryId, Vector3 position)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(carryId);
		writer.WriteVector3(position);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcPlayCarryDeathPart", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public void startTileTimerOnServer(int itemId, int xPos, int yPos, HouseDetails inside = null)
	{
		if (base.isServer)
		{
			WorldManager.Instance.startCountDownForTile(itemId, xPos, yPos, inside);
		}
	}

	public void overideOldFloor(int xPos, int yPos)
	{
		if (TownManager.manage.allShopFloors[(int)WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].GetComponent<LoadBuildingInsides>().shopFloor.GetComponent<NPCBuildingDoors>().myLocation] != null)
		{
			TownManager.manage.removeBuildingAlreadyRequestedOnUpgrade(xPos, yPos);
			TownManager.manage.allShopFloors[(int)WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].GetComponent<LoadBuildingInsides>().shopFloor.GetComponent<NPCBuildingDoors>().myLocation].removeSelfFromNavMesh();
		}
	}

	public void requestInterior(int xPos, int yPos)
	{
		if (TownManager.manage.checkIfBuildingInteriorHasBeenRequested(xPos, yPos))
		{
			return;
		}
		TownManager.manage.addBuildingAlreadyRequested(xPos, yPos);
		TileObject tileObjectForShopInterior = WorldManager.Instance.getTileObjectForShopInterior(WorldManager.Instance.onTileMap[xPos, yPos], new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
		LoadBuildingInsides tileObjectLoadInside = WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectLoadInside;
		if ((bool)WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectLoadInside && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectLoadInside.isMoveable)
		{
			BuildingManager.manage.addBuildingToMoveList(xPos, yPos);
		}
		else if ((bool)tileObjectForShopInterior.displayPlayerHouseTiles)
		{
			BuildingManager.manage.addBuildingToMoveList(xPos, yPos);
		}
		if ((bool)tileObjectLoadInside)
		{
			tileObjectLoadInside.serverSpawnsInteriorAndKeeper(tileObjectForShopInterior.loadInsidePos, xPos, yPos);
			if (tileObjectLoadInside.isMarketPlace)
			{
				MarketPlaceManager.manage.marketPos = new int[2] { xPos, yPos };
			}
		}
		if ((bool)tileObjectForShopInterior.displayPlayerHouseTiles)
		{
			tileObjectForShopInterior.setRotatiomNumber(WorldManager.Instance.rotationMap[xPos, yPos]);
		}
		Object.Destroy(tileObjectForShopInterior.gameObject);
	}

	public void MarkTreasureOnMapAndSpawn()
	{
		int num = 0;
		int num2 = 0;
		bool flag = false;
		int num3 = 0;
		while (!flag)
		{
			num = Random.Range(0, WorldManager.Instance.GetMapSize());
			num2 = Random.Range(0, WorldManager.Instance.GetMapSize());
			if (WorldManager.Instance.heightMap[num, num2] >= 0 && WorldManager.Instance.onTileMap[num, num2] == -1)
			{
				flag = true;
			}
			if (num3 > 1000000)
			{
				flag = true;
				Debug.LogError("Couldn't find a treasure location after 1,000,000 tries");
			}
			num3++;
		}
		RpcUpdateOnTileObject(30, num, num2);
		Vector2 vector = new Vector2(num * 2, num2 * 2);
		NetworkServer.Spawn(RenderMap.Instance.CreateNewNetworkedPlayerSetMarker(vector / 8f, 8).gameObject);
	}

	public void MarkNPCOnMap(int npcId)
	{
		for (int i = 0; i < NPCManager.manage.npcsOnMap.Count; i++)
		{
			if (NPCManager.manage.npcsOnMap[i].npcId == npcId)
			{
				NetworkServer.Spawn(RenderMap.Instance.CreateNewNetworkedNpcMarker(new Vector2(NPCManager.manage.npcsOnMap[i].currentPosition.x, NPCManager.manage.npcsOnMap[i].currentPosition.z) / 8f, npcId).gameObject);
			}
		}
	}

	public void tryAndMoveUnderGround()
	{
		if (base.isServer && canUseMineControls && MineEnterExit.mineEntrance.checkIfAllPlayersAreInElevator() && Inventory.Instance.getAmountOfItemInAllSlots(Inventory.Instance.getInvItemId(Inventory.Instance.minePass)) > 0)
		{
			Inventory.Instance.removeAmountOfItem(Inventory.Instance.getInvItemId(Inventory.Instance.minePass), 1);
			canUseMineControls = false;
			RpcMoveUnderGround();
		}
	}

	public void tryAndMoveAboveGround()
	{
		if (base.isServer && canUseMineControls && MineEnterExit.mineExit.checkIfAllPlayersAreInElevator())
		{
			canUseMineControls = false;
			RpcMoveAboveGround();
		}
	}

	public IEnumerator moveAboveGroundOnSinglePlayerDeath()
	{
		canUseMineControls = false;
		localChar.GetComponent<Rigidbody>().isKinematic = true;
		CameraController.control.blackFadeAnim.fadeIn();
		yield return StartCoroutine(moveUpMines(needsMineEntry: false));
		WeatherManager.Instance.ChangeWeather();
		CameraController.control.blackFadeAnim.fadeOut();
		localChar.GetComponent<Rigidbody>().isKinematic = false;
	}

	public void fireProjectile(int projectileId, Transform firedBy, Vector3 startPos, Vector3 dir)
	{
		GameObject obj = Object.Instantiate(projectile, startPos, Quaternion.identity);
		obj.GetComponent<Projectile>().SetUpProjectile(projectileId, firedBy, dir, 4f);
		obj.GetComponent<Projectile>().damageFriendly = false;
	}

	public void fireProjectile(int projectileId, Transform firedBy, Vector3 dir)
	{
		EquipItemToChar component = firedBy.GetComponent<EquipItemToChar>();
		if ((bool)component)
		{
			Object.Instantiate(projectile, component.holdPos.position + component.holdPos.forward, component.holdPos.rotation).GetComponent<Projectile>().SetUpProjectile(projectileId, firedBy, dir);
			return;
		}
		Transform transform = firedBy.Find("FireFrom");
		Object.Instantiate(projectile, transform.position, transform.rotation).GetComponent<Projectile>().SetUpProjectile(projectileId, firedBy, dir);
	}

	public void changeTileHeight(int newTileType, int xPos, int yPos, NetworkConnection con = null)
	{
		WorldManager.Instance.heightChunkHasChanged(xPos, yPos);
		List<DroppedItem> allDropsOnTile = WorldManager.Instance.getAllDropsOnTile(xPos, yPos);
		if (newTileType < 0 && WorldManager.Instance.onTileMap[xPos, yPos] == 30)
		{
			BuriedItem buriedItem = BuriedManager.manage.checkIfBuriedItem(xPos, yPos);
			if (buriedItem != null)
			{
				buriedItem.digUpItem();
			}
			else if (BuriedManager.manage.checkIfBuriedItem(xPos, yPos) == null)
			{
				BuriedItem buriedItem2 = BuriedManager.manage.createARandomItemWhenNotFound(xPos, yPos, con);
				if (buriedItem2 != null)
				{
					Instance.TargetGiveDigUpTreasureMilestone(con, buriedItem2.itemId);
					buriedItem2.digUpItem();
				}
			}
			RpcUpdateOnTileObject(-1, xPos, yPos);
		}
		else if (newTileType > 0 && allDropsOnTile.Count != 0)
		{
			if (allDropsOnTile[0].stackAmount == 1)
			{
				if (Inventory.Instance.allItems[allDropsOnTile[0].myItemId].burriedPlaceable)
				{
					RpcUpdateOnTileObject(Inventory.Instance.allItems[allDropsOnTile[0].myItemId].placeable.tileObjectId, xPos, yPos);
					if (con != null && Inventory.Instance.allItems[allDropsOnTile[0].myItemId].assosiatedTask != 0)
					{
						TargetGiveBuryItemMilestone(con, allDropsOnTile[0].myItemId);
					}
				}
				else
				{
					BuriedManager.manage.buryNewItem(allDropsOnTile[0].myItemId, allDropsOnTile[0].stackAmount, xPos, yPos);
					Instance.RpcUpdateOnTileObject(30, xPos, yPos);
				}
				allDropsOnTile[0].bury();
			}
		}
		else
		{
			_ = 0;
		}
		if ((bool)WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].dropOnChange || WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].changeTileKeepUnderTile)
		{
			RpcUpdateTileType(WorldManager.Instance.tileTypeStatusMap[xPos, yPos], xPos, yPos);
			return;
		}
		if (WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].changeToUnderTileAndChangeHeight)
		{
			RpcUpdateTileType(WorldManager.Instance.tileTypeStatusMap[xPos, yPos], xPos, yPos);
		}
		RpcUpdateTileHeight(newTileType, xPos, yPos);
	}

	[ClientRpc]
	public void RpcWaterExplodeOnLava(int[] xPositions, int[] yPositions)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, xPositions);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, yPositions);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcWaterExplodeOnLava", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcPlayTrapperSound(Vector3 trapperWhistlePos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(trapperWhistlePos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcPlayTrapperSound", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcFeedFishSound(Vector3 fishPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(fishPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcFeedFishSound", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcMoveOffIsland()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(NetworkMapSharer), "RpcMoveOffIsland", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcMoveUnderGround()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(NetworkMapSharer), "RpcMoveUnderGround", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcMoveAboveGround()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(NetworkMapSharer), "RpcMoveAboveGround", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcReturnHomeFromOffIsland()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(NetworkMapSharer), "RpcReturnHomeFromOffIsland", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcCharEmotes(int no, uint netId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(no);
		writer.WriteUInt(netId);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcCharEmotes", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcBreakToolReact(uint netId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(netId);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcBreakToolReact", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcMakeChatBubble(string message, uint netId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteString(message);
		writer.WriteUInt(netId);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcMakeChatBubble", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcSpawnATileObjectDrop(int tileObjectToSpawnFrom, int xPos, int yPos, int tileStatus)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(tileObjectToSpawnFrom);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(tileStatus);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcSpawnATileObjectDrop", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcDepositItemIntoChanger(int itemDeposit, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemDeposit);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcDepositItemIntoChanger", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcMoveHouseExterior(int xPos, int yPos, int newXpos, int newYPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(newXpos);
		writer.WriteInt(newYPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcMoveHouseExterior", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcMoveHouseInterior(int xPos, int yPos, int newXpos, int newYPos, int oldRotation, int newRotation)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(newXpos);
		writer.WriteInt(newYPos);
		writer.WriteInt(oldRotation);
		writer.WriteInt(newRotation);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcMoveHouseInterior", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcDepositItemIntoChangerInside(int itemDeposit, int xPos, int yPos, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemDeposit);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcDepositItemIntoChangerInside", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcUpdateHouseWall(int itemId, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcUpdateHouseWall", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcUpdateHouseExterior(HouseExterior exterior)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		GeneratedNetworkCode._Write_HouseExterior(writer, exterior);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcUpdateHouseExterior", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcAddToMuseum(int newItem, string donatedBy)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newItem);
		writer.WriteString(donatedBy);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcAddToMuseum", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcUpdateHouseFloor(int itemId, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcUpdateHouseFloor", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcPlaceOnTop(int newTileId, int xPos, int yPos, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newTileId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcPlaceOnTop", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcSitDown(int newSitPosition, int xPos, int yPos, int houseXPos, int houseYPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newSitPosition);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseXPos);
		writer.WriteInt(houseYPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcSitDown", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcGetUp(int sitPosition, int xPos, int yPos, int houseXPos, int houseYPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(sitPosition);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseXPos);
		writer.WriteInt(houseYPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcGetUp", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcEjectItemFromChanger(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcEjectItemFromChanger", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcEjectItemFromChangerInside(int xPos, int yPos, int houseXPos, int houseYPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseXPos);
		writer.WriteInt(houseYPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcEjectItemFromChangerInside", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcOpenCloseTile(int xPos, int yPos, int newOpenClose)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(newOpenClose);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcOpenCloseTile", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcNPCOpenGate(int xPos, int yPos, uint npcNetId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteUInt(npcNetId);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcNPCOpenGate", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public IEnumerator fakeOpenGate(int xPos, int yPos, TileObject gateObject, Transform npcTrans)
	{
		gateObject.tileOnOff.fakeOpen();
		while (Vector3.Distance(npcTrans.position, gateObject.transform.position) < 2.5f && npcTrans.gameObject.activeInHierarchy)
		{
			yield return null;
		}
		if (WorldManager.Instance.onTileStatusMap[xPos, yPos] == 0)
		{
			gateObject.tileOnOff.fakeClose();
		}
	}

	[ClientRpc]
	public void RpcHarvestObject(int newStatus, int xPos, int yPos, bool spawnDrop)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newStatus);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteBool(spawnDrop);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcHarvestObject", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcDigUpBuriedItemNoise(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcDigUpBuriedItemNoise", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcThunderSound()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(NetworkMapSharer), "RpcThunderSound", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcThunderStrike(Vector2 thunderPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector2(thunderPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcThunderStrike", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcActivateTrap(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcActivateTrap", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcClearOnTileObjectNoDrop(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcClearOnTileObjectNoDrop", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpChangeOnTileObjectNoDrop(int newId, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpChangeOnTileObjectNoDrop", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcUpdateOnTileObjectForDesync(int currentTileObject, int currentTileStatus, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(currentTileObject);
		writer.WriteInt(currentTileStatus);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcUpdateOnTileObjectForDesync", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcUpdateOnTileObject(int newTileType, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newTileType);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcUpdateOnTileObject", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcUpdateTileHeight(int tileHeightDif, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(tileHeightDif);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcUpdateTileHeight", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public void CheckLocalCharPreventFallThroughWorld(int xPos, int yPos)
	{
		if ((bool)localChar && WorldManager.Instance.isPositionOnMap(localChar.transform.position))
		{
			int num = Mathf.RoundToInt(localChar.transform.position.x / 2f);
			int num2 = Mathf.RoundToInt(localChar.transform.position.z / 2f);
			if (num == xPos && num2 == yPos && localChar.transform.position.y < (float)WorldManager.Instance.heightMap[num, num2] && localChar.transform.position.y >= (float)WorldManager.Instance.heightMap[num, num2] - 2f)
			{
				StartCoroutine(DelayLocalCharFall(xPos, yPos));
			}
		}
	}

	private IEnumerator DelayLocalCharFall(int xPos, int yPos)
	{
		int num = Mathf.RoundToInt(localChar.transform.position.x / 2f);
		int num2 = Mathf.RoundToInt(localChar.transform.position.z / 2f);
		if (num == xPos && num2 == yPos && localChar.transform.position.y < (float)WorldManager.Instance.heightMap[num, num2] && localChar.transform.position.y >= (float)WorldManager.Instance.heightMap[num, num2] - 3f)
		{
			Vector3 position = localChar.transform.position;
			position.y = (float)WorldManager.Instance.heightMap[num, num2] + 0.01f;
			localChar.transform.position = position;
		}
		yield return null;
		num = Mathf.RoundToInt(localChar.transform.position.x / 2f);
		num2 = Mathf.RoundToInt(localChar.transform.position.z / 2f);
		if (num == xPos && num2 == yPos && localChar.transform.position.y < (float)WorldManager.Instance.heightMap[num, num2] && localChar.transform.position.y >= (float)WorldManager.Instance.heightMap[num, num2] - 3f)
		{
			Vector3 position2 = localChar.transform.position;
			position2.y = (float)WorldManager.Instance.heightMap[num, num2] + 0.01f;
			localChar.transform.position = position2;
		}
		yield return null;
		num = Mathf.RoundToInt(localChar.transform.position.x / 2f);
		num2 = Mathf.RoundToInt(localChar.transform.position.z / 2f);
		if (num == xPos && num2 == yPos && localChar.transform.position.y < (float)WorldManager.Instance.heightMap[num, num2] && localChar.transform.position.y >= (float)WorldManager.Instance.heightMap[num, num2] - 3f)
		{
			Vector3 position3 = localChar.transform.position;
			position3.y = (float)WorldManager.Instance.heightMap[num, num2] + 0.01f;
			localChar.transform.position = position3;
		}
	}

	[ClientRpc]
	public void RpcUpdateTileType(int newType, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newType);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcUpdateTileType", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public int GetMaxHeightForThisTile(int xPos, int yPos)
	{
		return 0;
	}

	[TargetRpc]
	public void TargetRefreshChunkAfterSent(NetworkConnection con, int chunkX, int chunkY, bool waitForOnTile, bool waitForType, bool waitForHeight, bool waitForWater)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(chunkX);
		writer.WriteInt(chunkY);
		writer.WriteBool(waitForOnTile);
		writer.WriteBool(waitForType);
		writer.WriteBool(waitForHeight);
		writer.WriteBool(waitForWater);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetRefreshChunkAfterSent", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetRefreshNotNeeded(NetworkConnection con, int chunkX, int chunkY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(chunkX);
		writer.WriteInt(chunkY);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetRefreshNotNeeded", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetGiveDigUpTreasureMilestone(NetworkConnection con, int itemId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetGiveDigUpTreasureMilestone", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetGiveBuryItemMilestone(NetworkConnection con, int itemId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetGiveBuryItemMilestone", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetGiveHuntingXp(NetworkConnection con, int animalId, int variation)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(animalId);
		writer.WriteInt(variation);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetGiveHuntingXp", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcCheckHuntingTaskCompletion(int animalId, Vector3 killPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(animalId);
		writer.WriteVector3(killPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcCheckHuntingTaskCompletion", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetGiveHarvestMilestone(NetworkConnection con, int tileObjectGiving)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(tileObjectGiving);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetGiveHarvestMilestone", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator delayRefresh(int chunkX, int chunkY)
	{
		yield return null;
		yield return null;
		yield return null;
		WorldManager.Instance.refreshAllChunksInUse(chunkX, chunkY, networkRefresh: true);
	}

	[TargetRpc]
	public void TargetGiveSignDetailsForChunk(NetworkConnection con, SignDetails[] signsInChunk)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		GeneratedNetworkCode._Write_SignDetails_005B_005D(writer, signsInChunk);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetGiveSignDetailsForChunk", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetGiveSignDetailsForHouse(NetworkConnection con, SignDetails[] signsInChunk, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		GeneratedNetworkCode._Write_SignDetails_005B_005D(writer, signsInChunk);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetGiveSignDetailsForHouse", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetGiveChunkWaterDetails(NetworkConnection con, int chunkX, int chunkY, bool[] waterDetails)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(chunkX);
		writer.WriteInt(chunkY);
		GeneratedNetworkCode._Write_System_002EBoolean_005B_005D(writer, waterDetails);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetGiveChunkWaterDetails", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetGiveChunkOnTileDetails(NetworkConnection con, int chunkX, int chunkY, int[] onTileDetails, int[] otherDetails)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(chunkX);
		writer.WriteInt(chunkY);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, onTileDetails);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, otherDetails);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetGiveChunkOnTileDetails", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetGiveChunkOnTopDetails(NetworkConnection con, ItemOnTop[] onTopInThisChunk)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		GeneratedNetworkCode._Write_ItemOnTop_005B_005D(writer, onTopInThisChunk);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetGiveChunkOnTopDetails", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetGiveChunkTileTypeDetails(NetworkConnection con, int chunkX, int chunkY, int[] tileTypeDetails)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(chunkX);
		writer.WriteInt(chunkY);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, tileTypeDetails);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetGiveChunkTileTypeDetails", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetRequestShopStall(NetworkConnection con, bool[] stallDetails)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		GeneratedNetworkCode._Write_System_002EBoolean_005B_005D(writer, stallDetails);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetRequestShopStall", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetGiveChunkHeightDetails(NetworkConnection con, int chunkX, int chunkY, int[] heightDetails)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(chunkX);
		writer.WriteInt(chunkY);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, heightDetails);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetGiveChunkHeightDetails", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetRequestMuseum(NetworkConnection con, bool[] fishDonated, bool[] bugsDonated, bool[] underWaterCreatesDonated)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		GeneratedNetworkCode._Write_System_002EBoolean_005B_005D(writer, fishDonated);
		GeneratedNetworkCode._Write_System_002EBoolean_005B_005D(writer, bugsDonated);
		GeneratedNetworkCode._Write_System_002EBoolean_005B_005D(writer, underWaterCreatesDonated);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetRequestMuseum", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcPlaceMultiTiledObject(int multiTiledObjectId, int xPos, int yPos, int rotation)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(multiTiledObjectId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(rotation);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcPlaceMultiTiledObject", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcPlaceMultiTiledObjectPlaceUnder(int multiTiledObjectId, int xPos, int yPos, int rotation)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(multiTiledObjectId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(rotation);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcPlaceMultiTiledObjectPlaceUnder", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcPlaceBridgeTiledObject(int multiTiledObjectId, int xPos, int yPos, int rotation, int length)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(multiTiledObjectId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(rotation);
		writer.WriteInt(length);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcPlaceBridgeTiledObject", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public void getBulletinBoardAndSend(NetworkConnection conn)
	{
		TargetSyncBulletinBoardPosts(conn, BulletinBoard.board.attachedPosts.ToArray());
	}

	[ClientRpc]
	public void RpcAddNewTaskToClientBoard(PostOnBoard newPost)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		GeneratedNetworkCode._Write_PostOnBoard(writer, newPost);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcAddNewTaskToClientBoard", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcFillVillagerDetails(uint netId, int npcId, bool gen, int nameId, int skinId, int hairId, int hairColourId, int eyeId, int eyeColourId, int shirtId, int pantsId, int shoesId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(netId);
		writer.WriteInt(npcId);
		writer.WriteBool(gen);
		writer.WriteInt(nameId);
		writer.WriteInt(skinId);
		writer.WriteInt(hairId);
		writer.WriteInt(hairColourId);
		writer.WriteInt(eyeId);
		writer.WriteInt(eyeColourId);
		writer.WriteInt(shirtId);
		writer.WriteInt(pantsId);
		writer.WriteInt(shoesId);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcFillVillagerDetails", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetSyncBulletinBoardPosts(NetworkConnection conn, PostOnBoard[] allPosts)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		GeneratedNetworkCode._Write_PostOnBoard_005B_005D(writer, allPosts);
		SendTargetRPCInternal(conn, typeof(NetworkMapSharer), "TargetSyncBulletinBoardPosts", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetGiveStamina(NetworkConnection conn)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendTargetRPCInternal(conn, typeof(NetworkMapSharer), "TargetGiveStamina", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetSendBugCompLetter(NetworkConnection conn, int position)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(position);
		SendTargetRPCInternal(conn, typeof(NetworkMapSharer), "TargetSendBugCompLetter", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetSendFishingCompLetter(NetworkConnection conn, int position)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(position);
		SendTargetRPCInternal(conn, typeof(NetworkMapSharer), "TargetSendFishingCompLetter", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcSetRotation(int xPos, int yPos, int rotation)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(rotation);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcSetRotation", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetGetRotationForTile(NetworkConnection con, int xPos, int yPos, int rotation)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(rotation);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetGetRotationForTile", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcDeliverAnimal(uint deliveredBy, int animalDelivered, int variationDelivered, int rewardToSend, int trapType)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(deliveredBy);
		writer.WriteInt(animalDelivered);
		writer.WriteInt(variationDelivered);
		writer.WriteInt(rewardToSend);
		writer.WriteInt(trapType);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcDeliverAnimal", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcSellByWeight(uint deliveredBy, uint itemDelivered, int keeperId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(deliveredBy);
		writer.WriteUInt(itemDelivered);
		writer.WriteInt(keeperId);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcSellByWeight", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator waitForShopKeeperToBeReady(int keeperId, SellByWeight toSell)
	{
		yield return new WaitForSeconds(0.25f);
		while (!NPCManager.manage.getVendorNPC((NPCSchedual.Locations)keeperId).IsValidConversationTargetForAnyPlayer() && Vector3.Distance(localChar.transform.position, toSell.transform.position) < 15f)
		{
			yield return null;
		}
		if (NPCManager.manage.getVendorNPC((NPCSchedual.Locations)keeperId).IsValidConversationTargetForAnyPlayer() && Vector3.Distance(localChar.transform.position, toSell.transform.position) < 15f)
		{
			ConversationManager.manage.TalkToNPC(NPCManager.manage.getVendorNPC((NPCSchedual.Locations)keeperId), GiveNPC.give.giveItemToSellByWeightConversation);
		}
	}

	[ClientRpc]
	public void RpcClearHouseForMove(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcClearHouseForMove", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcPickUpContainerObject(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcPickUpContainerObject", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcPickUpContainerObjectInside(int xPos, int yPos, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcPickUpContainerObjectInside", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcRemoveMultiTiledObject(int multiTiledObjectId, int xPos, int yPos, int rotationRemove)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(multiTiledObjectId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(rotationRemove);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcRemoveMultiTiledObject", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcPlaceItemOnToTileObject(int give, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(give);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcPlaceItemOnToTileObject", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcGiveOnTileStatus(int give, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(give);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcGiveOnTileStatus", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcUseInstagrow(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcUseInstagrow", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcGiveOnTileStatusInside(int give, int xPos, int yPos, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(give);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcGiveOnTileStatusInside", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcCompleteBulletinBoard(int id)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(id);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcCompleteBulletinBoard", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcShowOffBuilding(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcShowOffBuilding", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcSyncDate(int day, int week, int month, int year, int currentMinute)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(day);
		writer.WriteInt(week);
		writer.WriteInt(month);
		writer.WriteInt(year);
		writer.WriteInt(currentMinute);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcSyncDate", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcMakeAWish(string wishersName, int newWish, Vector3 PartPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteString(wishersName);
		writer.WriteInt(newWish);
		writer.WriteVector3(PartPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcMakeAWish", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcAddADay(int newMineSeed)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newMineSeed);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcAddADay", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator nextDayDelay(int newMineSeed)
	{
		WorldManager.Instance.cleanOutObjects();
		yield return StartCoroutine(startChangeDayEffect());
		StartCoroutine(CharLevelManager.manage.openLevelUpWindow());
		StartCoroutine(fadeToBlack.fadeInDateText());
		StatusManager.manage.nextDayReset();
		if (base.isServer)
		{
			RenderMap.Instance.ClearAllNPCMarkers();
			RealWorldTimeLight.time.NetworkcurrentHour = 7;
			StartCoroutine(RealWorldTimeLight.time.startNewDay());
		}
		else
		{
			StartCoroutine(RealWorldTimeLight.time.startNewDayClient());
		}
		NPCManager.manage.resetNPCRequestsForNewDay();
		FarmAnimalManager.manage.newDayCheck();
		yield return StartCoroutine(WorldManager.Instance.nextDayChanges(WeatherManager.Instance.rainMgr.IsActive, newMineSeed));
		if (base.isServer)
		{
			NPCManager.manage.returnGuestNPCs();
			MarketPlaceManager.manage.placeMarketStallAndSpawnNPC();
			NPCManager.manage.StartNewDay(useTomorrowsDate: true);
			TownManager.manage.randomiseRecyclingBox();
			DestroySignalAtEndOfDay();
		}
		else
		{
			ScheduleManager.manage.giveNpcsNewDaySchedual(RealWorldTimeLight.time.getTomorrowsDay(), RealWorldTimeLight.time.getTomorrowsWeek(), RealWorldTimeLight.time.getTomorrowsMonth());
		}
		if (BulletinBoard.board.attachedPosts.Count > 0)
		{
			BulletinBoard.board.checkExpiredAndRemove();
			if (base.isServer)
			{
				BulletinBoard.board.selectRandomPost(newMineSeed);
			}
		}
		TownManager.manage.townMembersDonate();
		BankMenu.menu.addDailyInterest();
		StartCoroutine(RenderMap.Instance.updateMap());
		NetworkmineSeed = newMineSeed;
		if (base.isServer && GenerateUndergroundMap.generate.mineGeneratedToday)
		{
			RealWorldTimeLight.time.NetworkmineLevel = -1;
		}
		GenerateUndergroundMap.generate.mineGeneratedToday = false;
		GenerateVisitingIsland.Instance.offIslandGeneratedToday = false;
		wishManager.CheckWishOnDayChange();
		if (base.isServer)
		{
			yield return StartCoroutine(WorldManager.Instance.fenceCheck());
			AnimalManager.manage.nextDayAnimalChunks();
		}
		RealWorldTimeLight.time.setDegreesNewDay();
		if (base.isServer)
		{
			MarketPlaceManager.manage.checkForSpecialVisitors(RealWorldTimeLight.time.getTomorrowsDay());
			DestroyAllDroppedItemsAndCarryablesNotOnLevelAtEndOfDay();
		}
		RealWorldTimeLight.time.nextDay();
		WorldManager.Instance.changeDayEvent.Invoke();
		DailyTaskGenerator.generate.generateNewDailyTasks();
		MailManager.manage.sendDailyMail();
		HouseManager.manage.updateAllHouseFurniturePos();
		BuriedManager.manage.CheckAllBuriedItemsToGrowGiantTree();
		if (base.isServer)
		{
			WeatherManager.Instance.ClearWeatherOnSpecialDay();
		}
		while (CharLevelManager.manage.levelUpWindowOpen)
		{
			yield return null;
		}
		while (RealWorldTimeLight.time.underGround)
		{
			yield return null;
		}
		SaveLoad.saveOrLoad.newFileSaver.SaveGame(base.isServer);
		while (!nextDayIsReady)
		{
			yield return null;
		}
		wishManager.NetworkwishMadeToday = false;
		CatchingCompetitionManager.manage.UpdateCompDetails();
		TownEventManager.manage.checkForTownEventAndSetUp(WorldManager.Instance.day, WorldManager.Instance.week, WorldManager.Instance.month);
		QuestTracker.track.updatePinnedTask();
		SaveLoad.saveOrLoad.loadingScreen.completed();
		StartCoroutine(endChangeDayEffect());
		AnimalManager.manage.SetChangedOverNight(changeTo: false);
		sleeping = false;
		yield return new WaitForSeconds(2f);
		SaveLoad.saveOrLoad.loadingScreen.disappear();
	}

	[TargetRpc]
	public void TargetRequestHouse(NetworkConnection con, int xPos, int yPos, int[] onTile, int[] onTileStatus, int[] onTileRotation, int wall, int floor, ItemOnTop[] onTopItems)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, onTile);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, onTileStatus);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, onTileRotation);
		writer.WriteInt(wall);
		writer.WriteInt(floor);
		GeneratedNetworkCode._Write_ItemOnTop_005B_005D(writer, onTopItems);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetRequestHouse", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetRequestExterior(NetworkConnection con, int xPos, int yPos, int houseBase, int roof, int windows, int door, int wallMat, string wallColor, int houseMat, string houseColor, int roofMat, string roofColor, int fenceId, string buildingName)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseBase);
		writer.WriteInt(roof);
		writer.WriteInt(windows);
		writer.WriteInt(door);
		writer.WriteInt(wallMat);
		writer.WriteString(wallColor);
		writer.WriteInt(houseMat);
		writer.WriteString(houseColor);
		writer.WriteInt(roofMat);
		writer.WriteString(roofColor);
		writer.WriteInt(fenceId);
		writer.WriteString(buildingName);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetRequestExterior", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcGiveOnTopStatus(int newStatus, int xPos, int yPos, int onTopPos, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newStatus);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(onTopPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcGiveOnTopStatus", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcFillWithWater(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcFillWithWater", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcStallSold(int stallTypeId, int shopStallNo)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(stallTypeId);
		writer.WriteInt(shopStallNo);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcStallSold", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcSpinChair()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(NetworkMapSharer), "RpcSpinChair", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcUpgradeHouse(int newHouseId, int houseXPos, int houseYPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newHouseId);
		writer.WriteInt(houseXPos);
		writer.WriteInt(houseYPos);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcUpgradeHouse", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private void checkIfHouseNeedsUpgradeDelay()
	{
	}

	[ClientRpc]
	public void RpcChangeHouseOnTile(int newTileType, int xPos, int yPos, int rotation, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newTileType);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(rotation);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcChangeHouseOnTile", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private void checkAndSetStatusOnChange(int newId, int xPos, int yPos)
	{
		if (newId > -1)
		{
			if ((bool)WorldManager.Instance.allObjects[newId].tileObjectGrowthStages || (bool)WorldManager.Instance.allObjects[newId].tileObjectFurniture || (bool)WorldManager.Instance.allObjects[newId].tileOnOff || (bool)WorldManager.Instance.allObjects[newId].tileObjectChest)
			{
				WorldManager.Instance.onTileStatusMap[xPos, yPos] = 0;
			}
		}
		else
		{
			WorldManager.Instance.onTileStatusMap[xPos, yPos] = -1;
		}
	}

	private void onHairDresserSeatChange(bool old, bool newStatus)
	{
		NetworkhairDresserSeatOccupied = newStatus;
		if ((bool)HairDresserSeat.seat)
		{
			HairDresserSeat.seat.updateTheSeat(newStatus);
		}
	}

	private void checkMapButtons(string checkString)
	{
		for (int i = 0; i < RenderMap.Instance.mapIcons.Count; i++)
		{
			if (RenderMap.Instance.mapIcons[i].TelePointName == checkString)
			{
				if (checkString == "private")
				{
					MonoBehaviour.print("Placing private tele tower at " + new Vector3(privateTowerPos.x * 2f, 0f, privateTowerPos.y * 2f).ToString());
					RenderMap.Instance.mapIcons[i].SetPointingAtPositionAndLocalPointingAtPosition(new Vector3(privateTowerPos.x * 2f, 0f, privateTowerPos.y * 2f));
				}
				return;
			}
		}
		RenderMap.Instance.createTeleIcons(checkString);
	}

	private void northCheck(bool old, bool newStatus)
	{
		NetworknorthOn = newStatus;
		checkMapButtons("north");
	}

	private void eastCheck(bool old, bool newStatus)
	{
		NetworkeastOn = newStatus;
		checkMapButtons("east");
	}

	private void craftsmanWorkingChange(bool old, bool newCrafting)
	{
		NetworkcraftsmanWorking = newCrafting;
		CraftsmanManager.manage.switchCrafterConvo();
	}

	private void southCheck(bool old, bool newStatus)
	{
		NetworksouthOn = newStatus;
		checkMapButtons("south");
	}

	private void westCheck(bool old, bool newStatus)
	{
		NetworkwestOn = newStatus;
		checkMapButtons("west");
	}

	private void privateTowerCheck(Vector2 old, Vector2 newPosition)
	{
		NetworkprivateTowerPos = newPosition;
		if (privateTowerPos != Vector2.zero)
		{
			checkMapButtons("private");
			RequestChunkAtXPosAndYPos((int)privateTowerPos.x, (int)privateTowerPos.y);
		}
	}

	private void ChangeAuthorityOfAllCarryObjectInProximity(int posX, int posY)
	{
		Vector3 pos = new Vector3(posX * 2, WorldManager.Instance.heightMap[posX, posY], posY * 2);
		if (NetworkServer.active)
		{
			ServerChangeAuthorityOfAllCarryObjectInProximity(pos);
		}
	}

	[Server]
	public void ServerChangeAuthorityOfAllCarryObjectInProximity(Vector3 pos)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void NetworkMapSharer::ServerChangeAuthorityOfAllCarryObjectInProximity(UnityEngine.Vector3)' called when server was not active");
			return;
		}
		CharMovement closestPlayer = NetworkNavMesh.nav.GetClosestPlayer(pos);
		float radius = 10f;
		Collider[] array = new Collider[1000];
		Physics.OverlapSphereNonAlloc(pos, radius, array, LayerMask.GetMask("CarryItem", "ItemThrow", "Damageable"));
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (collider == null)
			{
				break;
			}
			if (!collider.gameObject.CompareTag("CarryObject"))
			{
				continue;
			}
			PickUpAndCarry componentInParent = collider.gameObject.GetComponentInParent<PickUpAndCarry>();
			if (componentInParent.IsCarriedByPlayer)
			{
				continue;
			}
			componentInParent.ServerChangeAuthority((NetworkConnectionToClient)closestPlayer.connectionToClient);
			if (componentInParent.beingCarriedBy == 0 || !NetworkIdentity.spawned.ContainsKey(componentInParent.beingCarriedBy))
			{
				if (!NetworkIdentity.spawned.ContainsKey(componentInParent.beingCarriedBy))
				{
					componentInParent.NetworkbeingCarriedBy = 0u;
				}
				componentInParent.TurnOffKinematicForAuthority();
			}
		}
	}

	private IEnumerator MoveOffIslandRoutine()
	{
		AirportEntranceExit.entrance.CloseDoors();
		DestroySignalAtEndOfDay();
		yield return new WaitForSeconds(1.5f);
		if (base.isServer)
		{
			if (!GenerateVisitingIsland.Instance.offIslandGeneratedToday)
			{
				GenerateVisitingIsland.Instance.offIslandGeneratedToday = true;
				yield return StartCoroutine(GenerateVisitingIsland.Instance.GenerateReefIslands());
			}
			yield return null;
			MapStorer.store.waitingForMapToStore = true;
			MapStorer.store.storeMap(MapStorer.LoadMapType.Overworld);
			while (MapStorer.store.waitingForMapToStore)
			{
				yield return null;
			}
			MapStorer.store.waitForMapToLoad = true;
			MapStorer.store.loadStoredMap(MapStorer.LoadMapType.OffIsland);
			while (MapStorer.store.waitForMapToLoad)
			{
				yield return null;
			}
			returnAgents.Invoke();
			RealWorldTimeLight.time.NetworkoffIsland = true;
			yield return null;
			onChangeMaps.Invoke();
			NetworkserverOffIslandIsLoaded = true;
			showDroppedItemsForLevel();
		}
		else
		{
			yield return StartCoroutine(GenerateVisitingIsland.Instance.GenerateReefIslands());
			MapStorer.store.getStoredOffIslandMapForConnect();
			while (!serverOffIslandIsLoaded)
			{
				yield return null;
			}
		}
		localChar.attackLockOn(isOn: true);
		localChar.myPickUp.ChangeKinematicForLevelChange(shouldBeKinematic: true);
		WorldManager.Instance.refreshAllChunksForSwitch(AirportEntranceExit.entrance.transform.position);
		bool wait = true;
		while (wait)
		{
			if ((bool)AirportEntranceExit.exit && AirportEntranceExit.exit.gameObject.activeInHierarchy)
			{
				wait = false;
			}
			yield return null;
		}
		yield return null;
		WorldManager.Instance.spawnPos.position = AirportEntranceExit.exit.offIslandSpawnPoint.transform.position;
		AirportEntranceExit.exit.StartFlyingAnimation();
		localChar.myPickUp.ChangeKinematicForLevelChange(shouldBeKinematic: false);
		localChar.transform.position = new Vector3(localChar.transform.position.x, localChar.transform.position.y, localChar.transform.position.z);
		localChar.attackLockOn(isOn: false);
		CameraController.control.transform.position = localChar.transform.position;
		StartCoroutine(RenderMap.Instance.ClearMapForOffIsland());
	}

	private IEnumerator moveDownMines()
	{
		MineEnterExit.mineEntrance.closeDoors();
		DestroySignalAtEndOfDay();
		yield return new WaitForSeconds(1.5f);
		if (base.isServer)
		{
			if (!GenerateUndergroundMap.generate.mineGeneratedToday)
			{
				GenerateUndergroundMap.generate.mineGeneratedToday = true;
				yield return StartCoroutine(GenerateUndergroundMap.generate.generateNewMinesForDay());
			}
			yield return null;
			MapStorer.store.waitingForMapToStore = true;
			MapStorer.store.storeMap(MapStorer.LoadMapType.Overworld);
			while (MapStorer.store.waitingForMapToStore)
			{
				yield return null;
			}
			MapStorer.store.waitForMapToLoad = true;
			MapStorer.store.loadStoredMap(MapStorer.LoadMapType.Underworld);
			while (MapStorer.store.waitForMapToLoad)
			{
				yield return null;
			}
			returnAgents.Invoke();
			RealWorldTimeLight.time.NetworkunderGround = true;
			onChangeMaps.Invoke();
			NetworkserverUndergroundIsLoaded = true;
			showDroppedItemsForLevel();
		}
		else
		{
			yield return StartCoroutine(GenerateUndergroundMap.generate.generateMineForClient(mineSeed));
			MapStorer.store.getStoredMineMapForConnect();
			while (!serverUndergroundIsLoaded)
			{
				yield return null;
			}
		}
		WorldManager.Instance.refreshAllChunksForSwitch(MineEnterExit.mineEntrance.transform.position);
		bool wait = true;
		localChar.attackLockOn(isOn: true);
		localChar.myPickUp.ChangeKinematicForLevelChange(shouldBeKinematic: true);
		while (wait)
		{
			if ((bool)MineEnterExit.mineExit && MineEnterExit.mineExit.gameObject.activeInHierarchy)
			{
				wait = false;
			}
			yield return null;
		}
		WorldManager.Instance.spawnPos.position = new Vector3(MineEnterExit.mineExit.transform.position.x, MineEnterExit.mineExit.position.position.y, MineEnterExit.mineExit.transform.position.z);
		MineEnterExit.mineExit.startElevatorTimer();
		localChar.myPickUp.ChangeKinematicForLevelChange(shouldBeKinematic: false);
		localChar.transform.position = new Vector3(localChar.transform.position.x, MineEnterExit.mineExit.position.position.y, localChar.transform.position.z);
		localChar.attackLockOn(isOn: false);
		CameraController.control.transform.position = localChar.transform.position;
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.EnterMines);
		StartCoroutine(RenderMap.Instance.ClearMapForUnderground());
	}

	private IEnumerator unlockDelay(int xPos, int yPos)
	{
		yield return new WaitForSeconds(0.5f);
		WorldManager.Instance.unlockClientTile(xPos, yPos);
	}

	public IEnumerator moveUpMines(bool needsMineEntry = true)
	{
		MineEnterExit.mineExit.closeDoors();
		yield return new WaitForSeconds(1.5f);
		if (base.isServer)
		{
			yield return null;
			MapStorer.store.waitingForMapToStore = true;
			MapStorer.store.storeMap(MapStorer.LoadMapType.Underworld);
			while (MapStorer.store.waitingForMapToStore)
			{
				yield return null;
			}
			MapStorer.store.loadStoredMap(MapStorer.LoadMapType.Overworld);
			MapStorer.store.waitForMapToLoad = true;
			while (MapStorer.store.waitForMapToLoad)
			{
				yield return null;
			}
			returnAgents.Invoke();
			RealWorldTimeLight.time.NetworkunderGround = false;
			onChangeMaps.Invoke();
			NetworkserverUndergroundIsLoaded = false;
			showDroppedItemsForLevel();
		}
		else
		{
			yield return StartCoroutine(GenerateMap.generate.generateNewMap(seed));
			while (serverUndergroundIsLoaded)
			{
				yield return null;
			}
		}
		WorldManager.Instance.refreshAllChunksForSwitch(MineEnterExit.mineExit.transform.position);
		localChar.attackLockOn(isOn: true);
		localChar.myPickUp.ChangeKinematicForLevelChange(shouldBeKinematic: true);
		bool wait = true;
		while (wait)
		{
			if (((bool)MineEnterExit.mineEntrance && MineEnterExit.mineEntrance.gameObject.activeInHierarchy) || !needsMineEntry)
			{
				wait = false;
			}
			yield return null;
		}
		if (needsMineEntry)
		{
			MineEnterExit.mineEntrance.startElevatorTimer();
		}
		else
		{
			MineEnterExit.mineEntrance.openDoorOnDeath();
		}
		if ((bool)nonLocalSpawnPos)
		{
			WorldManager.Instance.spawnPos.position = nonLocalSpawnPos.position;
		}
		else
		{
			WorldManager.Instance.spawnPos.position = GenerateMap.generate.originalSpawnPoint;
		}
		localChar.myPickUp.ChangeKinematicForLevelChange(shouldBeKinematic: false);
		localChar.transform.position = new Vector3(localChar.transform.position.x, MineEnterExit.mineEntrance.position.position.y, localChar.transform.position.z);
		localChar.attackLockOn(isOn: false);
		CameraController.control.transform.position = localChar.transform.position;
		RenderMap.Instance.ReturnMapToMainIslandView();
	}

	public IEnumerator ReturnFromOffIsland(bool needsMineEntry = true)
	{
		AirportEntranceExit.exit.CloseDoors();
		yield return new WaitForSeconds(1.5f);
		if (base.isServer)
		{
			yield return null;
			MapStorer.store.waitingForMapToStore = true;
			MapStorer.store.storeMap(MapStorer.LoadMapType.OffIsland);
			while (MapStorer.store.waitingForMapToStore)
			{
				yield return null;
			}
			MapStorer.store.loadStoredMap(MapStorer.LoadMapType.Overworld);
			MapStorer.store.waitForMapToLoad = true;
			while (MapStorer.store.waitForMapToLoad)
			{
				yield return null;
			}
			returnAgents.Invoke();
			RealWorldTimeLight.time.NetworkoffIsland = false;
			onChangeMaps.Invoke();
			NetworkserverUndergroundIsLoaded = false;
			showDroppedItemsForLevel();
		}
		else
		{
			yield return StartCoroutine(GenerateMap.generate.generateNewMap(seed));
			while (serverUndergroundIsLoaded)
			{
				yield return null;
			}
		}
		WorldManager.Instance.refreshAllChunksForSwitch(AirportEntranceExit.exit.transform.position);
		localChar.attackLockOn(isOn: true);
		localChar.myPickUp.ChangeKinematicForLevelChange(shouldBeKinematic: true);
		bool wait = true;
		while (wait)
		{
			if (((bool)AirportEntranceExit.entrance && AirportEntranceExit.entrance.gameObject.activeInHierarchy) || !needsMineEntry)
			{
				wait = false;
			}
			yield return null;
		}
		if (needsMineEntry)
		{
			AirportEntranceExit.entrance.StartFlyingAnimation();
		}
		if ((bool)nonLocalSpawnPos)
		{
			WorldManager.Instance.spawnPos.position = nonLocalSpawnPos.position;
		}
		else
		{
			WorldManager.Instance.spawnPos.position = GenerateMap.generate.originalSpawnPoint;
		}
		localChar.myPickUp.ChangeKinematicForLevelChange(shouldBeKinematic: false);
		localChar.transform.position = new Vector3(localChar.transform.position.x, localChar.transform.position.y, localChar.transform.position.z);
		localChar.attackLockOn(isOn: false);
		CameraController.control.transform.position = localChar.transform.position;
		RenderMap.Instance.ReturnMapToMainIslandView();
	}

	private IEnumerator startChangeDayEffect()
	{
		fadeToBlack.fadeIn();
		MusicManager.manage.stopMusic();
		yield return new WaitForSeconds(0.5f);
		SoundManager.Instance.play2DSound(SoundManager.Instance.goToSleepSound);
	}

	private IEnumerator endChangeDayEffect()
	{
		yield return StartCoroutine(fadeToBlack.fadeOutDateText());
		MusicManager.manage.startMusic();
		fadeToBlack.fadeOut();
	}

	private void showDroppedItemsForLevel()
	{
		for (int i = 0; i < WorldManager.Instance.itemsOnGround.Count; i++)
		{
			if (WorldManager.Instance.itemsOnGround[i].IsDropOnCurrentLevel())
			{
				if (!WorldManager.Instance.itemsOnGround[i].gameObject.activeSelf)
				{
					WorldManager.Instance.itemsOnGround[i].gameObject.SetActive(value: true);
					NetworkServer.Spawn(WorldManager.Instance.itemsOnGround[i].gameObject);
				}
			}
			else if (WorldManager.Instance.itemsOnGround[i].gameObject.activeSelf)
			{
				WorldManager.Instance.itemsOnGround[i].gameObject.SetActive(value: false);
				NetworkServer.UnSpawn(WorldManager.Instance.itemsOnGround[i].gameObject);
			}
		}
		for (int j = 0; j < WorldManager.Instance.allCarriables.Count; j++)
		{
			if (!(WorldManager.Instance.allCarriables[j].transform.position.y <= -12f))
			{
				if (!WorldManager.Instance.allCarriables[j].IsDropOnCurrentLevel() && WorldManager.Instance.allCarriables[j].gameObject.activeInHierarchy)
				{
					WorldManager.Instance.allCarriables[j].RemoveAuthorityBeforeBeforeServerDestroy();
					WorldManager.Instance.allCarriables[j].gameObject.SetActive(value: false);
					NetworkServer.UnSpawn(WorldManager.Instance.allCarriables[j].gameObject);
				}
				else if (WorldManager.Instance.allCarriables[j].IsDropOnCurrentLevel() && !WorldManager.Instance.allCarriables[j].gameObject.activeInHierarchy)
				{
					WorldManager.Instance.allCarriables[j].gameObject.SetActive(value: true);
					NetworkServer.Spawn(WorldManager.Instance.allCarriables[j].gameObject);
				}
			}
		}
	}

	private void DestroyAllDroppedItemsAndCarryablesNotOnLevelAtEndOfDay()
	{
		for (int num = WorldManager.Instance.itemsOnGround.Count - 1; num >= 0; num--)
		{
			DroppedItem droppedItem = WorldManager.Instance.itemsOnGround[num];
			if (!droppedItem.IsDropOnCurrentLevel() && !droppedItem.gameObject.activeSelf)
			{
				WorldManager.Instance.itemsOnGround.RemoveAt(num);
				Object.Destroy(droppedItem.gameObject);
			}
		}
		for (int num2 = WorldManager.Instance.allCarriables.Count - 1; num2 >= 0; num2--)
		{
			PickUpAndCarry pickUpAndCarry = WorldManager.Instance.allCarriables[num2];
			if (pickUpAndCarry.transform.position.y > -12f && !pickUpAndCarry.IsDropOnCurrentLevel() && !pickUpAndCarry.gameObject.activeInHierarchy)
			{
				WorldManager.Instance.allCarriables.RemoveAt(num2);
				Object.Destroy(pickUpAndCarry.gameObject);
			}
		}
	}

	[ClientRpc]
	private void RpcSendPhotoDetails(int photoSlot, byte[] package)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(photoSlot);
		writer.WriteBytesAndSize(package);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcSendPhotoDetails", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcSendFinalChunk(int photoSlot, byte[] package)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(photoSlot);
		writer.WriteBytesAndSize(package);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcSendFinalChunk", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	private void TargetSendPhotoDetails(NetworkConnection con, int photoSlot, byte[] package)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(photoSlot);
		writer.WriteBytesAndSize(package);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetSendPhotoDetails", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	private void TargetSendFinalChunk(NetworkConnection con, int photoSlot, byte[] package)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(photoSlot);
		writer.WriteBytesAndSize(package);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetSendFinalChunk", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetOpenBuildWindowForClient(NetworkConnection con, int buildingId, int[] alreadyGiven)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(buildingId);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, alreadyGiven);
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetOpenBuildWindowForClient", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetGivePermissionError(NetworkConnection con)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendTargetRPCInternal(con, typeof(NetworkMapSharer), "TargetGivePermissionError", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcSpawnCarryWorldObject(int carryId, Vector3 pos, Quaternion rotation)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(carryId);
		writer.WriteVector3(pos);
		writer.WriteQuaternion(rotation);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcSpawnCarryWorldObject", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public void SpawnMeteor()
	{
		NetworkServer.Spawn(Object.Instantiate(WeatherManager.Instance.meteorShowerMgr.fallingMeteorObject, new Vector3(1000f, 5000f, 1000f), Quaternion.identity));
	}

	[ClientRpc]
	public void RpcRefreshDeedIngredients(int buildingId, int[] alreadyGiven)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(buildingId);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, alreadyGiven);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcRefreshDeedIngredients", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcPlayBigStoneGrinderEffects(Vector3 position)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(position);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcPlayBigStoneGrinderEffects", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcPlayDestroyCarrySound(Vector3 position, int carryId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(position);
		writer.WriteInt(carryId);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcPlayDestroyCarrySound", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcPayTownDebt(int payment, uint payedBy)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(payment);
		writer.WriteUInt(payedBy);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcPayTownDebt", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public void ActivateTrap(uint animalToTrapId, int xPos, int yPos)
	{
		AnimalAI component = NetworkIdentity.spawned[animalToTrapId].GetComponent<AnimalAI>();
		if ((bool)component && WorldManager.Instance.onTileMap[xPos, yPos] != -1)
		{
			GameObject original = Instance.trapObject;
			if (WorldManager.Instance.onTileMap[xPos, yPos] == 306)
			{
				original = Instance.stickTrapObject;
			}
			NetworkNavMesh.nav.UnSpawnAnAnimal(component, saveToMap: false);
			TrappedAnimal component2 = Object.Instantiate(original, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2), Quaternion.identity).GetComponent<TrappedAnimal>();
			component2.NetworktrappedAnimalId = component.animalId;
			component2.NetworktrappedAnimalVariation = component.getVariationNo();
			component2.setAnimalInsideHealthDif(component.getMaxHealth() - component.getHealth());
			int num = 35;
			if (WorldManager.Instance.onTileMap[xPos, yPos] == 306)
			{
				num = 15;
			}
			if (component.getMaxHealth() > num && (WorldManager.Instance.onTileMap[xPos, yPos] != 306 || !((float)component.getHealth() <= component.animalRunAwayAtHealth + 2f)) && !((float)component.getHealth() <= (float)component.getMaxHealth() / 1.5f))
			{
				component2.Networkcaught = false;
				component2.startFreeSelfRoutine();
			}
			NetworkServer.Spawn(component2.gameObject);
			Instance.RpcActivateTrap(xPos, yPos);
			FarmAnimalManager.manage.removeAnimalHouse(xPos, yPos);
			WorldManager.Instance.onTileMap[xPos, yPos] = -1;
		}
		else
		{
			FarmAnimalManager.manage.removeAnimalHouse(xPos, yPos);
		}
	}

	[Server]
	public void ServerPlaceMarkerOnMap(Vector2 position, int iconSpriteIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void NetworkMapSharer::ServerPlaceMarkerOnMap(UnityEngine.Vector2,System.Int32)' called when server was not active");
		}
		else
		{
			NetworkServer.Spawn(RenderMap.Instance.CreateNewNetworkedPlayerSetMarker(position, iconSpriteIndex).gameObject);
		}
	}

	public void PlaceNickMarker(Vector3 pos)
	{
		if (base.isServer)
		{
			if (RenderMap.Instance.nickIcon == null)
			{
				RenderMap.Instance.nickIcon = RenderMap.Instance.CreateSpecialMapMarker(pos, -7);
				NetworkServer.Spawn(RenderMap.Instance.nickIcon.gameObject);
			}
			else
			{
				RenderMap.Instance.nickIcon.SetPointingAtPositionAndLocalPointingAtPosition(pos);
			}
		}
	}

	public void RemoveNickMarker()
	{
		if (base.isServer && (bool)RenderMap.Instance.nickIcon)
		{
			NetworkServer.Destroy(RenderMap.Instance.nickIcon.gameObject);
			RenderMap.Instance.nickIcon = null;
		}
	}

	[Server]
	public void RemoveMapPoint(MapPoint mapPoint)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void NetworkMapSharer::RemoveMapPoint(MapPoint)' called when server was not active");
		}
		else
		{
			mapPoints.Remove(mapPoint);
		}
	}

	public void placeAnimalInCollectionPoint(uint animalTrapPlaced)
	{
		PickUpAndCarry component = NetworkIdentity.spawned[animalTrapPlaced].GetComponent<PickUpAndCarry>();
		if ((bool)component)
		{
			TrappedAnimal component2 = NetworkIdentity.spawned[animalTrapPlaced].GetComponent<TrappedAnimal>();
			int rewardForCapturingAnimalIncludingBulletinBoards = BulletinBoard.board.getRewardForCapturingAnimalIncludingBulletinBoards(component2.trappedAnimalId, component2.trappedAnimalVariation);
			if (component.GetLastCarriedBy() == 0)
			{
				RpcDeliverAnimal(Instance.localChar.netId, component2.trappedAnimalId, component2.trappedAnimalVariation, rewardForCapturingAnimalIncludingBulletinBoards, Inventory.Instance.getInvItemId(component2.trapItemDropAfterOpen));
			}
			else
			{
				RpcDeliverAnimal(component.GetLastCarriedBy(), component2.trappedAnimalId, component2.trappedAnimalVariation, rewardForCapturingAnimalIncludingBulletinBoards, Inventory.Instance.getInvItemId(component2.trapItemDropAfterOpen));
			}
			component.Networkdelivered = true;
		}
	}

	[ClientRpc]
	public void RpcChangeTileObjectToColourVarient(int newItemId, int colourId, int xPos, int yPos, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newItemId);
		writer.WriteInt(colourId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcChangeTileObjectToColourVarient", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcSplashInWater(Vector3 position)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(position);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcSplashInWater", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcRingTownBell()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(NetworkMapSharer), "RpcRingTownBell", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcReleaseBugFromSitting(int bugId, Vector3 position)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(bugId);
		writer.WriteVector3(position);
		SendRPCInternal(typeof(NetworkMapSharer), "RpcReleaseBugFromSitting", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private void checkTeleportsOn()
	{
		if (WorldManager.Instance.onTileMap[TownManager.manage.northTowerPos[0], TownManager.manage.northTowerPos[1]] == 292)
		{
			NetworknorthOn = true;
		}
		if (WorldManager.Instance.onTileMap[TownManager.manage.eastTowerPos[0], TownManager.manage.eastTowerPos[1]] == 292)
		{
			NetworkeastOn = true;
		}
		if (WorldManager.Instance.onTileMap[TownManager.manage.southTowerPos[0], TownManager.manage.southTowerPos[1]] == 292)
		{
			NetworksouthOn = true;
		}
		if (WorldManager.Instance.onTileMap[TownManager.manage.westTowerPos[0], TownManager.manage.westTowerPos[1]] == 292)
		{
			NetworkwestOn = true;
		}
	}

	public void TurnOffAllSharkStatues(List<Vector2> statuePositions)
	{
		for (int i = 0; i < statuePositions.Count; i++)
		{
			if (WorldManager.Instance.onTileMap[(int)statuePositions[i].x, (int)statuePositions[i].y] == GenerateVisitingIsland.Instance.sharkStatue.tileObjectId)
			{
				RpChangeOnTileObjectNoDrop(GenerateVisitingIsland.Instance.normalSharkStatue.tileObjectId, (int)statuePositions[i].x, (int)statuePositions[i].y);
			}
		}
	}

	public IEnumerator sendPaintingsToClient(NetworkConnection con)
	{
		yield return new WaitForSeconds(1f);
		for (int p = 0; p < PhotoManager.manage.displayedPhotos.Length; p++)
		{
			if (!(MuseumManager.manage.paintingsOnDisplay[p] != null))
			{
				continue;
			}
			byte[] bytesToSend = PhotoManager.manage.getByteArrayForTransfer(PhotoManager.manage.displayedPhotos[p].photoName);
			List<byte> segment = new List<byte>();
			int segmentNo = 0;
			for (int i = 0; i < bytesToSend.Length; i++)
			{
				segment.Add(bytesToSend[i]);
				if (segment.Count >= bytesToSend.Length / 10 && segmentNo < 9)
				{
					segmentNo++;
					TargetSendPhotoDetails(con, p, segment.ToArray());
					segment.Clear();
					yield return new WaitForSeconds(0.02f);
				}
			}
			yield return new WaitForSeconds(0.02f);
			TargetSendFinalChunk(con, p, segment.ToArray());
		}
	}

	public IEnumerator sendNewPaintingToAll(int paintingNo)
	{
		if (!(MuseumManager.manage.paintingsOnDisplay[paintingNo] != null))
		{
			yield break;
		}
		byte[] bytesToSend = PhotoManager.manage.getByteArrayForTransfer(PhotoManager.manage.displayedPhotos[paintingNo].photoName);
		List<byte> segment = new List<byte>();
		int segmentNo = 0;
		for (int i = 0; i < bytesToSend.Length; i++)
		{
			segment.Add(bytesToSend[i]);
			if (segment.Count >= bytesToSend.Length / 10 && segmentNo < 9)
			{
				segmentNo++;
				RpcSendPhotoDetails(paintingNo, segment.ToArray());
				segment.Clear();
				yield return new WaitForSeconds(0.02f);
			}
		}
		yield return new WaitForSeconds(0.02f);
		RpcSendFinalChunk(paintingNo, segment.ToArray());
	}

	public void addChunkRequestedDelay(int chunkX, int chunkY)
	{
		chunkRequested.Add(new ChunkUpdateDelay(chunkX, chunkY));
	}

	public ChunkUpdateDelay getDelayForChunk(int chunkX, int chunkY)
	{
		for (int i = 0; i < chunkRequested.Count; i++)
		{
			if (chunkRequested[i].checkIfIsChunk(chunkX, chunkY))
			{
				return chunkRequested[i];
			}
		}
		return null;
	}

	private void OnDestroy()
	{
		mapPoints.Callback -= RenderMap.Instance.ScanAndUpdateScanAMapIconHighlights;
		tomorrowsWeather.Callback -= WeatherManager.Instance.ChangeWeather;
	}

	public void CreateTeleSignal(Vector3 position)
	{
		if (signal == null)
		{
			signal = Object.Instantiate(teleSignalObject, position, Quaternion.identity);
			NetworkServer.Spawn(signal);
		}
		else
		{
			signal.GetComponent<TeleSignal>().UpdatePosition(position);
		}
		todaysSignalPos = position;
	}

	public void DestroySignalAtEndOfDay()
	{
		if ((bool)signal)
		{
			NetworkServer.Destroy(signal);
			todaysSignalPos = Vector3.zero;
		}
	}

	public Vector3 GetSignalPosition()
	{
		return todaysSignalPos;
	}

	public void RequestChunkAtXPosAndYPos(int xPos, int yPos)
	{
		if (WorldManager.Instance.isPositionOnMap(xPos, yPos))
		{
			int num = Mathf.RoundToInt(xPos / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
			int num2 = Mathf.RoundToInt(yPos / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
			int num3 = num / WorldManager.Instance.getChunkSize();
			int num4 = num2 / WorldManager.Instance.getChunkSize();
			if ((bool)Instance && (bool)Instance.localChar && !Instance.isServer && !WorldManager.Instance.clientRequestedMap[num3 / WorldManager.Instance.getChunkSize(), num4 / WorldManager.Instance.getChunkSize()])
			{
				WorldManager.Instance.clientRequestedMap[num3 / WorldManager.Instance.chunkSize, num4 / WorldManager.Instance.getChunkSize()] = true;
				Instance.addChunkRequestedDelay(num3, num4);
				Instance.localChar.CmdRequestMapChunk(num3, num4);
			}
		}
	}

	public NetworkMapSharer()
	{
		InitSyncObject(mapPoints);
		InitSyncObject(todaysWeather);
		InitSyncObject(tomorrowsWeather);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlayCarryDeathPart(int carryId, Vector3 position)
	{
		Object.Instantiate(SaveLoad.saveOrLoad.carryablePrefabs[carryId].GetComponent<Damageable>().spawnWorldObjectOnDeath, position, Quaternion.identity);
		SoundManager.Instance.playASoundAtPoint(SaveLoad.saveOrLoad.carryablePrefabs[carryId].GetComponent<Damageable>().customDeathSound, position);
	}

	protected static void InvokeUserCode_RpcPlayCarryDeathPart(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayCarryDeathPart called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcPlayCarryDeathPart(reader.ReadInt(), reader.ReadVector3());
		}
	}

	protected void UserCode_RpcWaterExplodeOnLava(int[] xPositions, int[] yPositions)
	{
		List<int[]> list = new List<int[]>();
		for (int i = 0; i < xPositions.Length; i++)
		{
			WorldManager.Instance.onTileMap[xPositions[i], yPositions[i]] = -1;
			WorldManager.Instance.heightMap[xPositions[i], yPositions[i]]++;
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.extinquishLavalPart, new Vector3((float)(xPositions[i] * 2) + 1f, WorldManager.Instance.heightMap[xPositions[i], yPositions[i]], (float)(yPositions[i] * 2) + 1f), 50);
			if (base.isServer)
			{
				ChangeAuthorityOfAllCarryObjectInProximity(xPositions[i], yPositions[i]);
				WorldManager.Instance.checkAllCarryHeight(xPositions[i], yPositions[i]);
				WorldManager.Instance.updateDropsOnTileHeight(xPositions[i], yPositions[i]);
				WorldManager.Instance.onTileChunkHasChanged(xPositions[i], yPositions[i]);
				WorldManager.Instance.heightChunkHasChanged(xPositions[i], yPositions[i]);
			}
			int num = Mathf.RoundToInt(xPositions[i] / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
			int num2 = Mathf.RoundToInt(yPositions[i] / WorldManager.Instance.getChunkSize()) * WorldManager.Instance.getChunkSize();
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j][0] == num && list[j][1] == num2)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				int[] item = new int[2] { num, num2 };
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
			CheckLocalCharPreventFallThroughWorld(xPositions[i], yPositions[i]);
		}
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.extinguishLavaSound, new Vector3(xPositions[0] * 2, WorldManager.Instance.heightMap[xPositions[0], yPositions[0]], yPositions[0] * 2));
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.extinguishLavaSound, new Vector3(xPositions[xPositions.Length - 1] * 2, WorldManager.Instance.heightMap[xPositions[xPositions.Length - 1], yPositions[yPositions.Length - 1]], yPositions[yPositions.Length - 1] * 2));
		for (int k = 0; k < list.Count; k++)
		{
			WorldManager.Instance.refreshAllChunksInUse(list[k][0], list[k][1]);
		}
		NetworkNavMesh.nav.updateChunkInUse();
	}

	protected static void InvokeUserCode_RpcWaterExplodeOnLava(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcWaterExplodeOnLava called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcWaterExplodeOnLava(GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader));
		}
	}

	protected void UserCode_RpcPlayTrapperSound(Vector3 trapperWhistlePos)
	{
		if (Vector3.Distance(CameraController.control.transform.position, trapperWhistlePos) <= 250f)
		{
			SoundManager.Instance.play2DSound(SoundManager.Instance.trapperSound);
			NotificationManager.manage.createChatNotification(ConversationGenerator.generate.GetToolTip("Tip_HearWhistle"));
		}
	}

	protected static void InvokeUserCode_RpcPlayTrapperSound(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayTrapperSound called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcPlayTrapperSound(reader.ReadVector3());
		}
	}

	protected void UserCode_RpcFeedFishSound(Vector3 fishPos)
	{
		SoundManager.Instance.playASoundAtPoint(ChestWindow.chests.feedFishSound, fishPos);
	}

	protected static void InvokeUserCode_RpcFeedFishSound(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcFeedFishSound called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcFeedFishSound(reader.ReadVector3());
		}
	}

	protected void UserCode_RpcMoveOffIsland()
	{
		StartCoroutine(MoveOffIslandRoutine());
		WeatherManager.Instance.ChangeWeather();
	}

	protected static void InvokeUserCode_RpcMoveOffIsland(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcMoveOffIsland called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcMoveOffIsland();
		}
	}

	protected void UserCode_RpcMoveUnderGround()
	{
		canUseMineControls = false;
		StartCoroutine(moveDownMines());
		WeatherManager.Instance.StopWeather();
	}

	protected static void InvokeUserCode_RpcMoveUnderGround(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcMoveUnderGround called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcMoveUnderGround();
		}
	}

	protected void UserCode_RpcMoveAboveGround()
	{
		canUseMineControls = false;
		StartCoroutine(moveUpMines());
		WeatherManager.Instance.ChangeWeather();
	}

	protected static void InvokeUserCode_RpcMoveAboveGround(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcMoveAboveGround called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcMoveAboveGround();
		}
	}

	protected void UserCode_RpcReturnHomeFromOffIsland()
	{
		StartCoroutine(ReturnFromOffIsland());
		WeatherManager.Instance.ChangeWeather();
	}

	protected static void InvokeUserCode_RpcReturnHomeFromOffIsland(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcReturnHomeFromOffIsland called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcReturnHomeFromOffIsland();
		}
	}

	protected void UserCode_RpcCharEmotes(int no, uint netId)
	{
		NetworkIdentity.spawned[netId].GetComponent<EquipItemToChar>().doEmotion(no);
	}

	protected static void InvokeUserCode_RpcCharEmotes(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCharEmotes called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcCharEmotes(reader.ReadInt(), reader.ReadUInt());
		}
	}

	protected void UserCode_RpcBreakToolReact(uint netId)
	{
		NetworkIdentity.spawned[netId].GetComponent<EquipItemToChar>().breakItemAnimation();
	}

	protected static void InvokeUserCode_RpcBreakToolReact(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcBreakToolReact called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcBreakToolReact(reader.ReadUInt());
		}
	}

	protected void UserCode_RpcMakeChatBubble(string message, uint netId)
	{
		SoundManager.Instance.play2DSound(ChatBox.chat.chatSend);
		EquipItemToChar component = NetworkIdentity.spawned[netId].GetComponent<EquipItemToChar>();
		switch (message)
		{
		case "/laugh":
			component.doEmotion(1);
			break;
		case "/angry":
			component.doEmotion(2);
			break;
		case "/cry":
			component.doEmotion(3);
			break;
		default:
			_ = ChatBox.chat.chatLogOpen;
			ChatBox.chat.addToChatBox(component, message);
			break;
		}
	}

	protected static void InvokeUserCode_RpcMakeChatBubble(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcMakeChatBubble called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcMakeChatBubble(reader.ReadString(), reader.ReadUInt());
		}
	}

	protected void UserCode_RpcSpawnATileObjectDrop(int tileObjectToSpawnFrom, int xPos, int yPos, int tileStatus)
	{
		float y = 0f;
		float num = 1f;
		if (WorldManager.Instance.allObjectSettings[tileObjectToSpawnFrom].hasRandomRotation)
		{
			Random.InitState(xPos * yPos + xPos - yPos);
			y = Random.Range(0f, 360f);
			if (WorldManager.Instance.allObjectSettings[tileObjectToSpawnFrom].hasRandomScale)
			{
				num = Random.Range(0.75f, 1.1f);
			}
		}
		if (tileStatus > -1 && (bool)WorldManager.Instance.allObjects[tileObjectToSpawnFrom].tileObjectGrowthStages)
		{
			num *= WorldManager.Instance.allObjects[tileObjectToSpawnFrom].tileObjectGrowthStages.objectStages[tileStatus].transform.localScale.y;
		}
		Vector3 localScale = new Vector3(num, num, num);
		Vector3 position = new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2);
		Quaternion rotation = Quaternion.Euler(0f, y, 0f);
		Object.Instantiate(WorldManager.Instance.allObjectSettings[tileObjectToSpawnFrom].dropsObjectOnDeath, position, rotation).transform.localScale = localScale;
	}

	protected static void InvokeUserCode_RpcSpawnATileObjectDrop(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSpawnATileObjectDrop called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcSpawnATileObjectDrop(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcDepositItemIntoChanger(int itemDeposit, int xPos, int yPos)
	{
		WorldManager.Instance.unlockClientTile(xPos, yPos);
		if (base.isServer || WorldManager.Instance.clientHasRequestedChunk(xPos, yPos))
		{
			WorldManager.Instance.onTileStatusMap[xPos, yPos] = itemDeposit;
			TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
			if ((bool)tileObject)
			{
				tileObject.tileObjectItemChanger.playLocalDeposit(xPos, yPos);
			}
		}
	}

	protected static void InvokeUserCode_RpcDepositItemIntoChanger(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDepositItemIntoChanger called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcDepositItemIntoChanger(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcMoveHouseExterior(int xPos, int yPos, int newXpos, int newYPos)
	{
		HouseExterior houseExterior = HouseManager.manage.getHouseExterior(xPos, yPos);
		houseExterior.xPos = newXpos;
		houseExterior.yPos = newYPos;
	}

	protected static void InvokeUserCode_RpcMoveHouseExterior(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcMoveHouseExterior called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcMoveHouseExterior(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcMoveHouseInterior(int xPos, int yPos, int newXpos, int newYPos, int oldRotation, int newRotation)
	{
		HouseManager.manage.moveHousePos(xPos, yPos, newXpos, newYPos, oldRotation, newRotation);
	}

	protected static void InvokeUserCode_RpcMoveHouseInterior(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcMoveHouseInterior called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcMoveHouseInterior(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcDepositItemIntoChangerInside(int itemDeposit, int xPos, int yPos, int houseX, int houseY)
	{
		WorldManager.Instance.unlockClientTileHouse(xPos, yPos, houseX, houseY);
		HouseManager.manage.getHouseInfo(houseX, houseY).houseMapOnTileStatus[xPos, yPos] = itemDeposit;
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseX, houseY);
		if ((bool)displayPlayerHouseTiles)
		{
			TileObject tileObject = displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos];
			if ((bool)tileObject)
			{
				tileObject.tileObjectItemChanger.playLocalDeposit(xPos, yPos, HouseManager.manage.getHouseInfo(houseX, houseY));
			}
		}
	}

	protected static void InvokeUserCode_RpcDepositItemIntoChangerInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDepositItemIntoChangerInside called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcDepositItemIntoChangerInside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcUpdateHouseWall(int itemId, int houseX, int houseY)
	{
		HouseManager.manage.getHouseInfo(houseX, houseY).wall = itemId;
		Inventory.Instance.wallSlot.itemNo = itemId;
		Inventory.Instance.wallSlot.stack = 1;
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseX, houseY);
		if ((bool)displayPlayerHouseTiles)
		{
			displayPlayerHouseTiles.refreshWalls();
			displayPlayerHouseTiles.PlayWallOrFloorSound();
		}
	}

	protected static void InvokeUserCode_RpcUpdateHouseWall(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateHouseWall called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcUpdateHouseWall(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcUpdateHouseExterior(HouseExterior exterior)
	{
		HouseExterior houseExterior = HouseManager.manage.getHouseExterior(exterior.xPos, exterior.yPos);
		if (houseExterior != null)
		{
			exterior.copyToAnotherHouseExterior(houseExterior);
			HouseManager.manage.findHousesOnDisplay(houseExterior.xPos, houseExterior.yPos).updateHouseExterior(isCustomisation: true);
			RenderMap.Instance.UpdateIconName(houseExterior.xPos, houseExterior.yPos, houseExterior.houseName);
		}
	}

	protected static void InvokeUserCode_RpcUpdateHouseExterior(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateHouseExterior called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcUpdateHouseExterior(GeneratedNetworkCode._Read_HouseExterior(reader));
		}
	}

	protected void UserCode_RpcAddToMuseum(int newItem, string donatedBy)
	{
		MuseumManager.manage.donateItem(Inventory.Instance.allItems[newItem]);
		NotificationManager.manage.makeTopNotification(Inventory.Instance.allItems[newItem].getInvItemName(), string.Format(ConversationGenerator.generate.GetNotificationText("DonatedBy"), donatedBy));
	}

	protected static void InvokeUserCode_RpcAddToMuseum(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAddToMuseum called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcAddToMuseum(reader.ReadInt(), reader.ReadString());
		}
	}

	protected void UserCode_RpcUpdateHouseFloor(int itemId, int houseX, int houseY)
	{
		HouseManager.manage.getHouseInfo(houseX, houseY).floor = itemId;
		Inventory.Instance.floorSlot.itemNo = itemId;
		Inventory.Instance.floorSlot.stack = 1;
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseX, houseY);
		if ((bool)displayPlayerHouseTiles)
		{
			displayPlayerHouseTiles.refreshWalls();
			displayPlayerHouseTiles.PlayWallOrFloorSound();
		}
	}

	protected static void InvokeUserCode_RpcUpdateHouseFloor(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateHouseFloor called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcUpdateHouseFloor(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcPlaceOnTop(int newTileId, int xPos, int yPos, int houseX, int houseY)
	{
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseX, houseY);
		if (!displayPlayerHouseTiles)
		{
			return;
		}
		displayPlayerHouseTiles.refreshHouseTiles();
		if (newTileId == -1)
		{
			return;
		}
		Vector3 cursorPos = displayPlayerHouseTiles.getStartingPosTransform().position + new Vector3(xPos * 2, 0f, yPos * 2);
		TileObject tileObject = displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos];
		Vector3 position = new Vector3(xPos * 2, 0f, yPos * 2) + displayPlayerHouseTiles.getStartingPosTransform().position;
		if (tileObject == null)
		{
			Vector2 vector = WorldManager.Instance.findMultiTileObjectPos(xPos, yPos, HouseManager.manage.getHouseInfo(houseX, houseY));
			tileObject = displayPlayerHouseTiles.tileObjectsInHouse[(int)vector.x, (int)vector.y];
			if ((bool)tileObject)
			{
				position = tileObject.findClosestPlacedPosition(cursorPos).position;
			}
		}
		else if ((bool)tileObject)
		{
			position = tileObject.findClosestPlacedPosition(cursorPos).position;
		}
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.placeItem, new Vector3(xPos * 2, 0f, yPos * 2) + displayPlayerHouseTiles.getStartingPosTransform().position);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], position, 5);
	}

	protected static void InvokeUserCode_RpcPlaceOnTop(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaceOnTop called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcPlaceOnTop(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcSitDown(int newSitPosition, int xPos, int yPos, int houseXPos, int houseYPos)
	{
		if (houseXPos == -1 && houseYPos == -1)
		{
			WorldManager.Instance.onTileStatusMap[xPos, yPos] = newSitPosition;
			TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
			if ((bool)tileObject)
			{
				tileObject.tileObjectFurniture.updateOnTileStatus(xPos, yPos);
			}
			return;
		}
		HouseDetails houseInfo = HouseManager.manage.getHouseInfo(houseXPos, houseYPos);
		houseInfo.houseMapOnTileStatus[xPos, yPos] = newSitPosition;
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseXPos, houseYPos);
		if ((bool)displayPlayerHouseTiles)
		{
			TileObject tileObject2 = displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos];
			if ((bool)tileObject2)
			{
				tileObject2.tileObjectFurniture.updateOnTileStatus(xPos, yPos, houseInfo);
			}
			displayPlayerHouseTiles.refreshHouseTiles();
		}
	}

	protected static void InvokeUserCode_RpcSitDown(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSitDown called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcSitDown(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcGetUp(int sitPosition, int xPos, int yPos, int houseXPos, int houseYPos)
	{
		if (houseXPos == -1 && houseYPos == -1)
		{
			WorldManager.Instance.onTileStatusMap[xPos, yPos] = sitPosition;
			TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
			if ((bool)tileObject)
			{
				tileObject.tileObjectFurniture.updateOnTileStatus(xPos, yPos);
			}
			return;
		}
		HouseDetails houseInfo = HouseManager.manage.getHouseInfo(houseXPos, houseYPos);
		houseInfo.houseMapOnTileStatus[xPos, yPos] = sitPosition;
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseXPos, houseYPos);
		if ((bool)displayPlayerHouseTiles)
		{
			TileObject tileObject2 = displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos];
			if ((bool)tileObject2)
			{
				tileObject2.tileObjectFurniture.updateOnTileStatus(xPos, yPos, houseInfo);
			}
			displayPlayerHouseTiles.refreshHouseTiles();
		}
	}

	protected static void InvokeUserCode_RpcGetUp(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcGetUp called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcGetUp(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcEjectItemFromChanger(int xPos, int yPos)
	{
		WorldManager.Instance.onTileStatusMap[xPos, yPos] = -2;
		TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
		if ((bool)tileObject)
		{
			tileObject.tileObjectItemChanger.stopLocalProcessing();
		}
	}

	protected static void InvokeUserCode_RpcEjectItemFromChanger(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcEjectItemFromChanger called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcEjectItemFromChanger(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcEjectItemFromChangerInside(int xPos, int yPos, int houseXPos, int houseYPos)
	{
		HouseManager.manage.getHouseInfo(houseXPos, houseYPos).houseMapOnTileStatus[xPos, yPos] = -2;
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseXPos, houseYPos);
		if ((bool)displayPlayerHouseTiles)
		{
			TileObject tileObject = displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos];
			if ((bool)tileObject)
			{
				tileObject.tileObjectItemChanger.stopLocalProcessing();
			}
		}
	}

	protected static void InvokeUserCode_RpcEjectItemFromChangerInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcEjectItemFromChangerInside called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcEjectItemFromChangerInside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcOpenCloseTile(int xPos, int yPos, int newOpenClose)
	{
		if (!base.isServer && !WorldManager.Instance.clientHasRequestedChunk(xPos, yPos))
		{
			WorldManager.Instance.unlockClientTile(xPos, yPos);
			return;
		}
		WorldManager.Instance.onTileStatusMap[xPos, yPos] = newOpenClose;
		TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
		if ((bool)tileObject)
		{
			tileObject.tileOnOff.setOnOff(xPos, yPos, changedByPlayer: true);
		}
		NetworkNavMesh.nav.updateChunkInUse();
		StartCoroutine(unlockDelay(xPos, yPos));
	}

	protected static void InvokeUserCode_RpcOpenCloseTile(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOpenCloseTile called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcOpenCloseTile(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcNPCOpenGate(int xPos, int yPos, uint npcNetId)
	{
		if (!base.isServer && !WorldManager.Instance.clientHasRequestedChunk(xPos, yPos))
		{
			return;
		}
		TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
		if (NetworkIdentity.spawned.ContainsKey(npcNetId))
		{
			Transform npcTrans = NetworkIdentity.spawned[npcNetId].transform;
			if ((bool)tileObject)
			{
				StartCoroutine(fakeOpenGate(xPos, yPos, tileObject, npcTrans));
			}
		}
	}

	protected static void InvokeUserCode_RpcNPCOpenGate(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcNPCOpenGate called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcNPCOpenGate(reader.ReadInt(), reader.ReadInt(), reader.ReadUInt());
		}
	}

	protected void UserCode_RpcHarvestObject(int newStatus, int xPos, int yPos, bool spawnDrop)
	{
		if (!base.isServer && !WorldManager.Instance.clientHasRequestedChunk(xPos, yPos))
		{
			WorldManager.Instance.unlockClientTile(xPos, yPos);
			return;
		}
		WorldManager.Instance.onTileStatusMap[xPos, yPos] = newStatus;
		int num = WorldManager.Instance.onTileMap[xPos, yPos];
		if (newStatus != -1)
		{
			TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
			if ((bool)tileObject)
			{
				if (!tileObject.tileObjectGrowthStages.harvestSound)
				{
					tileObject.damage();
				}
				else
				{
					tileObject.damage(damageWithSound: false);
					SoundManager.Instance.playASoundAtPoint(tileObject.tileObjectGrowthStages.harvestSound, tileObject.transform.position);
				}
				tileObject.tileObjectGrowthStages.setStage(xPos, yPos);
				if (tileObject.tileObjectGrowthStages.mustBeInWater)
				{
					ParticleManager.manage.waterSplash(tileObject.transform.position, 15);
					SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.waterSplash, tileObject.transform.position);
				}
			}
		}
		else
		{
			TileObject tileObject2 = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
			if ((bool)tileObject2)
			{
				tileObject2.onDeath();
				_ = tileObject2.tileObjectGrowthStages.normalPickUp;
			}
			WorldManager.Instance.allObjectSettings[num].removeBeauty();
			WorldManager.Instance.onTileMap[xPos, yPos] = -1;
			WorldManager.Instance.refreshTileObjectsOnChunksInUse(xPos, yPos);
		}
		if (base.isServer && spawnDrop)
		{
			TileObject tileObjectForServerDrop = WorldManager.Instance.getTileObjectForServerDrop(num, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
			tileObjectForServerDrop.tileObjectGrowthStages.harvest(xPos, yPos);
			WorldManager.Instance.returnTileObject(tileObjectForServerDrop);
		}
		WorldManager.Instance.unlockClientTile(xPos, yPos);
	}

	protected static void InvokeUserCode_RpcHarvestObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcHarvestObject called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcHarvestObject(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadBool());
		}
	}

	protected void UserCode_RpcDigUpBuriedItemNoise(int xPos, int yPos)
	{
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.digUpBurriedItem, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
	}

	protected static void InvokeUserCode_RpcDigUpBuriedItemNoise(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDigUpBuriedItemNoise called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcDigUpBuriedItemNoise(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcThunderSound()
	{
		WeatherManager.Instance.stormMgr.PlayThunderSound();
	}

	protected static void InvokeUserCode_RpcThunderSound(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcThunderSound called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcThunderSound();
		}
	}

	protected void UserCode_RpcThunderStrike(Vector2 thunderPos)
	{
		WeatherManager.Instance.stormMgr.StrikeThunder(thunderPos);
	}

	protected static void InvokeUserCode_RpcThunderStrike(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcThunderStrike called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcThunderStrike(reader.ReadVector2());
		}
	}

	protected void UserCode_RpcActivateTrap(int xPos, int yPos)
	{
		if (!base.isServer && !WorldManager.Instance.clientHasRequestedChunk(xPos, yPos))
		{
			WorldManager.Instance.unlockClientTile(xPos, yPos);
			return;
		}
		WorldManager.Instance.onTileMap[xPos, yPos] = -1;
		WorldManager.Instance.refreshTileObjectsOnChunksInUse(xPos, yPos);
	}

	protected static void InvokeUserCode_RpcActivateTrap(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcActivateTrap called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcActivateTrap(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcClearOnTileObjectNoDrop(int xPos, int yPos)
	{
		WorldManager.Instance.onTileMap[xPos, yPos] = -1;
		WorldManager.Instance.refreshTileObjectsOnChunksInUse(xPos, yPos);
		WorldManager.Instance.onTileChunkHasChanged(xPos, yPos);
		NetworkNavMesh.nav.updateChunkInUse();
	}

	protected static void InvokeUserCode_RpcClearOnTileObjectNoDrop(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClearOnTileObjectNoDrop called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcClearOnTileObjectNoDrop(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpChangeOnTileObjectNoDrop(int newId, int xPos, int yPos)
	{
		WorldManager.Instance.onTileMap[xPos, yPos] = newId;
		WorldManager.Instance.refreshTileObjectsOnChunksInUse(xPos, yPos);
		WorldManager.Instance.onTileChunkHasChanged(xPos, yPos);
		NetworkNavMesh.nav.updateChunkInUse();
	}

	protected static void InvokeUserCode_RpChangeOnTileObjectNoDrop(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpChangeOnTileObjectNoDrop called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpChangeOnTileObjectNoDrop(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcUpdateOnTileObjectForDesync(int currentTileObject, int currentTileStatus, int xPos, int yPos)
	{
		WorldManager.Instance.onTileMap[xPos, yPos] = currentTileObject;
		WorldManager.Instance.onTileStatusMap[xPos, yPos] = currentTileStatus;
		WorldManager.Instance.unlockClientTile(xPos, yPos);
		WorldManager.Instance.onTileChunkHasChanged(xPos, yPos);
	}

	protected static void InvokeUserCode_RpcUpdateOnTileObjectForDesync(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateOnTileObjectForDesync called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcUpdateOnTileObjectForDesync(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcUpdateOnTileObject(int newTileType, int xPos, int yPos)
	{
		if (!base.isServer && !WorldManager.Instance.clientHasRequestedChunk(xPos, yPos))
		{
			WorldManager.Instance.unlockClientTile(xPos, yPos);
			return;
		}
		if (base.isServer)
		{
			ChangeAuthorityOfAllCarryObjectInProximity(xPos, yPos);
		}
		TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
		if ((bool)tileObject)
		{
			tileObject.onDeath();
		}
		if (newTileType != -1 && newTileType != 30)
		{
			if ((bool)WorldManager.Instance.allObjects[newTileType].tileObjectGrowthStages && WorldManager.Instance.allObjects[newTileType].tileObjectGrowthStages.needsTilledSoil)
			{
				SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.plantSeed, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
			}
			else
			{
				SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.placeItem, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
			}
			Vector3 position = new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2);
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], position, 5);
		}
		int num = WorldManager.Instance.onTileMap[xPos, yPos];
		if (num > -1)
		{
			WorldManager.Instance.allObjectSettings[num].removeBeauty();
			if ((bool)WorldManager.Instance.allObjects[num].tileObjectConnect && WorldManager.Instance.allObjects[num].tileObjectConnect.isFence)
			{
				WorldManager.Instance.fencedOffMap[xPos, yPos] = 0;
			}
			if ((bool)WorldManager.Instance.allObjects[num].tileObjectWritableSign)
			{
				SignManager.manage.removeSignAtPos(xPos, yPos, -1, -1);
			}
		}
		if (newTileType > -1)
		{
			WorldManager.Instance.allObjectSettings[newTileType].addBeauty();
		}
		WorldManager.Instance.onTileMap[xPos, yPos] = newTileType;
		if (base.isServer && num >= 0)
		{
			TileObject tileObjectForServerDrop = WorldManager.Instance.getTileObjectForServerDrop(num, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
			tileObjectForServerDrop.onDeathServer(xPos, yPos);
			WorldManager.Instance.returnTileObject(tileObjectForServerDrop);
		}
		checkAndSetStatusOnChange(newTileType, xPos, yPos);
		if (newTileType == -1 || newTileType == 30)
		{
			WorldManager.Instance.onTileStatusMap[xPos, yPos] = -1;
		}
		else if ((bool)WorldManager.Instance.allObjects[newTileType].tileObjectGrowthStages || (bool)WorldManager.Instance.allObjects[newTileType].tileObjectFurniture)
		{
			WorldManager.Instance.onTileStatusMap[xPos, yPos] = 0;
		}
		else if ((bool)WorldManager.Instance.allObjects[newTileType].tileObjectItemChanger)
		{
			WorldManager.Instance.onTileStatusMap[xPos, yPos] = -2;
		}
		WorldManager.Instance.refreshTileObjectsOnChunksInUse(xPos, yPos);
		if (newTileType > -1 && num == -1)
		{
			TileObject tileObject2 = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
			if ((bool)tileObject2)
			{
				tileObject2.placeDown();
			}
		}
		WorldManager.Instance.unlockClientTile(xPos, yPos);
		Instance.localChar.myInteract.ScheduleForRefreshSelection = true;
		if (base.isServer)
		{
			WorldManager.Instance.findSpaceForDropAfterTileObjectChange(xPos, yPos);
		}
		WorldManager.Instance.onTileChunkHasChanged(xPos, yPos);
		NetworkNavMesh.nav.updateChunkInUse();
		WorldManager.Instance.placeFenceInChunk(xPos, yPos);
	}

	protected static void InvokeUserCode_RpcUpdateOnTileObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateOnTileObject called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcUpdateOnTileObject(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcUpdateTileHeight(int tileHeightDif, int xPos, int yPos)
	{
		Vector3 position = new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2);
		if (WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].specialDustPart != -1)
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].specialDustPart], position);
		}
		else
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[4], position);
		}
		if (WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].onHeightChangePart != -1)
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].onHeightChangePart], position, 10);
		}
		if (tileHeightDif < 0)
		{
			SoundManager.Instance.playASoundAtPoint(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].onHeightDown, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
		}
		else
		{
			SoundManager.Instance.playASoundAtPoint(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].onHeightUp, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
		}
		WorldManager.Instance.heightMap[xPos, yPos] = Mathf.Clamp(WorldManager.Instance.heightMap[xPos, yPos] + tileHeightDif, -5, 15);
		if (base.isServer)
		{
			WorldManager.Instance.updateDropsOnTileHeight(xPos, yPos);
			if (WorldManager.Instance.heightMap[xPos, yPos] <= 0)
			{
				WorldManager.Instance.doWaterCheckOnHeightChange(xPos, yPos);
			}
			WorldManager.Instance.checkAllCarryHeight(xPos, yPos);
		}
		if (WorldManager.Instance.heightMap[xPos, yPos] > 0 && WorldManager.Instance.waterMap[xPos, yPos])
		{
			WorldManager.Instance.waterChunkHasChanged(xPos, yPos);
			WorldManager.Instance.waterMap[xPos, yPos] = false;
		}
		WorldManager.Instance.refreshAllChunksInUse(xPos, yPos);
		position += Vector3.up * tileHeightDif;
		if (WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].specialDustPart != -1)
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].specialDustPart], position);
		}
		else
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[4], position);
		}
		TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
		if ((bool)tileObject)
		{
			tileObject.transform.position = new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2);
		}
		WorldManager.Instance.unlockClientTile(xPos, yPos);
		NetworkNavMesh.nav.updateChunkInUse();
		CheckLocalCharPreventFallThroughWorld(xPos, yPos);
	}

	protected static void InvokeUserCode_RpcUpdateTileHeight(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateTileHeight called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcUpdateTileHeight(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcUpdateTileType(int newType, int xPos, int yPos)
	{
		WorldManager.Instance.tileTypeChunkHasChanged(xPos, yPos);
		if (newType == -1)
		{
			newType = 0;
		}
		if (WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].mowedVariation != newType && (bool)WorldManager.Instance.tileTypes[newType].onPutDown)
		{
			SoundManager.Instance.playASoundAtPoint(WorldManager.Instance.tileTypes[newType].onPutDown, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
		}
		if (WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].isPath)
		{
			TownManager.manage.addTownBeauty(-0.1f, TownManager.TownBeautyType.Path);
		}
		if (WorldManager.Instance.tileTypes[newType].isPath)
		{
			TownManager.manage.addTownBeauty(0.1f, TownManager.TownBeautyType.Path);
		}
		TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
		if ((bool)tileObject)
		{
			tileObject.damage(damageWithSound: false, damageParticleOn: false);
		}
		Vector3 position = new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2);
		if (WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].mowedVariation != newType)
		{
			if (WorldManager.Instance.tileTypes[newType].onChangeParticle == -1)
			{
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[4], position);
			}
			else
			{
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[WorldManager.Instance.tileTypes[newType].onChangeParticle], position, WorldManager.Instance.tileTypes[newType].changeParticleAmount);
			}
		}
		WorldManager.Instance.tileTypeMap[xPos, yPos] = newType;
		WorldManager.Instance.refreshAllChunksInUse(xPos, yPos);
		WorldManager.Instance.unlockClientTile(xPos, yPos);
	}

	protected static void InvokeUserCode_RpcUpdateTileType(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateTileType called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcUpdateTileType(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_TargetRefreshChunkAfterSent(NetworkConnection con, int chunkX, int chunkY, bool waitForOnTile, bool waitForType, bool waitForHeight, bool waitForWater)
	{
		getDelayForChunk(chunkX, chunkY)?.serverSetUp(waitForOnTile, waitForType, waitForHeight, waitForWater);
		if (!RealWorldTimeLight.time.underGround && !RealWorldTimeLight.time.offIsland)
		{
			StartCoroutine(RenderMap.Instance.updateMap());
		}
	}

	protected static void InvokeUserCode_TargetRefreshChunkAfterSent(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetRefreshChunkAfterSent called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetRefreshChunkAfterSent(NetworkClient.readyConnection, reader.ReadInt(), reader.ReadInt(), reader.ReadBool(), reader.ReadBool(), reader.ReadBool(), reader.ReadBool());
		}
	}

	protected void UserCode_TargetRefreshNotNeeded(NetworkConnection con, int chunkX, int chunkY)
	{
		chunkRequested.Remove(getDelayForChunk(chunkX, chunkY));
	}

	protected static void InvokeUserCode_TargetRefreshNotNeeded(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetRefreshNotNeeded called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetRefreshNotNeeded(NetworkClient.readyConnection, reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_TargetGiveDigUpTreasureMilestone(NetworkConnection con, int itemId)
	{
		if (itemId != -1)
		{
			PediaManager.manage.addCaughtToList(itemId);
		}
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DigUpTreasure);
	}

	protected static void InvokeUserCode_TargetGiveDigUpTreasureMilestone(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetGiveDigUpTreasureMilestone called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetGiveDigUpTreasureMilestone(NetworkClient.readyConnection, reader.ReadInt());
		}
	}

	protected void UserCode_TargetGiveBuryItemMilestone(NetworkConnection con, int itemId)
	{
		DailyTaskGenerator.generate.doATask(Inventory.Instance.allItems[itemId].assosiatedTask);
	}

	protected static void InvokeUserCode_TargetGiveBuryItemMilestone(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetGiveBuryItemMilestone called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetGiveBuryItemMilestone(NetworkClient.readyConnection, reader.ReadInt());
		}
	}

	protected void UserCode_TargetGiveHuntingXp(NetworkConnection con, int animalId, int variation)
	{
		CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Hunting, Mathf.Clamp(AnimalManager.manage.allAnimals[animalId].dangerValue / 80, 1, 100));
		if ((bool)AnimalManager.manage.allAnimals[animalId].GetComponent<SaveAlphaAnimal>())
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.HuntAlphaAnimal);
		}
		else
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.HuntAnimals);
		}
	}

	protected static void InvokeUserCode_TargetGiveHuntingXp(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetGiveHuntingXp called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetGiveHuntingXp(NetworkClient.readyConnection, reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcCheckHuntingTaskCompletion(int animalId, Vector3 killPos)
	{
		BulletinBoard.board.checkAllMissionsForAnimalKill(animalId, killPos);
	}

	protected static void InvokeUserCode_RpcCheckHuntingTaskCompletion(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCheckHuntingTaskCompletion called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcCheckHuntingTaskCompletion(reader.ReadInt(), reader.ReadVector3());
		}
	}

	protected void UserCode_TargetGiveHarvestMilestone(NetworkConnection con, int tileObjectGiving)
	{
		if (tileObjectGiving > 0 && (bool)WorldManager.Instance.allObjects[tileObjectGiving].tileObjectGrowthStages)
		{
			DailyTaskGenerator.generate.doATask(WorldManager.Instance.allObjects[tileObjectGiving].tileObjectGrowthStages.milestoneOnHarvest);
		}
	}

	protected static void InvokeUserCode_TargetGiveHarvestMilestone(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetGiveHarvestMilestone called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetGiveHarvestMilestone(NetworkClient.readyConnection, reader.ReadInt());
		}
	}

	protected void UserCode_TargetGiveSignDetailsForChunk(NetworkConnection con, SignDetails[] signsInChunk)
	{
		for (int i = 0; i < signsInChunk.Length; i++)
		{
			SignManager.manage.changeSignDetails(signsInChunk[i].xPos, signsInChunk[i].yPos, -1, -1, signsInChunk[i].signSays);
		}
	}

	protected static void InvokeUserCode_TargetGiveSignDetailsForChunk(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetGiveSignDetailsForChunk called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetGiveSignDetailsForChunk(NetworkClient.readyConnection, GeneratedNetworkCode._Read_SignDetails_005B_005D(reader));
		}
	}

	protected void UserCode_TargetGiveSignDetailsForHouse(NetworkConnection con, SignDetails[] signsInChunk, int houseX, int houseY)
	{
		for (int i = 0; i < signsInChunk.Length; i++)
		{
			SignManager.manage.changeSignDetails(signsInChunk[i].xPos, signsInChunk[i].yPos, houseX, houseY, signsInChunk[i].signSays);
		}
	}

	protected static void InvokeUserCode_TargetGiveSignDetailsForHouse(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetGiveSignDetailsForHouse called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetGiveSignDetailsForHouse(NetworkClient.readyConnection, GeneratedNetworkCode._Read_SignDetails_005B_005D(reader), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_TargetGiveChunkWaterDetails(NetworkConnection con, int chunkX, int chunkY, bool[] waterDetails)
	{
		WorldManager.Instance.fillWaterChunkDetails(chunkX, chunkY, waterDetails);
		WorldManager.Instance.changedMapWater[chunkX / 10, chunkY / 10] = true;
		WorldManager.Instance.chunkChangedMap[chunkX / 10, chunkY / 10] = true;
		if (getDelayForChunk(chunkX, chunkY) != null)
		{
			getDelayForChunk(chunkX, chunkY).waterGiven();
		}
	}

	protected static void InvokeUserCode_TargetGiveChunkWaterDetails(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetGiveChunkWaterDetails called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetGiveChunkWaterDetails(NetworkClient.readyConnection, reader.ReadInt(), reader.ReadInt(), GeneratedNetworkCode._Read_System_002EBoolean_005B_005D(reader));
		}
	}

	protected void UserCode_TargetGiveChunkOnTileDetails(NetworkConnection con, int chunkX, int chunkY, int[] onTileDetails, int[] otherDetails)
	{
		WorldManager.Instance.fillOnTileChunkDetails(chunkX, chunkY, onTileDetails, otherDetails);
		WorldManager.Instance.changedMapOnTile[chunkX / 10, chunkY / 10] = true;
		WorldManager.Instance.chunkChangedMap[chunkX / 10, chunkY / 10] = true;
		if (getDelayForChunk(chunkX, chunkY) != null)
		{
			getDelayForChunk(chunkX, chunkY).ontileGiven();
		}
	}

	protected static void InvokeUserCode_TargetGiveChunkOnTileDetails(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetGiveChunkOnTileDetails called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetGiveChunkOnTileDetails(NetworkClient.readyConnection, reader.ReadInt(), reader.ReadInt(), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader));
		}
	}

	protected void UserCode_TargetGiveChunkOnTopDetails(NetworkConnection con, ItemOnTop[] onTopInThisChunk)
	{
		for (int i = 0; i < onTopInThisChunk.Length; i++)
		{
			ItemOnTopManager.manage.addOnTopObject(onTopInThisChunk[i]);
			TileObject tileObject = WorldManager.Instance.findTileObjectInUse(onTopInThisChunk[i].sittingOnX, onTopInThisChunk[i].sittingOnY);
			if ((bool)tileObject)
			{
				tileObject.setXAndY(onTopInThisChunk[i].sittingOnX, onTopInThisChunk[i].sittingOnY);
			}
		}
	}

	protected static void InvokeUserCode_TargetGiveChunkOnTopDetails(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetGiveChunkOnTopDetails called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetGiveChunkOnTopDetails(NetworkClient.readyConnection, GeneratedNetworkCode._Read_ItemOnTop_005B_005D(reader));
		}
	}

	protected void UserCode_TargetGiveChunkTileTypeDetails(NetworkConnection con, int chunkX, int chunkY, int[] tileTypeDetails)
	{
		WorldManager.Instance.fillTileTypeChunkDetails(chunkX, chunkY, tileTypeDetails);
		WorldManager.Instance.changedMapTileType[chunkX / 10, chunkY / 10] = true;
		WorldManager.Instance.chunkChangedMap[chunkX / 10, chunkY / 10] = true;
		if (getDelayForChunk(chunkX, chunkY) != null)
		{
			getDelayForChunk(chunkX, chunkY).typeGiven();
		}
	}

	protected static void InvokeUserCode_TargetGiveChunkTileTypeDetails(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetGiveChunkTileTypeDetails called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetGiveChunkTileTypeDetails(NetworkClient.readyConnection, reader.ReadInt(), reader.ReadInt(), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader));
		}
	}

	protected void UserCode_TargetRequestShopStall(NetworkConnection con, bool[] stallDetails)
	{
		ShopManager.manage.fillStallsFromRequest(stallDetails);
	}

	protected static void InvokeUserCode_TargetRequestShopStall(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetRequestShopStall called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetRequestShopStall(NetworkClient.readyConnection, GeneratedNetworkCode._Read_System_002EBoolean_005B_005D(reader));
		}
	}

	protected void UserCode_TargetGiveChunkHeightDetails(NetworkConnection con, int chunkX, int chunkY, int[] heightDetails)
	{
		WorldManager.Instance.fillHeightChunkDetails(chunkX, chunkY, heightDetails);
		WorldManager.Instance.changedMapHeight[chunkX / 10, chunkY / 10] = true;
		WorldManager.Instance.chunkChangedMap[chunkX / 10, chunkY / 10] = true;
		if (getDelayForChunk(chunkX, chunkY) != null)
		{
			getDelayForChunk(chunkX, chunkY).heightGiven();
		}
	}

	protected static void InvokeUserCode_TargetGiveChunkHeightDetails(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetGiveChunkHeightDetails called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetGiveChunkHeightDetails(NetworkClient.readyConnection, reader.ReadInt(), reader.ReadInt(), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader));
		}
	}

	protected void UserCode_TargetRequestMuseum(NetworkConnection con, bool[] fishDonated, bool[] bugsDonated, bool[] underWaterCreatesDonated)
	{
		MuseumManager.manage.fishDonated = fishDonated;
		MuseumManager.manage.bugsDonated = bugsDonated;
		MuseumManager.manage.underWaterCreaturesDonated = underWaterCreatesDonated;
		if ((bool)MuseumDisplay.display)
		{
			MuseumDisplay.display.updateExhibits();
		}
		MuseumManager.manage.clientNeedsToRequest = false;
	}

	protected static void InvokeUserCode_TargetRequestMuseum(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetRequestMuseum called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetRequestMuseum(NetworkClient.readyConnection, GeneratedNetworkCode._Read_System_002EBoolean_005B_005D(reader), GeneratedNetworkCode._Read_System_002EBoolean_005B_005D(reader), GeneratedNetworkCode._Read_System_002EBoolean_005B_005D(reader));
		}
	}

	protected void UserCode_RpcPlaceMultiTiledObject(int multiTiledObjectId, int xPos, int yPos, int rotation)
	{
		WorldManager.Instance.allObjects[multiTiledObjectId].placeMultiTiledObject(xPos, yPos, rotation);
		checkAndSetStatusOnChange(multiTiledObjectId, xPos, yPos);
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.placeItem, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
		WorldManager.Instance.refreshChunksInChunksToRefreshList();
		NetworkNavMesh.nav.updateChunkInUse();
		WorldManager.Instance.unlockClientTile(xPos, yPos);
		if (multiTiledObjectId > -1)
		{
			TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
			if ((bool)tileObject)
			{
				tileObject.placeDown();
			}
		}
	}

	protected static void InvokeUserCode_RpcPlaceMultiTiledObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaceMultiTiledObject called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcPlaceMultiTiledObject(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcPlaceMultiTiledObjectPlaceUnder(int multiTiledObjectId, int xPos, int yPos, int rotation)
	{
		WorldManager.Instance.allObjects[multiTiledObjectId].placeMultiTiledObjectPlaceUnder(xPos, yPos, rotation);
		checkAndSetStatusOnChange(multiTiledObjectId, xPos, yPos);
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.placeItem, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
		WorldManager.Instance.refreshChunksInChunksToRefreshList();
		NetworkNavMesh.nav.updateChunkInUse();
		WorldManager.Instance.unlockClientTile(xPos, yPos);
		if (multiTiledObjectId > -1)
		{
			TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
			if ((bool)tileObject)
			{
				tileObject.placeDown();
			}
		}
	}

	protected static void InvokeUserCode_RpcPlaceMultiTiledObjectPlaceUnder(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaceMultiTiledObjectPlaceUnder called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcPlaceMultiTiledObjectPlaceUnder(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcPlaceBridgeTiledObject(int multiTiledObjectId, int xPos, int yPos, int rotation, int length)
	{
		WorldManager.Instance.allObjects[multiTiledObjectId].placeBridgeTiledObject(xPos, yPos, rotation, length);
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.placeItem, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
		WorldManager.Instance.refreshChunksInChunksToRefreshList();
		NetworkNavMesh.nav.updateChunkInUse();
		WorldManager.Instance.unlockClientTile(xPos, yPos);
	}

	protected static void InvokeUserCode_RpcPlaceBridgeTiledObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaceBridgeTiledObject called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcPlaceBridgeTiledObject(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcAddNewTaskToClientBoard(PostOnBoard newPost)
	{
		if (!base.isServer)
		{
			BulletinBoard.board.attachedPosts.Add(newPost);
			newPost.getTemplateAndAddToList();
		}
	}

	protected static void InvokeUserCode_RpcAddNewTaskToClientBoard(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAddNewTaskToClientBoard called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcAddNewTaskToClientBoard(GeneratedNetworkCode._Read_PostOnBoard(reader));
		}
	}

	protected void UserCode_RpcFillVillagerDetails(uint netId, int npcId, bool gen, int nameId, int skinId, int hairId, int hairColourId, int eyeId, int eyeColourId, int shirtId, int pantsId, int shoesId)
	{
		NPCManager.manage.npcInvs[npcId].fillAppearanceInv(gen, nameId, skinId, hairId, hairColourId, eyeId, eyeColourId, shirtId, pantsId, shoesId);
		NPCManager.manage.npcInvs[npcId].hasBeenRequested = true;
		if (netId != 0 && (bool)NetworkIdentity.spawned[netId])
		{
			NetworkIdentity.spawned[netId].GetComponent<NPCIdentity>().changeNPCAndEquip(npcId);
		}
	}

	protected static void InvokeUserCode_RpcFillVillagerDetails(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcFillVillagerDetails called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcFillVillagerDetails(reader.ReadUInt(), reader.ReadInt(), reader.ReadBool(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_TargetSyncBulletinBoardPosts(NetworkConnection conn, PostOnBoard[] allPosts)
	{
		BulletinBoard.board.onLocalConnect();
		for (int i = 0; i < allPosts.Length; i++)
		{
			BulletinBoard.board.attachedPosts.Add(allPosts[i]);
			allPosts[i].getTemplateAndAddToList();
		}
		BulletinBoard.board.openWindow();
	}

	protected static void InvokeUserCode_TargetSyncBulletinBoardPosts(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetSyncBulletinBoardPosts called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetSyncBulletinBoardPosts(NetworkClient.readyConnection, GeneratedNetworkCode._Read_PostOnBoard_005B_005D(reader));
		}
	}

	protected void UserCode_TargetGiveStamina(NetworkConnection conn)
	{
		StatusManager.manage.changeStamina(StatusManager.manage.getStaminaMax());
		StatusManager.manage.getRevivedByOtherChar();
	}

	protected static void InvokeUserCode_TargetGiveStamina(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetGiveStamina called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetGiveStamina(NetworkClient.readyConnection);
		}
	}

	protected void UserCode_TargetSendBugCompLetter(NetworkConnection conn, int position)
	{
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.PlaceInBugComp);
		Letter letter = new Letter(13, Letter.LetterType.BugCompWin, CatchingCompetitionManager.manage.bugCompTrophies[position].getItemId(), 1);
		letter.letterTemplateNo = position;
		MailManager.manage.mailInBox.Add(letter);
	}

	protected static void InvokeUserCode_TargetSendBugCompLetter(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetSendBugCompLetter called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetSendBugCompLetter(NetworkClient.readyConnection, reader.ReadInt());
		}
	}

	protected void UserCode_TargetSendFishingCompLetter(NetworkConnection conn, int position)
	{
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.PlaceInFishingComp);
		Letter letter = new Letter(14, Letter.LetterType.FishingCompWin, CatchingCompetitionManager.manage.fishCompTrophies[position].getItemId(), 1);
		letter.letterTemplateNo = position;
		MailManager.manage.mailInBox.Add(letter);
	}

	protected static void InvokeUserCode_TargetSendFishingCompLetter(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetSendFishingCompLetter called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetSendFishingCompLetter(NetworkClient.readyConnection, reader.ReadInt());
		}
	}

	protected void UserCode_RpcSetRotation(int xPos, int yPos, int rotation)
	{
		WorldManager.Instance.rotationMap[xPos, yPos] = rotation;
		TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
		if ((bool)tileObject)
		{
			tileObject.getRotation(xPos, yPos);
		}
	}

	protected static void InvokeUserCode_RpcSetRotation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetRotation called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcSetRotation(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_TargetGetRotationForTile(NetworkConnection con, int xPos, int yPos, int rotation)
	{
		WorldManager.Instance.rotationMap[xPos, yPos] = rotation;
		TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
		WorldManager.Instance.unlockClientTile(xPos, yPos);
		if ((bool)tileObject)
		{
			tileObject.transform.position = new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2);
			tileObject.getRotation(xPos, yPos);
			if ((bool)tileObject.displayPlayerHouseTiles)
			{
				tileObject.displayPlayerHouseTiles.setInteriorPosAndRotation(xPos, yPos);
			}
		}
		else
		{
			WorldManager.Instance.refreshTileObjectsOnChunksInUse(xPos, yPos);
		}
	}

	protected static void InvokeUserCode_TargetGetRotationForTile(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetGetRotationForTile called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetGetRotationForTile(NetworkClient.readyConnection, reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcDeliverAnimal(uint deliveredBy, int animalDelivered, int variationDelivered, int rewardToSend, int trapType)
	{
		if (!NetworkIdentity.spawned.ContainsKey(deliveredBy))
		{
			return;
		}
		CharMovement component = NetworkIdentity.spawned[deliveredBy].GetComponent<CharMovement>();
		string arg = AnimalManager.manage.allAnimals[animalDelivered].GetAnimalName();
		if (variationDelivered != 0)
		{
			arg = AnimalManager.manage.allAnimals[animalDelivered].GetAnimalVariationAdjective(variationDelivered) + " " + AnimalManager.manage.allAnimals[animalDelivered].GetAnimalName();
		}
		NotificationManager.manage.createChatNotification(string.Format(ConversationGenerator.generate.GetToolTip("Tip_PlayerDeliveredAnimal"), component.GetComponent<EquipItemToChar>().playerName, arg));
		if (!component.isLocalPlayer)
		{
			return;
		}
		if (animalDelivered == 29)
		{
			MailManager.manage.sendAChrissyAnimalCapturedLetter(trapType);
		}
		else
		{
			MailManager.manage.sendAnAnimalCapturedLetter(rewardToSend, trapType);
		}
		Damageable component2 = AnimalManager.manage.allAnimals[animalDelivered].GetComponent<Damageable>();
		if (!component2 || !component2.guaranteedDrops)
		{
			return;
		}
		for (int i = 0; (float)i <= 8f; i++)
		{
			InventoryItem randomDropFromTable = component2.guaranteedDrops.getRandomDropFromTable();
			if (randomDropFromTable != null)
			{
				MailManager.manage.SendASpecialDropAnimalCapturedLetter(randomDropFromTable.getItemId());
				break;
			}
		}
	}

	protected static void InvokeUserCode_RpcDeliverAnimal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDeliverAnimal called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcDeliverAnimal(reader.ReadUInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcSellByWeight(uint deliveredBy, uint itemDelivered, int keeperId)
	{
		if (deliveredBy == 0)
		{
			return;
		}
		SellByWeight componentInParent = NetworkIdentity.spawned[itemDelivered].GetComponentInParent<SellByWeight>();
		if (!componentInParent.hasAuthority)
		{
			return;
		}
		CharMovement component = NetworkIdentity.spawned[deliveredBy].GetComponent<CharMovement>();
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.placeInScaleSound, componentInParent.transform.position);
		if (component.isLocalPlayer)
		{
			GiveNPC.give.setSellingByWeight(componentInParent);
			if ((bool)NPCManager.manage.getVendorNPC((NPCSchedual.Locations)keeperId) && NPCManager.manage.getVendorNPC((NPCSchedual.Locations)keeperId).isAtWork())
			{
				StartCoroutine(waitForShopKeeperToBeReady(keeperId, componentInParent));
			}
		}
	}

	protected static void InvokeUserCode_RpcSellByWeight(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSellByWeight called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcSellByWeight(reader.ReadUInt(), reader.ReadUInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcClearHouseForMove(int xPos, int yPos)
	{
		TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
		if ((bool)tileObject && (bool)tileObject.displayPlayerHouseTiles)
		{
			tileObject.displayPlayerHouseTiles.clearForUpgrade();
		}
	}

	protected static void InvokeUserCode_RpcClearHouseForMove(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClearHouseForMove called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcClearHouseForMove(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcPickUpContainerObject(int xPos, int yPos)
	{
		int num = WorldManager.Instance.onTileMap[xPos, yPos];
		if (num >= 0 && WorldManager.Instance.allObjects[num].IsMultiTileObject())
		{
			WorldManager.Instance.allObjects[num].removeMultiTiledObject(xPos, yPos, WorldManager.Instance.rotationMap[xPos, yPos]);
		}
		else if (num >= 0 && !WorldManager.Instance.allObjects[num].IsMultiTileObject())
		{
			WorldManager.Instance.onTileMap[xPos, yPos] = -1;
		}
		SignManager.manage.removeSignAtPos(xPos, yPos, -1, -1);
		WorldManager.Instance.onTileStatusMap[xPos, yPos] = 0;
		WorldManager.Instance.unlockClientTile(xPos, yPos);
		WorldManager.Instance.refreshTileObjectsOnChunksInUse(xPos, yPos);
	}

	protected static void InvokeUserCode_RpcPickUpContainerObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPickUpContainerObject called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcPickUpContainerObject(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcPickUpContainerObjectInside(int xPos, int yPos, int houseX, int houseY)
	{
		HouseDetails houseInfo = HouseManager.manage.getHouseInfo(houseX, houseY);
		_ = houseInfo.houseMapOnTile[xPos, yPos];
		if (houseInfo.houseMapOnTile[xPos, yPos] != -1 && WorldManager.Instance.allObjects[houseInfo.houseMapOnTile[xPos, yPos]].IsMultiTileObject())
		{
			WorldManager.Instance.allObjects[houseInfo.houseMapOnTile[xPos, yPos]].removeMultiTiledObjectInside(xPos, yPos, houseInfo.houseMapRotation[xPos, yPos], houseInfo);
		}
		SignManager.manage.removeSignAtPos(xPos, yPos, houseX, houseY);
		houseInfo.houseMapOnTile[xPos, yPos] = -1;
		houseInfo.houseMapOnTileStatus[xPos, yPos] = -1;
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseX, houseY);
		if ((bool)displayPlayerHouseTiles)
		{
			displayPlayerHouseTiles.refreshHouseTiles();
		}
		WorldManager.Instance.unlockClientTileHouse(xPos, yPos, houseX, houseY);
	}

	protected static void InvokeUserCode_RpcPickUpContainerObjectInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPickUpContainerObjectInside called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcPickUpContainerObjectInside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcRemoveMultiTiledObject(int multiTiledObjectId, int xPos, int yPos, int rotationRemove)
	{
		WorldManager.Instance.allObjects[multiTiledObjectId].removeMultiTiledObject(xPos, yPos, rotationRemove);
		if ((bool)WorldManager.Instance.allObjects[multiTiledObjectId].tileObjectWritableSign)
		{
			SignManager.manage.removeSignAtPos(xPos, yPos, -1, -1);
		}
		if (base.isServer && multiTiledObjectId >= 0)
		{
			if (base.isServer && multiTiledObjectId == 302 && WorldManager.Instance.onTileStatusMap[xPos, yPos] > 0)
			{
				spawnAServerDrop(WorldManager.Instance.allObjects[multiTiledObjectId].GetComponentInChildren<ConstructionBoxInput>().canBeFilledWith.getItemId(), WorldManager.Instance.onTileStatusMap[xPos, yPos], new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
			}
			TileObject tileObjectForServerDrop = WorldManager.Instance.getTileObjectForServerDrop(multiTiledObjectId, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
			tileObjectForServerDrop.onDeathServer(xPos, yPos);
			WorldManager.Instance.returnTileObject(tileObjectForServerDrop);
			if ((bool)WorldManager.Instance.allObjects[multiTiledObjectId].tileObjectFurniture)
			{
				WorldManager.Instance.onTileStatusMap[xPos, yPos] = -1;
			}
		}
		checkAndSetStatusOnChange(-1, xPos, yPos);
		WorldManager.Instance.unlockClientTile(xPos, yPos);
		WorldManager.Instance.refreshTileObjectsOnChunksInUse(xPos, yPos);
		NetworkNavMesh.nav.updateChunkInUse();
	}

	protected static void InvokeUserCode_RpcRemoveMultiTiledObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRemoveMultiTiledObject called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcRemoveMultiTiledObject(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcPlaceItemOnToTileObject(int give, int xPos, int yPos)
	{
		WorldManager.Instance.onTileStatusMap[xPos, yPos] = give;
		TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.placeItem, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
		if ((bool)tileObject && (bool)tileObject.showObjectOnStatusChange)
		{
			tileObject.showObjectOnStatusChange.showGameObject(xPos, yPos);
		}
		WorldManager.Instance.unlockClientTile(xPos, yPos);
		if (WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].statusObjectsPickUpFirst.Length != 0 && (bool)WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].statusObjectsPickUpFirst[give] && (bool)WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].statusObjectsPickUpFirst[give].placeable)
		{
			WorldManager.Instance.allObjectSettings[WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].statusObjectsPickUpFirst[give].placeable.tileObjectId].addBeauty();
		}
	}

	protected static void InvokeUserCode_RpcPlaceItemOnToTileObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaceItemOnToTileObject called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcPlaceItemOnToTileObject(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcGiveOnTileStatus(int give, int xPos, int yPos)
	{
		WorldManager.Instance.onTileStatusMap[xPos, yPos] = give;
		TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
		if ((bool)tileObject)
		{
			if ((bool)tileObject.showObjectOnStatusChange)
			{
				tileObject.showObjectOnStatusChange.showGameObject(xPos, yPos);
			}
			if ((bool)tileObject.tileObjectGrowthStages)
			{
				tileObject.tileObjectGrowthStages.setStage(xPos, yPos);
			}
			if ((bool)tileObject.tileObjectItemChanger)
			{
				tileObject.tileObjectItemChanger.mapUpdatePos(xPos, yPos);
			}
			if ((bool)tileObject.tileOnOff)
			{
				tileObject.tileOnOff.setOnOff(xPos, yPos);
			}
			if ((bool)tileObject.tileObjectConnect)
			{
				tileObject.tileObjectConnect.connectToTiles(xPos, yPos);
			}
		}
		WorldManager.Instance.onTileChunkHasChanged(xPos, yPos);
		WorldManager.Instance.unlockClientTile(xPos, yPos);
	}

	protected static void InvokeUserCode_RpcGiveOnTileStatus(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcGiveOnTileStatus called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcGiveOnTileStatus(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcUseInstagrow(int xPos, int yPos)
	{
		WorldManager.Instance.unlockClientTile(xPos, yPos);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[36], new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2), 10);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2), 3);
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.placeInstagrow, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
	}

	protected static void InvokeUserCode_RpcUseInstagrow(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUseInstagrow called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcUseInstagrow(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcGiveOnTileStatusInside(int give, int xPos, int yPos, int houseX, int houseY)
	{
		MonoBehaviour.print("Position " + xPos + ", " + yPos + " inside house " + houseX + ", " + houseY + " had its status changed to " + give);
		HouseManager.manage.getHouseInfo(houseX, houseY).houseMapOnTileStatus[xPos, yPos] = give;
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseX, houseY);
		if ((bool)displayPlayerHouseTiles)
		{
			displayPlayerHouseTiles.refreshHouseTiles();
		}
	}

	protected static void InvokeUserCode_RpcGiveOnTileStatusInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcGiveOnTileStatusInside called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcGiveOnTileStatusInside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcCompleteBulletinBoard(int id)
	{
		if (id < BulletinBoard.board.attachedPosts.Count)
		{
			BulletinBoard.board.attachedPosts[id].completeTask(null);
			BulletinBoard.board.showSelectedPost();
			BulletinBoard.board.updateTaskButtons();
		}
	}

	protected static void InvokeUserCode_RpcCompleteBulletinBoard(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCompleteBulletinBoard called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcCompleteBulletinBoard(reader.ReadInt());
		}
	}

	protected void UserCode_RpcShowOffBuilding(int xPos, int yPos)
	{
		CameraController.control.showOffPos(xPos, yPos);
	}

	protected static void InvokeUserCode_RpcShowOffBuilding(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowOffBuilding called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcShowOffBuilding(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcSyncDate(int day, int week, int month, int year, int currentMinute)
	{
		if (!base.isServer)
		{
			WorldManager.Instance.day = day;
			WorldManager.Instance.week = week;
			WorldManager.Instance.month = month;
			WorldManager.Instance.year = year;
			RealWorldTimeLight.time.setUpDayAndDate();
			RealWorldTimeLight.time.currentMinute = currentMinute;
			TownEventManager.manage.checkForTownEventAndSetUp(WorldManager.Instance.day, WorldManager.Instance.week, WorldManager.Instance.month);
			ScheduleManager.manage.giveNpcsNewDaySchedual(day, week, month);
		}
	}

	protected static void InvokeUserCode_RpcSyncDate(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSyncDate called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcSyncDate(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcMakeAWish(string wishersName, int newWish, Vector3 PartPos)
	{
		NotificationManager.manage.makeTopNotification(string.Format(ConversationGenerator.generate.GetNotificationText("MadeAWish"), wishersName), ConversationGenerator.generate.GetNotificationText("WhatWillHappenTomorrow"), SoundManager.Instance.wishMadeSound);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.wishingWellPart, PartPos, 50);
	}

	protected static void InvokeUserCode_RpcMakeAWish(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcMakeAWish called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcMakeAWish(reader.ReadString(), reader.ReadInt(), reader.ReadVector3());
		}
	}

	protected void UserCode_RpcAddADay(int newMineSeed)
	{
		sleeping = true;
		StartCoroutine(nextDayDelay(newMineSeed));
	}

	protected static void InvokeUserCode_RpcAddADay(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAddADay called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcAddADay(reader.ReadInt());
		}
	}

	protected void UserCode_TargetRequestHouse(NetworkConnection con, int xPos, int yPos, int[] onTile, int[] onTileStatus, int[] onTileRotation, int wall, int floor, ItemOnTop[] onTopItems)
	{
		HouseDetails houseInfoForClientFill = HouseManager.manage.getHouseInfoForClientFill(xPos, yPos);
		houseInfoForClientFill.houseMapOnTile = WorldManager.Instance.fillHouseDetailsArray(onTile);
		houseInfoForClientFill.houseMapOnTileStatus = WorldManager.Instance.fillHouseDetailsArray(onTileStatus);
		houseInfoForClientFill.houseMapRotation = WorldManager.Instance.fillHouseDetailsArray(onTileRotation);
		houseInfoForClientFill.wall = wall;
		houseInfoForClientFill.floor = floor;
		for (int i = 0; i < onTopItems.Length; i++)
		{
			ItemOnTopManager.manage.addOnTopObject(onTopItems[i]);
		}
		HouseManager.manage.findHousesOnDisplay(xPos, yPos).refreshHouseTiles(firstTime: true);
	}

	protected static void InvokeUserCode_TargetRequestHouse(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetRequestHouse called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetRequestHouse(NetworkClient.readyConnection, reader.ReadInt(), reader.ReadInt(), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader), reader.ReadInt(), reader.ReadInt(), GeneratedNetworkCode._Read_ItemOnTop_005B_005D(reader));
		}
	}

	protected void UserCode_TargetRequestExterior(NetworkConnection con, int xPos, int yPos, int houseBase, int roof, int windows, int door, int wallMat, string wallColor, int houseMat, string houseColor, int roofMat, string roofColor, int fenceId, string buildingName)
	{
		HouseExterior houseInfoForClientExterior = HouseManager.manage.getHouseInfoForClientExterior(xPos, yPos);
		houseInfoForClientExterior.houseBase = houseBase;
		houseInfoForClientExterior.roof = roof;
		houseInfoForClientExterior.windows = windows;
		houseInfoForClientExterior.door = door;
		houseInfoForClientExterior.wallMat = wallMat;
		houseInfoForClientExterior.wallColor = wallColor;
		houseInfoForClientExterior.roofMat = roofMat;
		houseInfoForClientExterior.roofColor = roofColor;
		houseInfoForClientExterior.houseMat = houseMat;
		houseInfoForClientExterior.houseColor = houseColor;
		houseInfoForClientExterior.fence = fenceId;
		houseInfoForClientExterior.houseName = buildingName;
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(xPos, yPos);
		if ((bool)displayPlayerHouseTiles)
		{
			displayPlayerHouseTiles.updateHouseExterior();
		}
		RenderMap.Instance.UpdateIconName(xPos, yPos, buildingName);
	}

	protected static void InvokeUserCode_TargetRequestExterior(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetRequestExterior called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetRequestExterior(NetworkClient.readyConnection, reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadString(), reader.ReadInt(), reader.ReadString(), reader.ReadInt(), reader.ReadString(), reader.ReadInt(), reader.ReadString());
		}
	}

	protected void UserCode_RpcGiveOnTopStatus(int newStatus, int xPos, int yPos, int onTopPos, int houseX, int houseY)
	{
		if (houseX == -1 && houseY == -1)
		{
			ItemOnTop itemOnTopInPosition = ItemOnTopManager.manage.getItemOnTopInPosition(onTopPos, xPos, yPos, null);
			if (itemOnTopInPosition != null)
			{
				itemOnTopInPosition.itemStatus = newStatus;
				TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
				if ((bool)tileObject)
				{
					tileObject.checkOnTopInside(xPos, yPos, null);
				}
			}
			return;
		}
		HouseDetails houseInfoIfExists = HouseManager.manage.getHouseInfoIfExists(houseX, houseY);
		if (houseInfoIfExists == null)
		{
			return;
		}
		ItemOnTop itemOnTopInPosition2 = ItemOnTopManager.manage.getItemOnTopInPosition(onTopPos, xPos, yPos, houseInfoIfExists);
		if (itemOnTopInPosition2 != null)
		{
			itemOnTopInPosition2.itemStatus = newStatus;
			MonoBehaviour.print(itemOnTopInPosition2.getTileObjectId() + "on top of " + xPos + "," + yPos + "was updated with the tile status of " + newStatus);
			DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseX, houseY);
			if ((bool)displayPlayerHouseTiles)
			{
				displayPlayerHouseTiles.refreshHouseTiles();
			}
		}
	}

	protected static void InvokeUserCode_RpcGiveOnTopStatus(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcGiveOnTopStatus called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcGiveOnTopStatus(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcFillWithWater(int xPos, int yPos)
	{
		WorldManager.Instance.waterMap[xPos, yPos] = true;
		WorldManager.Instance.waterChunkHasChanged(xPos, yPos);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[18], new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2), 75);
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.treadWater, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
		WorldManager.Instance.refreshAllChunksInUse(xPos, yPos);
	}

	protected static void InvokeUserCode_RpcFillWithWater(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcFillWithWater called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcFillWithWater(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcStallSold(int stallTypeId, int shopStallNo)
	{
		ShopManager.manage.sellStall(stallTypeId, shopStallNo);
	}

	protected static void InvokeUserCode_RpcStallSold(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStallSold called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcStallSold(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcSpinChair()
	{
		if ((bool)HairDresserSeat.seat)
		{
			HairDresserSeat.seat.spinChair();
		}
	}

	protected static void InvokeUserCode_RpcSpinChair(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSpinChair called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcSpinChair();
		}
	}

	protected void UserCode_RpcUpgradeHouse(int newHouseId, int houseXPos, int houseYPos)
	{
		TileObject tileObject = WorldManager.Instance.findTileObjectInUse(houseXPos, houseYPos);
		if ((bool)tileObject)
		{
			tileObject.displayPlayerHouseTiles.clearForUpgrade();
			WorldManager.Instance.returnTileObject(tileObject);
		}
		else
		{
			DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseXPos, houseYPos);
			if ((bool)displayPlayerHouseTiles)
			{
				displayPlayerHouseTiles.clearForUpgrade();
			}
		}
		WorldManager.Instance.onTileMap[houseXPos, houseYPos] = newHouseId;
		WorldManager.Instance.onTileStatusMap[houseXPos, houseYPos] = 0;
		HouseManager.manage.getHouseInfo(houseXPos, houseYPos).upgradeHouseSize();
		WorldManager.Instance.refreshTileObjectsOnChunksInUse(houseXPos, houseYPos);
		if (base.isServer)
		{
			requestInterior(houseXPos, houseYPos);
		}
		if ((bool)localChar)
		{
			localChar.myInteract.ForceRequestHouse();
		}
	}

	protected static void InvokeUserCode_RpcUpgradeHouse(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpgradeHouse called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcUpgradeHouse(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcChangeHouseOnTile(int newTileType, int xPos, int yPos, int rotation, int houseX, int houseY)
	{
		HouseDetails houseInfo = HouseManager.manage.getHouseInfo(houseX, houseY);
		int num = houseInfo.houseMapOnTile[xPos, yPos];
		if (num > -1 && (bool)WorldManager.Instance.allObjects[num].tileObjectWritableSign)
		{
			SignManager.manage.removeSignAtPos(xPos, yPos, houseX, houseY);
		}
		if (houseInfo.houseMapOnTile[xPos, yPos] != -1 && WorldManager.Instance.allObjects[houseInfo.houseMapOnTile[xPos, yPos]].IsMultiTileObject())
		{
			WorldManager.Instance.allObjects[houseInfo.houseMapOnTile[xPos, yPos]].removeMultiTiledObjectInside(xPos, yPos, houseInfo.houseMapRotation[xPos, yPos], houseInfo);
		}
		if (newTileType != -1 && newTileType != 30)
		{
			SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.placeItem, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
		}
		if (newTileType != -1 && WorldManager.Instance.allObjects[newTileType].IsMultiTileObject())
		{
			WorldManager.Instance.allObjects[newTileType].PlaceMultiTiledObjectInside(xPos, yPos, rotation, houseInfo);
		}
		else
		{
			houseInfo.houseMapOnTile[xPos, yPos] = newTileType;
			if (newTileType > -1 && WorldManager.Instance.allObjects[newTileType].GetRotationFromMap())
			{
				houseInfo.houseMapRotation[xPos, yPos] = rotation;
			}
		}
		if (base.isServer && newTileType == -1 && num >= 0 && ((bool)WorldManager.Instance.allObjectSettings[num].dropsItemOnDeath || WorldManager.Instance.allObjectSettings[num].dropsStatusNumberOnDeath || (bool)WorldManager.Instance.allObjectSettings[num].dropFromLootTable))
		{
			DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseX, houseY);
			TileObject tileObjectForHouse = WorldManager.Instance.getTileObjectForHouse(num, displayPlayerHouseTiles.getStartingPosTransform().position + new Vector3(xPos * 2, 0f, yPos * 2), xPos, yPos, houseInfo);
			tileObjectForHouse.onDeathInsideServer(xPos, yPos, houseInfo.xPos, houseInfo.yPos);
			WorldManager.Instance.returnTileObject(tileObjectForHouse);
		}
		if (newTileType >= 0)
		{
			if ((bool)WorldManager.Instance.allObjects[newTileType].tileObjectFurniture)
			{
				houseInfo.houseMapOnTileStatus[xPos, yPos] = 0;
			}
			if ((bool)WorldManager.Instance.allObjects[newTileType].tileObjectItemChanger)
			{
				houseInfo.houseMapOnTileStatus[xPos, yPos] = -1;
			}
			if ((bool)WorldManager.Instance.allObjects[newTileType].tileObjectChest)
			{
				houseInfo.houseMapOnTileStatus[xPos, yPos] = 0;
			}
		}
		else
		{
			houseInfo.houseMapOnTileStatus[xPos, yPos] = -1;
		}
		DisplayPlayerHouseTiles displayPlayerHouseTiles2 = HouseManager.manage.findHousesOnDisplay(houseX, houseY);
		if ((bool)displayPlayerHouseTiles2)
		{
			if (newTileType != -1 && newTileType != 30)
			{
				SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.placeItem, new Vector3(xPos * 2, 0f, yPos * 2) + displayPlayerHouseTiles2.getStartingPosTransform().position);
				Vector3 position = new Vector3(xPos * 2, 0f, yPos * 2) + displayPlayerHouseTiles2.getStartingPosTransform().position;
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], position, 5);
			}
			displayPlayerHouseTiles2.refreshHouseTiles();
		}
		WorldManager.Instance.unlockClientTileHouse(xPos, yPos, houseX, houseY);
	}

	protected static void InvokeUserCode_RpcChangeHouseOnTile(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcChangeHouseOnTile called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcChangeHouseOnTile(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcSendPhotoDetails(int photoSlot, byte[] package)
	{
		if (!base.isServer)
		{
			for (int i = 0; i < package.Length; i++)
			{
				MuseumManager.manage.sentBytes[photoSlot].Add(package[i]);
			}
		}
	}

	protected static void InvokeUserCode_RpcSendPhotoDetails(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSendPhotoDetails called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcSendPhotoDetails(reader.ReadInt(), reader.ReadBytesAndSize());
		}
	}

	protected void UserCode_RpcSendFinalChunk(int photoSlot, byte[] package)
	{
		if (!base.isServer)
		{
			for (int i = 0; i < package.Length; i++)
			{
				MuseumManager.manage.sentBytes[photoSlot].Add(package[i]);
			}
			PhotoManager.manage.displayedPhotos[photoSlot] = new PhotoDetails();
			PhotoManager.manage.displayedPhotos[photoSlot].photoName = "Uploaded";
			MuseumManager.manage.paintingsOnDisplay[photoSlot] = PhotoManager.manage.loadPhotoFromByteArray(MuseumManager.manage.sentBytes[photoSlot].ToArray());
			MuseumManager.manage.sentBytes[photoSlot].Clear();
			if ((bool)MuseumDisplay.display)
			{
				MuseumDisplay.display.updatePhotoExhibits();
			}
		}
	}

	protected static void InvokeUserCode_RpcSendFinalChunk(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSendFinalChunk called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcSendFinalChunk(reader.ReadInt(), reader.ReadBytesAndSize());
		}
	}

	protected void UserCode_TargetSendPhotoDetails(NetworkConnection con, int photoSlot, byte[] package)
	{
		for (int i = 0; i < package.Length; i++)
		{
			MuseumManager.manage.sentBytes[photoSlot].Add(package[i]);
		}
	}

	protected static void InvokeUserCode_TargetSendPhotoDetails(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetSendPhotoDetails called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetSendPhotoDetails(NetworkClient.readyConnection, reader.ReadInt(), reader.ReadBytesAndSize());
		}
	}

	protected void UserCode_TargetSendFinalChunk(NetworkConnection con, int photoSlot, byte[] package)
	{
		for (int i = 0; i < package.Length; i++)
		{
			MuseumManager.manage.sentBytes[photoSlot].Add(package[i]);
		}
		PhotoManager.manage.displayedPhotos[photoSlot] = new PhotoDetails();
		PhotoManager.manage.displayedPhotos[photoSlot].photoName = "Uploaded";
		MuseumManager.manage.paintingsOnDisplay[photoSlot] = PhotoManager.manage.loadPhotoFromByteArray(MuseumManager.manage.sentBytes[photoSlot].ToArray());
		MuseumManager.manage.sentBytes[photoSlot].Clear();
		if ((bool)MuseumDisplay.display)
		{
			MuseumDisplay.display.updatePhotoExhibits();
		}
	}

	protected static void InvokeUserCode_TargetSendFinalChunk(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetSendFinalChunk called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetSendFinalChunk(NetworkClient.readyConnection, reader.ReadInt(), reader.ReadBytesAndSize());
		}
	}

	protected void UserCode_TargetOpenBuildWindowForClient(NetworkConnection con, int buildingId, int[] alreadyGiven)
	{
		DeedManager.manage.fillItemsAlreadyGivenForClient(buildingId, alreadyGiven);
		GiveNPC.give.OpenGiveWindow(GiveNPC.currentlyGivingTo.Build);
	}

	protected static void InvokeUserCode_TargetOpenBuildWindowForClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetOpenBuildWindowForClient called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetOpenBuildWindowForClient(NetworkClient.readyConnection, reader.ReadInt(), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader));
		}
	}

	protected void UserCode_TargetGivePermissionError(NetworkConnection con)
	{
		NotificationManager.manage.pocketsFull.ShowRequirePermission();
	}

	protected static void InvokeUserCode_TargetGivePermissionError(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetGivePermissionError called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_TargetGivePermissionError(NetworkClient.readyConnection);
		}
	}

	protected void UserCode_RpcSpawnCarryWorldObject(int carryId, Vector3 pos, Quaternion rotation)
	{
		Object.Instantiate(SaveLoad.saveOrLoad.carryablePrefabs[carryId].GetComponent<Damageable>().spawnWorldObjectOnDeath, pos, rotation);
	}

	protected static void InvokeUserCode_RpcSpawnCarryWorldObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSpawnCarryWorldObject called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcSpawnCarryWorldObject(reader.ReadInt(), reader.ReadVector3(), reader.ReadQuaternion());
		}
	}

	protected void UserCode_RpcRefreshDeedIngredients(int buildingId, int[] alreadyGiven)
	{
		DeedManager.manage.fillItemsAlreadyGivenForClient(buildingId, alreadyGiven);
	}

	protected static void InvokeUserCode_RpcRefreshDeedIngredients(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRefreshDeedIngredients called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcRefreshDeedIngredients(reader.ReadInt(), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader));
		}
	}

	protected void UserCode_RpcPlayBigStoneGrinderEffects(Vector3 position)
	{
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.bigStoneGrinderSound, position);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], position);
	}

	protected static void InvokeUserCode_RpcPlayBigStoneGrinderEffects(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayBigStoneGrinderEffects called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcPlayBigStoneGrinderEffects(reader.ReadVector3());
		}
	}

	protected void UserCode_RpcPlayDestroyCarrySound(Vector3 position, int carryId)
	{
		SoundManager.Instance.playASoundAtPoint(SaveLoad.saveOrLoad.carryablePrefabs[carryId].GetComponent<Damageable>().customDeathSound, position);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], position);
	}

	protected static void InvokeUserCode_RpcPlayDestroyCarrySound(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayDestroyCarrySound called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcPlayDestroyCarrySound(reader.ReadVector3(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcPayTownDebt(int payment, uint payedBy)
	{
		string playerName = NetworkIdentity.spawned[payedBy].GetComponent<EquipItemToChar>().playerName;
		NotificationManager.manage.createChatNotification(string.Format(ConversationGenerator.generate.GetToolTip("Tip_PlayerPaidTownDebt"), playerName, "<sprite=11>", payment.ToString("n0")));
	}

	protected static void InvokeUserCode_RpcPayTownDebt(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPayTownDebt called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcPayTownDebt(reader.ReadInt(), reader.ReadUInt());
		}
	}

	protected void UserCode_RpcChangeTileObjectToColourVarient(int newItemId, int colourId, int xPos, int yPos, int houseX, int houseY)
	{
		if (houseX == -1 && houseY == -1)
		{
			WorldManager.Instance.onTileMap[xPos, yPos] = newItemId;
			WorldManager.Instance.refreshTileObjectsOnChunksInUse(xPos, yPos);
			ParticleManager.manage.paintVehicle.GetComponent<ParticleSystemRenderer>().sharedMaterial = EquipWindow.equip.vehicleColours[colourId];
			ParticleSystem.ShapeModule shape = ParticleManager.manage.paintVehicle.shape;
			shape.enabled = true;
			shape.shapeType = ParticleSystemShapeType.Box;
			shape.scale = Vector3.one * 2f;
			ParticleManager.manage.paintVehicle.transform.position = new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2);
			SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.paintSound, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
			ParticleManager.manage.paintVehicle.Emit(50);
			WorldManager.Instance.unlockClientTile(xPos, yPos);
			return;
		}
		HouseManager.manage.getHouseInfo(houseX, houseY).houseMapOnTile[xPos, yPos] = newItemId;
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseX, houseY);
		if ((bool)displayPlayerHouseTiles)
		{
			displayPlayerHouseTiles.refreshHouseTiles();
			TileObject tileObject = displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos];
			if ((bool)tileObject)
			{
				ParticleManager.manage.paintVehicle.GetComponent<ParticleSystemRenderer>().sharedMaterial = EquipWindow.equip.vehicleColours[colourId];
				ParticleSystem.ShapeModule shape2 = ParticleManager.manage.paintVehicle.shape;
				shape2.enabled = true;
				shape2.shapeType = ParticleSystemShapeType.Box;
				shape2.scale = Vector3.one * 2f;
				ParticleManager.manage.paintVehicle.transform.position = tileObject.transform.position;
				SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.paintSound, tileObject.transform.position);
				ParticleManager.manage.paintVehicle.Emit(50);
			}
		}
		WorldManager.Instance.unlockClientTileHouse(xPos, yPos, houseX, houseY);
	}

	protected static void InvokeUserCode_RpcChangeTileObjectToColourVarient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcChangeTileObjectToColourVarient called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcChangeTileObjectToColourVarient(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcSplashInWater(Vector3 position)
	{
		ParticleManager.manage.waterSplash(position);
	}

	protected static void InvokeUserCode_RpcSplashInWater(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSplashInWater called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcSplashInWater(reader.ReadVector3());
		}
	}

	protected void UserCode_RpcRingTownBell()
	{
		NotificationManager.manage.createChatNotification("The Town Bell rings...");
		SoundManager.Instance.play2DSound(SoundManager.Instance.townBellSound);
		if ((bool)TownBell.Instance)
		{
			TownBell.Instance.RingBell();
		}
	}

	protected static void InvokeUserCode_RpcRingTownBell(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRingTownBell called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcRingTownBell();
		}
	}

	protected void UserCode_RpcReleaseBugFromSitting(int bugId, Vector3 position)
	{
		Object.Instantiate(AnimalManager.manage.releasedBug, position, Quaternion.identity).GetComponent<ReleaseBug>().setUpForBug(bugId);
	}

	protected static void InvokeUserCode_RpcReleaseBugFromSitting(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcReleaseBugFromSitting called on server.");
		}
		else
		{
			((NetworkMapSharer)obj).UserCode_RpcReleaseBugFromSitting(reader.ReadInt(), reader.ReadVector3());
		}
	}

	static NetworkMapSharer()
	{
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcPlayCarryDeathPart", InvokeUserCode_RpcPlayCarryDeathPart);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcWaterExplodeOnLava", InvokeUserCode_RpcWaterExplodeOnLava);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcPlayTrapperSound", InvokeUserCode_RpcPlayTrapperSound);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcFeedFishSound", InvokeUserCode_RpcFeedFishSound);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcMoveOffIsland", InvokeUserCode_RpcMoveOffIsland);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcMoveUnderGround", InvokeUserCode_RpcMoveUnderGround);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcMoveAboveGround", InvokeUserCode_RpcMoveAboveGround);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcReturnHomeFromOffIsland", InvokeUserCode_RpcReturnHomeFromOffIsland);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcCharEmotes", InvokeUserCode_RpcCharEmotes);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcBreakToolReact", InvokeUserCode_RpcBreakToolReact);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcMakeChatBubble", InvokeUserCode_RpcMakeChatBubble);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcSpawnATileObjectDrop", InvokeUserCode_RpcSpawnATileObjectDrop);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcDepositItemIntoChanger", InvokeUserCode_RpcDepositItemIntoChanger);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcMoveHouseExterior", InvokeUserCode_RpcMoveHouseExterior);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcMoveHouseInterior", InvokeUserCode_RpcMoveHouseInterior);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcDepositItemIntoChangerInside", InvokeUserCode_RpcDepositItemIntoChangerInside);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcUpdateHouseWall", InvokeUserCode_RpcUpdateHouseWall);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcUpdateHouseExterior", InvokeUserCode_RpcUpdateHouseExterior);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcAddToMuseum", InvokeUserCode_RpcAddToMuseum);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcUpdateHouseFloor", InvokeUserCode_RpcUpdateHouseFloor);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcPlaceOnTop", InvokeUserCode_RpcPlaceOnTop);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcSitDown", InvokeUserCode_RpcSitDown);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcGetUp", InvokeUserCode_RpcGetUp);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcEjectItemFromChanger", InvokeUserCode_RpcEjectItemFromChanger);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcEjectItemFromChangerInside", InvokeUserCode_RpcEjectItemFromChangerInside);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcOpenCloseTile", InvokeUserCode_RpcOpenCloseTile);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcNPCOpenGate", InvokeUserCode_RpcNPCOpenGate);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcHarvestObject", InvokeUserCode_RpcHarvestObject);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcDigUpBuriedItemNoise", InvokeUserCode_RpcDigUpBuriedItemNoise);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcThunderSound", InvokeUserCode_RpcThunderSound);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcThunderStrike", InvokeUserCode_RpcThunderStrike);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcActivateTrap", InvokeUserCode_RpcActivateTrap);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcClearOnTileObjectNoDrop", InvokeUserCode_RpcClearOnTileObjectNoDrop);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpChangeOnTileObjectNoDrop", InvokeUserCode_RpChangeOnTileObjectNoDrop);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcUpdateOnTileObjectForDesync", InvokeUserCode_RpcUpdateOnTileObjectForDesync);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcUpdateOnTileObject", InvokeUserCode_RpcUpdateOnTileObject);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcUpdateTileHeight", InvokeUserCode_RpcUpdateTileHeight);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcUpdateTileType", InvokeUserCode_RpcUpdateTileType);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcCheckHuntingTaskCompletion", InvokeUserCode_RpcCheckHuntingTaskCompletion);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcPlaceMultiTiledObject", InvokeUserCode_RpcPlaceMultiTiledObject);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcPlaceMultiTiledObjectPlaceUnder", InvokeUserCode_RpcPlaceMultiTiledObjectPlaceUnder);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcPlaceBridgeTiledObject", InvokeUserCode_RpcPlaceBridgeTiledObject);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcAddNewTaskToClientBoard", InvokeUserCode_RpcAddNewTaskToClientBoard);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcFillVillagerDetails", InvokeUserCode_RpcFillVillagerDetails);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcSetRotation", InvokeUserCode_RpcSetRotation);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcDeliverAnimal", InvokeUserCode_RpcDeliverAnimal);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcSellByWeight", InvokeUserCode_RpcSellByWeight);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcClearHouseForMove", InvokeUserCode_RpcClearHouseForMove);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcPickUpContainerObject", InvokeUserCode_RpcPickUpContainerObject);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcPickUpContainerObjectInside", InvokeUserCode_RpcPickUpContainerObjectInside);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcRemoveMultiTiledObject", InvokeUserCode_RpcRemoveMultiTiledObject);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcPlaceItemOnToTileObject", InvokeUserCode_RpcPlaceItemOnToTileObject);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcGiveOnTileStatus", InvokeUserCode_RpcGiveOnTileStatus);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcUseInstagrow", InvokeUserCode_RpcUseInstagrow);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcGiveOnTileStatusInside", InvokeUserCode_RpcGiveOnTileStatusInside);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcCompleteBulletinBoard", InvokeUserCode_RpcCompleteBulletinBoard);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcShowOffBuilding", InvokeUserCode_RpcShowOffBuilding);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcSyncDate", InvokeUserCode_RpcSyncDate);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcMakeAWish", InvokeUserCode_RpcMakeAWish);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcAddADay", InvokeUserCode_RpcAddADay);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcGiveOnTopStatus", InvokeUserCode_RpcGiveOnTopStatus);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcFillWithWater", InvokeUserCode_RpcFillWithWater);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcStallSold", InvokeUserCode_RpcStallSold);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcSpinChair", InvokeUserCode_RpcSpinChair);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcUpgradeHouse", InvokeUserCode_RpcUpgradeHouse);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcChangeHouseOnTile", InvokeUserCode_RpcChangeHouseOnTile);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcSendPhotoDetails", InvokeUserCode_RpcSendPhotoDetails);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcSendFinalChunk", InvokeUserCode_RpcSendFinalChunk);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcSpawnCarryWorldObject", InvokeUserCode_RpcSpawnCarryWorldObject);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcRefreshDeedIngredients", InvokeUserCode_RpcRefreshDeedIngredients);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcPlayBigStoneGrinderEffects", InvokeUserCode_RpcPlayBigStoneGrinderEffects);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcPlayDestroyCarrySound", InvokeUserCode_RpcPlayDestroyCarrySound);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcPayTownDebt", InvokeUserCode_RpcPayTownDebt);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcChangeTileObjectToColourVarient", InvokeUserCode_RpcChangeTileObjectToColourVarient);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcSplashInWater", InvokeUserCode_RpcSplashInWater);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcRingTownBell", InvokeUserCode_RpcRingTownBell);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "RpcReleaseBugFromSitting", InvokeUserCode_RpcReleaseBugFromSitting);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetRefreshChunkAfterSent", InvokeUserCode_TargetRefreshChunkAfterSent);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetRefreshNotNeeded", InvokeUserCode_TargetRefreshNotNeeded);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetGiveDigUpTreasureMilestone", InvokeUserCode_TargetGiveDigUpTreasureMilestone);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetGiveBuryItemMilestone", InvokeUserCode_TargetGiveBuryItemMilestone);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetGiveHuntingXp", InvokeUserCode_TargetGiveHuntingXp);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetGiveHarvestMilestone", InvokeUserCode_TargetGiveHarvestMilestone);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetGiveSignDetailsForChunk", InvokeUserCode_TargetGiveSignDetailsForChunk);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetGiveSignDetailsForHouse", InvokeUserCode_TargetGiveSignDetailsForHouse);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetGiveChunkWaterDetails", InvokeUserCode_TargetGiveChunkWaterDetails);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetGiveChunkOnTileDetails", InvokeUserCode_TargetGiveChunkOnTileDetails);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetGiveChunkOnTopDetails", InvokeUserCode_TargetGiveChunkOnTopDetails);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetGiveChunkTileTypeDetails", InvokeUserCode_TargetGiveChunkTileTypeDetails);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetRequestShopStall", InvokeUserCode_TargetRequestShopStall);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetGiveChunkHeightDetails", InvokeUserCode_TargetGiveChunkHeightDetails);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetRequestMuseum", InvokeUserCode_TargetRequestMuseum);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetSyncBulletinBoardPosts", InvokeUserCode_TargetSyncBulletinBoardPosts);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetGiveStamina", InvokeUserCode_TargetGiveStamina);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetSendBugCompLetter", InvokeUserCode_TargetSendBugCompLetter);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetSendFishingCompLetter", InvokeUserCode_TargetSendFishingCompLetter);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetGetRotationForTile", InvokeUserCode_TargetGetRotationForTile);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetRequestHouse", InvokeUserCode_TargetRequestHouse);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetRequestExterior", InvokeUserCode_TargetRequestExterior);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetSendPhotoDetails", InvokeUserCode_TargetSendPhotoDetails);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetSendFinalChunk", InvokeUserCode_TargetSendFinalChunk);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetOpenBuildWindowForClient", InvokeUserCode_TargetOpenBuildWindowForClient);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkMapSharer), "TargetGivePermissionError", InvokeUserCode_TargetGivePermissionError);
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(seed);
			writer.WriteInt(mineSeed);
			writer.WriteBool(hairDresserSeatOccupied);
			writer.WriteBool(serverUndergroundIsLoaded);
			writer.WriteBool(serverOffIslandIsLoaded);
			writer.WriteBool(northOn);
			writer.WriteBool(eastOn);
			writer.WriteBool(southOn);
			writer.WriteBool(westOn);
			writer.WriteVector2(privateTowerPos);
			writer.WriteInt(miningLevel);
			writer.WriteInt(loggingLevel);
			writer.WriteBool(craftsmanWorking);
			writer.WriteBool(craftsmanHasBerkonium);
			writer.WriteString(islandName);
			writer.WriteBool(nextDayIsReady);
			writer.WriteInt(movingBuilding);
			writer.WriteInt(townDebt);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(seed);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteInt(mineSeed);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteBool(hairDresserSeatOccupied);
			result = true;
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteBool(serverUndergroundIsLoaded);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			writer.WriteBool(serverOffIslandIsLoaded);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteBool(northOn);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x40L) != 0L)
		{
			writer.WriteBool(eastOn);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x80L) != 0L)
		{
			writer.WriteBool(southOn);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x100L) != 0L)
		{
			writer.WriteBool(westOn);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x200L) != 0L)
		{
			writer.WriteVector2(privateTowerPos);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x400L) != 0L)
		{
			writer.WriteInt(miningLevel);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x800L) != 0L)
		{
			writer.WriteInt(loggingLevel);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x1000L) != 0L)
		{
			writer.WriteBool(craftsmanWorking);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x2000L) != 0L)
		{
			writer.WriteBool(craftsmanHasBerkonium);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x4000L) != 0L)
		{
			writer.WriteString(islandName);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x8000L) != 0L)
		{
			writer.WriteBool(nextDayIsReady);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x10000L) != 0L)
		{
			writer.WriteInt(movingBuilding);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x20000L) != 0L)
		{
			writer.WriteInt(townDebt);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = seed;
			Networkseed = reader.ReadInt();
			int num2 = mineSeed;
			NetworkmineSeed = reader.ReadInt();
			if (!SyncVarEqual(num2, ref mineSeed))
			{
				onMineSeedChange(num2, mineSeed);
			}
			bool flag = hairDresserSeatOccupied;
			NetworkhairDresserSeatOccupied = reader.ReadBool();
			if (!SyncVarEqual(flag, ref hairDresserSeatOccupied))
			{
				onHairDresserSeatChange(flag, hairDresserSeatOccupied);
			}
			bool flag2 = serverUndergroundIsLoaded;
			NetworkserverUndergroundIsLoaded = reader.ReadBool();
			bool flag3 = serverOffIslandIsLoaded;
			NetworkserverOffIslandIsLoaded = reader.ReadBool();
			bool flag4 = northOn;
			NetworknorthOn = reader.ReadBool();
			if (!SyncVarEqual(flag4, ref northOn))
			{
				northCheck(flag4, northOn);
			}
			bool flag5 = eastOn;
			NetworkeastOn = reader.ReadBool();
			if (!SyncVarEqual(flag5, ref eastOn))
			{
				eastCheck(flag5, eastOn);
			}
			bool flag6 = southOn;
			NetworksouthOn = reader.ReadBool();
			if (!SyncVarEqual(flag6, ref southOn))
			{
				southCheck(flag6, southOn);
			}
			bool flag7 = westOn;
			NetworkwestOn = reader.ReadBool();
			if (!SyncVarEqual(flag7, ref westOn))
			{
				westCheck(flag7, westOn);
			}
			Vector2 vector = privateTowerPos;
			NetworkprivateTowerPos = reader.ReadVector2();
			if (!SyncVarEqual(vector, ref privateTowerPos))
			{
				privateTowerCheck(vector, privateTowerPos);
			}
			int num3 = miningLevel;
			NetworkminingLevel = reader.ReadInt();
			int num4 = loggingLevel;
			NetworkloggingLevel = reader.ReadInt();
			bool flag8 = craftsmanWorking;
			NetworkcraftsmanWorking = reader.ReadBool();
			if (!SyncVarEqual(flag8, ref craftsmanWorking))
			{
				craftsmanWorkingChange(flag8, craftsmanWorking);
			}
			bool flag9 = craftsmanHasBerkonium;
			NetworkcraftsmanHasBerkonium = reader.ReadBool();
			string text = islandName;
			NetworkislandName = reader.ReadString();
			bool flag10 = nextDayIsReady;
			NetworknextDayIsReady = reader.ReadBool();
			int num5 = movingBuilding;
			NetworkmovingBuilding = reader.ReadInt();
			int num6 = townDebt;
			NetworktownDebt = reader.ReadInt();
			return;
		}
		long num7 = (long)reader.ReadULong();
		if ((num7 & 1L) != 0L)
		{
			int num8 = seed;
			Networkseed = reader.ReadInt();
		}
		if ((num7 & 2L) != 0L)
		{
			int num9 = mineSeed;
			NetworkmineSeed = reader.ReadInt();
			if (!SyncVarEqual(num9, ref mineSeed))
			{
				onMineSeedChange(num9, mineSeed);
			}
		}
		if ((num7 & 4L) != 0L)
		{
			bool flag11 = hairDresserSeatOccupied;
			NetworkhairDresserSeatOccupied = reader.ReadBool();
			if (!SyncVarEqual(flag11, ref hairDresserSeatOccupied))
			{
				onHairDresserSeatChange(flag11, hairDresserSeatOccupied);
			}
		}
		if ((num7 & 8L) != 0L)
		{
			bool flag12 = serverUndergroundIsLoaded;
			NetworkserverUndergroundIsLoaded = reader.ReadBool();
		}
		if ((num7 & 0x10L) != 0L)
		{
			bool flag13 = serverOffIslandIsLoaded;
			NetworkserverOffIslandIsLoaded = reader.ReadBool();
		}
		if ((num7 & 0x20L) != 0L)
		{
			bool flag14 = northOn;
			NetworknorthOn = reader.ReadBool();
			if (!SyncVarEqual(flag14, ref northOn))
			{
				northCheck(flag14, northOn);
			}
		}
		if ((num7 & 0x40L) != 0L)
		{
			bool flag15 = eastOn;
			NetworkeastOn = reader.ReadBool();
			if (!SyncVarEqual(flag15, ref eastOn))
			{
				eastCheck(flag15, eastOn);
			}
		}
		if ((num7 & 0x80L) != 0L)
		{
			bool flag16 = southOn;
			NetworksouthOn = reader.ReadBool();
			if (!SyncVarEqual(flag16, ref southOn))
			{
				southCheck(flag16, southOn);
			}
		}
		if ((num7 & 0x100L) != 0L)
		{
			bool flag17 = westOn;
			NetworkwestOn = reader.ReadBool();
			if (!SyncVarEqual(flag17, ref westOn))
			{
				westCheck(flag17, westOn);
			}
		}
		if ((num7 & 0x200L) != 0L)
		{
			Vector2 vector2 = privateTowerPos;
			NetworkprivateTowerPos = reader.ReadVector2();
			if (!SyncVarEqual(vector2, ref privateTowerPos))
			{
				privateTowerCheck(vector2, privateTowerPos);
			}
		}
		if ((num7 & 0x400L) != 0L)
		{
			int num10 = miningLevel;
			NetworkminingLevel = reader.ReadInt();
		}
		if ((num7 & 0x800L) != 0L)
		{
			int num11 = loggingLevel;
			NetworkloggingLevel = reader.ReadInt();
		}
		if ((num7 & 0x1000L) != 0L)
		{
			bool flag18 = craftsmanWorking;
			NetworkcraftsmanWorking = reader.ReadBool();
			if (!SyncVarEqual(flag18, ref craftsmanWorking))
			{
				craftsmanWorkingChange(flag18, craftsmanWorking);
			}
		}
		if ((num7 & 0x2000L) != 0L)
		{
			bool flag19 = craftsmanHasBerkonium;
			NetworkcraftsmanHasBerkonium = reader.ReadBool();
		}
		if ((num7 & 0x4000L) != 0L)
		{
			string text2 = islandName;
			NetworkislandName = reader.ReadString();
		}
		if ((num7 & 0x8000L) != 0L)
		{
			bool flag20 = nextDayIsReady;
			NetworknextDayIsReady = reader.ReadBool();
		}
		if ((num7 & 0x10000L) != 0L)
		{
			int num12 = movingBuilding;
			NetworkmovingBuilding = reader.ReadInt();
		}
		if ((num7 & 0x20000L) != 0L)
		{
			int num13 = townDebt;
			NetworktownDebt = reader.ReadInt();
		}
	}
}
