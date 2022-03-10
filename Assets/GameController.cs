using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public List<GameObject> keyObjects = new List<GameObject>();
    public List<int> availableKeys = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        string queryString = Application.absoluteURL;

#if UNITY_EDITOR

        queryString = "https://localhost/?s=%E1%90%85%E1%93%B4%E1%90%8A%E1%90%A7%E1%90%A4";

        foreach (var key in availableKeys)
        {
            keyObjects[key].SetActive(true);
        }
        
#elif PLATFORM_WEBGL && !UNITY_EDITOR
        
#endif

        Debug.Log(string.Format("Application URL: {0}", queryString));
        var queryKeyValueParams = HttpUtility.ParseQueryString(queryString.Substring(queryString.IndexOf('?')));

        string param = queryKeyValueParams.Get("s");
        string encoded = HttpUtility.UrlEncode(param, Encoding.UTF8);
        // URL encoded: ????? 
        if (encoded.ToUpper() == "%E1%90%85%E1%93%B4%E1%90%8A%E1%90%A7%E1%90%A4")
        {
            keyObjects[0].SetActive(true);
        }

        param = queryKeyValueParams.Get("w");
        encoded = HttpUtility.UrlEncode(param, Encoding.UTF8);
        // URL encoded: ?????
        if (encoded.ToUpper() == "%E1%92%A5%E1%90%A6%E1%91%B3%E1%90%A7%E1%90%A4")
        {
            keyObjects[1].SetActive(true);
        }

        param = queryKeyValueParams.Get("n");
        string encoded = HttpUtility.UrlEncode(param, Encoding.UTF8);
        // URL encoded: ????????
        if (encoded.ToUpper() == "%E1%90%8A%E1%90%A2%E1%91%AD%E1%90%A6%E1%91%95%E1%91%B3%E1%90%A7%E1%90%A4")
        {
            keyObjects[2].SetActive(true);
        }

        param = queryKeyValueParams.Get("e");
        string encoded = HttpUtility.UrlEncode(param, Encoding.UTF8);
        // URL encoded: ??????
        if (encoded.ToUpper() == "%E1%90%8B%E1%90%A7%E1%90%B1%E1%90%A2%E1%91%B3%E1%90%A4")
        {
            keyObjects[3].SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
