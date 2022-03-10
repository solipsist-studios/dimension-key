using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Interactable : MonoBehaviour
{
    private Camera m_currentCamera;
    private Rigidbody m_rigidbody;
    private Vector3 m_screenPoint;
    private Vector3 m_offset;
    private Vector3 m_currentVelocity;
    private Vector3 m_previousPos;
    private bool m_isHeld = false;

    public event EventHandler Held;
    public event EventHandler Released;

    void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        m_currentCamera = FindCamera();
    }

    void OnMouseDown()
    {
        if (!m_isHeld)
        {
            m_screenPoint = m_currentCamera.WorldToScreenPoint(gameObject.transform.position);
            m_offset = gameObject.transform.position - m_currentCamera.ScreenToWorldPoint(GetMousePosWithScreenZ(m_screenPoint.z));

            m_isHeld = true;
            if (Held != null)
            {
                Held(this, new EventArgs());
            }
        }
    }

    void OnMouseUp()
    {
        m_rigidbody.velocity = m_currentVelocity;

        m_isHeld = false;

        if (Released != null)
        {
            Released(this, new EventArgs());
        }
    }

    void FixedUpdate()
    {
        if (m_currentCamera != null && m_isHeld)
        {
            Vector3 currentScreenPoint = GetMousePosWithScreenZ(m_screenPoint.z);
            m_rigidbody.velocity = Vector3.zero;
            m_rigidbody.MovePosition(m_currentCamera.ScreenToWorldPoint(currentScreenPoint) + m_offset);
            m_currentVelocity = (transform.position - m_previousPos) / Time.deltaTime;
            m_previousPos = transform.position;
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
