using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public List<GameObject> keyObjects = new List<GameObject>();
    public List<int> availableKeys = new List<int>();

    private readonly Dictionary<string, string> keyParams = new Dictionary<string, string>()
    {
        { "w", "%E1%92%A5%E1%90%A6%E1%91%B3%E1%90%A7%E1%90%A4" },
        { "s", "%E1%90%85%E1%93%B4%E1%90%8A%E1%90%A7%E1%90%A4" },
        { "e", "%E1%90%8B%E1%90%A7%E1%90%B1%E1%90%A2%E1%91%B3%E1%90%A4" },
        { "n", "%E1%90%8A%E1%90%A2%E1%91%AD%E1%90%A6%E1%91%95%E1%91%B3%E1%90%A7%E1%90%A4" }
    };

    // Awake is called before start
    private void Awake()
    {
        foreach (var key in this.availableKeys)
        {
            this.keyObjects[key].SetActive(false);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
#if UNITY_EDITOR
        foreach (var key in this.availableKeys)
        {
            this.keyObjects[key].SetActive(true);
        }
#endif

        string queryString = Application.absoluteURL;
        
        if (string.IsNullOrEmpty(queryString) || !queryString.Contains("?"))
        {
            Debug.Log("No query string provided, or no query parameters.");
            return;
        }
        Debug.Log(string.Format("Application URL: {0}", queryString));

        var queryKeyValueParams = HttpUtility.ParseQueryString(queryString.Substring(queryString.IndexOf('?')));
        int keyIndex = 0;
        foreach (var keyVal in this.keyParams)
        {
            if (!string.IsNullOrEmpty(queryKeyValueParams.Get(keyVal.Key)))
            {
                string param = queryKeyValueParams[keyVal.Key];
                string encodedVal = HttpUtility.UrlEncode(param, Encoding.UTF8);
                if (encodedVal != null && encodedVal.ToUpper() == keyVal.Value)
                {
                    this.keyObjects[keyIndex].SetActive(true);
                }
            }

            keyIndex++;
        }
    }
}
