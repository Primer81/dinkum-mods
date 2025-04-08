using Vintagestory.API.Common;

namespace Vintagestory.GameContent;

public class BlockGlacierIce : Block
{
	public override bool ShouldMergeFace(int facingIndex, Block neighbourblock, int intraChunkIndex3d)
	{
		return this == neighbourblock;
	}
}
