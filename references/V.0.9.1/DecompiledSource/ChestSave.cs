using System;

[Serializable]
public class ChestSave
{
	public int[] itemId = new int[24];

	public int[] itemStack = new int[24];

	public int xPos;

	public int yPos;

	public int houseX = -1;

	public int houseY = -1;

	public void SaveChestDetails(Chest chestToSave)
	{
		for (int i = 0; i < 24; i++)
		{
			itemId[i] = chestToSave.itemIds[i];
			itemStack[i] = chestToSave.itemStacks[i];
		}
		xPos = chestToSave.xPos;
		yPos = chestToSave.yPos;
		houseX = chestToSave.insideX;
		houseY = chestToSave.insideY;
	}

	public void LoadChestDetails()
	{
		Chest chest = new Chest(xPos, yPos);
		chest.itemIds = itemId;
		chest.itemStacks = itemStack;
		if (houseX != -1 && houseY != -1)
		{
			chest.inside = true;
			chest.insideX = houseX;
			chest.insideY = houseY;
		}
		ContainerManager.manage.activeChests.Add(chest);
	}

	public void SaveStash()
	{
	}

	public void LoadStash()
	{
		Chest chest = new Chest(xPos, yPos);
		chest.itemIds = itemId;
		chest.itemStacks = itemStack;
		if (houseX != -1 && houseY != -1)
		{
			chest.inside = true;
			chest.insideX = houseX;
			chest.insideY = houseY;
		}
		ContainerManager.manage.privateStashes.Add(chest);
	}
}
