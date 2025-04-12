using System.Collections;
using UnityEngine;

namespace Dinkum.Weather;

public class Storm : WeatherBase
{
	[SerializeField]
	private ItemHitBox lightningDamageBox;

	[SerializeField]
	private ParticleSystem lightingPart;

	[SerializeField]
	private AudioSource stormSoundSource;

	[SerializeField]
	private AudioClip[] thunderSounds;

	[SerializeField]
	private AudioClip[] lightningCrack;

	private readonly WaitForSeconds minTimeBetweenThunderStrikes = new WaitForSeconds(3.5f);

	protected override void Show()
	{
	}

	public override void SetActive(bool active)
	{
		base.SetActive(active);
		if (active)
		{
			weatherMgr.windMgr.StartStormyWind();
			if (NetworkMapSharer.Instance.isServer)
			{
				StartCoroutine("HandleStorm");
			}
		}
	}

	protected override void Hide()
	{
		weatherMgr.windMgr.StopStormyWind();
		stormSoundSource.Stop();
		if (NetworkMapSharer.Instance.isServer)
		{
			StopCoroutine("HandleStorm");
		}
		base.IsActive = false;
	}

	public void AdjustSoundVolumeToInsideEnvironments()
	{
		stormSoundSource.volume = Random.Range(0.15f, 0.25f) * SoundManager.Instance.GetGlobalSoundVolume();
	}

	public void AdjustSoundVolumeToOutsideEnvironments()
	{
		stormSoundSource.volume = Random.Range(0.35f, 0.5f) * SoundManager.Instance.GetGlobalSoundVolume();
	}

	private IEnumerator HandleStorm()
	{
		int maxLightningStrikesToday = Random.Range(1, 4);
		int strikesThatHappened = 0;
		while (base.IsActive)
		{
			if (Random.Range(0, 7) == 3 && !stormSoundSource.isPlaying)
			{
				if (!RealWorldTimeLight.time.underGround && strikesThatHappened < maxLightningStrikesToday && RealWorldTimeLight.time.currentHour != 0)
				{
					if (Random.Range(0, 130) == 1 || (NetworkMapSharer.Instance.wishManager.IsWishActive(WishManager.WishType.FortuitousWish) && Random.Range(0, 75) == 1))
					{
						Vector3 position = NetworkNavMesh.nav.charsConnected[Random.Range(0, NetworkNavMesh.nav.charsConnected.Count)].position;
						NetworkMapSharer.Instance.RpcThunderStrike(new Vector2(position.x + (float)Random.Range(-15, 15), position.z + (float)Random.Range(-15, 15)));
						strikesThatHappened++;
					}
					else
					{
						NetworkMapSharer.Instance.RpcThunderSound();
					}
				}
				else
				{
					NetworkMapSharer.Instance.RpcThunderSound();
				}
			}
			yield return minTimeBetweenThunderStrikes;
		}
	}

	public void StrikeThunder(Vector2 lightningPos)
	{
		StartCoroutine(PlayLightningStrike(lightningPos));
	}

	public void PlayThunderSound()
	{
		StartCoroutine(PlayLightningStrike(new Vector2(-500f, -500f)));
	}

