using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Dinkum.Weather;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class WeatherManager : NetworkBehaviour
{
	public static WeatherManager Instance;

	[HideInInspector]
	public Wind windMgr;

	[HideInInspector]
	public Storm stormMgr;

	[HideInInspector]
	public Overcast overcastMgr;

	[HideInInspector]
	public Rain rainMgr;

	[HideInInspector]
	public Fog fogMgr;

	[HideInInspector]
	public HeatWave heatWaveMgr;

	[HideInInspector]
	public MeteorShower meteorShowerMgr;

	[SerializeField]
	public Light weatherLight;

	[SerializeField]
	private WeatherPattern[] AllWeatherPatterns;

	[SerializeField]
	private int[] changeWeatherAtHours = new int[3] { 7, 12, 18 };

	public bool IsRunningTransition;

	[SyncVar(hook = "OnSnowDayChange")]
	public bool IsSnowDay;

	public WeatherData CurrentWeather { get; private set; }

	public bool IsMyPlayerInside { get; private set; }

	public bool IsOvercast => overcastMgr.IsActive;

	public bool IsRaining => rainMgr.IsActive;

	public bool IsFoggy => fogMgr.IsActive;

	public bool NetworkIsSnowDay
	{
		get
		{
			return IsSnowDay;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref IsSnowDay))
			{
				bool isSnowDay = IsSnowDay;
				SetSyncVar(value, ref IsSnowDay, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					OnSnowDayChange(isSnowDay, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	private void Awake()
	{
		Instance = this;
		windMgr = GetComponent<Wind>();
		stormMgr = GetComponent<Storm>();
		overcastMgr = GetComponent<Overcast>();
		rainMgr = GetComponent<Rain>();
		fogMgr = GetComponent<Fog>();
		heatWaveMgr = GetComponent<HeatWave>();
		meteorShowerMgr = GetComponent<MeteorShower>();
		windMgr.SetUp(this);
		stormMgr.SetUp(this);
		overcastMgr.SetUp(this);
		rainMgr.SetUp(this);
		fogMgr.SetUp(this);
		heatWaveMgr.SetUp(this);
		meteorShowerMgr.SetUp(this);
	}

	private void Start()
	{
		PhotoManager.manage.resetMaterialsToSeeThrough();
	}

	public void ChangeToInsideEnvironment(MusicManager.indoorMusic playMusic, bool noMusic = false)
	{
		IsMyPlayerInside = true;
		rainMgr.UpdateRain();
		fogMgr.globalFog.enabled = false;
		windMgr.SetWindParticlesShowing(value: false);
		if (stormMgr.IsActive)
		{
			stormMgr.AdjustSoundVolumeToInsideEnvironments();
		}
		MusicManager.manage.ChangeCharacterInsideOrOutside(newInside: true, playMusic, noMusic);
		RenderMap.Instance.ChangeMapWindow();
	}

	public void ChangeToOutsideEnvironment()
	{
		IsMyPlayerInside = false;
		rainMgr.UpdateRain();
		fogMgr.globalFog.enabled = true;
		if (!RealWorldTimeLight.time.underGround)
		{
			windMgr.SetWindParticlesShowing(value: true);
			windMgr.SetMaterialsToWindy();
		}
		if (stormMgr.IsActive)
		{
			stormMgr.AdjustSoundVolumeToOutsideEnvironments();
		}
		MusicManager.manage.ChangeCharacterInsideOrOutside(newInside: false, MusicManager.indoorMusic.Default);
		RenderMap.Instance.ChangeMapWindow();
	}

	public void CreateNewWeatherPatterns()
	{
		if (NetworkMapSharer.Instance.tomorrowsWeather.Count > 0)
		{
			SetTomorrowsWeatherPatternsAsTodays();
		}
		else
		{
			RandomizeTodaysWeatherPatterns();
		}
		RandomizeTomorrowsWeatherPattern();
	}

	public void CheckSnowDay()
	{
		if (WorldManager.Instance.month == 3 && NetworkMapSharer.Instance.todaysWeather.Count > 0 && NetworkMapSharer.Instance.todaysWeather[0] != null && NetworkMapSharer.Instance.todaysWeather[0].isSnowDay && NetworkMapSharer.Instance.todaysWeather[0].isRainy)
		{
			NetworkIsSnowDay = true;
		}
		else
		{
			NetworkIsSnowDay = false;
		}
	}

	public bool SnowFallPossible()
	{
		return NetworkMapSharer.Instance.todaysWeather[0].isSnowDay;
	}

	private void OnSnowDayChange(bool oldSnowDay, bool newSnowDay)
	{
		NetworkIsSnowDay = newSnowDay;
		SeasonManager.manage.checkSeasonAndChangeMaterials();
		if (rainMgr.IsActive)
		{
			rainMgr.SetActive(active: false);
			rainMgr.SetActive(active: true);
		}
	}

	public string GetCurrentWeatherDescription()
	{
		return GetWeatherDescription(CurrentWeather);
	}

	public string GetTomorrowsWeatherDescription()
	{
		return GetWeatherDescription(NetworkMapSharer.Instance.tomorrowsWeather[0]);
	}

	public string GetWeatherDescription(WeatherData weatherData)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"It is currently {GetCurrentTemperature()} ° and ");
		if (weatherData.isStormy)
		{
			stringBuilder.Append(UIAnimationManager.manage.WrapStringInYesColor("Storming") + ". With a");
		}
		else if (weatherData.isRainy)
		{
			stringBuilder.Append(UIAnimationManager.manage.WrapStringInYesColor("Raining") + ". With a");
		}
		else if (weatherData.isFoggy)
		{
			stringBuilder.Append(UIAnimationManager.manage.WrapStringInYesColor("Foggy") + ". With a");
		}
		else if (weatherData.isOvercast)
		{
			stringBuilder.Append(UIAnimationManager.manage.WrapStringInYesColor("Overcast") + ". With a");
		}
		else
		{
			stringBuilder.Append(UIAnimationManager.manage.WrapStringInYesColor("Fine") + ". With a");
		}
		if (weatherData.isWindy)
		{
			stringBuilder.Append(" Strong");
		}
		else
		{
			stringBuilder.Append(" Light");
		}
		if (weatherData.windDirection.z < 0)
		{
			stringBuilder.Append(" Northern ");
		}
		else if (weatherData.windDirection.z > 0)
		{
			stringBuilder.Append(" Southern ");
		}
		if (weatherData.windDirection.x < 0)
		{
			stringBuilder.Append(" Westernly ");
		}
		else if (weatherData.windDirection.x > 0)
		{
			stringBuilder.Append(" Easternly ");
		}
		stringBuilder.Append(" Wind.");
		return stringBuilder.ToString();
	}

	public string GetDescriptionOfTomorrowsWeatherPredictions()
	{
		return GetWeatherDescription(NetworkMapSharer.Instance.todaysWeather[0]);
	}

	public bool IsRainingTomorrow()
	{
		for (int i = 0; i < RealWorldTimeLight.ChangeWeatherPatternsNumTimesPerDay; i++)
		{
			if (NetworkMapSharer.Instance.todaysWeather[i].isRainy)
			{
				return true;
			}
		}
		return false;
	}

	[ClientRpc]
	public void RpcMakeItRainTomorrow()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(WeatherManager), "RpcMakeItRainTomorrow", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public void ChangeWeather(SyncList<WeatherData>.Operation op, int index, WeatherData oldItem, WeatherData newItem)
	{
		if ((uint)op == 0u && index == RealWorldTimeLight.ChangeWeatherPatternsNumTimesPerDay - 1)
		{
			ChangeWeather();
		}
	}

	public void ChangeWeather()
	{
		if (NetworkMapSharer.Instance.todaysWeather != null && NetworkMapSharer.Instance.todaysWeather.Count != 0)
		{
			CurrentWeather = NetworkMapSharer.Instance.todaysWeather[GetDayOfTimeWeatherIndex()];
			for (int i = 0; i < NetworkMapSharer.Instance.todaysWeather.Count; i++)
			{
				_ = NetworkMapSharer.Instance.todaysWeather[i];
			}
			for (int j = 0; j < NetworkMapSharer.Instance.tomorrowsWeather.Count; j++)
			{
				_ = NetworkMapSharer.Instance.tomorrowsWeather[j];
			}
			UpdateWindDirection();
			UpdateWeatherSystems();
			TransitionBetweenWeatherEvents();
			UpdatePhotoFog();
			UpdateSkyBoxColor();
			CheckSnowDay();
		}
	}

	private void UpdateWeatherSystems()
	{
		fogMgr.SetActive(CurrentWeather.isFoggy);
		rainMgr.SetActive(CurrentWeather.isRainy);
		overcastMgr.SetActive(CurrentWeather.isOvercast);
		stormMgr.SetActive(CurrentWeather.isStormy);
		windMgr.SetActive(CurrentWeather.isWindy);
		heatWaveMgr.SetActive(CurrentWeather.isHeatWave);
		meteorShowerMgr.SetActive(CurrentWeather.isMeteorShower);
	}

	public void StopWeather()
	{
		fogMgr.SetActive(active: false);
		overcastMgr.SetActive(active: false);
		rainMgr.SetActive(active: false);
		stormMgr.SetActive(active: false);
		windMgr.SetActive(active: false);
		heatWaveMgr.SetActive(active: false);
		meteorShowerMgr.SetActive(active: false);
	}

	public bool CheckIfCurrentHourShouldTriggerWeatherChange(int hour)
	{
		for (int i = 0; i < changeWeatherAtHours.Length; i++)
		{
			if (changeWeatherAtHours[i] == hour)
			{
				return true;
			}
		}
		return false;
	}

	public void CheckIfHasAlreadyRainedTodayAndWaterCropsOnLevelChange()
	{
		int num = RealWorldTimeLight.time.currentHour;
		if (num == 0)
		{
			num = 24;
		}
		for (int i = 0; i < changeWeatherAtHours.Length; i++)
		{
			if (num <= changeWeatherAtHours[i] && NetworkMapSharer.Instance.todaysWeather[i].isRainy)
			{
				WorldManager.Instance.WetTilledTilesWhenRainStarts();
				break;
			}
		}
	}

	public bool IsItRainingToday()
	{
		for (int i = 0; i < RealWorldTimeLight.ChangeWeatherPatternsNumTimesPerDay; i++)
		{
			if (NetworkMapSharer.Instance.todaysWeather[i].isRainy)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateSkyBoxColor()
	{
		StartCoroutine(BlendToDesiredSkyColour());
	}

	private IEnumerator BlendToDesiredSkyColour()
	{
		float timer = 0f;
		Color startingSkyColor = RenderSettings.skybox.GetColor("_SkyTint");
		int startingHour = RealWorldTimeLight.time.currentHour;
		while (timer <= 10f)
		{
			timer += Time.deltaTime;
			RenderSettings.skybox.SetColor("_SkyTint", Color.Lerp(startingSkyColor, RealWorldTimeLight.time.GetSkyDayColor(), timer / 10f));
			yield return null;
			if (startingHour != RealWorldTimeLight.time.currentHour)
			{
				break;
			}
		}
		RenderSettings.skybox.SetColor("_SkyTint", RealWorldTimeLight.time.GetSkyDayColor());
	}

	private void UpdateWindDirection()
	{
		windMgr.WindDirection = new Vector3(CurrentWeather.windDirection.x, 0f, CurrentWeather.windDirection.z);
	}

	public int GetDayOfTimeWeatherIndex()
	{
		int num = changeWeatherAtHours.Length - 1;
		if (RealWorldTimeLight.time.currentHour == 0)
		{
			return num;
		}
		for (int num2 = changeWeatherAtHours.Length - 1; num2 > 0; num2--)
		{
			if (RealWorldTimeLight.time.currentHour < changeWeatherAtHours[num2])
			{
				num--;
			}
		}
		return num;
	}

	public void ClearWeatherOnSpecialDay()
	{
		if (GetIsSpecialDayThatShouldBeSunny(WorldManager.Instance.day, WorldManager.Instance.week, WorldManager.Instance.month))
		{
			List<WeatherData> clearWeatherForSpecialDay = GetClearWeatherForSpecialDay();
			NetworkMapSharer.Instance.todaysWeather.Clear();
			for (int i = 0; i < RealWorldTimeLight.ChangeWeatherPatternsNumTimesPerDay; i++)
			{
				NetworkMapSharer.Instance.todaysWeather.Add(clearWeatherForSpecialDay[i]);
			}
		}
		if (WorldManager.Instance.month != 3 && NetworkMapSharer.Instance.todaysWeather[0].isSnowDay)
		{
			NetworkMapSharer.Instance.todaysWeather[0].isSnowDay = false;
		}
	}

	private bool GetIsSpecialDayThatShouldBeSunny(int checkDay, int checkWeek, int checkMonth)
	{
		if (!IsFirstTwoDaysOfPlaying(checkDay, checkWeek, checkMonth))
		{
			return TownEventManager.manage.checkEventDay(checkDay, checkWeek, checkMonth) != null;
		}
		return true;
	}

	private bool IsFirstTwoDaysOfPlaying(int checkDay, int checkWeek, int checkMonth)
	{
		if (checkDay <= 2 && checkWeek <= 1 && checkMonth <= 1)
		{
			return WorldManager.Instance.year <= 1;
		}
		return false;
	}

	private void SetTomorrowsWeatherPatternsAsTodays()
	{
		for (int i = 0; i < RealWorldTimeLight.ChangeWeatherPatternsNumTimesPerDay; i++)
		{
			NetworkMapSharer.Instance.todaysWeather[i] = NetworkMapSharer.Instance.tomorrowsWeather[i];
		}
		NetworkMapSharer.Instance.tomorrowsWeather.Clear();
	}

	private void RandomizeTodaysWeatherPatterns()
	{
		List<WeatherData> list = RandomizeNewWeatherPatternsForDay();
		NetworkMapSharer.Instance.todaysWeather.Clear();
		for (int i = 0; i < RealWorldTimeLight.ChangeWeatherPatternsNumTimesPerDay; i++)
		{
			NetworkMapSharer.Instance.todaysWeather.Add(list[i]);
		}
	}

	private void RandomizeTomorrowsWeatherPattern()
	{
		List<WeatherData> list = RandomizeNewWeatherPatternsForDay();
		NetworkMapSharer.Instance.tomorrowsWeather.Clear();
		for (int i = 0; i < RealWorldTimeLight.ChangeWeatherPatternsNumTimesPerDay; i++)
		{
			NetworkMapSharer.Instance.tomorrowsWeather.Add(list[i]);
		}
	}

	private List<WeatherData> RandomizeNewWeatherPatternsForDay()
	{
		List<WeatherData> list = new List<WeatherData>();
		for (int i = 0; i < RealWorldTimeLight.ChangeWeatherPatternsNumTimesPerDay; i++)
		{
			WeatherPattern weatherPattern = AllWeatherPatterns[WorldManager.Instance.month - 1];
			int[] array = RandomizeWindDirection();
			WeatherData weatherData = new WeatherData
			{
				isStormy = (Random.Range(0, 100) < weatherPattern.chanceForStorm),
				isOvercast = (Random.Range(0, 100) < weatherPattern.chanceForOvercast),
				isWindy = (Random.Range(0, 100) < weatherPattern.chanceForWind),
				isHeatWave = (Random.Range(0, 100) < weatherPattern.chanceForHeatWave),
				windDirection = new WindDirection
				{
					x = array[0],
					z = array[1]
				}
			};
			if (changeWeatherAtHours[i] >= 18)
			{
				weatherData.isHeatWave = false;
			}
			if (weatherData.isHeatWave)
			{
				weatherData.isFoggy = false;
				weatherData.isOvercast = false;
			}
			if (changeWeatherAtHours[i] <= 12)
			{
				weatherData.isFoggy = Random.Range(0, 100) < weatherPattern.chanceForFog;
				if (weatherData.isFoggy)
				{
					weatherData.isOvercast = true;
				}
			}
			if (weatherData.isStormy)
			{
				weatherData.isOvercast = true;
				weatherData.isRainy = true;
			}
			if (weatherData.isOvercast)
			{
				weatherData.isRainy = Random.Range(0, 100) < weatherPattern.chanceForRain;
			}
			if (weatherData.isOvercast || weatherData.isRainy || weatherData.isStormy || weatherData.isFoggy)
			{
				weatherData.isHeatWave = false;
			}
			else if (changeWeatherAtHours[i] >= 18)
			{
				int num = Random.Range(0, 100);
				if (WorldManager.Instance.year >= 2)
				{
					num = Mathf.Clamp(num - 3, 0, 100);
				}
				if (WorldManager.Instance.month == 2 || WorldManager.Instance.month == 3)
				{
					num = Mathf.Clamp(num - 3, 0, 100);
				}
				if (NetworkMapSharer.Instance.wishManager.IsWishActive(WishManager.WishType.FortuitousWish))
				{
					num = Mathf.Clamp(num - 4, 0, 100);
				}
				if (num == 0)
				{
					weatherData.isMeteorShower = true;
					weatherData.isHeatWave = false;
				}
				else
				{
					weatherData.isMeteorShower = false;
				}
			}
			if (i == 0 && weatherPattern.canSnow)
			{
				if (weatherData.isRainy)
				{
					weatherData.isSnowDay = true;
				}
				else
				{
					weatherData.isSnowDay = Random.Range(0, 2) == 1;
				}
			}
			list.Add(weatherData);
		}
		return list;
	}

	private List<WeatherData> GetClearWeatherForSpecialDay()
	{
		List<WeatherData> list = new List<WeatherData>();
		for (int i = 0; i < RealWorldTimeLight.ChangeWeatherPatternsNumTimesPerDay; i++)
		{
			_ = AllWeatherPatterns[WorldManager.Instance.month - 1];
			int[] array = RandomizeWindDirection();
			WeatherData weatherData = new WeatherData
			{
				isStormy = false,
				isOvercast = false,
				isWindy = false,
				isHeatWave = false,
				isMeteorShower = false,
				isFoggy = false,
				isSnowDay = false,
				windDirection = new WindDirection
				{
					x = array[0],
					z = array[1]
				}
			};
			weatherData.isMeteorShower = false;
			list.Add(weatherData);
		}
		return list;
	}

	private int[] RandomizeWindDirection()
	{
		int num = 0;
		int num2 = 0;
		while (num == 0 && num2 == 0)
		{
			num = Random.Range(-1, 2);
			num2 = Random.Range(-1, 2);
		}
		return new int[2] { num, num2 };
	}

	private void TransitionBetweenWeatherEvents()
	{
		StartCoroutine(TransitionCoroutine());
	}

	public IEnumerator TransitionCoroutine()
	{
		IsRunningTransition = true;
		float timer = 0f;
		float startingIntensity = RealWorldTimeLight.time.theSun.intensity;
		float startingOverCastAmount = RealWorldTimeLight.time.overCastTintedAmount;
		float startingFogTintAmount = RealWorldTimeLight.time.fogTintAmount;
		int startingHour = RealWorldTimeLight.time.currentHour;
		float startingFogDistance = CameraController.control.foggyDayRollInDistance;
		float desiredFogDistance = CameraController.control.normalFogDistance;
		if (Instance.fogMgr.IsActive || Instance.IsRaining)
		{
			desiredFogDistance = 10f;
			yield return StartCoroutine(FadeCloudsForOvercastChange(1f, startingHour, 3f));
		}
		while (timer <= 10f)
		{
			timer += Time.deltaTime;
			if (fogMgr.IsActive || rainMgr.IsActive)
			{
				RealWorldTimeLight.time.sunsDesiredDayTimeIntensity = Mathf.Lerp(startingIntensity, 0.5f, timer / 10f);
				RealWorldTimeLight.time.theSun.intensity = RealWorldTimeLight.time.sunsDesiredDayTimeIntensity;
				RealWorldTimeLight.time.overCastTintedAmount = Mathf.Lerp(startingOverCastAmount, 0.6f, timer / 10f);
				RealWorldTimeLight.time.fogTintAmount = Mathf.Lerp(startingFogTintAmount, 1f, timer / 10f);
			}
			else
			{
				RealWorldTimeLight.time.sunsDesiredDayTimeIntensity = Mathf.Lerp(startingIntensity, 1f, timer / 10f);
				RealWorldTimeLight.time.theSun.intensity = RealWorldTimeLight.time.sunsDesiredDayTimeIntensity;
				RealWorldTimeLight.time.overCastTintedAmount = Mathf.Lerp(startingOverCastAmount, 0f, timer / 10f);
				RealWorldTimeLight.time.fogTintAmount = Mathf.Lerp(startingFogTintAmount, 0f, timer / 10f);
			}
			CameraController.control.foggyDayRollInDistance = Mathf.Lerp(startingFogDistance, desiredFogDistance, timer / 10f);
			CameraController.control.updateDepthOfFieldAndFog(NewChunkLoader.loader.getChunkDistance());
			if (startingHour != RealWorldTimeLight.time.currentHour)
			{
				break;
			}
			yield return null;
		}
		if (fogMgr.IsActive || rainMgr.IsActive)
		{
			RealWorldTimeLight.time.sunsDesiredDayTimeIntensity = 0.5f;
			RealWorldTimeLight.time.theSun.intensity = RealWorldTimeLight.time.sunsDesiredDayTimeIntensity;
			RealWorldTimeLight.time.overCastTintedAmount = 0.6f;
			RealWorldTimeLight.time.fogTintAmount = 1f;
		}
		else
		{
			RealWorldTimeLight.time.sunsDesiredDayTimeIntensity = 1f;
			RealWorldTimeLight.time.theSun.intensity = RealWorldTimeLight.time.sunsDesiredDayTimeIntensity;
			RealWorldTimeLight.time.overCastTintedAmount = 0f;
			RealWorldTimeLight.time.fogTintAmount = 0f;
		}
		CameraController.control.foggyDayRollInDistance = desiredFogDistance;
		CameraController.control.updateDepthOfFieldAndFog(NewChunkLoader.loader.getChunkDistance());
		if (!IsFoggy && !IsRaining)
		{
			yield return StartCoroutine(FadeCloudsForOvercastChange(0f, startingHour, 10f));
		}
		IsRunningTransition = false;
	}

	private IEnumerator FadeCloudsForOvercastChange(float lerpTo, int startingHour, float fadeTime)
	{
		float timer = 0f;
		float startingCloudTintAmount = RealWorldTimeLight.time.cloudOvercastLerp;
		while (timer <= fadeTime)
		{
			timer += Time.deltaTime;
			RealWorldTimeLight.time.cloudOvercastLerp = Mathf.Lerp(startingCloudTintAmount, lerpTo, timer / fadeTime);
			if (startingHour != RealWorldTimeLight.time.currentHour)
			{
				break;
			}
			yield return null;
		}
		RealWorldTimeLight.time.cloudOvercastLerp = lerpTo;
	}

	private void UpdatePhotoFog()
	{
		if (!rainMgr.IsActive && fogMgr.IsActive)
		{
			if (IsMyPlayerInside)
			{
				PhotoManager.manage.photoFog.enabled = false;
			}
			else
			{
				PhotoManager.manage.photoFog.enabled = true;
			}
		}
	}

	private int GetCurrentTemperature()
	{
		int seasonAverageTemp = RealWorldTimeLight.time.seasonAverageTemp;
		float num = GenerateMap.generate.getDistanceToCentre((int)CameraController.control.transform.position.x / 2, (int)CameraController.control.transform.position.z / 2, 500f, 825f) * -18f + 18f;
		float num2 = GenerateMap.generate.getDistanceToCentre((int)CameraController.control.transform.position.x / 2, (int)CameraController.control.transform.position.z / 2, 200f, 200f) * 18f + -18f;
		return (int)((float)seasonAverageTemp + num + num2);
	}

	[ClientRpc]
	public void RpcMakeItSunnyToday()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(WeatherManager), "RpcMakeItSunnyToday", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	public bool IsSnowDayRoll(int checkSeed)
	{
		Random.InitState(checkSeed);
		if (Random.Range(0, 101) <= 51)
		{
			return true;
		}
		return false;
	}

	public void PlaceSnowBallsOnSnowDay()
	{
		int num = 100;
		int num2 = 10000;
		int num3 = Random.Range(200, 800);
		int num4 = Random.Range(200, 800);
		while (num2 > 0)
		{
			num3 = Random.Range(200, 800);
			num4 = Random.Range(200, 800);
			if (WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num3, num4]].collectsSnow && !WorldManager.Instance.waterMap[num3, num4] && AnimalManager.manage.checkIfTileIsWalkable(num3, num4))
			{
				num--;
				NetworkMapSharer.Instance.spawnACarryable(SaveLoad.saveOrLoad.carryablePrefabs[15], new Vector3(num3 * 2, WorldManager.Instance.heightMap[num3, num4], num4 * 2));
			}
			num2--;
			if (num == 0)
			{
				num2 = 0;
			}
		}
		int num5 = 5;
		num2 = 10000;
		num3 = Random.Range(200, 800);
		num4 = Random.Range(200, 800);
		while (num2 > 0)
		{
			num3 = Random.Range(200, 800);
			num4 = Random.Range(200, 800);
			if (WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num3, num4]].collectsSnow && !WorldManager.Instance.waterMap[num3, num4] && AnimalManager.manage.checkIfTileIsWalkable(num3, num4))
			{
				num5--;
				NetworkMapSharer.Instance.spawnACarryable(SaveLoad.saveOrLoad.carryablePrefabs[19], new Vector3(num3 * 2, WorldManager.Instance.heightMap[num3, num4], num4 * 2));
			}
			num2--;
			if (num5 == 0)
			{
				num2 = 0;
			}
		}
	}

	public void RemoveSnowBallsAndMenForNoSnowDay()
	{
		List<PickUpAndCarry> list = new List<PickUpAndCarry>();
		for (int i = 0; i < WorldManager.Instance.allCarriables.Count; i++)
		{
			if (WorldManager.Instance.allCarriables[i] != null && (WorldManager.Instance.allCarriables[i].prefabId == 15 || WorldManager.Instance.allCarriables[i].prefabId == 16 || WorldManager.Instance.allCarriables[i].prefabId == 19))
			{
				list.Add(WorldManager.Instance.allCarriables[i]);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			NetworkMapSharer.Instance.DestroyCarryable(list[j]);
		}
	}

	public void ChangeWindPatternsForDay(bool setWindy, bool setHeatWave, bool setRaining, bool setStorming, bool setFoggy, bool setSnowing, bool setMeteorshower)
	{
		NetworkMapSharer.Instance.todaysWeather.Clear();
		for (int i = 0; i < changeWeatherAtHours.Length; i++)
		{
			WeatherData weatherData = new WeatherData();
			weatherData.isWindy = setWindy;
			if (changeWeatherAtHours[i] < 18)
			{
				weatherData.isHeatWave = setHeatWave;
				weatherData.isFoggy = setFoggy;
			}
			else
			{
				weatherData.isMeteorShower = setMeteorshower;
			}
			weatherData.isStormy = setStorming;
			weatherData.isRainy = setRaining;
			if (setStorming)
			{
				weatherData.isRainy = true;
			}
			if (setSnowing)
			{
				weatherData.isSnowDay = true;
				weatherData.isRainy = true;
			}
			if (weatherData.isRainy || weatherData.isStormy)
			{
				weatherData.isOvercast = true;
			}
			if (weatherData.isMeteorShower)
			{
				weatherData.isOvercast = false;
				weatherData.isRainy = false;
				weatherData.isStormy = false;
				weatherData.isSnowDay = false;
			}
			NetworkMapSharer.Instance.todaysWeather.Add(weatherData);
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcMakeItRainTomorrow()
	{
		if (!GetIsSpecialDayThatShouldBeSunny(RealWorldTimeLight.time.getTomorrowsDay(), RealWorldTimeLight.time.getTomorrowsWeek(), RealWorldTimeLight.time.getTomorrowsMonth()))
		{
			for (int i = 0; i < RealWorldTimeLight.ChangeWeatherPatternsNumTimesPerDay; i++)
			{
				NetworkMapSharer.Instance.tomorrowsWeather[i].isRainy = true;
				NetworkMapSharer.Instance.tomorrowsWeather[i].isOvercast = true;
				NetworkMapSharer.Instance.tomorrowsWeather[i].isHeatWave = false;
				NetworkMapSharer.Instance.tomorrowsWeather[i].isMeteorShower = false;
			}
		}
	}

	protected static void InvokeUserCode_RpcMakeItRainTomorrow(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcMakeItRainTomorrow called on server.");
		}
		else
		{
			((WeatherManager)obj).UserCode_RpcMakeItRainTomorrow();
		}
	}

	protected void UserCode_RpcMakeItSunnyToday()
	{
		for (int i = 0; i < RealWorldTimeLight.ChangeWeatherPatternsNumTimesPerDay; i++)
		{
			NetworkMapSharer.Instance.todaysWeather[i].isRainy = false;
			NetworkMapSharer.Instance.todaysWeather[i].isStormy = false;
			NetworkMapSharer.Instance.todaysWeather[i].isFoggy = false;
			NetworkMapSharer.Instance.todaysWeather[i].isOvercast = false;
		}
		ChangeWeather();
		RealWorldTimeLight.time.ChangeLightOnPlayerChosenWeatherChange();
	}

	protected static void InvokeUserCode_RpcMakeItSunnyToday(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcMakeItSunnyToday called on server.");
		}
		else
		{
			((WeatherManager)obj).UserCode_RpcMakeItSunnyToday();
		}
	}

	static WeatherManager()
	{
		RemoteCallHelper.RegisterRpcDelegate(typeof(WeatherManager), "RpcMakeItRainTomorrow", InvokeUserCode_RpcMakeItRainTomorrow);
		RemoteCallHelper.RegisterRpcDelegate(typeof(WeatherManager), "RpcMakeItSunnyToday", InvokeUserCode_RpcMakeItSunnyToday);
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(IsSnowDay);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(IsSnowDay);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			bool isSnowDay = IsSnowDay;
			NetworkIsSnowDay = reader.ReadBool();
			if (!SyncVarEqual(isSnowDay, ref IsSnowDay))
			{
				OnSnowDayChange(isSnowDay, IsSnowDay);
			}
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			bool isSnowDay2 = IsSnowDay;
			NetworkIsSnowDay = reader.ReadBool();
			if (!SyncVarEqual(isSnowDay2, ref IsSnowDay))
			{
				OnSnowDayChange(isSnowDay2, IsSnowDay);
			}
		}
	}
}
