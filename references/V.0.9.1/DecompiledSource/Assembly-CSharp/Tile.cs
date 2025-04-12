using UnityEngine;

public class Tile : MonoBehaviour
{
	private int showingX;

	private int showingY;

	public MeshFilter filt;

	public TileObject onThisTile;

	public Transform myWater;

	public Transform _transform;

	private bool showingWater;

	private int showingMesh = -1;

	private int showingHeight;

	private int[,] neighboursHeight = new int[3, 3];

	private int[,] neighboursType = new int[3, 3];

	public MeshFilter waterFilter;

	private int showingExtentionInt;

	public void refreshTile(int newShowingX, int newShowingY)
	{
		if (newShowingX == 0 || newShowingY == 0 || newShowingX == WorldManager.Instance.GetMapSize() - 1 || newShowingY == WorldManager.Instance.GetMapSize() - 1)
		{
			setTileToOceanFloor();
			showingX = newShowingX;
			showingY = newShowingY;
			return;
		}
		if (WorldManager.Instance.waterMap[newShowingX, newShowingY])
		{
			if (!showingWater)
			{
				showingWater = true;
				myWater.gameObject.SetActive(value: true);
			}
			myWater.localPosition = new Vector3(0f, -WorldManager.Instance.heightMap[newShowingX, newShowingY] + 1, 0f);
		}
		else if (showingWater)
		{
			showingWater = false;
			myWater.gameObject.SetActive(value: false);
		}
		if (showingX != newShowingX || showingY != newShowingY)
		{
			clearNeighboursDetails();
		}
		showingX = newShowingX;
		showingY = newShowingY;
		tileRefresh();
	}

