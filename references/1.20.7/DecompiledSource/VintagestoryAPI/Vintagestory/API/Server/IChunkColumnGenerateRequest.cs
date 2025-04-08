using Vintagestory.API.Datastructures;

namespace Vintagestory.API.Server;

public interface IChunkColumnGenerateRequest
{
	IServerChunk[] Chunks { get; }

	int ChunkX { get; }

	int ChunkZ { get; }

	ITreeAttribute ChunkGenParams { get; }

	ushort[][] NeighbourTerrainHeight { get; }

	bool RequiresChunkBorderSmoothing { get; }
}
