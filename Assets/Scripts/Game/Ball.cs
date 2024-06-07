using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    int points;
    bool hit;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(int points)
    {
        this.points = points;
        hit = false;
    }

    public int getPoints()
    {
        return points;
    }

    public bool isHit()
    {
        return hit;
    }

    public void BallHit()
    {
        hit = true;
        gameObject.GetComponent<Renderer>().enabled = false;
    }
}
