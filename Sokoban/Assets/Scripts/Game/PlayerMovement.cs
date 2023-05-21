using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using System;


public class PlayerMovement : MonoBehaviour
{





    public class CoordPair
    {
        private int here_x;
        private int here_z;
        private int there_x;
        private int there_z;

        public CoordPair(int _x, int _z, int _x2, int _z2)
        {
            here_x = _x;
            here_z = _z;
            there_x = _x2;
            there_z = _z2;
        }

        public void SetHereX(int x)
        {
            here_x = x;
        }

        public void SetHereZ(int z)
        {
            here_z = z;
        }

        public void SetThereX(int x)
        {
            there_x = x;
        }

        public void SetThereZ(int z)
        {
            there_z = z;
        }


        public int GetHereX()
        {
            return here_x;
        }

        public int GetHereZ()
        {
            return here_z;
        }

        public int GetThereX()
        {
            return there_x;
        }

        public int GetThereZ()
        {
            return there_z;
        }

    }




    private PlayerControls playerControls;


    
    public enum Tile { None, Floor, Box, Wall, Goal, Player, PlayerGoal, BoxGoal, LeftPortal, RightPortal, UpPortal, DownPortal, Fire, BoxFire };
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
    private bool firstTime;
    private bool half;
    private bool willBeDestroyed;
    private Vector3 portalGoalPos;

    private List<CoordPair> portalPairings; 

    private int matchPortalX;
    private int matchPortalY;

    private bool portaling;

    [Header ("GAME EVENTS")]
    public GameEvent onTurnPass;
    public GameEvent onEnterPortal;
    public GameEvent onEnterPortal2;
    public GameEvent onEnterPortal3;
    public GameEvent onEnterPortal4;
    public GameEvent onEnterPortal5;
    public GameEvent onBoxPortal;
    public GameEvent onBoxPortal2;

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
        half = false;
        willBeDestroyed = false;

