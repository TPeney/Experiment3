using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StimuliController : MonoBehaviour
{
    MeshRenderer mesh;

    [SerializeField] string target1Text;
    [SerializeField] string target1NullText;
    [SerializeField] string target2Text;
    [SerializeField] string target2NullText;

    GameObject preTarget;

    GameObject target1;
    GameObject target1Null;

    GameObject target2;
    GameObject target2Null;

    bool isMoving = false;
    public bool IsMoving { get { return isMoving; } }

    void Start()
    {
        preTarget = transform.Find("PreTarget").gameObject;
        target1 = transform.Find("Target1").gameObject;
        target2 = transform.Find("Target2").gameObject;
        target1Null = transform.Find("Target1Null").gameObject;
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

    // Displays the specified target on the stimulus
    public void DisplayTarget(bool toggle, int targetType, bool isTarget = false)
    {

        if (targetType == 0) { preTarget.SetActive(toggle); }
        else if (targetType == 1)
        {
            if (isTarget) target1.SetActive(toggle);
            else target1Null.SetActive(toggle);
        }
        else if (targetType == 2)
        {
            if (isTarget) target2.SetActive(toggle);
            else target2Null.SetActive(toggle);
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

    public void MoveTo(Vector3 position)
    {
        transform.position = position;
    }

    public IEnumerator MoveTo(float displayTime, Vector3 start, Vector3 end, float duration)
    {
        isMoving = true;

        float startTime; ;
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

        isMoving = false;
    }
}
