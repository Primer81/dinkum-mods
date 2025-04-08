using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent;

public interface IAcceptsDecor
{
	bool CanAccept(Block decorBlock);

	void SetDecor(Block decorBlock, BlockFacing face);

	int GetDecor(BlockFacing face);
}
