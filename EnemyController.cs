using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int maxHealth = 20; // �G�̍ő�̗�
    public float moveSpeed = 2f; // �G�̈ړ����x

    private int currentHealth; // ���݂̗̑�
    private Transform player; // �v���C���[�̈ʒu

    private void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        // �v���C���[�Ɍ������Ĉړ�����
        Vector3 direction = player.position - transform.position;
        direction.Normalize();
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    // �_���[�W���󂯂鏈��
    public void TakeDamage(int damageAmount)
    {
        Debug.Log("�̂���HP:"+currentHealth);
        currentHealth -= damageAmount;

        // �̗͂�0�ȉ��ɂȂ�����G��j�󂷂�
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // �G���|���ꂽ���̏���
    private void Die()
    {
        // �����œG�̔j��A�A�j���[�V�����Đ��A���_���Z�Ȃǂ��s��
        Destroy(gameObject);
    }
}
