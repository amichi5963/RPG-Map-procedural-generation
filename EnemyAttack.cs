using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public int damage = 20; // “G‚ÌUŒ‚—Í

    private void OnTriggerEnter2D(Collider2D other)
    {
        // “G‚ªƒvƒŒƒCƒ„[‚É“–‚½‚Á‚½ê‡‚Ìˆ—
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
