using System;

[Serializable]
public class BuriedSave
{
	public BuriedItem[] buriedByPlayer = new BuriedItem[0];

	public void saveBuriedItems()
	{
		buriedByPlayer = BuriedManager.manage.allBuriedItems.ToArray();
	}

	public void loadBuriedItems()
	{
		if (buriedByPlayer != null)
		{
			for (int i = 0; i < buriedByPlayer.Length; i++)
			{
				BuriedManager.manage.allBuriedItems.Add(buriedByPlayer[i]);
			}
		}
	}
}
