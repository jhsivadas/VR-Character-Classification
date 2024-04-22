using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.Networking;

public class API_Call : MonoBehaviour
{
    public class Fact
    {
        public string fact { get; set; }
        public int length { get; set; }
    }

    public TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetRequest("https://catfact.ninja/fact"));
    }

    public void onRefresh()
    {
        Start();
    }

    IEnumerator GetRequest(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(string.Format("Something wrong: {0}", webRequest.error));
                    break;
                case UnityWebRequest.Result.Success:
                    Fact fact = JsonConvert.DeserializeObject<Fact>(webRequest.downloadHandler.text);
                    text.text = fact.fact;
                    break;
            }
        }
    }
}
