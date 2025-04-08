using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent;

public abstract class EntityParticle : ParticleBase
{
	private byte dirNormalizedX;

	private byte dirNormalizedY;

	private byte dirNormalizedZ;

	protected bool SwimOnLiquid;

	public abstract string Type { get; }

	protected float Size { get; set; } = 1f;


	protected float GravityStrength { get; set; } = 1f;


	public void OnSpawned(ParticlePhysics physicsSim)
	{
		lightrgbs = physicsSim.BlockAccess.GetLightRGBsAsInt((int)Position.X, (int)Position.Y, (int)Position.Z);
	}

	public override void TickNow(float dt, float physicsdt, ICoreClientAPI api, ParticlePhysics physicsSim)
	{
		motion.Set(Velocity.X * dt, Velocity.Y * dt, Velocity.Z * dt);
		float s = Size / 4f;
		updatePositionWithCollision(dt, api, physicsSim, s);
		Velocity.Y -= GravityStrength * dt;
		physicsSim.HandleBoyancy(Position, Velocity, SwimOnLiquid, GravityStrength, dt, s);
		tickCount++;
		if (tickCount > 2)
		{
			doSlowTick(physicsSim, dt * 3f);
		}
	}

	protected virtual void doSlowTick(ParticlePhysics physicsSim, float dt)
	{
		lightrgbs = physicsSim.BlockAccess.GetLightRGBsAsInt((int)Position.X, (int)Position.Y, (int)Position.Z);
		tickCount = 0;
		float len = Velocity.Length();
		dirNormalizedX = (byte)(Velocity.X / len * 128f);
		dirNormalizedY = (byte)(Velocity.Y / len * 128f);
		dirNormalizedZ = (byte)(Velocity.Z / len * 128f);
	}

	public override void UpdateBuffers(MeshData buffer, Vec3d cameraPos, ref int posPosition, ref int rgbaPosition, ref int flagPosition)
	{
		float f = 1f - prevPosAdvance;
		buffer.CustomFloats.Values[posPosition++] = (float)(Position.X - (double)(prevPosDeltaX * f) - cameraPos.X);
		buffer.CustomFloats.Values[posPosition++] = (float)(Position.Y - (double)(prevPosDeltaY * f) - cameraPos.Y);
		buffer.CustomFloats.Values[posPosition++] = (float)(Position.Z - (double)(prevPosDeltaZ * f) - cameraPos.Z);
		buffer.CustomFloats.Values[posPosition++] = Size;
		buffer.CustomBytes.Values[rgbaPosition++] = (byte)lightrgbs;
		buffer.CustomBytes.Values[rgbaPosition++] = (byte)(lightrgbs >> 8);
		buffer.CustomBytes.Values[rgbaPosition++] = (byte)(lightrgbs >> 16);
		buffer.CustomBytes.Values[rgbaPosition++] = (byte)(lightrgbs >> 24);
		buffer.CustomBytes.Values[rgbaPosition++] = ColorBlue;
		buffer.CustomBytes.Values[rgbaPosition++] = ColorGreen;
		buffer.CustomBytes.Values[rgbaPosition++] = ColorRed;
		buffer.CustomBytes.Values[rgbaPosition++] = ColorAlpha;
		buffer.CustomBytes.Values[rgbaPosition++] = dirNormalizedX;
		buffer.CustomBytes.Values[rgbaPosition++] = dirNormalizedY;
		buffer.CustomBytes.Values[rgbaPosition++] = dirNormalizedZ;
		rgbaPosition++;
		buffer.Flags[flagPosition++] = VertexFlags;
	}
}
