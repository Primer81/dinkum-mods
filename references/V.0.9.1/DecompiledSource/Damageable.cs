using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Damageable : NetworkBehaviour
{
	public enum onlyDamageWithToolType
	{
		All,
		Wood,
		HardWood,
		Stone,
		HardStone
	}

	[SyncVar(hook = "OnTakeDamage")]
	public int health = 100;

	[SyncVar(hook = "onFireChange")]
	public bool onFire;

	[SyncVar(hook = "onPoisonChange")]
	public bool poisoned;

	[SyncVar(hook = "onStunned")]
	private bool stunned;

	public int maxHealth = 100;

	private AnimalAI myAnimalAi;

	public CharMovement myChar;

	public Transform[] dropPositions;

	public InventoryItemLootTable lootDrops;

	public InventoryItemLootTable guaranteedDrops;

	public GameObject spawnWorldObjectOnDeath;

	public Animator myAnim;

	public bool instantDie;

	public bool isFriendly;

	private Vehicle isVehicle;

	public Transform headPos;

	public ASound customDamageSound;

	public ASound customDeathSound;

	[Header("Can only be damaged by tool")]
	public onlyDamageWithToolType damageType;

	public float defence = 1f;

	[Header("Immunities")]
	public bool fireImmune;

	public bool lightStunImmune;

	public int[] cantBeDamagedBy;

	private float fireTimerTotal = 10f;

	private bool canBeDamaged = true;

	private static WaitForSeconds delayWait;

	private static WaitForSeconds animalWait;

	private bool canBeStunned = true;

	private Coroutine regenRoutine;

	private static WaitForSeconds regenWait;

	public int Networkhealth
	{
		get
		{
			return health;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref health))
			{
				int oldHealth = health;
				SetSyncVar(value, ref health, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					OnTakeDamage(oldHealth, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public bool NetworkonFire
	{
		get
		{
			return onFire;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref onFire))
			{
				bool old = onFire;
				SetSyncVar(value, ref onFire, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, value: true);
					onFireChange(old, value);
					setSyncVarHookGuard(2uL, value: false);
				}
			}
		}
	}

	public bool Networkpoisoned
	{
		get
		{
			return poisoned;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref poisoned))
			{
				bool old = poisoned;
				SetSyncVar(value, ref poisoned, 4uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(4uL))
				{
					setSyncVarHookGuard(4uL, value: true);
					onPoisonChange(old, value);
					setSyncVarHookGuard(4uL, value: false);
				}
			}
		}
	}

	public bool Networkstunned
	{
		get
		{
			return stunned;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref stunned))
			{
				bool old = stunned;
				SetSyncVar(value, ref stunned, 8uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(8uL))
				{
					setSyncVarHookGuard(8uL, value: true);
					onStunned(old, value);
					setSyncVarHookGuard(8uL, value: false);
				}
			}
		}
	}

	private void OnEnable()
	{
		myAnimalAi = GetComponent<AnimalAI>();
		myChar = GetComponent<CharMovement>();
		if ((bool)myChar || (bool)GetComponent<AnimalAI_Pet>() || (bool)GetComponent<NPCAI>() || (bool)GetComponent<FarmAnimal>())
		{
			isFriendly = true;
		}
	}

	public bool checkIfCanBeDamagedBy(int animalId)
	{
		if (cantBeDamagedBy != null)
		{
			for (int i = 0; i < cantBeDamagedBy.Length; i++)
			{
				if (cantBeDamagedBy[i] == animalId)
				{
					return false;
				}
			}
		}
		return true;
	}

	public void doDamageFromStatus(int damageToDeal)
	{
		changeHealth(-damageToDeal);
	}

	public AnimalAI isAnAnimal()
	{
		return myAnimalAi;
	}

	public bool IsAVehicle()
	{
		return isVehicle;
	}

	public void HealDamage(int amount)
	{
		Networkhealth = Mathf.Clamp(health + amount, 0, maxHealth);
		RpcHealFromCasterParticles(amount);
	}

	[ClientRpc]
	private void RpcHealFromCasterParticles(int amount)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(amount);
		SendRPCInternal(typeof(Damageable), "RpcHealFromCasterParticles", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator HealingParticleDelay()
	{
		float timer = 0f;
		while (timer < 2.5f)
		{
			timer += Time.deltaTime;
			yield return null;
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.enemyHealParticle, base.transform.position, 1);
		}
	}

	public void attackAndDoDamage(int damageToDeal, Transform attackedBy, float knockBackAmount = 2.5f)
	{
		if (!canBeDamaged)
		{
			return;
		}
		StartCoroutine(canBeDamagedDelay());
		if ((bool)myAnimalAi && (bool)attackedBy)
		{
			myAnimalAi.takeHitAndKnockBack(attackedBy, knockBackAmount);
		}
		if ((bool)myChar)
		{
			if (NetworkMapSharer.Instance.wishManager.IsWishActive(WishManager.WishType.DangerousWish))
			{
				damageToDeal *= 2;
			}
			if (knockBackAmount > 0f)
			{
				Vector3 knockBackDir = -(attackedBy.position - base.transform.position).normalized;
				knockBackDir.y = 0f;
				myChar.RpcTakeKnockback(knockBackDir, knockBackAmount * 3.5f);
			}
			if (myChar.myEquip.disguiseId != -1)
			{
				myChar.myEquip.NetworkdisguiseId = -1;
			}
		}
		changeHealth(-Mathf.RoundToInt(Mathf.Clamp((float)damageToDeal / defence, 1f, 250f)));
	}

	private IEnumerator canBeDamagedDelay()
	{
		canBeDamaged = false;
		if ((bool)myAnimalAi)
		{
			yield return animalWait;
		}
		else
		{
			yield return delayWait;
		}
		canBeDamaged = true;
	}

	public void changeHealth(int dif)
	{
		Networkhealth = Mathf.Clamp(health + dif, 0, maxHealth);
	}

	private void OnTakeDamage(int oldHealth, int newHealth)
	{
		int num = newHealth - oldHealth;
		newHealth = (((bool)isVehicle && (!isVehicle || isVehicle.hasDriver())) ? health : Mathf.Clamp(newHealth, 0, maxHealth));
		if (oldHealth > newHealth && (bool)myChar && this == StatusManager.manage.connectedDamge)
		{
			StatusManager.manage.takeDamageUIChanges(Mathf.Abs(num));
		}
		if (newHealth < oldHealth && newHealth != maxHealth)
		{
			NotificationManager.manage.createDamageNotification(oldHealth - newHealth, base.transform);
			if ((bool)myAnim && !myAnimalAi)
			{
				if (checkIfShouldShowDamage(num))
				{
					myAnim.SetTrigger("TakeHit");
				}
			}
			else if ((bool)myAnimalAi)
			{
				if (newHealth <= 0)
				{
					onAnimalDeath();
				}
				else if (checkIfShouldShowDamage(num))
				{
					myAnimalAi.takeAHitLocal();
				}
			}
			else if ((bool)isVehicle && !isAnAnimal() && newHealth <= 0)
			{
				for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
				{
					if ((bool)Inventory.Instance.allItems[i].spawnPlaceable && (bool)Inventory.Instance.allItems[i].spawnPlaceable.GetComponent<Vehicle>() && Inventory.Instance.allItems[i].spawnPlaceable.GetComponent<Vehicle>().saveId == isVehicle.saveId)
					{
						NetworkMapSharer.Instance.spawnAServerDrop(i, isVehicle.getVariation() + 1, base.transform.position, null, tryNotToStack: true);
						break;
					}
				}
				if ((bool)GetComponent<VehicleStorage>())
				{
					GetComponent<VehicleStorage>().onDeath();
				}
			}
			if (StatusManager.manage.connectedDamge == this && checkIfShouldShowDamage(num))
			{
				CameraController.control.myShake.addToTraumaMax(0.35f, 0.5f);
			}
			if (checkIfShouldShowDamage(num))
			{
				if (newHealth <= 0)
				{
					ParticleManager.manage.emitAttackParticle(base.transform.position + Vector3.up / 2f, 100);
					ParticleManager.manage.emitRedAttackParticle(base.transform.position + Vector3.up / 2f, 100);
				}
				else
				{
					ParticleManager.manage.emitAttackParticle(base.transform.position + Vector3.up / 2f);
					ParticleManager.manage.emitRedAttackParticle(base.transform.position + Vector3.up / 2f);
				}
			}
			if (checkIfShouldShowDamage(num))
			{
				if ((bool)customDeathSound && newHealth <= 0)
				{
					SoundManager.Instance.playASoundAtPoint(customDeathSound, base.transform.position);
				}
				else if ((bool)customDamageSound && newHealth > 0)
				{
					SoundManager.Instance.playASoundAtPoint(customDamageSound, base.transform.position);
				}
				else if ((bool)myAnimalAi || (bool)myChar)
				{
					if (newHealth <= 0)
					{
						SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.finalImpactSound, base.transform.position);
					}
					else
					{
						SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.impactDamageSound, base.transform.position);
					}
				}
				else if (newHealth <= 0)
				{
					SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.nonOrganicFinalHitSound, base.transform.position);
				}
				else
				{
					SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.nonOrganicHitSound, base.transform.position);
				}
			}
			else if (health < maxHealth)
			{
				SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.statusDamageSound, base.transform.position);
			}
			if (newHealth <= 0 && (bool)spawnWorldObjectOnDeath)
			{
				if (base.isServer)
				{
					PickUpAndCarry component = GetComponent<PickUpAndCarry>();
					if ((bool)component)
					{
						component.RemoveAuthorityBeforeBeforeServerDestroy();
						if (component.beingCarriedBy != 0)
						{
							if (NetworkIdentity.spawned.ContainsKey(component.beingCarriedBy))
							{
								CharPickUp component2 = NetworkIdentity.spawned[component.beingCarriedBy].GetComponent<CharPickUp>();
								if ((bool)component2)
								{
									component2.RpcDropCarriedItem();
								}
							}
							component.NetworkbeingCarriedBy = 0u;
						}
					}
					StartCoroutine(DestroyWithDelay());
					DropGuaranteedDrops();
				}
			}
			else if (base.isServer && newHealth <= 0)
			{
				PickUpAndCarry component3 = GetComponent<PickUpAndCarry>();
				TrappedAnimal component4 = GetComponent<TrappedAnimal>();
				if ((bool)component3)
				{
					component3.RemoveAuthorityBeforeBeforeServerDestroy();
					if (component3.beingCarriedBy != 0)
					{
						if (NetworkIdentity.spawned.ContainsKey(component3.beingCarriedBy))
						{
							CharPickUp component5 = NetworkIdentity.spawned[component3.beingCarriedBy].GetComponent<CharPickUp>();
							if ((bool)component5)
							{
								component5.RpcDropCarriedItem();
							}
						}
						component3.NetworkbeingCarriedBy = 0u;
					}
				}
				if ((bool)component4 && !component4.hasBeenOpenedLocal)
				{
					component4.hasBeenOpenedLocal = true;
					component4.OpenTrap();
				}
				if ((bool)component3 && !component4)
				{
					Transform[] array = dropPositions;
					for (int j = 0; j < array.Length; j++)
					{
						_ = array[j];
						InventoryItem randomDropFromTable = lootDrops.getRandomDropFromTable();
						if ((bool)randomDropFromTable)
						{
							int xPType = -1;
							if ((bool)isAnAnimal())
							{
								xPType = 5;
							}
							if (randomDropFromTable.hasFuel)
							{
								NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(randomDropFromTable), randomDropFromTable.fuelMax, base.transform.position, null, tryNotToStack: true, xPType);
							}
							else
							{
								NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(randomDropFromTable), 1, base.transform.position, null, tryNotToStack: true, xPType);
							}
						}
					}
					if (!myAnimalAi && (bool)guaranteedDrops)
					{
						InventoryItem randomDropFromTable2 = guaranteedDrops.getRandomDropFromTable();
						if ((bool)randomDropFromTable2)
						{
							int xPType2 = -1;
							if ((bool)isAnAnimal())
							{
								xPType2 = 5;
							}
							if (randomDropFromTable2.hasFuel)
							{
								NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(randomDropFromTable2), randomDropFromTable2.fuelMax, base.transform.position, null, tryNotToStack: true, xPType2);
							}
							else
							{
								NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(randomDropFromTable2), 1, base.transform.position, null, tryNotToStack: true, xPType2);
							}
						}
					}
					StartCoroutine(DestroyWithDelay());
				}
			}
		}
		Networkhealth = newHealth;
		if (base.isServer && (bool)isVehicle && !isAnAnimal() && health <= 0)
		{
			if (base.connectionToClient != null)
			{
				base.netIdentity.RemoveClientAuthority();
			}
			SaveLoad.saveOrLoad.vehiclesToSave.Remove(isVehicle);
			StartCoroutine(DestroyWithDelay());
		}
		if (!isAnAnimal() && (bool)GetComponent<NetworkBall>())
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position, 10);
			if (base.isServer)
			{
				InventoryItem randomDropFromTable3 = lootDrops.getRandomDropFromTable();
				NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(randomDropFromTable3), 1, base.transform.position, null, tryNotToStack: true);
				NetworkServer.Destroy(base.gameObject);
			}
		}
		if (!isAnAnimal() && (bool)GetComponent<PaperLantern>())
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position, 10);
			if (base.isServer)
			{
				InventoryItem randomDropFromTable4 = lootDrops.getRandomDropFromTable();
				NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(randomDropFromTable4), 1, base.transform.position, null, tryNotToStack: true);
				GetComponent<PaperLantern>().HitDead();
			}
		}
	}

	private IEnumerator DestroyWithDelay()
	{
		yield return null;
		NetworkServer.Destroy(base.gameObject);
	}

	public bool checkIfShouldShowDamage(int healthDif)
	{
		if (health == maxHealth)
		{
			return false;
		}
		if (healthDif < -1 || (healthDif == -1 && !onFire && !poisoned))
		{
			return true;
		}
		return false;
	}

	public override void OnStopClient()
	{
		if ((base.isServer && health <= 0) || !base.isServer)
		{
			Transform[] array = dropPositions;
			foreach (Transform transform in array)
			{
				ParticleManager.manage.emitDeathParticle(transform.position);
			}
		}
		if ((bool)isVehicle && !isAnAnimal())
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position, 10);
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position - base.transform.forward * 0.5f, 10);
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position - base.transform.forward * 0.5f, 10);
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position + base.transform.right * 0.5f, 10);
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position - base.transform.right * 0.5f, 10);
		}
		PickUpAndCarry component = GetComponent<PickUpAndCarry>();
		if ((bool)component && health <= 0)
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position, 10);
			if (base.isServer && component.investigationItem)
			{
				BulletinBoard.board.checkForInvestigationPostAndComplete(base.transform.position);
			}
		}
		if (base.isServer && (bool)spawnWorldObjectOnDeath && health <= 0)
		{
			NetworkMapSharer.Instance.RpcSpawnCarryWorldObject(GetComponent<PickUpAndCarry>().prefabId, base.transform.position, base.transform.rotation);
		}
		if (TileObjectHealthBar.tile.currentlyHitting == this)
		{
			TileObjectHealthBar.tile.currentlyHitting = null;
		}
	}

	public void onAnimalDeath()
	{
		myAnimalAi.onDeath();
		if ((bool)myAnimalAi.saveAsAlpha)
		{
			MusicManager.manage.PlayCombatMusicStinger();
		}
		if (instantDie)
		{
			StartCoroutine(disapearAfterDeathAnimation(0f));
		}
		else
		{
			StartCoroutine(disapearAfterDeathAnimation(1.5f));
		}
		if (base.isServer && (bool)myAnimalAi && (bool)myAnimalAi.saveAsAlpha)
		{
			NetworkMapSharer.Instance.RpcCheckHuntingTaskCompletion(myAnimalAi.animalId, base.transform.position);
		}
	}

	public IEnumerator disapearAfterDeathAnimation(float waitTimeBefore)
	{
		yield return new WaitForSeconds(waitTimeBefore);
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.animalDiesSound, base.transform.position);
		if (!base.isServer)
		{
			yield break;
		}
		yield return new WaitForSeconds(0.15f);
		int num = 1;
		if (NetworkMapSharer.Instance.wishManager.IsWishActive(WishManager.WishType.DangerousWish))
		{
			num = 2;
		}
		for (int i = 0; i < num; i++)
		{
			Transform[] array = dropPositions;
			foreach (Transform transform in array)
			{
				InventoryItem randomDropFromTable = lootDrops.getRandomDropFromTable();
				if ((bool)randomDropFromTable)
				{
					int xPType = -1;
					if ((bool)isAnAnimal())
					{
						xPType = 5;
					}
					if (!randomDropFromTable.hasFuel)
					{
						NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(randomDropFromTable), 1, transform.position, null, tryNotToStack: true, xPType);
					}
					else
					{
						NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(randomDropFromTable), randomDropFromTable.fuelMax, transform.position, null, tryNotToStack: true, xPType);
					}
				}
			}
			if ((bool)guaranteedDrops)
			{
				DropGuaranteedDrops();
			}
		}
		NetworkNavMesh.nav.UnSpawnAnAnimal(myAnimalAi, saveToMap: false);
	}

	private void DropGuaranteedDrops()
	{
		if (!guaranteedDrops)
		{
			return;
		}
		InventoryItem randomDropFromTable = guaranteedDrops.getRandomDropFromTable();
		if ((bool)randomDropFromTable)
		{
			int xPType = -1;
			if ((bool)isAnAnimal())
			{
				xPType = 5;
			}
			if (!randomDropFromTable.hasFuel)
			{
				NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(randomDropFromTable), 1, base.transform.position, null, tryNotToStack: true, xPType);
			}
			else
			{
				NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(randomDropFromTable), randomDropFromTable.fuelMax, base.transform.position, null, tryNotToStack: true, xPType);
			}
		}
	}

	public override void OnStartServer()
	{
		canBeStunned = true;
		Networkhealth = maxHealth;
		canBeDamaged = true;
		NetworkonFire = false;
		Networkpoisoned = false;
		Networkstunned = false;
		isVehicle = GetComponent<Vehicle>();
	}

	[Command]
	public void CmdChangeHealth(int newHealth)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newHealth);
		SendCommandInternal(typeof(Damageable), "CmdChangeHealth", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeHealthTo(int newHealth)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newHealth);
		SendCommandInternal(typeof(Damageable), "CmdChangeHealthTo", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeMaxHealth(int newMaxHealth)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newMaxHealth);
		SendCommandInternal(typeof(Damageable), "CmdChangeMaxHealth", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdStopStatusEffects()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(Damageable), "CmdStopStatusEffects", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void poison()
	{
		Networkpoisoned = true;
	}

	public void setOnFire()
	{
		NetworkonFire = true;
	}

	public void onFireChange(bool old, bool newOnFire)
	{
		if (fireImmune)
		{
			newOnFire = false;
		}
		NetworkonFire = newOnFire;
		if (onFire)
		{
			StopCoroutine("onFireEffect");
			StartCoroutine("onFireEffect");
		}
	}

	[ClientRpc]
	public void RpcPutOutFireInWater()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(Damageable), "RpcPutOutFireInWater", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator coolOffSmokeParticles()
	{
		float smokeTimer = 0f;
		while (smokeTimer < 1f)
		{
			smokeTimer += Time.deltaTime;
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position + Vector3.up * 0.8f, 1);
			yield return null;
		}
	}

	private IEnumerator onFireEffect()
	{
		float fireTimer = 0f;
		float damageTimer = 0f;
		doDamageFromStatus(1);
		while (onFire)
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.fireStatusParticle, base.transform.position, Random.Range(1, 2));
			ParticleManager.manage.fireStatusGlowParticles.Emit(3);
			if (base.isServer)
			{
				if ((WorldManager.Instance.onTileMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)] == 527 && base.transform.position.y <= (float)WorldManager.Instance.heightMap[Mathf.RoundToInt(base.transform.position.x) / 2, Mathf.RoundToInt(base.transform.position.z) / 2] + 0.15f) || (WorldManager.Instance.waterMap[Mathf.RoundToInt(base.transform.position.x) / 2, Mathf.RoundToInt(base.transform.position.z) / 2] && (double)base.transform.position.y <= 0.15))
				{
					RpcPutOutFireInWater();
					NetworkonFire = false;
				}
				else if (damageTimer > 0.5f)
				{
					damageTimer = 0f;
					doDamageFromStatus(1);
				}
				else
				{
					damageTimer += Time.deltaTime;
				}
				if (WeatherManager.Instance.rainMgr.IsActive && !RealWorldTimeLight.time.underGround)
				{
					if (fireTimer < 1.25f)
					{
						fireTimer += Time.deltaTime;
					}
					else
					{
						NetworkonFire = false;
					}
				}
				else if (fireTimer < fireTimerTotal)
				{
					fireTimer += Time.deltaTime;
				}
				else
				{
					NetworkonFire = false;
				}
			}
			yield return null;
		}
	}

	public void SetFlameResistance(int level)
	{
		fireTimerTotal = 10f;
		if (level == 1)
		{
			fireTimerTotal = 7f;
		}
		if (level == 2)
		{
			fireTimerTotal = 5f;
		}
		if (level == 3)
		{
			fireTimerTotal = 3f;
		}
	}

	public void onPoisonChange(bool old, bool newPoision)
	{
		Networkpoisoned = newPoision;
		if (poisoned)
		{
			StopCoroutine("poisionEffect");
			StartCoroutine("poisionEffect");
			if ((bool)myChar && this == StatusManager.manage.connectedDamge)
			{
				StatusManager.manage.addBuff(StatusManager.BuffType.sickness, 30, 1);
			}
		}
	}

	public void onStunned(bool old, bool newStunned)
	{
		Networkstunned = newStunned;
		if (!myChar)
		{
			GetComponent<Animator>().SetBool("Stunned", newStunned);
		}
		else
		{
			if (myChar.isLocalPlayer)
			{
				myChar.stunned = newStunned;
			}
			if (newStunned && base.isServer)
			{
				StartCoroutine(StunnedRoutineChar());
			}
		}
		if (newStunned)
		{
			ParticleManager.manage.spawnStunnedParticle(this);
		}
		if (!myChar && newStunned && base.isServer)
		{
			StartCoroutine(stunnedRoutine());
		}
	}

	public IEnumerator stunTimer()
	{
		canBeStunned = false;
		while (stunned)
		{
			yield return null;
		}
		yield return new WaitForSeconds(Random.Range(35f, 60f));
		canBeStunned = true;
	}

	public void stun()
	{
		if ((bool)myAnimalAi && health > 0 && !stunned)
		{
			Networkstunned = true;
		}
		if ((bool)myChar && health > 0 && !stunned)
		{
			Networkstunned = true;
		}
	}

	public void stunWithLight(int damageAmount = 0)
	{
		if (!lightStunImmune && (bool)myAnimalAi && health > 0 && !stunned && canBeStunned)
		{
			if (damageAmount > 0)
			{
				attackAndDoDamage(damageAmount, null, 0f);
			}
			RpcPlayStunnedByLight();
			if (health > 0)
			{
				StartCoroutine(stunTimer());
				Networkstunned = true;
			}
		}
	}

	public void unStun()
	{
		Networkstunned = false;
	}

	public bool isStunned()
	{
		return stunned;
	}

	private IEnumerator stunnedRoutine()
	{
		float timer = 0f;
		if ((bool)myAnimalAi)
		{
			myAnimalAi.myAgent.isStopped = true;
		}
		while (stunned && health > 0 && timer < 5f)
		{
			timer += Time.deltaTime;
			yield return null;
		}
		if ((bool)myAnimalAi)
		{
			myAnimalAi.myAgent.isStopped = false;
		}
		Networkstunned = false;
	}

	private IEnumerator StunnedRoutineChar()
	{
		float timer = 0f;
		while (stunned && health > 0 && timer < 8f)
		{
			timer += Time.deltaTime;
			yield return null;
		}
		Networkstunned = false;
	}

	[ClientRpc]
	public void RpcPlayStunnedByLight()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(Damageable), "RpcPlayStunnedByLight", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator poisionEffect()
	{
		float poisionTimer = 0f;
		float damageTimer = 0f;
		if (base.isLocalPlayer)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.GetPoisoned);
		}
		while (poisoned)
		{
			if (Random.Range(0, 2) == 1)
			{
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.poisonStatusParticle, base.transform.position, Random.Range(1, 2));
			}
			if (base.isServer)
			{
				if (poisionTimer < 20f)
				{
					poisionTimer += Time.deltaTime;
				}
				else
				{
					Networkpoisoned = false;
				}
			}
			if (damageTimer > 1f)
			{
				damageTimer = 0f;
				if (base.isLocalPlayer)
				{
					StatusManager.manage.changeStamina(-0.35f);
				}
			}
			else
			{
				damageTimer += Time.deltaTime;
			}
			yield return null;
		}
	}

	public void startRegenAndSetTimer(float newTimer, int level)
	{
		if (regenRoutine != null)
		{
			StopCoroutine(regenRoutine);
		}
		regenRoutine = StartCoroutine(startHealthRegen(newTimer, level));
	}

	private IEnumerator startHealthRegen(float seconds, int level)
	{
		while (seconds > 0f)
		{
			yield return regenWait;
			if (health <= 0)
			{
				regenRoutine = null;
				yield break;
			}
			Networkhealth = Mathf.Clamp(health + level, 1, maxHealth);
			seconds -= 2f;
		}
		regenRoutine = null;
	}

	static Damageable()
	{
		delayWait = new WaitForSeconds(0.45f);
		animalWait = new WaitForSeconds(0.125f);
		regenWait = new WaitForSeconds(2f);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Damageable), "CmdChangeHealth", InvokeUserCode_CmdChangeHealth, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Damageable), "CmdChangeHealthTo", InvokeUserCode_CmdChangeHealthTo, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Damageable), "CmdChangeMaxHealth", InvokeUserCode_CmdChangeMaxHealth, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Damageable), "CmdStopStatusEffects", InvokeUserCode_CmdStopStatusEffects, requiresAuthority: true);
		RemoteCallHelper.RegisterRpcDelegate(typeof(Damageable), "RpcHealFromCasterParticles", InvokeUserCode_RpcHealFromCasterParticles);
		RemoteCallHelper.RegisterRpcDelegate(typeof(Damageable), "RpcPutOutFireInWater", InvokeUserCode_RpcPutOutFireInWater);
		RemoteCallHelper.RegisterRpcDelegate(typeof(Damageable), "RpcPlayStunnedByLight", InvokeUserCode_RpcPlayStunnedByLight);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcHealFromCasterParticles(int amount)
	{
		NotificationManager.manage.CreatePositiveDamageNotification(amount, base.transform);
		StartCoroutine(HealingParticleDelay());
	}

	protected static void InvokeUserCode_RpcHealFromCasterParticles(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcHealFromCasterParticles called on server.");
		}
		else
		{
			((Damageable)obj).UserCode_RpcHealFromCasterParticles(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeHealth(int newHealth)
	{
		changeHealth(newHealth);
	}

	protected static void InvokeUserCode_CmdChangeHealth(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeHealth called on client.");
		}
		else
		{
			((Damageable)obj).UserCode_CmdChangeHealth(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeHealthTo(int newHealth)
	{
		Networkhealth = Mathf.Clamp(newHealth, 0, maxHealth);
	}

	protected static void InvokeUserCode_CmdChangeHealthTo(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeHealthTo called on client.");
		}
		else
		{
			((Damageable)obj).UserCode_CmdChangeHealthTo(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeMaxHealth(int newMaxHealth)
	{
		maxHealth = newMaxHealth;
	}

	protected static void InvokeUserCode_CmdChangeMaxHealth(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeMaxHealth called on client.");
		}
		else
		{
			((Damageable)obj).UserCode_CmdChangeMaxHealth(reader.ReadInt());
		}
	}

	protected void UserCode_CmdStopStatusEffects()
	{
		NetworkonFire = false;
		Networkpoisoned = false;
	}

	protected static void InvokeUserCode_CmdStopStatusEffects(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStopStatusEffects called on client.");
		}
		else
		{
			((Damageable)obj).UserCode_CmdStopStatusEffects();
		}
	}

	protected void UserCode_RpcPutOutFireInWater()
	{
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.extinguishFire, base.transform.position);
		StartCoroutine(coolOffSmokeParticles());
	}

	protected static void InvokeUserCode_RpcPutOutFireInWater(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPutOutFireInWater called on server.");
		}
		else
		{
			((Damageable)obj).UserCode_RpcPutOutFireInWater();
		}
	}

	protected void UserCode_RpcPlayStunnedByLight()
	{
		if (Vector3.Distance(CameraController.control.transform.position, base.transform.position) < 20f && SoundManager.Instance.canPlayStunnedByLightSound())
		{
			SoundManager.Instance.playStunnedByLightSound();
			SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.stunnedByLightSound, base.transform.position);
		}
	}

	protected static void InvokeUserCode_RpcPlayStunnedByLight(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayStunnedByLight called on server.");
		}
		else
		{
			((Damageable)obj).UserCode_RpcPlayStunnedByLight();
		}
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(health);
			writer.WriteBool(onFire);
			writer.WriteBool(poisoned);
			writer.WriteBool(stunned);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(health);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteBool(onFire);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteBool(poisoned);
			result = true;
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteBool(stunned);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = health;
			Networkhealth = reader.ReadInt();
			if (!SyncVarEqual(num, ref health))
			{
				OnTakeDamage(num, health);
			}
			bool flag = onFire;
			NetworkonFire = reader.ReadBool();
			if (!SyncVarEqual(flag, ref onFire))
			{
				onFireChange(flag, onFire);
			}
			bool flag2 = poisoned;
			Networkpoisoned = reader.ReadBool();
			if (!SyncVarEqual(flag2, ref poisoned))
			{
				onPoisonChange(flag2, poisoned);
			}
			bool flag3 = stunned;
			Networkstunned = reader.ReadBool();
			if (!SyncVarEqual(flag3, ref stunned))
			{
				onStunned(flag3, stunned);
			}
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			int num3 = health;
			Networkhealth = reader.ReadInt();
			if (!SyncVarEqual(num3, ref health))
			{
				OnTakeDamage(num3, health);
			}
		}
		if ((num2 & 2L) != 0L)
		{
			bool flag4 = onFire;
			NetworkonFire = reader.ReadBool();
			if (!SyncVarEqual(flag4, ref onFire))
			{
				onFireChange(flag4, onFire);
			}
		}
		if ((num2 & 4L) != 0L)
		{
			bool flag5 = poisoned;
			Networkpoisoned = reader.ReadBool();
			if (!SyncVarEqual(flag5, ref poisoned))
			{
				onPoisonChange(flag5, poisoned);
			}
		}
		if ((num2 & 8L) != 0L)
		{
			bool flag6 = stunned;
			Networkstunned = reader.ReadBool();
			if (!SyncVarEqual(flag6, ref stunned))
			{
				onStunned(flag6, stunned);
			}
		}
	}
}
