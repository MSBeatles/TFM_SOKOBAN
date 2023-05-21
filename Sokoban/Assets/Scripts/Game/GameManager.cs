using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            StartCoroutine(VictoryScreen());
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

    private IEnumerator VictoryScreen()
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene("Victory");
    }
}
