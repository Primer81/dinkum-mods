using UnityEngine;

public class PrintStandingOn : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (Physics.Raycast(base.transform.position + Vector3.up / 2f, Vector3.down, out var hitInfo, 3f))
		{
			MonoBehaviour.print(hitInfo.transform.gameObject);
			MonoBehaviour.print(hitInfo.transform.root.gameObject.name);
		}
	}
}
