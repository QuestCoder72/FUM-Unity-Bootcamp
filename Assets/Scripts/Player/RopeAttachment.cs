using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeAttachment : MonoBehaviour
{
    [Header ("General Parameters")]
    [SerializeField] private GameObject hinge;
    [SerializeField] private LineRenderer lineRenderer;

    private HingeJoint2D joint;
    private Rigidbody2D playerRB;
    [SerializeField] private bool isAttached;

    [Header ("Swing Parameters")]
    
    [SerializeField] private float swingSpeed = 120f;
    [SerializeField] private float swingAngle = 90f;//The angle to swing before detaching
    [SerializeField] private float attachRange = 15f;
    [SerializeField] private float currentAngle = 0;
    [SerializeField] private float targetAngle;
    [SerializeField] private float boostForce = 10f;

    
    void Awake()
    {
        playerRB = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if(CanAttachToHinge())
                AttachToHinge();
            
        }
        else if(Input.GetMouseButtonUp(0))
        {
            DetachFromHinge();
        }

        if(isAttached)
        {
            lineRenderer.SetPosition(1, transform.position);

            //Rotate the hinge to create the swinging motion
            float rotationDirection = (targetAngle > hinge.transform.eulerAngles.z)? 1f : -1f;
            hinge.transform.Rotate(0, 0, rotationDirection * swingSpeed * Time.deltaTime);

            //Check if the player y position is above the hinge y position
            if(transform.position.y > hinge.transform.position.y)
            {
                DetachFromHinge();
                ApplyBoost();
            }

        }
    }

    private void FixedUpdate()
    {
        currentAngle = hinge.transform.eulerAngles.z;

        if(isAttached)
        {
            float deltaAngle = Mathf.DeltaAngle(currentAngle, targetAngle);
            
            if(Mathf.Abs(deltaAngle) < 1f)
            {
                DetachFromHinge();
                ApplyBoost();
            }
           
        }
    }

    private bool CanAttachToHinge()
    {
        //Check if player x and y are respectively bigger than hinge x and y
        return (Mathf.Abs(transform.position.x - hinge.transform.position.x) <= attachRange && transform.position.y < hinge.transform.position.y);
    }

    private void AttachToHinge()
    {
        if(!isAttached)
        {
            //Determine the target angle based on the player iniial position 
            if(transform.position.x < hinge.transform.position.x)
            {
                targetAngle = hinge.transform.eulerAngles.z + swingAngle; //Swing to the right
            }
            else
            {
                targetAngle = hinge.transform.eulerAngles.z - swingAngle; //Swing to the left
            }

            joint = gameObject.AddComponent<HingeJoint2D>();
            joint.connectedBody = hinge.GetComponent<Rigidbody2D>();
            //joint.enableCollision = false;

            //Uncheck freeze rotation constraints
            playerRB.constraints &= ~RigidbodyConstraints2D.FreezeRotation;

            lineRenderer.SetPosition(0, hinge.transform.position);
            lineRenderer.enabled = true;

            transform.parent = hinge.transform;
            isAttached = true;

            Debug.Log("Attached to Hinge");
        }
    }

    private void DetachFromHinge()
    {
        // if(isAttached)
        // {
            Destroy(joint);

            //Reapply freeze rotation constraints
            playerRB.constraints |= RigidbodyConstraints2D.FreezeRotation;

            lineRenderer.enabled = false;

            //Reset second position back to source
            lineRenderer.SetPosition(1, hinge.transform.position);

            transform.parent = null;
            isAttached = false;
            Debug.Log("Detached from Hinge");

            //reset player rotation changed by swinging back to zero
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            hinge.transform.eulerAngles = new Vector3(0, 0, 0);
        // }
    }

    private void ApplyBoost()
    {
        Vector2 boostDirection = (targetAngle > hinge.transform.eulerAngles.z)? new Vector2(1f, 1f) : new Vector2(-1f, 1f);
        playerRB.AddForce(boostDirection.normalized * boostForce, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if(isAttached)
        {
            DetachFromHinge();
        }
    }

    /// <summary>
    /// Sent each frame where a collider on another object is touching
    /// this object's collider (2D physics only).
    /// </summary>
    /// <param name="other">The Collision2D data associated with this collision.</param>
    private void OnCollisionStay2D(Collision2D other)
    {
        if(isAttached && other.gameObject == hinge)
        {
            DetachFromHinge();
        }
    }
}
