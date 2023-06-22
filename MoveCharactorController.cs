using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCharactorController : MonoBehaviour
{
    [SerializeField]private float speed = 0.05f;

    void Start()
    {
    }

    void Update()
    {
        Vector2 position = transform.position;
        float moves = speed * Time.deltaTime;
        if (Input.GetKey("left"))
        {
            position.x -= moves;
        }
        else if (Input.GetKey("right"))
        {
            position.x += moves;
        }
        else if (Input.GetKey("up"))
        {
            position.y += moves;
        }
        else if (Input.GetKey("down"))
        {
            position.y -= moves;
        }

        transform.position = position;
    }
}