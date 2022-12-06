using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StimuliController : MonoBehaviour
{
    Animator animator;
    [SerializeField] AnimationClip loomingMotion;
    [SerializeField] AnimationClip receedingMotion;

    MeshRenderer mesh;

    GameObject target1;
    GameObject target2;
    GameObject targetNull;

    bool isMoving = false;
    public bool IsMoving { get { return isMoving; } }

    void Start()
    {
        target1 = transform.Find("Target1").gameObject;
        target2 = transform.Find("Target2").gameObject;
        targetNull = transform.Find("TargetNull").gameObject;

        animator = GetComponent<Animator>();
        mesh = GetComponent<MeshRenderer>();

    }

    // Displays the specified target on the stimulus
    public void DisplayTarget(bool toggle, int target = 0)
    {
        if (target == 1) { target1.SetActive(toggle); }

        else if (target == 2) { target2.SetActive(toggle); }

        else { targetNull.SetActive(toggle); }
    }

    public void DisplayMesh(bool toggle)
    {
        mesh.enabled = toggle;
    }

    public void MoveTo(Vector3 position)
    {
        transform.position = position;
    }

    public IEnumerator MoveTo(Vector3 start, Vector3 end, float duration)
    {
        isMoving = true;

        float startTime = Time.time;
        float elapsedTime = 0f;
        float progress = 0f;

        while (progress < 1)
        {
            transform.position = Vector3.Lerp(start, end, progress);
            elapsedTime = (Time.time - startTime);
            progress = elapsedTime / duration;
            Debug.Log(startTime + ", " + Time.time + ", = " + elapsedTime);
            yield return null;
        }

        transform.position = end;

        isMoving = false;
    }
}
