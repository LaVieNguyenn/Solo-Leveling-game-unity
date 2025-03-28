using System.Collections;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    public GameObject skillPrefab; // Prefab chứa hiệu ứng Skill_R
    public Transform spawnPoint;   // Vị trí xuất hiện Skill_R (ví dụ: tay nhân vật)

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(SummonSkill());
        }
    }

    private IEnumerator SummonSkill()
    {
        if (skillPrefab == null)
        {
            Debug.LogError("❌ skillPrefab chưa được gán! Kiểm tra trong Inspector.");
            yield break;
        }

        // Tạo skill tại vị trí spawnPoint
        GameObject skill = Instantiate(skillPrefab, spawnPoint.position, Quaternion.identity);

        // Kiểm tra Animator trên skill
        Animator skillAnimator = skill.GetComponent<Animator>();
        if (skillAnimator == null)
        {
            Debug.LogError("❌ Skill Prefab không có Animator! Kiểm tra lại.");
            yield break;
        }

        skill.transform.SetParent(spawnPoint, worldPositionStays: false);

        Debug.Log("✔ Nhân vật sử dụng kỹ năng R!");
        skillAnimator.SetTrigger("TriggerSkillR");

        yield return new WaitForSeconds(0.1f); // Đợi 1 frame

        float animationTime = skillAnimator.GetCurrentAnimatorStateInfo(0).length;
        if (animationTime == 0)
        {
            Debug.LogError("❌ Không lấy được thời gian animation! Kiểm tra Animator.");
            animationTime = 1.0f;
        }

        yield return new WaitForSeconds(animationTime);

        Destroy(skill);
    }
}
