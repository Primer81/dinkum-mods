using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class ControlVehicle : NetworkBehaviour
{
	private Vehicle myVehicle;

	public Transform[] groundedCheckPos;

	public Transform collisionCheckPos;

	private bool grounded = true;

	[Header("Speed details -----------")]
	private float speed;

	public float acceleration = 1f;

	public float maxSpeed = 8f;

	public float turnSpeed = 10f;

	private float flyingSpeed;

	[Header("Slow down on mask")]
	public float slowDownSpeed = 1f;

	public float slowDownDividedBy = 10f;

	public bool useSlowDownMask;

	public bool instantSlowSpeedOnEnter;

	public LayerMask slowDownMask;

	public Transform[] slowDownGroundedCheck;

	private bool isOnSlowDownLayer;

	[Header("Other Options -----------")]
	public bool canJump = true;

	public bool canFly;

	public bool alwaysMovesForward;

	public bool canReverse = true;

	public bool onlyTurnIfMovingForward;

	public bool turningReversedWhenBackwards;

	public float reverseSpeedClamp0to1 = -0.25f;

	public LayerMask canJumpLayer;

	private Vector3 forwardDir;

	public Transform wallChecker;

	public Transform[] wallCheckers;

	public Transform[] backWallCheckers;

	public LayerMask wallCheckerLayers;

	public bool damagedObjectsOnCollision;

	public LayerMask doesDamageLayer;

	[Header("Other Options -----------")]
	public bool hasBoost;

	private float boostAmount = 2f;

	private float boostRecovery = 0.25f;

	private float boostAcceleration;

	private float topBoostSpeed;

	[SyncVar]
	public bool currentlyBoosting;

	private bool delayGrounded;

	private Coroutine takeOffTimer;

	private float vel;

	private static WaitForFixedUpdate jumpWait;

	public float jumpUpHeight = 3f;

	public float fallSpeed = -1f;

	private bool inJump;

	public bool NetworkcurrentlyBoosting
	{
		get
		{
			return currentlyBoosting;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref currentlyBoosting))
			{
				bool flag = currentlyBoosting;
				SetSyncVar(value, ref currentlyBoosting, 1uL);
			}
		}
	}

	private void Start()
	{
		myVehicle = GetComponent<Vehicle>();
	}

	private void OnDestroy()
	{
		if ((object)WorldManager.Instance != null)
		{
			for (int i = 0; i < groundedCheckPos.Length; i++)
			{
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], groundedCheckPos[i].position);
			}
		}
	}

	private void checkUndergroundAndCorrect()
	{
		if (base.transform.position.y < -20f)
		{
			if (WorldManager.Instance.isPositionOnMap(base.transform.position))
			{
				base.transform.position = new Vector3(base.transform.position.x, WorldManager.Instance.heightMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)], base.transform.position.z);
			}
			else
			{
				base.transform.position = new Vector3(base.transform.position.x, 0.6f, base.transform.position.z);
			}
		}
	}

	public void setGrounded()
	{
		if (delayGrounded)
		{
			grounded = false;
		}
		else if (Vector3.Distance(base.transform.position, CameraController.control.transform.position) > (float)NetworkNavMesh.nav.animalDistance)
		{
			grounded = true;
		}
		else if (groundedCheckPos.Length != 0)
		{
			for (int i = 0; i < groundedCheckPos.Length; i++)
			{
				grounded = Physics.CheckSphere(groundedCheckPos[i].position, 0.2f, canJumpLayer);
				if (grounded)
				{
					break;
				}
			}
		}
		else
		{
			grounded = Physics.CheckSphere(base.transform.position, 0.2f, canJumpLayer);
		}
	}

	private IEnumerator CanGroundTimer()
	{
		delayGrounded = true;
		float timer = 0.5f;
		while (timer > 0f)
		{
			timer -= Time.deltaTime;
			yield return null;
		}
		delayGrounded = false;
		takeOffTimer = null;
	}

	private void Update()
	{
		if (!base.hasAuthority || !myVehicle.hasDriver() || !myVehicle.mountingAnimationComplete)
		{
			return;
		}
		if (canFly)
		{
			if (InputMaster.input.JumpHeld() || InputMaster.input.VehicleUseHeld())
			{
				if (grounded && takeOffTimer == null)
				{
					takeOffTimer = StartCoroutine(CanGroundTimer());
				}
				if (alwaysMovesForward)
				{
					if (speed >= maxSpeed / 3f)
					{
						flyingSpeed = Mathf.Lerp(flyingSpeed, 3f, Time.deltaTime);
					}
				}
				else
				{
					flyingSpeed = Mathf.Lerp(flyingSpeed, 6f, Time.deltaTime);
				}
			}
			else if (InputMaster.input.VehicleInteractHeld())
			{
				flyingSpeed = Mathf.Lerp(flyingSpeed, -6f, Time.deltaTime);
			}
			else
			{
				flyingSpeed = Mathf.Lerp(flyingSpeed, 0f, Time.deltaTime);
			}
			if (flyingSpeed < 0.05f && flyingSpeed > 0f)
			{
				flyingSpeed = 0f;
			}
			if (flyingSpeed > -0.05f && flyingSpeed < 0f)
			{
				flyingSpeed = 0f;
			}
		}
		else if (canJump && InputMaster.input.Jump() && !inJump && grounded)
		{
			inJump = true;
			StartCoroutine(jumpFeel());
		}
	}

	private void FixedUpdate()
	{
		if ((bool)myVehicle.damageWhenUnderWater && base.transform.position.y <= -1f)
		{
			int xPos = (int)base.transform.position.x / 2;
			int yPos = (int)base.transform.position.z / 2;
			if (!WorldManager.Instance.isPositionOnMap(xPos, yPos) || WorldManager.Instance.waterMap[(int)base.transform.position.x / 2, (int)base.transform.position.z / 2])
			{
				if (base.isServer)
				{
					myVehicle.damageWhenUnderWater.attackAndDoDamage(100, null, 0f);
				}
				ParticleManager.manage.bigSplash(base.transform, 5);
			}
		}
		setGrounded();
		if (useSlowDownMask && Vector3.Distance(base.transform.position, CameraController.control.transform.position) <= (float)NetworkNavMesh.nav.animalDistance)
		{
			bool flag = isOnSlowDownLayer;
			if (slowDownGroundedCheck.Length != 0)
			{
				for (int i = 0; i < slowDownGroundedCheck.Length; i++)
				{
					isOnSlowDownLayer = Physics.CheckSphere(slowDownGroundedCheck[i].position, 0.2f, slowDownMask);
					if (isOnSlowDownLayer)
					{
						break;
					}
				}
			}
			else
			{
				isOnSlowDownLayer = Physics.CheckSphere(base.transform.position, 0.2f, slowDownMask);
			}
			if (instantSlowSpeedOnEnter && !flag && isOnSlowDownLayer)
			{
				speed = Mathf.Clamp(speed, 0f, maxSpeed / slowDownDividedBy);
			}
		}
		if (!base.hasAuthority)
		{
			return;
		}
		checkUndergroundAndCorrect();
		float num = maxSpeed;
		float num2 = acceleration;
		if (myVehicle.hasDriver() && myVehicle.mountingAnimationComplete)
		{
			float num3 = 0f - InputMaster.input.getLeftStick().x;
			float value = InputMaster.input.VehicleAccelerate();
			if (hasBoost)
			{
				if (InputMaster.input.VehicleUseHeld() && boostAmount > 0f)
				{
					topBoostSpeed = Mathf.Clamp(topBoostSpeed + Time.deltaTime * 2f, 1f, 1.5f);
					boostAcceleration = Mathf.Clamp(boostAcceleration + Time.deltaTime * 5f, 1f, 3f);
					num = maxSpeed * topBoostSpeed;
					num2 = acceleration * boostAcceleration;
					value = 1f;
					boostAmount = Mathf.Clamp(boostAmount -= Time.deltaTime, 0f, 2f);
					boostRecovery = 0f;
					if (!currentlyBoosting)
					{
						CmdSetCurrentlyBoosting(isBoosting: true);
						NetworkcurrentlyBoosting = true;
					}
				}
				else if (InputMaster.input.VehicleUseHeld())
				{
					if (currentlyBoosting)
					{
						CmdSetCurrentlyBoosting(isBoosting: false);
						NetworkcurrentlyBoosting = false;
					}
					boostRecovery = 0f;
					topBoostSpeed = 1f;
					boostAcceleration = 1f;
				}
				else
				{
					topBoostSpeed = 1f;
					boostAcceleration = 1f;
					num = Mathf.Clamp(num - Time.deltaTime, 0f, maxSpeed);
					if (boostRecovery < 1f)
					{
						boostRecovery = Mathf.Clamp(boostRecovery + Time.deltaTime, 0f, 2f);
					}
					else
					{
						boostAmount = Mathf.Clamp(boostAmount + Time.deltaTime, 0f, 2f);
					}
					if (currentlyBoosting)
					{
						CmdSetCurrentlyBoosting(isBoosting: false);
						NetworkcurrentlyBoosting = false;
					}
				}
			}
			value = (canReverse ? Mathf.Clamp(value, reverseSpeedClamp0to1, 1f) : Mathf.Clamp01(value));
			_ = isOnSlowDownLayer;
			if (Physics.Raycast(collisionCheckPos.position, collisionCheckPos.forward, out var _, 0.25f, canJumpLayer))
			{
				value = Mathf.Clamp(value, -2f, 0f);
			}
			else if (canFly && !isGroundedOnSlowSurface() && !grounded && alwaysMovesForward)
			{
				value = Mathf.Clamp(value, 0.5f, 2f);
			}
			if ((grounded && !canFly) || (!grounded && canFly) || num > maxSpeed)
			{
				if (value < 0f && speed > 0.1f)
				{
					speed = Mathf.SmoothDamp(speed, value * num, ref vel, 2f / (num2 * 3f));
				}
				else
				{
					speed = Mathf.SmoothDamp(speed, value * num, ref vel, 2f / num2);
				}
				forwardDir = Vector3.Lerp(forwardDir, base.transform.forward, Time.deltaTime * 3f);
			}
			else if (grounded && canFly && alwaysMovesForward)
			{
				value = Mathf.Clamp(value, -0.55f, 0.55f);
				speed = Mathf.SmoothDamp(speed, value * num, ref vel, 2f / num2);
				forwardDir = Vector3.Lerp(forwardDir, base.transform.forward, Time.deltaTime * 3f);
			}
			else if (isOnSlowDownLayer)
			{
				forwardDir = Vector3.Lerp(forwardDir, base.transform.forward, Time.deltaTime);
				if (instantSlowSpeedOnEnter)
				{
					speed = Mathf.SmoothDamp(speed, value * num / slowDownDividedBy, ref vel, 2f / num2);
				}
				else
				{
					speed = Mathf.SmoothDamp(speed, value * num / slowDownDividedBy, ref vel, 2f / slowDownSpeed);
				}
			}
			else
			{
				speed = Mathf.SmoothDamp(speed, value * 0.25f, ref vel, 2f);
			}
			if ((bool)wallChecker)
			{
				checkForDamage();
			}
			if (!beingPushedUp())
			{
				if (speed > 0f && wallCheckerTouching())
				{
					if (speed >= num / 2f)
					{
						SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.vehicleKnockBack, base.transform.position);
					}
					speed = (0f - speed) / 2f;
				}
				else if (speed < 0f && wallCheckerBackTouching())
				{
					speed = (0f - speed) / 2f;
				}
			}
			if (turningReversedWhenBackwards && value < 0f && speed < -0.1f)
			{
				num3 *= -1f;
			}
			if (!grounded && isOnSlowDownLayer)
			{
				if (!onlyTurnIfMovingForward)
				{
					base.transform.Rotate(0f, (0f - num3) * (turnSpeed / 4f) * Time.deltaTime, 0f);
				}
				else
				{
					base.transform.Rotate(0f, (0f - num3) * (turnSpeed * (Mathf.Abs(speed) / maxSpeed) / 4f) * Time.deltaTime, 0f);
				}
			}
			else if (!onlyTurnIfMovingForward)
			{
				base.transform.Rotate(0f, (0f - num3) * turnSpeed * Time.deltaTime, 0f);
			}
			else
			{
				base.transform.Rotate(0f, (0f - num3) * (turnSpeed * (Mathf.Abs(speed) / maxSpeed)) * Time.deltaTime, 0f);
			}
			if (canFly)
			{
				if (grounded)
				{
					myVehicle.myRig.useGravity = true;
				}
				else
				{
					myVehicle.myRig.useGravity = false;
					myVehicle.myRig.velocity = Vector3.zero;
				}
				if (flyingSpeed > 0f)
				{
					if (myVehicle.transform.position.y < 28f)
					{
						myVehicle.myRig.MovePosition(myVehicle.myRig.position + Vector3.up * flyingSpeed * Time.fixedDeltaTime);
					}
				}
				else
				{
					myVehicle.myRig.MovePosition(myVehicle.myRig.position + Vector3.up * flyingSpeed * Time.fixedDeltaTime);
				}
			}
		}
		else
		{
			if (wallCheckerTouching() && !beingPushedUp() && speed > 0f)
			{
				if (speed >= maxSpeed / 2f)
				{
					SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.vehicleKnockBack, base.transform.position);
				}
				speed = (0f - speed) / 2f;
			}
			speed = Mathf.SmoothDamp(speed, 0f, ref vel, 1.5f / num2);
		}
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(base.transform.eulerAngles.x, base.transform.eulerAngles.y, 0f), Time.deltaTime * 50f);
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(tiltXAxis(), base.transform.eulerAngles.y, 0f), Time.deltaTime * 8f);
		Vector3 vector = speed * forwardDir;
		if (!myVehicle.myRig.isKinematic)
		{
			myVehicle.myRig.MovePosition(myVehicle.myRig.position + vector * Time.fixedDeltaTime);
		}
		if (beingPushedUp())
		{
			myVehicle.myRig.MovePosition(myVehicle.myRig.position + Vector3.up * 8f * Time.fixedDeltaTime);
		}
	}

	public bool wallCheckerTouching()
	{
		if ((bool)wallChecker)
		{
			Vector3 normalized = new Vector3(wallChecker.forward.x, 0f, wallChecker.forward.z).normalized;
			if (Physics.Raycast(wallChecker.position, normalized, out var hitInfo, 0.35f, wallCheckerLayers) && hitInfo.transform != myVehicle.myHitBox)
			{
				return true;
			}
		}
		if (wallCheckers.Length != 0)
		{
			for (int i = 0; i < wallCheckers.Length; i++)
			{
				Vector3 normalized2 = new Vector3(wallCheckers[i].forward.x, 0f, wallCheckers[i].forward.z).normalized;
				if (Physics.Raycast(wallCheckers[i].position, normalized2, out var hitInfo2, 0.35f, wallCheckerLayers) && hitInfo2.transform != myVehicle.myHitBox)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool wallCheckerBackTouching()
	{
		if (backWallCheckers.Length != 0)
		{
			for (int i = 0; i < backWallCheckers.Length; i++)
			{
				Vector3 normalized = new Vector3(backWallCheckers[i].forward.x, 0f, backWallCheckers[i].forward.z).normalized;
				if (Physics.Raycast(backWallCheckers[i].position, normalized, out var hitInfo, 0.35f, wallCheckerLayers) && hitInfo.transform != myVehicle.myHitBox)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void checkForDamage()
	{
		if (!damagedObjectsOnCollision || !Physics.Raycast(wallChecker.position, wallChecker.forward, out var hitInfo, 0.35f, doesDamageLayer))
		{
			return;
		}
		Damageable componentInParent = hitInfo.transform.GetComponentInParent<Damageable>();
		if ((bool)componentInParent)
		{
			if (speed >= maxSpeed / 2f + maxSpeed / 4f)
			{
				CmdDoDamageWhenHit(componentInParent.netId, 3);
			}
			else
			{
				CmdDoDamageWhenHit(componentInParent.netId, 0);
			}
		}
	}

	public bool beingPushedUp()
	{
		if (WorldManager.Instance.isPositionOnMap(Mathf.RoundToInt((base.transform.position.x + base.transform.forward.x * 2f) / 2f), Mathf.RoundToInt((base.transform.position.z + base.transform.forward.z * 2f) / 2f)))
		{
			if (WorldManager.Instance.onTileMap[Mathf.RoundToInt((base.transform.position.x + base.transform.forward.x * 2f) / 2f), Mathf.RoundToInt((base.transform.position.z + base.transform.forward.z * 2f) / 2f)] == 568)
			{
				return false;
			}
			float num = WorldManager.Instance.heightMap[Mathf.RoundToInt((base.transform.position.x + base.transform.forward.x * 2f) / 2f), Mathf.RoundToInt((base.transform.position.z + base.transform.forward.z * 2f) / 2f)];
			if (!inJump && speed >= maxSpeed / 8f && base.transform.position.y < num && num - base.transform.position.y > 0f && num - base.transform.position.y < 1f)
			{
				return true;
			}
		}
		return false;
	}

	private void rotateVehicleToDir(float x, float y, float turnSpeed)
	{
		if (x != 0f || y != 0f)
		{
			Vector3 normalized = new Vector3(x, 0f, y).normalized;
			normalized = CameraController.control.transform.TransformDirection(normalized);
			if (base.transform.parent != null)
			{
				normalized = CameraController.control.transform.InverseTransformDirection(normalized);
			}
			Quaternion b = Quaternion.LookRotation(normalized);
			base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, b, Time.deltaTime * turnSpeed);
		}
	}

	public bool isGrounded()
	{
		return grounded;
	}

	public bool isGroundedOnSlowSurface()
	{
		return isOnSlowDownLayer;
	}

	public float tiltXAxis()
	{
		int num = Mathf.RoundToInt(base.transform.position.x / 2f);
		int num2 = Mathf.RoundToInt(base.transform.position.z / 2f);
		if (!WorldManager.Instance.isPositionOnMap(num, num2) || WorldManager.Instance.onTileMap[num, num2] != 568)
		{
			return 0f;
		}
		if (Physics.Raycast(base.transform.position + Vector3.up, -base.transform.up, out var hitInfo, 4f, canJumpLayer))
		{
			Vector3 normal = hitInfo.normal;
			Quaternion quaternion = Quaternion.FromToRotation(base.transform.up, normal) * base.transform.rotation;
			if (!(quaternion.eulerAngles.x > 180f))
			{
				return quaternion.eulerAngles.x;
			}
			return quaternion.eulerAngles.x - 360f;
		}
		return 0f;
	}

	private IEnumerator jumpFeel()
	{
		float desiredHeight = 0f;
		float multi = 25f;
		while (desiredHeight < jumpUpHeight)
		{
			yield return jumpWait;
			myVehicle.myRig.MovePosition(myVehicle.myRig.position + Vector3.up * desiredHeight * Time.fixedDeltaTime);
			desiredHeight = Mathf.Lerp(desiredHeight, jumpUpHeight + 1f, Time.fixedDeltaTime * multi);
			multi = Mathf.Lerp(multi, 10f, Time.deltaTime * 25f);
		}
		while (desiredHeight > 0f && !grounded)
		{
			yield return jumpWait;
			myVehicle.myRig.MovePosition(myVehicle.myRig.position + Vector3.up * desiredHeight * Time.fixedDeltaTime);
			if (!currentlyBoosting)
			{
				desiredHeight = Mathf.Lerp(desiredHeight, -1f, Time.deltaTime * 2f);
			}
		}
		while (!grounded)
		{
			yield return null;
		}
		inJump = false;
	}

	[Command]
	private void CmdDoDamageWhenHit(uint netId, int damageAmount)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(netId);
		writer.WriteInt(damageAmount);
		SendCommandInternal(typeof(ControlVehicle), "CmdDoDamageWhenHit", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdSetCurrentlyBoosting(bool isBoosting)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(isBoosting);
		SendCommandInternal(typeof(ControlVehicle), "CmdSetCurrentlyBoosting", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	static ControlVehicle()
	{
		jumpWait = new WaitForFixedUpdate();
		RemoteCallHelper.RegisterCommandDelegate(typeof(ControlVehicle), "CmdDoDamageWhenHit", InvokeUserCode_CmdDoDamageWhenHit, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(ControlVehicle), "CmdSetCurrentlyBoosting", InvokeUserCode_CmdSetCurrentlyBoosting, requiresAuthority: true);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdDoDamageWhenHit(uint netId, int damageAmount)
	{
		NetworkIdentity.spawned[netId].GetComponent<Damageable>().attackAndDoDamage(damageAmount, base.transform, 8f);
	}

	protected static void InvokeUserCode_CmdDoDamageWhenHit(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDoDamageWhenHit called on client.");
		}
		else
		{
			((ControlVehicle)obj).UserCode_CmdDoDamageWhenHit(reader.ReadUInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdSetCurrentlyBoosting(bool isBoosting)
	{
		NetworkcurrentlyBoosting = isBoosting;
	}

	protected static void InvokeUserCode_CmdSetCurrentlyBoosting(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetCurrentlyBoosting called on client.");
		}
		else
		{
			((ControlVehicle)obj).UserCode_CmdSetCurrentlyBoosting(reader.ReadBool());
		}
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(currentlyBoosting);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(currentlyBoosting);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			bool flag = currentlyBoosting;
			NetworkcurrentlyBoosting = reader.ReadBool();
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			bool flag2 = currentlyBoosting;
			NetworkcurrentlyBoosting = reader.ReadBool();
		}
	}
}
