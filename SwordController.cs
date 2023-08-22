using UnityEngine;

public class SwordController : MonoBehaviour
{
    public int damage = 10; // 剣の攻撃力
    public float rotationSpeed = 200f; // 剣の回転速度（度/秒）
    public float distanceFromPlayer = 1f; // プレイヤーからの距離

    private Transform player; // プレイヤーの位置

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        // プレイヤーの周りを回転する
        RotateAroundPlayer();
    }

    private void RotateAroundPlayer()
    {
        // プレイヤーの周りを回転する
        Vector3 playerPosition = player.position;
        float angle = Time.time * rotationSpeed; // 回転角度を時間に応じて更新

        // 座標の計算
        float x = playerPosition.x + distanceFromPlayer * Mathf.Cos(angle * Mathf.Deg2Rad);
        float y = playerPosition.y + distanceFromPlayer * Mathf.Sin(angle * Mathf.Deg2Rad);
        Vector3 newPosition = new Vector3(x, y, playerPosition.z);

        // 剣の位置を更新
        transform.position = newPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("痛み");
        // 剣が敵に当たった場合の処理
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }
}
