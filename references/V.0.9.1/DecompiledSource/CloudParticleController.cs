using System;
using System.Collections;
using UnityEngine;

public class CloudParticleController : MonoBehaviour
{
	public ParticleSystem[] cloudParticles;

	private WaitForSeconds cloudWait = new WaitForSeconds(20f);

	private void Start()
	{
		StartCoroutine(ControlClouds());
	}

	private IEnumerator ControlClouds()
	{
		while (true)
		{
			for (int i = 0; i < cloudParticles.Length; i++)
			{
				if (cloudParticles[i].particleCount == 0)
				{
					MoveCloudAndSpawnAtDifferentPlace(cloudParticles[i]);
				}
			}
			yield return cloudWait;
		}
	}

	private void MoveCloudAndSpawnAtDifferentPlace(ParticleSystem toMove)
	{
		Vector3 position = base.transform.position;
		float num = UnityEngine.Random.Range(0f, 360f);
		float num2 = UnityEngine.Random.Range(250f, 300f);
		float num3 = num2 * Mathf.Cos(num * ((float)Math.PI / 180f));
		float num4 = num2 * Mathf.Sin(num * ((float)Math.PI / 180f));
		Vector3 position2 = new Vector3(position.x + num3, UnityEngine.Random.Range(0f, 100f), position.z + num4);
		toMove.transform.position = position2;
		Vector3 vector = position - toMove.transform.position;
		vector.y = 0f;
		if (vector.sqrMagnitude > 0.0001f)
		{
			Quaternion rotation = Quaternion.Euler(vector.normalized);
			toMove.transform.rotation = rotation;
		}
		toMove.Emit(1);
	}
}
