using Vintagestory.API.Datastructures;

namespace Vintagestory.API.Common;

public abstract class CropBehavior
{
	public Block block;

	public CropBehavior(Block block)
	{
		this.block = block;
	}

	/// <summary>
	/// Initializes the crop with additional properties.
	/// </summary>
	/// <param name="properties"></param>
	public virtual void Initialize(JsonObject properties)
	{
	}

	/// <summary>
	/// Attempts to grow the crop.
	/// </summary>
	/// <param name="api">The Core API</param>
	/// <param name="farmland">The farmland below the crop.</param>
	/// <param name="currentTotalHours"></param>
	/// <param name="newGrowthStage">The next growth stage.</param>
	/// <param name="handling">Whether or not this event was handled.</param>
	/// <returns>Whether or not the crop grew.</returns>
	public virtual bool TryGrowCrop(ICoreAPI api, IFarmlandBlockEntity farmland, double currentTotalHours, int newGrowthStage, ref EnumHandling handling)
	{
		handling = EnumHandling.PassThrough;
		return false;
	}

	/// <summary>
	/// The event fired when the crop is planted.
	/// </summary>
	/// <param name="api">The core API.</param>
	public virtual void OnPlanted(ICoreAPI api)
	{
	}
}
