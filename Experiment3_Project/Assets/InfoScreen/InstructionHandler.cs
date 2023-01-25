using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UXF;

/// <summary>
/// Handles the functionality for displaying instructions to the participant via the information screen.
/// </summary>
public class InstructionHandler : MonoBehaviour
{
    [Header("Text Objects")]
    [SerializeField] TextMeshPro vrTeleport;
    [SerializeField] TextMeshPro expInstructions;
    [SerializeField] TextMeshPro expStartText;
    [SerializeField] TextMeshPro pracFailInfo;
    [SerializeField] TextMeshPro breakText;
    [SerializeField] TextMeshPro pauseText;
    [SerializeField] TextMeshPro expEndText;

    [Header("Controller Hint Text")]
    [SerializeField] TextMeshPro VRHleftControllerText;
    [SerializeField] TextMeshPro VRHrightControllerText;
    [SerializeField] TextMeshPro VRGleftControllerText;
    [SerializeField] TextMeshPro VRGrightControllerText;

    Animator screenAnimation;
    ExperimentHandler expHandler;

    public bool IsShowingInstruction { get; private set; } = false;
    bool responseReceived = false;

    private void Awake()
    {
        screenAnimation = GetComponent<Animator>();
        expHandler = FindObjectOfType<ExperimentHandler>();
    }

    /// <summary>
    /// Show text via in-game screen.
    /// </summary>
    /// <param name="instrText">The text to be shown.</param>
    /// <returns></returns>
    IEnumerator ShowInstructions(TextMeshPro instrText, bool resetCamera = false)
    {
        IsShowingInstruction = true;

        instrText.gameObject.SetActive(true);
        screenAnimation.SetTrigger("screenDown");
        float animationLength = screenAnimation.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSecondsRealtime(animationLength);

        while (!responseReceived) { yield return null; } // Set to true in ContinuePressed()

        if (resetCamera) { expHandler.ActiveCamera.GetComponent<PositionReset>().ResetView(); }

        screenAnimation.SetTrigger("screenUp");
        animationLength = screenAnimation.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSecondsRealtime(animationLength);
        instrText.gameObject.SetActive(false);

        responseReceived = false;
        IsShowingInstruction = false;
    }

    // Called from an event via the PlayerInput Component 
    public void ContinuePressed(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        responseReceived = true;
    }

    // Helper methods to start the ShowInstructions coroutine with various params
    public void ShowCameraResetInstructions() { StartCoroutine(ShowInstructions(vrTeleport, resetCamera: true)); }
    public void ShowExpInstructions() {
        int controlScheme = Session.instance.settings.GetInt("controlScheme");

        if (controlScheme == 2) // Correct text for control scheme 1 set by default in inspector
        {
            // TODO - If time, find a nicer way of doing this
            expInstructions.text = 
            "Your task is simple. You must determine which target letter is present:\n\n" +
            "S or H\n\n" +
            "If there is a 'S' - press right button.\n" +
            "If there is a 'H' - press left button.\n\n" +
            "Ignore all other letters.\n\n" +
            "Press trigger to begin a practice block.\n";

            VRHleftControllerText.text = "<- H";
            VRGleftControllerText.text = "<- H";

            VRHrightControllerText.text = "S ->";
            VRGrightControllerText.text = "S ->";
        }
        StartCoroutine(ShowInstructions(expInstructions)); }
    public void ShowExpStart() { StartCoroutine(ShowInstructions(expStartText)); }
    public void ShowPracFailWarning() { StartCoroutine(ShowInstructions(pracFailInfo)); }
    public void ShowBreak() { StartCoroutine(ShowInstructions(breakText)); }
    public void ShowExpEnd() { StartCoroutine(ShowInstructions(expEndText)); }
    public void ShowPause() {
        int trialsRemaining = Session.instance.CurrentBlock.trials.Count - Session.instance.CurrentTrial.number;
        pauseText.text = $"Experiment has been paused.\nThere are {trialsRemaining} trials remaining.\nPress trigger to resume.";
        StartCoroutine(ShowInstructions(pauseText)); 
    }
}