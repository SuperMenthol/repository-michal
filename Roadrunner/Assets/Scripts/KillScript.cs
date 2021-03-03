using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillScript : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        GameObject aOb = animator.gameObject;
        Destroy(aOb);
    }
}
