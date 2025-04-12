using System.Collections;
using UnityEngine;

namespace Dinkum.Weather;

public class Wind : WeatherBase
{
	[SerializeField]
	private ParticleSystem windParticles;

	[SerializeField]
	private ParticleSystem rainParts;

	[SerializeField]
	private ParticleSystem otherRainParts;

	public Material[] windyMaterials;

	[SerializeField]
	private AudioSource windSoundSource;

	[SerializeField]
	private AudioClip[] windSoundClips;

	private ParticleSystem.VelocityOverLifetimeModule windPart;

	private ParticleSystem.VelocityOverLifetimeModule rainWind;

	private ParticleSystem.VelocityOverLifetimeModule rainWind2;

	private ParticleSystem.ShapeModule rainShape;

	private ParticleSystem.ShapeModule rain2Shape;

	private Coroutine windRoutine;

	public Vector3 WindDirection { get; set; } = Vector3.zero;


	public Vector3 TomorrowsWindDrirection { get; set; }

	public float CurrentWindSpeed { get; private set; }

	public bool IsWindBlowing { get; private set; }

	public override void SetUp(WeatherManager weatherMgr)
	{
		base.SetUp(weatherMgr);
		windPart = windParticles.velocityOverLifetime;
		rainWind = rainParts.velocityOverLifetime;
		rainWind2 = otherRainParts.velocityOverLifetime;
		rainShape = rainParts.shape;
		rain2Shape = otherRainParts.shape;
	}

	public void SetWindParticlesShowing(bool value)
	{
		if (!value || base.IsActive)
		{
			windParticles.gameObject.SetActive(value);
		}
	}

	protected override void Show()
	{
	}

	public override void SetActive(bool active)
	{
		base.SetActive(active);
		if (active)
		{
			StopAllCoroutines();
			if (windRoutine != null)
			{
				StopCoroutine(windRoutine);
				windRoutine = null;
			}
			windRoutine = StartCoroutine(HandleWind());
		}
	}

	protected override void Hide()
	{
		SetMaterialsToDefault();
		SetWindDirectionToAllWindParts(0f, 0f);
		CurrentWindSpeed = 0f;
		windSoundSource.Stop();
		if (windRoutine != null)
		{
			StopCoroutine(windRoutine);
			windRoutine = null;
		}
		SetWindParticlesShowing(value: false);
		base.IsActive = false;
	}

	public void StartStormyWind()
	{
		rainShape.scale = new Vector3(55f, 55f, 0f);
		rain2Shape.scale = new Vector3(45f, 45f, 0f);
	}

	public void StopStormyWind()
	{
		rainShape.scale = new Vector3(80f, 80f, 0f);
		rain2Shape.scale = new Vector3(75f, 75f, 0f);
	}

	private IEnumerator HandleWind()
	{
		windSoundSource.enabled = true;
		SetMaterialsToWindy();
		CurrentWindSpeed = 0f;
		if (!weatherMgr.IsMyPlayerInside && !RealWorldTimeLight.time.underGround)
		{
			windParticles.gameObject.SetActive(value: true);
		}
		while (base.IsActive)
		{
			yield return null;
			if (weatherMgr.IsMyPlayerInside || RealWorldTimeLight.time.underGround)
			{
				windSoundSource.Stop();
				SetMaterialsToDefault();
				while (weatherMgr.IsMyPlayerInside || RealWorldTimeLight.time.underGround)
				{
					yield return null;
				}
				SetMaterialsToWindy();
			}
			else if (IsWindBlowing)
			{
				CurrentWindSpeed = Mathf.Lerp(CurrentWindSpeed, 1f, Time.deltaTime / 5f);
				SetWindDirectionToAllWindParts(8f * CurrentWindSpeed + 0.25f, 19f * CurrentWindSpeed + 0.25f);
				SetWindSpeedToMaterials(CurrentWindSpeed);
				yield return null;
				windSoundSource.volume = CurrentWindSpeed * SoundManager.Instance.GetGlobalSoundVolume();
				if (!windSoundSource.isPlaying)
				{
					windSoundSource.pitch = Random.Range(0.95f, 1.25f) * SoundManager.Instance.masterPitch;
					windSoundSource.PlayOneShot(windSoundClips[Random.Range(0, windSoundClips.Length)]);
				}
				if (CurrentWindSpeed >= 0.95f)
				{
					yield return new WaitForSeconds(Random.Range(6f, 7f));
					IsWindBlowing = false;
				}
			}
			else
			{
				CurrentWindSpeed = Mathf.Lerp(CurrentWindSpeed, 0f, Time.deltaTime / 5f);
				SetWindDirectionToAllWindParts(8f * CurrentWindSpeed + 0.1f, 19f * CurrentWindSpeed + 0.1f);
				SetWindSpeedToMaterials(CurrentWindSpeed);
				windSoundSource.volume = CurrentWindSpeed * 0.75f * SoundManager.Instance.GetGlobalSoundVolume();
				if (CurrentWindSpeed <= 0.1f)
				{
					IsWindBlowing = true;
				}
			}
		}
		windRoutine = null;
	}

	private void SetMaterialsToDefault()
	{
		for (int i = 0; i < windyMaterials.Length; i++)
		{
			if ((bool)windyMaterials[i])
			{
				windyMaterials[i].SetFloat("_ShakeTime", 0.5f);
				windyMaterials[i].SetFloat("_ShakeBending", 0.2f);
				windyMaterials[i].SetFloat("_WeatherWind", 1f);
			}
		}
	}

	public void SetMaterialsToWindy()
	{
		for (int i = 0; i < windyMaterials.Length; i++)
		{
			if ((bool)windyMaterials[i])
			{
				windyMaterials[i].SetFloat("_ShakeTime", 1f);
				windyMaterials[i].SetFloat("_ShakeBending", 0.5f);
				windyMaterials[i].SetFloat("_WeatherWind", 0.25f);
			}
		}
	}

	private void SetWindSpeedToMaterials(float currentWindSpeed)
	{
		for (int i = 0; i < windyMaterials.Length; i++)
		{
			if ((bool)windyMaterials[i])
			{
				windyMaterials[i].SetFloat("_WeatherWind", currentWindSpeed * 1.25f);
			}
		}
	}

	private void SetWindDirectionToAllWindParts(float min, float max)
	{
		windPart.x = new ParticleSystem.MinMaxCurve(min * WindDirection.x, max * WindDirection.x);
		rainWind.x = new ParticleSystem.MinMaxCurve(min * WindDirection.x, max * WindDirection.x);
		rainWind2.x = new ParticleSystem.MinMaxCurve(min * WindDirection.x, max * WindDirection.x);
		windPart.z = new ParticleSystem.MinMaxCurve(min * WindDirection.z, max * WindDirection.z);
		rainWind.z = new ParticleSystem.MinMaxCurve(min * WindDirection.z, max * WindDirection.z);
		rainWind2.z = new ParticleSystem.MinMaxCurve(min * WindDirection.z, max * WindDirection.z);
		windPart.y = new ParticleSystem.MinMaxCurve(0f, Mathf.Clamp(max, 0f, 5f));
	}

	public void OnDestroy()
	{
		SetMaterialsToDefault();
	}
}
