using System.Collections;
using UnityEngine;

public class Rainbow : MonoBehaviour
{
	public Material rainbowMat;

	public Color currentRainbowColour;

	private void OnEnable()
	{
		StartCoroutine(DoRainbowStuff());
	}

	public void SetDefaultColour()
	{
		currentRainbowColour.a = 0f;
		rainbowMat.color = currentRainbowColour;
	}

	private IEnumerator DoRainbowStuff()
	{
		float timer2 = 0f;
		if (RealWorldTimeLight.time.currentHour >= 18 || RealWorldTimeLight.time.currentHour == 0)
		{
			SetDefaultColour();
			base.gameObject.SetActive(value: false);
			yield break;
		}
		while (timer2 < 1f)
		{
			timer2 += Time.deltaTime / 10f;
			currentRainbowColour.a = Mathf.Lerp(0f, 0.45f, timer2);
			rainbowMat.color = currentRainbowColour;
			yield return null;
		}
		timer2 = 0f;
		yield return new WaitForSeconds(240f);
		while (timer2 < 1f)
		{
			timer2 += Time.deltaTime / 20f;
			currentRainbowColour.a = Mathf.Lerp(0.45f, 0f, timer2);
			rainbowMat.color = currentRainbowColour;
			yield return null;
		}
		base.gameObject.SetActive(value: false);
	}
}
