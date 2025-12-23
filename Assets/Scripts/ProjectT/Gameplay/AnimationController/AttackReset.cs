using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 애니메이터 상태 진입 시 특정 트리거를 리셋하는 StateMachineBehaviour
/// * 애니메이터 내 여러 상태에서 동일한 트리거를 사용할 때 유용
/// </summary>
public class AttackReset : StateMachineBehaviour
{
    [SerializeField] private string HASH_TRIGGER;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger(HASH_TRIGGER);
        // 필요하면 animator.gameObject.GetComponent<Sword_Common>()로 플래그 호출 가능
    }
}
