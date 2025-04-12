using UnityEngine;

public class AppearOnCarrialbeTouch : MonoBehaviour
{
	public LayerMask pickUpLayer;

	public GameObject appearWhenTouch;

	public BoxCollider myCollider;

	private void OnEnable()
	{
		CheckAndShow();
	}

	private void OnTriggerEnter(Collider other)
	{
		CheckAndShow();
	}

	private void OnTriggerExit(Collider other)
	{
		CheckAndShow();
	}

	private void CheckAndShow()
	{
		appearWhenTouch.SetActive(Physics.CheckBox(base.transform.position, myCollider.size / 2f, Quaternion.identity, pickUpLayer));
	}
}
