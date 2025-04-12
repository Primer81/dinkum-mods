using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LicenceButton : MonoBehaviour
{
	public Image licenceIcon;

	public TextMeshProUGUI licenceName;

	public TextMeshProUGUI licenceLevel;

	public TextMeshProUGUI licenceDesc;

	public TextMeshProUGUI licencePrice;

	public Image[] levelIcons;

	public Sprite nonLevelDot;

	public Sprite levelDot;

	public Image titleColour;

	public Image borderImage;

	public int myLicenceId;

	public void fillButton(int newLicenceId)
	{
		myLicenceId = newLicenceId;
		licenceIcon.sprite = LicenceManager.manage.licenceIcons[(int)LicenceManager.manage.allLicences[myLicenceId].type];
		licenceName.text = LicenceManager.manage.getLicenceName(LicenceManager.manage.allLicences[myLicenceId].type);
		titleColour.color = LicenceManager.manage.licenceColours[(int)(LicenceManager.manage.allLicences[myLicenceId].type - 1)];
		borderImage.color = LicenceManager.manage.licenceColours[(int)(LicenceManager.manage.allLicences[myLicenceId].type - 1)];
		if (LicenceManager.manage.allLicences[myLicenceId].getCurrentLevel() == LicenceManager.manage.allLicences[myLicenceId].getCurrentMaxLevel())
		{
			if (LicenceManager.manage.allLicences[myLicenceId].isConnectedWithSkillLevel() && LicenceManager.manage.allLicences[myLicenceId].getCurrentLevel() != LicenceManager.manage.allLicences[myLicenceId].getMaxLevel())
			{
				licenceDesc.text = string.Format(LicenceManager.manage.GetLicenceStatusDesc("LevelUpSkillToUnlock"), LicenceManager.manage.allLicences[myLicenceId].getConnectedSkillName());
			}
			else
			{
				licenceDesc.text = LicenceManager.manage.GetLicenceStatusDesc("MaxLevel");
			}
			licencePrice.text = "";
		}
		else
		{
			licenceDesc.text = LicenceManager.manage.getLicenceLevelDescription(LicenceManager.manage.allLicences[myLicenceId].type, LicenceManager.manage.allLicences[myLicenceId].getCurrentLevel() + 1);
			licencePrice.text = "<sprite=15> " + LicenceManager.manage.allLicences[(int)LicenceManager.manage.allLicences[myLicenceId].type].getNextLevelPrice().ToString("n0");
		}
		_ = LicenceManager.manage.allLicences[myLicenceId].type;
		_ = 1;
		if (LicenceManager.manage.allLicences[myLicenceId].getCurrentLevel() == 0)
		{
			licenceLevel.text = LicenceManager.manage.GetLicenceStatusDesc("NotHeld");
		}
		else
		{
			licenceLevel.text = string.Format(LicenceManager.manage.GetLicenceStatusDesc("LicenceLevel"), LicenceManager.manage.allLicences[myLicenceId].getCurrentLevel());
		}
		for (int i = 0; i < levelIcons.Length; i++)
		{
			if (i < LicenceManager.manage.allLicences[myLicenceId].getCurrentMaxLevel())
			{
				levelIcons[i].gameObject.SetActive(value: true);
				if (i < LicenceManager.manage.allLicences[myLicenceId].getCurrentLevel())
				{
					levelIcons[i].sprite = levelDot;
				}
				else
				{
					levelIcons[i].sprite = nonLevelDot;
				}
			}
			else
			{
				levelIcons[i].gameObject.SetActive(value: false);
			}
		}
		base.gameObject.SetActive(LicenceManager.manage.allLicences[myLicenceId].isUnlocked);
	}

	public void pressApplyButton()
	{
		LicenceManager.manage.openConfirmWindow(LicenceManager.manage.allLicences[myLicenceId].type);
	}

	public void updateButton()
	{
		fillButton(myLicenceId);
	}

	public void updateJournalButton()
	{
		fillDetailsForJournal(myLicenceId);
	}

	public void fillDetailsForJournal(int licenceId)
	{
		myLicenceId = licenceId;
		if (LicenceManager.manage.allLicences[myLicenceId].type == LicenceManager.LicenceTypes.None)
		{
			return;
		}
		licenceIcon.sprite = LicenceManager.manage.licenceIcons[(int)LicenceManager.manage.allLicences[myLicenceId].type];
		licenceName.text = LicenceManager.manage.getLicenceName(LicenceManager.manage.allLicences[myLicenceId].type);
		titleColour.color = LicenceManager.manage.licenceColours[(int)(LicenceManager.manage.allLicences[myLicenceId].type - 1)];
		borderImage.color = LicenceManager.manage.licenceColours[(int)(LicenceManager.manage.allLicences[myLicenceId].type - 1)];
		if (LicenceManager.manage.allLicences[myLicenceId].getCurrentLevel() == LicenceManager.manage.allLicences[myLicenceId].getCurrentMaxLevel())
		{
			licenceDesc.text = LicenceManager.manage.GetLicenceStatusDesc("MaxLevel");
			licencePrice.text = string.Format(LicenceManager.manage.GetLicenceStatusDesc("LicenceLevel"), LicenceManager.manage.allLicences[myLicenceId].getCurrentLevel().ToString());
		}
		else
		{
			licenceDesc.text = LicenceManager.manage.getLicenceLevelDescription(LicenceManager.manage.allLicences[myLicenceId].type, LicenceManager.manage.allLicences[myLicenceId].getCurrentLevel());
			licencePrice.text = string.Format(LicenceManager.manage.GetLicenceStatusDesc("LicenceLevel"), LicenceManager.manage.allLicences[myLicenceId].getCurrentLevel().ToString());
		}
		for (int i = 0; i < levelIcons.Length; i++)
		{
			if (i < LicenceManager.manage.allLicences[myLicenceId].getCurrentMaxLevel())
			{
				levelIcons[i].gameObject.SetActive(value: true);
				if (i < LicenceManager.manage.allLicences[myLicenceId].getCurrentLevel())
				{
					levelIcons[i].sprite = levelDot;
				}
				else
				{
					levelIcons[i].sprite = nonLevelDot;
				}
			}
			else
			{
				levelIcons[i].gameObject.SetActive(value: false);
			}
		}
		base.gameObject.SetActive(LicenceManager.manage.allLicences[myLicenceId].hasALevelOneOrHigher());
	}
}
