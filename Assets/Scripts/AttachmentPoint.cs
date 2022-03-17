using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttachmentType
{
    Hook,
    Loop
}

public class AttachmentPoint : MonoBehaviour
{
    // Unity accessible data
    public AttachmentType attachmentType;

    // Other data members
    public AttachableObject surface { get; set; }
    public List<AttachmentPoint> collidingAttachmentPoints { get; private set; } = new List<AttachmentPoint>();
    public AttachmentPoint attachedPoint { get; private set; }

    protected void Attach(AttachmentPoint other)
    {

    }

    protected void Detach()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (surface == null)
        {
            // Log initialization failure
            return;
        }

        if (surface.state == AttachableObjectState.Attached)
        {
            // Something is being attached TO this.
            return;
        }

        AttachmentPoint otherPoint = other.GetComponent<AttachmentPoint>();

        // Only attach to other attachment points
        if (otherPoint != null)
        {
            collidingAttachmentPoints.Add(otherPoint);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (surface == null)
        {
            // Log initialization failure
            return;
        }

        if (surface.state == AttachableObjectState.Attached)
        {
            // Something is being attached TO this.
            return;
        }

        AttachmentPoint otherPoint = other.GetComponent<AttachmentPoint>();

        // Only attach to other attachment points
        if (otherPoint != null)
        {
            // Only detach from other attachment points
            //surface.RemoveCollidingAttachmentPoint(this);
            collidingAttachmentPoints.Remove(otherPoint);
        }
    }
}
