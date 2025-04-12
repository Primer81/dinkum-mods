using System.Runtime.InteropServices;
using Mirror;

public class BuildingColourManagement : NetworkBehaviour
{
	[SyncVar(hook = "ChangeJohnsShop")]
	public int JohnsShopColour;

	public BuildingCustomisation JohnsShop;

	public BuildingCustomisation JohnsShopInterior;

	public int NetworkJohnsShopColour
	{
		get
		{
			return JohnsShopColour;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref JohnsShopColour))
			{
				int johnsShopColour = JohnsShopColour;
				SetSyncVar(value, ref JohnsShopColour, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					ChangeJohnsShop(johnsShopColour, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public void ChangeJohnsShop(int oldColour, int newColour)
	{
		JohnsShop.ChangeColourId(newColour);
	}

	private void MirrorProcessed()
	{
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(JohnsShopColour);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(JohnsShopColour);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int johnsShopColour = JohnsShopColour;
			NetworkJohnsShopColour = reader.ReadInt();
			if (!SyncVarEqual(johnsShopColour, ref JohnsShopColour))
			{
				ChangeJohnsShop(johnsShopColour, JohnsShopColour);
			}
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			int johnsShopColour2 = JohnsShopColour;
			NetworkJohnsShopColour = reader.ReadInt();
			if (!SyncVarEqual(johnsShopColour2, ref JohnsShopColour))
			{
				ChangeJohnsShop(johnsShopColour2, JohnsShopColour);
			}
		}
	}
}
