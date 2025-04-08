using System.IO;
using Vintagestory.API.MathTools;

namespace Vintagestory.API.Common;

/// <summary>
/// This is a versatile way of allowing a collectible to change to another after a certain time in the inventory.
/// </summary>
/// <example>
/// <code language="json">
///             "transitionablePropsByType": {
///             	"*-long-raw": [
///             		{
///             			"type": "Dry",
///             			"freshHours": { "avg": 0 },
///             			"transitionHours": { "avg": 168 },
///             			"transitionedStack": {
///             				"type": "item",
///             				"code": "bowstave-long-dry"
///             			},
///             			"transitionRatio": 1
///             		}
///             	]
///             },
/// </code>
/// <code language="json">
///             "transitionableProps": [
///             	{
///             		"type": "Perish",
///             		"freshHours": { "avg": 120 },
///             		"transitionHours": { "avg": 24 },
///             		"transitionedStack": {
///             			"type": "item",
///             			"code": "rot"
///             		},
///             		"transitionRatio": 0.5
///             	}
///             ],
/// </code>
/// </example>
[DocumentAsJson]
public class TransitionableProperties
{
	/// <summary>
	/// <!--<jsonoptional>Recommended</jsonoptional><jsondefault>None</jsondefault>-->
	/// What kind of transition can it make?
	/// </summary>
	[DocumentAsJson]
	public EnumTransitionType Type = EnumTransitionType.None;

	/// <summary>
	/// <!--<jsonoptional>Recommended</jsonoptional><jsondefault>36</jsondefault>-->
	/// The amount of hours before this item starts the transitioning process.
	/// </summary>
	[DocumentAsJson]
	public NatFloat FreshHours = NatFloat.createUniform(36f, 0f);

	/// <summary>
	/// <!--<jsonoptional>Recommended</jsonoptional><jsondefault>12</jsondefault>-->
	/// The amount of hours it takes for the item to transition, after <see cref="F:Vintagestory.API.Common.TransitionableProperties.FreshHours" /> has elapsed.
	/// </summary>
	[DocumentAsJson]
	public NatFloat TransitionHours = NatFloat.createUniform(12f, 0f);

	/// <summary>
	/// <!--<jsonoptional>Required</jsonoptional>-->
	/// The itemstack the collectible turns into upon transitioning.
	/// </summary>
	[DocumentAsJson]
	public JsonItemStack TransitionedStack;

	/// <summary>
	/// <!--<jsonoptional>Optional</jsonoptional><jsondefault>1</jsondefault>-->
	/// Conversion ratio of fresh stacksize to transitioned stack size
	/// </summary>
	[DocumentAsJson]
	public float TransitionRatio = 1f;

	/// <summary>
	/// Duplicates the properties, which includes cloning the stack that was eaten.
	/// </summary>
	/// <returns></returns>
	public TransitionableProperties Clone()
	{
		return new TransitionableProperties
		{
			FreshHours = FreshHours.Clone(),
			TransitionHours = TransitionHours.Clone(),
			TransitionRatio = TransitionRatio,
			TransitionedStack = TransitionedStack?.Clone(),
			Type = Type
		};
	}

	public void ToBytes(BinaryWriter writer)
	{
		writer.Write((ushort)Type);
		FreshHours.ToBytes(writer);
		TransitionHours.ToBytes(writer);
		TransitionedStack.ToBytes(writer);
		writer.Write(TransitionRatio);
	}

	public void FromBytes(BinaryReader reader, IClassRegistryAPI instancer)
	{
		Type = (EnumTransitionType)reader.ReadUInt16();
		FreshHours = NatFloat.createFromBytes(reader);
		TransitionHours = NatFloat.createFromBytes(reader);
		TransitionedStack = new JsonItemStack();
		TransitionedStack.FromBytes(reader, instancer);
		TransitionRatio = reader.ReadSingle();
	}
}
