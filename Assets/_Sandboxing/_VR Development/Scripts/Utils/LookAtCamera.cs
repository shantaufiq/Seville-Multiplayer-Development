using UnityEngine;

namespace Seville.Multiplayer.Launcer
{
    public class LookAtCamera : MonoBehaviour
    {
        [SerializeField] private GameObject mainCamera;

        public void SetCamera(GameObject cam)
        {
            mainCamera = cam;
        }

        private void Update()
        {
            if (mainCamera != null)
            {
                transform.LookAt(mainCamera.transform);
                transform.rotation *= Quaternion.LookRotation(Vector3.back);
            }
        }
    }
}