using UnityEngine;

[CreateAssetMenu(fileName = "NewPattern", menuName = "Weather/Weather Pattern")]
public class WeatherPattern : ScriptableObject
{
	public string tag;

	[Range(0f, 100f)]
	public int chanceForStorm;

	[Range(0f, 100f)]
	public int chanceForWind;

	[Range(0f, 100f)]
	public int chanceForHeatWave;

	[Range(0f, 100f)]
	public int chanceForOvercast;

	[Range(0f, 100f)]
	public int chanceForRain;

	[Range(0f, 100f)]
	public int chanceForFog;

	public bool canSnow;
}
