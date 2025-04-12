using System.Collections.Generic;
using UnityEngine;

public class ChunkMeshMerger : MonoBehaviour
{
	private const float tileSize = 2f;

	private List<Vector3> vertices = new List<Vector3>();

	private List<int> triangles = new List<int>();

	private List<Color> colors = new List<Color>();

	public Chunk myChunk;

	public MeshCollider meshCollider;

	public void GenerateChunkCollider()
	{
		GenerateTerrain();
	}

	private void GenerateTerrain()
	{
		vertices.Clear();
		triangles.Clear();
		colors.Clear();
		for (int i = myChunk.showingChunkX; i < myChunk.showingChunkX + WorldManager.Instance.getChunkSize(); i++)
		{
			for (int j = myChunk.showingChunkY; j < myChunk.showingChunkY + WorldManager.Instance.getChunkSize(); j++)
			{
				int num = WorldManager.Instance.heightMap[i, j];
				Vector3 position = new Vector3((float)(i - myChunk.showingChunkX) * 2f, num, (float)(j - myChunk.showingChunkY) * 2f);
				position += new Vector3(-1f, 0f, -1f);
				AddTopFace(position);
				if (i < WorldManager.Instance.heightMap.GetLength(0) - 1)
				{
					AddSideFace(position, Vector3.right, num - WorldManager.Instance.heightMap[i + 1, j]);
				}
				if (j < WorldManager.Instance.heightMap.GetLength(1) - 1)
				{
					AddSideFace(position, Vector3.forward, num - WorldManager.Instance.heightMap[i, j + 1]);
				}
			}
		}
		UpdateMesh();
	}

	private void UpdateMesh()
	{
		if (meshCollider.sharedMesh == null)
		{
			meshCollider.sharedMesh = new Mesh();
		}
		meshCollider.sharedMesh.Clear();
		meshCollider.sharedMesh.vertices = vertices.ToArray();
		meshCollider.sharedMesh.triangles = triangles.ToArray();
		meshCollider.sharedMesh.colors = colors.ToArray();
		meshCollider.sharedMesh.RecalculateNormals();
		meshCollider.sharedMesh = meshCollider.sharedMesh;
	}

	private void AddTopFace(Vector3 position)
	{
		vertices.AddRange(new Vector3[4]
		{
			position,
			position + Vector3.right * 2f,
			position + Vector3.right * 2f + Vector3.forward * 2f,
			position + Vector3.forward * 2f
		});
		Color red = Color.red;
		for (int i = 0; i < 4; i++)
		{
			colors.Add(red);
		}
		triangles.Add(vertices.Count - 4);
		triangles.Add(vertices.Count - 2);
		triangles.Add(vertices.Count - 3);
		triangles.Add(vertices.Count - 4);
		triangles.Add(vertices.Count - 1);
		triangles.Add(vertices.Count - 2);
	}

	private void AddSideFace(Vector3 position, Vector3 direction, int heightDifference)
	{
		if (heightDifference == 0)
		{
			return;
		}
		Color item = ((direction == Vector3.right) ? Color.green : Color.yellow);
		for (int i = 0; i < Mathf.Abs(heightDifference); i++)
		{
			Vector3 vector;
			Vector3 vector2;
			Vector3 vector3;
			Vector3 vector4;
			if (direction == Vector3.right)
			{
				vector = position + (Vector3.forward + Vector3.right) * 2f;
				vector2 = vector - Vector3.up * heightDifference;
				vector3 = vector - Vector3.forward * 2f;
				vector4 = vector3 - Vector3.up * heightDifference;
			}
			else
			{
				vector = position + Vector3.forward * 2f;
				vector2 = vector - Vector3.up * heightDifference;
				vector3 = vector + Vector3.right * 2f;
				vector4 = vector3 - Vector3.up * heightDifference;
			}
			vertices.AddRange(new Vector3[4] { vector, vector3, vector4, vector2 });
			for (int j = 0; j < 4; j++)
			{
				colors.Add(item);
			}
			triangles.Add(vertices.Count - 4);
			triangles.Add(vertices.Count - 2);
			triangles.Add(vertices.Count - 3);
			triangles.Add(vertices.Count - 4);
			triangles.Add(vertices.Count - 1);
			triangles.Add(vertices.Count - 2);
		}
	}
}
