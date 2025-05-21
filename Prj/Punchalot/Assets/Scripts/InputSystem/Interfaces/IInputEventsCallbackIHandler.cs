using System;
using UnityEngine;

namespace Scripts.InputSystem
{
    public interface IInputEventsCallbackIHandler 
    {
        //  ----------------------------------------    Pointer 
        public event Action<Vector2> OnPointerDown;
        public event Action<Vector2> OnPointerDrag;
        public event Action<Vector2> OnPointerUp;

        //  ----------------------------------------    Touch 0
        public event Action<Vector2> OnTouch0Down;
        public event Action<Vector2> OnTouch0Drag;
        public event Action<Vector2> OnTouch0Up;

        //  ----------------------------------------    Touch 1
        public event Action<Vector2> OnTouch1Down;
        public event Action<Vector2> OnTouch1Drag;
        public event Action<Vector2> OnTouch1Up;

        //  ---------------------------------------- 

        public event Action<bool> OnCursorLockValueUpdated;

        //  ----------------------------------------    Moue drag
        public event Action<Vector2> OnMouseDragDelta;
        public event Action<Vector2> OnMouseDrag;

        //  ----------------------------------------    Mouse buttons

        public event Action<Vector2> OnMouseLeftBtnDown;
        public event Action<Vector2> OnMouseLeftBtnUp;

        public event Action<Vector2> OnMouseMiddleBtnDown;
        public event Action<Vector2> OnMouseMiddleBtnDrag;
        public event Action<Vector2> OnMouseMiddleBtnUp;

        public event Action<Vector2> OnMouseRightBtnDown;
        public event Action<Vector2> OnMouseRightBtnUp;

        //  ----------------------------------------    Mouse Y scroll
        public event Action<float> OnMouseScrollY;
        public event Action<float> OnMouseScrollYCanceled;


        //  ----------------------------------------    Keyboard input
        #region Keyboard input

        //  ----------------------------------------    Control
        public event Action OnKeyboardCtrlDown;
        public event Action OnKeyboardCtrlUp;

        #endregion
    }
}
