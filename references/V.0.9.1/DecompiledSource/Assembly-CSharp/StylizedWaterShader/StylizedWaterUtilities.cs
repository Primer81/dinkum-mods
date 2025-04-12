using UnityEngine;

namespace StylizedWaterShader;

public static class StylizedWaterUtilities
{
	public static class CameraUtils
	{
		public static Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign, float clipPlaneOffset)
		{
			Vector3 point = pos + normal * clipPlaneOffset;
			Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
			Vector3 lhs = worldToCameraMatrix.MultiplyPoint(point);
			Vector3 rhs = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
			return new Vector4(rhs.x, rhs.y, rhs.z, 0f - Vector3.Dot(lhs, rhs));
		}

		public static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
		{
			reflectionMat.m00 = 1f - 2f * plane[0] * plane[0];
			reflectionMat.m01 = -2f * plane[0] * plane[1];
			reflectionMat.m02 = -2f * plane[0] * plane[2];
			reflectionMat.m03 = -2f * plane[3] * plane[0];
			reflectionMat.m10 = -2f * plane[1] * plane[0];
			reflectionMat.m11 = 1f - 2f * plane[1] * plane[1];
			reflectionMat.m12 = -2f * plane[1] * plane[2];
			reflectionMat.m13 = -2f * plane[3] * plane[1];
			reflectionMat.m20 = -2f * plane[2] * plane[0];
			reflectionMat.m21 = -2f * plane[2] * plane[1];
			reflectionMat.m22 = 1f - 2f * plane[2] * plane[2];
			reflectionMat.m23 = -2f * plane[3] * plane[2];
			reflectionMat.m30 = 0f;
			reflectionMat.m31 = 0f;
			reflectionMat.m32 = 0f;
			reflectionMat.m33 = 1f;
		}

		public static void CopyCameraSettings(Camera src, Camera dest)
		{
			if (!(dest == null))
			{
				dest.clearFlags = src.clearFlags;
				dest.backgroundColor = src.backgroundColor;
				dest.farClipPlane = src.farClipPlane;
				dest.nearClipPlane = src.nearClipPlane;
				dest.fieldOfView = src.fieldOfView;
				dest.aspect = src.aspect;
				dest.orthographic = src.orthographic;
				dest.orthographicSize = src.orthographicSize;
				dest.renderingPath = src.renderingPath;
				dest.targetDisplay = src.targetDisplay;
			}
		}
	}

	public static bool DEBUG;

	public static string[] ComposeDropdown(Texture2D[] resource, string replaceFilter)
	{
		string[] array = new string[resource.Length + 1];
		for (int i = 0; i < resource.Length; i++)
		{
			if (resource[i] == null)
			{
				array[i] = "(Missing)";
			}
			else
			{
				array[i] = resource[i].name.Replace(replaceFilter, string.Empty);
			}
		}
		array[resource.Length] = "Custom...";
		return array;
	}

	public static bool IsApproximatelyEqual(float a, float b)
	{
		return Mathf.Abs(a - b) < 0.05f;
	}

	public static bool HasVertexColors(Mesh mesh)
	{
		Color[] colors = mesh.colors;
		bool result = false;
		Color[] array = colors;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != Color.clear)
			{
				result = true;
				break;
			}
		}
		if (DEBUG)
		{
			Debug.Log("Mesh: " + mesh.name + " has vertex colors: " + result);
		}
		return result;
	}
}
