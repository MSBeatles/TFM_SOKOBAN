using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxMovement : MonoBehaviour
{
    private Vector3 goalPos;
    private Vector3 currentPos;
    private bool moving;
    private bool portaling;
    private bool isNew;
    [Header ("GAME EVENTS")]
    public GameEvent onGoalReached;
    public GameEvent onGoalExited;


    // Start is called before the first frame update
    void Start()
    {
        currentPos = transform.position;
        goalPos = currentPos;
        moving = false;
        portaling = false;
        isNew = true;
        StartCoroutine(NotNew());
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
                Debug.Log(portaling);
                if (portaling)
                {
                    Destroy(gameObject);
                }
            }
        }
    }


    void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "PlayerPush")
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
        else if (coll.tag == "Goal")
        {
            onGoalReached.Raise(this, 0);
        }
        else if (coll.tag == "Portal")
        {
            foreach (Collider col in GetComponents<Collider>())
            {
                col.enabled = false;
            }
            portaling = true;
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
        if (!portaling && isNew)
        {
            int[] pos = (int[]) _originalToSpawn;
            moving = true;
            Debug.Log(pos[0]);
            Debug.Log(pos[1]);
            goalPos.x = pos[0];
            goalPos.z = pos[1];
        }
    }

    private IEnumerator NotNew()
    {
        yield return new WaitForSeconds(0.1f);
        isNew = false;
    }
}
