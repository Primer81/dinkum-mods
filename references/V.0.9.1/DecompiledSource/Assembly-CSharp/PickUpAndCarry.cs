using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Serialization;

public class PickUpAndCarry : NetworkBehaviour
{
	public int prefabId;

	[FormerlySerializedAs("objectsEnabledOnDelevered")]
	public GameObject[] objectsToEnableOnDelivered;

	public ASound pickUpSound;

	public ASound putDownSound;

	public ASound balloonFillSound;

	public bool investigationItem;

	public bool photoRequestable = true;

	public LayerMask groundedLayer;

	public Collider damageCollider;

	public float dropToPosY = 2f;

	[SyncVar(hook = "OnCarriedChanged")]
	public uint beingCarriedBy;

	[SyncVar(hook = "OnDelivered")]
	public bool delivered;

	[SyncVar]
	public bool canBePickedUp = true;

	[SerializeField]
	private bool isFragile;

	[SerializeField]
	private Animator toTriggerFloating;

	[SerializeField]
	private Transform[] otherVehicleCheckPos;

	public bool HasDoneLocalDamage;

	private bool underWater;

	private MeshRenderer myMeshRenderer;

	private NetworkIdentity myNetworkIdentity;

	private NetworkTransform myNetworkTransform;

	private DroppedItem.DroppedLocation droppedOnMap;

	private LayerMask vehicleLayer;

	private LayerMask carryItemLayer;

	private const float throwForceFowards = 5f;

	private const float throwForceUpwards = 3.5f;

	private const float throwForceUpwardsExtraForceInJump = 3f;

	private const float destroyIfFragileAtMagnitude = 13.5f;

	private float floatSpeed;

	private float massAtInit;

	private uint lastCarriedBy;

	private bool hasBeenThrown;

	private bool isLandingValid;

	private bool pickUpAnimPlayed;

	private bool localLanded;

	private readonly int carryItemLayerIndex = 6;

	private readonly int itemThrowLayerIndex = 7;

	private readonly float vehicleRayCastLength = 1.2f;

	private readonly float vehicleSphereCheckRadius = 0.5f;

	private Vector3 vehicleLocalPos;

	public HoldPos CarriedByHoldPos { get; set; }

	public Rigidbody MyRigidBody { get; private set; }

	public bool ResetAfterPlayerWithAuthorityDisconnected { get; set; }

	public bool IsCarriedByPlayer
	{
		get
		{
			foreach (CharMovement item in NetworkNavMesh.nav.charMovementConnected)
			{
				if (item.netIdentity.netId == beingCarriedBy)
				{
					return true;
				}
			}
			return false;
		}
	}

	public uint NetworkbeingCarriedBy
	{
		get
		{
			return beingCarriedBy;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref beingCarriedBy))
			{
				uint old = beingCarriedBy;
				SetSyncVar(value, ref beingCarriedBy, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					OnCarriedChanged(old, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public bool Networkdelivered
	{
		get
		{
			return delivered;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref delivered))
			{
				bool old = delivered;
				SetSyncVar(value, ref delivered, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, value: true);
					OnDelivered(old, value);
					setSyncVarHookGuard(2uL, value: false);
				}
			}
		}
	}

