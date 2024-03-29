using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public int damage = 20; // 敵の攻撃力

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 敵がプレイヤーに当たった場合の処理
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}
