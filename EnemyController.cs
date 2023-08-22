using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int maxHealth = 20; // 敵の最大体力
    public float moveSpeed = 2f; // 敵の移動速度

    private int currentHealth; // 現在の体力
    private Transform player; // プレイヤーの位置

    private void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        // プレイヤーに向かって移動する
        Vector3 direction = player.position - transform.position;
        direction.Normalize();
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    // ダメージを受ける処理
    public void TakeDamage(int damageAmount)
    {
        Debug.Log("のこりHP:"+currentHealth);
        currentHealth -= damageAmount;

        // 体力が0以下になったら敵を破壊する
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 敵が倒された時の処理
    private void Die()
    {
        // ここで敵の破壊、アニメーション再生、得点加算などを行う
        Destroy(gameObject);
    }
}
