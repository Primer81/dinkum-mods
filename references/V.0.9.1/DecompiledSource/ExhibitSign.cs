using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class ExhibitSign : MonoBehaviour
{
	public Transform listExhibitInside;

	public ConversationObject convo;

	private static string[] emptyConvo;

	private void Start()
	{
		emptyConvo = new string[2]
		{
			ConversationManager.manage.GetLocByTag("AskAboutEmptyExhibit_0"),
			ConversationManager.manage.GetLocByTag("AskAboutEmptyExhibit_1")
		};
		updateMySign();
	}

	private void OnEnable()
	{
		LocalizationManager.OnLocalizeEvent += updateMySign;
		MuseumManager.manage.onExhibitUpdate.AddListener(updateMySign);
	}

	private void OnDisable()
	{
		LocalizationManager.OnLocalizeEvent += updateMySign;
		MuseumManager.manage.onExhibitUpdate.RemoveListener(updateMySign);
	}

	private void updateMySign()
	{
		emptyConvo = new string[2]
		{
			ConversationManager.manage.GetLocByTag("AskAboutEmptyExhibit_0"),
			ConversationManager.manage.GetLocByTag("AskAboutEmptyExhibit_1")
		};
		List<string> list = new List<string>();
		list.Add(ConversationManager.manage.GetLocByTag("AskAboutExhibit_0"));
		int num = 0;
		string text = "";
		bool flag = false;
		for (int i = 0; i < listExhibitInside.childCount; i++)
		{
			BugExhibit component = listExhibitInside.GetChild(i).GetComponent<BugExhibit>();
			if ((bool)component)
			{
				text = ((num != 0) ? (text + "\n" + UIAnimationManager.manage.GetItemColorTag(Inventory.Instance.allItems[component.showingBugId].getInvItemName())) : UIAnimationManager.manage.GetItemColorTag(Inventory.Instance.allItems[component.showingBugId].getInvItemName()));
				flag = true;
				num++;
			}
			else
			{
				FishTankFish component2 = listExhibitInside.GetChild(i).GetComponent<FishTankFish>();
				if ((bool)component2)
				{
					text = ((num != 0) ? (text + "\n" + UIAnimationManager.manage.GetItemColorTag(Inventory.Instance.allItems[component2.showingFishId].getInvItemName())) : UIAnimationManager.manage.GetItemColorTag(Inventory.Instance.allItems[component2.showingFishId].getInvItemName()));
					flag = true;
					num++;
				}
			}
			if (num == 4)
			{
				list.Add(text);
				text = "";
				num = 0;
			}
		}
		if (text != "")
		{
			list.Add(text);
		}
		if (!flag)
		{
			convo.targetOpenings.sequence = emptyConvo;
		}
		else
		{
			convo.targetOpenings.sequence = list.ToArray();
		}
	}
}
