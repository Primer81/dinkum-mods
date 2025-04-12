using UnityEngine;

public class DestroyObjectOnDestroyed : MonoBehaviour
{
	public GameObject toDestroy;

	private void OnDestroy()
	{
		Object.Destroy(toDestroy);
	}
}
