using System.Collections;
using UnityEngine;

public class ChristmasLight : MonoBehaviour
{
	public LineRenderer ren;

	public Transform[] points;

	private Vector3 worldPos;

	public static WaitForSeconds lightPlacementCheck = new WaitForSeconds(0.5f);

	private void OnEnable()
	{
		if (RealWorldEventChecker.check.getCurrentEvent() != RealWorldEventChecker.TimedEvent.Chrissy)
		{
			ren.enabled = false;
			return;
		}
		worldPos = base.transform.position;
		ren.enabled = true;
		StartCoroutine(placeLights());
	}

	public IEnumerator placeLights()
	{
		ren.positionCount = points.Length;
		for (int i = 0; i < points.Length; i++)
		{
			ren.SetPosition(i, points[i].position);
		}
		worldPos = base.transform.position;
		while (true)
		{
			if (worldPos != base.transform.position)
			{
				ren.positionCount = points.Length;
				for (int j = 0; j < points.Length; j++)
				{
					ren.SetPosition(j, points[j].position);
				}
				worldPos = base.transform.position;
			}
			yield return lightPlacementCheck;
		}
	}
}
