using Scripts.BaseSystems;
using Scripts.BaseSystems.Core;
using Scripts.InputSystem;
using UnityEngine;

namespace Scripts.ProjectSrc
{
    public class InGameSceneLoaderByKey : MonoBehaviour
    {
        [SerializeField]
        private GameObject _inGameSceneToLoad;

        [Space(15)]
        [SerializeField]
        [FilterByType(typeof(IInGameSceneEvents))]
        private Object _inGameSceneEventsObj;

        [SerializeField]
        [FilterByType(typeof(IInputEventsCallbackIHandler))]
        private Object _inputEventsCallbackIHandlerObj;

        private IInGameSceneEvents _iInGameSceneEvents;
        private IInGameSceneEvents IInGameSceneEvents
        {
            get
            {
                if (_iInGameSceneEvents == null)
                    _iInGameSceneEvents = _inGameSceneEventsObj.GetComponent<IInGameSceneEvents>();

                return _iInGameSceneEvents;
            }
        }

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

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            IInputEventsCallbackIHandler.OnKeyboardCtrlUp += KeyboardCtrlUp;
        }

        private void Unsubscribe()
        {
            IInputEventsCallbackIHandler.OnKeyboardCtrlUp -= KeyboardCtrlUp;
        }

        private void KeyboardCtrlUp()
        {
            IInGameSceneEvents.LoadInGameScene(_inGameSceneToLoad); 
        }
    }
}
