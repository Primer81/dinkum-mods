using UnityEngine;

public class CustomiseHouseZone : MonoBehaviour
{
	public ConversationObject confirmConvoSO;

	public ConversationObject confirmConvoWhenUpgrading;

	private TileObject houseCustomisation;

	private void Start()
	{
		houseCustomisation = GetComponentInParent<TileObject>();
		base.gameObject.AddComponent<InteractableObject>().customiseHouse = this;
	}

	public void useCustomisationKit()
	{
		HouseEditor.edit.setCurrentlyEditing(houseCustomisation.xPos, houseCustomisation.yPos);
		if (WorldManager.Instance.onTileStatusMap[houseCustomisation.xPos, houseCustomisation.yPos] != 1)
		{
			ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, confirmConvoSO);
		}
		else
		{
			ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, confirmConvoWhenUpgrading);
		}
	}
}
