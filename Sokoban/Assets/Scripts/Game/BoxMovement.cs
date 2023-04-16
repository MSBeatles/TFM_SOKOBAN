using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxMovement : MonoBehaviour
{
    private Vector3 goalPos;
    private Vector3 currentPos;
    private bool moving;

    [Header ("GAME EVENTS")]
    public GameEvent onGoalReached;
    public GameEvent onGoalExited;


    // Start is called before the first frame update
    void Start()
    {
        goalPos = new Vector3(0.0f, 0.0f, 0.0f);
        currentPos = transform.position;
        moving = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (moving)
        {
            transform.position = Vector3.MoveTowards(transform.position, goalPos, 3.5f * Time.deltaTime);
            if (Mathf.Abs(goalPos.z - transform.position.z) < 0.0001f && Mathf.Abs(goalPos.x - transform.position.x) < 0.0001)
            {
                moving = false;
                currentPos = goalPos;
            }
        }

    }


    void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "Player")
        {
            moving = true;
            Vector3 direction = transform.position - coll.transform.position;
            if (direction.x > 0)
            {
                goalPos = currentPos + new Vector3(1.0f, 0.0f, 0.0f);
            }
            else if (direction.x < 0)
            {
                goalPos = currentPos + new Vector3(-1.0f, 0.0f, 0.0f);

            }
            else if (direction.z > 0)
            {
                goalPos = currentPos + new Vector3(0.0f, 0.0f, 1.0f);
            }
            else if (direction.z < 0)
            {
                goalPos = currentPos + new Vector3(0.0f, 0.0f, -1.0f);
            }
        }
        if (coll.tag == "Goal")
        {
            onGoalReached.Raise();
        }
    }

    
    void OnTriggerExit(Collider coll)
    {
        if (coll.tag == "Goal")
        {
            onGoalExited.Raise();
        }
    }
}
