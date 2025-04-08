using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Vintagestory.GameContent;

public class ItemDeadButterfly : Item
{
	public override string GetHeldItemName(ItemStack itemStack)
	{
		return Lang.Get("item-deadbutterfly", Lang.Get("item-creature-butterfly-" + Variant["type"]));
	}

	public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
	{
		base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
		string descLangCode = Code?.Domain + ":itemdesc-creature-butterfly-" + Variant["type"];
		dsc.AppendLine("<font color=\"#ccc\"><i>" + Lang.Get(descLangCode) + "</i></font>");
		dsc.AppendLine(Lang.Get("Butterfly"));
	}
}
