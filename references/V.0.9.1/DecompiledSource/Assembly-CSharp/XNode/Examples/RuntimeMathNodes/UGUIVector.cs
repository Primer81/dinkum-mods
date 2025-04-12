using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Examples.RuntimeMathNodes;

public class UGUIVector : UGUIMathBaseNode
{
	public InputField valX;

	public InputField valY;

	public InputField valZ;

	private Vector vectorNode;

	public override void Start()
	{
		base.Start();
		vectorNode = node as Vector;
		valX.onValueChanged.AddListener(OnChangeValX);
		valY.onValueChanged.AddListener(OnChangeValY);
		valZ.onValueChanged.AddListener(OnChangeValZ);
		UpdateGUI();
	}

	public override void UpdateGUI()
	{
		NodePort inputPort = node.GetInputPort("x");
		NodePort inputPort2 = node.GetInputPort("y");
		NodePort inputPort3 = node.GetInputPort("z");
		valX.gameObject.SetActive(!inputPort.IsConnected);
		valY.gameObject.SetActive(!inputPort2.IsConnected);
		valZ.gameObject.SetActive(!inputPort3.IsConnected);
		Vector vector = node as Vector;
		valX.text = vector.x.ToString();
		valY.text = vector.y.ToString();
		valZ.text = vector.z.ToString();
	}

	private void OnChangeValX(string val)
	{
		vectorNode.x = float.Parse(valX.text);
	}

	private void OnChangeValY(string val)
	{
		vectorNode.y = float.Parse(valY.text);
	}

	private void OnChangeValZ(string val)
	{
		vectorNode.z = float.Parse(valZ.text);
	}
}
