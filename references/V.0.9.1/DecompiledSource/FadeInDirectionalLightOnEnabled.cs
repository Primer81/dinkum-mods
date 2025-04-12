using System.Collections;
using UnityEngine;

public class FadeInDirectionalLightOnEnabled : MonoBehaviour
{
	public Light dirLight;

	public float desiredIntensity = 1f;

	public float timerTotal = 4f;

	private void OnEnable()
	{
		StartCoroutine(FadeIn());
	}

	private IEnumerator FadeIn()
	{
		float timer = 0f;
		dirLight.intensity = 0f;
		for (; timer < timerTotal; timer += Time.deltaTime)
		{
			yield return null;
			dirLight.intensity = Mathf.Lerp(0f, desiredIntensity, timer / timerTotal);
		}
		dirLight.intensity = desiredIntensity;
	}
}
