using System.Collections;
using UnityEngine;

public class MeteorShower : WeatherBase
{
	public ParticleSystem meteorShowerPart;

	public GameObject fallingMeteorObject;

	public ASound shootingStarSound;

	private bool dropingMeteor;

	protected override void Show()
	{
		meteorShowerPart.gameObject.SetActive(value: true);
		StartCoroutine(DropMeteors());
		StartCoroutine(StartShootingStars());
	}

	protected override void Hide()
	{
		meteorShowerPart.gameObject.SetActive(value: false);
	}

	private IEnumerator DropMeteors()
	{
		if (dropingMeteor || !NetworkMapSharer.Instance || !NetworkMapSharer.Instance.isServer)
		{
			yield break;
		}
		dropingMeteor = true;
		int dropsAtHour = RealWorldTimeLight.time.getSunSetTime() + Random.Range(1, 6);
		WaitForSeconds wait = new WaitForSeconds(2f);
		while (dropsAtHour >= 24)
		{
			dropsAtHour = RealWorldTimeLight.time.getSunSetTime() + Random.Range(1, 6);
		}
		while (RealWorldTimeLight.time.currentHour >= 18)
		{
			if (RealWorldTimeLight.time.currentHour == dropsAtHour)
			{
				if (!RealWorldTimeLight.time.underGround)
				{
					NetworkMapSharer.Instance.SpawnMeteor();
				}
				break;
			}
			yield return wait;
		}
		dropingMeteor = false;
	}

	private IEnumerator StartShootingStars()
	{
		float randomTimeBeforeNextShootingStar = Random.Range(0.2f, 4f);
		yield return null;
		while (base.IsActive)
		{
			if (!RealWorldTimeLight.time.underGround && (RealWorldTimeLight.time.currentHour >= RealWorldTimeLight.time.getSunSetTime() + 1 || RealWorldTimeLight.time.currentHour == 0))
			{
				if (randomTimeBeforeNextShootingStar > 0f)
				{
					randomTimeBeforeNextShootingStar -= Time.deltaTime;
					if (Random.Range(0, 250) == 2)
					{
						meteorShowerPart.Emit(1);
					}
				}
				else
				{
					randomTimeBeforeNextShootingStar = Random.Range(0.2f, 4f);
					meteorShowerPart.Emit(Random.Range(2, 5));
					SoundManager.Instance.play2DSound(shootingStarSound);
				}
			}
			yield return null;
		}
	}
}
