using System;

[Serializable]
public class GivesBuffTypes
{
	public StatusManager.BuffType myType;

	public int myLevel;

	public int seconds;

	public void giveThisBuff(int seconds)
	{
		StatusManager.manage.addBuff(myType, seconds, myLevel);
	}
}