	public bool NetworkcanBePickedUp
	{
		get
		{
			return canBePickedUp;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref canBePickedUp))
			{
				bool flag = canBePickedUp;
				SetSyncVar(value, ref canBePickedUp, 4uL);
			}
		}
	}

	private void Start()
	{
		MyRigidBody = GetComponent<Rigidbody>();
		massAtInit = MyRigidBody.mass;
		myMeshRenderer = GetComponentInChildren<MeshRenderer>();
		myNetworkIdentity = GetComponent<NetworkIdentity>();
		myNetworkTransform = GetComponent<NetworkTransform>();
		base.gameObject.AddComponent<InteractableObject>().isPickUpAndCarry = this;
		carryItemLayer = LayerMask.GetMask("CarryItem");
		vehicleLayer = LayerMask.GetMask("VehicleForPlayers");
		if (prefabId >= 0 && !WorldManager.Instance.allCarriables.Contains(this))
		{
			WorldManager.Instance.allCarriables.Add(this);
		}
	}

	public override void OnStartServer()
	{
		SetDroppedOnMap();
		if (RealWorldTimeLight.time.underGround && RealWorldTimeLight.time.mineLevel == 2)
		{
			StartCoroutine(RunLavaCheck());
		}
	}

	public override void OnStartClient()
	{
		StartCoroutine(EnableDamageDelay());
	}

	private IEnumerator EnableDamageDelay()
	{
		if ((bool)damageCollider)
		{
			damageCollider.enabled = false;
			yield return new WaitForSeconds(0.5f);
			damageCollider.enabled = true;
		}
	}

	public override void OnStartAuthority()
	{
		if (ResetAfterPlayerWithAuthorityDisconnected)
		{
			TeleportToGround();
			ResetAfterPlayerWithAuthorityDisconnected = false;
		}
		StartCoroutine(AuthorityRoutine());
	}

	[Server]
	public void ServerChangeAuthority(NetworkConnectionToClient conn = null)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PickUpAndCarry::ServerChangeAuthority(Mirror.NetworkConnectionToClient)' called when server was not active");
		}
		else if (myNetworkIdentity.connectionToClient != conn)
		{
			if (myNetworkIdentity.connectionToClient != null)
			{
				myNetworkIdentity.RemoveClientAuthority();
			}
			myNetworkIdentity.AssignClientAuthority(conn);
		}
	}

	[Server]
	public void TurnOffKinematicForAuthority()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PickUpAndCarry::TurnOffKinematicForAuthority()' called when server was not active");
		}
		else
		{
			RpcTurnOffKinematicForAuthority();
		}
	}

	[ClientRpc]
	public void RpcTurnOffKinematicForAuthority()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(PickUpAndCarry), "RpcTurnOffKinematicForAuthority", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public void ChangeAuthority(NetworkConnection conn = null)
	{
		if (myNetworkIdentity.connectionToClient != conn)
		{
			if (NetworkServer.active)
			{
				NetworkMapSharer.Instance.ServerChangeAuthorityOfAllCarryObjectInProximity(base.transform.position);
			}
			if (myNetworkIdentity.connectionToClient != null)
			{
				myNetworkIdentity.RemoveClientAuthority();
			}
			myNetworkIdentity.AssignClientAuthority(conn);
		}
	}

	public void RemoveAuthorityBeforeBeforeServerDestroy()
	{
		ServerChangeAuthority((NetworkConnectionToClient)NetworkMapSharer.Instance.localChar.connectionToClient);
	}

	public void SetCarriedByPlayerNetId(uint networkId)
	{
		NetworkbeingCarriedBy = networkId;
		ChangeRigidbodyKinematic(isKinematic: true);
	}

	public void OnLoadCheckForVehicles()
	{
		StartCoroutine(CheckForVehicleStickAfterLoading());
	}

	public void MoveToNewDropPos(float newDropToPos)
	{
		dropToPosY = newDropToPos;
		base.transform.position = new Vector3(base.transform.position.x, newDropToPos, base.transform.position.z);
	}

	public void FallFromDestroyedVehicle()
	{
		pickUpAnimPlayed = false;
		NetworkbeingCarriedBy = 0u;
		TeleportToGround();
	}

	public void dropAndPlaceAtPos(float dropPos)
	{
		dropToPosY = dropPos;
		NetworkbeingCarriedBy = 0u;
	}

	public void DropAndPlaceAtDropPos(Vector3 dropSpotPos)
	{
		StartCoroutine(MoveToDropPos(dropSpotPos));
		NetworkbeingCarriedBy = 0u;
		dropToPosY = dropSpotPos.y;
	}

	public uint GetLastCarriedBy()
	{
		return lastCarriedBy;
	}

	public void OnDelivered(bool old, bool newDelivered)
	{
		Networkdelivered = newDelivered;
		if (delivered)
		{
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			GameObject[] array = objectsToEnableOnDelivered;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
			if ((bool)toTriggerFloating && !GetComponent<AnimalCarryBox>())
			{
				toTriggerFloating.SetTrigger("Floating");
			}
			SoundManager.Instance.playASoundAtPoint(balloonFillSound, base.transform.position);
			if (base.isServer)
			{
				ServerChangeAuthority((NetworkConnectionToClient)NetworkMapSharer.Instance.localChar.connectionToClient);
			}
		}
	}

	public bool IsDropOnCurrentLevel()
	{
		if (base.transform.position.y <= -12f)
		{
			return true;
		}
		if (droppedOnMap == DroppedItem.DroppedLocation.Underworld)
		{
			if (RealWorldTimeLight.time.underGround)
			{
				return true;
			}
			return false;
		}
		if (droppedOnMap == DroppedItem.DroppedLocation.OffIsland)
		{
			if (RealWorldTimeLight.time.offIsland)
			{
				return true;
			}
			return false;
		}
		if (droppedOnMap == DroppedItem.DroppedLocation.Overworld)
		{
			if (!RealWorldTimeLight.time.offIsland && !RealWorldTimeLight.time.underGround)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	private IEnumerator AuthorityRoutine()
	{
		while (base.hasAuthority)
		{
			yield return null;
			if (!base.hasAuthority || beingCarriedBy != 0 || MyRigidBody.isKinematic)
			{
				continue;
			}
			if (IsOutsidePlayerWithAuthorityProximity())
			{
				CmdSetKineticOutsidePlayerProximity();
				continue;
			}
			bool flag = false;
			if (Physics.Raycast(base.transform.position + Vector3.up * vehicleSphereCheckRadius, Vector3.down, out var hitInfo, vehicleRayCastLength, vehicleLayer))
			{
				CmdStickToVehicle(new Vector3(base.transform.position.x, hitInfo.point.y, base.transform.position.z), hitInfo.transform.InverseTransformPoint(hitInfo.point), hitInfo.transform.GetComponent<VehicleHitBox>().connectedTo.netId);
				flag = true;
			}
			else
			{
				for (int i = 0; i < otherVehicleCheckPos.Length; i++)
				{
					if (Physics.Raycast(otherVehicleCheckPos[i].position + Vector3.up * vehicleSphereCheckRadius, Vector3.down, out hitInfo, vehicleRayCastLength, vehicleLayer))
					{
						CmdStickToVehicle(new Vector3(base.transform.position.x, hitInfo.point.y, base.transform.position.z), hitInfo.transform.InverseTransformPoint(hitInfo.point), hitInfo.transform.GetComponent<VehicleHitBox>().connectedTo.netId);
						flag = true;
						break;
					}
				}
			}
			if (!flag && Mathf.Approximately(MyRigidBody.velocity.y, 0f) && Physics.CheckSphere(base.transform.position + Vector3.up * vehicleSphereCheckRadius, vehicleSphereCheckRadius, groundedLayer) && hasBeenThrown && isLandingValid)
			{
				CmdSetLanded();
				hasBeenThrown = false;
			}
		}
	}

	private void FixedUpdate()
	{
		if (delivered && base.hasAuthority)
		{
			HandleDelivered();
		}
		if (base.isServer && (bool)base.transform.parent && beingCarriedBy != 0 && base.transform.localPosition != vehicleLocalPos)
		{
			base.transform.localPosition = vehicleLocalPos;
		}
	}

	private void LateUpdate()
	{
		if ((bool)CarriedByHoldPos && pickUpAnimPlayed)
		{
			base.transform.position = CarriedByHoldPos.transform.position + Vector3.up - CarriedByHoldPos.transform.forward / 2f;
			base.transform.rotation = Quaternion.Euler(0f, CarriedByHoldPos.transform.eulerAngles.y, 0f);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		float magnitude = collision.relativeVelocity.magnitude;
		MyRigidBody.mass = massAtInit;
		if (hasBeenThrown)
		{
			isLandingValid = true;
		}
		if (isFragile && base.hasAuthority && magnitude > 13.5f && !HasDoneLocalDamage)
		{
			if ((bool)collision.transform.GetComponent<CharMovement>())
			{
				if (collision.transform.GetComponent<CharMovement>().netId != lastCarriedBy)
				{
					HasDoneLocalDamage = true;
					CmdDoDamage(5);
				}
			}
			else
			{
				HasDoneLocalDamage = true;
				CmdDoDamage(5);
			}
		}
		if (HasCarriableLayer() && beingCarriedBy == 0 && (bool)collision.gameObject.GetComponent<Vehicle>() && isFragile)
		{
			GetComponent<Damageable>().attackAndDoDamage(10, base.transform);
		}
	}

	private bool IsOutsidePlayerWithAuthorityProximity()
	{
		if (!base.hasAuthority)
		{
			return true;
		}
		return Vector3.Distance(NetworkMapSharer.Instance.localChar.transform.position, base.transform.position) >= 30f;
	}

	[Command]
	public void CmdDoDamage(int amount)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(amount);
		SendCommandInternal(typeof(PickUpAndCarry), "CmdDoDamage", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdSetKineticOutsidePlayerProximity()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(PickUpAndCarry), "CmdSetKineticOutsidePlayerProximity", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcSetKineticOutsidePlayerProximity()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(PickUpAndCarry), "RpcSetKineticOutsidePlayerProximity", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdStickToVehicle(Vector3 transformPos, Vector3 localTransformPos, uint vehicleId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(transformPos);
		writer.WriteVector3(localTransformPos);
		writer.WriteUInt(vehicleId);
		SendCommandInternal(typeof(PickUpAndCarry), "CmdStickToVehicle", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void CheckForStickOnLoad(Vector3 transformPos, Vector3 localTransformPos, uint vehicleId)
	{
		StickToVehicleOnServer(transformPos, localTransformPos);
		NetworkbeingCarriedBy = vehicleId;
	}

	private void StickToVehicleOnServer(Vector3 transformPos, Vector3 localTransformPos)
	{
		vehicleLocalPos = localTransformPos;
		ServerChangeAuthority((NetworkConnectionToClient)NetworkMapSharer.Instance.localChar.connectionToClient);
		base.transform.position = new Vector3(transformPos.x, transformPos.y, transformPos.z);
		ChangeRigidbodyKinematic(isKinematic: true);
		RpcSetLanded();
	}

	[Command]
	private void CmdSetLanded()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(PickUpAndCarry), "CmdSetLanded", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcSetLanded()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(PickUpAndCarry), "RpcSetLanded", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdResetPosition()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(PickUpAndCarry), "CmdResetPosition", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private void HandleDelivered()
	{
		ChangeRigidbodyKinematic(isKinematic: true);
		MyRigidBody.MovePosition(base.transform.position + Vector3.up * floatSpeed * Time.fixedDeltaTime);
		floatSpeed = Mathf.Lerp(floatSpeed, 2.5f, Time.deltaTime);
		if (base.transform.position.y > 100f)
		{
			NetworkServer.Destroy(base.gameObject);
		}
	}

	private void SetCollisionLayer(int layer)
	{
		if (!HasCarriableLayer())
		{
			return;
		}
		base.gameObject.layer = layer;
		foreach (Transform item in base.transform)
		{
			if (item.gameObject.layer != 15 && item.gameObject.layer != 21)
			{
				item.gameObject.layer = layer;
			}
		}
	}

	private bool HasCarriableLayer()
	{
		if (base.gameObject.layer != 7)
		{
			return base.gameObject.layer == 6;
		}
		return true;
	}

	private void TeleportToGround()
	{
		Vector3 destination = new Vector3(base.transform.position.x, WorldManager.Instance.heightMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)], base.transform.position.z);
		GetComponent<ProximityChecker>().AddPlayerObservingForOneFrame(NetworkMapSharer.Instance.localChar.connectionToClient);
		myNetworkTransform.CmdTeleport(destination);
	}

	private void OnCarriedChanged(uint old, uint newCarriedBy)
	{
		lastCarriedBy = old;
		NetworkbeingCarriedBy = newCarriedBy;
		base.transform.parent = null;
		if (newCarriedBy == 0 && (bool)CarriedByHoldPos)
		{
			SetCollisionLayer(itemThrowLayerIndex);
			NetworkbeingCarriedBy = 0u;
			ChangeRigidbodyKinematic(isKinematic: false);
			pickUpAnimPlayed = false;
			hasBeenThrown = true;
			isLandingValid = false;
			if (base.hasAuthority)
			{
				AddThrowForce();
			}
			else
			{
				pickUpAnimPlayed = false;
			}
		}
		if (newCarriedBy != 0)
		{
			myNetworkTransform.ResetObjectsInterpolation();
			SoundManager.Instance.playASoundAtPoint(pickUpSound, base.transform.position);
			EquipItemToChar component = NetworkIdentity.spawned[newCarriedBy].gameObject.GetComponent<EquipItemToChar>();
			if (base.isServer)
			{
				GetComponent<ProximityChecker>().SetAlwaysVisible(shouldAlwaysBeVisible: true);
			}
			if ((bool)component)
			{
				base.transform.parent = null;
				if ((bool)CarriedByHoldPos)
				{
					CarriedByHoldPos.StopTrackingDistanceForThrow();
				}
				CarriedByHoldPos = component.holdPos.GetComponent<HoldPos>();
				if ((bool)CarriedByHoldPos)
				{
					CarriedByHoldPos.StartTrackingDistanceForThrow();
				}
				ChangeRigidbodyKinematic(isKinematic: true);
				if (base.isServer)
				{
					NetworkMapSharer.Instance.ServerChangeAuthorityOfAllCarryObjectInProximity(base.transform.position);
					StartCoroutine(PickUpDelay());
				}
				else
				{
					StartCoroutine(PickUpDelayClient());
				}
				droppedOnMap = DroppedItem.DroppedLocation.Overworld;
			}
			else
			{
				base.transform.parent = NetworkIdentity.spawned[newCarriedBy].GetComponent<Vehicle>().myHitBox;
				myNetworkTransform.ResetObjectsInterpolation();
			}
		}
		else
		{
			SoundManager.Instance.playASoundAtPoint(putDownSound, base.transform.position);
			if ((bool)CarriedByHoldPos)
			{
				CarriedByHoldPos.StopTrackingDistanceForThrow();
				CarriedByHoldPos = null;
			}
			if (base.isServer)
			{
				GetComponent<ProximityChecker>().SetAlwaysVisible(shouldAlwaysBeVisible: false);
			}
		}
		if (beingCarriedBy == 0)
		{
			SetDroppedOnMap();
		}
		StartCoroutine(CheckForSplashTimer());
	}

	private void SetDroppedOnMap()
	{
		if (base.transform.position.y <= -12f)
		{
			droppedOnMap = DroppedItem.DroppedLocation.Overworld;
		}
		else if (RealWorldTimeLight.time.underGround)
		{
			droppedOnMap = DroppedItem.DroppedLocation.Underworld;
		}
		else if (RealWorldTimeLight.time.offIsland)
		{
			droppedOnMap = DroppedItem.DroppedLocation.OffIsland;
		}
		else
		{
			droppedOnMap = DroppedItem.DroppedLocation.Overworld;
		}
	}

	private IEnumerator CheckForVehicleStickAfterLoading()
	{
		yield return new WaitForSeconds(1.5f);
		CheckForVehicleStickOnLoad();
	}

	private void CheckForVehicleStickOnLoad()
	{
		if (Physics.CheckSphere(base.transform.position + Vector3.up * vehicleSphereCheckRadius / 2f, vehicleSphereCheckRadius, vehicleLayer) && Physics.Raycast(base.transform.position + Vector3.up * vehicleSphereCheckRadius / 2f, Vector3.down, out var hitInfo, vehicleRayCastLength * 2f, vehicleLayer))
		{
			CheckForStickOnLoad(new Vector3(base.transform.position.x, hitInfo.point.y, base.transform.position.z), hitInfo.transform.InverseTransformPoint(hitInfo.point), hitInfo.transform.GetComponent<VehicleHitBox>().connectedTo.netId);
			return;
		}
		for (int i = 0; i < otherVehicleCheckPos.Length; i++)
		{
			if (Physics.CheckSphere(otherVehicleCheckPos[i].position + Vector3.up * vehicleSphereCheckRadius / 2f, vehicleSphereCheckRadius, vehicleLayer) && Physics.Raycast(otherVehicleCheckPos[i].position + Vector3.up * vehicleSphereCheckRadius / 2f, Vector3.down, out var hitInfo2, vehicleRayCastLength * 2f, vehicleLayer))
			{
				CheckForStickOnLoad(new Vector3(base.transform.position.x, hitInfo2.point.y, base.transform.position.z), hitInfo2.transform.InverseTransformPoint(hitInfo2.point), hitInfo2.transform.GetComponent<VehicleHitBox>().connectedTo.netId);
				break;
			}
		}
	}

	private IEnumerator MoveToDropPos(Vector3 dropPos)
	{
		float throwTimer = 0f;
		bool damageWhenDropped = false;
		if (isFragile && dropPos.y <= base.transform.position.y && Mathf.Abs(base.transform.position.y - dropPos.y) >= 4f)
		{
			damageWhenDropped = true;
		}
		while (beingCarriedBy == 0 && !delivered)
		{
			Vector3 eulerAngles = base.transform.eulerAngles;
			throwTimer = Mathf.Clamp(throwTimer + Time.deltaTime, 0f, 0.25f);
			base.transform.position = Vector3.Lerp(base.transform.position, dropPos, throwTimer * 4f);
			eulerAngles.y = Mathf.Round(eulerAngles.y / 90f) * 90f;
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(eulerAngles), Time.deltaTime * 2f);
			if (base.isServer && damageWhenDropped && Vector3.Distance(base.transform.position, dropPos) < 0.25f && Mathf.Abs(base.transform.position.y - dropToPosY) <= 0.1f)
			{
				GetComponent<Damageable>().attackAndDoDamage(10, base.transform);
			}
			yield return null;
		}
	}

	private IEnumerator rotateToClosest90()
	{
		Quaternion desiredRot = Quaternion.Euler(0f, Mathf.Round(base.transform.rotation.y / 90f) * 90f, 0f);
		while (Quaternion.Angle(base.transform.rotation, desiredRot) > 5f)
		{
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, desiredRot, Time.deltaTime * 2f);
			yield return null;
		}
	}

	private void AddThrowForce()
	{
		float num = CarriedByHoldPos.GetMagitudeForCharacterMovement() * 10f;
		float num2 = 0f;
		if (!CarriedByHoldPos.parentChar.InJump)
		{
			num2 = 0.7f * num;
		}
		float num3 = 3.5f;
		if (CarriedByHoldPos.parentChar.InJump)
		{
			num3 += 3f;
		}
		Vector3 vector = CarriedByHoldPos.transform.forward * 5f + CarriedByHoldPos.transform.forward * num;
		Vector3 vector2 = base.transform.up * num3 + base.transform.up * num2;
		MyRigidBody.mass = 1f;
		MyRigidBody.AddForce(vector + vector2, ForceMode.Impulse);
	}

	private IEnumerator PickUpDelay()
	{
		float throwTimer = 0f;
		while (beingCarriedBy == 0 && throwTimer < 0.25f)
		{
			throwTimer += Time.deltaTime * 2f;
			Vector3 b = CarriedByHoldPos.transform.position + Vector3.up - CarriedByHoldPos.transform.forward / 2f;
			base.transform.position = Vector3.Lerp(base.transform.position, b, throwTimer * 4f);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(0f, CarriedByHoldPos.transform.root.eulerAngles.y, 0f), throwTimer * 4f);
			yield return null;
		}
		pickUpAnimPlayed = true;
	}

	private IEnumerator PickUpDelayClient()
	{
		uint holding = beingCarriedBy;
		float throwTimer = 0f;
		while (holding == beingCarriedBy && throwTimer < 0.25f)
		{
			throwTimer += Time.deltaTime * 2f;
			yield return null;
		}
		if (holding == beingCarriedBy)
		{
			pickUpAnimPlayed = true;
		}
	}

	private void OnDestroy()
	{
		if (prefabId >= 0 && !(WorldManager.Instance == null))
		{
			WorldManager.Instance.allCarriables.Remove(this);
		}
	}

	public void ChangeRigidbodyKinematic(bool isKinematic)
	{
		if (base.hasAuthority)
		{
			if (!investigationItem)
			{
				MyRigidBody.isKinematic = isKinematic;
			}
			else
			{
				MyRigidBody.isKinematic = true;
			}
		}
	}

	public override void OnStopAuthority()
	{
		MyRigidBody.isKinematic = true;
		myNetworkTransform.ResetObjectsInterpolation();
		base.OnStopAuthority();
	}

	private IEnumerator CheckForSplashTimer()
	{
		float timer = 0f;
		if (IsUnderWater())
		{
			yield break;
		}
		while (timer <= 10f)
		{
			timer += Time.deltaTime;
			yield return null;
			if (IsUnderWater())
			{
				ParticleManager.manage.bigSplash(base.transform);
				SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.waterSplash, base.transform.position);
				break;
			}
		}
	}

	private bool IsUnderWater()
	{
		if (WorldManager.Instance.isPositionOnMap(base.transform.position) && WorldManager.Instance.waterMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)] && base.transform.position.y <= 0.6f)
		{
			return true;
		}
		return false;
	}

	public IEnumerator RunLavaCheck()
	{
		WaitForSeconds myWait = new WaitForSeconds(Random.Range(0.1f, 0.155f));
		Damageable myDamage = GetComponent<Damageable>();
		while (RealWorldTimeLight.time.underGround && RealWorldTimeLight.time.mineLevel == 2)
		{
			yield return myWait;
			if (WorldManager.Instance.isPositionOnMap(base.transform.position))
			{
				int num = Mathf.RoundToInt(base.transform.position.x / 2f);
				int num2 = Mathf.RoundToInt(base.transform.position.z / 2f);
				if (WorldManager.Instance.onTileMap[num, num2] == 881 && base.transform.position.y <= (float)WorldManager.Instance.heightMap[num, num2] + 0.6f && !myDamage.onFire)
				{
					myDamage.NetworkonFire = true;
					myDamage.attackAndDoDamage(10, base.transform);
				}
			}
		}
	}

	public string GetName()
	{
		return ConversationGenerator.generate.GetCarriableName(prefabId);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcTurnOffKinematicForAuthority()
	{
		if (base.hasAuthority)
		{
			hasBeenThrown = true;
			isLandingValid = false;
			ChangeRigidbodyKinematic(isKinematic: false);
		}
	}

	protected static void InvokeUserCode_RpcTurnOffKinematicForAuthority(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcTurnOffKinematicForAuthority called on server.");
		}
		else
		{
			((PickUpAndCarry)obj).UserCode_RpcTurnOffKinematicForAuthority();
		}
	}

	protected void UserCode_CmdDoDamage(int amount)
	{
		GetComponent<Damageable>().attackAndDoDamage(amount, base.transform);
	}

	protected static void InvokeUserCode_CmdDoDamage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDoDamage called on client.");
		}
		else
		{
			((PickUpAndCarry)obj).UserCode_CmdDoDamage(reader.ReadInt());
		}
	}

	protected void UserCode_CmdSetKineticOutsidePlayerProximity()
	{
		RpcSetKineticOutsidePlayerProximity();
	}

	protected static void InvokeUserCode_CmdSetKineticOutsidePlayerProximity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetKineticOutsidePlayerProximity called on client.");
		}
		else
		{
			((PickUpAndCarry)obj).UserCode_CmdSetKineticOutsidePlayerProximity();
		}
	}

	protected void UserCode_RpcSetKineticOutsidePlayerProximity()
	{
		ChangeRigidbodyKinematic(isKinematic: true);
		hasBeenThrown = false;
		SetCollisionLayer(carryItemLayerIndex);
	}

	protected static void InvokeUserCode_RpcSetKineticOutsidePlayerProximity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetKineticOutsidePlayerProximity called on server.");
		}
		else
		{
			((PickUpAndCarry)obj).UserCode_RpcSetKineticOutsidePlayerProximity();
		}
	}

	protected void UserCode_CmdStickToVehicle(Vector3 transformPos, Vector3 localTransformPos, uint vehicleId)
	{
		StickToVehicleOnServer(transformPos, localTransformPos);
		NetworkbeingCarriedBy = vehicleId;
	}

	protected static void InvokeUserCode_CmdStickToVehicle(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStickToVehicle called on client.");
		}
		else
		{
			((PickUpAndCarry)obj).UserCode_CmdStickToVehicle(reader.ReadVector3(), reader.ReadVector3(), reader.ReadUInt());
		}
	}

	protected void UserCode_CmdSetLanded()
	{
		RpcSetLanded();
	}

	protected static void InvokeUserCode_CmdSetLanded(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetLanded called on client.");
		}
		else
		{
			((PickUpAndCarry)obj).UserCode_CmdSetLanded();
		}
	}

	protected void UserCode_RpcSetLanded()
	{
		SetCollisionLayer(carryItemLayerIndex);
		hasBeenThrown = false;
		ChangeRigidbodyKinematic(isKinematic: true);
	}

	protected static void InvokeUserCode_RpcSetLanded(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetLanded called on server.");
		}
		else
		{
			((PickUpAndCarry)obj).UserCode_RpcSetLanded();
		}
	}

	protected void UserCode_CmdResetPosition()
	{
		myNetworkTransform.RpcTeleport(base.transform.position);
	}

	protected static void InvokeUserCode_CmdResetPosition(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdResetPosition called on client.");
		}
		else
		{
			((PickUpAndCarry)obj).UserCode_CmdResetPosition();
		}
	}

	static PickUpAndCarry()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(PickUpAndCarry), "CmdDoDamage", InvokeUserCode_CmdDoDamage, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(PickUpAndCarry), "CmdSetKineticOutsidePlayerProximity", InvokeUserCode_CmdSetKineticOutsidePlayerProximity, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(PickUpAndCarry), "CmdStickToVehicle", InvokeUserCode_CmdStickToVehicle, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(PickUpAndCarry), "CmdSetLanded", InvokeUserCode_CmdSetLanded, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(PickUpAndCarry), "CmdResetPosition", InvokeUserCode_CmdResetPosition, requiresAuthority: true);
		RemoteCallHelper.RegisterRpcDelegate(typeof(PickUpAndCarry), "RpcTurnOffKinematicForAuthority", InvokeUserCode_RpcTurnOffKinematicForAuthority);
		RemoteCallHelper.RegisterRpcDelegate(typeof(PickUpAndCarry), "RpcSetKineticOutsidePlayerProximity", InvokeUserCode_RpcSetKineticOutsidePlayerProximity);
		RemoteCallHelper.RegisterRpcDelegate(typeof(PickUpAndCarry), "RpcSetLanded", InvokeUserCode_RpcSetLanded);
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteUInt(beingCarriedBy);
			writer.WriteBool(delivered);
			writer.WriteBool(canBePickedUp);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteUInt(beingCarriedBy);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteBool(delivered);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteBool(canBePickedUp);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			uint num = beingCarriedBy;
			NetworkbeingCarriedBy = reader.ReadUInt();
			if (!SyncVarEqual(num, ref beingCarriedBy))
			{
				OnCarriedChanged(num, beingCarriedBy);
			}
			bool flag = delivered;
			Networkdelivered = reader.ReadBool();
			if (!SyncVarEqual(flag, ref delivered))
			{
				OnDelivered(flag, delivered);
			}
			bool flag2 = canBePickedUp;
			NetworkcanBePickedUp = reader.ReadBool();
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			uint num3 = beingCarriedBy;
			NetworkbeingCarriedBy = reader.ReadUInt();
			if (!SyncVarEqual(num3, ref beingCarriedBy))
			{
				OnCarriedChanged(num3, beingCarriedBy);
			}
		}
		if ((num2 & 2L) != 0L)
		{
			bool flag3 = delivered;
			Networkdelivered = reader.ReadBool();
			if (!SyncVarEqual(flag3, ref delivered))
			{
				OnDelivered(flag3, delivered);
			}
		}
		if ((num2 & 4L) != 0L)
		{
			bool flag4 = canBePickedUp;
			NetworkcanBePickedUp = reader.ReadBool();
		}
	}
}
