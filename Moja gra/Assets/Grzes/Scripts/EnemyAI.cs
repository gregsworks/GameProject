using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    //navmesh metoda
    NavMeshAgent nm;
    //metoda dziêki której AI obiera swój cel
    Transform target;
    //dystans od celu
    public float distanceThreshhold = 10f;
    //zdrowie enemy
    public float enemyHealth = 100f;

    public enum AIState { idle,chasing,dead};

    public AIState aiState = AIState.idle;

    public int DeadCount;

    Animator myAnimationController;


    // Start is called before the first frame update
    void Start()
    {
        nm = GetComponent<NavMeshAgent>();
        myAnimationController = GetComponent<Animator>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(Think());
    }

    public void DeductHealth(float deductHealth)
    {
        enemyHealth -= deductHealth;

        if (enemyHealth <= 0)
        {
            EnemyDead();
            //DeadCount.instance.EnemyKill();
        }


    }

    void EnemyDead()
    {
        myAnimationController.SetBool("isDead", true);
        StartCoroutine(Dead());
    }

    IEnumerator Think()
    {
        while(true)
        {
            switch (aiState)
            {
                case AIState.idle:
                    float dist = Vector3.Distance(target.position, transform.position);
                    if (dist < distanceThreshhold)
                    {
                        aiState = AIState.chasing;
                        myAnimationController.SetBool("isVisible", true);
                    }
                    nm.SetDestination(transform.position);

                    if (enemyHealth <= 0)
                    {
                        aiState = AIState.dead;
                    }
                    break;
                case AIState.chasing:
                    dist = Vector3.Distance(target.position, transform.position);
                    if (dist > distanceThreshhold)
                    {
                        aiState = AIState.idle;
                        myAnimationController.SetBool("isVisible", false);
                    }
                    nm.SetDestination(target.position);

                    if (enemyHealth <= 0)
                    {
                        aiState = AIState.dead;
                    }
                    break;
                case AIState.dead:
                    nm.SetDestination(transform.position);
                    break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
    IEnumerator Dead()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
        FindObjectOfType<DeadCounter>().EnemyKill();
        Debug.Log("DEAD");
    }
}
