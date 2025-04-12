using UnityEngine;

public class OpenBluePrint : MonoBehaviour
{
	private CharMovement myChar;

	public bool isTreasureMap;

	public ReadableSign readWhenRecipeNotAvaliable;

	public ConversationObject treasureFoundText;

	public ConversationObject useTreasureOnNormalIsland;

	private void Start()
	{
		myChar = GetComponentInParent<CharMovement>();
	}

	public void openBluePrintMenu()
	{
		if (!myChar || !myChar.isLocalPlayer)
		{
			return;
		}
		if (!isTreasureMap)
		{
			if (!RandomObjectGenerator.generate.giveRandomRecipeFromBluePrint())
			{
				readWhenRecipeNotAvaliable.readSign();
			}
			else
			{
				Inventory.Instance.consumeItemInHand();
			}
		}
		else if (RealWorldTimeLight.time.offIsland)
		{
			readWhenRecipeNotAvaliable.signConvo = treasureFoundText;
			readWhenRecipeNotAvaliable.readSign();
			myChar.CmdMarkTreasureOnMap();
			Inventory.Instance.consumeItemInHand();
		}
		else
		{
			readWhenRecipeNotAvaliable.signConvo = useTreasureOnNormalIsland;
			readWhenRecipeNotAvaliable.readSign();
		}
	}
}
