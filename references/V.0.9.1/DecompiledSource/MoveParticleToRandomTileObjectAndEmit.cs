using System.Collections;
using UnityEngine;

public class MoveParticleToRandomTileObjectAndEmit : MonoBehaviour
{
	public TileObject moveToRandom;

	public ParticleSystem system;

	private Vector3 lastEmitPosition;

	private int triesSinceLastPos;

	private void OnEnable()
	{
		StartCoroutine(HandleParticles());
	}

	private IEnumerator HandleParticles()
	{
		int tileObjectId = moveToRandom.tileObjectId;
		WaitForSeconds wait = new WaitForSeconds(0.15f);
		while (true)
		{
			yield return wait;
			Vector3 position = CameraController.control.transform.position + new Vector3(Random.Range(-50, 50), 0f, Random.Range(-50, 50));
			if (WorldManager.Instance.isPositionOnMap(position))
			{
				int num = Mathf.RoundToInt(position.x / 2f);
				int num2 = Mathf.RoundToInt(position.z / 2f);
				if (WorldManager.Instance.onTileMap[num, num2] == tileObjectId)
				{
					base.transform.position = new Vector3(position.x, WorldManager.Instance.heightMap[num, num2], position.z);
					system.Emit(Random.Range(1, 6));
				}
			}
		}
	}

	private IEnumerator HandleParticlesOld()
	{
		int tileObjectId = moveToRandom.tileObjectId;
		WaitForSeconds wait = new WaitForSeconds(0.25f);
		while (true)
		{
			yield return wait;
			base.transform.position = GetRandomActiveTileObject(tileObjectId);
			system.Emit(5);
		}
	}

	public Vector3 GetRandomActiveTileObject(int tileObjectId)
	{
		if (WorldManager.Instance.allObjectsSorted[tileObjectId].Count == 0)
		{
			return new Vector3(-200f, -200f, -200f);
		}
		for (int num = 2000; num > 0; num--)
		{
			if (WorldManager.Instance.allObjectsSorted[tileObjectId][Random.Range(0, WorldManager.Instance.allObjectsSorted[tileObjectId].Count)].active)
			{
				return WorldManager.Instance.allObjectsSorted[tileObjectId][Random.Range(0, WorldManager.Instance.allObjectsSorted[tileObjectId].Count)].transform.position;
			}
		}
		return new Vector3(-200f, -200f, -200f);
	}
}
