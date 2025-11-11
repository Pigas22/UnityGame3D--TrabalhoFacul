using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Camera : UniversalAdditionalCameraData
{
    Vector3 position = new Vector3(0, 3, -5);
    Transform targetTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position = position;
        targetTransform = GameObject.FindWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 desiredPosition = targetTransform.position + position;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 7);
    }
}
