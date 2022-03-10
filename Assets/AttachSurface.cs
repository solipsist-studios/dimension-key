using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttachSurfaceState
{
    Unattached,
    Attached
}

public class AttachSurface : MonoBehaviour
{
    private AttachmentPoint m_targetPoint;
    private Vector3 m_targetPositionDelta;

    public bool showCollision = false;
    public AttachSurfaceState state = AttachSurfaceState.Unattached;
    public List<AttachmentPoint> attachmentPoints = new List<AttachmentPoint>();

    public void Attach()
    {
        // Need something to attach to
        if (m_targetPoint == null)
        {
            return;
        }

        this.state = AttachSurfaceState.Attached;
        
        // Disable rigidbody movement
        Rigidbody rigidbody = this.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.velocity = Vector3.zero;
        }

        this.transform.position += m_targetPositionDelta + new Vector3(0, this.transform.localScale.y, 0);

        // Set rotation to point up, and snap to 90 degree increments
        Quaternion curRot = this.transform.rotation;
        float yRot = SnapToRightAngle(curRot.eulerAngles.y);
        curRot.eulerAngles = new Vector3(0, yRot, 0);
        this.transform.rotation = curRot;
    }

    public void Detach()
    {
        this.state = AttachSurfaceState.Unattached;

        // Enable rigidbody movement
        Rigidbody rigidbody = this.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
        }
    }

    private float SnapToRightAngle(float angle)
    {
        //const float two_pi = 360.0f;
        //const float quad_1 = 45.0f;
        //const float quad_2 = 135.0f;
        //const float quad_3 = 225.0f;
        //const float quad_4 = 315.0f;
        //const float angle_1 = 0.0f;
        //const float angle_2 = 90.0f;
        //const float angle_3 = 180.0f;
        //const float angle_4 = 270.0f;

        const float pi_over_4 = 45.0f;
        const float pi_over_2 = 90.0f;

        //if (Mathf.Abs(angle) >= two_pi)
        //{
        //    angle %= two_pi;
        //}

        //if (angle < 0)
        //{
        //    angle += two_pi;
        //}

        //if (angle >= quad_4 || angle < quad_1)
        //{
        //    angle = angle_1;
        //}
        //else if (angle >= quad_1 && angle < quad_2)
        //{
        //    angle = angle_2;
        //}
        //else if (angle >= quad_2 && angle < quad_3)
        //{
        //    angle = angle_3;
        //}
        //else if (angle >= quad_3 && angle < quad_4)
        //{
        //    angle = angle_4;
        //}

        return Mathf.Floor(angle + pi_over_4) / pi_over_2;
    }

    private void Start()
    {
        foreach (AttachmentPoint attachmentPoint in attachmentPoints)
        {
            attachmentPoint.surface = this;
        }

        Interactable interactable = this.GetComponent<Interactable>();
        if (interactable != null)
        {
            interactable.Held += OnInteractableHeld;
            interactable.Released += OnInteractableReleased;
        }
    }

    private void OnInteractableHeld(object sender, System.EventArgs e)
    {
        if (this.state == AttachSurfaceState.Attached)
        {
            Detach();
        }
    }

    private void OnInteractableReleased(object sender, System.EventArgs e)
    {
        if (this.state == AttachSurfaceState.Unattached)
        {
            Attach();
        }
    }

    private void FixedUpdate()
    {
        if (this.state != AttachSurfaceState.Attached)
        {
            // Determine closest attachment point to attachment transform
            float minDistToAttachPoint = float.MaxValue;
            m_targetPoint = null;
            foreach (AttachmentPoint attachmentPoint in attachmentPoints)
            {
                attachmentPoint.GetComponent<MeshRenderer>().enabled = false;

                foreach (AttachmentPoint collidingPoint in attachmentPoint.collidingAttachmentPoints)
                {
                    Vector3 transformToTarget = collidingPoint.transform.position - attachmentPoint.transform.position;
                    float distToTargetPoint = transformToTarget.magnitude;

                    if (m_targetPoint == null || distToTargetPoint < minDistToAttachPoint)
                    {
                        m_targetPoint = attachmentPoint;
                        m_targetPositionDelta = transformToTarget;
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
