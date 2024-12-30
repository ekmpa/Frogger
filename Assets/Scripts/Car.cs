using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public enum DIRECTION { L, R }

    public DIRECTION direction;
    public int lane; 
    public float speed; 

    // Start is called before the first frame update
    void Start()
    {
        if(lane%2 == 0)
        {
            transform.localRotation = Quaternion.Euler(0, 180, 0);
        }

    }

    public void UpdateCar()
    {
        transform.position += speed * Time.deltaTime * transform.forward;
        if(transform.position.z <= Manager.extremePts.x)
        {
            transform.position += new Vector3(0,0,Manager.laneDimensions.y); 
        } else if (transform.position.z >= Manager.extremePts.y)
        {
            transform.position -= new Vector3(0, 0, Manager.laneDimensions.y);
        }
    }
}
