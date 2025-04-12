using UnityEngine;

public class WorkTable : MonoBehaviour
{
	public string workTableName = "Table";

	public InventoryItem tableNameItem;

	public CraftingManager.CraftingMenuType typeOfCrafting;

	public ReadableSign tableText;

	public InventoryItem itemNeeded;

	public ConversationObject noItemConvo;

	public ConversationObject hasItemConvo;

	private void Start()
	{
		base.gameObject.AddComponent<InteractableObject>().isWorkTable = this;
	}

	public void checkForItemAndChangeText()
	{
		if (Inventory.Instance.getAmountOfItemInAllSlots(itemNeeded.getItemId()) > 0)
		{
			tableText.signConvo = hasItemConvo;
		}
		else
		{
			tableText.signConvo = noItemConvo;
		}
	}

	public string GetWorkTableName()
	{
		if ((bool)tableNameItem)
		{
			return tableNameItem.getInvItemName();
		}
		if (workTableName.Contains("Tip_"))
		{
			return ConversationGenerator.generate.GetToolTip(workTableName);
		}
		return workTableName;
	}
}
