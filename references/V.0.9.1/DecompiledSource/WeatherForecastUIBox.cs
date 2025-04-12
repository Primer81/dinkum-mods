using UnityEngine;
using UnityEngine.UI;

public class WeatherForecastUIBox : MonoBehaviour
{
	public enum ForecastDay
	{
		Today,
		Tomorrow
	}

	public ForecastDay showingForcast;

	public Sprite sunnySprite;

	public Sprite rainingSprite;

	public Sprite stormingSprite;

	public Sprite overcastSprite;

	public Sprite noWindSprite;

	public Sprite windySprite;

	public Sprite foggySprite;

	public Sprite heatWaveSprite;

	public Sprite meteorShowerSprite;

	public Sprite snowSprite;

	public Image[] timeOfDay;

	public Image[] timeOfDayWind;

	public void OnEnable()
	{
		fillForecastUI();
	}

	public void fillForecastUI()
	{
		for (int i = 0; i < timeOfDay.Length; i++)
		{
			if (showingForcast == ForecastDay.Today)
			{
				timeOfDay[i].sprite = GetSpriteForWeatherData(NetworkMapSharer.Instance.todaysWeather[i], NetworkMapSharer.Instance.todaysWeather[0].isSnowDay);
				timeOfDayWind[i].sprite = GetSpriteForWindyWeatherData(NetworkMapSharer.Instance.todaysWeather[i]);
			}
			else
			{
				timeOfDay[i].sprite = GetSpriteForWeatherData(NetworkMapSharer.Instance.tomorrowsWeather[i], NetworkMapSharer.Instance.tomorrowsWeather[0].isSnowDay);
				timeOfDayWind[i].sprite = GetSpriteForWindyWeatherData(NetworkMapSharer.Instance.tomorrowsWeather[i]);
			}
		}
	}

	public Sprite GetSpriteForWeatherData(WeatherData weatherToCheck, bool isSnowDay)
	{
		if (weatherToCheck.isOvercast)
		{
			if (weatherToCheck.isStormy)
			{
				return stormingSprite;
			}
			if (weatherToCheck.isRainy)
			{
				if (WorldManager.Instance.month == 3 && isSnowDay)
				{
					return snowSprite;
				}
				return rainingSprite;
			}
			if (weatherToCheck.isFoggy)
			{
				return foggySprite;
			}
			return overcastSprite;
		}
		if (weatherToCheck.isHeatWave)
		{
			return heatWaveSprite;
		}
		if (weatherToCheck.isFoggy)
		{
			return foggySprite;
		}
		if (weatherToCheck.isMeteorShower)
		{
			return meteorShowerSprite;
		}
		return sunnySprite;
	}

	public Sprite GetSpriteForWindyWeatherData(WeatherData weatherToCheck)
	{
		if (weatherToCheck.isWindy)
		{
			return windySprite;
		}
		return noWindSprite;
	}
}
