using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using UnityEngine;
using UnityEngine.UI;
using WebXR;

using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    // Constants
    private const int KEY_COUNT = 4;
    private readonly Dictionary<string, string> keyParams = new Dictionary<string, string>(KEY_COUNT)
    {
        { "w", "%E1%92%A5%E1%90%A6%E1%91%B3%E1%90%A7%E1%90%A4" },
        { "s", "%E1%90%85%E1%93%B4%E1%90%8A%E1%90%A7%E1%90%A4" },
        { "e", "%E1%90%8B%E1%90%A7%E1%90%B1%E1%90%A2%E1%91%B3%E1%90%A4" },
        { "n", "%E1%90%8A%E1%90%A2%E1%91%AD%E1%90%A6%E1%91%95%E1%91%B3%E1%90%A7%E1%90%A4" }
    };
    private readonly string[] directions = new string[KEY_COUNT] { "w", "s", "e", "n" };

    // Other member data
    private List<int> m_availableKeys = new List<int>();
    private List<AttachableObject> m_attachedKeys = new List<AttachableObject>(KEY_COUNT);

    // Unity accessible data
    public string debugQueryString;
    public List<AttachableObject> keyObjects = new List<AttachableObject>(KEY_COUNT);
    public List<GameObject> lockObjects = new List<GameObject>(KEY_COUNT);
    public MeshRenderer buttonMesh;
    public List<Material> keyMaterials = new List<Material>(KEY_COUNT);
    public Material successMaterial;
    public Material neutralMaterial;
    public Transform spawnPoint;
    public Button enterARButton;
    public GameObject unsupportedText;

    public void OnEnterARClicked()
    {
        WebXRManager.Instance.ToggleAR();
        enterARButton.gameObject.SetActive(false);
    }

    private void Awake()
    {
        foreach (var keyObj in this.keyObjects)
        {
            keyObj.OnAttached += KeyObj_OnAttached;
            keyObj.OnDetached += KeyObj_OnDetached;
            keyObj.gameObject.SetActive(false);
        }
    }

    private void KeyObj_OnAttached(object sender, AttachmentPoint attachPoint)
    {
        // Update combination status
        this.m_attachedKeys.Add((AttachableObject)sender);

        CheckKeyConfiguration();
    }

    private void KeyObj_OnDetached(object sender, AttachmentPoint attachPoint)
    {
        // Update combination status
        this.m_attachedKeys.Remove(sender as AttachableObject);

        CheckKeyConfiguration();
    }

    private AttachmentPoint GetUnattachedPointOfType(AttachableObject attachedObj, AttachmentType type)
    {
        if (attachedObj == null)
        {
            Debug.Log("[GetAdjacentAttachmentPoint] Invalid object");
            return null;
        }

        foreach (AttachmentPoint point in attachedObj.attachmentPoints)
        {
            if (point.attachmentType == type && !point.isAttached)
            {
                return point;
            }
        }

        return null;
    }

    private void CheckKeyConfiguration()
    {
        const int layerMask = ~((1 << 1) | (1 << 2)); // !TransparentFX layer || IgnoreRaycast Layer
        const string strLockPrefix = "LockPart";
        const float keyEpsilon = 0.02069999f;

        // Clear the lock materials
        foreach (GameObject obj in this.lockObjects)
        {
            obj.GetComponentInChildren<MeshRenderer>().material = this.neutralMaterial;
        }

        // Offset > 3 will ensure KeyPart1 is the first one placed
        int offset = 4;

        // Check the attached keys
        for (int i = 1; i <= this.m_attachedKeys.Count; ++i)
        {
            string correctKeyName = "KeyPart" + i;
            AttachableObject attachedKey = this.m_attachedKeys[i - 1];
            if (attachedKey == null || attachedKey.name != correctKeyName)
            {
                continue;
            }

            GameObject initialLockObj = null;
            if (i == 1)
            {
                foreach (GameObject lockObj in this.lockObjects)
                {
                    AttachmentPoint keyAttachment = lockObj.GetComponentInChildren<AttachmentPoint>().attachedPoint;
                    if (keyAttachment != null && keyAttachment.parentObject == attachedKey)
                    {
                        // update offset
                        offset = int.Parse(lockObj.name.Substring(strLockPrefix.Length)) - 1;
                        initialLockObj = lockObj;

                        break;
                    }
                }
            }

            // Get the other attachment point
            AttachmentPoint adjPoint = GetUnattachedPointOfType(attachedKey, AttachmentType.Loop);
            if (adjPoint == null)
            {
                continue;
            }

            // Project down to see if we hit the correct Lock tile
            RaycastHit rayHit;
            if (!Physics.Raycast(adjPoint.transform.position + (Vector3.up * keyEpsilon), Vector3.down, out rayHit, 1.0f, layerMask) ||
                !rayHit.collider.name.StartsWith(strLockPrefix))
            {
                continue;
            }

            int lockIdx = int.Parse(rayHit.collider.name.Substring(strLockPrefix.Length));

            if (lockIdx == 0 && i == 1 && initialLockObj != null)
            {
                initialLockObj.GetComponentInChildren<MeshRenderer>().material = this.successMaterial;
            }
            else if (i + offset == lockIdx)
            {
                // This is the correct orientation, so update the tile
                rayHit.collider.GetComponentInChildren<MeshRenderer>().material = this.successMaterial;
            }

        }
    }

    private void Start()
    {
        try
        {
            enterARButton.gameObject.SetActive(WebXRManager.Instance.isSupportedAR);
            unsupportedText.SetActive(!WebXRManager.Instance.isSupportedAR);

            //enterARButton.onClick.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            enterARButton.gameObject.SetActive(false);
            unsupportedText.SetActive(true);
        }

#if UNITY_EDITOR
        string queryString = this.debugQueryString;
#else
        string queryString = Application.absoluteURL;
#endif
        
        if (string.IsNullOrEmpty(queryString) || !queryString.Contains("?"))
        {
            Debug.Log("No query string provided, or no query parameters.");
            return;
        }
        Debug.Log(string.Format("Application URL: {0}", queryString));

        var queryKeyValueParams = HttpUtility.ParseQueryString(queryString.Substring(queryString.IndexOf('?')));
        bool matAssigned = false;
        foreach (var keyVal in this.keyParams)
        {
            if (!string.IsNullOrEmpty(queryKeyValueParams.Get(keyVal.Key)))
            {
                string param = queryKeyValueParams[keyVal.Key];
                string encodedVal = HttpUtility.UrlEncode(param, Encoding.UTF8);
                int dirIndex = Array.IndexOf(this.directions, keyVal.Key);

                if (encodedVal != null && encodedVal.ToUpper() == keyVal.Value)
                {
                    this.m_availableKeys.Add(dirIndex);
                }

                // Set the button color to the 0th element in the query params
                if (!matAssigned)
                {
                    this.buttonMesh.material = this.keyMaterials[dirIndex];
                    matAssigned = true;
                }
            }
        }
    }

    private void Reset()
    {
        foreach (var key in this.m_availableKeys)
        {
            var keyObj = this.keyObjects[key];
            keyObj.Detach();
            keyObj.gameObject.SetActive(true);
            keyObj.transform.position = this.spawnPoint.position;
            keyObj.transform.rotation = Random.rotation;
            
            var physObj = keyObj.GetComponent<Rigidbody>();
            physObj.velocity = Vector3.zero;
        }
    }

    public void OnResetButtonPressed()
    {
        Reset();
    }
}
