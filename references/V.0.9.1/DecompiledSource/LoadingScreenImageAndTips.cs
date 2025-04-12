using System.Collections;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenImageAndTips : MonoBehaviour
{
	public Image picture;

	public Sprite[] randomImages;

	public string[] imageCredit;

	public Color fadeOutColour;

	public GameObject tipBox;

	public TextMeshProUGUI tipWords;

	public string[] tips;

	public TextMeshProUGUI creditText;

	public GameObject creditWindow;

	private int lastTip = -1;

	private void OnEnable()
	{
		picture.color = fadeOutColour;
		tipBox.SetActive(value: false);
		picture.rectTransform.anchoredPosition = Vector2.zero;
		int num = Random.Range(0, randomImages.Length);
		picture.sprite = randomImages[num];
		if (LocalizationManager.CurrentLanguage == "English" && imageCredit[num] != "")
		{
			creditText.text = imageCredit[num] ?? "";
		}
		else
		{
			creditText.text = "";
		}
		creditWindow.SetActive(value: false);
		StartCoroutine(fadeInImage());
	}

	private IEnumerator fadeInImage()
	{
		yield return new WaitForSeconds(0.8f);
		float fadeTime = 0f;
		while (fadeTime < 1f)
		{
			fadeTime += Time.deltaTime * 2f;
			picture.color = Color.Lerp(fadeOutColour, Color.white, fadeTime);
			yield return null;
		}
		picture.color = Color.white;
		int num;
		for (num = Random.Range(0, tips.Length); num == lastTip; num = Random.Range(0, tips.Length))
		{
		}
		lastTip = num;
		string text = (LocalizedString)("LoadingScreenTips/LoadingTip_" + num);
		if (text != null && text != "")
		{
			tipWords.text = text;
		}
		else
		{
			tipWords.text = tips[Random.Range(0, tips.Length)];
		}
		if (creditText.text != "")
		{
			creditWindow.SetActive(value: true);
		}
		tipBox.SetActive(value: true);
	}

	public void fadeAway()
	{
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(fadeAwayFast());
		}
	}

	private IEnumerator fadeAwayFast()
	{
		tipBox.SetActive(value: false);
		creditWindow.SetActive(value: false);
		float fadeTime = 0f;
		while (fadeTime < 1f)
		{
			fadeTime += Time.deltaTime * 10f;
			picture.color = Color.Lerp(Color.white, fadeOutColour, fadeTime);
			yield return null;
		}
		picture.color = fadeOutColour;
	}
}
