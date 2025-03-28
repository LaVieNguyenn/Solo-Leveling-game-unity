using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;

public class SungJinWooPlayer : MonoBehaviour
{
    public static SungJinWooPlayer Instanse;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    [SerializeField] private float moveSpeed;
    private Vector3 playMoveDirection;
    // Update is called once per frame

    void Awake()
    {
        if (Instanse != null && Instanse != this)
        {
            Destroy(this);
        }
        else {
            Instanse = this;
        }
    }

    void Update()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        playMoveDirection = new Vector3 (inputX, inputY).normalized;

        animator.SetFloat("moveX", inputX);
        animator.SetFloat("moveY", inputY);

        if (playMoveDirection == Vector3.zero)
        {
            animator.SetBool("moving", false);
        }
        else animator.SetBool("moving", true);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(playMoveDirection.x * moveSpeed, playMoveDirection.y * moveSpeed);
    }


}
