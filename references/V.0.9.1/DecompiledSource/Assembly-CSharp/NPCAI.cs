using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityChan;
using UnityEngine;
using UnityEngine.AI;

public class NPCAI : NetworkBehaviour
{
	[SyncVar(hook = "onTalkingChange")]
	public uint talkingTo;

	private Transform talkingToTransform;

	[SyncVar(hook = "onFollowChange")]
	public uint followingNetId;

	public NPCIdentity myId;

	public NPCMapAgent myManager;

	public Animator myAnim;

	public AnimateCharFace faceAnim;

	public bool grounded;

	public LayerMask jumpLayers;

	public LayerMask jumpOverLayers;

	public NavMeshAgent myAgent;

	public Transform following;

	public CharMovement followingChar;

	public NPCIk npcIk;

	public bool isSign;

	private bool inJump;

	private Quaternion lastRotation;

	private NPCJob myJob;

	private Vector3 lastPos;

	private Vector3 lastPosAnim;

	public AudioSource myAudio;

	private int baseSpeed = 3;

	private bool inWater;

	public NPCDoesTasks doesTask;

	private bool canWalkOnEntrance;

	private bool isCloseEnoughToFollowing;

	public SpringManager hairSpring;

	public NPCChatBubble chatBubble;

	[SyncVar(hook = "OnSittingChanged")]
	public bool isSitting;

	private int roundedX;

	private int roundedY;

	private Vector3 lastNavPos;

	private RaycastHit hit;

	private float hairVel;

	private float travelSpeed;

	private Coroutine gateRoutine;

	public static WaitForSeconds npcWait;

	private static NavMeshPathStatus cantComplete;

	private int seatPos;

	private int seatX = -1;

	private int seatY = -1;

	public uint NetworktalkingTo
	{
		get
		{
			return talkingTo;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref talkingTo))
			{
				uint old = talkingTo;
				SetSyncVar(value, ref talkingTo, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					onTalkingChange(old, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public uint NetworkfollowingNetId
	{
		get
		{
			return followingNetId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref followingNetId))
			{
				uint oldFollow = followingNetId;
				SetSyncVar(value, ref followingNetId, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, value: true);
					onFollowChange(oldFollow, value);
					setSyncVarHookGuard(2uL, value: false);
				}
			}
		}
	}

