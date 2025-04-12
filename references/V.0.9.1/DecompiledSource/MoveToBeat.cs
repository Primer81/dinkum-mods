using UnityEngine;

public class MoveToBeat : MonoBehaviour
{
	private int qSamples = 1024;

	private float refValue = 0.1f;

	private float rmsValue;

	private float volume = 2f;

	public float scaleMultiplier = 1f;

	private float[] samples;

	private AudioSource audioToBounce;

	public bool localCharInZone;

	private Transform childTransform;

	private void Start()
	{
		audioToBounce = GetComponent<AudioSource>();
		samples = new float[qSamples];
		childTransform = base.transform.GetChild(0);
	}

	private void GetVolume()
	{
		audioToBounce.GetOutputData(samples, 0);
		float num = 0f;
		for (int i = 0; i < qSamples; i++)
		{
			num += samples[i] * samples[i];
		}
		rmsValue = Mathf.Sqrt(num / (float)qSamples);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.root == NetworkMapSharer.Instance.localChar.transform)
		{
			localCharInZone = true;
			MusicManager.manage.enterBoomBoxZone();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.transform.root == NetworkMapSharer.Instance.localChar.transform)
		{
			MusicManager.manage.exitBoomBoxZone();
			localCharInZone = false;
		}
	}

	private void OnDisable()
	{
		if (localCharInZone)
		{
			MusicManager.manage.exitBoomBoxZone();
		}
	}

	private void Update()
	{
		if (CameraController.control.IsCloseToCamera50(base.transform.position))
		{
			GetVolume();
			float num = Mathf.Clamp(1f + rmsValue * scaleMultiplier, 0.8f, 1.25f);
			childTransform.localScale = new Vector3(num, num, num);
		}
	}
}
