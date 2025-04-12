using UnityEngine;

public class CopyPositionOnEnabled : MonoBehaviour
{
	public Transform copyMe;

	private void OnDisable()
	{
		copyMe.localPosition = base.transform.localPosition;
	}
}
