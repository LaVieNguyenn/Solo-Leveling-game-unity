using UnityEngine;

public class AttackState : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {   
        AttackSungJinWoo attackComponent = animator.GetComponent<AttackSungJinWoo>();
        if (attackComponent != null)
        {
            attackComponent.OnAttackAnimationEnd();
        }
    }


}
