using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class NPCHoldsItems : NetworkBehaviour
{
	private GameObject holdingPrefab;

	[SyncVar(hook = "equipNewItemNetwork")]
	private int currentlyHoldingNo = -1;

	public InventoryItem currentlyHolding;

	public Transform holdPos;

	public Animator holdingPrefabAnimator;

	private Transform leftHandPos;

	private Transform rightHandPos;

	public Transform rightHandHoldPos;

	public Transform rightHandMoveHitboxTo;

	public Transform lookable;

	public float lookingWeight;

	private float lefthandWeight = 1f;

	private Animator myAnim;

	private bool lookLock;

	[SyncVar(hook = "OnUsingChange")]
	public bool usingItem;

	[SyncVar(hook = "onAttackPosChange")]
	public Vector2 currentlyTargetingPos = Vector2.zero;

	private TileObject objectAttacking;

	public InventoryItem torchItem;

	public InventoryItem partyHorn;

	private ToolDoesDamage useTool;

	private MeleeAttacks toolWeapon;

	private static Vector3 dif;

	public int NetworkcurrentlyHoldingNo
	{
		get
		{
			return currentlyHoldingNo;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref currentlyHoldingNo))
			{
				int oldItem = currentlyHoldingNo;
				SetSyncVar(value, ref currentlyHoldingNo, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					equipNewItemNetwork(oldItem, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public bool NetworkusingItem
	{
		get
		{
			return usingItem;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref usingItem))
			{
				bool old = usingItem;
				SetSyncVar(value, ref usingItem, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, value: true);
					OnUsingChange(old, value);
					setSyncVarHookGuard(2uL, value: false);
				}
			}
		}
	}

	public Vector2 NetworkcurrentlyTargetingPos
	{
		get
		{
			return currentlyTargetingPos;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref currentlyTargetingPos))
			{
				Vector2 oldAttack = currentlyTargetingPos;
				SetSyncVar(value, ref currentlyTargetingPos, 4uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(4uL))
				{
					setSyncVarHookGuard(4uL, value: true);
					onAttackPosChange(oldAttack, value);
					setSyncVarHookGuard(4uL, value: false);
				}
			}
		}
	}

	private void Awake()
	{
		myAnim = GetComponent<Animator>();
	}

	private void Start()
	{
		myAnim = GetComponent<Animator>();
	}

	public override void OnStartClient()
	{
		myAnim = GetComponent<Animator>();
		equipNewItemNetwork(currentlyHoldingNo, currentlyHoldingNo);
	}

	public override void OnStartServer()
	{
		myAnim = GetComponent<Animator>();
		equipNewItemNetwork(currentlyHoldingNo, currentlyHoldingNo);
		StartCoroutine(trapperCarriesTorchAtNight());
	}

	private IEnumerator trapperCarriesTorchAtNight()
	{
		yield return null;
		yield return null;
		yield return null;
		if (GetComponent<NPCIdentity>().NPCNo != 5)
		{
			yield break;
		}
		while (true)
		{
			yield return null;
			if (RealWorldTimeLight.time.currentHour > RealWorldTimeLight.time.getSunSetTime() && currentlyHoldingNo == -1)
			{
				NetworkcurrentlyHoldingNo = torchItem.getItemId();
			}
		}
	}

	private void Update()
	{
		animateOnUse(usingItem);
	}

	public void OnUsingChange(bool old, bool newUsing)
	{
		NetworkusingItem = newUsing;
		if ((bool)currentlyHolding)
		{
			myAnim.SetBool("Using", usingItem);
		}
		else
		{
			myAnim.SetBool("Using", value: false);
		}
	}

	public void changeItemHolding(int newNo)
	{
		NetworkcurrentlyHoldingNo = newNo;
		if (newNo == -1)
		{
			NetworkusingItem = false;
		}
	}

	private void equipNewItemNetwork(int oldItem, int inventoryItemNo)
	{
		myAnim.SetBool("TwoArms", value: false);
		StopCoroutine(lerpLeftHandWeight(1f));
		lefthandWeight = 1f;
		setLeftHandWeight(1f);
		NetworkcurrentlyHoldingNo = inventoryItemNo;
		if ((bool)holdingPrefab || ((bool)holdingPrefab && currentlyHoldingNo == -1))
		{
			Object.Destroy(holdingPrefab);
			currentlyHolding = null;
			useTool = null;
			toolWeapon = null;
		}
		if (!currentlyHolding && inventoryItemNo != -1)
		{
			currentlyHolding = Inventory.Instance.allItems[inventoryItemNo];
			if (Inventory.Instance.allItems[inventoryItemNo].useRightHandAnim)
			{
				holdingPrefab = Object.Instantiate(Inventory.Instance.allItems[inventoryItemNo].itemPrefab, rightHandHoldPos);
				if (myAnim.GetInteger("HoldingTool") != (int)Inventory.Instance.allItems[inventoryItemNo].myAnimType)
				{
					myAnim.SetTrigger("ChangeItem");
				}
				myAnim.SetInteger("HoldingTool", (int)Inventory.Instance.allItems[inventoryItemNo].myAnimType);
				useTool = holdingPrefab.GetComponent<ToolDoesDamage>();
				if ((bool)useTool)
				{
					useTool.attachNPC(GetComponent<NPCDoesTasks>());
				}
				toolWeapon = holdingPrefab.GetComponent<MeleeAttacks>();
				if ((bool)toolWeapon)
				{
					toolWeapon.attachNPC(this);
				}
			}
			else
			{
				holdingPrefab = Object.Instantiate(Inventory.Instance.allItems[inventoryItemNo].itemPrefab, holdPos);
				myAnim.SetInteger("HoldingTool", -1);
			}
			SetItemTexture component = holdingPrefab.GetComponent<SetItemTexture>();
			if ((bool)component)
			{
				component.setTexture(Inventory.Instance.allItems[inventoryItemNo]);
				if ((bool)component.changeSize)
				{
					component.changeSizeOfTrans(Inventory.Instance.allItems[inventoryItemNo].transform.localScale);
				}
			}
			holdingPrefab.transform.localPosition = Vector3.zero;
			holdingPrefab.transform.localPosition = Vector3.zero;
			holdingPrefabAnimator = holdingPrefab.GetComponent<Animator>();
			leftHandPos = holdingPrefab.transform.Find("Animation/LeftHandle");
			rightHandPos = holdingPrefab.transform.Find("Animation/RightHandle");
			lookable = holdingPrefab.transform.Find("Animation/Lookable");
		}
		else if (inventoryItemNo == -1)
		{
			myAnim.SetInteger("HoldingTool", -1);
		}
	}

	public void animateOnUse(bool beingUsed)
	{
		if ((bool)holdingPrefabAnimator)
		{
			holdingPrefabAnimator.SetBool("Using", beingUsed);
			if (beingUsed || lookLock)
			{
				float b = 0f;
				if ((bool)lookable && (bool)currentlyHolding)
				{
					b = 0.05f;
				}
				lookingWeight = Mathf.Lerp(lookingWeight, b, Time.deltaTime * 5f);
			}
			else
			{
				lookingWeight = Mathf.Lerp(lookingWeight, 0f, Time.deltaTime * 5f);
			}
		}
		else
		{
			lookingWeight = Mathf.Lerp(lookingWeight, 0f, Time.deltaTime * 10f);
		}
	}

	public void setLeftHandWeight(float newWeight)
	{
		StopCoroutine("lerpLeftHandWeight");
		StartCoroutine(lerpLeftHandWeight(newWeight));
	}

	private IEnumerator lerpLeftHandWeight(float newLeftHandWeight)
	{
		float timer = 0f;
		while (timer < 1f)
		{
			lefthandWeight = Mathf.Lerp(lefthandWeight, newLeftHandWeight, timer);
			timer += Time.deltaTime * 2f;
			yield return null;
		}
	}

	private void OnAnimatorIK()
	{
		if ((bool)rightHandPos)
		{
			myAnim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
			myAnim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
			myAnim.SetIKPosition(AvatarIKGoal.RightHand, rightHandPos.position + dif);
			myAnim.SetIKRotation(AvatarIKGoal.RightHand, rightHandPos.rotation);
		}
		if ((bool)leftHandPos)
		{
			myAnim.SetIKPositionWeight(AvatarIKGoal.LeftHand, lefthandWeight);
			myAnim.SetIKRotationWeight(AvatarIKGoal.LeftHand, lefthandWeight);
			myAnim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandPos.position + dif);
			myAnim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandPos.rotation);
		}
	}

	private void onAttackPosChange(Vector2 oldAttack, Vector2 newAttackPos)
	{
		NetworkcurrentlyTargetingPos = newAttackPos;
		objectAttacking = WorldManager.Instance.findTileObjectInUse((int)currentlyTargetingPos.x, (int)currentlyTargetingPos.y);
	}

	public void catchBug(uint bugNetId)
	{
		NetworkcurrentlyHoldingNo = NetworkIdentity.spawned[bugNetId].GetComponent<BugTypes>().getBugTypeId();
		NetworkNavMesh.nav.UnSpawnAnAnimal(NetworkIdentity.spawned[bugNetId].GetComponent<AnimalAI>(), saveToMap: false);
		RpcCatchAndHoldBug();
	}

	[ClientRpc]
	public void RpcCatchAndHoldBug()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(NPCHoldsItems), "RpcCatchAndHoldBug", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator bugNetWait(int netId)
	{
		while (!currentlyHolding || !currentlyHolding.bug)
		{
			yield return null;
		}
		while (!holdingPrefabAnimator)
		{
			yield return null;
		}
		holdingPrefabAnimator.GetComponent<Animator>().SetTrigger("UseBugNet");
		holdingPrefab.transform.Find("Animation/LeftHandleNet/" + netId).gameObject.SetActive(value: true);
		leftHandPos = holdingPrefab.transform.Find("Animation/LeftHandleNet");
		myAnim.SetBool("TwoArms", value: true);
	}

	public void takeObjectGiven(int newId)
	{
	}

	public void doDamageNow()
	{
		if ((bool)holdingPrefab && (bool)useTool)
		{
			useTool.doDamageNow();
		}
	}

	public void checkRefill()
	{
		if ((bool)holdingPrefab && (bool)useTool)
		{
			useTool.checkRefill();
		}
	}

	public void playToolParticles()
	{
		if ((bool)holdingPrefab)
		{
			holdingPrefab.GetComponent<ActivateAnimationParticles>().emitParticles(20);
		}
	}

	public void playToolSound()
	{
		if ((bool)holdingPrefab)
		{
			holdingPrefab.GetComponent<ActivateAnimationParticles>().playSound();
		}
	}

	public void lookLockFrames(int frame)
	{
		if ((bool)toolWeapon)
		{
			toolWeapon.turnOnLookLockForFramesWithoutUsing(frame);
		}
	}

	public void startAttack()
	{
		if ((bool)toolWeapon)
		{
			toolWeapon.attack();
		}
	}

	public void lockPosForFrames(int frames)
	{
		if ((bool)toolWeapon)
		{
			toolWeapon.lockPosForFrames(frames);
		}
	}

	public void toolDoesDamageToolPosNo(int noToUse)
	{
		if ((bool)toolWeapon)
		{
			toolWeapon.toolDoesDamageToolPosNo(noToUse);
		}
	}

	public void makeSwingSound()
	{
		if ((bool)toolWeapon)
		{
			toolWeapon.playSwordSwingSound();
		}
	}

	public void playSwingPartsForFrames(int frames)
	{
		if ((bool)toolWeapon)
		{
			toolWeapon.playSwingPartsForFrames();
		}
	}

	public void checkForClang()
	{
		if ((bool)useTool && useTool.checkIfNeedClang())
		{
			useTool.playClangSound();
			myAnim.SetTrigger("Clang");
		}
	}

	static NPCHoldsItems()
	{
		dif = new Vector3(0f, -0.18f, 0f);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NPCHoldsItems), "RpcCatchAndHoldBug", InvokeUserCode_RpcCatchAndHoldBug);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcCatchAndHoldBug()
	{
		StartCoroutine(bugNetWait(GetComponent<NPCDoesTasks>().myBugNet.getItemId()));
	}

	protected static void InvokeUserCode_RpcCatchAndHoldBug(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCatchAndHoldBug called on server.");
		}
		else
		{
			((NPCHoldsItems)obj).UserCode_RpcCatchAndHoldBug();
		}
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(currentlyHoldingNo);
			writer.WriteBool(usingItem);
			writer.WriteVector2(currentlyTargetingPos);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(currentlyHoldingNo);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteBool(usingItem);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteVector2(currentlyTargetingPos);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = currentlyHoldingNo;
			NetworkcurrentlyHoldingNo = reader.ReadInt();
			if (!SyncVarEqual(num, ref currentlyHoldingNo))
			{
				equipNewItemNetwork(num, currentlyHoldingNo);
			}
			bool flag = usingItem;
			NetworkusingItem = reader.ReadBool();
			if (!SyncVarEqual(flag, ref usingItem))
			{
				OnUsingChange(flag, usingItem);
			}
			Vector2 vector = currentlyTargetingPos;
			NetworkcurrentlyTargetingPos = reader.ReadVector2();
			if (!SyncVarEqual(vector, ref currentlyTargetingPos))
			{
				onAttackPosChange(vector, currentlyTargetingPos);
			}
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			int num3 = currentlyHoldingNo;
			NetworkcurrentlyHoldingNo = reader.ReadInt();
			if (!SyncVarEqual(num3, ref currentlyHoldingNo))
			{
				equipNewItemNetwork(num3, currentlyHoldingNo);
			}
		}
		if ((num2 & 2L) != 0L)
		{
			bool flag2 = usingItem;
			NetworkusingItem = reader.ReadBool();
			if (!SyncVarEqual(flag2, ref usingItem))
			{
				OnUsingChange(flag2, usingItem);
			}
		}
		if ((num2 & 4L) != 0L)
		{
			Vector2 vector2 = currentlyTargetingPos;
			NetworkcurrentlyTargetingPos = reader.ReadVector2();
			if (!SyncVarEqual(vector2, ref currentlyTargetingPos))
			{
				onAttackPosChange(vector2, currentlyTargetingPos);
			}
		}
	}
}
