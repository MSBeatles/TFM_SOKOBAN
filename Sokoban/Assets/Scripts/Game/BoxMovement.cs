using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxMovement : MonoBehaviour
{
    private Vector3 goalPos;
    private Vector3 currentPos;
    private bool moving;
    private bool portaling;
    private bool ice;
    private bool isNew;
    private bool stop;
    private bool canCrossPortal;
    private float incX;
    private float incZ;
    private Vector3 direction;
    [Header ("GAME EVENTS")]
    public GameEvent onGoalReached;
    public GameEvent onGoalExited;
    public GameEvent onPortalTouched;


    // Start is called before the first frame update
    void Start()
    {
        currentPos = transform.position;
        goalPos = currentPos;
        moving = false;
        portaling = false;
        isNew = true;
        direction = new Vector3(0.0f, 0.0f, 0.0f);
        ice = false;
        incX = 0.0f;
        incZ = 0.0f;
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
                if (portaling)
                {
                    Destroy(gameObject);
                }
                if (ice && !stop && !isNew)
                {
                    moving = true;
                    if (direction.x > 0)
                    {
                        incX = 1.0f;
                        incZ = 0.0f;
                    }
                    else if (direction.x < 0)
                    {
                        incX = -1.0f;
                        incZ = 0.0f;
                    }
                    else if (direction.z > 0)
                    {
                        incX = 0.0f;
                        incZ = 1.0f;
                    }
                    else if (direction.z < 0)
                    {
                        incX = 0.0f;
                        incZ = -1.0f;
                    }
                    ice = false;
                    goalPos = currentPos + new Vector3(incX, 0.0f, incZ);
                }
                if (isNew)
                {
                    isNew = false;
                }
            }
        }
    }


    void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "PlayerPush")
        {
            isNew = false;
            stop = false;
            canCrossPortal = true;
            moving = true;
            direction = transform.position - coll.transform.position;
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
        else if (coll.tag == "Goal")
        {
            onGoalReached.Raise(this, 0);
        }
        else if (coll.tag == "Portal")
        {
            onPortalTouched.Raise(this, true);
            foreach (Collider col in GetComponents<Collider>())
            {
                col.enabled = false;
            }
            portaling = true;
            moving = true;
            if (canCrossPortal == false)
            {
                stop = true;
                goalPos = currentPos;
                portaling = false;
                foreach (Collider col in GetComponents<Collider>())
                {
                    col.enabled = true;
                }
            }
        }
        else if (coll.tag == "Ice")
        {
            ice = true;
            moving = true;
            direction = coll.transform.position - transform.position;
            Debug.Log("Direction: " + direction);
        }
        else if (coll.tag == "Crate" || coll.tag == "Wall")
        {
            stop = true;
            goalPos = currentPos;
        }
    }

    
    void OnTriggerExit(Collider coll)
    {
        if (coll.tag == "Goal")
        {
            onGoalExited.Raise(this, 0);
        }
    }

    public void ReceivePortalMovement(Component sender, object _originalToSpawn)
    {
        int[] positions = (int[]) _originalToSpawn;
        if (!portaling && positions[0] == (int)currentPos.x && positions[1] == (int)currentPos.z)
        {
            moving = true;
            goalPos.x = positions[2];
            goalPos.z = positions[3];
        }
    }

    public void CanCrossPortal(Component sender, object _canCross)
    {
        if (sender is PlayerMovement)
        {
            canCrossPortal = false;
        }
    }

}
