using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class WishManager : NetworkBehaviour
{
	public enum WishType
	{
		None,
		PeacefulWish,
		DangerousWish,
		BountifulWish,
		SpotlessWish,
		FortuitousWish
	}

	[SyncVar]
	public bool wishMadeToday;

	[SyncVar]
	public int currentWishType;

	public int tomorrowsWishType;

	public Color defaultColour;

	public Color peacefulColour;

	public Color dangerousColour;

	public Color bountifulColour;

	public Color fortuitousColour;

	public Color spotlessColour;

	public bool NetworkwishMadeToday
	{
		get
		{
			return wishMadeToday;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref wishMadeToday))
			{
				bool flag = wishMadeToday;
				SetSyncVar(value, ref wishMadeToday, 1uL);
			}
		}
	}

	public int NetworkcurrentWishType
	{
		get
		{
			return currentWishType;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref currentWishType))
			{
				int num = currentWishType;
				SetSyncVar(value, ref currentWishType, 2uL);
			}
		}
	}

	public int GetWishCost()
	{
		return 150000;
	}

	public string GetCurrentWishName()
	{
		if (currentWishType == 1)
		{
			return string.Format(ConversationManager.manage.GetLocByTag("CurrentWishNameInEffect"), UIAnimationManager.manage.GetItemColorTag(ConversationManager.manage.GetLocByTag("Peaceful_Wish")));
		}
		if (currentWishType == 2)
		{
			return string.Format(ConversationManager.manage.GetLocByTag("CurrentWishNameInEffect"), UIAnimationManager.manage.GetItemColorTag(ConversationManager.manage.GetLocByTag("Dangerous_Wish")));
		}
		if (currentWishType == 3)
		{
			return string.Format(ConversationManager.manage.GetLocByTag("CurrentWishNameInEffect"), UIAnimationManager.manage.GetItemColorTag(ConversationManager.manage.GetLocByTag("Bountiful_Wish")));
		}
		if (currentWishType == 5)
		{
			return string.Format(ConversationManager.manage.GetLocByTag("CurrentWishNameInEffect"), UIAnimationManager.manage.GetItemColorTag(ConversationManager.manage.GetLocByTag("Fortuitous_Wish")));
		}
		if (currentWishType == 4)
		{
			return string.Format(ConversationManager.manage.GetLocByTag("CurrentWishNameInEffect"), UIAnimationManager.manage.GetItemColorTag(ConversationManager.manage.GetLocByTag("Spotless_Wish")));
		}
		return ConversationManager.manage.GetLocByTag("No_Wish");
	}

	public void CheckWishOnDayChange()
	{
		if (!base.isServer || !wishMadeToday)
		{
			return;
		}
		if (tomorrowsWishType != currentWishType)
		{
			if (currentWishType == 1 && tomorrowsWishType != 1)
			{
				AnimalManager.manage.MakeAnimalsNormal();
				AnimalManager.manage.SetChangedOverNight(changeTo: true);
			}
			if (tomorrowsWishType == 1)
			{
				AnimalManager.manage.MakeAnimalsEasy();
				AnimalManager.manage.SetChangedOverNight(changeTo: true);
			}
		}
		NetworkcurrentWishType = tomorrowsWishType;
	}

	public bool IsWishActive(WishType toCheck)
	{
		return currentWishType == (int)toCheck;
	}

	public Color GetCurrentWishColour()
	{
		if (currentWishType == 1)
		{
			return peacefulColour;
		}
		if (currentWishType == 2)
		{
			return dangerousColour;
		}
		if (currentWishType == 3)
		{
			return bountifulColour;
		}
		if (currentWishType == 5)
		{
			return fortuitousColour;
		}
		if (currentWishType == 4)
		{
			return spotlessColour;
		}
		return defaultColour;
	}

	private void MirrorProcessed()
	{
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(wishMadeToday);
			writer.WriteInt(currentWishType);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(wishMadeToday);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteInt(currentWishType);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			bool flag = wishMadeToday;
			NetworkwishMadeToday = reader.ReadBool();
			int num = currentWishType;
			NetworkcurrentWishType = reader.ReadInt();
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			bool flag2 = wishMadeToday;
			NetworkwishMadeToday = reader.ReadBool();
		}
		if ((num2 & 2L) != 0L)
		{
			int num3 = currentWishType;
			NetworkcurrentWishType = reader.ReadInt();
		}
	}
}
