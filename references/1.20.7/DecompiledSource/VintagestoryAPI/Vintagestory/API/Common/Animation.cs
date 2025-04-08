using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace Vintagestory.API.Common;

/// <summary>
/// Represents a shape animation and can calculate the transformation matrices for each frame to be sent to the shader
/// Process
/// 1. For each frame, for each root element, calculate the transformation matrix. Curent model matrix is identy mat.
/// 1.1. Get previous and next key frame. Apply translation, rotation and scale to model matrix.
/// 1.2. Store this matrix as animationmatrix in list
/// 1.3. For each child element
/// 1.3.1. Multiply local transformation matrix with the animation matrix. This matrix is now the curent model matrix. Go to 1 with child elements as root elems
///
/// 2. For each frame, for each joint
/// 2.1. Calculate the inverse model matrix 
/// 2.2. Multiply stored animationmatrix with the inverse model matrix
///
/// 3. done
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class Animation
{
	[JsonProperty]
	public int QuantityFrames;

	[JsonProperty]
	public string Name;

	[JsonProperty]
	public string Code;

	[JsonProperty]
	public int Version;

	[JsonProperty]
	public bool EaseAnimationSpeed;

	[JsonProperty]
	public AnimationKeyFrame[] KeyFrames;

	[JsonProperty]
	public EnumEntityActivityStoppedHandling OnActivityStopped = EnumEntityActivityStoppedHandling.Rewind;

	[JsonProperty]
	public EnumEntityAnimationEndHandling OnAnimationEnd;

	public uint CodeCrc32;

	public AnimationFrame[][] PrevNextKeyFrameByFrame;

	protected HashSet<int> jointsDone = new HashSet<int>();

	[OnDeserialized]
	internal void OnDeserializedMethod(StreamingContext context)
	{
		if (Code == null)
		{
			Code = Name;
		}
		CodeCrc32 = AnimationMetaData.GetCrc32(Code);
	}

	/// <summary>
	/// Compiles the animation into a bunch of matrices, 31 matrices per frame.
	/// </summary>
	/// <param name="rootElements"></param>
	/// <param name="jointsById"></param>
	/// <param name="recursive">When false, will only do root elements</param>
	public void GenerateAllFrames(ShapeElement[] rootElements, Dictionary<int, AnimationJoint> jointsById, bool recursive = true)
	{
		for (int m = 0; m < rootElements.Length; m++)
		{
			rootElements[m].CacheInverseTransformMatrixRecursive();
		}
		AnimationFrame[] resolvedKeyFrames = new AnimationFrame[KeyFrames.Length];
		for (int l = 0; l < resolvedKeyFrames.Length; l++)
		{
			resolvedKeyFrames[l] = new AnimationFrame
			{
				FrameNumber = KeyFrames[l].Frame
			};
		}
		if (KeyFrames.Length == 0)
		{
			throw new Exception("Animation '" + Code + "' has no keyframes, this will cause other errors every time it is ticked");
		}
		if (jointsById.Count >= GlobalConstants.MaxAnimatedElements)
		{
			if (GlobalConstants.MaxAnimatedElements < 46 && jointsById.Count <= 46)
			{
				throw new Exception("Max joint cap of " + GlobalConstants.MaxAnimatedElements + " reached, needs to be at least " + jointsById.Count + ". In clientsettings.json, please try increasing the \"maxAnimatedElements\": setting to 46.  This works for most GPUs.  Otherwise you might need to disable the creature.");
			}
			throw new Exception("A mod's entity has " + jointsById.Count + " animation joints which exceeds the max joint cap of " + GlobalConstants.MaxAnimatedElements + ". Sorry, you'll have to either disable this creature or simplify the model.");
		}
		for (int k = 0; k < resolvedKeyFrames.Length; k++)
		{
			jointsDone.Clear();
			GenerateFrame(k, resolvedKeyFrames, rootElements, jointsById, Mat4f.Create(), resolvedKeyFrames[k].RootElementTransforms, recursive);
		}
		for (int j = 0; j < resolvedKeyFrames.Length; j++)
		{
			resolvedKeyFrames[j].FinalizeMatrices(jointsById);
		}
		PrevNextKeyFrameByFrame = new AnimationFrame[QuantityFrames][];
		for (int i = 0; i < QuantityFrames; i++)
		{
			getLeftRightResolvedFrame(i, resolvedKeyFrames, out var left, out var right);
			PrevNextKeyFrameByFrame[i] = new AnimationFrame[2] { left, right };
		}
	}

	protected void GenerateFrame(int indexNumber, AnimationFrame[] resKeyFrames, ShapeElement[] elements, Dictionary<int, AnimationJoint> jointsById, float[] modelMatrix, List<ElementPose> transforms, bool recursive = true)
	{
		int frameNumber = resKeyFrames[indexNumber].FrameNumber;
		if (frameNumber >= QuantityFrames)
		{
			throw new InvalidOperationException("Invalid animation '" + Code + "'. Has QuantityFrames set to " + QuantityFrames + " but a key frame at frame " + frameNumber + ". QuantityFrames always must be higher than frame number");
		}
		foreach (ShapeElement element in elements)
		{
			ElementPose animTransform = new ElementPose();
			animTransform.ForElement = element;
			GenerateFrameForElement(frameNumber, element, ref animTransform);
			transforms.Add(animTransform);
			float[] animModelMatrix = Mat4f.CloneIt(modelMatrix);
			Mat4f.Mul(animModelMatrix, animModelMatrix, element.GetLocalTransformMatrix(Version, null, animTransform));
			if (element.JointId > 0 && !jointsDone.Contains(element.JointId))
			{
				resKeyFrames[indexNumber].SetTransform(element.JointId, animModelMatrix);
				jointsDone.Add(element.JointId);
			}
			if (recursive && element.Children != null)
			{
				GenerateFrame(indexNumber, resKeyFrames, element.Children, jointsById, animModelMatrix, animTransform.ChildElementPoses);
			}
		}
	}

	protected void GenerateFrameForElement(int frameNumber, ShapeElement element, ref ElementPose transform)
	{
		for (int flag = 0; flag < 3; flag++)
		{
			getTwoKeyFramesElementForFlag(frameNumber, element, flag, out var curKelem, out var nextKelem);
			if (curKelem != null)
			{
				float t;
				if (nextKelem == null || curKelem == nextKelem)
				{
					nextKelem = curKelem;
					t = 0f;
				}
				else if (nextKelem.Frame < curKelem.Frame)
				{
					int quantity = nextKelem.Frame + (QuantityFrames - curKelem.Frame);
					t = (float)GameMath.Mod(frameNumber - curKelem.Frame, QuantityFrames) / (float)quantity;
				}
				else
				{
					t = (float)(frameNumber - curKelem.Frame) / (float)(nextKelem.Frame - curKelem.Frame);
				}
				lerpKeyFrameElement(curKelem, nextKelem, flag, t, ref transform);
				transform.RotShortestDistanceX = curKelem.RotShortestDistanceX;
				transform.RotShortestDistanceY = curKelem.RotShortestDistanceY;
				transform.RotShortestDistanceZ = curKelem.RotShortestDistanceZ;
			}
		}
	}

	protected void lerpKeyFrameElement(AnimationKeyFrameElement prev, AnimationKeyFrameElement next, int forFlag, float t, ref ElementPose transform)
	{
		if (prev != null || next != null)
		{
			switch (forFlag)
			{
			case 0:
				transform.translateX = GameMath.Lerp((float)prev.OffsetX.Value / 16f, (float)next.OffsetX.Value / 16f, t);
				transform.translateY = GameMath.Lerp((float)prev.OffsetY.Value / 16f, (float)next.OffsetY.Value / 16f, t);
				transform.translateZ = GameMath.Lerp((float)prev.OffsetZ.Value / 16f, (float)next.OffsetZ.Value / 16f, t);
				break;
			case 1:
				transform.degX = GameMath.Lerp((float)prev.RotationX.Value, (float)next.RotationX.Value, t);
				transform.degY = GameMath.Lerp((float)prev.RotationY.Value, (float)next.RotationY.Value, t);
				transform.degZ = GameMath.Lerp((float)prev.RotationZ.Value, (float)next.RotationZ.Value, t);
				break;
			default:
				transform.scaleX = GameMath.Lerp((float)prev.StretchX.Value, (float)next.StretchX.Value, t);
				transform.scaleY = GameMath.Lerp((float)prev.StretchY.Value, (float)next.StretchY.Value, t);
				transform.scaleZ = GameMath.Lerp((float)prev.StretchZ.Value, (float)next.StretchZ.Value, t);
				break;
			}
		}
	}

	protected void getTwoKeyFramesElementForFlag(int frameNumber, ShapeElement forElement, int forFlag, out AnimationKeyFrameElement left, out AnimationKeyFrameElement right)
	{
		left = null;
		right = null;
		int rightKfIndex = seekRightKeyFrame(frameNumber, forElement, forFlag);
		if (rightKfIndex != -1)
		{
			right = KeyFrames[rightKfIndex].GetKeyFrameElement(forElement);
			int leftKfIndex = seekLeftKeyFrame(rightKfIndex, forElement, forFlag);
			if (leftKfIndex == -1)
			{
				left = right;
			}
			else
			{
				left = KeyFrames[leftKfIndex].GetKeyFrameElement(forElement);
			}
		}
	}

	private int seekRightKeyFrame(int aboveFrameNumber, ShapeElement forElement, int forFlag)
	{
		int firstIndex = -1;
		for (int i = 0; i < KeyFrames.Length; i++)
		{
			AnimationKeyFrame keyframe = KeyFrames[i];
			AnimationKeyFrameElement kelem = keyframe.GetKeyFrameElement(forElement);
			if (kelem != null && kelem.IsSet(forFlag))
			{
				if (firstIndex == -1)
				{
					firstIndex = i;
				}
				if (keyframe.Frame > aboveFrameNumber)
				{
					return i;
				}
			}
		}
		return firstIndex;
	}

	private int seekLeftKeyFrame(int leftOfKeyFrameIndex, ShapeElement forElement, int forFlag)
	{
		for (int i = 0; i < KeyFrames.Length; i++)
		{
			int index = GameMath.Mod(leftOfKeyFrameIndex - i - 1, KeyFrames.Length);
			AnimationKeyFrameElement kelem = KeyFrames[index].GetKeyFrameElement(forElement);
			if (kelem != null && kelem.IsSet(forFlag))
			{
				return index;
			}
		}
		return -1;
	}

	protected void getLeftRightResolvedFrame(int frameNumber, AnimationFrame[] frames, out AnimationFrame left, out AnimationFrame right)
	{
		left = null;
		right = null;
		int keyframeIndex = frames.Length - 1;
		bool loopAround = false;
		while (keyframeIndex >= -1)
		{
			AnimationFrame keyframe = frames[GameMath.Mod(keyframeIndex, frames.Length)];
			keyframeIndex--;
			if (keyframe.FrameNumber <= frameNumber || loopAround)
			{
				left = keyframe;
				break;
			}
			if (keyframeIndex == -1)
			{
				loopAround = true;
			}
		}
		keyframeIndex += 2;
		AnimationFrame nextkeyframe = frames[GameMath.Mod(keyframeIndex, frames.Length)];
		right = nextkeyframe;
	}

	public Animation Clone()
	{
		return new Animation
		{
			Code = Code,
			CodeCrc32 = CodeCrc32,
			EaseAnimationSpeed = EaseAnimationSpeed,
			jointsDone = jointsDone,
			KeyFrames = CloneKeyFrames(),
			Name = Name,
			OnActivityStopped = OnActivityStopped,
			OnAnimationEnd = OnAnimationEnd,
			QuantityFrames = QuantityFrames,
			Version = Version
		};
	}

	private AnimationKeyFrame[] CloneKeyFrames()
	{
		AnimationKeyFrame[] elems = new AnimationKeyFrame[KeyFrames.Length];
		for (int i = 0; i < KeyFrames.Length; i++)
		{
			elems[i] = KeyFrames[i].Clone();
		}
		return elems;
	}
}
