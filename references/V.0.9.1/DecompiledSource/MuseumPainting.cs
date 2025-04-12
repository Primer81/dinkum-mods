using UnityEngine;

public class MuseumPainting : MonoBehaviour
{
	public int paintingNo;

	public MeshRenderer mesh;

	private void Start()
	{
		base.gameObject.AddComponent<InteractableObject>().isPainting = this;
	}

	public void askAboutPainting()
	{
		ConversationManager.manage.SetTalkingAboutPhotoId(paintingNo);
		ConversationManager.manage.lastConversationTarget = NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Museum);
		if (NetworkMapSharer.Instance.isServer)
		{
			if (PhotoManager.manage.savedPhotos.Count == 0)
			{
				if (PhotoManager.manage.displayedPhotos[paintingNo] != null && PhotoManager.manage.displayedPhotos[paintingNo].photoName != "Dummy")
				{
					ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, ConversationManager.manage.museumConvos.askAboutPaintingInFrameNoPhotos);
				}
				else
				{
					ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, ConversationManager.manage.museumConvos.askAboutEmptyFrameNoPhotos);
				}
			}
			else if (PhotoManager.manage.displayedPhotos[paintingNo] != null && PhotoManager.manage.displayedPhotos[paintingNo].photoName != "Dummy")
			{
				ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, ConversationManager.manage.museumConvos.askAboutPaintingInFrameWithPhotos);
			}
			else
			{
				ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, ConversationManager.manage.museumConvos.askAboutEmptyFrameWithPhotos);
			}
		}
		else if (PhotoManager.manage.displayedPhotos[paintingNo] != null && PhotoManager.manage.displayedPhotos[paintingNo].photoName != "Dummy")
		{
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, ConversationManager.manage.museumConvos.nonLocalAskAboutPhoto);
		}
		else
		{
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, ConversationManager.manage.museumConvos.nonLocalAskAboutEmpty);
		}
	}

	public bool checkIfMuseumKeeperIsAtWork()
	{
		return NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Museum).isAtWork();
	}

	public bool checkIfMuseumKeeperCanBeTalkedTo()
	{
		return NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Museum).IsValidConversationTargetForAnyPlayer();
	}

	public void updatePainting()
	{
		if (MuseumManager.manage.paintingsOnDisplay[paintingNo] != null)
		{
			mesh.materials[1].SetTexture("_MainTex", MuseumManager.manage.paintingsOnDisplay[paintingNo]);
		}
	}
}
