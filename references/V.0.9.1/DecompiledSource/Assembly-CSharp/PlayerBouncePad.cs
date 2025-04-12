using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBouncePad : MonoBehaviour
{
	public List<CharMovement> charsOnPad;

	public ParticleSystem myPart;

	private ParticleSystem.ShapeModule myPartShape;

	public CapsuleCollider jumpCollider;

	public float startSize = 0.5f;

	public ASound soundEffect;

	public float remainingTime = 4f;

	private bool padActive;

	public void OnEnable()
	{
		myPartShape = myPart.shape;
		myPartShape.radius = startSize;
		StartCoroutine(GrowAndStay());
	}

	private IEnumerator GrowAndStay()
	{
		yield return null;
		float timer = 1f;
		float currentSize = startSize;
		padActive = true;
		jumpCollider.enabled = true;
		while (timer > 0f)
		{
			myPart.Emit(Mathf.RoundToInt(100f * currentSize));
			timer -= Time.deltaTime * 6.5f;
			currentSize += Time.deltaTime * 6.5f;
			yield return null;
			myPartShape.radius = currentSize;
			jumpCollider.radius = currentSize * 2f;
		}
		float delayTimer = remainingTime;
		while (delayTimer > 0f)
		{
			yield return null;
			delayTimer -= Time.deltaTime;
			myPart.Emit(Mathf.RoundToInt(100f * currentSize));
		}
		padActive = false;
		jumpCollider.enabled = false;
		RemoveAllBuffs();
		yield return new WaitForSeconds(3f);
		Object.Destroy(base.gameObject);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (padActive)
		{
			CharMovement componentInParent = other.GetComponentInParent<CharMovement>();
			if ((bool)componentInParent && !charsOnPad.Contains(componentInParent))
			{
				charsOnPad.Add(componentInParent);
				GiveBuff(componentInParent);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (padActive)
		{
			CharMovement componentInParent = other.GetComponentInParent<CharMovement>();
			if ((bool)componentInParent && charsOnPad.Contains(componentInParent))
			{
				charsOnPad.Remove(componentInParent);
				RemoveBuff(componentInParent);
			}
		}
	}

	private void OnDestroy()
	{
		RemoveAllBuffs();
	}

	public void GiveBuff(CharMovement giveBuffTo)
	{
		giveBuffTo.jumpUpHeight += 55f;
	}

	public void RemoveBuff(CharMovement giveBuffTo)
	{
		giveBuffTo.jumpUpHeight -= 55f;
	}

	public void RemoveAllBuffs()
	{
		for (int i = 0; i < charsOnPad.Count; i++)
		{
			RemoveBuff(charsOnPad[i]);
		}
		charsOnPad.Clear();
	}
}
