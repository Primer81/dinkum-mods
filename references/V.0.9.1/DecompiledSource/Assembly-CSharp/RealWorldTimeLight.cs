using System;
using System.Collections;
using System.Runtime.InteropServices;
using I2.Loc;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class RealWorldTimeLight : NetworkBehaviour
{
	public static RealWorldTimeLight time;

	public int seasonAverageTemp = 25;

	public Light theSun;

	public Light theMoon;

	public Light insideLight;

	private Vector3 desiredSunPos;

	public UnityEvent onDayNightChange;

	public UnityEvent clockTickEvent;

	public UnityEvent taskChecker;

	public UnityEvent mineLevelChangeEvent = new UnityEvent();

	[SyncVar(hook = "onTimeChange")]
	public int currentHour;

	[SyncVar(hook = "onMineLevelChange")]
	public int mineLevel;

	[SyncVar(hook = "onUnderGround")]
	public bool underGround;

	[SyncVar(hook = "OnMoveBetweenIsland")]
	public bool offIsland;

	[SyncVar(hook = "OnChangeTimeSpeed")]
	public float timeScale = 2f;

	public int currentMinute;

	public bool isNightTime;

	public TextMeshProUGUI DateText;

	public TextMeshProUGUI TimeText;

	public TextMeshProUGUI DayText;

	public TextMeshProUGUI SeasonText;

	public bool useTime = true;

	private float startingTime;

	private DateTime RealDate;

	private int startTime;

	private float desiredDegrees;

	private float SunRotationDegrees;

	private float vel;

	public int[] seasonAverageTemps;

	public Material skyCloudMaterials;

	public Material glassLightOff;

	public Material glassLightOn;

	public Color LightOnEmission;

	public Color LightOffColor;

	public Color lightOnColour;

	public Color lightOnSeethroughEmission;

	public Color lampEmission;

	public Material outsideWindowMaterial;

	public Material insideWindowMaterial;

	public Material seeThroughGlassOutside;

	public Material lampMaterial;

	public Material doorBlackness;

	public Color doorBlacknessAtNight;

	private Coroutine clientSecondsCount;

	[Header("Night sky stuff")]
	public GameObject moonObject;

	public GameObject stars;

	public Material starMat;

	public Color starColor;

	[Header("Morning Colours -----")]
	public Color sunRiseSetColor;

	public Color fogRiseColor;

	public Color sunRiseSetGroundColor;

	[Header("Daytime Colours -----")]
	public Color defaultSunColor;

	public Color fogDayColor;

	public Color dayTimeGroundColor;

	[Header("Nighttime Colours -----")]
	public Color nightTimeColor;

	public Color fogNightColor;

	public Color nightTimeGroundColor;

	[Header("Different Skys -----")]
	public Color daySky;

	public Color rainSky;

	[Header("Cloud Colours -----")]
	public Material cloudMat;

	public Color defaultCloudColour;

	public Color cloudOverCastColour;

	public Color cloudSunRiseColor;

	public Color cloudNightColor;

	[Header("Other Colours -----")]
	public Color rainingDif;

	public Color rainFogColour;

	public Color lavaLevelFogColour;

	public Color undergroveFogColour;

	[Header("Water Settings----")]
	public Material waterMat;

	public Material waterBedMat;

	public Material waterFallBedMat;

	private Coroutine sunAndLightRoutine;

	public GameObject mineRoof;

	private Coroutine clockRoutine;

	public GameObject clouds;

	private Coroutine undergroundLateRoutine;

	public GameObject mineSporeParticles;

	public ASound rumbleSound;

	private int sunSetTime = 17;

	private int sunSetDif;

	private WaitForSeconds wait = new WaitForSeconds(2f);

	private float currentAtmosphere = 1f;

	[Header("Atmosphere--------")]
	public float dayAtmosphere = 0.65f;

	public float nightAtmosphere = 2f;

	public float sunsDesiredDayTimeIntensity = 1f;

	private string PMText = "<size=10>PM</size>";

	private string AMText = "<size=10>AM</size>";

	public float nightIntensityLight = 0.5f;

	public float nightIntensityAmbient = 0.2f;

	public float nightIntensityReflection = 0.4f;

	public float dayTimeIntensity = 1f;

	public float overCastTintedAmount;

	public float cloudOvercastLerp;

	private float currentSpeed = 2f;

	public float fogTintAmount;

	public WorldArea CurrentWorldArea;

	public static int ChangeWeatherPatternsNumTimesPerDay { get; }

	public bool IsLocalPlayerInside { get; set; }

	public int NetworkcurrentHour
	{
		get
		{
			return currentHour;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref currentHour))
			{
				int oldHour = currentHour;
				SetSyncVar(value, ref currentHour, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					onTimeChange(oldHour, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public int NetworkmineLevel
	{
		get
		{
			return mineLevel;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref mineLevel))
			{
				int oldLevel = mineLevel;
				SetSyncVar(value, ref mineLevel, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, value: true);
					onMineLevelChange(oldLevel, value);
					setSyncVarHookGuard(2uL, value: false);
				}
			}
		}
	}

	public bool NetworkunderGround
	{
		get
		{
			return underGround;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref underGround))
			{
				bool old = underGround;
				SetSyncVar(value, ref underGround, 4uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(4uL))
				{
					setSyncVarHookGuard(4uL, value: true);
					onUnderGround(old, value);
					setSyncVarHookGuard(4uL, value: false);
				}
			}
		}
	}

	public bool NetworkoffIsland
	{
		get
		{
			return offIsland;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref offIsland))
			{
				bool oldValue = offIsland;
				SetSyncVar(value, ref offIsland, 8uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(8uL))
				{
					setSyncVarHookGuard(8uL, value: true);
					OnMoveBetweenIsland(oldValue, value);
					setSyncVarHookGuard(8uL, value: false);
				}
			}
		}
	}

	public float NetworktimeScale
	{
		get
		{
			return timeScale;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref timeScale))
			{
				float oldSpeed = timeScale;
				SetSyncVar(value, ref timeScale, 16uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(16uL))
				{
					setSyncVarHookGuard(16uL, value: true);
					OnChangeTimeSpeed(oldSpeed, value);
					setSyncVarHookGuard(16uL, value: false);
				}
			}
		}
	}

	private void Awake()
	{
		time = this;
	}

	private void OnEnable()
	{
		LocalizationManager.OnLocalizeEvent += setUpDayAndDate;
	}

	public override void OnStartServer()
	{
		if (WorldManager.Instance.day == 1 && WorldManager.Instance.week == 1 && WorldManager.Instance.month == 1 && WorldManager.Instance.year == 1)
		{
			NetworkcurrentHour = 10;
		}
		else
		{
			clockRoutine = StartCoroutine(runClock());
		}
	}

	public override void OnStartClient()
	{
		OnChangeTimeSpeed(timeScale, timeScale);
	}

	public void startTimeOnFirstDay()
	{
		clockRoutine = StartCoroutine(runClock());
	}

	private void Start()
	{
		glassLightOn.EnableKeyword("_EMISSION");
		outsideWindowMaterial.EnableKeyword("_EMISSION");
		insideWindowMaterial.EnableKeyword("_EMISSION");
		seeThroughGlassOutside.EnableKeyword("_EMISSION");
		lampMaterial.EnableKeyword("_EMISSION");
		clockTick();
		if (sunAndLightRoutine == null)
		{
			sunAndLightRoutine = StartCoroutine(moveSunAndLighting());
		}
		onDayNightChange = new UnityEvent();
		onDayNightChange.Invoke();
		int num = currentHour * 3600 + currentMinute * 60 + 60;
		SunRotationDegrees = (float)num * 0.0041667f - 90f;
		desiredDegrees = SunRotationDegrees;
		setUpDayAndDate();
		StartCoroutine(fadeInWindows());
	}

	public void setNewSunsetDif(int newDif)
	{
		sunSetDif = newDif;
	}

	public int getSunSetTime()
	{
		return sunSetTime + sunSetDif;
	}

	private IEnumerator runClock()
	{
		while (true)
		{
			clockTick();
			clockTickEvent.Invoke();
			yield return wait;
			if (currentHour != 0)
			{
				currentMinute++;
			}
			if (currentMinute >= 60)
			{
				currentMinute = 0;
				if (currentHour != 0)
				{
					NetworkcurrentHour = currentHour + 1;
				}
			}
			if (currentMinute == 0 || currentMinute == 7 || currentMinute == 15 || currentMinute == 22 || currentMinute == 30 || currentMinute == 37 || currentMinute == 45 || currentMinute == 53)
			{
				taskChecker.Invoke();
			}
		}
	}

	private IEnumerator clientRunClock(int startingMintue = 0)
	{
		currentMinute = startingMintue;
		while (currentMinute < 60)
		{
			clockTick();
			clockTickEvent.Invoke();
			yield return wait;
			if (currentHour != 0)
			{
				currentMinute++;
			}
		}
	}

	public IEnumerator startNewDay()
	{
		NetworkcurrentHour = 7;
		StopCoroutine(clockRoutine);
		clockRoutine = null;
		while (!NetworkMapSharer.Instance.nextDayIsReady)
		{
			yield return null;
		}
		clockRoutine = StartCoroutine(runClock());
	}

	public IEnumerator startNewDayClient()
	{
		while (!NetworkMapSharer.Instance.nextDayIsReady)
		{
			yield return null;
		}
		StopCoroutine(clientSecondsCount);
		clientSecondsCount = null;
		currentMinute = 0;
		clientSecondsCount = StartCoroutine(clientRunClock());
	}

	public void goInside()
	{
		IsLocalPlayerInside = true;
		if (sunAndLightRoutine != null)
		{
			StopCoroutine(sunAndLightRoutine);
			sunAndLightRoutine = null;
		}
		sunAndLightRoutine = StartCoroutine(moveSunAndLighting());
		theSun.gameObject.SetActive(value: false);
		theMoon.gameObject.SetActive(value: false);
		insideLight.enabled = true;
	}

	public void goOutside()
	{
		IsLocalPlayerInside = false;
		if (underGround)
		{
			ChangeSunColor(Color.black, Color.black, 1f);
			cloudMat.color = Color.clear;
			setFogColor(GetUndergroundFogColour());
		}
		else
		{
			theSun.gameObject.SetActive(value: true);
			theMoon.gameObject.SetActive(value: true);
		}
		if (sunAndLightRoutine != null)
		{
			StopCoroutine(sunAndLightRoutine);
			sunAndLightRoutine = null;
		}
		sunAndLightRoutine = StartCoroutine(moveSunAndLighting());
		insideLight.enabled = false;
	}

	public void ChangeLightOnPlayerChosenWeatherChange()
	{
		sunAndLightRoutine = StartCoroutine(moveSunAndLighting());
	}

	public void nextDay()
	{
		WorldManager.Instance.day++;
		if (WorldManager.Instance.day > 7)
		{
			WorldManager.Instance.week++;
			WorldManager.Instance.day = 1;
		}
		if (WorldManager.Instance.week > 4)
		{
			WorldManager.Instance.week = 1;
			WorldManager.Instance.month++;
		}
		if (WorldManager.Instance.month > 4)
		{
			WorldManager.Instance.month = 1;
			WorldManager.Instance.year++;
		}
		setUpDayAndDate();
	}

	public int getTomorrowsDay()
	{
		if (WorldManager.Instance.day + 1 > 7)
		{
			return 1;
		}
		return WorldManager.Instance.day + 1;
	}

	public int getTomorrowsWeek()
	{
		if (WorldManager.Instance.day + 1 > 7)
		{
			if (WorldManager.Instance.week + 1 > 4)
			{
				return 1;
			}
			return WorldManager.Instance.week + 1;
		}
		return WorldManager.Instance.week;
	}

	public int getTomorrowsMonth()
	{
		if (WorldManager.Instance.day + 1 > 7 && WorldManager.Instance.week + 1 > 4)
		{
			if (WorldManager.Instance.month + 1 > 4)
			{
				return 1;
			}
			return WorldManager.Instance.month + 1;
		}
		return WorldManager.Instance.month;
	}

	public void getDaySkyBox()
	{
		RenderSettings.skybox.SetColor("_SkyTint", GetSkyDayColor());
	}

	public void setUpDayAndDate()
	{
		seasonAverageTemp = seasonAverageTemps[WorldManager.Instance.month - 1];
		DayText.text = getDayName(WorldManager.Instance.day - 1);
		if (DayText.text.Length >= 3)
		{
			DayText.text = DayText.text.Substring(0, 3).ToUpper();
		}
		DateText.text = (WorldManager.Instance.day + (WorldManager.Instance.week - 1) * 7).ToString("00");
		SeasonText.text = getSeasonName(WorldManager.Instance.month - 1);
		if (SeasonText.text.Length >= 3)
		{
			SeasonText.text = SeasonText.text.Substring(0, 3).ToUpper();
		}
		SeasonManager.manage.checkSeasonAndChangeMaterials();
	}

	public void Update()
	{
		desiredDegrees = Mathf.SmoothDamp(desiredDegrees, SunRotationDegrees, ref vel, 1f);
		if (!underGround && currentHour != 0)
		{
			theSun.transform.eulerAngles = new Vector3(desiredDegrees, -135f, 0f);
		}
		else
		{
			theSun.transform.eulerAngles = new Vector3(270f, -135f, 0f);
		}
	}

	public Color GetSkyDayColor()
	{
		if (WeatherManager.Instance.overcastMgr.IsActive)
		{
			return rainSky;
		}
		return daySky;
	}

	private Color getDayCloudColor()
	{
		if (WeatherManager.Instance.IsOvercast)
		{
			return Color.Lerp(Color.grey, Color.clear, 0.75f);
		}
		return Color.Lerp(Color.clear, Color.white, 0.15f);
	}

	public void setDegreesNewDay()
	{
		desiredDegrees = SunRotationDegrees;
	}

	private string returnWith0(int value)
	{
		if (value < 10)
		{
			return "0" + value;
		}
		return value.ToString() ?? "";
	}

	public void clockTick()
	{
		int num = currentHour;
		int num2 = currentMinute;
		if (currentHour >= 12 && currentHour <= 12 + sunSetDif)
		{
			num = 12;
			float num3 = (currentHour - 12) * 60;
			num2 = Mathf.RoundToInt(((float)currentMinute + num3) / (float)(sunSetDif + 1));
		}
		else if (currentHour > 12)
		{
			num -= sunSetDif;
		}
		int num4 = num * 3600 + num2 * 60;
		SunRotationDegrees = (float)num4 * 0.0041667f - 90f;
		SunRotationDegrees %= 360f;
		showTimeOnClock();
	}

	private void showTimeOnClock()
	{
		int num = currentHour;
		if (!OptionsMenu.options.use24HourTime && currentHour > 12)
		{
			num -= 12;
		}
		if (currentHour != 0)
		{
			if (currentHour < 12)
			{
				if (OptionsMenu.options.use24HourTime)
				{
					TimeText.text = num.ToString("00") + ":" + currentMinute.ToString("00");
				}
				else
				{
					TimeText.text = num.ToString("00") + ":" + currentMinute.ToString("00") + AMText;
				}
			}
			else if (OptionsMenu.options.use24HourTime)
			{
				TimeText.text = num.ToString("00") + ":" + currentMinute.ToString("00");
			}
			else
			{
				TimeText.text = num.ToString("00") + ":" + currentMinute.ToString("00") + PMText;
			}
		}
		else
		{
			TimeText.text = ConversationGenerator.generate.GetTimeNameByTag("Late");
		}
		if (currentSpeed != 2f)
		{
			if (currentSpeed > 2f)
			{
				TimeText.text = "<b><<</b>" + TimeText.text;
			}
			else
			{
				TimeText.text = "<b>>></b>" + TimeText.text;
			}
		}
	}

	public void ChangeSunColor(Color changeFrom, Color changeTo, float amount)
	{
		if (underGround)
		{
			theSun.color = changeTo;
		}
		else if (overCastTintedAmount > 0f)
		{
			theSun.color = Color.Lerp(Color.Lerp(changeFrom, changeTo, amount), rainingDif, overCastTintedAmount);
		}
		else
		{
			theSun.color = Color.Lerp(changeFrom, changeTo, amount);
		}
	}

	public void ChangeCloudColour(Color changeFrom, Color changeTo, float amount)
	{
		cloudMat.color = Color.Lerp(changeFrom, changeTo, amount);
	}

	private Color getMorningCloudColour()
	{
		return Color.Lerp(cloudSunRiseColor, cloudOverCastColour, cloudOvercastLerp);
	}

	private Color getDayTimeCloudColour()
	{
		return Color.Lerp(defaultCloudColour, cloudOverCastColour, cloudOvercastLerp);
	}

	public void changeSpeed(float newSpeed)
	{
		NetworktimeScale = newSpeed;
	}

	public void OnChangeTimeSpeed(float oldSpeed, float newSpeed)
	{
		NetworktimeScale = newSpeed;
		currentSpeed = newSpeed;
		wait = new WaitForSeconds(newSpeed);
		if (base.isServer)
		{
			if (oldSpeed != newSpeed)
			{
				if (clockRoutine != null)
				{
					StopCoroutine(clockRoutine);
				}
				clockRoutine = StartCoroutine(runClock());
			}
		}
		else
		{
			if (clientSecondsCount != null)
			{
				StopCoroutine(clientSecondsCount);
			}
			clientSecondsCount = StartCoroutine(clientRunClock(currentMinute));
		}
		if (oldSpeed != newSpeed)
		{
			if (newSpeed > oldSpeed)
			{
				SoundManager.Instance.play2DSound(SoundManager.Instance.slowDownTime);
			}
			else
			{
				SoundManager.Instance.play2DSound(SoundManager.Instance.speedUpTime);
			}
		}
	}

	public float getCurrentSpeed()
	{
		return currentSpeed;
	}

	private void onTimeChange(int oldHour, int newHour)
	{
		currentMinute = 0;
		if (!base.isServer)
		{
			if (clientSecondsCount != null)
			{
				StopCoroutine(clientSecondsCount);
			}
			clientSecondsCount = StartCoroutine(clientRunClock());
		}
		if (newHour == 15 && base.isServer)
		{
			MarketPlaceManager.manage.SpawnNed();
		}
		if (newHour == 10 && base.isServer)
		{
			MarketPlaceManager.manage.SpawnGoGoAgent();
		}
		if (newHour == 24)
		{
			NetworkcurrentHour = 0;
		}
		else
		{
			NetworkcurrentHour = newHour;
			if (oldHour != newHour && (bool)WeatherManager.Instance && WeatherManager.Instance.CheckIfCurrentHourShouldTriggerWeatherChange(currentHour))
			{
				WeatherManager.Instance.ChangeWeather();
			}
		}
		StartCoroutine(fadeInWindows());
		if (sunAndLightRoutine != null)
		{
			StopCoroutine(sunAndLightRoutine);
			sunAndLightRoutine = null;
		}
		sunAndLightRoutine = StartCoroutine(moveSunAndLighting());
	}

	private void onMineLevelChange(int oldLevel, int newLevel)
	{
		NetworkmineLevel = newLevel;
		mineLevelChangeEvent.Invoke();
	}

	private IEnumerator fadeInWindows()
	{
		int checkingMinute = -1;
		if (currentHour >= getSunSetTime() || currentHour == 0)
		{
			while (currentHour == getSunSetTime())
			{
				yield return null;
				if (checkingMinute != currentMinute)
				{
					glassLightOn.color = Color.Lerp(LightOffColor, Color.white, (float)currentMinute / 60f);
					glassLightOn.SetColor("_EmissionColor", Color.Lerp(Color.black, LightOnEmission, (float)currentMinute / 60f));
					glassLightOff.color = Color.Lerp(Color.white, LightOffColor, (float)currentMinute / 60f);
					glassLightOff.SetColor("_EmissionColor", Color.Lerp(LightOnEmission, Color.black, (float)currentMinute / 60f));
					outsideWindowMaterial.color = Color.Lerp(LightOffColor, lightOnColour, (float)currentMinute / 60f);
					outsideWindowMaterial.SetColor("_EmissionColor", Color.Lerp(Color.black, LightOnEmission, (float)currentMinute / 60f));
					seeThroughGlassOutside.SetColor("_EmissionColor", Color.Lerp(Color.black, lightOnSeethroughEmission, (float)currentMinute / 60f));
					lampMaterial.SetColor("_EmissionColor", Color.Lerp(Color.black, lampEmission, (float)currentMinute / 60f));
					insideWindowMaterial.color = Color.Lerp(lightOnColour, Color.Lerp(LightOffColor, Color.black, 0.5f), (float)currentMinute / 60f);
					insideWindowMaterial.SetColor("_EmissionColor", Color.Lerp(LightOnEmission, Color.black, (float)currentMinute / 60f));
					doorBlackness.SetColor("_Color", Color.Lerp(Color.black, doorBlacknessAtNight, (float)currentMinute / 60f));
				}
			}
			glassLightOff.color = LightOffColor;
			glassLightOff.SetColor("_EmissionColor", Color.black);
			glassLightOn.color = Color.white;
			glassLightOn.SetColor("_EmissionColor", LightOnEmission);
			doorBlackness.SetColor("_Color", doorBlacknessAtNight);
			insideWindowMaterial.color = Color.Lerp(LightOffColor, Color.black, 0.5f);
			insideWindowMaterial.SetColor("_EmissionColor", Color.black);
			outsideWindowMaterial.color = lightOnColour;
			outsideWindowMaterial.SetColor("_EmissionColor", LightOnEmission);
			seeThroughGlassOutside.SetColor("_EmissionColor", lightOnSeethroughEmission);
			lampMaterial.SetColor("_EmissionColor", lampEmission);
		}
		else
		{
			outsideWindowMaterial.color = LightOffColor;
			outsideWindowMaterial.SetColor("_EmissionColor", Color.black);
			seeThroughGlassOutside.SetColor("_EmissionColor", Color.black);
			lampMaterial.SetColor("_EmissionColor", Color.black);
			doorBlackness.SetColor("_Color", Color.black);
			insideWindowMaterial.color = lightOnColour;
			insideWindowMaterial.SetColor("_EmissionColor", LightOnEmission);
		}
	}

	private void onRain(bool rain)
	{
		WeatherManager.Instance.rainMgr.UpdateRain();
	}

	private void onUnderGround(bool old, bool newUnderground)
	{
		NetworkunderGround = newUnderground;
		ChangeCurrentWorldArea();
		if (underGround)
		{
			mineRoof.SetActive(value: true);
			WeatherManager.Instance.rainMgr.UpdateRain();
			setFogColor(GetUndergroundFogColour());
			clouds.SetActive(value: false);
			if (CameraController.control.landAmbientAudio.clip != CameraController.control.GetUndergroundAmbienceClip(mineLevel))
			{
				CameraController.control.landAmbientAudio.clip = CameraController.control.GetUndergroundAmbienceClip(mineLevel);
				CameraController.control.landAmbientAudio.Play();
			}
			CameraController.control.landAmbienceMax = 0.25f;
		}
		else
		{
			if (!WeatherManager.Instance.IsOvercast)
			{
				clouds.SetActive(value: true);
			}
			mineRoof.SetActive(value: false);
			if (old && !newUnderground)
			{
				WeatherManager.Instance.CheckIfHasAlreadyRainedTodayAndWaterCropsOnLevelChange();
			}
		}
		onDayNightChange.Invoke();
		CameraController.control.updateDepthOfFieldAndFog(NewChunkLoader.loader.getChunkDistance());
		CameraController.control.undergroundSoundZone.SetActive(newUnderground);
		if (underGround && undergroundLateRoutine == null)
		{
			undergroundLateRoutine = StartCoroutine(undergroundTimerRoutine());
		}
		CameraController.control.SetFogUnderground(underGround);
		GenerateUndergroundMap.generate.TurnOnSpecialUndergroundEffects(underGround, mineLevel);
		if (base.isServer && timeScale != 2f)
		{
			NetworktimeScale = 2f;
		}
	}

	private void OnMoveBetweenIsland(bool oldValue, bool newValue)
	{
		NetworkoffIsland = newValue;
		ChangeCurrentWorldArea();
		if (oldValue && !newValue)
		{
			WeatherManager.Instance.CheckIfHasAlreadyRainedTodayAndWaterCropsOnLevelChange();
		}
		if (offIsland && undergroundLateRoutine == null)
		{
			undergroundLateRoutine = StartCoroutine(undergroundTimerRoutine());
		}
		if (base.isServer && timeScale != 2f)
		{
			NetworktimeScale = 2f;
		}
	}

	private void ChangeCurrentWorldArea()
	{
		if (!underGround && !offIsland)
		{
			CurrentWorldArea = WorldArea.MAIN_ISLAND;
		}
		else if (underGround)
		{
			CurrentWorldArea = WorldArea.UNDERGROUND;
		}
		else if (offIsland)
		{
			CurrentWorldArea = WorldArea.OFF_ISLAND;
		}
	}

	private Color GetUndergroundFogColour()
	{
		if (mineLevel == 2)
		{
			return lavaLevelFogColour;
		}
		if (mineLevel == 1)
		{
			return undergroveFogColour;
		}
		return Color.black;
	}

	private IEnumerator moveSunAndLighting()
	{
		if (IsLocalPlayerInside)
		{
			RenderSettings.ambientIntensity = dayTimeIntensity;
			RenderSettings.reflectionIntensity = 1f;
		}
		else if (underGround)
		{
			RenderSettings.ambientIntensity = 0.15f;
			RenderSettings.reflectionIntensity = 0.25f;
			setFogColor(GetUndergroundFogColour());
			waterMat.SetFloat("_Darkness", 4f);
		}
		else
		{
			bool flag = isNightTime;
			if (currentHour >= getSunSetTime() || currentHour <= 6)
			{
				isNightTime = true;
			}
			else
			{
				isNightTime = false;
			}
			if (isNightTime != flag)
			{
				onDayNightChange.Invoke();
			}
			if (currentHour == getSunSetTime())
			{
				theMoon.enabled = true;
				theMoon.shadows = LightShadows.None;
				theMoon.shadowStrength = 0.3f;
				theSun.enabled = true;
				stars.SetActive(value: true);
				moonObject.SetActive(value: true);
				CameraController.control.landAmbienceMax = 0f;
				float lastTime = -1f;
				while (currentHour == getSunSetTime())
				{
					yield return null;
					float num = (float)currentMinute / 60f;
					SetAtmosphere(Mathf.Lerp(currentAtmosphere, Mathf.Lerp(dayAtmosphere, nightAtmosphere, num), Time.deltaTime));
					theSun.intensity = Mathf.Lerp(theSun.intensity, Mathf.Lerp(sunsDesiredDayTimeIntensity, 0f, num), Time.deltaTime);
					theMoon.intensity = Mathf.Lerp(theMoon.intensity, Mathf.Lerp(0f, 0.25f, num), Time.deltaTime);
					if (num != lastTime)
					{
						RenderSettings.ambientIntensity = Mathf.Lerp(dayTimeIntensity, nightIntensityAmbient, num);
						RenderSettings.reflectionIntensity = Mathf.Lerp(1f, nightIntensityReflection, num);
						ChangeSunColor(sunRiseSetColor, nightTimeColor, num);
						ChangeCloudColour(getMorningCloudColour(), cloudNightColor, num);
						setFogColor(Color.Lerp(getMorningFogColour(), fogNightColor, num));
						starColor.a = Mathf.Lerp(0f, 1f, num);
						starMat.color = starColor;
						waterMat.SetFloat("_Darkness", Mathf.Lerp(1f, 4f, num));
						waterFallBedMat.SetFloat("_Darkness", Mathf.Lerp(1f, 4f, num));
						waterBedMat.SetFloat("_Darkness", Mathf.Lerp(1f, 4f, num));
						lastTime = num;
					}
				}
			}
			else if (currentHour == 6)
			{
				theMoon.enabled = true;
				theMoon.shadows = LightShadows.None;
				theMoon.shadowStrength = 0f;
				theSun.enabled = true;
				CameraController.control.landAmbienceMax = 0f;
				stars.SetActive(value: true);
				moonObject.SetActive(value: true);
				float lastTime = -1f;
				while (currentHour == 6)
				{
					yield return null;
					float num2 = (float)currentMinute / 60f;
					SetAtmosphere(Mathf.Lerp(currentAtmosphere, Mathf.Lerp(nightAtmosphere, dayAtmosphere, num2), Time.deltaTime));
					theSun.intensity = Mathf.Lerp(theSun.intensity, Mathf.Lerp(0f, sunsDesiredDayTimeIntensity, num2), Time.deltaTime);
					theMoon.intensity = Mathf.Lerp(theMoon.intensity, Mathf.Lerp(0.25f, 0f, num2), Time.deltaTime);
					if (num2 != lastTime)
					{
						RenderSettings.ambientIntensity = Mathf.Lerp(nightIntensityAmbient, dayTimeIntensity, num2);
						RenderSettings.reflectionIntensity = Mathf.Lerp(nightIntensityReflection, 1f, num2);
						ChangeSunColor(nightTimeColor, sunRiseSetColor, num2);
						ChangeCloudColour(cloudNightColor, getMorningCloudColour(), num2);
						setFogColor(Color.Lerp(fogNightColor, getMorningFogColour(), num2));
						starColor.a = Mathf.Lerp(1f, 0f, num2);
						starMat.color = starColor;
						waterMat.SetFloat("_Darkness", Mathf.Lerp(4f, 1f, num2));
						waterBedMat.SetFloat("_Darkness", Mathf.Lerp(4f, 1f, num2));
						waterFallBedMat.SetFloat("_Darkness", Mathf.Lerp(4f, 1f, num2));
					}
				}
			}
			else if (currentHour < 6 || currentHour > getSunSetTime())
			{
				stars.SetActive(value: true);
				moonObject.SetActive(value: true);
				starColor.a = 1f;
				starMat.color = starColor;
				theMoon.enabled = true;
				theSun.enabled = false;
				theSun.intensity = 0f;
				theMoon.intensity = 0.25f;
				theMoon.shadows = LightShadows.Soft;
				if (theMoon.shadowStrength != 0.3f)
				{
					StartCoroutine(FadeMoonShadowsIn());
				}
				ChangeSunColor(nightTimeColor, nightTimeColor, 1f);
				ChangeCloudColour(cloudNightColor, cloudNightColor, 1f);
				SetAtmosphere(nightAtmosphere);
				RenderSettings.ambientIntensity = nightIntensityAmbient;
				RenderSettings.reflectionIntensity = nightIntensityReflection;
				setFogColor(fogNightColor);
				waterMat.SetFloat("_Darkness", 4f);
				waterFallBedMat.SetFloat("_Darkness", 4f);
				waterBedMat.SetFloat("_Darkness", 4f);
				if (CameraController.control.landAmbientAudio.clip != CameraController.control.nightTimeAmbience)
				{
					CameraController.control.landAmbientAudio.clip = CameraController.control.nightTimeAmbience;
					CameraController.control.landAmbientAudio.Play();
				}
				CameraController.control.landAmbienceMax = 0.25f;
				while (currentHour == getSunSetTime() + 1)
				{
					yield return null;
					CameraController.control.landAmbienceMax = Mathf.Lerp(0f, 0.25f, (float)currentMinute / 60f);
				}
				while (currentHour == 5)
				{
					yield return null;
					CameraController.control.landAmbienceMax = Mathf.Lerp(0.25f, 0f, (float)currentMinute / 60f);
				}
			}
			else
			{
				stars.SetActive(value: false);
				moonObject.SetActive(value: false);
				theMoon.enabled = false;
				theSun.enabled = true;
				theSun.intensity = sunsDesiredDayTimeIntensity;
				theMoon.intensity = 0f;
				SetAtmosphere(dayAtmosphere);
				RenderSettings.ambientIntensity = dayTimeIntensity;
				RenderSettings.reflectionIntensity = 1f;
				if (CameraController.control.landAmbientAudio.clip != CameraController.control.dayTimeAmbience)
				{
					CameraController.control.landAmbientAudio.clip = CameraController.control.dayTimeAmbience;
					CameraController.control.landAmbientAudio.Play();
				}
				CameraController.control.landAmbienceMax = 0.25f;
				waterMat.SetFloat("_Darkness", 1f);
				waterFallBedMat.SetFloat("_Darkness", 1f);
				waterBedMat.SetFloat("_Darkness", 1f);
				if (currentHour == getSunSetTime() - 2)
				{
					while (currentHour == getSunSetTime() - 2)
					{
						yield return null;
						setFogColor(getDayTimeFogColour());
						ChangeSunColor(defaultSunColor, sunRiseSetColor, (float)currentMinute / 60f);
						ChangeCloudColour(getDayCloudColor(), getMorningCloudColour(), (float)currentMinute / 60f);
					}
				}
				else if (currentHour == getSunSetTime() - 1 || currentHour == 7)
				{
					ChangeSunColor(sunRiseSetColor, sunRiseSetColor, 1f);
					ChangeCloudColour(getMorningCloudColour(), getMorningCloudColour(), 1f);
					while (currentHour == 7)
					{
						yield return null;
						ChangeSunColor(sunRiseSetColor, sunRiseSetColor, 1f);
						ChangeCloudColour(getMorningCloudColour(), getMorningCloudColour(), 1f);
						setFogColor(Color.Lerp(getMorningFogColour(), getDayTimeFogColour(), (float)currentMinute / 60f));
						CameraController.control.landAmbienceMax = Mathf.Lerp(0f, 0.25f, (float)currentMinute / 60f);
					}
					while (currentHour == getSunSetTime() - 1)
					{
						yield return null;
						setFogColor(Color.Lerp(getDayTimeFogColour(), getMorningFogColour(), (float)currentMinute / 60f));
					}
				}
				else if (currentHour == 8)
				{
					while (currentHour == 8)
					{
						yield return null;
						setFogColor(getDayTimeFogColour());
						ChangeSunColor(sunRiseSetColor, defaultSunColor, (float)currentMinute / 60f);
						ChangeCloudColour(getMorningCloudColour(), getDayTimeCloudColour(), (float)currentMinute / 60f);
					}
				}
				else
				{
					ChangeCloudColour(getDayTimeCloudColour(), getDayTimeCloudColour(), 1f);
					if (WeatherManager.Instance.IsRunningTransition)
					{
						int startingHour = currentHour;
						while (WeatherManager.Instance.IsRunningTransition)
						{
							ChangeSunColor(defaultSunColor, defaultSunColor, 1f);
							ChangeCloudColour(getDayTimeCloudColour(), getDayTimeCloudColour(), 1f);
							setFogColor(getDayTimeFogColour());
							yield return null;
							if (startingHour != currentHour)
							{
								yield break;
							}
						}
					}
					else
					{
						setFogColor(getDayTimeFogColour());
						ChangeSunColor(defaultSunColor, defaultSunColor, 1f);
					}
				}
			}
		}
		sunAndLightRoutine = null;
	}

	public void SetAtmosphere(float newCurrentAtmosphere)
	{
		currentAtmosphere = newCurrentAtmosphere;
		RenderSettings.skybox.SetFloat("_AtmosphereThickness", currentAtmosphere);
	}

	public string getDayName(int dayId)
	{
		return (LocalizedString)("Time/dayName_" + dayId);
	}

	public string getSeasonName(int seasonId)
	{
		return (LocalizedString)("Time/seasonName_" + seasonId);
	}

	public void setFogColor(Color set)
	{
		RenderSettings.fogColor = set;
		RenderSettings.skybox.SetColor("_GroundColor", RenderSettings.fogColor);
		waterMat.SetColor("_FogColor", set);
		waterFallBedMat.SetColor("_FogColor", set);
		waterBedMat.SetColor("_FogColor", set);
	}

	public Color getDayTimeFogColour()
	{
		if (fogTintAmount > 0f)
		{
			return Color.Lerp(fogDayColor, rainFogColour, fogTintAmount);
		}
		return fogDayColor;
	}

	public Color getMorningFogColour()
	{
		if (WeatherManager.Instance.IsOvercast)
		{
			return rainFogColour;
		}
		return fogRiseColor;
	}

	[ClientRpc]
	public void RPCMinesRumble()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(RealWorldTimeLight), "RPCMinesRumble", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator playSoundAndShakeScreen()
	{
		if (underGround || offIsland)
		{
			float timer = 2.5f;
			SoundManager.Instance.play2DSound(rumbleSound);
			while (timer > 0f && underGround)
			{
				timer -= Time.deltaTime;
				CameraController.control.shakeScreenMax(timer / 10f, 0.15f);
				yield return null;
			}
		}
	}

	private IEnumerator undergroundTimerRoutine()
	{
		float damageTimer = 0f;
		float rumbleTimer = 0f;
		float rumbleTimerMax = UnityEngine.Random.Range(5f, 10f);
		bool evelevenRumble = false;
		bool elevenThirty = false;
		while (underGround || offIsland)
		{
			yield return null;
			if (!evelevenRumble && currentHour == 23)
			{
				evelevenRumble = true;
				StartCoroutine(playSoundAndShakeScreen());
			}
			if (!elevenThirty && currentHour == 23 && currentMinute == 30)
			{
				elevenThirty = true;
				StartCoroutine(playSoundAndShakeScreen());
			}
			if (currentHour == 0)
			{
				if (NetworkMapSharer.Instance.localChar.transform.position.y > -5f)
				{
					if (!mineSporeParticles.activeInHierarchy)
					{
						mineSporeParticles.SetActive(value: true);
					}
				}
				else if (mineSporeParticles.activeInHierarchy)
				{
					mineSporeParticles.SetActive(value: false);
				}
				if (!base.isServer)
				{
					continue;
				}
				if (damageTimer >= 1.8f)
				{
					damageTimer = 0f;
					for (int i = 0; i < NetworkNavMesh.nav.charsConnected.Count; i++)
					{
						if (NetworkNavMesh.nav.charsConnected[i].transform.position.y > -5f)
						{
							NetworkNavMesh.nav.charsConnected[i].GetComponent<Damageable>().changeHealth(-1);
						}
					}
				}
				else
				{
					damageTimer += Time.deltaTime;
				}
				rumbleTimer += Time.deltaTime;
				if (rumbleTimer >= rumbleTimerMax)
				{
					RPCMinesRumble();
					rumbleTimer = 0f;
					rumbleTimerMax = UnityEngine.Random.Range(5f, 10f);
				}
			}
			else
			{
				mineSporeParticles.SetActive(value: false);
			}
		}
		mineSporeParticles.SetActive(value: false);
		undergroundLateRoutine = null;
	}

	private IEnumerator FadeMoonShadowsIn()
	{
		float timer = 0f;
		while (timer < 1f)
		{
			theMoon.shadowStrength = Mathf.Lerp(0f, 0.3f, timer);
			timer += Time.deltaTime;
			yield return null;
		}
		theMoon.shadowStrength = 0.3f;
	}

	private void OnDestroy()
	{
		LocalizationManager.OnLocalizeEvent -= setUpDayAndDate;
		RenderSettings.skybox.SetColor("_SkyTint", daySky);
		outsideWindowMaterial.color = LightOffColor;
		outsideWindowMaterial.SetColor("_EmissionColor", Color.black);
		seeThroughGlassOutside.SetColor("_EmissionColor", Color.black);
		doorBlackness.SetColor("_Color", Color.black);
		insideWindowMaterial.color = lightOnColour;
		insideWindowMaterial.SetColor("_EmissionColor", LightOnEmission);
		lampMaterial.SetColor("_EmissionColor", Color.black);
		ChangeSunColor(defaultSunColor, defaultSunColor, 1f);
		ChangeCloudColour(defaultCloudColour, defaultCloudColour, 1f);
		setFogColor(fogDayColor);
		waterMat.SetFloat("_Darkness", 1f);
		waterFallBedMat.SetFloat("_Darkness", 1f);
		waterBedMat.SetFloat("_Darkness", 1f);
	}

	static RealWorldTimeLight()
	{
		ChangeWeatherPatternsNumTimesPerDay = 3;
		RemoteCallHelper.RegisterRpcDelegate(typeof(RealWorldTimeLight), "RPCMinesRumble", InvokeUserCode_RPCMinesRumble);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RPCMinesRumble()
	{
		StartCoroutine(playSoundAndShakeScreen());
	}

	protected static void InvokeUserCode_RPCMinesRumble(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RPCMinesRumble called on server.");
		}
		else
		{
			((RealWorldTimeLight)obj).UserCode_RPCMinesRumble();
		}
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(currentHour);
			writer.WriteInt(mineLevel);
			writer.WriteBool(underGround);
			writer.WriteBool(offIsland);
			writer.WriteFloat(timeScale);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(currentHour);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteInt(mineLevel);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteBool(underGround);
			result = true;
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteBool(offIsland);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			writer.WriteFloat(timeScale);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = currentHour;
			NetworkcurrentHour = reader.ReadInt();
			if (!SyncVarEqual(num, ref currentHour))
			{
				onTimeChange(num, currentHour);
			}
			int num2 = mineLevel;
			NetworkmineLevel = reader.ReadInt();
			if (!SyncVarEqual(num2, ref mineLevel))
			{
				onMineLevelChange(num2, mineLevel);
			}
			bool flag = underGround;
			NetworkunderGround = reader.ReadBool();
			if (!SyncVarEqual(flag, ref underGround))
			{
				onUnderGround(flag, underGround);
			}
			bool flag2 = offIsland;
			NetworkoffIsland = reader.ReadBool();
			if (!SyncVarEqual(flag2, ref offIsland))
			{
				OnMoveBetweenIsland(flag2, offIsland);
			}
			float num3 = timeScale;
			NetworktimeScale = reader.ReadFloat();
			if (!SyncVarEqual(num3, ref timeScale))
			{
				OnChangeTimeSpeed(num3, timeScale);
			}
			return;
		}
		long num4 = (long)reader.ReadULong();
		if ((num4 & 1L) != 0L)
		{
			int num5 = currentHour;
			NetworkcurrentHour = reader.ReadInt();
			if (!SyncVarEqual(num5, ref currentHour))
			{
				onTimeChange(num5, currentHour);
			}
		}
		if ((num4 & 2L) != 0L)
		{
			int num6 = mineLevel;
			NetworkmineLevel = reader.ReadInt();
			if (!SyncVarEqual(num6, ref mineLevel))
			{
				onMineLevelChange(num6, mineLevel);
			}
		}
		if ((num4 & 4L) != 0L)
		{
			bool flag3 = underGround;
			NetworkunderGround = reader.ReadBool();
			if (!SyncVarEqual(flag3, ref underGround))
			{
				onUnderGround(flag3, underGround);
			}
		}
		if ((num4 & 8L) != 0L)
		{
			bool flag4 = offIsland;
			NetworkoffIsland = reader.ReadBool();
			if (!SyncVarEqual(flag4, ref offIsland))
			{
				OnMoveBetweenIsland(flag4, offIsland);
			}
		}
		if ((num4 & 0x10L) != 0L)
		{
			float num7 = timeScale;
			NetworktimeScale = reader.ReadFloat();
			if (!SyncVarEqual(num7, ref timeScale))
			{
				OnChangeTimeSpeed(num7, timeScale);
			}
		}
	}
}
