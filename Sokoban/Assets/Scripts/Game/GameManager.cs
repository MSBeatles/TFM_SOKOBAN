using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    private int goals;
    private int goals_reached;

    // Start is called before the first frame update
    void Start()
    {
        goals = 0;
        goals_reached = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (goals_reached > 0 && goals_reached == goals)
        {
            Debug.Log("YOU WIN");
        }
    }


    public void GoalSet()
    {
        goals++;
    }


    public void GoalReached()
    {
        goals_reached++;
    }


    public void GoalExited()
    {
        goals_reached--;
    }
}
