using TMPro;
using UnityEngine;

public class NPCMapAgent
{
	public class npcDesire
	{
		public NPCSchedual.Locations desiredLocation;

		private NPCMapAgent myMapAgent;

		private int npcId = -1;

		public NPCBuildingDoors lastWalkedInto;

		public NPCBuildingDoors wantToWalkInto;

		public Transform following;

		public Vector3 desiredPos;

		private int insideCheckTimer;

		public npcDesire(int newNpcId, NPCMapAgent newMapAgent)
		{
			myMapAgent = newMapAgent;
			npcId = newNpcId;
		}

		public void checkDesire(NPCAI NPCai)
		{
			if ((bool)following)
			{
				desiredLocation = following.GetComponent<CharMovement>().GetCurrentlyInsideBuilding();
			}
			else if (RealWorldTimeLight.time.currentHour < 24)
			{
				desiredLocation = NPCManager.manage.NPCDetails[npcId].mySchedual.getDesiredLocation(npcId, RealWorldTimeLight.time.currentHour, WorldManager.Instance.day);
			}
			else
			{
				desiredLocation = NPCManager.manage.NPCDetails[npcId].mySchedual.getDesiredLocation(npcId, 0, WorldManager.Instance.day);
			}
			setWantToWalkInto();
			desiredPos = myMapAgent.getPositionForLiveAgent();
			checkIfComplete(NPCai);
		}

		public void checkDesire(NPCAI NPCai, int hour, int dayToCheck)
		{
			desiredLocation = NPCManager.manage.NPCDetails[npcId].mySchedual.getDesiredLocation(npcId, hour, dayToCheck);
			desiredPos = myMapAgent.getPositionForLiveAgent();
			checkIfComplete(NPCai);
		}

		public void setWantToWalkInto()
		{
			wantToWalkInto = TownManager.manage.allShopFloors[(int)desiredLocation];
		}

		private bool CanExistOnLevel()
		{
			if ((bool)following)
			{
				return true;
			}
			if (!RealWorldTimeLight.time.underGround && !RealWorldTimeLight.time.offIsland)
			{
				return true;
			}
			return false;
		}

		private void checkIfComplete(NPCAI NPCai)
		{
			if (NPCai != null)
			{
				if (insideCheckTimer < 10)
				{
					insideCheckTimer++;
				}
				else if ((bool)lastWalkedInto && Mathf.Abs(NPCai.myAgent.transform.position.y - lastWalkedInto.inside.position.y) > 10f)
				{
					lastWalkedInto = null;
					wantToWalkInto = null;
				}
				if ((bool)lastWalkedInto && lastWalkedInto != TownManager.manage.allShopFloors[(int)desiredLocation])
				{
					if ((bool)lastWalkedInto && (bool)NPCai && NPCai.talkingTo == 0 && Vector3.Distance(NPCai.transform.position, lastWalkedInto.inside.position) < 1.5f)
					{
						insideCheckTimer = 0;
						if (CanExistOnLevel() && NPCai.checkPositionIsOnNavmesh(lastWalkedInto.outside.position))
						{
							NPCManager.manage.checkForNPCsInTheWayAndMakeThemMove(lastWalkedInto.outside);
							NPCai.myAgent.Warp(lastWalkedInto.outside.position);
							NPCai.transform.position = NPCai.myAgent.transform.position;
						}
						else
						{
							NPCai.myAgent.enabled = false;
							NPCai.myAgent.transform.position = lastWalkedInto.outside.position;
							NPCai.transform.position = NPCai.myAgent.transform.position;
							NetworkNavMesh.nav.UnSpawnNPCOnTile(NPCai);
						}
						lastWalkedInto = null;
						setWantToWalkInto();
					}
				}
				else if ((bool)wantToWalkInto && wantToWalkInto != lastWalkedInto)
				{
					if ((bool)NPCai && NPCai.talkingTo == 0 && Vector3.Distance(NPCai.transform.position, wantToWalkInto.outside.position) < 2f)
					{
						insideCheckTimer = 0;
						NPCManager.manage.checkForNPCsInTheWayAndMakeThemMove(wantToWalkInto.inside);
						NPCai.myAgent.Warp(wantToWalkInto.inside.position);
						NPCai.transform.position = NPCai.myAgent.transform.position;
						lastWalkedInto = wantToWalkInto;
						setWantToWalkInto();
					}
				}
				else
				{
					Vector3.Distance(NPCai.transform.position, new Vector3(desiredPos.x, WorldManager.Instance.heightMap[Mathf.RoundToInt(desiredPos.x / 2f), Mathf.RoundToInt(desiredPos.z / 2f)], desiredPos.z * 2f));
					_ = 4f;
				}
			}
			else if (Vector3.Distance(myMapAgent.currentPosition, desiredPos) <= 5f && (bool)wantToWalkInto)
			{
				lastWalkedInto = wantToWalkInto;
				NetworkNavMesh.nav.SpawnAnNPCFromMapToPlaceInBuilding(npcId, wantToWalkInto.inside.position);
				insideCheckTimer = 0;
			}
		}

