using UnityEngine;

public class moveToWaterLevel : MonoBehaviour
{
	private void OnEnable()
	{
		ResetToWaterLevel();
	}

	public void ResetToWaterLevel()
	{
		base.transform.position = new Vector3(base.transform.position.x, 0.6f, base.transform.position.z);
	}
}
