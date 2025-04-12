using System.Collections;
using TMPro;
using UnityEngine;

public class AdjustFontSize : MonoBehaviour
{
	public TextMeshProUGUI textBox;

	public float defaultTextSize = 25f;

	private void OnEnable()
	{
		textBox.fontSize = defaultTextSize;
		StartCoroutine(CheckSizeAndChange());
	}

	private IEnumerator CheckSizeAndChange()
	{
		yield return null;
		textBox.ForceMeshUpdate();
		while (textBox.preferredWidth > 330f && textBox.fontSize > 10f)
		{
			textBox.fontSize -= 1f;
			textBox.ForceMeshUpdate();
			yield return null;
		}
	}
}
