using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Mailbox : MonoBehaviour
{
    public ScrollRect scrollRect;
    public TextMeshProUGUI historyDisplay;
    public TMP_InputField inputField;
    public Button sendButton;

    public List<Message> history;

    Mailman mailman;

    [SerializeField]
    TaskOrchestrator orchestrator;

    // Start is called before the first frame update
    void Start() {
        mailman = new Mailman();
        history = new List<Message>();
        
        historyDisplay.text = "system>> Hi! :)) \nsystem>> Welcome to the world! \nsystem>> helooooooo \n";
        sendButton.onClick.AddListener(SendMessage);
        inputField.onEndEdit.AddListener(delegate {
            if (Input.GetKeyDown(KeyCode.Return)) {
                SendMessage();
            }
        });
        inputField.onValueChanged.AddListener(delegate {
            if (inputField.text.Length > 0) {
                sendButton.interactable = true;
            } else {
                sendButton.interactable = false;
            }
        });
        sendButton.interactable = false;
    }

    void SendMessage() {
        string userInput = inputField.text;
        if (string.IsNullOrEmpty(userInput)) return;

        history.Add(new Message("human", userInput));
        historyDisplay.text += "user>> " + userInput + "\n";
        ScrollToBottom();
        inputField.text = "";

        Payload payload = new Payload(userInput, history);
        mailman.Request(payload).ContinueWith(task => {
            if (task.Result != null) {
                // Debug.Log("Response: " + task.Result);

                var dict = MiniJSON.Json.Deserialize(task.Result) as Dictionary<string, object>;

                // Debug.Log("Response dict: " + dict.ToString());
                // Debug.Log("Response dict[output]: " + dict["output"].ToString());
                if (dict == null || !dict.ContainsKey("output")) {
                    Debug.LogError("Invalid response format");
                    return;
                }

                List<Dictionary<string, object>> toolCalls = ExtractToolCalls(dict);

                foreach (var toolCall in toolCalls)
                {
                    var call = toolCall["call"] as Dictionary<string, object>;
                    var message = toolCall["message"] as string;

                    historyDisplay.text += "world>> " + call["tool"] + call["tool_input"] + "\n";
                    historyDisplay.text += message + "\n";

                    ScrollToBottom();
                }

                var output = dict["output"] as Dictionary<string, object>;
                var outputText = output["output"] as string;

                // strip outputtext of newlines:
                outputText = outputText.Replace("\n", " ");

                historyDisplay.text += "system>> " + outputText + "\n";
                ScrollToBottom();
                history.Add(new Message("ai", outputText));
            }
            inputField.ActivateInputField();
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    List<Dictionary<string, object>> ExtractToolCalls(Dictionary<string, object> dict) {
        var steps = (List<object>)((Dictionary<string, object>)dict["output"])["intermediate_steps"];
        var result = new List<Dictionary<string, object>>();

        foreach (var stepObj in steps)
        {
            var stepArray = (List<object>)stepObj;

            var agentAction = (Dictionary<string, object>)stepArray[0];
            var message = stepArray[1]; // this is the string response, e.g. "Moved to (2, 4)."

            var messageLog = (List<object>)agentAction["message_log"];
            var firstMessage = (Dictionary<string, object>)messageLog[0];

            var additionalKwargs = (Dictionary<string, object>)firstMessage["additional_kwargs"];
            var toolCalls = (List<object>)additionalKwargs["tool_calls"];
            var firstToolCall = (Dictionary<string, object>)toolCalls[0];

            var function = (Dictionary<string, object>)firstToolCall["function"];
            var toolName = (string)function["name"];
            var toolArgs = (string)function["arguments"];

            // Construct result entry
            var callDict = new Dictionary<string, object>
            {
                { "tool", toolName },
                { "tool_input", toolArgs }
            };

            result.Add(new Dictionary<string, object>
            {
                { "call", callDict },
                { "message", message }
            });

            // parse tool args into a dictionary
            Debug.Log("toolArgs: " + toolArgs);
            var argsDict = MiniJSON.Json.Deserialize(toolArgs) as Dictionary<string, object>;
            if (argsDict == null)
            {
                Debug.LogError("Failed to parse tool arguments.");
                continue;
            }
            orchestrator.PerformTask(toolName, argsDict);
        }

        return result;
    }

    void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();

        // Ensure scroll rect content fits properly
        RectTransform contentRect = scrollRect.content;
        RectTransform viewportRect = scrollRect.viewport;

        float contentHeight = contentRect.rect.height;
        float viewportHeight = viewportRect.rect.height;

        if (contentHeight > viewportHeight)
        {
            // Align content's bottom with viewport's bottom
            contentRect.anchoredPosition = new Vector2(
                contentRect.anchoredPosition.x,
                contentHeight - viewportHeight
            );
        }
        else
        {
            // No scroll needed
            contentRect.anchoredPosition = new Vector2(
                contentRect.anchoredPosition.x,
                0f
            );
        }
    }


    // Update is called once per frame
    void Update() {
        
    }
}
