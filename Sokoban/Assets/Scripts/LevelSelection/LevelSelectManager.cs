using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LevelSelectManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void LoadLevel()
    {
        PlayerPrefs.SetInt("Width", 8);
        PlayerPrefs.SetInt("Height", 8);
        SceneManager.LoadScene("Level");
    }
}
