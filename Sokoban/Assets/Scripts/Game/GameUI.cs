using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{

    public Sprite soundButtonImage;
    public Sprite soundButtonImage2;
    public Button soundButton;
    public AudioSource music;

    public TextMeshProUGUI leveltext;
    public TextMeshProUGUI turnstext;

    private int turns;
    private int sound;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("sound"))
        {
            sound = PlayerPrefs.GetInt("sound");
        }
        else
        {
            sound = 1;
        }
        if (sound == 0)
        {
            soundButton.image.sprite = soundButtonImage2;
            music.volume = 0.0f;
        }
        else
        {
            soundButton.image.sprite = soundButtonImage;
            music.volume = 0.5f;
        }
        string level = PlayerPrefs.GetString("ChosenLevel");
        leveltext.text = level.Substring(0, 5) + " " + level.Substring(6);

        turns = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void RestartLevel()
    {
        SceneManager.LoadScene("Level");
    }


    public void ToggleSound()
    {
        if (sound == 1)
        {
            soundButton.image.sprite = soundButtonImage2;
            music.volume = 0.0f;
            sound = 0;
            PlayerPrefs.SetInt("sound", 0);
        }
        else if (sound == 0)
        {
            soundButton.image.sprite = soundButtonImage;
            music.volume = 0.5f;
            sound = 1;
            PlayerPrefs.SetInt("sound", 1);
        }
    }

    public void Home()
    {
        SceneManager.LoadScene("Main Menu");
    }


    public void TurnPass(Component sender, object _sent)
    {
        turns++;
        turnstext.text = "Moves: " + turns.ToString();
        
    }
}
