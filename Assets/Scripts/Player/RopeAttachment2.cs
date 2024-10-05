using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeAttachment2 : MonoBehaviour
{
    [SerializeField] private GameObject hinge;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float swingSpeed = 50f;
    [SerializeField] private float maxSwingSpeed = 200f;
    [SerializeField] private float accelaration = 10f;
    [SerializeField] private float currentSwingSpeed;
    [SerializeField] private float boostForce = 10f;
    [SerializeField] private float attachRange = 10f;

    [SerializeField] PlayerMovement playerMovement;
   
    private HingeJoint2D joint;
    private Rigidbody2D playerRB;
    private bool isAttached;

    void Awake()
    {
        playerRB = GetComponent<Rigidbody2D>();
        currentSwingSpeed = swingSpeed;
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
        }
    }

    private void FixedUpdate()
    {
        if(isAttached)
        {
            //Gradually increase the swing speed
            currentSwingSpeed = Mathf.Min(currentSwingSpeed + accelaration * Time.fixedDeltaTime, maxSwingSpeed);

            float bufferZone = 9f;

            JointMotor2D motor = joint.motor;
            //motor.motorSpeed = (transform.position.x < hinge.transform.position.x)? currentSwingSpeed : -currentSwingSpeed;
            if(transform.position.x < hinge.transform.position.x - bufferZone)
            {
                motor.motorSpeed = currentSwingSpeed;
            }
            else if(transform.position.x > hinge.transform.position.x + bufferZone)
            {
                motor.motorSpeed = -currentSwingSpeed;
            }
            joint.motor = motor;

            //Check if the player y position is above hinge y position
            if(transform.position.y > hinge.transform.position.y)
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
            joint = hinge.AddComponent<HingeJoint2D>();
            joint.connectedBody = playerRB;

            //Determine the initial motorspeed based on the player position relative to the hinge
            float initialMotorSpeed = (transform.position.x < hinge.transform.position.x)? swingSpeed : -swingSpeed;

            //Set motor properties
            joint.useMotor = true;

            JointMotor2D motor = new JointMotor2D{motorSpeed = initialMotorSpeed, maxMotorTorque = 2000f};
            joint.motor = motor;

            joint.anchor = Vector2.zero;
            joint.connectedAnchor = hinge.transform.InverseTransformPoint(transform.position);

            //Set angle limits
            // joint.useLimits = true;
            // JointAngleLimits2D limits = new JointAngleLimits2D{min = minAngle, max = maxAngle};
            // joint.limits = limits;

            joint.enableCollision = false;

            //Uncheck freeze rotation constraints
            playerRB.constraints &= ~RigidbodyConstraints2D.FreezeRotation;

            lineRenderer.SetPosition(0, hinge.transform.position);
            lineRenderer.enabled = true;

            playerMovement.horizontal = 0;
            transform.parent = hinge.transform;
            isAttached = true;
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

            playerMovement.horizontal = Input.GetAxisRaw("Horizontal");
            //reset player rotation changed by swinging back to zero
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            hinge.transform.eulerAngles = new Vector3(0, 0, 0);
        // }
    }

    private void ApplyBoost()
    {
        playerRB.velocity = new Vector2(playerRB.velocity.x * 0.1f, playerRB.velocity.y * 0.1f);
        playerRB.angularVelocity = 0f;

        Vector2 boostDirection = (transform.position.x < hinge.transform.position.x)? new Vector2(1f, 0.5f) : new Vector2(-1f, 0.5f);
        float cappedBoost = Mathf.Clamp(boostForce, 0, 10);
        playerRB.AddForce(boostDirection.normalized * cappedBoost, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if(isAttached)
        {
            DetachFromHinge();
        }
    }
}
