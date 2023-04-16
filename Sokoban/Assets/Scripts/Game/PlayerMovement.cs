using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

public class PlayerMovement : MonoBehaviour
{

    private PlayerControls playerControls;


    
    private enum Tile { Floor, Box, Wall, Goal, Player, BoxGoal, PlayerGoal };
    private Tile[,] tiles;
    /**private GameObject[,] myObjects;
    private Vector3 playerPos;
    private int boxes;
    private int goals;**/

    private Vector3 currentPos;
    private int currentX;
    private int currentZ;

    private Vector3 goalPos;
    private int maxZ;
    private int maxX;
    private bool canMove;


    [Header ("GAME EVENTS")]
    public GameEvent onTurnPass;

    //We initialize
    private void Awake()
    {
        playerControls = new PlayerControls();
    }


    //We enable the controls
    private void OnEnable()
    {
        playerControls.Enable();
    }

    //We disable the controls
    private void OnDisable()
    {
        playerControls.Disable();
    }


    // Start is called before the first frame update
    void Start()
    {
        //We subscribe to the Move mapping and call the Method Move
        playerControls.Player.Move.performed += Move;

        //We get the current position of our player, make it the goal position AND keep the currentX and currentY as integers
        currentPos = transform.position;
        goalPos = currentPos;
        currentX = (int)currentPos.x;
        currentZ = (int)currentPos.z;



        /////////////////////////TILE ARRAY GENERATION/////////////////////////////////////
        //THIS LINE WILL CHANGE TO BECOME "ASSETS\\LEVELDESIGNS\\" + CHOSENLEVEL;
        string path = "Assets\\LevelDesigns\\Test_1.txt";
        //Debug.Log(path);

        //We read the file with the level
        StreamReader reader = new StreamReader(path, true);

        //The first line and second line are the width and height respectively
        string line;
        line = reader.ReadLine();
        int.TryParse(line, out maxX);
        line = reader.ReadLine();
        int.TryParse(line, out maxZ);

        //We initialize the tile array with the values taken from the level file
        tiles = new Tile[maxX, maxZ];

        //We loop through all the lines, and save in "tiles" what each tile has (wall, floor, goal, box, or player). We will use this to check if the player can or can't move in one direction
        for (int j = 0; j < maxZ; j++)
        {
            line = "";
            line = reader.ReadLine();
            Debug.Log(line);
            for (int i = 0; i < maxX; i++)
            {
                if (line[i] == 'W')
                {
                    tiles[i, j] = Tile.Wall;
                }
                else if (line[i] == 'G')
                {
                    tiles[i, j] = Tile.Goal;
                }
                else if (line[i] == 'F')
                {
                    tiles[i, j] = Tile.Floor;
                }
                else if (line[i] == 'B')
                {
                    tiles[i, j] = Tile.Box;
                }
                else if (line[i] == 'P')
                {
                    tiles[i, j] = Tile.Player;
                }
            }
        }
    }

