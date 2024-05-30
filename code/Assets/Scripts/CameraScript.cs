using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// script that will allow the camera to follow the player
public class CameraScript : MonoBehaviour
{
    public Transform playerTransform;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, transform.position.z);

    }
    
}
