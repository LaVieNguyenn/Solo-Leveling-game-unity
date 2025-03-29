using UnityEngine;

public class AttackSungJinWoo : MonoBehaviour
{

    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private float attackDamage = 3f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayers;

    private float originalAttackX;

    public bool _isFacingRight = true;

    public bool IsFacingRight
    {
        get { return _isFacingRight; }
        private set
        {
            if (_isFacingRight != value)
            {
                transform.localScale *= new Vector2(-1, 1);
            }

            _isFacingRight = value;
        }
    }

    private void SetFacingDirection(Vector3 playerMoveDirection)
    {
        if (playerMoveDirection.x > 0 && !IsFacingRight)
        {
            IsFacingRight = true;
        }
        else if (playerMoveDirection.x < 0 && IsFacingRight)
        {
            IsFacingRight = false;
        }
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
            animator.SetTrigger("attack");
            DealDamage();
        }
    }


    private void DealDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Enemy>()?.TakeDamage(attackDamage);
        }
        Debug.Log("Attack gây " + attackDamage + " sát th??ng lên enemy!");
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        DrawArc(attackPoint.position, attackRange, 270f, 460f, 4);
    }

    private void DrawArc(Vector3 center, float radius, float startAngle, float endAngle, int segments)
    {
        float angleStep = (endAngle - startAngle) / segments;
        float angle = startAngle;
        Vector3 previousPoint = center + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * radius, Mathf.Sin(angle * Mathf.Deg2Rad) * radius, 0);

        for (int i = 1; i <= segments; i++)
        {
            angle += angleStep;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * radius, Mathf.Sin(angle * Mathf.Deg2Rad) * radius, 0);
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }
    }
}