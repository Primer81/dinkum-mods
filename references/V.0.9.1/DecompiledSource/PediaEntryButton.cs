using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PediaEntryButton : MonoBehaviour
{
	public Image itemImage;

	private PediaEntry showingEntry;

	public GameObject nameTag;

	public TextMeshProUGUI nameText;

	public WindowAnimator myAnim;

	public GameObject museumIcon;

	public HeartContainer[] hearts;

	public HoverToolTipOnButton myHoverName;

	private bool isVillager;

	private bool isRecipe;

	public void setUpButton(PediaEntry myEntry, int i)
	{
		isVillager = myEntry.entryType == 5;
		isRecipe = myEntry.entryType == 6;
		hideHearts();
		if (isRecipe)
		{
			museumIcon.SetActive(value: false);
			itemImage.sprite = Inventory.Instance.allItems[myEntry.itemId].getSprite();
			nameText.text = Inventory.Instance.allItems[myEntry.itemId].getInvItemName();
			nameTag.SetActive(value: true);
			myHoverName.hoveringText = "???";
		}
		else if (isVillager)
		{
			museumIcon.SetActive(value: false);
			if (NPCManager.manage.npcStatus[myEntry.itemId].hasMet)
			{
				itemImage.sprite = NPCManager.manage.NPCDetails[myEntry.itemId].GetNPCSprite(myEntry.itemId);
				nameText.text = NPCManager.manage.NPCDetails[myEntry.itemId].GetNPCName();
				nameTag.SetActive(value: true);
				showHearts(NPCManager.manage.npcStatus[myEntry.itemId].relationshipLevel);
				myHoverName.hoveringText = NPCManager.manage.NPCDetails[myEntry.itemId].GetNPCName();
			}
			else
			{
				itemImage.sprite = PediaManager.manage.notFoundSprite;
				nameTag.SetActive(value: false);
				myHoverName.hoveringText = "???";
			}
		}
		else
		{
			if (myEntry.amountCaught > 0)
			{
				itemImage.sprite = Inventory.Instance.allItems[myEntry.itemId].getSprite();
				nameText.text = Inventory.Instance.allItems[myEntry.itemId].getInvItemName();
				nameTag.SetActive(value: true);
				myHoverName.hoveringText = Inventory.Instance.allItems[myEntry.itemId].getInvItemName();
			}
			else
			{
				itemImage.sprite = PediaManager.manage.notFoundSprite;
				nameTag.SetActive(value: false);
				myHoverName.hoveringText = "???";
			}
			if ((bool)Inventory.Instance.allItems[myEntry.itemId].bug || (bool)Inventory.Instance.allItems[myEntry.itemId].fish || (bool)Inventory.Instance.allItems[myEntry.itemId].underwaterCreature)
			{
				museumIcon.gameObject.SetActive(!MuseumManager.manage.checkIfDonationNeeded(Inventory.Instance.allItems[myEntry.itemId]));
			}
			else
			{
				museumIcon.gameObject.SetActive(value: false);
			}
		}
		showingEntry = myEntry;
		myAnim.openDelay = 0.01f;
		base.gameObject.SetActive(value: false);
		base.gameObject.SetActive(value: true);
	}

	public int getEntryNumber()
	{
		return showingEntry.itemId;
	}

	public SeasonAndTime getMySeasonAndTimeForSort()
	{
		if ((bool)Inventory.Instance.allItems[showingEntry.itemId].bug)
		{
			return Inventory.Instance.allItems[showingEntry.itemId].bug.mySeason;
		}
		if ((bool)Inventory.Instance.allItems[showingEntry.itemId].fish)
		{
			return Inventory.Instance.allItems[showingEntry.itemId].fish.mySeason;
		}
		if ((bool)Inventory.Instance.allItems[showingEntry.itemId].underwaterCreature)
		{
			return Inventory.Instance.allItems[showingEntry.itemId].underwaterCreature.mySeason;
		}
		if ((bool)Inventory.Instance.allItems[showingEntry.itemId].relic)
		{
			return Inventory.Instance.allItems[showingEntry.itemId].relic.myseason;
		}
		return null;
	}

	public void pressButton()
	{
		if (isVillager && NPCManager.manage.npcStatus[showingEntry.itemId].hasMet)
		{
			PediaManager.manage.showEntryDetails(showingEntry);
		}
		else if (isRecipe)
		{
			PediaManager.manage.showEntryDetails(showingEntry);
		}
		else if (showingEntry.amountCaught > 0)
		{
			PediaManager.manage.showEntryDetails(showingEntry);
		}
	}

	public void showHearts(int friendshipAmount)
	{
		for (int i = 0; i < hearts.Length; i++)
		{
			hearts[i].gameObject.SetActive(value: true);
			hearts[i].updateHealth(friendshipAmount);
		}
	}

	public void hideHearts()
	{
		for (int i = 0; i < hearts.Length; i++)
		{
			hearts[i].gameObject.SetActive(value: false);
		}
	}
}
