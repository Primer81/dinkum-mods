using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
	public static LoadingScreen load;

	public FadeBlackness blackness;

	public TextMeshProUGUI loadingText;

	public Image loadingBar;

	public Animator loadingAnim;

	public LoadingScreenImageAndTips loadingScreenImages;

	public TopNotification saveGameConfirmed;

	public void appear(string screenText, bool loadingTipsOn = false)
	{
		loadingAnim.gameObject.SetActive(value: true);
		loadingAnim.SetBool("Completed", value: false);
		base.gameObject.SetActive(value: true);
		loadingBar.fillAmount = 0f;
		loadingText.text = ConversationGenerator.generate.GetToolTip(screenText);
		loadingText.ForceMeshUpdate();
		blackness.fadeIn();
		loadingScreenImages.gameObject.SetActive(loadingTipsOn);
	}

	public void disappear()
	{
		loadingAnim.gameObject.SetActive(value: false);
		loadingScreenImages.fadeAway();
		blackness.fadeOut();
		Invoke("hideAfterUse", 1f);
	}

	private void hideAfterUse()
	{
		base.gameObject.SetActive(value: false);
	}

	public void showPercentage(float currentPercent)
	{
		loadingBar.fillAmount = currentPercent;
	}

	public void loadingBarOnlyAppear()
	{
		loadingAnim.SetBool("Completed", value: false);
		loadingAnim.gameObject.SetActive(value: true);
		loadingBar.fillAmount = 0f;
	}

	public void completed()
	{
		loadingAnim.SetBool("Completed", value: true);
	}
}
