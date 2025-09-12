using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    Camera mainCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = mainCamera.transform.position + mainCamera.transform.forward * 1.5f;
        transform.rotation = mainCamera.transform.rotation;
    }
}
