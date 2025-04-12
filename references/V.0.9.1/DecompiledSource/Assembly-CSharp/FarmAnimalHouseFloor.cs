using UnityEngine;

public class FarmAnimalHouseFloor : MonoBehaviour
{
	public Transform sleepingSpot;

	public bool smallAnimalsOnly;

	public bool mediumAnimalsOnly;

	public int xPos;

	public int yPos;

	public TileObjectBridge isBridge;

	public WorldArea myArea;

	public void setXY(int newXPos, int newYPos)
	{
		xPos = newXPos;
		yPos = newYPos;
		myArea = RealWorldTimeLight.time.CurrentWorldArea;
	}

	public void setBridge(int xPos, int yPos)
	{
		if ((bool)isBridge)
		{
			isBridge.setUpBridge(WorldManager.Instance.onTileStatusMap[xPos, yPos]);
		}
	}

	public bool checkIfBridge()
	{
		return isBridge;
	}
}
