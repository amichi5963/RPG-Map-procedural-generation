using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public int damage = 20; // �G�̍U����

    private void OnTriggerEnter2D(Collider2D other)
    {
        // �G���v���C���[�ɓ��������ꍇ�̏���
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
