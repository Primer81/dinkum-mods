using System.Collections;
using UnityEngine;

public class MeleeAttacks : MonoBehaviour
{
	private CharMovement myCharMovement;

	private NPCHoldsItems myNPC;

	private CharInteract myInteract;

	public Transform toolPos;

	public Transform[] otherToolPos;

	public ASound swordSwing;

	public ItemHitBox myHitBox;

	public Animator myAnim;

	[Header("Swing Particles---------")]
	public ParticleSystem swingParts;

	public int showSwingPartsForFrames;

	public bool playSwingSoundWithPart;

	private float multiplier = 1f;

	[Header("Tool options---------------")]
	public bool isWeapon = true;

	public bool useEnergyOnSwing;

	public bool consumeFuelOnSwing;

	public bool hitBoxMovesToHoldPos;

	[Header("Attack Info ---------------")]
	public int attackFramesLength = 3;

	public float forwardSpeed;

	public float forwardTime;

	[Header("Stand Still ---------------")]
	public int framesBeforeAttackLocked;

	public int framesAfterAttackLocked;

	public bool lockedDuringAttack;

	private bool attackPlaying;

	public bool faceTargetsWhenAttacking;

	[Header("Other---------------")]
	public bool slowWalkWhileUsing;

	public bool cantClimbWhileUsing;

	public GameObject spawnAOEObjectOnAttack;

	public bool fireProjectileAoe;

	public int projectileId = 1;

	public SpecialWeaponEffect playEffect;

	private bool lockingPos;

	private bool hasCaughtBug;

