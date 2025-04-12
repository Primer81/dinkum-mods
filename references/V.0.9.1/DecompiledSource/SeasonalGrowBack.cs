using System;

[Serializable]
public class SeasonalGrowBack
{
	public BiomSpawnTable summerGrowback;

	public BiomSpawnTable autumnGrowback;

	public BiomSpawnTable winterGrowback;

	public BiomSpawnTable springGrowback;

	public BiomSpawnTable getThisSeasonsTable()
	{
		if (WorldManager.Instance.month == 1)
		{
			return summerGrowback;
		}
		if (WorldManager.Instance.month == 2)
		{
			return autumnGrowback;
		}
		if (WorldManager.Instance.month == 3)
		{
			return winterGrowback;
		}
		if (WorldManager.Instance.month == 4)
		{
			return springGrowback;
		}
		return summerGrowback;
	}
}
