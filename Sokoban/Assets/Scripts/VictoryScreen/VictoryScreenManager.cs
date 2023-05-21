using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreenManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void NextLevel()
    {
        string currentLevel = PlayerPrefs.GetString("ChosenLevel");
        int number = int.Parse(currentLevel.Substring(6));
        number++;
        PlayerPrefs.SetString("ChosenLevel", "Level " + number.ToString());
        SceneManager.LoadScene("Level");
    }

    public void ChooseLevel()
    {
        SceneManager.LoadScene("Level Selection");
    }
}
