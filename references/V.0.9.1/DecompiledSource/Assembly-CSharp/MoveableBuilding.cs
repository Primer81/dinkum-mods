using System;
using UnityEngine;

[Serializable]
internal class MoveableBuilding
{
	private int xPos;

	private int yPos;

	private bool beingMoved;

	public MoveableBuilding(int xPosition, int yPosition)
	{
		xPos = xPosition;
		yPos = yPosition;
	}

	public bool isBeingUpgraded()
	{
		return WorldManager.Instance.onTileStatusMap[xPos, yPos] == 1;
	}

	public int getBuildingId()
	{
		return WorldManager.Instance.onTileMap[xPos, yPos];
	}

	public void moveBuildingToNewPos(int newPosX, int newPosY)
	{
		int buildingId = getBuildingId();
		int give = WorldManager.Instance.onTileStatusMap[xPos, yPos];
		if ((bool)WorldManager.Instance.allObjects[buildingId].displayPlayerHouseTiles)
		{
			HouseManager.manage.findHousesOnDisplay(xPos, yPos).MoveCarriablesOnHouseMove();
			NetworkMapSharer.Instance.RpcMoveHouseExterior(xPos, yPos, newPosX, newPosY);
			NetworkMapSharer.Instance.RpcMoveHouseInterior(xPos, yPos, newPosX, newPosY, WorldManager.Instance.rotationMap[xPos, yPos], WorldManager.Instance.rotationMap[newPosX, newPosY]);
			NetworkMapSharer.Instance.RpcGiveOnTileStatus(give, newPosX, newPosY);
		}
		NetworkMapSharer.Instance.RpcClearHouseForMove(xPos, yPos);
		NetworkMapSharer.Instance.RpcRemoveMultiTiledObject(buildingId, xPos, yPos, WorldManager.Instance.rotationMap[xPos, yPos]);
		int rotation = WorldManager.Instance.rotationMap[newPosX, newPosY];
		NetworkMapSharer.Instance.RpcSetRotation(newPosX, newPosY, rotation);
		NetworkMapSharer.Instance.RpcUpdateOnTileObject(buildingId, newPosX, newPosY);
		BuildingManager.manage.currentlyMoving = -1;
		NetworkMapSharer.Instance.NetworkmovingBuilding = -1;
		xPos = newPosX;
		yPos = newPosY;
	}

	public void doSpecialActionOnMove(int moveBuildingId, int movedToXPos, int movedToYPos)
	{
		if (moveBuildingId == 315)
		{
			NetworkMapSharer.Instance.NetworkprivateTowerPos = new Vector2(movedToXPos, movedToYPos);
		}
	}

	public bool checkIfBuildingIsPlayerHouse()
	{
		for (int i = 0; i < TownManager.manage.playerHouseStages.Length; i++)
		{
			if (TownManager.manage.playerHouseStages[i].placeable.tileObjectId == getBuildingId())
			{
				return true;
			}
		}
		return false;
	}
}
