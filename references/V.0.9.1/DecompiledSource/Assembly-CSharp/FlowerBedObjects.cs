using System.Collections;
using UnityEngine;

public class FlowerBedObjects : MonoBehaviour
{
	public GameObject[] flowersForFlowerbedGameObject;

	private int showing;

	public void SetFlowerBedObject(int flowerId)
	{
		if (showing != flowerId)
		{
			flowersForFlowerbedGameObject[showing].SetActive(value: false);
			flowersForFlowerbedGameObject[Mathf.Clamp(flowerId, 0, flowersForFlowerbedGameObject.Length - 1)].SetActive(value: true);
			StartCoroutine(AnimateAppear());
			showing = Mathf.Clamp(flowerId, 0, flowersForFlowerbedGameObject.Length - 1);
		}
	}

	private IEnumerator AnimateAppear()
	{
		if (CameraController.control.IsCloseToCamera(base.transform.position))
		{
			float journey = 0f;
			float duration = 0.35f;
			while (journey <= duration)
			{
				journey += Time.deltaTime;
				float time = Mathf.Clamp01(journey / duration);
				float t = UIAnimationManager.manage.windowsOpenCurve.Evaluate(time);
				float num = Mathf.LerpUnclamped(0.25f, 1f, t);
				float y = Mathf.LerpUnclamped(0.1f, 1f, t);
				base.transform.localScale = new Vector3(num, y, num);
				yield return null;
			}
		}
	}
}
