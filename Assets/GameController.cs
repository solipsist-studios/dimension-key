using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using UnityEngine;

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

    // Unity accessible data
    public string debugQueryString;
    public List<AttachableObject> keyObjects = new List<AttachableObject>(KEY_COUNT);
    public MeshRenderer buttonMesh;
    public List<Material> keyMaterials = new List<Material>(KEY_COUNT);
    public Transform spawnPoint;

    // Other member data
    private List<int> availableKeys = new List<int>();

    private void Awake()
    {
        foreach (var keyObj in this.keyObjects)
        {
            keyObj.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
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
                    this.availableKeys.Add(dirIndex);
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
        foreach (var key in this.availableKeys)
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
