using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class ProjectileSpawn : NetworkBehaviour
{
	private Vector3 flyingDirection;

	private float flyingSpeed;

	private Rigidbody myRig;

	public LayerMask collisionLayers;

	[SyncVar]
	private Vector3 syncDirection;

	[SyncVar]
	private float syncSpeed;

	[SyncVar(hook = "OnChangeInvId")]
	private int dropId;

	public InventoryItem[] dropOrder;

	public Mesh[] dropMeshes;

	public MeshFilter myMeshFilter;

	public Vector3 NetworksyncDirection
	{
		get
		{
			return syncDirection;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref syncDirection))
			{
				Vector3 vector = syncDirection;
				SetSyncVar(value, ref syncDirection, 1uL);
			}
		}
	}

	public float NetworksyncSpeed
	{
		get
		{
			return syncSpeed;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref syncSpeed))
			{
				float num = syncSpeed;
				SetSyncVar(value, ref syncSpeed, 2uL);
			}
		}
	}

	public int NetworkdropId
	{
		get
		{
			return dropId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref dropId))
			{
				int oldId = dropId;
				SetSyncVar(value, ref dropId, 4uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(4uL))
				{
					setSyncVarHookGuard(4uL, value: true);
					OnChangeInvId(oldId, value);
					setSyncVarHookGuard(4uL, value: false);
				}
			}
		}
	}

	public override void OnStartClient()
	{
		OnChangeInvId(dropId, dropId);
		if (base.hasAuthority)
		{
			SetDirectionAndSpeed(syncDirection, syncSpeed);
		}
	}

	public void SyncDirectionAndSpeed(Vector3 direction, float speed)
	{
		NetworksyncDirection = direction;
		NetworksyncSpeed = speed;
	}

	public void SetInvItem(int newInvItem)
	{
		NetworkdropId = newInvItem;
	}

	private void SetDirectionAndSpeed(Vector3 direction, float speed)
	{
		myRig = GetComponent<Rigidbody>();
		myRig.isKinematic = false;
		flyingDirection = direction;
		flyingSpeed = speed;
		myRig.AddForce(flyingDirection * flyingSpeed + Vector3.up, ForceMode.Impulse);
		StartCoroutine(FlyingAndLanding());
	}

	private IEnumerator FlyingAndLanding()
	{
		float timer = 0f;
		while (timer < 2f)
		{
			timer += Time.deltaTime;
			yield return null;
		}
		bool flag = Physics.CheckSphere(base.transform.position, 0.3f, collisionLayers);
		while (!flag)
		{
			yield return new WaitForFixedUpdate();
			flag = Physics.CheckSphere(base.transform.position, 0.8f, collisionLayers);
		}
		CmdHandleDestroyAndDrop();
	}

	[Command]
	private void CmdHandleDestroyAndDrop()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(ProjectileSpawn), "CmdHandleDestroyAndDrop", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private void OnChangeInvId(int oldId, int newId)
	{
		NetworkdropId = newId;
		for (int i = 0; i < dropOrder.Length; i++)
		{
			if (dropOrder[i].getItemId() == dropId)
			{
				myMeshFilter.sharedMesh = dropMeshes[i];
				break;
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdHandleDestroyAndDrop()
	{
		NetworkMapSharer.Instance.spawnAServerDrop(dropId, 1, base.transform.position);
		NetworkServer.Destroy(base.gameObject);
	}

	protected static void InvokeUserCode_CmdHandleDestroyAndDrop(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHandleDestroyAndDrop called on client.");
		}
		else
		{
			((ProjectileSpawn)obj).UserCode_CmdHandleDestroyAndDrop();
		}
	}

	static ProjectileSpawn()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(ProjectileSpawn), "CmdHandleDestroyAndDrop", InvokeUserCode_CmdHandleDestroyAndDrop, requiresAuthority: true);
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteVector3(syncDirection);
			writer.WriteFloat(syncSpeed);
			writer.WriteInt(dropId);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteVector3(syncDirection);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteFloat(syncSpeed);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteInt(dropId);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			Vector3 vector = syncDirection;
			NetworksyncDirection = reader.ReadVector3();
			float num = syncSpeed;
			NetworksyncSpeed = reader.ReadFloat();
			int num2 = dropId;
			NetworkdropId = reader.ReadInt();
			if (!SyncVarEqual(num2, ref dropId))
			{
				OnChangeInvId(num2, dropId);
			}
			return;
		}
		long num3 = (long)reader.ReadULong();
		if ((num3 & 1L) != 0L)
		{
			Vector3 vector2 = syncDirection;
			NetworksyncDirection = reader.ReadVector3();
		}
		if ((num3 & 2L) != 0L)
		{
			float num4 = syncSpeed;
			NetworksyncSpeed = reader.ReadFloat();
		}
		if ((num3 & 4L) != 0L)
		{
			int num5 = dropId;
			NetworkdropId = reader.ReadInt();
			if (!SyncVarEqual(num5, ref dropId))
			{
				OnChangeInvId(num5, dropId);
			}
		}
	}
}
