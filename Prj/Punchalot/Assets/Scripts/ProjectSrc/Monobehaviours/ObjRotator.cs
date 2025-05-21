using Scripts.BaseSystems;
using Scripts.BaseSystems.Core;
using Scripts.InputSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.ProjectSrc
{
    public class ObjRotator : MonoBehaviour
    {
        [SerializeField]
        private float _rotationSpeed = 1f;

        [Header("Upvector axis: Y")]
        [SerializeField]
        private Transform _upVectorSource;

        [Space(15)]
        [SerializeField]
        [FilterByType(typeof(IFlagHolder))]
        private Object _flagHolderObj; 

        [SerializeField]
        [FilterByType(typeof(IInputEventsCallbackIHandler))]
        private Object _inputEventsCallbackIHandler;


        private IInputEventsCallbackIHandler _iInputEventsCallbackIHandler;
        private IInputEventsCallbackIHandler IInputEventsCallbackIHandler
        {
            get
            {
                if (_iInputEventsCallbackIHandler == null)
                    _iInputEventsCallbackIHandler = _inputEventsCallbackIHandler.GetComponent<IInputEventsCallbackIHandler>();

                return _iInputEventsCallbackIHandler;
            }
        }

        private IFlagHolder _iFlagHolder;
        private IFlagHolder IFlagHolder
        {
            get
            {
                if (_iFlagHolder == null)
                    _iFlagHolder = _flagHolderObj.GetComponent<IFlagHolder>();

                return _iFlagHolder;
            }
        }

        private Vector2 PreviousPointerPosition { get; set; }

        private bool IsDragSubscribed { get; set; }

        private bool _isRotating;
        private bool IsRotating
        {
            get => _isRotating;

            set
            {
                _isRotating = value;
                /*
                if(value)
                {
                    if (IsDragSubscribed) return;

                    IsDragSubscribed = value; 
                    IInputEventsCallbackIHandler.OnPointerDrag += PointerDrag;
                    return; 
                }

                if (!IsDragSubscribed) return;

                IsDragSubscribed = value;
                IInputEventsCallbackIHandler.OnPointerDrag -= PointerDrag;
                */
            }
        }

        private void OnEnable()
        {
            if (_upVectorSource == null)
                Debug.LogError("\t _upVectorSource is NULL! ");

            IsRotating = IFlagHolder.Value;  

            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            IFlagHolder.OnNewValueAssigned += NewRotationFlagValueAssigned;

            IInputEventsCallbackIHandler.OnMouseDragDelta += ControllerDrag;
         //   IInputEventsCallbackIHandler.OnPointerUp += PointerUp;
        }

        private void Unsubscribe()
        {
            IFlagHolder.OnNewValueAssigned -= NewRotationFlagValueAssigned;

            IInputEventsCallbackIHandler.OnMouseDragDelta -= ControllerDrag;
        //    IInputEventsCallbackIHandler.OnPointerUp -= PointerUp;
        }


        private void NewRotationFlagValueAssigned( bool newRotationFlag)
        {
            IsRotating = newRotationFlag;
        }

        private void ControllerDrag(Vector2 vector)
        {
            if (!IsRotating) return;

            Rotate(vector); 
            //  Rotate(PreviousPointerPosition, vector);
        }

        private void Rotate(Vector2 mouseDragDelta)
        {
            if (_upVectorSource == null)
                Debug.LogError("\t _upVectorSource is NULL! ");

            var eulerAngle = transform.rotation.eulerAngles;
            transform.Rotate(_upVectorSource.up,   mouseDragDelta.x * _rotationSpeed, Space.World);
            transform.Rotate(transform.right, -mouseDragDelta.y * _rotationSpeed, Space.World);
        }

        private void Rotate(Vector2 pointerDown, Vector2 poinerDrag)
        {
            Vector2 delta = poinerDrag - pointerDown;
            //  _rotationSpeed

            if (_upVectorSource == null)
                Debug.LogError("\t _upVectorSource is NULL! ");

            var eulerAngle = transform.rotation.eulerAngles;
            transform.Rotate(_upVectorSource.up, eulerAngle .x - delta.x * _rotationSpeed, Space.World);
            transform.Rotate(transform.right, eulerAngle.y + delta.y * _rotationSpeed, Space.World);

            PreviousPointerPosition = poinerDrag;
        }
    }
}
