using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCManager : MonoBehaviour
{
	public List<NPCMapAgent> npcsOnMap = new List<NPCMapAgent>();

	public List<NPCStatus> npcStatus = new List<NPCStatus>();

	public List<NPCInventory> npcInvs = new List<NPCInventory>();

	public static NPCManager manage;

	public NPCAI sign;

	public NPCAI[] vendorNPCs;

	public NPCAI NPCPrefab;

	public NPCRequest[] NPCRequests;

	public NPCDetails[] NPCDetails;

	public NPCSchedual.Locations[] visitingSchedual;

	public Mesh defaultNpcMesh;

	public Mesh defaultInsideMouth;

	public GameObject npcmapAgentMarker;

	public NPCPathFinding pathFinder;

	private WaitForSeconds wait = new WaitForSeconds(1f);

	public void Awake()
	{
		manage = this;
		npcStatus = new List<NPCStatus>();
		npcInvs = new List<NPCInventory>();
		for (int i = 0; i < NPCDetails.Length; i++)
		{
			npcStatus.Add(new NPCStatus());
			npcInvs.Add(new NPCInventory());
			if (i == 6)
			{
				npcStatus[6].hasMet = true;
			}
		}
		vendorNPCs = new NPCAI[Enum.GetNames(typeof(NPCSchedual.Locations)).Length];
		NPCRequests = new NPCRequest[NPCDetails.Length];
		visitingSchedual = new NPCSchedual.Locations[24];
		for (int j = 0; j < visitingSchedual.Length; j++)
		{
			visitingSchedual[j] = NPCSchedual.Locations.Wonder;
			if (j >= 6 && j <= 15)
			{
				visitingSchedual[j] = NPCSchedual.Locations.Market_place;
			}
			else if (j == 16)
			{
				visitingSchedual[j] = NPCSchedual.Locations.Exit;
			}
			else
			{
				visitingSchedual[j] = NPCSchedual.Locations.Wonder;
			}
		}
	}

	public void Start()
	{
		for (int i = 0; i < NPCDetails.Length; i++)
		{
			NPCRequests[i] = new NPCRequest();
		}
	}

	public void resetNPCRequestsForNewDay()
	{
		for (int i = 0; i < NPCDetails.Length; i++)
		{
			if (NPCRequests[i].CheckIfUnfinishedRequestToday())
			{
				MonoBehaviour.print("Accepted request was not filled for NPC: " + i);
				npcStatus[i].addToRelationshipLevel(-2);
			}
			if (npcStatus[i].relationshipLevel >= 95 && !npcStatus[i].hasBeenTalkedToToday && !NPCRequests[i].CheckIfIDidARequestToday())
			{
				MonoBehaviour.print("Didn't interact with the NPC today " + i);
				npcStatus[i].addToRelationshipLevel(-1);
			}
			npcStatus[i].acceptedRequest = false;
			npcStatus[i].completedRequest = false;
			NPCRequests[i].generatedToday = false;
			NPCRequests[i].RefreshAcceptedAndCompletedOnNewDay();
			Debug.Log("Ckearing compelted here");
		}
	}

	public void resetNPCRequestsForSave()
	{
		for (int i = 0; i < NPCDetails.Length; i++)
		{
		}
	}

	public void refreshAllAnimators(bool on)
	{
	}

	public NPCAI getVendorNPC(NPCSchedual.Locations worksAt)
	{
		return vendorNPCs[(int)worksAt];
	}

	public int getNoOfNPCsMovedIn()
	{
		int num = 0;
		for (int i = 0; i < npcStatus.Count; i++)
		{
			if (npcStatus[i].checkIfHasMovedIn())
			{
				num++;
			}
		}
		return num;
	}

	public bool shouldAskToMoveIn(int npcId)
	{
		if (!NetworkMapSharer.Instance.isServer)
		{
			return false;
		}
		if (!npcStatus[npcId].checkIfHasMovedIn() && !npcStatus[npcId].hasAskedToMoveIn)
		{
			if (NPCDetails[npcId].spendBeforeMoveIn <= npcStatus[npcId].moneySpentAtStore && NPCDetails[npcId].relationshipBeforeMove <= npcStatus[npcId].relationshipLevel)
			{
				if ((bool)NPCDetails[npcId].deedOnMoveRequest)
				{
					npcStatus[npcId].hasAskedToMoveIn = true;
					return true;
				}
				return false;
			}
			return false;
		}
		return false;
	}

	public void startNPCs()
	{
		Invoke("startNPCDelay", 0.25f);
	}

	public void clearAllActiveNPCWantToWalkIntoForBandStand()
	{
		for (int i = 0; i < npcsOnMap.Count; i++)
		{
			npcsOnMap[i].clearLastWalkedIntoAndWantToWalkInto(TownManager.manage.allShopFloors[25]);
		}
	}

	public void turnOnNpcDebugMarkers()
	{
		for (int i = 0; i < npcsOnMap.Count; i++)
		{
			if ((bool)npcsOnMap[i].debugMarker)
			{
				UnityEngine.Object.Destroy(npcsOnMap[i].debugMarker);
			}
			npcsOnMap[i].debugMarker = UnityEngine.Object.Instantiate(npcmapAgentMarker, npcsOnMap[i].currentPosition, Quaternion.identity);
			npcsOnMap[i].debugMarker.name = "NPC " + npcsOnMap[i].npcId;
		}
	}

	public void removeDebugMarkers()
	{
		for (int i = 0; i < npcsOnMap.Count; i++)
		{
			if ((bool)npcsOnMap[i].debugMarker)
			{
				UnityEngine.Object.Destroy(npcsOnMap[i].debugMarker);
			}
		}
	}

	private void startNPCDelay()
	{
		ScheduleManager.manage.giveNpcsNewDaySchedual(WorldManager.Instance.day, WorldManager.Instance.week, WorldManager.Instance.month);
		for (int i = 0; i < NPCDetails.Length; i++)
		{
			if (npcStatus[i].checkIfHasMovedIn())
			{
				setUpNPCAgent(i, (int)(WorldManager.Instance.spawnPos.position.x / 2f), (int)(WorldManager.Instance.spawnPos.position.z / 2f));
			}
		}
		MarketPlaceManager.manage.getNpcOnLoad();
		StartCoroutine(moveNpcs());
		StartNewDay();
		TownManager.manage.checkIfFirstConnect();
	}

	public void setUpNPCAgent(int npcId, int xPos, int yPos)
	{
		for (int i = 0; i < npcsOnMap.Count; i++)
		{
			if (npcsOnMap[i].npcId == npcId)
			{
				Debug.LogWarning("Tried to create a map agent for NPC that already has one");
				return;
			}
		}
		NPCMapAgent item = new NPCMapAgent(npcId, xPos, yPos);
		npcsOnMap.Add(item);
	}

	public void setNPCInSideBuilding(int npcId, NPCSchedual.Locations location)
	{
		for (int i = 0; i < npcsOnMap.Count; i++)
		{
			if (npcsOnMap[i].npcId == npcId)
			{
				npcsOnMap[i].setBuildingCurrentlyIn(location);
				break;
			}
		}
	}

	public void checkForNPCsInTheWayAndMakeThemMove(Transform position)
	{
		for (int i = 0; i < npcsOnMap.Count; i++)
		{
			if ((bool)npcsOnMap[i].activeNPC && Vector3.Distance(npcsOnMap[i].activeNPC.transform.position, position.position) <= 3.5f)
			{
				npcsOnMap[i].activeNPC.myAgent.SetDestination(position.position + position.forward + position.right * UnityEngine.Random.Range(-3, 3));
			}
		}
	}

	public void moveInNPC(int NPCId)
	{
		npcStatus[NPCId].moveInNPC();
		generateVillagerNPCOnMoveIn(NPCId);
		bool flag = false;
		for (int i = 0; i < npcsOnMap.Count; i++)
		{
			if (npcsOnMap[i].npcId == NPCId)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			NPCMapAgent item = new NPCMapAgent(NPCId, (int)(WorldManager.Instance.spawnPos.position.x / 2f), (int)(WorldManager.Instance.spawnPos.position.z / 2f));
			npcsOnMap.Add(item);
		}
	}

	public NPCMapAgent getMapAgentForNPC(int npcId)
	{
		for (int i = 0; i < npcsOnMap.Count; i++)
		{
			if (npcsOnMap[i].npcId == npcId)
			{
				return npcsOnMap[i];
			}
		}
		return null;
	}

	public void moveNpcToPlayerAndStartTalking(int npcId, bool turnplayer = false, ConversationObject sayThis = null, bool showBlackFade = true)
	{
		for (int i = 0; i < npcsOnMap.Count; i++)
		{
			Vector3 vector = NetworkMapSharer.Instance.localChar.transform.position - CameraController.control.transform.forward * 2f;
			if (npcsOnMap[i].npcId != npcId)
			{
				continue;
			}
			if ((bool)npcsOnMap[i].activeNPC)
			{
				if (Vector3.Distance(npcsOnMap[i].activeNPC.transform.position, vector) > 12f)
				{
					npcsOnMap[i].activeNPC.myAgent.Warp(vector);
					npcsOnMap[i].activeNPC.transform.position = vector;
				}
				npcsOnMap[i].activeNPC.myAgent.SetDestination(NetworkMapSharer.Instance.localChar.transform.position + NetworkMapSharer.Instance.localChar.transform.forward * 1.5f);
			}
			else
			{
				npcsOnMap[i].currentPosition = vector;
				NetworkNavMesh.nav.SpawnAnNPCOnTileAndWarp(npcsOnMap[i].npcId, Mathf.RoundToInt(npcsOnMap[i].currentPosition.x / 2f), Mathf.RoundToInt(npcsOnMap[i].currentPosition.z / 2f), vector);
			}
			StartCoroutine(faceNPC(npcsOnMap[i].activeNPC.transform));
			StartCoroutine(delayTalk(i, sayThis, showBlackFade));
			break;
		}
	}

	private IEnumerator faceNPC(Transform faceTransform)
	{
		float timer = 0f;
		while (timer < 1f)
		{
			timer += Time.deltaTime;
			Vector3 normalized = (faceTransform.position - NetworkMapSharer.Instance.localChar.transform.position).normalized;
			normalized.y = 0f;
			Quaternion b = Quaternion.LookRotation(normalized);
			NetworkMapSharer.Instance.localChar.transform.rotation = Quaternion.Slerp(NetworkMapSharer.Instance.localChar.transform.rotation, b, Time.deltaTime * 10f);
			yield return null;
		}
	}

	private IEnumerator delayTalk(int npcNo, ConversationObject sayThis = null, bool showFade = true)
	{
		while (!NetworkMapSharer.Instance.localChar)
		{
			yield return null;
		}
		while (npcNo >= npcsOnMap.Count || npcsOnMap[npcNo] == null || !npcsOnMap[npcNo].activeNPC)
		{
			yield return null;
		}
		npcsOnMap[npcNo].activeNPC.myAgent.SetDestination(NetworkMapSharer.Instance.localChar.transform.position);
		npcsOnMap[npcNo].activeNPC.myAgent.transform.position = new Vector3(npcsOnMap[npcNo].activeNPC.myAgent.transform.position.x, NetworkMapSharer.Instance.localChar.transform.position.y, npcsOnMap[npcNo].activeNPC.myAgent.transform.position.z);
		while (ConversationManager.manage.IsConversationActive)
		{
			yield return null;
		}
		npcsOnMap[npcNo].activeNPC.TalkToNpcWithDelay(2f, sayThis);
		yield return new WaitForSeconds(1f);
		if (showFade)
		{
			CameraController.control.blackFadeAnim.fadeOut();
		}
	}

	public NPCAI spawnFreeNPC(int NPCId, int xPos, int yPos)
	{
		for (int i = 0; i < npcsOnMap.Count; i++)
		{
			if (npcsOnMap[i].npcId == NPCId)
			{
				if (!npcsOnMap[i].activeNPC)
				{
					NPCAI component = UnityEngine.Object.Instantiate(NPCPrefab, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2), Quaternion.identity).GetComponent<NPCAI>();
					component.GetComponent<NPCIdentity>().changeNPCAndEquip(NPCId);
					npcsOnMap[i].pullNpcFromMap(component);
					return component;
				}
				return npcsOnMap[i].activeNPC;
			}
		}
		return null;
	}

	public NPCAI spawnFreeNPCAtPos(int NPCId, Vector3 spawnPos)
	{
		for (int i = 0; i < npcsOnMap.Count; i++)
		{
			if (npcsOnMap[i].npcId == NPCId)
			{
				if (!npcsOnMap[i].activeNPC)
				{
					NPCAI component = UnityEngine.Object.Instantiate(NPCPrefab, spawnPos, Quaternion.identity).GetComponent<NPCAI>();
					component.GetComponent<NPCIdentity>().changeNPCAndEquip(NPCId);
					npcsOnMap[i].pullNpcFromMap(component);
					return component;
				}
				npcsOnMap[i].activeNPC.myAgent.Warp(spawnPos);
				npcsOnMap[i].activeNPC.transform.position = spawnPos;
				return npcsOnMap[i].activeNPC;
			}
		}
		return null;
	}

	public void giveBackNpcDontSave(NPCAI npcToReturn)
	{
		NetworkNavMesh.nav.UnSpawnNPCDontSaveToMap(npcToReturn);
	}

	public NPCMapAgent getNPCMapAgentForNPC(int id)
	{
		for (int i = 0; i < npcsOnMap.Count; i++)
		{
			if (npcsOnMap[i].npcId == id)
			{
				return npcsOnMap[i];
			}
		}
		return null;
	}

	public void returnGuestNPCs()
	{
		List<NPCMapAgent> list = new List<NPCMapAgent>();
		for (int i = 0; i < npcsOnMap.Count; i++)
		{
			if (!npcStatus[npcsOnMap[i].npcId].checkIfHasMovedIn())
			{
				list.Add(npcsOnMap[i]);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			list[j].removeSelf();
			npcsOnMap.Remove(list[j]);
		}
	}

	public void StartNewDay(bool useTomorrowsDate = false)
	{
		if (useTomorrowsDate)
		{
			ScheduleManager.manage.giveNpcsNewDaySchedual(RealWorldTimeLight.time.getTomorrowsDay(), RealWorldTimeLight.time.getTomorrowsWeek(), RealWorldTimeLight.time.getTomorrowsMonth());
		}
		else
		{
			ScheduleManager.manage.giveNpcsNewDaySchedual(WorldManager.Instance.day, WorldManager.Instance.week, WorldManager.Instance.month);
		}
		for (int i = 0; i < npcsOnMap.Count; i++)
		{
			npcsOnMap[i].setNewDayDesire(useTomorrowsDate);
			npcsOnMap[i].setFollowing(null);
			if ((bool)npcsOnMap[i].activeNPC)
			{
				npcsOnMap[i].activeNPC.NetworkfollowingNetId = 0u;
				npcsOnMap[i].activeNPC.NetworktalkingTo = 0u;
				npcsOnMap[i].activeNPC.doesTask.npcStartNewDay();
			}
			npcStatus[i].hasBeenTalkedToToday = false;
			npcStatus[i].hasGossipedToday = false;
			npcStatus[i].refreshCharactersGreeted();
			npcStatus[i].hasHungOutToday = false;
			npcsOnMap[i].warpNpcInside();
		}
	}

	private IEnumerator moveNpcs()
	{
		yield return wait;
		while (true)
		{
			yield return wait;
			for (int i = 0; i < npcsOnMap.Count; i++)
			{
				npcsOnMap[i].movePosition();
			}
		}
	}

	public void generateVillagerNPCOnMoveIn(int npcId)
	{
		if (NPCDetails[npcId].isAVillager)
		{
			UnityEngine.Random.InitState(NetworkMapSharer.Instance.seed + npcId);
			bool flag = false;
			if (UnityEngine.Random.Range(0, 7) <= 3)
			{
				flag = true;
			}
			npcInvs[npcId].isFem = flag;
			npcInvs[npcId].nameId = UnityEngine.Random.Range(0, NPCDetails[npcId].randomNames.Length);
			npcInvs[npcId].hairId = RandomObjectGenerator.generate.getRandomHair(flag);
			npcInvs[npcId].hairColorId = UnityEngine.Random.Range(0, 6);
			npcInvs[npcId].eyesId = UnityEngine.Random.Range(0, CharacterCreatorScript.create.skinTones.Length);
			npcInvs[npcId].eyeColorId = UnityEngine.Random.Range(0, CharacterCreatorScript.create.eyeColours.Length);
			npcInvs[npcId].skinId = UnityEngine.Random.Range(0, CharacterCreatorScript.create.skinTones.Length);
			InventoryItem randomShirtForGender = RandomObjectGenerator.generate.getRandomShirtForGender(flag);
			npcInvs[npcId].shirtId = Inventory.Instance.getInvItemId(randomShirtForGender);
			InventoryItem randomPants = RandomObjectGenerator.generate.getRandomPants();
			npcInvs[npcId].pantsId = Inventory.Instance.getInvItemId(randomPants);
			InventoryItem randomShoes = RandomObjectGenerator.generate.getRandomShoes();
			npcInvs[npcId].shoesId = Inventory.Instance.getInvItemId(randomShoes);
		}
	}

	public void requestNPCInvs()
	{
		for (int i = 0; i < NPCDetails.Length; i++)
		{
			if (NPCDetails[i].isAVillager)
			{
				npcInvs[i].hasBeenRequested = false;
				NetworkMapSharer.Instance.localChar.CmdRequestNPCInv(i);
			}
		}
	}

	public NPCAI returnLiveAgentWithNPCId(int NPCId)
	{
		for (int i = 0; i < npcsOnMap.Count; i++)
		{
			if (npcsOnMap[i].npcId == NPCId)
			{
				return npcsOnMap[i].activeNPC;
			}
		}
		return null;
	}

	public Vector3 checkPositionIsOnNavmesh(Vector3 destinationToCheck)
	{
		if (NavMesh.SamplePosition(destinationToCheck, out var hit, 5f, NPCPrefab.myAgent.areaMask))
		{
			return hit.position;
		}
		return Vector3.zero;
	}
}
