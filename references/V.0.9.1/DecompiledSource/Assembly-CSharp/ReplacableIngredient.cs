using System;

[Serializable]
public class ReplacableIngredient
{
	public InventoryItem replaceableItem;

	public InventoryItem[] replaceableWith;

	public string ReplacedIngrediantName;
}
