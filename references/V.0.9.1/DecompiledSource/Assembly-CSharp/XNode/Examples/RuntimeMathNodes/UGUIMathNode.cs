using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Examples.RuntimeMathNodes;

public class UGUIMathNode : UGUIMathBaseNode
{
	public InputField valA;

	public InputField valB;

	public Dropdown dropDown;

	private MathNode mathNode;

	public override void Start()
	{
		base.Start();
		mathNode = node as MathNode;
		valA.onValueChanged.AddListener(OnChangeValA);
		valB.onValueChanged.AddListener(OnChangeValB);
		dropDown.onValueChanged.AddListener(OnChangeDropdown);
		UpdateGUI();
	}

	public override void UpdateGUI()
	{
		NodePort inputPort = node.GetInputPort("a");
		NodePort inputPort2 = node.GetInputPort("b");
		valA.gameObject.SetActive(!inputPort.IsConnected);
		valB.gameObject.SetActive(!inputPort2.IsConnected);
		valA.text = mathNode.a.ToString();
		valB.text = mathNode.b.ToString();
		dropDown.value = (int)mathNode.mathType;
	}

	private void OnChangeValA(string val)
	{
		mathNode.a = float.Parse(valA.text);
	}

	private void OnChangeValB(string val)
	{
		mathNode.b = float.Parse(valB.text);
	}

	private void OnChangeDropdown(int val)
	{
		mathNode.mathType = (MathNode.MathType)val;
	}
}
