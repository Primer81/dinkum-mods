using System.Collections;
using TMPro;
using UnityEngine;

public class PocketsFullNotification : MonoBehaviour
{
	private float onTimer;

	private Coroutine running;

	public Animator myAnim;

	public TextMeshProUGUI promptText;

	public void showNoLicence(LicenceManager.LicenceTypes type)
	{
		promptText.text = ConversationGenerator.generate.GetToolTip("Tip_NeedLicence");
		turnOn(isHolding: false);
	}

	public void showMustBeEmpty()
	{
		promptText.text = ConversationGenerator.generate.GetToolTip("Tip_MustBeEmpty");
		turnOn(isHolding: false);
	}

	public void removeItemsOnTop()
	{
		promptText.text = ConversationGenerator.generate.GetToolTip("Tip_ItemOnTop");
		turnOn(isHolding: false);
	}

	public void showPondFull()
	{
		promptText.text = ConversationGenerator.generate.GetToolTip("Tip_PondFull");
		turnOn(isHolding: false);
	}

	public void ShowRequirePermission()
	{
		promptText.text = ConversationGenerator.generate.GetToolTip("Tip_Permission_Required");
		turnOn(isHolding: false);
	}

	public void showTooFull()
	{
		if (StatusManager.manage.getBuffLevel(StatusManager.BuffType.sickness) >= 1)
		{
			promptText.text = ConversationGenerator.generate.GetToolTip("Tip_TooSick");
		}
		else
		{
			promptText.text = ConversationGenerator.generate.GetToolTip("Tip_TooFull");
		}
		turnOn(isHolding: false);
	}

	public void showPocketsFull(bool isHolding)
	{
		promptText.text = ConversationGenerator.generate.GetToolTip("Tip_PocketsFull");
		turnOn(isHolding);
	}

	public void showCanPlaceText(string showText)
	{
		promptText.text = showText;
		turnOn(isHolding: true);
	}

	public void hidePopUp()
	{
		onTimer = 0f;
	}

	private void turnOn(bool isHolding)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			isHolding = false;
		}
		if (!isHolding && base.gameObject.activeInHierarchy)
		{
			myAnim.SetTrigger("Bounce");
		}
		base.gameObject.SetActive(value: true);
		onTimer = 2f;
		if (!isHolding)
		{
			SoundManager.Instance.play2DSound(SoundManager.Instance.pocketsFull);
		}
		if (running == null)
		{
			running = StartCoroutine(runPocketsFull());
		}
	}

	private IEnumerator runPocketsFull()
	{
		while (onTimer > 0f)
		{
			yield return null;
			onTimer -= Time.deltaTime;
			if (!Inventory.Instance.CanMoveCharacter())
			{
				break;
			}
		}
		running = null;
		base.gameObject.SetActive(value: false);
	}
}
