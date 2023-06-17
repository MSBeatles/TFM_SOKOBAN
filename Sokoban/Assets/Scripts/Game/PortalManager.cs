using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{

    public GameObject player;
    public GameObject box;
    private bool playerExists;

    [Header ("GAME EVENTS")]
    public GameEvent onEnterPortal;
    public GameEvent onEnterPortal2;
    public GameEvent onEnterPortal3;
    public GameEvent onEnterPortal5;

    // Start is called before the first frame update
    void Start()
    {
        playerExists = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void CreateNewPlayer(Component sender, object _spawnPoint)
    {
        int[] spawnPoint = (int[]) _spawnPoint;
        GameObject myPlayer = Instantiate(player, new Vector3(spawnPoint[0], 0.8f, spawnPoint[1]), Quaternion.identity);
        foreach (Collider col in myPlayer.GetComponents<Collider>())
        {
            col.enabled = false;
        }
        playerExists = true;
    }


    public void CreateNewBox(Component sender, object _spawnPoint)
    {
        if (sender is PlayerMovement)
        {
            int[] spawnPoint = (int[])_spawnPoint;
            GameObject myBox = Instantiate(box, new Vector3(spawnPoint[0], 0.8f, spawnPoint[1]), Quaternion.identity);
            foreach (Collider col in myBox.GetComponents<Collider>())
            {
                col.enabled = false;
            }
            StartCoroutine(RestoreColliders(myBox));
        }
    }


    public void ReceiveTiles(Component sender, object _tiles)
    {
        if (playerExists && sender is PlayerMovement)
        {
            onEnterPortal.Raise(this, _tiles);
        }
    }

    public void ReceiveGoalPos(Component sender, object _portalGoalPos)
    {
        if (playerExists && sender is PlayerMovement)
        {
            onEnterPortal2.Raise(this, _portalGoalPos);
        }
    }

    public void ReceivePortalPairings(Component sender, object _portalPairings)
    {
        if (playerExists && sender is PlayerMovement)
        {
            onEnterPortal3.Raise(this, _portalPairings);
        }
    }

    public void ReceiveDestroyPlayer(Component sender, object _destroy)
    {
        if (playerExists && sender is PlayerMovement)
        {
            onEnterPortal5.Raise(this, _destroy);
        }
    }






    IEnumerator RestoreColliders(GameObject myBox)
    {
        yield return new WaitForSeconds(0.6f);
        foreach (Collider col in myBox.GetComponents<Collider>())
        {
            col.enabled = true;
        }
        playerExists = false;
    }
}
