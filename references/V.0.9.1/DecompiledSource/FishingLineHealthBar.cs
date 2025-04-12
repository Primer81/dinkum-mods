using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FishingLineHealthBar : MonoBehaviour
{
	public static FishingLineHealthBar bar;

	public RectTransform myTrans;

	public GameObject toHide;

	public Image healthBar;

	public Color fullHealthColour;

	public Color lowHealthColour;

	public AudioSource mySound;

	private float lastShowingHealth = 1f;

	private float hideTimer;

	public GameObject goText;

	public GameObject stopText;

	private Vector2 shakePos = Vector2.zero;

	private void Awake()
	{
		bar = this;
	}

	public void showHealthbar()
	{
		StartCoroutine(barWorks());
	}

	public IEnumerator barWorks()
	{
		while (NetworkMapSharer.Instance.localChar.myPickUp.netRod.lineIsCasted && NetworkMapSharer.Instance.localChar.myPickUp.netRod.fishOnLine == -1)
		{
			yield return null;
		}
		if (NetworkMapSharer.Instance.localChar.myPickUp.netRod.lineIsCasted && NetworkMapSharer.Instance.localChar.myPickUp.netRod.fishOnLine != -1)
		{
			StartCoroutine(goAndStop());
			lastShowingHealth = 1f;
			while (NetworkMapSharer.Instance.localChar.myPickUp.netRod.lineIsCasted)
			{
				yield return null;
				healthBar.fillAmount = NetworkMapSharer.Instance.localChar.myPickUp.netRod.currentLineHealth / (float)NetworkMapSharer.Instance.localChar.myPickUp.netRod.fullLineHealth;
				if (healthBar.fillAmount == lastShowingHealth)
				{
					if (hideTimer <= 0f)
					{
						toHide.SetActive(value: false);
					}
					else
					{
						hideTimer -= Time.deltaTime;
					}
				}
				else
				{
					toHide.SetActive(value: true);
					lastShowingHealth = healthBar.fillAmount;
					hideTimer = 2f;
				}
				healthBar.color = Color.Lerp(fullHealthColour, lowHealthColour, 1f - healthBar.fillAmount);
				shakePos = Vector2.Lerp(shakePos, Vector2.zero, Time.deltaTime * 2f);
				myTrans.anchoredPosition = new Vector2(-35f, 0f) + shakePos;
			}
		}
		stopSound();
		toHide.SetActive(value: false);
	}

	public IEnumerator goAndStop()
	{
		if (!NetworkMapSharer.Instance.localChar.myPickUp.netRod.lineIsCasted || NetworkMapSharer.Instance.localChar.myPickUp.netRod.fishOnLine == -1)
		{
			yield break;
		}
		while (NetworkMapSharer.Instance.localChar.myPickUp.netRod.lineIsCasted)
		{
			if (toHide.activeSelf)
			{
				if (NetworkMapSharer.Instance.localChar.myPickUp.netRod.pulling && NetworkMapSharer.Instance.localChar.localUsing)
				{
					stopText.SetActive(value: true);
				}
				else
				{
					stopText.SetActive(value: false);
				}
				if (!NetworkMapSharer.Instance.localChar.myPickUp.netRod.pulling && !NetworkMapSharer.Instance.localChar.localUsing)
				{
					goText.SetActive(value: true);
				}
				else
				{
					goText.SetActive(value: false);
				}
			}
			yield return null;
		}
	}

	public void shakeBar()
	{
		shakePos = new Vector3(Random.Range(1f, -1f), Random.Range(1f, -1f));
	}

	public void playSound()
	{
		if (!mySound.isPlaying)
		{
			mySound.pitch = 8f;
			mySound.volume = 0.25f * SoundManager.Instance.getUiVolume();
			mySound.Play();
		}
	}

	public void stopSound()
	{
		mySound.Pause();
	}
}
