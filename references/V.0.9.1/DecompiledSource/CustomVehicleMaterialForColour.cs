using UnityEngine;

public class CustomVehicleMaterialForColour : MonoBehaviour
{
	public Material[] colours;

	public MeshRenderer myRen;

	public void changeToNewColour(int newColour)
	{
		myRen.sharedMaterial = colours[newColour];
	}
}
