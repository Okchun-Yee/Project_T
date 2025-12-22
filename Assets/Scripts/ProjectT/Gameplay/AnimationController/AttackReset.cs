using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackReset : StateMachineBehaviour
{
    [SerializeField] private string HASH_TRIGGER;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger(HASH_TRIGGER);
        // 필요하면 animator.gameObject.GetComponent<Sword_Common>()로 플래그 호출 가능
    }
}
