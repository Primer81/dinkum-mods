using System;
using UnityEngine;

[Serializable]
public class BuriedItem
{
	public int xPos;

	public int yPos;

	public int heightBuriedAt;

	public int itemId;

	public int stackedAmount = 1;

	public void digUpItem()
	{
		NetworkMapSharer.Instance.spawnAServerDrop(itemId, stackedAmount, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
		BuriedManager.manage.allBuriedItems.Remove(this);
	}

	public BuriedItem(int itemNo, int stack, int xP, int yP)
	{
		itemId = itemNo;
		stackedAmount = stack;
		xPos = xP;
		yPos = yP;
	}

	public bool matches(int checkX, int checkY)
	{
		if (checkX == xPos && checkY == yPos)
		{
			return true;
		}
		return false;
	}

	public BuriedItem()
	{
	}
}
