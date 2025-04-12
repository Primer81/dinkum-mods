using UnityEngine;

public class KeyboardOrControlTutorial : MonoBehaviour
{
	public GameObject keyboard;

	public GameObject controller;

	private void OnEnable()
	{
		if (Inventory.Instance.usingMouse)
		{
			keyboard.SetActive(value: true);
			controller.SetActive(value: false);
		}
		else
		{
			keyboard.SetActive(value: false);
			controller.SetActive(value: true);
		}
	}
}
