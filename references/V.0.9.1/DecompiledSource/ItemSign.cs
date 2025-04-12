using UnityEngine;

public class ItemSign : MonoBehaviour
{
	public SpriteRenderer itemRenderer;

	public bool isSilo;

	public InventoryItem itemCanPlaceIn;

	private void Start()
	{
		base.gameObject.AddComponent<InteractableObject>().isItemSign = this;
	}

	public void updateStatus(int newStatus)
	{
		if (newStatus < 0)
		{
			itemRenderer.sprite = null;
		}
		else
		{
			itemRenderer.sprite = Inventory.Instance.allItems[newStatus].getSprite();
		}
	}
}
