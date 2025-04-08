using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent.Mechanics;

public interface IMechanicalPowerRenderable
{
	float AngleRad { get; }

	Block Block { get; }

	BlockPos Position { get; }

	Vec4f LightRgba { get; }

	int[] AxisSign { get; }

	CompositeShape Shape { get; }
}
