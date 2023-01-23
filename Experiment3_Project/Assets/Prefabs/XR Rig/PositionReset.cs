using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

[ExecuteAlways] // Script to reset XRrigs back to their starting points
public class PositionReset : MonoBehaviour
{
    Transform resetTransform;
    GameObject player;
    Camera playerHead;

    private void Start()
    {
        resetTransform = transform;
        player = gameObject;
        playerHead = GetComponentInChildren<Camera>();
    }

    public void ResetViewPressed(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        ResetView();
    }

    public void ResetView()
    {
        if (!gameObject.activeInHierarchy) { return; }
        
        // Reset rotation
        var rotationAngleY = resetTransform.rotation.eulerAngles.y - playerHead.transform.rotation.eulerAngles.y;
        player.transform.Rotate(0, rotationAngleY, 0);

        // Reset position
        var distanceDiff = resetTransform.position -
            playerHead.transform.position;
        player.transform.position += distanceDiff;
    }
}
