using System.Collections;
using UnityEngine;

public class EnableBookAnimation : MonoBehaviour
{
	public Animator myAnim;

	public GameObject bookTabs;

	public Transform bookBack;

	public Transform bookCover;

	public Transform bookPages;

	private WaitForSeconds tabWait = new WaitForSeconds(1f);

	private void OnEnable()
	{
		if (TownManager.manage.journalUnlocked)
		{
			if (OptionsMenu.options.animateJournalOpen)
			{
				myAnim.enabled = true;
			}
			StartCoroutine(tabCheck());
		}
	}

	public void disableAnimation()
	{
		myAnim.enabled = false;
	}

	private IEnumerator tabCheck()
	{
		if (OptionsMenu.options.animateJournalOpen)
		{
			yield return tabWait;
		}
		bookTabs.SetActive(value: true);
		bookBack.localScale = Vector3.one;
		if (!OptionsMenu.options.animateJournalOpen)
		{
			bookCover.localScale = Vector3.one;
			bookCover.gameObject.SetActive(value: false);
		}
		bookPages.localScale = Vector3.one;
	}

	private void OnDisable()
	{
	}
}
