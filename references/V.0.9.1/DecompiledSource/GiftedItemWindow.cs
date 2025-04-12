using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GiftedItemWindow : MonoBehaviour
{
	public static GiftedItemWindow gifted;

	public GameObject window;

	private List<int> itemsToBeGiven = new List<int>();

	private List<int> amountToBeGiven = new List<int>();

	private List<int> recipeLearnt = new List<int>();

	private List<int> recipeOverflow = new List<int>();

	private List<int> licenceToBeGiven = new List<int>();

	public TextMeshProUGUI recipeOrReceivedText;

	public TextMeshProUGUI titleText;

	public TextMeshProUGUI descText;

	public TextMeshProUGUI amountText;

	public Image itemIcon;

	public ASound windowPopUpSound;

	public ASound recipePopUpSound;

	public ASound licnecePopUpSound;

	public ASound giftWindowPopupSound;

	public GameObject nextArrow;

	public GameObject blueprintIcon;

	public bool windowOpen;

	public Color textColor;

	public Color recipeTextColor;

	public InventoryItem mapItem;

	public InventoryItem journalItem;

	public GameObject giftBox;

	public GameObject licenceStamp;

	public GameObject overflowWindow;

	private void Awake()
	{
		gifted = this;
	}

	public void addLicenceToBeGiven(int licenceId)
	{
		licenceToBeGiven.Add(licenceId);
	}

	public void addToListToBeGiven(int itemId, int stackAmount)
	{
		itemsToBeGiven.Add(itemId);
		amountToBeGiven.Add(stackAmount);
	}

	public void addRecipeToUnlock(int itemId)
	{
		if (Inventory.Instance.allItems[itemId].craftable.showInRecipeOverflow)
		{
			recipeOverflow.Add(itemId);
		}
		else
		{
			recipeLearnt.Add(itemId);
		}
	}

	public void openWindowAndGiveItems(float delay = 0.5f)
	{
		if (!windowOpen)
		{
			windowOpen = true;
			titleText.gameObject.SetActive(value: false);
			descText.gameObject.SetActive(value: false);
			itemIcon.gameObject.SetActive(value: false);
			StartCoroutine(giveItemDelay(delay));
		}
	}

	public void setTextColor(Color newColor)
	{
		recipeOrReceivedText.color = newColor;
		titleText.color = newColor;
		descText.color = newColor;
	}

	public IEnumerator playSoundWithDelay(ASound soundToPlay, float delay)
	{
		SoundManager.Instance.play2DSound(giftWindowPopupSound);
		yield return new WaitForSeconds(delay);
		SoundManager.Instance.play2DSound(soundToPlay);
	}

	private IEnumerator giveItemDelay(float delay)
	{
		bool somethingWasMailed = false;
		blueprintIcon.SetActive(value: false);
		yield return null;
		while (ConversationManager.manage.IsConversationActive)
		{
			yield return null;
		}
		yield return new WaitForSeconds(delay);
		bool ready = false;
		giftBox.SetActive(value: false);
		licenceStamp.SetActive(value: true);
		for (int j = 0; j < licenceToBeGiven.Count; j++)
		{
			setTextColor(textColor);
			recipeOrReceivedText.text = ConversationGenerator.generate.GetToolTip("Tip_New_Licence");
			blueprintIcon.SetActive(value: false);
			titleText.text = LicenceManager.manage.getLicenceName((LicenceManager.LicenceTypes)licenceToBeGiven[j]) + " " + ConversationGenerator.generate.GetToolTip("Tip_Level") + " " + LicenceManager.manage.allLicences[licenceToBeGiven[j]].getCurrentLevel();
			descText.text = ConversationGenerator.generate.GetToolTip("Tip_On_Ya");
			amountText.text = "";
			itemIcon.sprite = LicenceManager.manage.licenceIcons[licenceToBeGiven[j]];
			window.SetActive(value: true);
			StartCoroutine(playSoundWithDelay(licnecePopUpSound, 0.5f));
			itemIcon.gameObject.SetActive(value: true);
			titleText.gameObject.SetActive(value: true);
			descText.gameObject.SetActive(value: true);
			nextArrow.SetActive(value: true);
			yield return new WaitForSeconds(1f);
			while (!ready)
			{
				yield return null;
				if (InputMaster.input.UISelect())
				{
					ready = true;
					SoundManager.Instance.play2DSound(ConversationManager.manage.nextTextSound);
					nextArrow.SetActive(value: false);
				}
			}
			window.SetActive(value: false);
			titleText.gameObject.SetActive(value: false);
			descText.gameObject.SetActive(value: false);
			itemIcon.gameObject.SetActive(value: false);
			ready = false;
			yield return new WaitForSeconds(delay);
		}
		licenceStamp.SetActive(value: false);
		giftBox.SetActive(value: true);
		for (int j = 0; j < itemsToBeGiven.Count; j++)
		{
			setTextColor(textColor);
			recipeOrReceivedText.text = ConversationGenerator.generate.GetToolTip("Tip_You_Received");
			titleText.text = Inventory.Instance.allItems[itemsToBeGiven[j]].getInvItemName();
			if (!Inventory.Instance.allItems[itemsToBeGiven[j]].isFurniture)
			{
				descText.text = Inventory.Instance.allItems[itemsToBeGiven[j]].getItemDescription(itemsToBeGiven[j]);
			}
			else
			{
				descText.text = "";
			}
			itemIcon.sprite = Inventory.Instance.allItems[itemsToBeGiven[j]].getSprite();
			if (amountToBeGiven[j] > 1)
			{
				amountText.text = amountToBeGiven[j].ToString("n0");
			}
			else
			{
				amountText.text = "";
			}
			window.SetActive(value: true);
			StartCoroutine(playSoundWithDelay(windowPopUpSound, 0.5f));
			itemIcon.gameObject.SetActive(value: true);
			titleText.gameObject.SetActive(value: true);
			descText.gameObject.SetActive(value: true);
			nextArrow.SetActive(value: true);
			yield return new WaitForSeconds(1f);
			while (!ready)
			{
				yield return null;
				if (!InputMaster.input.UISelect())
				{
					continue;
				}
				ready = true;
				if (itemsToBeGiven[j] == Inventory.Instance.getInvItemId(mapItem))
				{
					TownManager.manage.mapUnlocked = true;
					RenderMap.Instance.ChangeMapWindow();
				}
				else if (itemsToBeGiven[j] == Inventory.Instance.getInvItemId(journalItem))
				{
					TownManager.manage.unlockJournalAndStartTime();
				}
				else if (itemsToBeGiven[j] == Inventory.Instance.getInvItemId(Inventory.Instance.moneyItem))
				{
					Inventory.Instance.changeWallet(amountToBeGiven[j]);
				}
				else if (Inventory.Instance.allItems[itemsToBeGiven[j]].hasFuel)
				{
					if (!Inventory.Instance.addItemToInventory(itemsToBeGiven[j], Inventory.Instance.allItems[itemsToBeGiven[j]].fuelMax, showNotification: false))
					{
						MailManager.manage.sendAnInvFullLetter(itemsToBeGiven[j], Inventory.Instance.allItems[itemsToBeGiven[j]].fuelMax);
						somethingWasMailed = true;
					}
				}
				else if (!Inventory.Instance.addItemToInventory(itemsToBeGiven[j], amountToBeGiven[j], showNotification: false))
				{
					MailManager.manage.sendAnInvFullLetter(itemsToBeGiven[j], amountToBeGiven[j]);
					somethingWasMailed = true;
				}
				SoundManager.Instance.play2DSound(ConversationManager.manage.nextTextSound);
				nextArrow.SetActive(value: false);
			}
			window.SetActive(value: false);
			titleText.gameObject.SetActive(value: false);
			descText.gameObject.SetActive(value: false);
			itemIcon.gameObject.SetActive(value: false);
			ready = false;
			yield return new WaitForSeconds(1f);
		}
		giftBox.SetActive(value: false);
		for (int j = 0; j < recipeLearnt.Count; j++)
		{
			setTextColor(recipeTextColor);
			recipeOrReceivedText.text = ConversationGenerator.generate.GetToolTip("Tip_New_Crafting_Recipe");
			blueprintIcon.SetActive(value: true);
			titleText.text = Inventory.Instance.allItems[recipeLearnt[j]].getInvItemName();
			descText.text = Inventory.Instance.allItems[recipeLearnt[j]].getItemDescription(recipeLearnt[j]);
			amountText.text = "";
			itemIcon.sprite = Inventory.Instance.allItems[recipeLearnt[j]].getSprite();
			window.SetActive(value: true);
			StartCoroutine(playSoundWithDelay(recipePopUpSound, 0.5f));
			itemIcon.gameObject.SetActive(value: true);
			titleText.gameObject.SetActive(value: true);
			descText.gameObject.SetActive(value: true);
			nextArrow.SetActive(value: true);
			yield return new WaitForSeconds(1f);
			while (!ready)
			{
				yield return null;
				if (InputMaster.input.UISelect())
				{
					ready = true;
					CharLevelManager.manage.unlockRecipe(Inventory.Instance.allItems[recipeLearnt[j]]);
					SoundManager.Instance.play2DSound(ConversationManager.manage.nextTextSound);
					nextArrow.SetActive(value: false);
				}
			}
			window.SetActive(value: false);
			titleText.gameObject.SetActive(value: false);
			descText.gameObject.SetActive(value: false);
			itemIcon.gameObject.SetActive(value: false);
			ready = false;
			yield return new WaitForSeconds(delay);
		}
		int overflowTracker = 0;
		while (overflowTracker < recipeOverflow.Count)
		{
			setTextColor(recipeTextColor);
			recipeOrReceivedText.text = ConversationGenerator.generate.GetToolTip("Tip_New_Crafting_Recipes");
			blueprintIcon.SetActive(value: true);
			titleText.text = "";
			descText.text = "";
			amountText.text = "";
			window.SetActive(value: true);
			StartCoroutine(playSoundWithDelay(recipePopUpSound, 0.5f));
			overflowWindow.SetActive(value: true);
			nextArrow.SetActive(value: true);
			for (int l = 0; l < 12; l++)
			{
				if (l + overflowTracker < recipeOverflow.Count)
				{
					overflowWindow.transform.Find("Overflow " + l).gameObject.SetActive(value: true);
					overflowWindow.transform.Find("Overflow " + l).transform.GetChild(0).GetComponent<Image>().sprite = Inventory.Instance.allItems[recipeOverflow[l + overflowTracker]].getSprite();
					CharLevelManager.manage.unlockRecipe(Inventory.Instance.allItems[recipeOverflow[l + overflowTracker]]);
				}
				else
				{
					overflowWindow.transform.Find("Overflow " + l).gameObject.SetActive(value: false);
				}
			}
			yield return new WaitForSeconds(1f);
			while (!ready)
			{
				yield return null;
				if (InputMaster.input.UISelect())
				{
					ready = true;
					SoundManager.Instance.play2DSound(ConversationManager.manage.nextTextSound);
					nextArrow.SetActive(value: false);
				}
			}
			overflowTracker += 11;
			window.SetActive(value: false);
			titleText.gameObject.SetActive(value: false);
			descText.gameObject.SetActive(value: false);
			itemIcon.gameObject.SetActive(value: false);
			overflowWindow.SetActive(value: false);
			ready = false;
			yield return new WaitForSeconds(delay);
		}
		itemsToBeGiven.Clear();
		amountToBeGiven.Clear();
		recipeLearnt.Clear();
		recipeOverflow.Clear();
		if (CharLevelManager.manage.unlockWindowOpen)
		{
			CharLevelManager.manage.refreshCurrentTier();
		}
		windowOpen = false;
		if (licenceToBeGiven.Count != 0)
		{
			licenceToBeGiven.Clear();
			LicenceManager.manage.openLicenceWindow();
		}
		if (somethingWasMailed)
		{
			NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("ItemSentToMailBox"), ConversationGenerator.generate.GetNotificationText("ItemSentToMailBox_Sub"));
		}
	}
}
