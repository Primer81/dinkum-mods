using UnityEngine;

public class EntryExit : MonoBehaviour
{
	public Transform linkedTo;

	public bool isEntry = true;

	public bool isPlayerHouseDoor;

	public bool interiorDoor;

	public DisplayPlayerHouseTiles connectedPlayerHouse;

	public bool isMuseumDoor;

	public MusicManager.indoorMusic myMusic;

	public bool noMusic;

	private int npcId = -1;

	public GameObject interiorToTurnOnOrOff;

	private TileObject thisBuilding;

	public NPCSchedual.Locations myLocation;

	public void feedInNPCId(int newId)
	{
		npcId = newId;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!canEnter())
		{
			return;
		}
		CharInteract componentInParent = other.transform.GetComponentInParent<CharInteract>();
		if (!componentInParent || !componentInParent.isLocalPlayer || !componentInParent.CanWalkThroughDoor())
		{
			return;
		}
		componentInParent.StartWalkThroughDoorTimer();
		CharMovement componentInParent2 = other.transform.GetComponentInParent<CharMovement>();
		componentInParent2.CmdUpdateCurrentlyInside((int)myLocation);
		if ((bool)interiorToTurnOnOrOff && isEntry)
		{
			interiorToTurnOnOrOff.SetActive(value: true);
		}
		if ((bool)interiorToTurnOnOrOff && !isEntry)
		{
			interiorToTurnOnOrOff.SetActive(value: false);
		}
		componentInParent2.forceNoStandingOn(linkedTo.position + linkedTo.forward * 0.8f);
		if (componentInParent2.isLocalPlayer)
		{
			if (isEntry && componentInParent2.underWater)
			{
				componentInParent2.changeToAboveWater();
			}
			else if (WorldManager.Instance.waterMap[Mathf.RoundToInt(linkedTo.transform.position.x / 2f), Mathf.RoundToInt(linkedTo.transform.position.z / 2f)] && linkedTo.transform.position.y < -2f && linkedTo.transform.position.y >= (float)WorldManager.Instance.heightMap[Mathf.RoundToInt(linkedTo.transform.position.x / 2f), Mathf.RoundToInt(linkedTo.transform.position.z / 2f)])
			{
				componentInParent2.changeToUnderWater();
			}
		}
		componentInParent.transform.position = linkedTo.position + linkedTo.forward * 0.8f;
		componentInParent.GetComponent<Rigidbody>().velocity = Vector3.zero;
		CameraController.control.moveToFollowing();
		if (!interiorDoor)
		{
			if (isEntry)
			{
				NewChunkLoader.loader.inside = true;
				if (!isPlayerHouseDoor)
				{
					componentInParent.myEquip.setInsideOrOutside(insideOrOut: true, playersHouse: false);
				}
				WeatherManager.Instance.ChangeToInsideEnvironment(myMusic, noMusic);
				RealWorldTimeLight.time.goInside();
				if ((bool)thisBuilding)
				{
					TownManager.manage.savedInside[0] = thisBuilding.xPos;
					TownManager.manage.savedInside[1] = thisBuilding.yPos;
				}
			}
			else
			{
				NewChunkLoader.loader.inside = false;
				WeatherManager.Instance.ChangeToOutsideEnvironment();
				RealWorldTimeLight.time.goOutside();
				if (!isPlayerHouseDoor)
				{
					componentInParent.myEquip.setInsideOrOutside(insideOrOut: false, playersHouse: false);
				}
				if ((bool)thisBuilding)
				{
					TownManager.manage.savedInside[0] = -1;
					TownManager.manage.savedInside[1] = -1;
				}
			}
		}
		CameraController.control.moveToFollowing();
		if (isPlayerHouseDoor)
		{
			if (isEntry && (bool)connectedPlayerHouse)
			{
				if ((bool)componentInParent)
				{
					componentInParent.ChangeInsideOut(isEntry, HouseManager.manage.getHouseInfo(connectedPlayerHouse.housePosX, connectedPlayerHouse.housePosY));
				}
			}
			else if ((bool)componentInParent)
			{
				componentInParent.ChangeInsideOut(isEntry: false);
			}
		}
		if (isMuseumDoor && MuseumManager.manage.clientNeedsToRequest)
		{
			componentInParent.GetComponent<CharMovement>().CmdRequestMuseumInterior();
		}
	}

	public bool canEnter()
	{
		if (npcId != -1)
		{
			if (NPCManager.manage.NPCDetails[npcId].mySchedual.checkIfOpen(npcId))
			{
				return true;
			}
			return false;
		}
		return true;
	}

	private void OnEnable()
	{
		if (!thisBuilding)
		{
			thisBuilding = base.transform.root.GetComponent<TileObject>();
		}
		if ((bool)interiorToTurnOnOrOff && (bool)thisBuilding && thisBuilding.xPos == TownManager.manage.savedInside[0] && thisBuilding.yPos == TownManager.manage.savedInside[1])
		{
			interiorToTurnOnOrOff.SetActive(value: true);
		}
	}
}
