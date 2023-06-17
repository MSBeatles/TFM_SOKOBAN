using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour
{
    private enum Tile { None, Floor, Box, Wall, Goal, Player, BoxGoal, LeftPortal, RightPortal, UpPortal, DownPortal, Fire };
    private Tile[,] tiles;
    private GameObject[,] myObjects;
    private int numPortals;
    private int[,] portals;
    private Vector3 playerPos;
    private int boxes;
    private Vector3 currentPos;
    private Vector3 goalPos;
    private int currentX;
    private int currentY;
    private int goodBoxes;
    private int matchPortalX;
    private int matchPortalY;
    public GameObject floor;
    public GameObject box;
    public GameObject wall;
    public GameObject goal;
    public GameObject player;
    public GameObject portal;
    public GameObject fire;
    public GameObject ice;
    public Transform camera;


    ////MEVES D'ARA
    [Header ("GAME EVENTS")]
    public GameEvent onGoalSet;
    public GameEvent onSetX;
    public GameEvent onSetZ;


    private Transform t_player;

    string path;

    private int maxX;
    private int maxY;
    private int direction;

    // Start is called before the first frame update
    void Start()
    {
        //Aquí carreguem el nivell
        //string path = Application.persistentDataPath + "\\" + PlayerPrefs.GetString("ChosenLevel");
        //string path = "Assets\\LevelDesigns\\" + PlayerPrefs.GetString("ChosenLevel") + ".txt";
        path = Application.dataPath + "/StreamingAssets/LevelDesigns/" + PlayerPrefs.GetString("ChosenLevel") + ".txt";
        //Debug.Log(path);
        StreamReader reader = new StreamReader(path, true);


        //Primer de tot agafem maxX i maxY
        string line;
        line = reader.ReadLine();
        int.TryParse(line, out maxX);
        PlayerPrefs.SetFloat("Width", maxX);
        onSetX.Raise(this, maxX);
        line = reader.ReadLine();
        int.TryParse(line, out maxY);
        PlayerPrefs.SetFloat("Height", maxY);
        onSetZ.Raise(this, maxY);


        tiles = new Tile[maxX, maxY];
        myObjects = new GameObject[maxX, maxY];

        //matchPortalX = 0;
        //matchPortalY = 0;


        for (int j = 0; j < maxY; j++)
        {
            int i = 0;
            line = "";
            line = reader.ReadLine();
            //Debug.Log(line);
            for (int k = 0; k < line.Length; k++)
            {
                if (line[k] == 'W')
                {
                    //myObjects[i, j] = Instantiate(wall, new Vector3(i, 1.0f, j), Quaternion.identity);
                    Instantiate(wall, new Vector3(i, 1.0f, j), Quaternion.identity);
                    Instantiate(floor, new Vector3(i, 0.0f, j), Quaternion.identity);
                    //tiles[i, j] = Tile.Wall;
                    i++;
                }
                else if (line[k] == 'G')
                {
                    //myObjects[i, j] = Instantiate(goal, new Vector3(i, 0.0f, j), Quaternion.identity);
                    Instantiate(goal, new Vector3(i, 0.0f, j), Quaternion.identity);
                    //tiles[i, j] = Tile.Goal;
                    onGoalSet.Raise(this, 0);
                    i++;
                }
                else if (line[k] == 'F')
                {
                    //myObjects[i, j] = Instantiate(floor, new Vector3(i, 0.0f, j), Quaternion.identity);
                    Instantiate(floor, new Vector3(i, 0.0f, j), Quaternion.identity);
                    //tiles[i, j] = Tile.Floor;
                    i++;

                }
                else if (line[k] == 'B')
                {
                    //myObjects[i, j] = Instantiate(box, new Vector3(i, 0.95f, j), Quaternion.identity);
                    Instantiate(box, new Vector3(i, 0.95f, j), Quaternion.identity);
                    Instantiate(floor, new Vector3(i, 0.0f, j), Quaternion.identity);
                    //tiles[i, j] = Tile.Box;
                    boxes++;
                    i++;
                }
                else if (line[k] == 'P')
                {
                    //myObjects[i, j] = Instantiate(player, new Vector3(i, 0.8f, j), Quaternion.identity);
                    Instantiate(player, new Vector3(i, 0.8f, j), Quaternion.identity);
                    Instantiate(floor, new Vector3(i, 0.0f, j), Quaternion.identity);
                    //tiles[i, j] = Tile.Player;
                    currentPos = new Vector3(i, 1.0f, j);
                    currentY = j;
                    currentX = i;
                    //t_player = myObjects[i, j].GetComponent<Transform>();
                    i++;
                }
                else if (line[k] == 'D')
                {
                    //myObjects[i, j] = Instantiate(portal, new Vector3(i, 1.0f, j), transform.rotation * Quaternion.Euler(0, 180, 0));
                    Instantiate(portal, new Vector3(i, 1.0f, j), transform.rotation * Quaternion.Euler(0, 180, 0));
                    //tiles[i, j] = Tile.DownPortal;
                    i++;
                    
                }
                else if (line[k] == 'U')
                {
                    //myObjects[i, j] = Instantiate(portal, new Vector3(i, 1.0f, j), Quaternion.identity);
                    Instantiate(portal, new Vector3(i, 1.0f, j), Quaternion.identity);
                    //tiles[i, j] = Tile.UpPortal;
                    i++;
                    
                }
                else if (line[k] == 'L')
                {
                    //myObjects[i, j] = Instantiate(portal, new Vector3(i, 1.0f, j), transform.rotation * Quaternion.Euler(0, 270, 0));
                    Instantiate(portal, new Vector3(i, 1.0f, j), transform.rotation * Quaternion.Euler(0, 270, 0));
                    //tiles[i, j] = Tile.LeftPortal;
                    i++;
                    
                }
                else if (line[k] == 'R')
                {
                    //myObjects[i, j] = Instantiate(portal, new Vector3(i, 1.0f, j), transform.rotation * Quaternion.Euler(0, 90, 0));
                    Instantiate(portal, new Vector3(i, 1.0f, j), transform.rotation * Quaternion.Euler(0, 90, 0));
                    //tiles[i, j] = Tile.RightPortal;
                    i++;
                    
                }
                else if (line[k] == 'Q')
                {
                    //myObjects[i, j] = Instantiate(fire, new Vector3(i, 0.0f, j), transform.rotation * Quaternion.identity);
                    Instantiate(fire, new Vector3(i, 0.0f, j), transform.rotation * Quaternion.identity);
                    //tiles[i, j] = Tile.RightPortal;
                    i++;
                }
                else if (line[k] == 'I')
                {
                    Instantiate(ice, new Vector3(i, 0.0f, j), transform.rotation * Quaternion.identity);
                    i++;
                }
            }
        }

        for (int j = -1; j <= maxY; j++)
        {
            Instantiate(wall, new Vector3(-1, 1.0f, j), Quaternion.identity);
            Instantiate(wall, new Vector3(maxX, 1.0f, j), Quaternion.identity);
            Instantiate(floor, new Vector3(-1, 0.0f, j), Quaternion.identity);
            Instantiate(floor, new Vector3(maxX, 0.0f, j), Quaternion.identity);
        }
        for (int i = -1; i <= maxX; i++)
        {
            Instantiate(wall, new Vector3(i, 1.0f, -1), Quaternion.identity);
            Instantiate(wall, new Vector3(i, 1.0f, maxY), Quaternion.identity);
            Instantiate(floor, new Vector3(i, 0.0f, -1), Quaternion.identity);
            Instantiate(floor, new Vector3(i, 0.0f, maxY), Quaternion.identity);
        }

        PlayerPrefs.SetInt("LevelBoxes", boxes);
        camera.Translate(new Vector3((maxX / 2.0f) - 0.5f, 0.0f, (maxY / 2.0f) - 0.5f));
        reader.Close();
    }

    // Update is called once per frame
    void Update()
    {
        /**if (!moving)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (CanGoDown())
                {
                    MoveDown();
                    direction = 0;
                    UpdateTiles();
                }
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (CanGoUp())
                {
                    MoveUp();
                    direction = 1;
                    UpdateTiles();
                }
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (CanGoLeft())
                {
                    MoveLeft();
                    direction = 2;
                    UpdateTiles();
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (CanGoRight())
                {
                    MoveRight();
                    direction = 3;
                    UpdateTiles();
                }
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene("LevelPlayer");
            }
        }
        else
        {
            t_player.position = Vector3.MoveTowards(t_player.position, goalPos, 3.5f * Time.deltaTime);
            if (Mathf.Abs(goalPos.y - t_player.position.y) < 0.0001f && Mathf.Abs(goalPos.x - t_player.position.x) < 0.0001)
            {
                moving = false;
                currentPos = goalPos;
                currentX = (int)currentPos.x;
                currentY = (int)currentPos.y;
                //UpdateTiles();
            }
        }
        //Debug.Log ("Boxes: " + boxes);
        //Debug.Log("Good Boxes: " + goodBoxes);
        /**if (goodBoxes == boxes)
        {
            SceneManager.LoadScene("VictoryScreen");
        }**/
    }



    /**public void MoveDown()
    {
        goalPos = currentPos;
        goalPos.y -= 1.0f;
    }

    public void MoveUp()
    {
        goalPos = currentPos;
        goalPos.y += 1.0f;
    }

    public void MoveLeft()
    {
        goalPos = currentPos;
        goalPos.x -= 1.0f;
    }

    public void MoveRight()
    {
        goalPos = currentPos;
        goalPos.x += 1.0f;
    }

    public bool CanGoDown()
    {
        bool canGo = true;
        if (currentY == 0 || tiles[currentX, currentY - 1] == Tile.Wall)
        {
            canGo = false;
        }
        else if (tiles[currentX, currentY - 1] == Tile.Box || tiles[currentX, currentY - 1] == Tile.BoxGoal)
        {
            if (currentY == 1)
            {
                canGo = false;
            }
            else if (tiles[currentX, currentY - 2] == Tile.Box || tiles[currentX, currentY - 2] == Tile.Wall || tiles[currentX, currentY - 2] == Tile.BoxGoal)
            {
                canGo = false;
            }
        }
        return canGo;
    }

    public bool CanGoUp()
    {
        bool canGo = true;
        if (currentY == maxY - 1  || tiles[currentX, currentY + 1] == Tile.Wall)
        {
            canGo = false;
        }
        else if (tiles[currentX, currentY + 1] == Tile.Box || tiles[currentX, currentY + 1] == Tile.BoxGoal)
        {
            if (currentY == maxY - 2)
            {
                canGo = false;
            }
            else if (tiles[currentX, currentY + 2] == Tile.Box || tiles[currentX, currentY + 2] == Tile.Wall || tiles[currentX, currentY + 2] == Tile.BoxGoal)
            {
                canGo = false;
            }
        }
        return canGo;
    }

    public bool CanGoLeft()
    {
        bool canGo = true;
        if (currentX == 0 || tiles[currentX - 1, currentY] == Tile.Wall)
        {
            canGo = false;
        }
        else if (tiles[currentX - 1, currentY] == Tile.Box || tiles[currentX - 1, currentY] == Tile.BoxGoal)
        {
            if (currentX == 1)
            {
                canGo = false;
            }
            else if (tiles[currentX - 2, currentY] == Tile.Box || tiles[currentX - 2, currentY] == Tile.Wall || tiles[currentX - 2, currentY] == Tile.BoxGoal)
            {
                canGo = false;
            }
        }
        return canGo;
    }

    public bool CanGoRight()
    {
        bool canGo = true;
        if (currentX == maxX - 1 || tiles[currentX + 1, currentY] == Tile.Wall)
        {
            canGo = false;
        }
        else if (tiles[currentX + 1, currentY] == Tile.Box || tiles[currentX + 1, currentY] == Tile.BoxGoal)
        {
            if (currentX == maxX - 2)
            {
                canGo = false;
            }
            else if (tiles[currentX + 2, currentY] == Tile.Box || tiles[currentX + 2, currentY] == Tile.Wall || tiles[currentX + 2, currentY] == Tile.BoxGoal)
            {
                canGo = false;
            }
        }
        return canGo;
    }


    public void UpdateTiles()
    {
        //Avall
        if (direction == 0)
        {
            //Si estem movent una caixa cap avall, aleshores actualitzem els corresponents
            //Debug.Log(currentX);
            //Debug.Log(currentY);
            if (tiles[currentX, currentY - 1] == Tile.Box || tiles[currentX, currentY - 1] == Tile.BoxGoal)
            {
                if (tiles[currentX, currentY - 2] == Tile.Floor)
                {
                    tiles[currentX, currentY - 2] = Tile.Box;
                }
                else if (tiles[currentX, currentY - 2] == Tile.Goal)
                {
                    tiles[currentX, currentY - 2] = Tile.BoxGoal;
                    goodBoxes++;
                }

                if (tiles[currentX, currentY -1] == Tile.Box)
                {
                    tiles[currentX, currentY - 1] = Tile.Floor;
                }
                else if (tiles[currentX, currentY -1] == Tile.BoxGoal)
                {
                    tiles[currentX, currentY - 1] = Tile.Goal;
                    goodBoxes--;
                }
            }
        }
        //Amunt
        if (direction == 1)
        {
            //Si estem movent una caixa cap amunt, aleshores actualitzem els corresponents
            //Debug.Log(currentX);
            //Debug.Log(currentY);
            if (tiles[currentX, currentY + 1] == Tile.Box || tiles[currentX, currentY + 1] == Tile.BoxGoal)
            {
                if (tiles[currentX, currentY + 2] == Tile.Floor)
                {
                    tiles[currentX, currentY + 2] = Tile.Box;
                }
                else if (tiles[currentX, currentY + 2] == Tile.Goal)
                {
                    tiles[currentX, currentY + 2] = Tile.BoxGoal;
                    goodBoxes++;
                }

                if (tiles[currentX, currentY + 1] == Tile.Box)
                {
                    tiles[currentX, currentY + 1] = Tile.Floor;
                }
                else if (tiles[currentX, currentY + 1] == Tile.BoxGoal)
                {
                    tiles[currentX, currentY + 1] = Tile.Goal;
                    goodBoxes--;
                }
            }
        }
        //Esquerra
        if (direction == 2)
        {
            //Si estem movent una caixa cap amunt, aleshores actualitzem els corresponents
            //Debug.Log(currentX);
            //Debug.Log(currentY);
            if (tiles[currentX - 1, currentY] == Tile.Box || tiles[currentX - 1, currentY] == Tile.BoxGoal)
            {
                if (tiles[currentX - 2, currentY] == Tile.Floor)
                {
                    tiles[currentX - 2, currentY] = Tile.Box;
                }
                else if (tiles[currentX - 2, currentY] == Tile.Goal)
                {
                    tiles[currentX - 2, currentY] = Tile.BoxGoal;
                    goodBoxes++;
                }

                if (tiles[currentX - 1, currentY] == Tile.Box)
                {
                    tiles[currentX - 1, currentY] = Tile.Floor;
                }
                else if (tiles[currentX - 1, currentY] == Tile.BoxGoal)
                {
                    tiles[currentX - 1, currentY] = Tile.Goal;
                    goodBoxes--;
                }
            }

        }
        //Dreta
        if (direction == 3)
        {
            //Si estem movent una caixa cap amunt, aleshores actualitzem els corresponents
            //Debug.Log(currentX);
            //Debug.Log(currentY);
            if (tiles[currentX + 1, currentY] == Tile.Box || tiles[currentX + 1, currentY] == Tile.BoxGoal)
            {
                if (tiles[currentX + 2, currentY] == Tile.Floor)
                {
                    tiles[currentX + 2, currentY] = Tile.Box;
                }
                else if (tiles[currentX + 2, currentY] == Tile.Goal)
                {
                    tiles[currentX + 2, currentY] = Tile.BoxGoal;
                    goodBoxes++;
                }

                if (tiles[currentX + 1, currentY] == Tile.Box)
                {
                    tiles[currentX + 1, currentY] = Tile.Floor;
                }
                else if (tiles[currentX + 1, currentY] == Tile.BoxGoal)
                {
                    tiles[currentX + 1, currentY] = Tile.Goal;
                    goodBoxes--;
                }
            }

        }
        //Moure la caixa (ho faré des de la caixa mateix)
        //Comprovar si la caixa està damunt d'una goal (ho faré des de la caixa mateix)
        //Actualitzar myObjects POTSER NO CAL
    }
            
**/
}
