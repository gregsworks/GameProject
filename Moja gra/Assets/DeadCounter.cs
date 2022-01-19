using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadCounter : MonoBehaviour
{
    public int DeadCount = 0;

    public Animator myAnimationController;

    public void EnemyKill()
    {
        DeadCount += 1;
    }

    void Update()
    {
        if (DeadCount == 2)
        {
            myAnimationController.SetBool("isGateOpened", true);
        }

    }
}
