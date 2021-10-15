using UnityEngine;
using UnityEngine.AI;

/*
 * Move with NavMesh Controller
 */
public class PlayerMoveController : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent navMeshAgent;

    void Awake()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int mask = 1 << 8;
            if (Physics.Raycast(ray, out hit, mask))
            {
                navMeshAgent.SetDestination(hit.point);
            }
        }
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        float acceleration = 20f;
        float deceleration = 60f;
        float closeEnoughMeters = 4f;

        navMeshAgent.acceleration = (navMeshAgent.remainingDistance < closeEnoughMeters) ? deceleration : acceleration;

        float speed = Vector3.Project(navMeshAgent.desiredVelocity, transform.forward).magnitude;
        animator.SetFloat("speed", speed);
    }
}
