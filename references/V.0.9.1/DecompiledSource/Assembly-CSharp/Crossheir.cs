using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Crossheir : MonoBehaviour
{
	public static Crossheir cross;

	public Image crossheirIcon;

	private RectTransform crossheirRect;

	public Image ammoInvIcon;

	public Color crossheirColour;

	public TextMeshProUGUI ammoCounter;

	public Transform ammoCounterBox;

	private void Awake()
	{
		cross = this;
		crossheirRect = crossheirIcon.GetComponent<RectTransform>();
		base.gameObject.SetActive(value: false);
	}

	public void turnOnCrossheir()
	{
		base.gameObject.SetActive(value: true);
	}

	public void setAmmo(Sprite ammoIcon, int newAmmo)
	{
		ammoInvIcon.sprite = ammoIcon;
		ammoCounter.text = newAmmo.ToString("n0");
	}

	public void setPower(float currentPower, float maxPower)
	{
		ammoCounterBox.SetParent(null);
		crossheirColour.a = currentPower / maxPower / 1.5f - 0.1f;
		crossheirIcon.color = crossheirColour;
		ammoInvIcon.color = crossheirColour * 1.5f;
		ammoCounter.color = crossheirColour * 1.5f;
		float num = 1f - currentPower / maxPower / 1.75f;
		crossheirIcon.transform.localScale = new Vector3(num, num, 1f);
		ammoCounterBox.SetParent(crossheirIcon.transform);
		ammoCounterBox.localPosition = new Vector3(75f, 75f, 0f);
	}

	public void fadeOut()
	{
		StartCoroutine(fadeOutCrossheir());
	}

	public void turnOffCrossheir()
	{
		ammoCounterBox.SetParent(null);
		crossheirIcon.transform.localScale = Vector3.one;
		ammoCounterBox.SetParent(crossheirIcon.transform);
		ammoCounterBox.localPosition = new Vector3(75f, 75f, 0f);
		base.gameObject.SetActive(value: false);
	}

	private IEnumerator fadeOutCrossheir()
	{
		float timer = 0f;
		while (timer < 1f)
		{
			crossheirColour.a = Mathf.Lerp(crossheirColour.a, 0f, timer);
			crossheirIcon.color = crossheirColour;
			ammoInvIcon.color = crossheirColour * 1.5f;
			ammoCounter.color = crossheirColour * 1.5f;
			timer += Time.deltaTime * 2f;
			yield return null;
		}
	}
}
