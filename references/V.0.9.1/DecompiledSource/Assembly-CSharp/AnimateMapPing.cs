using UnityEngine;
using UnityEngine.UI;

public class AnimateMapPing : MonoBehaviour
{
	private Image myImage;

	private void Start()
	{
		myImage = GetComponent<Image>();
	}

	private void Update()
	{
		base.transform.localScale += new Vector3(0.1f, 0.1f, 0f);
		_ = base.transform.localScale.y;
		_ = 1.5f;
	}
}
