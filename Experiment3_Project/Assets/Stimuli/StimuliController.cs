using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StimuliController : MonoBehaviour
{
    Animator animator;

    GameObject target1;
    GameObject target2;
    GameObject targetNull;

    void Start()
    {
        target1 = transform.Find("Target1").gameObject;
        target2 = transform.Find("Target2").gameObject;
        targetNull = transform.Find("TargetNull").gameObject;

        animator = GetComponent<Animator>();

    }

    // Displays the specified target on the stimulus
    public void DisplayTarget(bool toggle, int target = 0)
    {
        if (target == 1) { target1.SetActive(toggle); }

        else if (target == 2) { target2.SetActive(toggle); }

        else { targetNull.SetActive(toggle); }
    }

    public void PlayLooming()
    {
        animator.SetTrigger("Loom");
    }

    public void PlayReceding()
    {
        animator.SetTrigger("Recede");
    }

}
