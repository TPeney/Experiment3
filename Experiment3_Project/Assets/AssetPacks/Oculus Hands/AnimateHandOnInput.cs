using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimateHandOnInput : MonoBehaviour
{
    [SerializeField] InputActionProperty pinchAnimationAction;
    [SerializeField] InputActionProperty gripAnimationAction;

    [SerializeField] Animator handAnimator;

    void Update()
    {
        ReadControllerInput();
    }

    private void ReadControllerInput()
    {
        float gripValue = gripAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("Grip", gripValue);

        float triggerValue = pinchAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("Trigger", triggerValue);
    }
}
