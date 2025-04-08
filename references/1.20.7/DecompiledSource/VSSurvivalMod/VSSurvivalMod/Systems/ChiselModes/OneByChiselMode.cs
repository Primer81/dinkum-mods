using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace VSSurvivalMod.Systems.ChiselModes;

public class OneByChiselMode : ChiselMode
{
	public override int ChiselSize => 1;

	public override DrawSkillIconDelegate DrawAction(ICoreClientAPI capi)
	{
		return ItemClay.Drawcreate1_svg;
	}
}
