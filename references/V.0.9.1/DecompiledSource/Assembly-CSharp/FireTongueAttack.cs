using System.Collections;
using UnityEngine;

public class FireTongueAttack : MonoBehaviour
{
	public GameObject tongue;

	private void OnEnable()
	{
		tongue.SetActive(value: false);
	}

	public void fireTongue()
	{
		tongue.SetActive(value: true);
		StartCoroutine(disableTongue());
	}

	private IEnumerator disableTongue()
	{
		yield return new WaitForSeconds(0.8f);
		tongue.SetActive(value: false);
	}
}
