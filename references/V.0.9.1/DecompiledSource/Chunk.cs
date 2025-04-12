using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
	public GameObject tilePrefab;

	public Tile[,] chunksTiles;

	public int showingChunkX;

	public int showingChunkY;

	public int waterTilesOnChunk;

	public int oceanTilesOnChunk;

	public Transform _transform;

	public MeshFilter finalFilter;

	public MeshRenderer finalRen;

	public MeshCollider finalCollider;

	public MeshFilter waterFilt;

	public MeshCollider waterCollider;

	public MeshCollider swimmingCollider;

	public static Mesh[,] waterMeshes = new Mesh[10, 10];

	private Coroutine combiningRoutine;

	public ChunkMeshMerger chunkCollisionMeshGenerator;

	private bool currentlyInstanced;

	private List<CombineInstance> allSubMeshes = new List<CombineInstance>();

	private List<CombineInstance> waterCombine = new List<CombineInstance>();

	private List<Material> mats = new List<Material>();

	private void Start()
	{
		finalFilter.sharedMesh = new Mesh();
	}

	private void Awake()
	{
		setUpTiles();
	}

	public void OnEnable()
	{
		combiningRoutine = null;
	}

	public void setUpTiles()
	{
		chunksTiles = new Tile[WorldManager.Instance.chunkSize, WorldManager.Instance.chunkSize];
		for (int i = 0; i < WorldManager.Instance.chunkSize; i++)
		{
			for (int j = 0; j < WorldManager.Instance.chunkSize; j++)
			{
				chunksTiles[j, i] = Object.Instantiate(tilePrefab, base.transform.position + new Vector3(j * WorldManager.Instance.getTileSize(), 0f, i * WorldManager.Instance.getTileSize()), Quaternion.identity, base.transform).GetComponent<Tile>();
			}
		}
	}

	public void refreshChunk(bool fullRefresh = true)
	{
		currentlyInstanced = false;
		if (fullRefresh)
		{
			for (int i = 0; i < WorldManager.Instance.chunkSize; i++)
			{
				for (int j = 0; j < WorldManager.Instance.chunkSize; j++)
				{
					chunksTiles[j, i].refreshTile(showingChunkX + j, showingChunkY + i);
				}
			}
			if (combiningRoutine != null)
			{
				StopCoroutine(combiningRoutine);
				combiningRoutine = null;
			}
			combiningRoutine = StartCoroutine(combineChildren());
			return;
		}
		if (showingChunkX < 0 || showingChunkY < 0 || showingChunkY >= 1000 || showingChunkX >= 1000)
		{
			MonoBehaviour.print("Returning on showing chunk " + showingChunkX + "," + showingChunkY);
			return;
		}
		bool flag = false;
		for (int k = 0; k < WorldManager.Instance.chunkSize; k++)
		{
			for (int l = 0; l < WorldManager.Instance.chunkSize; l++)
			{
				if ((l == 0 || k == 0 || l == WorldManager.Instance.chunkSize - 1 || k == WorldManager.Instance.chunkSize - 1) && chunksTiles[l, k].refreshForNeighbours())
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			if (combiningRoutine != null)
			{
				StopCoroutine(combiningRoutine);
				combiningRoutine = null;
			}
			combiningRoutine = StartCoroutine(combineChildren());
		}
	}

	public void returnAllTileObjects()
	{
		for (int i = 0; i < WorldManager.Instance.chunkSize; i++)
		{
			for (int j = 0; j < WorldManager.Instance.chunkSize; j++)
			{
				chunksTiles[j, i].returnTileObjects();
			}
		}
	}

	public void refreshChunksOnTileObjects(bool neighbourCheck = false)
	{
		currentlyInstanced = false;
		if (!neighbourCheck)
		{
			for (int i = 0; i < WorldManager.Instance.chunkSize; i++)
			{
				for (int j = 0; j < WorldManager.Instance.chunkSize; j++)
				{
					chunksTiles[j, i].refreshTileObjects();
				}
			}
		}
		else
		{
			for (int k = 0; k < WorldManager.Instance.chunkSize; k++)
			{
				for (int l = 0; l < WorldManager.Instance.chunkSize; l++)
				{
					if (l == 0 || k == 0 || l == WorldManager.Instance.chunkSize - 1 || k == WorldManager.Instance.chunkSize - 1)
					{
						chunksTiles[l, k].refreshTileObjects();
					}
				}
			}
		}
		countWaterTiles();
	}

	public void refreshChunkToEmptySeaChunk()
	{
		currentlyInstanced = false;
		for (int i = 0; i < WorldManager.Instance.chunkSize; i++)
		{
			for (int j = 0; j < WorldManager.Instance.chunkSize; j++)
			{
				chunksTiles[j, i].setTileToOceanFloor();
			}
		}
		oceanTilesOnChunk = 100;
		waterTilesOnChunk = 0;
		List<CombineInstance> list = new List<CombineInstance>();
		List<CombineInstance> list2 = new List<CombineInstance>();
		for (int k = 0; k < WorldManager.Instance.chunkSize; k++)
		{
			for (int l = 0; l < WorldManager.Instance.chunkSize; l++)
			{
				CombineInstance item = default(CombineInstance);
				item.mesh = chunksTiles[l, k].filt.sharedMesh;
				item.transform = chunksTiles[l, k].filt.transform.localToWorldMatrix;
				list2.Add(item);
				CombineInstance item2 = default(CombineInstance);
				item2.mesh = chunksTiles[l, k].waterFilter.sharedMesh;
				item2.transform = chunksTiles[l, k].waterFilter.transform.localToWorldMatrix;
				list.Add(item2);
			}
		}
		Object.Destroy(waterFilt.sharedMesh);
		waterFilt.sharedMesh = new Mesh();
		waterFilt.sharedMesh.CombineMeshes(list.ToArray());
		waterCollider.sharedMesh = waterFilt.sharedMesh;
		swimmingCollider.sharedMesh = waterFilt.sharedMesh;
		Object.Destroy(finalFilter.sharedMesh);
		finalFilter.sharedMesh = new Mesh();
		finalFilter.sharedMesh.CombineMeshes(list2.ToArray());
		finalRen.materials = new Material[1] { WorldManager.Instance.tileTypes[3].myTileMaterial };
		base.transform.position = new Vector3(showingChunkX * 2, 0f, showingChunkY * 2);
	}

	public void refreshToNewChunk()
	{
		if ((bool)NetworkMapSharer.Instance && (bool)NetworkMapSharer.Instance.localChar && !NetworkMapSharer.Instance.isServer && !WorldManager.Instance.clientRequestedMap[showingChunkX / WorldManager.Instance.getChunkSize(), showingChunkY / WorldManager.Instance.getChunkSize()])
		{
			WorldManager.Instance.clientRequestedMap[showingChunkX / WorldManager.Instance.chunkSize, showingChunkY / WorldManager.Instance.getChunkSize()] = true;
			NetworkMapSharer.Instance.addChunkRequestedDelay(showingChunkX, showingChunkY);
			NetworkMapSharer.Instance.localChar.CmdRequestMapChunk(showingChunkX, showingChunkY);
		}
		refreshNewChunk();
	}

	public void setChunkAndRefresh(int newshowingX, int newshowingY, bool fullRefresh = false)
	{
		currentlyInstanced = false;
		showingChunkX = newshowingX;
		showingChunkY = newshowingY;
		base.gameObject.SetActive(value: true);
		if (fullRefresh)
		{
			refreshToNewChunk();
		}
		else if (showingChunkX < 0 || showingChunkX >= WorldManager.Instance.GetMapSize() || showingChunkY < 0 || showingChunkY >= WorldManager.Instance.GetMapSize())
		{
			refreshChunkToEmptySeaChunk();
		}
		else
		{
			refreshToNewChunk();
		}
	}

	public void returnOnTileObjects()
	{
		currentlyInstanced = false;
		if (showingChunkX < 0 || showingChunkX >= WorldManager.Instance.GetMapSize() || showingChunkY < 0 || showingChunkY >= WorldManager.Instance.GetMapSize())
		{
			refreshChunkToEmptySeaChunk();
			return;
		}
		for (int i = 0; i < WorldManager.Instance.chunkSize; i++)
		{
			for (int j = 0; j < WorldManager.Instance.chunkSize; j++)
			{
				chunksTiles[j, i].returnTileObjects();
			}
		}
	}

	private void refreshNewChunk()
	{
		currentlyInstanced = false;
		if (showingChunkX < 0 || showingChunkX >= WorldManager.Instance.GetMapSize() || showingChunkY < 0 || showingChunkY >= WorldManager.Instance.GetMapSize())
		{
			if (combiningRoutine != null)
			{
				StopCoroutine(combiningRoutine);
				combiningRoutine = null;
			}
			refreshChunkToEmptySeaChunk();
			return;
		}
		for (int i = 0; i < WorldManager.Instance.chunkSize; i++)
		{
			for (int j = 0; j < WorldManager.Instance.chunkSize; j++)
			{
				chunksTiles[j, i].refreshTile(showingChunkX + j, showingChunkY + i);
			}
		}
		countWaterTiles();
		if (combiningRoutine != null)
		{
			StopCoroutine(combiningRoutine);
			combiningRoutine = null;
		}
		combiningRoutine = StartCoroutine(combineChildren());
	}

	public void countWaterTiles()
	{
		oceanTilesOnChunk = 0;
		waterTilesOnChunk = 0;
		for (int i = 0; i < WorldManager.Instance.chunkSize; i++)
		{
			for (int j = 0; j < WorldManager.Instance.chunkSize; j++)
			{
				if (WorldManager.Instance.isPositionOnMap(showingChunkX + j, showingChunkY + i))
				{
					if (WorldManager.Instance.waterMap[showingChunkX + j, showingChunkY + i])
					{
						if (WorldManager.Instance.tileTypeMap[showingChunkX + j, showingChunkY + i] == 3 || WorldManager.Instance.tileTypeMap[showingChunkX + j, showingChunkY + i] == 41)
						{
							oceanTilesOnChunk++;
						}
						else
						{
							waterTilesOnChunk++;
						}
					}
					else if (WorldManager.Instance.onTileMap[showingChunkX + j, showingChunkY + i] == 527)
					{
						if (waterTilesOnChunk < 50)
						{
							waterTilesOnChunk += 25;
						}
						else
						{
							waterTilesOnChunk++;
						}
					}
				}
				else
				{
					oceanTilesOnChunk++;
				}
			}
		}
	}

	public IEnumerator combineChildren()
	{
		yield return null;
		finalRen.enabled = false;
		base.transform.position = Vector3.zero;
		TileIdInMat[] array = new TileIdInMat[WorldManager.Instance.tileTypes.Length];
		TileIdInMat tileIdInMat = new TileIdInMat();
		waterCombine.Clear();
		mats.Clear();
		int num = 0;
		for (int i = 0; i < WorldManager.Instance.chunkSize; i++)
		{
			for (int j = 0; j < WorldManager.Instance.chunkSize; j++)
			{
				int num2 = WorldManager.Instance.tileTypeMap[showingChunkX + j, showingChunkY + i];
				CombineInstance item = default(CombineInstance);
				item.mesh = chunksTiles[j, i].filt.sharedMesh;
				item.transform = chunksTiles[j, i].filt.transform.localToWorldMatrix;
				if (array[num2] == null)
				{
					array[num2] = new TileIdInMat();
				}
				array[num2].idsWithMyMat.Add(item);
				if (chunksTiles[j, i].filt.sharedMesh.subMeshCount > 1)
				{
					CombineInstance item2 = default(CombineInstance);
					item2.mesh = chunksTiles[j, i].filt.sharedMesh;
					item2.transform = chunksTiles[j, i].filt.transform.localToWorldMatrix;
					item2.subMeshIndex = 1;
					if (WorldManager.Instance.tileTypes[num2].sideOfTileSame)
					{
						array[num2].idsWithMyMat.Add(item2);
					}
					else
					{
						tileIdInMat.idsWithMyMat.Add(item2);
					}
				}
				if (WorldManager.Instance.waterMap[showingChunkX + j, showingChunkY + i])
				{
					CombineInstance item3 = default(CombineInstance);
					item3.mesh = chunksTiles[j, i].waterFilter.mesh;
					item3.transform = chunksTiles[j, i].waterFilter.transform.localToWorldMatrix;
					waterCombine.Add(item3);
				}
				num++;
			}
		}
		allSubMeshes.Clear();
		for (int k = 0; k < array.Length; k++)
		{
			if (array[k] != null && array[k].idsWithMyMat.Count > 0)
			{
				finalFilter.sharedMesh = new Mesh();
				finalFilter.sharedMesh.CombineMeshes(array[k].idsWithMyMat.ToArray());
				CombineInstance item4 = default(CombineInstance);
				item4.mesh = finalFilter.sharedMesh;
				item4.transform = finalFilter.transform.localToWorldMatrix;
				allSubMeshes.Add(item4);
				mats.Add(WorldManager.Instance.tileTypes[k].myTileMaterial);
			}
		}
		if (tileIdInMat.idsWithMyMat.Count > 0)
		{
			finalFilter.sharedMesh = new Mesh();
			finalFilter.sharedMesh.CombineMeshes(tileIdInMat.idsWithMyMat.ToArray());
			CombineInstance item5 = default(CombineInstance);
			item5.mesh = finalFilter.sharedMesh;
			item5.transform = finalFilter.transform.localToWorldMatrix;
			allSubMeshes.Add(item5);
			mats.Add(WorldManager.Instance.stoneSide);
		}
		Object.Destroy(waterFilt.sharedMesh);
		if (waterCombine.Count > 0)
		{
			waterFilt.sharedMesh = new Mesh();
			waterFilt.sharedMesh.CombineMeshes(waterCombine.ToArray());
			waterCollider.sharedMesh = waterFilt.sharedMesh;
			swimmingCollider.sharedMesh = waterFilt.sharedMesh;
		}
		Object.Destroy(finalFilter.sharedMesh);
		finalFilter.sharedMesh = new Mesh();
		finalFilter.sharedMesh.CombineMeshes(allSubMeshes.ToArray(), mergeSubMeshes: false);
		for (int l = 0; l < allSubMeshes.Count; l++)
		{
			Object.Destroy(allSubMeshes[l].mesh);
		}
		finalRen.materials = mats.ToArray();
		finalFilter.transform.localPosition = new Vector3(showingChunkX * 2, 0f, showingChunkY * 2);
		finalRen.enabled = true;
		yield return null;
		Object.Destroy(finalCollider.sharedMesh);
		Physics.BakeMesh(finalFilter.sharedMesh.GetInstanceID(), convex: false);
		finalCollider.sharedMesh = finalFilter.sharedMesh;
		combiningRoutine = null;
	}

	private void OnDestroy()
	{
		Object.Destroy(finalFilter.sharedMesh);
		Object.Destroy(finalCollider.sharedMesh);
		Object.Destroy(waterFilt.sharedMesh);
		Object.Destroy(waterCollider.sharedMesh);
		Object.Destroy(swimmingCollider.sharedMesh);
	}

	public void WeldVertices(Mesh aMesh, float aMaxDelta = 0.001f)
	{
		Vector3[] vertices = aMesh.vertices;
		Vector3[] normals = aMesh.normals;
		Vector2[] uv = aMesh.uv;
		List<int> list = new List<int>();
		int[] array = new int[vertices.Length];
		for (int i = 0; i < vertices.Length; i++)
		{
			Vector3 vector = vertices[i];
			Vector3 to = normals[i];
			Vector2 vector2 = uv[i];
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				int num = list[j];
				if ((vertices[num] - vector).sqrMagnitude <= aMaxDelta && Vector3.Angle(normals[num], to) <= aMaxDelta && (uv[num] - vector2).sqrMagnitude <= aMaxDelta)
				{
					array[i] = j;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				array[i] = list.Count;
				list.Add(i);
			}
		}
		Vector3[] array2 = new Vector3[list.Count];
		Vector3[] array3 = new Vector3[list.Count];
		Vector2[] array4 = new Vector2[list.Count];
		for (int k = 0; k < list.Count; k++)
		{
			int num2 = list[k];
			array2[k] = vertices[num2];
			array3[k] = normals[num2];
			array4[k] = uv[num2];
		}
		int[] triangles = aMesh.triangles;
		for (int l = 0; l < triangles.Length; l++)
		{
			triangles[l] = array[triangles[l]];
		}
		aMesh.triangles = triangles;
		aMesh.vertices = array2;
		aMesh.normals = array3;
		aMesh.uv = array4;
	}

	public Mesh moveMeshUV(float x, float y, Mesh waterMesh)
	{
		Vector2[] array = new Vector2[waterMesh.vertices.Length];
		array = waterMesh.uv;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new Vector2(array[i].x + x, array[i].y + y);
		}
		waterMesh.uv = array;
		return waterMesh;
	}
}
