using UnityEngine;

public class SwordController : MonoBehaviour
{
    public int damage = 10; // ���̍U����
    public float rotationSpeed = 200f; // ���̉�]���x�i�x/�b�j
    public float distanceFromPlayer = 1f; // �v���C���[����̋���

    private Transform player; // �v���C���[�̈ʒu

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        // �v���C���[�̎������]����
        RotateAroundPlayer();
    }

    private void RotateAroundPlayer()
    {
        // �v���C���[�̎������]����
        Vector3 playerPosition = player.position;
        float angle = Time.time * rotationSpeed; // ��]�p�x�����Ԃɉ����čX�V

        // ���W�̌v�Z
        float x = playerPosition.x + distanceFromPlayer * Mathf.Cos(angle * Mathf.Deg2Rad);
        float y = playerPosition.y + distanceFromPlayer * Mathf.Sin(angle * Mathf.Deg2Rad);
        Vector3 newPosition = new Vector3(x, y, playerPosition.z);

        // ���̈ʒu���X�V
        transform.position = newPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("�ɂ�");
        // �����G�ɓ��������ꍇ�̏���
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
