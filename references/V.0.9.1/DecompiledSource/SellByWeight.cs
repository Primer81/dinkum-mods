using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class SellByWeight : NetworkBehaviour
{
	public float minKG = 0.75f;

	public float maxKG = 5f;

	public int rewardPerKG = 5000;

	[SyncVar(hook = "onWeightChange")]
	private float myWeight;

	public string itemName = "Wary Mu Egg";

	public bool playEffectOnSpawn;

	public DailyTaskGenerator.genericTaskType taskWhenSold;

	public float NetworkmyWeight
	{
		get
		{
			return myWeight;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref myWeight))
			{
				float oldWeight = myWeight;
				SetSyncVar(value, ref myWeight, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					onWeightChange(oldWeight, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public override void OnStartServer()
	{
		generateWeight();
	}

	public override void OnStartClient()
	{
		if (playEffectOnSpawn)
		{
			SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.dropItem, base.transform.position);
			ParticleManager.manage.emitAttackParticle(base.transform.position, 0);
		}
	}

	private void generateWeight()
	{
		NetworkmyWeight = Random.Range(minKG, maxKG);
	}

	public float getMyWeight()
	{
		return myWeight;
	}

	public void onWeightChange(float oldWeight, float newWeight)
	{
		NetworkmyWeight = newWeight;
	}

	public int getPrice()
	{
		int num = (int)(myWeight * (float)rewardPerKG);
		return num + (int)((float)num / 10f * (float)LicenceManager.manage.allLicences[8].getCurrentLevel());
	}

	public string GetName()
	{
		return GetComponent<PickUpAndCarry>().GetName();
	}

	private void MirrorProcessed()
	{
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteFloat(myWeight);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteFloat(myWeight);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			float num = myWeight;
			NetworkmyWeight = reader.ReadFloat();
			if (!SyncVarEqual(num, ref myWeight))
			{
				onWeightChange(num, myWeight);
			}
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			float num3 = myWeight;
			NetworkmyWeight = reader.ReadFloat();
			if (!SyncVarEqual(num3, ref myWeight))
			{
				onWeightChange(num3, myWeight);
			}
		}
	}
}
