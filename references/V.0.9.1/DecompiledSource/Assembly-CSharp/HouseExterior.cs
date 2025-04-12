using System;

[Serializable]
public class HouseExterior
{
	public int xPos = -1;

	public int yPos = -1;

	public bool playerHouse;

	public int houseLevel;

	public int roof;

	public int houseBase;

	public int windows;

	public int door;

	public int fence;

	public int wallMat;

	public string wallColor = "#FFFFFF";

	public int roofMat;

	public string roofColor = "#FFFFFF";

	public int houseMat;

	public string houseColor = "#557D57";

	public string houseName = "";

	public HouseExterior(int newXPos, int newYPos)
	{
		if ((bool)HouseManager.manage)
		{
			xPos = newXPos;
			yPos = newYPos;
			if (HouseManager.manage.checkIfHouseIsPlayersHouse(newXPos, newYPos))
			{
				houseName = string.Format(ConversationGenerator.generate.GetBuildingName("Players House"), Inventory.Instance.playerName);
			}
			else
			{
				houseName = ConversationGenerator.generate.GetBuildingName("Guest House");
			}
		}
	}

	public HouseExterior()
	{
	}

	public void copyToAnotherHouseExterior(HouseExterior copyTo)
	{
		copyTo.roof = roof;
		copyTo.houseBase = houseBase;
		copyTo.windows = windows;
		copyTo.door = door;
		copyTo.wallMat = wallMat;
		copyTo.wallColor = wallColor;
		copyTo.roofMat = roofMat;
		copyTo.roofColor = roofColor;
		copyTo.houseMat = houseMat;
		copyTo.houseColor = houseColor;
		copyTo.houseLevel = houseLevel;
		copyTo.fence = fence;
		copyTo.houseName = houseName;
	}

	public void copyFromAnotherHouseExterior(HouseExterior copyFrom)
	{
		roof = copyFrom.roof;
		houseBase = copyFrom.houseBase;
		windows = copyFrom.windows;
		door = copyFrom.door;
		wallMat = copyFrom.wallMat;
		wallColor = copyFrom.wallColor;
		roofMat = copyFrom.roofMat;
		roofColor = copyFrom.roofColor;
		houseMat = copyFrom.houseMat;
		houseColor = copyFrom.houseColor;
		houseLevel = copyFrom.houseLevel;
		fence = copyFrom.fence;
		houseName = copyFrom.houseName;
	}
}
