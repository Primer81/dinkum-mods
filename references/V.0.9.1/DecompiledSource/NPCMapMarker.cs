using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NPCMapMarker : MonoBehaviour
{
	public Image icon;

	private float fullTimer = 25f;

	private float flashTimer = 1f;

	private void OnEnable()
	{
		StartCoroutine(FlashWhenTimerLow());
	}

	private IEnumerator FlashWhenTimerLow()
	{
		icon.enabled = true;
		while (fullTimer > 5f)
		{
			fullTimer -= Time.deltaTime;
			yield return null;
		}
		icon.enabled = true;
		while (fullTimer > 0f)
		{
			yield return null;
			fullTimer -= Time.deltaTime;
			if (flashTimer > 0f)
			{
				flashTimer -= Time.deltaTime * (5f - fullTimer + 1f);
				continue;
			}
			flashTimer = 1f;
			icon.enabled = !icon.enabled;
		}
		icon.enabled = false;
	}
}
