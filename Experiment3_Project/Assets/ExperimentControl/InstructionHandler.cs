using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class InstructionHandler : MonoBehaviour
{
    [Header("Text Objects")]
    [SerializeField] TextMeshPro expStartText;
    [SerializeField] TextMeshPro breakText;
    [SerializeField] TextMeshPro expEndText;

    Animator screenAnimation;

    private void Awake()
    {
        screenAnimation = GetComponent<Animator>();
    }

    public bool IsShowingInstruction { get { return isShowingInstruction; } }
    bool isShowingInstruction = false;
    bool responseReceived = false;

    IEnumerator ShowInstructions(TextMeshPro instrText)
    {
        isShowingInstruction = true;

        instrText.gameObject.SetActive(true);
        screenAnimation.SetTrigger("screenDown");
        float animationLength = screenAnimation.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSecondsRealtime(animationLength);

        while (!responseReceived) { yield return null; }

        screenAnimation.SetTrigger("screenUp");
        animationLength = screenAnimation.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSecondsRealtime(animationLength);
        instrText.gameObject.SetActive(false);

        responseReceived = false;
        isShowingInstruction = false;
    }

    public void ContinuePressed(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }

        responseReceived = true;
    }

    public void ShowExpStart() { StartCoroutine(ShowInstructions(expStartText)); }
    public void ShowBreak() { StartCoroutine(ShowInstructions(breakText)); }
    public void ShowExpEnd() { StartCoroutine(ShowInstructions(expEndText)); }
}