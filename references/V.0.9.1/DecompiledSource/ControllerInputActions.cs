using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class ControllerInputActions : IInputActionCollection2, IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
{
	public struct ControlsActions
	{
		private ControllerInputActions m_Wrapper;

		public InputAction Move => m_Wrapper.m_Controls_Move;

		public InputAction Look => m_Wrapper.m_Controls_Look;

		public InputAction TriggerLook => m_Wrapper.m_Controls_TriggerLook;

		public InputAction SwapCamera => m_Wrapper.m_Controls_SwapCamera;

		public InputAction Use => m_Wrapper.m_Controls_Use;

		public InputAction Interact => m_Wrapper.m_Controls_Interact;

		public InputAction Jump => m_Wrapper.m_Controls_Jump;

		public InputAction Other => m_Wrapper.m_Controls_Other;

		public InputAction RB => m_Wrapper.m_Controls_RB;

		public InputAction LB => m_Wrapper.m_Controls_LB;

		public InputAction Inventory => m_Wrapper.m_Controls_Inventory;

		public InputAction Journal => m_Wrapper.m_Controls_Journal;

		public InputAction DropItem => m_Wrapper.m_Controls_DropItem;

		public InputAction MousePosition => m_Wrapper.m_Controls_MousePosition;

		public InputAction SwapToController => m_Wrapper.m_Controls_SwapToController;

		public InputAction SwapToKeyboard => m_Wrapper.m_Controls_SwapToKeyboard;

		public InputAction VehicleAccelerate => m_Wrapper.m_Controls_VehicleAccelerate;

		public InputAction VehicleUse => m_Wrapper.m_Controls_VehicleUse;

		public InputAction VehicleInteract => m_Wrapper.m_Controls_VehicleInteract;

		public InputAction OtherKeyboard => m_Wrapper.m_Controls_OtherKeyboard;

		public InputAction NumKeys => m_Wrapper.m_Controls_NumKeys;

		public InputAction RBKeyBoard => m_Wrapper.m_Controls_RBKeyBoard;

		public InputAction LBKeyBoard => m_Wrapper.m_Controls_LBKeyBoard;

		public bool enabled => Get().enabled;

		public ControlsActions(ControllerInputActions wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Controls;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(ControlsActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IControlsActions instance)
		{
			if (instance != null && !m_Wrapper.m_ControlsActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_ControlsActionsCallbackInterfaces.Add(instance);
				Move.started += instance.OnMove;
				Move.performed += instance.OnMove;
				Move.canceled += instance.OnMove;
				Look.started += instance.OnLook;
				Look.performed += instance.OnLook;
				Look.canceled += instance.OnLook;
				TriggerLook.started += instance.OnTriggerLook;
				TriggerLook.performed += instance.OnTriggerLook;
				TriggerLook.canceled += instance.OnTriggerLook;
				SwapCamera.started += instance.OnSwapCamera;
				SwapCamera.performed += instance.OnSwapCamera;
				SwapCamera.canceled += instance.OnSwapCamera;
				Use.started += instance.OnUse;
				Use.performed += instance.OnUse;
				Use.canceled += instance.OnUse;
				Interact.started += instance.OnInteract;
				Interact.performed += instance.OnInteract;
				Interact.canceled += instance.OnInteract;
				Jump.started += instance.OnJump;
				Jump.performed += instance.OnJump;
				Jump.canceled += instance.OnJump;
				Other.started += instance.OnOther;
				Other.performed += instance.OnOther;
				Other.canceled += instance.OnOther;
				RB.started += instance.OnRB;
				RB.performed += instance.OnRB;
				RB.canceled += instance.OnRB;
				LB.started += instance.OnLB;
				LB.performed += instance.OnLB;
				LB.canceled += instance.OnLB;
				Inventory.started += instance.OnInventory;
				Inventory.performed += instance.OnInventory;
				Inventory.canceled += instance.OnInventory;
				Journal.started += instance.OnJournal;
				Journal.performed += instance.OnJournal;
				Journal.canceled += instance.OnJournal;
				DropItem.started += instance.OnDropItem;
				DropItem.performed += instance.OnDropItem;
				DropItem.canceled += instance.OnDropItem;
				MousePosition.started += instance.OnMousePosition;
				MousePosition.performed += instance.OnMousePosition;
				MousePosition.canceled += instance.OnMousePosition;
				SwapToController.started += instance.OnSwapToController;
				SwapToController.performed += instance.OnSwapToController;
				SwapToController.canceled += instance.OnSwapToController;
				SwapToKeyboard.started += instance.OnSwapToKeyboard;
				SwapToKeyboard.performed += instance.OnSwapToKeyboard;
				SwapToKeyboard.canceled += instance.OnSwapToKeyboard;
				VehicleAccelerate.started += instance.OnVehicleAccelerate;
				VehicleAccelerate.performed += instance.OnVehicleAccelerate;
				VehicleAccelerate.canceled += instance.OnVehicleAccelerate;
				VehicleUse.started += instance.OnVehicleUse;
				VehicleUse.performed += instance.OnVehicleUse;
				VehicleUse.canceled += instance.OnVehicleUse;
				VehicleInteract.started += instance.OnVehicleInteract;
				VehicleInteract.performed += instance.OnVehicleInteract;
				VehicleInteract.canceled += instance.OnVehicleInteract;
				OtherKeyboard.started += instance.OnOtherKeyboard;
				OtherKeyboard.performed += instance.OnOtherKeyboard;
				OtherKeyboard.canceled += instance.OnOtherKeyboard;
				NumKeys.started += instance.OnNumKeys;
				NumKeys.performed += instance.OnNumKeys;
				NumKeys.canceled += instance.OnNumKeys;
				RBKeyBoard.started += instance.OnRBKeyBoard;
				RBKeyBoard.performed += instance.OnRBKeyBoard;
				RBKeyBoard.canceled += instance.OnRBKeyBoard;
				LBKeyBoard.started += instance.OnLBKeyBoard;
				LBKeyBoard.performed += instance.OnLBKeyBoard;
				LBKeyBoard.canceled += instance.OnLBKeyBoard;
			}
		}

		private void UnregisterCallbacks(IControlsActions instance)
		{
			Move.started -= instance.OnMove;
			Move.performed -= instance.OnMove;
			Move.canceled -= instance.OnMove;
			Look.started -= instance.OnLook;
			Look.performed -= instance.OnLook;
			Look.canceled -= instance.OnLook;
			TriggerLook.started -= instance.OnTriggerLook;
			TriggerLook.performed -= instance.OnTriggerLook;
			TriggerLook.canceled -= instance.OnTriggerLook;
			SwapCamera.started -= instance.OnSwapCamera;
			SwapCamera.performed -= instance.OnSwapCamera;
			SwapCamera.canceled -= instance.OnSwapCamera;
			Use.started -= instance.OnUse;
			Use.performed -= instance.OnUse;
			Use.canceled -= instance.OnUse;
			Interact.started -= instance.OnInteract;
			Interact.performed -= instance.OnInteract;
			Interact.canceled -= instance.OnInteract;
			Jump.started -= instance.OnJump;
			Jump.performed -= instance.OnJump;
			Jump.canceled -= instance.OnJump;
			Other.started -= instance.OnOther;
			Other.performed -= instance.OnOther;
			Other.canceled -= instance.OnOther;
			RB.started -= instance.OnRB;
			RB.performed -= instance.OnRB;
			RB.canceled -= instance.OnRB;
			LB.started -= instance.OnLB;
			LB.performed -= instance.OnLB;
			LB.canceled -= instance.OnLB;
			Inventory.started -= instance.OnInventory;
			Inventory.performed -= instance.OnInventory;
			Inventory.canceled -= instance.OnInventory;
			Journal.started -= instance.OnJournal;
			Journal.performed -= instance.OnJournal;
			Journal.canceled -= instance.OnJournal;
			DropItem.started -= instance.OnDropItem;
			DropItem.performed -= instance.OnDropItem;
			DropItem.canceled -= instance.OnDropItem;
			MousePosition.started -= instance.OnMousePosition;
			MousePosition.performed -= instance.OnMousePosition;
			MousePosition.canceled -= instance.OnMousePosition;
			SwapToController.started -= instance.OnSwapToController;
			SwapToController.performed -= instance.OnSwapToController;
			SwapToController.canceled -= instance.OnSwapToController;
			SwapToKeyboard.started -= instance.OnSwapToKeyboard;
			SwapToKeyboard.performed -= instance.OnSwapToKeyboard;
			SwapToKeyboard.canceled -= instance.OnSwapToKeyboard;
			VehicleAccelerate.started -= instance.OnVehicleAccelerate;
			VehicleAccelerate.performed -= instance.OnVehicleAccelerate;
			VehicleAccelerate.canceled -= instance.OnVehicleAccelerate;
			VehicleUse.started -= instance.OnVehicleUse;
			VehicleUse.performed -= instance.OnVehicleUse;
			VehicleUse.canceled -= instance.OnVehicleUse;
			VehicleInteract.started -= instance.OnVehicleInteract;
			VehicleInteract.performed -= instance.OnVehicleInteract;
			VehicleInteract.canceled -= instance.OnVehicleInteract;
			OtherKeyboard.started -= instance.OnOtherKeyboard;
			OtherKeyboard.performed -= instance.OnOtherKeyboard;
			OtherKeyboard.canceled -= instance.OnOtherKeyboard;
			NumKeys.started -= instance.OnNumKeys;
			NumKeys.performed -= instance.OnNumKeys;
			NumKeys.canceled -= instance.OnNumKeys;
			RBKeyBoard.started -= instance.OnRBKeyBoard;
			RBKeyBoard.performed -= instance.OnRBKeyBoard;
			RBKeyBoard.canceled -= instance.OnRBKeyBoard;
			LBKeyBoard.started -= instance.OnLBKeyBoard;
			LBKeyBoard.performed -= instance.OnLBKeyBoard;
			LBKeyBoard.canceled -= instance.OnLBKeyBoard;
		}

		public void RemoveCallbacks(IControlsActions instance)
		{
			if (m_Wrapper.m_ControlsActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IControlsActions instance)
		{
			foreach (IControlsActions controlsActionsCallbackInterface in m_Wrapper.m_ControlsActionsCallbackInterfaces)
			{
				UnregisterCallbacks(controlsActionsCallbackInterface);
			}
			m_Wrapper.m_ControlsActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct UIActions
	{
		private ControllerInputActions m_Wrapper;

		public InputAction Navigate => m_Wrapper.m_UI_Navigate;

		public InputAction Select => m_Wrapper.m_UI_Select;

		public InputAction UIAlt => m_Wrapper.m_UI_UIAlt;

		public InputAction SelectActiveConfirmButton => m_Wrapper.m_UI_SelectActiveConfirmButton;

		public InputAction Cancel => m_Wrapper.m_UI_Cancel;

		public InputAction Point => m_Wrapper.m_UI_Point;

		public InputAction ScrollWheel => m_Wrapper.m_UI_ScrollWheel;

		public InputAction MiddleClick => m_Wrapper.m_UI_MiddleClick;

		public InputAction RightClick => m_Wrapper.m_UI_RightClick;

		public InputAction TrackedDevicePosition => m_Wrapper.m_UI_TrackedDevicePosition;

		public InputAction TrackedDeviceOrientation => m_Wrapper.m_UI_TrackedDeviceOrientation;

		public InputAction OpenMap => m_Wrapper.m_UI_OpenMap;

		public InputAction Whistle => m_Wrapper.m_UI_Whistle;

		public InputAction VC => m_Wrapper.m_UI_VC;

		public InputAction OpenChat => m_Wrapper.m_UI_OpenChat;

		public InputAction KeyboardUpperCase => m_Wrapper.m_UI_KeyboardUpperCase;

		public bool enabled => Get().enabled;

		public UIActions(ControllerInputActions wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_UI;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(UIActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IUIActions instance)
		{
			if (instance != null && !m_Wrapper.m_UIActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_UIActionsCallbackInterfaces.Add(instance);
				Navigate.started += instance.OnNavigate;
				Navigate.performed += instance.OnNavigate;
				Navigate.canceled += instance.OnNavigate;
				Select.started += instance.OnSelect;
				Select.performed += instance.OnSelect;
				Select.canceled += instance.OnSelect;
				UIAlt.started += instance.OnUIAlt;
				UIAlt.performed += instance.OnUIAlt;
				UIAlt.canceled += instance.OnUIAlt;
				SelectActiveConfirmButton.started += instance.OnSelectActiveConfirmButton;
				SelectActiveConfirmButton.performed += instance.OnSelectActiveConfirmButton;
				SelectActiveConfirmButton.canceled += instance.OnSelectActiveConfirmButton;
				Cancel.started += instance.OnCancel;
				Cancel.performed += instance.OnCancel;
				Cancel.canceled += instance.OnCancel;
				Point.started += instance.OnPoint;
				Point.performed += instance.OnPoint;
				Point.canceled += instance.OnPoint;
				ScrollWheel.started += instance.OnScrollWheel;
				ScrollWheel.performed += instance.OnScrollWheel;
				ScrollWheel.canceled += instance.OnScrollWheel;
				MiddleClick.started += instance.OnMiddleClick;
				MiddleClick.performed += instance.OnMiddleClick;
				MiddleClick.canceled += instance.OnMiddleClick;
				RightClick.started += instance.OnRightClick;
				RightClick.performed += instance.OnRightClick;
				RightClick.canceled += instance.OnRightClick;
				TrackedDevicePosition.started += instance.OnTrackedDevicePosition;
				TrackedDevicePosition.performed += instance.OnTrackedDevicePosition;
				TrackedDevicePosition.canceled += instance.OnTrackedDevicePosition;
				TrackedDeviceOrientation.started += instance.OnTrackedDeviceOrientation;
				TrackedDeviceOrientation.performed += instance.OnTrackedDeviceOrientation;
				TrackedDeviceOrientation.canceled += instance.OnTrackedDeviceOrientation;
				OpenMap.started += instance.OnOpenMap;
				OpenMap.performed += instance.OnOpenMap;
				OpenMap.canceled += instance.OnOpenMap;
				Whistle.started += instance.OnWhistle;
				Whistle.performed += instance.OnWhistle;
				Whistle.canceled += instance.OnWhistle;
				VC.started += instance.OnVC;
				VC.performed += instance.OnVC;
				VC.canceled += instance.OnVC;
				OpenChat.started += instance.OnOpenChat;
				OpenChat.performed += instance.OnOpenChat;
				OpenChat.canceled += instance.OnOpenChat;
				KeyboardUpperCase.started += instance.OnKeyboardUpperCase;
				KeyboardUpperCase.performed += instance.OnKeyboardUpperCase;
				KeyboardUpperCase.canceled += instance.OnKeyboardUpperCase;
			}
		}

		private void UnregisterCallbacks(IUIActions instance)
		{
			Navigate.started -= instance.OnNavigate;
			Navigate.performed -= instance.OnNavigate;
			Navigate.canceled -= instance.OnNavigate;
			Select.started -= instance.OnSelect;
			Select.performed -= instance.OnSelect;
			Select.canceled -= instance.OnSelect;
			UIAlt.started -= instance.OnUIAlt;
			UIAlt.performed -= instance.OnUIAlt;
			UIAlt.canceled -= instance.OnUIAlt;
			SelectActiveConfirmButton.started -= instance.OnSelectActiveConfirmButton;
			SelectActiveConfirmButton.performed -= instance.OnSelectActiveConfirmButton;
			SelectActiveConfirmButton.canceled -= instance.OnSelectActiveConfirmButton;
			Cancel.started -= instance.OnCancel;
			Cancel.performed -= instance.OnCancel;
			Cancel.canceled -= instance.OnCancel;
			Point.started -= instance.OnPoint;
			Point.performed -= instance.OnPoint;
			Point.canceled -= instance.OnPoint;
			ScrollWheel.started -= instance.OnScrollWheel;
			ScrollWheel.performed -= instance.OnScrollWheel;
			ScrollWheel.canceled -= instance.OnScrollWheel;
			MiddleClick.started -= instance.OnMiddleClick;
			MiddleClick.performed -= instance.OnMiddleClick;
			MiddleClick.canceled -= instance.OnMiddleClick;
			RightClick.started -= instance.OnRightClick;
			RightClick.performed -= instance.OnRightClick;
			RightClick.canceled -= instance.OnRightClick;
			TrackedDevicePosition.started -= instance.OnTrackedDevicePosition;
			TrackedDevicePosition.performed -= instance.OnTrackedDevicePosition;
			TrackedDevicePosition.canceled -= instance.OnTrackedDevicePosition;
			TrackedDeviceOrientation.started -= instance.OnTrackedDeviceOrientation;
			TrackedDeviceOrientation.performed -= instance.OnTrackedDeviceOrientation;
			TrackedDeviceOrientation.canceled -= instance.OnTrackedDeviceOrientation;
			OpenMap.started -= instance.OnOpenMap;
			OpenMap.performed -= instance.OnOpenMap;
			OpenMap.canceled -= instance.OnOpenMap;
			Whistle.started -= instance.OnWhistle;
			Whistle.performed -= instance.OnWhistle;
			Whistle.canceled -= instance.OnWhistle;
			VC.started -= instance.OnVC;
			VC.performed -= instance.OnVC;
			VC.canceled -= instance.OnVC;
			OpenChat.started -= instance.OnOpenChat;
			OpenChat.performed -= instance.OnOpenChat;
			OpenChat.canceled -= instance.OnOpenChat;
			KeyboardUpperCase.started -= instance.OnKeyboardUpperCase;
			KeyboardUpperCase.performed -= instance.OnKeyboardUpperCase;
			KeyboardUpperCase.canceled -= instance.OnKeyboardUpperCase;
		}

		public void RemoveCallbacks(IUIActions instance)
		{
			if (m_Wrapper.m_UIActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IUIActions instance)
		{
			foreach (IUIActions uIActionsCallbackInterface in m_Wrapper.m_UIActionsCallbackInterfaces)
			{
				UnregisterCallbacks(uIActionsCallbackInterface);
			}
			m_Wrapper.m_UIActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public interface IControlsActions
	{
		void OnMove(InputAction.CallbackContext context);

		void OnLook(InputAction.CallbackContext context);

		void OnTriggerLook(InputAction.CallbackContext context);

		void OnSwapCamera(InputAction.CallbackContext context);

		void OnUse(InputAction.CallbackContext context);

		void OnInteract(InputAction.CallbackContext context);

		void OnJump(InputAction.CallbackContext context);

		void OnOther(InputAction.CallbackContext context);

		void OnRB(InputAction.CallbackContext context);

		void OnLB(InputAction.CallbackContext context);

		void OnInventory(InputAction.CallbackContext context);

		void OnJournal(InputAction.CallbackContext context);

		void OnDropItem(InputAction.CallbackContext context);

		void OnMousePosition(InputAction.CallbackContext context);

		void OnSwapToController(InputAction.CallbackContext context);

		void OnSwapToKeyboard(InputAction.CallbackContext context);

		void OnVehicleAccelerate(InputAction.CallbackContext context);

		void OnVehicleUse(InputAction.CallbackContext context);

		void OnVehicleInteract(InputAction.CallbackContext context);

		void OnOtherKeyboard(InputAction.CallbackContext context);

		void OnNumKeys(InputAction.CallbackContext context);

		void OnRBKeyBoard(InputAction.CallbackContext context);

		void OnLBKeyBoard(InputAction.CallbackContext context);
	}

	public interface IUIActions
	{
		void OnNavigate(InputAction.CallbackContext context);

		void OnSelect(InputAction.CallbackContext context);

		void OnUIAlt(InputAction.CallbackContext context);

		void OnSelectActiveConfirmButton(InputAction.CallbackContext context);

		void OnCancel(InputAction.CallbackContext context);

		void OnPoint(InputAction.CallbackContext context);

		void OnScrollWheel(InputAction.CallbackContext context);

		void OnMiddleClick(InputAction.CallbackContext context);

		void OnRightClick(InputAction.CallbackContext context);

		void OnTrackedDevicePosition(InputAction.CallbackContext context);

		void OnTrackedDeviceOrientation(InputAction.CallbackContext context);

		void OnOpenMap(InputAction.CallbackContext context);

		void OnWhistle(InputAction.CallbackContext context);

		void OnVC(InputAction.CallbackContext context);

		void OnOpenChat(InputAction.CallbackContext context);

		void OnKeyboardUpperCase(InputAction.CallbackContext context);
	}

	private readonly InputActionMap m_Controls;

	private List<IControlsActions> m_ControlsActionsCallbackInterfaces = new List<IControlsActions>();

	private readonly InputAction m_Controls_Move;

	private readonly InputAction m_Controls_Look;

	private readonly InputAction m_Controls_TriggerLook;

	private readonly InputAction m_Controls_SwapCamera;

	private readonly InputAction m_Controls_Use;

	private readonly InputAction m_Controls_Interact;

	private readonly InputAction m_Controls_Jump;

	private readonly InputAction m_Controls_Other;

	private readonly InputAction m_Controls_RB;

	private readonly InputAction m_Controls_LB;

	private readonly InputAction m_Controls_Inventory;

	private readonly InputAction m_Controls_Journal;

	private readonly InputAction m_Controls_DropItem;

	private readonly InputAction m_Controls_MousePosition;

	private readonly InputAction m_Controls_SwapToController;

	private readonly InputAction m_Controls_SwapToKeyboard;

	private readonly InputAction m_Controls_VehicleAccelerate;

	private readonly InputAction m_Controls_VehicleUse;

	private readonly InputAction m_Controls_VehicleInteract;

	private readonly InputAction m_Controls_OtherKeyboard;

	private readonly InputAction m_Controls_NumKeys;

	private readonly InputAction m_Controls_RBKeyBoard;

	private readonly InputAction m_Controls_LBKeyBoard;

	private readonly InputActionMap m_UI;

	private List<IUIActions> m_UIActionsCallbackInterfaces = new List<IUIActions>();

	private readonly InputAction m_UI_Navigate;

	private readonly InputAction m_UI_Select;

	private readonly InputAction m_UI_UIAlt;

	private readonly InputAction m_UI_SelectActiveConfirmButton;

	private readonly InputAction m_UI_Cancel;

	private readonly InputAction m_UI_Point;

	private readonly InputAction m_UI_ScrollWheel;

	private readonly InputAction m_UI_MiddleClick;

	private readonly InputAction m_UI_RightClick;

	private readonly InputAction m_UI_TrackedDevicePosition;

	private readonly InputAction m_UI_TrackedDeviceOrientation;

	private readonly InputAction m_UI_OpenMap;

	private readonly InputAction m_UI_Whistle;

	private readonly InputAction m_UI_VC;

	private readonly InputAction m_UI_OpenChat;

	private readonly InputAction m_UI_KeyboardUpperCase;

	private int m_KeyboardMouseSchemeIndex = -1;

	private int m_GamepadSchemeIndex = -1;

	public InputActionAsset asset { get; }

	public InputBinding? bindingMask
	{
		get
		{
			return asset.bindingMask;
		}
		set
		{
			asset.bindingMask = value;
		}
	}

	public ReadOnlyArray<InputDevice>? devices
	{
		get
		{
			return asset.devices;
		}
		set
		{
			asset.devices = value;
		}
	}

	public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

	public IEnumerable<InputBinding> bindings => asset.bindings;

	public ControlsActions Controls => new ControlsActions(this);

	public UIActions UI => new UIActions(this);

	public InputControlScheme KeyboardMouseScheme
	{
		get
		{
			if (m_KeyboardMouseSchemeIndex == -1)
			{
				m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard&Mouse");
			}
			return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
		}
	}

	public InputControlScheme GamepadScheme
	{
		get
		{
			if (m_GamepadSchemeIndex == -1)
			{
				m_GamepadSchemeIndex = asset.FindControlSchemeIndex("Gamepad");
			}
			return asset.controlSchemes[m_GamepadSchemeIndex];
		}
	}

	public ControllerInputActions()
	{
		asset = InputActionAsset.FromJson("{\r\n    \"name\": \"ControllerInputActions\",\r\n    \"maps\": [\r\n        {\r\n            \"name\": \"Controls\",\r\n            \"id\": \"6b9ac0aa-94bc-4d64-a6e9-163096760de6\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"Move\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"2a2bb537-5cd4-4ef4-9d54-aed3773f422e\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Look\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"9206a75a-1326-4904-8a52-8d684a754565\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"TriggerLook\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"92aa74ba-02a8-4242-be4f-fc414131c849\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"SwapCamera\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"b3018232-2f29-4877-b928-c9826286470d\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Use\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"f6f276a4-dba0-4a67-9275-bef2b0586993\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Interact\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"d2c8d314-6ea0-4c8b-8503-9d88d44e25aa\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Jump\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"71906a43-45f6-4780-b019-2f8745330464\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Other\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"390d1383-c590-45a5-937f-2774552a7b3f\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"RB\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"4f0d7701-d07e-47d4-a967-e774285a42ad\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"LB\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"0db580a8-7679-4733-9f42-98075f6155aa\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Inventory\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"d48dc8bd-231a-4f27-81be-c6c42be8508f\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Journal\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"a8c6740c-677a-4dab-9170-9d20ccc7729d\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"DropItem\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"5b769cf5-b689-4f9e-9377-35e941d4563d\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"MousePosition\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"60f86b65-d4b5-409e-8cf4-a3f781cbbd50\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"SwapToController\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"8239a610-c65a-4a9e-a063-995f676beeb1\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"SwapToKeyboard\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"11c2aae1-67f8-4ef6-a681-3ca5ef2d78a8\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"VehicleAccelerate\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"4d5e8afb-cf01-4723-8fb7-40fc3559bca5\",\r\n                    \"expectedControlType\": \"Axis\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"VehicleUse\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"d20dfa6e-1262-4e4f-a04e-66c53ba07681\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"VehicleInteract\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"9871edac-36cc-4687-8769-71b46f1834d1\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"OtherKeyboard\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"c867be90-9d40-4c41-91fd-8bfb0b89da89\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"NumKeys\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"3a0f4186-fa68-4a59-98bf-b461160fe220\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"RBKeyBoard\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"8b01c01d-ee87-46ca-a875-11bb77173eb7\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"LBKeyBoard\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"0f64c989-7360-4459-b1fc-c5584fbd4565\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"978bfe49-cc26-4a3d-ab7b-7d7a29327403\",\r\n                    \"path\": \"<Gamepad>/leftStick\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"StickDeadzone\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"WASD\",\r\n                    \"id\": \"00ca640b-d935-4593-8157-c05846ea39b3\",\r\n                    \"path\": \"Dpad\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"StickDeadzone(min=0.05)\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"e2062cb9-1b15-46a2-838c-2f8d72a0bdd9\",\r\n                    \"path\": \"<Keyboard>/w\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"320bffee-a40b-4347-ac70-c210eb8bc73a\",\r\n                    \"path\": \"<Keyboard>/s\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"d2581a9b-1d11-4566-b27d-b92aff5fabbc\",\r\n                    \"path\": \"<Keyboard>/a\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"fcfe95b8-67b9-4526-84b5-5d0bc98d6400\",\r\n                    \"path\": \"<Keyboard>/d\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"Alt\",\r\n                    \"id\": \"3c96de67-b7ae-436e-b559-6d40292b8f82\",\r\n                    \"path\": \"Dpad\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"StickDeadzone(min=0.05)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"4c734224-3bda-426a-9e6f-32b44707dc66\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"765fdd2c-d374-46e8-91aa-ba7ad0e8ee3b\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"54827516-5750-4f72-ab67-5bc225b5c86c\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"f54c2a19-cecb-4bc2-9d66-1e69752ac820\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c1f7a91b-d0fd-4a62-997e-7fb9b69bf235\",\r\n                    \"path\": \"<Gamepad>/rightStick\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"StickDeadzone\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Look\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"2D Vector\",\r\n                    \"id\": \"bf1b2102-fb0e-4484-8d70-38e0b2a21587\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"StickDeadzone\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Look\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"458f94e4-4fdc-4bd3-ace9-e779d87df441\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Look\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"da295510-4ec2-42c1-8c16-745cb36f77f4\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Look\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"347c7c82-c1d6-443d-8b68-87d328c5316b\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Look\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"d174cb6c-3c49-422b-9892-9803fb7d7385\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Look\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"143bb1cd-cc10-4eca-a2f0-a3664166fe91\",\r\n                    \"path\": \"<Gamepad>/buttonWest\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Use\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"05f6913d-c316-48b2-a6bb-e225f14c7960\",\r\n                    \"path\": \"<Mouse>/leftButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Use\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"644daf4f-f82b-4372-b1e4-c6d27d75ba83\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Use\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"28da0391-e571-473b-8a72-259788843025\",\r\n                    \"path\": \"<Gamepad>/rightTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Normalize(max=1)\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Use\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"62cebeef-9ded-4c20-832e-eda12f1b53cb\",\r\n                    \"path\": \"<Gamepad>/buttonEast\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Jump\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"a0f983dd-1548-4ff7-b226-be7cdced8d98\",\r\n                    \"path\": \"<Keyboard>/space\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Jump\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c92584b5-fa6b-46bc-a7ba-a9f594d657e2\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Jump\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c1183183-69ad-44bf-9ef5-bde18da78895\",\r\n                    \"path\": \"<Gamepad>/buttonSouth\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Interact\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"507f52fd-d93d-4c63-8ba9-bc72ebc31d0f\",\r\n                    \"path\": \"<Mouse>/rightButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Interact\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f7456e98-9361-49d2-892d-3ef4a3179b68\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Interact\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c5c18188-5e75-4c50-ae82-c80503e9042c\",\r\n                    \"path\": \"<Gamepad>/leftTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Normalize(max=1)\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Interact\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"becfd62e-f3f5-4688-b413-fbe710c48f2a\",\r\n                    \"path\": \"<Gamepad>/select\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Journal\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"34a90e62-258f-4575-ac12-7997117e53f8\",\r\n                    \"path\": \"<Keyboard>/escape\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Journal\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"3e85c2cc-da6e-49c9-a1de-52d0094d91cc\",\r\n                    \"path\": \"<Keyboard>/J\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Journal\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"2a658c21-9194-4439-8134-23683a16059b\",\r\n                    \"path\": \"<Gamepad>/start\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Inventory\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"49f24a09-7063-40d9-9597-f099b3694328\",\r\n                    \"path\": \"<Keyboard>/tab\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Inventory\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"88b9ebf7-e27f-4adc-aa89-ca0dc493ed5a\",\r\n                    \"path\": \"<Keyboard>/I\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Inventory\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"322db67c-dc20-4127-b987-ee0e5b37de7d\",\r\n                    \"path\": \"<Gamepad>/leftStickPress\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"DropItem\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"5ac6252f-e5af-40cc-93dd-a6b0e82c7f93\",\r\n                    \"path\": \"<Keyboard>/q\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"DropItem\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f0045809-37e6-4f85-9562-eae56be515fa\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"DropItem\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"ffb3f6f1-dad0-4d63-9901-f0e01e8d54c5\",\r\n                    \"path\": \"<Gamepad>/buttonNorth\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Other\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"a5bb8567-918c-473b-b838-88c3a55db605\",\r\n                    \"path\": \"<Gamepad>/rightShoulder\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"RB\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"95aacb90-4f99-4bab-9cfd-f7632b43affb\",\r\n                    \"path\": \"<Gamepad>/leftShoulder\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"LB\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"cbbbe74c-bd95-439f-a4af-6fcd353ce70f\",\r\n                    \"path\": \"<Mouse>/position\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MousePosition\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8635e74f-63da-4dd8-bd5c-132d4db13e58\",\r\n                    \"path\": \"<Gamepad>/buttonWest\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"SwapToController\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f6e59457-6d0a-4bb7-b4f6-d05410db65d2\",\r\n                    \"path\": \"<Gamepad>/buttonEast\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"SwapToController\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"a61f17e5-a9fe-40b8-99eb-3e865b228141\",\r\n                    \"path\": \"<Gamepad>/buttonNorth\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"SwapToController\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"863c4d24-e0d5-46fa-ac2c-f737db6c4ff6\",\r\n                    \"path\": \"<Gamepad>/buttonSouth\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"SwapToController\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f5ed32b4-1ffb-44c8-87b4-e86c1c03a3af\",\r\n                    \"path\": \"<Mouse>/middleButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"TriggerLook\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"68225db9-65db-4553-a5df-784ae2d6d7ff\",\r\n                    \"path\": \"<Keyboard>/leftShift\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"TriggerLook\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4a256c2b-4d6e-46bf-9d36-322f2c48de89\",\r\n                    \"path\": \"<Gamepad>/rightStickPress\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"SwapCamera\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"9eefa23e-13ed-4bfe-b440-95c670c40a45\",\r\n                    \"path\": \"<Keyboard>/z\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"SwapCamera\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"a8b76da1-cd02-4ffb-af92-65235ab29afb\",\r\n                    \"path\": \"<Keyboard>/anyKey\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"SwapToKeyboard\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"1f49460c-3fb8-4ad2-937d-be119f76935f\",\r\n                    \"path\": \"<Mouse>/leftButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"SwapToKeyboard\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"32d0fc17-f5ab-46f2-86cf-22a70f3f2a8b\",\r\n                    \"path\": \"<Mouse>/rightButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"SwapToKeyboard\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"46930a0b-c2ec-4624-854d-d6683f213da3\",\r\n                    \"path\": \"<Mouse>/middleButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"SwapToKeyboard\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"Triggers\",\r\n                    \"id\": \"30f9aa1a-c251-4923-8b2a-bc2ca145005f\",\r\n                    \"path\": \"1DAxis\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"VehicleAccelerate\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"WASDForwardBack\",\r\n                    \"id\": \"04aa62e1-712e-442e-b82b-483b79af39a4\",\r\n                    \"path\": \"1DAxis\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"VehicleAccelerate\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"negative\",\r\n                    \"id\": \"9eb56747-ba7a-492b-88e3-44aa2ae0f8f6\",\r\n                    \"path\": \"<Keyboard>/s\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"VehicleAccelerate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"positive\",\r\n                    \"id\": \"6c9006de-3a8f-4147-9a87-da8daf7dab98\",\r\n                    \"path\": \"<Keyboard>/w\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"VehicleAccelerate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"negative\",\r\n                    \"id\": \"47e66401-3c53-4c61-8b36-71226f41e6b0\",\r\n                    \"path\": \"<Gamepad>/leftTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"VehicleAccelerate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"positive\",\r\n                    \"id\": \"046a8e37-c8ac-46cd-b79d-85034da83337\",\r\n                    \"path\": \"<Gamepad>/rightTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"VehicleAccelerate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"WASDForwardBack\",\r\n                    \"id\": \"fb5c5d4b-5091-4c6f-b64d-19088e6432f4\",\r\n                    \"path\": \"1DAxis\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"VehicleAccelerate\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"negative\",\r\n                    \"id\": \"c2aa5c51-7c4c-45e2-96a5-ac2700490aee\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"VehicleAccelerate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"positive\",\r\n                    \"id\": \"a7b0e091-9a7a-44c0-8df7-5221546b4d04\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"VehicleAccelerate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"negative\",\r\n                    \"id\": \"32d18d58-8faa-4f76-89e0-4a8c19bfaeef\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"VehicleAccelerate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"positive\",\r\n                    \"id\": \"5292f0dd-db04-4681-81b1-263981fba8a1\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"VehicleAccelerate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"7cd896ca-b7d4-4cd8-9781-a269e9d7044c\",\r\n                    \"path\": \"<Gamepad>/buttonWest\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"AxisDeadzone\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"VehicleUse\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"01b2c07f-8762-42e0-9945-b1d86d69e700\",\r\n                    \"path\": \"<Mouse>/leftButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"VehicleUse\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"3e4dfa2b-9e1e-4200-b4f2-10f1e3fb99e5\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"VehicleUse\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"0987e4cc-3099-4293-b59b-2cd1429f1ce7\",\r\n                    \"path\": \"<Gamepad>/buttonSouth\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"VehicleInteract\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"e90992bb-78ba-4d1f-bde2-59215f6164f5\",\r\n                    \"path\": \"<Mouse>/rightButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"VehicleInteract\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"a9486f28-caad-4d4d-95af-ffb9555f4806\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"VehicleInteract\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"5919f7f7-d186-4ec6-a87d-327d01ce3675\",\r\n                    \"path\": \"<Keyboard>/leftShift\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"OtherKeyboard\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"5a20f40d-eda7-46f4-9253-95e2d4ef1e52\",\r\n                    \"path\": \"<Keyboard>/1\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"NumKeys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"042a47dc-a4e0-44dd-9fb7-960662a47040\",\r\n                    \"path\": \"<Keyboard>/2\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=2)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"NumKeys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f5ce2e92-b065-44f6-b154-41f6050a2e7d\",\r\n                    \"path\": \"<Keyboard>/3\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=3)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"NumKeys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8d00e48a-8b36-4562-94b4-7b394bd98d6c\",\r\n                    \"path\": \"<Keyboard>/4\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=4)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"NumKeys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"721fda2f-c5ca-4885-8665-db32a77b0e2e\",\r\n                    \"path\": \"<Keyboard>/5\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=5)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"NumKeys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"5cb18fee-32a6-4c83-945a-4285c40143c9\",\r\n                    \"path\": \"<Keyboard>/6\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=6)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"NumKeys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"cc75614b-1a23-43fc-902d-c7c81da72f31\",\r\n                    \"path\": \"<Keyboard>/7\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=7)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"NumKeys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"82bea5b2-6292-43e4-9b71-762dedde913f\",\r\n                    \"path\": \"<Keyboard>/8\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=8)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"NumKeys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"7ffa266e-58bf-44a8-bfdc-66203fc3debb\",\r\n                    \"path\": \"<Keyboard>/9\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=9)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"NumKeys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"12fa6f11-72c3-41d8-acaf-d1dd946ef049\",\r\n                    \"path\": \"<Keyboard>/0\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=10)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"NumKeys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"0edd1be6-73f6-4bb2-bcb8-429d0f19358a\",\r\n                    \"path\": \"<Keyboard>/minus\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=11)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"NumKeys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"36e7235d-a609-469e-b3db-d796ba432bae\",\r\n                    \"path\": \"<Keyboard>/equals\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=12)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"NumKeys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f6fa4fd6-4f36-45ea-b95b-5e5203f98951\",\r\n                    \"path\": \"<Keyboard>/t\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"RBKeyBoard\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"b604f9fc-81d6-41a3-b2c4-b5e6c8952f83\",\r\n                    \"path\": \"<Keyboard>/r\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"LBKeyBoard\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"d4469e66-b1bf-468a-86e9-59ff6397dd22\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"SwapCamera\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"cfb746a0-13c3-4742-b3b0-76693054b759\",\r\n                    \"path\": \"<Keyboard>/e\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Other\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"139661bb-23e4-4959-9cfb-f55067fde7ea\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Other\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"ddfb5a68-e1ed-4c21-a84f-aa505ca254a4\",\r\n                    \"path\": \"<Mouse>/backButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"TriggerLook\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"82fb259d-1987-4843-a0ce-4ed65c4a08e1\",\r\n                    \"path\": \"<Mouse>/forwardButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"TriggerLook\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"UI\",\r\n            \"id\": \"66dc2691-a956-4bb3-9289-968c7df9dc8c\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"Navigate\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"63f52baa-5f9e-4251-8d83-e297de762ba4\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Select\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"848dfc3b-fc68-461d-8596-e73f0c42ac51\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"UIAlt\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"2b17871b-7e50-495b-b060-d3e81328d54e\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"SelectActiveConfirmButton\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"a54d8f92-bac0-46f9-9536-be6fab2d981a\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Cancel\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"37e96650-5c5b-4966-be23-368ea05f64b2\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Point\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"cfa4c8ed-b126-4caa-80c9-227e11ebb783\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"ScrollWheel\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"46566833-5f71-4d70-a9e9-7f2dacbd7359\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"MiddleClick\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"5622da6f-015f-434b-8ab8-fec52504e6ed\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"RightClick\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"d83dbe3a-7267-43ec-bd31-95ecdb2a9cdc\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"TrackedDevicePosition\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"73779674-eaa9-4b10-addb-ae91a108bd87\",\r\n                    \"expectedControlType\": \"Vector3\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"TrackedDeviceOrientation\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"b856f2de-9c55-4a00-bff2-fa86de145de7\",\r\n                    \"expectedControlType\": \"Quaternion\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"OpenMap\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"6d6f0133-c75f-4c0f-890a-507c7ba3fd97\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Whistle\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"a29e6eaf-c7c7-49a4-8f58-fac7fe615c96\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"VC\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"41fa4a7b-1894-4bb9-9f44-a56507192395\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"OpenChat\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"7486ba3a-bd50-4063-b963-201cb7c31792\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"KeyboardUpperCase\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"4a016ec9-901e-4aff-b044-5a86497d3f06\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"Gamepad\",\r\n                    \"id\": \"809f371f-c5e2-4e7a-83a1-d867598f40dd\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"fb8277d4-c5cd-4663-9dc7-ee3f0b506d90\",\r\n                    \"path\": \"<Gamepad>/dpad\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"StickDeadzone\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"3bc731f6-4ff1-4e1c-8f1c-2d8dd1bbd23c\",\r\n                    \"path\": \"<Gamepad>/leftStick\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"StickDeadzone(min=0.25),NormalizeVector2\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"9e92bb26-7e3b-4ec4-b06b-3c8f8e498ddc\",\r\n                    \"path\": \"<Mouse>/leftButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Select\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"31100a40-e5e7-4fa4-b1fb-b269e1be08cc\",\r\n                    \"path\": \"<Gamepad>/buttonSouth\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Select\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"d81c7cb5-cd81-4170-bcd0-d825bd2d3a9e\",\r\n                    \"path\": \"<Keyboard>/escape\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Cancel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"82627dcc-3b13-4ba9-841d-e4b746d6553e\",\r\n                    \"path\": \"<Gamepad>/buttonEast\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Cancel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4658b8f0-9869-4b9c-8bf7-91f6c7807a46\",\r\n                    \"path\": \"<VirtualMouse>/position\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Point\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c52c8e0b-8179-41d3-b8a1-d149033bbe86\",\r\n                    \"path\": \"<Mouse>/position\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Point\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"2448b72e-fb75-48df-8489-274ea930ea3d\",\r\n                    \"path\": \"<Pointer>/position\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Point\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"38c99815-14ea-4617-8627-164d27641299\",\r\n                    \"path\": \"<Mouse>/scroll\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"ScrollWheel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"24066f69-da47-44f3-a07e-0015fb02eb2e\",\r\n                    \"path\": \"<Mouse>/middleButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"MiddleClick\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4c191405-5738-4d4b-a523-c6a301dbf754\",\r\n                    \"path\": \"<Mouse>/rightButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"RightClick\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"fb2e0e67-f92e-4da3-9dce-2781156649da\",\r\n                    \"path\": \"<Gamepad>/start\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"SelectActiveConfirmButton\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"6173fecb-2af0-480c-a912-9cacb29bfde6\",\r\n                    \"path\": \"<Keyboard>/enter\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"SelectActiveConfirmButton\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8ef2f1cc-f114-473f-b410-67bcc0538e7f\",\r\n                    \"path\": \"<Mouse>/rightButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"UIAlt\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"d42ac151-ec01-4207-95da-13ddca63628f\",\r\n                    \"path\": \"<Gamepad>/buttonWest\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"UIAlt\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c3fbaa5d-7a07-4e18-8aa3-e038abea2be2\",\r\n                    \"path\": \"<Gamepad>/dpad/up\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"OpenMap\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"a690eb43-2531-4238-86d0-337ee8f3e949\",\r\n                    \"path\": \"<Gamepad>/dpad/down\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"OpenChat\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f1692268-4841-409a-9590-ca4bbd1643c6\",\r\n                    \"path\": \"<Gamepad>/leftTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"KeyboardUpperCase\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"a5a81860-448d-4ed5-a12a-90f106b7c9fa\",\r\n                    \"path\": \"<Gamepad>/rightTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"KeyboardUpperCase\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"173b6c0e-c663-4db3-a796-15bdebab8a21\",\r\n                    \"path\": \"<Keyboard>/m\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"OpenMap\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"05ca6b4b-c386-4701-a6ff-bf29ffdece85\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"OpenMap\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"2c92921a-9e47-4b58-9a15-70f14e94cbd6\",\r\n                    \"path\": \"<Keyboard>/enter\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"OpenChat\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"afe22291-2c05-4941-bec4-a1a57af2c5fc\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"OpenChat\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"ceccb572-1c0b-453a-b172-ea9ad08d3b13\",\r\n                    \"path\": \"<Gamepad>/dpad/left\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Whistle\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c22e3433-9bc9-4928-bdf6-6e3a7b1b60da\",\r\n                    \"path\": \"<Keyboard>/n\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Whistle\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"bb9f7546-b19a-4f94-80c4-6b0c51294792\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Whistle\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c30b2f33-4681-44a0-86ac-a458f8cac123\",\r\n                    \"path\": \"<Gamepad>/dpad/right\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"VC\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"931eb74a-3ec8-41f9-96f9-2b141dacdfb8\",\r\n                    \"path\": \"<Keyboard>/v\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"VC\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"407eee72-1c0a-4a58-8238-a1fef2548fb5\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"VC\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        }\r\n    ],\r\n    \"controlSchemes\": [\r\n        {\r\n            \"name\": \"Keyboard&Mouse\",\r\n            \"bindingGroup\": \"Keyboard&Mouse\",\r\n            \"devices\": [\r\n                {\r\n                    \"devicePath\": \"<Mouse>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                },\r\n                {\r\n                    \"devicePath\": \"<Keyboard>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"Gamepad\",\r\n            \"bindingGroup\": \"Gamepad\",\r\n            \"devices\": [\r\n                {\r\n                    \"devicePath\": \"<Gamepad>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                }\r\n            ]\r\n        }\r\n    ]\r\n}");
		m_Controls = asset.FindActionMap("Controls", throwIfNotFound: true);
		m_Controls_Move = m_Controls.FindAction("Move", throwIfNotFound: true);
		m_Controls_Look = m_Controls.FindAction("Look", throwIfNotFound: true);
		m_Controls_TriggerLook = m_Controls.FindAction("TriggerLook", throwIfNotFound: true);
		m_Controls_SwapCamera = m_Controls.FindAction("SwapCamera", throwIfNotFound: true);
		m_Controls_Use = m_Controls.FindAction("Use", throwIfNotFound: true);
		m_Controls_Interact = m_Controls.FindAction("Interact", throwIfNotFound: true);
		m_Controls_Jump = m_Controls.FindAction("Jump", throwIfNotFound: true);
		m_Controls_Other = m_Controls.FindAction("Other", throwIfNotFound: true);
		m_Controls_RB = m_Controls.FindAction("RB", throwIfNotFound: true);
		m_Controls_LB = m_Controls.FindAction("LB", throwIfNotFound: true);
		m_Controls_Inventory = m_Controls.FindAction("Inventory", throwIfNotFound: true);
		m_Controls_Journal = m_Controls.FindAction("Journal", throwIfNotFound: true);
		m_Controls_DropItem = m_Controls.FindAction("DropItem", throwIfNotFound: true);
		m_Controls_MousePosition = m_Controls.FindAction("MousePosition", throwIfNotFound: true);
		m_Controls_SwapToController = m_Controls.FindAction("SwapToController", throwIfNotFound: true);
		m_Controls_SwapToKeyboard = m_Controls.FindAction("SwapToKeyboard", throwIfNotFound: true);
		m_Controls_VehicleAccelerate = m_Controls.FindAction("VehicleAccelerate", throwIfNotFound: true);
		m_Controls_VehicleUse = m_Controls.FindAction("VehicleUse", throwIfNotFound: true);
		m_Controls_VehicleInteract = m_Controls.FindAction("VehicleInteract", throwIfNotFound: true);
		m_Controls_OtherKeyboard = m_Controls.FindAction("OtherKeyboard", throwIfNotFound: true);
		m_Controls_NumKeys = m_Controls.FindAction("NumKeys", throwIfNotFound: true);
		m_Controls_RBKeyBoard = m_Controls.FindAction("RBKeyBoard", throwIfNotFound: true);
		m_Controls_LBKeyBoard = m_Controls.FindAction("LBKeyBoard", throwIfNotFound: true);
		m_UI = asset.FindActionMap("UI", throwIfNotFound: true);
		m_UI_Navigate = m_UI.FindAction("Navigate", throwIfNotFound: true);
		m_UI_Select = m_UI.FindAction("Select", throwIfNotFound: true);
		m_UI_UIAlt = m_UI.FindAction("UIAlt", throwIfNotFound: true);
		m_UI_SelectActiveConfirmButton = m_UI.FindAction("SelectActiveConfirmButton", throwIfNotFound: true);
		m_UI_Cancel = m_UI.FindAction("Cancel", throwIfNotFound: true);
		m_UI_Point = m_UI.FindAction("Point", throwIfNotFound: true);
		m_UI_ScrollWheel = m_UI.FindAction("ScrollWheel", throwIfNotFound: true);
		m_UI_MiddleClick = m_UI.FindAction("MiddleClick", throwIfNotFound: true);
		m_UI_RightClick = m_UI.FindAction("RightClick", throwIfNotFound: true);
		m_UI_TrackedDevicePosition = m_UI.FindAction("TrackedDevicePosition", throwIfNotFound: true);
		m_UI_TrackedDeviceOrientation = m_UI.FindAction("TrackedDeviceOrientation", throwIfNotFound: true);
		m_UI_OpenMap = m_UI.FindAction("OpenMap", throwIfNotFound: true);
		m_UI_Whistle = m_UI.FindAction("Whistle", throwIfNotFound: true);
		m_UI_VC = m_UI.FindAction("VC", throwIfNotFound: true);
		m_UI_OpenChat = m_UI.FindAction("OpenChat", throwIfNotFound: true);
		m_UI_KeyboardUpperCase = m_UI.FindAction("KeyboardUpperCase", throwIfNotFound: true);
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy(asset);
	}

	public bool Contains(InputAction action)
	{
		return asset.Contains(action);
	}

	public IEnumerator<InputAction> GetEnumerator()
	{
		return asset.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Enable()
	{
		asset.Enable();
	}

	public void Disable()
	{
		asset.Disable();
	}

	public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
	{
		return asset.FindAction(actionNameOrId, throwIfNotFound);
	}

	public int FindBinding(InputBinding bindingMask, out InputAction action)
	{
		return asset.FindBinding(bindingMask, out action);
	}
}
