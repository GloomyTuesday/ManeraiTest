using Scripts.BaseSystems;
using Scripts.BaseSystems.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.ProjectSrc
{
    public class IndexMain : MonoBehaviour
    {

        [SerializeField]
        [Uneditable]
        private string _unitySceneToLoadName = "";

        [SerializeField]
        private UnityEngine.Object _unitySceneToLoad;

        [Space(15)]
        [SerializeField]
        [FilterByType(typeof(IUnitySceneEventsInvoker))]
        private UnityEngine.Object _unitySceneEventsInvokerObj;


        private void OnValidate()
        {
            if (_unitySceneToLoad != null)
                _unitySceneToLoadName = _unitySceneToLoad.name; 
        }

        private void OnEnable()
        {
            SceneManager.LoadScene(_unitySceneToLoadName);
        }
    }
}
