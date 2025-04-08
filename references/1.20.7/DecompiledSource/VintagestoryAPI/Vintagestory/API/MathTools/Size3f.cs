namespace Vintagestory.API.MathTools;

/// <summary>
/// Represents a vector of 3 floats. Go bug Tyron of you need more utility methods in this class.
/// </summary>
[DocumentAsJson]
public class Size3f
{
	/// <summary>
	/// <!--<jsonoptional>Recommended</jsonoptional><jsondefault>0</jsondefault>-->
	/// The X-dimension of this size.
	/// </summary>
	[DocumentAsJson]
	public float Width;

	/// <summary>
	/// <!--<jsonoptional>Recommended</jsonoptional><jsondefault>0</jsondefault>-->
	/// The Y-dimension for this size.
	/// </summary>
	[DocumentAsJson]
	public float Height;

	/// <summary>
	/// <!--<jsonoptional>Recommended</jsonoptional><jsondefault>0</jsondefault>-->
	/// The Z-dimension for this size.
	/// </summary>
	[DocumentAsJson]
	public float Length;

	public Size3f()
	{
	}

	public Size3f(float width, float height, float length)
	{
		Width = width;
		Height = height;
		Length = length;
	}

	public Size3f Clone()
	{
		return new Size3f(Width, Height, Length);
	}

	public bool CanContain(Size3f obj)
	{
		if (Width >= obj.Width && Height >= obj.Height)
		{
			return Length >= obj.Length;
		}
		return false;
	}
}
