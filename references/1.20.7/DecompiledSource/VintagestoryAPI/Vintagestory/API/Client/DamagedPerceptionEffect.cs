using System;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Vintagestory.API.Client;

public class DamagedPerceptionEffect : PerceptionEffect
{
	private long damageVignettingUntil;

	private float strength;

	private int duration;

	private readonly NormalizedSimplexNoise noiseGenerator;

	public DamagedPerceptionEffect(ICoreClientAPI capi)
		: base(capi)
	{
		noiseGenerator = NormalizedSimplexNoise.FromDefaultOctaves(4, 1.0, 0.9, 123L);
	}

	public override void OnOwnPlayerDataReceived(EntityPlayer player)
	{
		player.WatchedAttributes.RegisterModifiedListener("onHurt", OnHurt);
	}

	private void OnHurt()
	{
		EntityPlayer player = capi.World.Player.Entity;
		strength = player.WatchedAttributes.GetFloat("onHurt");
		if (strength != 0f && capi.World.Player.Entity.RemainingActivityTime("invulnerable") > 0)
		{
			duration = GameMath.Clamp(200 + (int)(strength * 10f), 200, 600) * 3;
			damageVignettingUntil = capi.ElapsedMilliseconds + duration;
			float angle = player.WatchedAttributes.GetFloat("onHurtDir");
			if (angle < -99f)
			{
				capi.Render.ShaderUniforms.DamageVignettingSide = 0f;
				return;
			}
			float angleDist = GameMath.AngleRadDistance(player.Pos.Yaw - (float)Math.PI / 2f, angle);
			capi.Render.ShaderUniforms.DamageVignettingSide = GameMath.Clamp(angleDist / ((float)Math.PI / 2f), -1f, 1f);
		}
	}

	public override void OnBeforeGameRender(float dt)
	{
		if (!capi.IsGamePaused)
		{
			HandleDamageEffects(Math.Min(dt, 1f));
		}
	}

	private void HandleDamageEffects(float dt)
	{
		if (!capi.World.Player.Entity.Alive)
		{
			ApplyDeathEffects(dt);
			return;
		}
		float healthThreshold = CalculateHealthThreshold();
		float elapsedSeconds = (float)capi.InWorldEllapsedMilliseconds / 1000f;
		ApplyDamageSepiaEffect(healthThreshold, elapsedSeconds);
		ApplyDamageVignette(elapsedSeconds, healthThreshold);
		ApplyMotionEffects(healthThreshold, elapsedSeconds);
	}

	private void ApplyDeathEffects(float dt)
	{
		capi.Render.ShaderUniforms.ExtraSepia += (2f - capi.Render.ShaderUniforms.ExtraSepia) * Math.Min(1f, dt * 5f);
		capi.Render.ShaderUniforms.DamageVignetting += (1.25f - capi.Render.ShaderUniforms.DamageVignetting) * Math.Min(1f, dt * 5f);
		capi.Render.ShaderUniforms.DamageVignettingSide += (0f - capi.Render.ShaderUniforms.DamageVignettingSide) * Math.Min(1f, dt * 5f);
	}

	private float CalculateHealthThreshold()
	{
		ITreeAttribute treeAttribute = capi.World.Player.Entity.WatchedAttributes.GetTreeAttribute("health");
		float percentageHealthLeft = (treeAttribute?.GetFloat("currenthealth") / treeAttribute?.GetFloat("maxhealth")).GetValueOrDefault(1f);
		return Math.Max(0f, (0.23f - percentageHealthLeft) * 1f / 0.18f);
	}

	private void ApplyDamageSepiaEffect(float healthThreshold, float elapsedSeconds)
	{
		capi.Render.ShaderUniforms.ExtraSepia = ((healthThreshold <= 0f) ? 0f : GameMath.Clamp(healthThreshold * (float)noiseGenerator.Noise(0.0, elapsedSeconds / 3f) * 1.2f, 0f, 1.2f));
	}

	private void ApplyDamageVignette(float elapsedSeconds, float healthThreshold)
	{
		int val = GameMath.Clamp((int)(damageVignettingUntil - capi.ElapsedMilliseconds), 0, duration);
		float noise = (float)noiseGenerator.Noise(12412.0, elapsedSeconds / 2f) * 0.5f + (float)Math.Pow(Math.Abs(GameMath.Sin(elapsedSeconds * 1f / 0.7f)), 30.0) * 0.5f;
		float lowHealth = Math.Min(healthThreshold * 1.5f, 1f) * (noise * 0.75f + 0.5f);
		capi.Render.ShaderUniforms.DamageVignetting = GameMath.Clamp(GameMath.Clamp(strength / 2f, 0.5f, 3.5f) * ((float)val / (float)Math.Max(1, duration)) + lowHealth, 0f, 1.5f);
	}

	private void ApplyMotionEffects(float healthThreshold, float elapsedSeconds)
	{
		if (!(healthThreshold <= 0f))
		{
			if (capi.World.Rand.NextDouble() < 0.01)
			{
				capi.World.AddCameraShake(0.15f * healthThreshold);
			}
			capi.Input.MouseYaw += healthThreshold * (float)(noiseGenerator.Noise(76.0, elapsedSeconds / 50f) - 0.5) * 0.003f;
			float dp = healthThreshold * (float)(noiseGenerator.Noise(elapsedSeconds / 50f, 987.0) - 0.5) * 0.003f;
			capi.World.Player.Entity.Pos.Pitch += dp;
			capi.Input.MousePitch += dp;
		}
	}
}
