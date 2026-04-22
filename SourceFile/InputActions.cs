using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class InputActions : IInputActionCollection2, IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
{
	public struct UIActions(InputActions wrapper)
	{
		private InputActions m_Wrapper = wrapper;

		public InputAction Navigate => m_Wrapper.m_UI_Navigate;

		public InputAction Submit => m_Wrapper.m_UI_Submit;

		public InputAction Cancel => m_Wrapper.m_UI_Cancel;

		public InputAction Point => m_Wrapper.m_UI_Point;

		public InputAction Click => m_Wrapper.m_UI_Click;

		public InputAction ScrollWheel => m_Wrapper.m_UI_ScrollWheel;

		public InputAction MiddleClick => m_Wrapper.m_UI_MiddleClick;

		public InputAction RightClick => m_Wrapper.m_UI_RightClick;

		public InputAction TrackedDevicePosition => m_Wrapper.m_UI_TrackedDevicePosition;

		public InputAction TrackedDeviceOrientation => m_Wrapper.m_UI_TrackedDeviceOrientation;

		public InputAction ScrollSublist => m_Wrapper.m_UI_ScrollSublist;

		public InputAction Pause => m_Wrapper.m_UI_Pause;

		public bool enabled => Get().enabled;

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
				Submit.started += instance.OnSubmit;
				Submit.performed += instance.OnSubmit;
				Submit.canceled += instance.OnSubmit;
				Cancel.started += instance.OnCancel;
				Cancel.performed += instance.OnCancel;
				Cancel.canceled += instance.OnCancel;
				Point.started += instance.OnPoint;
				Point.performed += instance.OnPoint;
				Point.canceled += instance.OnPoint;
				Click.started += instance.OnClick;
				Click.performed += instance.OnClick;
				Click.canceled += instance.OnClick;
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
				ScrollSublist.started += instance.OnScrollSublist;
				ScrollSublist.performed += instance.OnScrollSublist;
				ScrollSublist.canceled += instance.OnScrollSublist;
				Pause.started += instance.OnPause;
				Pause.performed += instance.OnPause;
				Pause.canceled += instance.OnPause;
			}
		}

		private void UnregisterCallbacks(IUIActions instance)
		{
			Navigate.started -= instance.OnNavigate;
			Navigate.performed -= instance.OnNavigate;
			Navigate.canceled -= instance.OnNavigate;
			Submit.started -= instance.OnSubmit;
			Submit.performed -= instance.OnSubmit;
			Submit.canceled -= instance.OnSubmit;
			Cancel.started -= instance.OnCancel;
			Cancel.performed -= instance.OnCancel;
			Cancel.canceled -= instance.OnCancel;
			Point.started -= instance.OnPoint;
			Point.performed -= instance.OnPoint;
			Point.canceled -= instance.OnPoint;
			Click.started -= instance.OnClick;
			Click.performed -= instance.OnClick;
			Click.canceled -= instance.OnClick;
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
			ScrollSublist.started -= instance.OnScrollSublist;
			ScrollSublist.performed -= instance.OnScrollSublist;
			ScrollSublist.canceled -= instance.OnScrollSublist;
			Pause.started -= instance.OnPause;
			Pause.performed -= instance.OnPause;
			Pause.canceled -= instance.OnPause;
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

	public struct MovementActions(InputActions wrapper)
	{
		private InputActions m_Wrapper = wrapper;

		public InputAction Move => m_Wrapper.m_Movement_Move;

		public InputAction Look => m_Wrapper.m_Movement_Look;

		public InputAction Dodge => m_Wrapper.m_Movement_Dodge;

		public InputAction Slide => m_Wrapper.m_Movement_Slide;

		public InputAction Jump => m_Wrapper.m_Movement_Jump;

		public bool enabled => Get().enabled;

		public InputActionMap Get()
		{
			return m_Wrapper.m_Movement;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(MovementActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IMovementActions instance)
		{
			if (instance != null && !m_Wrapper.m_MovementActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_MovementActionsCallbackInterfaces.Add(instance);
				Move.started += instance.OnMove;
				Move.performed += instance.OnMove;
				Move.canceled += instance.OnMove;
				Look.started += instance.OnLook;
				Look.performed += instance.OnLook;
				Look.canceled += instance.OnLook;
				Dodge.started += instance.OnDodge;
				Dodge.performed += instance.OnDodge;
				Dodge.canceled += instance.OnDodge;
				Slide.started += instance.OnSlide;
				Slide.performed += instance.OnSlide;
				Slide.canceled += instance.OnSlide;
				Jump.started += instance.OnJump;
				Jump.performed += instance.OnJump;
				Jump.canceled += instance.OnJump;
			}
		}

		private void UnregisterCallbacks(IMovementActions instance)
		{
			Move.started -= instance.OnMove;
			Move.performed -= instance.OnMove;
			Move.canceled -= instance.OnMove;
			Look.started -= instance.OnLook;
			Look.performed -= instance.OnLook;
			Look.canceled -= instance.OnLook;
			Dodge.started -= instance.OnDodge;
			Dodge.performed -= instance.OnDodge;
			Dodge.canceled -= instance.OnDodge;
			Slide.started -= instance.OnSlide;
			Slide.performed -= instance.OnSlide;
			Slide.canceled -= instance.OnSlide;
			Jump.started -= instance.OnJump;
			Jump.performed -= instance.OnJump;
			Jump.canceled -= instance.OnJump;
		}

		public void RemoveCallbacks(IMovementActions instance)
		{
			if (m_Wrapper.m_MovementActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IMovementActions instance)
		{
			foreach (IMovementActions movementActionsCallbackInterface in m_Wrapper.m_MovementActionsCallbackInterfaces)
			{
				UnregisterCallbacks(movementActionsCallbackInterface);
			}
			m_Wrapper.m_MovementActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct FistActions(InputActions wrapper)
	{
		private InputActions m_Wrapper = wrapper;

		public InputAction Punch => m_Wrapper.m_Fist_Punch;

		public InputAction ChangeFist => m_Wrapper.m_Fist_ChangeFist;

		public InputAction PunchFeedbacker => m_Wrapper.m_Fist_PunchFeedbacker;

		public InputAction PunchKnuckleblaster => m_Wrapper.m_Fist_PunchKnuckleblaster;

		public InputAction Hook => m_Wrapper.m_Fist_Hook;

		public bool enabled => Get().enabled;

		public InputActionMap Get()
		{
			return m_Wrapper.m_Fist;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(FistActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IFistActions instance)
		{
			if (instance != null && !m_Wrapper.m_FistActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_FistActionsCallbackInterfaces.Add(instance);
				Punch.started += instance.OnPunch;
				Punch.performed += instance.OnPunch;
				Punch.canceled += instance.OnPunch;
				ChangeFist.started += instance.OnChangeFist;
				ChangeFist.performed += instance.OnChangeFist;
				ChangeFist.canceled += instance.OnChangeFist;
				PunchFeedbacker.started += instance.OnPunchFeedbacker;
				PunchFeedbacker.performed += instance.OnPunchFeedbacker;
				PunchFeedbacker.canceled += instance.OnPunchFeedbacker;
				PunchKnuckleblaster.started += instance.OnPunchKnuckleblaster;
				PunchKnuckleblaster.performed += instance.OnPunchKnuckleblaster;
				PunchKnuckleblaster.canceled += instance.OnPunchKnuckleblaster;
				Hook.started += instance.OnHook;
				Hook.performed += instance.OnHook;
				Hook.canceled += instance.OnHook;
			}
		}

		private void UnregisterCallbacks(IFistActions instance)
		{
			Punch.started -= instance.OnPunch;
			Punch.performed -= instance.OnPunch;
			Punch.canceled -= instance.OnPunch;
			ChangeFist.started -= instance.OnChangeFist;
			ChangeFist.performed -= instance.OnChangeFist;
			ChangeFist.canceled -= instance.OnChangeFist;
			PunchFeedbacker.started -= instance.OnPunchFeedbacker;
			PunchFeedbacker.performed -= instance.OnPunchFeedbacker;
			PunchFeedbacker.canceled -= instance.OnPunchFeedbacker;
			PunchKnuckleblaster.started -= instance.OnPunchKnuckleblaster;
			PunchKnuckleblaster.performed -= instance.OnPunchKnuckleblaster;
			PunchKnuckleblaster.canceled -= instance.OnPunchKnuckleblaster;
			Hook.started -= instance.OnHook;
			Hook.performed -= instance.OnHook;
			Hook.canceled -= instance.OnHook;
		}

		public void RemoveCallbacks(IFistActions instance)
		{
			if (m_Wrapper.m_FistActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IFistActions instance)
		{
			foreach (IFistActions fistActionsCallbackInterface in m_Wrapper.m_FistActionsCallbackInterfaces)
			{
				UnregisterCallbacks(fistActionsCallbackInterface);
			}
			m_Wrapper.m_FistActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct WeaponActions(InputActions wrapper)
	{
		private InputActions m_Wrapper = wrapper;

		public InputAction PrimaryFire => m_Wrapper.m_Weapon_PrimaryFire;

		public InputAction SecondaryFire => m_Wrapper.m_Weapon_SecondaryFire;

		public InputAction NextVariation => m_Wrapper.m_Weapon_NextVariation;

		public InputAction PreviousVariation => m_Wrapper.m_Weapon_PreviousVariation;

		public InputAction Revolver => m_Wrapper.m_Weapon_Revolver;

		public InputAction Shotgun => m_Wrapper.m_Weapon_Shotgun;

		public InputAction Nailgun => m_Wrapper.m_Weapon_Nailgun;

		public InputAction Railcannon => m_Wrapper.m_Weapon_Railcannon;

		public InputAction RocketLauncher => m_Wrapper.m_Weapon_RocketLauncher;

		public InputAction SpawnerArm => m_Wrapper.m_Weapon_SpawnerArm;

		public InputAction NextWeapon => m_Wrapper.m_Weapon_NextWeapon;

		public InputAction PreviousWeapon => m_Wrapper.m_Weapon_PreviousWeapon;

		public InputAction LastUsedWeapon => m_Wrapper.m_Weapon_LastUsedWeapon;

		public InputAction WheelLook => m_Wrapper.m_Weapon_WheelLook;

		public InputAction VariationSlot1 => m_Wrapper.m_Weapon_VariationSlot1;

		public InputAction VariationSlot2 => m_Wrapper.m_Weapon_VariationSlot2;

		public InputAction VariationSlot3 => m_Wrapper.m_Weapon_VariationSlot3;

		public bool enabled => Get().enabled;

		public InputActionMap Get()
		{
			return m_Wrapper.m_Weapon;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(WeaponActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IWeaponActions instance)
		{
			if (instance != null && !m_Wrapper.m_WeaponActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_WeaponActionsCallbackInterfaces.Add(instance);
				PrimaryFire.started += instance.OnPrimaryFire;
				PrimaryFire.performed += instance.OnPrimaryFire;
				PrimaryFire.canceled += instance.OnPrimaryFire;
				SecondaryFire.started += instance.OnSecondaryFire;
				SecondaryFire.performed += instance.OnSecondaryFire;
				SecondaryFire.canceled += instance.OnSecondaryFire;
				NextVariation.started += instance.OnNextVariation;
				NextVariation.performed += instance.OnNextVariation;
				NextVariation.canceled += instance.OnNextVariation;
				PreviousVariation.started += instance.OnPreviousVariation;
				PreviousVariation.performed += instance.OnPreviousVariation;
				PreviousVariation.canceled += instance.OnPreviousVariation;
				Revolver.started += instance.OnRevolver;
				Revolver.performed += instance.OnRevolver;
				Revolver.canceled += instance.OnRevolver;
				Shotgun.started += instance.OnShotgun;
				Shotgun.performed += instance.OnShotgun;
				Shotgun.canceled += instance.OnShotgun;
				Nailgun.started += instance.OnNailgun;
				Nailgun.performed += instance.OnNailgun;
				Nailgun.canceled += instance.OnNailgun;
				Railcannon.started += instance.OnRailcannon;
				Railcannon.performed += instance.OnRailcannon;
				Railcannon.canceled += instance.OnRailcannon;
				RocketLauncher.started += instance.OnRocketLauncher;
				RocketLauncher.performed += instance.OnRocketLauncher;
				RocketLauncher.canceled += instance.OnRocketLauncher;
				SpawnerArm.started += instance.OnSpawnerArm;
				SpawnerArm.performed += instance.OnSpawnerArm;
				SpawnerArm.canceled += instance.OnSpawnerArm;
				NextWeapon.started += instance.OnNextWeapon;
				NextWeapon.performed += instance.OnNextWeapon;
				NextWeapon.canceled += instance.OnNextWeapon;
				PreviousWeapon.started += instance.OnPreviousWeapon;
				PreviousWeapon.performed += instance.OnPreviousWeapon;
				PreviousWeapon.canceled += instance.OnPreviousWeapon;
				LastUsedWeapon.started += instance.OnLastUsedWeapon;
				LastUsedWeapon.performed += instance.OnLastUsedWeapon;
				LastUsedWeapon.canceled += instance.OnLastUsedWeapon;
				WheelLook.started += instance.OnWheelLook;
				WheelLook.performed += instance.OnWheelLook;
				WheelLook.canceled += instance.OnWheelLook;
				VariationSlot1.started += instance.OnVariationSlot1;
				VariationSlot1.performed += instance.OnVariationSlot1;
				VariationSlot1.canceled += instance.OnVariationSlot1;
				VariationSlot2.started += instance.OnVariationSlot2;
				VariationSlot2.performed += instance.OnVariationSlot2;
				VariationSlot2.canceled += instance.OnVariationSlot2;
				VariationSlot3.started += instance.OnVariationSlot3;
				VariationSlot3.performed += instance.OnVariationSlot3;
				VariationSlot3.canceled += instance.OnVariationSlot3;
			}
		}

		private void UnregisterCallbacks(IWeaponActions instance)
		{
			PrimaryFire.started -= instance.OnPrimaryFire;
			PrimaryFire.performed -= instance.OnPrimaryFire;
			PrimaryFire.canceled -= instance.OnPrimaryFire;
			SecondaryFire.started -= instance.OnSecondaryFire;
			SecondaryFire.performed -= instance.OnSecondaryFire;
			SecondaryFire.canceled -= instance.OnSecondaryFire;
			NextVariation.started -= instance.OnNextVariation;
			NextVariation.performed -= instance.OnNextVariation;
			NextVariation.canceled -= instance.OnNextVariation;
			PreviousVariation.started -= instance.OnPreviousVariation;
			PreviousVariation.performed -= instance.OnPreviousVariation;
			PreviousVariation.canceled -= instance.OnPreviousVariation;
			Revolver.started -= instance.OnRevolver;
			Revolver.performed -= instance.OnRevolver;
			Revolver.canceled -= instance.OnRevolver;
			Shotgun.started -= instance.OnShotgun;
			Shotgun.performed -= instance.OnShotgun;
			Shotgun.canceled -= instance.OnShotgun;
			Nailgun.started -= instance.OnNailgun;
			Nailgun.performed -= instance.OnNailgun;
			Nailgun.canceled -= instance.OnNailgun;
			Railcannon.started -= instance.OnRailcannon;
			Railcannon.performed -= instance.OnRailcannon;
			Railcannon.canceled -= instance.OnRailcannon;
			RocketLauncher.started -= instance.OnRocketLauncher;
			RocketLauncher.performed -= instance.OnRocketLauncher;
			RocketLauncher.canceled -= instance.OnRocketLauncher;
			SpawnerArm.started -= instance.OnSpawnerArm;
			SpawnerArm.performed -= instance.OnSpawnerArm;
			SpawnerArm.canceled -= instance.OnSpawnerArm;
			NextWeapon.started -= instance.OnNextWeapon;
			NextWeapon.performed -= instance.OnNextWeapon;
			NextWeapon.canceled -= instance.OnNextWeapon;
			PreviousWeapon.started -= instance.OnPreviousWeapon;
			PreviousWeapon.performed -= instance.OnPreviousWeapon;
			PreviousWeapon.canceled -= instance.OnPreviousWeapon;
			LastUsedWeapon.started -= instance.OnLastUsedWeapon;
			LastUsedWeapon.performed -= instance.OnLastUsedWeapon;
			LastUsedWeapon.canceled -= instance.OnLastUsedWeapon;
			WheelLook.started -= instance.OnWheelLook;
			WheelLook.performed -= instance.OnWheelLook;
			WheelLook.canceled -= instance.OnWheelLook;
			VariationSlot1.started -= instance.OnVariationSlot1;
			VariationSlot1.performed -= instance.OnVariationSlot1;
			VariationSlot1.canceled -= instance.OnVariationSlot1;
			VariationSlot2.started -= instance.OnVariationSlot2;
			VariationSlot2.performed -= instance.OnVariationSlot2;
			VariationSlot2.canceled -= instance.OnVariationSlot2;
			VariationSlot3.started -= instance.OnVariationSlot3;
			VariationSlot3.performed -= instance.OnVariationSlot3;
			VariationSlot3.canceled -= instance.OnVariationSlot3;
		}

		public void RemoveCallbacks(IWeaponActions instance)
		{
			if (m_Wrapper.m_WeaponActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IWeaponActions instance)
		{
			foreach (IWeaponActions weaponActionsCallbackInterface in m_Wrapper.m_WeaponActionsCallbackInterfaces)
			{
				UnregisterCallbacks(weaponActionsCallbackInterface);
			}
			m_Wrapper.m_WeaponActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct HUDActions(InputActions wrapper)
	{
		private InputActions m_Wrapper = wrapper;

		public InputAction Stats => m_Wrapper.m_HUD_Stats;

		public bool enabled => Get().enabled;

		public InputActionMap Get()
		{
			return m_Wrapper.m_HUD;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(HUDActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IHUDActions instance)
		{
			if (instance != null && !m_Wrapper.m_HUDActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_HUDActionsCallbackInterfaces.Add(instance);
				Stats.started += instance.OnStats;
				Stats.performed += instance.OnStats;
				Stats.canceled += instance.OnStats;
			}
		}

		private void UnregisterCallbacks(IHUDActions instance)
		{
			Stats.started -= instance.OnStats;
			Stats.performed -= instance.OnStats;
			Stats.canceled -= instance.OnStats;
		}

		public void RemoveCallbacks(IHUDActions instance)
		{
			if (m_Wrapper.m_HUDActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IHUDActions instance)
		{
			foreach (IHUDActions hUDActionsCallbackInterface in m_Wrapper.m_HUDActionsCallbackInterfaces)
			{
				UnregisterCallbacks(hUDActionsCallbackInterface);
			}
			m_Wrapper.m_HUDActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public interface IUIActions
	{
		void OnNavigate(CallbackContext context);

		void OnSubmit(CallbackContext context);

		void OnCancel(CallbackContext context);

		void OnPoint(CallbackContext context);

		void OnClick(CallbackContext context);

		void OnScrollWheel(CallbackContext context);

		void OnMiddleClick(CallbackContext context);

		void OnRightClick(CallbackContext context);

		void OnTrackedDevicePosition(CallbackContext context);

		void OnTrackedDeviceOrientation(CallbackContext context);

		void OnScrollSublist(CallbackContext context);

		void OnPause(CallbackContext context);
	}

	public interface IMovementActions
	{
		void OnMove(CallbackContext context);

		void OnLook(CallbackContext context);

		void OnDodge(CallbackContext context);

		void OnSlide(CallbackContext context);

		void OnJump(CallbackContext context);
	}

	public interface IFistActions
	{
		void OnPunch(CallbackContext context);

		void OnChangeFist(CallbackContext context);

		void OnPunchFeedbacker(CallbackContext context);

		void OnPunchKnuckleblaster(CallbackContext context);

		void OnHook(CallbackContext context);
	}

	public interface IWeaponActions
	{
		void OnPrimaryFire(CallbackContext context);

		void OnSecondaryFire(CallbackContext context);

		void OnNextVariation(CallbackContext context);

		void OnPreviousVariation(CallbackContext context);

		void OnRevolver(CallbackContext context);

		void OnShotgun(CallbackContext context);

		void OnNailgun(CallbackContext context);

		void OnRailcannon(CallbackContext context);

		void OnRocketLauncher(CallbackContext context);

		void OnSpawnerArm(CallbackContext context);

		void OnNextWeapon(CallbackContext context);

		void OnPreviousWeapon(CallbackContext context);

		void OnLastUsedWeapon(CallbackContext context);

		void OnWheelLook(CallbackContext context);

		void OnVariationSlot1(CallbackContext context);

		void OnVariationSlot2(CallbackContext context);

		void OnVariationSlot3(CallbackContext context);
	}

	public interface IHUDActions
	{
		void OnStats(CallbackContext context);
	}

	private readonly InputActionMap m_UI;

	private List<IUIActions> m_UIActionsCallbackInterfaces = new List<IUIActions>();

	private readonly InputAction m_UI_Navigate;

	private readonly InputAction m_UI_Submit;

	private readonly InputAction m_UI_Cancel;

	private readonly InputAction m_UI_Point;

	private readonly InputAction m_UI_Click;

	private readonly InputAction m_UI_ScrollWheel;

	private readonly InputAction m_UI_MiddleClick;

	private readonly InputAction m_UI_RightClick;

	private readonly InputAction m_UI_TrackedDevicePosition;

	private readonly InputAction m_UI_TrackedDeviceOrientation;

	private readonly InputAction m_UI_ScrollSublist;

	private readonly InputAction m_UI_Pause;

	private readonly InputActionMap m_Movement;

	private List<IMovementActions> m_MovementActionsCallbackInterfaces = new List<IMovementActions>();

	private readonly InputAction m_Movement_Move;

	private readonly InputAction m_Movement_Look;

	private readonly InputAction m_Movement_Dodge;

	private readonly InputAction m_Movement_Slide;

	private readonly InputAction m_Movement_Jump;

	private readonly InputActionMap m_Fist;

	private List<IFistActions> m_FistActionsCallbackInterfaces = new List<IFistActions>();

	private readonly InputAction m_Fist_Punch;

	private readonly InputAction m_Fist_ChangeFist;

	private readonly InputAction m_Fist_PunchFeedbacker;

	private readonly InputAction m_Fist_PunchKnuckleblaster;

	private readonly InputAction m_Fist_Hook;

	private readonly InputActionMap m_Weapon;

	private List<IWeaponActions> m_WeaponActionsCallbackInterfaces = new List<IWeaponActions>();

	private readonly InputAction m_Weapon_PrimaryFire;

	private readonly InputAction m_Weapon_SecondaryFire;

	private readonly InputAction m_Weapon_NextVariation;

	private readonly InputAction m_Weapon_PreviousVariation;

	private readonly InputAction m_Weapon_Revolver;

	private readonly InputAction m_Weapon_Shotgun;

	private readonly InputAction m_Weapon_Nailgun;

	private readonly InputAction m_Weapon_Railcannon;

	private readonly InputAction m_Weapon_RocketLauncher;

	private readonly InputAction m_Weapon_SpawnerArm;

	private readonly InputAction m_Weapon_NextWeapon;

	private readonly InputAction m_Weapon_PreviousWeapon;

	private readonly InputAction m_Weapon_LastUsedWeapon;

	private readonly InputAction m_Weapon_WheelLook;

	private readonly InputAction m_Weapon_VariationSlot1;

	private readonly InputAction m_Weapon_VariationSlot2;

	private readonly InputAction m_Weapon_VariationSlot3;

	private readonly InputActionMap m_HUD;

	private List<IHUDActions> m_HUDActionsCallbackInterfaces = new List<IHUDActions>();

	private readonly InputAction m_HUD_Stats;

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

	public UIActions UI => new UIActions(this);

	public MovementActions Movement => new MovementActions(this);

	public FistActions Fist => new FistActions(this);

	public WeaponActions Weapon => new WeaponActions(this);

	public HUDActions HUD => new HUDActions(this);

	public InputControlScheme KeyboardMouseScheme
	{
		get
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			if (m_KeyboardMouseSchemeIndex == -1)
			{
				m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard & Mouse");
			}
			return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
		}
	}

	public InputControlScheme GamepadScheme
	{
		get
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			if (m_GamepadSchemeIndex == -1)
			{
				m_GamepadSchemeIndex = asset.FindControlSchemeIndex("Gamepad");
			}
			return asset.controlSchemes[m_GamepadSchemeIndex];
		}
	}

	public InputActions()
	{
		asset = InputActionAsset.FromJson("{\n    \"name\": \"InputActions\",\n    \"maps\": [\n        {\n            \"name\": \"UI\",\n            \"id\": \"272f6d14-89ba-496f-b7ff-215263d3219f\",\n            \"actions\": [\n                {\n                    \"name\": \"Navigate\",\n                    \"type\": \"Value\",\n                    \"id\": \"c95b2375-e6d9-4b88-9c4c-c5e76515df4b\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Submit\",\n                    \"type\": \"Button\",\n                    \"id\": \"7607c7b6-cd76-4816-beef-bd0341cfe950\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Cancel\",\n                    \"type\": \"Button\",\n                    \"id\": \"15cef263-9014-4fd5-94d9-4e4a6234a6ef\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Point\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"32b35790-4ed0-4e9a-aa41-69ac6d629449\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Click\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"3c7022bf-7922-4f7c-a998-c437916075ad\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"ScrollWheel\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"0489e84a-4833-4c40-bfae-cea84b696689\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"MiddleClick\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"dad70c86-b58c-4b17-88ad-f5e53adf419e\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"RightClick\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"44b200b1-1557-4083-816c-b22cbdf77ddf\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"TrackedDevicePosition\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"24908448-c609-4bc3-a128-ea258674378a\",\n                    \"expectedControlType\": \"Vector3\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"TrackedDeviceOrientation\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"9caa3d8a-6b2f-4e8e-8bad-6ede561bd9be\",\n                    \"expectedControlType\": \"Quaternion\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ScrollSublist\",\n                    \"type\": \"Value\",\n                    \"id\": \"6a48eebe-4a36-47fa-a511-0489aa7c315f\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Pause\",\n                    \"type\": \"Button\",\n                    \"id\": \"97668417-6564-4b1c-8acf-ec55ca459e96\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"Gamepad\",\n                    \"id\": \"809f371f-c5e2-4e7a-83a1-d867598f40dd\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"14a5d6e8-4aaf-4119-a9ef-34b8c2c548bf\",\n                    \"path\": \"<Gamepad>/leftStick/up\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"2db08d65-c5fb-421b-983f-c71163608d67\",\n                    \"path\": \"<Gamepad>/leftStick/down\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"8ba04515-75aa-45de-966d-393d9bbd1c14\",\n                    \"path\": \"<Gamepad>/leftStick/left\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"fcd248ae-a788-4676-a12e-f4d81205600b\",\n                    \"path\": \"<Gamepad>/leftStick/right\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"fb8277d4-c5cd-4663-9dc7-ee3f0b506d90\",\n                    \"path\": \"<Gamepad>/dpad\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"Joystick\",\n                    \"id\": \"e25d9774-381c-4a61-b47c-7b6b299ad9f9\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"Keyboard\",\n                    \"id\": \"ff527021-f211-4c02-933e-5976594c46ed\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"eb480147-c587-4a33-85ed-eb0ab9942c43\",\n                    \"path\": \"<Keyboard>/upArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"85d264ad-e0a0-4565-b7ff-1a37edde51ac\",\n                    \"path\": \"<Keyboard>/downArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"cea9b045-a000-445b-95b8-0c171af70a3b\",\n                    \"path\": \"<Keyboard>/leftArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"4cda81dc-9edd-4e03-9d7c-a71a14345d0b\",\n                    \"path\": \"<Keyboard>/rightArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"9e92bb26-7e3b-4ec4-b06b-3c8f8e498ddc\",\n                    \"path\": \"*/{Submit}\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Submit\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"82627dcc-3b13-4ba9-841d-e4b746d6553e\",\n                    \"path\": \"*/{Cancel}\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Cancel\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"c52c8e0b-8179-41d3-b8a1-d149033bbe86\",\n                    \"path\": \"<Mouse>/position\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Point\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e1394cbc-336e-44ce-9ea8-6007ed6193f7\",\n                    \"path\": \"<Pen>/position\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Point\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"4faf7dc9-b979-4210-aa8c-e808e1ef89f5\",\n                    \"path\": \"<Mouse>/leftButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard & Mouse\",\n                    \"action\": \"Click\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"8d66d5ba-88d7-48e6-b1cd-198bbfef7ace\",\n                    \"path\": \"<Pen>/tip\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard & Mouse\",\n                    \"action\": \"Click\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"38c99815-14ea-4617-8627-164d27641299\",\n                    \"path\": \"<Mouse>/scroll\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard & Mouse\",\n                    \"action\": \"ScrollWheel\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"24066f69-da47-44f3-a07e-0015fb02eb2e\",\n                    \"path\": \"<Mouse>/middleButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard & Mouse\",\n                    \"action\": \"MiddleClick\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"4c191405-5738-4d4b-a523-c6a301dbf754\",\n                    \"path\": \"<Mouse>/rightButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard & Mouse\",\n                    \"action\": \"RightClick\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"Gamepad\",\n                    \"id\": \"9463292f-a2ff-4649-a9c3-067667e79776\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"ScrollSublist\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"9bc3b935-5dc2-4404-8371-7b2485dff1ce\",\n                    \"path\": \"<Gamepad>/rightStick/up\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ScrollSublist\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"7ade12cb-a9f7-4a5a-96ae-e9fe630b2134\",\n                    \"path\": \"<Gamepad>/rightStick/down\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ScrollSublist\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"5f79f2a3-f8fc-432c-ae63-af30cb39d55d\",\n                    \"path\": \"<Gamepad>/rightStick/left\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ScrollSublist\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"2868eca4-a73a-4197-9c69-ddeb36bcf151\",\n                    \"path\": \"<Gamepad>/rightStick/right\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ScrollSublist\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"5b1930a7-f80a-47a3-a5fc-ae79cf605e47\",\n                    \"path\": \"<Keyboard>/escape\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Pause\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"4fa5dbc3-d196-44a5-b7b0-0ea98081c9de\",\n                    \"path\": \"<Gamepad>/start\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Pause\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"Movement\",\n            \"id\": \"e96bd924-debe-467e-b08f-8b58a3e62a8e\",\n            \"actions\": [\n                {\n                    \"name\": \"Move\",\n                    \"type\": \"Value\",\n                    \"id\": \"cb0ce271-47aa-4c76-82e1-9c39bb2a7eb3\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Look\",\n                    \"type\": \"Value\",\n                    \"id\": \"7ef2043f-2b68-4e31-9373-8e06a7366297\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Dodge\",\n                    \"type\": \"Button\",\n                    \"id\": \"33b91605-d5d0-4013-9789-7592610c7cf8\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Slide\",\n                    \"type\": \"Value\",\n                    \"id\": \"624c1b28-2b1e-4f89-bbb5-17cc64cba594\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Jump\",\n                    \"type\": \"Button\",\n                    \"id\": \"0fb09bdc-b16f-45ea-b0e1-9ae06cd92ce9\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"WASD\",\n                    \"id\": \"a431c87e-8bbb-44ed-9798-da71bb2c7d86\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"8d3ef497-0b3d-4b31-86a5-5663b2ba2ffa\",\n                    \"path\": \"<Keyboard>/w\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"4ef08ab9-0134-405a-b2ba-eac89353df45\",\n                    \"path\": \"<Keyboard>/s\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"2e9b3987-e852-45b8-b589-b6fb48e6ff84\",\n                    \"path\": \"<Keyboard>/a\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"6cea9ced-9601-4881-b8df-2faf804438b7\",\n                    \"path\": \"<Keyboard>/d\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"d82497ad-d674-4a6b-b956-80038bb4412f\",\n                    \"path\": \"<Gamepad>/leftStick\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"a29c100e-7b84-4f6f-8dfd-b76e859796d6\",\n                    \"path\": \"<Mouse>/delta\",\n                    \"interactions\": \"\",\n                    \"processors\": \"ScaleVector2(x=0.05,y=0.05)\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Look\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"5ae79f6e-9f44-48f0-ba8c-1e011df2ef42\",\n                    \"path\": \"<Gamepad>/rightStick\",\n                    \"interactions\": \"\",\n                    \"processors\": \"StickDeadzone,ScaleVector2DeltaTime,ScaleVector2(x=50,y=50)\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Look\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"27ff067c-1c14-4964-938d-802b6623cdff\",\n                    \"path\": \"<Keyboard>/leftShift\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Dodge\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"5382b377-bc60-452a-ad20-9d6bf609c9af\",\n                    \"path\": \"<Gamepad>/leftStickPress\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Dodge\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"ebd4e61a-b0c2-4c72-9496-26b0f5b21e11\",\n                    \"path\": \"<Keyboard>/leftCtrl\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Slide\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"bbf96605-8c7d-4975-8ba4-d68695687bda\",\n                    \"path\": \"<Gamepad>/buttonEast\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Slide\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"0903b737-349d-4732-81f0-48b0ca8de9bd\",\n                    \"path\": \"<Gamepad>/rightStickPress\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Slide\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"a4c4b550-cd65-4036-af7a-ef2378e4308e\",\n                    \"path\": \"<Keyboard>/space\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Jump\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"cea2ad63-b360-4664-b975-b8504e6ff901\",\n                    \"path\": \"<Gamepad>/buttonSouth\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Jump\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"Fist\",\n            \"id\": \"40b954b6-30b1-4981-92c3-1d4f9a6c17f2\",\n            \"actions\": [\n                {\n                    \"name\": \"Punch\",\n                    \"type\": \"Button\",\n                    \"id\": \"869113fb-bc1d-4a2b-9dec-eb22e9945d80\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Change Fist\",\n                    \"type\": \"Button\",\n                    \"id\": \"522acb46-9df3-45f6-939e-a51a03d400d9\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Punch (Feedbacker)\",\n                    \"type\": \"Button\",\n                    \"id\": \"c445e670-6075-441b-8b22-7dba0e8d42d4\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Punch (Knuckleblaster)\",\n                    \"type\": \"Button\",\n                    \"id\": \"bb0cab8e-f957-4032-8191-8ece32b5a14c\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Hook\",\n                    \"type\": \"Button\",\n                    \"id\": \"10724cb7-a181-40fc-9b86-c4d0ea53a8bb\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"\",\n                    \"id\": \"67198acc-fa9a-481d-9d9a-d76dc60d8715\",\n                    \"path\": \"<Keyboard>/f\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Punch\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"60b0442a-1009-4d7e-a114-dbd44433d3a4\",\n                    \"path\": \"<Gamepad>/buttonWest\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Punch\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"23f28a42-e471-4602-9417-1f4658a13806\",\n                    \"path\": \"<Keyboard>/r\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Hook\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"Gamepad\",\n                    \"id\": \"9cd09ed9-ad4b-460a-b14c-4119905d26bc\",\n                    \"path\": \"OneModifier\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Hook\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"modifier\",\n                    \"id\": \"829dbbb7-04f0-450b-a532-364e192a9940\",\n                    \"path\": \"<Gamepad>/leftShoulder\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Hook\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"binding\",\n                    \"id\": \"0ff7fe63-f392-4798-8a1b-482e56b30dd3\",\n                    \"path\": \"<Gamepad>/rightShoulder\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Hook\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"fdfc4715-57ab-49d2-bdd5-ec5ac779724c\",\n                    \"path\": \"<Keyboard>/g\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Change Fist\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"655dd916-82a6-4eeb-afaa-9b7dcad0c182\",\n                    \"path\": \"<Gamepad>/dpad/down\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Change Fist\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"Weapon\",\n            \"id\": \"6b9886b7-c7e2-4025-93cd-cda0cdac230a\",\n            \"actions\": [\n                {\n                    \"name\": \"Primary Fire\",\n                    \"type\": \"Value\",\n                    \"id\": \"02a37447-3cc3-449b-b39d-fdc77f11f764\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Secondary Fire\",\n                    \"type\": \"Button\",\n                    \"id\": \"15bebcb7-335a-459f-87b9-13e61e491c11\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Next Variation\",\n                    \"type\": \"Button\",\n                    \"id\": \"18f559c7-89b9-4171-ad32-a91c22e3e5ff\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Previous Variation\",\n                    \"type\": \"Button\",\n                    \"id\": \"b3234529-5d0b-4e5f-b8eb-11ff37849585\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Revolver\",\n                    \"type\": \"Button\",\n                    \"id\": \"e2f53064-bda2-4654-b185-29dca350eaa0\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"Tap(duration=1.401298E-45,pressPoint=0.5)\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Shotgun\",\n                    \"type\": \"Button\",\n                    \"id\": \"0763f1a7-6c7e-49a1-9a3c-4b146c27e04c\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"Tap(duration=1.401298E-45)\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Nailgun\",\n                    \"type\": \"Button\",\n                    \"id\": \"4ed2d805-e1e4-4025-ac6f-3432e06b1ce5\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"Tap(duration=1.401298E-45)\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Railcannon\",\n                    \"type\": \"Button\",\n                    \"id\": \"c7f3d928-3d5b-45d5-9d90-6b319e8311fb\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"Tap(duration=1.401298E-45)\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Rocket Launcher\",\n                    \"type\": \"Button\",\n                    \"id\": \"7849759d-8615-4f07-8de0-a28f26126ea2\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"Tap(duration=1.401298E-45)\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Spawner Arm\",\n                    \"type\": \"Button\",\n                    \"id\": \"3089e314-3d30-45ce-a07f-7d1cf04bb37f\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"Tap(duration=1.401298E-45)\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Next Weapon\",\n                    \"type\": \"Button\",\n                    \"id\": \"d7c48ca7-dbd6-4f18-80f3-9f18e5c8ab1f\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Previous Weapon\",\n                    \"type\": \"Button\",\n                    \"id\": \"76594c5b-371a-4c5b-a9be-8a023cbe4cff\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Last Used Weapon\",\n                    \"type\": \"Button\",\n                    \"id\": \"ba0a138f-bf7b-4f3e-8bc8-d404f83020c1\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"WheelLook\",\n                    \"type\": \"Value\",\n                    \"id\": \"dc2612ed-51f2-4421-b9a6-f2e82ca48fa3\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Variation Slot 1\",\n                    \"type\": \"Button\",\n                    \"id\": \"03278bce-78f4-4124-99a3-5f08692f158f\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Variation Slot 2\",\n                    \"type\": \"Button\",\n                    \"id\": \"edecf429-1346-41df-80bd-f5fdc3865b3f\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Variation Slot 3\",\n                    \"type\": \"Button\",\n                    \"id\": \"77f2f8d4-0612-407f-8628-9f02978ea515\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"\",\n                    \"id\": \"93335d5d-6f0a-4f2d-8637-b20440416ad9\",\n                    \"path\": \"<Mouse>/leftButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Primary Fire\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"6bc77874-60ce-4a73-aa47-17440efd7e34\",\n                    \"path\": \"<Gamepad>/rightTrigger\",\n                    \"interactions\": \"Hold\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Primary Fire\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"eb709d49-df96-435a-b74b-842378d8101f\",\n                    \"path\": \"<Mouse>/rightButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Secondary Fire\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"13d52b9e-bc2d-4c2c-ba4e-c800584b05ad\",\n                    \"path\": \"<Gamepad>/leftTrigger\",\n                    \"interactions\": \"Hold\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Secondary Fire\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"5a614cd8-3f18-44ca-85e3-46e8fa633d73\",\n                    \"path\": \"<Keyboard>/e\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Next Variation\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"832b43cb-5024-4933-9050-c19e5b4c26ae\",\n                    \"path\": \"<Gamepad>/buttonNorth\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Next Variation\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"7a005e4a-4688-4c48-91ad-aff216b70387\",\n                    \"path\": \"<Keyboard>/1\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Revolver\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"173d4f78-e12d-4366-97fb-98c9b4053ce3\",\n                    \"path\": \"<Keyboard>/2\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Shotgun\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"99b705e4-4419-4585-9682-5e66be07a062\",\n                    \"path\": \"<Keyboard>/3\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Nailgun\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"b160dc8e-9231-4830-b57d-11feca18f885\",\n                    \"path\": \"<Keyboard>/4\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Railcannon\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"23aa32cc-3c8e-444c-a6ef-6df11f9674da\",\n                    \"path\": \"<Keyboard>/5\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Rocket Launcher\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e93c0bc5-5274-46a6-9c6c-d51176e13d98\",\n                    \"path\": \"<Keyboard>/6\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Spawner Arm\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"ab01fc7f-eb43-4fc4-b5d0-452819ca3158\",\n                    \"path\": \"<Gamepad>/dpad/up\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Last Used Weapon\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"4308293d-7e6d-46df-ab18-3a434ad9c88d\",\n                    \"path\": \"<Gamepad>/rightStick\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"WheelLook\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"ba90b80a-c77e-41f1-9c18-8defadce77c3\",\n                    \"path\": \"<Mouse>/delta\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"WheelLook\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"4e7c2a98-432f-4ea5-9f19-a6d5e94ce4f1\",\n                    \"path\": \"<Gamepad>/rightShoulder\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Next Weapon\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"53776e46-c411-4ead-9029-28a726300449\",\n                    \"path\": \"<Gamepad>/leftShoulder\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Previous Weapon\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"284d0b5f-8f69-4ddc-944a-49fdc92b78a0\",\n                    \"path\": \"<Keyboard>/q\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Previous Variation\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"HUD\",\n            \"id\": \"f220c680-c3c2-4bae-a82a-81a1f1bb9d0c\",\n            \"actions\": [\n                {\n                    \"name\": \"Stats\",\n                    \"type\": \"Button\",\n                    \"id\": \"296faedb-712e-44d6-baf4-661c82946274\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"\",\n                    \"id\": \"d4ab4fd3-dfe1-47d3-8390-bc85ee66f1e4\",\n                    \"path\": \"<Keyboard>/tab\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard & Mouse\",\n                    \"action\": \"Stats\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"915dede1-3f36-4970-ab7e-7c0a74bedb87\",\n                    \"path\": \"<Gamepad>/select\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Stats\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        }\n    ],\n    \"controlSchemes\": [\n        {\n            \"name\": \"Keyboard & Mouse\",\n            \"bindingGroup\": \"Keyboard & Mouse\",\n            \"devices\": [\n                {\n                    \"devicePath\": \"<Keyboard>\",\n                    \"isOptional\": false,\n                    \"isOR\": false\n                },\n                {\n                    \"devicePath\": \"<Mouse>\",\n                    \"isOptional\": false,\n                    \"isOR\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"Gamepad\",\n            \"bindingGroup\": \"Gamepad\",\n            \"devices\": [\n                {\n                    \"devicePath\": \"<Gamepad>\",\n                    \"isOptional\": false,\n                    \"isOR\": false\n                }\n            ]\n        }\n    ]\n}");
		m_UI = asset.FindActionMap("UI", true);
		m_UI_Navigate = m_UI.FindAction("Navigate", true);
		m_UI_Submit = m_UI.FindAction("Submit", true);
		m_UI_Cancel = m_UI.FindAction("Cancel", true);
		m_UI_Point = m_UI.FindAction("Point", true);
		m_UI_Click = m_UI.FindAction("Click", true);
		m_UI_ScrollWheel = m_UI.FindAction("ScrollWheel", true);
		m_UI_MiddleClick = m_UI.FindAction("MiddleClick", true);
		m_UI_RightClick = m_UI.FindAction("RightClick", true);
		m_UI_TrackedDevicePosition = m_UI.FindAction("TrackedDevicePosition", true);
		m_UI_TrackedDeviceOrientation = m_UI.FindAction("TrackedDeviceOrientation", true);
		m_UI_ScrollSublist = m_UI.FindAction("ScrollSublist", true);
		m_UI_Pause = m_UI.FindAction("Pause", true);
		m_Movement = asset.FindActionMap("Movement", true);
		m_Movement_Move = m_Movement.FindAction("Move", true);
		m_Movement_Look = m_Movement.FindAction("Look", true);
		m_Movement_Dodge = m_Movement.FindAction("Dodge", true);
		m_Movement_Slide = m_Movement.FindAction("Slide", true);
		m_Movement_Jump = m_Movement.FindAction("Jump", true);
		m_Fist = asset.FindActionMap("Fist", true);
		m_Fist_Punch = m_Fist.FindAction("Punch", true);
		m_Fist_ChangeFist = m_Fist.FindAction("Change Fist", true);
		m_Fist_PunchFeedbacker = m_Fist.FindAction("Punch (Feedbacker)", true);
		m_Fist_PunchKnuckleblaster = m_Fist.FindAction("Punch (Knuckleblaster)", true);
		m_Fist_Hook = m_Fist.FindAction("Hook", true);
		m_Weapon = asset.FindActionMap("Weapon", true);
		m_Weapon_PrimaryFire = m_Weapon.FindAction("Primary Fire", true);
		m_Weapon_SecondaryFire = m_Weapon.FindAction("Secondary Fire", true);
		m_Weapon_NextVariation = m_Weapon.FindAction("Next Variation", true);
		m_Weapon_PreviousVariation = m_Weapon.FindAction("Previous Variation", true);
		m_Weapon_Revolver = m_Weapon.FindAction("Revolver", true);
		m_Weapon_Shotgun = m_Weapon.FindAction("Shotgun", true);
		m_Weapon_Nailgun = m_Weapon.FindAction("Nailgun", true);
		m_Weapon_Railcannon = m_Weapon.FindAction("Railcannon", true);
		m_Weapon_RocketLauncher = m_Weapon.FindAction("Rocket Launcher", true);
		m_Weapon_SpawnerArm = m_Weapon.FindAction("Spawner Arm", true);
		m_Weapon_NextWeapon = m_Weapon.FindAction("Next Weapon", true);
		m_Weapon_PreviousWeapon = m_Weapon.FindAction("Previous Weapon", true);
		m_Weapon_LastUsedWeapon = m_Weapon.FindAction("Last Used Weapon", true);
		m_Weapon_WheelLook = m_Weapon.FindAction("WheelLook", true);
		m_Weapon_VariationSlot1 = m_Weapon.FindAction("Variation Slot 1", true);
		m_Weapon_VariationSlot2 = m_Weapon.FindAction("Variation Slot 2", true);
		m_Weapon_VariationSlot3 = m_Weapon.FindAction("Variation Slot 3", true);
		m_HUD = asset.FindActionMap("HUD", true);
		m_HUD_Stats = m_HUD.FindAction("Stats", true);
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy((UnityEngine.Object)(object)asset);
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
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return asset.FindBinding(bindingMask, ref action);
	}
}
