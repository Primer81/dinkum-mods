using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class LawnMower : NetworkBehaviour
{
	public Vehicle myVehicle;

	public TileObject[] objectsCanCut;

	public ParticleSystem backPart;

	public ParticleSystem underPart;

	public ASound mowerRunOverSound;

	public Transform[] mowFromPosition;

	private bool collectsSeeds;

	public LayerMask dropLayer;

	public InventoryItem[] grassSeeds;

	[SyncVar(hook = "OnBladeDownChange")]
	public bool bladeDown;

	public GameObject showBladeDown;

	public ASound putBladeDownSound;

	private ParticleSystemRenderer rend;

	private ParticleSystemRenderer rend2;

	private float actionTimer;

	private float driverTimer = 0.5f;

	public bool NetworkbladeDown
	{
		get
		{
			return bladeDown;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref bladeDown))
			{
				bool oldBladeDown = bladeDown;
				SetSyncVar(value, ref bladeDown, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					OnBladeDownChange(oldBladeDown, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	private void Start()
	{
		if (mowFromPosition == null || mowFromPosition.Length == 0)
		{
			mowFromPosition = new Transform[1] { base.transform };
		}
		if (grassSeeds.Length != 0)
		{
			collectsSeeds = true;
		}
		if ((bool)backPart)
		{
			rend = backPart.GetComponent<ParticleSystemRenderer>();
		}
		if ((bool)underPart)
		{
			rend2 = underPart.GetComponent<ParticleSystemRenderer>();
		}
	}

	public override void OnStartClient()
	{
		OnBladeDownChange(bladeDown, bladeDown);
	}

	public override void OnStartAuthority()
	{
		StartCoroutine("mowTheLawn");
	}

	private IEnumerator mowTheLawn()
	{
		driverTimer = 0.5f;
		while (base.hasAuthority)
		{
			yield return null;
			if (myVehicle.hasDriver())
			{
				if (driverTimer > 0f)
				{
					driverTimer -= Time.deltaTime;
				}
				if (driverTimer <= 0f && InputMaster.input.VehicleInteract())
				{
					CmdSetNewBlade(!bladeDown);
					yield return new WaitForSeconds(0.25f);
				}
				if (actionTimer > 0f)
				{
					actionTimer -= Time.deltaTime;
				}
				if (!bladeDown || !(actionTimer <= 0f))
				{
					continue;
				}
				for (int i = 0; i < mowFromPosition.Length; i++)
				{
					int num = Mathf.RoundToInt(mowFromPosition[i].position.x / 2f);
					int num2 = Mathf.RoundToInt(mowFromPosition[i].position.z / 2f);
					if ((WorldManager.Instance.onTileMap[num, num2] > 0 && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[num, num2]].isGrass) || WorldManager.Instance.onTileMap[num, num2] == 30)
					{
						if (shouldCutTileType(num, num2))
						{
							if (base.hasAuthority)
							{
								DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.CutGrass);
								DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.LawnMower);
								CmdCutTheGrass(num, num2);
							}
							StartCoroutine(playCutParticles(WorldManager.Instance.tileTypeMap[num, num2]));
							actionTimer = 0.15f;
						}
					}
					else if (WorldManager.Instance.onTileMap[num, num2] == -1 && shouldCutTileType(num, num2))
					{
						if (base.hasAuthority)
						{
							DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.LawnMower);
							CmdChangeTileType(num, num2);
						}
						StartCoroutine(playCutParticles(WorldManager.Instance.tileTypeMap[num, num2]));
						actionTimer = 0.15f;
					}
					else if (WorldManager.Instance.onTileMap[num, num2] == -1 && hasBeenCut(num, num2))
					{
						StartCoroutine(playAlreadyCutParticles(WorldManager.Instance.tileTypeMap[num, num2]));
						actionTimer = 0.15f;
					}
				}
				if (collectsSeeds)
				{
					CollectSeeds();
				}
			}
			else
			{
				driverTimer = 0.5f;
				if (bladeDown)
				{
					CmdSetNewBlade(!bladeDown);
					yield return new WaitForSeconds(0.25f);
				}
			}
		}
	}

	public void CollectSeeds()
	{
		if (!Physics.CheckSphere(base.transform.position + base.transform.forward * 2f, 5f, dropLayer))
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position + base.transform.forward * 2f, 5f, dropLayer);
		for (int i = 0; i < array.Length; i++)
		{
			DroppedItem component = array[i].GetComponent<DroppedItem>();
			if ((bool)component && IsGrassSeed(component.myItemId) && NetworkMapSharer.Instance.localChar.myEquip.myPermissions.CheckIfCanPickUp())
			{
				if (Inventory.Instance.checkIfItemCanFit(component.myItemId, component.stackAmount))
				{
					SoundManager.Instance.play2DSound(SoundManager.Instance.pickUpItem);
					component.pickUpLocal();
					NetworkMapSharer.Instance.localChar.myPickUp.CmdPickUp(component.netId);
				}
				else
				{
					NotificationManager.manage.turnOnPocketsFullNotification(holdingButton: true);
				}
			}
		}
	}

	private bool IsGrassSeed(int checkId)
	{
		for (int i = 0; i < grassSeeds.Length; i++)
		{
			if (grassSeeds[i].getItemId() == checkId)
			{
				return true;
			}
		}
		return false;
	}

	[Command]
	public void CmdCutTheGrass(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(LawnMower), "CmdCutTheGrass", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeTileType(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(LawnMower), "CmdChangeTileType", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void mowTileType(int xPos, int yPos)
	{
		if (WorldManager.Instance.tileTypeMap[xPos, yPos] == 1)
		{
			NetworkMapSharer.Instance.RpcUpdateTileType(23, xPos, yPos);
		}
		if (WorldManager.Instance.tileTypeMap[xPos, yPos] == 15)
		{
			NetworkMapSharer.Instance.RpcUpdateTileType(24, xPos, yPos);
		}
		if (WorldManager.Instance.tileTypeMap[xPos, yPos] == 4)
		{
			NetworkMapSharer.Instance.RpcUpdateTileType(25, xPos, yPos);
		}
	}

	public bool shouldCutTileType(int xPos, int yPos)
	{
		if (WorldManager.Instance.tileTypeMap[xPos, yPos] != 1 && WorldManager.Instance.tileTypeMap[xPos, yPos] != 4)
		{
			return WorldManager.Instance.tileTypeMap[xPos, yPos] == 15;
		}
		return true;
	}

	public bool hasBeenCut(int xPos, int yPos)
	{
		if (WorldManager.Instance.tileTypeMap[xPos, yPos] != 23 && WorldManager.Instance.tileTypeMap[xPos, yPos] != 24)
		{
			return WorldManager.Instance.tileTypeMap[xPos, yPos] == 25;
		}
		return true;
	}

	private IEnumerator playCutParticles(int cuttingTileTypeId)
	{
		rend.sharedMaterial = WorldManager.Instance.tileTypes[cuttingTileTypeId].myTileMaterial;
		rend2.sharedMaterial = WorldManager.Instance.tileTypes[cuttingTileTypeId].myTileMaterial;
		float cutTimer = 0.55f;
		SoundManager.Instance.playASoundAtPoint(mowerRunOverSound, base.transform.position);
		while (cutTimer > 0f)
		{
			yield return null;
			cutTimer -= Time.deltaTime;
			if (Random.Range(0, 5) == 2)
			{
				backPart.Emit(4);
				underPart.Emit(4);
			}
		}
	}

	private IEnumerator playAlreadyCutParticles(int cuttingTileTypeId)
	{
		rend.sharedMaterial = WorldManager.Instance.tileTypes[cuttingTileTypeId].myTileMaterial;
		float cutTimer = 0.15f;
		while (cutTimer > 0f)
		{
			yield return null;
			cutTimer -= Time.deltaTime;
			if (Random.Range(0, 10) == 2)
			{
				backPart.Emit(1);
			}
		}
	}

	[Command]
	public void CmdSetNewBlade(bool newBladeDown)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(newBladeDown);
		SendCommandInternal(typeof(LawnMower), "CmdSetNewBlade", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void OnBladeDownChange(bool oldBladeDown, bool newBladeDown)
	{
		NetworkbladeDown = newBladeDown;
		showBladeDown.SetActive(bladeDown);
		SoundManager.Instance.playASoundAtPoint(putBladeDownSound, base.transform.position);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdCutTheGrass(int xPos, int yPos)
	{
		NetworkMapSharer.Instance.RpcUpdateOnTileObject(-1, xPos, yPos);
		mowTileType(xPos, yPos);
	}

	protected static void InvokeUserCode_CmdCutTheGrass(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCutTheGrass called on client.");
		}
		else
		{
			((LawnMower)obj).UserCode_CmdCutTheGrass(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeTileType(int xPos, int yPos)
	{
		mowTileType(xPos, yPos);
	}

	protected static void InvokeUserCode_CmdChangeTileType(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeTileType called on client.");
		}
		else
		{
			((LawnMower)obj).UserCode_CmdChangeTileType(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdSetNewBlade(bool newBladeDown)
	{
		NetworkbladeDown = newBladeDown;
	}

	protected static void InvokeUserCode_CmdSetNewBlade(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetNewBlade called on client.");
		}
		else
		{
			((LawnMower)obj).UserCode_CmdSetNewBlade(reader.ReadBool());
		}
	}

	static LawnMower()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(LawnMower), "CmdCutTheGrass", InvokeUserCode_CmdCutTheGrass, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(LawnMower), "CmdChangeTileType", InvokeUserCode_CmdChangeTileType, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(LawnMower), "CmdSetNewBlade", InvokeUserCode_CmdSetNewBlade, requiresAuthority: true);
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(bladeDown);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(bladeDown);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			bool flag = bladeDown;
			NetworkbladeDown = reader.ReadBool();
			if (!SyncVarEqual(flag, ref bladeDown))
			{
				OnBladeDownChange(flag, bladeDown);
			}
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			bool flag2 = bladeDown;
			NetworkbladeDown = reader.ReadBool();
			if (!SyncVarEqual(flag2, ref bladeDown))
			{
				OnBladeDownChange(flag2, bladeDown);
			}
		}
	}
}
