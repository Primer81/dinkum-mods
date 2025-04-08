namespace Vintagestory.API.Server;

/// <summary>
/// The stages a chunk goes through when being created
/// </summary>
public enum EnumWorldGenPass
{
	/// <summary>
	/// Nothing generated yet
	/// </summary>
	None,
	/// <summary>
	/// Does not require neighbour chunks to exist. Should generates 3d rock terrain mostly. Default generators by execute order:
	/// 0 = Basic 3D Terrain (granite+rock)
	/// 0.1 = Rock Strata
	/// 0.3 = Cave generator
	/// 0.4 = Block layers (soil, gravel, sand, ice, tall grass, etc.)
	/// </summary>
	Terrain,
	/// <summary>
	/// Requires neighbour chunks. Ravines, Lakes, Boulders.  Default generators by execute order:
	/// 0.1 = Hot springs
	/// 0.2 = Deposits (Ores, Peat, Clay, etc.)
	/// 0.3 = Worldgen Structures
	/// 0.4 = Above sealevel Lakes
	/// 0.5 = Worldgen Structures Post Pass
	/// </summary>
	TerrainFeatures,
	/// <summary>
	/// Requires neighbour chunks. Default generators by execute order:
	/// 0.2 = Story structures. Creates exclusion zones for the other vegetation passes
	/// 0.5 = Block Patches, Shrubs and Trees
	/// 0.9 = Rivulets (single block water sources)
	/// 0.95 = Sunlight flooding only inside current chunk
	/// </summary>
	Vegetation,
	/// <summary>
	/// Requires neighbour chunks. Does the lighting of the chunk.
	/// 0 = Snow layer
	/// 0.95 = Sunlight flooding into neighbouring chunks
	/// </summary>
	NeighbourSunLightFlood,
	/// <summary>
	/// Requires neighbour chunks. Nothing left to generate, but neighbour chunks might still generate stuff into this chunk
	/// 0.1 = Generate creatures
	/// </summary>
	PreDone,
	/// <summary>
	/// Chunk generation complete. This pass is not triggered as an event. 
	/// </summary>
	Done
}