    private void Move(InputAction.CallbackContext context)
    {
        if (canMove){
            if (context.ReadValue<Vector2>().x < 0.0f)
            {
                if (CanMoveLeft())
                {
                    canMove = false;
                    MoveLeft();
                    onTurnPass.Raise();
                }
            }
            else if (context.ReadValue<Vector2>().x > 0.0f)
            {
                if (CanMoveRight())
                {
                    canMove = false;
                    MoveRight();
                    onTurnPass.Raise();
                }
            }
            else if (context.ReadValue<Vector2>().y < 0.0f)
            {
                if (CanMoveDown())
                {
                    canMove = false;
                    MoveDown();
                    onTurnPass.Raise();
                }
            }
            else if (context.ReadValue<Vector2>().y > 0.0f)
            {
                if (CanMoveUp())
                {
                    canMove = false;
                    MoveUp();
                    onTurnPass.Raise();
                }
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, goalPos, 2.5f * Time.deltaTime);
        if (transform.position == goalPos)
        {
            currentPos = goalPos;
            currentX = (int)currentPos.x;
            currentZ = (int)currentPos.z;
            canMove = true;
        }
    }




    //MOVEMENT METHODS
    private bool CanMoveDown()
    {
        Debug.Log(currentZ);
        Debug.Log(maxZ);
        if (currentZ < 1)
        {
            return false;
        }
        //If there's a wall in that direction, it can't move.
        if (tiles[currentX, currentZ - 1] == Tile.Wall)
        {
            return false;
        }
        //If there's a box followed by another box or a wall in that direction, it can't move.
        else if (tiles[currentX, currentZ - 1] == Tile.Box)
        {
            if (currentZ >= 2)
            {
                if (tiles[currentX, currentZ - 2] == Tile.Box || tiles[currentX, currentZ - 2] == Tile.Wall)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }
        return true;
    }

    private bool CanMoveUp()
    {        
        Debug.Log(currentZ);
        Debug.Log(maxZ);
        if (currentZ >= maxZ - 1)
        {
            return false;
        }

        //If there's a wall in that direction, it can't move.
        if (tiles[currentX, currentZ + 1] == Tile.Wall)
        {
            return false;
        }
        //If there's a box followed by another box or a wall in that direction, it can't move.
        else if (tiles[currentX, currentZ + 1] == Tile.Box)
        {
            if (currentZ < maxZ - 2)
            {
                if (tiles[currentX, currentZ + 2] == Tile.Box || tiles[currentX, currentZ + 2] == Tile.Wall)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    private bool CanMoveLeft()
    {
        Debug.Log(currentX);
        Debug.Log(maxX);
        if (currentX < 1)
        {
            return false;
        }
        //If there's a wall in that direction, it can't move.
        if (tiles[currentX - 1, currentZ] == Tile.Wall)
        {
            return false;
        }
        //If there's a box followed by another box or a wall in that direction, it can't move.
        else if (tiles[currentX - 1, currentZ] == Tile.Box)
        {
            if (currentX >= 2)
            {
                if (tiles[currentX - 2, currentZ] == Tile.Box || tiles[currentX - 2, currentZ] == Tile.Wall)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    private bool CanMoveRight()
    {
        Debug.Log(currentX);
        Debug.Log(maxX);
        if (currentX >= maxX - 1)
        {
            return false;
        }
        //If there's a wall in that direction, it can't move.
        if (tiles[currentX + 1, currentZ] == Tile.Wall)
        {
            return false;
        }
        //If there's a box followed by another box or a wall in that direction, it can't move.
        else if (tiles[currentX + 1, currentZ] == Tile.Box)
        {
            if (currentX < maxX - 2)
            {
                if (tiles[currentX + 2, currentZ] == Tile.Box || tiles[currentX + 2, currentZ] == Tile.Wall)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }


    private void MoveLeft()
    {
        UpdateTiles(0);
        goalPos = currentPos;
        goalPos.x -= 1.0f;
    }


    private void MoveRight()
    {
        UpdateTiles(1);
        goalPos = currentPos;
        goalPos.x += 1.0f;
    }


    private void MoveDown()
    {
        UpdateTiles(2);
        goalPos = currentPos;
        goalPos.z -= 1.0f;
    }


    private void MoveUp()
    {
        UpdateTiles(3);
        goalPos = currentPos;
        goalPos.z += 1.0f;
    }


    private void UpdateTiles(int movement)
    {
        int goalX;
        int goalZ;
        int nextX;
        int nextZ;

        //If we're moving to the left
        if (movement == 0)
        {
            goalX = currentX - 1;
            nextX = currentX - 2;
            goalZ = currentZ;
            nextZ = currentZ;
        }
        //If we're moving to the right
        else if (movement == 1)
        {
            goalX = currentX + 1;
            nextX = currentX + 2;
            goalZ = currentZ;
            nextZ = currentZ;
        }
        //If we're moving down
        else if (movement == 2)
        {
            goalX = currentX;
            nextX = currentX;
            goalZ = currentZ - 1;
            nextZ = currentZ - 2;
        }
        //If we're moving up
        else
        {
            goalX = currentX;
            nextX = currentX;
            goalZ = currentZ + 1;
            nextZ = currentZ + 2;
        }

        //If we're moving into floor
        if (tiles[goalX, goalZ] == Tile.Floor)
        {
            //If we're standing on the floor, we swap floor for player
            if (tiles[currentX, currentZ] == Tile.Player)
            {
                tiles[goalX, goalZ] = Tile.Player;
                tiles[currentX, currentZ] = Tile.Floor;
            }
            //If we're standing on a goal, we move the player and leave a goal only
            else if (tiles[currentX, currentZ] == Tile.PlayerGoal)
            {
                tiles[goalX, goalZ] = Tile.Player;
                tiles[currentX, currentZ] = Tile.Goal;
            }

        }
        //If we're moving into a goal
        else if (tiles[goalX, goalZ] == Tile.Goal)
        {
            //If we're currently on the floor, we swap the goal for playergoal and leave a floor
            if (tiles[currentX, currentZ] == Tile.Player)
            {
                tiles[goalX, goalZ] = Tile.PlayerGoal;
                tiles[currentX, currentZ] = Tile.Floor;    
            }
            //If we're currently on a goal, we move onto a playergoal and leave a goal
            else if (tiles[currentX, currentZ] == Tile.PlayerGoal)
            {
                tiles[goalX, goalZ] = Tile.PlayerGoal;
                tiles[currentX, currentZ] = Tile.Goal;
            }
        }
        //If we're moving into a Box (pushing it)
        else if (tiles[goalX, goalZ] == Tile.Box)
        {
            tiles[goalX, goalZ] = Tile.Player;
            //If next to the box there's a goal, we will put a BoxGoal there, a
            if (tiles[nextX, nextZ] == Tile.Goal)
            {
                tiles[nextX, nextZ] = Tile.BoxGoal;
            }
            else if (tiles[nextX, nextZ] == Tile.Floor)
            {
                tiles[nextX, nextZ] = Tile.Box;
            }
            if (tiles[currentX, currentZ] == Tile.Player)
            {
                tiles[currentX, currentZ] = Tile.Floor;
            }
            else if (tiles[currentX, currentZ] == Tile.PlayerGoal)
            {
                tiles[currentX, currentZ] = Tile.Goal;
            }
        }
        else if (tiles[goalX, goalZ] == Tile.BoxGoal)
        {
            tiles[goalX, goalZ] = Tile.PlayerGoal;
            //If next to the box there's a goal, we will put a BoxGoal there, a
            if (tiles[nextX, nextZ] == Tile.Goal)
            {
                tiles[nextX, nextZ] = Tile.BoxGoal;
            }
            else if (tiles[nextX, nextZ] == Tile.Floor)
            {
                tiles[nextX, nextZ] = Tile.Box;
            }
            if (tiles[currentX, currentZ] == Tile.Player)
            {
                tiles[currentX, currentZ] = Tile.Floor;
            }
            else if (tiles[currentX, currentZ] == Tile.PlayerGoal)
            {
                tiles[currentX, currentZ] = Tile.Goal;
            }
        }
    }










    void OnCollisionEnter(Collision coll)
    {

        if (coll.transform.tag == "Wall")
        {
            goalPos = currentPos;
        }
    }

}
