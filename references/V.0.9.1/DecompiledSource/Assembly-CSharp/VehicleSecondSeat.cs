using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class VehicleSecondSeat : NetworkBehaviour
{
	[SyncVar(hook = "OnPassengerIdChange")]
	public uint passengerId;

	public Collider sittingPos;

	public Transform passengerTrans;

	private bool mountingAnimationComplete;

	private Vehicle myVehicle;

	public uint NetworkpassengerId
	{
		get
		{
			return passengerId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref passengerId))
			{
				uint oldId = passengerId;
				SetSyncVar(value, ref passengerId, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					OnPassengerIdChange(oldId, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	private void Start()
	{
		myVehicle = GetComponent<Vehicle>();
	}

	public void OnPassengerIdChange(uint oldId, uint newId)
	{
		NetworkpassengerId = newId;
		if (newId == 0)
		{
			if ((bool)passengerTrans && myVehicle.driverSittingAnimationBoolName != "")
			{
				passengerTrans.GetComponent<Animator>().SetBool(myVehicle.driverSittingAnimationBoolName, value: false);
			}
			passengerTrans = null;
			mountingAnimationComplete = false;
		}
		else if (NetworkIdentity.spawned.ContainsKey(passengerId))
		{
			passengerTrans = NetworkIdentity.spawned[passengerId].GetComponent<Transform>();
			if ((bool)passengerTrans)
			{
				StartCoroutine(moveDriverToSeat(passengerTrans, passengerTrans.GetComponent<Animator>()));
			}
		}
		if ((bool)passengerTrans)
		{
			sittingPos.enabled = false;
		}
		else
		{
			sittingPos.enabled = true;
		}
	}

	public void LateUpdate()
	{
		if ((bool)passengerTrans && mountingAnimationComplete)
		{
			passengerTrans.position = sittingPos.transform.position;
			passengerTrans.rotation = sittingPos.transform.rotation;
		}
	}

	private IEnumerator moveDriverToSeat(Transform movePassengerTransform, Animator driverAnim)
	{
		mountingAnimationComplete = false;
		uint newPassengerId = passengerId;
		float timer = 0f;
		Vector3 originalPos = movePassengerTransform.position;
		if (myVehicle.driverSittingAnimationBoolName != "")
		{
			driverAnim.SetTrigger("StartDriving");
		}
		while (newPassengerId == passengerId && timer < 0.25f)
		{
			timer += Time.deltaTime;
			movePassengerTransform.position = Vector3.Lerp(originalPos, passengerTrans.position, timer / 0.25f);
			passengerTrans.rotation = Quaternion.Lerp(passengerTrans.rotation, passengerTrans.rotation, timer / 0.25f);
			yield return null;
			if (myVehicle.driverSittingAnimationBoolName != "")
			{
				driverAnim.SetBool(myVehicle.driverSittingAnimationBoolName, value: true);
			}
		}
		mountingAnimationComplete = true;
	}

	public override void OnStopClient()
	{
		if (base.isServer && passengerId != 0)
		{
			NetworkIdentity.spawned[passengerId].GetComponent<CharPickUp>().RpcStopDrivingFromServer();
		}
		if ((bool)passengerTrans && myVehicle.driverSittingAnimationBoolName != "")
		{
			passengerTrans.GetComponent<Animator>().SetBool(myVehicle.driverSittingAnimationBoolName, value: false);
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
			writer.WriteUInt(passengerId);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteUInt(passengerId);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			uint num = passengerId;
			NetworkpassengerId = reader.ReadUInt();
			if (!SyncVarEqual(num, ref passengerId))
			{
				OnPassengerIdChange(num, passengerId);
			}
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			uint num3 = passengerId;
			NetworkpassengerId = reader.ReadUInt();
			if (!SyncVarEqual(num3, ref passengerId))
			{
				OnPassengerIdChange(num3, passengerId);
			}
		}
	}
}
