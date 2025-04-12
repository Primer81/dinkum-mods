using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffIcon : MonoBehaviour
{
	public Image icon;

	public Image levelIcon;

	public TextMeshProUGUI secondsRemaining;

	public void SetUpBuffIcon(Sprite iconSprite, string buffName, string buffDesc)
	{
		icon.sprite = iconSprite;
		GetComponent<HoverToolTipOnButton>().hoveringText = buffName;
		GetComponent<HoverToolTipOnButton>().hoveringDesc = buffDesc;
	}

	public void SetBuffTimeText(int newSeconds)
	{
		if (Mathf.RoundToInt((float)newSeconds / 60f) > 1)
		{
			secondsRemaining.text = Mathf.RoundToInt((float)newSeconds / 60f) + ConversationGenerator.generate.GetDescriptionDetails("FoodTimerMins");
		}
		else if (newSeconds >= 60)
		{
			secondsRemaining.text = "1" + ConversationGenerator.generate.GetDescriptionDetails("FoodTimerMins");
		}
		else
		{
			secondsRemaining.text = newSeconds + ConversationGenerator.generate.GetDescriptionDetails("FoodTimerSeconds");
		}
	}

	public void SetBuffLevel(int buffLevel)
	{
		levelIcon.enabled = buffLevel > 1;
		if (buffLevel == 2)
		{
			levelIcon.sprite = StatusManager.manage.buffLevel2Sprite;
		}
		else
		{
			levelIcon.sprite = StatusManager.manage.buffLevel3Sprite;
		}
	}
}
