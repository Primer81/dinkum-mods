using UnityEngine;

public class MoveToMineRoof : MonoBehaviour
{
	private void OnEnable()
	{
		Invoke("moveDelay", Random.Range(0.01f, 0.1f));
	}

	private void moveDelay()
	{
		base.transform.position = new Vector3(base.transform.position.x, 9.5f, base.transform.position.z);
	}
}
