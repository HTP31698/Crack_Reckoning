using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    [SerializeField]
    public Transform target;
    private Animator animator;

    private NavMeshAgent agent;
    private float posX;
    private float posZ;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        posX = gameObject.transform.position.x;
        posZ = gameObject.transform.position.z;
    }

    private void Update()
    {
        //Target Move
        Vector3 movepos = new Vector3(posX, target.position.y + agent.stoppingDistance, posZ);
        agent.SetDestination(movepos);

        //Target Attack
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    animator.SetTrigger("Attack");
                }
            }
        }
    }
}