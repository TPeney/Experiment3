using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimateHandOnInput : MonoBehaviour
{
    [SerializeField] InputActionProperty triggerTouchedAction;
    [SerializeField] InputActionProperty triggerValueAction;

    [SerializeField] InputActionProperty gripTouchedAction;

    [SerializeField] InputActionProperty buttonTouchedAction;
    [SerializeField] InputActionProperty buttonValueAction;

    [SerializeField] Animator handAnimator;

    void Update()
    {
        ReadControllerInput();
    }

    private void ReadControllerInput()
    {
        float gripValue = gripTouchedAction.action.ReadValue<float>(); // 0 or 1
        handAnimator.SetFloat("gripValue", gripValue);

        float triggerTouched = triggerTouchedAction.action.ReadValue<float>(); // 0 or 1
        float triggerValue = triggerValueAction.action.ReadValue<float>(); // 0 to 1

        float buttonTouched = buttonTouchedAction.action.ReadValue<float>(); // 0 or 1
        float buttonValue = buttonValueAction.action.ReadValue<float>(); // 0 or 1
    }
}
