using System;

[Serializable]
public class WeatherSave
{
	public WeatherData[] todaysWeather = new WeatherData[RealWorldTimeLight.ChangeWeatherPatternsNumTimesPerDay];

	public WeatherData[] tomorrowsWeather = new WeatherData[RealWorldTimeLight.ChangeWeatherPatternsNumTimesPerDay];

	public void SaveWeather()
	{
		for (int i = 0; i < RealWorldTimeLight.ChangeWeatherPatternsNumTimesPerDay; i++)
		{
			todaysWeather[i] = NetworkMapSharer.Instance.todaysWeather[i];
		}
		for (int j = 0; j < RealWorldTimeLight.ChangeWeatherPatternsNumTimesPerDay; j++)
		{
			tomorrowsWeather[j] = NetworkMapSharer.Instance.tomorrowsWeather[j];
		}
	}

	public void LoadWeather()
	{
		NetworkMapSharer.Instance.todaysWeather.Clear();
		NetworkMapSharer.Instance.tomorrowsWeather.Clear();
		for (int i = 0; i < RealWorldTimeLight.ChangeWeatherPatternsNumTimesPerDay; i++)
		{
			NetworkMapSharer.Instance.todaysWeather.Add(todaysWeather[i]);
		}
		for (int j = 0; j < RealWorldTimeLight.ChangeWeatherPatternsNumTimesPerDay; j++)
		{
			NetworkMapSharer.Instance.tomorrowsWeather.Add(tomorrowsWeather[j]);
		}
		if (NetworkMapSharer.Instance.todaysWeather.Count != 3)
		{
			WeatherManager.Instance.CreateNewWeatherPatterns();
		}
		WeatherManager.Instance.ClearWeatherOnSpecialDay();
		WeatherManager.Instance.CheckSnowDay();
	}
}
