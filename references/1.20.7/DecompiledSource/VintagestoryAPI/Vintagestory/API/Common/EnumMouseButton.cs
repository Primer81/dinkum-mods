namespace Vintagestory.API.Common;

/// <summary>
/// A list of mouse buttons.
/// </summary>
[DocumentAsJson]
public enum EnumMouseButton
{
	Left = 0,
	Middle = 1,
	Right = 2,
	Button4 = 3,
	Button5 = 4,
	Button6 = 5,
	Button7 = 6,
	Button8 = 7,
	/// <summary>
	/// Used to signal to event handlers, but not actually a button: activated when the wheel is scrolled.
	/// </summary>
	Wheel = 13,
	None = 255
}
