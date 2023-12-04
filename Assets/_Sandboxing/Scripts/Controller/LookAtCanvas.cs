using UnityEngine;

public class LookAtCanvas : MonoBehaviour
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
