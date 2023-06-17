using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    private int x;
    private int z;

    // Start is called before the first frame update
    void Start()
    {
        int x = 0;
        int z = 0;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3 (x/2.0f - 0.5f, (x+z)/2.0f + 2.0f, z/2.0f - 0.5f);
    }


    public void SetX(Component sender, object _maxX)
    {
        x = (int)_maxX;
    }

    public void SetZ(Component sender, object _maxZ)
    {
        z = (int)_maxZ;
    }
}
