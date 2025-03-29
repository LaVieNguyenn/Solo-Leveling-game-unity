using System.Collections;
using UnityEngine;

public class DashCombo : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashDistance = 5f;         // Khoảng cách dịch chuyển
    public float dashDuration = 0.2f;         // Thời gian dash (di chuyển)
    public float dashCooldown = 3f;           // Cooldown cho dash (3 giây mỗi lần)
    private float lastDashTime = -999f;
    private bool isDashing = false;

    [Header("Dash Effects")]
    public GameObject dashSmokePrefab;          // Hiệu ứng khói dưới chân (hiệu ứng 1)
    public GameObject dashLightningTravelPrefab;  // Hiệu ứng sấm sét trong quá trình dash (hiệu ứng 2)
    public GameObject dashLightningFinalPrefab;   // Hiệu ứng sấm sét xung quanh sau dash (hiệu ứng 3)

    // Thời gian hiển thị cho từng hiệu ứng (điều chỉnh theo animation của bạn)
    public float smokeEffectDuration = 0.5f;     // Thời gian để hiệu ứng khói tự hủy
    public float lightningTravelDuration = 0.2f; // Thời gian hiệu ứng sấm di chuyển
    public float lightningFinalDuration = 0.5f;  // Thời gian hiệu ứng sấm cuối

    // ---------------------
    // Phần xử lý quay mặt theo hướng phím
    public bool _isFacingRight = true;
    public bool IsFacingRight
    {
        get { return _isFacingRight; }
        private set
        {
            if (_isFacingRight != value)
            {
                // Đảo ngược scale để flip hướng
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
            _isFacingRight = value;
        }
    }

    private void SetFacingDirection(Vector3 moveDirection)
    {
        if (moveDirection.x > 0 && !IsFacingRight)
        {
            IsFacingRight = true;
        }
        else if (moveDirection.x < 0 && IsFacingRight)
        {
            IsFacingRight = false;
        }
    }
    // ---------------------

    void Update()
    {
        // Kích hoạt dash khi nhấn Space trong khi giữ Shift và có hướng input (nếu không có input, dùng hướng di chuyển cuối cùng)
        if (Input.GetKeyDown(KeyCode.Space) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            && Time.time - lastDashTime >= dashCooldown && !isDashing)
        {
            Vector3 dashDirection = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0f).normalized;
            if (dashDirection == Vector3.zero)
            {
                dashDirection = PlayerController.Instance.lastMoveDirection;
            }
            // Cập nhật hướng mặt của player dựa theo input
            //SetFacingDirection(dashDirection);
            StartCoroutine(DoDash(dashDirection));
        }
    }

    IEnumerator DoDash(Vector3 dashDirection)
    {
        isDashing = true;
        lastDashTime = Time.time;

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + dashDirection * dashDistance;

        // Tính góc quay dựa trên hướng dash
        float angle = Mathf.Atan2(dashDirection.y, dashDirection.x) * Mathf.Rad2Deg;
        if (Mathf.Abs(dashDirection.y) < 0.1f)
        {
            angle = 0f;
        }
        Quaternion effectRotation = Quaternion.Euler(0, 0, angle);

        // --- Hiệu ứng 1: Biến mất & khói ---
        if (dashSmokePrefab != null)
        {
            // Instantiate hiệu ứng khói tại vị trí hiện tại
            GameObject smoke = Instantiate(dashSmokePrefab, transform.position, Quaternion.identity);
            // Hủy hiệu ứng khói sau thời gian quy định
            Destroy(smoke, smokeEffectDuration);
        }
        // Ẩn SpriteRenderer của player để tạo hiệu ứng biến mất
        GetComponent<SpriteRenderer>().enabled = false;

        // --- Hiệu ứng 2: Sấm sét di chuyển ---
        GameObject lightningTravel = null;
        if (dashLightningTravelPrefab != null)
        {
            lightningTravel = Instantiate(dashLightningTravelPrefab, transform.position, effectRotation);
            Destroy(lightningTravel, lightningTravelDuration);
        }

        // Di chuyển player từ startPos đến targetPos trong khoảng thời gian dashDuration
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / dashDuration);
            yield return null;
        }
        transform.position = targetPos;

        // --- Hiệu ứng 3: Sau khi dash ---
        // Hiện lại player
        GetComponent<SpriteRenderer>().enabled = true;
        if (dashLightningFinalPrefab != null)
        {
            GameObject lightningFinal = Instantiate(dashLightningFinalPrefab, transform.position, Quaternion.identity);
            Destroy(lightningFinal, lightningFinalDuration);
        }

        isDashing = false;
    }
}