	private void Start()
	{
		myInteract = GetComponentInParent<CharInteract>();
		myCharMovement = GetComponentInParent<CharMovement>();
		if (slowWalkWhileUsing && (bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			StartCoroutine(slowWalkOnUse());
		}
		if (cantClimbWhileUsing && (bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			StartCoroutine(slowWalkNoClimeOnUse(myCharMovement.myEquip.currentlyHoldingItemId));
		}
		if (hitBoxMovesToHoldPos && (bool)myCharMovement)
		{
			myHitBox.transform.parent = myCharMovement.myEquip.rightHandToolHitPos;
			myHitBox.transform.localPosition = Vector3.zero;
			myHitBox.transform.localRotation = Quaternion.identity;
		}
	}

	public void attack()
	{
		if (((bool)myCharMovement && myCharMovement.isLocalPlayer) || ((bool)myNPC && myNPC.isServer))
		{
			if (!attackPlaying)
			{
				attackPlaying = true;
				StartCoroutine(genericAttack());
			}
		}
		else
		{
			StartCoroutine(nonLocalSwingSound());
		}
		if ((bool)playEffect)
		{
			playEffect.playSpecialEffect();
		}
	}

	private IEnumerator nonLocalSwingSound()
	{
		if (framesBeforeAttackLocked != 0 || lockedDuringAttack)
		{
			if (framesBeforeAttackLocked != 0 && lockedDuringAttack)
			{
				yield return new WaitForSeconds((float)framesBeforeAttackLocked / 60f);
			}
			else if (framesBeforeAttackLocked != 0)
			{
				yield return new WaitForSeconds((float)framesBeforeAttackLocked / 60f);
			}
		}
		playSwordSwingSound();
	}

	private IEnumerator genericAttack()
	{
		if (faceTargetsWhenAttacking && (bool)myCharMovement)
		{
			myCharMovement.faceClosestTarget();
		}
		if ((bool)myCharMovement && (framesBeforeAttackLocked != 0 || lockedDuringAttack))
		{
			if (framesBeforeAttackLocked != 0 && lockedDuringAttack)
			{
				StartCoroutine(myCharMovement.charLockedStill((float)(framesBeforeAttackLocked + attackFramesLength + framesAfterAttackLocked) / 60f));
				yield return new WaitForSeconds((float)framesBeforeAttackLocked / 60f);
			}
			else if (framesBeforeAttackLocked != 0)
			{
				yield return StartCoroutine(myCharMovement.charLockedStill((float)framesBeforeAttackLocked / 60f));
			}
			else if (lockedDuringAttack)
			{
				StartCoroutine(myCharMovement.charLockedStill((float)(attackFramesLength + framesAfterAttackLocked) / 60f));
			}
		}
		if (!playSwingSoundWithPart)
		{
			playSwordSwingSound();
		}
		myHitBox.startAttack();
		if ((bool)spawnAOEObjectOnAttack || fireProjectileAoe)
		{
			myCharMovement.CmdFireAOE();
		}
		if ((bool)myNPC)
		{
			StartCoroutine(endAttackDelay());
			yield break;
		}
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			if (consumeFuelOnSwing)
			{
				Inventory.Instance.useItemWithFuel();
			}
			if (useEnergyOnSwing)
			{
				StatusManager.manage.changeStamina(0f - Inventory.Instance.invSlots[Inventory.Instance.selectedSlot].itemInSlot.getStaminaCost());
			}
		}
		StartCoroutine(endAttackDelay());
		if (forwardSpeed > 0f)
		{
			StartCoroutine(myCharMovement.charAttacksForward(forwardSpeed, forwardTime));
		}
	}

	private IEnumerator endAttackDelay()
	{
		yield return new WaitForSeconds((float)(attackFramesLength + framesBeforeAttackLocked) / 60f);
		myHitBox.endAttack();
		if ((bool)myCharMovement && framesAfterAttackLocked != 0 && !lockedDuringAttack)
		{
			yield return StartCoroutine(myCharMovement.charLockedStill((float)framesAfterAttackLocked / 60f));
		}
		attackPlaying = false;
	}

	private IEnumerator slowWalkOnUse()
	{
		while (true)
		{
			if (base.gameObject.activeInHierarchy)
			{
				if (myCharMovement.localUsing)
				{
					myCharMovement.isSneaking(isSneaking: true);
				}
				else
				{
					myCharMovement.isSneaking(isSneaking: false);
				}
			}
			yield return null;
		}
	}

	private IEnumerator slowWalkNoClimeOnUse(int holdingId)
	{
		while (myCharMovement.myEquip.currentlyHoldingItemId == holdingId)
		{
			if (base.gameObject.activeInHierarchy)
			{
				if (myCharMovement.localUsing)
				{
					myCharMovement.isSneaking(isSneaking: true);
					myCharMovement.canClimb = false;
				}
				else
				{
					myCharMovement.isSneaking(isSneaking: false);
					myCharMovement.canClimb = true;
				}
			}
			yield return null;
		}
	}

	public void lockPosForFrames(int frames)
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer && !lockingPos)
		{
			StartCoroutine(lockPosOnly(frames));
		}
	}

	private IEnumerator lockPosOnly(int frames)
	{
		lockingPos = true;
		yield return StartCoroutine(myCharMovement.charLockedStill((float)frames / 60f));
		lockingPos = false;
	}

	public void turnOnLookLockForFrames(int framesToLock)
	{
		if ((bool)myCharMovement && myCharMovement.myEquip.usingItem)
		{
			StartCoroutine(lookLockFrames(framesToLock));
		}
	}

	public void turnOnLookLockForFramesWithoutUsing(int framesToLock)
	{
		if ((bool)myCharMovement)
		{
			StartCoroutine(lookLockFrames(framesToLock));
		}
	}

	private IEnumerator lookLockFrames(int frames)
	{
		myCharMovement.myEquip.setLookLock(isLocked: true);
		yield return new WaitForSeconds((float)frames / 60f);
		myCharMovement.myEquip.setLookLock(isLocked: false);
	}

	public void lockPlayerInPlace()
	{
		StartCoroutine(myCharMovement.charAttacksForward());
	}

	private void OnDestroy()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myCharMovement.unlockAll();
			endCharSneak();
			myCharMovement.canClimb = true;
		}
		if ((bool)myCharMovement)
		{
			myCharMovement.myEquip.setLookLock(isLocked: false);
		}
		if (cantClimbWhileUsing && (bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myCharMovement.isSneaking(isSneaking: false);
			myCharMovement.canClimb = true;
		}
		if ((hitBoxMovesToHoldPos && (bool)myCharMovement) || (hitBoxMovesToHoldPos && (bool)myNPC))
		{
			Object.Destroy(myHitBox.gameObject);
		}
	}

	public void charAttackForward(float newMultiplier = 1f)
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			multiplier = newMultiplier;
			myHitBox.startAttack();
			StartCoroutine(myCharMovement.charAttacksForward());
		}
		if ((bool)swordSwing)
		{
			SoundManager.Instance.playASoundAtPoint(swordSwing, base.transform.position);
		}
	}

	public void faceTarget()
	{
		if ((bool)myCharMovement)
		{
			myCharMovement.faceClosestTarget();
		}
	}

	public void lockPositionAllowRotation()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myCharMovement.attackLockOn(isOn: true);
			myCharMovement.moveLockRotateSlowOn(isOn: true);
		}
	}

	public void lockPositionAndRotation()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myCharMovement.attackLockOn(isOn: true);
		}
	}

	public void fireProjectile(int projectileNo = 0)
	{
	}

	public void rotationOnlyAttack_Start()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myCharMovement.lockRotation(isLocked: true);
			myCharMovement.isSneaking(isSneaking: true);
		}
	}

	public void rotationOnlyAttack_End()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myCharMovement.lockRotation(isLocked: false);
			myCharMovement.isSneaking(isSneaking: false);
		}
	}

	public void startAttackNoLock()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			if (useEnergyOnSwing)
			{
				StatusManager.manage.changeStamina(0f - Inventory.Instance.invSlots[Inventory.Instance.selectedSlot].itemInSlot.getStaminaCost());
			}
			if (consumeFuelOnSwing)
			{
				Inventory.Instance.useItemWithFuel();
			}
			if ((bool)myHitBox)
			{
				myHitBox.startAttack();
			}
		}
		if ((bool)myNPC && myNPC.isServer && (bool)myHitBox)
		{
			myHitBox.startAttack();
		}
		if ((bool)swordSwing)
		{
			SoundManager.Instance.playASoundAtPoint(swordSwing, base.transform.position);
		}
	}

	public void startAttackSlowWalk()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			if (useEnergyOnSwing)
			{
				StatusManager.manage.changeStamina(0f - Inventory.Instance.invSlots[Inventory.Instance.selectedSlot].itemInSlot.getStaminaCost());
			}
			if (consumeFuelOnSwing)
			{
				Inventory.Instance.useItemWithFuel();
			}
			if ((bool)myHitBox)
			{
				myHitBox.startAttack();
			}
			myCharMovement.isSneaking(isSneaking: true);
		}
		if ((bool)myNPC && myNPC.isServer && (bool)myHitBox)
		{
			myHitBox.startAttack();
		}
		if ((bool)swordSwing)
		{
			SoundManager.Instance.playASoundAtPoint(swordSwing, base.transform.position);
		}
	}

	public void endAttackSlowWalk()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			if ((bool)myHitBox)
			{
				myHitBox.endAttack();
			}
			myCharMovement.isSneaking(isSneaking: false);
		}
		if ((bool)myNPC && myNPC.isServer && (bool)myHitBox)
		{
			myHitBox.endAttack();
		}
	}

	public void endAttackNoLock()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer && (bool)myHitBox)
		{
			myHitBox.endAttack();
		}
		if ((bool)myNPC && myNPC.isServer && (bool)myHitBox)
		{
			myHitBox.endAttack();
		}
	}

	public void slowWalkNoClimb()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myCharMovement.canClimb = false;
			myCharMovement.isSneaking(isSneaking: true);
		}
	}

	public void stopSlowWalkNoClimb()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myCharMovement.canClimb = true;
			myCharMovement.isSneaking(isSneaking: false);
		}
	}

	public void startCharSneak()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myCharMovement.isSneaking(isSneaking: true);
		}
	}

	public void endCharSneak()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myCharMovement.isSneaking(isSneaking: false);
		}
	}

	public void charStartAttackStill()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			if (useEnergyOnSwing)
			{
				StatusManager.manage.changeStamina(0f - Inventory.Instance.invSlots[Inventory.Instance.selectedSlot].itemInSlot.getStaminaCost());
			}
			if (consumeFuelOnSwing)
			{
				Inventory.Instance.useItemWithFuel();
			}
			myCharMovement.attackLockOn(isOn: true);
			myCharMovement.moveLockRotateSlowOn(isOn: false);
			if ((bool)myHitBox)
			{
				myHitBox.startAttack();
			}
		}
		if ((bool)myNPC && myNPC.isServer && (bool)myHitBox)
		{
			myHitBox.startAttack();
		}
		if ((bool)swordSwing)
		{
			SoundManager.Instance.playASoundAtPoint(swordSwing, base.transform.position);
		}
	}

	public void endAttackStill()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			if ((bool)myHitBox)
			{
				myHitBox.endAttack();
			}
			myCharMovement.attackLockOn(isOn: false);
			myCharMovement.moveLockRotateSlowOn(isOn: false);
		}
		if ((bool)myNPC && myNPC.isServer && (bool)myHitBox)
		{
			myHitBox.endAttack();
		}
	}

	public void fireAOE()
	{
		if (!fireProjectileAoe)
		{
			Vector3 position = myHitBox.transform.position;
			if (WorldManager.Instance.isPositionOnMap((int)(myHitBox.transform.position.x / 2f), (int)(myHitBox.transform.position.z / 2f)))
			{
				position.y = WorldManager.Instance.heightMap[(int)myHitBox.transform.position.x / 2, (int)myHitBox.transform.position.z / 2];
			}
			if (Mathf.Abs(position.y - myCharMovement.transform.position.y) <= 0.5f)
			{
				GameObject gameObject = Object.Instantiate(spawnAOEObjectOnAttack, position, Quaternion.identity);
				if ((bool)gameObject.GetComponent<GroundAttack>())
				{
					gameObject.GetComponent<GroundAttack>().attachedToPlayer = myCharMovement;
				}
				EruptionAttack component = gameObject.GetComponent<EruptionAttack>();
				if ((bool)component)
				{
					component.fireFromPlayer(myCharMovement);
				}
			}
		}
		else
		{
			NetworkMapSharer.Instance.fireProjectile(projectileId, myCharMovement.transform, myHitBox.transform.position, myHitBox.transform.forward);
			NetworkMapSharer.Instance.fireProjectile(projectileId, myCharMovement.transform, myHitBox.transform.position + myHitBox.transform.right / 2f, myHitBox.transform.forward + myHitBox.transform.right / 4f);
			NetworkMapSharer.Instance.fireProjectile(projectileId, myCharMovement.transform, myHitBox.transform.position - myHitBox.transform.right / 2f, myHitBox.transform.forward - myHitBox.transform.right / 4f);
		}
	}

	private IEnumerator pauseAttack()
	{
		if ((bool)myCharMovement && myCharMovement.myEquip.currentlyHoldingItemId > -1)
		{
			myAnim.SetFloat("speed", 0f);
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			myAnim.SetFloat("speed", 1f);
		}
		else
		{
			myAnim.SetFloat("speed", 0f);
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			myAnim.SetFloat("speed", 1f);
		}
	}

	public void npcRelease()
	{
		if ((bool)myNPC)
		{
			myNPC.NetworkusingItem = false;
		}
	}

	public void toolDoesDamageToolPos()
	{
		if ((bool)toolPos && (bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myInteract.doDamageToolPos(toolPos.position);
		}
	}

	public void toolDoesDamageToolPosNo(int noToUse)
	{
		if (otherToolPos.Length > noToUse && otherToolPos[noToUse] != null && (bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myInteract.doDamageToolPos(otherToolPos[noToUse].position);
		}
	}

	public void finishAttack()
	{
		if ((bool)myCharMovement)
		{
			_ = myCharMovement.isLocalPlayer;
		}
	}

	public void attackAndDealDamage(Damageable damageable)
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer && damageable.health > 0)
		{
			if (myHitBox.attackDamageAmount > 0)
			{
				CameraController.control.shakeScreenMax(0.35f, 0.2f);
			}
			TileObjectHealthBar.tile.setCurrentlyHitting(damageable);
			InputMaster.input.doRumble(0.25f);
			if (!damageable.IsAVehicle() || (damageable.IsAVehicle() && myCharMovement.myEquip.myPermissions.CheckIfCanInteractWithVehicles()))
			{
				myCharMovement.CmdDealDamage(damageable.netId, 1f + (float)StatusManager.manage.getBuffLevel(StatusManager.BuffType.huntingBuff) / 6f);
			}
			else
			{
				NotificationManager.manage.pocketsFull.ShowRequirePermission();
			}
			Inventory.Instance.useItemWithFuel();
		}
		if ((bool)myNPC)
		{
			damageable.attackAndDoDamage(Mathf.RoundToInt(myNPC.currentlyHolding.weaponDamage), base.transform.root, myNPC.currentlyHolding.weaponKnockback);
		}
	}

	public void catchBug(BugTypes catchThisBug)
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer && !hasCaughtBug)
		{
			int invItemId = Inventory.Instance.getInvItemId(catchThisBug.bugInvItem());
			SoundManager.Instance.play2DSound(SoundManager.Instance.pickUpItem);
			ParticleManager.manage.emitAttackParticle(myHitBox.transform.position, 5);
			hasCaughtBug = true;
			myCharMovement.CmdCatchBug(catchThisBug.netId);
			Inventory.Instance.useItemWithFuel();
			if (catchThisBug.IsSparkling())
			{
				RandomObjectGenerator.generate.GiveRandomBugReward();
			}
			myCharMovement.myEquip.catchAndShowBug(invItemId);
			if (!NetworkMapSharer.Instance.isServer)
			{
				catchThisBug.gameObject.SetActive(value: false);
			}
		}
		else if ((bool)myNPC && myNPC.isServer && !hasCaughtBug)
		{
			hasCaughtBug = true;
			myNPC.catchBug(catchThisBug.netId);
		}
	}

	public void CatchBugInTerrarium(int removeThisBugFromTerrarium, int terrariumX, int terrariumY)
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer && !hasCaughtBug)
		{
			SoundManager.Instance.play2DSound(SoundManager.Instance.pickUpItem);
			ParticleManager.manage.emitAttackParticle(myHitBox.transform.position, 5);
			hasCaughtBug = true;
			myCharMovement.CmdRemoveBugFromTerrarium(removeThisBugFromTerrarium, terrariumX, terrariumY);
			Inventory.Instance.useItemWithFuel();
			myCharMovement.myEquip.catchAndShowBug(removeThisBugFromTerrarium);
		}
	}

	public void playSwingParticles()
	{
		swingParts.Emit(8);
	}

	public void playSwingPartsForFrames()
	{
		if (playSwingSoundWithPart)
		{
			playSwordSwingSound();
		}
		if ((bool)swingParts)
		{
			StartCoroutine(swingPartsForFrames(showSwingPartsForFrames));
		}
	}

	private IEnumerator swingPartsForFrames(int frames)
	{
		while (frames > 0)
		{
			swingParts.Emit(10);
			yield return null;
			frames--;
		}
	}

	public void lookLockOn()
	{
		if ((bool)myCharMovement)
		{
			myCharMovement.myEquip.setLookLock(isLocked: true);
		}
	}

	public void lookLockOff()
	{
		if ((bool)myCharMovement)
		{
			myCharMovement.myEquip.setLookLock(isLocked: false);
		}
	}

	public void playGroundParticles()
	{
		ParticleManager.manage.emitGroundAttackParticles(toolPos.position);
	}

	public void playSwordSwingSound()
	{
		if ((bool)swordSwing)
		{
			SoundManager.Instance.playASoundAtPoint(swordSwing, base.transform.position);
		}
	}

	public void attachNPC(NPCHoldsItems NPC)
	{
		myNPC = NPC;
		if (hitBoxMovesToHoldPos)
		{
			myHitBox.transform.parent = myNPC.rightHandMoveHitboxTo;
			myHitBox.transform.localPosition = Vector3.zero;
			myHitBox.transform.localRotation = Quaternion.identity;
		}
	}

	public void UseStaminaOnAttackForLocalPlayer()
	{
		if (consumeFuelOnSwing && (bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			Inventory.Instance.useItemWithFuel();
		}
	}
}
