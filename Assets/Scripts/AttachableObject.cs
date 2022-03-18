using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttachableObjectState
{
    Unattached,
    Attached
}

public class AttachableObject : MonoBehaviour
{
    // Other data members
    private AttachmentPoint m_targetPoint;
    private AttachmentPoint m_sourcePoint;

    // Unity accessible data
    public bool showCollision = false;
    public AttachableObjectState state = AttachableObjectState.Unattached;
    public List<AttachmentPoint> attachmentPoints = new List<AttachmentPoint>();

    // ⚡Events⚡
    public event EventHandler<AttachmentPoint> OnAttached;
    public event EventHandler<AttachmentPoint> OnDetached;

    public void Attach()
    {
        // Need something to attach to
        if (!IsValidAttachment())
        {
            return;
        }
        
        // Disable rigidbody movement
        // TODO: instead, add a Fixed Joint
        Rigidbody rigidbody = this.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.velocity = Vector3.zero;
        }

        // Set rotation to point up, and snap to 90 degree increments
        Quaternion curRot = this.transform.rotation;
        float yRot = SnapToRightAngle(curRot.eulerAngles.y);
        curRot.eulerAngles = new Vector3(0, yRot, 0);
        this.transform.rotation = curRot;

        // Recalculate position delta after rotation
        Vector3 transformToTarget = m_targetPoint.transform.position - m_sourcePoint.transform.position;
        this.transform.position += transformToTarget;

        this.m_sourcePoint.Attach(this.m_targetPoint);
        this.m_targetPoint.Attach(this.m_sourcePoint);
        this.state = AttachableObjectState.Attached;

        if (this.OnAttached != null)
        {
            this.OnAttached.Invoke(this, m_sourcePoint);
        }    
    }

    public void Detach()
    {
        this.state = AttachableObjectState.Unattached;

        // Enable rigidbody movement
        Rigidbody rigidbody = this.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
        }

        if (m_sourcePoint != null)
        {
            m_sourcePoint.Detach();
        }
        if (m_targetPoint != null)
        {
            m_targetPoint.Detach();
        }

        if (this.OnDetached != null)
        {
            this.OnDetached.Invoke(this, m_sourcePoint);
        }

        m_targetPoint = null;
    }

    private bool IsValidAttachment()
    {
        if (m_targetPoint == null || m_sourcePoint == null || m_targetPoint.isAttached)
        {
            return false;
        }

        return m_targetPoint.attachmentType != m_sourcePoint.attachmentType;
    }

    private float SnapToRightAngle(float angle)
    {
        const float two_pi = 360.0f;
        const float quad_1 = 45.0f;
        const float quad_2 = 135.0f;
        const float quad_3 = 225.0f;
        const float quad_4 = 315.0f;
        const float angle_1 = 0.0f;
        const float angle_2 = 90.0f;
        const float angle_3 = 180.0f;
        const float angle_4 = 270.0f;

        //const float pi_over_4 = 45.0f;
        //const float pi_over_2 = 90.0f;

        if (Mathf.Abs(angle) >= two_pi)
        {
            angle %= two_pi;
        }

        if (angle < 0)
        {
            angle += two_pi;
        }

        if (angle >= quad_4 || angle < quad_1)
        {
            angle = angle_1;
        }
        else if (angle >= quad_1 && angle < quad_2)
        {
            angle = angle_2;
        }
        else if (angle >= quad_2 && angle < quad_3)
        {
            angle = angle_3;
        }
        else if (angle >= quad_3 && angle < quad_4)
        {
            angle = angle_4;
        }

        //return Mathf.Floor(angle + pi_over_4) / pi_over_2;

        return angle;
    }

    private void Awake()
    {
        foreach (AttachmentPoint attachmentPoint in attachmentPoints)
        {
            attachmentPoint.parentObject = this;
        }
    }

    private void Start()
    {
        Interactable interactable = this.GetComponent<Interactable>();
        if (interactable != null)
        {
            interactable.Held += OnInteractableHeld;
            interactable.Released += OnInteractableReleased;
        }
    }

    private void OnInteractableHeld(object sender, System.EventArgs e)
    {
        if (this.state == AttachableObjectState.Attached)
        {
            Detach();
        }
    }

    private void OnInteractableReleased(object sender, System.EventArgs e)
    {
        if (this.state == AttachableObjectState.Unattached)
        {
            Attach();
        }
    }

    private void FixedUpdate()
    {
        if (this.state != AttachableObjectState.Attached)
        {
            // Determine closest attachment point to attachment transform
            float minDistToAttachPoint = float.MaxValue;
            m_targetPoint = null;
            foreach (AttachmentPoint attachmentPoint in attachmentPoints)
            {
                attachmentPoint.GetComponent<MeshRenderer>().enabled = false;

                foreach (AttachmentPoint collidingPoint in attachmentPoint.collidingAttachmentPoints)
                {
                    bool isValidAttachment = attachmentPoint.attachmentType != collidingPoint.attachmentType;
                    if (!isValidAttachment)
                    {
                        continue;
                    }

                    Vector3 transformToTarget = collidingPoint.transform.position - attachmentPoint.transform.position;
                    float distToTargetPoint = transformToTarget.magnitude;
                    
                    if (m_targetPoint == null || distToTargetPoint < minDistToAttachPoint)
                    {
                        m_targetPoint = collidingPoint; 
                        m_sourcePoint = attachmentPoint;
                        minDistToAttachPoint = distToTargetPoint;
                    }
                }
            }

            if (m_targetPoint != null && showCollision)
            {
                // Set the visibility of the highlight
                m_targetPoint.GetComponent<MeshRenderer>().enabled = true;
            }
        }

        // if attached, show the transform preview
        //

    }
}
