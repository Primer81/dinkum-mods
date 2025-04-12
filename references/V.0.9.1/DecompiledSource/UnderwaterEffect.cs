using System;
using UnityEngine;

public class UnderwaterEffect : MonoBehaviour
{
	public Camera mainCamera;

	public Transform quad;

	public float waterHeight = -0.6f;

	private float quadDistance = 0.5f;

	private void Start()
	{
		if (mainCamera == null)
		{
			mainCamera = Camera.main;
		}
		quad.localPosition = new Vector3(0f, 0f, quadDistance);
		AdjustQuadWidthToFitScreen();
	}

	private void Update()
	{
		AdjustQuadHeight();
	}

	private void AdjustQuadWidthToFitScreen()
	{
		float x = 2f * quadDistance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * ((float)Math.PI / 180f)) * mainCamera.aspect;
		quad.localScale = new Vector3(x, quad.localScale.y, 1f);
	}

	private void AdjustQuadHeight()
	{
		if (mainCamera.transform.position.y >= waterHeight)
		{
			quad.localScale = new Vector3(quad.localScale.x, 0f, quad.localScale.z);
			return;
		}
		Vector3 position = mainCamera.transform.position + mainCamera.transform.forward * quadDistance;
		position.y = waterHeight;
		Vector3 vector = mainCamera.WorldToScreenPoint(position);
		if (vector.y <= 0f)
		{
			float maxQuadHeight = GetMaxQuadHeight();
			quad.localScale = new Vector3(quad.localScale.x, maxQuadHeight, quad.localScale.z);
			quad.localPosition = new Vector3(quad.localPosition.x, (0f - maxQuadHeight) / 2f, quad.localPosition.z);
		}
		else
		{
			float y = Mathf.Clamp01(vector.y / (float)Screen.height) * GetMaxQuadHeight();
			quad.localScale = new Vector3(quad.localScale.x, y, quad.localScale.z);
			quad.localPosition = new Vector3(quad.localPosition.x, (0f - quad.localScale.y) / 2f, quad.localPosition.z);
		}
	}

	private float GetMaxQuadHeight()
	{
		return 2f * quadDistance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * ((float)Math.PI / 180f));
	}
}
