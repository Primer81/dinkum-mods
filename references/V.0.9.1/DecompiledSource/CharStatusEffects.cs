using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class CharStatusEffects : NetworkBehaviour
{
	[SyncVar(hook = "OnChangeWet")]
	public bool IsWet;

	public ParticleSystem wetParticle;

	private CharMovement myChar;

	private CharNetworkAnimator myCharAnimator;

	private bool localWet;

	private int localWetTimer;

	[SyncVar]
	private Vector3 KiteWorldPos;

	private Damageable myDamageable;

	private float emitTimer;

	public bool NetworkIsWet
	{
		get
		{
			return IsWet;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref IsWet))
			{
				bool isWet = IsWet;
				SetSyncVar(value, ref IsWet, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					OnChangeWet(isWet, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public Vector3 NetworkKiteWorldPos
	{
		get
		{
			return KiteWorldPos;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref KiteWorldPos))
			{
				Vector3 kiteWorldPos = KiteWorldPos;
				SetSyncVar(value, ref KiteWorldPos, 2uL);
			}
		}
	}

	private void Start()
	{
		myChar = GetComponent<CharMovement>();
		myDamageable = GetComponent<Damageable>();
		myCharAnimator = GetComponent<CharNetworkAnimator>();
	}

	public override void OnStartLocalPlayer()
	{
		myChar = GetComponent<CharMovement>();
		myDamageable = GetComponent<Damageable>();
		myCharAnimator = GetComponent<CharNetworkAnimator>();
		StartCoroutine(HandleStatus());
		StartCoroutine(RunUnderGroundChecks());
	}

	public override void OnStartClient()
	{
		OnChangeWet(IsWet, IsWet);
	}

	private IEnumerator HandleStatus()
	{
		WaitForSeconds wait = new WaitForSeconds(0.5f);
		while (true)
		{
			yield return wait;
			if (!IsWet)
			{
				if ((myChar.swimming || myChar.underWater || IsInRain()) && !localWet)
				{
					localWetTimer = 0;
					localWet = true;
					CmdSetWet(newWet: true);
				}
			}
			else if (IsWet && !myChar.swimming && !myChar.underWater && !IsInRain() && localWet)
			{
				if (localWetTimer <= 10)
				{
					localWetTimer++;
					continue;
				}
				localWet = false;
				CmdSetWet(newWet: false);
			}
		}
	}

	public bool IsInRain()
	{
		if (WeatherManager.Instance.SnowFallPossible())
		{
			return false;
		}
		if (WeatherManager.Instance.IsMyPlayerInside || RealWorldTimeLight.time.underGround)
		{
			return false;
		}
		if (WeatherManager.Instance.IsRaining && RealWorldTimeLight.time.currentMinute > 2)
		{
			return true;
		}
		return false;
	}

	private void OnChangeWet(bool oldValue, bool newValue)
	{
		NetworkIsWet = newValue;
		if (IsWet)
		{
			StartCoroutine(HandleWetParticles());
		}
		else
		{
			wetParticle.Stop();
		}
	}

	private IEnumerator HandleWetParticles()
	{
		while (IsWet)
		{
			if (ShouldShowWetParticles())
			{
				if (emitTimer > 0.25f)
				{
					emitTimer = 0f;
					wetParticle.Emit(1);
				}
				emitTimer += Time.deltaTime;
			}
			yield return null;
		}
		wetParticle.Stop();
	}

	[Command]
	public void CmdSetWet(bool newWet)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(newWet);
		SendCommandInternal(typeof(CharStatusEffects), "CmdSetWet", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private bool ShouldShowWetParticles()
	{
		if (myChar.isLocalPlayer)
		{
			if (myChar.swimming || myChar.underWater)
			{
				return false;
			}
		}
		else if (myChar.underWater || myCharAnimator.swimming)
		{
			return false;
		}
		return true;
	}

	public void ConnectKite(HandHeldKite kite)
	{
		if (kite != null && base.isLocalPlayer)
		{
			StartCoroutine(SyncKitePosition(kite));
		}
	}

	private IEnumerator SyncKitePosition(HandHeldKite kite)
	{
		WaitForSeconds kiteSyncWait = new WaitForSeconds(0.1f);
		while (kite != null)
		{
			if (Vector3.Distance(kite.GetKitePos(), KiteWorldPos) > 0.25f)
			{
				CmdSetNewKitePos(kite.GetKitePos());
			}
			yield return kiteSyncWait;
		}
		CmdSetNewKitePos(Vector3.zero);
	}

	[Command]
	private void CmdSetNewKitePos(Vector3 newKitePos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(newKitePos);
		SendCommandInternal(typeof(CharStatusEffects), "CmdSetNewKitePos", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public Vector3 GetNetworkKitePosition()
	{
		return KiteWorldPos;
	}

	private IEnumerator RunUnderGroundChecks()
	{
		float hurtTimer = 1f;
		while (true)
		{
			if (RealWorldTimeLight.time.underGround)
			{
				bool flag;
				if (hurtTimer < 1f)
				{
					flag = false;
					hurtTimer += Time.deltaTime;
				}
				else
				{
					flag = true;
				}
				if (WorldManager.Instance.isPositionOnMap(base.transform.position))
				{
					int num = Mathf.RoundToInt(base.transform.position.x / 2f);
					int num2 = Mathf.RoundToInt(base.transform.position.z / 2f);
					if (myChar.grounded && WorldManager.Instance.onTileMap[num, num2] == 881 && base.transform.position.y <= (float)WorldManager.Instance.heightMap[num, num2] + 0.6f)
					{
						if (!myDamageable.onFire)
						{
							CmdSetOnFire();
						}
						if (flag)
						{
							CmdTakeDamageFromGround(10);
							hurtTimer = 0f;
						}
					}
				}
			}
			yield return null;
		}
	}

	[Command]
	private void CmdSetOnFire()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharStatusEffects), "CmdSetOnFire", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdTakeDamageFromGround(int damageAmount)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(damageAmount);
		SendCommandInternal(typeof(CharStatusEffects), "CmdTakeDamageFromGround", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdSetWet(bool newWet)
	{
		NetworkIsWet = newWet;
	}

	protected static void InvokeUserCode_CmdSetWet(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetWet called on client.");
		}
		else
		{
			((CharStatusEffects)obj).UserCode_CmdSetWet(reader.ReadBool());
		}
	}

	protected void UserCode_CmdSetNewKitePos(Vector3 newKitePos)
	{
		NetworkKiteWorldPos = newKitePos;
	}

	protected static void InvokeUserCode_CmdSetNewKitePos(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetNewKitePos called on client.");
		}
		else
		{
			((CharStatusEffects)obj).UserCode_CmdSetNewKitePos(reader.ReadVector3());
		}
	}

	protected void UserCode_CmdSetOnFire()
	{
		myDamageable.setOnFire();
	}

	protected static void InvokeUserCode_CmdSetOnFire(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetOnFire called on client.");
		}
		else
		{
			((CharStatusEffects)obj).UserCode_CmdSetOnFire();
		}
	}

	protected void UserCode_CmdTakeDamageFromGround(int damageAmount)
	{
		myDamageable.doDamageFromStatus(damageAmount);
	}

	protected static void InvokeUserCode_CmdTakeDamageFromGround(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTakeDamageFromGround called on client.");
		}
		else
		{
			((CharStatusEffects)obj).UserCode_CmdTakeDamageFromGround(reader.ReadInt());
		}
	}

	static CharStatusEffects()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharStatusEffects), "CmdSetWet", InvokeUserCode_CmdSetWet, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharStatusEffects), "CmdSetNewKitePos", InvokeUserCode_CmdSetNewKitePos, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharStatusEffects), "CmdSetOnFire", InvokeUserCode_CmdSetOnFire, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharStatusEffects), "CmdTakeDamageFromGround", InvokeUserCode_CmdTakeDamageFromGround, requiresAuthority: true);
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(IsWet);
			writer.WriteVector3(KiteWorldPos);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(IsWet);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteVector3(KiteWorldPos);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			bool isWet = IsWet;
			NetworkIsWet = reader.ReadBool();
			if (!SyncVarEqual(isWet, ref IsWet))
			{
				OnChangeWet(isWet, IsWet);
			}
			Vector3 kiteWorldPos = KiteWorldPos;
			NetworkKiteWorldPos = reader.ReadVector3();
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			bool isWet2 = IsWet;
			NetworkIsWet = reader.ReadBool();
			if (!SyncVarEqual(isWet2, ref IsWet))
			{
				OnChangeWet(isWet2, IsWet);
			}
		}
		if ((num & 2L) != 0L)
		{
			Vector3 kiteWorldPos2 = KiteWorldPos;
			NetworkKiteWorldPos = reader.ReadVector3();
		}
	}
}
