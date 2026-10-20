using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class AIConnector : MonoBehaviour
{
    public ChatManager chat;
    public string url = "https://alchemyz-api-visn.onrender.com/chat";

    [System.Serializable] class Req { public string message; public string user_id; }
    [System.Serializable] class Res { public string reply; }

    public void Ask(string text)
    {
        StartCoroutine(Send(text));
    }

    IEnumerator Send(string text)
    {
        string json = JsonUtility.ToJson(new Req
        {
            message = text,
            user_id = SystemInfo.deviceUniqueIdentifier
        });

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.timeout = 90;

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Res res = JsonUtility.FromJson<Res>(www.downloadHandler.text);
                chat.AddMessage(chat.profPrefab, res.reply);
            }
            else
            {
                chat.AddMessage(chat.profPrefab, "Error: " + www.error);
            }
        }
    }
    void Start()
    {
        StartCoroutine(WakeUp());
    }

    IEnumerator WakeUp()
    {
        string baseUrl = url.Replace("/chat", "");
        using (UnityWebRequest www = UnityWebRequest.Get(baseUrl))
        {
            www.timeout = 90;
            yield return www.SendWebRequest();
        }
    }
}