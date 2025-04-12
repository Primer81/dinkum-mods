using UnityEngine;

public class TurnOnObjectOnEnable : MonoBehaviour
{
	public GameObject toBeDisabledOrEnabled;

	private void OnDisable()
	{
		toBeDisabledOrEnabled.SetActive(value: false);
	}

	private void OnEnable()
	{
		toBeDisabledOrEnabled.SetActive(value: true);
	}
}
