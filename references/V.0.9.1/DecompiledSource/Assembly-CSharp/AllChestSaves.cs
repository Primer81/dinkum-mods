using System;

[Serializable]
public class AllChestSaves
{
	public ChestSave[] allChests = new ChestSave[0];

	public void SaveAllChests()
	{
		allChests = new ChestSave[ContainerManager.manage.activeChests.Count];
		for (int i = 0; i < ContainerManager.manage.activeChests.Count; i++)
		{
			ChestSave chestSave = new ChestSave();
			chestSave.SaveChestDetails(ContainerManager.manage.activeChests[i]);
			allChests[i] = chestSave;
		}
	}

	public void LoadAllChests()
	{
		if (allChests != null)
		{
			for (int i = 0; i < allChests.Length; i++)
			{
				allChests[i].LoadChestDetails();
			}
		}
	}
}
