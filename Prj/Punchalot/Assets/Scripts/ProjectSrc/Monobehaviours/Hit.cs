using Scripts.BaseSystems;
using Scripts.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.ProjectSrc
{
    public class Hit : MonoBehaviour
    {
        private enum Input { MouseLeftBtn = 0, MouseRightBtn = 1 };

        [Uneditable]
        [SerializeField]
        private float _hitPower; 

        [SerializeField]
        private Rigidbody _rigidbodyToTransform;
        private Transform _rigidbodyParent; 

        [Space(15)]
        [SerializeField]
        private Transform _loadPoint;

        [SerializeField]
        private Transform _hitPoint;

        [SerializeField]
        private Transform _initPoint;

        [SerializeField]
        private Transform _currentPosition;

        [Space(15)]
        [Header("Transform speed animation curve")]
        [SerializeField]
        private AnimationCurve _loadAnimCurve;
        [SerializeField]
        private AnimationCurve _hitAnimCurve;
        [SerializeField]
        private AnimationCurve _restoreToInitAnimCurve;

        [Space(15)]
        [SerializeField]
        private float _loadTime;

        [SerializeField]
        private float _hitTime;

        [SerializeField]
        private float _restoreToInitTime;

        [Space(15)]
        [SerializeField]
        private Input _inputType;

        [Space(15)]
        [SerializeField]
        [FilterByType(typeof(IInputEventsCallbackIHandler))]
        private UnityEngine.Object _inputEventsCallbackIHandlerObj;

        [SerializeField]
        [FilterByType(typeof(ITimeUtilitiesEventsCallbackHandler))]
        private UnityEngine.Object _timeUtilitiesEventsCallbackHandlerObj;

        private IInputEventsCallbackIHandler _iInputEventsCallbackIHandler;
        private IInputEventsCallbackIHandler IInputEventsCallbackIHandler
        {
            get
            {
                if (_iInputEventsCallbackIHandler == null)
                    _iInputEventsCallbackIHandler = _inputEventsCallbackIHandlerObj.GetComponent<IInputEventsCallbackIHandler>();

                return _iInputEventsCallbackIHandler;
            }
        }

        private ITimeUtilitiesEventsCallbackHandler _ITimeUtilitiesEventsCallbackHandler;
        private ITimeUtilitiesEventsCallbackHandler ITimeUtilitiesEventsCallbackHandler
        {
            get
            {
                if (_ITimeUtilitiesEventsCallbackHandler == null)
                    _ITimeUtilitiesEventsCallbackHandler = _timeUtilitiesEventsCallbackHandlerObj.GetComponent<ITimeUtilitiesEventsCallbackHandler>();

                return _ITimeUtilitiesEventsCallbackHandler;
            }
        }

        private bool SubscribedInputEvents { get; set; }
        private bool SubscribedUpdateEvents { get; set; }

        /*
        private Vector3 StartPositionWorld { get; set; }
        private Vector3 StartScaleLocal { get; set; }
        private Quaternion StartRotationWorld { get; set; }
        
        private Vector3 TargetPositionWorld { get; set; }
        private Vector3 TargetScaleLocal { get; set; }
        private Quaternion TargetRotationWorld { get; set; }
        */

        private Transform TargetTransform { get; set; }
        private Transform StartTransform { get; set; }

        private AnimationCurve ActiveAnimCurve { get; set; }

        private float PassedTime { get; set; }
        private float Duration { get; set; }

        private bool ReadyToHit { get; set; }

        private List<Action> NextSequenceAction { get; set; } = new List<Action>(); 

        private bool MoveThroughPhysics { get; set; }

        private void OnValidate()
        {
            if (_loadTime < 0)
                _loadTime = 0;

            if (_hitTime < 0)
                _hitTime = 0;
        }

        private void OnEnable()
        {
            _rigidbodyParent = _rigidbodyToTransform.transform.parent;
            StartTransform = _currentPosition; 

            Subcribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subcribe()
        {
            switch (_inputType)
            {
                case Input.MouseLeftBtn:
                    IInputEventsCallbackIHandler.OnMouseLeftBtnDown += MouseLeftBtnDown;
                    IInputEventsCallbackIHandler.OnMouseLeftBtnUp += MouseLeftBtnUp;

                    break;

                case Input.MouseRightBtn:
                    IInputEventsCallbackIHandler.OnMouseRightBtnDown += MouseRightBtnDown;
                    IInputEventsCallbackIHandler.OnMouseRightBtnUp += MouseRightBtnUp;

                    break;
            }

            SubscribedInputEvents = true;
        }

        private void Unsubscribe()
        {
            switch (_inputType)
            {
                case Input.MouseLeftBtn:
                    IInputEventsCallbackIHandler.OnMouseLeftBtnDown -= MouseLeftBtnDown;
                    IInputEventsCallbackIHandler.OnMouseLeftBtnUp -= MouseLeftBtnUp;

                    break;

                case Input.MouseRightBtn:
                    IInputEventsCallbackIHandler.OnMouseRightBtnDown -= MouseRightBtnDown;
                    IInputEventsCallbackIHandler.OnMouseRightBtnUp -= MouseRightBtnUp;

                    break;
            }

            SubscribedInputEvents = false;

            if (SubscribedUpdateEvents)
            {
                ITimeUtilitiesEventsCallbackHandler.OnUnityFixedUpdate -= UnityFixedUpdate;
                SubscribedUpdateEvents = false;
            }
        }

        private void MouseLeftBtnDown(Vector2 vector)
        {
            NextSequenceAction.Clear();
            StartLoading(); 
        }

        private void MouseLeftBtnUp(Vector2 vector)
        {
            NextSequenceAction.Clear(); 
            NextSequenceAction.Add(StartRestoreToInit); 
            StartHit();
        }

        private void MouseRightBtnDown(Vector2 vector)
        {
            NextSequenceAction.Clear();
            StartLoading();
        }

        private void MouseRightBtnUp(Vector2 vector)
        {
            NextSequenceAction.Clear();
            NextSequenceAction.Add(StartRestoreToInit);
            StartHit();
        }

        private void StartAnimationSequence(
            Transform targetTransform,
            AnimationCurve activeAnimationCurve,
            float duration
            )
        {
            Duration = duration;
            PassedTime = 0;

            StartTransform.position = _rigidbodyToTransform.position;
            StartTransform.localScale = _rigidbodyToTransform.transform.localScale;
            StartTransform.rotation = _rigidbodyToTransform.rotation;

            TargetTransform = targetTransform;

            ActiveAnimCurve = activeAnimationCurve;

            if (!SubscribedUpdateEvents)
            {
                ITimeUtilitiesEventsCallbackHandler.OnUnityFixedUpdate += UnityFixedUpdate;
                SubscribedUpdateEvents = true;
            }
        }

        private void StartLoading()
        {
            MoveThroughPhysics = false; 

            StartAnimationSequence(
                _loadPoint,
                _loadAnimCurve,
                _loadTime
                ); 
        }

        private void StartHit()
        {
            MoveThroughPhysics = true; 

            StartAnimationSequence(
                _hitPoint,
                _loadAnimCurve,
                _loadTime
                );
        }

        private void StartRestoreToInit()
        {
            MoveThroughPhysics = false; 

            StartAnimationSequence(
                _initPoint,
                _restoreToInitAnimCurve,
                _restoreToInitTime
                );
        }

        private void UnityFixedUpdate()
        {
            if(TargetTransform == null)
            {
                ITimeUtilitiesEventsCallbackHandler.OnUnityFixedUpdate -= UnityFixedUpdate;
                SubscribedUpdateEvents = false;
                return;
            }


            if (PassedTime >= Duration)
            {
                if(NextSequenceAction.Count>0)
                {
                    var nexAction = NextSequenceAction[0];
                    NextSequenceAction.RemoveAt(0);
                    nexAction();
                    return; 
                }

                ITimeUtilitiesEventsCallbackHandler.OnUnityFixedUpdate -= UnityFixedUpdate;
                SubscribedUpdateEvents = false;

                _rigidbodyToTransform.transform.localScale = TargetTransform.localScale;

                if (MoveThroughPhysics)
                {
                    _rigidbodyToTransform.MovePosition(TargetTransform.position);
                    _rigidbodyToTransform.MoveRotation(TargetTransform.rotation);

                    return; 
                }

                _rigidbodyToTransform.transform.position = TargetTransform.position;
                _rigidbodyToTransform.transform.rotation = TargetTransform.rotation;

                return;
            }

            PassedTime += Time.fixedDeltaTime;

            if (PassedTime <= 0) return;

            var t = PassedTime / Duration;

            if (t > 1)
                t = 1; 

            var lerpValue = ActiveAnimCurve.Evaluate(t);

            var newPosition = Vector3.Lerp( StartTransform.position, TargetTransform.position, lerpValue );
            var newRotation = Quaternion.Lerp(StartTransform.rotation, TargetTransform.rotation, lerpValue);

            _rigidbodyToTransform.transform.localScale = Vector3.Lerp(StartTransform.localScale, TargetTransform.localScale, lerpValue );

            if(MoveThroughPhysics)
            {
                _rigidbodyToTransform.MovePosition(newPosition);
                _rigidbodyToTransform.MoveRotation(newRotation);
                return; 
            }

            _rigidbodyToTransform.transform.position = newPosition;
            _rigidbodyToTransform.transform.rotation = newRotation;
        }

    }
}
