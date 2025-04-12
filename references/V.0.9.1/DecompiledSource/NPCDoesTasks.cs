using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class NPCDoesTasks : NetworkBehaviour
{
	public enum typeOfTask
	{
		None,
		FindingCrop,
		FindingSeat,
		FindingSomeoneToTalkTo,
		FindingAnimalToPet,
		HavingASnack,
		FollowingAndWatering,
		FollowingAndAttacking,
		FollowingAndDiggingTreasure,
		FollowingAndHarvesting,
		ClappingForPlayer,
		HuntingBugs,
		ClappingAtSky,
		BlowingPartyHorn,
		HoldingBalloon,
		LookAtSkyAndClap,
		HoldingKite,
		LookAtSkyAndGlee
	}

	public NPCHoldsItems npcHolds;

	public NPCAI myAi;

	public bool hasTask;

	public Vector3 taskPosition;

	public InventoryItem myWaterCan;

	public InventoryItem myWeapon;

	public InventoryItem myAxe;

	public InventoryItem myPickaxe;

	public InventoryItem myShovel;

	public InventoryItem myBugNet;

	public LayerMask myEnemies;

	public LayerMask talkToLayer;

	public LayerMask bugLayer;

	private CharMovement following;

	public typeOfTask currentTask;

	[SyncVar(hook = "OnGetScared")]
	public bool isScared;

	private float timeSinceLastWonder;

	public InventoryItem metalDetector;

	public InventoryItem[] playerWeapons;

	public InventoryItem[] playerAxes;

	public InventoryItem[] playerPickaxes;

	public InventoryItem[] playerWateringCans;

	public InventoryItem partyHorn;

	public InventoryItem balloon;

	public InventoryItem kite;

	private Coroutine myTaskRoutine;

	private WaitForSeconds waitTime = new WaitForSeconds(0.5f);

	private InteriorLocationOfInterest interiorPointOfInterest;

	private NPCAI wantsToTalkTo;

	private FarmAnimal wantsToPet;

	private BugTypes wantToCatch;

	private WaitForSeconds taskGap = new WaitForSeconds(0.5f);

	private Vector3 sittingPosition;

	private static WaitForSeconds waitWhileInDanger;

	private bool inWater;

	private AnimalAI wantsToAttack;

	private float scaredDistance = 26f;

	private int watchPlayerHoldingId = -1;

	private MetalDetectorUse watchingDetector;

	public bool NetworkisScared
	{
		get
		{
			return isScared;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref isScared))
			{
				bool oldScared = isScared;
				SetSyncVar(value, ref isScared, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					OnGetScared(oldScared, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	private void Start()
	{
		npcHolds = GetComponent<NPCHoldsItems>();
		randomiseTasks();
	}

	public override void OnStartServer()
	{
		hasTask = false;
		taskPosition = Vector3.zero;
		myAi = GetComponent<NPCAI>();
		randomiseTasks();
		myTaskRoutine = StartCoroutine(lookForTasks());
		RealWorldTimeLight.time.taskChecker.AddListener(randomiseTasks);
	}

	private void OnDisable()
	{
		RealWorldTimeLight.time.taskChecker.RemoveListener(randomiseTasks);
	}

	public override void OnStartClient()
	{
		myAi = GetComponent<NPCAI>();
	}

	public void npcStartNewDay()
	{
		hasTask = false;
		taskPosition = Vector3.zero;
		wantsToTalkTo = null;
		wantToCatch = null;
		wantsToPet = null;
		currentTask = typeOfTask.None;
		if (myTaskRoutine != null)
		{
			StopCoroutine(myTaskRoutine);
		}
		myTaskRoutine = StartCoroutine(lookForTasks());
	}

	private void OnEnable()
	{
		if ((bool)myAi)
		{
			myAi.myAgent.updateRotation = true;
		}
	}

	public void randomiseTasks()
	{
		if (hasTask)
		{
			return;
		}
		if ((bool)following)
		{
			hasTask = false;
		}
		else if (CatchingCompetitionManager.manage.IsBugCompToday() && CatchingCompetitionManager.manage.competitionActive())
		{
			currentTask = typeOfTask.HuntingBugs;
		}
		else if (TownEventManager.manage.townEventOn == TownEventManager.TownEventType.IslandDay)
		{
			if (RealWorldTimeLight.time.currentHour >= 12 && RealWorldTimeLight.time.currentHour < 15)
			{
				if (Random.Range(0, 3) == 2)
				{
					currentTask = typeOfTask.FindingSomeoneToTalkTo;
				}
				else
				{
					currentTask = typeOfTask.None;
				}
				npcHolds.changeItemHolding(TownEventManager.manage.snag.getItemId());
			}
			else if (RealWorldTimeLight.time.currentHour >= 0 && RealWorldTimeLight.time.currentHour <= 19)
			{
				int num = Random.Range(0, 13);
				if (num == 1)
				{
					currentTask = typeOfTask.FindingSeat;
				}
				else if (num <= 6)
				{
					currentTask = typeOfTask.FindingSomeoneToTalkTo;
				}
				else if (num <= 8)
				{
					currentTask = typeOfTask.BlowingPartyHorn;
				}
				else if (num <= 11)
				{
					currentTask = typeOfTask.HoldingBalloon;
				}
				else
				{
					currentTask = typeOfTask.None;
				}
			}
			else
			{
				switch (Random.Range(0, 8))
				{
				case 0:
					currentTask = typeOfTask.FindingSeat;
					break;
				case 1:
					currentTask = typeOfTask.FindingSomeoneToTalkTo;
					break;
				default:
					currentTask = typeOfTask.LookAtSkyAndClap;
					break;
				}
			}
		}
		else if (TownEventManager.manage.townEventOn == TownEventManager.TownEventType.SkyFest)
		{
			if (RealWorldTimeLight.time.currentHour >= 0 && RealWorldTimeLight.time.currentHour < 12)
			{
				int num2 = Random.Range(0, 10);
				if (num2 == 1)
				{
					currentTask = typeOfTask.FindingSeat;
				}
				else if (num2 <= 6)
				{
					currentTask = typeOfTask.FindingSomeoneToTalkTo;
				}
				else
				{
					currentTask = typeOfTask.None;
				}
			}
			else if (RealWorldTimeLight.time.currentHour >= 0 && RealWorldTimeLight.time.currentHour <= 12)
			{
				int num3 = Random.Range(0, 10);
				if (num3 == 1)
				{
					currentTask = typeOfTask.FindingSeat;
				}
				else if (num3 <= 6)
				{
					currentTask = typeOfTask.FindingSomeoneToTalkTo;
				}
				else if (num3 <= 7)
				{
					currentTask = typeOfTask.HoldingKite;
				}
				else
				{
					currentTask = typeOfTask.None;
				}
			}
			else if (RealWorldTimeLight.time.currentHour >= 0 && RealWorldTimeLight.time.currentHour >= 15 && RealWorldTimeLight.time.currentHour < 20)
			{
				currentTask = typeOfTask.HoldingKite;
			}
			else
			{
				switch (Random.Range(0, 8))
				{
				case 0:
					currentTask = typeOfTask.FindingSeat;
					break;
				case 1:
					currentTask = typeOfTask.FindingSomeoneToTalkTo;
					break;
				case 3:
					currentTask = typeOfTask.HoldingKite;
					break;
				default:
					currentTask = typeOfTask.LookAtSkyAndGlee;
					break;
				}
			}
		}
		else
		{
			int num4 = Random.Range(0, 20);
			if (num4 <= 2)
			{
				currentTask = typeOfTask.FindingCrop;
			}
			else if (num4 <= 7)
			{
				currentTask = typeOfTask.FindingSeat;
			}
			else if (num4 <= 11)
			{
				currentTask = typeOfTask.FindingSomeoneToTalkTo;
			}
			else if (num4 <= 14)
			{
				if (RealWorldTimeLight.time.currentHour == 0 || RealWorldTimeLight.time.currentHour > 18)
				{
					currentTask = typeOfTask.None;
				}
				else
				{
					currentTask = typeOfTask.FindingAnimalToPet;
				}
			}
			else
			{
				currentTask = typeOfTask.None;
			}
			if (currentTask == typeOfTask.None && (bool)RealWorldTimeLight.time && (bool)WeatherManager.Instance && WeatherManager.Instance.CurrentWeather != null && num4 > 10 && RealWorldTimeLight.time.currentHour >= RealWorldTimeLight.time.getSunSetTime() + 1 && WeatherManager.Instance.CurrentWeather.isMeteorShower)
			{
				currentTask = typeOfTask.LookAtSkyAndClap;
			}
		}
		waitTime = new WaitForSeconds(Random.Range(0.95f, 1.1f));
	}

	private IEnumerator lookForTasks()
	{
		yield return null;
		yield return null;
		if (GetComponent<NPCIdentity>().NPCNo == 5 || GetComponent<NPCIdentity>().NPCNo == 16)
		{
			yield break;
		}
		while (true)
		{
			yield return waitTime;
			if (myAi.myManager != null && myAi.myManager.checkIfNPCIsFree())
			{
				if (myAi.talkingTo != 0)
				{
					continue;
				}
				if ((bool)myAi.following)
				{
					yield return StartCoroutine(followingRoutine());
					continue;
				}
				if (myAi.myManager.isInsideABuilding())
				{
					yield return StartCoroutine(checkIfShouldGreet());
					findATaskInside();
					if (hasTask)
					{
						yield return StartCoroutine(walkToTaskInside());
						continue;
					}
					timeSinceLastWonder += 1f;
					wonderAround();
					continue;
				}
				yield return StartCoroutine(checkIfShouldBeScared());
				yield return StartCoroutine(checkIfShouldGreet());
				findATask();
				if (hasTask)
				{
					yield return StartCoroutine(walkToTask());
					continue;
				}
				timeSinceLastWonder += 1f;
				wonderAround();
			}
			else
			{
				if (myAi.myAgent.isActiveAndEnabled && myAi.myAgent.isOnNavMesh)
				{
					myAi.myAgent.isStopped = false;
					myAi.myAgent.updateRotation = true;
				}
				if (TownEventManager.manage.isEventToday(TownEventManager.TownEventType.IslandDay) && RealWorldTimeLight.time.currentHour >= 12 && RealWorldTimeLight.time.currentHour < 15)
				{
					npcHolds.changeItemHolding(TownEventManager.manage.snag.getItemId());
				}
				else
				{
					npcHolds.changeItemHolding(-1);
				}
				currentTask = typeOfTask.None;
				hasTask = false;
			}
		}
	}

	public void wonderAround()
	{
		if (!hasTask && myAi.talkingTo == 0 && myAi.myManager.checkIfNPCIsFree() && (Random.Range(0, 75) == 5 || timeSinceLastWonder >= 15f || (RealWorldTimeLight.time.currentHour == 7 && RealWorldTimeLight.time.currentMinute < 5) || myAi.myManager.checkIfNPCHasJustExited()) && myAi.myAgent.isActiveAndEnabled && myAi.myAgent.isOnNavMesh && myAi.myAgent.remainingDistance <= myAi.myAgent.stoppingDistance)
		{
			myAi.myAgent.SetDestination(myAi.getSureRandomPos());
			timeSinceLastWonder = 0f;
		}
	}

	public void findATaskInside()
	{
		NPCBuildingDoors buildingCurrentlyInside = myAi.myManager.getBuildingCurrentlyInside();
		myAi.myAgent.SetDestination(myAi.myAgent.transform.position + (myAi.myAgent.transform.forward * 2f + myAi.myAgent.transform.right * Random.Range(-2f, 2f)));
		if (Random.Range(0, 3) == 1 && !hasTask && buildingCurrentlyInside != null && buildingCurrentlyInside.locationActivities != null)
		{
			hasTask = true;
			currentTask = typeOfTask.FindingSeat;
			interiorPointOfInterest = buildingCurrentlyInside.locationActivities.getAPlaceOfInterest();
			taskPosition = interiorPointOfInterest.position.position;
		}
		else if (!hasTask && Random.Range(0, 3) == 2)
		{
			currentTask = typeOfTask.FindingSomeoneToTalkTo;
			interiorPointOfInterest = null;
			if (!Physics.CheckSphere(base.transform.position, 15f, talkToLayer))
			{
				return;
			}
			Collider[] array = Physics.OverlapSphere(base.transform.position, 15f, talkToLayer);
			if (array.Length != 0)
			{
				int num = Random.Range(0, array.Length);
				if (array[num].transform.root != base.transform)
				{
					wantsToTalkTo = array[num].GetComponentInParent<NPCAI>();
					if ((bool)wantsToTalkTo && wantsToTalkTo.talkingTo == 0 && myAi.myManager.checkIfNPCIsFree())
					{
						taskPosition = wantsToTalkTo.transform.position;
						hasTask = true;
					}
					else
					{
						wantsToTalkTo = null;
					}
				}
			}
			else
			{
				hasTask = false;
			}
		}
		else
		{
			interiorPointOfInterest = null;
			hasTask = false;
		}
	}

	public void findATask()
	{
		if (hasTask)
		{
			return;
		}
		if (currentTask == typeOfTask.HuntingBugs)
		{
			if (npcHolds.currentlyHolding != myBugNet)
			{
				npcHolds.changeItemHolding(myBugNet.getItemId());
			}
			if (!Physics.CheckSphere(base.transform.position + base.transform.forward * 25f, 23f, bugLayer))
			{
				return;
			}
			Collider[] array = Physics.OverlapSphere(base.transform.position + base.transform.forward * 25f, 25f, bugLayer);
			if (array.Length == 0)
			{
				return;
			}
			int num = Random.Range(0, array.Length);
			if (array[num].transform.root != base.transform)
			{
				wantToCatch = array[num].GetComponentInParent<BugTypes>();
				if ((bool)wantToCatch)
				{
					taskPosition = wantToCatch.transform.position;
					hasTask = true;
				}
			}
		}
		else if (currentTask == typeOfTask.FindingCrop)
		{
			Vector3 vector = WorldManager.Instance.findClosestTileObjectAround(base.transform.position, TownManager.manage.allCropsTypes, 20, checkIfWatered: true);
			if (vector != Vector3.zero)
			{
				taskPosition = vector;
				hasTask = true;
			}
		}
		else if (currentTask == typeOfTask.FindingSeat)
		{
			Vector3 vector2 = WorldManager.Instance.findClosestTileObjectAround(base.transform.position, TownManager.manage.allTownSeats, 20, checkIfWatered: false, checkIfSeatEmpty: true);
			if (vector2 != Vector3.zero && !WorldManager.Instance.isSeatTaken(vector2))
			{
				taskPosition = vector2;
				hasTask = true;
				npcHolds.changeItemHolding(-1);
			}
			else
			{
				npcHolds.changeItemHolding(-1);
				hasTask = false;
				randomiseTasks();
			}
		}
		else if (currentTask == typeOfTask.FindingSomeoneToTalkTo)
		{
			if (Physics.CheckSphere(base.transform.position + base.transform.forward * 25f, 23f, talkToLayer))
			{
				Collider[] array2 = Physics.OverlapSphere(base.transform.position + base.transform.forward * 25f, 25f, talkToLayer);
				if (array2.Length != 0)
				{
					int num2 = Random.Range(0, array2.Length);
					if (array2[num2].transform.root != base.transform)
					{
						wantsToTalkTo = array2[num2].GetComponentInParent<NPCAI>();
						if ((bool)wantsToTalkTo && wantsToTalkTo.talkingTo == 0 && myAi.myManager.checkIfNPCIsFree())
						{
							taskPosition = wantsToTalkTo.transform.position;
							hasTask = true;
						}
						else
						{
							wantsToTalkTo = null;
						}
					}
				}
				else
				{
					hasTask = false;
				}
			}
			else
			{
				wantsToTalkTo = null;
				hasTask = false;
				randomiseTasks();
			}
		}
		else if (currentTask == typeOfTask.FindingAnimalToPet)
		{
			if (FarmAnimalManager.manage.activeAnimalAgents.Count > 0)
			{
				int index = Random.Range(0, FarmAnimalManager.manage.activeAnimalAgents.Count);
				if (FarmAnimalManager.manage.activeAnimalAgents[index] != null && RealWorldTimeLight.time.currentHour < 17 && Vector3.Distance(base.transform.position, FarmAnimalManager.manage.activeAnimalAgents[index].transform.position) < 70f)
				{
					wantsToPet = FarmAnimalManager.manage.activeAnimalAgents[index];
					taskPosition = wantsToPet.transform.position;
					hasTask = true;
				}
				else
				{
					wantsToPet = null;
					hasTask = false;
					randomiseTasks();
				}
			}
			else
			{
				wantsToPet = null;
			}
		}
		else if (currentTask == typeOfTask.HavingASnack)
		{
			if (Random.Range(0, 50) > 48)
			{
				taskPosition = base.transform.position;
				hasTask = true;
			}
		}
		else if (currentTask == typeOfTask.BlowingPartyHorn)
		{
			if (Random.Range(0, 50) > 48)
			{
				taskPosition = base.transform.position;
				hasTask = true;
			}
		}
		else if (currentTask == typeOfTask.LookAtSkyAndClap)
		{
			if (Random.Range(0, 3) > 1)
			{
				taskPosition = base.transform.position;
				hasTask = true;
			}
		}
		else if (currentTask == typeOfTask.HoldingBalloon)
		{
			if (Random.Range(0, 50) > 48)
			{
				taskPosition = base.transform.position + new Vector3(Random.Range(-4, 4), 0f, Random.Range(-4, 4));
				hasTask = true;
			}
		}
		else if (currentTask == typeOfTask.HoldingKite)
		{
			taskPosition = base.transform.position + new Vector3(Random.Range(-4, 4), 0f, Random.Range(-4, 4));
			hasTask = true;
		}
		else
		{
			if (TownEventManager.manage.isEventToday(TownEventManager.TownEventType.IslandDay) && RealWorldTimeLight.time.currentHour >= 12 && RealWorldTimeLight.time.currentHour < 15)
			{
				npcHolds.changeItemHolding(TownEventManager.manage.snag.getItemId());
			}
			else
			{
				npcHolds.changeItemHolding(-1);
			}
			hasTask = false;
		}
	}

	private int getRandomSeatPosition(Vector3 seatPos)
	{
		if (WorldManager.Instance.onTileStatusMap[(int)seatPos.x / 2, (int)seatPos.z / 2] <= 0)
		{
			return Random.Range(1, 2);
		}
		if (WorldManager.Instance.onTileStatusMap[(int)seatPos.x / 2, (int)seatPos.z / 2] == 1)
		{
			return 2;
		}
		return 1;
	}

	public void onToolDoesDamage()
	{
		if (npcHolds.currentlyHolding == myWaterCan)
		{
			int resultingPlaceableTileType = npcHolds.currentlyHolding.getResultingPlaceableTileType(WorldManager.Instance.tileTypeMap[(int)npcHolds.currentlyTargetingPos.x, (int)npcHolds.currentlyTargetingPos.y]);
			if (resultingPlaceableTileType != 0)
			{
				NetworkMapSharer.Instance.RpcUpdateTileType(resultingPlaceableTileType, (int)npcHolds.currentlyTargetingPos.x, (int)npcHolds.currentlyTargetingPos.y);
			}
		}
		else if ((bool)npcHolds.currentlyHolding && npcHolds.currentlyHolding.myAnimType == InventoryItem.typeOfAnimation.ShovelAnimation)
		{
			if (npcHolds.usingItem)
			{
				if (npcHolds.currentlyHolding == myShovel)
				{
					NetworkMapSharer.Instance.changeTileHeight(-1, (int)npcHolds.currentlyTargetingPos.x, (int)npcHolds.currentlyTargetingPos.y, following.myInteract.connectionToClient);
					StartCoroutine(swapShovel(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[(int)npcHolds.currentlyTargetingPos.x, (int)npcHolds.currentlyTargetingPos.y]].uniqueShovel.getItemId()));
				}
				else
				{
					NetworkMapSharer.Instance.changeTileHeight(1, (int)npcHolds.currentlyTargetingPos.x, (int)npcHolds.currentlyTargetingPos.y, following.myInteract.connectionToClient);
					StartCoroutine(swapShovel(myShovel.getItemId()));
				}
			}
		}
		else
		{
			RpcNpcDoesDamageToTileObject();
		}
	}

	private IEnumerator waterPlant()
	{
		myAi.myAgent.isStopped = true;
		yield return null;
		npcHolds.NetworkcurrentlyTargetingPos = new Vector2((int)taskPosition.x / 2, (int)taskPosition.z / 2);
		if (npcHolds.currentlyHolding != myWaterCan)
		{
			npcHolds.changeItemHolding(Inventory.Instance.getInvItemId(myWaterCan));
		}
		yield return StartCoroutine(faceCurrentTask());
		if (hasTask)
		{
			yield return taskGap;
			npcHolds.NetworkusingItem = true;
			yield return taskGap;
			npcHolds.NetworkusingItem = false;
			yield return taskGap;
			yield return taskGap;
		}
		hasTask = false;
		myAi.myAgent.isStopped = false;
	}

	private IEnumerator swapShovel(int newShovelId)
	{
		npcHolds.NetworkusingItem = false;
		yield return null;
		npcHolds.changeItemHolding(newShovelId);
	}

	private IEnumerator digUp()
	{
		myAi.myAgent.isStopped = true;
		myAi.myAgent.updateRotation = false;
		yield return null;
		npcHolds.NetworkcurrentlyTargetingPos = new Vector2((int)taskPosition.x / 2, (int)taskPosition.z / 2);
		if (npcHolds.currentlyHolding != myShovel)
		{
			npcHolds.changeItemHolding(Inventory.Instance.getInvItemId(myShovel));
		}
		yield return StartCoroutine(faceCurrentTask());
		if (hasTask)
		{
			yield return taskGap;
			npcHolds.NetworkusingItem = true;
			while (npcHolds.currentlyHolding == myShovel)
			{
				yield return null;
			}
		}
		yield return taskGap;
		while (Vector3.Distance(following.transform.position, base.transform.position) <= 12f && WorldManager.Instance.getAllDropsOnTile((int)npcHolds.currentlyTargetingPos.x, (int)npcHolds.currentlyTargetingPos.y).Count > 0)
		{
			yield return null;
		}
		yield return taskGap;
		npcHolds.NetworkusingItem = true;
		while (npcHolds.currentlyHolding != myShovel)
		{
			yield return null;
		}
		yield return taskGap;
		myAi.myAgent.updateRotation = true;
		myAi.myAgent.isStopped = false;
	}

	private IEnumerator sitDownInSeat()
	{
		if (!WorldManager.Instance.isSeatTaken(taskPosition))
		{
			TileObject tileObjectForServerDrop = WorldManager.Instance.getTileObjectForServerDrop(WorldManager.Instance.onTileMap[(int)taskPosition.x / 2, (int)taskPosition.z / 2], taskPosition);
			WorldManager.Instance.returnTileObject(tileObjectForServerDrop);
			int seatSpace = 1;
			if ((bool)tileObjectForServerDrop.tileObjectFurniture.seatPosition2)
			{
				seatSpace = getRandomSeatPosition(taskPosition);
			}
			_ = taskPosition;
			Vector3 sittingPosition;
			Quaternion sittingRotation;
			if (seatSpace == 1)
			{
				sittingPosition = tileObjectForServerDrop.tileObjectFurniture.seatPosition1.transform.position;
				sittingRotation = tileObjectForServerDrop.tileObjectFurniture.seatPosition1.transform.rotation;
			}
			else
			{
				sittingPosition = tileObjectForServerDrop.tileObjectFurniture.seatPosition2.transform.position;
				sittingRotation = tileObjectForServerDrop.tileObjectFurniture.seatPosition2.transform.rotation;
			}
			if (!WorldManager.Instance.isSeatTaken(taskPosition, seatSpace))
			{
				Vector3 walkToSitDownPos = myAi.getClosestPosOnNavMesh(sittingPosition - tileObjectForServerDrop.transform.forward * 2f);
				myAi.myAgent.SetDestination(walkToSitDownPos);
				bool madeItToSeat = true;
				while (Vector3.Distance(base.transform.position, walkToSitDownPos) > 2f && !WorldManager.Instance.isSeatTaken(taskPosition, seatSpace))
				{
					if (!myAi.canStillReachTaskLocation(walkToSitDownPos) || !myAi.myManager.checkIfNPCIsFree())
					{
						madeItToSeat = false;
						break;
					}
					yield return null;
				}
				if (madeItToSeat && !WorldManager.Instance.isSeatTaken(taskPosition, seatSpace) && myAi.myManager.checkIfNPCIsFree())
				{
					myAi.myAgent.isStopped = true;
					GetComponent<stopNPCsPushing>().stopSelf();
					myAi.SitDownOutside(seatSpace, (int)taskPosition.x / 2, (int)taskPosition.z / 2);
					float sittingTime = Random.Range(45f, 60f);
					while (sittingTime > 0f && myAi.myManager.checkIfNPCIsFree())
					{
						base.transform.position = Vector3.Lerp(base.transform.position, sittingPosition, Time.deltaTime * 8f);
						base.transform.rotation = Quaternion.Lerp(base.transform.rotation, sittingRotation, Time.deltaTime * 8f);
						if (returnClosestEnemy() != null)
						{
							sittingTime = 0f;
						}
						if (myAi.talkingTo == 0)
						{
							sittingTime -= Time.deltaTime;
						}
						yield return null;
					}
					myAi.myAgent.Warp(walkToSitDownPos);
					myAi.StandUpOutside(seatSpace, (int)taskPosition.x / 2, (int)taskPosition.z / 2);
					myAi.myAgent.isStopped = false;
					GetComponent<stopNPCsPushing>().startSelf();
					yield return new WaitForSeconds(0.5f);
				}
			}
		}
		currentTask = typeOfTask.None;
		hasTask = false;
	}

	private IEnumerator sitDownOnInsideSeat()
	{
		float talkTimer = 0f;
		if (!TuckshopManager.manage.isSeatTaken(interiorPointOfInterest.seatId))
		{
			_ = taskPosition;
			Vector3 walkToSitDownPos = myAi.getClosestPosOnNavMesh(interiorPointOfInterest.position.position);
			myAi.myAgent.SetDestination(walkToSitDownPos);
			bool madeItToSeat = true;
			while (Vector3.Distance(base.transform.position, walkToSitDownPos) > 2.5f && !TuckshopManager.manage.isSeatTaken(interiorPointOfInterest.seatId))
			{
				if (!myAi.myManager.checkIfNPCIsFree())
				{
					madeItToSeat = false;
					break;
				}
				yield return null;
			}
			if (madeItToSeat && !TuckshopManager.manage.isSeatTaken(interiorPointOfInterest.seatId) && myAi.myManager.checkIfNPCIsFree())
			{
				myAi.myAgent.isStopped = true;
				GetComponent<stopNPCsPushing>().stopSelf();
				myAi.SitDownInside();
				TuckshopManager.manage.sitInSeat(interiorPointOfInterest.seatId);
				float sittingTime = Random.Range(120f, 180f);
				while (sittingTime > 0f && myAi.myManager.checkIfNPCIsFree())
				{
					base.transform.position = Vector3.Lerp(base.transform.position, interiorPointOfInterest.position.position, Time.deltaTime * 8f);
					base.transform.rotation = Quaternion.Lerp(base.transform.rotation, interiorPointOfInterest.position.rotation, Time.deltaTime * 8f);
					if (myAi.talkingTo == 0)
					{
						sittingTime -= Time.deltaTime;
					}
					if (wantsToTalkTo == null && Random.Range(0, 220) == 2)
					{
						lookForSomeoneToTalkToWhileSittingDown();
					}
					if ((bool)wantsToTalkTo)
					{
						if (wantsToTalkTo.isSittingDown() && wantsToTalkTo.myManager.checkIfNPCIsFree())
						{
							if (talkTimer == 0f)
							{
								if (wantsToTalkTo.talkingTo == 0 && myAi.talkingTo == 0)
								{
									wantsToTalkTo.NetworktalkingTo = base.netId;
									myAi.NetworktalkingTo = wantsToTalkTo.netId;
									talkTimer += Time.deltaTime;
								}
							}
							else if (talkTimer < 2f)
							{
								talkTimer += Time.deltaTime;
								if (Random.Range(0, 120) == 25)
								{
									yield return StartCoroutine(NPCManager.manage.NPCDetails[myAi.myId.NPCNo].GetComponent<NpcBubbleTalk>().startRandomConvo(myAi, wantsToTalkTo));
									talkTimer = 2f;
								}
							}
							else
							{
								if ((bool)wantsToTalkTo && wantsToTalkTo.talkingTo == myAi.netId)
								{
									wantsToTalkTo.NetworktalkingTo = 0u;
								}
								if (myAi.talkingTo == wantsToTalkTo.netId)
								{
									myAi.NetworktalkingTo = 0u;
								}
								talkTimer = 0f;
								wantsToTalkTo = null;
							}
						}
						else
						{
							if ((bool)wantsToTalkTo && wantsToTalkTo.talkingTo == myAi.netId)
							{
								wantsToTalkTo.NetworktalkingTo = 0u;
							}
							if (myAi.talkingTo == wantsToTalkTo.netId)
							{
								myAi.NetworktalkingTo = 0u;
							}
							talkTimer = 0f;
							wantsToTalkTo = null;
						}
					}
					yield return null;
				}
				if ((bool)wantsToTalkTo)
				{
					if ((bool)wantsToTalkTo && wantsToTalkTo.talkingTo == myAi.netId)
					{
						wantsToTalkTo.NetworktalkingTo = 0u;
					}
					if (myAi.talkingTo == wantsToTalkTo.netId)
					{
						myAi.NetworktalkingTo = 0u;
					}
					wantsToTalkTo = null;
				}
				myAi.myAgent.Warp(walkToSitDownPos);
				TuckshopManager.manage.getUpFromSeat(interiorPointOfInterest.seatId);
				myAi.StandUpInside();
				myAi.myAgent.isStopped = false;
				GetComponent<stopNPCsPushing>().startSelf();
				yield return new WaitForSeconds(0.5f);
			}
		}
		currentTask = typeOfTask.None;
		hasTask = false;
	}

	public void lookForSomeoneToTalkToWhileSittingDown()
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, 5f, talkToLayer);
		if (array.Length == 0)
		{
			return;
		}
		int num = Random.Range(0, array.Length);
		if (array[num].transform.root != base.transform)
		{
			wantsToTalkTo = array[num].GetComponentInParent<NPCAI>();
			if (wantsToTalkTo.talkingTo != 0 || !wantsToTalkTo.isSitting)
			{
				wantsToTalkTo = null;
			}
		}
	}

	private IEnumerator walkToTaskInside()
	{
		while (hasTask)
		{
			if (myAi.talkingTo != 0)
			{
				yield return null;
			}
			yield return StartCoroutine(checkIfShouldGreet());
			if (!myAi.myManager.checkIfNPCIsFree() || !myAi.myManager.isInsideABuilding())
			{
				npcHolds.changeItemHolding(-1);
				currentTask = typeOfTask.None;
				taskPosition = Vector3.zero;
				hasTask = false;
				break;
			}
			if (Vector3.Distance(base.transform.position, taskPosition) < 2.5f || (currentTask == typeOfTask.FindingSeat && Vector3.Distance(base.transform.position, taskPosition) <= 6f) || (currentTask == typeOfTask.FindingAnimalToPet && Vector3.Distance(base.transform.position, taskPosition) <= 15f))
			{
				if (currentTask == typeOfTask.FindingSeat)
				{
					yield return StartCoroutine(sitDownOnInsideSeat());
				}
				else if (currentTask == typeOfTask.FindingSomeoneToTalkTo)
				{
					yield return StartCoroutine(talkToOtherNpc());
				}
			}
			else if (myAi.talkingTo == 0)
			{
				if (myAi.myAgent.isOnNavMesh)
				{
					if (currentTask != typeOfTask.FindingSeat && !myAi.canStillReachTaskLocation(taskPosition))
					{
						hasTask = false;
					}
					else if (myAi.myAgent.remainingDistance <= myAi.myAgent.stoppingDistance)
					{
						myAi.myAgent.SetDestination(taskPosition);
					}
				}
				else
				{
					hasTask = false;
				}
			}
			if (currentTask == typeOfTask.FindingSomeoneToTalkTo && (bool)wantsToTalkTo)
			{
				if (wantsToTalkTo.talkingTo != 0 || wantsToTalkTo.followingNetId != 0)
				{
					hasTask = false;
				}
				else
				{
					taskPosition = wantsToTalkTo.transform.position;
				}
			}
		}
	}

	private IEnumerator walkToTask()
	{
		while (hasTask)
		{
			if (myAi.talkingTo != 0)
			{
				yield return null;
			}
			if (!myAi.myManager.checkIfNPCIsFree() || myAi.myManager.isInsideABuilding())
			{
				npcHolds.changeItemHolding(-1);
				currentTask = typeOfTask.None;
				taskPosition = Vector3.zero;
				hasTask = false;
			}
			yield return StartCoroutine(checkIfShouldBeScared());
			yield return StartCoroutine(checkIfShouldGreet());
			if (Vector3.Distance(base.transform.position, taskPosition) < 2.5f || (currentTask == typeOfTask.FindingSeat && Vector3.Distance(base.transform.position, taskPosition) <= 6f) || (currentTask == typeOfTask.FindingAnimalToPet && Vector3.Distance(base.transform.position, taskPosition) <= 15f))
			{
				if (currentTask == typeOfTask.FindingCrop)
				{
					yield return StartCoroutine(waterPlant());
				}
				else if (currentTask == typeOfTask.FindingSeat)
				{
					yield return StartCoroutine(sitDownInSeat());
				}
				else if (currentTask == typeOfTask.FindingSomeoneToTalkTo)
				{
					yield return StartCoroutine(talkToOtherNpc());
				}
				else if (currentTask == typeOfTask.FindingAnimalToPet)
				{
					if (Vector3.Distance(base.transform.position, taskPosition) <= 2.5f)
					{
						yield return StartCoroutine(petAnimal());
					}
					else if (!myAi.canStillReachTaskLocation(taskPosition))
					{
						hasTask = false;
						randomiseTasks();
					}
				}
				else if (currentTask == typeOfTask.HavingASnack)
				{
					yield return StartCoroutine(haveASnack());
				}
				else if (currentTask == typeOfTask.BlowingPartyHorn)
				{
					yield return StartCoroutine(blowOnPartyHorn());
				}
				else if (currentTask == typeOfTask.HoldingBalloon)
				{
					yield return StartCoroutine(holdingBalloon());
				}
				else if (currentTask == typeOfTask.HoldingKite)
				{
					yield return StartCoroutine(HoldingKite());
				}
				else if (currentTask == typeOfTask.LookAtSkyAndClap)
				{
					yield return StartCoroutine(lookAtSkyAndClapServer(Random.Range(10, 25)));
				}
				else if (currentTask == typeOfTask.LookAtSkyAndGlee)
				{
					yield return StartCoroutine(LookAtSkyAndGleeServer(Random.Range(10, 25)));
				}
				else if (currentTask == typeOfTask.HuntingBugs)
				{
					yield return StartCoroutine(tryAndCatchTheBug());
				}
				else if (currentTask == typeOfTask.ClappingAtSky)
				{
					yield return StartCoroutine(tryAndCatchTheBug());
				}
			}
			else if (myAi.talkingTo == 0)
			{
				if (myAi.myAgent.isOnNavMesh)
				{
					if (currentTask != typeOfTask.FindingSeat && !myAi.canStillReachTaskLocation(taskPosition))
					{
						hasTask = false;
						randomiseTasks();
					}
					else if (myAi.myAgent.remainingDistance <= myAi.myAgent.stoppingDistance)
					{
						myAi.myAgent.SetDestination(taskPosition);
					}
				}
				else
				{
					hasTask = false;
				}
			}
			if (currentTask == typeOfTask.FindingSomeoneToTalkTo && (bool)wantsToTalkTo)
			{
				if (wantsToTalkTo.talkingTo != 0 || wantsToTalkTo.followingNetId != 0)
				{
					hasTask = false;
				}
				else
				{
					taskPosition = wantsToTalkTo.transform.position;
				}
			}
			if (currentTask == typeOfTask.HuntingBugs)
			{
				if (npcHolds.currentlyHolding != myBugNet)
				{
					npcHolds.changeItemHolding(myBugNet.getItemId());
				}
				if ((bool)wantToCatch)
				{
					taskPosition = wantToCatch.transform.position;
				}
				else
				{
					hasTask = false;
				}
			}
			if (currentTask == typeOfTask.FindingAnimalToPet && (bool)wantsToPet)
			{
				taskPosition = wantsToPet.transform.position;
			}
			if (currentTask == typeOfTask.FindingSeat && WorldManager.Instance.isSeatTaken(taskPosition))
			{
				hasTask = false;
			}
			if (currentTask == typeOfTask.FindingCrop && WorldManager.Instance.hasSquareBeenWatered(taskPosition))
			{
				hasTask = false;
			}
			yield return null;
		}
	}

	private IEnumerator checkIfShouldGreet()
	{
		int randomGreeting = Random.Range(0, NetworkNavMesh.nav.charsConnected.Count);
		if (!NPCManager.manage.npcStatus[myAi.myId.NPCNo].checkIfHasBeenGreeted(randomGreeting) && Vector3.Dot(NetworkNavMesh.nav.charsConnected[randomGreeting].transform.position - base.transform.position, base.transform.forward) >= 0.5f && Vector3.Distance(base.transform.position, NetworkNavMesh.nav.charsConnected[randomGreeting].position) < 8f)
		{
			NPCManager.manage.npcStatus[myAi.myId.NPCNo].greetCharacter(randomGreeting);
			yield return StartCoroutine(faceGreeting(NetworkNavMesh.nav.charsConnected[randomGreeting]));
			yield return StartCoroutine(greetCharacter(NetworkNavMesh.nav.charsConnected[randomGreeting]));
		}
	}

	private IEnumerator checkIfShouldBeScared()
	{
		Transform transform = returnClosestEnemy();
		if ((bool)transform)
		{
			NetworkisScared = true;
			myAi.myAgent.SetDestination(getSureRunPos(transform));
			float runTimer = 0f;
			while ((bool)transform)
			{
				if (myAi.checkIfHasArrivedAtDestination() || runTimer == 60f)
				{
					runTimer = 0f;
					myAi.myAgent.SetDestination(getSureRunPos(transform));
				}
				runTimer += 1f;
				yield return waitWhileInDanger;
				transform = returnClosestEnemy();
			}
		}
		if (isScared)
		{
			NetworkisScared = false;
		}
	}

	private IEnumerator faceCurrentTask()
	{
		myAi.myAgent.updateRotation = false;
		Vector3 normalized = (taskPosition - base.transform.position).normalized;
		Quaternion desiredRotation = Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z));
		float turnTimer = 0f;
		while (Mathf.Abs(Quaternion.Angle(myAi.myAgent.transform.rotation, desiredRotation)) > 2f && turnTimer < 8f && myAi.myAgent.isActiveAndEnabled)
		{
			turnTimer += Time.deltaTime;
			normalized = (taskPosition - base.transform.position).normalized;
			desiredRotation = Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z));
			myAi.myAgent.transform.rotation = Quaternion.RotateTowards(myAi.myAgent.transform.rotation, desiredRotation, 4f);
			yield return null;
		}
		if (turnTimer >= 8f)
		{
			hasTask = false;
		}
		myAi.myAgent.updateRotation = true;
	}

	private IEnumerator faceGreeting(Transform greeting)
	{
		myAi.myAgent.updateRotation = false;
		myAi.myAgent.isStopped = true;
		Vector3 normalized = (greeting.position - base.transform.position).normalized;
		Quaternion desiredRotation = Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z));
		float faceTimer = 3f;
		while (Mathf.Abs(Quaternion.Angle(myAi.myAgent.transform.rotation, desiredRotation)) > 2f && faceTimer > 0f && myAi.myAgent.isActiveAndEnabled)
		{
			normalized = (greeting.position - base.transform.position).normalized;
			desiredRotation = Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z));
			myAi.myAgent.transform.rotation = Quaternion.RotateTowards(myAi.myAgent.transform.rotation, desiredRotation, 4f);
			faceTimer -= Time.deltaTime;
			yield return null;
		}
		myAi.myAgent.updateRotation = true;
	}

	private IEnumerator greetCharacter(Transform greeting)
	{
		float greetTimer = 0f;
		myAi.myAgent.updateRotation = false;
		Vector3 normalized = (greeting.position - base.transform.position).normalized;
		Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z));
		RpcWave(greeting.GetComponent<NetworkIdentity>().netId);
		while (greetTimer <= 1f && myAi.myAgent.isActiveAndEnabled)
		{
			greetTimer += Time.deltaTime;
			normalized = (greeting.position - base.transform.position).normalized;
			Quaternion to = Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z));
			myAi.myAgent.transform.rotation = Quaternion.RotateTowards(myAi.myAgent.transform.rotation, to, 4f);
			yield return null;
		}
		myAi.myAgent.isStopped = false;
		myAi.myAgent.updateRotation = true;
	}

	private IEnumerator clapForCharacter(Transform greeting)
	{
		float greetTimer = 0f;
		myAi.myAgent.updateRotation = false;
		Vector3 normalized = (greeting.position - base.transform.position).normalized;
		Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z));
		RpcClap();
		while (greetTimer <= 2f && myAi.myAgent.isActiveAndEnabled)
		{
			greetTimer += Time.deltaTime;
			normalized = (greeting.position - base.transform.position).normalized;
			Quaternion to = Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z));
			myAi.myAgent.transform.rotation = Quaternion.RotateTowards(myAi.myAgent.transform.rotation, to, 4f);
			yield return null;
		}
		myAi.myAgent.isStopped = false;
		myAi.myAgent.updateRotation = true;
	}

	private IEnumerator talkToOtherNpc()
	{
		if (wantsToTalkTo.talkingTo == 0 && wantsToTalkTo.followingNetId == 0)
		{
			wantsToTalkTo.NetworktalkingTo = base.netId;
			myAi.NetworktalkingTo = wantsToTalkTo.netId;
			yield return new WaitForSeconds(Random.Range(2f, 5f));
			if ((bool)wantsToTalkTo)
			{
				wantsToTalkTo.NetworktalkingTo = 0u;
			}
			myAi.NetworktalkingTo = 0u;
		}
		if (TownEventManager.manage.isEventToday(TownEventManager.TownEventType.IslandDay) || TownEventManager.manage.isEventToday(TownEventManager.TownEventType.SkyFest))
		{
			int[] array = new int[5] { 1, 8, 13, 17, 12 };
			RpcPlayEmotion(array[Random.Range(0, array.Length)]);
			wantsToTalkTo.doesTask.RpcPlayEmotion(array[Random.Range(0, array.Length)]);
			yield return new WaitForSeconds(2f);
		}
		hasTask = false;
		yield return waitTime;
		if (!myAi.myManager.checkIfNPCIsFree() || Random.Range(0, 3) == 2)
		{
			currentTask = typeOfTask.None;
		}
	}

	[ClientRpc]
	public void RpcPlayEmotion(int emotionId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(emotionId);
		SendRPCInternal(typeof(NPCDoesTasks), "RpcPlayEmotion", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator animateEmotion(int emotionId)
	{
		myAi.faceAnim.setEmotionNo(emotionId);
		for (float timer = 2f; timer > 0f; timer -= Time.deltaTime)
		{
			yield return null;
		}
		myAi.faceAnim.stopEmotions();
	}

	private IEnumerator petAnimal()
	{
		myAi.myAgent.isStopped = true;
		yield return null;
		npcHolds.changeItemHolding(-1);
		yield return StartCoroutine(faceCurrentTask());
		if (hasTask)
		{
			RpcPatAnimation();
			wantsToPet.RpcPetAnimal();
			yield return taskGap;
			yield return taskGap;
			yield return taskGap;
			yield return taskGap;
			hasTask = false;
		}
		if (Random.Range(0, 3) == 2)
		{
			currentTask = typeOfTask.None;
		}
		wantsToPet = null;
		myAi.myAgent.isStopped = false;
	}

	private IEnumerator haveASnack()
	{
		if (myAi.myAgent.isActiveAndEnabled && myAi.myAgent.isOnNavMesh)
		{
			myAi.myAgent.isStopped = true;
			yield return taskGap;
			int num = 0;
			while (!Inventory.Instance.allItems[num].consumeable)
			{
				num = Random.Range(0, Inventory.Instance.allItems.Length);
			}
			npcHolds.changeItemHolding(num);
			yield return taskGap;
			npcHolds.NetworkusingItem = true;
			yield return taskGap;
			yield return taskGap;
			yield return taskGap;
			yield return taskGap;
			npcHolds.NetworkusingItem = false;
			npcHolds.changeItemHolding(-1);
			hasTask = false;
			myAi.myAgent.isStopped = false;
			currentTask = typeOfTask.None;
		}
	}

	private IEnumerator blowOnPartyHorn()
	{
		if (myAi.myAgent.isActiveAndEnabled && myAi.myAgent.isOnNavMesh)
		{
			myAi.myAgent.isStopped = true;
			yield return taskGap;
			npcHolds.changeItemHolding(partyHorn.getItemId());
			yield return taskGap;
			npcHolds.NetworkusingItem = true;
			yield return taskGap;
			yield return taskGap;
			yield return taskGap;
			yield return taskGap;
			npcHolds.NetworkusingItem = false;
			yield return taskGap;
			yield return taskGap;
			yield return taskGap;
			yield return new WaitForSeconds(2f);
			npcHolds.changeItemHolding(-1);
			myAi.myAgent.isStopped = false;
			hasTask = false;
			currentTask = typeOfTask.None;
		}
	}

	private IEnumerator holdingBalloon()
	{
		npcHolds.changeItemHolding(balloon.getItemId());
		yield return new WaitForSeconds(Random.Range(30, 60));
		hasTask = false;
		currentTask = typeOfTask.None;
	}

	public bool IsFlyingKite()
	{
		if (npcHolds.currentlyHolding == null)
		{
			return false;
		}
		if (npcHolds.currentlyHolding == NPCManager.manage.NPCDetails[myAi.myId.NPCNo].skyFestKite || npcHolds.currentlyHolding == kite)
		{
			return true;
		}
		return false;
	}

	private IEnumerator HoldingKite()
	{
		if ((bool)NPCManager.manage.NPCDetails[myAi.myId.NPCNo].skyFestKite)
		{
			npcHolds.changeItemHolding(NPCManager.manage.NPCDetails[myAi.myId.NPCNo].skyFestKite.getItemId());
		}
		else
		{
			npcHolds.changeItemHolding(kite.getItemId());
		}
		float holdingTimer = Random.Range(60, 180);
		while (holdingTimer > 0f)
		{
			yield return null;
			holdingTimer -= Time.deltaTime;
			if (myAi.myAgent.isOnNavMesh && myAi.talkingTo == 0 && myAi.myAgent.remainingDistance <= myAi.myAgent.stoppingDistance)
			{
				myAi.myAgent.SetDestination(myAi.getSureRandomPos());
			}
		}
		hasTask = false;
		currentTask = typeOfTask.None;
	}

	private IEnumerator lookAtSkyAndClapServer(int seconds)
	{
		myAi.myAgent.isStopped = true;
		RpcLookAtSkyAndClap(seconds);
		yield return new WaitForSeconds(seconds);
		hasTask = false;
		currentTask = typeOfTask.None;
		myAi.myAgent.isStopped = false;
	}

	private IEnumerator LookAtSkyAndGleeServer(int seconds)
	{
		myAi.myAgent.isStopped = true;
		RpcLookAtSkyAndGlee(seconds);
		yield return new WaitForSeconds(seconds);
		hasTask = false;
		currentTask = typeOfTask.None;
		myAi.myAgent.isStopped = false;
	}

	public IEnumerator lookAtSkyForSeconds(int seconds, int emoteNumber)
	{
		GetComponent<NPCIk>().lookAtSky = true;
		yield return new WaitForSeconds((float)seconds / 2f);
		StartCoroutine(animateEmotion(12));
		yield return new WaitForSeconds((float)seconds / 2f);
		GetComponent<NPCIk>().lookAtSky = false;
	}

	[ClientRpc]
	public void RpcLookAtSkyAndClap(int seconds)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(seconds);
		SendRPCInternal(typeof(NPCDoesTasks), "RpcLookAtSkyAndClap", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcLookAtSkyAndGlee(int seconds)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(seconds);
		SendRPCInternal(typeof(NPCDoesTasks), "RpcLookAtSkyAndGlee", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator attackTileForPlayer()
	{
		myAi.myAgent.isStopped = true;
		npcHolds.NetworkcurrentlyTargetingPos = new Vector2((int)taskPosition.x / 2, (int)taskPosition.z / 2);
		yield return StartCoroutine(faceCurrentTask());
		float attackTimer = 0.9f;
		if (hasTask)
		{
			npcHolds.NetworkusingItem = true;
			while (attackTimer > 0f && WorldManager.Instance.onTileMap[(int)taskPosition.x / 2, (int)taskPosition.z / 2] > -1 && WorldManager.Instance.onTileMap[(int)taskPosition.x / 2, (int)taskPosition.z / 2] != 30)
			{
				yield return StartCoroutine(faceCurrentTask());
				attackTimer = ((!following.myEquip.usingItem) ? (attackTimer - Time.deltaTime) : 0.9f);
				yield return null;
			}
			npcHolds.NetworkusingItem = false;
		}
		hasTask = false;
		myAi.myAgent.isStopped = false;
	}

	public void setFollowing(uint newFollowing)
	{
		if (base.isServer)
		{
			hasTask = false;
		}
		if (newFollowing == 0)
		{
			following = null;
		}
		else if (NetworkIdentity.spawned.ContainsKey(newFollowing))
		{
			following = NetworkIdentity.spawned[newFollowing].GetComponent<CharMovement>();
		}
		hasTask = false;
	}

	public void isInWater(bool isInWater)
	{
		inWater = isInWater;
	}

	[ClientRpc]
	private void RpcPatAnimation()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(NPCDoesTasks), "RpcPatAnimation", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcWave(uint greetingNetId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(greetingNetId);
		SendRPCInternal(typeof(NPCDoesTasks), "RpcWave", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcClap()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(NPCDoesTasks), "RpcClap", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator clapTimer()
	{
		GetComponent<Animator>().SetInteger("Emotion", 12);
		yield return new WaitForSeconds(2f);
		GetComponent<Animator>().SetInteger("Emotion", 0);
	}

	[ClientRpc]
	private void RpcNpcDoesDamageToTileObject()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(NPCDoesTasks), "RpcNpcDoesDamageToTileObject", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private Transform checkForEnemies()
	{
		if (Physics.CheckSphere(base.transform.position, 6f, myEnemies))
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position, 6f, myEnemies);
			if (array.Length != 0)
			{
				int num = 0;
				float num2 = 9f;
				bool flag = false;
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].transform.position.y > 0f && array[i].GetComponentInParent<AnimalAI>().isAttackingOrBeingAttackedBy(following.transform))
					{
						float num3 = Vector3.Distance(base.transform.position, array[i].transform.position);
						if (num3 < num2)
						{
							num = i;
							num2 = num3;
							flag = true;
						}
					}
				}
				if (flag)
				{
					return array[num].transform;
				}
			}
		}
		return null;
	}

	private IEnumerator tryAndCatchTheBug()
	{
		if (!wantToCatch)
		{
			yield break;
		}
		taskPosition = wantToCatch.transform.position;
		yield return StartCoroutine(faceCurrentTask());
		npcHolds.NetworkcurrentlyTargetingPos = new Vector2((int)taskPosition.x / 2, (int)taskPosition.z / 2);
		npcHolds.changeItemHolding(myBugNet.getItemId());
		npcHolds.NetworkusingItem = true;
		while ((bool)wantToCatch && wantToCatch.gameObject.activeInHierarchy && Vector3.Distance(base.transform.position, wantToCatch.transform.position) > 1.5f)
		{
			taskPosition = wantToCatch.transform.position;
			myAi.myAgent.SetDestination(taskPosition);
			if (!wantToCatch || Vector3.Distance(base.transform.position, wantToCatch.transform.position) > 3.5f)
			{
				break;
			}
			yield return null;
		}
		if ((bool)wantToCatch && wantToCatch.gameObject.activeInHierarchy)
		{
			taskPosition = wantToCatch.transform.position;
			yield return StartCoroutine(faceCurrentTask());
			yield return null;
		}
		npcHolds.NetworkusingItem = false;
		if (npcHolds.usingItem)
		{
			npcHolds.NetworkusingItem = false;
		}
		if (!wantToCatch || !wantToCatch.gameObject.activeInHierarchy)
		{
			hasTask = false;
		}
		float catchDelay2 = 0.5f;
		myAi.myAgent.isStopped = true;
		while (catchDelay2 > 0f)
		{
			catchDelay2 -= Time.deltaTime;
			yield return null;
		}
		if ((bool)npcHolds.currentlyHolding.bug)
		{
			catchDelay2 = 1.5f;
			while (catchDelay2 > 0f)
			{
				catchDelay2 -= Time.deltaTime;
				yield return null;
			}
		}
		myAi.myAgent.isStopped = false;
		if ((bool)wantToCatch && wantToCatch.gameObject.activeInHierarchy)
		{
			wantToCatch = null;
			hasTask = false;
		}
	}

	private IEnumerator attackEnemy()
	{
		if ((bool)wantsToAttack)
		{
			taskPosition = wantsToAttack.transform.position;
			yield return StartCoroutine(faceCurrentTask());
			myAi.myAgent.isStopped = true;
			npcHolds.NetworkcurrentlyTargetingPos = new Vector2((int)taskPosition.x / 2, (int)taskPosition.z / 2);
			npcHolds.changeItemHolding(myWeapon.getItemId());
			npcHolds.NetworkusingItem = true;
			while ((bool)wantsToAttack && wantsToAttack.gameObject.activeInHierarchy && wantsToAttack.getHealth() > 0 && Vector3.Distance(wantsToAttack.transform.position, base.transform.position) < 2.5f)
			{
				taskPosition = wantsToAttack.transform.position;
				yield return StartCoroutine(faceCurrentTask());
				yield return null;
			}
			npcHolds.NetworkusingItem = false;
			hasTask = false;
			myAi.myAgent.isStopped = false;
		}
	}

	public bool attacksCharacters(LayerMask checkMask)
	{
		return (int)checkMask == ((int)checkMask | 0x100);
	}

	public Transform returnClosestEnemy(float multi = 1f)
	{
		if (Physics.CheckSphere(base.transform.position + base.transform.forward * scaredDistance / 4f, scaredDistance * multi, myEnemies))
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position + base.transform.forward * scaredDistance / 4f, scaredDistance * multi, myEnemies);
			if (array.Length != 0)
			{
				int num = 0;
				float num2 = 2000f;
				bool flag = false;
				for (int i = 0; i < array.Length; i++)
				{
					if (!(array[i].transform.position.y > 0f))
					{
						continue;
					}
					AnimalAI_Attack componentInParent = array[i].GetComponentInParent<AnimalAI_Attack>();
					if ((bool)componentInParent && attacksCharacters(componentInParent.myPrey) && !componentInParent.GetComponent<AnimalAI>().waterOnly && WorldManager.Instance.isPositionInSameFencedArea(componentInParent.transform.position, base.transform.position))
					{
						float num3 = Vector3.Distance(base.transform.position, array[i].transform.position);
						if (num3 < num2)
						{
							num = i;
							num2 = num3;
							flag = true;
						}
					}
				}
				if (flag)
				{
					return array[num].transform.root;
				}
			}
		}
		return null;
	}

	public Vector3 getSureRunPos(Transform runningFrom)
	{
		Vector3 vector = base.transform.position;
		for (int i = 0; i < 500; i++)
		{
			Vector3 vector2 = new Vector3(runningFrom.position.x, base.transform.position.y, runningFrom.position.z);
			vector = base.transform.position + (base.transform.position - vector2).normalized * scaredDistance;
			vector += new Vector3(Random.Range(0f - scaredDistance, scaredDistance), 0f, Random.Range(0f - scaredDistance, scaredDistance));
			if (WorldManager.Instance.isPositionOnMap(vector))
			{
				vector.y = WorldManager.Instance.heightMap[Mathf.RoundToInt(vector.x / 2f), Mathf.RoundToInt(vector.z / 2f)];
			}
			vector = myAi.checkDestination(vector);
			if (vector != base.transform.position)
			{
				break;
			}
		}
		if (vector == base.transform.position)
		{
			Vector3 vector3 = new Vector3(runningFrom.position.x, base.transform.position.y, runningFrom.position.z) + new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
			vector = base.transform.position + (base.transform.position - vector3) * scaredDistance + new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(-8f, 8f));
		}
		return vector;
	}

	private void OnGetScared(bool oldScared, bool newScared)
	{
		NetworkisScared = newScared;
		myAi.myAnim.SetBool("Scared", newScared);
		if (newScared)
		{
			int num = Random.Range(0, 6);
			myAi.chatBubble.tryAndTalk(ConversationGenerator.generate.GetNPCBubbleText("Fear_" + num), 2f, overrideOldBubble: true);
		}
		else
		{
			int num2 = Random.Range(0, 6);
			myAi.chatBubble.tryAndTalk(ConversationGenerator.generate.GetNPCBubbleText("Saftey_" + num2), 2f, overrideOldBubble: true);
		}
	}

	private IEnumerator followingRoutine()
	{
		while ((bool)following)
		{
			yield return null;
			if (inWater)
			{
				npcHolds.changeItemHolding(-1);
			}
			if (hasTask)
			{
				if (Vector3.Distance(following.transform.position, base.transform.position) > 12f)
				{
					clearTask();
					continue;
				}
				if (Vector3.Distance(base.transform.position, taskPosition) <= 2.5f || (currentTask == typeOfTask.FollowingAndDiggingTreasure && Vector3.Distance(base.transform.position, taskPosition) <= 3f))
				{
					if (currentTask == typeOfTask.FollowingAndDiggingTreasure)
					{
						yield return StartCoroutine(digUp());
						clearTask();
					}
					else if (currentTask == typeOfTask.FollowingAndWatering)
					{
						yield return StartCoroutine(waterPlant());
						clearTask();
					}
					else if (currentTask == typeOfTask.FollowingAndAttacking)
					{
						yield return StartCoroutine(attackEnemy());
					}
					else if (currentTask == typeOfTask.FollowingAndHarvesting)
					{
						yield return StartCoroutine(attackTileForPlayer());
					}
					continue;
				}
				if (!myAi.canStillReachTaskLocation(taskPosition))
				{
					clearTask();
				}
				else if (currentTask == typeOfTask.FollowingAndDiggingTreasure)
				{
					if (following.myEquip.currentlyHoldingItemId != metalDetector.getItemId())
					{
						watchingDetector = null;
						clearTask();
					}
				}
				else if (currentTask == typeOfTask.FollowingAndAttacking)
				{
					if ((bool)wantsToAttack)
					{
						if (wantsToAttack.gameObject.activeInHierarchy && wantsToAttack.getHealth() > 0)
						{
							npcHolds.changeItemHolding(myWeapon.getItemId());
							taskPosition = wantsToAttack.transform.position;
							hasTask = true;
						}
						else
						{
							wantsToAttack = null;
							clearTask();
						}
					}
					else
					{
						currentTask = typeOfTask.None;
						hasTask = false;
					}
				}
				else if (WorldManager.Instance.onTileMap[(int)taskPosition.x / 2, (int)taskPosition.z / 2] == -1 || WorldManager.Instance.onTileMap[(int)taskPosition.x / 2, (int)taskPosition.z / 2] == 30)
				{
					clearTask();
				}
				if (myAi.talkingTo != 0)
				{
					continue;
				}
				if (myAi.myAgent.isOnNavMesh)
				{
					if (myAi.myAgent.remainingDistance <= myAi.myAgent.stoppingDistance)
					{
						myAi.myAgent.SetDestination(taskPosition);
					}
				}
				else
				{
					hasTask = false;
				}
				continue;
			}
			if ((bool)following && watchPlayerHoldingId != following.myEquip.currentlyHoldingItemId && (bool)following.myEquip.itemCurrentlyHolding && ((bool)following.myEquip.itemCurrentlyHolding.fish || (bool)following.myEquip.itemCurrentlyHolding.bug))
			{
				npcHolds.changeItemHolding(-1);
				yield return StartCoroutine(clapForCharacter(following.transform));
			}
			checkWhatPlayerIsHoldingAndChangeItem();
			if (npcHolds.currentlyHolding == myShovel)
			{
				if (following.myEquip.currentlyHoldingItemId != metalDetector.getItemId())
				{
					clearTask();
				}
				else if ((bool)watchingDetector && watchingDetector.foundSomething)
				{
					currentTask = typeOfTask.FollowingAndDiggingTreasure;
					taskPosition = watchingDetector.getLastCheckPositionForNPCFollow();
					hasTask = true;
				}
				continue;
			}
			if (npcHolds.currentlyHolding == myWaterCan)
			{
				currentTask = typeOfTask.FollowingAndWatering;
				Vector3 vector = WorldManager.Instance.findClosestTileObjectAround(base.transform.position, TownManager.manage.allCropsTypes, 9, checkIfWatered: true);
				if (vector != Vector3.zero)
				{
					taskPosition = vector;
					hasTask = true;
				}
				continue;
			}
			Transform transform = checkForEnemies();
			if ((bool)transform)
			{
				wantsToAttack = transform.GetComponent<AnimalAI>();
				if ((bool)wantsToAttack)
				{
					taskPosition = wantsToAttack.transform.position;
					hasTask = true;
					npcHolds.changeItemHolding(myWeapon.getItemId());
					currentTask = typeOfTask.FollowingAndAttacking;
				}
			}
			else if (WorldManager.Instance.onTileMap[(int)following.myInteract.currentlyAttackingPos.x, (int)following.myInteract.currentlyAttackingPos.y] != -1 && following.myEquip.usingItem)
			{
				currentTask = typeOfTask.FollowingAndHarvesting;
				taskPosition = new Vector3((int)following.myInteract.currentlyAttackingPos.x * 2, WorldManager.Instance.heightMap[(int)following.myInteract.currentlyAttackingPos.x, (int)following.myInteract.currentlyAttackingPos.y], (int)following.myInteract.currentlyAttackingPos.y * 2);
				hasTask = true;
			}
		}
	}

	public void clearTask()
	{
		currentTask = typeOfTask.None;
		hasTask = false;
	}

	public void checkWhatPlayerIsHoldingAndChangeItem()
	{
		if (following.myEquip.currentlyHoldingItemId == watchPlayerHoldingId)
		{
			return;
		}
		npcHolds.changeItemHolding(-1);
		watchingDetector = null;
		if (following.myEquip.currentlyHoldingItemId > -1)
		{
			if ((bool)Inventory.Instance.allItems[following.myEquip.currentlyHoldingItemId].consumeable || (bool)Inventory.Instance.allItems[following.myEquip.currentlyHoldingItemId].placeable)
			{
				watchPlayerHoldingId = following.myEquip.currentlyHoldingItemId;
				return;
			}
			if (following.myEquip.currentlyHoldingItemId == metalDetector.getItemId())
			{
				npcHolds.changeItemHolding(myShovel.getItemId());
				watchingDetector = following.myEquip.holdingPrefab.GetComponent<MetalDetectorUse>();
			}
			else if ((bool)wantsToAttack)
			{
				npcHolds.changeItemHolding(myWeapon.getItemId());
			}
			else
			{
				for (int i = 0; i < playerAxes.Length; i++)
				{
					if (following.myEquip.currentlyHoldingItemId == playerAxes[i].getItemId())
					{
						npcHolds.changeItemHolding(myAxe.getItemId());
						watchPlayerHoldingId = following.myEquip.currentlyHoldingItemId;
						return;
					}
				}
				for (int j = 0; j < playerWeapons.Length; j++)
				{
					if (following.myEquip.currentlyHoldingItemId == playerWeapons[j].getItemId())
					{
						npcHolds.changeItemHolding(myWeapon.getItemId());
						watchPlayerHoldingId = following.myEquip.currentlyHoldingItemId;
						return;
					}
				}
				for (int k = 0; k < playerPickaxes.Length; k++)
				{
					if (following.myEquip.currentlyHoldingItemId == playerPickaxes[k].getItemId())
					{
						npcHolds.changeItemHolding(myPickaxe.getItemId());
						watchPlayerHoldingId = following.myEquip.currentlyHoldingItemId;
						return;
					}
				}
				for (int l = 0; l < playerWateringCans.Length; l++)
				{
					if (following.myEquip.currentlyHoldingItemId == playerWateringCans[l].getItemId())
					{
						npcHolds.changeItemHolding(myWaterCan.getItemId());
						watchPlayerHoldingId = following.myEquip.currentlyHoldingItemId;
						return;
					}
				}
			}
		}
		watchPlayerHoldingId = following.myEquip.currentlyHoldingItemId;
	}

	static NPCDoesTasks()
	{
		waitWhileInDanger = new WaitForSeconds(0.05f);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NPCDoesTasks), "RpcPlayEmotion", InvokeUserCode_RpcPlayEmotion);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NPCDoesTasks), "RpcLookAtSkyAndClap", InvokeUserCode_RpcLookAtSkyAndClap);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NPCDoesTasks), "RpcLookAtSkyAndGlee", InvokeUserCode_RpcLookAtSkyAndGlee);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NPCDoesTasks), "RpcPatAnimation", InvokeUserCode_RpcPatAnimation);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NPCDoesTasks), "RpcWave", InvokeUserCode_RpcWave);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NPCDoesTasks), "RpcClap", InvokeUserCode_RpcClap);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NPCDoesTasks), "RpcNpcDoesDamageToTileObject", InvokeUserCode_RpcNpcDoesDamageToTileObject);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlayEmotion(int emotionId)
	{
		StartCoroutine(animateEmotion(emotionId));
	}

	protected static void InvokeUserCode_RpcPlayEmotion(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayEmotion called on server.");
		}
		else
		{
			((NPCDoesTasks)obj).UserCode_RpcPlayEmotion(reader.ReadInt());
		}
	}

	protected void UserCode_RpcLookAtSkyAndClap(int seconds)
	{
		StartCoroutine(lookAtSkyForSeconds(seconds, 12));
	}

	protected static void InvokeUserCode_RpcLookAtSkyAndClap(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcLookAtSkyAndClap called on server.");
		}
		else
		{
			((NPCDoesTasks)obj).UserCode_RpcLookAtSkyAndClap(reader.ReadInt());
		}
	}

	protected void UserCode_RpcLookAtSkyAndGlee(int seconds)
	{
		StartCoroutine(lookAtSkyForSeconds(seconds, 8));
	}

	protected static void InvokeUserCode_RpcLookAtSkyAndGlee(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcLookAtSkyAndGlee called on server.");
		}
		else
		{
			((NPCDoesTasks)obj).UserCode_RpcLookAtSkyAndGlee(reader.ReadInt());
		}
	}

	protected void UserCode_RpcPatAnimation()
	{
		GetComponent<Animator>().SetTrigger("Pet");
	}

	protected static void InvokeUserCode_RpcPatAnimation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPatAnimation called on server.");
		}
		else
		{
			((NPCDoesTasks)obj).UserCode_RpcPatAnimation();
		}
	}

	protected void UserCode_RpcWave(uint greetingNetId)
	{
		EquipItemToChar componentInParent = NetworkIdentity.spawned[greetingNetId].GetComponentInParent<EquipItemToChar>();
		if ((bool)componentInParent)
		{
			if (TownEventManager.manage.townEventOn == TownEventManager.TownEventType.None)
			{
				int num = Random.Range(0, 5);
				myAi.chatBubble.tryAndTalk(string.Format(ConversationGenerator.generate.GetNPCBubbleText("Greeting_" + num), componentInParent.playerName), 2f);
			}
			else if (TownEventManager.manage.townEventOn == TownEventManager.TownEventType.IslandDay)
			{
				int num2 = Random.Range(0, 2);
				myAi.chatBubble.tryAndTalk(string.Format(ConversationGenerator.generate.GetNPCBubbleText("IslandDayGreeting_" + num2), NetworkMapSharer.Instance.islandName, componentInParent.playerName, 2f));
			}
			else if (TownEventManager.manage.townEventOn == TownEventManager.TownEventType.SkyFest)
			{
				int num3 = Random.Range(0, 4);
				myAi.chatBubble.tryAndTalk(string.Format(ConversationGenerator.generate.GetNPCBubbleText("SkyFestGreeting_" + num3), componentInParent.playerName), 2f);
			}
		}
		GetComponent<Animator>().SetInteger("Emotion", 4);
	}

	protected static void InvokeUserCode_RpcWave(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcWave called on server.");
		}
		else
		{
			((NPCDoesTasks)obj).UserCode_RpcWave(reader.ReadUInt());
		}
	}

	protected void UserCode_RpcClap()
	{
		StartCoroutine(clapTimer());
	}

	protected static void InvokeUserCode_RpcClap(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClap called on server.");
		}
		else
		{
			((NPCDoesTasks)obj).UserCode_RpcClap();
		}
	}

	protected void UserCode_RpcNpcDoesDamageToTileObject()
	{
		TileObject tileObject = WorldManager.Instance.findTileObjectInUse((int)npcHolds.currentlyTargetingPos.x, (int)npcHolds.currentlyTargetingPos.y);
		if ((bool)tileObject && following.myInteract.CheckIfCanDamage(npcHolds.currentlyTargetingPos))
		{
			tileObject.damage();
			tileObject.currentHealth -= npcHolds.currentlyHolding.damagePerAttack;
			Vector3 position = tileObject.transform.position;
			ParticleManager.manage.emitAttackParticle(position);
			if (tileObject.currentHealth <= 0f)
			{
				following.myInteract.FollowingNPCKillsItem((int)npcHolds.currentlyTargetingPos.x, (int)npcHolds.currentlyTargetingPos.y, tileObject);
			}
		}
	}

	protected static void InvokeUserCode_RpcNpcDoesDamageToTileObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcNpcDoesDamageToTileObject called on server.");
		}
		else
		{
			((NPCDoesTasks)obj).UserCode_RpcNpcDoesDamageToTileObject();
		}
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(isScared);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(isScared);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			bool flag = isScared;
			NetworkisScared = reader.ReadBool();
			if (!SyncVarEqual(flag, ref isScared))
			{
				OnGetScared(flag, isScared);
			}
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			bool flag2 = isScared;
			NetworkisScared = reader.ReadBool();
			if (!SyncVarEqual(flag2, ref isScared))
			{
				OnGetScared(flag2, isScared);
			}
		}
	}
}
