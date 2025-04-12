using UnityEngine;

public class RainMaker : MonoBehaviour
{
	public enum ChangeWeatherType
	{
		Rain,
		Sunshine
	}

	public ReadableSign readable;

	private CharMovement myChar;

	public ASound rainMakerSound;

	public ChangeWeatherType myChange;

	private void Start()
	{
		myChar = GetComponentInParent<CharMovement>();
	}

	public void doDamageNow()
	{
		if ((bool)myChar && myChar.isLocalPlayer)
		{
			if (myChange == ChangeWeatherType.Rain)
			{
				myChar.CmdRainMaker();
			}
			else
			{
				myChar.CmdCupOfSunshine();
			}
			readable.readSign();
		}
		SoundManager.Instance.play2DSound(rainMakerSound);
		Inventory.Instance.useItemWithFuel();
	}
}
