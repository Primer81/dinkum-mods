using Vintagestory.API.Common;
using Vintagestory.ServerMods.WorldEdit;

namespace Vintagestory.ServerMods;

public static class ChiselToolRegisterUtil
{
	public static void Register(ModSystem mod)
	{
		((Vintagestory.ServerMods.WorldEdit.WorldEdit)mod).RegisterTool("chiselbrush", typeof(MicroblockTool));
	}
}
