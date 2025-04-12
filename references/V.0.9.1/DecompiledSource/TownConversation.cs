using UnityEngine;

public class TownConversation : MonoBehaviour
{
	public ConversationObject openingConversation;

	[Header("Options-----")]
	public ConversationObject noDeedsAvalible;

	public ConversationObject payOffTownDebtFirst;

	public ConversationObject openDeedConvo;

	public ConversationObject noRoomForDeeds;

	[Header("Confirmation-----")]
	public ConversationObject confirmDeedConvo;

	public ConversationObject closeDeedWindowConvo;

	[Header("Not a local -----")]
	public ConversationObject notALocalConvo;

	public ConversationObject openingConversationSO;

	[Header("Options-----")]
	public ConversationObject noDeedsAvalibleSO;

	public ConversationObject payOffTownDebtFirstSO;

	public ConversationObject openDeedConvoSO;

	public ConversationObject noRoomForDeedsSO;

	[Header("Confirmation-----")]
	public ConversationObject confirmDeedConvoSO;

	public ConversationObject closeDeedWindowConvoSO;

	[Header("Not a local -----")]
	public ConversationObject notALocalConvoSO;

	public ConversationObject AskAboutDeeds()
	{
		if (!NetworkMapSharer.Instance.isServer)
		{
			return notALocalConvo;
		}
		if (NetworkMapSharer.Instance.townDebt > 0)
		{
			return payOffTownDebtFirst;
		}
		if (!DeedManager.manage.checkIfDeedsAvaliable())
		{
			return noDeedsAvalible;
		}
		if (!Inventory.Instance.checkIfItemCanFit(0, 1))
		{
			return noRoomForDeeds;
		}
		return openDeedConvo;
	}
}
