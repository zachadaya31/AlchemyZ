using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class AIConnector : MonoBehaviour
{
    public ChatManager chat;
    public string url = "http://192.168.1.43:5000/chat";

    [System.Serializable] class Req { public string message; }
    [System.Serializable] class Res { public string reply; }

    public void Ask(string text)
    {
        StartCoroutine(Send(text));
    }

    IEnumerator Send(string text)
    {
        string json = JsonUtility.ToJson(new Req { message = text });

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

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
}