using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    [SerializeField] private float moveSpeed;
    public Vector3 playerMoveDirection;
    public Vector3 lastMoveDirection;
    public float playerMaxHealth;
    public float playerHealth;

    public int experience;
    public int currentLevel;
    public int maxLevel;

    [SerializeField] private List<Weapon> inactiveWeapons;
    public List<Weapon> activeWeapons;
    [SerializeField] private List<Weapon> upgradeableWeapons;
    public List<Weapon> maxLevelWeapons;

    private bool isImmune;
    [SerializeField] private float immunityDuration;
    [SerializeField] private float immunityTimer;
 
    private Animator skillAnimator;

 

    public GameObject skillEffectPrefab; // Gán trong Inspector
    private GameObject skillEffectInstance;

    //Biến cooldown cho Skill R
    [SerializeField] private float skillCooldown = 40f;
    private float lastSkillTime = -40f;

    //Biến sát thương cho Skill R
    [SerializeField] private float skillDamage = 100f;
    public List<int> playerLevels;
    
    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(this);
        } else {
            Instance = this;
        }
    }

    void Start(){
        lastMoveDirection = new Vector3(0, -1);
        for (int i = playerLevels.Count; i < maxLevel; i++){
            playerLevels.Add(Mathf.CeilToInt(playerLevels[playerLevels.Count - 1] * 1.1f + 15));
        }
        playerHealth = playerMaxHealth;
        UIController.Instance.UpdateHealthSlider();
        UIController.Instance.UpdateExperienceSlider();
        AddWeapon(Random.Range(0, inactiveWeapons.Count));

        if (skillEffectPrefab != null)
        {
            skillEffectInstance = Instantiate(skillEffectPrefab, transform.position, Quaternion.identity);
            skillEffectInstance.SetActive(false); // Ẩn đi ban đầu
        }
        else
        {
            Debug.LogError("skillEffectPrefab chưa được gán! Kéo Prefab vào Inspector.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        playerMoveDirection = new Vector3(inputX, inputY).normalized;

        if (playerMoveDirection == Vector3.zero){
            animator.SetBool("moving", false);
        } else if (Time.timeScale != 0) {
            animator.SetBool("moving", true);
            animator.SetFloat("moveX", inputX);
            animator.SetFloat("moveY", inputY);
            lastMoveDirection = playerMoveDirection;
        }

        if (immunityTimer > 0){
            immunityTimer -= Time.deltaTime;
        } else {
            isImmune = false;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (Time.time - lastSkillTime >= skillCooldown)
            {
                lastSkillTime = Time.time;
                SkillVideoController.Instance.PlaySkillVideo(); // Phát video trước khi thực hiện skill
            }
            else
            {
                Debug.Log("Skill R đang hồi chiêu! Thời gian còn lại: " + Mathf.Ceil(skillCooldown - (Time.time - lastSkillTime)) + " giây");
            }
        }
    }



    public void UseSkill()
    {
        Debug.Log("Nhân vật sử dụng kỹ năng R!");

        // Xoay hướng animation dựa vào hướng di chuyển cuối cùng
        if (lastMoveDirection.x < 0) // Nếu di chuyển trái
        {
            transform.localScale = new Vector3(-1, 1, 1); // Lật trái
        }
        else if (lastMoveDirection.x > 0) // Nếu di chuyển phải
        {
            transform.localScale = new Vector3(1, 1, 1); // Giữ hướng phải
        }

        animator.SetTrigger("useSkill");

        if (skillEffectInstance != null)
        {
            skillEffectInstance.SetActive(true);
            skillEffectInstance.transform.position = transform.position; // Đặt hiệu ứng ở vị trí nhân vật
            StartCoroutine(HideSkillEffect(0.5f)); // Ẩn sau 0.5s
        }

        StartCoroutine(ResetSkillAnimation()); // Reset Animation
        // Gây sát thương khi kích hoạt kỹ năng
        DealSkillDamage();
    }


    // Gây sát thương khi sử dụng kỹ năng
    private void DealSkillDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, 5f); // Bán kính 5 đơn vị

        foreach (Collider2D enemy in hitEnemies)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(skillDamage);
            }
        }

        Debug.Log("Skill R gây " + skillDamage + " sát thương lên kẻ địch xung quanh!");
    }




    IEnumerator HideSkillEffect(float delay)
    {
        yield return new WaitForSeconds(delay);
        skillEffectInstance.SetActive(false);
    }


    IEnumerator ResetSkillAnimation()
    {
        yield return new WaitForSeconds(0.5f); // Thời gian dựa trên Animation
        animator.ResetTrigger("useSkill");
    }






    void FixedUpdate(){
        rb.linearVelocity = new Vector3(playerMoveDirection.x * moveSpeed, playerMoveDirection.y * moveSpeed);
    }

    public void TakeDamage(float damage){
        if (!isImmune){
            isImmune = true;
            immunityTimer = immunityDuration;
            playerHealth -= damage;
            UIController.Instance.UpdateHealthSlider();
            if (playerHealth <= 0){
                gameObject.SetActive(false);
                GameManager.Instance.GameOver();
            }
        }
    }

    public void GetExperience(int experienceToGet){
        experience += experienceToGet;
        UIController.Instance.UpdateExperienceSlider();
        if (experience >= playerLevels[currentLevel - 1]){
            LevelUp();
        }
    }

    public void LevelUp(){
        experience -= playerLevels[currentLevel - 1];
        currentLevel++;
        UIController.Instance.UpdateExperienceSlider();
        //UIController.Instance.levelUpButtons[0].ActivateButton(activeWeapon);

        upgradeableWeapons.Clear();

        if (activeWeapons.Count > 0){
            upgradeableWeapons.AddRange(activeWeapons);
        }
        if (inactiveWeapons.Count > 0){
            upgradeableWeapons.AddRange(inactiveWeapons);
        }
        for (int i = 0; i < UIController.Instance.levelUpButtons.Length; i++){
            if (upgradeableWeapons.ElementAtOrDefault(i) != null){
                UIController.Instance.levelUpButtons[i].ActivateButton(upgradeableWeapons[i]);
                UIController.Instance.levelUpButtons[i].gameObject.SetActive(true);
            } else {
                UIController.Instance.levelUpButtons[i].gameObject.SetActive(false);
            }
        }

        UIController.Instance.LevelUpPanelOpen();
    }

    private void AddWeapon(int index){
        activeWeapons.Add(inactiveWeapons[index]);
        inactiveWeapons[index].gameObject.SetActive(true);
        inactiveWeapons.RemoveAt(index);
    }

    public void ActivateWeapon(Weapon weapon){
        weapon.gameObject.SetActive(true);
        activeWeapons.Add(weapon);
        inactiveWeapons.Remove(weapon);
    }

    public void IncreaseMaxHealth(int value){
        playerMaxHealth += value;
        playerHealth = playerMaxHealth;
        UIController.Instance.UpdateHealthSlider();

        UIController.Instance.LevelUpPanelClose();
        AudioController.Instance.PlaySound(AudioController.Instance.selectUpgrade);
    }

    public void IncreaseMovementSpeed(float multiplier){
        moveSpeed *= multiplier;

        UIController.Instance.LevelUpPanelClose();
        AudioController.Instance.PlaySound(AudioController.Instance.selectUpgrade);
    }
}
