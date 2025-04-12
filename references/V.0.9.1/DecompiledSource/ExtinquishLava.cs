using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtinquishLava : MonoBehaviour
{
	public TileObject lavaObject;

	private void OnEnable()
	{
		StartCoroutine(CheckForLavaAndExtinquish());
	}

	private IEnumerator CheckForLavaAndExtinquish()
	{
		if (!NetworkMapSharer.Instance.isServer)
		{
			yield break;
		}
		int startingX = Mathf.RoundToInt(base.transform.position.x / 2f);
		int startingY = Mathf.RoundToInt(base.transform.position.z / 2f);
		for (int radius = 1; radius <= 4; radius++)
		{
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			for (int i = -radius; i <= radius; i++)
			{
				for (int j = -radius; j <= radius; j++)
				{
					if (j * j + i * i <= radius * radius && CheckIfTileIsLava(startingX + j, startingY + i))
					{
						list.Add(startingX + j);
						list2.Add(startingY + i);
					}
				}
			}
			if (list.Count != 0)
			{
				NetworkMapSharer.Instance.RpcWaterExplodeOnLava(list.ToArray(), list2.ToArray());
			}
			yield return new WaitForSeconds(0.15f);
		}
	}

	private bool CheckIfTileIsLava(int xPos, int yPos)
	{
		if (WorldManager.Instance.isPositionOnMap(xPos, yPos) && WorldManager.Instance.onTileMap[xPos, yPos] == lavaObject.tileObjectId)
		{
			return true;
		}
		return false;
	}
}
