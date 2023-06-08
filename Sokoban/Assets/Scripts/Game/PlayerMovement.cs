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


    
    public enum Tile { None, Floor, Box, Wall, Goal, Player, PlayerGoal, BoxGoal, LeftPortal, RightPortal, UpPortal, DownPortal, Fire, BoxFire, Ice, BoxIce, PlayerIce };
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
        boxGoalPos = new int[2];
        boxGoalPos[0] = -1;
        boxGoalPos[1] = -1;
        secondTilePos = new int[2];

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
        portaling = false;
        if (canMove){
            if (context.ReadValue<Vector2>().x < 0.0f)
            {
                if (CanMove(1))
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
                if (CanMove(0))
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
                if (CanMove(3))
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
                if (CanMove(2))
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
            boxGoalPos[0] = -1;
            boxGoalPos[1] = -1;
            if (!portaling && !half)
            {
                //Debug.Log("A: " + currentPos.x + ", " + currentPos.z);
                currentPos = goalPos;
                currentX = (int)currentPos.x;
                currentZ = (int)currentPos.z;
                canMove = true;
            }
            else if (portaling && !half)
            {
                //Debug.Log("B: " + currentPos.x + ", " + currentPos.z);
                Destroy(gameObject);
            }
            else if (!portaling && half)
            {
                Debug.Log("C: " + currentPos.x + ", " + currentPos.z);
                canMove = true;
                goalPos = new Vector3 (currentX, 1.0f, currentZ);
            }
            else if (portaling && half)
            {
                Debug.Log("D: " + currentPos.x + ", " + currentPos.z);
                Debug.Log("D: " + currentX + ", " + currentZ);
                canMove = true;
                goalPos = new Vector3 (currentX, 1.0f, currentZ);
            }
            if (willBeDestroyed && half)
            {
                //Debug.Log("E: " + currentPos.x + ", " + currentPos.z);
                goalPos = new Vector3 (currentX, 1.0f, currentZ);
                if (transform.position == goalPos)
                {
                    //Debug.Log("F: " + currentPos.x + ", " + currentPos.z);
                    Destroy(gameObject);
                }
            }
        }
    }


    private bool CanMove(int direction)
    {
        //0 = right, 1 = left, 2 = up, 3 = down
        int[,] directions = {{1, 0}, {-1, 0}, {0, 1}, {0, -1}};
        Tile[,] allBannedPortals = {
            {Tile.UpPortal, Tile.DownPortal, Tile.RightPortal},
            {Tile.UpPortal, Tile.DownPortal, Tile.LeftPortal}, 
            {Tile.LeftPortal, Tile.UpPortal, Tile.RightPortal}, 
            {Tile.DownPortal, Tile.LeftPortal, Tile.RightPortal}
        };
        Tile[] allowedPortals = {Tile.LeftPortal, Tile.RightPortal, Tile.DownPortal, Tile.UpPortal};

        //We set the goals, and next.
        int goalX = currentX + directions[direction, 0];
        int goalZ = currentZ + directions[direction, 1];
        int nextX = currentX + directions[direction, 0] * 2;
        int nextZ = currentZ + directions[direction, 1] * 2;
        
        secondTilePos[0] = goalX;
        secondTilePos[1] = goalZ;
        
        int new_direction = direction;
        int box_direction = direction;

        bool boxBool = false;

        if (goalX < 0 || goalX > maxX - 1 || goalZ < 0 || goalZ > maxZ - 1)
        {
            return false;
        }

        Tile[] bannedPortals = {allBannedPortals[direction, 0], allBannedPortals[direction, 1], allBannedPortals[direction, 2]};
        Tile allowedPortal = allowedPortals[direction];

        //Now we have to determine if there is a portal. In that case, we will have to rewrite goals or nexts.
        if (tiles[goalX, goalZ] == allowedPortal)
        {
            int [] new_info = PortalPlayerOut(goalX, goalZ);
            new_direction = new_info[0];
            boxBool = true;
            goalX = new_info[1] + directions[new_direction, 0];
            goalZ = new_info[2] + directions[new_direction, 1];
            nextX = new_info[1] + directions[new_direction, 0] * 2;
            nextZ = new_info[2] + directions[new_direction, 1] * 2;            
            secondTilePos[0] = goalX;
            secondTilePos[1] = goalZ;
        }
        
        
        if (tiles[goalX, goalZ] == Tile.Box || tiles[goalX, goalZ] == Tile.BoxFire || tiles[goalX, goalZ] == Tile.BoxGoal)
        {
            if (nextX < 0 || nextX > maxX - 1 || nextZ < 0 || nextZ > maxZ - 1)
            {
                return false;
            }
            if (tiles[nextX, nextZ] == allowedPortal)
            {
                int [] new_info = PortalBoxOut(nextX, nextZ);
                box_direction = new_info[0];

                nextX = new_info[1] + directions[box_direction, 0];
                nextZ = new_info[2] + directions[box_direction, 1];
            }
        }

        if(goalX < 0 || goalZ < 0 || goalX >= maxX || goalZ >= maxZ)
        {
            return false;
        }
        else if(tiles[goalX, goalZ] == Tile.Wall || bannedPortals.Contains(tiles[goalX, goalZ]) == true || tiles[goalX, goalZ] == Tile.Fire)
        {
            return false;
        }
        else if (tiles[goalX, goalZ] == Tile.Box || tiles[goalX, goalZ] == Tile.BoxGoal)
        {
            if (boxBool)
            {
                CalculateBoxGoalPos(new_direction, goalX, goalZ);
            }
            else
            {
                CalculateBoxGoalPos(direction, goalX, goalZ);
            }
            //Two or more cells and:
            if (nextZ <= maxZ && nextX <= maxX && nextX >= 0 && nextZ >= 0)
            {
                //Another box or a wall in that direction, it can't move.
                if (tiles[nextX, nextZ] == Tile.Wall || bannedPortals.Contains(tiles[nextX, nextZ]) || tiles[nextX, nextZ] == Tile.Box || tiles[nextX, nextZ] == Tile.BoxGoal || tiles[nextX, nextZ] == Tile.BoxFire)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        else if (tiles[goalX, goalZ] == Tile.BoxFire)
        {
            half = true;
            if (nextZ <= maxZ && nextX <= maxX && nextX >= 0 && nextZ >= 0)
            {
                //Another box or a wall in that direction, it can't move.
                if (tiles[nextX, nextZ] == Tile.Box || tiles[nextX, nextZ] == Tile.BoxGoal || tiles[nextX, nextZ] == Tile.Wall || bannedPortals.Contains(tiles[nextX, nextZ]) || tiles[nextX, nextZ] == Tile.BoxFire)
                {
                    half = false;
                    return false;
                }
                //Anything else, it can move.
                else
                {
                    if (boxBool)
                    {
                        CalculateBoxGoalPos(new_direction, goalX, goalZ);
                    }
                    else
                    {
                        CalculateBoxGoalPos(direction, goalX, goalZ);
                    }
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
        int [] info = {new_direction, exitX, exitZ};

        if (tiles[exitX + directions_modify[new_direction, 0], exitZ + directions_modify[new_direction, 1]] == Tile.BoxFire)
        {
            half = true;
            goalX = exitX + (directions_modify[new_direction, 0] / 3.0f);
            goalZ = exitZ + (directions_modify[new_direction, 1] / 3.0f);
        }
        else
        {
            goalX = exitX + directions_modify[new_direction, 0];
            goalZ = exitZ + directions_modify[new_direction, 1];
        }
        portalGoalPos = new Vector3(goalX, transform.position.y, goalZ);
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
        int [] info = {new_direction, exitX, exitZ};

        goalX = exitX + directions_modify[new_direction, 0];
        goalZ = exitZ + directions_modify[new_direction, 1];

        int[] spawnPoint = new int[2];
        spawnPoint[0] = exitX;
        spawnPoint[1] = exitZ;
        int[] destinationPoint = new int[2];
        destinationPoint[0] = goalX;
        destinationPoint[1] = goalZ;
        if (tiles[goalX, goalZ] == Tile.Goal || tiles[goalX, goalZ] == Tile.Floor || tiles[goalX, goalZ] == Tile.Fire)
        {
            StartCoroutine(SendInfoBox(spawnPoint, destinationPoint));
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
        int[,] affectedTiles = {{(int)currentPos.x, (int)currentPos.z}, {secondTilePos[0], secondTilePos[1]}, {boxGoalPos[0], boxGoalPos[1]}};
        UpdateTiles(affectedTiles);
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
        int[,] affectedTiles = {{(int)currentPos.x, (int)currentPos.z}, {secondTilePos[0], secondTilePos[1]}, {boxGoalPos[0], boxGoalPos[1]}};
        UpdateTiles(affectedTiles);
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
        int[,] affectedTiles = {{(int)currentPos.x, (int)currentPos.z}, {secondTilePos[0], secondTilePos[1]}, {boxGoalPos[0], boxGoalPos[1]}};
        UpdateTiles(affectedTiles);
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
        int[,] affectedTiles = {{(int)currentPos.x, (int)currentPos.z}, {secondTilePos[0], secondTilePos[1]}, {boxGoalPos[0], boxGoalPos[1]}};
        UpdateTiles(affectedTiles);
    }


    private void UpdateTiles(int[,] affectedTiles)
    {
        int firstX = affectedTiles[0, 0];
        int firstZ = affectedTiles[0, 1];
        int secondX = affectedTiles[1, 0];
        int secondZ = affectedTiles[1, 1];
        int thirdX = affectedTiles[2, 0];
        int thirdZ = affectedTiles[2, 1];

        if (tiles[firstX, firstZ] == Tile.Player && tiles[secondX, secondZ] != Tile.BoxFire)
        {
            tiles[firstX, firstZ] = Tile.Floor;
        }
        else if (tiles[firstX, firstZ] == Tile.PlayerGoal && tiles[secondX, secondZ] != Tile.BoxFire)
        {
            tiles[firstX, firstZ] = Tile.Goal;
        }
        if (tiles[secondX, secondZ] == Tile.Box || tiles[secondX, secondZ] == Tile.BoxFire || tiles[secondX, secondZ] == Tile.BoxGoal)
        {
            thirdX = affectedTiles[2, 0];
            thirdZ = affectedTiles[2, 1];

            if (tiles[thirdX, thirdZ] == Tile.Fire)
            {
                tiles[thirdX, thirdZ] = Tile.BoxFire;
            }
            else if (tiles[thirdX, thirdZ] == Tile.Goal)
            {
                tiles[thirdX, thirdZ] = Tile.BoxGoal;
            }
            else if (tiles[thirdX, thirdZ] == Tile.Floor)
            {
                tiles[thirdX, thirdZ] = Tile.Box;
            }
        }
        if (tiles[secondX, secondZ] == Tile.Box || tiles[secondX, secondZ] == Tile.Floor)
        {
            tiles[secondX, secondZ] = Tile.Player;
        }
        else if (tiles[secondX, secondZ] == Tile.BoxGoal || tiles[secondX, secondZ] == Tile.Goal)
        {
            tiles[secondX, secondZ] = Tile.PlayerGoal;
        }
        else if (tiles[secondX, secondZ] == Tile.BoxFire)
        {
            tiles[secondX, secondZ] = Tile.Fire;
        }
    }


    private void CalculateBoxGoalPos(int direction, int startingX, int startingZ)
    {
        int[,] directions = {{1, 0}, {-1, 0}, {0, 1}, {0, -1}};
        Tile[,] allBannedPortals = {{Tile.UpPortal, Tile.DownPortal, Tile.RightPortal}, {Tile.UpPortal, Tile.DownPortal, Tile.LeftPortal}, {Tile.LeftPortal, Tile.UpPortal, Tile.RightPortal}, {Tile.DownPortal, Tile.LeftPortal, Tile.RightPortal}};
        Tile[] allowedPortals = {Tile.LeftPortal, Tile.RightPortal, Tile.DownPortal, Tile.UpPortal};
        Tile[] directionPortals = {Tile.RightPortal, Tile.LeftPortal, Tile.UpPortal, Tile.DownPortal};

        Tile[] bannedPortals = {allBannedPortals[direction, 0], allBannedPortals[direction, 1], allBannedPortals[direction, 2]};
        Tile allowedPortal = allowedPortals[direction];

        int exitX = startingX;
        int exitZ = startingZ;
        int final_direction = direction;

        if (tiles[startingX + directions[direction, 0], startingZ + directions[direction, 1]] == allowedPortal)
        {
            foreach (CoordPair pair in portalPairings)
            {
                if (pair.GetHereX() == startingX + directions[direction, 0] && pair.GetHereZ() == startingZ + directions[direction, 1])
                {
                    exitX = pair.GetThereX();
                    exitZ = pair.GetThereZ();
                    final_direction = Array.IndexOf(directionPortals, tiles[exitX, exitZ]);
                }
            }
        }
        int goalX = exitX + directions[final_direction, 0];
        int goalZ = exitZ + directions[final_direction, 1];




        if (tiles[goalX, goalZ] == Tile.Fire || tiles[goalX, goalZ] == Tile.Goal || tiles[goalX, goalZ] == Tile.Floor)
        {
            boxGoalPos[0] = goalX;
            boxGoalPos[1] = goalZ;
        }
        else if (tiles[goalX, goalZ] == Tile.Wall || bannedPortals.Contains(tiles[goalX, goalZ]))
        {
            boxGoalPos[0] = startingX;
            boxGoalPos[1] = startingZ;
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
            half = willBeDestroyed;
            Debug.Log("EVENT RECEIVED: " + willBeDestroyed + "\nPOS: " + transform.position.x + ", " + transform.position.z + "\nHALF: " + half);
        }
    }
}
