using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 0.05f;
    private Animator animator;

    void Start()
    {
        animator=GetComponent<Animator>();
    }

    void Update()
    {
        Vector2 position = transform.position;
        float moves = speed * Time.deltaTime;
        animator.SetBool("isWalking", false);
        if (Input.GetKey("left"))
        {
            animator.SetBool("isWalking", true);
            position.x -= moves;
        }
        if (Input.GetKey("right"))
        {
            animator.SetBool("isWalking", true);
            position.x += moves;
        }
        if (Input.GetKey("up"))
        {
            animator.SetBool("isWalking", true);
            position.y += moves;
        }
        if (Input.GetKey("down"))
        {
            animator.SetBool("isWalking", true);
            position.y -= moves;
        }

        transform.position = position;
    }
}