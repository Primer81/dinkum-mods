using System;
using System.Collections.Generic;

[Serializable]
public class DropSaves
{
	public DropToSave[] savedDrops;

	public void saveDrops()
	{
		List<DroppedItem> dropsToSave = WorldManager.Instance.getDropsToSave();
		List<DropToSave> list = new List<DropToSave>();
		for (int i = 0; i < dropsToSave.Count; i++)
		{
			if (dropsToSave[i] != null)
			{
				if (dropsToSave[i].inside == null)
				{
					list.Add(new DropToSave(dropsToSave[i].myItemId, dropsToSave[i].stackAmount, dropsToSave[i].desiredPos, -1, -1));
				}
				else
				{
					list.Add(new DropToSave(dropsToSave[i].myItemId, dropsToSave[i].stackAmount, dropsToSave[i].desiredPos, dropsToSave[i].inside.xPos, dropsToSave[i].inside.yPos));
				}
			}
		}
		savedDrops = list.ToArray();
	}

	public void loadDrops()
	{
		for (int i = 0; i < savedDrops.Length; i++)
		{
			savedDrops[i].spawnDrop();
		}
	}
}
