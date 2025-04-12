using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyWindows : MonoBehaviour
{
	public static CurrencyWindows currency;

	public Transform cornerPos;

	public Transform moneyInvPos;

	[Header("Actual Boxes")]
	public Transform walletBox;

	public Transform permitPointBox;

	public GameObject buffBox;

	private RectTransform walletRect;

	private RectTransform buffRect;

	private GridLayoutGroup buffGrid;

	public GameObject foodBox;

	private RectTransform foodRect;

	private GridLayoutGroup foodGrid;

	[Header("Task Boxes")]
	public Transform journalTaskPos;

	public Transform sideTaskBarSmall;

	public Transform sideTaskBarLarge;

	private RectTransform cornerPosRect;

	private bool cornerPosOn;

	private bool journalOpen;

	private bool buffBoxInInventorySlot;

	private Coroutine runningPointsCoroutine;

	private Coroutine runningMoneyRoutine;

	private void Awake()
	{
		currency = this;
	}

	private void Start()
	{
		walletRect = walletBox.GetComponent<RectTransform>();
		cornerPosRect = cornerPos.GetComponent<RectTransform>();
		buffRect = buffBox.GetComponent<RectTransform>();
		buffGrid = buffBox.GetComponent<GridLayoutGroup>();
		foodGrid = foodBox.GetComponent<GridLayoutGroup>();
		foodRect = foodBox.GetComponent<RectTransform>();
	}

	public void openInv()
	{
		if (GiveNPC.give.giveWindowOpen)
		{
			walletBox.gameObject.SetActive(value: true);
			buffBox.SetActive(value: false);
		}
		else
		{
			walletBox.SetParent(moneyInvPos);
			walletRect.anchoredPosition = Vector2.zero;
			walletBox.gameObject.SetActive(value: true);
			buffBox.SetActive(value: true);
		}
		moneyInvPos.gameObject.SetActive(value: false);
		Invoke("moveMoneyBoxDelay", 0.1f);
	}

	private void moveMoneyBoxDelay()
	{
		moneyInvPos.transform.position = new Vector2(Inventory.Instance.invSlots[11].transform.position.x, 120f);
		moneyInvPos.transform.localPosition = new Vector3(moneyInvPos.transform.localPosition.x + 84f, 120f);
		moneyInvPos.gameObject.SetActive(value: true);
		MoveBuffs();
	}

	public void closeInv()
	{
		walletBox.SetParent(cornerPos);
		walletBox.SetSiblingIndex(0);
		MoveBuffs();
	}

	public void windowOn(bool enabled)
	{
		cornerPos.gameObject.SetActive(enabled);
		cornerPosOn = enabled;
		MoveBuffs();
	}

	public void openJournal()
	{
		if (MenuButtonsTop.menu.subMenuOpen || QuestTracker.track.trackerOpen || PhotoManager.manage.photoTabOpen || LicenceManager.manage.windowOpen || HouseEditor.edit.windowOpen)
		{
			walletBox.gameObject.SetActive(value: false);
			buffBox.SetActive(value: false);
		}
		for (int i = 0; i < DailyTaskGenerator.generate.taskIcons.Length; i++)
		{
			DailyTaskGenerator.generate.taskIcons[i].transform.SetParent(journalTaskPos);
			DailyTaskGenerator.generate.taskIcons[i].makeBig();
		}
		journalOpen = true;
		RenderMap.Instance.ChangeMapWindow();
		sideTaskBarSmall.gameObject.SetActive(value: false);
		permitPointBox.gameObject.SetActive(value: true);
	}

	public void closeJournal()
	{
		walletBox.gameObject.SetActive(value: true);
		buffBox.SetActive(value: true);
		for (int i = 0; i < DailyTaskGenerator.generate.taskIcons.Length; i++)
		{
			DailyTaskGenerator.generate.taskIcons[i].transform.SetParent(sideTaskBarSmall);
			DailyTaskGenerator.generate.taskIcons[i].makeSmall();
		}
		journalOpen = false;
		RenderMap.Instance.ChangeMapWindow();
		if (TownManager.manage.journalUnlocked)
		{
			sideTaskBarSmall.gameObject.SetActive(value: true);
		}
	}

	public void checkIfPointsNeeded()
	{
		if ((bool)cornerPosRect && runningPointsCoroutine == null)
		{
			runningPointsCoroutine = StartCoroutine(checkForPointChange());
		}
	}

	public void checkIfMoneyBoxNeeded()
	{
		if ((bool)cornerPosRect && runningMoneyRoutine == null)
		{
			runningMoneyRoutine = StartCoroutine(checkForWalletChange());
		}
	}

	private IEnumerator checkForPointChange()
	{
		float afterWaitTimer = 0f;
		walletBox.gameObject.SetActive(value: false);
		if (!PermitPointsManager.manage.isPointTotalShown())
		{
			while (afterWaitTimer <= 2f)
			{
				permitPointBox.gameObject.SetActive(value: true);
				if (RenderMap.Instance.mapCircle.gameObject.activeInHierarchy)
				{
					cornerPosRect.anchoredPosition = new Vector2(-165f, 0f);
				}
				else
				{
					cornerPosRect.anchoredPosition = new Vector2(0f, 0f);
				}
				cornerPos.gameObject.SetActive(value: true);
				afterWaitTimer = ((!PermitPointsManager.manage.isPointTotalShown()) ? 0f : (afterWaitTimer + Time.deltaTime));
				yield return null;
				MoveBuffs();
			}
		}
		cornerPosRect.anchoredPosition = new Vector2(0f, 0f);
		cornerPos.gameObject.SetActive(cornerPosOn);
		runningPointsCoroutine = null;
		MoveBuffs();
		if (!journalOpen)
		{
			walletBox.gameObject.SetActive(value: true);
		}
	}

	private IEnumerator checkForWalletChange()
	{
		float afterWaitTimer = 0f;
		permitPointBox.gameObject.SetActive(value: false);
		if (!Inventory.Instance.isWalletTotalShown())
		{
			while (afterWaitTimer <= 2f)
			{
				if (RenderMap.Instance.mapCircle.gameObject.activeInHierarchy)
				{
					cornerPosRect.anchoredPosition = new Vector2(-165f, 0f);
				}
				else
				{
					cornerPosRect.anchoredPosition = new Vector2(0f, 0f);
				}
				afterWaitTimer = ((!Inventory.Instance.isWalletTotalShown()) ? 0f : (afterWaitTimer + Time.deltaTime));
				cornerPos.gameObject.SetActive(value: true);
				if (!Inventory.Instance.invOpen)
				{
					walletBox.gameObject.SetActive(value: true);
					walletBox.SetParent(cornerPos);
					walletBox.SetSiblingIndex(0);
					MoveBuffs();
				}
				yield return null;
			}
		}
		cornerPosRect.anchoredPosition = new Vector2(0f, 0f);
		cornerPos.gameObject.SetActive(cornerPosOn);
		permitPointBox.gameObject.SetActive(value: true);
		MoveBuffs();
		runningMoneyRoutine = null;
	}

	public void MoveBuffs()
	{
		if (!Inventory.Instance.invOpen)
		{
			SetRectSettings(buffRect);
			SetRectSettings(foodRect);
			SetGridSettings(buffGrid);
			SetGridSettings(foodGrid);
			if (walletBox.gameObject.activeInHierarchy)
			{
				buffRect.anchoredPosition = new Vector2(cornerPosRect.anchoredPosition.x - 220f, cornerPosRect.anchoredPosition.y - 65f);
				foodRect.anchoredPosition = new Vector2(cornerPosRect.anchoredPosition.x - 220f, cornerPosRect.anchoredPosition.y - 30f);
			}
			else
			{
				buffRect.anchoredPosition = new Vector2(cornerPosRect.anchoredPosition.x - 200f, cornerPosRect.anchoredPosition.y - 65f);
				foodRect.anchoredPosition = new Vector2(cornerPosRect.anchoredPosition.x - 200f, cornerPosRect.anchoredPosition.y - 30f);
			}
			if (buffBoxInInventorySlot)
			{
				buffBoxInInventorySlot = false;
				buffBox.SetActive(value: false);
				buffBox.SetActive(value: true);
				foodBox.SetActive(value: false);
				foodBox.SetActive(value: true);
			}
		}
	}

	public void SetRectSettings(RectTransform rectTransform)
	{
		if (!Inventory.Instance.invOpen)
		{
			rectTransform.pivot = new Vector3(1f, 1f);
			rectTransform.anchorMin = new Vector2(1f, 1f);
			rectTransform.anchorMax = new Vector2(1f, 1f);
		}
	}

	public void SetGridSettings(GridLayoutGroup gridToAdjust)
	{
		if (!Inventory.Instance.invOpen)
		{
			gridToAdjust.startCorner = GridLayoutGroup.Corner.UpperRight;
			gridToAdjust.childAlignment = TextAnchor.UpperRight;
			gridToAdjust.cellSize = new Vector2(20f, 20f);
		}
	}
}