	private IEnumerator PlayLightningStrike(Vector2 lightningXY)
	{
		if (!RealWorldTimeLight.time.underGround)
		{
			weatherMgr.weatherLight.intensity = 0f;
			weatherMgr.weatherLight.enabled = true;
		}
		Vector3 vector = new Vector3(lightningXY.x, 35f, lightningXY.y);
		if ((int)lightningXY.x / 2 >= 0 && (int)lightningXY.x < WorldManager.Instance.GetMapSize() && (int)lightningXY.y / 2 >= 0 && (int)lightningXY.x / 2 < WorldManager.Instance.GetMapSize())
		{
			vector = new Vector3(lightningXY.x, WorldManager.Instance.heightMap[(int)lightningXY.x / 2, (int)lightningXY.y / 2], lightningXY.y);
		}
		if (Vector3.Distance(vector, CameraController.control.transform.position) < 150f)
		{
			stormSoundSource.pitch = Random.Range(0.85f, 1.1f);
			stormSoundSource.volume = Random.Range(0.25f, 0.35f) * SoundManager.Instance.GetGlobalSoundVolume();
			if (weatherMgr.IsMyPlayerInside || RealWorldTimeLight.time.underGround)
			{
				stormSoundSource.volume = Random.Range(0.15f, 0.25f) * SoundManager.Instance.GetGlobalSoundVolume();
			}
			stormSoundSource.PlayOneShot(lightningCrack[Random.Range(0, lightningCrack.Length)]);
			lightingPart.transform.position = vector + Vector3.up * 15f;
			lightingPart.Emit(1);
			yield return null;
			lightingPart.Emit(1);
		}
		else
		{
			stormSoundSource.pitch = Random.Range(0.85f, 1.1f);
			stormSoundSource.volume = Random.Range(0.15f, 0.25f) * SoundManager.Instance.GetGlobalSoundVolume();
			if (weatherMgr.IsMyPlayerInside || RealWorldTimeLight.time.underGround)
			{
				stormSoundSource.volume = Random.Range(0.15f, 0.25f) * SoundManager.Instance.GetGlobalSoundVolume();
			}
			stormSoundSource.PlayOneShot(thunderSounds[Random.Range(0, thunderSounds.Length)]);
		}
		yield return new WaitForSeconds(0.15f);
		if (RealWorldTimeLight.time.underGround)
		{
			yield break;
		}
		float timer4 = 0f;
		while (timer4 < 1f)
		{
			weatherMgr.weatherLight.intensity = Mathf.Lerp(0f, 0.6f, timer4);
			timer4 += Time.deltaTime * 4f;
			yield return null;
		}
		timer4 = 0f;
		while (timer4 < 1f)
		{
			weatherMgr.weatherLight.intensity = Mathf.Lerp(0f, 0.2f, timer4);
			timer4 += Time.deltaTime * 4f;
			yield return null;
		}
		timer4 = 0f;
		while (timer4 < 1f)
		{
			weatherMgr.weatherLight.intensity = Mathf.Lerp(0f, 0.6f, timer4);
			timer4 += Time.deltaTime * 4f;
			yield return null;
		}
		timer4 = 0f;
		while (timer4 < 1f)
		{
			weatherMgr.weatherLight.intensity = Mathf.Lerp(0.6f, 0f, timer4);
			timer4 += Time.deltaTime * 4f;
			yield return null;
		}
		weatherMgr.weatherLight.intensity = 0f;
		weatherMgr.weatherLight.enabled = false;
		int xPos = (int)lightningXY.x / 2;
		int yPos = (int)lightningXY.y / 2;
		if (WorldManager.Instance.isPositionOnMap(xPos, yPos))
		{
			lightningDamageBox.transform.position = new Vector3((float)xPos * 2f, WorldManager.Instance.heightMap[xPos, yPos], (float)yPos * 2f);
		}
		else
		{
			lightningDamageBox.transform.position = new Vector3((float)xPos * 2f, 0.6f, (float)yPos * 2f);
		}
		lightningDamageBox.startAttack();
		yield return new WaitForSeconds(0.15f);
		yield return new WaitForSeconds(0.15f);
		lightningDamageBox.endAttack();
		if (xPos >= 0 && xPos < WorldManager.Instance.GetMapSize() && yPos >= 0 && yPos < WorldManager.Instance.GetMapSize() && (WorldManager.Instance.onTileMap[xPos, yPos] == -1 || WorldManager.Instance.onTileMap[xPos, yPos] == 0 || WorldManager.Instance.onTileMap[xPos, yPos] == 6 || WorldManager.Instance.onTileMap[xPos, yPos] == 38 || WorldManager.Instance.onTileMap[xPos, yPos] == 136 || (WorldManager.Instance.onTileMap[xPos, yPos] >= 0 && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].isGrass)))
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], new Vector3((float)xPos * 2f, WorldManager.Instance.heightMap[xPos, yPos], (float)yPos * 2f));
			if (NetworkMapSharer.Instance.isServer)
			{
				NetworkMapSharer.Instance.spawnACarryable(SaveLoad.saveOrLoad.carryablePrefabs[8], new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
			}
		}
	}
}
