using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bronka : MonoBehaviour
{
    RaycastHit hit;

    Transform shootPoint;

    [SerializeField]
    float weaponRange;

    void Shoot()
    {
        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, weaponRange))
        {
            if(hit.transform.tag == "Dummie")
            {
                Debug.Log("Hit Enemy");
            }
            else
            {
                Debug.Log("Hit Something Else");
            }
        }
    }

}
