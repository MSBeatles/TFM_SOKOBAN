/**using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PortalPairing : MonoBehaviour
{

    public int x;
    public int y;
    //Type will be 0, 1, 2 or 3 depending on where it looks: 0 = Up, 1 = Left, 2 = Down, 3 = Right
    public int type;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void getX(Transform _transform, object _x)
    {
        if (_transform.position.x - this.transform.position.x <= 0.02f && _transform.position.y - this.transform.position.y <= 0.02f && _transform.position.z - this.transform.position.z <= 0.02f)
        {
            x = _x;
        }
    }


    public void getY(Transform _transform, object _y)
    {
        if (_transform.position.x - this.transform.position.x <= 0.02f && _transform.position.y - this.transform.position.y <= 0.02f && _transform.position.z - this.transform.position.z <= 0.02f)
        {
            y = _y;
        }
    }
}
**/