using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundAttack : MonoBehaviour
{
	private List<Damageable> ringCentre = new List<Damageable>();

	public LayerMask damageLayer;

	public AnimalAI attachedToAnimal;

	public CharMovement attachedToPlayer;

	public ParticleSystem myPart;

	private ParticleSystem.ShapeModule myPartShape;

	[Header("Water AOE Settings")]
	public bool isInWater;

	[Header("Settings")]
	public float startSize = 0.5f;

	public ASound soundEffect;

	[Header("Stay In Position AOE")]
	public bool isDelayed;

	public float remainingTime = 4f;

	public int delayedDamageAmount = 2;

	public bool stunOnly;

	public bool healsEnemies;

	public bool stunsPlayerOnly;

	public void OnEnable()
	{
		myPartShape = myPart.shape;
		myPartShape.radius = startSize;
		if (isDelayed)
		{
			StartCoroutine(growAndStay());
		}
		else
		{
			StartCoroutine(growInSize());
		}
	}

	private IEnumerator growAndStay()
	{
		yield return null;
		float timer = 5f;
		float currentSize = startSize;
		SoundManager.Instance.playASoundAtPoint(soundEffect, base.transform.position);
		while (timer > 0f)
		{
			myPart.Emit(Mathf.RoundToInt(100f * currentSize));
			timer -= Time.deltaTime * 6.5f;
			currentSize += Time.deltaTime * 6.5f;
			yield return null;
			myPartShape.radius = currentSize;
			checkForCollisionsSphere(currentSize);
		}
		float delayTimer = remainingTime;
		while (delayTimer > 0f)
		{
			yield return null;
			delayTimer -= Time.deltaTime;
			checkForCollisionsSphere(currentSize);
			myPart.Emit(Mathf.RoundToInt(100f * currentSize));
		}
		yield return new WaitForSeconds(3f);
		Object.Destroy(base.gameObject);
	}

	private IEnumerator growInSize()
	{
		yield return null;
		if (Vector3.Distance(CameraController.control.transform.position, base.transform.position) < 25f)
		{
			CameraController.control.myShake.addToTraumaMax(0.35f, 0.5f);
		}
		if (isInWater)
		{
			StartCoroutine(PlaySplashOnEntry());
		}
		float timer = 8.5f;
		float currentSize = startSize;
		SoundManager.Instance.playASoundAtPoint(soundEffect, base.transform.position);
		while (timer > 0f)
		{
			myPart.Emit(Mathf.RoundToInt(100f * currentSize));
			timer -= Time.deltaTime * 6.5f;
			currentSize += Time.deltaTime * 6.5f;
			yield return null;
			myPartShape.radius = currentSize;
			checkForCollisionsRing(currentSize);
			if (isInWater)
			{
				myPart.transform.position = new Vector3(myPart.transform.position.x, -0.6f, myPart.transform.position.z);
			}
		}
		yield return new WaitForSeconds(3f);
		Object.Destroy(base.gameObject);
	}

	public void checkForCollisionsSphere(float radius)
	{
		if (!NetworkMapSharer.Instance.isServer)
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, radius, damageLayer);
		for (int i = 0; i < array.Length; i++)
		{
			Damageable componentInParent = array[i].GetComponentInParent<Damageable>();
			if (!componentInParent || ringCentre.Contains(componentInParent))
			{
				continue;
			}
			if (healsEnemies && (bool)componentInParent.isAnAnimal())
			{
				if ((bool)componentInParent.isAnAnimal())
				{
					componentInParent.HealDamage(10);
					ringCentre.Add(componentInParent);
				}
			}
			else if (stunOnly)
			{
				if ((bool)componentInParent.isAnAnimal() && !componentInParent.isStunned())
				{
					componentInParent.stun();
				}
			}
			else
			{
				if ((bool)componentInParent.isAnAnimal() && (!componentInParent.isAnAnimal() || componentInParent.isAnAnimal().animalId == attachedToAnimal.animalId))
				{
					continue;
				}
				if (healsEnemies && (bool)componentInParent.isAnAnimal())
				{
					if ((bool)componentInParent.isAnAnimal())
					{
						componentInParent.HealDamage(1);
					}
				}
				else
				{
					componentInParent.attackAndDoDamage(delayedDamageAmount, attachedToAnimal.transform, 0f);
				}
			}
		}
	}

	public void checkForCollisionsRing(float radius)
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, radius - 0.75f, damageLayer);
		ringCentre.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			ringCentre.Add(array[i].GetComponentInParent<Damageable>());
		}
		Collider[] array2 = Physics.OverlapSphere(base.transform.position, radius, damageLayer);
		for (int j = 0; j < array2.Length; j++)
		{
			if (!(Mathf.Abs(array2[j].transform.root.position.y - base.transform.position.y) < 0.45f) && (!attachedToPlayer || !(Mathf.Abs(array2[j].transform.root.position.y - base.transform.position.y) < 0.85f)))
			{
				continue;
			}
			Damageable componentInParent = array2[j].GetComponentInParent<Damageable>();
			if (!componentInParent || ringCentre.Contains(componentInParent))
			{
				continue;
			}
			if ((bool)attachedToPlayer)
			{
				if (!componentInParent.isFriendly)
				{
					if ((bool)componentInParent && attachedToPlayer == NetworkMapSharer.Instance.localChar)
					{
						TileObjectHealthBar.tile.setCurrentlyHitting(componentInParent);
					}
					if (NetworkMapSharer.Instance.isServer)
					{
						componentInParent.attackAndDoDamage(10, attachedToPlayer.transform, 4f);
					}
				}
			}
			else if ((!componentInParent.isAnAnimal() || ((bool)componentInParent.isAnAnimal() && componentInParent.isAnAnimal().animalId != attachedToAnimal.animalId)) && NetworkMapSharer.Instance.isServer)
			{
				componentInParent.attackAndDoDamage(10, attachedToAnimal.transform, 4f);
			}
		}
	}

	private IEnumerator PlaySplashOnEntry()
	{
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.bigWaterSplash, myPart.transform.position);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.giantSplashPart, myPart.transform.position, 3);
		yield return null;
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.giantSplashPart, myPart.transform.position, 3);
		yield return null;
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.giantSplashPart, myPart.transform.position, 3);
		yield return null;
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.giantSplashPart, myPart.transform.position, 3);
		yield return null;
	}
}
