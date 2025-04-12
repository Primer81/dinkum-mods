using UnityEngine;

public class DonateSwapConvo : MonoBehaviour
{
	public ConversationObject acceptingDonationConvoSO;

	public ConversationObject notAcceptingDonationsSO;

	public ConversationObject GetConvo()
	{
		if (NetworkMapSharer.Instance.townDebt <= 0)
		{
			return notAcceptingDonationsSO;
		}
		return acceptingDonationConvoSO;
	}
}
