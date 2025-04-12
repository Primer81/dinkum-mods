using System.Collections;
using UnityEngine;

namespace Dinkum.Weather;

public class Rain : WeatherBase
{
	[SerializeField]
	private GameObject rainParticleObject;

	[SerializeField]
	private GameObject snowParticleObject;

	[SerializeField]
	private ParticleSystem rainParticleParticleSystem;

	[SerializeField]
	private ParticleSystem rainNoColliderParticleParticleSystem;

	[SerializeField]
	private ParticleSystem snowParticleParticleSystem;

	[SerializeField]
	private ParticleSystem snowNoColliderParticleParticleSystem;

	[SerializeField]
	private AudioSource rainSoundSource;

	[SerializeField]
	private AudioClip rainAudioClip;

	[SerializeField]
	private AudioClip snowAudioClip;

	private ParticleSystem.EmissionModule rainParticleEmissionModule;

	private ParticleSystem.EmissionModule rainNoColliderParticleEmissionModule;

	private ParticleSystem.EmissionModule snowParticleEmissionModule;

	private ParticleSystem.EmissionModule snowNoColliderParticleEmissionModule;

	private float desiredRainVolume = 0.6f;

	private float rainVolumeFadeMultiplier;

	public GameObject rainbowObject;

	private void Start()
	{
		rainParticleEmissionModule = rainParticleParticleSystem.emission;
		rainNoColliderParticleEmissionModule = rainNoColliderParticleParticleSystem.emission;
		snowParticleEmissionModule = snowParticleParticleSystem.emission;
		snowNoColliderParticleEmissionModule = snowNoColliderParticleParticleSystem.emission;
		rainbowObject.GetComponent<Rainbow>().SetDefaultColour();
		SoundManager.Instance.onMasterChange.AddListener(OnChangingWeatherSoundVolume);
	}

	public bool SnowInsteadOfRain()
	{
		if (WeatherManager.Instance.SnowFallPossible())
		{
			return true;
		}
		return false;
	}

	protected override void Show()
	{
		WorldManager.Instance.WetTilledTilesWhenRainStarts();
		if (SnowInsteadOfRain())
		{
			rainParticleObject.SetActive(value: false);
			rainSoundSource.clip = snowAudioClip;
			StartCoroutine(FadeInRain(snowParticleEmissionModule, snowNoColliderParticleEmissionModule));
		}
		else
		{
			snowParticleObject.SetActive(value: true);
			rainSoundSource.clip = rainAudioClip;
			StartCoroutine(FadeInRain(rainParticleEmissionModule, rainNoColliderParticleEmissionModule));
		}
	}

	protected override void Hide()
	{
		if (SnowInsteadOfRain())
		{
			rainParticleObject.SetActive(value: false);
			StartCoroutine(FadeOutRainAndDisable(snowParticleEmissionModule, snowNoColliderParticleEmissionModule));
		}
		else
		{
			snowParticleObject.SetActive(value: true);
			StartCoroutine(FadeOutRainAndDisable(rainParticleEmissionModule, rainNoColliderParticleEmissionModule));
		}
	}

	public void UpdateRain()
	{
		if (!base.IsActive)
		{
			return;
		}
		if (!RealWorldTimeLight.time.underGround)
		{
			if (weatherMgr.IsMyPlayerInside)
			{
				UpdateRainingForCharacterBeingInside();
			}
			else
			{
				UpdateRainingForCharacterBeingOutside();
			}
		}
		else
		{
			DisableRain();
		}
	}

	private void UpdateRainingForCharacterBeingOutside()
	{
		rainSoundSource.enabled = true;
		rainSoundSource.pitch = 1f;
		desiredRainVolume = 0.6f;
		if (SnowInsteadOfRain())
		{
			desiredRainVolume = 0.4f;
		}
		rainSoundSource.volume = desiredRainVolume * SoundManager.Instance.GetGlobalSoundVolume() * rainVolumeFadeMultiplier;
		if (SnowInsteadOfRain())
		{
			rainParticleObject.SetActive(value: false);
			snowParticleObject.SetActive(value: true);
		}
		else
		{
			rainParticleObject.SetActive(value: true);
			snowParticleObject.SetActive(value: false);
		}
		PhotoManager.manage.photoFog.enabled = true;
	}

	private void UpdateRainingForCharacterBeingInside()
	{
		rainSoundSource.enabled = true;
		rainSoundSource.pitch = 0.5f;
		desiredRainVolume = 0.25f;
		rainSoundSource.volume = desiredRainVolume * SoundManager.Instance.GetGlobalSoundVolume() * rainVolumeFadeMultiplier;
		if (SnowInsteadOfRain())
		{
			rainParticleObject.SetActive(value: false);
			snowParticleObject.SetActive(value: false);
		}
		else
		{
			rainParticleObject.SetActive(value: false);
			snowParticleObject.SetActive(value: false);
		}
	}

