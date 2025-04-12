using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.AI;

public class NetworkFishingRod : NetworkBehaviour
{
	private EquipItemToChar myEquip;

	private CharMovement myChar;

	public FishingRodCastAndReel connectedFishingRod;

	public ASound fishLatch;

	public ASound fishBloop;

	public bool lineIsCasted;

	[SyncVar]
	private uint fishId;

	private AnimalAI fishConnectedAi;

	[SyncVar]
	public Vector3 reelPosition;

	[SyncVar(hook = "FishOnLineChange")]
	public int fishOnLine = -1;

	[SyncVar(hook = "onBitten")]
	public bool bitten;

	[SyncVar]
	public bool pulling;

	public GameObject sharkDummy;

	private bool castRpcCalled;

	public LayerMask waterLayer;

	private bool pullingFromPond;

	private bool pullingSparklingFish;

	private Coroutine rodControlRoutine;

	private bool charging;

	public float currentLineHealth = 1f;

	public int fullLineHealth = 1;

	public uint NetworkfishId
	{
		get
		{
			return fishId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref fishId))
			{
				uint num = fishId;
				SetSyncVar(value, ref fishId, 1uL);
			}
		}
	}

	public Vector3 NetworkreelPosition
	{
		get
		{
			return reelPosition;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref reelPosition))
			{
				Vector3 vector = reelPosition;
				SetSyncVar(value, ref reelPosition, 2uL);
			}
		}
	}

	public int NetworkfishOnLine
	{
		get
		{
			return fishOnLine;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref fishOnLine))
			{
				int old = fishOnLine;
				SetSyncVar(value, ref fishOnLine, 4uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(4uL))
				{
					setSyncVarHookGuard(4uL, value: true);
					FishOnLineChange(old, value);
					setSyncVarHookGuard(4uL, value: false);
				}
			}
		}
	}

	public bool Networkbitten
	{
		get
		{
			return bitten;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref bitten))
			{
				bool old = bitten;
				SetSyncVar(value, ref bitten, 8uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(8uL))
				{
					setSyncVarHookGuard(8uL, value: true);
					onBitten(old, value);
					setSyncVarHookGuard(8uL, value: false);
				}
			}
		}
	}

	public bool Networkpulling
	{
		get
		{
			return pulling;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref pulling))
			{
				bool flag = pulling;
				SetSyncVar(value, ref pulling, 16uL);
			}
		}
	}

	private void Start()
	{
		myEquip = GetComponent<EquipItemToChar>();
		myChar = GetComponent<CharMovement>();
	}

	[Command]
	public void CmdChangeReelPos(Vector3 newPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(newPos);
		SendCommandInternal(typeof(NetworkFishingRod), "CmdChangeReelPos", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdHookFish()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(NetworkFishingRod), "CmdHookFish", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcSetSparkling()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(NetworkFishingRod), "RpcSetSparkling", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public void HookShark(uint sharkId)
	{
		NetworkfishId = sharkId;
		NetworkfishOnLine = -2;
		Quaternion rotation = NetworkIdentity.spawned[fishId].transform.rotation;
		fishConnectedAi = NetworkIdentity.spawned[fishId].GetComponent<AnimalAI>();
		NetworkNavMesh.nav.UnSpawnAnAnimal(fishConnectedAi, saveToMap: false);
		StartCoroutine(fishStruggle());
		RpcFishBiteOnToLine(rotation, -2);
	}

	[Command]
	public void CmdScareFish()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(NetworkFishingRod), "CmdScareFish", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdCastLine(Vector3 castPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(castPos);
		SendCommandInternal(typeof(NetworkFishingRod), "CmdCastLine", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdBreakLine()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(NetworkFishingRod), "CmdBreakLine", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdCancleCast()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(NetworkFishingRod), "CmdCancleCast", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdCompleteReel()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(NetworkFishingRod), "CmdCompleteReel", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcDoCast()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(NetworkFishingRod), "RpcDoCast", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcCompleteReel()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(NetworkFishingRod), "RpcCompleteReel", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcFishBiteOnToLine(Quaternion dummyRotation, int fishId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteQuaternion(dummyRotation);
		writer.WriteInt(fishId);
		SendRPCInternal(typeof(NetworkFishingRod), "RpcFishBiteOnToLine", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcFishBreaksLine()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(NetworkFishingRod), "RpcFishBreaksLine", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public void FishOnLineChange(int old, int newFishId)
	{
		NetworkfishOnLine = newFishId;
		if ((bool)connectedFishingRod && newFishId == -1)
		{
			connectedFishingRod.spawnFishDummy(fishOnLine, Quaternion.identity);
		}
	}

	public void biteLine(uint fishBitingId)
	{
		if (base.isServer && !bitten)
		{
			NetworkfishId = fishBitingId;
			StartCoroutine(onBiteTimer());
		}
	}

	public void fakeBitLine()
	{
		if ((bool)connectedFishingRod)
		{
			connectedFishingRod.fakeBiteBobber();
		}
	}

	public void onBitten(bool old, bool newBitten)
	{
		Networkbitten = newBitten;
		if (base.isLocalPlayer && newBitten)
		{
			InputMaster.input.doRumble(0.5f, 2f);
			StartCoroutine(onLocalBiteTimer());
		}
		else if (newBitten)
		{
			connectedFishingRod.biteBobber();
		}
	}

	private void Update()
	{
		if (!base.isServer || pullingFromPond)
		{
			return;
		}
		if (pulling && fishOnLine != -1)
		{
			float num = 1f;
			num = ((fishOnLine < 0) ? 4f : (num + (float)Inventory.Instance.allItems[fishOnLine].fish.mySeason.myRarity / 2f));
			Vector3 normalized = (reelPosition - base.transform.root.position).normalized;
			Vector3 sourcePosition = reelPosition + normalized * num * Time.deltaTime;
			sourcePosition.y = 0.6f;
			if (NavMesh.SamplePosition(sourcePosition, out var hit, 4f, fishConnectedAi.myAgent.areaMask))
			{
				NetworkreelPosition = new Vector3(hit.position.x, -0.4f, hit.position.z);
			}
		}
		else if (fishOnLine != -1 && !myEquip.usingItem)
		{
			float num2 = 1f;
			num2 = ((fishOnLine < 0) ? 4f : (num2 + (float)Inventory.Instance.allItems[fishOnLine].fish.mySeason.myRarity / 2f));
			Vector3 normalized2 = (reelPosition - base.transform.root.position).normalized;
			Vector3 sourcePosition2 = reelPosition + normalized2 * num2 * Time.deltaTime;
			sourcePosition2.y = 0.6f;
			if (NavMesh.SamplePosition(sourcePosition2, out var hit2, 4f, fishConnectedAi.myAgent.areaMask))
			{
				NetworkreelPosition = new Vector3(hit2.position.x, -0.4f, hit2.position.z);
			}
		}
	}

	public float getFishOnLineDepthDifference()
	{
		if (fishOnLine > -1)
		{
			return 0f - Mathf.Clamp(Inventory.Instance.allItems[fishOnLine].transform.localScale.y, 1f, 3f);
		}
		return -0.4f;
	}

	private IEnumerator onDragScare()
	{
		connectedFishingRod.bobber.tag = "Scare";
		yield return new WaitForSeconds(1f);
		if ((bool)connectedFishingRod)
		{
			connectedFishingRod.bobber.tag = "Untagged";
		}
	}

	private IEnumerator onMissCatch()
	{
		connectedFishingRod.bobber.tag = "Scare";
		yield return new WaitForSeconds(5f);
		if ((bool)connectedFishingRod)
		{
			connectedFishingRod.bobber.tag = "Untagged";
		}
	}

	private IEnumerator onBiteTimer()
	{
		Networkbitten = true;
		yield return new WaitForSeconds(0.25f);
		Networkbitten = false;
	}

	private IEnumerator onLocalBiteTimer()
	{
		connectedFishingRod.biteBobber();
		for (float timeToCatch = 0f; timeToCatch <= 0.45f; timeToCatch += Time.deltaTime)
		{
			yield return null;
			if (myChar.localUsing)
			{
				CmdHookFish();
				InputMaster.input.doRumble(0.4f);
				break;
			}
		}
		CmdScareFish();
	}

	private IEnumerator fishStruggle()
	{
		Networkpulling = true;
		int num = 26000;
		float nonPullTimeAddition = 0.6f;
		if (fishOnLine >= 0)
		{
			num = Inventory.Instance.allItems[fishOnLine].value;
			nonPullTimeAddition = 3f / (1f + (float)Inventory.Instance.allItems[fishOnLine].fish.mySeason.myRarity);
		}
		float maxPullTime = Mathf.Clamp(num / 2000, 0.5f, 2.5f);
		float timeAddToNonPull = 0f;
		Networkpulling = true;
		float firstPullTime = 4f - Mathf.Clamp(Vector3.Distance(reelPosition, base.transform.position) / 4f, 0.5f, 3f);
		while (firstPullTime > 0f)
		{
			yield return null;
			firstPullTime -= Time.deltaTime;
			if (Vector3.Distance(reelPosition, base.transform.position) > 30f)
			{
				break;
			}
		}
		while (fishOnLine != -1)
		{
			Networkpulling = !pulling;
			if (pulling)
			{
				Networkpulling = true;
				yield return new WaitForSeconds(Random.Range(1f, 1.4f) + maxPullTime);
				maxPullTime = Mathf.Clamp(maxPullTime / 1.5f, 0f, 400f);
			}
			else
			{
				yield return new WaitForSeconds(Random.Range(1.4f, 2f) + timeAddToNonPull);
				timeAddToNonPull += nonPullTimeAddition;
			}
		}
		Networkpulling = false;
	}

	[ClientRpc]
	private void RpcFakeBite()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(NetworkFishingRod), "RpcFakeBite", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public override void OnStopClient()
	{
		if ((bool)connectedFishingRod && (bool)connectedFishingRod.bobberDummy)
		{
			Object.Destroy(connectedFishingRod.bobberDummy);
		}
	}

	public void startControls()
	{
		if (base.isLocalPlayer)
		{
			if (rodControlRoutine != null)
			{
				StopCoroutine(rodControlRoutine);
			}
			rodControlRoutine = StartCoroutine(controllRod());
		}
	}

	public void stopControls()
	{
		if (base.isLocalPlayer && rodControlRoutine != null)
		{
			castRpcCalled = false;
			StopCoroutine(rodControlRoutine);
		}
	}

	public void castLine()
	{
	}

	public void startCastCharge()
	{
		charging = true;
	}

	private IEnumerator controllRod()
	{
		while (true)
		{
			if (Inventory.Instance.CanMoveCharacter())
			{
				yield return null;
			}
			if (charging)
			{
				while (charging)
				{
					myChar.attackLockOn(isOn: true);
					myChar.moveLockRotateSlowOn(isOn: true);
					float chargeDistance = 5f;
					connectedFishingRod.setDistance(chargeDistance);
					connectedFishingRod.chargeSounds.Play();
					while (myChar.localUsing)
					{
						chargeDistance = Mathf.Clamp(chargeDistance + Time.deltaTime * 7.5f, 5f, 20f);
						if (chargeDistance >= 20f)
						{
							connectedFishingRod.chargeSounds.Stop();
						}
						connectedFishingRod.setDistance(chargeDistance);
						yield return null;
					}
					charging = false;
					lineIsCasted = true;
					connectedFishingRod.chargeSounds.Stop();
					myChar.moveLockRotateSlowOn(isOn: false);
					pullingFromPond = false;
					pullingSparklingFish = false;
					float timer = 0f;
					while (timer < 0.25f)
					{
						timer += Time.deltaTime;
						yield return null;
					}
					if ((bool)connectedFishingRod)
					{
						connectedFishingRod.castLine();
					}
					while (!castRpcCalled)
					{
						yield return null;
					}
					castRpcCalled = false;
					yield return new WaitForSeconds(0.35f);
				}
			}
			else if (lineIsCasted)
			{
				float timer = connectedFishingRod.reelSpeed;
				fullLineHealth = connectedFishingRod.maxLineHealth;
				currentLineHealth = fullLineHealth;
				FishingLineHealthBar.bar.showHealthbar();
				float chargeDistance = 0.35f;
				float nonPullGracePeriod = 0.6f;
				float pondTimer = 0f;
				while (lineIsCasted)
				{
					if ((!pullingFromPond || (pullingFromPond && fishOnLine == -1)) && (InputMaster.input.UICancel() || InputMaster.input.drop() || (Inventory.Instance.usingMouse && InputMaster.input.Interact())))
					{
						MenuButtonsTop.menu.closed = false;
						MenuButtonsTop.menu.closeButtonDelay();
						connectedFishingRod.cancelButton();
					}
					if (!pullingFromPond && isCastCompleted() && BobberInFishPond(out var PondXAndY))
					{
						if (pondTimer > 0.55f)
						{
							if (Inventory.Instance.CheckIfFishOrBugCanFit())
							{
								CmdReelInFishFromPond((int)PondXAndY.x, (int)PondXAndY.y);
							}
							else
							{
								NotificationManager.manage.turnOnPocketsFullNotification();
							}
							pullingFromPond = true;
						}
						pondTimer += Time.deltaTime;
					}
					else if (pullingFromPond && fishOnLine >= 0)
					{
						if (pondTimer > 1f)
						{
							lineIsCasted = false;
							CmdCompleteReel();
						}
						else
						{
							pondTimer += Time.deltaTime;
						}
					}
					else if (myChar.localUsing)
					{
						timer += Time.deltaTime;
						if (timer >= connectedFishingRod.reelSpeed)
						{
							connectedFishingRod.reelIn();
							timer = 0f;
						}
						if (pulling)
						{
							if (chargeDistance > 0f)
							{
								chargeDistance -= Time.deltaTime;
							}
							else
							{
								float num = 3f;
								if (fishOnLine >= 0)
								{
									num = Vector3.Distance(Vector3.zero, Inventory.Instance.allItems[fishOnLine].transform.localScale) * (float)(Inventory.Instance.allItems[fishOnLine].fish.mySeason.myRarity + 1) / 2f;
								}
								currentLineHealth -= Time.deltaTime * num;
								InputMaster.input.doRumble(0.6f);
								FishingLineHealthBar.bar.shakeBar();
								FishingLineHealthBar.bar.playSound();
							}
						}
						else
						{
							FishingLineHealthBar.bar.stopSound();
							chargeDistance = 0.35f;
						}
						if (currentLineHealth <= 0f)
						{
							connectedFishingRod.breakFishOff();
						}
						nonPullGracePeriod = 0.6f;
					}
					else
					{
						if (chargeDistance < 0.35f)
						{
							chargeDistance += Time.deltaTime / 2f;
						}
						timer = connectedFishingRod.reelSpeed;
						if (fishOnLine != -1 && !pulling)
						{
							nonPullGracePeriod -= Time.deltaTime;
							if (nonPullGracePeriod <= 0f)
							{
								currentLineHealth -= Time.deltaTime / 4f;
								FishingLineHealthBar.bar.shakeBar();
								InputMaster.input.doRumble(0.3f);
							}
							if (currentLineHealth <= 0f)
							{
								connectedFishingRod.breakFishOff();
							}
						}
						else
						{
							nonPullGracePeriod = 0.6f;
						}
						FishingLineHealthBar.bar.stopSound();
					}
					yield return null;
				}
				myChar.attackLockOn(isOn: false);
			}
			yield return null;
		}
	}

	public bool WasPulledFromPond()
	{
		return pullingFromPond;
	}

	public bool WasLastCaughtSparkling()
	{
		return pullingSparklingFish;
	}

	public void spawnFakeShark(float rotationY)
	{
		if (base.isLocalPlayer)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.SharkHunter);
			CmdSpawnSharkAtPos(rotationY);
		}
	}

	[Command]
	public void CmdReelInFishFromPond(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(NetworkFishingRod), "CmdReelInFishFromPond", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSpawnSharkAtPos(float rotationY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteFloat(rotationY);
		SendCommandInternal(typeof(NetworkFishingRod), "CmdSpawnSharkAtPos", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator stunSharkAfterLand(Damageable damage)
	{
		yield return null;
		damage.stun();
	}

	public bool isCastCompleted()
	{
		if ((bool)connectedFishingRod && connectedFishingRod.isCastCompleted())
		{
			return true;
		}
		return false;
	}

	public bool BobberInFishPond(out Vector2 PondXAndY)
	{
		if (Physics.Raycast(reelPosition + Vector3.up / 2f, Vector3.down, out var hitInfo, 25f, waterLayer) && hitInfo.transform.CompareTag("FishPond"))
		{
			TileObject componentInParent = hitInfo.transform.GetComponentInParent<TileObject>();
			PondXAndY = new Vector2(componentInParent.xPos, componentInParent.yPos);
			return true;
		}
		PondXAndY = Vector2.zero;
		return false;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdChangeReelPos(Vector3 newPos)
	{
		if (fishOnLine != -1)
		{
			newPos += Vector3.down / 1.5f;
		}
		StartCoroutine(onDragScare());
		NetworkreelPosition = newPos;
	}

	protected static void InvokeUserCode_CmdChangeReelPos(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeReelPos called on client.");
		}
		else
		{
			((NetworkFishingRod)obj).UserCode_CmdChangeReelPos(reader.ReadVector3());
		}
	}

	protected void UserCode_CmdHookFish()
	{
		FishType component = NetworkIdentity.spawned[fishId].GetComponent<FishType>();
		NetworkfishOnLine = component.getFishTypeId();
		if (component.IsSparkling())
		{
			RpcSetSparkling();
		}
		Quaternion rotation = NetworkIdentity.spawned[fishId].transform.rotation;
		fishConnectedAi = NetworkIdentity.spawned[fishId].GetComponent<AnimalAI>();
		NetworkNavMesh.nav.UnSpawnAnAnimal(fishConnectedAi, saveToMap: false);
		StartCoroutine(fishStruggle());
		RpcFishBiteOnToLine(rotation, fishOnLine);
	}

	protected static void InvokeUserCode_CmdHookFish(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHookFish called on client.");
		}
		else
		{
			((NetworkFishingRod)obj).UserCode_CmdHookFish();
		}
	}

	protected void UserCode_RpcSetSparkling()
	{
		pullingSparklingFish = true;
	}

	protected static void InvokeUserCode_RpcSetSparkling(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetSparkling called on server.");
		}
		else
		{
			((NetworkFishingRod)obj).UserCode_RpcSetSparkling();
		}
	}

	protected void UserCode_CmdScareFish()
	{
		StartCoroutine(onMissCatch());
	}

	protected static void InvokeUserCode_CmdScareFish(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdScareFish called on client.");
		}
		else
		{
			((NetworkFishingRod)obj).UserCode_CmdScareFish();
		}
	}

	protected void UserCode_CmdCastLine(Vector3 castPos)
	{
		pullingFromPond = false;
		pullingSparklingFish = false;
		NetworkreelPosition = castPos;
		RpcDoCast();
	}

	protected static void InvokeUserCode_CmdCastLine(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCastLine called on client.");
		}
		else
		{
			((NetworkFishingRod)obj).UserCode_CmdCastLine(reader.ReadVector3());
		}
	}

	protected void UserCode_CmdBreakLine()
	{
		if (fishOnLine != -1)
		{
			NetworkNavMesh.nav.spawnAFishBreakFree(fishOnLine, connectedFishingRod.bobber.position, connectedFishingRod.bobber.transform);
		}
		RpcFishBreaksLine();
		Networkpulling = false;
		NetworkfishOnLine = -1;
		fishConnectedAi = null;
		NetworkfishId = 0u;
	}

	protected static void InvokeUserCode_CmdBreakLine(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdBreakLine called on client.");
		}
		else
		{
			((NetworkFishingRod)obj).UserCode_CmdBreakLine();
		}
	}

	protected void UserCode_CmdCancleCast()
	{
		connectedFishingRod.cancelCastCompleteOnServer();
		if (fishOnLine != -1)
		{
			NetworkNavMesh.nav.spawnAFishBreakFree(fishOnLine, connectedFishingRod.bobber.position, connectedFishingRod.bobber.transform);
			NetworkfishOnLine = -1;
			RpcFishBreaksLine();
		}
		RpcCompleteReel();
	}

	protected static void InvokeUserCode_CmdCancleCast(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCancleCast called on client.");
		}
		else
		{
			((NetworkFishingRod)obj).UserCode_CmdCancleCast();
		}
	}

	protected void UserCode_CmdCompleteReel()
	{
		RpcCompleteReel();
		NetworkfishOnLine = -1;
		fishConnectedAi = null;
		NetworkfishId = 0u;
	}

	protected static void InvokeUserCode_CmdCompleteReel(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCompleteReel called on client.");
		}
		else
		{
			((NetworkFishingRod)obj).UserCode_CmdCompleteReel();
		}
	}

	protected void UserCode_RpcDoCast()
	{
		castRpcCalled = true;
		connectedFishingRod.doCast();
	}

	protected static void InvokeUserCode_RpcDoCast(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDoCast called on server.");
		}
		else
		{
			((NetworkFishingRod)obj).UserCode_RpcDoCast();
		}
	}

	protected void UserCode_RpcCompleteReel()
	{
		if ((bool)connectedFishingRod)
		{
			connectedFishingRod.completeReel();
		}
	}

	protected static void InvokeUserCode_RpcCompleteReel(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCompleteReel called on server.");
		}
		else
		{
			((NetworkFishingRod)obj).UserCode_RpcCompleteReel();
		}
	}

	protected void UserCode_RpcFishBiteOnToLine(Quaternion dummyRotation, int fishId)
	{
		ParticleManager.manage.emitAttackParticle(connectedFishingRod.bobber.position);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.waterParticle, connectedFishingRod.bobber.position + Vector3.up / 4f);
		SoundManager.Instance.playASoundAtPoint(fishLatch, connectedFishingRod.bobber.position);
		if ((bool)connectedFishingRod)
		{
			connectedFishingRod.spawnFishDummy(fishId, dummyRotation);
		}
	}

	protected static void InvokeUserCode_RpcFishBiteOnToLine(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcFishBiteOnToLine called on server.");
		}
		else
		{
			((NetworkFishingRod)obj).UserCode_RpcFishBiteOnToLine(reader.ReadQuaternion(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcFishBreaksLine()
	{
		if ((bool)connectedFishingRod)
		{
			connectedFishingRod.removeDummyFish();
		}
		SoundManager.Instance.playASoundAtPoint(fishLatch, connectedFishingRod.bobber.position);
		SoundManager.Instance.playASoundAtPoint(fishBloop, connectedFishingRod.bobber.position);
		ParticleManager.manage.emitAttackParticle(connectedFishingRod.bobber.position);
		ParticleManager.manage.emitRedAttackParticle(connectedFishingRod.bobber.position);
	}

	protected static void InvokeUserCode_RpcFishBreaksLine(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcFishBreaksLine called on server.");
		}
		else
		{
			((NetworkFishingRod)obj).UserCode_RpcFishBreaksLine();
		}
	}

	protected void UserCode_RpcFakeBite()
	{
		connectedFishingRod.fakeBiteBobber();
	}

	protected static void InvokeUserCode_RpcFakeBite(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcFakeBite called on server.");
		}
		else
		{
			((NetworkFishingRod)obj).UserCode_RpcFakeBite();
		}
	}

	protected void UserCode_CmdReelInFishFromPond(int xPos, int yPos)
	{
		pullingFromPond = true;
		int firstFishFromPond = ContainerManager.manage.GetFirstFishFromPond(xPos, yPos);
		if (firstFishFromPond != -1)
		{
			NetworkfishOnLine = firstFishFromPond;
			RpcFishBiteOnToLine(Quaternion.identity, firstFishFromPond);
		}
	}

	protected static void InvokeUserCode_CmdReelInFishFromPond(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdReelInFishFromPond called on client.");
		}
		else
		{
			((NetworkFishingRod)obj).UserCode_CmdReelInFishFromPond(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdSpawnSharkAtPos(float rotationY)
	{
		AnimalAI animalAI = AnimalManager.manage.spawnFreeAnimal(5, base.transform.position);
		animalAI.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
		NetworkServer.Spawn(animalAI.gameObject);
		StartCoroutine(stunSharkAfterLand(animalAI.GetComponent<Damageable>()));
	}

	protected static void InvokeUserCode_CmdSpawnSharkAtPos(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnSharkAtPos called on client.");
		}
		else
		{
			((NetworkFishingRod)obj).UserCode_CmdSpawnSharkAtPos(reader.ReadFloat());
		}
	}

	static NetworkFishingRod()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkFishingRod), "CmdChangeReelPos", InvokeUserCode_CmdChangeReelPos, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkFishingRod), "CmdHookFish", InvokeUserCode_CmdHookFish, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkFishingRod), "CmdScareFish", InvokeUserCode_CmdScareFish, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkFishingRod), "CmdCastLine", InvokeUserCode_CmdCastLine, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkFishingRod), "CmdBreakLine", InvokeUserCode_CmdBreakLine, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkFishingRod), "CmdCancleCast", InvokeUserCode_CmdCancleCast, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkFishingRod), "CmdCompleteReel", InvokeUserCode_CmdCompleteReel, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkFishingRod), "CmdReelInFishFromPond", InvokeUserCode_CmdReelInFishFromPond, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkFishingRod), "CmdSpawnSharkAtPos", InvokeUserCode_CmdSpawnSharkAtPos, requiresAuthority: true);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkFishingRod), "RpcSetSparkling", InvokeUserCode_RpcSetSparkling);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkFishingRod), "RpcDoCast", InvokeUserCode_RpcDoCast);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkFishingRod), "RpcCompleteReel", InvokeUserCode_RpcCompleteReel);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkFishingRod), "RpcFishBiteOnToLine", InvokeUserCode_RpcFishBiteOnToLine);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkFishingRod), "RpcFishBreaksLine", InvokeUserCode_RpcFishBreaksLine);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkFishingRod), "RpcFakeBite", InvokeUserCode_RpcFakeBite);
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteUInt(fishId);
			writer.WriteVector3(reelPosition);
			writer.WriteInt(fishOnLine);
			writer.WriteBool(bitten);
			writer.WriteBool(pulling);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteUInt(fishId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteVector3(reelPosition);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteInt(fishOnLine);
			result = true;
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteBool(bitten);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			writer.WriteBool(pulling);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			uint num = fishId;
			NetworkfishId = reader.ReadUInt();
			Vector3 vector = reelPosition;
			NetworkreelPosition = reader.ReadVector3();
			int num2 = fishOnLine;
			NetworkfishOnLine = reader.ReadInt();
			if (!SyncVarEqual(num2, ref fishOnLine))
			{
				FishOnLineChange(num2, fishOnLine);
			}
			bool flag = bitten;
			Networkbitten = reader.ReadBool();
			if (!SyncVarEqual(flag, ref bitten))
			{
				onBitten(flag, bitten);
			}
			bool flag2 = pulling;
			Networkpulling = reader.ReadBool();
			return;
		}
		long num3 = (long)reader.ReadULong();
		if ((num3 & 1L) != 0L)
		{
			uint num4 = fishId;
			NetworkfishId = reader.ReadUInt();
		}
		if ((num3 & 2L) != 0L)
		{
			Vector3 vector2 = reelPosition;
			NetworkreelPosition = reader.ReadVector3();
		}
		if ((num3 & 4L) != 0L)
		{
			int num5 = fishOnLine;
			NetworkfishOnLine = reader.ReadInt();
			if (!SyncVarEqual(num5, ref fishOnLine))
			{
				FishOnLineChange(num5, fishOnLine);
			}
		}
		if ((num3 & 8L) != 0L)
		{
			bool flag3 = bitten;
			Networkbitten = reader.ReadBool();
			if (!SyncVarEqual(flag3, ref bitten))
			{
				onBitten(flag3, bitten);
			}
		}
		if ((num3 & 0x10L) != 0L)
		{
			bool flag4 = pulling;
			Networkpulling = reader.ReadBool();
		}
	}
}
