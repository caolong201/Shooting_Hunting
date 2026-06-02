using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using DG.Tweening;
using IE.RSB;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class DogNavAgentController : MonoBehaviour
{
    public enum Stage
    {
        Idle = 0,
        RunToAttack,
        Attack,
        BackHome
    }
    private Stage stage = Stage.Idle;
    
    private Transform target;

    [Header("NavMesh")]
    public float repathRate = 0.1f; 
    public float stopDistance = 1.2f; 

    NavMeshAgent agent;
    Animator animator;
    Coroutine repathCo;
    
    private Vector3 startPosition;
    private Action OnAttackFinished;

    [SerializeField] private Transform vfxBlood;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator  = GetComponent<Animator>();

        agent.stoppingDistance   = stopDistance;
        agent.autoBraking        = true;
        agent.updateRotation     = true;     
        agent.autoTraverseOffMeshLink = true;
    }

    void OnDisable()
    {
        if (repathCo != null) StopCoroutine(repathCo);
    }

    public void Attack(Transform target, System.Action onFinish)
    {
        this.target = target;
        this.OnAttackFinished = onFinish;
        
        startPosition = transform.position;
        if (target != null) repathCo = StartCoroutine(RepathLoop());
        
        animator?.SetInteger("AnimationNo", 2);
        stage = Stage.RunToAttack;
    }

    IEnumerator RepathLoop()
    {
        var wait = new WaitForSeconds(repathRate);
        while (enabled)
        {
            if (target != null)
            {
                agent.isStopped = false;
                agent.SetDestination(target.position);
            }
            yield return wait;
        }
    }

    void Update()
    {
        if(stage == Stage.Idle) return;
      
        bool arrived = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
        
        if (stage == Stage.BackHome)
        {
            if (arrived)
            {
                agent.isStopped = true;
                stage = Stage.Idle;
                Destroy(gameObject);
            }
        }
        else
        {
            if (target == null) return;
            
            Vector3 look = new Vector3(target.position.x, transform.position.y, target.position.z);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(look - transform.position),
                Time.deltaTime * 10f);
            
            if (arrived && stage != Stage.Attack)
            {
                Attack();
            }
        }
    }
    
    private void Attack()
    {
        stage = Stage.Attack;
        animator?.SetInteger("AnimationNo", 3);
        DOVirtual.DelayedCall(0.8f, () =>
        {
           // agent.isStopped = true;
            if (repathCo != null) StopCoroutine(repathCo);
            target.GetComponent<Enemy>().HuntingDogAttacked(vfxBlood);
            animator?.SetInteger("AnimationNo", -1);

            DOVirtual.DelayedCall(.1f, () =>
            {
                stage = Stage.BackHome;
                animator?.SetInteger("AnimationNo", 2);
                
                agent.ResetPath();
                agent.isStopped = false;
                agent.SetDestination(startPosition);
                
                DOVirtual.DelayedCall(1f, () =>
                {
                    OnAttackFinished?.Invoke();
                });
            });
        });
    }

}
