using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskOrchestrator : MonoBehaviour
{
    public Lightbulb lightbulb;
    public Agent agent;


    public bool PerformTask(string taskName, Dictionary<string, object> parameters)
    {
        Debug.Log($"Performing task: {taskName} with parameters: {parameters}");

        switch (taskName) {
            case "turn_lights_on":
                lightbulb.On();
                break;
            case "turn_lights_off":
                lightbulb.Off();
                break;
            case "move_self":
                Debug.Log($"Moving self with parameters: {parameters}");
                foreach (var key in parameters.Keys) {
                    Debug.Log($"Key in parameter: '{key}'");
                }
                if (parameters.ContainsKey("dx") && parameters.ContainsKey("dy")) {
                    Debug.Log("Parameters for move_self are present.");
                    var dx = parameters["dx"];
                    var dy = parameters["dy"];

                    Debug.Log($"dx: {dx}, dy: {dy}");
                    Debug.Log($"dx type: {dx.GetType()}");
                    Debug.Log($"dy type: {dy.GetType()}");

                    float fdx = System.Convert.ToSingle(dx);
                    float fdy = System.Convert.ToSingle(dy);

                    Debug.Log($"Moving agent by dx: {fdx}, dy: {fdy}");
                    Vector2 newPosition = new Vector2(transform.position.x + fdx, transform.position.y + fdy);
                    agent.MoveTo(newPosition);
                } else {
                    Debug.LogWarning("Parameters for move_self are missing.");
                }
                break;
            default:
                Debug.LogWarning($"Unknown task: {taskName}");
                return false;
        }

        return true; 
    }
}
