using System.Collections;
using UnityEngine;

public class MapStorer : MonoBehaviour
{
	public enum LoadMapType
	{
		Overworld,
		Underworld,
		OffIsland
	}

	public static MapStorer store;

	public int[,] overWorldHeight;

	public int[,] overWorldOnTile;

	public int[,] overWorldTileType;

	public int[,] overWorldOnTileStatus;

	public int[,] overWorldTileTypeStatus;

	public int[,] overWorldRotationMap;

	public bool[,] overWorldWaterMap;

	public bool[,] overWorldChangedMap;

	public bool[,] overWorldWaterChangeMap;

	public bool[,] overWorldHeightChangeMap;

	public bool[,] overWorldOnTileChangedMap;

	public bool[,] overWorldTileTypeChangedMap;

	public int[,] underWorldHeight;

	public int[,] underWorldOnTile;

	public int[,] underWorldTileType;

	public int[,] underWorldOnTileStatus;

	public int[,] underWorldTileTypeStatus;

	public int[,] underWorldRotationMap;

	public bool[,] underWorldWaterMap;

	public bool[,] underWorldChangedMap;

	public bool[,] underWorldHeightChangedMap;

	public bool[,] underWorldWaterChangedMap;

	public bool[,] underworldOnTileChangedMap;

	public bool[,] underworldTileTypeChangedMap;

	public int[,] offIslandHeight;

	public int[,] offIslandOnTile;

	public int[,] offIslandTileType;

	public int[,] offIslandOnTileStatus;

	public int[,] offIslandTileTypeStatus;

	public int[,] offIslandRotationMap;

	public bool[,] offIslandWaterMap;

	public bool[,] offIslandChangedMap;

	public bool[,] offIslandHeightChangedMap;

	public bool[,] offIslandWaterChangedMap;

	public bool[,] offIslandOnTileChangedMap;

	public bool[,] offIslandTileTypeChangedMap;

	public bool overWorldStored;

	public bool waitingForMapToStore;

	public bool waitForMapToLoad;

	private bool underworldMapArraysCreated;

	private bool offIslandMapArraysCreated;

	private WaitForEndOfFrame wait = new WaitForEndOfFrame();

	private void Awake()
	{
		store = this;
	}

	public void CreateUnderworldMapArrays()
	{
		if (!underworldMapArraysCreated)
		{
			underWorldHeight = new int[WorldManager.Instance.GetMapSize(), WorldManager.Instance.GetMapSize()];
			underWorldOnTile = new int[WorldManager.Instance.GetMapSize(), WorldManager.Instance.GetMapSize()];
			underWorldTileType = new int[WorldManager.Instance.GetMapSize(), WorldManager.Instance.GetMapSize()];
			underWorldOnTileStatus = new int[WorldManager.Instance.GetMapSize(), WorldManager.Instance.GetMapSize()];
			underWorldTileTypeStatus = new int[WorldManager.Instance.GetMapSize(), WorldManager.Instance.GetMapSize()];
			underWorldRotationMap = new int[WorldManager.Instance.GetMapSize(), WorldManager.Instance.GetMapSize()];
			underWorldWaterMap = new bool[WorldManager.Instance.GetMapSize(), WorldManager.Instance.GetMapSize()];
			underWorldChangedMap = new bool[WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize(), WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize()];
			underWorldHeightChangedMap = new bool[WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize(), WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize()];
			underWorldWaterChangedMap = new bool[WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize(), WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize()];
			underworldOnTileChangedMap = new bool[WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize(), WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize()];
			underworldTileTypeChangedMap = new bool[WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize(), WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize()];
			underworldMapArraysCreated = true;
		}
	}

	public void CreateOffIslandMapArrays()
	{
		if (!offIslandMapArraysCreated)
		{
			offIslandHeight = new int[WorldManager.Instance.GetMapSize(), WorldManager.Instance.GetMapSize()];
			offIslandOnTile = new int[WorldManager.Instance.GetMapSize(), WorldManager.Instance.GetMapSize()];
			offIslandTileType = new int[WorldManager.Instance.GetMapSize(), WorldManager.Instance.GetMapSize()];
			offIslandOnTileStatus = new int[WorldManager.Instance.GetMapSize(), WorldManager.Instance.GetMapSize()];
			offIslandTileTypeStatus = new int[WorldManager.Instance.GetMapSize(), WorldManager.Instance.GetMapSize()];
			offIslandRotationMap = new int[WorldManager.Instance.GetMapSize(), WorldManager.Instance.GetMapSize()];
			offIslandWaterMap = new bool[WorldManager.Instance.GetMapSize(), WorldManager.Instance.GetMapSize()];
			offIslandChangedMap = new bool[WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize(), WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize()];
			offIslandHeightChangedMap = new bool[WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize(), WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize()];
			offIslandWaterChangedMap = new bool[WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize(), WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize()];
			offIslandOnTileChangedMap = new bool[WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize(), WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize()];
			offIslandTileTypeChangedMap = new bool[WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize(), WorldManager.Instance.GetMapSize() / WorldManager.Instance.getChunkSize()];
			offIslandMapArraysCreated = true;
		}
	}

