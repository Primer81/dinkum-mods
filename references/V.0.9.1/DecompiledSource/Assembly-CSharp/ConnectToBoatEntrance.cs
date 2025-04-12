using System.Collections;
using UnityEngine;

public class ConnectToBoatEntrance : MonoBehaviour
{
	public static ConnectToBoatEntrance connect;

	public bool isMainConnect;

	public bool isBoat = true;

	public bool isInterior;

	public EntryExit myEntryExit;

	public EntryExit boatEntry;

	public EntryExit interiorEntryExit;

	public GameObject interiorToHideAndRotate;

	public NPCBuildingDoors myDoorAndFloor;

	public Transform cutOutWalls;

	private Quaternion storedRotation;

	private void Awake()
	{
		if (isMainConnect)
		{
			connect = this;
			StartCoroutine(interiorFollowsBoatRotation());
		}
	}

	private void OnEnable()
	{
		if (!isMainConnect)
		{
			if (isBoat)
			{
				connect.boatEntry = myEntryExit;
				myEntryExit.interiorToTurnOnOrOff = connect.interiorToHideAndRotate;
				myEntryExit.linkedTo = connect.interiorEntryExit.transform;
				connect.interiorEntryExit.linkedTo = myEntryExit.transform;
				connect.boatEntry.interiorToTurnOnOrOff = connect.interiorToHideAndRotate;
			}
			else
			{
				connect.interiorEntryExit = myEntryExit;
			}
		}
	}

	private IEnumerator interiorFollowsBoatRotation()
	{
		storedRotation = Quaternion.identity;
		while (true)
		{
			yield return null;
			if (!boatEntry || !(boatEntry.transform.root.rotation != storedRotation))
			{
				continue;
			}
			interiorToHideAndRotate.transform.rotation = boatEntry.transform.rotation;
			interiorToHideAndRotate.transform.localEulerAngles = new Vector3(0f, interiorToHideAndRotate.transform.localEulerAngles.y, 0f);
			cutOutWalls.transform.rotation = interiorToHideAndRotate.transform.rotation;
			if ((bool)NetworkMapSharer.Instance && NetworkMapSharer.Instance.isServer && (bool)NPCManager.manage.getMapAgentForNPC(11).activeNPC)
			{
				NPCAI activeNPC = NPCManager.manage.getMapAgentForNPC(11).activeNPC;
				activeNPC.myAgent.transform.position = myDoorAndFloor.inside.position;
				if ((bool)activeNPC)
				{
					activeNPC.myAgent.ResetPath();
					activeNPC.myAgent.Warp(myDoorAndFloor.inside.position);
					activeNPC.myAgent.SetDestination(myDoorAndFloor.inside.position);
				}
			}
			storedRotation = boatEntry.transform.root.rotation;
		}
	}

	public void setUpBoatFloor()
	{
		myDoorAndFloor.setConnectedToBuildingId(20);
		myDoorAndFloor.addMeshesToNavMesh();
	}
}
