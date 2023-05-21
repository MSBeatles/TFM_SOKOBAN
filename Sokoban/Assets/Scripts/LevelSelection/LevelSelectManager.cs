using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;


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


    public void LoadLevel(Button btn)
    {
        PlayerPrefs.SetString("ChosenLevel", btn.GetComponentInChildren<TMP_Text>().text);
        SceneManager.LoadScene("Level");
    }
}