		public void warpNpcToDesiredPos(NPCAI NPCai)
		{
			if ((bool)NPCai)
			{
				NPCai.myAgent.Warp(wantToWalkInto.inside.position);
				if (NPCai.myAgent.isActiveAndEnabled && NPCai.myAgent.isOnNavMesh)
				{
					NPCai.myAgent.SetDestination(wantToWalkInto.inside.position + wantToWalkInto.inside.forward * 2.5f);
				}
				else
				{
					NPCai.myAgent.transform.position = wantToWalkInto.inside.position;
				}
				NPCai.transform.position = NPCai.myAgent.transform.position;
				lastWalkedInto = wantToWalkInto;
			}
		}
	}

	public int npcId;

	public NPCAI activeNPC;

	private npcDesire desire;

	private float mapMoveTimer;

	public Vector3 currentPosition;

	private Vector3 currentlyMovingTo;

	public GameObject debugMarker;

	public NPCMapAgent(int npcNo, int startingX, int startingY)
	{
		npcId = npcNo;
		desire = new npcDesire(npcNo, this);
		currentPosition = new Vector3(startingX * 2, WorldManager.Instance.heightMap[startingX, startingY], startingY * 2);
	}

	public void setFollowing(Transform newFollowing)
	{
		desire.following = newFollowing;
	}

	public uint getFollowingId()
	{
		if ((bool)desire.following)
		{
			return desire.following.GetComponentInParent<CharMovement>().netId;
		}
		return 0u;
	}

	public bool isAtWork()
	{
		if (!desire.lastWalkedInto)
		{
			return false;
		}
		if (desire.lastWalkedInto.myLocation == NPCSchedual.Locations.Market_place)
		{
			return true;
		}
		return NPCManager.manage.NPCDetails[npcId].workLocation == desire.lastWalkedInto.myLocation;
	}

	public void saveNpcToMap(Vector3 currentPos)
	{
		currentPosition = currentPos;
		activeNPC = null;
	}

	public void removeSelf()
	{
		if ((bool)activeNPC)
		{
			NPCManager.manage.giveBackNpcDontSave(activeNPC);
		}
	}

	public void pullNpcFromMap(NPCAI myNPC)
	{
		activeNPC = myNPC;
	}

	private void getNewTarget()
	{
		desire.checkDesire(activeNPC);
		if (desire.desiredLocation != 0 || (bool)desire.following)
		{
			currentlyMovingTo = desire.desiredPos;
		}
		else
		{
			currentlyMovingTo = currentPosition;
		}
	}

	public bool hasDesiredRotation()
	{
		if ((bool)desire.wantToWalkInto && (bool)desire.wantToWalkInto.workPos && (bool)activeNPC && activeNPC.isAtWork() && desire.lastWalkedInto == TownManager.manage.allShopFloors[(int)desire.desiredLocation])
		{
			return true;
		}
		return false;
	}

	public Quaternion getDesiredRotation()
	{
		if ((bool)desire.wantToWalkInto && (bool)desire.wantToWalkInto.workPos && (bool)activeNPC && activeNPC.isAtWork())
		{
			return desire.wantToWalkInto.workPos.rotation;
		}
		return activeNPC.transform.rotation;
	}