	public void storeMap(LoadMapType typeToStore)
	{
		StartCoroutine(storeMapWithDelay(typeToStore));
	}

	private IEnumerator storeMapWithDelay(LoadMapType typeToStore)
	{
		waitingForMapToStore = true;
		switch (typeToStore)
		{
		case LoadMapType.Overworld:
			overWorldHeight = WorldManager.Instance.heightMap;
			overWorldOnTile = WorldManager.Instance.onTileMap;
			overWorldTileType = WorldManager.Instance.tileTypeMap;
			yield return wait;
			overWorldOnTileStatus = WorldManager.Instance.onTileStatusMap;
			overWorldTileTypeStatus = WorldManager.Instance.tileTypeStatusMap;
			overWorldRotationMap = WorldManager.Instance.rotationMap;
			yield return wait;
			overWorldChangedMap = WorldManager.Instance.chunkChangedMap;
			yield return wait;
			overWorldHeightChangeMap = WorldManager.Instance.changedMapHeight;
			overWorldWaterChangeMap = WorldManager.Instance.changedMapWater;
			overWorldOnTileChangedMap = WorldManager.Instance.changedMapOnTile;
			overWorldTileTypeChangedMap = WorldManager.Instance.changedMapTileType;
			overWorldWaterMap = WorldManager.Instance.waterMap;
			overWorldStored = true;
			break;
		case LoadMapType.Underworld:
			underWorldHeight = WorldManager.Instance.heightMap;
			underWorldOnTile = WorldManager.Instance.onTileMap;
			underWorldTileType = WorldManager.Instance.tileTypeMap;
			yield return wait;
			underWorldOnTileStatus = WorldManager.Instance.onTileStatusMap;
			underWorldTileTypeStatus = WorldManager.Instance.tileTypeStatusMap;
			underWorldRotationMap = WorldManager.Instance.rotationMap;
			yield return wait;
			underWorldChangedMap = WorldManager.Instance.chunkChangedMap;
			yield return wait;
			underWorldHeightChangedMap = WorldManager.Instance.changedMapHeight;
			underWorldWaterChangedMap = WorldManager.Instance.changedMapWater;
			underworldOnTileChangedMap = WorldManager.Instance.changedMapOnTile;
			underworldTileTypeChangedMap = WorldManager.Instance.changedMapTileType;
			underWorldWaterMap = WorldManager.Instance.waterMap;
			break;
		case LoadMapType.OffIsland:
			offIslandHeight = WorldManager.Instance.heightMap;
			offIslandOnTile = WorldManager.Instance.onTileMap;
			offIslandTileType = WorldManager.Instance.tileTypeMap;
			yield return wait;
			offIslandOnTileStatus = WorldManager.Instance.onTileStatusMap;
			offIslandTileTypeStatus = WorldManager.Instance.tileTypeStatusMap;
			offIslandRotationMap = WorldManager.Instance.rotationMap;
			yield return wait;
			offIslandChangedMap = WorldManager.Instance.chunkChangedMap;
			yield return wait;
			offIslandHeightChangedMap = WorldManager.Instance.changedMapHeight;
			offIslandWaterChangedMap = WorldManager.Instance.changedMapWater;
			offIslandOnTileChangedMap = WorldManager.Instance.changedMapOnTile;
			offIslandTileTypeChangedMap = WorldManager.Instance.changedMapTileType;
			offIslandWaterMap = WorldManager.Instance.waterMap;
			break;
		}
		waitingForMapToStore = false;
	}

	public void loadStoredMap(LoadMapType typeToLoad)
	{
		StartCoroutine(loadStoredMapWithDelay(typeToLoad));
	}

