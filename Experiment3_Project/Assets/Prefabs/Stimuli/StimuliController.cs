using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Collection of public functions to manipulate a stimuli object.
/// </summary>
public class StimuliController : MonoBehaviour
{
    [Header("Target Text Values - Assign in Prefab")]
    [SerializeField] string target1Text;
    [SerializeField] string target1NullText;
    [SerializeField] string target2Text;
    [SerializeField] string target2NullText;

    // Target GameObjects
    GameObject preTarget;
    GameObject target1;
    GameObject target1Null;
    GameObject target2;
    GameObject target2Null;

    MeshRenderer mesh;

    public bool IsMoving { get; private set; } = false;

    void Start()
    {
        preTarget = transform.Find("PreTarget").gameObject;
        target1 = transform.Find("Target1").gameObject;
        target1Null = transform.Find("Target1Null").gameObject;
        target2 = transform.Find("Target2").gameObject;
        target2Null = transform.Find("Target2Null").gameObject;

        mesh = GetComponent<MeshRenderer>();

        AssignTargetText();
    }

    private void AssignTargetText()
    {
        target1.GetComponent<TextMeshPro>().text = target1Text;
        target1Null.GetComponent<TextMeshPro>().text = target1NullText;
        target2.GetComponent<TextMeshPro>().text = target2Text;
        target2Null.GetComponent<TextMeshPro>().text = target2NullText;
    }

    /// <summary>
    /// Toggles the visiblity of the specified target on the stimulus.
    /// </summary>
    /// <param name="toggle">true = show, false = hide.</param>
    /// <param name="targetType">Target Type by int: 0 is PreTarget.</param>
    /// <param name="isTarget">Whether the target to be shown is the actual target for the trial.</param>
    public void DisplayTarget(bool toggle, int targetType, bool isTarget = false)
    {
        if (targetType == 0) { preTarget.SetActive(toggle); }
        else if (targetType == 1)
        {
            if (isTarget) target1.SetActive(toggle);
            else target1Null.SetActive(toggle); // Null targets are distractors
        }
        else if (targetType == 2)
        {
            if (isTarget) target2.SetActive(toggle);
            else target2Null.SetActive(toggle); // Null targets are distractors
        }
        else
        {
            Debug.Log("Invalid target type given");
        }
    }

    public void DisplayMesh(bool toggle)
    {
        mesh.enabled = toggle;
    }

    /// <summary>
    /// Coroutine to move the stimuli between two points through Lerp.
    /// </summary>
    /// <param name="displayTime">Duration to be shown in starting position before movement occurs.</param>
    /// <param name="start">Starting postion.</param>
    /// <param name="end">Ending position.</param>
    /// <param name="duration">How long the movement should take.</param>
    /// <returns></returns>
    public IEnumerator MoveTo(float displayTime, Vector3 start, Vector3 end, float duration)
    {
        IsMoving = true;

        float startTime;
        float elapsedTime;
        float progress = 0f;

        transform.position = start;

        yield return new WaitForSecondsRealtime(displayTime);

        startTime = Time.time;

        while (progress < 1)
        {
            transform.position = Vector3.Lerp(start, end, progress);
            elapsedTime = (Time.time - startTime);
            progress = elapsedTime / duration;
            yield return null;
        }

        transform.position = end;
        IsMoving = false;
    }

    public void MoveTo(Vector3 position)
    {
        transform.position = position;
    }
}
