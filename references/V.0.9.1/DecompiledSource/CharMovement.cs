using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class CharMovement : NetworkBehaviour
{
	public Transform charRendererTransform;

	public CharInteract myInteract;

	public EquipItemToChar myEquip;

	public CharPickUp myPickUp;

	public CharTalkUse myTalkUse;

	public Animator myAnim;

	private float runningMultipier;

	private float animSpeed;

	public LayerMask jumpLayers;

	public LayerMask autoWalkLayer;

	public LayerMask swimLayers;

	public LayerMask vehicleLayers;

	private Transform cameraContainer;

	public CapsuleCollider col;

	public bool grounded;

	public bool swimming;

	[SyncVar(hook = "onChangeUnderWater")]
	public bool underWater;

	private float pickUpTimer;

	private float pickUpTileObjectTimer;

	private Vector3 lasHighLighterPos = Vector3.zero;

	private bool attackLock;

	private bool moveLockRotateSlow;

	private bool rotationLock;

	private bool sneaking;

	public bool localUsing;

	public GameObject underWaterHit;

	public Vehicle driving;

	public Vehicle standingOnVehicle;

	public Transform standingOnTrans;

	[SyncVar(hook = "onChangeStamina")]
	public int stamina = 50;

	[SyncVar]
	public uint standingOn;

	public uint localStandingOn;

	public LayerMask myEnemies;

	public int followedBy = -1;

	private RuntimeAnimatorController defaultController;

	private NetworkFishingRod myRod;

	public GameObject reviveBox;

	public bool localBlocking;

	public Transform wallCheck1;

	public Transform wallCheck2;

	public bool usingHangGlider;

	public bool usingBoogieBoard;

	[SyncVar]
	private bool inDanger;

	private bool lastSwimming;

	public InventoryItem divingHelmet;

	public NetworkTransform normalNetworkTransform;

	public NetworkTransform standingOnNetworkTransform;

	public Vector3 rendererOffset = new Vector3(0f, 0.18f, 0f);

	private Vector3 followVel = Vector3.one;

	private Vector3 clerpedPos;

	public float lerpRendererTransformSpeed = 0.1f;

	private readonly float maxLerpRendererTransformSpeed = 100f;

	private bool climbingLadder;

	[SyncVar]
	public bool climbing;

	public bool stunned;

	public bool canClimb = true;

	private float jumpDif;

	private float swimDif = 1f;

	private float swimBuff;

	private float swimSpeedItem;

	private float runDif;

	private WaitForSeconds passengerWait = new WaitForSeconds(0.05f);

	private static WaitForFixedUpdate jumpWait;

	public float jumpUpHeight = 3f;

	public float fallSpeed = -1f;

	private bool jumpFalling;

	private bool facingTarget;

	public bool isCurrentlyTalking;

	private NPCSchedual.Locations currentlyInsideBuilding;

	private bool animatedTired;

	private bool beingKnockedBack;

	private WaitForSeconds swimWait = new WaitForSeconds(0.25f);

	private bool landedInWater;

	private bool canKick = true;

	private List<AnimalAI_Attack> currentlyInDangerOf = new List<AnimalAI_Attack>();

	private Coroutine InDangerCheck;

	public Rigidbody MyRigidBody { get; private set; }

	public float CurrentSpeed { get; private set; }

	public bool InJump { get; private set; }

	public bool NetworkunderWater
	{
		get
		{
			return underWater;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref underWater))
			{
				bool old = underWater;
				SetSyncVar(value, ref underWater, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					onChangeUnderWater(old, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public int Networkstamina
	{
		get
		{
			return stamina;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref stamina))
			{
				int oldStam = stamina;
				SetSyncVar(value, ref stamina, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, value: true);
					onChangeStamina(oldStam, value);
					setSyncVarHookGuard(2uL, value: false);
				}
			}
		}
	}

	public uint NetworkstandingOn
	{
		get
		{
			return standingOn;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref standingOn))
			{
				uint num = standingOn;
				SetSyncVar(value, ref standingOn, 4uL);
			}
		}
	}

	public bool NetworkinDanger
	{
		get
		{
			return inDanger;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref inDanger))
			{
				bool flag = inDanger;
				SetSyncVar(value, ref inDanger, 8uL);
			}
		}
	}

	public bool Networkclimbing
	{
		get
		{
			return climbing;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref climbing))
			{
				bool flag = climbing;
				SetSyncVar(value, ref climbing, 16uL);
			}
		}
	}

	private void Start()
	{
		col = GetComponent<CapsuleCollider>();
		MyRigidBody = GetComponent<Rigidbody>();
		myAnim = GetComponent<Animator>();
		myRod = GetComponent<NetworkFishingRod>();
		defaultController = myAnim.runtimeAnimatorController;
		MyRigidBody.interpolation = RigidbodyInterpolation.Interpolate;
		clerpedPos = base.transform.position + rendererOffset;
	}

	public override void OnStartServer()
	{
		NetworkstandingOn = 0u;
		NetworkMapSharer.Instance.RpcSyncDate(WorldManager.Instance.day, WorldManager.Instance.week, WorldManager.Instance.month, WorldManager.Instance.year, RealWorldTimeLight.time.currentMinute);
		NetworkPlayersManager.manage.addPlayer(this);
		TargetCheckVersion(base.connectionToClient, Inventory.Instance.allItems.Length, WorldManager.Instance.allObjects.Length);
	}

	public override void OnStopServer()
	{
		NetworkPlayersManager.manage.removePlayer(this);
	}

	public override void OnStartLocalPlayer()
	{
		MyRigidBody = GetComponent<Rigidbody>();
		NetworkMapSharer.Instance.localChar = this;
		Inventory.Instance.CheckIfBagInInventory();
		cameraContainer = CameraController.control.transform;
		StatusManager.manage.connectPlayer(GetComponent<Damageable>());
		RenderMap.Instance.ConnectMainChar(base.transform);
		lockClientOnLoad();
		RenderMap.Instance.unTrackOtherPlayers(base.transform);
		myEquip.CmdEquipNewItem(Inventory.Instance.invSlots[Inventory.Instance.selectedSlot].itemNo);
		HouseDetails houseDetails = null;
		if (TownManager.manage.savedInside[0] != -1 && TownManager.manage.savedInside[1] != -1)
		{
			houseDetails = HouseManager.manage.getHouseInfoIfExists(TownManager.manage.savedInside[0], TownManager.manage.savedInside[1]);
		}
		if (base.isServer && houseDetails != null)
		{
			myInteract.ChangeInsideOut(isEntry: true, houseDetails);
			WeatherManager.Instance.ChangeToInsideEnvironment(MusicManager.indoorMusic.Default);
			RealWorldTimeLight.time.goInside();
			MusicManager.manage.ChangeCharacterInsideOrOutside(newInside: true, MusicManager.indoorMusic.Default);
			Inventory.Instance.equipNewSelectedSlot();
			StatusManager.manage.addBuff(StatusManager.BuffType.wellrested, 600, 1);
		}
		else if (base.transform.position.y <= -12f)
		{
			WeatherManager.Instance.ChangeToInsideEnvironment(MusicManager.indoorMusic.Default);
			RealWorldTimeLight.time.goInside();
			myEquip.setInsideOrOutside(insideOrOut: true, playersHouse: false);
			MusicManager.manage.ChangeCharacterInsideOrOutside(newInside: true, MusicManager.indoorMusic.Default);
		}
		NetworkMapSharer.Instance.personalSpawnPoint = base.transform.position;
		base.transform.eulerAngles = new Vector3(0f, TownManager.manage.savedRot, 0f);
		StartCoroutine(swimmingAndDivingStamina());
		if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.offIsland)
		{
			CmdRequestEntranceMapIcon();
		}
	}

	public override void OnStartClient()
	{
		col = GetComponent<CapsuleCollider>();
		MyRigidBody = GetComponent<Rigidbody>();
		myAnim = GetComponent<Animator>();
		NetworkNavMesh.nav.addAPlayer(base.transform);
		updateStandingOn(standingOn);
		if (!base.isLocalPlayer)
		{
			MyRigidBody.isKinematic = true;
		}
	}

	public override void OnStopClient()
	{
		if (base.isServer)
		{
			NetworkNavMesh.nav.removeSleepingChar(base.transform);
		}
		NetworkNavMesh.nav.removeAPlayer(base.transform);
	}

	private void Update()
	{
		if (!myEquip.isInVehicle())
		{
			if ((bool)myEquip.itemCurrentlyHolding && myEquip.itemCurrentlyHolding.isATool)
			{
				if (base.isLocalPlayer)
				{
					myAnim.SetBool(CharNetworkAnimator.usingAnimName, localUsing);
				}
				else
				{
					myAnim.SetBool(CharNetworkAnimator.usingAnimName, myEquip.usingItem);
				}
			}
			else
			{
				myAnim.SetBool(CharNetworkAnimator.usingAnimName, value: false);
			}
		}
		if (!base.isLocalPlayer)
		{
			return;
		}
		lastSwimming = swimming;
		swimming = Physics.CheckSphere(base.transform.position, 0.1f, swimLayers);
		if (!swimming && WorldManager.Instance.checkIfUnderWater(base.transform.position) && EquipWindow.equip.hatSlot.itemNo != divingHelmet.getItemId())
		{
			changeToUnderWater();
		}
		if (!lastSwimming && swimming)
		{
			StartCoroutine(landInWaterTimer());
		}
		if (!climbingLadder)
		{
			grounded = Physics.CheckSphere(base.transform.position + Vector3.up * 0.3f, 0.6f, jumpLayers);
		}
		else
		{
			grounded = true;
		}
		if (underWater)
		{
			if (EquipWindow.equip.hatSlot.itemNo == divingHelmet.getItemId())
			{
				swimming = false;
				myEquip.setSwimming(newSwimming: false);
			}
			else
			{
				myEquip.setSwimming(swimming);
			}
			float num = -2f;
			if (WorldManager.Instance.isPositionOnMap(base.transform.position))
			{
				num = WorldManager.Instance.heightMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)];
			}
			if (base.transform.position.y < num)
			{
				base.transform.position = new Vector3(base.transform.position.x, num, base.transform.position.z);
			}
		}
		else
		{
			myEquip.setSwimming(swimming);
		}
		myAnim.SetBool(CharNetworkAnimator.groundedAnimName, grounded);
		myAnim.SetBool(CharNetworkAnimator.climbingAnimName, climbingLadder);
		if (base.transform.position.y < -1000f)
		{
			MyRigidBody.velocity = Vector3.zero;
			base.transform.position = new Vector3(base.transform.position.x, 10f, base.transform.position.z);
			NewChunkLoader.loader.inside = false;
			myInteract.ChangeInsideOut(isEntry: false);
			WeatherManager.Instance.ChangeToOutsideEnvironment();
			RealWorldTimeLight.time.goOutside();
			myEquip.setInsideOrOutside(insideOrOut: false, playersHouse: false);
		}
		if (StatusManager.manage.dead)
		{
			if (localUsing)
			{
				myEquip.CmdUsingItem(isUsing: false);
				localUsing = false;
			}
			if (underWater)
			{
				MyRigidBody.useGravity = true;
				NetworkunderWater = false;
				col.enabled = true;
				underWaterHit.SetActive(value: false);
				SoundManager.Instance.switchUnderWater(newUnderWater: false);
				myAnim.SetBool(CharNetworkAnimator.underwaterAnimName, value: false);
				CmdChangeUnderWater(newUnderWater: false);
			}
			myEquip.animateOnUse(beingUsed: false, blocking: false);
			if ((double)base.transform.position.y < 0.5 && WorldManager.Instance.waterMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)])
			{
				MyRigidBody.isKinematic = true;
				base.transform.position = Vector3.Lerp(base.transform.position, new Vector3(base.transform.position.x, 0.5f, base.transform.position.z), Time.deltaTime / 2f);
			}
			return;
		}
		if (myPickUp.sitting)
		{
			if (!myInteract.IsPlacingDeed && InputMaster.input.Other())
			{
				myPickUp.pressY();
			}
			if ((bool)myEquip.itemCurrentlyHolding && (bool)myEquip.itemCurrentlyHolding.consumeable)
			{
				checkUseItemButton();
			}
			return;
		}
		if (PhotoManager.manage.cameraViewOpen && !PhotoManager.manage.usingTripod && (InputMaster.input.Other() || InputMaster.input.Journal() || InputMaster.input.OpenInventory()))
		{
			myEquip.holdingPrefabAnimator.SetTrigger("CloseCamera");
			CmdCloseCamera();
		}
		if (!Inventory.Instance.CanMoveCharacter())
		{
			if (localUsing && !myEquip.isWhistling())
			{
				localUsing = false;
				myEquip.CmdUsingItem(isUsing: false);
			}
			myEquip.animateOnUse(beingUsed: false, blocking: false);
			return;
		}
		if (underWater)
		{
			if (InputMaster.input.JumpHeld())
			{
				MyRigidBody.MovePosition(MyRigidBody.position + Vector3.up * 3f * Time.deltaTime);
			}
			if (base.transform.position.y >= -0.38f)
			{
				changeToAboveWater();
			}
		}
		if (!myEquip.isInVehicle() && InputMaster.input.Whistle())
		{
			myEquip.CharWhistles();
		}
		if (!myInteract.IsPlacingDeed && !myRod.lineIsCasted && InputMaster.input.Jump() && !underWater)
		{
			if (grounded && !InJump)
			{
				InJump = true;
				StartCoroutine(jumpFeel());
			}
			else if ((swimming && usingBoogieBoard && !InJump) || (swimming && !InJump && Physics.Raycast(base.transform.position + base.transform.forward * 1.5f + Vector3.up * 3f, Vector3.down, 3f, jumpLayers)))
			{
				InJump = true;
				StartCoroutine(jumpFeel());
			}
		}
		if (InputMaster.input.Interact() && !myInteract.IsPlacingDeed && !myRod.lineIsCasted)
		{
			if (!myPickUp.isCarryingSomething())
			{
				if (!myPickUp.pickUp())
				{
					myInteract.DoTileInteraction();
				}
			}
			else
			{
				myPickUp.pickUp();
			}
		}
		if (InputMaster.input.Other() && !myPickUp.drivingVehicle && !PhotoManager.manage.cameraViewOpen)
		{
			if (myInteract.tileHighlighter.position != lasHighLighterPos)
			{
				pickUpTileObjectTimer = 0f;
				lasHighLighterPos = myInteract.tileHighlighter.position;
			}
			if (!myInteract.RotateObjectBeingPlacedPreview())
			{
				myInteract.pickUpTileObject();
			}
		}
		if (localUsing || !myEquip.itemCurrentlyHolding || !myEquip.itemCurrentlyHolding.canBlock)
		{
			if (localBlocking)
			{
				CmdChangeBlocking(isBlocking: false);
				localBlocking = false;
			}
			if (InputMaster.input.Other() && swimming && !underWater)
			{
				if (EquipWindow.equip.hatSlot.itemNo == divingHelmet.getItemId())
				{
					swimming = false;
				}
				changeToUnderWater();
			}
			if (InputMaster.input.InteractHeld())
			{
				if (pickUpTimer < 1f)
				{
					pickUpTimer += Time.deltaTime;
				}
				if (pickUpTimer > 0.25f)
				{
					myPickUp.holdingPickUp = true;
				}
			}
			else
			{
				myPickUp.holdingPickUp = false;
				pickUpTimer = 0f;
			}
		}
		if (InputMaster.input.Use())
		{
			if (PhotoManager.manage.cameraViewOpen)
			{
				if ((bool)myEquip.itemCurrentlyHolding && !(myEquip.itemCurrentlyHolding.itemName == "Camera"))
				{
				}
			}
			else if (!myEquip.IsDriving())
			{
				myPickUp.pressX();
			}
		}
		if (InputMaster.input.Other())
		{
			myPickUp.pressY();
		}
		checkUseItemButton();
	}

	public void checkUseItemButton()
	{
		if (PhotoManager.manage.cameraViewOpen)
		{
			return;
		}
		if (InputMaster.input.Use() && myEquip.needsHandPlaceable())
		{
			localUsing = false;
			myEquip.CmdUsingItem(isUsing: false);
			StartCoroutine(replaceHandPlaceableDelay());
		}
		else if ((InputMaster.input.UseHeld() && StatusManager.manage.CanSwingWithStamina()) || myEquip.isWhistling())
		{
			if (myInteract.GetSelectedTileNeedsServerRefresh())
			{
				myInteract.CmdCurrentlyAttackingPos((int)myInteract.selectedTile.x, (int)myInteract.selectedTile.y);
			}
			if (!localUsing)
			{
				localUsing = true;
				myEquip.CmdUsingItem(isUsing: true);
			}
			myEquip.animateOnUse(beingUsed: true, localBlocking);
		}
		else
		{
			if (localUsing)
			{
				localUsing = false;
				myEquip.CmdUsingItem(isUsing: false);
			}
			myEquip.animateOnUse(beingUsed: false, localBlocking);
		}
	}

	private void LateUpdate()
	{
		if (!base.isLocalPlayer && myPickUp.sittingPos != Vector3.zero)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, myPickUp.sittingPos, Time.deltaTime * 8f);
		}
	}

	private void FixedUpdate()
	{
		if (!base.isLocalPlayer || StatusManager.manage.dead)
		{
			return;
		}
		if (myPickUp.drivingVehicle && myPickUp.currentlyDriving.mountingAnimationComplete)
		{
			base.transform.position = driving.driversPos.position;
			base.transform.rotation = driving.driversPos.rotation;
		}
		if (myPickUp.sitting)
		{
			if ((bool)myPickUp.sittingPosition)
			{
				base.transform.position = Vector3.Lerp(base.transform.position, myPickUp.sittingPosition.position, Time.deltaTime * 8f);
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, myPickUp.sittingPosition.rotation, Time.deltaTime * 8f);
			}
			if (!NetworkMapSharer.Instance.nextDayIsReady)
			{
				myPickUp.sittingPosition = null;
			}
		}
		if (myInteract.IsPlacingDeed || myPickUp.drivingVehicle || (bool)myPickUp.currentPassengerPos || !Inventory.Instance.CanMoveCharacter())
		{
			if (!myPickUp.drivingVehicle || !myPickUp.currentlyDriving.animateCharAsWell)
			{
				myAnim.SetFloat(CharNetworkAnimator.walkingAnimName, Mathf.Lerp(myAnim.GetFloat(CharNetworkAnimator.walkingAnimName), 0f, Time.deltaTime * 2f));
			}
			return;
		}
		if (!driving)
		{
			RaycastHit hitInfo;
			if ((bool)base.transform.parent && localStandingOn != 0 && !InJump)
			{
				if (!Physics.Raycast(base.transform.position + Vector3.up / 4f, Vector3.down, out hitInfo, 1f, vehicleLayers))
				{
					updateStandingOnLocal(0u);
				}
			}
			else if ((bool)base.transform.parent && localStandingOn != 0 && InJump)
			{
				if (!Physics.Raycast(base.transform.position + Vector3.up / 4f, Vector3.down, out hitInfo, 8f, vehicleLayers))
				{
					updateStandingOnLocal(0u);
				}
			}
			else if (localStandingOn == 0 && !InJump && Physics.Raycast(base.transform.position + Vector3.up / 4f, Vector3.down, out hitInfo, 0.5f, vehicleLayers))
			{
				updateStandingOnLocal(hitInfo.transform.gameObject.GetComponent<VehicleHitBox>().connectedTo.netId);
			}
			if ((((bool)standingOnTrans && standingOnVehicle == null) || ((bool)standingOnTrans && !NetworkIdentity.spawned.ContainsKey(localStandingOn))) && localStandingOn != 0)
			{
				updateStandingOnLocal(0u);
			}
		}
		if (!attackLock && !myPickUp.sitting)
		{
			MoveCharacterWithStick();
			return;
		}
		myAnim.SetFloat(CharNetworkAnimator.walkingAnimName, Mathf.Lerp(myAnim.GetFloat(CharNetworkAnimator.walkingAnimName), 0f, Time.deltaTime * 5f));
		if (moveLockRotateSlow)
		{
			MoveCharacterWithStick();
		}
	}

	private void MoveCharacterWithStick()
	{
		if (!stunned)
		{
			charMoves(InputMaster.input.getLeftStick().x, InputMaster.input.getLeftStick().y);
		}
		else
		{
			charMoves(0f - InputMaster.input.getLeftStick().x, 0f - InputMaster.input.getLeftStick().y);
		}
	}

	private void UpdateMeshRendererOffset()
	{
		Vector3 target = base.transform.position + rendererOffset;
		clerpedPos = Vector3.SmoothDamp(clerpedPos, target, ref followVel, lerpRendererTransformSpeed, maxLerpRendererTransformSpeed, Time.deltaTime);
		charRendererTransform.position = clerpedPos;
	}

	private void charMoves(float xSpeed, float zSpeed)
	{
		if (InputMaster.input.SprintHeld())
		{
			xSpeed /= 2f;
			zSpeed /= 2f;
		}
		bool flag = false;
		if (xSpeed != 0f || zSpeed != 0f)
		{
			if ((StatusManager.manage.tired && !usingHangGlider) || sneaking)
			{
				CurrentSpeed = Mathf.Lerp(CurrentSpeed, 5f, Time.fixedDeltaTime * 2f);
				flag = true;
			}
			else if (!swimming)
			{
				CurrentSpeed = Mathf.Lerp(CurrentSpeed, 9f + runDif, Time.fixedDeltaTime * 2f);
			}
			else
			{
				CurrentSpeed = Mathf.Lerp(CurrentSpeed, 9f, Time.fixedDeltaTime * 2f);
			}
			flag = true;
		}
		else
		{
			CurrentSpeed = Mathf.Lerp(CurrentSpeed, 5f, Time.fixedDeltaTime * 2f);
		}
		if (!rotationLock)
		{
			if (CurrentSpeed < 3f)
			{
				rotateCharToDir(xSpeed, zSpeed, 4f);
			}
			else
			{
				rotateCharToDir(xSpeed, zSpeed);
			}
		}
		Vector3 vector = cameraContainer.TransformDirection(Vector3.forward) * zSpeed;
		Vector3 vector2 = cameraContainer.TransformDirection(Vector3.right) * xSpeed;
		Vector3 vector3 = Vector3.ClampMagnitude(vector + vector2, 1f);
		Vector3 vector4 = vector3 * CurrentSpeed;
		if (climbingLadder)
		{
			MyRigidBody.useGravity = false;
		}
		else
		{
			MyRigidBody.useGravity = true;
		}
		RaycastHit hitInfo;
		if (climbingLadder)
		{
			MyRigidBody.velocity = new Vector3(0f, 0f, 0f);
			float num = Mathf.Clamp(Mathf.Abs(zSpeed + xSpeed), 0f, 0.5f);
			MyRigidBody.MovePosition(MyRigidBody.position + num * Vector3.up * CurrentSpeed / 1.25f * Time.fixedDeltaTime);
			if (CanClimbLadder() && Physics.Raycast(base.transform.position, base.transform.forward, out hitInfo, col.radius + 0.15f, jumpLayers))
			{
				if (!hitInfo.transform.CompareTag("Ladder"))
				{
					climbingLadder = false;
					myEquip.setClimbing(newClimbing: false);
					CmdChangeClimbing(newClimb: false);
				}
			}
			else if (CanClimbLadder() && Physics.Raycast(base.transform.position + Vector3.up, base.transform.forward, out hitInfo, col.radius + 0.15f, jumpLayers))
			{
				if (!hitInfo.transform.CompareTag("Ladder"))
				{
					climbingLadder = false;
					myEquip.setClimbing(newClimbing: false);
					CmdChangeClimbing(newClimb: false);
				}
			}
			else
			{
				climbingLadder = false;
				myEquip.setClimbing(newClimbing: false);
				CmdChangeClimbing(newClimb: false);
			}
		}
		else if (underWater)
		{
			if (EquipWindow.equip.hatSlot.itemNo == divingHelmet.getItemId())
			{
				myAnim.SetBool(CharNetworkAnimator.underwaterAnimName, value: false);
			}
			MyRigidBody.velocity = new Vector3(0f, 0f, 0f);
			if (EquipWindow.equip.hatSlot.itemNo == divingHelmet.getItemId())
			{
				grounded = Physics.CheckSphere(base.transform.position, 0.1f, jumpLayers);
				if (!InputMaster.input.JumpHeld() && !grounded)
				{
					MyRigidBody.MovePosition(MyRigidBody.position + Vector3.down * 1.5f * Time.deltaTime);
				}
				if (canClimb && !InJump && Physics.Raycast(base.transform.position, vector3, col.radius + 0.35f, autoWalkLayer) && Physics.Raycast(base.transform.position + Vector3.up * 1.35f + vector3, Vector3.down, 0.55f, autoWalkLayer))
				{
					MyRigidBody.MovePosition(MyRigidBody.position + (vector4 + Vector3.up * 25f) * Time.fixedDeltaTime);
				}
				else if (!InJump && Physics.Raycast(base.transform.position + Vector3.up, base.transform.forward, out hitInfo, col.radius + 0.15f, jumpLayers))
				{
					if (hitInfo.transform.CompareTag("Ladder"))
					{
						climbingLadder = true;
						myEquip.setClimbing(newClimbing: true);
						CmdChangeClimbing(newClimb: true);
					}
				}
				else if ((InJump || !Physics.Raycast(base.transform.position + Vector3.up, wallCheck1.forward, col.radius + 0.15f, jumpLayers) || !Physics.Raycast(base.transform.position + Vector3.up, wallCheck2.forward, col.radius + 0.15f, jumpLayers)) && (!InJump || !Physics.Raycast(base.transform.position, vector3, col.radius + 0.15f, jumpLayers)))
				{
					MyRigidBody.MovePosition(MyRigidBody.position + vector4 / 2f * swimDif * Time.fixedDeltaTime);
				}
			}
			else
			{
				if (!InputMaster.input.JumpHeld() && !grounded)
				{
					MyRigidBody.MovePosition(MyRigidBody.position + Vector3.down * Time.deltaTime);
				}
				MyRigidBody.MovePosition(MyRigidBody.position + vector4 / 4.5f * swimDif * Time.fixedDeltaTime);
			}
		}
		else if (swimming)
		{
			if (!landedInWater)
			{
				MyRigidBody.MovePosition(MyRigidBody.position + vector4 / 3f * swimDif * Time.fixedDeltaTime);
				if (!InJump && Physics.Raycast(base.transform.position + Vector3.up, base.transform.forward, out hitInfo, col.radius + 0.25f, jumpLayers) && hitInfo.transform.CompareTag("Ladder"))
				{
					climbingLadder = true;
					myEquip.setClimbing(newClimbing: true);
					CmdChangeClimbing(newClimb: true);
				}
			}
			else
			{
				MyRigidBody.MovePosition(MyRigidBody.position + vector4 / 6f * swimDif * Time.fixedDeltaTime);
				if (!InJump && Physics.Raycast(base.transform.position + Vector3.up, base.transform.forward, out hitInfo, col.radius + 0.25f, jumpLayers) && hitInfo.transform.CompareTag("Ladder"))
				{
					climbingLadder = true;
					myEquip.setClimbing(newClimbing: true);
					CmdChangeClimbing(newClimb: true);
				}
			}
		}
		else if (canClimb && !InJump && Physics.Raycast(base.transform.position, vector3, col.radius + 0.35f, autoWalkLayer) && Physics.Raycast(base.transform.position + Vector3.up * 1.35f + vector3, Vector3.down, 0.55f, autoWalkLayer))
		{
			MyRigidBody.MovePosition(MyRigidBody.position + (vector4 + Vector3.up * 25f) * Time.fixedDeltaTime);
		}
		else if (!InJump && Physics.Raycast(base.transform.position + Vector3.up, base.transform.forward, out hitInfo, col.radius + 0.15f, jumpLayers))
		{
			if (hitInfo.transform.CompareTag("Ladder"))
			{
				climbingLadder = true;
				myEquip.setClimbing(newClimbing: true);
				CmdChangeClimbing(newClimb: true);
			}
		}
		else if (InJump || !Physics.Raycast(base.transform.position + Vector3.up, wallCheck1.forward, col.radius + 0.15f, jumpLayers) || !Physics.Raycast(base.transform.position + Vector3.up, wallCheck2.forward, col.radius + 0.15f, jumpLayers))
		{
			if (InJump && Physics.Raycast(base.transform.position, vector3, out hitInfo, col.radius + 0.15f, jumpLayers))
			{
				if (CanClimbLadder() && hitInfo.transform.CompareTag("Ladder"))
				{
					MonoBehaviour.print("Climbed ladder in a jump");
					climbingLadder = true;
					myEquip.setClimbing(newClimbing: true);
					CmdChangeClimbing(newClimb: true);
				}
			}
			else if ((!InJump || !Physics.Raycast(base.transform.position, vector3 + base.transform.right / 3f, col.radius + 0.15f, jumpLayers)) && (!InJump || !Physics.Raycast(base.transform.position, vector3 - base.transform.right / 3f, col.radius + 0.15f, jumpLayers)))
			{
				if ((bool)standingOnTrans)
				{
					base.transform.localPosition = base.transform.localPosition + standingOnTrans.InverseTransformDirection(vector4) * Time.fixedDeltaTime;
				}
				else
				{
					MyRigidBody.MovePosition(MyRigidBody.position + vector4 * Time.fixedDeltaTime);
				}
			}
		}
		animSpeed = Mathf.Lerp(animSpeed, Mathf.Clamp01(Mathf.Abs(zSpeed) + Mathf.Abs(xSpeed)), Time.deltaTime * 10f);
		if (StatusManager.manage.tired || sneaking)
		{
			animSpeed /= 1.2f;
		}
		myAnim.SetBool(CharNetworkAnimator.swimmingAnimName, swimming);
		if ((bool)myAnim)
		{
			if (swimming)
			{
				runningMultipier = Mathf.Lerp(runningMultipier, 1f, Time.fixedDeltaTime);
				myAnim.SetFloat(CharNetworkAnimator.walkingAnimName, animSpeed * runningMultipier);
			}
			else if (!grounded || attackLock)
			{
				runningMultipier = Mathf.Lerp(runningMultipier, 0f, Time.fixedDeltaTime * 2f);
				myAnim.SetFloat(CharNetworkAnimator.walkingAnimName, animSpeed * runningMultipier);
			}
			else if (flag)
			{
				runningMultipier = Mathf.Lerp(runningMultipier, 2f, Time.fixedDeltaTime * 5f);
				myAnim.SetFloat(CharNetworkAnimator.walkingAnimName, animSpeed * runningMultipier);
			}
			else
			{
				runningMultipier = Mathf.Lerp(runningMultipier, 1f, Time.fixedDeltaTime);
				myAnim.SetFloat(CharNetworkAnimator.walkingAnimName, animSpeed * runningMultipier);
			}
		}
	}

	public void startAttackSpeed(float newSpeed)
	{
		CurrentSpeed = newSpeed;
		runningMultipier = 1f;
	}

	public void charMovesForward()
	{
		CurrentSpeed = Mathf.Lerp(CurrentSpeed, 0f, Time.fixedDeltaTime * 2f);
		Vector3 vector = base.transform.forward * CurrentSpeed;
		if (swimming)
		{
			MyRigidBody.MovePosition(MyRigidBody.position + vector / 2.5f * swimDif * Time.fixedDeltaTime);
		}
		else if (!InJump || (InJump && !Physics.Raycast(base.transform.position, base.transform.forward, col.radius + 0.1f, jumpLayers)))
		{
			MyRigidBody.MovePosition(MyRigidBody.position + vector * Time.fixedDeltaTime);
		}
		animSpeed = 2f;
		myAnim.SetBool(CharNetworkAnimator.swimmingAnimName, swimming);
		if ((bool)myAnim)
		{
			runningMultipier = Mathf.Lerp(runningMultipier, 0f, Time.deltaTime * 3f);
			myAnim.SetFloat(CharNetworkAnimator.walkingAnimName, animSpeed * runningMultipier);
		}
	}

	public void isSneaking(bool isSneaking)
	{
		sneaking = isSneaking;
		if (isSneaking)
		{
			base.transform.tag = "Sneaking";
		}
		else
		{
			base.transform.tag = "Untagged";
		}
	}

	private void rotateCharToDir(float x, float y, float rotSpeed = 7f)
	{
		if (x != 0f || y != 0f)
		{
			Vector3 normalized = new Vector3(x, 0f, y).normalized;
			normalized = cameraContainer.transform.TransformDirection(normalized);
			if (standingOnTrans != null)
			{
				normalized = standingOnTrans.InverseTransformDirection(normalized);
			}
			if (normalized != Vector3.zero)
			{
				Quaternion b = Quaternion.LookRotation(normalized);
				base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, b, Time.deltaTime * rotSpeed);
			}
		}
	}

	public void setSpeedDif(float dif)
	{
		runDif = dif;
	}

	public void setSwimBuff(float dif)
	{
		swimBuff = dif;
		swimDif = 1f + (swimSpeedItem + swimBuff);
	}

	public void addOrRemoveJumpDif(int dif)
	{
		jumpDif += dif;
	}

	public void changeSwimSpeedItem(float dif)
	{
		swimSpeedItem = dif;
		swimDif = 1f + (swimSpeedItem + swimBuff);
	}

	public void giveIdolStats(int idolId)
	{
		jumpDif += Inventory.Instance.allItems[idolId].equipable.jumpDif;
		swimDif += Mathf.Clamp(Inventory.Instance.allItems[idolId].equipable.swimSpeedDif, 1f, 100f);
		runDif += Inventory.Instance.allItems[idolId].equipable.runSpeedDif;
	}

	public void removeIdolStatus(int idolId)
	{
		jumpDif -= Inventory.Instance.allItems[idolId].equipable.jumpDif;
		swimDif -= Mathf.Clamp(Inventory.Instance.allItems[idolId].equipable.swimSpeedDif, 1f, 100f);
		runDif -= Inventory.Instance.allItems[idolId].equipable.runSpeedDif;
	}

	private IEnumerator jumpFeel()
	{
		float desiredHeight = 0f;
		float multi = 25f;
		while (desiredHeight < jumpUpHeight)
		{
			yield return jumpWait;
			if (!base.transform.parent)
			{
				MyRigidBody.MovePosition(MyRigidBody.position + Vector3.up * desiredHeight * Time.fixedDeltaTime);
			}
			else
			{
				base.transform.localPosition = base.transform.localPosition + Vector3.up * desiredHeight * Time.fixedDeltaTime;
			}
			desiredHeight = Mathf.Lerp(desiredHeight, jumpUpHeight + 1f, Time.fixedDeltaTime * multi);
			multi = Mathf.Lerp(multi, 10f, Time.deltaTime * 25f);
		}
		while (desiredHeight > 0f && !Physics.CheckSphere(base.transform.position + Vector3.up * 0.3f, 0.6f, jumpLayers) && !Physics.CheckSphere(base.transform.position, 0.1f, swimLayers))
		{
			yield return jumpWait;
			if (!base.transform.parent)
			{
				MyRigidBody.MovePosition(MyRigidBody.position + Vector3.up * desiredHeight * Time.fixedDeltaTime);
			}
			else
			{
				base.transform.localPosition = base.transform.localPosition + Vector3.up * desiredHeight * Time.fixedDeltaTime;
			}
			desiredHeight = Mathf.Lerp(desiredHeight, -1f, Time.deltaTime * 2f);
			jumpFalling = true;
			if (climbingLadder)
			{
				jumpFalling = false;
				InJump = false;
				yield break;
			}
		}
		while (!Physics.CheckSphere(base.transform.position + Vector3.up * 0.3f, 0.6f, jumpLayers) && !Physics.CheckSphere(base.transform.position, 0.1f, swimLayers))
		{
			yield return null;
		}
		jumpFalling = false;
		InJump = false;
	}

	public void lockClientOnLoad()
	{
		MyRigidBody.isKinematic = true;
		CameraController.control.transform.position = base.transform.position;
		CameraController.control.SetFollowTransform(base.transform);
		attackLock = true;
	}

	public void unlockClientOnLoad()
	{
		attackLock = false;
		MyRigidBody.isKinematic = false;
	}

	public void lockCharOnFreeCam()
	{
		MyRigidBody.isKinematic = true;
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		attackLock = true;
	}

	public void unlocklockCharOnFreeCam()
	{
		attackLock = false;
		MyRigidBody.isKinematic = false;
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
	}

	public void getInVehiclePassenger()
	{
		MyRigidBody.isKinematic = true;
		col.enabled = false;
		if (localStandingOn != 0)
		{
			updateStandingOnLocal(0u);
		}
	}

	public void getOutVehiclePassenger()
	{
		col.enabled = true;
		if (base.isLocalPlayer)
		{
			MyRigidBody.isKinematic = false;
		}
		else
		{
			MyRigidBody.isKinematic = true;
		}
	}

	public void getInVehicle(Vehicle drivingVehicle)
	{
		MyRigidBody.isKinematic = true;
		driving = drivingVehicle;
		if (localStandingOn != 0)
		{
			updateStandingOnLocal(0u);
		}
	}

	public void getOutVehicle()
	{
		col.enabled = true;
		if (base.isLocalPlayer)
		{
			MyRigidBody.isKinematic = false;
		}
		else
		{
			MyRigidBody.isKinematic = true;
		}
		driving = null;
	}

	public void onChangeUnderWater(bool old, bool newUnderWater)
	{
		NetworkunderWater = newUnderWater;
		if (!base.isLocalPlayer)
		{
			NetworkunderWater = newUnderWater;
			col.enabled = !newUnderWater;
			underWaterHit.SetActive(newUnderWater);
			myAnim.SetBool(CharNetworkAnimator.underwaterAnimName, newUnderWater);
		}
	}

	public void unlockAll()
	{
		isSneaking(isSneaking: false);
		canClimb = true;
		attackLock = false;
		moveLockRotateSlow = false;
		rotationLock = false;
	}

	public void lockRotation(bool isLocked)
	{
		if (rotationLock != isLocked)
		{
			CurrentSpeed = 4f;
		}
		rotationLock = isLocked;
	}

	public void faceClosestTarget()
	{
		if (base.isLocalPlayer)
		{
			StartCoroutine(findClosestTargetAndFace());
		}
	}

	private IEnumerator findClosestInteractable()
	{
		yield return null;
	}

	private IEnumerator findClosestTargetAndFace()
	{
		facingTarget = true;
		float y = InputMaster.input.getLeftStick().y;
		float x = InputMaster.input.getLeftStick().x;
		Vector3 vector = base.transform.forward;
		if (y != 0f && x != 0f)
		{
			Vector3 vector2 = cameraContainer.TransformDirection(Vector3.forward) * y;
			Vector3 vector3 = cameraContainer.TransformDirection(Vector3.left) * x;
			vector = Vector3.ClampMagnitude(vector2 + vector3, 1f);
		}
		if (Physics.CheckSphere(base.transform.position + Vector3.up + vector * 2f, 2.5f, myEnemies))
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position + Vector3.up + vector * 2.5f, 3f, myEnemies);
			for (int i = 0; i < array.Length; i++)
			{
				if (!(array[i].transform != base.transform) || !array[i])
				{
					continue;
				}
				AnimalAI component = array[i].GetComponent<AnimalAI>();
				if ((bool)component && !component.isDead() && !component.isAPet())
				{
					float lookTimer = 0f;
					Quaternion desiredLook = Quaternion.LookRotation((new Vector3(array[i].transform.position.x, base.transform.position.y, array[i].transform.position.z) - base.transform.position).normalized);
					while (lookTimer < 1f)
					{
						lookTimer += Time.deltaTime;
						base.transform.rotation = Quaternion.Lerp(base.transform.rotation, desiredLook, Time.deltaTime * 7.5f);
						yield return null;
					}
					break;
				}
			}
		}
		facingTarget = false;
	}

	public void attackLockOn(bool isOn)
	{
		if (attackLock != isOn)
		{
			CurrentSpeed = 4f;
		}
		attackLock = isOn;
	}

	public void moveLockRotateSlowOn(bool isOn)
	{
		if (moveLockRotateSlow != isOn)
		{
			CurrentSpeed = 4f;
		}
		moveLockRotateSlow = isOn;
	}

	[Command]
	public void CmdChangeTalkTo(uint npcToTalkTo, bool isTalking)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(npcToTalkTo);
		writer.WriteBool(isTalking);
		SendCommandInternal(typeof(CharMovement), "CmdChangeTalkTo", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRequestInterior(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharMovement), "CmdRequestInterior", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdUpgradeGuestHouse(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharMovement), "CmdUpgradeGuestHouse", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRequestHouseInterior(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharMovement), "CmdRequestHouseInterior", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRequestHouseExterior(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharMovement), "CmdRequestHouseExterior", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdDonateItemToMuseum(int itemId, string playerName)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		writer.WriteString(playerName);
		SendCommandInternal(typeof(CharMovement), "CmdDonateItemToMuseum", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRequestShopStatus()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdRequestShopStatus", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRequestMuseumInterior()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdRequestMuseumInterior", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeUnderWater(bool newUnderWater)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(newUnderWater);
		SendCommandInternal(typeof(CharMovement), "CmdChangeUnderWater", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdNPCStartFollow(uint tellNPCtoFollow, uint transformToFollow)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(tellNPCtoFollow);
		writer.WriteUInt(transformToFollow);
		SendCommandInternal(typeof(CharMovement), "CmdNPCStartFollow", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdUpdateCurrentlyInside(int insideId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(insideId);
		SendCommandInternal(typeof(CharMovement), "CmdUpdateCurrentlyInside", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public NPCSchedual.Locations GetCurrentlyInsideBuilding()
	{
		return currentlyInsideBuilding;
	}

	[Command]
	public void CmdRequestMapChunk(int chunkPosX, int chunkPosY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(chunkPosX);
		writer.WriteInt(chunkPosY);
		SendCommandInternal(typeof(CharMovement), "CmdRequestMapChunk", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRequestItemOnTopForChunk(int chunkX, int chunkY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(chunkX);
		writer.WriteInt(chunkY);
		SendCommandInternal(typeof(CharMovement), "CmdRequestItemOnTopForChunk", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSendChatMessage(string newMessage)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteString(newMessage);
		SendCommandInternal(typeof(CharMovement), "CmdSendChatMessage", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSendEmote(int newEmote)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newEmote);
		SendCommandInternal(typeof(CharMovement), "CmdSendEmote", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdDealDirectDamage(uint netId, int damageAmount)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(netId);
		writer.WriteInt(damageAmount);
		SendCommandInternal(typeof(CharMovement), "CmdDealDirectDamage", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdDealDamage(uint netId, float multiplier)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(netId);
		writer.WriteFloat(multiplier);
		SendCommandInternal(typeof(CharMovement), "CmdDealDamage", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdTakeDamage(int damageAmount)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(damageAmount);
		SendCommandInternal(typeof(CharMovement), "CmdTakeDamage", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdCloseChest(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharMovement), "CmdCloseChest", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRequestOnTileStatus(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharMovement), "CmdRequestOnTileStatus", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRequestTileRotation(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharMovement), "CmdRequestTileRotation", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdReviveSelf()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdReviveSelf", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdCatchBug(uint bugToCatch)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(bugToCatch);
		SendCommandInternal(typeof(CharMovement), "CmdCatchBug", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRemoveBugFromTerrarium(int bugIdToRemove, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(bugIdToRemove);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharMovement), "CmdRemoveBugFromTerrarium", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcUpdateStandOn(uint standingOnForRPC)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(standingOnForRPC);
		SendRPCInternal(typeof(CharMovement), "RpcUpdateStandOn", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private void updateStandingOn(uint standingOnForRPC)
	{
		if (standingOnForRPC != 0)
		{
			normalNetworkTransform.enabled = false;
			standingOnTrans = NetworkIdentity.spawned[standingOnForRPC].GetComponent<Vehicle>().myHitBox;
			base.transform.SetParent(standingOnTrans);
			if (base.isLocalPlayer)
			{
				standingOnVehicle = standingOnTrans.GetComponent<VehicleHitBox>().connectedTo;
			}
			standingOnNetworkTransform.enabled = true;
		}
		else
		{
			standingOnNetworkTransform.enabled = false;
			base.transform.SetParent(null);
			standingOnTrans = null;
			standingOnVehicle = null;
			normalNetworkTransform.enabled = true;
		}
	}

	public void onChangeStamina(int oldStam, int newStam)
	{
		Networkstamina = newStam;
		if (newStam == 0)
		{
			if (!animatedTired)
			{
				animatedTired = true;
				myAnim.SetBool("Tired", value: true);
			}
		}
		else if (animatedTired)
		{
			animatedTired = false;
			myAnim.SetBool("Tired", value: false);
		}
	}

	[Command]
	public void CmdSetNewStamina(int newStam)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newStam);
		SendCommandInternal(typeof(CharMovement), "CmdSetNewStamina", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdDropItem(int itemId, int stackAmount, Vector3 dropPos, Vector3 desirePos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		writer.WriteInt(stackAmount);
		writer.WriteVector3(dropPos);
		writer.WriteVector3(desirePos);
		SendCommandInternal(typeof(CharMovement), "CmdDropItem", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSetSongForBoomBox(int itemId, int xPos, int yPos, int houseX, int houseY, int onTopPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		writer.WriteInt(onTopPos);
		SendCommandInternal(typeof(CharMovement), "CmdSetSongForBoomBox", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPlaceFishInPond(int itemId, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharMovement), "CmdPlaceFishInPond", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPlaceBugInTerrarium(int itemId, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharMovement), "CmdPlaceBugInTerrarium", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcReleaseBug(int bugId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(bugId);
		SendRPCInternal(typeof(CharMovement), "RpcReleaseBug", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcReleaseFish(int fishId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(fishId);
		SendRPCInternal(typeof(CharMovement), "RpcReleaseFish", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public void updateStandingOnLocal(uint newStandingOnId)
	{
		if (newStandingOnId == 0 || NetworkIdentity.spawned.ContainsKey(newStandingOnId))
		{
			localStandingOn = newStandingOnId;
			CmdChangeStandingOn(newStandingOnId);
		}
	}

	[Command]
	public void CmdChangeStandingOn(uint newStandOn)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(newStandOn);
		SendCommandInternal(typeof(CharMovement), "CmdChangeStandingOn", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdAgreeToCraftsmanCrafting()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdAgreeToCraftsmanCrafting", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPlaceAnimalInCollectionPoint(uint animalTrapPlaced)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(animalTrapPlaced);
		SendCommandInternal(typeof(CharMovement), "CmdPlaceAnimalInCollectionPoint", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSpawnAnimalBox(int animalId, int variation, string animalName, Vector3 position, Quaternion rotation)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(animalId);
		writer.WriteInt(variation);
		writer.WriteString(animalName);
		writer.WriteVector3(position);
		writer.WriteQuaternion(rotation);
		SendCommandInternal(typeof(CharMovement), "CmdSpawnAnimalBox", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator moveBoxToPos(PickUpAndCarry carry)
	{
		yield return null;
		carry.dropToPosY = FarmAnimalMenu.menu.spawnFarmAnimalPos.position.y;
		carry.transform.position = FarmAnimalMenu.menu.spawnFarmAnimalPos.position;
	}

	[Command]
	public void CmdSellByWeight(uint itemPlaced)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(itemPlaced);
		SendCommandInternal(typeof(CharMovement), "CmdSellByWeight", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdActivateTrap(uint animalToTrapId, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(animalToTrapId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharMovement), "CmdActivateTrap", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSetOnFire(uint damageableId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(damageableId);
		SendCommandInternal(typeof(CharMovement), "CmdSetOnFire", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdBuyItemFromStall(int stallType, int shopStallNo)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(stallType);
		writer.WriteInt(shopStallNo);
		SendCommandInternal(typeof(CharMovement), "CmdBuyItemFromStall", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdCloseCamera()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdCloseCamera", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcCloseCamera()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(CharMovement), "RpcCloseCamera", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcTakeKnockback(Vector3 knockBackDir, float knockBackAmount)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(knockBackDir);
		writer.WriteFloat(knockBackAmount);
		SendRPCInternal(typeof(CharMovement), "RpcTakeKnockback", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeClockTickSpeed(float newWorldSpeed)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteFloat(newWorldSpeed);
		SendCommandInternal(typeof(CharMovement), "CmdChangeClockTickSpeed", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPlacePlayerPlacedIconOnMap(Vector2 position, int iconSpriteIndex)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector2(position);
		writer.WriteInt(iconSpriteIndex);
		SendCommandInternal(typeof(CharMovement), "CmdPlacePlayerPlacedIconOnMap", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSetPlayerPlacedMapIconHighlightValue(uint netId, bool value)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(netId);
		writer.WriteBool(value);
		SendCommandInternal(typeof(CharMovement), "CmdSetPlayerPlacedMapIconHighlightValue", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CommandRemovePlayerPlacedMapIcon(uint netId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(netId);
		SendCommandInternal(typeof(CharMovement), "CommandRemovePlayerPlacedMapIcon", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdToggleHighlightForAutomaticallySetMapIcon(int tileX, int tileY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(tileX);
		writer.WriteInt(tileY);
		SendCommandInternal(typeof(CharMovement), "CmdToggleHighlightForAutomaticallySetMapIcon", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Server]
	public void ToggleHighlightForAutomaticallySetMapIcon(int tileX, int tileY)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void CharMovement::ToggleHighlightForAutomaticallySetMapIcon(System.Int32,System.Int32)' called when server was not active");
			return;
		}
		MapPoint mapPoint = default(MapPoint);
		mapPoint.X = tileX;
		mapPoint.Y = tileY;
		MapPoint item = mapPoint;
		if (NetworkMapSharer.Instance.mapPoints.Contains(item))
		{
			NetworkMapSharer.Instance.mapPoints.Remove(item);
		}
		else
		{
			NetworkMapSharer.Instance.mapPoints.Add(item);
		}
	}

	[Command]
	public void CmdRequestNPCInv(int npcId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(npcId);
		SendCommandInternal(typeof(CharMovement), "CmdRequestNPCInv", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPayTownDebt(int payment)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(payment);
		SendCommandInternal(typeof(CharMovement), "CmdPayTownDebt", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdGetDeedIngredients(int buildingId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(buildingId);
		SendCommandInternal(typeof(CharMovement), "CmdGetDeedIngredients", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdDonateDeedIngredients(int buildingId, int[] alreadyGiven)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(buildingId);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, alreadyGiven);
		SendCommandInternal(typeof(CharMovement), "CmdDonateDeedIngredients", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdCharFaints()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdCharFaints", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcSetCharFaints(bool isFainted)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(isFainted);
		SendRPCInternal(typeof(CharMovement), "RpcSetCharFaints", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeBlocking(bool isBlocking)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(isBlocking);
		SendCommandInternal(typeof(CharMovement), "CmdChangeBlocking", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdAcceptBulletinBoardPost(int id)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(id);
		SendCommandInternal(typeof(CharMovement), "CmdAcceptBulletinBoardPost", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcAcceptBulletinBoardPost(int id)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(id);
		SendRPCInternal(typeof(CharMovement), "RpcAcceptBulletinBoardPost", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdCompleteBulletinBoardPost(int id)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(id);
		SendCommandInternal(typeof(CharMovement), "CmdCompleteBulletinBoardPost", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcCompleteBulletinBoardPost(int id)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(id);
		SendRPCInternal(typeof(CharMovement), "RpcCompleteBulletinBoardPost", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSetDefenceBuff(float newDefence)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteFloat(newDefence);
		SendCommandInternal(typeof(CharMovement), "CmdSetDefenceBuff", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSetFireResistance(int resistanceLevel)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(resistanceLevel);
		SendCommandInternal(typeof(CharMovement), "CmdSetFireResistance", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdFireAOE()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdFireAOE", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcFireAOE()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(CharMovement), "RpcFireAOE", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSetHealthRegen(float timer, int level)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteFloat(timer);
		writer.WriteInt(level);
		SendCommandInternal(typeof(CharMovement), "CmdSetHealthRegen", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdGiveHealthBack(int amount)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(amount);
		SendCommandInternal(typeof(CharMovement), "CmdGiveHealthBack", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeClimbing(bool newClimb)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(newClimb);
		SendCommandInternal(typeof(CharMovement), "CmdChangeClimbing", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdTeleport(string teledir)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteString(teledir);
		SendCommandInternal(typeof(CharMovement), "CmdTeleport", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdTeleportToSignal()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdTeleportToSignal", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcTeleportCharToVector(Vector3 endPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(endPos);
		SendRPCInternal(typeof(CharMovement), "RpcTeleportCharToVector", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcTeleportChar(int[] pos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, pos);
		SendRPCInternal(typeof(CharMovement), "RpcTeleportChar", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public IEnumerator teleportCharToPos(int[] pos)
	{
		_ = new int[2]
		{
			(int)base.transform.position.x / 2,
			(int)base.transform.position.z / 2
		};
		ParticleManager.manage.startTeleportParticles(base.transform, pos);
		if (base.isLocalPlayer)
		{
			StartCoroutine(charLockedStill(2.5f));
			SoundManager.Instance.play2DSound(SoundManager.Instance.teleportCharge);
		}
		else
		{
			SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.teleportCharge, base.transform.position);
			SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.teleportCharge, new Vector3((float)pos[0] * 2f + 1f, (float)WorldManager.Instance.heightMap[pos[0], pos[1]] + 0.61f, (float)pos[1] * 2f + 1.5f));
		}
		yield return new WaitForSeconds(1.5f);
		if (base.isLocalPlayer)
		{
			SoundManager.Instance.play2DSound(SoundManager.Instance.teleportSound);
		}
		else
		{
			SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.teleportSound, base.transform.position);
			SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.teleportSound, new Vector3((float)pos[0] * 2f + 1f, (float)WorldManager.Instance.heightMap[pos[0], pos[1]] + 0.61f, (float)pos[1] * 2f + 1.5f));
		}
		yield return new WaitForSeconds(0.25f);
		if (base.isLocalPlayer)
		{
			base.transform.position = new Vector3((float)pos[0] * 2f + 1f, (float)WorldManager.Instance.heightMap[pos[0], pos[1]] + 0.61f, (float)pos[1] * 2f + 1.5f);
			CameraController.control.transform.position = NetworkMapSharer.Instance.localChar.transform.position;
			NewChunkLoader.loader.forceInstantUpdateAtPos();
			StartCoroutine(FreezeCharAfterTeleport());
			base.transform.position = new Vector3((float)pos[0] * 2f + 1f, (float)WorldManager.Instance.heightMap[pos[0], pos[1]] + 0.61f, (float)pos[1] * 2f + 1.5f);
		}
	}

	public IEnumerator TeleportCharToVector(Vector3 endPos)
	{
		_ = new int[2]
		{
			(int)base.transform.position.x / 2,
			(int)base.transform.position.z / 2
		};
		ParticleManager.manage.startTeleportParticlesVector(base.transform, endPos);
		if (base.isLocalPlayer)
		{
			StartCoroutine(charLockedStill(2.5f));
			SoundManager.Instance.play2DSound(SoundManager.Instance.teleportCharge);
		}
		else
		{
			SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.teleportCharge, base.transform.position);
			SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.teleportCharge, endPos);
		}
		yield return new WaitForSeconds(1.5f);
		if (base.isLocalPlayer)
		{
			SoundManager.Instance.play2DSound(SoundManager.Instance.teleportSound);
		}
		else
		{
			SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.teleportSound, base.transform.position);
			SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.teleportSound, endPos);
		}
		yield return new WaitForSeconds(0.25f);
		if (base.isLocalPlayer)
		{
			base.transform.position = endPos;
			CameraController.control.transform.position = NetworkMapSharer.Instance.localChar.transform.position;
			NewChunkLoader.loader.forceInstantUpdateAtPos();
			StartCoroutine(FreezeCharAfterTeleport());
			CameraController.control.transform.position = NetworkMapSharer.Instance.localChar.transform.position;
		}
	}

	public IEnumerator FreezeCharAfterTeleport()
	{
		float timer = 0f;
		MyRigidBody.isKinematic = true;
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		while (timer < 5f)
		{
			yield return null;
			timer += Time.deltaTime;
			if (Physics.Raycast(base.transform.position + Vector3.up * 12f, Vector3.down, out var _, 17f, jumpLayers))
			{
				timer = 10f;
			}
		}
		MyRigidBody.isKinematic = false;
	}

	public IEnumerator charAttacksForward(float forwardSpeed = 5f, float forwardTime = 0.35f)
	{
		attackLockOn(isOn: true);
		float attackTimer = 0f;
		while (attackTimer < forwardTime)
		{
			yield return null;
			attackTimer += Time.deltaTime;
			forwardSpeed -= Time.deltaTime;
			MyRigidBody.MovePosition(MyRigidBody.position + base.transform.forward * forwardSpeed * Time.fixedDeltaTime);
		}
		attackLockOn(isOn: false);
	}

	public bool isInDanger()
	{
		return inDanger;
	}

	public bool CanClimbLadder()
	{
		if (InJump)
		{
			return jumpFalling;
		}
		return true;
	}

	public IEnumerator charLockedStill(float time)
	{
		attackLockOn(isOn: true);
		yield return new WaitForSeconds(time);
		attackLockOn(isOn: false);
	}

	private IEnumerator knockBack(Vector3 dir, float knockBackAmount)
	{
		beingKnockedBack = true;
		attackLockOn(isOn: true);
		float knockTimer = 0f;
		while (knockTimer < 0.35f)
		{
			yield return null;
			knockTimer += Time.deltaTime;
			if (!Physics.Raycast(base.transform.position + Vector3.up * 0.2f, dir, col.radius + 0.2f, jumpLayers))
			{
				MyRigidBody.MovePosition(MyRigidBody.position + dir * knockBackAmount * Time.fixedDeltaTime);
			}
		}
		attackLockOn(isOn: false);
		beingKnockedBack = false;
	}

	private IEnumerator swimmingAndDivingStamina()
	{
		int swimDamageTimer = 0;
		while (true)
		{
			yield return null;
			while (swimming)
			{
				if (EquipWindow.equip.hatSlot.itemNo == divingHelmet.getItemId())
				{
					if (underWater)
					{
						StatusManager.manage.changeStamina(-0.25f);
					}
					else if (swimming)
					{
						StatusManager.manage.changeStamina(-0.1f);
					}
				}
				else if (!usingBoogieBoard)
				{
					if (underWater)
					{
						StatusManager.manage.changeStamina(-0.5f);
					}
					else
					{
						StatusManager.manage.changeStamina(-0.25f);
					}
				}
				else if (underWater)
				{
					StatusManager.manage.changeStamina(-0.25f);
				}
				else
				{
					StatusManager.manage.changeStamina(-0.1f);
				}
				if (StatusManager.manage.getStamina() == 0f)
				{
					if (swimDamageTimer == 4)
					{
						StatusManager.manage.changeStatus(-1, 0f);
						swimDamageTimer = 0;
					}
					else
					{
						swimDamageTimer++;
					}
				}
				yield return swimWait;
			}
		}
	}

	public IEnumerator landInWaterTimer()
	{
		landedInWater = true;
		float timer = 0.65f;
		while (timer >= 0f)
		{
			timer -= Time.deltaTime;
			yield return null;
			if (!swimming)
			{
				break;
			}
		}
		landedInWater = false;
	}

	private IEnumerator replaceHandPlaceableDelay()
	{
		yield return new WaitForSeconds(0.2f);
		myEquip.placeHandPlaceable();
		myInteract.ScheduleForRefreshSelection = true;
	}

	public void forceNoStandingOn(Vector3 forceToStandAtPos)
	{
		if (localStandingOn != 0)
		{
			StartCoroutine(delayFallingAfterForceNoStanding(forceToStandAtPos));
			updateStandingOnLocal(0u);
		}
	}

	public IEnumerator delayFallingAfterForceNoStanding(Vector3 forceToStandAtPos)
	{
		while ((bool)standingOnTrans)
		{
			MyRigidBody.MovePosition(new Vector3(MyRigidBody.position.x, forceToStandAtPos.y, MyRigidBody.position.z));
			MyRigidBody.velocity = Vector3.zero;
			yield return null;
		}
		MyRigidBody.MovePosition(new Vector3(MyRigidBody.position.x, forceToStandAtPos.y, MyRigidBody.position.z));
		MyRigidBody.velocity = Vector3.zero;
	}

	[TargetRpc]
	public void TargetKick(NetworkConnection conn)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendTargetRPCInternal(conn, typeof(CharMovement), "TargetKick", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdUpdateHouseExterior(HouseExterior exterior)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		GeneratedNetworkCode._Write_HouseExterior(writer, exterior);
		SendCommandInternal(typeof(CharMovement), "CmdUpdateHouseExterior", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRainMaker()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdRainMaker", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdCupOfSunshine()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdCupOfSunshine", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void changeToAboveWater()
	{
		MyRigidBody.useGravity = true;
		NetworkunderWater = false;
		col.enabled = true;
		underWaterHit.SetActive(value: false);
		SoundManager.Instance.switchUnderWater(newUnderWater: false);
		myAnim.SetBool(CharNetworkAnimator.underwaterAnimName, value: false);
		CmdChangeUnderWater(newUnderWater: false);
	}

	public void changeToUnderWater()
	{
		NetworkunderWater = true;
		col.enabled = false;
		underWaterHit.SetActive(value: true);
		SoundManager.Instance.switchUnderWater(newUnderWater: true);
		if (EquipWindow.equip.hatSlot.itemNo != divingHelmet.getItemId())
		{
			myAnim.SetBool(CharNetworkAnimator.underwaterAnimName, value: true);
		}
		CmdChangeUnderWater(newUnderWater: true);
		MyRigidBody.useGravity = false;
		pickUpTimer = 0f;
	}

	[TargetRpc]
	public void TargetCheckVersion(NetworkConnection conn, int invItemCount, int worldItemsCount)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(invItemCount);
		writer.WriteInt(worldItemsCount);
		SendTargetRPCInternal(conn, typeof(CharMovement), "TargetCheckVersion", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSetMinelayer(int newLayerNo)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newLayerNo);
		SendCommandInternal(typeof(CharMovement), "CmdSetMinelayer", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdTakePhotoSound(Vector3 position)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(position);
		SendCommandInternal(typeof(CharMovement), "CmdTakePhotoSound", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdTakeItemFromNPC(uint netId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(netId);
		SendCommandInternal(typeof(CharMovement), "CmdTakeItemFromNPC", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdMarkTreasureOnMap()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdMarkTreasureOnMap", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcCameraEffectSound(Vector3 position)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(position);
		SendRPCInternal(typeof(CharMovement), "RpcCameraEffectSound", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdRequestEntranceMapIcon()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdRequestEntranceMapIcon", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetScanMapIconAtPosition(NetworkConnection conn, int tileId, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(tileId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendTargetRPCInternal(conn, typeof(CharMovement), "TargetScanMapIconAtPosition", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void TryAndMakeWish(int wishType)
	{
		if (!NetworkMapSharer.Instance.wishManager.wishMadeToday)
		{
			Inventory.Instance.changeWallet(-NetworkMapSharer.Instance.wishManager.GetWishCost());
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.MakeAWish);
			CmdMakeAWish(wishType);
		}
	}

	[Command]
	public void CmdMakeAWish(int wishType)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(wishType);
		SendCommandInternal(typeof(CharMovement), "CmdMakeAWish", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void KickABall(uint netId)
	{
		if (canKick)
		{
			CmdKickABall(netId, base.transform.forward, CurrentSpeed);
			canKick = false;
			StartCoroutine(ResetCanKick());
		}
	}

	private IEnumerator ResetCanKick()
	{
		yield return new WaitForSeconds(1f);
		canKick = true;
	}

	[Command]
	public void CmdKickABall(uint ballId, Vector3 kickDir, float power)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(ballId);
		writer.WriteVector3(kickDir);
		writer.WriteFloat(power);
		SendCommandInternal(typeof(CharMovement), "CmdKickABall", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdUseInstaGrow(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharMovement), "CmdUseInstaGrow", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSendTeleSignal()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdSendTeleSignal", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRingTownBell()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdRingTownBell", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public Vector3 GetClerpPos()
	{
		return clerpedPos;
	}

	public void AddAnEnemy(AnimalAI_Attack newAttack)
	{
		if (!currentlyInDangerOf.Contains(newAttack))
		{
			currentlyInDangerOf.Add(newAttack);
			if (InDangerCheck == null)
			{
				InDangerCheck = StartCoroutine(CheckIfStillInDanger());
			}
		}
	}

	public void RemoveAnEnemy(AnimalAI_Attack removeAttack)
	{
		if (currentlyInDangerOf.Contains(removeAttack))
		{
			currentlyInDangerOf.Remove(removeAttack);
		}
	}

	private IEnumerator CheckIfStillInDanger()
	{
		WaitForSeconds wait = new WaitForSeconds(0.5f);
		if (currentlyInDangerOf.Count > 0)
		{
			NetworkinDanger = true;
		}
		while (currentlyInDangerOf.Count > 0)
		{
			yield return wait;
			for (int num = currentlyInDangerOf.Count - 1; num >= 0; num--)
			{
				if (!currentlyInDangerOf[num].gameObject.activeInHierarchy)
				{
					currentlyInDangerOf.RemoveAt(num);
				}
			}
		}
		NetworkinDanger = false;
		InDangerCheck = null;
	}

	[Command]
	public void CmdChangeWeather(bool setWindy, bool setHeatWave, bool setRaining, bool setStorming, bool setFoggy, bool setSnowing, bool setMeteorshower)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(setWindy);
		writer.WriteBool(setHeatWave);
		writer.WriteBool(setRaining);
		writer.WriteBool(setStorming);
		writer.WriteBool(setFoggy);
		writer.WriteBool(setSnowing);
		writer.WriteBool(setMeteorshower);
		SendCommandInternal(typeof(CharMovement), "CmdChangeWeather", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	static CharMovement()
	{
		jumpWait = new WaitForFixedUpdate();
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdChangeTalkTo", InvokeUserCode_CmdChangeTalkTo, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestInterior", InvokeUserCode_CmdRequestInterior, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdUpgradeGuestHouse", InvokeUserCode_CmdUpgradeGuestHouse, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestHouseInterior", InvokeUserCode_CmdRequestHouseInterior, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestHouseExterior", InvokeUserCode_CmdRequestHouseExterior, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdDonateItemToMuseum", InvokeUserCode_CmdDonateItemToMuseum, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestShopStatus", InvokeUserCode_CmdRequestShopStatus, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestMuseumInterior", InvokeUserCode_CmdRequestMuseumInterior, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdChangeUnderWater", InvokeUserCode_CmdChangeUnderWater, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdNPCStartFollow", InvokeUserCode_CmdNPCStartFollow, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdUpdateCurrentlyInside", InvokeUserCode_CmdUpdateCurrentlyInside, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestMapChunk", InvokeUserCode_CmdRequestMapChunk, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestItemOnTopForChunk", InvokeUserCode_CmdRequestItemOnTopForChunk, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSendChatMessage", InvokeUserCode_CmdSendChatMessage, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSendEmote", InvokeUserCode_CmdSendEmote, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdDealDirectDamage", InvokeUserCode_CmdDealDirectDamage, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdDealDamage", InvokeUserCode_CmdDealDamage, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdTakeDamage", InvokeUserCode_CmdTakeDamage, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdCloseChest", InvokeUserCode_CmdCloseChest, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestOnTileStatus", InvokeUserCode_CmdRequestOnTileStatus, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestTileRotation", InvokeUserCode_CmdRequestTileRotation, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdReviveSelf", InvokeUserCode_CmdReviveSelf, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdCatchBug", InvokeUserCode_CmdCatchBug, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRemoveBugFromTerrarium", InvokeUserCode_CmdRemoveBugFromTerrarium, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSetNewStamina", InvokeUserCode_CmdSetNewStamina, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdDropItem", InvokeUserCode_CmdDropItem, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSetSongForBoomBox", InvokeUserCode_CmdSetSongForBoomBox, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdPlaceFishInPond", InvokeUserCode_CmdPlaceFishInPond, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdPlaceBugInTerrarium", InvokeUserCode_CmdPlaceBugInTerrarium, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdChangeStandingOn", InvokeUserCode_CmdChangeStandingOn, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdAgreeToCraftsmanCrafting", InvokeUserCode_CmdAgreeToCraftsmanCrafting, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdPlaceAnimalInCollectionPoint", InvokeUserCode_CmdPlaceAnimalInCollectionPoint, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSpawnAnimalBox", InvokeUserCode_CmdSpawnAnimalBox, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSellByWeight", InvokeUserCode_CmdSellByWeight, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdActivateTrap", InvokeUserCode_CmdActivateTrap, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSetOnFire", InvokeUserCode_CmdSetOnFire, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdBuyItemFromStall", InvokeUserCode_CmdBuyItemFromStall, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdCloseCamera", InvokeUserCode_CmdCloseCamera, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdChangeClockTickSpeed", InvokeUserCode_CmdChangeClockTickSpeed, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdPlacePlayerPlacedIconOnMap", InvokeUserCode_CmdPlacePlayerPlacedIconOnMap, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSetPlayerPlacedMapIconHighlightValue", InvokeUserCode_CmdSetPlayerPlacedMapIconHighlightValue, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CommandRemovePlayerPlacedMapIcon", InvokeUserCode_CommandRemovePlayerPlacedMapIcon, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdToggleHighlightForAutomaticallySetMapIcon", InvokeUserCode_CmdToggleHighlightForAutomaticallySetMapIcon, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestNPCInv", InvokeUserCode_CmdRequestNPCInv, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdPayTownDebt", InvokeUserCode_CmdPayTownDebt, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdGetDeedIngredients", InvokeUserCode_CmdGetDeedIngredients, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdDonateDeedIngredients", InvokeUserCode_CmdDonateDeedIngredients, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdCharFaints", InvokeUserCode_CmdCharFaints, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdChangeBlocking", InvokeUserCode_CmdChangeBlocking, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdAcceptBulletinBoardPost", InvokeUserCode_CmdAcceptBulletinBoardPost, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdCompleteBulletinBoardPost", InvokeUserCode_CmdCompleteBulletinBoardPost, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSetDefenceBuff", InvokeUserCode_CmdSetDefenceBuff, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSetFireResistance", InvokeUserCode_CmdSetFireResistance, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdFireAOE", InvokeUserCode_CmdFireAOE, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSetHealthRegen", InvokeUserCode_CmdSetHealthRegen, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdGiveHealthBack", InvokeUserCode_CmdGiveHealthBack, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdChangeClimbing", InvokeUserCode_CmdChangeClimbing, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdTeleport", InvokeUserCode_CmdTeleport, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdTeleportToSignal", InvokeUserCode_CmdTeleportToSignal, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdUpdateHouseExterior", InvokeUserCode_CmdUpdateHouseExterior, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRainMaker", InvokeUserCode_CmdRainMaker, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdCupOfSunshine", InvokeUserCode_CmdCupOfSunshine, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSetMinelayer", InvokeUserCode_CmdSetMinelayer, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdTakePhotoSound", InvokeUserCode_CmdTakePhotoSound, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdTakeItemFromNPC", InvokeUserCode_CmdTakeItemFromNPC, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdMarkTreasureOnMap", InvokeUserCode_CmdMarkTreasureOnMap, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestEntranceMapIcon", InvokeUserCode_CmdRequestEntranceMapIcon, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdMakeAWish", InvokeUserCode_CmdMakeAWish, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdKickABall", InvokeUserCode_CmdKickABall, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdUseInstaGrow", InvokeUserCode_CmdUseInstaGrow, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSendTeleSignal", InvokeUserCode_CmdSendTeleSignal, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRingTownBell", InvokeUserCode_CmdRingTownBell, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdChangeWeather", InvokeUserCode_CmdChangeWeather, requiresAuthority: true);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcUpdateStandOn", InvokeUserCode_RpcUpdateStandOn);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcReleaseBug", InvokeUserCode_RpcReleaseBug);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcReleaseFish", InvokeUserCode_RpcReleaseFish);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcCloseCamera", InvokeUserCode_RpcCloseCamera);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcTakeKnockback", InvokeUserCode_RpcTakeKnockback);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcSetCharFaints", InvokeUserCode_RpcSetCharFaints);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcAcceptBulletinBoardPost", InvokeUserCode_RpcAcceptBulletinBoardPost);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcCompleteBulletinBoardPost", InvokeUserCode_RpcCompleteBulletinBoardPost);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcFireAOE", InvokeUserCode_RpcFireAOE);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcTeleportCharToVector", InvokeUserCode_RpcTeleportCharToVector);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcTeleportChar", InvokeUserCode_RpcTeleportChar);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcCameraEffectSound", InvokeUserCode_RpcCameraEffectSound);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "TargetKick", InvokeUserCode_TargetKick);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "TargetCheckVersion", InvokeUserCode_TargetCheckVersion);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "TargetScanMapIconAtPosition", InvokeUserCode_TargetScanMapIconAtPosition);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdChangeTalkTo(uint npcToTalkTo, bool isTalking)
	{
		if (isTalking)
		{
			NetworkIdentity.spawned[npcToTalkTo].GetComponent<NPCAI>().NetworktalkingTo = base.netId;
			isCurrentlyTalking = true;
		}
		else
		{
			NetworkIdentity.spawned[npcToTalkTo].GetComponent<NPCAI>().NetworktalkingTo = 0u;
			isCurrentlyTalking = false;
		}
	}

	protected static void InvokeUserCode_CmdChangeTalkTo(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeTalkTo called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdChangeTalkTo(reader.ReadUInt(), reader.ReadBool());
		}
	}

	protected void UserCode_CmdRequestInterior(int xPos, int yPos)
	{
		NetworkMapSharer.Instance.requestInterior(xPos, yPos);
	}

	protected static void InvokeUserCode_CmdRequestInterior(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestInterior called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestInterior(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdUpgradeGuestHouse(int xPos, int yPos)
	{
		NetworkMapSharer.Instance.RpcGiveOnTileStatus(1, xPos, yPos);
	}

	protected static void InvokeUserCode_CmdUpgradeGuestHouse(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUpgradeGuestHouse called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdUpgradeGuestHouse(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdRequestHouseInterior(int xPos, int yPos)
	{
		HouseDetails houseInfo = HouseManager.manage.getHouseInfo(xPos, yPos);
		NetworkMapSharer.Instance.TargetRequestHouse(base.connectionToClient, xPos, yPos, WorldManager.Instance.getHouseDetailsArray(houseInfo.houseMapOnTile), WorldManager.Instance.getHouseDetailsArray(houseInfo.houseMapOnTileStatus), WorldManager.Instance.getHouseDetailsArray(houseInfo.houseMapRotation), houseInfo.wall, houseInfo.floor, ItemOnTopManager.manage.getAllItemsOnTopInHouse(houseInfo));
	}

	protected static void InvokeUserCode_CmdRequestHouseInterior(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestHouseInterior called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestHouseInterior(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdRequestHouseExterior(int xPos, int yPos)
	{
		HouseExterior houseExterior = HouseManager.manage.getHouseExterior(xPos, yPos);
		NetworkMapSharer.Instance.TargetRequestExterior(base.connectionToClient, xPos, yPos, houseExterior.houseBase, houseExterior.roof, houseExterior.windows, houseExterior.door, houseExterior.wallMat, houseExterior.wallColor, houseExterior.houseMat, houseExterior.houseColor, houseExterior.roofMat, houseExterior.roofColor, houseExterior.fence, houseExterior.houseName);
	}

	protected static void InvokeUserCode_CmdRequestHouseExterior(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestHouseExterior called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestHouseExterior(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdDonateItemToMuseum(int itemId, string playerName)
	{
		NetworkMapSharer.Instance.RpcAddToMuseum(itemId, playerName);
	}

	protected static void InvokeUserCode_CmdDonateItemToMuseum(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDonateItemToMuseum called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdDonateItemToMuseum(reader.ReadInt(), reader.ReadString());
		}
	}

	protected void UserCode_CmdRequestShopStatus()
	{
		NetworkMapSharer.Instance.TargetRequestShopStall(base.connectionToClient, ShopManager.manage.getBoolArrayForSync());
	}

	protected static void InvokeUserCode_CmdRequestShopStatus(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestShopStatus called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestShopStatus();
		}
	}

	protected void UserCode_CmdRequestMuseumInterior()
	{
		NetworkMapSharer.Instance.TargetRequestMuseum(base.connectionToClient, MuseumManager.manage.fishDonated, MuseumManager.manage.bugsDonated, MuseumManager.manage.underWaterCreaturesDonated);
		StartCoroutine(NetworkMapSharer.Instance.sendPaintingsToClient(base.connectionToClient));
	}

	protected static void InvokeUserCode_CmdRequestMuseumInterior(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestMuseumInterior called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestMuseumInterior();
		}
	}

	protected void UserCode_CmdChangeUnderWater(bool newUnderWater)
	{
		NetworkunderWater = newUnderWater;
	}

	protected static void InvokeUserCode_CmdChangeUnderWater(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeUnderWater called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdChangeUnderWater(reader.ReadBool());
		}
	}

	protected void UserCode_CmdNPCStartFollow(uint tellNPCtoFollow, uint transformToFollow)
	{
		NetworkIdentity.spawned[tellNPCtoFollow].GetComponent<NPCAI>().NetworkfollowingNetId = transformToFollow;
		if (transformToFollow != 0)
		{
			followedBy = NetworkIdentity.spawned[tellNPCtoFollow].GetComponent<NPCAI>().myId.NPCNo;
		}
		else
		{
			followedBy = -1;
		}
	}

	protected static void InvokeUserCode_CmdNPCStartFollow(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdNPCStartFollow called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdNPCStartFollow(reader.ReadUInt(), reader.ReadUInt());
		}
	}

	protected void UserCode_CmdUpdateCurrentlyInside(int insideId)
	{
		currentlyInsideBuilding = (NPCSchedual.Locations)insideId;
	}

	protected static void InvokeUserCode_CmdUpdateCurrentlyInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUpdateCurrentlyInside called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdUpdateCurrentlyInside(reader.ReadInt());
		}
	}

	protected void UserCode_CmdRequestMapChunk(int chunkPosX, int chunkPosY)
	{
		NetworkMapSharer.Instance.callRequest(base.connectionToClient, chunkPosX, chunkPosY);
	}

	protected static void InvokeUserCode_CmdRequestMapChunk(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestMapChunk called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestMapChunk(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdRequestItemOnTopForChunk(int chunkX, int chunkY)
	{
		ItemOnTop[] itemsOnTopInChunk = WorldManager.Instance.getItemsOnTopInChunk(chunkX, chunkY);
		if (WorldManager.Instance.chunkHasItemsOnTop(chunkX, chunkY))
		{
			NetworkMapSharer.Instance.TargetGiveChunkOnTopDetails(base.connectionToClient, itemsOnTopInChunk);
		}
	}

	protected static void InvokeUserCode_CmdRequestItemOnTopForChunk(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestItemOnTopForChunk called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestItemOnTopForChunk(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdSendChatMessage(string newMessage)
	{
		NetworkMapSharer.Instance.RpcMakeChatBubble(newMessage, base.netId);
	}

	protected static void InvokeUserCode_CmdSendChatMessage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendChatMessage called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSendChatMessage(reader.ReadString());
		}
	}

	protected void UserCode_CmdSendEmote(int newEmote)
	{
		NetworkMapSharer.Instance.RpcCharEmotes(newEmote, base.netId);
	}

	protected static void InvokeUserCode_CmdSendEmote(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendEmote called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSendEmote(reader.ReadInt());
		}
	}

	protected void UserCode_CmdDealDirectDamage(uint netId, int damageAmount)
	{
		if (NetworkIdentity.spawned.ContainsKey(netId))
		{
			NetworkIdentity.spawned[netId].GetComponent<Damageable>().attackAndDoDamage(damageAmount, base.transform);
		}
	}

	protected static void InvokeUserCode_CmdDealDirectDamage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDealDirectDamage called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdDealDirectDamage(reader.ReadUInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdDealDamage(uint netId, float multiplier)
	{
		if (!NetworkIdentity.spawned.ContainsKey(netId))
		{
			return;
		}
		Damageable component = NetworkIdentity.spawned[netId].GetComponent<Damageable>();
		if ((bool)component && component.IsAVehicle() && !myEquip.myPermissions.CheckIfCanInteractWithVehicles())
		{
			return;
		}
		if (Inventory.Instance.allItems[myEquip.currentlyHoldingItemId].weaponDamage * multiplier > 0f)
		{
			component.attackAndDoDamage(Mathf.RoundToInt(Inventory.Instance.allItems[myEquip.currentlyHoldingItemId].weaponDamage * multiplier), base.transform, Inventory.Instance.allItems[myEquip.currentlyHoldingItemId].weaponKnockback);
		}
		if ((bool)myEquip.itemCurrentlyHolding)
		{
			MeleeAttacks component2 = myEquip.itemCurrentlyHolding.itemPrefab.GetComponent<MeleeAttacks>();
			if ((bool)component2 && component2.myHitBox.checkForStun())
			{
				if (component2.myHitBox.stunWithLight)
				{
					component.stunWithLight(component2.myHitBox.attackDamageAmount);
				}
				else
				{
					component.stun();
				}
			}
		}
		if (component.health <= 0)
		{
			if ((bool)component.isAnAnimal() && component.health <= 0)
			{
				NetworkMapSharer.Instance.TargetGiveHuntingXp(base.connectionToClient, component.isAnAnimal().animalId, component.isAnAnimal().getVariationNo());
			}
		}
		else if (Inventory.Instance.allItems[myEquip.currentlyHoldingItemId].itemPrefab.GetComponent<MeleeAttacks>().myHitBox.fireDamage)
		{
			component.setOnFire();
		}
	}

	protected static void InvokeUserCode_CmdDealDamage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDealDamage called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdDealDamage(reader.ReadUInt(), reader.ReadFloat());
		}
	}

	protected void UserCode_CmdTakeDamage(int damageAmount)
	{
		GetComponent<Damageable>().attackAndDoDamage(damageAmount, base.transform);
	}

	protected static void InvokeUserCode_CmdTakeDamage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTakeDamage called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdTakeDamage(reader.ReadInt());
		}
	}

	protected void UserCode_CmdCloseChest(int xPos, int yPos)
	{
		ContainerManager.manage.playerCloseChest(xPos, yPos, myInteract.InsideHouseDetails);
	}

	protected static void InvokeUserCode_CmdCloseChest(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCloseChest called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdCloseChest(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdRequestOnTileStatus(int xPos, int yPos)
	{
		NetworkMapSharer.Instance.RpcGiveOnTileStatus(WorldManager.Instance.onTileStatusMap[xPos, yPos], xPos, yPos);
	}

	protected static void InvokeUserCode_CmdRequestOnTileStatus(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestOnTileStatus called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestOnTileStatus(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdRequestTileRotation(int xPos, int yPos)
	{
		NetworkMapSharer.Instance.TargetGetRotationForTile(base.connectionToClient, xPos, yPos, WorldManager.Instance.rotationMap[xPos, yPos]);
	}

	protected static void InvokeUserCode_CmdRequestTileRotation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestTileRotation called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestTileRotation(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdReviveSelf()
	{
		GetComponent<Damageable>().NetworkonFire = false;
		GetComponent<Damageable>().Networkhealth = 5;
		Networkstamina = 5;
		RpcSetCharFaints(isFainted: false);
	}

	protected static void InvokeUserCode_CmdReviveSelf(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdReviveSelf called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdReviveSelf();
		}
	}

	protected void UserCode_CmdCatchBug(uint bugToCatch)
	{
		AnimalAI component = NetworkIdentity.spawned[bugToCatch].GetComponent<AnimalAI>();
		if ((bool)component)
		{
			NetworkNavMesh.nav.UnSpawnAnAnimal(component, saveToMap: false);
		}
		else
		{
			NetworkServer.Destroy(NetworkIdentity.spawned[bugToCatch].gameObject);
		}
	}

	protected static void InvokeUserCode_CmdCatchBug(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCatchBug called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdCatchBug(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdRemoveBugFromTerrarium(int bugIdToRemove, int xPos, int yPos)
	{
		ContainerManager.manage.GetBugFromTerrariun(bugIdToRemove, xPos, yPos);
	}

	protected static void InvokeUserCode_CmdRemoveBugFromTerrarium(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRemoveBugFromTerrarium called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRemoveBugFromTerrarium(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcUpdateStandOn(uint standingOnForRPC)
	{
		updateStandingOn(standingOnForRPC);
	}

	protected static void InvokeUserCode_RpcUpdateStandOn(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateStandOn called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcUpdateStandOn(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdSetNewStamina(int newStam)
	{
		Networkstamina = newStam;
	}

	protected static void InvokeUserCode_CmdSetNewStamina(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetNewStamina called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSetNewStamina(reader.ReadInt());
		}
	}

	protected void UserCode_CmdDropItem(int itemId, int stackAmount, Vector3 dropPos, Vector3 desirePos)
	{
		if ((bool)Inventory.Instance.allItems[itemId].bug)
		{
			RpcReleaseBug(itemId);
			return;
		}
		if ((bool)Inventory.Instance.allItems[itemId].fish)
		{
			RpcReleaseFish(itemId);
			return;
		}
		int myConnectedId = NetworkNavMesh.nav.GetMyConnectedId(this);
		NetworkMapSharer.Instance.CharDropsAServerDrop(itemId, stackAmount, dropPos, desirePos, myInteract.InsideHouseDetails, tryNotToStack: false, myConnectedId);
	}

	protected static void InvokeUserCode_CmdDropItem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDropItem called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdDropItem(reader.ReadInt(), reader.ReadInt(), reader.ReadVector3(), reader.ReadVector3());
		}
	}

	protected void UserCode_CmdSetSongForBoomBox(int itemId, int xPos, int yPos, int houseX, int houseY, int onTopPos)
	{
		if (houseX == -1 && houseY == -1)
		{
			if (onTopPos == -1)
			{
				if (WorldManager.Instance.onTileStatusMap[xPos, yPos] > 0)
				{
					myPickUp.TargetAddPickupToInv(base.connectionToClient, WorldManager.Instance.onTileStatusMap[xPos, yPos], 1);
				}
				NetworkMapSharer.Instance.RpcGiveOnTileStatus(itemId, xPos, yPos);
				return;
			}
			ItemOnTop itemOnTopInPosition = ItemOnTopManager.manage.getItemOnTopInPosition(onTopPos, xPos, yPos, null);
			if (itemOnTopInPosition != null && itemOnTopInPosition.itemStatus > 0)
			{
				myPickUp.TargetAddPickupToInv(base.connectionToClient, itemOnTopInPosition.itemStatus, 1);
			}
			NetworkMapSharer.Instance.RpcGiveOnTopStatus(itemId, xPos, yPos, onTopPos, houseX, houseY);
			return;
		}
		if (onTopPos == -1)
		{
			HouseDetails houseInfoIfExists = HouseManager.manage.getHouseInfoIfExists(houseX, houseY);
			if (houseInfoIfExists != null && houseInfoIfExists.houseMapOnTileStatus[xPos, yPos] > 0)
			{
				myPickUp.TargetAddPickupToInv(base.connectionToClient, houseInfoIfExists.houseMapOnTileStatus[xPos, yPos], 1);
			}
			NetworkMapSharer.Instance.RpcGiveOnTileStatusInside(itemId, xPos, yPos, houseX, houseY);
			return;
		}
		HouseDetails houseInfoIfExists2 = HouseManager.manage.getHouseInfoIfExists(houseX, houseY);
		if (houseInfoIfExists2 != null)
		{
			ItemOnTop itemOnTopInPosition2 = ItemOnTopManager.manage.getItemOnTopInPosition(onTopPos, xPos, yPos, houseInfoIfExists2);
			if (itemOnTopInPosition2 != null && itemOnTopInPosition2.itemStatus > 0)
			{
				myPickUp.TargetAddPickupToInv(base.connectionToClient, itemOnTopInPosition2.itemStatus, 1);
			}
		}
		NetworkMapSharer.Instance.RpcGiveOnTopStatus(itemId, xPos, yPos, onTopPos, houseX, houseY);
	}

	protected static void InvokeUserCode_CmdSetSongForBoomBox(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetSongForBoomBox called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSetSongForBoomBox(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPlaceFishInPond(int itemId, int xPos, int yPos)
	{
		RpcReleaseFish(itemId);
		ContainerManager.manage.AddFishToPond(itemId, xPos, yPos);
	}

	protected static void InvokeUserCode_CmdPlaceFishInPond(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceFishInPond called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdPlaceFishInPond(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPlaceBugInTerrarium(int itemId, int xPos, int yPos)
	{
		RpcReleaseBug(itemId);
		ContainerManager.manage.AddFishToPond(itemId, xPos, yPos);
	}

	protected static void InvokeUserCode_CmdPlaceBugInTerrarium(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceBugInTerrarium called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdPlaceBugInTerrarium(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcReleaseBug(int bugId)
	{
		Object.Instantiate(AnimalManager.manage.releasedBug, base.transform.position + base.transform.forward, base.transform.rotation).GetComponent<ReleaseBug>().setUpForBug(bugId);
	}

	protected static void InvokeUserCode_RpcReleaseBug(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcReleaseBug called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcReleaseBug(reader.ReadInt());
		}
	}

	protected void UserCode_RpcReleaseFish(int fishId)
	{
		Object.Instantiate(AnimalManager.manage.releaseFish, base.transform.position + base.transform.forward, base.transform.rotation).GetComponent<ReleaseBug>().setUpForFish(fishId);
	}

	protected static void InvokeUserCode_RpcReleaseFish(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcReleaseFish called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcReleaseFish(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeStandingOn(uint newStandOn)
	{
		if (newStandOn == 0 || NetworkIdentity.spawned.ContainsKey(newStandOn))
		{
			NetworkstandingOn = newStandOn;
			RpcUpdateStandOn(standingOn);
		}
		else
		{
			NetworkstandingOn = 0u;
			RpcUpdateStandOn(standingOn);
		}
	}

	protected static void InvokeUserCode_CmdChangeStandingOn(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeStandingOn called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdChangeStandingOn(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdAgreeToCraftsmanCrafting()
	{
		NetworkMapSharer.Instance.NetworkcraftsmanWorking = true;
	}

	protected static void InvokeUserCode_CmdAgreeToCraftsmanCrafting(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAgreeToCraftsmanCrafting called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdAgreeToCraftsmanCrafting();
		}
	}

	protected void UserCode_CmdPlaceAnimalInCollectionPoint(uint animalTrapPlaced)
	{
		PickUpAndCarry component = NetworkIdentity.spawned[animalTrapPlaced].GetComponent<PickUpAndCarry>();
		if ((bool)component)
		{
			TrappedAnimal component2 = NetworkIdentity.spawned[animalTrapPlaced].GetComponent<TrappedAnimal>();
			int rewardForCapturingAnimalIncludingBulletinBoards = BulletinBoard.board.getRewardForCapturingAnimalIncludingBulletinBoards(component2.trappedAnimalId, component2.trappedAnimalVariation);
			NetworkMapSharer.Instance.RpcDeliverAnimal(component.GetLastCarriedBy(), component2.trappedAnimalId, component2.trappedAnimalVariation, rewardForCapturingAnimalIncludingBulletinBoards, Inventory.Instance.getInvItemId(component2.trapItemDropAfterOpen));
			component.Networkdelivered = true;
		}
	}

	protected static void InvokeUserCode_CmdPlaceAnimalInCollectionPoint(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceAnimalInCollectionPoint called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdPlaceAnimalInCollectionPoint(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdSpawnAnimalBox(int animalId, int variation, string animalName, Vector3 position, Quaternion rotation)
	{
		GameObject gameObject = Object.Instantiate(FarmAnimalMenu.menu.animalBoxPrefab, position, rotation);
		gameObject.GetComponent<AnimalCarryBox>().setUp(animalId, variation, animalName);
		NetworkServer.Spawn(gameObject, base.connectionToClient);
		StartCoroutine(moveBoxToPos(gameObject.GetComponent<PickUpAndCarry>()));
	}

	protected static void InvokeUserCode_CmdSpawnAnimalBox(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnAnimalBox called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSpawnAnimalBox(reader.ReadInt(), reader.ReadInt(), reader.ReadString(), reader.ReadVector3(), reader.ReadQuaternion());
		}
	}

	protected void UserCode_CmdSellByWeight(uint itemPlaced)
	{
		NetworkIdentity.spawned[itemPlaced].GetComponent<PickUpAndCarry>().RemoveAuthorityBeforeBeforeServerDestroy();
		NetworkServer.Destroy(NetworkIdentity.spawned[itemPlaced].gameObject);
	}

	protected static void InvokeUserCode_CmdSellByWeight(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSellByWeight called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSellByWeight(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdActivateTrap(uint animalToTrapId, int xPos, int yPos)
	{
		AnimalAI component = NetworkIdentity.spawned[animalToTrapId].GetComponent<AnimalAI>();
		if ((bool)component && WorldManager.Instance.onTileMap[xPos, yPos] != -1)
		{
			GameObject original = NetworkMapSharer.Instance.trapObject;
			if (WorldManager.Instance.onTileMap[xPos, yPos] == 306)
			{
				original = NetworkMapSharer.Instance.stickTrapObject;
			}
			NetworkNavMesh.nav.UnSpawnAnAnimal(component, saveToMap: false);
			TrappedAnimal component2 = Object.Instantiate(original, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2), Quaternion.identity).GetComponent<TrappedAnimal>();
			component2.NetworktrappedAnimalId = component.animalId;
			component2.NetworktrappedAnimalVariation = component.getVariationNo();
			NetworkServer.Spawn(component2.gameObject);
			NetworkMapSharer.Instance.RpcActivateTrap(xPos, yPos);
			WorldManager.Instance.onTileMap[xPos, yPos] = -1;
		}
	}

	protected static void InvokeUserCode_CmdActivateTrap(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdActivateTrap called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdActivateTrap(reader.ReadUInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdSetOnFire(uint damageableId)
	{
		NetworkIdentity.spawned[damageableId].GetComponent<Damageable>().setOnFire();
	}

	protected static void InvokeUserCode_CmdSetOnFire(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetOnFire called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSetOnFire(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdBuyItemFromStall(int stallType, int shopStallNo)
	{
		NetworkMapSharer.Instance.RpcStallSold(stallType, shopStallNo);
	}

	protected static void InvokeUserCode_CmdBuyItemFromStall(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdBuyItemFromStall called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdBuyItemFromStall(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdCloseCamera()
	{
		RpcCloseCamera();
	}

	protected static void InvokeUserCode_CmdCloseCamera(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCloseCamera called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdCloseCamera();
		}
	}

	protected void UserCode_RpcCloseCamera()
	{
		if (!base.isLocalPlayer && (bool)myEquip.itemCurrentlyHolding && myEquip.itemCurrentlyHolding.itemName == "Camera")
		{
			myEquip.holdingPrefabAnimator.SetTrigger("CloseCamera");
		}
	}

	protected static void InvokeUserCode_RpcCloseCamera(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCloseCamera called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcCloseCamera();
		}
	}

	protected void UserCode_RpcTakeKnockback(Vector3 knockBackDir, float knockBackAmount)
	{
		if (base.isLocalPlayer && !beingKnockedBack)
		{
			StartCoroutine(knockBack(knockBackDir, knockBackAmount));
		}
	}

	protected static void InvokeUserCode_RpcTakeKnockback(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcTakeKnockback called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcTakeKnockback(reader.ReadVector3(), reader.ReadFloat());
		}
	}

	protected void UserCode_CmdChangeClockTickSpeed(float newWorldSpeed)
	{
		RealWorldTimeLight.time.NetworktimeScale = newWorldSpeed;
	}

	protected static void InvokeUserCode_CmdChangeClockTickSpeed(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeClockTickSpeed called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdChangeClockTickSpeed(reader.ReadFloat());
		}
	}

	protected void UserCode_CmdPlacePlayerPlacedIconOnMap(Vector2 position, int iconSpriteIndex)
	{
		NetworkServer.Spawn(RenderMap.Instance.CreateNewNetworkedPlayerSetMarker(position, iconSpriteIndex).gameObject);
	}

	protected static void InvokeUserCode_CmdPlacePlayerPlacedIconOnMap(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlacePlayerPlacedIconOnMap called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdPlacePlayerPlacedIconOnMap(reader.ReadVector2(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdSetPlayerPlacedMapIconHighlightValue(uint netId, bool value)
	{
		NetworkIdentity.spawned[netId].gameObject.GetComponent<mapIcon>().NetworkIconShouldBeHighlighted = value;
	}

	protected static void InvokeUserCode_CmdSetPlayerPlacedMapIconHighlightValue(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetPlayerPlacedMapIconHighlightValue called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSetPlayerPlacedMapIconHighlightValue(reader.ReadUInt(), reader.ReadBool());
		}
	}

	protected void UserCode_CommandRemovePlayerPlacedMapIcon(uint netId)
	{
		NetworkServer.Destroy(NetworkIdentity.spawned[netId].gameObject);
	}

	protected static void InvokeUserCode_CommandRemovePlayerPlacedMapIcon(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CommandRemovePlayerPlacedMapIcon called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CommandRemovePlayerPlacedMapIcon(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdToggleHighlightForAutomaticallySetMapIcon(int tileX, int tileY)
	{
		ToggleHighlightForAutomaticallySetMapIcon(tileX, tileY);
	}

	protected static void InvokeUserCode_CmdToggleHighlightForAutomaticallySetMapIcon(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdToggleHighlightForAutomaticallySetMapIcon called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdToggleHighlightForAutomaticallySetMapIcon(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdRequestNPCInv(int npcId)
	{
		NPCInventory nPCInventory = NPCManager.manage.npcInvs[npcId];
		NPCAI nPCAI = NPCManager.manage.returnLiveAgentWithNPCId(npcId);
		uint num = 0u;
		if ((bool)nPCAI)
		{
			num = nPCAI.netId;
		}
		NetworkMapSharer.Instance.RpcFillVillagerDetails(num, npcId, nPCInventory.isFem, nPCInventory.nameId, nPCInventory.skinId, nPCInventory.hairId, nPCInventory.hairColorId, nPCInventory.eyesId, nPCInventory.eyeColorId, nPCInventory.shirtId, nPCInventory.pantsId, nPCInventory.shoesId);
	}

	protected static void InvokeUserCode_CmdRequestNPCInv(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestNPCInv called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestNPCInv(reader.ReadInt());
		}
	}

	protected void UserCode_CmdPayTownDebt(int payment)
	{
		TownManager.manage.payTownDebt(payment);
		NetworkMapSharer.Instance.RpcPayTownDebt(payment, base.netId);
	}

	protected static void InvokeUserCode_CmdPayTownDebt(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPayTownDebt called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdPayTownDebt(reader.ReadInt());
		}
	}

	protected void UserCode_CmdGetDeedIngredients(int buildingId)
	{
		NetworkMapSharer.Instance.TargetOpenBuildWindowForClient(base.connectionToClient, buildingId, DeedManager.manage.getItemsAlreadyGivenForDeed(buildingId));
	}

	protected static void InvokeUserCode_CmdGetDeedIngredients(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGetDeedIngredients called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdGetDeedIngredients(reader.ReadInt());
		}
	}

	protected void UserCode_CmdDonateDeedIngredients(int buildingId, int[] alreadyGiven)
	{
		NetworkMapSharer.Instance.RpcRefreshDeedIngredients(buildingId, alreadyGiven);
	}

	protected static void InvokeUserCode_CmdDonateDeedIngredients(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDonateDeedIngredients called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdDonateDeedIngredients(reader.ReadInt(), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader));
		}
	}

	protected void UserCode_CmdCharFaints()
	{
		RpcSetCharFaints(isFainted: true);
	}

	protected static void InvokeUserCode_CmdCharFaints(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCharFaints called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdCharFaints();
		}
	}

	protected void UserCode_RpcSetCharFaints(bool isFainted)
	{
		myAnim.SetBool("Fainted", isFainted);
		reviveBox.SetActive(isFainted);
	}

	protected static void InvokeUserCode_RpcSetCharFaints(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCharFaints called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcSetCharFaints(reader.ReadBool());
		}
	}

	protected void UserCode_CmdChangeBlocking(bool isBlocking)
	{
		myEquip.Networkblocking = isBlocking;
	}

	protected static void InvokeUserCode_CmdChangeBlocking(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeBlocking called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdChangeBlocking(reader.ReadBool());
		}
	}

	protected void UserCode_CmdAcceptBulletinBoardPost(int id)
	{
		RpcAcceptBulletinBoardPost(id);
	}

	protected static void InvokeUserCode_CmdAcceptBulletinBoardPost(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAcceptBulletinBoardPost called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdAcceptBulletinBoardPost(reader.ReadInt());
		}
	}

	protected void UserCode_RpcAcceptBulletinBoardPost(int id)
	{
		BulletinBoard.board.attachedPosts[id].acceptTask(this);
		BulletinBoard.board.showSelectedPost();
		BulletinBoard.board.updateTaskButtons();
	}

	protected static void InvokeUserCode_RpcAcceptBulletinBoardPost(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAcceptBulletinBoardPost called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcAcceptBulletinBoardPost(reader.ReadInt());
		}
	}

	protected void UserCode_CmdCompleteBulletinBoardPost(int id)
	{
		RpcCompleteBulletinBoardPost(id);
	}

	protected static void InvokeUserCode_CmdCompleteBulletinBoardPost(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCompleteBulletinBoardPost called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdCompleteBulletinBoardPost(reader.ReadInt());
		}
	}

	protected void UserCode_RpcCompleteBulletinBoardPost(int id)
	{
		BulletinBoard.board.attachedPosts[id].completeTask(this);
		BulletinBoard.board.showSelectedPost();
		BulletinBoard.board.updateTaskButtons();
	}

	protected static void InvokeUserCode_RpcCompleteBulletinBoardPost(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCompleteBulletinBoardPost called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcCompleteBulletinBoardPost(reader.ReadInt());
		}
	}

	protected void UserCode_CmdSetDefenceBuff(float newDefence)
	{
		GetComponent<Damageable>().defence = newDefence;
	}

	protected static void InvokeUserCode_CmdSetDefenceBuff(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetDefenceBuff called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSetDefenceBuff(reader.ReadFloat());
		}
	}

	protected void UserCode_CmdSetFireResistance(int resistanceLevel)
	{
		GetComponent<Damageable>().SetFlameResistance(resistanceLevel);
	}

	protected static void InvokeUserCode_CmdSetFireResistance(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetFireResistance called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSetFireResistance(reader.ReadInt());
		}
	}

	protected void UserCode_CmdFireAOE()
	{
		RpcFireAOE();
	}

	protected static void InvokeUserCode_CmdFireAOE(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdFireAOE called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdFireAOE();
		}
	}

	protected void UserCode_RpcFireAOE()
	{
		if ((bool)myEquip.holdingPrefab)
		{
			MeleeAttacks component = myEquip.holdingPrefab.GetComponent<MeleeAttacks>();
			if ((bool)component.spawnAOEObjectOnAttack || component.fireProjectileAoe)
			{
				component.fireAOE();
			}
		}
	}

	protected static void InvokeUserCode_RpcFireAOE(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcFireAOE called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcFireAOE();
		}
	}

	protected void UserCode_CmdSetHealthRegen(float timer, int level)
	{
		GetComponent<Damageable>().startRegenAndSetTimer(timer, level);
	}

	protected static void InvokeUserCode_CmdSetHealthRegen(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetHealthRegen called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSetHealthRegen(reader.ReadFloat(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdGiveHealthBack(int amount)
	{
		GetComponent<Damageable>().changeHealth(amount);
	}

	protected static void InvokeUserCode_CmdGiveHealthBack(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGiveHealthBack called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdGiveHealthBack(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeClimbing(bool newClimb)
	{
		Networkclimbing = newClimb;
	}

	protected static void InvokeUserCode_CmdChangeClimbing(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeClimbing called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdChangeClimbing(reader.ReadBool());
		}
	}

	protected void UserCode_CmdTeleport(string teledir)
	{
		int[] pos = new int[2];
		switch (teledir)
		{
		case "private":
			pos = new int[2]
			{
				(int)NetworkMapSharer.Instance.privateTowerPos.x,
				(int)NetworkMapSharer.Instance.privateTowerPos.y
			};
			break;
		case "north":
			pos = TownManager.manage.northTowerPos;
			break;
		case "east":
			pos = TownManager.manage.eastTowerPos;
			break;
		case "south":
			pos = TownManager.manage.southTowerPos;
			break;
		case "west":
			pos = TownManager.manage.westTowerPos;
			break;
		}
		RpcTeleportChar(pos);
	}

	protected static void InvokeUserCode_CmdTeleport(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTeleport called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdTeleport(reader.ReadString());
		}
	}

	protected void UserCode_CmdTeleportToSignal()
	{
		RpcTeleportCharToVector(NetworkMapSharer.Instance.GetSignalPosition());
	}

	protected static void InvokeUserCode_CmdTeleportToSignal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTeleportToSignal called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdTeleportToSignal();
		}
	}

	protected void UserCode_RpcTeleportCharToVector(Vector3 endPos)
	{
		StartCoroutine(TeleportCharToVector(endPos));
	}

	protected static void InvokeUserCode_RpcTeleportCharToVector(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcTeleportCharToVector called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcTeleportCharToVector(reader.ReadVector3());
		}
	}

	protected void UserCode_RpcTeleportChar(int[] pos)
	{
		StartCoroutine(teleportCharToPos(pos));
	}

	protected static void InvokeUserCode_RpcTeleportChar(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcTeleportChar called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcTeleportChar(GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader));
		}
	}

	protected void UserCode_TargetKick(NetworkConnection conn)
	{
		CustomNetworkManager.manage.lobby.LeaveGameLobby();
		SaveLoad.saveOrLoad.returnToMenu();
	}

	protected static void InvokeUserCode_TargetKick(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetKick called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_TargetKick(NetworkClient.readyConnection);
		}
	}

	protected void UserCode_CmdUpdateHouseExterior(HouseExterior exterior)
	{
		NetworkMapSharer.Instance.RpcUpdateHouseExterior(exterior);
	}

	protected static void InvokeUserCode_CmdUpdateHouseExterior(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUpdateHouseExterior called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdUpdateHouseExterior(GeneratedNetworkCode._Read_HouseExterior(reader));
		}
	}

	protected void UserCode_CmdRainMaker()
	{
		WeatherManager.Instance.RpcMakeItRainTomorrow();
	}

	protected static void InvokeUserCode_CmdRainMaker(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRainMaker called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRainMaker();
		}
	}

	protected void UserCode_CmdCupOfSunshine()
	{
		WeatherManager.Instance.RpcMakeItSunnyToday();
	}

	protected static void InvokeUserCode_CmdCupOfSunshine(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCupOfSunshine called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdCupOfSunshine();
		}
	}

	protected void UserCode_TargetCheckVersion(NetworkConnection conn, int invItemCount, int worldItemsCount)
	{
		if (Inventory.Instance.allItems.Length != invItemCount || WorldManager.Instance.allObjects.Length != worldItemsCount)
		{
			NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NotSameVersionError"), ConversationGenerator.generate.GetNotificationText("NotSameVersionError_Sub"));
		}
	}

	protected static void InvokeUserCode_TargetCheckVersion(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetCheckVersion called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_TargetCheckVersion(NetworkClient.readyConnection, reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdSetMinelayer(int newLayerNo)
	{
		RealWorldTimeLight.time.NetworkmineLevel = newLayerNo;
	}

	protected static void InvokeUserCode_CmdSetMinelayer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetMinelayer called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSetMinelayer(reader.ReadInt());
		}
	}

	protected void UserCode_CmdTakePhotoSound(Vector3 position)
	{
		RpcCameraEffectSound(position);
	}

	protected static void InvokeUserCode_CmdTakePhotoSound(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTakePhotoSound called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdTakePhotoSound(reader.ReadVector3());
		}
	}

	protected void UserCode_CmdTakeItemFromNPC(uint netId)
	{
		if (NetworkIdentity.spawned.ContainsKey(netId))
		{
			NetworkIdentity.spawned[netId].GetComponent<NPCHoldsItems>().changeItemHolding(-1);
		}
	}

	protected static void InvokeUserCode_CmdTakeItemFromNPC(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTakeItemFromNPC called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdTakeItemFromNPC(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdMarkTreasureOnMap()
	{
		NetworkMapSharer.Instance.MarkTreasureOnMapAndSpawn();
	}

	protected static void InvokeUserCode_CmdMarkTreasureOnMap(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdMarkTreasureOnMap called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdMarkTreasureOnMap();
		}
	}

	protected void UserCode_RpcCameraEffectSound(Vector3 position)
	{
		if (!base.isLocalPlayer)
		{
			SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.cameraSound, position);
		}
	}

	protected static void InvokeUserCode_RpcCameraEffectSound(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCameraEffectSound called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcCameraEffectSound(reader.ReadVector3());
		}
	}

	protected void UserCode_CmdRequestEntranceMapIcon()
	{
		if (RealWorldTimeLight.time.underGround)
		{
			TargetScanMapIconAtPosition(base.connectionToClient, WorldManager.Instance.onTileMap[(int)GenerateUndergroundMap.generate.entrancePosition.x, (int)GenerateUndergroundMap.generate.entrancePosition.y], (int)GenerateUndergroundMap.generate.entrancePosition.x, (int)GenerateUndergroundMap.generate.entrancePosition.y);
		}
		else
		{
			TargetScanMapIconAtPosition(base.connectionToClient, WorldManager.Instance.onTileMap[(int)GenerateVisitingIsland.Instance.entrancePosition.x, (int)GenerateVisitingIsland.Instance.entrancePosition.y], (int)GenerateVisitingIsland.Instance.entrancePosition.x, (int)GenerateVisitingIsland.Instance.entrancePosition.y);
		}
	}

	protected static void InvokeUserCode_CmdRequestEntranceMapIcon(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestEntranceMapIcon called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestEntranceMapIcon();
		}
	}

	protected void UserCode_TargetScanMapIconAtPosition(NetworkConnection conn, int tileId, int xPos, int yPos)
	{
		WorldManager.Instance.onTileMap[xPos, yPos] = tileId;
		RenderMap.Instance.CheckIfNeedsIcon(xPos, yPos);
		if (RealWorldTimeLight.time.underGround)
		{
			RenderMap.Instance.RenameIcon(WorldManager.Instance.onTileMap[xPos, yPos], "Mine");
		}
		else
		{
			RenderMap.Instance.RenameIcon(WorldManager.Instance.onTileMap[xPos, yPos], "Airport");
		}
	}

	protected static void InvokeUserCode_TargetScanMapIconAtPosition(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetScanMapIconAtPosition called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_TargetScanMapIconAtPosition(NetworkClient.readyConnection, reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdMakeAWish(int wishType)
	{
		if (!NetworkMapSharer.Instance.wishManager.wishMadeToday)
		{
			NetworkMapSharer.Instance.wishManager.NetworkwishMadeToday = true;
			NetworkMapSharer.Instance.wishManager.tomorrowsWishType = wishType;
			NetworkMapSharer.Instance.RpcMakeAWish(myEquip.playerName, wishType, base.transform.position + base.transform.forward * 1.8f + Vector3.up * 1.9f);
		}
	}

	protected static void InvokeUserCode_CmdMakeAWish(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdMakeAWish called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdMakeAWish(reader.ReadInt());
		}
	}

	protected void UserCode_CmdKickABall(uint ballId, Vector3 kickDir, float power)
	{
		if (NetworkIdentity.spawned.ContainsKey(ballId))
		{
			NetworkIdentity.spawned[ballId].GetComponent<NetworkBall>().RpcKickInDirection(kickDir, power);
		}
	}

	protected static void InvokeUserCode_CmdKickABall(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdKickABall called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdKickABall(reader.ReadUInt(), reader.ReadVector3(), reader.ReadFloat());
		}
	}

	protected void UserCode_CmdUseInstaGrow(int xPos, int yPos)
	{
		if (WorldManager.Instance.onTileMap[xPos, yPos] >= 0 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages)
		{
			int give = Mathf.Clamp(WorldManager.Instance.onTileStatusMap[xPos, yPos] + Random.Range(1, 4), 0, WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages.objectStages.Length - 1);
			NetworkMapSharer.Instance.RpcGiveOnTileStatus(give, xPos, yPos);
			NetworkMapSharer.Instance.RpcUseInstagrow(xPos, yPos);
		}
	}

	protected static void InvokeUserCode_CmdUseInstaGrow(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUseInstaGrow called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdUseInstaGrow(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdSendTeleSignal()
	{
		NetworkMapSharer.Instance.CreateTeleSignal(base.transform.position);
	}

	protected static void InvokeUserCode_CmdSendTeleSignal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendTeleSignal called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSendTeleSignal();
		}
	}

	protected void UserCode_CmdRingTownBell()
	{
		NetworkMapSharer.Instance.RpcRingTownBell();
		RenderMap.Instance.ClearAllNPCMarkers();
		for (int i = 0; i < NPCManager.manage.NPCDetails.Length; i++)
		{
			if (NPCManager.manage.npcStatus[i].hasMovedIn)
			{
				NetworkMapSharer.Instance.MarkNPCOnMap(i);
			}
		}
		RenderMap.Instance.StartNPCMarkerCountdown();
	}

	protected static void InvokeUserCode_CmdRingTownBell(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRingTownBell called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRingTownBell();
		}
	}

	protected void UserCode_CmdChangeWeather(bool setWindy, bool setHeatWave, bool setRaining, bool setStorming, bool setFoggy, bool setSnowing, bool setMeteorshower)
	{
		WeatherManager.Instance.ChangeWindPatternsForDay(setWindy, setHeatWave, setRaining, setStorming, setFoggy, setSnowing, setMeteorshower);
	}

	protected static void InvokeUserCode_CmdChangeWeather(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeWeather called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdChangeWeather(reader.ReadBool(), reader.ReadBool(), reader.ReadBool(), reader.ReadBool(), reader.ReadBool(), reader.ReadBool(), reader.ReadBool());
		}
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(underWater);
			writer.WriteInt(stamina);
			writer.WriteUInt(standingOn);
			writer.WriteBool(inDanger);
			writer.WriteBool(climbing);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(underWater);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteInt(stamina);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteUInt(standingOn);
			result = true;
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteBool(inDanger);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			writer.WriteBool(climbing);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			bool flag = underWater;
			NetworkunderWater = reader.ReadBool();
			if (!SyncVarEqual(flag, ref underWater))
			{
				onChangeUnderWater(flag, underWater);
			}
			int num = stamina;
			Networkstamina = reader.ReadInt();
			if (!SyncVarEqual(num, ref stamina))
			{
				onChangeStamina(num, stamina);
			}
			uint num2 = standingOn;
			NetworkstandingOn = reader.ReadUInt();
			bool flag2 = inDanger;
			NetworkinDanger = reader.ReadBool();
			bool flag3 = climbing;
			Networkclimbing = reader.ReadBool();
			return;
		}
		long num3 = (long)reader.ReadULong();
		if ((num3 & 1L) != 0L)
		{
			bool flag4 = underWater;
			NetworkunderWater = reader.ReadBool();
			if (!SyncVarEqual(flag4, ref underWater))
			{
				onChangeUnderWater(flag4, underWater);
			}
		}
		if ((num3 & 2L) != 0L)
		{
			int num4 = stamina;
			Networkstamina = reader.ReadInt();
			if (!SyncVarEqual(num4, ref stamina))
			{
				onChangeStamina(num4, stamina);
			}
		}
		if ((num3 & 4L) != 0L)
		{
			uint num5 = standingOn;
			NetworkstandingOn = reader.ReadUInt();
		}
		if ((num3 & 8L) != 0L)
		{
			bool flag5 = inDanger;
			NetworkinDanger = reader.ReadBool();
		}
		if ((num3 & 0x10L) != 0L)
		{
			bool flag6 = climbing;
			Networkclimbing = reader.ReadBool();
		}
	}
}
