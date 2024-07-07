using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedEventListener : MonoBehaviour
{
    // Reference to the PlayerDataManagement script
    public GroundCheck groundCheck;

    private void OnEnable()
    {
        // Subscribe to the event
        groundCheck.OnPlayerGroundedChanged += HandlePlayerGroundedChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe from the event
        groundCheck.OnPlayerGroundedChanged -= HandlePlayerGroundedChanged;
    }

    private void HandlePlayerGroundedChanged(bool isGrounded)
    {
        // Handle the event
        Debug.Log("Player grounded state changed: " + isGrounded);
    }
}
