using System.Collections;
using UnityEngine;

public class WeaponSummon : MonoBehaviour
{
    [Header("Summon Effect")]
    public GameObject summonKnifePrefab;   // Prefab hiệu ứng Summon (có Animator)
    public float summonDelay = 0.5f;         // Thời gian chờ cho animation Summon chạy tại vị trí

    [Header("Knife Projectile")]
    public GameObject knifePrefab;           // Prefab dao bay (có SpriteRenderer, Animator, Rigidbody2D,...)

    [Header("Knife Settings")]
    public float projectileSpeed = 10f;      // Tốc độ dao bay
    public int numberOfProjectiles = 8;      // Số dao phóng ra xung quanh
    public float spawnRadius = 1f;           // Bán kính vòng tròn spawn dao

    private bool isSummoning = false;

    void Update()
    {
        // Khi bấm phím K thì bắt đầu summon
        if (Input.GetKeyDown(KeyCode.K) && !isSummoning)
        {
            StartCoroutine(SummonKnives());
        }
    }

    IEnumerator SummonKnives()
    {
        isSummoning = true;

        // Lấy vị trí trung tâm (ví dụ: vị trí của Player)
        Vector3 centerPos = PlayerController.Instance.GetComponent<SpriteRenderer>().bounds.center;

        // Với mỗi dao, tính vị trí spawn theo vòng tròn dựa trên số lượng dao
        for (int i = 0; i < numberOfProjectiles; i++)
        {
            float angle = i * (360f / numberOfProjectiles);
            Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f) * spawnRadius;
            Vector3 spawnPos = centerPos + offset;

            // Với mỗi vị trí, khởi tạo hiệu ứng summon và sau đó spawn dao
            StartCoroutine(SummonOneKnife(spawnPos, offset.normalized));
        }

        // Đợi một khoảng thêm để đảm bảo các coroutine được gọi (tùy chỉnh nếu cần)
        yield return new WaitForSeconds(summonDelay + 0.1f);
        isSummoning = false;
    }

    IEnumerator SummonOneKnife(Vector3 spawnPos, Vector3 direction)
    {
        // Tính góc quay dựa trên direction (mặc định sprite dao hướng phải)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // 1) Instantiate hiệu ứng Summon tại spawnPos với rotation tương ứng
        GameObject summonEffect = Instantiate(summonKnifePrefab, spawnPos, rotation);

        // 2) Đợi thời gian summonDelay để hiệu ứng chạy xong
        yield return new WaitForSeconds(summonDelay);

        // 3) Instantiate dao bay tại cùng vị trí với rotation giống hệt để đồng bộ
        GameObject knife = Instantiate(knifePrefab, spawnPos, rotation);
        Rigidbody2D rb = knife.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * projectileSpeed;
        }

        // 4) Huỷ hiệu ứng Summon nếu không cần nữa
        Destroy(summonEffect);
    }
}