        //Llista que guarda parelles de portals: [[a,b],[c,d]], [[e,f],[g,h]]
        portalPairings = new List<CoordPair>();
        portaling = false;
        firstTime = true;
        /////////////////////////TILE ARRAY GENERATION/////////////////////////////////////
        //THIS LINE WILL CHANGE TO BECOME "ASSETS\\LEVELDESIGNS\\" + CHOSENLEVEL;
        //If the tile array is not full, we read the file. If it's already been filled, we don't.
        if (tiles == null)
        {
            string path = "Assets\\LevelDesigns\\" + PlayerPrefs.GetString("ChosenLevel") + ".txt";

            //We read the file with the level
            StreamReader reader = new StreamReader(path, true);

            //The first line and second line are the width and height respectively
            string line;
            line = reader.ReadLine();
            int.TryParse(line, out maxX);
            line = reader.ReadLine();
            int.TryParse(line, out maxZ);

            tiles = new Tile[maxX, maxZ];

            //We initialize the tile array with the values taken from the level file

            //We loop through all the lines, and save in "tiles" what each tile has (wall, floor, goal, box, or player). We will use this to check if the player can or can't move in one direction
            for (int j = 0; j < maxZ; j++)
            {
                int i = 0;
                line = "";
                line = reader.ReadLine();
                for (int k = 0; k < line.Length; k++)
                {
                    if (line[k] == 'W')
                    {
                        tiles[i, j] = Tile.Wall;
                        i++;
                    }
                    else if (line[k] == 'G')
                    {
                        tiles[i, j] = Tile.Goal;
                        i++;
                    }
                    else if (line[k] == 'F')
                    {
                        tiles[i, j] = Tile.Floor;
                        i++;

                    }
                    else if (line[k] == 'B')
                    {
                        tiles[i, j] = Tile.Box;
                        i++;
                    }
                    else if (line[k] == 'P')
                    {
                        tiles[i, j] = Tile.Player;
                        i++;
                    }
                    else if (line[k] == 'D')
                    {
                        tiles[i, j] = Tile.DownPortal;
                        matchPortalX = (int)Char.GetNumericValue(line[k+1]);
                        matchPortalY = (int)Char.GetNumericValue(line[k+2]);
                        portalPairings.Add(new CoordPair(i, j, matchPortalX, matchPortalY));
                        i++;
                        
                    }
                    else if (line[k] == 'U')
                    {
                        tiles[i, j] = Tile.UpPortal;
                        matchPortalX = (int)Char.GetNumericValue(line[k+1]);
                        matchPortalY = (int)Char.GetNumericValue(line[k+2]);
                        portalPairings.Add(new CoordPair(i, j, matchPortalX, matchPortalY));
                        i++;
                        
                    }
                    else if (line[k] == 'L')
                    {
                        tiles[i, j] = Tile.LeftPortal;
                        matchPortalX = (int)Char.GetNumericValue(line[k+1]);
                        matchPortalY = (int)Char.GetNumericValue(line[k+2]);
                        portalPairings.Add(new CoordPair(i, j, matchPortalX, matchPortalY));
                        i++;
                        
                    }
                    else if (line[k] == 'R')
                    {
                        tiles[i, j] = Tile.RightPortal;
                        matchPortalX = (int)Char.GetNumericValue(line[k+1]);
                        matchPortalY = (int)Char.GetNumericValue(line[k+2]);
                        portalPairings.Add(new CoordPair(i, j, matchPortalX, matchPortalY));
                        i++;
                        
                    }
                    else if (line[k] == 'Q')
                    {
                        tiles[i, j] = Tile.Fire;
                        i++;
                    }
                }
            }
        }        
    }

    private void Move(InputAction.CallbackContext context)
    {
        half = false;
        if (canMove){
            if (context.ReadValue<Vector2>().x < 0.0f)
            {
                if (CanMoveLeft())
                {
                    if (half)
                    {
                        canMove = false;
                        MoveLeft(0.3f);
                        onTurnPass.Raise(this, 0);
                    }
                    else
                    {
                        canMove = false;
                        MoveLeft(1.0f);
                        onTurnPass.Raise(this, 0);
                    }

                }
            }
            else if (context.ReadValue<Vector2>().x > 0.0f)
            {
                if (CanMoveRight())
                {
                    if (half)
                    {
                        canMove = false;
                        MoveRight(0.3f);
                        onTurnPass.Raise(this, 0);
                    }
                    else
                    {
                        canMove = false;
                        MoveRight(1.0f);
                        onTurnPass.Raise(this, 0);
                    }

                }
            }
            else if (context.ReadValue<Vector2>().y < 0.0f)
            {
                if (CanMoveDown())
                {
                    if (half)
                    {
                        canMove = false;
                        MoveDown(0.3f);
                        onTurnPass.Raise(this, 0);
                    }
                    else
                    {
                        canMove = false;
                        MoveDown(1.0f);
                        onTurnPass.Raise(this, 0);
                    }
                }
            }
            else if (context.ReadValue<Vector2>().y > 0.0f)
            {
                if (CanMoveUp())
                {
                    if (half)
                    {
                        canMove = false;
                        MoveUp(0.3f);
                        onTurnPass.Raise(this, 0);
                    }
                    else
                    {
                        canMove = false;
                        MoveUp(1.0f);
                        onTurnPass.Raise(this, 0);
                    }
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
            if (!portaling && !half)
            {
                currentPos = goalPos;
                currentX = (int)currentPos.x;
                currentZ = (int)currentPos.z;
                canMove = true;
            }
            else if (portaling && !half)
            {
                Destroy(gameObject);
            }
            else if (!portaling && half)
            {
                canMove = true;
                goalPos = new Vector3 (currentX, 1.0f, currentZ);
                half = false;
            }
            else if (portaling && half)
            {
                canMove = true;
                goalPos = new Vector3 (currentX, 1.0f, currentZ);
            }
            if (willBeDestroyed && half)
            {
                goalPos = new Vector3 (currentX, 1.0f, currentZ);
                if (transform.position == goalPos)
                {
                    Destroy(gameObject);
                }
            }
        }
    }




    //MOVEMENT METHODS
    private bool CanMoveDown()
    {
        if (currentZ < 1)
        {
            return false;
        }
        //If there's a wall in that direction, it can't move.
        if (tiles[currentX, currentZ - 1] == Tile.Wall || tiles[currentX, currentZ - 1] == Tile.DownPortal || tiles[currentX, currentZ - 1] == Tile.LeftPortal || tiles[currentX, currentZ - 1] == Tile.RightPortal || tiles[currentX, currentZ - 1] == Tile.Fire)
        {
            return false;
        }
        //If there's a portal in that direction, we'll check what's on the other side.
        else if (tiles[currentX, currentZ - 1] == Tile.UpPortal)
        {
            return CheckPortalPlayerOut(currentX, currentZ - 1);
        }
        //If there's a box followed by;
        else if (tiles[currentX, currentZ - 1] == Tile.Box || tiles[currentX, currentZ - 1] == Tile.BoxGoal)
        {
            //Two or more cells and:
            if (currentZ >= 2)
            {
                //Another box or a wall in that direction, it can't move.
                if (tiles[currentX, currentZ - 2] == Tile.Box || tiles[currentX, currentZ - 2] == Tile.BoxGoal || tiles[currentX, currentZ - 2] == Tile.Wall || tiles[currentX, currentZ - 2] == Tile.DownPortal || tiles[currentX, currentZ - 2] == Tile.LeftPortal || tiles[currentX, currentZ - 2] == Tile.RightPortal || tiles[currentX, currentZ - 2] == Tile.BoxFire)
                {
                    return false;
                }
                //A portal it can enter, we'll check what's on the other side.
                else if (tiles[currentX, currentZ - 2] == Tile.UpPortal)
                {
                    return CheckPortalBoxOut(currentX, currentZ - 2);
                }
            }
            else
            {
                return false;
            }
        }
        //If there's a box on a fire block followed by:
        else if (tiles[currentX, currentZ - 1] == Tile.BoxFire)
        {
            //Two or more cells and:
            if (currentZ >= 2)
            {
                //Another box or a wall in that direction, it can't move.
                if (tiles[currentX, currentZ - 2] == Tile.Box || tiles[currentX, currentZ - 2] == Tile.BoxGoal || tiles[currentX, currentZ - 2] == Tile.Wall || tiles[currentX, currentZ - 2] == Tile.DownPortal || tiles[currentX, currentZ - 2] == Tile.LeftPortal || tiles[currentX, currentZ - 2] == Tile.RightPortal || tiles[currentX, currentZ - 2] == Tile.BoxFire)
                {
                    half = false;
                    return false;
                }
                //A portal it can enter, we'll check what's on the other side.
                else if (tiles[currentX, currentZ - 2] == Tile.UpPortal)
                {
                    bool result = CheckPortalBoxOut(currentX, currentZ - 2);
                    half = result;
                    return result;
                }
                else
                {
                    half = true;
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    //TODO: FIREBOX ACTS LIKE A WALL FOR POS+2
    //TODO: FIREBOX ACTS WEIRD FOR POS+1: MOVE 0.25 AND GO BACK (BOOL CALLED INTOFIRE THAT TRIGGERS ON UPDATE) (DON'T MOVE PLAYER, MOVE FIREBOX IN TILES)



    private bool CanMoveUp()
    {        
        if (currentZ >= maxZ - 1)
        {
            return false;
        }
        //If there's a wall in that direction, it can't move.
        if (tiles[currentX, currentZ + 1] == Tile.Wall || tiles[currentX, currentZ + 1] == Tile.UpPortal || tiles[currentX, currentZ + 1] == Tile.LeftPortal || tiles[currentX, currentZ + 1] == Tile.RightPortal || tiles[currentX, currentZ + 1] == Tile.Fire)
        {
            return false;
        }
        //If there's a portal in that direction, we'll check what's on the other side.
        else if (tiles[currentX, currentZ + 1] == Tile.DownPortal)
        {
            return CheckPortalPlayerOut(currentX, currentZ + 1);
        }
        //If there's a box followed by:
        else if (tiles[currentX, currentZ + 1] == Tile.Box || tiles[currentX, currentZ + 1] == Tile.BoxGoal)
        {
            //Two or more cells and:
            if (currentZ < maxZ - 2)
            {
                //Another box or a wall in that direction, it can't move.
                if (tiles[currentX, currentZ + 2] == Tile.Box || tiles[currentX, currentZ + 2] == Tile.BoxGoal || tiles[currentX, currentZ + 2] == Tile.Wall || tiles[currentX, currentZ + 2] == Tile.UpPortal || tiles[currentX, currentZ + 2] == Tile.LeftPortal || tiles[currentX, currentZ + 2] == Tile.RightPortal || tiles[currentX, currentZ + 2] == Tile.BoxFire)
                {
                    return false;
                }
                //A portal it can enter, we'll cehck what's on the other side.
                else if (tiles[currentX, currentZ + 2] == Tile.DownPortal)
                {
                    return CheckPortalBoxOut(currentX, currentZ + 2);
                }
            }
            else
            {
                return false;
            }
        }
        //If there's a box on a fire block followed by:
        else if (tiles[currentX, currentZ + 1] == Tile.BoxFire)
        {
            //Two or more cells and:
            if (currentZ < maxZ - 2)
            {
                //Another box or a wall in that direction, it can't move.
                if (tiles[currentX, currentZ + 2] == Tile.Box || tiles[currentX, currentZ + 2] == Tile.BoxGoal || tiles[currentX, currentZ + 2] == Tile.Wall || tiles[currentX, currentZ + 2] == Tile.UpPortal || tiles[currentX, currentZ + 2] == Tile.LeftPortal || tiles[currentX, currentZ + 2] == Tile.RightPortal || tiles[currentX, currentZ + 2] == Tile.BoxFire)
                {
                    half = false;
                    return false;
                }
                //A portal it can enter, we'll check what's on the other side.
                else if (tiles[currentX, currentZ + 2] == Tile.DownPortal)
                {
                    bool result = CheckPortalBoxOut(currentX, currentZ + 2);
                    half = result;
                    return result;
                }
                else
                {
                    half = true;
                    return true;
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
        if (currentX < 1)
        {
            return false;
        }
        //If there's a wall in that direction, it can't move.
        if (tiles[currentX - 1, currentZ] == Tile.Wall || tiles[currentX - 1, currentZ] == Tile.UpPortal || tiles[currentX - 1, currentZ] == Tile.DownPortal || tiles[currentX - 1, currentZ] == Tile.LeftPortal || tiles[currentX - 1, currentZ] == Tile.Fire)
        {
            return false;
        }
        //If there's a portal in that direction, we'll check what's on the other side.
        else if (tiles[currentX - 1, currentZ] == Tile.RightPortal)
        {
            return CheckPortalPlayerOut(currentX - 1, currentZ);
        }
        //If there's a box followed by:
        else if (tiles[currentX - 1, currentZ] == Tile.Box || tiles[currentX - 1, currentZ] == Tile.BoxGoal)
        {
            //Two or more cells and:
            if (currentX >= 2)
            {
                //another box or a wall in that direction, it can't move. 
                if (tiles[currentX - 2, currentZ] == Tile.Box || tiles[currentX - 2, currentZ] == Tile.BoxGoal || tiles[currentX - 2, currentZ] == Tile.Wall || tiles[currentX - 2, currentZ] == Tile.UpPortal || tiles[currentX - 2, currentZ] == Tile.DownPortal || tiles[currentX - 2, currentZ] == Tile.LeftPortal || tiles[currentX - 2, currentZ] == Tile.BoxFire)
                {
                    return false;
                }
                //A portal it can enter, we'll check what's on the other side.
                else if (tiles[currentX - 2, currentZ] == Tile.RightPortal)
                {
                    return CheckPortalBoxOut(currentX - 2, currentZ);
                }
            }
            else
            {
                return false;
            }
        }
        //If there's a box on a fire block followed by:
        else if (tiles[currentX - 1, currentZ] == Tile.BoxFire)
        {
            //Two or more cells and:
            if (currentX >= 2)
            {
                //Another box or a wall in that direction, it can't move.
                if (tiles[currentX - 2, currentZ] == Tile.Box || tiles[currentX - 2, currentZ] == Tile.BoxGoal || tiles[currentX - 2, currentZ] == Tile.Wall || tiles[currentX - 2, currentZ] == Tile.DownPortal || tiles[currentX - 2, currentZ] == Tile.LeftPortal || tiles[currentX - 2, currentZ] == Tile.UpPortal || tiles[currentX - 2, currentZ] == Tile.BoxFire)
                {
                    half = false;
                    return false;
                }
                //A portal it can enter, we'll check what's on the other side.
                else if (tiles[currentX - 2, currentZ] == Tile.RightPortal)
                {
                    bool result = CheckPortalBoxOut(currentX - 2, currentZ);
                    half = result;
                    return result;
                }
                else
                {
                    half = true;
                    return true;
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
        if (currentX >= maxX - 1)
        {
            return false;
        }
        //If there's a wall in that direction, it can't move.
        if (tiles[currentX + 1, currentZ] == Tile.Wall || tiles[currentX + 1, currentZ] == Tile.UpPortal || tiles[currentX + 1, currentZ] == Tile.DownPortal || tiles[currentX + 1, currentZ] == Tile.RightPortal || tiles[currentX + 1, currentZ] == Tile.Fire)
        {
            return false;
        }
        //If there's a portal in that direction, we'll check what's on the other side.
        else if (tiles[currentX + 1, currentZ] == Tile.LeftPortal)
        {
            return CheckPortalPlayerOut(currentX + 1, currentZ);
        }
        //If there's a box followed by:
        else if (tiles[currentX + 1, currentZ] == Tile.Box || tiles[currentX + 1, currentZ] == Tile.BoxGoal)
        {
            //Two or more cells and:
            if (currentX < maxX - 2)
            {
                //another box or a wall in that direction, it can't move.
                if (tiles[currentX + 2, currentZ] == Tile.Box || tiles[currentX + 2, currentZ] == Tile.BoxGoal || tiles[currentX + 2, currentZ] == Tile.Wall || tiles[currentX + 2, currentZ] == Tile.UpPortal || tiles[currentX + 2, currentZ] == Tile.DownPortal || tiles[currentX + 2, currentZ] == Tile.RightPortal)
                {
                    return false;
                }
                //A portal it can enter, we'll check what's on the other side.
                else if (tiles[currentX + 2, currentZ] == Tile.LeftPortal)
                {
                    return CheckPortalBoxOut(currentX + 2, currentZ);
                }
            }
            else
            {
                return false;
            }
        }
        //If there's a box on a fire block followed by:
        else if (tiles[currentX + 1, currentZ] == Tile.BoxFire)
        {
            half = true;
            //Two or more cells and:
            if (currentX < maxX - 2)
            {
                //Another box or a wall in that direction, it can't move.
                if (tiles[currentX + 2, currentZ] == Tile.Box || tiles[currentX + 2, currentZ] == Tile.BoxGoal || tiles[currentX + 2, currentZ] == Tile.Wall || tiles[currentX + 2, currentZ] == Tile.DownPortal || tiles[currentX + 2, currentZ] == Tile.LeftPortal || tiles[currentX + 2, currentZ] == Tile.UpPortal || tiles[currentX + 2, currentZ] == Tile.BoxFire)
                {
                    half = false;
                    return false;
                }
                //A portal it can enter, we'll check what's on the other side.
                else if (tiles[currentX + 2, currentZ] == Tile.LeftPortal)
                {
                    bool result = CheckPortalBoxOut(currentX + 2, currentZ);
                    half = result;
                    return result;
                }
                else
                {
                    half = true;
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }



    private bool CheckPortalPlayerOut(int entry_x, int entry_z)
    {
        int exitX = 0;
        int exitZ = 0;
        int goalX = 0;
        int goalZ = 0;
        int goalXNext = 0;
        int goalZNext = 0;
        float goalXHalf = 0;
        float goalZHalf = 0;


        Tile forbiddenPortal1 = Tile.Wall;
        Tile forbiddenPortal2 = Tile.Wall;
        Tile forbiddenPortal3 = Tile.Wall;
        
        foreach (CoordPair pair in portalPairings)
        {
            if (pair.GetHereX() == entry_x && pair.GetHereZ() == entry_z)
            {
                exitX = pair.GetThereX();
                exitZ = pair.GetThereZ();
            }
        }
        if (tiles[exitX, exitZ] == Tile.DownPortal)
        {
            goalX = exitX;
            goalZ = exitZ - 1;
            goalXNext = exitX;
            goalZNext = exitZ - 2;
            goalXHalf = exitX;
            goalZHalf = exitZ - 0.3f;
            forbiddenPortal1 = Tile.DownPortal;
            forbiddenPortal2 = Tile.RightPortal;
            forbiddenPortal3 = Tile.LeftPortal;
        }
        else if (tiles[exitX, exitZ] == Tile.UpPortal)
        {
            goalX = exitX;
            goalZ = exitZ + 1;
            goalXNext = exitX;
            goalZNext = exitZ + 2;
            goalXHalf = exitX;
            goalZHalf = exitZ + 0.3f;
            forbiddenPortal1 = Tile.UpPortal;
            forbiddenPortal2 = Tile.RightPortal;
            forbiddenPortal3 = Tile.LeftPortal;
        }
        else if (tiles[exitX, exitZ] == Tile.LeftPortal)
        {
            goalX = exitX - 1;
            goalZ = exitZ;
            goalXNext = exitX - 2;
            goalZNext = exitZ;
            goalXHalf = exitX - 0.3f;
            goalZHalf = exitZ;
            forbiddenPortal1 = Tile.DownPortal;
            forbiddenPortal2 = Tile.UpPortal;
            forbiddenPortal3 = Tile.LeftPortal;
        }
        else if (tiles[exitX, exitZ] == Tile.RightPortal)
        {
            goalX = exitX + 1;
            goalZ = exitZ;
            goalXNext = exitX + 2;
            goalZNext = exitZ;
            goalXHalf = exitX + 0.3f;
            goalZHalf = exitZ;
            forbiddenPortal1 = Tile.DownPortal;
            forbiddenPortal2 = Tile.UpPortal;
            forbiddenPortal3 = Tile.RightPortal;
        }
        //If there's a fire or a wall, return false.
        if (tiles[goalX, goalZ] == Tile.Fire || tiles[goalX, goalZ] ==  Tile.Wall)
        {
            return false;
        }
        //If the player exits the portal and there is a box, we will check the next block. Otherwise it can move.
        else if (tiles[goalX, goalZ] == Tile.Box || tiles[goalX, goalZ] == Tile.BoxGoal)
        {
            if (tiles[goalXNext, goalZNext] == Tile.Box || tiles[goalXNext, goalZNext] == Tile.BoxGoal || tiles[goalXNext, goalZNext] == Tile.Wall || tiles[goalXNext, goalZNext] == forbiddenPortal1 || tiles[goalXNext, goalZNext] == forbiddenPortal2 || tiles[goalXNext, goalZNext] == forbiddenPortal3 || tiles[goalXNext, goalZNext] == Tile.BoxFire)
            {
                return false;
            }
            else
            {
                portalGoalPos = new Vector3(goalX, transform.position.y, goalZ);
                return true;
            }
        }
        //If there's a boxFire, we will move half. We will also send the information to the new player, that will be destroyed.
        else if (tiles[goalX, goalZ] == Tile.BoxFire)
        {
            half = true;
            portalGoalPos = new Vector3(goalXHalf, transform.position.y, goalZHalf);
            return true;
        }
        portalGoalPos = new Vector3(goalX, transform.position.y, goalZ);
        return true;
    }


    private bool CheckPortalBoxOut(int entry_x, int entry_z)
    {
        int exitX = 0;
        int exitZ = 0;
        int goalX = 0;
        int goalZ = 0;

        
        foreach (CoordPair pair in portalPairings)
        {
            if (pair.GetHereX() == entry_x && pair.GetHereZ() == entry_z)
            {
                exitX = pair.GetThereX();
                exitZ = pair.GetThereZ();
            }
        }
        if (tiles[exitX, exitZ] == Tile.DownPortal)
        {
            goalX = exitX;
            goalZ = exitZ - 1;
        }
        else if (tiles[exitX, exitZ] == Tile.UpPortal)
        {
            goalX = exitX;
            goalZ = exitZ + 1;
        }
        else if (tiles[exitX, exitZ] == Tile.LeftPortal)
        {
            goalX = exitX - 1;
            goalZ = exitZ;
        }
        else if (tiles[exitX, exitZ] == Tile.RightPortal)
        {
            goalX = exitX + 1;
            goalZ = exitZ;
        }
        //If the box exits the portal and there is a box, wall or incompatible portal, we return false.
        if (tiles[goalX, goalZ] == Tile.Box || tiles[goalX, goalZ] == Tile.BoxGoal || tiles[goalX, goalZ] == Tile.BoxFire)
        {
            return false;
        }
        int[] spawnPoint = new int[2];
        spawnPoint[0] = exitX;
        spawnPoint[1] = exitZ;
        int[] destinationPoint = new int[2];
        destinationPoint[0] = goalX;
        destinationPoint[1] = goalZ;
        StartCoroutine(SendInfoBox(spawnPoint, destinationPoint));
        return true;
    }


    private void MoveLeft(float distance)
    {
        UpdateTiles(0);
        if (firstTime)
        {
            foreach (Collider col in GetComponents<Collider>())
            {
                col.enabled = true;
            }
            firstTime = false;
        }
        goalPos = currentPos;
        goalPos.x -= distance;
    }


    private void MoveRight(float distance)
    {
        UpdateTiles(1);
        if (firstTime)
        {
            foreach (Collider col in GetComponents<Collider>())
            {
                col.enabled = true;
            }
            firstTime = false;
        }
        goalPos = currentPos;
        goalPos.x += distance;
    }


    private void MoveDown(float distance)
    {
        UpdateTiles(2);
        if (firstTime)
        {
            foreach (Collider col in GetComponents<Collider>())
            {
                col.enabled = true;
            }
            firstTime = false;
        }
        goalPos = currentPos;
        goalPos.z -= distance;
    }


    private void MoveUp(float distance)
    {
        UpdateTiles(3);
        if (firstTime)
        {
            foreach (Collider col in GetComponents<Collider>())
            {
                col.enabled = true;
            }
            firstTime = false;
        }
        goalPos = currentPos;
        goalPos.z += distance;
    }


    private void UpdateTiles(int movement)
    {
        int goalX;
        int goalZ;
        int nextX;
        int nextZ;
        Tile allowedPortal = Tile.Floor;

        //If we're moving to the left
        if (movement == 0)
        {
            goalX = currentX - 1;
            nextX = currentX - 2;
            goalZ = currentZ;
            nextZ = currentZ;

            allowedPortal = Tile.RightPortal;
        }
        //If we're moving to the right
        else if (movement == 1)
        {
            goalX = currentX + 1;
            nextX = currentX + 2;
            goalZ = currentZ;
            nextZ = currentZ;

            allowedPortal = Tile.LeftPortal;

        }
        //If we're moving down
        else if (movement == 2)
        {
            goalX = currentX;
            nextX = currentX;
            goalZ = currentZ - 1;
            nextZ = currentZ - 2;

            allowedPortal = Tile.UpPortal;
        }
        //If we're moving up
        else
        {
            goalX = currentX;
            nextX = currentX;
            goalZ = currentZ + 1;
            nextZ = currentZ + 2;

            allowedPortal = Tile.DownPortal;
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
            //If next to the box there's a goal, we will put a BoxGoal there
            if (tiles[nextX, nextZ] == Tile.Goal)
            {
                tiles[nextX, nextZ] = Tile.BoxGoal;
            }
            else if (tiles[nextX, nextZ] == Tile.Floor)
            {
                tiles[nextX, nextZ] = Tile.Box;
            }
            else if (tiles[nextX, nextZ] == Tile.Fire)
            {
                tiles[nextX, nextZ] = Tile.BoxFire;
            }
            else if (tiles[nextX, nextZ] == allowedPortal)
            {
                int exitX = 0;
                int exitZ = 0;

                //Find what's after the portal
                foreach (CoordPair pair in portalPairings)
                {
                    if (pair.GetHereX() == nextX && pair.GetHereZ() == nextZ)
                    {
                        exitX = pair.GetThereX();
                        exitZ = pair.GetThereZ();
                    }
                }
                if (tiles[exitX, exitZ] == Tile.DownPortal)
                {
                    nextX = exitX;
                    nextZ = exitZ - 1;
                }
                else if (tiles[exitX, exitZ] == Tile.UpPortal)
                {
                    nextX = exitX;
                    nextZ = exitZ + 1;
                }
                else if (tiles[exitX, exitZ] == Tile.LeftPortal)
                {
                    nextX = exitX - 1;
                    nextZ = exitZ;
                }
                else if (tiles[exitX, exitZ] == Tile.RightPortal)
                {
                    nextX = exitX + 1;
                    nextZ = exitZ;
                }
                if (tiles[nextX, nextZ] == Tile.Goal)
                {
                    tiles[nextX, nextZ] = Tile.BoxGoal;
                }
                else if (tiles[nextX, nextZ] == Tile.Floor)
                {
                    tiles[nextX, nextZ] = Tile.Box;
                }
                else if (tiles[nextX, nextZ] == Tile.Fire)
                {
                    tiles[nextX, nextZ] = Tile.BoxFire;
                }
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
            //If next to the box there's a goal, we will put a BoxGoal there
            if (tiles[nextX, nextZ] == Tile.Goal)
            {
                tiles[nextX, nextZ] = Tile.BoxGoal;
            }
            else if (tiles[nextX, nextZ] == Tile.Floor)
            {
                tiles[nextX, nextZ] = Tile.Box;
            }
            else if (tiles[nextX, nextZ] == Tile.Fire)
            {
                tiles[nextX, nextZ] = Tile.BoxFire;
            }
            else if (tiles[nextX, nextZ] == allowedPortal)
            {
                int exitX = 0;
                int exitZ = 0;

                //Find what's after the portal
                foreach (CoordPair pair in portalPairings)
                {
                    if (pair.GetHereX() == nextX && pair.GetHereZ() == nextZ)
                    {
                        exitX = pair.GetThereX();
                        exitZ = pair.GetThereZ();
                    }
                }
                if (tiles[exitX, exitZ] == Tile.DownPortal)
                {
                    nextX = exitX;
                    nextZ = exitZ - 1;
                }
                else if (tiles[exitX, exitZ] == Tile.UpPortal)
                {
                    nextX = exitX;
                    nextZ = exitZ + 1;
                }
                else if (tiles[exitX, exitZ] == Tile.LeftPortal)
                {
                    nextX = exitX - 1;
                    nextZ = exitZ;
                }
                else if (tiles[exitX, exitZ] == Tile.RightPortal)
                {
                    nextX = exitX + 1;
                    nextZ = exitZ;
                }
                if (tiles[nextX, nextZ] == Tile.Goal)
                {
                    tiles[nextX, nextZ] = Tile.BoxGoal;
                }
                else if (tiles[nextX, nextZ] == Tile.Floor)
                {
                    tiles[nextX, nextZ] = Tile.Box;
                }
                else if (tiles[nextX, nextZ] == Tile.Fire)
                {
                    tiles[nextX, nextZ] = Tile.BoxFire;
                }
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
        //If we're moving into a BoxFire (pushing it)
        else if (tiles[goalX, goalZ] == Tile.BoxFire)
        {
            tiles[goalX, goalZ] = Tile.Fire;
            //If next to the box there's a goal, we will put a BoxGoal there
            if (tiles[nextX, nextZ] == Tile.Goal)
            {
                tiles[nextX, nextZ] = Tile.BoxGoal;
            }
            else if (tiles[nextX, nextZ] == Tile.Floor)
            {
                tiles[nextX, nextZ] = Tile.Box;
            }
            else if (tiles[nextX, nextZ] == Tile.Fire)
            {
                tiles[nextX, nextZ] = Tile.BoxFire;
            }
            else if (tiles[nextX, nextZ] == allowedPortal)
            {
                int exitX = 0;
                int exitZ = 0;

                //Find what's after the portal
                foreach (CoordPair pair in portalPairings)
                {
                    if (pair.GetHereX() == nextX && pair.GetHereZ() == nextZ)
                    {
                        exitX = pair.GetThereX();
                        exitZ = pair.GetThereZ();
                    }
                }
                if (tiles[exitX, exitZ] == Tile.DownPortal)
                {
                    nextX = exitX;
                    nextZ = exitZ - 1;
                }
                else if (tiles[exitX, exitZ] == Tile.UpPortal)
                {
                    nextX = exitX;
                    nextZ = exitZ + 1;
                }
                else if (tiles[exitX, exitZ] == Tile.LeftPortal)
                {
                    nextX = exitX - 1;
                    nextZ = exitZ;
                }
                else if (tiles[exitX, exitZ] == Tile.RightPortal)
                {
                    nextX = exitX + 1;
                    nextZ = exitZ;
                }
                if (tiles[nextX, nextZ] == Tile.Goal)
                {
                    tiles[nextX, nextZ] = Tile.BoxGoal;
                }
                else if (tiles[nextX, nextZ] == Tile.Floor)
                {
                    tiles[nextX, nextZ] = Tile.Box;
                }
                else if (tiles[nextX, nextZ] == Tile.Fire)
                {
                    tiles[nextX, nextZ] = Tile.BoxFire;
                }
            }
        }
        //If we're entering a portal
        else if (tiles[goalX, goalZ] == allowedPortal)
        {
            int exitX = 0;
            int exitZ = 0;
            //Find what's after the portal.
            foreach (CoordPair pair in portalPairings)
            {
                if (pair.GetHereX() == goalX && pair.GetHereZ() == goalZ)
                {
                    exitX = pair.GetThereX();
                    exitZ = pair.GetThereZ();
                }
            }
            //Check the kind of portal we're leaving
            if (tiles[exitX, exitZ] == Tile.DownPortal)
            {
                goalX = exitX;
                nextX = exitX;
                goalZ = exitZ - 1;
                nextZ = exitZ - 2;
            }
            else if (tiles[exitX, exitZ] == Tile.UpPortal)
            {
                goalX = exitX;
                nextX = exitX;
                goalZ = exitZ + 1;
                nextZ = exitZ + 2;
            }
            else if (tiles[exitX, exitZ] == Tile.LeftPortal)
            {
                goalX = exitX - 1;
                nextX = exitX - 2;
                goalZ = exitZ;
                nextZ = exitZ;
            }
            else if (tiles[exitX, exitZ] == Tile.RightPortal)
            {
                goalX = exitX + 1;
                nextX = exitX + 2;
                goalZ = exitZ;
                nextZ = exitZ;
            }
            //If we're moving into a goal, we set it to playergoal.
            if (tiles[goalX, goalZ] == Tile.Goal)
            {
                tiles[goalX, goalZ] = Tile.PlayerGoal;
            }
            //If we're moving into floor, we set it to player.
            else if (tiles[goalX, goalZ] == Tile.Floor)
            {
                tiles[goalX, goalZ] = Tile.Player;
            }
            //If we're moving into a box, we set it to player and check beyond:
            else if (tiles[goalX, goalZ] == Tile.Box)
            {
                tiles[goalX, goalZ] = Tile.Player;
                //If there's a goal, we set it to boxgoal
                if (tiles[nextX, nextZ] == Tile.Goal)
                {
                    tiles[nextX, nextZ] = Tile.BoxGoal;
                }
                //If there's a floor, we set it to box
                else if (tiles[nextX, nextZ] == Tile.Floor)
                {
                    tiles[nextX, nextZ] = Tile.Box;
                }
                else if (tiles[nextX, nextZ] == Tile.Fire)
                {
                    tiles[nextX, nextZ] = Tile.BoxFire;
                }
            }
            //If we're moving into a boxgoal, we set it to playergoal and check beyond:
            else if (tiles[goalX, goalZ] == Tile.BoxGoal)
            {
                tiles[goalX, goalZ] = Tile.PlayerGoal;
                //If there's a goal, we set it to boxgoal
                if (tiles[nextX, nextZ] == Tile.Goal)
                {
                    tiles[nextX, nextZ] = Tile.BoxGoal;
                }
                //If there's a floor, we set it to box
                else if (tiles[nextX, nextZ] == Tile.Floor)
                {
                    tiles[nextX, nextZ] = Tile.Box;
                }
                else if (tiles[nextX, nextZ] == Tile.Fire)
                {
                    tiles[nextX, nextZ] = Tile.BoxFire;
                }
            }
            //We check what we left behind
            if (tiles[currentX, currentZ] == Tile.Player)
            {
                tiles[currentX, currentZ] = Tile.Floor;
            }
            else if (tiles[currentX, currentZ] == Tile. PlayerGoal)
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


    void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "Portal")
        {
            //Set portaling to true
            portaling = true;
            int sendX = 0;
            int sendZ = 0;
            //Disable all the colliders of the player
            foreach (Collider col in GetComponents<Collider>())
            {
                col.enabled = false;
            }
            
            foreach (CoordPair pair in portalPairings)
            {
                if (!half)
                {
                    if (pair.GetHereX() == goalPos.x && pair.GetHereZ() == goalPos.z)
                    {
                        sendX = pair.GetThereX();
                        sendZ = pair.GetThereZ();
                    }
                }
                else
                {
                    if (Mathf.Abs(pair.GetHereX() - goalPos.x) < 1 && Mathf.Abs(pair.GetHereZ() - goalPos.z) < 1)
                    {
                        sendX = pair.GetThereX();
                        sendZ = pair.GetThereZ();
                    }
                }
                
            }
            int[] send = new int[2];
            send[0] = sendX;
            send[1] = sendZ;
            onEnterPortal4.Raise(this, send);
            StartCoroutine(SendInfoPlayer());
        }
    }




    IEnumerator SendInfoPlayer()
    {
        yield return new WaitForSeconds(0.05f);
        onEnterPortal.Raise(this, tiles);
        onEnterPortal2.Raise(this, portalGoalPos);
        onEnterPortal3.Raise(this, portalPairings);
        onEnterPortal5.Raise(this, half);
    }

    IEnumerator SendInfoBox(int[] spawnPoint, int[] destinationPoint)
    {
        yield return new WaitForSeconds(0.1f);
        onBoxPortal.Raise(this, spawnPoint);
        yield return new WaitForSeconds(0.1f);
        onBoxPortal2.Raise(this, destinationPoint);
    }


    public void ReceiveTiles(Component sender, object _tiles)
    {
        if (!portaling && sender is PortalManager)
        {
            this.tiles = (Tile[,]) _tiles;
        }
    }

    public void ReceiveGoalPos(Component sender, object _portalGoalPos)
    {
        if (!portaling && sender is PortalManager)
        {
            goalPos = (Vector3) _portalGoalPos;
        }
    }

    public void ReceivePortalPairings(Component sender, object _portalPairings)
    {
        if (!portaling && sender is PortalManager)
        {
            portalPairings = (List<CoordPair>) _portalPairings;
        }
    }

    public void ReceiveWillBeDestroyed(Component sender, object _willDestroy)
    {
        if (!portaling && sender is PortalManager)
        {
            willBeDestroyed = (bool)_willDestroy;
        }
    }

}
