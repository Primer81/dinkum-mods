using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationManager : MonoBehaviour
{
	public enum toolTipType
	{
		None,
		multiTiledPlacing,
		singleTiledPlacing,
		PickUp,
		Dive,
		StopDriving,
		GetUp,
		CarryingItem,
		CarryingAnimal,
		Fishing,
		InChest,
		InGiveMenu,
		InChestWhileHoldingItem,
		DrivingBoostVehicle,
		DrivingTractor,
		DrivingMower
	}

	public static NotificationManager manage;

	public GameObject itemNotificationPrefab;

	public GameObject damageNotificationPrefab;

	public GameObject chatBubblePrefab;

	public PickUpNotification buttonPromptNotification;

	public RectTransform buttonPromptTransform;

	public Transform notificationWindow;

	public Transform topNotificationWindow;

	public TopNotification topNotification;

	public List<PickUpNotification> notifications;

	private RectTransform canvasTrans;

	private float notificationTimer;

	private bool floatUp = true;

	private float floatDif;

	public GameObject hintWindow;

	public PickUpNotification xButtonHint;

	public PickUpNotification yButtonHint;

	public PickUpNotification bButtonHint;

	public PickUpNotification aButtonHint;

	public PickUpNotification splitStackHint;

	public PickUpNotification quickMoveStackHint;

	public PickUpNotification quickStackButtonHint;

	public PocketsFullNotification pocketsFull;

	public Transform speechBubbleWindow;

	private float sideNotificationYdif = -35f;

	private Coroutine moveNotificationsRunning;

	private toolTipType showingTip;

	private bool showingUsingMouse = true;

	private Coroutine topNotificationRunning;

	private List<string> toNotify = new List<string>();

	private List<string> subTextNot = new List<string>();

	private List<ASound> soundToPlay = new List<ASound>();

	private void Awake()
	{
		manage = this;
		canvasTrans = GetComponent<RectTransform>();
		buttonPromptNotification.gameObject.SetActive(value: false);
	}

	private IEnumerator moveNotifications()
	{
		yield return null;
		while (notifications.Count > 0)
		{
			for (int i = 0; i < notifications.Count; i++)
			{
				float value = Mathf.Lerp(notifications[i].transform.localPosition.x, 0f, Time.deltaTime * 25f);
				notifications[i].transform.localPosition = new Vector3(Mathf.Clamp(value, 0f, 100f), (float)(i * -35) + sideNotificationYdif, 0f);
			}
			float num = 3f;
			if (notifications.Count > 8)
			{
				num = 0.05f;
			}
			else if (notifications.Count > 6)
			{
				num = 0.2f;
			}
			else if (notifications.Count > 5)
			{
				num = 0.5f;
			}
			else if (notifications.Count > 4)
			{
				num = 1f;
			}
			else if (notifications.Count > 3)
			{
				num = 1.5f;
			}
			sideNotificationYdif = Mathf.Lerp(sideNotificationYdif, 0f, Time.deltaTime * 10f);
			if (notificationTimer < num)
			{
				notificationTimer += Time.deltaTime;
			}
			else
			{
				Mathf.Lerp(notifications[0].transform.localPosition.x, 150f, Time.deltaTime * 25f);
				while (notifications[0].transform.localPosition.x < 100f)
				{
					yield return null;
					float x = Mathf.Lerp(notifications[0].transform.localPosition.x, 150f, Time.deltaTime * 25f);
					notifications[0].transform.localPosition = new Vector3(x, 0f + sideNotificationYdif, 0f);
				}
				notificationTimer = 0f;
				Object.Destroy(notifications[0].gameObject);
				notifications.Remove(notifications[0]);
				sideNotificationYdif = -35f;
			}
			yield return null;
		}
	}

	public void createDamageNotification(int damgeToShow, Transform connectToTrans)
	{
		Object.Instantiate(damageNotificationPrefab, connectToTrans.position + new Vector3(Random.Range(-0.15f, 0.15f), Random.Range(0.15f, 0.25f), Random.Range(-0.15f, 0.15f)), Quaternion.identity).GetComponent<DamageNumberPop>().setDamageText(damgeToShow);
	}

	public void CreatePositiveDamageNotification(int damgeToShow, Transform connectToTrans)
	{
		Object.Instantiate(damageNotificationPrefab, connectToTrans.position + new Vector3(Random.Range(-0.15f, 0.15f), Random.Range(0.15f, 0.25f), Random.Range(-0.15f, 0.15f)), Quaternion.identity).GetComponent<DamageNumberPop>().setHealthIncreaseText(damgeToShow);
	}

	public void createChatBubble(string message, Transform connectedTransform)
	{
		Object.Instantiate(chatBubblePrefab, CameraController.control.mainCamera.WorldToScreenPoint(connectedTransform.position), Quaternion.identity, base.transform).GetComponent<SpeechBubble>();
	}

	public void createPickUpNotification(int inventoryId, int stackAmount = 1)
	{
		notificationTimer = 0f;
		bool flag = false;
		if (Inventory.Instance.allItems[inventoryId].checkIfStackable())
		{
			for (int i = 0; i < notifications.Count; i++)
			{
				if (notifications[i].showingItemId == inventoryId)
				{
					notifications[i].fillItemDetails(notifications[i].showingItemId, notifications[i].showingStack + stackAmount);
					notifications[i].GetComponent<WindowAnimator>().refreshAnimation();
					PickUpNotification item = notifications[i];
					notifications.RemoveAt(i);
					notifications.Add(item);
					flag = true;
					break;
				}
			}
		}
		if (notifications.Count == 0)
		{
			sideNotificationYdif = -35f;
			if (moveNotificationsRunning != null)
			{
				StopCoroutine(moveNotificationsRunning);
				moveNotificationsRunning = null;
			}
			moveNotificationsRunning = StartCoroutine(moveNotifications());
		}
		if (!flag)
		{
			PickUpNotification component = Object.Instantiate(itemNotificationPrefab, notificationWindow).GetComponent<PickUpNotification>();
			component.transform.localPosition = new Vector3(100f, notifications.Count * -35 - 25, 0f);
			component.fillItemDetails(inventoryId, stackAmount);
			notifications.Add(component);
			if (notifications.Count < 2)
			{
				notificationTimer = 0f;
			}
		}
	}

	public void turnOnPocketsFullNotification(bool holdingButton = false)
	{
		pocketsFull.showPocketsFull(holdingButton);
	}

	public void createChatNotification(string chatText, bool specialTip = false)
	{
		if (specialTip)
		{
			List<PickUpNotification> list = new List<PickUpNotification>();
			for (int i = 0; i < notifications.Count; i++)
			{
				if (notifications[i].itemText.text == chatText)
				{
					list.Add(notifications[i]);
				}
			}
			foreach (PickUpNotification item in list)
			{
				notifications.Remove(item);
				sideNotificationYdif = -35f;
				Object.Destroy(item.gameObject);
			}
		}
		if (notifications.Count == 0)
		{
			sideNotificationYdif = -35f;
			if (moveNotificationsRunning != null)
			{
				StopCoroutine(moveNotificationsRunning);
				moveNotificationsRunning = null;
			}
			moveNotificationsRunning = StartCoroutine(moveNotifications());
		}
		PickUpNotification component = Object.Instantiate(itemNotificationPrefab, notificationWindow).GetComponent<PickUpNotification>();
		component.transform.localPosition = new Vector3(100f, notifications.Count * -35 - 25, 0f);
		component.fillChatDetails(chatText);
		notifications.Add(component);
		if (notifications.Count < 5)
		{
			notificationTimer = -3f;
		}
	}

	public void fillIconForType(Image itemImage, TextMeshProUGUI buttonOverrideText, Input_Rebind.RebindType buttonType)
	{
		if (ButtonIcons.icons.isOverridden(buttonType))
		{
			itemImage.sprite = ButtonIcons.icons.isAMouseButton(Input_Rebind.rebind.getKeyBindingForInGame(buttonType));
			if (itemImage.sprite != null)
			{
				buttonOverrideText.gameObject.SetActive(value: false);
				itemImage.enabled = true;
				return;
			}
			buttonOverrideText.text = Input_Rebind.rebind.getKeyBindingForInGame(buttonType);
			if (buttonOverrideText.text.Length >= 3)
			{
				buttonOverrideText.text = buttonOverrideText.text.Replace("Left", "");
				itemImage.sprite = ButtonIcons.icons.genericLong.keyBoardIcon;
			}
			else
			{
				itemImage.sprite = ButtonIcons.icons.genericSmall.keyBoardIcon;
			}
			itemImage.enabled = true;
			buttonOverrideText.gameObject.SetActive(value: true);
		}
		else
		{
			Sprite spriteForType = ButtonIcons.icons.getSpriteForType(buttonType);
			if (spriteForType == null)
			{
				itemImage.enabled = false;
			}
			else
			{
				itemImage.enabled = true;
			}
			buttonOverrideText.gameObject.SetActive(value: false);
			itemImage.sprite = spriteForType;
		}
	}

	public void showButtonPrompt(string promptText, string buttonName, Vector3 buttonPromptPosition)
	{
		if (Inventory.Instance.usingMouse)
		{
			switch (buttonName)
			{
			case "Z":
				buttonPromptNotification.fillButtonPrompt(promptText, Input_Rebind.RebindType.SwapCameraMode);
				break;
			case "B":
				buttonPromptNotification.fillButtonPrompt(promptText, Input_Rebind.RebindType.Interact);
				break;
			case "X":
				buttonPromptNotification.fillButtonPrompt(promptText, Input_Rebind.RebindType.Use);
				break;
			case "no":
				buttonPromptNotification.fillButtonPrompt(promptText, buttonPromptNotification.cantSprite);
				break;
			default:
				buttonPromptNotification.fillButtonPrompt(promptText, null);
				break;
			}
		}
		else
		{
			switch (buttonName)
			{
			case "Z":
				buttonPromptNotification.fillButtonPrompt(promptText, buttonPromptNotification.controllerRightStick);
				break;
			case "B":
				buttonPromptNotification.fillButtonPrompt(promptText, buttonPromptNotification.controllerB);
				break;
			case "X":
				buttonPromptNotification.fillButtonPrompt(promptText, buttonPromptNotification.controllerX);
				break;
			case "no":
				buttonPromptNotification.fillButtonPrompt(promptText, buttonPromptNotification.cantSprite);
				break;
			default:
				buttonPromptNotification.fillButtonPrompt(promptText, null);
				break;
			}
		}
		buttonPromptNotification.gameObject.SetActive(value: true);
		Vector3 vector = CameraController.control.mainCamera.WorldToViewportPoint(buttonPromptPosition + Vector3.up * 2f + Vector3.up * floatDif);
		if (floatUp)
		{
			if (floatDif > 0.15f)
			{
				floatUp = false;
			}
			floatDif += 0.005f;
		}
		else
		{
			if (floatDif < -0.15f)
			{
				floatUp = true;
			}
			floatDif -= 0.005f;
		}
		buttonPromptTransform.anchoredPosition = new Vector2(vector.x * canvasTrans.sizeDelta.x - canvasTrans.sizeDelta.x * 0.5f, vector.y * canvasTrans.sizeDelta.y - canvasTrans.sizeDelta.y * 0.5f);
		buttonPromptTransform.anchoredPosition += new Vector2(buttonPromptTransform.sizeDelta.x * 4f + 50f, 0f);
	}

	public void hideButtonPrompt()
	{
		if (buttonPromptNotification.gameObject.activeSelf)
		{
			buttonPromptNotification.gameObject.SetActive(value: false);
		}
	}

	public void hitWindowOpen(bool isOpen)
	{
		if (hintWindow.gameObject.activeInHierarchy != isOpen)
		{
			hintWindow.gameObject.SetActive(isOpen);
		}
	}

	public void resetHintButtons()
	{
		showingTip = toolTipType.None;
		toolTipHintBox("null", "null", "null", "null");
	}

	public void hintWindowOpen(toolTipType toolTip)
	{
		if (toolTip == showingTip && showingUsingMouse == Inventory.Instance.usingMouse)
		{
			return;
		}
		showingTip = toolTip;
		showingUsingMouse = Inventory.Instance.usingMouse;
		if (showingTip == toolTipType.None)
		{
			hintWindow.gameObject.SetActive(value: false);
			return;
		}
		hintWindow.gameObject.SetActive(value: true);
		if (showingTip == toolTipType.InChest || showingTip == toolTipType.InGiveMenu || showingTip == toolTipType.InChestWhileHoldingItem)
		{
			toolTipMenu(showingTip);
		}
		else if (showingTip == toolTipType.PickUp)
		{
			toolTipHintBox("null", ConversationGenerator.generate.GetToolTip("Tip_PickUp"), "null", "null");
		}
		else if (showingTip == toolTipType.Dive)
		{
			toolTipHintBox("null", ConversationGenerator.generate.GetToolTip("Tip_Dive"), "null", "null");
		}
		else if (showingTip == toolTipType.Fishing)
		{
			toolTipHintBox(ConversationGenerator.generate.GetToolTip("Tip_ReelIn"), "null", ConversationGenerator.generate.GetToolTip("Tip_Cancel"), "null");
		}
		else if (showingTip == toolTipType.CarryingAnimal)
		{
			toolTipHintBox(ConversationGenerator.generate.GetToolTip("Tip_Release"), "null", "null", ConversationGenerator.generate.GetToolTip("Tip_Drop"));
		}
		else if (showingTip == toolTipType.CarryingItem)
		{
			toolTipHintBox("null", "null", "null", ConversationGenerator.generate.GetToolTip("Tip_Drop"));
		}
		else if (showingTip == toolTipType.multiTiledPlacing)
		{
			toolTipHintBox(ConversationGenerator.generate.GetToolTip("Tip_PlaceDeed"), ConversationGenerator.generate.GetToolTip("Tip_Rotate"), ConversationGenerator.generate.GetToolTip("Tip_Cancel"), "null");
		}
		else if (showingTip == toolTipType.singleTiledPlacing)
		{
			toolTipHintBox("null", ConversationGenerator.generate.GetToolTip("Tip_Rotate"), "null", "null");
		}
		else if (showingTip == toolTipType.StopDriving)
		{
			toolTipHintBox("null", ConversationGenerator.generate.GetToolTip("Tip_StopDriving"), "null", "null");
		}
		else if (showingTip == toolTipType.DrivingTractor)
		{
			if (Inventory.Instance.usingMouse)
			{
				toolTipHintBox(ConversationGenerator.generate.GetToolTip("Tip_Use"), ConversationGenerator.generate.GetToolTip("Tip_StopDriving"), ConversationGenerator.generate.GetToolTip("Tip_Switch"), "null");
			}
			else
			{
				toolTipHintBox(ConversationGenerator.generate.GetToolTip("Tip_Use"), ConversationGenerator.generate.GetToolTip("Tip_StopDriving"), "null", ConversationGenerator.generate.GetToolTip("Tip_Switch"));
			}
		}
		else if (showingTip == toolTipType.DrivingBoostVehicle)
		{
			toolTipHintBox(ConversationGenerator.generate.GetToolTip("Tip_Boost"), ConversationGenerator.generate.GetToolTip("Tip_StopDriving"), "null", "null");
		}
		else if (showingTip == toolTipType.DrivingMower)
		{
			if (Inventory.Instance.usingMouse)
			{
				toolTipHintBox("null", ConversationGenerator.generate.GetToolTip("Tip_StopDriving"), ConversationGenerator.generate.GetToolTip("Tip_SwapBlade"), "null");
			}
			else
			{
				toolTipHintBox("null", ConversationGenerator.generate.GetToolTip("Tip_StopDriving"), "null", ConversationGenerator.generate.GetToolTip("Tip_SwapBlade"));
			}
		}
		else if (showingTip == toolTipType.GetUp)
		{
			toolTipHintBox("null", ConversationGenerator.generate.GetToolTip("Tip_GetUp"), "null", "null");
		}
	}

	public void toolTipMenu(toolTipType menuType)
	{
		switch (menuType)
		{
		case toolTipType.InGiveMenu:
			if (Inventory.Instance.usingMouse)
			{
				splitStackHint.fillButtonPrompt((LocalizedString)"ToolTips/Tip_SelectAmount", splitStackHint.controllerRightClick);
			}
			else
			{
				splitStackHint.fillButtonPrompt((LocalizedString)"ToolTips/Tip_SelectAmount", splitStackHint.controllerX);
			}
			splitStackHint.gameObject.SetActive(value: true);
			quickMoveStackHint.gameObject.SetActive(value: false);
			quickStackButtonHint.gameObject.SetActive(value: false);
			break;
		case toolTipType.InChest:
			if (Inventory.Instance.usingMouse)
			{
				splitStackHint.fillButtonPrompt((LocalizedString)"ToolTips/Tip_SplitStack", splitStackHint.controllerRightClick);
			}
			else
			{
				splitStackHint.fillButtonPrompt((LocalizedString)"ToolTips/Tip_SplitStack", splitStackHint.controllerX);
			}
			splitStackHint.gameObject.SetActive(value: true);
			if (Inventory.Instance.usingMouse)
			{
				quickMoveStackHint.fillButtonPrompt((LocalizedString)"ToolTips/Tip_QuickMove", quickMoveStackHint.controllerE);
			}
			else
			{
				quickMoveStackHint.fillButtonPrompt((LocalizedString)"ToolTips/Tip_QuickMove", quickMoveStackHint.controllerY);
			}
			quickMoveStackHint.gameObject.SetActive(value: true);
			if (ChestWindow.chests.chestWindowOpen)
			{
				quickStackButtonHint.gameObject.SetActive(value: true);
				if (Inventory.Instance.usingMouse)
				{
					quickStackButtonHint.fillButtonPrompt((LocalizedString)"ToolTips/Tip_StackAll", quickStackButtonHint.controllerE);
				}
				else
				{
					quickStackButtonHint.fillButtonPrompt((LocalizedString)"ToolTips/Tip_StackAll", quickStackButtonHint.controllerY);
				}
			}
			else
			{
				quickStackButtonHint.gameObject.SetActive(value: false);
			}
			break;
		case toolTipType.InChestWhileHoldingItem:
			if (Inventory.Instance.usingMouse)
			{
				splitStackHint.fillButtonPrompt((LocalizedString)"ToolTips/Tip_PlaceOne", splitStackHint.controllerRightClick);
			}
			else
			{
				splitStackHint.fillButtonPrompt((LocalizedString)"ToolTips/Tip_PlaceOne", splitStackHint.controllerX);
			}
			splitStackHint.gameObject.SetActive(value: true);
			quickMoveStackHint.gameObject.SetActive(value: false);
			quickStackButtonHint.gameObject.SetActive(value: false);
			break;
		default:
			quickMoveStackHint.gameObject.SetActive(value: false);
			splitStackHint.gameObject.SetActive(value: false);
			quickStackButtonHint.gameObject.SetActive(value: false);
			break;
		}
		xButtonHint.gameObject.SetActive(value: false);
		yButtonHint.gameObject.SetActive(value: false);
		bButtonHint.gameObject.SetActive(value: false);
		aButtonHint.gameObject.SetActive(value: false);
	}

	public void toolTipHintBox(string xHintText, string yHintText, string bHintText, string aButtonText)
	{
		if (xHintText != "null")
		{
			xButtonHint.gameObject.SetActive(value: true);
			if (Inventory.Instance.usingMouse)
			{
				xButtonHint.fillButtonPrompt(xHintText, Input_Rebind.RebindType.Use);
			}
			else
			{
				xButtonHint.fillButtonPrompt(xHintText, xButtonHint.controllerX);
			}
		}
		else
		{
			xButtonHint.gameObject.SetActive(value: false);
		}
		if (bHintText != "null")
		{
			bButtonHint.gameObject.SetActive(value: true);
			if (Inventory.Instance.usingMouse)
			{
				bButtonHint.fillButtonPrompt(bHintText, Input_Rebind.RebindType.Interact);
			}
			else
			{
				bButtonHint.fillButtonPrompt(bHintText, bButtonHint.controllerB);
			}
		}
		else
		{
			bButtonHint.gameObject.SetActive(value: false);
		}
		if (yHintText != "null")
		{
			yButtonHint.gameObject.SetActive(value: true);
			if (Inventory.Instance.usingMouse)
			{
				yButtonHint.fillButtonPrompt(yHintText, Input_Rebind.RebindType.Other);
			}
			else
			{
				yButtonHint.fillButtonPrompt(yHintText, yButtonHint.controllerY);
			}
		}
		else
		{
			yButtonHint.gameObject.SetActive(value: false);
		}
		if (aButtonText != "null")
		{
			aButtonHint.gameObject.SetActive(value: true);
			if (Inventory.Instance.usingMouse)
			{
				aButtonHint.fillButtonPrompt(aButtonText, Input_Rebind.RebindType.Interact);
			}
			else
			{
				aButtonHint.fillButtonPrompt(aButtonText, aButtonHint.controllerB);
			}
		}
		else
		{
			aButtonHint.gameObject.SetActive(value: false);
		}
		quickMoveStackHint.gameObject.SetActive(value: false);
		splitStackHint.gameObject.SetActive(value: false);
		quickStackButtonHint.gameObject.SetActive(value: false);
	}

	public void makeTopNotification(string notificationString, string subText = "", ASound notificationSound = null, float secondsToShow = 5f)
	{
		toNotify.Add(notificationString);
		subTextNot.Add(subText);
		soundToPlay.Add(notificationSound);
		if (topNotificationRunning == null)
		{
			topNotificationRunning = StartCoroutine(runTopNotification(secondsToShow));
		}
	}

	private IEnumerator runTopNotification(float secondsToShow)
	{
		topNotification.gameObject.SetActive(value: false);
		yield return null;
		while (ConversationManager.manage.IsConversationActive)
		{
			yield return null;
		}
		while (GiftedItemWindow.gifted.windowOpen)
		{
			yield return null;
		}
		while (toNotify.Count > 0)
		{
			topNotification.setText(toNotify[0], subTextNot[0]);
			topNotification.gameObject.SetActive(value: true);
			yield return null;
			topNotification.startShowText();
			if (soundToPlay[0] != null)
			{
				SoundManager.Instance.play2DSound(soundToPlay[0]);
			}
			yield return new WaitForSeconds(secondsToShow);
			float num = 0f;
			float num2 = 0f;
			if (num < 1f || num2 < 100f)
			{
				_ = num + Time.deltaTime;
				yield return null;
			}
			yield return StartCoroutine(topNotification.GetComponent<WindowAnimator>().closeWithMask());
			toNotify.RemoveAt(0);
			subTextNot.RemoveAt(0);
			soundToPlay.RemoveAt(0);
		}
		topNotification.gameObject.SetActive(value: false);
		topNotificationRunning = null;
	}
}