	public void moveOffNavMesh(Vector3 positionToMoveTo)
	{
		activeNPC.myAgent.transform.position = positionToMoveTo;
		activeNPC.transform.position = positionToMoveTo;
		NetworkNavMesh.nav.UnSpawnNPCOnTile(activeNPC);
	}

	public Vector3 getPositionForLiveAgent()
	{
		desire.setWantToWalkInto();
		if ((bool)desire.following && !activeNPC)
		{
			return desire.following.position;
		}
		if (desire.desiredLocation == NPCSchedual.Locations.Exit || ((bool)desire.lastWalkedInto && desire.lastWalkedInto != desire.wantToWalkInto))
		{
			if ((bool)desire.lastWalkedInto)
			{
				return desire.lastWalkedInto.inside.position;
			}
			return Vector3.zero;
		}
		if (desire.desiredLocation == NPCSchedual.Locations.Wonder)
		{
			if (TownEventManager.manage.specialEventLocation == NPCSchedual.Locations.BandStand)
			{
				if (NPCManager.manage.npcStatus[npcId].checkIfHasMovedIn() && Vector3.Distance(TownManager.manage.allShopFloors[25].outside.transform.position, currentPosition) > 25f)
				{
					return TownManager.manage.allShopFloors[25].outside.transform.position + new Vector3(Random.Range(-12f, 12f), 0f, Random.Range(-12f, 12f));
				}
			}
			else if (Random.Range(0, 150) <= 3)
			{
				float num = Random.Range(40f, 80f);
				if (NPCManager.manage.npcStatus[npcId].checkIfHasMovedIn() && (bool)TownManager.manage.allShopFloors[(int)NPCManager.manage.NPCDetails[npcId].mySchedual.todaysSchedual[6]])
				{
					if (Vector3.Distance(TownManager.manage.allShopFloors[(int)NPCManager.manage.NPCDetails[npcId].mySchedual.todaysSchedual[6]].outside.transform.position, currentPosition) > num)
					{
						return TownManager.manage.allShopFloors[(int)NPCManager.manage.NPCDetails[npcId].mySchedual.todaysSchedual[6]].outside.transform.position + new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(-5f, 5f));
					}
				}
				else if (!NPCManager.manage.npcStatus[npcId].checkIfHasMovedIn() && (bool)TownManager.manage.allShopFloors[(int)NPCManager.manage.visitingSchedual[6]] && Vector3.Distance(TownManager.manage.allShopFloors[(int)NPCManager.manage.visitingSchedual[6]].outside.transform.position, currentPosition) > num)
				{
					return TownManager.manage.allShopFloors[(int)NPCManager.manage.visitingSchedual[6]].outside.transform.position + new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(-5f, 5f));
				}
			}
			return Vector3.zero;
		}
		if ((bool)desire.wantToWalkInto)
		{
			if (desire.lastWalkedInto == desire.wantToWalkInto)
			{
				if ((bool)desire.wantToWalkInto.workPos && (bool)activeNPC && activeNPC.isAtWork())
				{
					return desire.wantToWalkInto.workPos.position;
				}
				return Vector3.zero;
			}
			return desire.wantToWalkInto.outside.position;
		}
		return Vector3.zero;
	}

	public void movePosition()
	{
		getNewTarget();
		if (!activeNPC)
		{
			if (NetworkNavMesh.nav.doesPositionHaveNavChunk(Mathf.RoundToInt(currentPosition.x / 2f), Mathf.RoundToInt(currentPosition.z / 2f)))
			{
				Vector3 vector = NPCManager.manage.checkPositionIsOnNavmesh(new Vector3(currentPosition.x, WorldManager.Instance.heightMap[Mathf.RoundToInt(currentPosition.x / 2f), Mathf.RoundToInt(currentPosition.z / 2f)], currentPosition.z));
				if (vector != Vector3.zero)
				{
					currentPosition = vector;
					NetworkNavMesh.nav.SpawnAnNPCAtPosition(npcId, currentPosition);
				}
				else
				{
					moveTowardsDesiredPos();
				}
			}
		}
		else
		{
			currentPosition = activeNPC.transform.position;
		}
		if (!activeNPC)
		{
			if (mapMoveTimer > 2f || ((bool)desire.following && mapMoveTimer > 2f))
			{
				mapMoveTimer = 0f;
				moveTowardsDesiredPos();
			}
			else
			{
				mapMoveTimer += 1f;
			}
		}
		if ((bool)debugMarker)
		{
			if ((bool)activeNPC)
			{
				debugMarker.transform.position = activeNPC.transform.position;
			}
			else
			{
				debugMarker.transform.position = currentPosition;
			}
			string text = "Want to walk into: " + desire.wantToWalkInto?.ToString() + "\nLast walked into: " + desire.lastWalkedInto?.ToString() + "\nAt work:" + isAtWork();
			debugMarker.GetComponentInChildren<TextMeshPro>().text = text;
		}
	}

	private void moveTowardsDesiredPos()
	{
		if (currentlyMovingTo == Vector3.zero || currentlyMovingTo == currentPosition)
		{
			if (Random.Range(0, 20) == 2)
			{
				currentlyMovingTo = currentPosition + new Vector3(Random.Range(-4, 5), 0f, Random.Range(-4, 5));
				currentlyMovingTo = getWalkableTileForMapNpc(currentlyMovingTo, WorldManager.Instance.fencedOffMap[(int)currentPosition.x / 2, (int)currentPosition.z / 2]);
				if (!WorldManager.Instance.waterMap[(int)currentPosition.x / 2, (int)currentPosition.z / 2] && Mathf.Abs(WorldManager.Instance.heightMap[(int)currentPosition.x / 2, (int)currentPosition.z / 2] - WorldManager.Instance.heightMap[(int)currentlyMovingTo.x / 2, (int)currentlyMovingTo.z / 2]) <= 1)
				{
					currentPosition = currentlyMovingTo;
				}
			}
			return;
		}
		Vector3 vector = currentPosition;
		if (Vector3.Distance(currentPosition, currentlyMovingTo) > 8f)
		{
			if (Vector3.Distance(currentPosition, currentlyMovingTo) > 16f)
			{
				vector = currentPosition + (currentlyMovingTo - currentPosition).normalized * 8f;
				vector.y = WorldManager.Instance.heightMap[Mathf.RoundToInt(vector.x / 2f), Mathf.RoundToInt(vector.z / 2f)];
				vector = getWalkableTileForMapNpc(vector, WorldManager.Instance.fencedOffMap[(int)currentPosition.x / 2, (int)currentPosition.z / 2]);
				if (vector != Vector3.zero)
				{
					currentPosition = vector;
				}
			}
			else
			{
				currentPosition = currentlyMovingTo;
			}
		}
		else
		{
			currentPosition = currentlyMovingTo;
		}
	}

	public Vector3 getWalkableTileForMapNpc(Vector3 checkPos, int currentFencePos)
	{
		int num = Mathf.RoundToInt(checkPos.x / 2f);
		int num2 = Mathf.RoundToInt(checkPos.z / 2f);
		int num3 = 5;
		if (!WorldManager.Instance.isPositionOnMap(num, num2))
		{
			return Vector3.zero;
		}
		if (isSpaceStandable(num, num2, currentFencePos) && NPCManager.manage.pathFinder.CanReach(new Vector2Int(Mathf.RoundToInt(currentPosition.x / 2f), Mathf.RoundToInt(currentPosition.z / 2f)), new Vector2Int(num, num2)))
		{
			return checkPos;
		}
		Vector3 result = Vector3.zero;
		float num4 = num3 * 2;
		for (int i = -num3; i < num3; i++)
		{
			for (int j = -num3; j < num3; j++)
			{
				if (isSpaceStandable(num + i, num2 + j, currentFencePos) && NPCManager.manage.pathFinder.CanReach(new Vector2Int(Mathf.RoundToInt(currentPosition.x / 2f), Mathf.RoundToInt(currentPosition.z / 2f)), new Vector2Int(num + i, num2 + j)))
				{
					Vector3 vector = new Vector3((num + i) * 2, WorldManager.Instance.heightMap[num, num2], (num2 + j) * 2);
					float num5 = Vector3.Distance(checkPos, vector);
					if (num5 < num4)
					{
						result = vector;
						num4 = num5;
					}
				}
			}
		}
		return result;
	}

	private bool isSpaceStandable(int xPos, int yPos, int currentFencePos)
	{
		if (xPos == 0 && yPos == 0)
		{
			return true;
		}
		if (!WorldManager.Instance.isPositionOnMap(xPos, yPos))
		{
			return false;
		}
		if (currentFencePos <= 0 && WorldManager.Instance.fencedOffMap[xPos, yPos] > 0)
		{
			return false;
		}
		if ((!WorldManager.Instance.waterMap[xPos, yPos] || (WorldManager.Instance.waterMap[xPos, yPos] && WorldManager.Instance.heightMap[xPos, yPos] >= 0)) && (WorldManager.Instance.onTileMap[xPos, yPos] == -1 || (WorldManager.Instance.onTileMap[xPos, yPos] >= 0 && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].walkable)))
		{
			return true;
		}
		return false;
	}

	public void setBuildingCurrentlyIn(NPCSchedual.Locations locationToSet)
	{
		desire.lastWalkedInto = TownManager.manage.allShopFloors[(int)locationToSet];
	}

	public void warpNpcInside()
	{
		if ((bool)activeNPC)
		{
			activeNPC.StandUpAtEndOfDay();
		}
		if ((bool)activeNPC && (bool)desire.wantToWalkInto)
		{
			desire.warpNpcToDesiredPos(activeNPC);
		}
		else if (!activeNPC && (bool)desire.wantToWalkInto)
		{
			desire.lastWalkedInto = desire.wantToWalkInto;
			NetworkNavMesh.nav.SpawnAnNPCFromMapToPlaceInBuilding(npcId, desire.wantToWalkInto.inside.position);
		}
	}

	public bool checkIfNPCHasJustExited()
	{
		if (RealWorldTimeLight.time.currentMinute <= 5)
		{
			return desire.desiredLocation == NPCSchedual.Locations.Exit;
		}
		return false;
	}

	public NPCBuildingDoors getBuildingCurrentlyInside()
	{
		return desire.lastWalkedInto;
	}

	public NPCSchedual.Locations CurrentlyInLocation()
	{
		if (desire.lastWalkedInto == null)
		{
			return NPCSchedual.Locations.Wonder;
		}
		return desire.lastWalkedInto.myLocation;
	}

	public bool isInsideABuilding()
	{
		if (desire.lastWalkedInto != null && desire.desiredLocation != NPCManager.manage.NPCDetails[npcId].workLocation && desire.wantToWalkInto == desire.lastWalkedInto)
		{
			return true;
		}
		return false;
	}

	public bool checkIfNPCIsFree()
	{
		if ((bool)activeNPC && activeNPC.followingNetId != 0)
		{
			return true;
		}
		if ((bool)desire.lastWalkedInto && desire.wantToWalkInto != desire.lastWalkedInto)
		{
			return false;
		}
		if (desire.desiredLocation == NPCSchedual.Locations.Wonder)
		{
			return true;
		}
		if (desire.desiredLocation == NPCSchedual.Locations.Exit)
		{
			if (!desire.wantToWalkInto && !desire.lastWalkedInto)
			{
				return true;
			}
		}
		else if (desire.desiredLocation != NPCManager.manage.NPCDetails[npcId].workLocation && desire.desiredLocation != NPCSchedual.Locations.Market_place && desire.wantToWalkInto == desire.lastWalkedInto)
		{
			return true;
		}
		return false;
	}

	public void setNewDayDesire(bool useTomorrowsDate = false)
	{
		desire.following = null;
		desire.wantToWalkInto = null;
		desire.lastWalkedInto = null;
		desire.desiredLocation = NPCSchedual.Locations.Wonder;
		if (useTomorrowsDate)
		{
			desire.checkDesire(activeNPC, 6, RealWorldTimeLight.time.getTomorrowsDay());
		}
		else
		{
			desire.checkDesire(activeNPC, 6, WorldManager.Instance.day);
		}
		getPositionForLiveAgent();
	}

	public void clearLastWalkedIntoAndWantToWalkInto(NPCBuildingDoors location)
	{
		if (desire.lastWalkedInto == location)
		{
			desire.wantToWalkInto = null;
			desire.lastWalkedInto = null;
		}
	}
}
