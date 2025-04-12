using UnityEngine;

public class TuckshopSpecialBoard : MonoBehaviour
{
	public SpriteRenderer image;

	public ConversationObject signConvo;

	private void Start()
	{
		TuckshopManager.manage.changeSpecialItemBoard.AddListener(setNewSpecial);
	}

	private void OnEnable()
	{
		setNewSpecial();
	}

	public void setNewSpecial()
	{
		image.sprite = Inventory.Instance.allItems[TuckshopManager.manage.specialItemId].getSprite();
		signConvo.targetOpenings.talkingAboutItem = Inventory.Instance.allItems[TuckshopManager.manage.specialItemId];
	}
}
