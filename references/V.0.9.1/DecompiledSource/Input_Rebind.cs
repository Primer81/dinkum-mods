using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class Input_Rebind : MonoBehaviour
{
	public enum RebindType
	{
		Movement,
		Jump,
		Use,
		Interact,
		Other,
		Inventory,
		Journal,
		DropItem,
		VehicleMovement,
		VehicleUse,
		VehicleInteract,
		SwapCameraMode,
		MoveCameraButton,
		OpenMap,
		OpenChat,
		VoiceChat
	}

	[Serializable]
	private class BindingWrapperClass
	{
		public List<BindingSerializable> bindingList = new List<BindingSerializable>();
	}

	[Serializable]
	private struct BindingSerializable
	{
		public string id;

		public string path;

		public BindingSerializable(string bindingId, string bindingPath)
		{
			id = bindingId;
			path = bindingPath;
		}
	}

	public static Input_Rebind rebind;

	public InputActionReference move;

	public InputActionReference jump;

	public InputActionReference use;

	public InputActionReference interact;

	public InputActionReference other;

	public InputActionReference inventory;

	public InputActionReference journal;

	public InputActionReference vehicleMovement;

	public InputActionReference vehicleUse;

	public InputActionReference vehicleInteract;

	public InputActionReference swapCameraMode;

	public InputActionReference openMap;

	public InputActionReference openChat;

	public InputActionReference moveCameraButton;

	public InputActionReference voiceChat;

	public bool binding;

	public RemapButton[] allButtons;

	private RemapButton currentlyMapping;

	private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

	private void Awake()
	{
		rebind = this;
	}

	private void Start()
	{
		jump.Set(InputMaster.input.controls.Controls.Jump);
		move.Set(InputMaster.input.controls.Controls.Move);
		use.Set(InputMaster.input.controls.Controls.Use);
		interact.Set(InputMaster.input.controls.Controls.Interact);
		other.Set(InputMaster.input.controls.Controls.Other);
		inventory.Set(InputMaster.input.controls.Controls.Inventory);
		journal.Set(InputMaster.input.controls.Controls.Journal);
		vehicleMovement.Set(InputMaster.input.controls.Controls.VehicleAccelerate);
		vehicleUse.Set(InputMaster.input.controls.Controls.VehicleUse);
		vehicleInteract.Set(InputMaster.input.controls.Controls.VehicleInteract);
		swapCameraMode.Set(InputMaster.input.controls.Controls.SwapCamera);
		openMap.Set(InputMaster.input.controls.UI.OpenMap);
		openChat.Set(InputMaster.input.controls.UI.OpenChat);
		moveCameraButton.Set(InputMaster.input.controls.Controls.TriggerLook);
		voiceChat.Set(InputMaster.input.controls.UI.VC);
		refreshButtonBindings();
	}

	public void resetAllBindings()
	{
		new BindingWrapperClass();
		foreach (InputActionMap actionMap in InputMaster.input.controls.asset.actionMaps)
		{
			actionMap.RemoveAllBindingOverrides();
		}
		PlayerPrefs.DeleteKey("ControlOverrides");
		refreshButtonBindings();
	}

	public void resetToDefault(RebindType myType, int id = 1, int altId = 2)
	{
		if (myType == RebindType.Movement)
		{
			move.action.RemoveBindingOverride(2 + id);
			move.action.RemoveBindingOverride(2 + altId);
		}
		if (myType == RebindType.VehicleMovement)
		{
			vehicleMovement.action.RemoveBindingOverride(2 + id);
			vehicleMovement.action.RemoveBindingOverride(2 + altId);
		}
		if (myType == RebindType.Jump)
		{
			jump.action.RemoveBindingOverride(id);
			jump.action.RemoveBindingOverride(altId);
		}
		if (myType == RebindType.Use)
		{
			use.action.RemoveBindingOverride(id);
			use.action.RemoveBindingOverride(altId);
		}
		if (myType == RebindType.Interact)
		{
			interact.action.RemoveBindingOverride(id);
			interact.action.RemoveBindingOverride(altId);
		}
		if (myType == RebindType.Other)
		{
			other.action.RemoveBindingOverride(id);
			other.action.RemoveBindingOverride(altId);
		}
		if (myType == RebindType.Inventory)
		{
			inventory.action.RemoveBindingOverride(id);
			inventory.action.RemoveBindingOverride(altId);
		}
		if (myType == RebindType.Journal)
		{
			journal.action.RemoveBindingOverride(id);
			journal.action.RemoveBindingOverride(altId);
		}
		if (myType == RebindType.DropItem)
		{
			InputMaster.input.controls.Controls.DropItem.RemoveBindingOverride(id);
			InputMaster.input.controls.Controls.DropItem.RemoveBindingOverride(altId);
		}
		if (myType == RebindType.VehicleUse)
		{
			vehicleUse.action.RemoveBindingOverride(id);
			vehicleUse.action.RemoveBindingOverride(altId);
		}
		if (myType == RebindType.VehicleInteract)
		{
			vehicleInteract.action.RemoveBindingOverride(id);
			vehicleInteract.action.RemoveBindingOverride(altId);
		}
		if (myType == RebindType.SwapCameraMode)
		{
			swapCameraMode.action.RemoveBindingOverride(id);
			swapCameraMode.action.RemoveBindingOverride(altId);
		}
		if (myType == RebindType.OpenMap)
		{
			openMap.action.RemoveBindingOverride(id);
			openMap.action.RemoveBindingOverride(altId);
		}
		if (myType == RebindType.OpenChat)
		{
			openChat.action.RemoveBindingOverride(id);
			openChat.action.RemoveBindingOverride(altId);
		}
		if (myType == RebindType.MoveCameraButton)
		{
			moveCameraButton.action.RemoveBindingOverride(id);
			moveCameraButton.action.RemoveBindingOverride(altId);
		}
		refreshButtonBindings();
		SaveBindings();
	}

	public bool checkIfIsOverridden(RebindType myType)
	{
		if (myType == RebindType.Movement)
		{
			return false;
		}
		_ = 8;
		_ = 1;
		switch (myType)
		{
		case RebindType.Use:
			if (use.action.bindings[1].overridePath != null)
			{
				return use.action.bindings[1].path != use.action.bindings[1].overridePath;
			}
			return false;
		case RebindType.Interact:
			if (interact.action.bindings[1].overridePath != null)
			{
				return interact.action.bindings[1].path != interact.action.bindings[1].overridePath;
			}
			return false;
		case RebindType.Other:
			if (other.action.bindings[1].overridePath != null)
			{
				return other.action.bindings[1].path != other.action.bindings[1].overridePath;
			}
			return false;
		default:
			_ = 5;
			_ = 6;
			_ = 7;
			_ = 9;
			_ = 10;
			switch (myType)
			{
			case RebindType.SwapCameraMode:
				if (swapCameraMode.action.bindings[1].overridePath != null)
				{
					return swapCameraMode.action.bindings[1].path != swapCameraMode.action.bindings[1].overridePath;
				}
				return false;
			case RebindType.OpenMap:
				if (openMap.action.bindings[1].overridePath != null)
				{
					return openMap.action.bindings[1].path != openMap.action.bindings[1].overridePath;
				}
				return false;
			default:
				return false;
			}
		}
	}

	public void refreshButtonBindings()
	{
		for (int i = 0; i < allButtons.Length; i++)
		{
			getKeyBindingForOptionButton(allButtons[i]);
		}
	}

	public void startRebind(RebindType myType, int id, bool alt = false)
	{
		if (myType == RebindType.Movement)
		{
			StartRebindingComposite(move, id);
		}
		if (myType == RebindType.VehicleMovement)
		{
			StartRebindingComposite(vehicleMovement, id);
		}
		if (myType == RebindType.Jump)
		{
			StartRebinding(jump, id);
		}
		if (myType == RebindType.Use)
		{
			StartRebinding(use, id);
		}
		if (myType == RebindType.Interact)
		{
			StartRebinding(interact, id);
		}
		if (myType == RebindType.Other)
		{
			StartRebinding(other, id);
		}
		if (myType == RebindType.Inventory)
		{
			StartRebinding(inventory, id);
		}
		if (myType == RebindType.Journal)
		{
			StartRebinding(journal, id);
		}
		if (myType == RebindType.DropItem)
		{
			StartRebinding(InputMaster.input.controls.Controls.DropItem, id);
		}
		if (myType == RebindType.VehicleUse)
		{
			StartRebinding(vehicleUse, id);
		}
		if (myType == RebindType.VehicleInteract)
		{
			StartRebinding(vehicleInteract, id);
		}
		if (myType == RebindType.SwapCameraMode)
		{
			StartRebinding(swapCameraMode, id);
		}
		if (myType == RebindType.OpenMap)
		{
			StartRebinding(openMap, id);
		}
		if (myType == RebindType.OpenChat)
		{
			StartRebinding(openChat, id);
		}
		if (myType == RebindType.MoveCameraButton)
		{
			StartRebinding(moveCameraButton, id);
		}
		currentlyMapping = setButtonsForListening(myType, id, alt);
		currentlyMapping.checkListening(myType, alt);
	}

	public void getKeyBindingForOptionButton(RemapButton button)
	{
		RebindType myType = button.myType;
		int myTypeId = button.myTypeId;
		if (myType == RebindType.Movement)
		{
			getKeyBindingComposite(button, move, myTypeId);
		}
		if (myType == RebindType.VehicleMovement)
		{
			getKeyBindingComposite(button, vehicleMovement, myTypeId);
		}
		if (myType == RebindType.Jump)
		{
			getKeyBinding(button, jump);
		}
		if (myType == RebindType.Use)
		{
			getKeyBinding(button, use);
		}
		if (myType == RebindType.Interact)
		{
			getKeyBinding(button, interact);
		}
		if (myType == RebindType.Other)
		{
			getKeyBinding(button, other);
		}
		if (myType == RebindType.Inventory)
		{
			getKeyBinding(button, inventory);
		}
		if (myType == RebindType.Inventory)
		{
			getKeyBinding(button, inventory);
		}
		if (myType == RebindType.Journal)
		{
			getKeyBinding(button, journal);
		}
		if (myType == RebindType.DropItem)
		{
			getKeyBinding(button, InputMaster.input.controls.Controls.DropItem);
		}
		if (myType == RebindType.VehicleUse)
		{
			getKeyBinding(button, vehicleUse);
		}
		if (myType == RebindType.VehicleInteract)
		{
			getKeyBinding(button, vehicleInteract);
		}
		if (myType == RebindType.SwapCameraMode)
		{
			getKeyBinding(button, swapCameraMode);
		}
		if (myType == RebindType.OpenMap)
		{
			getKeyBinding(button, openMap);
		}
		if (myType == RebindType.OpenChat)
		{
			getKeyBinding(button, openChat);
		}
		if (myType == RebindType.MoveCameraButton)
		{
			getKeyBinding(button, moveCameraButton);
		}
	}

	public string getKeyBindingForInGame(RebindType myType, int id = 1)
	{
		return myType switch
		{
			RebindType.Movement => getBindingName(move, id), 
			RebindType.VehicleMovement => getBindingName(vehicleMovement, id), 
			RebindType.Jump => getBindingName(jump), 
			RebindType.Use => getBindingName(use), 
			RebindType.Interact => getBindingName(interact), 
			RebindType.Other => getBindingName(other), 
			RebindType.Inventory => getBindingName(inventory), 
			_ => myType switch
			{
				RebindType.Inventory => getBindingName(inventory), 
				RebindType.Journal => getBindingName(journal), 
				RebindType.DropItem => getBindingName(InputMaster.input.controls.Controls.DropItem), 
				RebindType.VehicleUse => getBindingName(vehicleUse), 
				RebindType.VehicleInteract => getBindingName(vehicleInteract), 
				RebindType.SwapCameraMode => getBindingName(swapCameraMode), 
				RebindType.OpenMap => getBindingName(openMap), 
				RebindType.OpenChat => getBindingName(openChat), 
				RebindType.MoveCameraButton => getBindingName(moveCameraButton), 
				RebindType.VoiceChat => getBindingName(voiceChat), 
				_ => "", 
			}, 
		};
	}

	public void StartRebinding(InputAction inputToRebind, int indexId = 1)
	{
		InputMaster.input.controls.Disable();
		InputMaster.input.controls.Controls.Disable();
		InputMaster.input.controls.UI.Disable();
		StartCoroutine(waitForBindingComplete(inputToRebind));
		rebindingOperation = inputToRebind.PerformInteractiveRebinding().WithTargetBinding(indexId).WithControlsExcluding("<Gamepad>")
			.WithExpectedControlType("Button")
			.OnMatchWaitForAnother(0.1f)
			.OnComplete(delegate
			{
				RebindComplete();
			})
			.Start();
	}

	public void StartRebindingComposite(InputAction inputToRebind, int id)
	{
		InputMaster.input.controls.Disable();
		InputMaster.input.controls.Controls.Disable();
		InputMaster.input.controls.UI.Disable();
		StartCoroutine(waitForBindingCompositeComplete(inputToRebind, id));
		rebindingOperation = inputToRebind.PerformInteractiveRebinding().WithTargetBinding(2 + id).WithControlsExcluding("<Gamepad>")
			.WithExpectedControlType("Button")
			.OnMatchWaitForAnother(0.1f)
			.OnComplete(delegate
			{
				RebindComplete();
			})
			.Start()
			.OnCancel(delegate
			{
				RebindComplete();
			});
	}

	private void RebindComplete()
	{
		binding = false;
		rebindingOperation.Dispose();
		InputMaster.input.controls.Enable();
		InputMaster.input.controls.Controls.Enable();
		InputMaster.input.controls.UI.Enable();
		MonoBehaviour.print("Rebind Complete");
		NotificationManager.manage.resetHintButtons();
		NotificationManager.manage.buttonPromptNotification.fillButtonPrompt("", null);
		SaveBindings();
	}

	public RemapButton setButtonsForListening(RebindType checkType, int id, bool alt)
	{
		for (int i = 0; i < allButtons.Length; i++)
		{
			if ((!alt && allButtons[i].myType == checkType && allButtons[i].myTypeId == id) || (alt && allButtons[i].myType == checkType && allButtons[i].myAltTypeId == id))
			{
				return allButtons[i];
			}
		}
		return null;
	}

	private IEnumerator waitForBindingComplete(InputAction keyName)
	{
		binding = true;
		while (binding)
		{
			yield return null;
		}
		keyName.GetBindingIndexForControl(keyName.controls[0]);
		currentlyMapping.finishListening(getBindingName(keyName), getBindingName(keyName, currentlyMapping.myAltTypeId));
	}

	private IEnumerator waitForBindingCompositeComplete(InputAction keyName, int id)
	{
		binding = true;
		while (binding)
		{
			yield return null;
		}
		currentlyMapping.finishListening(getBindingName(keyName, 2 + currentlyMapping.myTypeId), getBindingName(keyName, 2 + currentlyMapping.myAltTypeId));
	}

	public void getKeyBinding(RemapButton currentlyMapping, InputAction keyName)
	{
		currentlyMapping.finishListening(getBindingName(keyName));
		if ((bool)currentlyMapping.altKey)
		{
			currentlyMapping.finishListening(getBindingName(keyName), getBindingName(keyName, currentlyMapping.myAltTypeId));
		}
	}

	public void getKeyBindingComposite(RemapButton currentlyMapping, InputAction keyName, int id)
	{
		currentlyMapping.finishListening(getBindingName(keyName, 2 + id), getBindingName(keyName, 2 + currentlyMapping.myAltTypeId));
	}

	public string getBindingName(InputAction keyName, int index = 1)
	{
		if (keyName == null)
		{
			return "no keyname given";
		}
		if (index >= keyName.bindings.Count || string.IsNullOrEmpty(keyName.bindings[index].effectivePath))
		{
			return "";
		}
		if (keyName.bindings[index].effectivePath.Contains("Mouse"))
		{
			return InputControlPath.ToHumanReadableString(keyName.bindings[index].effectivePath, InputControlPath.HumanReadableStringOptions.UseShortNames);
		}
		return InputControlPath.ToHumanReadableString(keyName.bindings[index].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
	}

	public void SaveBindings()
	{
		StoreControlOverrides();
	}

	public void LoadBindings()
	{
		LoadControlOverrides();
		refreshButtonBindings();
	}

	public void StoreControlOverrides()
	{
		BindingWrapperClass bindingWrapperClass = new BindingWrapperClass();
		foreach (InputActionMap actionMap in InputMaster.input.controls.asset.actionMaps)
		{
			foreach (InputBinding binding in actionMap.bindings)
			{
				if (!string.IsNullOrEmpty(binding.overridePath))
				{
					bindingWrapperClass.bindingList.Add(new BindingSerializable(binding.id.ToString(), binding.overridePath));
				}
			}
		}
		PlayerPrefs.SetString("ControlOverrides", JsonUtility.ToJson(bindingWrapperClass));
		PlayerPrefs.Save();
	}

	public void LoadControlOverrides()
	{
		if (!PlayerPrefs.HasKey("ControlOverrides"))
		{
			return;
		}
		BindingWrapperClass obj = JsonUtility.FromJson(PlayerPrefs.GetString("ControlOverrides"), typeof(BindingWrapperClass)) as BindingWrapperClass;
		Dictionary<Guid, string> dictionary = new Dictionary<Guid, string>();
		foreach (BindingSerializable binding in obj.bindingList)
		{
			dictionary.Add(new Guid(binding.id), binding.path);
		}
		foreach (InputActionMap actionMap in InputMaster.input.controls.asset.actionMaps)
		{
			ReadOnlyArray<InputBinding> bindings = actionMap.bindings;
			for (int i = 0; i < bindings.Count; i++)
			{
				if (dictionary.TryGetValue(bindings[i].id, out var value))
				{
					actionMap.ApplyBindingOverride(i, new InputBinding
					{
						overridePath = value
					});
				}
			}
		}
	}
}