	public void fillNeighbourDetails()
	{
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (WorldManager.Instance.isPositionOnMap(showingX - 1 + j, showingY - 1 + i))
				{
					neighboursHeight[j, i] = WorldManager.Instance.heightMap[showingX - 1 + j, showingY - 1 + i];
					neighboursType[j, i] = WorldManager.Instance.tileTypeMap[showingX - 1 + j, showingY - 1 + i];
				}
				else
				{
					neighboursHeight[j, i] = -2;
					neighboursType[j, i] = 3;
				}
			}
		}
	}

	public void clearNeighboursDetails()
	{
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				neighboursType[j, i] = -1;
				neighboursHeight[j, i] = -7;
			}
		}
	}

	public bool checkIfNeihbourDetailsChanged()
	{
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (neighboursType[j, i] != WorldManager.Instance.tileTypeMap[showingX - 1 + j, showingY - 1 + i] || neighboursHeight[j, i] != WorldManager.Instance.heightMap[showingX - 1 + j, showingY - 1 + i])
				{
					fillNeighbourDetails();
					return true;
				}
			}
		}
		return false;
	}

	public void returnTileObjects()
	{
		if ((bool)onThisTile)
		{
			WorldManager.Instance.returnTileObject(onThisTile);
			onThisTile = null;
		}
	}

	public void checkHeight()
	{
		if (showingHeight != WorldManager.Instance.heightMap[showingX, showingY])
		{
			_transform.localPosition = new Vector3(_transform.localPosition.x, WorldManager.Instance.heightMap[showingX, showingY], _transform.localPosition.z);
			showingHeight = WorldManager.Instance.heightMap[showingX, showingY];
		}
	}

	public void refreshTileObjects()
	{
		if (showingX == 0 || showingY == 0 || showingX == WorldManager.Instance.GetMapSize() - 1 || showingY == WorldManager.Instance.GetMapSize() - 1)
		{
			setTileToOceanFloor();
		}
		else
		{
			checkOnTileObject();
		}
	}

	public void tileRefresh()
	{
		if (checkIfNeihbourDetailsChanged())
		{
			checkNeighboursAndChangeMesh();
			checkNeighboursAndChangeMaterial();
		}
		checkOnTileObject();
	}

	public void setTileToOceanFloor()
	{
		showingMesh = 10;
		if ((bool)onThisTile)
		{
			WorldManager.Instance.returnTileObject(onThisTile);
		}
		showingHeight = -2;
		_transform.localPosition = new Vector3(_transform.localPosition.x, -2f, _transform.localPosition.z);
		myWater.localPosition = new Vector3(0f, 3f, 0f);
		changeMaterial(WorldManager.Instance.tileTypes[3], new Vector2(1f, 2f), 0);
		myWater.gameObject.SetActive(value: true);
		showingWater = true;
	}

	public void changeDayRefresh()
	{
	}

	public bool refreshForNeighbours()
	{
		bool result = false;
		if (checkIfNeihbourDetailsChanged())
		{
			checkNeighboursAndChangeMesh();
			checkNeighboursAndChangeMaterial();
			result = true;
		}
		checkOnTileObject();
		return result;
	}

	public void checkNeighboursAndChangeMesh()
	{
		bool flag = checkIfSameAsNeighbour(showingX - 1, showingY);
		bool flag2 = checkIfSameAsNeighbour(showingX + 1, showingY);
		bool flag3 = checkIfSameAsNeighbour(showingX, showingY + 1);
		bool flag4 = checkIfSameAsNeighbour(showingX, showingY - 1);
		if (showingHeight != WorldManager.Instance.heightMap[showingX, showingY])
		{
			_transform.localPosition = new Vector3(_transform.localPosition.x, WorldManager.Instance.heightMap[showingX, showingY], _transform.localPosition.z);
			showingHeight = WorldManager.Instance.heightMap[showingX, showingY];
		}
		if (flag && flag2 && flag3 && flag4)
		{
			bool flag5 = checkIfSameAsNeighbour(showingX + 1, showingY + 1);
			bool flag6 = checkIfSameAsNeighbour(showingX - 1, showingY + 1);
			bool flag7 = checkIfSameAsNeighbour(showingX - 1, showingY - 1);
			bool flag8 = checkIfSameAsNeighbour(showingX + 1, showingY - 1);
			if (flag7 && flag8 && flag5 && flag6)
			{
				showingMesh = 10;
			}
			else if (!flag7 && !flag8 && !flag5 && !flag6)
			{
				showingMesh = 0;
			}
			else if (!flag7 && flag8 && !flag5 && flag6)
			{
				showingMesh = 43;
			}
			else if (flag7 && !flag8 && flag5 && !flag6)
			{
				showingMesh = 42;
			}
			else if (flag7 && !flag8 && !flag5 && !flag6)
			{
				showingMesh = 44;
			}
			else if (!flag7 && flag8 && !flag5 && !flag6)
			{
				showingMesh = 45;
			}
			else if (!flag7 && !flag8 && !flag5 && flag6)
			{
				showingMesh = 46;
			}
			else if (!flag7 && !flag8 && flag5 && !flag6)
			{
				showingMesh = 47;
			}
			else if (!flag7 && !flag8 && flag5 && !flag6)
			{
				showingMesh = 47;
			}
			else if (!flag7 && !flag8)
			{
				showingMesh = 1;
			}
			else if (!flag5 && !flag6)
			{
				showingMesh = 2;
			}
			else if (!flag5 && !flag8)
			{
				showingMesh = 3;
			}
			else if (!flag6 && !flag7)
			{
				showingMesh = 4;
			}
			else if (!flag5)
			{
				showingMesh = 5;
			}
			else if (!flag6)
			{
				showingMesh = 6;
			}
			else if (!flag7)
			{
				showingMesh = 7;
			}
			else if (!flag8)
			{
				showingMesh = 8;
			}
			else
			{
				showingMesh = 10;
			}
		}
		else if (!flag && !flag2 && !flag3 && flag4)
		{
			showingMesh = 11;
		}
		else if (!flag && !flag2 && flag3 && !flag4)
		{
			showingMesh = 12;
		}
		else if (!flag && flag2 && !flag3 && !flag4)
		{
			showingMesh = 13;
		}
		else if (flag && !flag2 && !flag3 && !flag4)
		{
			showingMesh = 14;
		}
		else if (!flag && flag2 && !flag3 && flag4)
		{
			if (!checkIfSameAsNeighbour(showingX + 1, showingY - 1))
			{
				showingMesh = 15;
			}
			else
			{
				showingMesh = 16;
			}
		}
		else if (flag && !flag2 && !flag3 && flag4)
		{
			if (!checkIfSameAsNeighbour(showingX - 1, showingY - 1))
			{
				showingMesh = 17;
			}
			else
			{
				showingMesh = 18;
			}
		}
		else if (!flag && flag2 && flag3 && !flag4)
		{
			if (!checkIfSameAsNeighbour(showingX + 1, showingY + 1))
			{
				showingMesh = 19;
			}
			else
			{
				showingMesh = 20;
			}
		}
		else if (flag && !flag2 && flag3 && !flag4)
		{
			if (!checkIfSameAsNeighbour(showingX - 1, showingY + 1))
			{
				showingMesh = 21;
			}
			else
			{
				showingMesh = 22;
			}
		}
		else if (!flag && flag2 && flag3 && flag4)
		{
			bool flag9 = checkIfSameAsNeighbour(showingX + 1, showingY + 1);
			bool flag10 = checkIfSameAsNeighbour(showingX + 1, showingY - 1);
			if (flag9 && flag10)
			{
				showingMesh = 23;
			}
			else if (!flag9 && !flag10)
			{
				showingMesh = 24;
			}
			else if (!flag9 && flag10)
			{
				showingMesh = 25;
			}
			else if (!flag10 && flag9)
			{
				showingMesh = 26;
			}
			else
			{
				showingMesh = 23;
			}
		}
		else if (flag && !flag2 && flag3 && flag4)
		{
			bool flag11 = checkIfSameAsNeighbour(showingX - 1, showingY + 1);
			bool flag12 = checkIfSameAsNeighbour(showingX - 1, showingY - 1);
			if (flag11 && flag12)
			{
				showingMesh = 27;
			}
			else if (!flag11 && !flag12)
			{
				showingMesh = 28;
			}
			else if (!flag11 && flag12)
			{
				showingMesh = 29;
			}
			else if (!flag12 && flag11)
			{
				showingMesh = 30;
			}
			else
			{
				showingMesh = 27;
			}
		}
		else if (flag && flag2 && !flag3 && flag4)
		{
			bool flag13 = checkIfSameAsNeighbour(showingX - 1, showingY - 1);
			bool flag14 = checkIfSameAsNeighbour(showingX + 1, showingY - 1);
			if (flag13 && flag14)
			{
				showingMesh = 31;
			}
			else if (!flag13 && !flag14)
			{
				showingMesh = 32;
			}
			else if (flag13 && !flag14)
			{
				showingMesh = 33;
			}
			else if (!flag13 && flag14)
			{
				showingMesh = 34;
			}
			else
			{
				showingMesh = 31;
			}
		}
		else if (flag && flag2 && flag3 && !flag4)
		{
			bool flag15 = checkIfSameAsNeighbour(showingX - 1, showingY + 1);
			bool flag16 = checkIfSameAsNeighbour(showingX + 1, showingY + 1);
			if (flag15 && flag16)
			{
				showingMesh = 35;
			}
			else if (!flag15 && !flag16)
			{
				showingMesh = 36;
			}
			else if (flag15 && !flag16)
			{
				showingMesh = 37;
			}
			else if (!flag15 && flag16)
			{
				showingMesh = 38;
			}
			else
			{
				showingMesh = 35;
			}
		}
		else if (flag && flag2 && !flag3 && !flag4)
		{
			showingMesh = 39;
		}
		else if (!flag && !flag2 && flag3 && flag4)
		{
			showingMesh = 40;
		}
		else
		{
			showingMesh = 41;
		}
	}

	public void checkNeighboursAndChangeMaterial()
	{
		_ = new int[2]
		{
			showingX,
			showingY - 1
		};
		bool flag = checkIfSameAsNeighbourMaterial(showingX - 1, showingY);
		bool flag2 = checkIfSameAsNeighbourMaterial(showingX + 1, showingY);
		bool flag3 = checkIfSameAsNeighbourMaterial(showingX, showingY + 1);
		bool flag4 = checkIfSameAsNeighbourMaterial(showingX, showingY - 1);
		if (flag && flag2 && flag3 && flag4)
		{
			changeMaterial(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[showingX, showingY]], new Vector2(1f, 2f), 0);
		}
		else if (!flag && !flag2 && !flag3 && flag4)
		{
			changeMaterial(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[showingX, showingY]], new Vector2(3f, 2f), 1);
		}
		else if (!flag && !flag2 && flag3 && !flag4)
		{
			changeMaterial(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[showingX, showingY]], new Vector2(3f, 0f), 2);
		}
		else if (!flag && flag2 && !flag3 && !flag4)
		{
			changeMaterial(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[showingX, showingY]], new Vector2(0f, 4f), 3);
		}
		else if (flag && !flag2 && !flag3 && !flag4)
		{
			changeMaterial(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[showingX, showingY]], new Vector2(2f, 4f), 4);
		}
		else if (!flag && flag2 && !flag3 && flag4)
		{
			changeMaterial(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[showingX, showingY]], new Vector2(0f, 7f), 5);
		}
		else if (flag && !flag2 && !flag3 && flag4)
		{
			changeMaterial(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[showingX, showingY]], new Vector2(2f, 7f), 6);
		}
		else if (!flag && flag2 && flag3 && !flag4)
		{
			changeMaterial(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[showingX, showingY]], new Vector2(0f, 1f), 7);
		}
		else if (flag && !flag2 && flag3 && !flag4)
		{
			changeMaterial(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[showingX, showingY]], new Vector2(2f, 1f), 8);
		}
		else if (!flag && flag2 && flag3 && flag4)
		{
			changeMaterial(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[showingX, showingY]], new Vector2(0f, 2f), 9);
		}
		else if (flag && !flag2 && flag3 && flag4)
		{
			changeMaterial(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[showingX, showingY]], new Vector2(2f, 2f), 10);
		}
		else if (flag && flag2 && !flag3 && flag4)
		{
			changeMaterial(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[showingX, showingY]], new Vector2(1f, 7f), 11);
		}
		else if (flag && flag2 && flag3 && !flag4)
		{
			changeMaterial(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[showingX, showingY]], new Vector2(1f, 1f), 12);
		}
		else if (flag && flag2 && !flag3 && !flag4)
		{
			changeMaterial(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[showingX, showingY]], new Vector2(1f, 4f), 13);
		}
		else if (!flag && !flag2 && flag3 && flag4)
		{
			changeMaterial(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[showingX, showingY]], new Vector2(3f, 1f), 14);
		}
		else
		{
			changeMaterial(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[showingX, showingY]], new Vector2(3f, 7f), 15);
		}
	}

	public bool checkIfNeedsExtention(int neighbourX, int neighbourY)
	{
		if (WorldManager.Instance.heightMap[neighbourX, neighbourY] < WorldManager.Instance.heightMap[showingX, showingY] - 18)
		{
			return true;
		}
		return false;
	}

	public bool checkIfSameAsNeighbour(int neighbourX, int neighbourY)
	{
		if (WorldManager.Instance.heightMap[showingX, showingY] <= WorldManager.Instance.heightMap[neighbourX, neighbourY])
		{
			return true;
		}
		return false;
	}

	public bool checkIfSameHeightAsNeighbour(int neighbourX, int neighbourY)
	{
		if (WorldManager.Instance.heightMap[showingX, showingY] == WorldManager.Instance.heightMap[neighbourX, neighbourY])
		{
			return true;
		}
		return false;
	}

	public bool checkIfSameAsNeighbourMaterial(int neighbourX, int neighbourY, bool checkNeighbourHeight = true)
	{
		if (WorldManager.Instance.tileTypeMap[showingX, showingY] == WorldManager.Instance.tileTypeMap[neighbourX, neighbourY] || WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[showingX, showingY]].mowedVariation == WorldManager.Instance.tileTypeMap[neighbourX, neighbourY])
		{
			if (checkNeighbourHeight)
			{
				if (WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[showingX, showingY]].sideOfTileSame || checkIfSameHeightAsNeighbour(neighbourX, neighbourY))
				{
					return true;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public void setWaterNo(int x, int y)
	{
		if (Chunk.waterMeshes[x, y] != null)
		{
			waterFilter.sharedMesh = Chunk.waterMeshes[x, y];
			return;
		}
		Vector2[] array = new Vector2[waterFilter.sharedMesh.vertices.Length];
		array = waterFilter.sharedMesh.uv;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new Vector2(array[i].x + (float)x, array[i].y + (float)y + (float)y);
		}
		waterFilter.sharedMesh.uv = array;
		Chunk.waterMeshes[x, y] = waterFilter.sharedMesh;
	}

	public void checkOnTileObject()
	{
		int num = WorldManager.Instance.onTileMap[showingX, showingY];
		if (num <= -1)
		{
			if ((bool)onThisTile)
			{
				WorldManager.Instance.returnTileObject(onThisTile);
				onThisTile = null;
			}
			return;
		}
		if ((bool)onThisTile && onThisTile.tileObjectId != num)
		{
			WorldManager.Instance.returnTileObject(onThisTile);
			onThisTile = null;
		}
		if (num > -1 && !onThisTile)
		{
			onThisTile = WorldManager.Instance.getTileObject(num, showingX, showingY);
		}
		if ((bool)onThisTile)
		{
			if ((onThisTile.GetRotationFromMap() || onThisTile.IsMultiTileObject()) && WorldManager.Instance.rotationMap[showingX, showingY] == 0 && !WorldManager.Instance.CheckTileClientLock(showingX, showingY) && (bool)NetworkMapSharer.Instance.localChar)
			{
				NetworkMapSharer.Instance.localChar.CmdRequestTileRotation(showingX, showingY);
			}
			onThisTile.setXAndY(showingX, showingY);
			onThisTile._transform.position = new Vector3(onThisTile._transform.position.x, WorldManager.Instance.heightMap[showingX, showingY], onThisTile._transform.position.z);
		}
	}

	public Mesh createNewTileMesh(Mesh dup)
	{
		return new Mesh
		{
			vertices = dup.vertices,
			triangles = dup.triangles,
			uv = dup.uv,
			normals = dup.normals,
			colors = dup.colors,
			tangents = dup.tangents
		};
	}

	public void changeMaterial(TileTypes tileType, Vector2 newOffSet, int meshVariation)
	{
		if (tileType == null)
		{
			tileType = WorldManager.Instance.fallBackTileType;
		}
		if (!(filt.sharedMesh != MeshManager.manage.allMeshVariations[showingMesh, meshVariation]))
		{
			return;
		}
		if (MeshManager.manage.allMeshVariations[showingMesh, meshVariation] == null)
		{
			Mesh uniqueMeshForVariation = MeshManager.manage.getUniqueMeshForVariation(showingMesh);
			if (newOffSet != Vector2.zero)
			{
				Vector2[] array = new Vector2[uniqueMeshForVariation.vertices.Length];
				array = uniqueMeshForVariation.uv;
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new Vector2(array[i].x + newOffSet.x, array[i].y + newOffSet.y);
				}
				uniqueMeshForVariation.uv = array;
			}
			MeshManager.manage.allMeshVariations[showingMesh, meshVariation] = uniqueMeshForVariation;
		}
		filt.sharedMesh = MeshManager.manage.allMeshVariations[showingMesh, meshVariation];
	}
}
