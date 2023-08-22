using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100; // プレイヤーの最大体力

    private int currentHealth; // 現在の体力

    private void Start()
    {
        currentHealth = maxHealth;
    }

    // ダメージを受ける処理
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        // プレイヤーの体力が0以下になったらゲームオーバー
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // プレイヤーが倒された時の処理（ゲームオーバー処理など）
    private void Die()
    {
        // ゲームオーバー処理をここに追加する
        Debug.Log("Game Over!");
    }
}
