using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;

public class AttackSungJinWoo : MonoBehaviour
{

    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        ProcessAttackInput();
    }

    private void ProcessAttackInput()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            if (horizontalInput < 0)
            {
                spriteRenderer.flipX = true;
            }
            else if (horizontalInput > 0)
            {
                spriteRenderer.flipX = false;
            }
            animator.SetTrigger("attack");
        }
    }

    public void OnAttackAnimationEnd()
    {
        if (spriteRenderer.flipX == true)
        {
            spriteRenderer.flipX = false;
        }
    }
}