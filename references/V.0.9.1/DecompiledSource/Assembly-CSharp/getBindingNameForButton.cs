using TMPro;
using UnityEngine;

public class getBindingNameForButton : MonoBehaviour
{
	public TextMeshProUGUI buttonText;

	public Input_Rebind.RebindType type;

	public int bindingIndex = 1;

	public void OnEnable()
	{
		buttonText.text = Input_Rebind.rebind.getKeyBindingForInGame(type, bindingIndex);
	}
}
