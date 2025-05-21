using UnityEngine;

namespace Scripts.ProjectSrc
{
    public class CursorLocker : MonoBehaviour
    {
        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
