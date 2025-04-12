using UnityEngine;

public class PlayerHouseExterior : MonoBehaviour
{
	public enum houseParts
	{
		houseBase,
		door,
		window,
		roof,
		houseDetailsColor,
		fence
	}

	public int levelNo = 1;

	public GameObject[] houseLevels;

	public HousePartSelection[] roofs;

	public HousePartSelection[] bases;

	public HousePartSelection[] windows;

	public HousePartSelection[] doors;

	public Material[] houseMaterials;

	public Material[] roofMaterials;

	public Material[] wallMaterials;

	private Material houseColor;

	private Material wallColor;

	private Material roofColor;

	public GameObject[] fenceOptions;

	private bool renderersSet;

	public MeshRenderer doorMat;

	private void Start()
	{
		setUpRenderers();
	}

	public void setExterior(HouseExterior houseDetails)
	{
		setUpRenderers();
		ColorUtility.TryParseHtmlString(houseDetails.houseColor, out var color);
		ColorUtility.TryParseHtmlString(houseDetails.wallColor, out var color2);
		ColorUtility.TryParseHtmlString(houseDetails.roofColor, out var color3);
		Object.Destroy(houseColor);
		Object.Destroy(wallColor);
		Object.Destroy(roofColor);
		houseColor = Object.Instantiate(houseMaterials[houseDetails.houseMat]);
		houseColor.color = color;
		wallColor = Object.Instantiate(houseMaterials[houseDetails.wallMat]);
		wallColor.color = color2;
		roofColor = Object.Instantiate(roofMaterials[Mathf.Clamp(houseDetails.roofMat, 0, roofMaterials.Length)]);
		roofColor.color = color3;
		for (int i = 0; i < houseLevels.Length; i++)
		{
			if (i == houseDetails.houseLevel)
			{
				houseLevels[i].gameObject.SetActive(value: true);
			}
			else
			{
				houseLevels[i].gameObject.SetActive(value: false);
			}
		}
		for (int j = 0; j < roofs.Length; j++)
		{
			roofs[j].setPart(houseDetails.roof, wallColor, houseColor, roofColor);
		}
		for (int k = 0; k < bases.Length; k++)
		{
			bases[k].setPart(houseDetails.houseBase, wallColor, houseColor, roofColor);
		}
		for (int l = 0; l < windows.Length; l++)
		{
			windows[l].setPart(houseDetails.windows, wallColor, houseColor, roofColor);
		}
		for (int m = 0; m < doors.Length; m++)
		{
			doors[m].setPart(houseDetails.door, wallColor, houseColor, roofColor);
		}
		for (int n = 0; n < fenceOptions.Length; n++)
		{
			if (houseDetails.fence == n)
			{
				fenceOptions[n].SetActive(value: true);
			}
			else
			{
				fenceOptions[n].SetActive(value: false);
			}
		}
		doorMat.material = houseColor;
	}

	public void setUpRenderers()
	{
		if (!renderersSet)
		{
			for (int i = 0; i < roofs.Length; i++)
			{
				roofs[i].setUpRenderers();
			}
			for (int j = 0; j < bases.Length; j++)
			{
				bases[j].setUpRenderers();
			}
			for (int k = 0; k < windows.Length; k++)
			{
				windows[k].setUpRenderers();
			}
			for (int l = 0; l < doors.Length; l++)
			{
				doors[l].setUpRenderers();
			}
			renderersSet = true;
		}
	}
}
