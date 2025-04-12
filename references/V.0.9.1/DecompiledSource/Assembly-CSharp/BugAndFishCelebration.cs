using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BugAndFishCelebration : MonoBehaviour
{
	public static BugAndFishCelebration bugAndFishCel;

	public GameObject celebrationWindow;

	public TextMeshProUGUI celebrationText;

	public bool celebrationWindowOpen;

	public ASound celebrationSound;

	public ConversationObject inventoryFullConvo;

	private WindowAnimator myAnim;

	public GameObject compPointsShown;

	public TextMeshProUGUI compPointText;

	public GameObject sparklingRewardWindow;

	public Image sparklingRewardIcon;

	private int currentSparklingReward = -1;

	private void Awake()
	{
		bugAndFishCel = this;
		myAnim = celebrationWindow.GetComponent<WindowAnimator>();
	}

	public void openWindow(int invItem)
	{
		HandleSparklingReward();
		celebrationWindowOpen = true;
		Inventory.Instance.quickSlotBar.gameObject.SetActive(value: false);
		Inventory.Instance.checkQuickSlotDesc();
		celebrationText.text = string.Format(ConversationGenerator.generate.GetToolTip("Tip_CaughtSomething"), UIAnimationManager.manage.GetItemColorTag(Inventory.Instance.allItems[invItem].getInvItemName()));
		SoundManager.Instance.play2DSound(celebrationSound);
		MenuButtonsTop.menu.closed = false;
		StartCoroutine(whileOpen(invItem));
		compPointsShown.SetActive(value: false);
	}

	private IEnumerator whileOpen(int itemId)
	{
		StartCoroutine(CameraController.control.zoomInFishOrBug());
		yield return new WaitForSeconds(0.25f);
		celebrationWindow.SetActive(value: true);
		yield return new WaitForSeconds(0.15f);
		StartCoroutine(lettersAppear());
		while (celebrationWindowOpen)
		{
			yield return null;
			if (InputMaster.input.Interact() || InputMaster.input.Use() || InputMaster.input.UICancel())
			{
				SoundManager.Instance.play2DSound(ConversationManager.manage.nextTextSound);
				yield return StartCoroutine(myAnim.closeWithMask(2f));
				closeWindow();
			}
		}
		if (!Inventory.Instance.addItemToInventory(itemId, 1, showNotification: false))
		{
			inventoryFullConvo.targetOpenings.talkingAboutItem = Inventory.Instance.allItems[itemId];
			ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, inventoryFullConvo);
		}
		Inventory.Instance.checkQuickSlotDesc();
	}

	private IEnumerator lettersAppear()
	{
		celebrationText.maxVisibleCharacters = 0;
		yield return new WaitForSeconds(0.25f);
		for (int i = 0; i < celebrationText.textInfo.characterCount + 1; i++)
		{
			if (!celebrationWindowOpen)
			{
				break;
			}
			celebrationText.maxVisibleCharacters = i;
			if (i % 3 == 0)
			{
				SoundManager.Instance.play2DSound(SoundManager.Instance.signTalk);
			}
			yield return null;
		}
	}

	public void closeWindow()
	{
		MenuButtonsTop.menu.closeButtonDelay();
		celebrationWindow.SetActive(value: false);
		celebrationWindowOpen = false;
	}

	public void openCompPoints(float showPoints)
	{
		compPointText.text = "+" + string.Format(ConversationGenerator.generate.GetDescriptionDetails("CompPoints"), showPoints);
		compPointsShown.SetActive(value: true);
	}

	public void HandleSparklingReward()
	{
		if (currentSparklingReward != -1)
		{
			sparklingRewardIcon.sprite = Inventory.Instance.allItems[currentSparklingReward].getSprite();
			sparklingRewardWindow.SetActive(value: true);
			if (!Inventory.Instance.addItemToInventory(currentSparklingReward, 1))
			{
				DropSparklingRewardIfInvFull();
			}
		}
		else
		{
			sparklingRewardWindow.SetActive(value: false);
		}
		currentSparklingReward = -1;
	}

	public void DropSparklingRewardIfInvFull()
	{
		Vector3 position = NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position;
		position.y = NetworkMapSharer.Instance.localChar.transform.position.y;
		Vector3 position2 = NetworkMapSharer.Instance.localChar.transform.position;
		NetworkMapSharer.Instance.localChar.CmdDropItem(currentSparklingReward, 1, position2, position);
	}

	public void AddSparklingReward(int itemId)
	{
		currentSparklingReward = itemId;
	}
}
