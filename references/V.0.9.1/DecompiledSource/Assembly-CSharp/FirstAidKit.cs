using UnityEngine;

public class FirstAidKit : MonoBehaviour
{
	private CharMovement myChar;

	public ASound healSound;

	public ASound zipSound;

	private void Start()
	{
		myChar = GetComponentInParent<CharMovement>();
	}

	public void HealNow()
	{
		if ((bool)myChar && myChar.isLocalPlayer)
		{
			myChar.CmdGiveHealthBack(15);
			Inventory.Instance.useItemWithFuel();
		}
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.healParticle, myChar.transform.position, 50);
		SoundManager.Instance.playASoundAtPoint(healSound, base.transform.position);
	}

	public void MakeZipSound()
	{
		SoundManager.Instance.playASoundAtPoint(zipSound, base.transform.position);
	}
}
