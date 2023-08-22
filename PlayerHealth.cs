using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100; // �v���C���[�̍ő�̗�

    private int currentHealth; // ���݂̗̑�

    private void Start()
    {
        currentHealth = maxHealth;
    }

    // �_���[�W���󂯂鏈��
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        // �v���C���[�̗̑͂�0�ȉ��ɂȂ�����Q�[���I�[�o�[
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // �v���C���[���|���ꂽ���̏����i�Q�[���I�[�o�[�����Ȃǁj
    private void Die()
    {
        // �Q�[���I�[�o�[�����������ɒǉ�����
        Debug.Log("Game Over!");
    }
}
