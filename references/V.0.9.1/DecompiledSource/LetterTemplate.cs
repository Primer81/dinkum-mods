using UnityEngine;

[CreateAssetMenu(fileName = "New_Letter", menuName = "LetterTemplate")]
public class LetterTemplate : ScriptableObject
{
	public enum rewardType
	{
		Furniture,
		Clothing,
		WallPaperOrFlooring
	}

	public InventoryItem gift;

	public InventoryItemLootTable giftFromTable;

	public bool useRandomFromType;

	public rewardType randomFromType;

	public int stackOfGift = 1;

	[TextArea(10, 10)]
	public string letterText;

	public string signOff = "From,";

	public int getRandomGiftFromType()
	{
		int result = Random.Range(0, Inventory.Instance.allItems.Length);
		if (randomFromType == rewardType.Furniture)
		{
			result = RandomObjectGenerator.generate.getRandomFurniture().getItemId();
		}
		else if (randomFromType == rewardType.Clothing)
		{
			result = RandomObjectGenerator.generate.getRandomClothing();
		}
		else if (randomFromType == rewardType.WallPaperOrFlooring)
		{
			result = ((Random.Range(0, 2) != 0) ? RandomObjectGenerator.generate.getRandomFlooring().getItemId() : RandomObjectGenerator.generate.getRandomWallPaper().getItemId());
		}
		return result;
	}

	public string GetLetterText()
	{
		string text = ConversationGenerator.generate.GetLetterText(base.name);
		if (text == null || text == "")
		{
			return letterText;
		}
		return text;
	}
}
