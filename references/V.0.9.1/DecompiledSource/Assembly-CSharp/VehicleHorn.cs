using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class VehicleHorn : NetworkBehaviour
{
	public float hornVolume = 1f;

	public AudioSource hornSource;

	public Vehicle myVehicle;

	[SyncVar(hook = "hornChange")]
	public bool hornOn;

	public bool localHorn;

	public bool NetworkhornOn
	{
		get
		{
			return hornOn;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref hornOn))
			{
				bool oldHorn = hornOn;
				SetSyncVar(value, ref hornOn, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					hornChange(oldHorn, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	private void Update()
	{
		if (base.hasAuthority && myVehicle.hasDriver())
		{
			if (InputMaster.input.WhistleHeld())
			{
				if (!localHorn)
				{
					localHorn = true;
					CmdSetHornOn(hornIsOn: true);
				}
			}
			else if (localHorn)
			{
				localHorn = false;
				CmdSetHornOn(hornIsOn: false);
			}
		}
		else if (base.hasAuthority && localHorn)
		{
			CmdSetHornOn(hornIsOn: false);
		}
		if (!hornOn && hornSource.isPlaying)
		{
			stopHorn();
		}
	}

	public void hornChange(bool oldHorn, bool newHorn)
	{
		NetworkhornOn = newHorn;
		if (hornOn)
		{
			hornSource.volume = hornVolume * SoundManager.Instance.GetGlobalSoundVolume();
			hornSource.Stop();
			hornSource.Play();
		}
	}

	public override void OnStartAuthority()
	{
		NetworkhornOn = false;
		localHorn = false;
	}

	public override void OnStopAuthority()
	{
		NetworkhornOn = false;
		localHorn = false;
	}

	public void stopHorn()
	{
		hornSource.volume = Mathf.Clamp(hornSource.volume - Time.deltaTime * 4f, 0f, 7f);
		if (hornSource.volume == 0f)
		{
			hornSource.Stop();
		}
	}

	[Command]
	private void CmdSetHornOn(bool hornIsOn)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(hornIsOn);
		SendCommandInternal(typeof(VehicleHorn), "CmdSetHornOn", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdSetHornOn(bool hornIsOn)
	{
		NetworkhornOn = hornIsOn;
	}

	protected static void InvokeUserCode_CmdSetHornOn(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetHornOn called on client.");
		}
		else
		{
			((VehicleHorn)obj).UserCode_CmdSetHornOn(reader.ReadBool());
		}
	}

	static VehicleHorn()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(VehicleHorn), "CmdSetHornOn", InvokeUserCode_CmdSetHornOn, requiresAuthority: true);
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(hornOn);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(hornOn);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			bool flag = hornOn;
			NetworkhornOn = reader.ReadBool();
			if (!SyncVarEqual(flag, ref hornOn))
			{
				hornChange(flag, hornOn);
			}
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			bool flag2 = hornOn;
			NetworkhornOn = reader.ReadBool();
			if (!SyncVarEqual(flag2, ref hornOn))
			{
				hornChange(flag2, hornOn);
			}
		}
	}
}
