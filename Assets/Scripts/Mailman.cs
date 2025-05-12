using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class UnityWebRequestExtensions
{
    public static Task<UnityWebRequest> SendWebRequestAsync(this UnityWebRequest request)
    {
        var tcs = new TaskCompletionSource<UnityWebRequest>();
        var operation = request.SendWebRequest();

        operation.completed += _ => {
            tcs.SetResult(request);
        };

        return tcs.Task;
    }
}

// [System.Serializable]
// public class MessageData
// {
//     public string content;

//     public MessageData(string content)
//     {
//         this.content = content;
//     }
// }

[System.Serializable]
public class Message
{
    public string type; // "human", "ai", or "function"
    public string content;
    public Dictionary<string, object> additional_kwargs = new Dictionary<string, object>();

    public Message(string type, string content)
    {
        this.type = type;
        this.content = content;
    }
}


public class Payload
{
    public string input; // Matches the "input" field in the API
    public List<Message> chat_history;

    public Payload(string input = null, List<Message> chat_history = null)
    {
        this.input = input ?? string.Empty;
        this.chat_history = chat_history ?? new List<Message>();
    }
}

public class Mailman
{
    string url = "http://127.0.0.1:8000/invoke";

    public async Task<string> Request(Payload payload) {
        var request = new UnityWebRequest(url, "POST");
        string jsonBody = JsonUtility.ToJson(payload);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        await request.SendWebRequestAsync();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
            Debug.LogError(request.error);
            return null;
        } else {
            return request.downloadHandler.text;
        }
    }
}
