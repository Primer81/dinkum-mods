using System;
using System.Collections.Generic;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace Vintagestory.API.Common;

public interface IWorldChunk
{
	bool Empty { get; set; }

	/// <summary>
	/// Holds a reference to the current map data of this chunk column
	/// </summary>
	IMapChunk MapChunk { get; }

	/// <summary>
	/// Holds all the blockids for each coordinate, access via index: (y * chunksize + z) * chunksize + x
	/// </summary>
	IChunkBlocks Data { get; }

	/// <summary>
	/// Use <see cref="P:Vintagestory.API.Common.IWorldChunk.Data" /> instead
	/// </summary>
	[Obsolete("Use Data field")]
	IChunkBlocks Blocks { get; }

	/// <summary>
	/// Holds all the lighting data for each coordinate, access via index: (y * chunksize + z) * chunksize + x
	/// </summary>
	IChunkLight Lighting { get; }

	/// <summary>
	/// Faster (non-blocking) access to blocks at the cost of sometimes returning 0 instead of the real block. Use <see cref="P:Vintagestory.API.Common.IWorldChunk.Data" /> if you need reliable block access. Also should only be used for reading. Currently used for the particle system.
	/// </summary>
	IChunkBlocks MaybeBlocks { get; }

	/// <summary>
	/// An array holding all Entities currently residing in this chunk. This array may be larger than the amount of entities in the chunk.
	/// </summary>
	Entity[] Entities { get; }

	/// <summary>
	/// Actual count of entities in this chunk
	/// </summary>
	int EntitiesCount { get; }

	/// <summary>
	/// An array holding block Entities currently residing in this chunk. This array may be larger than the amount of block entities in the chunk.
	/// </summary>
	Dictionary<BlockPos, BlockEntity> BlockEntities { get; set; }

	/// <summary>
	/// Returns a list of a in-chunk indexed positions of all light sources in this chunk
	/// </summary>
	HashSet<int> LightPositions { get; set; }

	/// <summary>
	/// Whether this chunk got unloaded
	/// </summary>
	bool Disposed { get; }

	/// <summary>
	/// Can be used to store non-serialized mod data that is only serialized into the standard moddata dictionary on unload. This prevents the need for constant serializing/deserializing. Useful when storing large amounts of data. Is not populated on chunk load, you need to populate it with stored data yourself using GetModData()
	/// </summary>
	Dictionary<string, object> LiveModData { get; set; }

	/// <summary>
	/// Blockdata and Light might be compressed, always call this method if you want to access these
	/// </summary>
	void Unpack();

	/// <summary>
	/// Like Unpack(), except it must be used readonly: the calling code promises not to write any changes to this chunk's blocks or lighting
	/// </summary>
	bool Unpack_ReadOnly();

	/// <summary>
	/// Like Unpack_ReadOnly(), except it actually reads and returns the block ID at index<br />
	/// (Returns 0 if the chunk was disposed)
	/// </summary>
	int UnpackAndReadBlock(int index, int layer);

	/// <summary>
	/// Like Unpack_ReadOnly(), except it actually reads and returns the Light at index<br />
	/// (Returns 0 if the chunk was disposed)
	/// </summary>
	ushort Unpack_AndReadLight(int index);

	/// <summary>
	/// A version of Unpack_AndReadLight which also returns the lightSat<br />
	/// (Returns 0 if the chunk was disposed)
	/// </summary>
	ushort Unpack_AndReadLight(int index, out int lightSat);

	/// <summary>
	/// Marks this chunk as modified. If called on server side it will be stored to disk on the next autosave or during shutdown, if called on client not much happens (but it will be preserved from packing for next ~8 seconds)
	/// </summary>
	void MarkModified();

	/// <summary>
	/// Marks this chunk as recently accessed. This will prevent the chunk from getting compressed by the in-memory chunk compression algorithm
	/// </summary>
	void MarkFresh();

	/// <summary>
	/// Adds an entity to the chunk.
	/// </summary>
	/// <param name="entity">The entity to add.</param>
	void AddEntity(Entity entity);

	/// <summary>
	/// Removes an entity from the chunk.
	/// </summary>
	/// <param name="entityId">the ID for the entity</param>
	/// <returns>Whether or not the entity was removed.</returns>
	bool RemoveEntity(long entityId);

	/// <summary>
	/// Allows setting of arbitrary, permanantly stored moddata of this chunk. When set on the server before the chunk is sent to the client, the data will also be sent to the client.
	/// When set on the client the data is discarded once the chunk gets unloaded
	/// </summary>
	/// <param name="key"></param>
	/// <param name="data"></param>
	void SetModdata(string key, byte[] data);

	/// <summary>
	/// Removes the permanently stored data.
	/// </summary>
	/// <param name="key"></param>
	void RemoveModdata(string key);

	/// <summary>
	/// Retrieve arbitrary, permantly stored mod data
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	byte[] GetModdata(string key);

	/// <summary>
	/// Allows setting of arbitrary, permanantly stored moddata of this chunk. When set on the server before the chunk is sent to the client, the data will also be sent to the client.
	/// When set on the client the data is discarded once the chunk gets unloaded
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="key"></param>
	/// <param name="data"></param>
	void SetModdata<T>(string key, T data);

	/// <summary>
	/// Retrieve arbitrary, permantly stored mod data
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="key"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	T GetModdata<T>(string key, T defaultValue = default(T));

