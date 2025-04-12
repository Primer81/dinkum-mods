using System.Collections;
using UnityEngine;

public class MatchMaterialOfTileType : MonoBehaviour
{
	public TileObject myTileObject;

	public MeshRenderer renToSwap;

	private int showing = -1;

	private void OnEnable()
	{
		StartCoroutine(WaitForPlacement());
	}

	private IEnumerator WaitForPlacement()
	{
		yield return null;
		if ((bool)WorldManager.Instance && WorldManager.Instance.isPositionOnMap(myTileObject.xPos, myTileObject.yPos) && (bool)myTileObject && (bool)renToSwap)
		{
			int num = Mathf.Clamp(WorldManager.Instance.tileTypeMap[myTileObject.xPos, myTileObject.yPos], 0, WorldManager.Instance.tileTypes.Length - 1);
			if (showing != num)
			{
				renToSwap.material = WorldManager.Instance.tileTypes[num].myTileMaterial;
			}
		}
	}
}
