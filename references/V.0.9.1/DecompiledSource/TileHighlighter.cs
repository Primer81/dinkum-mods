using UnityEngine;

public class TileHighlighter : MonoBehaviour
{
	public static TileHighlighter highlight;

	public Transform cornerTopR;

	public Transform cornerTopL;

	public Transform cornerBottomR;

	public Transform cornerBottomL;

	public GameObject mainHighlighter;

	public GameObject arrow;

	public Transform centreFill;

	public Transform myChar;

	public GameObject visibleContainer;

	public Material tileHighlighter;

	private float highlighterStrength = 1f;

	private bool showingBase;

	private bool hidden;

	public Transform upTransform;

	public Transform downTransform;

	public Transform leftTransform;

	public Transform rightTransform;

	public Transform upLeftTransform;

	public Transform upRightTransform;

	public Transform followUpTrans;

	public Transform followDownTrans;

	public Transform followLeftTrans;

	public Transform followRightTrans;

	public Transform followUpLeftTrans;

	public Transform followUpRightTrans;

	public bool off;

	public void setAtHighliter()
	{
		highlight = this;
	}

	private void Update()
	{
		if (off)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			if (hidden)
			{
				return;
			}
			if (Mathf.Abs(base.transform.position.y - myChar.position.y) < 1.6f)
			{
				if (highlighterStrength >= 1.3f)
				{
					highlighterStrength = Mathf.Lerp(highlighterStrength, 1f, Time.deltaTime * 8f);
					tileHighlighter.SetFloat("_RimPower", highlighterStrength);
					visibleContainer.SetActive(value: true);
				}
			}
			else if (highlighterStrength >= 8f)
			{
				visibleContainer.SetActive(value: false);
			}
			else
			{
				highlighterStrength = Mathf.Lerp(highlighterStrength, 10f, Time.deltaTime * 8f);
				tileHighlighter.SetFloat("_RimPower", highlighterStrength);
			}
		}
	}

	public void checkIfHidden(InventoryItem item)
	{
		if (off)
		{
			hidden = true;
			base.gameObject.SetActive(value: false);
		}
		if (item == null || (bool)item.consumeable || item.hideHighlighter)
		{
			hidden = true;
			base.gameObject.SetActive(value: false);
		}
		else
		{
			hidden = false;
			base.gameObject.SetActive(value: true);
		}
	}

	public void updateSides()
	{
		if (!off)
		{
			if ((bool)leftTransform)
			{
				followLeftTrans.gameObject.SetActive(value: true);
				followLeftTrans.position = new Vector3(leftTransform.position.x, mainHighlighter.transform.position.y, leftTransform.position.z);
			}
			else
			{
				followLeftTrans.gameObject.SetActive(value: false);
			}
			if ((bool)rightTransform)
			{
				followRightTrans.gameObject.SetActive(value: true);
				followRightTrans.position = new Vector3(rightTransform.position.x, mainHighlighter.transform.position.y, rightTransform.position.z);
			}
			else
			{
				followRightTrans.gameObject.SetActive(value: false);
			}
			if ((bool)upTransform)
			{
				followUpTrans.gameObject.SetActive(value: true);
				followUpTrans.position = new Vector3(upTransform.position.x, mainHighlighter.transform.position.y, upTransform.position.z);
			}
			else
			{
				followUpTrans.gameObject.SetActive(value: false);
			}
			if ((bool)upLeftTransform)
			{
				followUpLeftTrans.gameObject.SetActive(value: true);
				followUpLeftTrans.position = new Vector3(upLeftTransform.position.x, mainHighlighter.transform.position.y, upLeftTransform.position.z);
			}
			else
			{
				followUpLeftTrans.gameObject.SetActive(value: false);
			}
			if ((bool)upRightTransform)
			{
				followUpRightTrans.gameObject.SetActive(value: true);
				followUpRightTrans.position = new Vector3(upRightTransform.position.x, mainHighlighter.transform.position.y, upRightTransform.position.z);
			}
			else
			{
				followUpRightTrans.gameObject.SetActive(value: false);
			}
		}
	}

	public void ShowNormal()
	{
		mainHighlighter.SetActive(value: true);
		centreFill.gameObject.SetActive(value: false);
		cornerBottomL.gameObject.SetActive(value: false);
		cornerBottomR.gameObject.SetActive(value: false);
		cornerTopL.gameObject.SetActive(value: false);
		cornerTopR.gameObject.SetActive(value: false);
	}

	public void showOneTileRot()
	{
		mainHighlighter.gameObject.SetActive(value: true);
		centreFill.gameObject.SetActive(value: false);
		cornerBottomL.gameObject.SetActive(value: false);
		cornerBottomR.gameObject.SetActive(value: false);
		cornerTopL.gameObject.SetActive(value: false);
		cornerTopR.gameObject.SetActive(value: false);
	}

	public void ShowMultiTiled(int xSize, int ySize)
	{
		mainHighlighter.gameObject.SetActive(value: false);
		centreFill.localScale = new Vector3(xSize, ySize, 1f);
		xSize--;
		ySize--;
		centreFill.gameObject.SetActive(value: true);
		cornerTopL.localPosition = new Vector3(-xSize, 0.51f, ySize);
		cornerTopR.localPosition = new Vector3(xSize, 0.51f, ySize);
		cornerBottomL.localPosition = new Vector3(-xSize, 0.51f, -ySize);
		cornerBottomR.localPosition = new Vector3(xSize, 0.51f, -ySize);
		cornerBottomL.gameObject.SetActive(value: true);
		cornerBottomR.gameObject.SetActive(value: true);
		cornerTopL.gameObject.SetActive(value: true);
		cornerTopR.gameObject.SetActive(value: true);
	}
}