	/// <summary>
	/// Retrieve a block from this chunk ignoring ice/water layer, performs Unpack() and a modulo operation on the position arg to get a local position in the 0..chunksize range (it's your job to pick out the right chunk before calling this method)
	/// </summary>
	/// <param name="world"></param>
	/// <param name="position"></param>
	/// <returns></returns>
	Block GetLocalBlockAtBlockPos(IWorldAccessor world, BlockPos position);

	Block GetLocalBlockAtBlockPos(IWorldAccessor world, int posX, int posY, int posZ, int layer);

	/// <summary>
	/// As GetLocalBlockAtBlockPos except lock-free, use it inside paired LockForReading(true/false) calls
	/// </summary>
	/// <param name="world"></param>
	/// <param name="position"></param>
	/// <param name="layer"></param>
	/// <returns></returns>
	Block GetLocalBlockAtBlockPos_LockFree(IWorldAccessor world, BlockPos position, int layer = 0);

	/// <summary>
	/// Retrieve a block entity from this chunk
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	BlockEntity GetLocalBlockEntityAtBlockPos(BlockPos pos);

	/// <summary>
	/// Sets a decor block to the side of an existing block. Use air block (id 0) to remove a decor.<br />
	/// </summary>
	/// <param name="index3d"></param>
	/// <param name="onFace"></param>
	/// <param name="block"></param>
	/// <returns>False if there already exists a block in this position and facing</returns>
	bool SetDecor(Block block, int index3d, BlockFacing onFace);

	/// <summary>
	/// Sets a decor block to a specific sub-position on the side of an existing block. Use air block (id 0) to remove a decor.<br />
	/// </summary>
	/// <param name="block"></param>
	/// <param name="index3d"></param>
	/// <param name="faceAndSubposition"></param>
	/// <returns>False if there already exists a block in this position and facing</returns>
	bool SetDecor(Block block, int index3d, int faceAndSubposition);

	/// <summary>
	/// If allowed by a player action, removes all decors at given position and calls OnBrokenAsDecor() on all selected decors and drops the items that are returned from Block.GetDrops()
	/// </summary>
	/// <param name="world"></param>
	/// <param name="pos"></param>
	/// <param name="side">If null, all the decor blocks on all sides are removed</param>
	/// <param name="decorIndex">If not null breaks only this part of the decor for give face. Requires side to be set.</param>
	bool BreakDecor(IWorldAccessor world, BlockPos pos, BlockFacing side = null, int? decorIndex = null);

	/// <summary>
	/// Removes a decor block from given position, saves a few cpu cycles by not calculating index3d
	/// </summary>
	/// <param name="world"></param>
	/// <param name="pos"></param>
	/// <param name="index3d"></param>
	/// <param name="callOnBrokenAsDecor">When set to true it will call block.OnBrokenAsDecor(...) which is used to drop the decors of that block</param>
	void BreakAllDecorFast(IWorldAccessor world, BlockPos pos, int index3d, bool callOnBrokenAsDecor = true);

	/// <summary>
	///
	/// </summary>
	/// <param name="blockAccessor"></param>
	/// <param name="pos"></param>
	/// <returns></returns>
	Block[] GetDecors(IBlockAccessor blockAccessor, BlockPos pos);

	/// <summary>
	///
	/// </summary>
	/// <param name="blockAccessor"></param>
	/// <param name="position"></param>
	/// <returns></returns>
	Dictionary<int, Block> GetSubDecors(IBlockAccessor blockAccessor, BlockPos position);

	Block GetDecor(IBlockAccessor blockAccessor, BlockPos pos, int decorIndex);

	/// <summary>
	/// Set entire Decors for a chunk - used in Server-&gt;Client updates
	/// </summary>
	/// <param name="newDecors"></param>
	void SetDecors(Dictionary<int, Block> newDecors);

	/// <summary>
	/// Adds extra selection boxes in case a decor block is attached at given position
	/// </summary>
	/// <param name="blockAccessor"></param>
	/// <param name="pos"></param>
	/// <param name="orig"></param>
	/// <returns></returns>
	Cuboidf[] AdjustSelectionBoxForDecor(IBlockAccessor blockAccessor, BlockPos pos, Cuboidf[] orig);

	/// <summary>
	/// Only to be implemented client side
	/// </summary>
	void FinishLightDoubleBuffering();

	/// <summary>
	/// Returns the higher light absorption between solids and fluids block layers
	/// </summary>
	/// <param name="index3d"></param>
	/// <param name="blockPos"></param>
	/// <param name="blockTypes"></param>
	/// <returns></returns>
	int GetLightAbsorptionAt(int index3d, BlockPos blockPos, IList<Block> blockTypes);

	/// <summary>
	/// For bulk chunk GetBlock operations, allows the chunkDataLayers to be pre-locked for reading, instead of entering and releasing one lock per read
	/// <br />Best used mainly on the server side unless you know what you are doing.  The client-side Chunk Tesselator can need read-access to a chunk at any time so making heavy use of this would cause rendering delays on the client
	/// <br />Make sure always to call ReleaseBulkReadLock() when finished.  Use a try/finally block if necessary, and complete all read operations within 8 seconds
	/// </summary>
	void AcquireBlockReadLock();

	/// <summary>
	/// For bulk chunk GetBlock operations, allows the chunkDataLayers to be pre-locked for reading, instead of entering and releasing one lock per read
	/// <br />Make sure always to call ReleaseBulkReadLock() when finished.  Use a try/finally block if necessary, and complete all read operations within 8 seconds
	/// </summary>
	void ReleaseBlockReadLock();

	/// <summary>
	/// Free up chunk data and pool
	/// </summary>
	void Dispose();
}
