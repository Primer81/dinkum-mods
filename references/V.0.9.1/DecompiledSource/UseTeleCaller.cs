using UnityEngine;

public class UseTeleCaller : MonoBehaviour
{
	private CharMovement isChar;

	public ConversationObject teleporterDoesntWorkConvo;

	public ConversationObject useTeleJumper;

	public ConversationObject noNetwork;

	public ConversationObject noSignal;

	public ConversationObject yesSignal;

	public ConversationObject creatingNewSignal;

	public ConversationObject movingOldSignal;

	public ConversationObject insufficientFunds;

	public bool debugTele;

	private void Start()
	{
		isChar = GetComponentInParent<CharMovement>();
	}

	public void doDamageNow()
	{
		if (debugTele)
		{
			if ((bool)isChar && isChar.isLocalPlayer)
			{
				RenderMap.Instance.canTele = true;
				RenderMap.Instance.debugTeleport = true;
				MenuButtonsTop.menu.switchToMap();
			}
		}
		else
		{
			if (!isChar || !isChar.isLocalPlayer)
			{
				return;
			}
			if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.offIsland || TeleTowerCount() == 0 || isChar.myInteract.IsInsidePlayerHouse)
			{
				ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, noNetwork);
				return;
			}
			if (TeleSignal.currentSignal != null)
			{
				useTeleJumper.targetResponses[1].branchToConversation = yesSignal;
				useTeleJumper.targetResponses[2].branchToConversation = movingOldSignal;
			}
			else
			{
				useTeleJumper.targetResponses[1].branchToConversation = noSignal;
				useTeleJumper.targetResponses[2].branchToConversation = creatingNewSignal;
			}
			ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, useTeleJumper);
		}
	}

	public bool moreThanOneTeleOn()
	{
		int num = 0;
		if (NetworkMapSharer.Instance.privateTowerPos != Vector2.zero)
		{
			num++;
		}
		if (NetworkMapSharer.Instance.northOn)
		{
			num++;
		}
		if (NetworkMapSharer.Instance.eastOn)
		{
			num++;
		}
		if (NetworkMapSharer.Instance.southOn)
		{
			num++;
		}
		if (NetworkMapSharer.Instance.westOn)
		{
			num++;
		}
		return num > 1;
	}

	public int TeleTowerCount()
	{
		int num = 0;
		if (NetworkMapSharer.Instance.privateTowerPos != Vector2.zero)
		{
			num++;
		}
		if (NetworkMapSharer.Instance.northOn)
		{
			num++;
		}
		if (NetworkMapSharer.Instance.eastOn)
		{
			num++;
		}
		if (NetworkMapSharer.Instance.southOn)
		{
			num++;
		}
		if (NetworkMapSharer.Instance.westOn)
		{
			num++;
		}
		return num;
	}
}
