﻿using UnityEngine;
using System.Collections;

public class PlayerCollision : MonoBehaviour {

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        SendMessageUpwards("OnPlayerCollisionEnter", other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        SendMessageUpwards("OnPlayerCollisionExit", other);
    }
}
