using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoad : MonoBehaviour
{
    public int iLevelToLoad;
    public string sLevelToLoad;

    public bool useIntegerToLoadLevel = false;

    private void OnTriggerEnter(Collider collision)
    {
        GameObject collisionGameObject = collision.gameObject;
        if(collision.CompareTag("Player"))
        {
            LoadScene();
        }

        void LoadScene()
        {
            if (useIntegerToLoadLevel)
            {
                SceneManager.LoadScene(iLevelToLoad);
            }
            else
            {
                SceneManager.LoadScene(sLevelToLoad);
            }
        }
    }
}
