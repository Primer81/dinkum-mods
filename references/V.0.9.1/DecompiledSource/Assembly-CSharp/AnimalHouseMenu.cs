using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnimalHouseMenu : MonoBehaviour
{
	public TextMeshProUGUI shelterTitleText;

	public TMP_InputField animalNameField;

	public Image animalIcon;

	public HeartContainer[] hearts;

	public TextMeshProUGUI sellAmount;

	public TextMeshProUGUI sellButtonText;

	private FarmAnimalDetails showingAnimal;

	public GameObject sellButton;

	public GameObject moveButton;

	public GameObject confirmPopUp;

	public Image confirmImage;

	public TextMeshProUGUI confirmText;

	public TextMeshProUGUI animalAge;

	public GameObject openWindow;

	public TextMeshProUGUI pettedText;

	public TextMeshProUGUI shelterText;

	public TextMeshProUGUI eatenText;

	private void OnEnable()
	{
		openWindow.SetActive(value: false);
		confirmPopUp.SetActive(value: false);
	}

	public void openConfirm()
	{
		confirmPopUp.SetActive(value: true);
		confirmText.text = string.Format(ConversationGenerator.generate.GetJournalNameByTag("SellAnimalNameForDinkAmount"), UIAnimationManager.manage.GetCharacterNameTag(showingAnimal.animalName), "<sprite=11>" + getSellValue().ToString("n0"));
	}

	public void sellAnimalInHouse()
	{
		int sellValue = getSellValue();
		Inventory.Instance.changeWallet(sellValue);
		checkFarmAnimalSendLetter();
		showingAnimal.sell();
		FarmAnimalMenu.menu.refreshJournalButtons();
		confirmPopUp.SetActive(value: false);
		openWindow.SetActive(value: false);
	}

	public int getSellValue()
	{
		int num = (int)((float)AnimalManager.manage.allAnimals[showingAnimal.animalId].GetComponent<FarmAnimal>().baseValue / 2f);
		int num2 = 3;
		if (showingAnimal.animalRelationShip > 50)
		{
			num2 = 6;
		}
		else if (showingAnimal.animalRelationShip > 85)
		{
			num2 = 8;
		}
		return num + (int)((float)num * ((float)num2 * ((float)showingAnimal.animalRelationShip / 100f)));
	}

	public void checkFarmAnimalSendLetter()
	{
		if ((bool)AnimalManager.manage.allAnimals[showingAnimal.animalId].GetComponent<FarmAnimal>().recieveOnSell)
		{
			MonoBehaviour.print(showingAnimal.animalId + " Selling and leaving a " + AnimalManager.manage.allAnimals[showingAnimal.animalId].GetComponent<FarmAnimal>().recieveOnSell.getInvItemName());
			MailManager.manage.sendAnimalSoldLetter(AnimalManager.manage.allAnimals[showingAnimal.animalId].GetComponent<FarmAnimal>().recieveOnSell.getItemId(), 1);
		}
	}

	public void renameAnimalFromBook()
	{
		if (showingAnimal != null)
		{
			showingAnimal.animalName = animalNameField.text;
			if (FarmAnimalManager.manage.activeAnimalAgents[showingAnimal.agentListId] != null)
			{
				FarmAnimalManager.manage.activeAnimalAgents[showingAnimal.agentListId].renameAnimal(showingAnimal);
			}
		}
	}

	public void fillData(FarmAnimalDetails animalToShow)
	{
		confirmPopUp.SetActive(value: false);
		openWindow.SetActive(value: false);
		openWindow.SetActive(value: true);
		showingAnimal = animalToShow;
		animalNameField.text = showingAnimal.animalName;
		animalIcon.enabled = true;
		if (animalToShow.hasEaten)
		{
			eatenText.text = ConversationGenerator.generate.GetJournalNameByTag("Eaten") + "\n<sprite=17>";
		}
		else
		{
			eatenText.text = ConversationGenerator.generate.GetJournalNameByTag("Eaten") + "\n<sprite=16>";
		}
		if (animalToShow.hasHouse())
		{
			shelterText.text = ConversationGenerator.generate.GetJournalNameByTag("Shelter") + "\n<sprite=17>";
		}
		else
		{
			shelterText.text = ConversationGenerator.generate.GetJournalNameByTag("Shelter") + "\n<sprite=16>";
		}
		if (animalToShow.hasBeenPatted)
		{
			pettedText.text = ConversationGenerator.generate.GetJournalNameByTag("Petted") + "\n<sprite=17>";
		}
		else
		{
			pettedText.text = ConversationGenerator.generate.GetJournalNameByTag("Petted") + "\n<sprite=16>";
		}
		HeartContainer[] array = hearts;
		foreach (HeartContainer obj in array)
		{
			obj.updateHealth(showingAnimal.animalRelationShip);
			obj.gameObject.SetActive(value: true);
		}
		if (showingAnimal.animalVariation == -1)
		{
			animalIcon.sprite = AnimalManager.manage.allAnimals[showingAnimal.animalId].GetComponent<FarmAnimal>().defualtIcon;
		}
		else
		{
			animalIcon.sprite = AnimalManager.manage.allAnimals[showingAnimal.animalId].GetComponent<AnimalVariation>().variationIcons[showingAnimal.animalVariation];
		}
		if (showingAnimal.animalAge >= 28)
		{
			if (showingAnimal.animalAge / 28 / 4 >= 1)
			{
				if (showingAnimal.animalAge / 28 / 4 > 1)
				{
					animalAge.text = string.Format(ConversationGenerator.generate.GetTimeNameByTag("Age_Years"), showingAnimal.animalAge / 28 / 4);
				}
				else
				{
					animalAge.text = string.Format(ConversationGenerator.generate.GetTimeNameByTag("Age_Year"), showingAnimal.animalAge / 28 / 4);
				}
			}
			else if (showingAnimal.animalAge / 28 > 1)
			{
				animalAge.text = string.Format(ConversationGenerator.generate.GetTimeNameByTag("Age_Months"), showingAnimal.animalAge / 28);
			}
			else
			{
				animalAge.text = string.Format(ConversationGenerator.generate.GetTimeNameByTag("Age_Month"), showingAnimal.animalAge / 28);
			}
		}
		else if (showingAnimal.animalAge > 1)
		{
			animalAge.text = string.Format(ConversationGenerator.generate.GetTimeNameByTag("Age_Days"), showingAnimal.animalAge);
		}
		else
		{
			animalAge.text = string.Format(ConversationGenerator.generate.GetTimeNameByTag("Age_Day"), showingAnimal.animalAge);
		}
		sellAmount.text = "<sprite=11>" + getSellValue().ToString("n0");
		sellButtonText.text = string.Format(ConversationGenerator.generate.GetJournalNameByTag("SellAnimalName"), showingAnimal.animalName);
		sellButton.SetActive(value: true);
		if (!NetworkMapSharer.Instance.isServer)
		{
			sellButton.SetActive(value: false);
		}
	}
}
