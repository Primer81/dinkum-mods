using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDetailManager : MonoBehaviour
{
	public static PlayerDetailManager manage;

	public bool windowOpen;

	public GameObject tabsPage;

	public GameObject licenceWindow;

	public GameObject levelWindow;

	public SkillBox[] levelBoxes;

	public RawImage playerImage;

	public TextMeshProUGUI nameTextBox;

	public TextMeshProUGUI islandNameTextBox;

	public TextMeshProUGUI moneyTextBox;

	public TextMeshProUGUI bankAccountTextBox;

	public TextMeshProUGUI residentAgeText;

	private void Awake()
	{
		manage = this;
	}

	public void openTab()
	{
		windowOpen = true;
		switchToLevelWindow();
		tabsPage.SetActive(value: true);
	}

	public void closeTab()
	{
		windowOpen = false;
		tabsPage.SetActive(value: false);
	}

	public void switchToLevelWindow()
	{
		levelWindow.SetActive(value: true);
		licenceWindow.SetActive(value: false);
		nameTextBox.text = Inventory.Instance.playerName;
		islandNameTextBox.text = Inventory.Instance.islandName;
		moneyTextBox.text = "<sprite=11>" + Inventory.Instance.wallet.ToString("n0");
		if (BankMenu.menu.accountBalance > 0)
		{
			bankAccountTextBox.text = "<sprite=11>" + BankMenu.menu.accountBalance.ToString("n0");
		}
		else
		{
			bankAccountTextBox.text = "";
		}
		residentAgeText.text = ConversationGenerator.generate.GetJournalNameByTag("ResidentFor") + " ";
		residentAgeText.text += string.Format(ConversationGenerator.generate.GetTimeText("Age_Days"), WorldManager.Instance.day + (WorldManager.Instance.week - 1) * 7);
		if (WorldManager.Instance.month > 1)
		{
			TextMeshProUGUI textMeshProUGUI = residentAgeText;
			textMeshProUGUI.text = textMeshProUGUI.text + ", " + string.Format(ConversationGenerator.generate.GetTimeText("Age_Months"), WorldManager.Instance.month);
		}
		if (WorldManager.Instance.year > 1)
		{
			TextMeshProUGUI textMeshProUGUI2 = residentAgeText;
			textMeshProUGUI2.text = textMeshProUGUI2.text + ", " + string.Format(ConversationGenerator.generate.GetTimeText("Age_Years"), WorldManager.Instance.year);
		}
		playerImage.texture = CharacterCreatorScript.create.loadSlotPhoto();
		updateLevelDetails();
	}

	public void switchToLicenceWindow()
	{
		levelWindow.SetActive(value: false);
		licenceWindow.SetActive(value: true);
		LicenceManager.manage.refreshCharacterJournalTab();
	}

	private void updateLevelDetails()
	{
		for (int i = 0; i < levelBoxes.Length; i++)
		{
			levelBoxes[i].setToCurrent(i, CharLevelManager.manage.currentXp[i]);
		}
	}
}
