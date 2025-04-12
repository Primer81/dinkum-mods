using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
	public InventoryItem itemInSlot;

	public Sprite invSlotFull;

	public Sprite invSlotEmpty;

	public int itemNo = -1;

	public int stack;

	public Image slotBackgroundImage;

	public Image itemIcon;

	public TextMeshProUGUI stackText;

	public Image backingGlow;

	public Color selectedInQuickSlot;

	public Color cursorSelected;

	public Color originalColour;

	public Color defaulBarColor;

	public Sprite emptySprite;

	public InvSlotAnimator myAnim;

	public Equipable equipSlot;

	public GameObject fuelBar;

	public Image fuelBarFill;

	public GameObject stackBack;

	private bool isSelected;

	public int chestSlotNo = -1;

	public bool canNotBeSnappedTo;

	public GameObject selectedForGiveIcon;

	public GameObject giveAmountPopUp;

	public TextMeshProUGUI giveAmountText;

	public GameObject warningSign;

	private bool isBeingSnown = true;

	private bool selectedForGive;

	private bool disabledForGive;

	private int giveAmount;

	public bool slotUnlocked;

	private bool playingWarning;

	private void Awake()
	{
	}

	private void Start()
	{
		if ((bool)selectedForGiveIcon)
		{
			selectedForGiveIcon.SetActive(value: false);
		}
	}

	public void updateSlotContentsAndRefresh(int newItemNo, int amount)
	{
		itemNo = newItemNo;
		stack = amount;
		refreshSlot();
		if (chestSlotNo != -1 && ChestWindow.chests.chestWindowOpen)
		{
			ChestWindow.chests.makeALocalChange(chestSlotNo);
		}
		setUpSlotColours();
	}

	public void refreshSlot(bool playAnimation = true)
	{
		if ((bool)fuelBar)
		{
			if (itemNo != -1 && Inventory.Instance.allItems[itemNo].hasFuel)
			{
				if (stack > Inventory.Instance.allItems[itemNo].fuelMax)
				{
					stack = Inventory.Instance.allItems[itemNo].fuelMax;
				}
				if (itemIcon.gameObject.activeSelf)
				{
					fuelBar.gameObject.SetActive(value: true);
				}
				if (Inventory.Instance.allItems[itemNo].customFuelColour != Color.clear)
				{
					fuelBarFill.color = Inventory.Instance.allItems[itemNo].customFuelColour;
				}
				else
				{
					fuelBarFill.color = defaulBarColor;
				}
				if (!Inventory.Instance.invOpen)
				{
					float num = (float)stack / (float)Inventory.Instance.allItems[itemNo].fuelMax;
					if (fuelBarFill.fillAmount != num && num <= 0.05f && !playingWarning && (bool)warningSign && base.gameObject.activeInHierarchy)
					{
						StartCoroutine(FlashWarning());
					}
					fuelBarFill.fillAmount = (float)stack / (float)Inventory.Instance.allItems[itemNo].fuelMax;
				}
				else
				{
					fuelBarFill.fillAmount = (float)stack / (float)Inventory.Instance.allItems[itemNo].fuelMax;
				}
			}
			else if (itemNo != -1 && Inventory.Instance.allItems[itemNo].hasColourVariation)
			{
				fuelBarFill.color = EquipWindow.equip.vehicleColoursUI[Mathf.Clamp(stack - 1, 0, EquipWindow.equip.vehicleColoursUI.Length - 1)];
				fuelBarFill.fillAmount = 1f;
				if (itemIcon.gameObject.activeSelf)
				{
					fuelBar.gameObject.SetActive(value: true);
				}
			}
			else
			{
				fuelBar.gameObject.SetActive(value: false);
			}
		}
		if (playAnimation)
		{
			myAnim.UpdateSlotContents();
		}
		if (itemNo > -1 && stack <= 0 && (bool)itemInSlot && !Inventory.Instance.allItems[itemNo].hasFuel)
		{
			itemNo = -1;
			stack = 0;
		}
		if (itemNo > -1)
		{
			itemInSlot = Inventory.Instance.allItems[itemNo];
			itemIcon.sprite = itemInSlot.getSprite();
			itemIcon.enabled = true;
			if (!itemInSlot.hasFuel && !itemInSlot.hasColourVariation && stack > 1)
			{
				if (stack > 9999)
				{
					stackText.text = (stack / 1000).ToString("n0") + "K";
				}
				else
				{
					stackText.text = stack.ToString("n0");
				}
				if (stack > 999)
				{
					stackText.enableAutoSizing = true;
				}
				else
				{
					stackText.enableAutoSizing = false;
					stackText.fontSize = 13f;
				}
				if ((bool)stackBack)
				{
					stackBack.SetActive(value: true);
				}
				stackText.enabled = true;
			}
			else
			{
				stackText.enabled = false;
				if ((bool)stackBack)
				{
					stackBack.SetActive(value: false);
				}
			}
			slotBackgroundImage.sprite = UIAnimationManager.manage.getSlotSprite(itemNo);
		}
		else
		{
			itemInSlot = null;
			if ((bool)emptySprite)
			{
				itemIcon.sprite = emptySprite;
			}
			else
			{
				itemIcon.enabled = false;
			}
			stackText.enabled = false;
			if ((bool)stackBack)
			{
				stackBack.SetActive(value: false);
			}
			GetComponent<Image>().sprite = invSlotEmpty;
		}
		BulletinBoard.board.checkAllMissionsForItems();
		if (disabledForGive)
		{
			disableForGive();
		}
	}

	public void hideSlot(bool isSlotShown)
	{
		base.gameObject.SetActive(isSlotShown);
	}

	private void OnDisable()
	{
		if (!canNotBeSnappedTo)
		{
			Inventory.Instance.buttonsToSnapTo.Remove(GetComponent<RectTransform>());
		}
		if (isSelected)
		{
			base.transform.localScale = new Vector3(1f, 1f, 1f);
		}
		if ((bool)warningSign)
		{
			warningSign.SetActive(value: false);
			playingWarning = false;
		}
	}

	private void OnEnable()
	{
		if (!canNotBeSnappedTo && !Inventory.Instance.buttonsToSnapTo.Contains(GetComponent<RectTransform>()))
		{
			Inventory.Instance.buttonsToSnapTo.Add(GetComponent<RectTransform>());
		}
	}

	public void selectInQuickSlot()
	{
		if (!disabledForGive)
		{
			slotBackgroundImage.color = selectedInQuickSlot;
			myAnim.SelectSlot();
		}
		isSelected = true;
	}

	public void deselectSlot()
	{
		if (!disabledForGive)
		{
			slotBackgroundImage.color = originalColour;
			myAnim.DeSelectSlot();
			setUpSlotColours();
		}
		isSelected = false;
	}

	public void setUpSlotColours()
	{
		if ((bool)backingGlow)
		{
			backingGlow.enabled = false;
		}
	}

	public bool isSelectedForGive()
	{
		return selectedForGive;
	}

	public void selectThisSlotForGive()
	{
		if (GiveNPC.give.giveWindowOpen && GiveNPC.give.giveMenuTypeOpen != GiveNPC.currentlyGivingTo.Sell && GiveNPC.give.giveMenuTypeOpen != GiveNPC.currentlyGivingTo.Build && GiveNPC.give.giveMenuTypeOpen != GiveNPC.currentlyGivingTo.Tech && GiveNPC.give.giveMenuTypeOpen != GiveNPC.currentlyGivingTo.SellToTrapper && GiveNPC.give.giveMenuTypeOpen != GiveNPC.currentlyGivingTo.SellToJimmy && GiveNPC.give.giveMenuTypeOpen != GiveNPC.currentlyGivingTo.SellToTuckshop && GiveNPC.give.giveMenuTypeOpen != GiveNPC.currentlyGivingTo.SellToBugComp && GiveNPC.give.giveMenuTypeOpen != GiveNPC.currentlyGivingTo.Museum && GiveNPC.give.giveMenuTypeOpen != GiveNPC.currentlyGivingTo.SellToFishingComp)
		{
			GiveNPC.give.clearAllSelectedSlots();
		}
		itemIcon.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
		base.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
		selectedForGive = true;
		selectedForGiveIcon.transform.localScale = new Vector3(1f, 1f, 1f);
		selectedForGiveIcon.SetActive(value: true);
		selectedForGiveOrNotAnim();
	}

	public void deselectThisSlotForGive()
	{
		removeAllGiveAmount();
		itemIcon.transform.localScale = new Vector3(1f, 1f, 1f);
		base.transform.localScale = new Vector3(1f, 1f, 1f);
		selectedForGive = false;
		selectedForGiveIcon.transform.localScale = new Vector3(1f, 1f, 1f);
		selectedForGiveIcon.SetActive(value: false);
		selectedForGiveOrNotAnim();
	}

	public bool isDisabledForGive()
	{
		return disabledForGive;
	}

	public void disableForGive()
	{
		removeAllGiveAmount();
		disabledForGive = true;
		slotBackgroundImage.color = Color.Lerp(Color.white, Color.black, 0.35f);
		itemIcon.color = Color.Lerp(Color.white, Color.black, 0.35f);
		stackBack.SetActive(value: false);
		fuelBar.SetActive(value: false);
	}

	public void slotDisableOnly()
	{
		disabledForGive = true;
	}

	public void slotEnableOnly()
	{
		disabledForGive = false;
	}

	public void clearDisable()
	{
		disabledForGive = false;
		slotBackgroundImage.color = Color.white;
		itemIcon.color = Color.white;
		updateSlotContentsAndRefresh(itemNo, stack);
	}

	public int getGiveAmount()
	{
		return giveAmount;
	}

	public void addGiveAmount(int amount = 1)
	{
		if (!selectedForGive)
		{
			SoundManager.Instance.play2DSound(SoundManager.Instance.selectSlotForGive);
			selectThisSlotForGive();
		}
		if (!Inventory.Instance.allItems[itemNo].hasFuel && stack != 1)
		{
			if (giveAmount + amount > stack)
			{
				SoundManager.Instance.play2DSound(SoundManager.Instance.buttonCantPressSound);
			}
			else
			{
				SoundManager.Instance.play2DSound(SoundManager.Instance.selectSlotForGive);
			}
			if (GiveNPC.give.giveMenuTypeOpen == GiveNPC.currentlyGivingTo.SellToJimmy)
			{
				giveAmount = Mathf.Clamp(giveAmount + amount, 50, stack);
			}
			else
			{
				giveAmount = Mathf.Clamp(giveAmount + amount, 0, stack);
			}
			giveAmountPopUp.SetActive(value: true);
			giveAmountText.text = giveAmount.ToString();
		}
	}

	public void removeAllGiveAmount()
	{
		if ((bool)giveAmountPopUp)
		{
			giveAmountPopUp.gameObject.SetActive(value: false);
			giveAmount = 0;
		}
	}

	private void selectedForGiveOrNotAnim()
	{
	}

	private IEnumerator selectSlotForGiveAnimation()
	{
		yield return null;
	}

	private IEnumerator FlashWarning()
	{
		int inSlotAtStart = itemNo;
		playingWarning = true;
		float showTimerAmount = 0.5f;
		float hideTimerAmount = 0.2f;
		float timer5 = showTimerAmount;
		warningSign.SetActive(value: true);
		while (timer5 > 0f)
		{
			timer5 -= Time.deltaTime;
			yield return null;
			if (inSlotAtStart != itemNo)
			{
				warningSign.SetActive(value: false);
				playingWarning = false;
				yield break;
			}
		}
		timer5 = hideTimerAmount;
		warningSign.SetActive(value: false);
		while (timer5 > 0f)
		{
			timer5 -= Time.deltaTime;
			yield return null;
			if (inSlotAtStart != itemNo)
			{
				warningSign.SetActive(value: false);
				playingWarning = false;
				yield break;
			}
		}
		warningSign.SetActive(value: true);
		timer5 = showTimerAmount;
		while (timer5 > 0f)
		{
			timer5 -= Time.deltaTime;
			yield return null;
			if (inSlotAtStart != itemNo)
			{
				warningSign.SetActive(value: false);
				playingWarning = false;
				yield break;
			}
		}
		timer5 = hideTimerAmount;
		warningSign.SetActive(value: false);
		while (timer5 > 0f)
		{
			timer5 -= Time.deltaTime;
			yield return null;
			if (inSlotAtStart != itemNo)
			{
				warningSign.SetActive(value: false);
				playingWarning = false;
				yield break;
			}
		}
		warningSign.SetActive(value: true);
		timer5 = showTimerAmount;
		while (timer5 > 0f)
		{
			timer5 -= Time.deltaTime;
			yield return null;
			if (inSlotAtStart != itemNo)
			{
				warningSign.SetActive(value: false);
				playingWarning = false;
				yield break;
			}
		}
		playingWarning = false;
		warningSign.SetActive(value: false);
	}
}