	private void DisableRain()
	{
		if (rainParticleObject.activeInHierarchy || snowParticleObject.activeInHierarchy)
		{
			rainParticleObject.SetActive(value: false);
			snowParticleObject.SetActive(value: false);
			rainSoundSource.enabled = false;
		}
	}

	private void OnChangingWeatherSoundVolume()
	{
		rainSoundSource.volume = desiredRainVolume * SoundManager.Instance.GetGlobalSoundVolume() * rainVolumeFadeMultiplier;
	}

	public override void SetActive(bool active)
	{
		base.SetActive(active);
		UpdateRain();
	}

	private IEnumerator FadeInRain(ParticleSystem.EmissionModule firstEmmision, ParticleSystem.EmissionModule secondEmission)
	{
		int startingHour = RealWorldTimeLight.time.currentHour;
		float timer = 0f;
		firstEmmision.rateOverTime = 0f;
		while (timer < 10f)
		{
			yield return null;
			timer += Time.deltaTime;
			firstEmmision.rateOverTime = Mathf.Lerp(0f, 600f, timer / 10f);
			secondEmission.rateOverTime = Mathf.Lerp(0f, 2500f, timer / 10f);
			rainVolumeFadeMultiplier = Mathf.Lerp(0f, 1f, timer / 10f);
			rainSoundSource.volume = desiredRainVolume * SoundManager.Instance.GetGlobalSoundVolume() * rainVolumeFadeMultiplier;
			if (startingHour != RealWorldTimeLight.time.currentHour)
			{
				break;
			}
		}
		firstEmmision.rateOverTime = 600f;
		secondEmission.rateOverTime = 2500f;
		rainVolumeFadeMultiplier = 1f;
		rainSoundSource.volume = desiredRainVolume * SoundManager.Instance.GetGlobalSoundVolume() * rainVolumeFadeMultiplier;
	}

	private IEnumerator FadeOutRainAndDisable(ParticleSystem.EmissionModule firstEmmision, ParticleSystem.EmissionModule secondEmission)
	{
		float timer = 0f;
		float startingEmition1 = firstEmmision.rateOverTimeMultiplier;
		float startingEmition2 = secondEmission.rateOverTimeMultiplier;
		int startingHour = RealWorldTimeLight.time.currentHour;
		while (timer < 10f)
		{
			yield return null;
			timer += Time.deltaTime;
			firstEmmision.rateOverTime = Mathf.Lerp(startingEmition1, 0f, timer / 10f);
			secondEmission.rateOverTime = Mathf.Lerp(startingEmition2, 0f, timer / 10f);
			rainVolumeFadeMultiplier = Mathf.Lerp(1f, 0f, timer / 10f);
			rainSoundSource.volume = desiredRainVolume * SoundManager.Instance.GetGlobalSoundVolume() * rainVolumeFadeMultiplier;
			if (startingHour != RealWorldTimeLight.time.currentHour)
			{
				break;
			}
		}
		firstEmmision.rateOverTime = 0f;
		secondEmission.rateOverTime = 0f;
		rainVolumeFadeMultiplier = 0f;
		rainSoundSource.volume = desiredRainVolume * SoundManager.Instance.GetGlobalSoundVolume() * rainVolumeFadeMultiplier;
		while (timer > 0f)
		{
			yield return null;
			timer -= Time.deltaTime * 2f;
			if (startingHour != RealWorldTimeLight.time.currentHour)
			{
				break;
			}
		}
		if (!SnowInsteadOfRain() && RealWorldTimeLight.time.currentHour != 0 && RealWorldTimeLight.time.currentHour <= 13)
		{
			if (!WeatherManager.Instance.IsRaining && !WeatherManager.Instance.IsFoggy)
			{
				Random.InitState(NetworkMapSharer.Instance.mineSeed * NetworkMapSharer.Instance.tomorrowsMineSeed);
				if (Random.Range(0, 100) <= 50)
				{
					rainbowObject.SetActive(value: true);
					Random.InitState(NetworkMapSharer.Instance.mineSeed * NetworkMapSharer.Instance.tomorrowsMineSeed);
					rainbowObject.transform.rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
				}
				else
				{
					rainbowObject.SetActive(value: false);
				}
			}
		}
		else
		{
			rainbowObject.SetActive(value: false);
		}
		if (!WeatherManager.Instance.IsRaining)
		{
			DisableRain();
		}
	}
}
