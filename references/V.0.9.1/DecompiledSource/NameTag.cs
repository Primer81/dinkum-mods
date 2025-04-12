using TMPro;
using UnityEngine;

public class NameTag : MonoBehaviour
{
	public TextMeshPro nameText;

	public bool isFishTag;

	public MeshRenderer nameTextRen;

	public MeshRenderer boxTextRen;

	public void turnOn(string newText)
	{
		nameText.text = newText;
		base.gameObject.SetActive(value: true);
	}

	public void turnOff()
	{
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (!isFishTag)
		{
			base.transform.LookAt(CameraController.control.cameraTrans);
			return;
		}
		base.transform.LookAt(CameraController.control.cameraTrans);
		base.transform.position = new Vector3(base.transform.position.x, 1f, base.transform.position.z);
	}

	public void enableMeshes()
	{
		nameTextRen.enabled = true;
		boxTextRen.enabled = true;
	}
}
