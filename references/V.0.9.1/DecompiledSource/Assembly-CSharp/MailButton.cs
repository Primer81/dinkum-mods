using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailButton : MonoBehaviour
{
	public int letterId;

	public TextMeshProUGUI buttonText;

	public Color iconColor;

	public GameObject selectedIcon;

	public Image openedOrUnOpenedIcon;

	public Sprite openedSprite;

	public Sprite unopenedSprite;

	public GameObject presentIcon;

	public Image nameBack;

	public GameObject savedIcon;

	public void showLetter()
	{
		MailManager.manage.showLetter(letterId);
	}

	public void showOpen(bool isOpened, int attachedItem, bool saved)
	{
		openedOrUnOpenedIcon.gameObject.GetComponent<WindowAnimator>().refreshAnimation();
		if (isOpened)
		{
			openedOrUnOpenedIcon.sprite = openedSprite;
		}
		else
		{
			openedOrUnOpenedIcon.sprite = unopenedSprite;
		}
		openedOrUnOpenedIcon.color = iconColor;
		if (iconColor == Color.white)
		{
			nameBack.color = Color.grey;
		}
		else
		{
			nameBack.color = iconColor;
		}
		if (attachedItem != -1)
		{
			presentIcon.SetActive(value: true);
		}
		else
		{
			presentIcon.SetActive(value: false);
		}
		savedIcon.SetActive(saved);
	}
}
