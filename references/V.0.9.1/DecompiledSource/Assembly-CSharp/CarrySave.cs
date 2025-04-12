using System;
using System.Collections.Generic;

[Serializable]
public class CarrySave
{
	public CarryableObject[] allCarryables;

	public void saveAllCarryable()
	{
		List<PickUpAndCarry> list = new List<PickUpAndCarry>();
		for (int i = 0; i < WorldManager.Instance.allCarriables.Count; i++)
		{
			if (WorldManager.Instance.allCarriables[i] != null && (bool)WorldManager.Instance.allCarriables[i].gameObject && WorldManager.Instance.allCarriables[i].IsDropOnCurrentLevel() && !WorldManager.Instance.allCarriables[i].delivered)
			{
				list.Add(WorldManager.Instance.allCarriables[i]);
			}
		}
		allCarryables = new CarryableObject[list.Count];
		for (int j = 0; j < list.Count; j++)
		{
			allCarryables[j] = new CarryableObject(list[j]);
		}
	}

	public void loadAllCarryable()
	{
		if (allCarryables != null)
		{
			for (int i = 0; i < allCarryables.Length; i++)
			{
				allCarryables[i].SpawnTheObject();
			}
		}
	}
}
