using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskOrchestrator : MonoBehaviour
{
    public Lightbulb lightbulb;

    

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
            default:
                Debug.LogWarning($"Unknown task: {taskName}");
                return false;
        }

        return true; 
    }
}
