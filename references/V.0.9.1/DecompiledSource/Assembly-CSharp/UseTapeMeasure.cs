using UnityEngine;

public class UseTapeMeasure : MonoBehaviour
{
	private CharMovement myChar;

	public ASound tapeMeasureSound;

	private void Start()
	{
		myChar = GetComponentInParent<CharMovement>();
	}

	public void doDamageNow()
	{
		if ((bool)myChar && myChar.isLocalPlayer)
		{
			TapeMeasureManager.manage.useTapeMeasure();
		}
		SoundManager.Instance.playASoundAtPoint(tapeMeasureSound, base.transform.position);
	}

	public void OnDisable()
	{
		if ((bool)myChar && myChar.isLocalPlayer && TapeMeasureManager.manage.isCurrentlyMeasuring())
		{
			TapeMeasureManager.manage.clearTapeMeasure();
		}
	}
}
