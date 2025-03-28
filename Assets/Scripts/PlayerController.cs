using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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

    public GameObject skillBeru; // Kéo prefab quái vật vào đây
    private GameObject monsterInstance;
    public AudioSource skillIAudio; // Kéo AudioSource


    public GameObject skillTusk; // Kéo prefab quái vật vào đây
 
    private float cooldownU = 5f;

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
    private bool canMove = true;


    public GameObject skillEffectPrefab; // Gán trong Inspector
    private GameObject skillEffectInstance;
    private GameObject skillIEffectInstance;

    //Biến cooldown cho Skill R
    [SerializeField] private float skillCooldown = 40f;
    private float lastSkillTime = -40f;

    //Biến sát thương cho Skill R
    [SerializeField] private float skillDamage = 100f;
    public List<int> playerLevels;


    public float skillIRadius = 3f; // Bán kính sát thương I
    public int skillIDamage = 50;   // Sát thương gây ra I


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
        if (!canMove) return;
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

        if (Input.GetKeyDown(KeyCode.I))
        {
            UseSkillI();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            UseSkillU();
        }
    }


    public void UseSkillI()
    {
        if (!canMove) return; // Nếu đang dùng kỹ năng khác thì không làm gì

        Debug.Log("🔥 Triệu hồi quái vật!");

        // Nhân vật đứng yên
        canMove = false;
        rb.linearVelocity = Vector2.zero; // Dừng chuyển động

        // Kiểm tra prefab đã gán chưa
        if (skillBeru == null)
        {
            Debug.LogError("⚠ MonsterPrefab chưa được gán trong Inspector!");
            return;
        }

        // Phát âm thanh khi kích hoạt kỹ năng
        if (skillIAudio != null)
        {
            Debug.Log("🔊 Đang phát âm thanh skill I!");
            skillIAudio.Play();
        }
        else
        {
            Debug.LogError("❌ Không tìm thấy AudioSource cho skill I!");
        }

        // Triệu hồi quái vật
        monsterInstance = Instantiate(skillBeru, transform.position + new Vector3(1, 0, 0), Quaternion.identity);

        // Gây sát thương xung quanh nhân vật
        DealDamageAround();


        // Bắt đầu coroutine chờ animation kết thúc
        StartCoroutine(DestroyMonsterAfterAnimation());
    }

    public void UseSkillU()
    {
        if (!canMove) return; // Nếu đang dùng kỹ năng khác thì không làm gì

        Debug.Log("🔥 Triệu hồi quái vật!");

        // Nhân vật đứng yên
        canMove = false;
        rb.linearVelocity = Vector2.zero; // Dừng chuyển động

        // Kiểm tra prefab đã gán chưa
        if (skillTusk == null)
        {
            Debug.LogError("⚠ MonsterPrefab chưa được gán trong Inspector!");
            return;
        }

        //kich hoat bat tu
        isImmune = true;

        // Triệu hồi quái vật
        monsterInstance = Instantiate(skillTusk, transform.position + new Vector3(1, 0, 0), Quaternion.identity);


        // Bắt đầu coroutine chờ animation kết thúc
        StartCoroutine(DestroyMonsterAfterAnimation());
    }


    private void DealDamageAround()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, skillIRadius);

        foreach (Collider2D enemy in enemies)
        {
            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.TakeDamage(skillIDamage);
                Debug.Log($" Gây {skillIDamage} sát thương lên {enemy.name}");
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, skillIRadius);
    }




    IEnumerator EndSkillIEffect()
    {
        yield return new WaitForSeconds(1.5f); // Đợi animation chạy xong

        skillIEffectInstance.SetActive(false); // Ẩn hiệu ứng
        canMove = true; // Cho phép di chuyển lại
    }






    public void UseSkill()
    {
        Debug.Log("Nhân vật sử dụng kỹ năng R!");

        // Dừng di chuyển khi sử dụng kỹ năng
        rb.linearVelocity = Vector2.zero;
        playerMoveDirection = Vector3.zero;

        // Vô hiệu hóa điều khiển di chuyển
        canMove = false;

        // Xoay hướng animation dựa vào hướng di chuyển cuối cùng
        if (lastMoveDirection.x < 0) // Nếu di chuyển trái
        {
            transform.localScale = new Vector3(-1, 1, 1); // Lật trái
        }
        else if (lastMoveDirection.x > 0) // Nếu di chuyển phải
        {
            transform.localScale = new Vector3(1, 1, 1); // Giữ hướng phải
        }

        // Kích hoạt animation sử dụng kỹ năng
        animator.SetTrigger("useSkill");

        // Triệu hồi hiệu ứng kỹ năng bên cạnh nhân vật
        if (skillEffectPrefab != null)
        {
            skillEffectInstance = Instantiate(skillEffectPrefab, transform.position + new Vector3(1, 0, 0), Quaternion.identity);
            StartCoroutine(HideSkillEffect(1.0f)); // Ẩn sau 1 giây
        }

        // Gọi coroutine để bật lại di chuyển sau khi animation kết thúc
        StartCoroutine(ResetSkillAnimation());
    }


    private IEnumerator DestroyMonsterAfterAnimation()
    {
        Animator monsterAnimator = monsterInstance.GetComponent<Animator>();

        // Chờ đến khi animation kết thúc
        yield return new WaitForSeconds(monsterAnimator.GetCurrentAnimatorStateInfo(0).length);

        Destroy(monsterInstance); // Xóa quái vật
        isImmune = false;
        canMove = true; // Cho phép nhân vật di chuyển lại
        
    }

    IEnumerator HideSkillEffect(float delay)
    {
        yield return new WaitForSeconds(delay);
        skillEffectInstance.SetActive(false);
    }


    IEnumerator ResetSkillAnimation()
    {
        yield return new WaitForSeconds(0.5f); // Thời gian dựa trên Animation
        canMove = true;
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
