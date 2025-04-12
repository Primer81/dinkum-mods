using UnityEngine;

public class ToolWorksAtNight : MonoBehaviour
{
	public MeleeAttacks myAttacks;

	public void useAtNight()
	{
		if (RealWorldTimeLight.time.currentHour >= RealWorldTimeLight.time.getSunSetTime() + 1 || RealWorldTimeLight.time.currentHour <= 6 || RealWorldTimeLight.time.underGround)
		{
			myAttacks.attack();
		}
		else
		{
			myAttacks.UseStaminaOnAttackForLocalPlayer();
		}
	}
}
