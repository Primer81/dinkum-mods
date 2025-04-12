using UnityEngine;

public class StartMovingLightsOnEnabled : MonoBehaviour
{
	private void OnEnable()
	{
		RealWorldEventChecker.check.startMovingLights();
	}
}
