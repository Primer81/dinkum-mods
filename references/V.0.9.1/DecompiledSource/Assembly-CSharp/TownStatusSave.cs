using System;

[Serializable]
internal class TownStatusSave
{
	public float[] beautyLevels = new float[6];

	public int moneySpentInTownTotal;

	public int townDebt;

	public int paidDebt;

	public int currentWish;

	public void saveTownStatus()
	{
		beautyLevels = TownManager.manage.beautyLevels;
		moneySpentInTownTotal = TownManager.manage.moneySpentInTownTotal;
		townDebt = NetworkMapSharer.Instance.townDebt;
		currentWish = NetworkMapSharer.Instance.wishManager.currentWishType;
	}

	public void loadTownStatus()
	{
		TownManager.manage.beautyLevels = beautyLevels;
		TownManager.manage.moneySpentInTownTotal = moneySpentInTownTotal;
		NetworkMapSharer.Instance.NetworktownDebt = townDebt;
		NetworkMapSharer.Instance.wishManager.NetworkcurrentWishType = currentWish;
	}
}
