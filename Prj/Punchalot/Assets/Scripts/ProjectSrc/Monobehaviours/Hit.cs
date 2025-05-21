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

        private Vector3 StartPointLocal { get; set; }
        private Vector3 StartScaleLocal { get; set; }
        private Quaternion StartRotationLocal { get; set; }

        private Vector3 TargetPositionLocal { get; set; }
        private Vector3 TargetScaleLocal { get; set; }
        private Quaternion TargetRotationLocal { get; set; }


        private Transform TargetTransform { get; set; }


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
            Vector3 targetPointLocal,
            Vector3 targetScaleLocal,
            Quaternion targetRotationLocal,
            AnimationCurve activeAnimationCurve,
            float duration
            )
        {
            Duration = duration;
            PassedTime = 0;

            StartPointLocal = _rigidbodyToTransform.transform.localPosition;
            StartScaleLocal = _rigidbodyToTransform.transform.localScale;
            StartRotationLocal = _rigidbodyToTransform.transform.localRotation;

            TargetPositionLocal = targetPointLocal;
            TargetScaleLocal = targetScaleLocal;
            TargetRotationLocal = targetRotationLocal;

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
            Debug.Log("\t " + gameObject.name + "\t Loading");

            TargetTransform = _loadPoint;

            StartAnimationSequence(
                _loadPoint.localPosition,
                _loadPoint.localScale,
                _loadPoint.localRotation,
                _loadAnimCurve,
                _loadTime
                ); 
        }

        private void StartHit()
        {
            MoveThroughPhysics = true; 
            Debug.Log("\t " + gameObject.name + "\t Hit ");

            TargetTransform = _hitPoint;

            StartAnimationSequence(
                _hitPoint.localPosition,
                _hitPoint.localScale,
                _hitPoint.localRotation,
                _loadAnimCurve,
                _loadTime
                );
        }

        private void StartRestoreToInit()
        {
            MoveThroughPhysics = false; 
            Debug.Log("\t "+gameObject.name+"\t To init ");

            TargetTransform = _initPoint;

            StartAnimationSequence(
                _initPoint.localPosition,
                _initPoint.localScale,
                _initPoint.localRotation,
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

            TargetPositionLocal = TargetTransform.localPosition;
            TargetRotationLocal = TargetTransform.localRotation;

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

                _rigidbodyToTransform.transform.localScale = TargetScaleLocal;

                if (MoveThroughPhysics)
                {
                    _rigidbodyToTransform.MovePosition(_rigidbodyParent.TransformPoint(TargetPositionLocal));
                    _rigidbodyToTransform.MoveRotation(_rigidbodyParent.rotation * TargetRotationLocal);
                    return; 
                }

                _rigidbodyToTransform.transform.localPosition = TargetPositionLocal;
                _rigidbodyToTransform.transform.localRotation = TargetRotationLocal;

                return;
            }

            PassedTime += Time.fixedDeltaTime;

            if (PassedTime <= 0) return;

            var t = PassedTime / Duration;

            if (t > 1)
                t = 1; 

            var lerpValue = ActiveAnimCurve.Evaluate(t);

            var newLocalPosition = Vector3.Lerp( StartPointLocal, TargetPositionLocal, lerpValue );
            var newLocalRotation = Quaternion.Lerp(StartRotationLocal, TargetRotationLocal, lerpValue);
            _rigidbodyToTransform.transform.localScale = Vector3.Lerp( StartScaleLocal, TargetScaleLocal, lerpValue );

            if(MoveThroughPhysics)
            {
                _rigidbodyToTransform.MovePosition(_rigidbodyParent.TransformPoint(newLocalPosition));
                _rigidbodyToTransform.MoveRotation(_rigidbodyParent.rotation * newLocalRotation);
                return; 
            }

            _rigidbodyToTransform.transform.localPosition = newLocalPosition;
            _rigidbodyToTransform.transform.localRotation = newLocalRotation;
        }

    }
}
