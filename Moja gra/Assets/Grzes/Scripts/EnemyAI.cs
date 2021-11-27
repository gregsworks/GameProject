using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{

    NavMeshAgent nm;
    public Transform target;

    public float distanceThreshhold = 10f;

    public enum AIState { idle,chasing};

    public AIState aiState = AIState.idle;
 
    // Start is called before the first frame update
    void Start()
    {
        nm = GetComponent<NavMeshAgent>();
        StartCoroutine(Think());
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
                    }
                    nm.SetDestination(transform.position);
                    break;
                case AIState.chasing:
                    dist = Vector3.Distance(target.position, transform.position);
                    if (dist > distanceThreshhold)
                    {
                        aiState = AIState.idle;
                    }
                    nm.SetDestination(target.position);
                    break;
                default:
                    break;
            }
            yield return new WaitForSeconds(1f);
        }
    }
}
