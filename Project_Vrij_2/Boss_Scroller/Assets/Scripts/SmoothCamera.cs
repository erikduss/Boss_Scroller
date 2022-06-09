using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCamera : MonoBehaviour
{
    public float dampTime = 1.15f;
    private Vector3 velocity = Vector3.zero;
    public Transform target;

    public float maxXValue = 0;
    public float minXValue = -30f;

    private GameObject staticCameraTarget;

    private void Start()
    {
        staticCameraTarget = new GameObject();
        staticCameraTarget.name = "StaticCameraTarget";
        GameObject.Instantiate(staticCameraTarget);
    }

    // Update is called once per frame
    void Update()
    {
        if (target)
        {
            Vector3 point = Camera.main.WorldToViewportPoint(target.position);
            Vector3 delta = target.position - Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
            Vector3 destination = transform.position + delta;

            if (destination.y < 1) destination.y = 1;
            if (destination.x < minXValue) destination.x = minXValue;
            if (destination.x > maxXValue) destination.x = maxXValue;

            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
        }
    }

    public void SetStaticCamera(Vector3 targetPosition)
    {
        staticCameraTarget.transform.position = targetPosition;
        target = staticCameraTarget.transform;
    }
}