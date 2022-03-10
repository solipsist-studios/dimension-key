using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachmentPoint : MonoBehaviour
{
    public List<AttachmentPoint> collidingAttachmentPoints { get; private set; } = new List<AttachmentPoint>();

    public AttachSurface surface { get; set; }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (surface == null)
        {
            // Log initialization failure
            return;
        }

        if (surface.state == AttachSurfaceState.Attached)
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

        if (surface.state == AttachSurfaceState.Attached)
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
