using UnityEngine;

public class Consumeable : MonoBehaviour
{
	public int durationSeconds = 60;

	public float staminaGain;

	public int healthGain;

	public bool givesTempPoints;

	public float tempStaminaGain;

	public int tempHealthGain;

	[Header("Special buffs-----")]
	public GivesBuffTypes[] myBuffs;

	public bool isMeat;

	public bool isFruit;

	public bool isAnimalProduct;

	public bool isVegitable;

	public bool specialSnag;

	public void giveBuffs(int seconds)
	{
		for (int i = 0; i < myBuffs.Length; i++)
		{
			myBuffs[i].giveThisBuff(seconds);
		}
	}
}
