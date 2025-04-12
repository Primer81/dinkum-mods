using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class CharNetworkOptions : NetworkBehaviour
{
	[SyncVar(hook = "onChangePvPOn")]
	public bool pvpOn;

	public bool NetworkpvpOn
	{
		get
		{
			return pvpOn;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref pvpOn))
			{
				bool oldOn = pvpOn;
				SetSyncVar(value, ref pvpOn, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					onChangePvPOn(oldOn, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public override void OnStartClient()
	{
		StartCoroutine(waitAFrame());
	}

	private IEnumerator waitAFrame()
	{
		yield return null;
		yield return null;
		onChangePvPOn(pvpOn, pvpOn);
	}

	public void onChangePvPOn(bool oldOn, bool newOn)
	{
		NetworkpvpOn = newOn;
		GetComponent<Damageable>().isFriendly = !pvpOn;
	}

	[Command]
	public void CmdSwapPvpOn()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharNetworkOptions), "CmdSwapPvpOn", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdSwapPvpOn()
	{
		NetworkpvpOn = !pvpOn;
		if (!pvpOn)
		{
			NetworkMapSharer.Instance.RpcMakeChatBubble("<color=red>" + ConversationGenerator.generate.GetToolTip("Tip_TurnPVPOff"), base.netId);
		}
		else
		{
			NetworkMapSharer.Instance.RpcMakeChatBubble("<color=red>" + ConversationGenerator.generate.GetToolTip("Tip_TurnPVPOn"), base.netId);
		}
	}

	protected static void InvokeUserCode_CmdSwapPvpOn(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSwapPvpOn called on client.");
		}
		else
		{
			((CharNetworkOptions)obj).UserCode_CmdSwapPvpOn();
		}
	}

	static CharNetworkOptions()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharNetworkOptions), "CmdSwapPvpOn", InvokeUserCode_CmdSwapPvpOn, requiresAuthority: true);
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(pvpOn);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(pvpOn);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			bool flag = pvpOn;
			NetworkpvpOn = reader.ReadBool();
			if (!SyncVarEqual(flag, ref pvpOn))
			{
				onChangePvPOn(flag, pvpOn);
			}
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			bool flag2 = pvpOn;
			NetworkpvpOn = reader.ReadBool();
			if (!SyncVarEqual(flag2, ref pvpOn))
			{
				onChangePvPOn(flag2, pvpOn);
			}
		}
	}
}
