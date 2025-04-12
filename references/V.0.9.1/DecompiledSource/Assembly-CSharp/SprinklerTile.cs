using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprinklerTile : MonoBehaviour
{
	public int horizontalSize;

	public int verticlSize;

	public bool isTank;

	public bool isSilo;

	public Animator anim;

	public TileObject myTileObject;

	public TileObject[] toFill;

	private bool routineRunning;

	private void OnEnable()
	{
		WorldManager.Instance.changeDayEvent.AddListener(startSprinkler);
		startSprinkler();
	}

	private void OnDisable()
	{
		WorldManager.Instance.changeDayEvent.RemoveListener(startSprinkler);
		routineRunning = false;
	}

	public void waterTiles(int xPos, int yPos, List<int[]> waterTanks)
	{
		if (isSilo)
		{
			if (!NetworkMapSharer.Instance.isServer)
			{
				return;
			}
			for (int i = -horizontalSize; i <= horizontalSize + 1; i++)
			{
				for (int j = -verticlSize; j <= verticlSize + 1; j++)
				{
					if (WorldManager.Instance.onTileStatusMap[xPos, yPos] < 1 || WorldManager.Instance.onTileMap[xPos + i, yPos + j] <= -1)
					{
						continue;
					}
					for (int k = 0; k < toFill.Length; k++)
					{
						if (WorldManager.Instance.onTileMap[xPos + i, yPos + j] == toFill[k].tileObjectId && WorldManager.Instance.onTileStatusMap[xPos + i, yPos + j] != 1)
						{
							NetworkMapSharer.Instance.RpcGiveOnTileStatus(1, xPos + i, yPos + j);
							WorldManager.Instance.onTileStatusMap[xPos + i, yPos + j] = 1;
							WorldManager.Instance.onTileStatusMap[xPos, yPos]--;
							break;
						}
					}
				}
			}
			NetworkMapSharer.Instance.RpcGiveOnTileStatus(WorldManager.Instance.onTileStatusMap[xPos, yPos], xPos, yPos);
			return;
		}
		bool flag = false;
		for (int l = 0; l < waterTanks.Count; l++)
		{
			int num = 0;
			int num2 = 0;
			if (xPos > waterTanks[l][0])
			{
				num = 1;
			}
			if (yPos > waterTanks[l][1])
			{
				num2 = 1;
			}
			if (Mathf.Abs(waterTanks[l][0] - xPos) <= WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[waterTanks[l][0], waterTanks[l][1]]].sprinklerTile.horizontalSize + num && Mathf.Abs(waterTanks[l][1] - yPos) <= WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[waterTanks[l][0], waterTanks[l][1]]].sprinklerTile.verticlSize + num2)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			WorldManager.Instance.onTileStatusMap[xPos, yPos] = 1;
			for (int m = -horizontalSize; m < horizontalSize + 1; m++)
			{
				for (int n = -verticlSize; n < verticlSize + 1; n++)
				{
					if (WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos + m, yPos + n]].wetVersion != -1)
					{
						WorldManager.Instance.tileTypeMap[xPos + m, yPos + n] = WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos + m, yPos + n]].wetVersion;
						WorldManager.Instance.chunkHasChangedToday[Mathf.RoundToInt((xPos + m) / 10), Mathf.RoundToInt((yPos + n) / 10)] = true;
					}
					if (WorldManager.Instance.onTileMap[xPos + m, yPos + n] != -1)
					{
						continue;
					}
					if (WorldManager.Instance.tileTypeMap[xPos + m, yPos + n] == 1)
					{
						if (NetworkMapSharer.Instance.isServer && Random.Range(0, 10) == 5)
						{
							NetworkMapSharer.Instance.RpcUpdateOnTileObject(403, xPos + m, yPos + n);
							NetworkMapSharer.Instance.RpcGiveOnTileStatus(2, xPos + m, yPos + n);
						}
						else if (WorldManager.Instance.onTileMap[xPos + m, yPos + n] == -1)
						{
							WorldManager.Instance.onTileMap[xPos + m, yPos + n] = GenerateMap.generate.bushLandGrowBack.objectsInBiom[0].tileObjectId;
						}
						WorldManager.Instance.chunkHasChangedToday[Mathf.RoundToInt((xPos + m) / 10), Mathf.RoundToInt((yPos + n) / 10)] = true;
					}
					if (WorldManager.Instance.tileTypeMap[xPos + m, yPos + n] == 4)
					{
						if (NetworkMapSharer.Instance.isServer && Random.Range(0, 7) == 5)
						{
							NetworkMapSharer.Instance.RpcUpdateOnTileObject(407, xPos + m, yPos + n);
							NetworkMapSharer.Instance.RpcGiveOnTileStatus(2, xPos + m, yPos + n);
						}
						else if (WorldManager.Instance.onTileMap[xPos + m, yPos + n] == -1)
						{
							WorldManager.Instance.onTileMap[xPos + m, yPos + n] = GenerateMap.generate.tropicalGrowBack.objectsInBiom[0].tileObjectId;
							WorldManager.Instance.onTileStatusMap[xPos + m, yPos + n] = 2;
						}
						WorldManager.Instance.chunkHasChangedToday[Mathf.RoundToInt((xPos + m) / 10), Mathf.RoundToInt((yPos + n) / 10)] = true;
					}
					if (WorldManager.Instance.tileTypeMap[xPos + m, yPos + n] == 15)
					{
						if (NetworkMapSharer.Instance.isServer && Random.Range(0, 10) == 5)
						{
							NetworkMapSharer.Instance.RpcUpdateOnTileObject(402, xPos + m, yPos + n);
							NetworkMapSharer.Instance.RpcGiveOnTileStatus(2, xPos + m, yPos + n);
						}
						else if (WorldManager.Instance.onTileMap[xPos + m, yPos + n] == -1)
						{
							WorldManager.Instance.onTileMap[xPos + m, yPos + n] = GenerateMap.generate.coldLandGrowBack.objectsInBiom[0].tileObjectId;
						}
						WorldManager.Instance.chunkHasChangedToday[Mathf.RoundToInt((xPos + m) / 10), Mathf.RoundToInt((yPos + n) / 10)] = true;
					}
				}
			}
			if (NetworkMapSharer.Instance.isServer)
			{
				WorldManager.Instance.sprinkerContinuesToWater(xPos, yPos);
			}
		}
		else
		{
			WorldManager.Instance.onTileStatusMap[xPos, yPos] = 0;
		}
	}

	public void startSprinkler()
	{
		if (!isTank && !isSilo && !routineRunning)
		{
			routineRunning = true;
			StartCoroutine(sprinklerAnim());
		}
	}

	private IEnumerator sprinklerAnim()
	{
		while (myTileObject.xPos == 0 && myTileObject.xPos == 0)
		{
			yield return null;
		}
		if (RealWorldTimeLight.time.currentHour >= 1 && RealWorldTimeLight.time.currentHour < 9 && WorldManager.Instance.onTileStatusMap[myTileObject.xPos, myTileObject.yPos] == 1)
		{
			anim.SetFloat("Offset", Random.Range(0f, 1f));
			anim.SetBool("SprinklerOn", value: true);
			while (RealWorldTimeLight.time.currentHour >= 1 && RealWorldTimeLight.time.currentHour < 9)
			{
				yield return null;
			}
			yield return new WaitForSeconds(Random.Range(0f, 1.5f));
			anim.SetBool("SprinklerOn", value: false);
		}
		routineRunning = false;
		WorldManager.Instance.onTileStatusMap[myTileObject.xPos, myTileObject.yPos] = 0;
	}
}
