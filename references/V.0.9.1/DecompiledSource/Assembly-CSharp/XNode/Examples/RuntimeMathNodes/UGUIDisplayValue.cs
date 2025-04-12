using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Examples.RuntimeMathNodes;

public class UGUIDisplayValue : UGUIMathBaseNode
{
	public Text label;

	private void Update()
	{
		object inputValue = (node as DisplayValue).GetInputValue<object>("input");
		if (inputValue != null)
		{
			label.text = inputValue.ToString();
		}
		else
		{
			label.text = "n/a";
		}
	}
}
