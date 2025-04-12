using System.Collections;
using Mirror;
using UnityEngine;

public class BombExplodes : NetworkBehaviour
{
	public GameObject hideOnExplode;

	public ASound bombExplodesSound;

	public LayerMask landOnMask;

	public LayerMask damageLayer;

	public Light explosionLight;

	public bool isToadBomb;

	public GameObject toadBombEffect;

	public override void OnStartClient()
	{
		if (!isToadBomb)
		{
			StartCoroutine(explodeTimer());
		}
		else
		{
			StartCoroutine(ToadBombExplodeTimer());
		}
	}

	private IEnumerator ToadBombExplodeTimer()
	{
		float timeBeforeExplode = 2f;
		int num = Mathf.RoundToInt(base.transform.position.x / 2f);
		int num2 = Mathf.RoundToInt(base.transform.position.z / 2f);
		int fallToHeight = -2;
		if (WorldManager.Instance.isPositionOnMap(num, num2))
		{
			fallToHeight = WorldManager.Instance.heightMap[num, num2];
		}
		new Vector3(base.transform.position.x, fallToHeight, base.transform.position.z);
		float fallSpeed = 9f;
		while (timeBeforeExplode > 0f)
		{
			timeBeforeExplode -= Time.deltaTime;
			if (base.transform.position.y > (float)fallToHeight && !Physics.Raycast(base.transform.position - Vector3.up / 10f, Vector3.down, out var _, 0.12f, landOnMask))
			{
				base.transform.position += Vector3.down * Time.deltaTime * fallSpeed;
				fallSpeed = Mathf.Lerp(fallSpeed, 15f, Time.deltaTime * 2f);
			}
			yield return null;
		}
		StartCoroutine(explosionLightFlash());
		hideOnExplode.SetActive(value: false);
		Vector3 position = base.transform.position;
		SoundManager.Instance.playASoundAtPoint(bombExplodesSound, position);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.toadSplosion, position, 80);
		Object.Instantiate(toadBombEffect, base.transform.position, Quaternion.identity);
		if (base.isServer)
		{
			yield return new WaitForSeconds(0.5f);
			NetworkServer.Destroy(base.gameObject);
		}
	}

	private IEnumerator explodeTimer()
	{
		float timeBeforeExplode = 2f;
		int xPos = Mathf.RoundToInt(base.transform.position.x / 2f);
		int yPos = Mathf.RoundToInt(base.transform.position.z / 2f);
		int fallToHeight = -2;
		if (WorldManager.Instance.isPositionOnMap(xPos, yPos))
		{
			fallToHeight = WorldManager.Instance.heightMap[xPos, yPos];
		}
		new Vector3(base.transform.position.x, fallToHeight, base.transform.position.z);
		float fallSpeed = 9f;
		while (timeBeforeExplode > 0f)
		{
			timeBeforeExplode -= Time.deltaTime;
			if (base.transform.position.y > (float)fallToHeight && !Physics.Raycast(base.transform.position - Vector3.up / 10f, Vector3.down, out var _, 0.12f, landOnMask))
			{
				base.transform.position += Vector3.down * Time.deltaTime * fallSpeed;
				fallSpeed = Mathf.Lerp(fallSpeed, 15f, Time.deltaTime * 2f);
			}
			yield return null;
		}
		Vector3 particlePos = base.transform.position;
		SoundManager.Instance.playASoundAtPoint(bombExplodesSound, particlePos);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.explosion, particlePos, 60);
		float num = Vector3.Distance(CameraController.control.transform.position, base.transform.position);
		float num2 = 0.75f - Mathf.Clamp(num / 25f, 0f, 0.75f);
		if (num2 > 0f)
		{
			CameraController.control.shakeScreen(num2);
		}
		StartCoroutine(explosionLightFlash());
		hideOnExplode.SetActive(value: false);
		if (base.isServer)
		{
			blowUpPos(xPos, yPos, 0, 0);
			Collider[] array = Physics.OverlapSphere(base.transform.position - Vector3.up * 1.5f, 4f, damageLayer);
			for (int i = 0; i < array.Length; i++)
			{
				Damageable component = array[i].transform.root.GetComponent<Damageable>();
				if ((bool)component)
				{
					component.attackAndDoDamage(25, base.transform);
					component.setOnFire();
				}
			}
		}
		ParticleManager.manage.emitAttackParticle(particlePos, 50);
		ParticleManager.manage.emitRedAttackParticle(particlePos, 25);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], particlePos, 50);
		yield return new WaitForSeconds(0.05f);
		ParticleManager.manage.emitAttackParticle(particlePos + Vector3.left * 2f, 25);
		ParticleManager.manage.emitAttackParticle(particlePos + Vector3.right * 2f, 25);
		ParticleManager.manage.emitAttackParticle(particlePos + Vector3.forward * 2f, 25);
		ParticleManager.manage.emitAttackParticle(particlePos + Vector3.back * 2f, 25);
		ParticleManager.manage.emitRedAttackParticle(particlePos + Vector3.left * 2f, 25);
		ParticleManager.manage.emitRedAttackParticle(particlePos + Vector3.right * 2f, 25);
		ParticleManager.manage.emitRedAttackParticle(particlePos + Vector3.forward * 2f, 25);
		ParticleManager.manage.emitRedAttackParticle(particlePos + Vector3.back * 2f, 25);
		if (base.isServer)
		{
			blowUpPos(xPos, yPos, 1, 0);
			blowUpPos(xPos, yPos, -1, 0);
			blowUpPos(xPos, yPos, 0, 1);
			blowUpPos(xPos, yPos, 0, -1);
			yield return null;
			blowUpPos(xPos, yPos, 1, 1);
			blowUpPos(xPos, yPos, -1, -1);
			blowUpPos(xPos, yPos, -1, 1);
			blowUpPos(xPos, yPos, 1, -1);
			yield return null;
			blowUpPos(xPos, yPos, 2, 2, ignoreHeight: true);
			blowUpPos(xPos, yPos, -2, -2, ignoreHeight: true);
			blowUpPos(xPos, yPos, -2, 1, ignoreHeight: true);
			blowUpPos(xPos, yPos, 2, -2, ignoreHeight: true);
		}
		yield return new WaitForSeconds(0.05f);
		hideOnExplode.gameObject.SetActive(value: false);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], particlePos + Vector3.left * 2f, 5);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], particlePos + Vector3.right * 2f, 5);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], particlePos + Vector3.forward * 2f, 5);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], particlePos + Vector3.back * 2f, 5);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], particlePos + Vector3.left * 2f, 5);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], particlePos + Vector3.right * 2f, 5);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], particlePos + Vector3.forward * 2f, 5);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], particlePos + Vector3.back * 2f, 5);
		if (base.isServer)
		{
			yield return new WaitForSeconds(0.5f);
			NetworkServer.Destroy(base.gameObject);
		}
	}

	public void blowUpPos(int xPos, int yPos, int xdif, int yDif, bool ignoreHeight = false)
	{
		if (WorldManager.Instance.isPositionOnMap(xPos + xdif, yPos + yDif) && (shouldDestroyOnTile(xPos + xdif, yPos + yDif) || WorldManager.Instance.onTileMap[xPos + xdif, yPos + yDif] == -1))
		{
			if (WorldManager.Instance.onTileMap[xPos + xdif, yPos + yDif] != -1)
			{
				NetworkMapSharer.Instance.RpcUpdateOnTileObject(-1, xPos + xdif, yPos + yDif);
			}
			if (!ignoreHeight && (WorldManager.Instance.heightMap[xPos, yPos] == WorldManager.Instance.heightMap[xPos + xdif, yPos + yDif] || WorldManager.Instance.heightMap[xPos, yPos] - 1 == WorldManager.Instance.heightMap[xPos + xdif, yPos + yDif] || WorldManager.Instance.heightMap[xPos, yPos] + 1 == WorldManager.Instance.heightMap[xPos + xdif, yPos + yDif]))
			{
				NetworkMapSharer.Instance.changeTileHeight(-1, xPos + xdif, yPos + yDif);
			}
		}
	}

	public bool shouldDestroyOnTile(int xPos, int yPos)
	{
		if (WorldManager.Instance.onTileMap[xPos, yPos] == -1)
		{
			return false;
		}
		if (WorldManager.Instance.onTileMap[xPos, yPos] < -1)
		{
			return false;
		}
		if (WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].isMultiTileObject)
		{
			return false;
		}
		if (WorldManager.Instance.onTileMap[xPos, yPos] > -1 && (WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].isWood || WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].isHardWood || WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].isSmallPlant || WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].isStone || WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].isHardStone))
		{
			return true;
		}
		return false;
	}

	private IEnumerator explosionLightFlash()
	{
		explosionLight.gameObject.SetActive(value: true);
		while (explosionLight.intensity > 0f)
		{
			yield return null;
			explosionLight.intensity -= Time.deltaTime * 15f;
		}
	}

	private void MirrorProcessed()
	{
	}
}
