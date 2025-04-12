using UnityEngine;

public class WaterfallMist : MonoBehaviour
{
	public Transform moveTo;

	private void OnEnable()
	{
		Invoke("placeWaterFall", 0.1f);
	}

	public void placeWaterFall()
	{
		int num = WorldManager.Instance.heightMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)];
		moveTo.transform.position = new Vector3(base.transform.position.x, num, base.transform.position.z);
		moveTo.transform.localPosition = new Vector3(0f, 1f, moveTo.localPosition.z);
	}
}