	public bool NetworkisSitting
	{
		get
		{
			return isSitting;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref isSitting))
			{
				bool oldSitting = isSitting;
				SetSyncVar(value, ref isSitting, 4uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(4uL))
				{
					setSyncVarHookGuard(4uL, value: true);
					OnSittingChanged(oldSitting, value);
					setSyncVarHookGuard(4uL, value: false);
				}
			}
		}
	}

	private void Start()
	{
		roundedX = Mathf.RoundToInt(base.transform.position.x / 2f);
		roundedY = Mathf.RoundToInt(base.transform.position.z / 2f);
	}

	public override void OnStartServer()
	{
		doesTask = GetComponent<NPCDoesTasks>();
		NetworkfollowingNetId = 0u;
		NetworktalkingTo = 0u;
		if ((bool)myAgent)
		{
			myAgent.updateRotation = true;
			myAgent.transform.parent = null;
			StartCoroutine(startNPC());
		}
		if (!isSign)
		{
			NetworkMapSharer.Instance.returnAgents.AddListener(despawnOnEvent);
		}
		if (myManager != null && myManager.getFollowingId() != 0)
		{
			NetworkfollowingNetId = myManager.getFollowingId();
			onFollowChange(followingNetId, followingNetId);
		}
		NetworkisSitting = false;
	}

	private void OnEnable()
	{
		grounded = true;
		if (!isSign)
		{
			myAnim.SetBool(CharNetworkAnimator.groundedAnimName, grounded);
		}
	}

	private void Awake()
	{
		myJob = GetComponent<NPCJob>();
		lastPos = base.transform.position;
	}

	public void OnDisable()
	{
		if (base.isServer)
		{
			if (isSitting && seatX != -1 && seatY != -1)
			{
				NetworkMapSharer.Instance.RpcGetUp(Mathf.Clamp(WorldManager.Instance.onTileStatusMap[seatX, seatY] - seatPos, 0, 3), seatX, seatY, -1, -1);
			}
			StopAllCoroutines();
			if ((bool)myAgent)
			{
				myAgent.enabled = false;
			}
			NetworkMapSharer.Instance.returnAgents.RemoveListener(despawnOnEvent);
		}
	}

	public bool isAtWork()
	{
		return myJob.atWork;
	}

	public override void OnStartClient()
	{
		if (!isSign)
		{
			doesTask = GetComponent<NPCDoesTasks>();
			lastRotation = base.transform.rotation;
			if (!base.isServer)
			{
				Object.Destroy(myAgent.gameObject);
			}
			OnSittingChanged(isSitting, isSitting);
		}
	}

	public void moveSpawnSpot(Vector3 newSpawnPos)
	{
		StopAllCoroutines();
		myAgent.enabled = false;
		myAgent.transform.position = newSpawnPos;
		base.transform.position = newSpawnPos;
		StartCoroutine(startNPC());
	}

	private IEnumerator SolveStuck()
	{
		while (true)
		{
			if (!checkIfHasArrivedAtDestination() && Vector3.Distance(lastNavPos, base.transform.position) < 0.1f)
			{
				Vector3 destination = myAgent.destination;
				myAgent.SetDestination(base.transform.position + base.transform.forward + new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(-8f, 8f)));
				yield return new WaitForSeconds(3f);
				myAgent.ResetPath();
				myAgent.SetDestination(destination);
			}
			lastNavPos = base.transform.position;
			yield return new WaitForSeconds(1f);
		}
	}

	private void LateUpdate()
	{
		animateNPC();
		if (!isSitting && Physics.Raycast(base.transform.position + Vector3.up, Vector3.down, out hit, 2f, jumpLayers))
		{
			base.transform.position = Vector3.Lerp(base.transform.position, new Vector3(base.transform.position.x, hit.point.y, base.transform.position.z), Time.deltaTime * 2f);
			lastPos = base.transform.position;
		}
	}

	private void animateNPC()
	{
		if (isSign)
		{
			return;
		}
		if (Vector3.Distance(base.transform.position, lastPos) >= 8f)
		{
			lastPos = base.transform.position;
		}
		float num = Vector3.Distance(base.transform.position, lastPosAnim);
		if (Time.deltaTime != 0f)
		{
			num /= Time.deltaTime;
		}
		if (num / 6f * 2f < 0.1f && Quaternion.Angle(base.transform.rotation, lastRotation) > 0.2f)
		{
			num = 2.5f;
		}
		if (Vector3.Distance(base.transform.position, lastPosAnim) >= 4f)
		{
			travelSpeed = 0f;
		}
		else if (num > 0.15f)
		{
			travelSpeed = Mathf.Lerp(travelSpeed, num, Time.deltaTime * 6f);
		}
		else
		{
			travelSpeed = Mathf.Lerp(travelSpeed, num, Time.deltaTime * 3f);
		}
		if (base.transform.position.y > -10f && base.transform.position.y < 1f && WorldManager.Instance.waterMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)] && WorldManager.Instance.heightMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)] < 0)
		{
			myAnim.SetBool(CharNetworkAnimator.swimmingAnimName, value: true);
		}
		else
		{
			myAnim.SetBool(CharNetworkAnimator.swimmingAnimName, value: false);
		}
		lastPosAnim = base.transform.position;
		if ((bool)hairSpring)
		{
			if (travelSpeed > 0.5f)
			{
				hairSpring.dynamicRatio = Mathf.SmoothDamp(hairSpring.dynamicRatio, Mathf.Clamp(1f - num, 0.15f, 1f), ref hairVel, 0.05f);
			}
			else
			{
				hairSpring.dynamicRatio = Mathf.SmoothDamp(hairSpring.dynamicRatio, Mathf.Clamp(1f - num, 0.15f, 1f), ref hairVel, 0.15f);
			}
		}
		myAnim.SetFloat("WalkSpeed", travelSpeed / 6f * 2f);
		lastRotation = base.transform.rotation;
	}

	private void Update()
	{
		if (isSign || !base.isServer)
		{
			return;
		}
		roundedX = Mathf.RoundToInt(base.transform.position.x / 2f);
		roundedY = Mathf.RoundToInt(base.transform.position.z / 2f);
		if (!myAgent || !myAgent.enabled)
		{
			if ((bool)myAnim)
			{
				myAnim.SetBool(CharNetworkAnimator.groundedAnimName, value: true);
			}
			return;
		}
		if (talkingTo == 0 && (bool)following && !doesTask.hasTask)
		{
			if ((bool)following && (bool)followingChar && myManager != null && myManager.CurrentlyInLocation() != followingChar.GetCurrentlyInsideBuilding())
			{
				myAgent.SetDestination(myManager.getPositionForLiveAgent());
			}
			else if (Vector3.Distance(following.position, base.transform.position) > 4f)
			{
				myAgent.SetDestination(following.position - following.forward * 2f);
				isCloseEnoughToFollowing = false;
			}
			else if (!isCloseEnoughToFollowing)
			{
				isCloseEnoughToFollowing = true;
				myAgent.SetDestination(base.transform.position + base.transform.forward);
			}
		}
		if (!myAgent.isActiveAndEnabled || !myAgent.isOnNavMesh)
		{
			return;
		}
		Vector3 b = myAgent.transform.position;
		if (!checkIfHasArrivedAtDestination() && WorldManager.Instance.onTileMap[roundedX, roundedY] > 0 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[roundedX, roundedY]].tileOnOff && WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[roundedX, roundedY]].tileOnOff.isGate && WorldManager.Instance.onTileStatusMap[roundedX, roundedY] == 0 && gateRoutine == null)
		{
			gateRoutine = StartCoroutine(openGateAndThenClose(roundedX, roundedY));
		}
		if (isSitting)
		{
			b = base.transform.position;
		}
		else if (inJump)
		{
			Vector3 b2 = new Vector3(myAgent.transform.position.x, base.transform.position.y, myAgent.transform.position.z);
			base.transform.position = Vector3.Lerp(base.transform.position, b2, Time.fixedDeltaTime * 10f);
		}
		else
		{
			if (myAgent.transform.position.y > -10f && myAgent.transform.position.y < 1f && WorldManager.Instance.waterMap[roundedX, roundedY] && WorldManager.Instance.heightMap[roundedX, roundedY] < 0)
			{
				b = new Vector3(myAgent.transform.position.x, -0.35f, myAgent.transform.position.z);
				if (!inWater)
				{
					myAgent.speed = (float)baseSpeed / 1.5f;
					doesTask.isInWater(isInWater: true);
					inWater = true;
				}
			}
			else
			{
				if (inWater)
				{
					if ((bool)following)
					{
						myAgent.speed = (float)baseSpeed * 2f;
					}
					else
					{
						myAgent.speed = baseSpeed;
					}
					doesTask.isInWater(isInWater: false);
					inWater = false;
				}
				if (CameraController.control.IsCloseToCamera(base.transform.position) && Physics.Raycast(myAgent.transform.position + Vector3.up, Vector3.down, out hit, 2f, jumpLayers))
				{
					b = new Vector3(b.x, hit.point.y, b.z);
				}
			}
			if (((bool)talkingToTransform && checkIfHasArrivedAtDestination()) || ((bool)talkingToTransform && Vector3.Distance(base.transform.position, talkingToTransform.position) <= 2.5f))
			{
				if (myAgent.remainingDistance > 1f)
				{
					myAgent.SetDestination(base.transform.position);
				}
				characterTurnsToLookAtTalkingTo();
			}
			if (!isSitting)
			{
				base.transform.position = Vector3.Lerp(base.transform.position, b, Time.fixedDeltaTime * 8f);
				base.transform.rotation = myAgent.transform.rotation;
			}
			if (!following)
			{
				if (myJob.isRunningLate() || doesTask.isScared || doesTask.IsFlyingKite())
				{
					myAgent.speed = (float)baseSpeed * 2.8f;
				}
				else
				{
					myAgent.speed = baseSpeed;
				}
			}
		}
		if (talkingTo != 0 && talkingToTransform == null)
		{
			NetworktalkingTo = 0u;
		}
		if (talkingTo != 0 && (bool)talkingToTransform && Vector3.Distance(talkingToTransform.position, base.transform.position) > 30f)
		{
			NetworktalkingTo = 0u;
		}
	}

	public void TalkToNpcWithDelay(float delay, ConversationObject forcedConvo = null)
	{
		ConversationManager.manage.SetStartTalkDelay(delay);
		TalkToNPC(forcedConvo);
	}

	public void TalkToNPC(ConversationObject forcedConvo = null)
	{
		if ((bool)forcedConvo)
		{
			ConversationManager.manage.TalkToNPC(this, forcedConvo, hasStartDelay: true, forceUseCustom: true);
		}
		else if ((myJob.atWork && (bool)GetVendorConversation() && RealWorldTimeLight.time.currentHour != 0) || myId.NPCNo == 11 || myId.NPCNo == 5 || myId.NPCNo == 16 || myId.NPCNo == 17)
		{
			ConversationManager.manage.TalkToNPC(this, GetVendorConversation(), hasStartDelay: true);
		}
		else
		{
			ConversationManager.manage.TalkToNPC(this, null, hasStartDelay: true);
		}
	}

	public void playingTalkingAnimation(bool isPlaying)
	{
		if (!isSign)
		{
			if (isPlaying && !myAnim.GetBool("Talking"))
			{
				myAnim.SetFloat("TalkingOffset", Random.Range(0f, 1f));
			}
			myAnim.SetBool("Talking", isPlaying);
		}
	}

	private IEnumerator startNPC()
	{
		myAgent.updateRotation = true;
		yield return new WaitForSeconds(0.1f);
		NavMeshHit navMeshHit;
		while (!NavMesh.SamplePosition(base.transform.position, out navMeshHit, 4f, myAgent.areaMask))
		{
			yield return null;
		}
		myAgent.transform.position = navMeshHit.position;
		myAgent.enabled = true;
		myAgent.Warp(navMeshHit.position);
		while (!myAgent.isOnNavMesh)
		{
			yield return null;
		}
		base.transform.position = myAgent.transform.position;
		base.transform.rotation = myAgent.transform.rotation;
		myAgent.SetDestination(base.transform.position + new Vector3(Random.Range(-12f, 12f), 0f, Random.Range(-12f, 12f)));
		myAgent.isStopped = false;
		if (base.gameObject.activeInHierarchy)
		{
			while (myManager == null)
			{
				myManager = NPCManager.manage.getNPCMapAgentForNPC(myId.NPCNo);
				yield return null;
			}
			StartCoroutine(NPCWonder());
			StartCoroutine(NpcTask());
		}
	}

	private IEnumerator jumpFeel()
	{
		yield return new WaitForSeconds(0.05f);
	}

	public bool IsValidConversationTargetForAnyPlayer()
	{
		if (isSign)
		{
			return true;
		}
		if ((followingNetId == 0 && talkingTo == 0) || (followingNetId == 0 && talkingTo == NetworkMapSharer.Instance.localChar.netId))
		{
			return true;
		}
		return false;
	}

	public bool canBeTalkedToFollowing()
	{
		if (followingNetId == NetworkMapSharer.Instance.localChar.netId)
		{
			return true;
		}
		return false;
	}

	private void characterTurnsToLookAtTalkingTo()
	{
		if ((bool)talkingToTransform && !isSitting)
		{
			Quaternion b = Quaternion.LookRotation(new Vector3(talkingToTransform.position.x, base.transform.position.y, talkingToTransform.position.z) - base.transform.position);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b, Time.deltaTime * 4f);
			myAgent.transform.rotation = base.transform.rotation;
		}
	}

	public void onTalkingChange(uint old, uint newTalkingTo)
	{
		if (isSign)
		{
			return;
		}
		NetworktalkingTo = newTalkingTo;
		if (talkingTo != 0)
		{
			if (NetworkIdentity.spawned.ContainsKey(newTalkingTo))
			{
				talkingToTransform = NetworkIdentity.spawned[newTalkingTo].transform;
			}
			if ((bool)talkingToTransform && base.isServer && myAgent.isActiveAndEnabled && myAgent.isOnNavMesh)
			{
				myAgent.SetDestination(talkingToTransform.position + talkingToTransform.forward * 2.5f);
			}
		}
		else
		{
			talkingToTransform = null;
		}
		if (talkingTo != 0 && talkingToTransform != null && talkingToTransform != NetworkMapSharer.Instance.localChar.transform)
		{
			StartCoroutine(talkToNonLocal());
		}
		else if (talkingTo == 0)
		{
			playingTalkingAnimation(isPlaying: false);
		}
	}

	public void onFollowChange(uint oldFollow, uint newFollowId)
	{
		if (isSign)
		{
			return;
		}
		NetworkfollowingNetId = newFollowId;
		if ((bool)doesTask)
		{
			doesTask.setFollowing(newFollowId);
		}
		if (newFollowId != 0)
		{
			if (base.isServer)
			{
				myAgent.speed = (float)baseSpeed * 2.8f;
				if (!following)
				{
					int areaMask = myAgent.areaMask;
					areaMask += 1 << NavMesh.GetAreaFromName("Water");
					myAgent.areaMask = areaMask;
				}
			}
			following = NetworkIdentity.spawned[newFollowId].transform;
			followingChar = NetworkIdentity.spawned[newFollowId].GetComponent<CharMovement>();
			myManager.setFollowing(following);
			return;
		}
		if (base.isServer)
		{
			myAgent.speed = baseSpeed;
			if ((bool)following)
			{
				int areaMask2 = myAgent.areaMask;
				areaMask2 += 0 << NavMesh.GetAreaFromName("Water");
				myAgent.areaMask = areaMask2;
			}
		}
		following = null;
		followingChar = null;
		myManager.setFollowing(null);
	}

	private IEnumerator NPCWonder()
	{
		while (true)
		{
			if ((RealWorldTimeLight.time.offIsland || RealWorldTimeLight.time.underGround) && !isInside())
			{
				NetworkNavMesh.nav.UnSpawnNPCOnTile(this);
				yield return null;
			}
			bool flag = NetworkNavMesh.nav.doesPositionHaveNavChunk((int)base.transform.position.x / 2, (int)base.transform.position.z / 2);
			if (!isInside() && !flag)
			{
				NetworkNavMesh.nav.UnSpawnNPCOnTile(this);
			}
			if (!NetworkNavMesh.nav.isCloseEnoughToNavChunk((int)base.transform.position.x / 2, (int)base.transform.position.z / 2))
			{
				myAgent.updatePosition = false;
				myAgent.transform.position = myAgent.nextPosition;
			}
			else
			{
				myAgent.updatePosition = true;
			}
			yield return null;
		}
	}

	public bool checkIfHasArrivedAtDestination()
	{
		if (!myAgent.enabled || !myAgent.isOnNavMesh)
		{
			return true;
		}
		if (!myAgent.hasPath)
		{
			return true;
		}
		if (myAgent.remainingDistance - 1f <= myAgent.stoppingDistance && myAgent.path.status != cantComplete)
		{
			return true;
		}
		if (myAgent.pathStatus != 0)
		{
			return true;
		}
		return false;
	}

	public bool isInside()
	{
		if (myManager == null)
		{
			return true;
		}
		if (myManager.isAtWork())
		{
			return true;
		}
		if (myManager.isInsideABuilding())
		{
			return true;
		}
		return false;
	}

	public void despawnOnEvent()
	{
		if ((base.isActiveAndEnabled && !isInside()) || TownEventManager.manage.townEventOn != 0)
		{
			NetworkNavMesh.nav.UnSpawnNPCOnTile(this);
		}
	}

	public void changeWalkOnEntrance(bool can)
	{
		if (can != canWalkOnEntrance)
		{
			if (can)
			{
				jumpLayers = (int)jumpLayers + (1 << NavMesh.GetAreaFromName("Entrance"));
			}
			else
			{
				jumpLayers = (int)jumpLayers - (1 << NavMesh.GetAreaFromName("Entrance"));
			}
		}
		canWalkOnEntrance = can;
	}

	public Transform getTalkingToTransform()
	{
		return talkingToTransform;
	}

	private IEnumerator talkToNonLocal()
	{
		while (talkingTo != 0)
		{
			playingTalkingAnimation(isPlaying: true);
			float waitingForNoTalk = Random.Range(0.25f, 0.45f);
			while (waitingForNoTalk > 0f)
			{
				yield return null;
				waitingForNoTalk -= Time.deltaTime;
				if (Random.Range(0, 30) == 10)
				{
					faceAnim.eyes.sayWord();
				}
			}
		}
		playingTalkingAnimation(isPlaying: false);
	}

	private IEnumerator openGateAndThenClose(int xPos, int yPos)
	{
		NetworkMapSharer.Instance.RpcNPCOpenGate(xPos, yPos, base.netId);
		while (Mathf.RoundToInt(base.transform.position.x) / 2 == xPos && Mathf.RoundToInt(base.transform.position.z) / 2 == yPos)
		{
			yield return null;
		}
		gateRoutine = null;
	}

	private void RotateAgentTowards(Transform target)
	{
		Quaternion b = Quaternion.LookRotation((target.position - myAgent.transform.position).normalized);
		myAgent.transform.rotation = Quaternion.Slerp(myAgent.transform.rotation, b, Time.deltaTime * myAgent.angularSpeed);
	}

	private IEnumerator NpcTask()
	{
		npcWait = new WaitForSeconds(Random.Range(0.5f, 0.85f));
		int cyclesBeforeRetry = 0;
		while (true)
		{
			yield return npcWait;
			if (myManager == null)
			{
				Debug.LogWarning("NPC has no Manager");
			}
			if (followingNetId == 0 && talkingTo == 0 && myAgent.isActiveAndEnabled && myAgent.isOnNavMesh && myManager != null)
			{
				myJob.NetworkatWork = myManager.isAtWork();
				Vector3 newDesiredLocation = myManager.getPositionForLiveAgent();
				if ((isInside() && newDesiredLocation != Vector3.zero) || (checkIfHasArrivedAtDestination() && newDesiredLocation != Vector3.zero) || (cyclesBeforeRetry >= 5 && newDesiredLocation != Vector3.zero))
				{
					cyclesBeforeRetry = 0;
					if (isInside() || NetworkNavMesh.nav.doesPositionHaveNavChunk((int)newDesiredLocation.x / 2, (int)newDesiredLocation.z / 2))
					{
						if (Vector3.Distance(myAgent.transform.position, newDesiredLocation) <= myAgent.stoppingDistance)
						{
							while (Vector3.Distance(myAgent.transform.position, newDesiredLocation) <= myAgent.stoppingDistance && myManager.hasDesiredRotation())
							{
								yield return null;
								myAgent.transform.rotation = Quaternion.Slerp(myAgent.transform.rotation, myManager.getDesiredRotation(), Time.deltaTime * 2f);
							}
						}
						else
						{
							myAgent.SetDestination(newDesiredLocation);
						}
					}
					else
					{
						Vector3 destination = base.transform.position + (newDesiredLocation - base.transform.position).normalized * 16f;
						myAgent.SetDestination(destination);
						Vector3 positionToMoveTo = myAgent.transform.position + myAgent.transform.forward * 3.5f;
						if (!NetworkNavMesh.nav.doesPositionHaveNavChunk(Mathf.RoundToInt(positionToMoveTo.x / 2f), Mathf.RoundToInt(positionToMoveTo.z / 2f)))
						{
							myManager.moveOffNavMesh(positionToMoveTo);
						}
					}
				}
			}
			cyclesBeforeRetry++;
		}
	}

	public bool isSittingDown()
	{
		return isSitting;
	}

	public void OnSittingChanged(bool oldSitting, bool newSitting)
	{
		NetworkisSitting = newSitting;
		if (isSitting)
		{
			myAnim.SetTrigger("Sitting");
			myAnim.SetBool("SittingOrLaying", value: true);
		}
		else
		{
			myAnim.SetTrigger("Standing");
			myAnim.SetBool("SittingOrLaying", value: false);
		}
	}

	public void SitDownInside()
	{
		NetworkisSitting = true;
	}

	public void StandUpInside()
	{
		NetworkisSitting = false;
	}

	public void StandUpAtEndOfDay()
	{
		NetworkisSitting = false;
	}

	public void SitDownOutside(int sittingInPos, int xPos, int yPos)
	{
		seatPos = sittingInPos;
		seatX = xPos;
		seatY = yPos;
		NetworkisSitting = true;
		RpcSitDownOutside(sittingInPos, xPos, yPos);
	}

	public void StandUpOutside(int sittingInPos, int xPos, int yPos)
	{
		seatPos = 0;
		seatX = -1;
		seatY = -1;
		NetworkisSitting = false;
		RpcGetUp(sittingInPos, xPos, yPos);
	}

	[ClientRpc]
	private void RpcSitDownOutside(int sittingInPos, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(sittingInPos);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NPCAI), "RpcSitDownOutside", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcGetUp(int sittingInPos, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(sittingInPos);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NPCAI), "RpcGetUp", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public ConversationObject GetVendorConversation()
	{
		return NPCManager.manage.NPCDetails[myId.NPCNo].keeperConvos;
	}

	public Vector3 getSureRandomPos()
	{
		Vector3 destinationToCheck = myAgent.transform.position + new Vector3(Random.Range(-15f, 15f), 0f, Random.Range(-15f, 15f));
		destinationToCheck = checkDestination(destinationToCheck);
		int num = 0;
		while (destinationToCheck == base.transform.position)
		{
			destinationToCheck = myAgent.transform.position + new Vector3(Random.Range(-10, 10), 0f, Random.Range(-10, 10));
			destinationToCheck = checkDestination(destinationToCheck);
			num++;
			if (num >= 30)
			{
				break;
			}
		}
		if (num >= 30)
		{
			destinationToCheck = myAgent.transform.position + new Vector3(Random.Range(-10, 10), 0f, Random.Range(-10, 10));
		}
		return destinationToCheck;
	}

	public Vector3 checkDestination(Vector3 destinationToCheck)
	{
		if (NavMesh.SamplePosition(destinationToCheck, out var navMeshHit, 5f, myAgent.areaMask))
		{
			if (myAgent.isActiveAndEnabled && myAgent.isOnNavMesh)
			{
				NavMeshPath navMeshPath = new NavMeshPath();
				myAgent.CalculatePath(navMeshHit.position, navMeshPath);
				if (navMeshPath.status == cantComplete)
				{
					return base.transform.position;
				}
			}
			if (NavMesh.SamplePosition(destinationToCheck, out navMeshHit, 1f, myAgent.areaMask))
			{
				return navMeshHit.position;
			}
		}
		return base.transform.position;
	}

	public bool checkPositionIsOnNavmesh(Vector3 destinationToCheck)
	{
		if (NavMesh.SamplePosition(destinationToCheck, out var _, 3f, myAgent.areaMask))
		{
			return true;
		}
		return false;
	}

	public Vector3 getClosestPosOnNavMesh(Vector3 destinationToCheck)
	{
		if (NavMesh.SamplePosition(destinationToCheck, out var navMeshHit, 2.5f, myAgent.areaMask))
		{
			return navMeshHit.position;
		}
		return destinationToCheck;
	}

	public bool canStillReachTaskLocation(Vector3 taskPos)
	{
		if (myAgent.isActiveAndEnabled)
		{
			return false;
		}
		NavMeshPath navMeshPath = new NavMeshPath();
		myAgent.CalculatePath(taskPos, navMeshPath);
		if (navMeshPath.status != 0)
		{
			return false;
		}
		return true;
	}

	static NPCAI()
	{
		npcWait = new WaitForSeconds(1f);
		cantComplete = NavMeshPathStatus.PathPartial;
		RemoteCallHelper.RegisterRpcDelegate(typeof(NPCAI), "RpcSitDownOutside", InvokeUserCode_RpcSitDownOutside);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NPCAI), "RpcGetUp", InvokeUserCode_RpcGetUp);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcSitDownOutside(int sittingInPos, int xPos, int yPos)
	{
		seatPos = sittingInPos;
		seatX = xPos;
		seatY = yPos;
		WorldManager.Instance.onTileStatusMap[xPos, yPos] = Mathf.Clamp(WorldManager.Instance.onTileStatusMap[xPos, yPos] + sittingInPos, 0, 3);
		TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
		if ((bool)tileObject && (bool)tileObject.tileObjectFurniture)
		{
			tileObject.tileObjectFurniture.updateOnTileStatus(xPos, yPos);
		}
	}

	protected static void InvokeUserCode_RpcSitDownOutside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSitDownOutside called on server.");
		}
		else
		{
			((NPCAI)obj).UserCode_RpcSitDownOutside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcGetUp(int sittingInPos, int xPos, int yPos)
	{
		WorldManager.Instance.onTileStatusMap[xPos, yPos] = Mathf.Clamp(WorldManager.Instance.onTileStatusMap[xPos, yPos] - sittingInPos, 0, 3);
		TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
		if ((bool)tileObject)
		{
			tileObject.tileObjectFurniture.updateOnTileStatus(xPos, yPos);
		}
	}

	protected static void InvokeUserCode_RpcGetUp(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcGetUp called on server.");
		}
		else
		{
			((NPCAI)obj).UserCode_RpcGetUp(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteUInt(talkingTo);
			writer.WriteUInt(followingNetId);
			writer.WriteBool(isSitting);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteUInt(talkingTo);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteUInt(followingNetId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteBool(isSitting);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			uint num = talkingTo;
			NetworktalkingTo = reader.ReadUInt();
			if (!SyncVarEqual(num, ref talkingTo))
			{
				onTalkingChange(num, talkingTo);
			}
			uint num2 = followingNetId;
			NetworkfollowingNetId = reader.ReadUInt();
			if (!SyncVarEqual(num2, ref followingNetId))
			{
				onFollowChange(num2, followingNetId);
			}
			bool flag = isSitting;
			NetworkisSitting = reader.ReadBool();
			if (!SyncVarEqual(flag, ref isSitting))
			{
				OnSittingChanged(flag, isSitting);
			}
			return;
		}
		long num3 = (long)reader.ReadULong();
		if ((num3 & 1L) != 0L)
		{
			uint num4 = talkingTo;
			NetworktalkingTo = reader.ReadUInt();
			if (!SyncVarEqual(num4, ref talkingTo))
			{
				onTalkingChange(num4, talkingTo);
			}
		}
		if ((num3 & 2L) != 0L)
		{
			uint num5 = followingNetId;
			NetworkfollowingNetId = reader.ReadUInt();
			if (!SyncVarEqual(num5, ref followingNetId))
			{
				onFollowChange(num5, followingNetId);
			}
		}
		if ((num3 & 4L) != 0L)
		{
			bool flag2 = isSitting;
			NetworkisSitting = reader.ReadBool();
			if (!SyncVarEqual(flag2, ref isSitting))
			{
				OnSittingChanged(flag2, isSitting);
			}
		}
	}
}
