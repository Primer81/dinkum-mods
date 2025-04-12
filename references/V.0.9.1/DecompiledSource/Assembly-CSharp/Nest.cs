using System;
using UnityEngine;

[Serializable]
public class Nest
{
	public int xPos;

	public int yPos;

	public int daysSinceEgg;

	public Nest()
	{
	}

	public Nest(int checkX, int checkY)
	{
		xPos = checkX;
		yPos = checkY;
		daysSinceEgg = 5;
	}

	public bool check(int checkX, int checkY)
	{
		if (checkX == xPos && checkY == yPos)
		{
			return true;
		}
		return false;
	}

	public bool canHaveEgg()
	{
		return daysSinceEgg > 3;
	}

	public void addDaySinceEgg()
	{
		daysSinceEgg++;
	}

	public bool isEggNearby()
	{
		for (int i = 0; i < WorldManager.Instance.allCarriables.Count; i++)
		{
			if (WorldManager.Instance.allCarriables[i] != null && WorldManager.Instance.allCarriables[i].prefabId == 3 && Vector3.Distance(WorldManager.Instance.allCarriables[i].transform.position, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2)) < 6f)
			{
				return true;
			}
		}
		return false;
	}

	public void giveEgg()
	{
		daysSinceEgg = 0;
	}
}
