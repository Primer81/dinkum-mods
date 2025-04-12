using System.Collections;
using UnityEngine;

public class EatItem : MonoBehaviour
{
	public ASound chewsound;

	public ParticleSystem chewParticle;

	private Animator parentAnim;

	private CharMovement myCharMovement;

	private NPCHoldsItems npcHold;

	private CharacterHeadIK headIk;

	private Animator foodAnim;

	private CharPickUp myPickup;

	private bool usingChanger;

	public void Start()
	{
		npcHold = GetComponentInParent<NPCHoldsItems>();
		myCharMovement = GetComponentInParent<CharMovement>();
		myPickup = GetComponentInParent<CharPickUp>();
		if ((bool)myCharMovement)
		{
			parentAnim = myCharMovement.myAnim;
			headIk = myCharMovement.GetComponent<CharacterHeadIK>();
			foodAnim = GetComponent<Animator>();
			StartCoroutine(slowWalkOnUse());
		}
		if ((bool)npcHold)
		{
			parentAnim = npcHold.GetComponent<Animator>();
		}
		if (!parentAnim && (bool)foodAnim)
		{
			foodAnim.Rebind();
			Object.Destroy(GetComponent<Animator>());
		}
	}

	private IEnumerator slowWalkOnUse()
	{
		while (true)
		{
			if (myCharMovement.isLocalPlayer)
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
			if ((bool)headIk)
			{
				if (myCharMovement.myEquip.usingItem)
				{
					headIk.setIsEating(newEating: true);
				}
				else
				{
					headIk.setIsEating(newEating: false);
				}
			}
			yield return null;
			if (!myCharMovement.isLocalPlayer)
			{
				continue;
			}
			if (Physics.Raycast(base.transform.position + base.transform.forward * 1.5f + Vector3.up * 3f, Vector3.down, out var hitInfo, 3f, myPickup.pickUpLayerMask))
			{
				if ((bool)hitInfo.transform.GetComponentInParent<ItemDepositAndChanger>())
				{
					foodAnim.SetBool("TooFull", value: true);
					usingChanger = true;
				}
				else
				{
					foodAnim.SetBool("TooFull", StatusManager.manage.isTooFull());
					usingChanger = false;
				}
			}
			else
			{
				foodAnim.SetBool("TooFull", StatusManager.manage.isTooFull());
				usingChanger = false;
			}
		}
	}

	public void OnDestroy()
	{
		if ((bool)myCharMovement)
		{
			myCharMovement.isSneaking(isSneaking: false);
		}
		if ((bool)headIk)
		{
			headIk.setIsEating(newEating: false);
		}
	}

	public void giveToFullWarning()
	{
		if (!usingChanger)
		{
			NotificationManager.manage.pocketsFull.showTooFull();
		}
	}

	public void EatObject()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer && !StatusManager.manage.isTooFull())
		{
			eatObjectBenefits();
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.EatSomething);
			Inventory.Instance.consumeItemInHand();
		}
	}

	private void npcEatRemoveDelay()
	{
		base.gameObject.SetActive(value: false);
	}

	public void PlayEatSound()
	{
		SoundManager.Instance.playASoundAtPoint(chewsound, base.transform.position);
		if ((bool)chewParticle)
		{
			chewParticle.Emit(8);
		}
	}

	public void playEatAnimation()
	{
		if ((bool)parentAnim)
		{
			parentAnim.SetTrigger("Eating");
		}
	}

	private void eatObjectBenefits()
	{
		StatusManager.manage.EatFoodAndAddStatus(Inventory.Instance.invSlots[Inventory.Instance.selectedSlot].itemInSlot);
		if (Inventory.Instance.invSlots[Inventory.Instance.selectedSlot].itemInSlot.consumeable.specialSnag)
		{
			StatusManager.manage.snagsEaten++;
			_ = StatusManager.manage.snagsEaten;
			_ = 2;
		}
	}
}
