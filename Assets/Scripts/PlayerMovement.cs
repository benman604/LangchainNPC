using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Mailman mailman;

    // Start is called before the first frame update
    void Start()
    {
        mailman = new Mailman();
        Payload payload = new Payload("Hello, how are you?");
        mailman.Request(payload).ContinueWith(task => {
            if (task.Result != null)
            {
                Debug.Log("Response: " + task.Result);
            }
        });    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
