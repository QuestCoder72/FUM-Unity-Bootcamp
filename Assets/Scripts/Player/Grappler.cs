using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappler : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private DistanceJoint2D distanceJoint;

    private float timer;
    private float currentAngle = 0;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float angle = 20f;

    // Start is called before the first frame update
    void Start()
    {
        distanceJoint.enabled = false;    
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            Vector2 mousePos = (Vector2)mainCamera.ScreenToWorldPoint(Input.mousePosition);
            lineRenderer.SetPosition(0, mousePos);
            lineRenderer.SetPosition(1, transform.position);
            distanceJoint.connectedAnchor = mousePos;
            distanceJoint.enabled = true;
            lineRenderer.enabled = true;

            //swing code
            timer += speed * Time.deltaTime;
            float angle = Mathf.Sin(timer) * this.angle;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + currentAngle));
        }
        else if(Input.GetKeyUp(KeyCode.Mouse0))
        {
            distanceJoint.enabled = false;
            lineRenderer.enabled = false;
        }
        if(distanceJoint.enabled)
        {
            lineRenderer.SetPosition(1, transform.position);
        }
    }
}
