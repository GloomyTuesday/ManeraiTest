using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.InputSystem
{
    public class InputSystemObserver : MonoBehaviour
    {
        [SerializeField]
        private bool _cursorLocked; 

        [Space(15)]
        [SerializeField]
        private InputEventsSrc _inputEventsSrc;

        private InputActionsSrc _inputActionsSrc;

        private InputAction _pointerPositionAction;
        private InputAction _pointerDownAction;
        private bool PointerDownFlag { get; set; }

        private InputAction _touch0PositionAction;
        private InputAction _touch0DownAction;
        private bool Touch0DownFlag { get; set; }

        private InputAction _touch1PositionAction;
        private InputAction _touch1DownAction;
        private bool Touch1DownFlag { get; set; }

        private InputAction _mouseLeftBtnDownAction;
        private InputAction _mouseMiddleBtnDownAction;
        private InputAction _mouseRightBtnDownAction;

        private bool MouseLeftBtnDownFlag { get; set; }
        private bool MouseMiddleBtnDownFlag { get; set; }
        private bool MouseRightBtnDownFlag { get; set; }

        private InputAction _mouseScrollY;

        private InputAction _mouseMoveAction;
        private InputAction _mouseMoveDeltaAction;

        private bool CursorLocked { get; set; }

        #region Keyboard input
        private InputAction _keyboardCtrl;
        private bool CtrlDown { get; set; }
        #endregion

        /// <summary>
        ///     As keys is used Hashcode
        /// </summary>
        private Dictionary<int, IRaycastConstraint> RaycastConstraintDictionary { get; set; } = new Dictionary<int, IRaycastConstraint>();

        /// <summary>
        ///     Contains information if input of certain type hit an rect transform constraint
        /// </summary>
        public Dictionary<InputTypeId, bool> RectTransformHitDictionary { get; set; } = new Dictionary<InputTypeId, bool>(); 

        private IInputEventsCallbackInvoker _IInputEventsCallbackInvoker;
        private IInputEventsCallbackInvoker IInputEventsCallbackInvoker
        {
            get
            {
                if (_IInputEventsCallbackInvoker == null)
                    _IInputEventsCallbackInvoker = _inputEventsSrc;

                return _IInputEventsCallbackInvoker;
            }
        }

        private IInputEventsHandler _IInputEventsHandler;
        private IInputEventsHandler IInputEventsHandler
        {
            get
            {
                if (_IInputEventsHandler == null)
                    _IInputEventsHandler = _inputEventsSrc;

                return _IInputEventsHandler;
            }
        }

        private Vector2 MousePreviousAbsolutePosiiton { get; set; } = Vector2.zero;
        private Vector2 MouseDragDelta { get; set; } = Vector2.zero;
        private Vector2 MousePosition { get; set; } = Vector2.zero;

        private Vector2 PointerPosition { get; set; } = Vector2.zero;
        private Vector2 Touch0Position { get; set; } = Vector2.zero;
        private Vector2 Touch1Position { get; set; } = Vector2.zero;

        private bool IsUsedTouchScreen { get; set; }

        private void OnValidate()
        {
            _inputEventsSrc.CursorLock = _cursorLocked;
        }

        private void OnEnable()
        {
            _inputEventsSrc.CursorLock = _cursorLocked;

            IsUsedTouchScreen = Input.touchSupported;

            _inputActionsSrc = new InputActionsSrc();

            _pointerPositionAction = _inputActionsSrc.Player.PointerPosition;
            _pointerPositionAction.Enable();
            _pointerDownAction = _inputActionsSrc.Player.PointerDown;
            _pointerDownAction.Enable();

            _touch0PositionAction = _inputActionsSrc.Player.Touch0Position;
            _touch0PositionAction.Enable();
            _touch0DownAction = _inputActionsSrc.Player.Touch0Down;
            _touch0DownAction.Enable();

            _touch1PositionAction = _inputActionsSrc.Player.Touch1Position;
            _touch1PositionAction.Enable();
            _touch1DownAction = _inputActionsSrc.Player.Touch1Down;
            _touch1DownAction.Enable();

            _mouseLeftBtnDownAction = _inputActionsSrc.Player.MouseLeftBtnDown;
            _mouseLeftBtnDownAction.Enable(); 
            _mouseMiddleBtnDownAction = _inputActionsSrc.Player.MouseMiddleBtnDown;
            _mouseMiddleBtnDownAction.Enable();
            _mouseRightBtnDownAction = _inputActionsSrc.Player.MouseRightBtnDown;
            _mouseRightBtnDownAction.Enable();

            _mouseScrollY = _inputActionsSrc.Player.MouseScrollY;
            _mouseScrollY.Enable();

            _mouseMoveAction = _inputActionsSrc.Player.MouseMove;
            _mouseMoveAction.Enable();

            _mouseMoveDeltaAction = _inputActionsSrc.Player.MouseMoveDelta;
            _mouseMoveDeltaAction.Enable();

            #region Keyboard input

            _keyboardCtrl = _inputActionsSrc.Keyboard.Ctrl;
            _keyboardCtrl.Enable();

            #endregion

            CursorLocked = IInputEventsHandler.CursorLock; 

            Subscribe();
        }

        private void OnDisable()
        {
            _pointerPositionAction.Disable();
            _touch1PositionAction.Disable();
            _pointerDownAction.Disable();
            _mouseLeftBtnDownAction.Disable(); 
            _mouseMiddleBtnDownAction.Disable(); 
            _mouseRightBtnDownAction.Disable();
            _mouseScrollY.Disable();
            _mouseMoveAction.Disable();
            _mouseMoveDeltaAction.Disable();

            _keyboardCtrl.Disable();
            Unsubscribe();
        }

        private void Subscribe()
        {
            //  _inputActionsSrc.Player.MouseMove.performed += MouseMovePerformed;

            IInputEventsHandler.OnSetCursorLockValue += SetCursorLockValue;

            IInputEventsHandler.OnRegisterRaycastConstraint += RegisterRaycastConstraint;
            IInputEventsHandler.OnUnRegisterRaycastConstraint += UnRegisterRaycastConstraint;

            IInputEventsHandler.OnIsRectConstraintHit += IsRectConstraintHit;

            //  ----------------------------------------    Pointer
            _inputActionsSrc.Player.PointerPosition.performed += PointerPositionPerformed;
            _inputActionsSrc.Player.PointerDown.performed += PointerDownPerformed;
            _inputActionsSrc.Player.PointerDown.canceled += PointerUpPerformed;
            IInputEventsHandler.OnGetPointerPosition += GetPointerPosition;
            IInputEventsHandler.OnIsPointerDown += IsPointerDown;

            //  ----------------------------------------    Touch 0
            _inputActionsSrc.Player.Touch0Position.performed += Touch0PositionPerformed;
            _inputActionsSrc.Player.Touch0Down.started += Touch0DownPerformed;
            _inputActionsSrc.Player.Touch0Down.canceled += Touch0UpPerformed;
            IInputEventsHandler.OnGetTouch0Position += GetTouch0Position;
            IInputEventsHandler.OnIsTouch0Down += IsTouch0Down;

            //  ----------------------------------------    Touch 1
            _inputActionsSrc.Player.Touch1Position.performed += Touch1PositionPerformed;
            _inputActionsSrc.Player.Touch1Down.started += Touch1DownPerformed;
            _inputActionsSrc.Player.Touch1Down.canceled += Touch1UpPerformed;
            IInputEventsHandler.OnGetTouch1Position += GetTouch1Position;
            IInputEventsHandler.OnIsTouch1Down += IsTouch1Down;

            //  ----------------------------------------
            IInputEventsHandler.OnSetCursorLockValue += SetCursorLockValue;

            //  ----------------------------------------    Mouse buttons

            _inputActionsSrc.Player.MouseLeftBtnDown.started += MouseLeftBtnDownPerformed;
            _inputActionsSrc.Player.MouseLeftBtnDown.canceled += MouseLeftBtnUpPerformed;

            _inputActionsSrc.Player.MouseMiddleBtnDown.started += MouseMiddleBtnDownPerformed;
            _inputActionsSrc.Player.MouseMiddleBtnDown.canceled += MouseMiddleBtnUpPerformed;
            IInputEventsHandler.OnGetMouseMiddleBtnPointerPosition += GetMouseMiddleBtnPointerPosition;

            _inputActionsSrc.Player.MouseRightBtnDown.started += MouseRightBtnDownPerformed;
            _inputActionsSrc.Player.MouseRightBtnDown.canceled += MouseRightBtnUpPerformed;

            IInputEventsHandler.OnIsMouseMiddleBtnDown += IsMouseMiddleBtnDown;

            //  ----------------------------------------    Mouse Y scroll
            _inputActionsSrc.Player.MouseScrollY.performed += MouseScrollYPerformed;
            _inputActionsSrc.Player.MouseScrollY.canceled += MouseScrollYCanceled;

            //  ----------------------------------------    Mouse

            _inputActionsSrc.Player.MouseMoveDelta.performed += MouseMoveDeltaPerformed;
            //  _inputActionsSrc.Player.MouseMove.performed += MouseMovePerformed;

            //  ----------------------------------------    Keyboard input
            #region Keyboard input
            _inputActionsSrc.Keyboard.Ctrl.performed += KeyboardCtrlDown;
            _inputActionsSrc.Keyboard.Ctrl.canceled += KeyboardCtrlUp;
            IInputEventsHandler.OnIsKeyboardCtrlDown += IsKeyboardControlDown;
            #endregion
        }

        private void Unsubscribe()
        {
           //   _inputActionsSrc.Player.MouseMove.performed -= MouseMovePerformed;

            IInputEventsHandler.OnSetCursorLockValue -= SetCursorLockValue;

            IInputEventsHandler.OnRegisterRaycastConstraint -= RegisterRaycastConstraint;
            IInputEventsHandler.OnUnRegisterRaycastConstraint -= UnRegisterRaycastConstraint;

            IInputEventsHandler.OnIsRectConstraintHit -= IsRectConstraintHit;

            //  ----------------------------------------    Pointer
            _inputActionsSrc.Player.PointerPosition.performed -= PointerPositionPerformed;
            _inputActionsSrc.Player.PointerDown.performed -= PointerDownPerformed;
            _inputActionsSrc.Player.PointerDown.canceled -= PointerUpPerformed;
            IInputEventsHandler.OnGetPointerPosition -= GetPointerPosition;
            IInputEventsHandler.OnIsPointerDown -= IsPointerDown;

            //  ----------------------------------------    Touch 0
            _inputActionsSrc.Player.Touch0Position.performed -= Touch0PositionPerformed;
            _inputActionsSrc.Player.Touch0Down.started -= Touch0DownPerformed;
            _inputActionsSrc.Player.Touch0Down.canceled -= Touch0UpPerformed;
            IInputEventsHandler.OnGetTouch0Position -= GetTouch0Position;
            IInputEventsHandler.OnIsTouch0Down -= IsTouch0Down;

            //  ----------------------------------------    Touch 1
            _inputActionsSrc.Player.Touch1Position.performed -= Touch1PositionPerformed;
            _inputActionsSrc.Player.Touch1Down.started -= Touch1DownPerformed;
            _inputActionsSrc.Player.Touch1Down.canceled -= Touch1UpPerformed;
            IInputEventsHandler.OnGetTouch1Position -= GetTouch1Position;
            IInputEventsHandler.OnIsTouch1Down -= IsTouch1Down;

            //  ----------------------------------------    Mouse buttons

            _inputActionsSrc.Player.MouseLeftBtnDown.started -= MouseLeftBtnDownPerformed;
            _inputActionsSrc.Player.MouseLeftBtnDown.canceled -= MouseLeftBtnUpPerformed;

            _inputActionsSrc.Player.MouseMiddleBtnDown.started -= MouseMiddleBtnDownPerformed;
            _inputActionsSrc.Player.MouseMiddleBtnDown.canceled -= MouseMiddleBtnUpPerformed;
            IInputEventsHandler.OnGetMouseMiddleBtnPointerPosition -= GetMouseMiddleBtnPointerPosition;

            _inputActionsSrc.Player.MouseRightBtnDown.started -= MouseRightBtnDownPerformed;
            _inputActionsSrc.Player.MouseRightBtnDown.canceled -= MouseRightBtnUpPerformed;

            IInputEventsHandler.OnIsMouseMiddleBtnDown -= IsMouseMiddleBtnDown;

            //  ----------------------------------------    Mouse Y scroll
            _inputActionsSrc.Player.MouseScrollY.performed -= MouseScrollYPerformed;
            _inputActionsSrc.Player.MouseScrollY.canceled -= MouseScrollYCanceled;

            //  ----------------------------------------
            IInputEventsHandler.OnSetCursorLockValue -= SetCursorLockValue;

            //  ----------------------------------------    Mouse
            _inputActionsSrc.Player.MouseMoveDelta.performed -= MouseMoveDeltaPerformed;
            //  _inputActionsSrc.Player.MouseMove.performed -= MouseMovePerformed;

            //  ----------------------------------------    Keyboard input
            #region Keyboard input
            _inputActionsSrc.Keyboard.Ctrl.performed -= KeyboardCtrlDown;
            _inputActionsSrc.Keyboard.Ctrl.canceled -= KeyboardCtrlUp;
            IInputEventsHandler.OnIsKeyboardCtrlDown -= IsKeyboardControlDown;
            #endregion
        }

        private void SetCursorLockValue(bool value)
        {
           CursorLocked = value;
        }

        private void RegisterRaycastConstraint(IRaycastConstraint constraint)
        {
            var hashCode = constraint.GetHashCode();
            if (RaycastConstraintDictionary.ContainsKey(hashCode)) return;

            RaycastConstraintDictionary.Add(hashCode, constraint); 
        }

        private void UnRegisterRaycastConstraint(IRaycastConstraint constraint)
        {
            var hashCode = constraint.GetHashCode();
            if (!RaycastConstraintDictionary.ContainsKey(hashCode)) return;

            RaycastConstraintDictionary.Remove(hashCode);
        }

        private bool IsRectConstraintHit(InputTypeId inputTypeId)
        {
            if (!RectTransformHitDictionary.ContainsKey(inputTypeId)) return false;

            return RectTransformHitDictionary[inputTypeId]; 
        }

        private bool GetHitData(Vector2 position)
        {
            foreach (var item in RaycastConstraintDictionary)
                if (item.Value.ContactCheck(position)) return true; 
            
            return false; 
        }

        private void UpdateHitResult( Vector2 position, InputTypeId inputTypeId )
        {
            if (!RectTransformHitDictionary.ContainsKey(inputTypeId))
                RectTransformHitDictionary.Add(inputTypeId, false);

            var hitResult = GetHitData(position);
            RectTransformHitDictionary[inputTypeId] = hitResult;
        }

        private void CloseHitResult(InputTypeId inputTypeId)
        {
            if (!RectTransformHitDictionary.ContainsKey(inputTypeId))
            {
                RectTransformHitDictionary.Add(inputTypeId, false);
                return; 
            }

            RectTransformHitDictionary[inputTypeId] = false;
        }

        private void MouseMoveDeltaPerformed(InputAction.CallbackContext context)
        {
            MouseDragDelta = context.ReadValue<Vector2>();
            MousePosition = _mouseMoveAction.ReadValue<Vector2>();

            IInputEventsCallbackInvoker.MouseDragDelta(MouseDragDelta);
            IInputEventsCallbackInvoker.MouseDrag(MousePosition);
        }

        /*
        private void MouseMovePerformed(InputAction.CallbackContext context)
        {
            //  MouseDragDelta = _mouseMoveAction.ReadValue<Vector2>();
            MouseDragDelta = _mouseMoveDeltaAction.ReadValue<Vector2>();

            MouseAbsolutePosition = _mouseMoveAction.ReadValue<Vector2>();
            //  MouseDragDelta = MouseAbsolutePosition - MousePreviousAbsolutePosiiton;

            MousePreviousAbsolutePosiiton = MouseAbsolutePosition;

            MouseNormalizedPosition = CursorLocked ? 
                Vector2.zero : 
                new Vector2(
                            MouseAbsolutePosition.x / Screen.width,
                            MouseAbsolutePosition.y / Screen.height
                            );

            IInputEventsCallbackInvoker.MouseDragDelta(MouseDragDelta); 
            IInputEventsCallbackInvoker.MouseDrag(MouseNormalizedPosition); 
        }
        */

        private Vector2 GetPointerPosition() => PointerPosition;

        //  As touch 0 is used pointer events(but still do not delete this code in case touch0 is needed )
        private Vector2 GetTouch0Position() => Touch0Position;
        private Vector2 GetTouch1Position() => Touch1Position;
        private Vector2 GetMouseMiddleBtnPointerPosition() => MouseDragDelta;

        private bool IsPointerDown() => PointerDownFlag;
        private bool IsTouch0Down() => Touch0DownFlag;
        private bool IsTouch1Down() => Touch1DownFlag;
        private bool IsMouseMiddleBtnDown() => MouseMiddleBtnDownFlag;

        //  ----------------------------------------    Pointer
        #region Pointer 
        private void PointerPositionPerformed(InputAction.CallbackContext context)
        {
            PointerPosition = _pointerPositionAction.ReadValue<Vector2>();
            UpdateHitResult(PointerPosition, InputTypeId.Pointer);
            IInputEventsCallbackInvoker.PointerDrag(PointerPosition);
        }

        private void PointerDownPerformed(InputAction.CallbackContext context)
        {
            //  In order to prevent double calling for devices with touchscreen Pointer event is called from Touch0DownPerformed
            PointerPosition = _pointerPositionAction.ReadValue<Vector2>();
            PointerDownFlag = true;
            UpdateHitResult(PointerPosition, InputTypeId.Pointer);
            IInputEventsCallbackInvoker.PointerDown(PointerPosition); 
        }

        private void PointerUpPerformed(InputAction.CallbackContext context)
        {
            //  In order to prevent double calling for devices with touchscreen Pointer event is called from Touch0UpPerformed
            PointerDownFlag = false;
            CloseHitResult(InputTypeId.Pointer);
            IInputEventsCallbackInvoker.PointerUp(PointerPosition);
        }
        #endregion

        //  ----------------------------------------    Touch 0
        #region Touch 0
        private void Touch0PositionPerformed(InputAction.CallbackContext context)
        {
            if (!Touch0DownFlag) return; 
            Touch0Position = _touch0PositionAction.ReadValue<Vector2>();
            UpdateHitResult(Touch0Position, InputTypeId.Touch0); 
            IInputEventsCallbackInvoker.Touch0Drag(Touch0Position);
        }

        private void Touch0DownPerformed(InputAction.CallbackContext context)
        {
            Touch0DownFlag = true;
            Touch0Position = _touch0PositionAction.ReadValue<Vector2>();
            UpdateHitResult(Touch0Position, InputTypeId.Touch0);
            IInputEventsCallbackInvoker.Touch0Down(Touch0Position);
        }

        private void Touch0UpPerformed(InputAction.CallbackContext context)
        {
            Touch0DownFlag = false;
            CloseHitResult(InputTypeId.Touch0);
            IInputEventsCallbackInvoker.Touch0Up(Touch0Position);
        }
        #endregion

        //  ----------------------------------------    Touch 1
        #region Touch 1
        private void Touch1PositionPerformed(InputAction.CallbackContext context)
        {
            if (!Touch1DownFlag) return;
            Touch1Position = _touch1PositionAction.ReadValue<Vector2>();
            UpdateHitResult(Touch1Position, InputTypeId.Touch1);
            IInputEventsCallbackInvoker.Touch1Drag(Touch1Position);
           
        }

        private void Touch1DownPerformed(InputAction.CallbackContext context)
        {
            Touch1DownFlag = true;
            Touch1Position = _touch1PositionAction.ReadValue<Vector2>();
            UpdateHitResult(Touch1Position, InputTypeId.Touch1);
            IInputEventsCallbackInvoker.Touch1Down(Touch1Position);
        }

        private void Touch1UpPerformed(InputAction.CallbackContext context)
        {
            Touch1DownFlag = false;
            CloseHitResult(InputTypeId.Touch1);
            IInputEventsCallbackInvoker.Touch1Up(Touch1Position);
        }
        #endregion

        //  ----------------------------------------    Mouse middle button
        #region Mouse middle button

        private void MouseLeftBtnDownPerformed(InputAction.CallbackContext context)
        {
            MouseLeftBtnDownFlag = true;
            UpdateHitResult(MousePosition, InputTypeId.MouseLeftBtn);
            IInputEventsCallbackInvoker.MouseLeftBtnDown(MousePosition); 
        }

        private void MouseLeftBtnUpPerformed(InputAction.CallbackContext context)
        {
            MouseLeftBtnDownFlag = false;
            IInputEventsCallbackInvoker.MouseLeftBtnUp(MousePosition);
        }

        private void MouseRightBtnDownPerformed(InputAction.CallbackContext context)
        {
            MouseRightBtnDownFlag = true;
            UpdateHitResult(MousePosition, InputTypeId.MouseRightBtn);
            IInputEventsCallbackInvoker.MouseRightBtnDown(MousePosition);
        }

        private void MouseRightBtnUpPerformed(InputAction.CallbackContext context)
        {
            MouseRightBtnDownFlag = false;
            IInputEventsCallbackInvoker.MouseRightBtnUp(MousePosition);
        }

        private void MouseMiddleBtnDownPerformed(InputAction.CallbackContext context)
        {
            MouseMiddleBtnDownFlag = true;
            UpdateHitResult(MousePosition, InputTypeId.MouseMiddleBtn);
            IInputEventsCallbackInvoker.MouseMiddleBtnDown(MousePosition); 
        }

        private void MouseMiddleBtnUpPerformed(InputAction.CallbackContext context)
        {
            CloseHitResult(InputTypeId.MouseMiddleBtn);
            IInputEventsCallbackInvoker.MouseMiddleBtnUp(MousePosition);
            MouseMiddleBtnDownFlag = false;
        }
        #endregion

        //  ----------------------------------------    Mouse Y scroll
        #region Mouse Y scroll
        private void MouseScrollYPerformed(InputAction.CallbackContext context)
        {
            var mouseScrollValue = context.ReadValue<float>();

            if (mouseScrollValue == 0)
            {
                IInputEventsCallbackInvoker.MouseScrollYCanceled(mouseScrollValue);
            }

            IInputEventsCallbackInvoker.MouseScrollY(mouseScrollValue);
        }

        private void MouseScrollYCanceled(InputAction.CallbackContext context)
        {
            IInputEventsCallbackInvoker.MouseScrollYCanceled(0);
        }
        #endregion


        //  ----------------------------------------    Keyboard input
        #region Keyboard input
        private void KeyboardCtrlDown(InputAction.CallbackContext context)
        {
            CtrlDown = true; 
            IInputEventsCallbackInvoker.KeyboardControlDown(); 
        }

        private void KeyboardCtrlUp(InputAction.CallbackContext context)
        {
            CtrlDown = false; 
            IInputEventsCallbackInvoker.KeyboardCtrlUp();
        }

        private bool IsKeyboardControlDown() => CtrlDown; 
        #endregion
    }
}

