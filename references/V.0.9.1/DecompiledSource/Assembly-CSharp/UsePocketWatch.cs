using UnityEngine;

public class UsePocketWatch : MonoBehaviour
{
	public ASound soundOnUse;

	public bool slowsTime;

	private CharMovement myChar;

	public ConversationObject watchDoesntWork;

	private void Start()
	{
		myChar = GetComponentInParent<CharMovement>();
	}

	public void usePocketWatch()
	{
		SoundManager.Instance.playASoundAtPoint(soundOnUse, base.transform.position);
		if (!myChar || !myChar.isLocalPlayer)
		{
			return;
		}
		if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.offIsland)
		{
			ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, watchDoesntWork);
		}
		else if (slowsTime)
		{
			if (RealWorldTimeLight.time.getCurrentSpeed() == 2f)
			{
				myChar.CmdChangeClockTickSpeed(4f);
			}
			else if (RealWorldTimeLight.time.getCurrentSpeed() != 2f)
			{
				myChar.CmdChangeClockTickSpeed(2f);
			}
		}
		else if (RealWorldTimeLight.time.getCurrentSpeed() == 2f)
		{
			myChar.CmdChangeClockTickSpeed(0.05f);
		}
		else if (RealWorldTimeLight.time.getCurrentSpeed() != 2f)
		{
			myChar.CmdChangeClockTickSpeed(2f);
		}
	}
}
