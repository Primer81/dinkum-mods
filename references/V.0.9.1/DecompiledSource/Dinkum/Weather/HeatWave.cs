using System.Collections;
using UnityEngine;

namespace Dinkum.Weather;

public class HeatWave : WeatherBase
{
	public ParticleSystem heatWaveParticles;

	protected override void Show()
	{
		heatWaveParticles.gameObject.SetActive(value: true);
		StartCoroutine(FadeInHeatTemperature(0.5f));
	}

	protected override void Hide()
	{
		heatWaveParticles.gameObject.SetActive(value: false);
		StartCoroutine(FadeInHeatTemperature(0f));
	}

	private IEnumerator FadeInHeatTemperature(float endingTemp)
	{
		float timer = 0f;
		int startingHour = RealWorldTimeLight.time.currentHour;
		float startingTemp = StatusManager.manage.GetCurrentColourTemperature();
		while (timer < 10f && RealWorldTimeLight.time.currentHour == startingHour)
		{
			timer += Time.deltaTime;
			StatusManager.manage.SetTemeratureColourForWeatherEvent(Mathf.Lerp(startingTemp, endingTemp, timer));
			yield return null;
		}
		StatusManager.manage.SetTemeratureColourForWeatherEvent(endingTemp);
	}
}
