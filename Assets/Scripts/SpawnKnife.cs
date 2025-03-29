using UnityEngine;

public class SpawnKnife : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Transform[] weaponSpawnPoints;
    [SerializeField] private GameObject[] weaponProjectilePrefabs;

    [SerializeField] private float[] weaponDamages;
    [SerializeField] private float projectileSpeed = 10f;

    private float lastHorizontal = 1f;

}
