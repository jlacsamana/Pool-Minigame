using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    Vector2 direction;
    float velocity;
    public static float deceleration = 1;

    // Start is called before the first frame update
    void Start()
    {
        direction = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        velocity -= deceleration * Time.fixedDeltaTime;
        if (velocity < 0)
        {
            velocity = 0;
        }

        gameObject.transform.Translate(velocity * Time.fixedDeltaTime * direction.x, 0, velocity * Time.fixedDeltaTime * direction.y);
    }

    public void Initialize()
    {
        direction = Vector2.zero;
        velocity = 0;

    }

    public void setBallMovement(Vector2 newDirection, float newVelocity)
    {
        direction = newDirection;
        velocity = newVelocity;
    }

    public float getVelocity()
    {
        return velocity;
    }

    public Vector2 getDirection()
    {
        return direction;
    }

}
