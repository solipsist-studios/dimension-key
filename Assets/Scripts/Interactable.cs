using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Interactable : MonoBehaviour
{
    private Camera m_currentCamera;
    private Rigidbody m_rigidbody;
    private Vector3 m_screenPoint;
    private Vector3 m_currentVelocity;
    private Vector3 m_previousPos;
    private bool m_isHeld = false;

    // Unity accessible data
    public Vector3 offset;

    public event EventHandler Held;
    public event EventHandler Released;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        m_currentCamera = FindCamera();
        Debug.Log("[Interactable] Name=" + gameObject.name + " Camera=" + m_currentCamera.name);
    }

    private void OnMouseDown()
    {
        if (!m_isHeld)
        {
            m_screenPoint = m_currentCamera.WorldToScreenPoint(gameObject.transform.position);
            offset = gameObject.transform.position - m_currentCamera.ScreenToWorldPoint(GetMousePosWithScreenZ(m_screenPoint.z));

            m_isHeld = true;
            if (Held != null)
            {
                Held(this, new EventArgs());
            }
        }
    }

    private void OnMouseUp()
    {
        m_rigidbody.velocity = m_currentVelocity;

        m_isHeld = false;

        if (Released != null)
        {
            Released(this, new EventArgs());
        }
    }

    private void FixedUpdate()
    {
        if (m_currentCamera != null && m_isHeld)
        {
            const int transparentFXLayer = 1 << 1; // TransparentFX layer
            Vector3 curScreenPoint = GetMousePosWithScreenZ(m_screenPoint.z);
            Vector3 curWorldPoint = m_currentCamera.ScreenToWorldPoint(curScreenPoint);
            Vector3 cameraToScreenPointVec = curWorldPoint - m_currentCamera.transform.position;
            RaycastHit rayHit;

            // Clip to our ground plane
            if (Physics.Raycast(m_currentCamera.transform.position, cameraToScreenPointVec.normalized, out rayHit, cameraToScreenPointVec.magnitude, transparentFXLayer))
            {
                curWorldPoint = rayHit.point;
            }

            // Enable rigidbody movement
            m_rigidbody.velocity = Vector3.zero;
            m_rigidbody.MovePosition(curWorldPoint + offset);
            m_currentVelocity = (transform.position - m_previousPos) / Time.deltaTime;
            m_previousPos = transform.position;

            // Align rotation to camera
            transform.forward = m_currentCamera.transform.forward;
            transform.eulerAngles = new Vector3(0.0f, transform.eulerAngles.y, transform.eulerAngles.z);
        }
    }

    private Vector3 GetMousePosWithScreenZ(float screenZ)
    {
        return new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenZ);
    }

    private Camera FindCamera()
    {
        Camera[] cameras = FindObjectsOfType<Camera>();
        Camera result = null;
        int camerasSum = 0;
        foreach (var camera in cameras)
        {
            if (camera.enabled)
            {
                result = camera;
                camerasSum++;
            }
        }
        if (camerasSum > 1)
        {
            result = null;
        }
        return result;
    }
}
