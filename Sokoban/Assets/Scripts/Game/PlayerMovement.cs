using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using System;
using System.Linq;


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


    
    public enum Tile { None, Floor, Box, Wall, Goal, Player, PlayerGoal, BoxGoal, LeftPortal, RightPortal, UpPortal, DownPortal, Fire, BoxFire, Ice, BoxIce, PlayerIce, OldBoxIce };
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
    private int[] boxGoalPos;
    private int[] secondTilePos;
    private bool playerHasPortaled;
    private int ice_dir;

    private int[] boxDestinationPoint;
    private int[] boxSpawnPoint;

    private List<CoordPair> portalPairings; 

    private int matchPortalX;
    private int matchPortalY;

    private bool portaling;
    private bool boxHasPortaled;
    private bool boxPermission;

    [Header ("GAME EVENTS")]
    public GameEvent onTurnPass;
    public GameEvent onEnterPortal;
    public GameEvent onEnterPortal2;
    public GameEvent onEnterPortal3;
    public GameEvent onEnterPortal4;
    public GameEvent onEnterPortal5;
    public GameEvent onBoxPortal;
    public GameEvent onBoxPortal2;
    public GameEvent onBoxStop;

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
        boxGoalPos = new int[2];
        boxGoalPos[0] = -1;
        boxGoalPos[1] = -1;
        playerHasPortaled = false;
        secondTilePos = new int[2];

        boxSpawnPoint = new int[2];
        boxDestinationPoint = new int[2];
        boxPermission = false;
        ice_dir = -1;
        boxHasPortaled = false;
        //Llista que guarda parelles de portals: [[a,b],[c,d]], [[e,f],[g,h]]
        portalPairings = new List<CoordPair>();
        portaling = false;
        firstTime = true;
        /////////////////////////TILE ARRAY GENERATION/////////////////////////////////////
        //THIS LINE WILL CHANGE TO BECOME "ASSETS\\LEVELDESIGNS\\" + CHOSENLEVEL;
        //If the tile array is not full, we read the file. If it's already been filled, we don't.
        if (tiles == null)
        {
            string path = Application.dataPath + "/StreamingAssets/LevelDesigns/" + PlayerPrefs.GetString("ChosenLevel") + ".txt";


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
                    else if (line[k] == 'I')
                    {
                        tiles[i, j] = Tile.Ice;
                        i++;
                    }
                }
            }
        }        
    }

    private void Move(InputAction.CallbackContext context)
    {
        half = false;
        portaling = false;
        ice_dir = -1;
        playerHasPortaled = false;
        secondTilePos[0] = -1;
        secondTilePos[1] = -1;
        if (canMove){
            if (context.ReadValue<Vector2>().x < 0.0f)
            {
                int[] result = CalculatePlayerMovement(1, currentX, currentZ, -1, 0);
                if (result[0] != -1)
                {
                    if (half && ice_dir == -1)
                    {
                        canMove = false;
                        MoveLeft(0.3f);
                        onTurnPass.Raise(this, 0);
                    }
                    else if (half && ice_dir != -1)
                    {
                        UpdateTilesPlayer(currentX, currentZ, result[0], result[1]);
                        canMove = false;
                        float distance = Mathf.Abs(result[1] - currentX);
                        MoveLeft(distance - 1);
                        onTurnPass.Raise(this, 0);
                    }
                    else
                    {
                        if (playerHasPortaled)
                        {
                            UpdateTilesPlayer(currentX, currentZ, secondTilePos[0], secondTilePos[1]);
                        }
                        else
                        {
                            UpdateTilesPlayer(currentX, currentZ, result[1], result[2]);
                        }
                        canMove = false;
                        float distance = Mathf.Abs(result[1] - currentX);
                        MoveLeft(distance);
                        onTurnPass.Raise(this, 0);
                    } 

                }
            }
            else if (context.ReadValue<Vector2>().x > 0.0f)
            {
                int[] result = CalculatePlayerMovement(0, currentX, currentZ, 1, 0);
                if (result[0] != -1)
                {
                    if (half && ice_dir == -1)
                    {
                        canMove = false;
                        MoveRight(0.3f);
                        onTurnPass.Raise(this, 0);
                    }
                    else if(half && ice_dir != -1)
                    {
                        UpdateTilesPlayer(currentX, currentZ, result[0], result[1]);
                        canMove = false;
                        float distance = Mathf.Abs(result[1] - currentX);
                        MoveRight(distance - 1);
                        onTurnPass.Raise(this, 0);
                    }
                    else
                    {
                        if (playerHasPortaled)
                        {
                            UpdateTilesPlayer(currentX, currentZ, secondTilePos[0], secondTilePos[1]);
                        }
                        else
                        {
                            UpdateTilesPlayer(currentX, currentZ, result[1], result[2]);
                        }
                        canMove = false;
                        float distance = Mathf.Abs(result[1] - currentX);
                        MoveRight(distance);
                        onTurnPass.Raise(this, 0);
                    }

                }
            }
            else if (context.ReadValue<Vector2>().y < 0.0f)
            {
                int[] result = CalculatePlayerMovement(3, currentX, currentZ, 0, -1);
                if (result[0] != -1)
                {
                    if (half && ice_dir == -1)
                    {
                        canMove = false;
                        MoveDown(0.3f);
                        onTurnPass.Raise(this, 0);
                    }
                    else if (half && ice_dir != -1)
                    {
                        UpdateTilesPlayer(currentX, currentZ, result[0], result[1]);
                        canMove = false;
                        float distance = Mathf.Abs(result[2] - currentZ);
                        MoveDown(distance - 1);
                        onTurnPass.Raise(this, 0);
                    }
                    else
                    {
                        if (playerHasPortaled)
                        {
                            UpdateTilesPlayer(currentX, currentZ, secondTilePos[0], secondTilePos[1]);
                        }
                        else
                        {
                            UpdateTilesPlayer(currentX, currentZ, result[1], result[2]);
                        }
                        canMove = false;
                        float distance = Mathf.Abs(result[2] - currentZ);
                        MoveDown(distance);
                        onTurnPass.Raise(this, 0);
                    }
                }
            }
            else if (context.ReadValue<Vector2>().y > 0.0f)
            {
                int[] result = CalculatePlayerMovement(2, currentX, currentZ, 0, 1);
                if (result[0] != -1)
                {
                    if (half && ice_dir == -1)
                    {
                        canMove = false;
                        MoveUp(0.3f);
                        onTurnPass.Raise(this, 0);
                    }
                    else if (half && ice_dir != -1)
                    {
                        UpdateTilesPlayer(currentX, currentZ, result[0], result[1]);
                        canMove = false;
                        float distance = Mathf.Abs(result[2] - currentZ);
                        MoveUp(distance - 1);
                        onTurnPass.Raise(this, 0);
                    }
                    else
                    {
                        if (playerHasPortaled)
                        {
                            UpdateTilesPlayer(currentX, currentZ, secondTilePos[0], secondTilePos[1]);
                        }
                        else
                        {
                            UpdateTilesPlayer(currentX, currentZ, result[1], result[2]);
                        }
                        canMove = false;
                        float distance = Mathf.Abs(result[2] - currentZ);
                        MoveUp(distance);
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
            boxGoalPos[0] = -1;
            boxGoalPos[1] = -1;
            if (!portaling && !half)
            {
                currentPos = goalPos;
                currentX = (int)currentPos.x;
                currentZ = (int)currentPos.z;
                canMove = true;
            }
            if (ice_dir != -1 && half)
            {
                currentPos = goalPos;
                currentX = (int)currentPos.x;
                currentZ = (int)currentPos.z;
                if (ice_dir == 0)
                {
                    canMove = false;
                    MoveRight(0.3f);
                }
                else if (ice_dir == 1)
                {
                    canMove = false;
                    MoveLeft(0.3f);
                }
                else if (ice_dir == 2)
                {
                    canMove = false;
                    MoveUp(0.3f);
                }
                else if (ice_dir == 3)
                {
                    canMove = false;
                    MoveDown(0.3f);
                }
                ice_dir = -1;
            }
            else if (portaling && !half)
            {
                Destroy(gameObject);
            }
            else if (!portaling && half)
            {
                canMove = true;
                goalPos = new Vector3 (currentX, 1.0f, currentZ);
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
        if (boxHasPortaled && boxPermission)
        {
            StartCoroutine(SendInfoBox(boxSpawnPoint, boxDestinationPoint));
            boxHasPortaled = false;
            boxPermission = false;
        }
    }



    private int[] CalculatePlayerMovement(int direction, int startX, int startZ, int incX, int incZ)
    {
        int goalX = startX + incX;
        int goalZ = startZ + incZ;
        int[] result;

        Tile[,] allBannedPortals = {
            {Tile.UpPortal, Tile.DownPortal, Tile.RightPortal},
            {Tile.UpPortal, Tile.DownPortal, Tile.LeftPortal}, 
            {Tile.LeftPortal, Tile.UpPortal, Tile.RightPortal}, 
            {Tile.DownPortal, Tile.LeftPortal, Tile.RightPortal}
        };
        Tile[] allowedPortals = {Tile.LeftPortal, Tile.RightPortal, Tile.DownPortal, Tile.UpPortal};
        Tile[] bannedPortals = {allBannedPortals[direction, 0], allBannedPortals[direction, 1], allBannedPortals[direction, 2]};
        Tile allowedPortal = allowedPortals[direction];

        //FIRST OF ALL CHECK IF WE ARE IN BOUNDS
        if (goalX < 0 || goalX > maxX - 1 || goalZ < 0 || goalZ > maxZ - 1)
        {
            result = new int[] {-1, startX, startZ};
            return result;
        }

        Tile goal = tiles[goalX, goalZ];

        //If there's a wall, ForbiddenPortal or Fire, return startX, startZ
        if (goal == Tile.Wall || goal == Tile.Fire || bannedPortals.Contains(goal))
        {
            result = new int[] {-1, startX, startZ};
            return result;
        }

        //If there's floor or a goal, return startX + incX, startZ + incZ.
        else if (goal == Tile.Floor || goal == Tile.Goal || goal == Tile.OldBoxIce)
        {
            result = new int[] {direction, goalX, goalZ};
            return result;
        }

        //If there's ice, we call this recursively
        else if (goal == Tile.Ice)
        {
            result = CalculatePlayerMovement(direction, goalX, goalZ, incX, incZ);
            result[0] = direction;
            ice_dir = direction;
            return result;
        }

        //If there's a box or boxgoal, we call CalculateBoxMovement (int direction, int goalX, int goalZ, int incX, int incZ) and see there if the box can move or not.
        else if (goal == Tile.Box || goal == Tile.BoxGoal)
        {
            int[] boxResult = CalculateBoxMovement(direction, goalX, goalZ, incX, incZ);
            if (boxResult[0] == -1)
            {
                result = new int[] {-1, startX, startZ};
            }
            else
            {
                if (boxHasPortaled)
                {
                    UpdateTilesBox(goalX, goalZ, boxDestinationPoint[0], boxDestinationPoint[1]);
                }
                else
                {
                    UpdateTilesBox(goalX, goalZ, boxResult[1], boxResult[2]);
                }

                result = new int[] {direction, goalX, goalZ};
            }
            return result;
        }
        
        //If there's an AllowedPortal, return the position of the portal (or 1 third) and call PortalPlayerOut. Check other variables.
        else if (goal == allowedPortal)
        {
            playerHasPortaled = true;
            int[] portalInfo = PortalPlayerOut(goalX, goalZ);
            int[] postPortalInfo = CalculatePlayerMovement(portalInfo[0], portalInfo[1], portalInfo[2], portalInfo[3], portalInfo[4]);
            if(postPortalInfo[0] == -1 && !half)
            {
                result = new int[] {-1, startX, startZ};
            }
            else
            {
                result = new int[] {direction, goalX, goalZ};
            }
            return result;
        }
        //If there's a BoxFire, set half to true and return goalX, goalZ. call CalculateBoxMovement (int direction, int goalX, int goalZ, int incX, int incZ).
        else if (goal == Tile.BoxFire)
        {
            int[] box_info = CalculateBoxMovement(direction, goalX, goalZ, incX, incZ);
            if (box_info[0] == -1)
            {
                half = false;
                result = new int[] { -1, startX, startZ };
            }
            else
            {
                if (boxHasPortaled)
                {
                    UpdateTilesBox(goalX, goalZ, boxDestinationPoint[0], boxDestinationPoint[1]);
                }
                else
                {
                    UpdateTilesBox(goalX, goalZ, box_info[1], box_info[2]);
                }
                half = true;
                result = new int[] { direction, goalX, goalZ };
            }
            return result;
        }
        //If there's a BoxIce, return the startX + incX, startZ + incZ and call CalculateBoxMovement (int direction, int goalX, int goalZ, int incX, int incZ)
        else if (goal == Tile.BoxIce)
        {
            int[] boxInfo = CalculateBoxMovement(direction, goalX, goalZ, incX, incZ);
            if (boxInfo[0] == -1)
            {
                result = new int[] {-1, startX, startZ};
            }
            else
            {
                result = new int[] {direction, goalX, goalZ};
                if (boxHasPortaled)
                {
                    UpdateTilesBox(goalX, goalZ, boxDestinationPoint[0], boxDestinationPoint[1]);
                }
                else
                {
                    UpdateTilesBox(goalX, goalZ, boxInfo[1], boxInfo[2]);
                }
            }
            return result;
        }
        //This case never happens (for now)
        else
        {
            result = new int[] {-1, -1, -1};
            return result;
        }
    }

    private int[] CalculateBoxMovement(int direction, int startX, int startZ, int incX, int incZ)
    {
        int goalX = startX + incX;
        int goalZ = startZ + incZ;
        int[] result;

        Tile[,] allBannedPortals = {
            {Tile.UpPortal, Tile.DownPortal, Tile.RightPortal},
            {Tile.UpPortal, Tile.DownPortal, Tile.LeftPortal}, 
            {Tile.LeftPortal, Tile.UpPortal, Tile.RightPortal}, 
            {Tile.DownPortal, Tile.LeftPortal, Tile.RightPortal}
        };
        Tile[] allowedPortals = {Tile.LeftPortal, Tile.RightPortal, Tile.DownPortal, Tile.UpPortal};
        Tile[] bannedPortals = {allBannedPortals[direction, 0], allBannedPortals[direction, 1], allBannedPortals[direction, 2]};
        Tile allowedPortal = allowedPortals[direction];

        //FIRST OF ALL CHECK IF WE ARE IN BOUNDS
        if (goalX < 0 || goalX > maxX - 1 || goalZ < 0 || goalZ > maxZ - 1)
        {
            result = new int[] {-1, startX, startZ};
            return result;
        }

        Tile goal = tiles[goalX, goalZ];
        
        //If there's floor, goal or fire return startX + incX, startZ + incZ.
        if (goal == Tile.Floor || goal == Tile.Goal || goal == Tile.Fire)
        {
            result = new int[] {direction, goalX, goalZ};
            return result;
        }
        //If there's a wall, box, boxgoal, boxfire or forbidden portal return startX, startZ
        else if (goal == Tile.Wall || goal == Tile.Box || goal == Tile.BoxGoal || goal == Tile.BoxFire || bannedPortals.Contains(goal))
        {
            result = new int[] {-1, startX, startZ};
            return result;
        }
        //If there's an AllowedPortal: 1. Calculate what's beyond the portal.
        else if (goal == allowedPortal)
        {
            //If the box can move beyond the portal, return dir, goalX, goalZ. If it can't, return -1, startX, startZ.
            int[] postPortalInfo = PortalBoxOut(goalX, goalZ);
            if (postPortalInfo[0] == -1)
            {
                result = new int[] {-1, startX, startZ};
            }
            else
            {
                result = new int[] {postPortalInfo[0], goalX, goalZ};
            }
            return result;
        }
        //If there's ice, we call this recursively
        else if (goal == Tile.Ice)
        {
            result = CalculateBoxMovement(direction, goalX, goalZ, incX, incZ);
            result[0] = direction;
            return result;
        }
        //If there's a BoxIce, return startX + incX, startZ + incZ and call this recursively.
        else if (goal == Tile.BoxIce)
        {
            result = new int[] {-1, startX, startZ};
            return result;
        }
        //This case never happens (for now)
        else
        {
            result = new int[] {-1, -1, -1};
            return result;
        }
    }

    private int[] PortalPlayerOut(int entryX, int entryZ)
    {
        int exitX = 0;
        int exitZ = 0;
        float goalX = 0;
        float goalZ = 0;

        foreach (CoordPair pair in portalPairings)
        {
            if (pair.GetHereX() == entryX && pair.GetHereZ() == entryZ)
            {
                exitX = pair.GetThereX();
                exitZ = pair.GetThereZ();
            }
        }

        Tile [] directions = {Tile.RightPortal, Tile.LeftPortal, Tile.UpPortal, Tile.DownPortal};
        int[,] directions_modify = {{1, 0}, {-1, 0}, {0, 1}, {0, -1}};
        int new_direction = Array.IndexOf(directions, tiles[exitX, exitZ]);
        int incX = directions_modify[new_direction, 0];
        int incZ = directions_modify[new_direction, 1];
        int [] info = {new_direction, exitX, exitZ, incX, incZ};


        //Aquests càlculs són per l'altre jugador. El que farà spawn quan surti el portal
        if (tiles[exitX + incX, exitZ + incZ] == Tile.BoxFire)
        {
            half = true;
            goalX = exitX + (directions_modify[new_direction, 0] / 3.0f);
            goalZ = exitZ + (directions_modify[new_direction, 1] / 3.0f);
        }
        else if (tiles[exitX + incX, exitZ + incZ] == Tile.BoxIce)
        {
            int[] result = CalculatePlayerMovement(new_direction, exitX, exitZ, incX, incZ);
            goalX = result[1];
            goalZ = result[2];
        }
        else if (tiles[exitX + incX, exitZ + incZ] == Tile.Ice)
        {
            int[] result = CalculatePlayerMovement(new_direction, exitX + incX, exitZ + incZ, incX, incZ);
            goalX = result[1];
            goalZ = result[2];
        }
        else
        {
            goalX = exitX + directions_modify[new_direction, 0];
            goalZ = exitZ + directions_modify[new_direction, 1];
        }
        portalGoalPos = new Vector3(goalX, transform.position.y, goalZ);
        secondTilePos[0] = (int)goalX;
        secondTilePos[1] = (int)goalZ;
        return info;
    }


    private int[] PortalBoxOut(int portalX, int portalZ)
    {
        int exitX = 0;
        int exitZ = 0;
        int goalX = 0;
        int goalZ = 0;

        foreach (CoordPair pair in portalPairings)
        {
            if (pair.GetHereX() == portalX && pair.GetHereZ() == portalZ)
            {
                exitX = pair.GetThereX();
                exitZ = pair.GetThereZ();
            }
        }

        Tile [] directions = {Tile.RightPortal, Tile.LeftPortal, Tile.UpPortal, Tile.DownPortal};
        int[,] directions_modify = {{1, 0}, {-1, 0}, {0, 1}, {0, -1}};
        int new_direction = Array.IndexOf(directions, tiles[exitX, exitZ]);

        goalX = exitX + directions_modify[new_direction, 0];
        goalZ = exitZ + directions_modify[new_direction, 1];

        int [] info = {new_direction, exitX, exitZ, goalX, goalZ};


        if (tiles[goalX, goalZ] == Tile.Goal || tiles[goalX, goalZ] == Tile.Floor || tiles[goalX, goalZ] == Tile.Fire)
        {
            boxSpawnPoint[0] = exitX;
            boxSpawnPoint[1] = exitZ;
            boxDestinationPoint[0] = goalX;
            boxDestinationPoint[1] = goalZ;
            boxHasPortaled = true;   
        }
        else if (tiles[goalX, goalZ] == Tile.Ice)
        {
            int[] postPortalInfo = CalculateBoxMovement(new_direction, goalX, goalZ, directions_modify[new_direction, 0], directions_modify[new_direction, 1]);
            if (postPortalInfo[0] == -1)
            {
                info[0] = -1;
            }
            else
            {
                boxHasPortaled = true;
                boxSpawnPoint[0] = exitX;
                boxSpawnPoint[1] = exitZ;
                boxDestinationPoint[0] = postPortalInfo[1];
                boxDestinationPoint[1] = postPortalInfo[2];
            }
        }
        else
        {
            info[0] = -1;
        }
        return info;
    }


    private void MoveLeft(float distance)
    {
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


    private void UpdateTilesPlayer(int firstX, int firstZ, int secondX, int secondZ)
    {
        Tile currentTile = tiles[firstX, firstZ];
        Tile goalTile = tiles[secondX, secondZ];
        //public enum Tile { None, Floor, Box, Wall, Goal, Player, PlayerGoal, BoxGoal, LeftPortal, RightPortal, UpPortal, DownPortal, Fire, BoxFire, Ice, BoxIce, PlayerIce };

        if (currentTile == Tile.Player)
        {
            tiles[firstX, firstZ] = Tile.Floor;
        }
        else if (currentTile == Tile.PlayerGoal)
        {
            tiles[firstX, firstZ] = Tile.Goal;
        }
        else if (currentTile == Tile.PlayerIce)
        {
            tiles[firstX, firstZ] = Tile.Ice;
        }

        if (goalTile == Tile.Floor || goalTile == Tile.Box)
        {
            tiles[secondX, secondZ] = Tile.Player;
        }
        else if (goalTile == Tile.Goal || goalTile == Tile.BoxGoal)
        {
            tiles[secondX, secondZ] = Tile.PlayerGoal;
        }
        else if (goalTile == Tile.Ice || goalTile == Tile.BoxIce || goalTile == Tile.OldBoxIce)
        {
            tiles[secondX, secondZ] = Tile.PlayerIce;
        }
    }


    private void UpdateTilesBox(int firstX, int firstZ, int secondX, int secondZ)
    {
        Tile currentTile = tiles[firstX, firstZ];
        Tile goalTile = tiles[secondX, secondZ];
        //public enum Tile { None, Floor, Box, Wall, Goal, Player, PlayerGoal, BoxGoal, LeftPortal, RightPortal, UpPortal, DownPortal, Fire, BoxFire, Ice, BoxIce, PlayerIce };

        if (currentTile == Tile.Box)
        {
            tiles[firstX, firstZ] = Tile.Floor;
        }
        else if (currentTile == Tile.BoxGoal)
        {
            tiles[firstX, firstZ] = Tile.Goal;
        }
        else if (currentTile == Tile.BoxFire)
        {
            tiles[firstX, firstZ] = Tile.Fire;
        }
        else if (currentTile == Tile.BoxIce)
        {
            tiles[firstX, firstZ] = Tile.OldBoxIce;
        }

        if (goalTile == Tile.Floor)
        {
            tiles[secondX, secondZ] = Tile.Box;
        }
        else if (goalTile == Tile.Goal)
        {
            tiles[secondX, secondZ] = Tile.BoxGoal;
        }
        else if (goalTile == Tile.Fire)
        {
            tiles[secondX, secondZ] = Tile.BoxFire;
        }
        else if (goalTile == Tile.Ice)
        {
            tiles[secondX, secondZ] = Tile.BoxIce;
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
        yield return new WaitForSeconds(0.05f);
        onEnterPortal2.Raise(this, portalGoalPos);
        onEnterPortal3.Raise(this, portalPairings);
        onEnterPortal5.Raise(this, half);
    }

    IEnumerator SendInfoBox(int[] spawnPoint, int[] destinationPoint)
    {
        yield return new WaitForSeconds(0.1f);
        onBoxPortal.Raise(this, spawnPoint);
        yield return new WaitForSeconds(0.1f);
        int[] toSend = { spawnPoint[0], spawnPoint[1], destinationPoint[0], destinationPoint[1] };
        onBoxPortal2.Raise(this, toSend);
        boxSpawnPoint[0] = -1;
        boxSpawnPoint[1] = -1;
        boxDestinationPoint[0] = -1;
        boxDestinationPoint[1] = -1;
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
            half = willBeDestroyed;
        }
    }


    public void ReceiveBoxPermission(Component sender, object _permission)
    {
        if (sender is BoxMovement)
        {
            boxPermission = true;
            if (boxHasPortaled == false)
            {
                onBoxStop.Raise(this, false);
            }
        }
    }
}
