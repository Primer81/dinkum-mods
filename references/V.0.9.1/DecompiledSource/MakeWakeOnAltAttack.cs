using System.Collections;
using UnityEngine;

public class MakeWakeOnAltAttack : MonoBehaviour
{
	public Transform[] wakePositions;

	public void MakeWakeNow()
	{
		StartCoroutine(SplashOverFrames());
	}

	private IEnumerator SplashOverFrames()
	{
		for (int i = 0; i < wakePositions.Length; i++)
		{
			ParticleManager.manage.bigSplash(wakePositions[i]);
		}
		yield return null;
		yield return null;
		yield return null;
		for (int j = 0; j < wakePositions.Length; j++)
		{
			ParticleManager.manage.bigSplash(wakePositions[j]);
		}
		yield return null;
		yield return null;
		yield return null;
		for (int k = 0; k < wakePositions.Length; k++)
		{
			ParticleManager.manage.bigSplash(wakePositions[k]);
		}
		yield return null;
		yield return null;
		yield return null;
		for (int l = 0; l < wakePositions.Length; l++)
		{
			ParticleManager.manage.bigSplash(wakePositions[l]);
		}
		yield return null;
		yield return null;
		yield return null;
		for (int m = 0; m < wakePositions.Length; m++)
		{
			ParticleManager.manage.bigSplash(wakePositions[m]);
		}
		yield return null;
		yield return null;
		yield return null;
		for (int n = 0; n < wakePositions.Length; n++)
		{
			ParticleManager.manage.bigSplash(wakePositions[n]);
		}
		yield return null;
		yield return null;
		yield return null;
		for (int num = 0; num < wakePositions.Length; num++)
		{
			ParticleManager.manage.bigSplash(wakePositions[num]);
		}
	}
}
