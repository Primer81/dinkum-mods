using System.Collections;
using UnityEngine;

public class ControlTutorial : MonoBehaviour
{
	public static ControlTutorial tutorial;

	public GameObject controlTutorial;

	public GameObject cameraTutorial;

	public GameObject freeCamTutorial;

	public GameObject bagTutorial;

	public GameObject journalTutorial;

	public bool tutorialStart;

	private void Awake()
	{
		tutorial = this;
	}

	public void startTutorial()
	{
		StartCoroutine(tutorialRoutine());
	}

	private void Update()
	{
		if (tutorialStart)
		{
			startTutorial();
			tutorialStart = false;
		}
	}

	private IEnumerator tutorialRoutine()
	{
		bool hasBagBeenOpened = false;
		while (ConversationManager.manage.IsConversationActive)
		{
			yield return true;
		}
		yield return null;
		while (GiftedItemWindow.gifted.windowOpen)
		{
			yield return null;
		}
		controlTutorial.gameObject.SetActive(value: true);
		bool exitControls = false;
		yield return new WaitForSeconds(1f);
		while (!exitControls)
		{
			float x = InputMaster.input.getLeftStick().x;
			float y = InputMaster.input.getLeftStick().y;
			if (x != 0f || y != 0f)
			{
				exitControls = true;
			}
			yield return null;
			if (!hasBagBeenOpened && Inventory.Instance.invOpen)
			{
				hasBagBeenOpened = true;
			}
		}
		controlTutorial.gameObject.SetActive(value: false);
		while (WeatherManager.Instance.IsMyPlayerInside)
		{
			yield return null;
		}
		while (!ConversationManager.manage.IsConversationActive)
		{
			yield return true;
		}
		while (ConversationManager.manage.IsConversationActive || GiftedItemWindow.gifted.windowOpen)
		{
			yield return true;
		}
		yield return new WaitForSeconds(1f);
		if (!hasBagBeenOpened)
		{
			bagTutorial.gameObject.SetActive(value: true);
			bool exitBagTutorial = false;
			while (!exitBagTutorial)
			{
				if (InputMaster.input.OpenInventory())
				{
					exitBagTutorial = true;
				}
				yield return null;
				if (needsToHide())
				{
					yield return StartCoroutine(hideTutorialBoxWhileInMenu(bagTutorial));
				}
			}
			bagTutorial.gameObject.SetActive(value: false);
		}
		while (!TownManager.manage.journalUnlocked)
		{
			yield return null;
		}
		yield return new WaitForSeconds(3f);
		journalTutorial.gameObject.SetActive(value: true);
		bool exitJournalTutorial = false;
		while (!exitJournalTutorial)
		{
			if (InputMaster.input.Journal())
			{
				exitJournalTutorial = true;
			}
			yield return null;
			if (needsToHide(includeSubMenu: false))
			{
				yield return StartCoroutine(hideTutorialBoxWhileInMenu(journalTutorial));
			}
		}
		journalTutorial.SetActive(value: false);
	}

	public bool needsToHide(bool includeSubMenu = true)
	{
		if (ConversationManager.manage.IsConversationActive)
		{
			return true;
		}
		if (includeSubMenu && MenuButtonsTop.menu.subMenuOpen)
		{
			return true;
		}
		if (GiftedItemWindow.gifted.windowOpen)
		{
			return true;
		}
		return false;
	}

	private IEnumerator hideTutorialBoxWhileInMenu(GameObject boxToHide)
	{
		if (ConversationManager.manage.IsConversationActive)
		{
			boxToHide.SetActive(value: false);
			while (ConversationManager.manage.IsConversationActive)
			{
				yield return true;
			}
			boxToHide.SetActive(value: true);
		}
		if (MenuButtonsTop.menu.subMenuOpen)
		{
			boxToHide.SetActive(value: false);
			while (MenuButtonsTop.menu.subMenuOpen)
			{
				yield return true;
			}
			boxToHide.SetActive(value: true);
		}
		if (GiftedItemWindow.gifted.windowOpen)
		{
			boxToHide.SetActive(value: false);
			while (GiftedItemWindow.gifted.windowOpen)
			{
				yield return null;
			}
			boxToHide.SetActive(value: true);
		}
	}
}
