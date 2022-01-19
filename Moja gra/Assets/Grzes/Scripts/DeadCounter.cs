using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (DeadCount == 10)
        {
            myAnimationController.SetBool("isGateOpened", true);
        }

        if (DeadCount == 20)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 2);
        }
    }
}
