using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TownAward : MonoBehaviour
{
	public TextMeshProUGUI awardName;

	public Image backgroundImage;

	public Image awardIcon;

	public TextMeshProUGUI percent;

	public Sprite doesNotHaveAwardBackground;

	public Sprite hasAwardBackground;

	public GameObject sparkles;

	public GameObject sparkles2;

	public GameObject sparkleParticle;

	public Animator animator;

	private Color iconColor = Color.white;

	private void Start()
	{
	}

	public void fillAwardPercent(float percentage)
	{
		if (percentage < 100f)
		{
			backgroundImage.sprite = doesNotHaveAwardBackground;
			iconColor.a = 0.25f;
			awardIcon.color = iconColor;
			animator.enabled = false;
			sparkles.gameObject.SetActive(value: false);
			sparkles2.gameObject.SetActive(value: false);
			sparkleParticle.gameObject.SetActive(value: false);
		}
		else
		{
			backgroundImage.sprite = hasAwardBackground;
			awardIcon.color = Color.white;
			animator.enabled = true;
			sparkles.gameObject.SetActive(value: true);
			sparkles2.gameObject.SetActive(value: true);
			sparkleParticle.gameObject.SetActive(value: true);
		}
		percent.text = percentage.ToString("F1") + "%";
	}
}
