using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class TuckshopManager : NetworkBehaviour
{
	public static TuckshopManager manage;

	public TuckshopSeat[] seats;

	public readonly SyncList<bool> someoneInSeat = new SyncList<bool>();

	[SyncVar(hook = "OnSpecialItemChange")]
	public int specialItemId;

	public UnityEvent changeSpecialItemBoard = new UnityEvent();

	public int NetworkspecialItemId
	{
		get
		{
			return specialItemId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref specialItemId))
			{
				int oldId = specialItemId;
				SetSyncVar(value, ref specialItemId, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					OnSpecialItemChange(oldId, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	private void Awake()
	{
		manage = this;
	}

	public override void OnStartServer()
	{
		for (int i = 0; i < seats.Length; i++)
		{
			someoneInSeat.Add(item: false);
		}
		setSpecialItem();
	}

	public override void OnStartClient()
	{
		someoneInSeat.Callback += OnSeatChange;
		for (int i = 0; i < someoneInSeat.Count; i++)
		{
			this.OnSeatChange(SyncList<bool>.Operation.OP_SET, i, someoneInSeat[i], someoneInSeat[i]);
		}
		OnSpecialItemChange(specialItemId, specialItemId);
	}

	public void sitInSeat(int seatId)
	{
		someoneInSeat[seatId] = true;
	}

	public void getUpFromSeat(int seatId)
	{
		someoneInSeat[seatId] = false;
	}

	public void OnSeatChange(SyncList<bool>.Operation op, int seatNo, bool oldBool, bool newBool)
	{
		if ((bool)seats[seatNo])
		{
			seats[seatNo].updateTheSeat(newBool);
		}
	}

	public void connectSeatToId(TuckshopSeat newSeat)
	{
		seats[newSeat.mySeatId] = newSeat;
		if (someoneInSeat.Count > newSeat.mySeatId)
		{
			newSeat.updateTheSeat(someoneInSeat[newSeat.mySeatId]);
		}
	}

	public bool isSeatTaken(int seatId)
	{
		if (seatId < 0)
		{
			return true;
		}
		return someoneInSeat[seatId];
	}

	public void setSpecialItem()
	{
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].consumeable && !Inventory.Instance.allItems[i].isOneOfKindUniqueItem && !Inventory.Instance.allItems[i].isUniqueItem)
			{
				list.Add(Inventory.Instance.allItems[i]);
			}
		}
		NetworkspecialItemId = list[Random.Range(0, list.Count)].getItemId();
	}

	public void OnSpecialItemChange(int oldId, int newId)
	{
		changeSpecialItemBoard.Invoke();
		NetworkspecialItemId = newId;
		NPCManager.manage.NPCDetails[12].keeperConvos.targetResponses[0].talkingAboutItem = Inventory.Instance.allItems[specialItemId];
	}

	public TuckshopManager()
	{
		InitSyncObject(someoneInSeat);
	}

	private void MirrorProcessed()
	{
	}

	static TuckshopManager()
	{
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(specialItemId);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(specialItemId);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = specialItemId;
			NetworkspecialItemId = reader.ReadInt();
			if (!SyncVarEqual(num, ref specialItemId))
			{
				OnSpecialItemChange(num, specialItemId);
			}
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			int num3 = specialItemId;
			NetworkspecialItemId = reader.ReadInt();
			if (!SyncVarEqual(num3, ref specialItemId))
			{
				OnSpecialItemChange(num3, specialItemId);
			}
		}
	}
}
