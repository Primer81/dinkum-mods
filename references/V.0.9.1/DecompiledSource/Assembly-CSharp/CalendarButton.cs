using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CalendarButton : MonoBehaviour
{
	public int showingDate;

	public TextMeshProUGUI date;

	public Image todayTag;

	public Image todayTag2;

	public Image eventTag;

	public Image bugCatchingTag;

	public Image fishCatchingTag;

	public Image specialEventTag;

	public Image npcImage;

	public Image dayComplete;

	public HoverToolTipOnButton buttonHoverText;

	public Color specialEventHoverColour;

	public Color normalHoverColour;

	private void Start()
	{
		date.text = showingDate.ToString();
	}

	public void showDateDetails()
	{
		turnOffHoverText();
		int num = WorldManager.Instance.day + (WorldManager.Instance.week - 1) * 7;
		if (showingDate == num)
		{
			todayTag.enabled = true;
			todayTag2.enabled = true;
		}
		else
		{
			todayTag.enabled = false;
			todayTag2.enabled = false;
		}
		npcImage.enabled = false;
		eventTag.enabled = false;
		bugCatchingTag.enabled = false;
		fishCatchingTag.enabled = false;
		specialEventTag.enabled = false;
		for (int i = 0; i < NPCManager.manage.NPCDetails.Length; i++)
		{
			if (NPCManager.manage.npcStatus[i].hasMet && NPCManager.manage.NPCDetails[i].birthSeason == WorldManager.Instance.month && NPCManager.manage.NPCDetails[i].birthday == showingDate)
			{
				npcImage.sprite = NPCManager.manage.NPCDetails[i].npcSprite;
				npcImage.enabled = true;
				setUpHover(string.Format(ConversationManager.manage.GetLocByTag("NPC Birthday"), NPCManager.manage.NPCDetails[i].GetNPCName()), normalHoverColour);
				break;
			}
		}
		TownEvent townEvent = TownEventManager.manage.checkEventForToday(showingDate);
		if (townEvent != null)
		{
			eventTag.enabled = true;
			specialEventTag.sprite = townEvent.eventSprite;
			specialEventTag.enabled = true;
			setUpHover(townEvent.getEventName(), specialEventHoverColour);
		}
		else if (CatchingCompetitionManager.manage.isBugCompDay(showingDate))
		{
			eventTag.enabled = true;
			bugCatchingTag.enabled = true;
			setUpHover(ConversationManager.manage.GetLocByTag("Bug Catching Comp"), specialEventHoverColour);
		}
		else if (CatchingCompetitionManager.manage.isFishCompDay(showingDate))
		{
			eventTag.enabled = true;
			fishCatchingTag.enabled = true;
			setUpHover(ConversationManager.manage.GetLocByTag("Fishing Comp"), specialEventHoverColour);
		}
		else if (TownEventManager.manage.IsJohnsAnniversary(showingDate))
		{
			eventTag.enabled = true;
			setUpHover(ConversationManager.manage.GetLocByTag("Johns Anniversary"), specialEventHoverColour);
		}
		dayComplete.enabled = showingDate < num;
	}

	public void setUpHover(string text, Color hoverColour)
	{
		buttonHoverText.enabled = true;
		buttonHoverText.hoveringText = text;
		buttonHoverText.hoverColour = hoverColour;
	}

	public void turnOffHoverText()
	{
		if (buttonHoverText == null)
		{
			buttonHoverText = GetComponent<HoverToolTipOnButton>();
		}
		buttonHoverText.enabled = false;
	}
}
