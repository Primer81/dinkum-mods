using UnityEngine;

public class ReadableSign : MonoBehaviour
{
	public bool usePromptInsteadOfReadPromt;

	public ConversationObject signConvo;

	private DonateSwapConvo donate;

	private ConstructionBoxInput construct;

	public bool isCompBook;

	private void Start()
	{
		donate = GetComponent<DonateSwapConvo>();
		construct = GetComponent<ConstructionBoxInput>();
		base.gameObject.AddComponent<InteractableObject>().isSign = this;
	}

	public void readSign()
	{
		if (isCompBook)
		{
			ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, CatchingCompetitionManager.manage.getBookConvo());
		}
		if ((bool)construct)
		{
			ConversationManager.manage.donatingToBuilding = construct;
			ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, construct.getConversation());
		}
		if ((bool)donate)
		{
			ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, donate.GetConvo());
		}
		else
		{
			ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, signConvo);
		}
	}
}
