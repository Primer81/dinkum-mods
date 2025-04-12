using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class CharChestCarrier : NetworkBehaviour
{
	[SyncVar(hook = "OnChestCarryChange")]
	public int carryingChestId = -1;

	private CharInteract myInteract;

	private GameObject carryingPrefab;

	private Chest carryingChestDetails;

	private ChestCarrierObject carrierInPlayerHands;

	public InventoryItem trolleyObject;

	[SyncVar(hook = "OnTreeCarryChange")]
	public int treeCarryId;

	[SyncVar(hook = "OnTreeStatusChange")]
	public int treeCarryStatus;

	private TreeMovingGloves treeGloves;

	private string signText = "";

	public int NetworkcarryingChestId
	{
		get
		{
			return carryingChestId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref carryingChestId))
			{
				int oldId = carryingChestId;
				SetSyncVar(value, ref carryingChestId, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					OnChestCarryChange(oldId, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public int NetworktreeCarryId
	{
		get
		{
			return treeCarryId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref treeCarryId))
			{
				int oldInt = treeCarryId;
				SetSyncVar(value, ref treeCarryId, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, value: true);
					OnTreeCarryChange(oldInt, value);
					setSyncVarHookGuard(2uL, value: false);
				}
			}
		}
	}

	public int NetworktreeCarryStatus
	{
		get
		{
			return treeCarryStatus;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref treeCarryStatus))
			{
				int oldInt = treeCarryStatus;
				SetSyncVar(value, ref treeCarryStatus, 4uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(4uL))
				{
					setSyncVarHookGuard(4uL, value: true);
					OnTreeStatusChange(oldInt, value);
					setSyncVarHookGuard(4uL, value: false);
				}
			}
		}
	}

	private void Start()
	{
		myInteract = GetComponent<CharInteract>();
	}

	public override void OnStartClient()
	{
		OnChestCarryChange(carryingChestId, carryingChestId);
		base.OnStartClient();
	}

	private void OnChestCarryChange(int oldId, int newId)
	{
		NetworkcarryingChestId = newId;
		ShowNewPrefabOnTrolley();
		if (base.isLocalPlayer)
		{
			if (carryingChestId == -1)
			{
				trolleyObject.placeable = null;
			}
			else
			{
				trolleyObject.placeable = WorldManager.Instance.allObjects[carryingChestId];
			}
		}
	}

	private void ShowNewPrefabOnTrolley()
	{
		if ((bool)carryingPrefab)
		{
			Object.Destroy(carryingPrefab);
		}
		if (carryingChestId != -1 && (bool)carrierInPlayerHands)
		{
			carryingPrefab = Object.Instantiate(WorldManager.Instance.allObjects[carryingChestId], carrierInPlayerHands.objectSpawnPoint).gameObject;
			if (WorldManager.Instance.allObjects[carryingChestId].IsMultiTileObject())
			{
				carryingPrefab.transform.localPosition = new Vector3(0f - (float)WorldManager.Instance.allObjects[carryingChestId].GetXSize() / 2f, 0f, 0f);
			}
			else
			{
				carryingPrefab.transform.localPosition = Vector3.zero;
			}
			carryingPrefab.transform.localRotation = Quaternion.identity;
			Collider[] componentsInChildren = carryingPrefab.GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
	}

	public void PickUpChestAtCurrentPos()
	{
		if (!IsChestOnSelectedTile((int)myInteract.selectedTile.x, (int)myInteract.selectedTile.y))
		{
			return;
		}
		if (myInteract.InsideHouseDetails != null)
		{
			if (!WorldManager.Instance.checkTileClientLockHouse((int)myInteract.selectedTile.x, (int)myInteract.selectedTile.y, myInteract.InsideHouseDetails.xPos, myInteract.InsideHouseDetails.yPos))
			{
				WorldManager.Instance.lockTileHouseClient((int)myInteract.selectedTile.x, (int)myInteract.selectedTile.y, myInteract.InsideHouseDetails.xPos, myInteract.InsideHouseDetails.yPos);
				CmdPickUpChest((int)myInteract.selectedTile.x, (int)myInteract.selectedTile.y);
			}
		}
		else if (!WorldManager.Instance.CheckTileClientLock((int)myInteract.selectedTile.x, (int)myInteract.selectedTile.y))
		{
			Debug.Log((int)myInteract.selectedTile.x + "," + (int)myInteract.selectedTile.y);
			WorldManager.Instance.lockTileClient((int)myInteract.selectedTile.x, (int)myInteract.selectedTile.y);
			CmdPickUpChest((int)myInteract.selectedTile.x, (int)myInteract.selectedTile.y);
		}
	}

	public bool PutDownChestAtCurrentPos()
	{
		if (((myInteract.InsideHouseDetails != null && myInteract.CheckIfCanDamageInside(myInteract.selectedTile)) || myInteract.CheckIfCanDamage(myInteract.selectedTile)) && IfMultiTiledObjectAndCanBePlacedHere((int)myInteract.selectedTile.x, (int)myInteract.selectedTile.y))
		{
			if (myInteract.InsideHouseDetails == null && myInteract.myEquip.IsCurrentlyHoldingSinglePlaceableItem())
			{
				myInteract.CmdPlaceSingleChestOutside(myInteract.myEquip.itemCurrentlyHolding.placeable.tileObjectId, (int)myInteract.selectedTile.x, (int)myInteract.selectedTile.y, myInteract.ReturnPlacingObjectRotation());
			}
			else
			{
				myInteract.doDamage();
			}
			CmdPlaceTheChest((int)myInteract.selectedTile.x, (int)myInteract.selectedTile.y);
			return true;
		}
		return false;
	}

	[Command]
	private void CmdPickUpChest(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharChestCarrier), "CmdPickUpChest", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcRemoveChestFromClientsListsIfItsThere(int xPos, int yPos, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendRPCInternal(typeof(CharChestCarrier), "RpcRemoveChestFromClientsListsIfItsThere", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPlaceTheChest(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharChestCarrier), "CmdPlaceTheChest", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void SetCarrierInPlayerHands(ChestCarrierObject newLocalCarrier)
	{
		carrierInPlayerHands = newLocalCarrier;
		OnChestCarryChange(carryingChestId, carryingChestId);
	}

	public bool IsHoldingChest()
	{
		return carryingChestId != -1;
	}

	public bool NeedsToEnterPlacementMode()
	{
		if ((myInteract.InsideHouseDetails != null && !myInteract.IsPlacingDeed) || (myInteract.myEquip.CurrentlyHoldingMultiTiledPlaceableItem() && !myInteract.IsPlacingDeed))
		{
			return true;
		}
		return false;
	}

	public void EnterPlacementMode()
	{
		myInteract.doDamage();
	}

	public bool CheckIfChestIsOnSelectedTileForButtonPress()
	{
		if (IsChestOnSelectedTile((int)myInteract.selectedTile.x, (int)myInteract.selectedTile.y))
		{
			return true;
		}
		return false;
	}

	public bool IsChestOnSelectedTile(int xPos, int yPos)
	{
		if (myInteract.InsideHouseDetails != null && myInteract.InsideHouseDetails.houseMapOnTile[xPos, yPos] >= 0 && (bool)WorldManager.Instance.allObjects[myInteract.InsideHouseDetails.houseMapOnTile[xPos, yPos]].tileObjectChest)
		{
			if (WorldManager.Instance.allObjects[myInteract.InsideHouseDetails.houseMapOnTile[xPos, yPos]].tileObjectChest.isFishPond || WorldManager.Instance.allObjects[myInteract.InsideHouseDetails.houseMapOnTile[xPos, yPos]].tileObjectChest.isBugTerrarium)
			{
				return false;
			}
			if (WorldManager.Instance.allObjects[myInteract.InsideHouseDetails.houseMapOnTile[xPos, yPos]].canBePlaceOn() && ItemOnTopManager.manage.hasItemsOnTop(xPos, yPos, myInteract.InsideHouseDetails))
			{
				if (base.isLocalPlayer)
				{
					NotificationManager.manage.pocketsFull.removeItemsOnTop();
				}
				return false;
			}
			if (myInteract.InsideHouseDetails.houseMapOnTileStatus[xPos, yPos] == 0)
			{
				return true;
			}
		}
		if (WorldManager.Instance.onTileMap[xPos, yPos] >= 0 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest)
		{
			if (WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest.isFishPond || WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest.isBugTerrarium)
			{
				return false;
			}
			if (WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].canBePlaceOn() && ItemOnTopManager.manage.hasItemsOnTop(xPos, yPos, myInteract.InsideHouseDetails))
			{
				if (base.isLocalPlayer)
				{
					NotificationManager.manage.pocketsFull.removeItemsOnTop();
				}
				return false;
			}
			if (WorldManager.Instance.onTileStatusMap[xPos, yPos] == 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSelectedTileEmpty(int xPos, int yPos)
	{
		if (WorldManager.Instance.onTileMap[xPos, yPos] == -1 || WorldManager.Instance.onTileMap[xPos, yPos] == 30)
		{
			return true;
		}
		return false;
	}

	public bool IfMultiTiledObjectAndCanBePlacedHere(int xPos, int yPos)
	{
		if (myInteract.IsHighlighterBlockedByVehicleOrCarryItem())
		{
			return false;
		}
		if (!myInteract.myEquip.CurrentlyHoldingMultiTiledPlaceableItem())
		{
			return true;
		}
		if (myInteract.InsideHouseDetails == null)
		{
			if (myInteract.myEquip.CurrentlyHoldingMultiTiledPlaceableItem() && myInteract.myEquip.itemCurrentlyHolding.placeable.checkIfMultiTileObjectCanBePlaced(xPos, yPos, myInteract.ReturnPlacingObjectRotation()))
			{
				return myInteract.myEquip.itemCurrentlyHolding.placeable.checkIfMultiTileObjectIsUnderWater(myInteract.myEquip.itemCurrentlyHolding, xPos, yPos, myInteract.ReturnPlacingObjectRotation());
			}
			return false;
		}
		return myInteract.myEquip.itemCurrentlyHolding.placeable.CheckIfMultiTileObjectCanBePlacedInside(myInteract.InsideHouseDetails, xPos, yPos, myInteract.ReturnPlacingObjectRotation());
	}

	public override void OnStopServer()
	{
		if (!IsHoldingChest())
		{
			return;
		}
		NetworkMapSharer.Instance.spawnAServerDropToSave(WorldManager.Instance.allObjectSettings[carryingChestId].dropsItemOnDeath.getItemId(), 1, base.transform.position, myInteract.InsideHouseDetails);
		for (int i = 0; i < carryingChestDetails.itemIds.Length; i++)
		{
			if (carryingChestDetails.itemIds[i] >= 0)
			{
				NetworkMapSharer.Instance.spawnAServerDropToSave(carryingChestDetails.itemIds[i], carryingChestDetails.itemStacks[i], base.transform.position, myInteract.InsideHouseDetails);
			}
		}
	}

	public bool IsHoldingTree()
	{
		return treeCarryId != -1;
	}

	public bool CheckIfTileIsTreeForButton()
	{
		return CheckIfTileIsTree((int)myInteract.selectedTile.x, (int)myInteract.selectedTile.y);
	}

	public bool CheckIfTileIsEmptyForTreeForButton()
	{
		return CheckIfTileIsEmptyForTree((int)myInteract.selectedTile.x, (int)myInteract.selectedTile.y);
	}

	public bool CheckIfTileIsTree(int xPos, int yPos)
	{
		int num = WorldManager.Instance.onTileMap[xPos, yPos];
		if (num >= 0 && (bool)WorldManager.Instance.allObjectSettings[num].dropsObjectOnDeath && (WorldManager.Instance.allObjectSettings[num].isHardWood || WorldManager.Instance.allObjectSettings[num].isWood))
		{
			return true;
		}
		return false;
	}

	public bool CheckIfTileIsEmptyForTree(int xPos, int yPos)
	{
		int num = WorldManager.Instance.onTileMap[xPos, yPos];
		if (num == -1 || num == 30 || (num > -1 && WorldManager.Instance.allObjectSettings[num].isGrass))
		{
			return true;
		}
		return false;
	}

	public void SetTreeCarryGloves(TreeMovingGloves setTo)
	{
		treeGloves = setTo;
		setTo.SpawnTreeInHands(treeCarryId, treeCarryStatus);
	}

	private void OnTreeCarryChange(int oldInt, int newInt)
	{
		NetworktreeCarryId = newInt;
		if ((bool)treeGloves)
		{
			treeGloves.SpawnTreeInHands(treeCarryId, treeCarryStatus);
			SoundManager.Instance.playASoundAtPoint(treeGloves.pickUpTreeSound, base.transform.position);
		}
	}

	private void OnTreeStatusChange(int oldInt, int newInt)
	{
		NetworktreeCarryStatus = newInt;
		if ((bool)treeGloves)
		{
			treeGloves.SpawnTreeInHands(treeCarryId, treeCarryStatus);
		}
	}

	public void PickUpTree()
	{
		CmdPickUpTree((int)myInteract.selectedTile.x, (int)myInteract.selectedTile.y);
	}

	public void PlaceTree()
	{
		CmdPlaceTree((int)myInteract.selectedTile.x, (int)myInteract.selectedTile.y);
	}

	[Command]
	private void CmdPickUpTree(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharChestCarrier), "CmdPickUpTree", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPlaceTree(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharChestCarrier), "CmdPlaceTree", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdPickUpChest(int xPos, int yPos)
	{
		if (IsChestOnSelectedTile(xPos, yPos))
		{
			HouseDetails insideHouseDetails = myInteract.InsideHouseDetails;
			if (insideHouseDetails == null)
			{
				signText = SignManager.manage.getSignText(xPos, yPos, -1, -1);
				NetworkcarryingChestId = WorldManager.Instance.onTileMap[xPos, yPos];
				carryingChestDetails = ContainerManager.manage.ReturnChestAndRemoveFromListForChestMovement(xPos, yPos, -1, -1);
				NetworkMapSharer.Instance.RpcPickUpContainerObject(xPos, yPos);
				RpcRemoveChestFromClientsListsIfItsThere(xPos, yPos, -1, -1);
			}
			else
			{
				signText = SignManager.manage.getSignText(xPos, yPos, insideHouseDetails.xPos, insideHouseDetails.yPos);
				NetworkcarryingChestId = insideHouseDetails.houseMapOnTile[xPos, yPos];
				carryingChestDetails = ContainerManager.manage.ReturnChestAndRemoveFromListForChestMovement(xPos, yPos, insideHouseDetails.xPos, insideHouseDetails.yPos);
				NetworkMapSharer.Instance.RpcPickUpContainerObjectInside(xPos, yPos, insideHouseDetails.xPos, insideHouseDetails.yPos);
				RpcRemoveChestFromClientsListsIfItsThere(xPos, yPos, insideHouseDetails.xPos, insideHouseDetails.yPos);
			}
		}
	}

	protected static void InvokeUserCode_CmdPickUpChest(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickUpChest called on client.");
		}
		else
		{
			((CharChestCarrier)obj).UserCode_CmdPickUpChest(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcRemoveChestFromClientsListsIfItsThere(int xPos, int yPos, int houseX, int houseY)
	{
		if (!base.isServer)
		{
			ContainerManager.manage.ReturnChestAndRemoveFromListForChestMovement(xPos, yPos, houseX, houseY);
		}
	}

	protected static void InvokeUserCode_RpcRemoveChestFromClientsListsIfItsThere(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRemoveChestFromClientsListsIfItsThere called on server.");
		}
		else
		{
			((CharChestCarrier)obj).UserCode_RpcRemoveChestFromClientsListsIfItsThere(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPlaceTheChest(int xPos, int yPos)
	{
		if (carryingChestDetails == null)
		{
			return;
		}
		HouseDetails insideHouseDetails = myInteract.InsideHouseDetails;
		if (insideHouseDetails == null)
		{
			carryingChestDetails.SetNewChestPosition(xPos, yPos, -1, -1);
			if (signText != "")
			{
				GetComponent<CharPickUp>().RpcUpdateSignDetails(xPos, yPos, -1, -1, signText);
				signText = "";
			}
		}
		else
		{
			carryingChestDetails.SetNewChestPosition(xPos, yPos, insideHouseDetails.xPos, insideHouseDetails.yPos);
			if (signText != "")
			{
				GetComponent<CharPickUp>().RpcUpdateSignDetails(xPos, yPos, insideHouseDetails.xPos, insideHouseDetails.yPos, signText);
				signText = "";
			}
		}
		ContainerManager.manage.PlaceChestBackIntoListAfterMove(carryingChestDetails);
		carryingChestDetails = null;
		NetworkcarryingChestId = -1;
	}

	protected static void InvokeUserCode_CmdPlaceTheChest(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceTheChest called on client.");
		}
		else
		{
			((CharChestCarrier)obj).UserCode_CmdPlaceTheChest(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPickUpTree(int xPos, int yPos)
	{
		if (CheckIfTileIsTree(xPos, yPos))
		{
			NetworktreeCarryId = WorldManager.Instance.onTileMap[xPos, yPos];
			NetworktreeCarryStatus = WorldManager.Instance.onTileStatusMap[xPos, yPos];
			NetworkMapSharer.Instance.RpChangeOnTileObjectNoDrop(-1, xPos, yPos);
		}
	}

	protected static void InvokeUserCode_CmdPickUpTree(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickUpTree called on client.");
		}
		else
		{
			((CharChestCarrier)obj).UserCode_CmdPickUpTree(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPlaceTree(int xPos, int yPos)
	{
		if (CheckIfTileIsEmptyForTree(xPos, yPos))
		{
			NetworkMapSharer.Instance.RpcUpdateOnTileObject(treeCarryId, xPos, yPos);
			NetworkMapSharer.Instance.RpcGiveOnTileStatus(treeCarryStatus, xPos, yPos);
			NetworktreeCarryId = -1;
			NetworktreeCarryStatus = -1;
		}
	}

	protected static void InvokeUserCode_CmdPlaceTree(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceTree called on client.");
		}
		else
		{
			((CharChestCarrier)obj).UserCode_CmdPlaceTree(reader.ReadInt(), reader.ReadInt());
		}
	}

	static CharChestCarrier()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharChestCarrier), "CmdPickUpChest", InvokeUserCode_CmdPickUpChest, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharChestCarrier), "CmdPlaceTheChest", InvokeUserCode_CmdPlaceTheChest, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharChestCarrier), "CmdPickUpTree", InvokeUserCode_CmdPickUpTree, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharChestCarrier), "CmdPlaceTree", InvokeUserCode_CmdPlaceTree, requiresAuthority: true);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharChestCarrier), "RpcRemoveChestFromClientsListsIfItsThere", InvokeUserCode_RpcRemoveChestFromClientsListsIfItsThere);
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(carryingChestId);
			writer.WriteInt(treeCarryId);
			writer.WriteInt(treeCarryStatus);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(carryingChestId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteInt(treeCarryId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteInt(treeCarryStatus);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = carryingChestId;
			NetworkcarryingChestId = reader.ReadInt();
			if (!SyncVarEqual(num, ref carryingChestId))
			{
				OnChestCarryChange(num, carryingChestId);
			}
			int num2 = treeCarryId;
			NetworktreeCarryId = reader.ReadInt();
			if (!SyncVarEqual(num2, ref treeCarryId))
			{
				OnTreeCarryChange(num2, treeCarryId);
			}
			int num3 = treeCarryStatus;
			NetworktreeCarryStatus = reader.ReadInt();
			if (!SyncVarEqual(num3, ref treeCarryStatus))
			{
				OnTreeStatusChange(num3, treeCarryStatus);
			}
			return;
		}
		long num4 = (long)reader.ReadULong();
		if ((num4 & 1L) != 0L)
		{
			int num5 = carryingChestId;
			NetworkcarryingChestId = reader.ReadInt();
			if (!SyncVarEqual(num5, ref carryingChestId))
			{
				OnChestCarryChange(num5, carryingChestId);
			}
		}
		if ((num4 & 2L) != 0L)
		{
			int num6 = treeCarryId;
			NetworktreeCarryId = reader.ReadInt();
			if (!SyncVarEqual(num6, ref treeCarryId))
			{
				OnTreeCarryChange(num6, treeCarryId);
			}
		}
		if ((num4 & 4L) != 0L)
		{
			int num7 = treeCarryStatus;
			NetworktreeCarryStatus = reader.ReadInt();
			if (!SyncVarEqual(num7, ref treeCarryStatus))
			{
				OnTreeStatusChange(num7, treeCarryStatus);
			}
		}
	}
}
