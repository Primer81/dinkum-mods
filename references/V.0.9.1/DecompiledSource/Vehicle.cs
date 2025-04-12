using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Vehicle : NetworkBehaviour
{
	public enum colourVariations
	{
		Bases,
		Black,
		Blue,
		Green,
		Orange,
		Pink,
		Purple,
		Red,
		White,
		Yellow,
		Chrome,
		Gold
	}

	public int saveId;

	public Transform driversPos;

	public BoxCollider driverBoxCollider;

	public Transform myHitBox;

	public Transform hitBoxFollow;

	[SyncVar(hook = "onDriverChange")]
	private uint driver;

	public Transform driverTrans;

	public CharMovement driverChar;

	public Rigidbody myRig;

	private bool beingDestroyed;

	[Header("Driver animation stuff")]
	public string driverSittingAnimationBoolName = "Sitting";

	public Transform rightHandle;

	public Transform leftHandle;

	public Transform leftFoot;

	public Transform rightFoot;

	public Transform lookAtPos;

	public VehicleMakeParticles vehicleAnimator;

	public ASound startupSound;

	public Sprite mapIconSprite;

	public Damageable damageWhenUnderWater;

	public int requiresLicenceLevel = 1;

	public bool animateCharAsWell;

	[Header("Vehicle Colours")]
	public bool canBePainted = true;

	[SyncVar(hook = "onColourChange")]
	public int colourVaration;

	public MeshRenderer[] meshToChangeColours;

	public Color defaultTint;

	public MeshRenderer[] meshRenderersToTintColours;

	private Material tintMat;

	[Header("Stuff to disable when its an animal you are riding")]
	public AnimalAI myAi;

	public AnimateAnimalAI myAnimalAnimations;

	private NetworkTransform driversNetworkTransform;

	public bool useCameraVehicleDistance;

	public WorldArea WorldAreaLocation;

	private Vector3 folVelocity;

	private Vector3 rotVelocity;

	private Vector3 previousPosition;

	private Vector3 currentPosition;

	public bool mountingAnimationComplete;

	public mapIcon MyMapIcon { get; private set; }

	public uint Networkdriver
	{
		get
		{
			return driver;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref driver))
			{
				uint old = driver;
				SetSyncVar(value, ref driver, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					onDriverChange(old, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public int NetworkcolourVaration
	{
		get
		{
			return colourVaration;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref colourVaration))
			{
				int oldColour = colourVaration;
				SetSyncVar(value, ref colourVaration, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, value: true);
					onColourChange(oldColour, value);
					setSyncVarHookGuard(2uL, value: false);
				}
			}
		}
	}

	public void setVariation(int newVariation)
	{
		NetworkcolourVaration = newVariation;
	}

	public int getVariation()
	{
		return colourVaration;
	}

	public override void OnStartServer()
	{
		int num = (int)base.transform.position.x / 2;
		int num2 = (int)base.transform.position.z / 2;
		Networkdriver = 0u;
		if (WorldManager.Instance.isPositionOnMap(num, num2))
		{
			if (WorldManager.Instance.waterMap[num, num2] && base.transform.position.y < 0.6f)
			{
				myRig.velocity.Set(0f, 0f, 0f);
				base.transform.position = new Vector3(base.transform.position.x, 0.6f, base.transform.position.z);
			}
			else if (base.transform.position.y <= (float)(WorldManager.Instance.heightMap[num, num2] - 1))
			{
				myRig.velocity.Set(0f, 0f, 0f);
				base.transform.position = new Vector3(base.transform.position.x, WorldManager.Instance.heightMap[(int)base.transform.position.x / 2, (int)base.transform.position.z / 2], base.transform.position.z);
			}
		}
		else if (base.transform.position.y < 0.6f)
		{
			myRig.velocity.Set(0f, 0f, 0f);
			base.transform.position = new Vector3(base.transform.position.x, 0.6f, base.transform.position.z);
		}
		myHitBox.gameObject.SetActive(value: true);
		InitializeMapIcon();
		UpdateCarryItemCarriedById();
		StartCoroutine(LavaLevelBehaviour());
	}

	public void destroyServerSelf()
	{
		if (saveId >= 0)
		{
			SaveLoad.saveOrLoad.vehiclesToSave.Remove(this);
			NetworkServer.Destroy(base.gameObject);
		}
	}

	public override void OnStopClient()
	{
		if ((bool)driverTrans && driverSittingAnimationBoolName != "")
		{
			driverTrans.GetComponent<Animator>().SetBool(driverSittingAnimationBoolName, value: false);
		}
		if (base.isServer && driver != 0)
		{
			NetworkIdentity.spawned[driver].GetComponent<CharPickUp>().RpcStopDrivingFromServer();
			onDriverChange(driver, 0u);
		}
	}

	public override void OnStartClient()
	{
		onDriverChange(driver, driver);
		onColourChange(colourVaration, colourVaration);
		myHitBox.parent = null;
		previousPosition = hitBoxFollow.position;
		currentPosition = hitBoxFollow.position;
	}

	private void Start()
	{
		myRig = GetComponent<Rigidbody>();
		vehicleAnimator = GetComponent<VehicleMakeParticles>();
		base.gameObject.AddComponent<InteractableObject>().isVehicle = this;
		WorldAreaLocation = RealWorldTimeLight.time.CurrentWorldArea;
		if (!myAi)
		{
			NetworkMapSharer.Instance.onChangeMaps.AddListener(OnUpdateVehicleVisibilityBasedOnWorldArea);
		}
	}

	private void InitializeMapIcon()
	{
		StartCoroutine(DelaySpawnMapIcon());
	}

	private IEnumerator DelaySpawnMapIcon()
	{
		yield return null;
		yield return null;
		if (saveId < 0 || !VehicleIsOnThisLevel())
		{
			yield break;
		}
		if (MyMapIcon == null)
		{
			GameObject gameObject = RenderMap.Instance.CreateMapIconForVehicle(base.netId, saveId).gameObject;
			MyMapIcon = gameObject.GetComponent<mapIcon>();
			MyMapIcon.NetworkVehicleFollowingId = base.netId;
			if (gameObject != null)
			{
				NetworkServer.Spawn(gameObject);
			}
		}
		else
		{
			MyMapIcon.NetworkVehicleFollowingId = base.netId;
		}
	}

	private void UpdateCarryItemCarriedById()
	{
		PickUpAndCarry[] componentsInChildren = hitBoxFollow.GetComponentsInChildren<PickUpAndCarry>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].NetworkbeingCarriedBy = base.netId;
		}
	}

	public override void OnStartAuthority()
	{
		if (Vector3.Distance(CameraController.control.transform.position, base.transform.position) <= (float)(NewChunkLoader.loader.getChunkDistance() * WorldManager.Instance.getChunkSize() - WorldManager.Instance.getChunkSize()))
		{
			GetComponent<Rigidbody>().isKinematic = false;
			if (driver == 0)
			{
				StartCoroutine(noDriverTimer());
			}
		}
		else
		{
			GetComponent<Rigidbody>().isKinematic = true;
			moveToFloorHeightIfUnder();
		}
	}

	public void moveToFloorHeightIfUnder()
	{
		if (WorldManager.Instance.isPositionOnMap((int)base.transform.position.x / 2, (int)base.transform.position.z / 2) && base.transform.position.y < (float)WorldManager.Instance.heightMap[(int)base.transform.position.x / 2, (int)base.transform.position.z / 2])
		{
			base.transform.position = new Vector3(base.transform.position.x, WorldManager.Instance.heightMap[(int)base.transform.position.x / 2, (int)base.transform.position.z / 2], base.transform.position.z);
		}
	}

	public void stopVehicleFallingOnMapChange()
	{
		StopAllCoroutines();
		GetComponent<Rigidbody>().isKinematic = true;
	}

	public override void OnStopAuthority()
	{
		GetComponent<Rigidbody>().isKinematic = true;
	}

	private void OnEnable()
	{
		if (saveId >= 0 && !SaveLoad.saveOrLoad.vehiclesToSave.Contains(this))
		{
			SaveLoad.saveOrLoad.vehiclesToSave.Add(this);
		}
		myHitBox.gameObject.SetActive(value: true);
	}

	public void OnDestroy()
	{
		if (beingDestroyed)
		{
			SaveLoad.saveOrLoad.vehiclesToSave.Remove(this);
		}
		if (base.isServer && (bool)MyMapIcon)
		{
			NetworkServer.Destroy(MyMapIcon.gameObject);
		}
		NetworkMapSharer.Instance.onChangeMaps.RemoveListener(OnUpdateVehicleVisibilityBasedOnWorldArea);
		if ((bool)myHitBox)
		{
			myHitBox.DetachChildren();
			Object.Destroy(myHitBox.gameObject);
			SetItemsCarriedToFallAfterDestroyed();
		}
	}

	private void OnUpdateVehicleVisibilityBasedOnWorldArea()
	{
		if (!NetworkMapSharer.Instance.isServer)
		{
			return;
		}
		if (!VehicleIsOnThisLevel() && base.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(value: false);
			hitBoxFollow.gameObject.SetActive(value: false);
			myHitBox.gameObject.SetActive(value: false);
			if ((bool)MyMapIcon)
			{
				NetworkServer.Destroy(MyMapIcon.gameObject);
				MyMapIcon = null;
			}
			NetworkServer.UnSpawn(base.gameObject);
		}
		else if (VehicleIsOnThisLevel() && !base.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(value: true);
			hitBoxFollow.gameObject.SetActive(value: true);
			myHitBox.gameObject.SetActive(value: false);
			NetworkServer.Spawn(base.gameObject);
			InitializeMapIcon();
		}
	}

	public bool currentlyHasDriver()
	{
		if (driver == 0)
		{
			return false;
		}
		return true;
	}

	private void LateUpdate()
	{
		if (!base.hasAuthority)
		{
			Vector3 position = Vector3.Lerp(previousPosition, currentPosition, Time.time / Time.fixedDeltaTime % 1f);
			myHitBox.position = position;
			myHitBox.rotation = Quaternion.Lerp(myHitBox.rotation, hitBoxFollow.rotation, Time.deltaTime * 20f);
		}
		if ((bool)driverChar && !driverChar.isLocalPlayer && mountingAnimationComplete)
		{
			driverTrans.position = driversPos.position;
			driverTrans.rotation = driversPos.rotation;
		}
	}

	private void Update()
	{
		if ((bool)driverChar && !driverChar.isLocalPlayer && mountingAnimationComplete)
		{
			driverTrans.position = driversPos.position;
			driverTrans.rotation = driversPos.rotation;
		}
	}

	private void SetItemsCarriedToFallAfterDestroyed()
	{
		if (base.isServer && NetworkServer.active)
		{
			NetworkMapSharer.Instance.ServerChangeAuthorityOfAllCarryObjectInProximity(base.transform.position);
		}
	}

	private void FixedUpdate()
	{
		previousPosition = currentPosition;
		currentPosition = hitBoxFollow.position;
		if (base.hasAuthority)
		{
			myHitBox.position = hitBoxFollow.position;
			myHitBox.rotation = hitBoxFollow.rotation;
		}
	}

	private void onDriverChange(uint old, uint newId)
	{
		Networkdriver = newId;
		if (newId == 0)
		{
			if ((bool)driverTrans)
			{
				if ((bool)driverChar)
				{
					driverChar.col.enabled = true;
					if ((bool)driversNetworkTransform)
					{
						driversNetworkTransform = null;
					}
					driverChar.myEquip.stopVehicleHands();
					if (driverSittingAnimationBoolName != "")
					{
						driverChar.myAnim.SetBool(driverSittingAnimationBoolName, value: false);
					}
					driverChar = null;
				}
				if ((bool)vehicleAnimator)
				{
					vehicleAnimator.setCharacterAnimator(null);
				}
			}
			myRig.useGravity = true;
			mountingAnimationComplete = false;
			driverTrans = null;
			driverBoxCollider.gameObject.SetActive(value: true);
		}
		else
		{
			if (NetworkIdentity.spawned.ContainsKey(newId))
			{
				driverTrans = NetworkIdentity.spawned[newId].GetComponent<Transform>();
				if ((bool)driverTrans)
				{
					StartCoroutine(moveDriverToSeat(driverTrans, driverTrans.GetComponent<Animator>()));
					driverChar = driverTrans.GetComponent<CharMovement>();
					driversNetworkTransform = driverChar.GetComponent<NetworkTransform>();
					if ((bool)driverChar)
					{
						driverChar.col.enabled = false;
					}
				}
			}
			if ((bool)startupSound)
			{
				SoundManager.Instance.playASoundAtPoint(startupSound, base.transform.position);
			}
			driverBoxCollider.gameObject.SetActive(value: false);
		}
		changeRigidbodyOnDriverChange();
		checkForAnimalAnimationOnDriverChange();
	}

	public void changeRigidbodyOnDriverChange()
	{
		if (base.hasAuthority)
		{
			if (driver == 0)
			{
				StartCoroutine(noDriverTimer());
			}
			else
			{
				myRig.isKinematic = false;
			}
		}
		else
		{
			myRig.isKinematic = true;
		}
	}

	public void startDriving(uint driverId)
	{
		Networkdriver = driverId;
		GetComponent<NetworkTransform>().RpcTeleport(base.transform.position);
	}

	public void StopDriving()
	{
		Networkdriver = 0u;
		if ((bool)myAi && base.isServer)
		{
			GetComponent<NetworkIdentity>().RemoveClientAuthority();
			GetComponent<NetworkIdentity>().AssignClientAuthority(NetworkMapSharer.Instance.localChar.connectionToClient);
		}
	}

	public void checkForAnimalAnimationOnDriverChange()
	{
		if (!myAi)
		{
			return;
		}
		if (driver == 0)
		{
			myRig.constraints = RigidbodyConstraints.FreezeAll;
			myAnimalAnimations.enabled = true;
			vehicleAnimator.enabled = false;
		}
		else
		{
			myRig.constraints = RigidbodyConstraints.FreezeRotation;
			myAnimalAnimations.enabled = false;
			vehicleAnimator.enabled = true;
			if (myAi.isSleeping())
			{
				myAi.wakeUpAnimal();
			}
			GetComponent<Animator>().SetBool("Sleeping", value: false);
		}
		if ((bool)myAi && base.hasAuthority && !base.isServer)
		{
			myAi.enabled = false;
		}
		else
		{
			myAi.enabled = true;
		}
		if ((bool)myAi && base.isServer)
		{
			if (driver == 0)
			{
				myAi.enabled = true;
				myAi.forceSetUp();
				myAi.myAgent.transform.position = base.transform.position;
				myAi.myAgent.transform.rotation = base.transform.rotation;
				myAi.myAgent.gameObject.SetActive(value: true);
			}
			else
			{
				myAi.enabled = false;
				myAi.myAgent.gameObject.SetActive(value: false);
			}
		}
	}

	public bool hasDriver()
	{
		return driver != 0;
	}

	private IEnumerator moveDriverToSeat(Transform driverTransform, Animator driverAnim)
	{
		mountingAnimationComplete = false;
		uint driverId = driver;
		float timer = 0f;
		Vector3 originalPos = driverTransform.position;
		if (driverSittingAnimationBoolName != "")
		{
			driverAnim.SetTrigger("StartDriving");
		}
		while (driverId == driver && timer < 0.25f)
		{
			timer += Time.deltaTime;
			driverTransform.position = Vector3.Lerp(originalPos, driversPos.position, timer / 0.25f);
			driverTrans.rotation = Quaternion.Lerp(driverTrans.rotation, driversPos.rotation, timer / 0.25f);
			yield return null;
			if (driverSittingAnimationBoolName != "")
			{
				driverAnim.SetBool(driverSittingAnimationBoolName, value: true);
			}
		}
		driverTrans.GetComponent<EquipItemToChar>().setVehicleHands(this);
		if ((bool)vehicleAnimator)
		{
			vehicleAnimator.setCharacterAnimator(driverTrans.GetComponent<Animator>());
		}
		mountingAnimationComplete = true;
	}

	private IEnumerator noDriverTimer()
	{
		if (base.hasAuthority)
		{
			float noDriverTimer = 0f;
			while (driver == 0 && noDriverTimer < 10f)
			{
				noDriverTimer += Time.deltaTime;
				yield return null;
				if (Vector3.Distance(CameraController.control.transform.position, base.transform.position) >= (float)(NewChunkLoader.loader.getChunkDistance() * WorldManager.Instance.getChunkSize() - WorldManager.Instance.getChunkSize()))
				{
					break;
				}
			}
			if (driver == 0)
			{
				myRig.isKinematic = true;
			}
		}
		moveToFloorHeightIfUnder();
		if (!myAi && driver == 0)
		{
			_ = base.hasAuthority;
		}
	}

	public void onColourChange(int oldColour, int newColour)
	{
		NetworkcolourVaration = Mathf.Clamp(newColour, 0, EquipWindow.equip.vehicleColours.Length - 1);
		if (meshToChangeColours.Length != 0)
		{
			for (int i = 0; i < meshToChangeColours.Length; i++)
			{
				meshToChangeColours[i].sharedMaterial = EquipWindow.equip.vehicleColours[colourVaration];
			}
		}
		if (meshRenderersToTintColours.Length != 0)
		{
			if (!tintMat)
			{
				tintMat = Object.Instantiate(meshRenderersToTintColours[0].sharedMaterial);
				for (int j = 0; j < meshRenderersToTintColours.Length; j++)
				{
					meshRenderersToTintColours[j].sharedMaterial = tintMat;
				}
			}
			if (colourVaration == 0)
			{
				tintMat.color = defaultTint;
				tintMat.SetFloat("_Glossiness", 0f);
				tintMat.SetFloat("_Metallic", 0f);
			}
			else
			{
				tintMat.color = EquipWindow.equip.vehicleColoursUI[colourVaration];
				tintMat.SetFloat("_Glossiness", EquipWindow.equip.vehicleColours[colourVaration].GetFloat("_Glossiness"));
				tintMat.SetFloat("_Metallic", EquipWindow.equip.vehicleColours[colourVaration].GetFloat("_Metallic"));
			}
		}
		if ((bool)MyMapIcon)
		{
			MyMapIcon.Icon.color = EquipWindow.equip.vehicleColoursUI[colourVaration];
		}
		if ((bool)GetComponent<CustomVehicleMaterialForColour>())
		{
			GetComponent<CustomVehicleMaterialForColour>().changeToNewColour(newColour);
		}
	}

	private bool VehicleIsOnThisLevel()
	{
		return WorldAreaLocation == RealWorldTimeLight.time.CurrentWorldArea;
	}

	private IEnumerator LavaLevelBehaviour()
	{
		float fireTimer = 10f;
		Damageable myDamage = GetComponent<Damageable>();
		while (RealWorldTimeLight.time.underGround && RealWorldTimeLight.time.mineLevel == 2)
		{
			if (fireTimer < 0f && !myDamage.onFire)
			{
				myDamage.NetworkonFire = true;
				fireTimer = 10f;
			}
			fireTimer -= Time.deltaTime;
			yield return null;
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
			writer.WriteUInt(driver);
			writer.WriteInt(colourVaration);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteUInt(driver);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteInt(colourVaration);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			uint num = driver;
			Networkdriver = reader.ReadUInt();
			if (!SyncVarEqual(num, ref driver))
			{
				onDriverChange(num, driver);
			}
			int num2 = colourVaration;
			NetworkcolourVaration = reader.ReadInt();
			if (!SyncVarEqual(num2, ref colourVaration))
			{
				onColourChange(num2, colourVaration);
			}
			return;
		}
		long num3 = (long)reader.ReadULong();
		if ((num3 & 1L) != 0L)
		{
			uint num4 = driver;
			Networkdriver = reader.ReadUInt();
			if (!SyncVarEqual(num4, ref driver))
			{
				onDriverChange(num4, driver);
			}
		}
		if ((num3 & 2L) != 0L)
		{
			int num5 = colourVaration;
			NetworkcolourVaration = reader.ReadInt();
			if (!SyncVarEqual(num5, ref colourVaration))
			{
				onColourChange(num5, colourVaration);
			}
		}
	}
}