	private IEnumerator loadStoredMapWithDelay(LoadMapType typeToLoad)
	{
		waitForMapToLoad = true;
		switch (typeToLoad)
		{
		case LoadMapType.Overworld:
			WorldManager.Instance.heightMap = overWorldHeight;
			WorldManager.Instance.onTileMap = overWorldOnTile;
			WorldManager.Instance.tileTypeMap = overWorldTileType;
			yield return wait;
			WorldManager.Instance.onTileStatusMap = overWorldOnTileStatus;
			WorldManager.Instance.tileTypeStatusMap = overWorldTileTypeStatus;
			WorldManager.Instance.rotationMap = overWorldRotationMap;
			yield return wait;
			WorldManager.Instance.chunkChangedMap = overWorldChangedMap;
			yield return wait;
			WorldManager.Instance.changedMapHeight = overWorldHeightChangeMap;
			WorldManager.Instance.changedMapWater = overWorldWaterChangeMap;
			WorldManager.Instance.changedMapOnTile = overWorldOnTileChangedMap;
			WorldManager.Instance.changedMapTileType = overWorldTileTypeChangedMap;
			WorldManager.Instance.waterMap = overWorldWaterMap;
			break;
		case LoadMapType.Underworld:
			WorldManager.Instance.heightMap = underWorldHeight;
			WorldManager.Instance.onTileMap = underWorldOnTile;
			WorldManager.Instance.tileTypeMap = underWorldTileType;
			yield return wait;
			WorldManager.Instance.onTileStatusMap = underWorldOnTileStatus;
			WorldManager.Instance.tileTypeStatusMap = underWorldTileTypeStatus;
			WorldManager.Instance.rotationMap = underWorldRotationMap;
			yield return wait;
			WorldManager.Instance.chunkChangedMap = underWorldChangedMap;
			yield return wait;
			WorldManager.Instance.changedMapHeight = underWorldHeightChangedMap;
			WorldManager.Instance.changedMapWater = underWorldWaterChangedMap;
			WorldManager.Instance.changedMapOnTile = underworldOnTileChangedMap;
			WorldManager.Instance.changedMapTileType = underworldTileTypeChangedMap;
			WorldManager.Instance.waterMap = underWorldWaterMap;
			break;
		case LoadMapType.OffIsland:
			WorldManager.Instance.heightMap = offIslandHeight;
			WorldManager.Instance.onTileMap = offIslandOnTile;
			WorldManager.Instance.tileTypeMap = offIslandTileType;
			yield return wait;
			WorldManager.Instance.onTileStatusMap = offIslandOnTileStatus;
			WorldManager.Instance.tileTypeStatusMap = offIslandTileTypeStatus;
			WorldManager.Instance.rotationMap = offIslandRotationMap;
			yield return wait;
			WorldManager.Instance.chunkChangedMap = offIslandChangedMap;
			yield return wait;
			WorldManager.Instance.changedMapHeight = offIslandHeightChangedMap;
			WorldManager.Instance.changedMapWater = offIslandWaterChangedMap;
			WorldManager.Instance.changedMapOnTile = offIslandOnTileChangedMap;
			WorldManager.Instance.changedMapTileType = offIslandTileTypeChangedMap;
			WorldManager.Instance.waterMap = offIslandWaterMap;
			break;
		}
		waitForMapToLoad = false;
	}

	public void getStoredMineMapForConnect()
	{
		WorldManager.Instance.heightMap = underWorldHeight;
		WorldManager.Instance.onTileMap = underWorldOnTile;
		WorldManager.Instance.tileTypeMap = underWorldTileType;
		WorldManager.Instance.onTileStatusMap = underWorldOnTileStatus;
		WorldManager.Instance.tileTypeStatusMap = underWorldTileTypeStatus;
		WorldManager.Instance.rotationMap = underWorldRotationMap;
		WorldManager.Instance.chunkChangedMap = underWorldChangedMap;
		WorldManager.Instance.changedMapHeight = underWorldHeightChangedMap;
		WorldManager.Instance.changedMapWater = underWorldWaterChangedMap;
		WorldManager.Instance.changedMapOnTile = underworldOnTileChangedMap;
		WorldManager.Instance.changedMapTileType = underworldTileTypeChangedMap;
		WorldManager.Instance.waterMap = underWorldWaterMap;
		NetworkMapSharer.Instance.onChangeMaps.Invoke();
	}

	public void getStoredOffIslandMapForConnect()
	{
		WorldManager.Instance.heightMap = offIslandHeight;
		WorldManager.Instance.onTileMap = offIslandOnTile;
		WorldManager.Instance.tileTypeMap = offIslandTileType;
		WorldManager.Instance.onTileStatusMap = offIslandOnTileStatus;
		WorldManager.Instance.tileTypeStatusMap = offIslandTileTypeStatus;
		WorldManager.Instance.rotationMap = offIslandRotationMap;
		WorldManager.Instance.chunkChangedMap = offIslandChangedMap;
		WorldManager.Instance.changedMapHeight = offIslandHeightChangedMap;
		WorldManager.Instance.changedMapWater = offIslandWaterChangedMap;
		WorldManager.Instance.changedMapOnTile = offIslandOnTileChangedMap;
		WorldManager.Instance.changedMapTileType = offIslandTileTypeChangedMap;
		WorldManager.Instance.waterMap = offIslandWaterMap;
		NetworkMapSharer.Instance.onChangeMaps.Invoke();
	}
}
