using UnityEngine;

public class MuseumConvoGroup : MonoBehaviour
{
	public ConversationObject noItemsGiven;

	public ConversationObject allItemsAreAlreadyDonated;

	public ConversationObject allItemsAreNew;

	public ConversationObject itemCantBeDonated;

	public ConversationObject askIfHasAnotherDonation;

	public ConversationObject notLocal;

	public ConversationObject moreThanOneItemToDonate;

	public ConversationObject moreThanOneItemToDonateButDuplicates;

	public ConversationObject askAboutPaintingInFrameNoPhotos;

	public ConversationObject askAboutPaintingInFrameWithPhotos;

	public ConversationObject askAboutEmptyFrameNoPhotos;

	public ConversationObject askAboutEmptyFrameWithPhotos;

	public ConversationObject thankForPhoto;

	public ConversationObject nonLocalAskAboutPhoto;

	public ConversationObject nonLocalAskAboutEmpty;

	public ConversationObject GetDonationConversation(bool newDonation)
	{
		if (!newDonation)
		{
			return allItemsAreAlreadyDonated;
		}
		return allItemsAreNew;
	}

	public ConversationObject GetDonationConversationOnMultipleSlots(bool doublesGiven)
	{
		if (doublesGiven)
		{
			return moreThanOneItemToDonateButDuplicates;
		}
		return moreThanOneItemToDonate;
	}
}
