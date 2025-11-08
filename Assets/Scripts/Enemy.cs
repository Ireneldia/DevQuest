using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [Header("Preset Fields")] 
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject splashFx;
    [SerializeField] private Transform player; // 플레이어 위치
    
    [Header("Settings")]
    [SerializeField] private float attackRange = 2f; // 공격 범위
    [SerializeField] private float chaseRange = 10f; // 추적 범위 (새로 추가)
    [SerializeField] private float patrolRange = 5f; // 순찰 범위
    
    private NavMeshAgent navMeshAgent; // NavMeshAgent 추가
    private Vector3 patrolTarget;
    
    public enum State 
    {
        None,
        Idle,
        Chase, // 추적 상태 추가
        Attack
    }
    
    [Header("Debug")]
    public State state = State.None;
    public State nextState = State.None;

    private bool attackDone;

    private void Start()
    { 
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        state = State.None;
        nextState = State.Idle;
        SetNewPatrolTarget();
    }

    private void Update()
    {
        //1. 스테이트 전환 상황 판단
        if (nextState == State.None) 
        {
            switch (state) 
            {
                case State.Idle:
                    // 플레이어가 추적 범위 안에 있으면 Chase로 전환
                    if (player != null && Vector3.Distance(transform.position, player.position) < chaseRange)
                    {
                        nextState = State.Chase;
                    }
                    // 패트롤 목표에 도달하면 새로운 목표 설정
                    else if (navMeshAgent.remainingDistance < 0.5f)
                    {
                        SetNewPatrolTarget();
                    }
                    break;
                    
                case State.Chase:
                    // 플레이어가 추적 범위를 벗어나면 Idle로
                    if (player == null || Vector3.Distance(transform.position, player.position) > chaseRange + 2f)
                    {
                        nextState = State.Idle;
                        SetNewPatrolTarget();
                    }
                    // 공격 범위 안에 있으면 Attack
                    else if (Vector3.Distance(transform.position, player.position) < attackRange)
                    {
                        nextState = State.Attack;
                    }
                    break;
                    
                case State.Attack:
                    if (attackDone)
                    {
                        nextState = State.Chase; // 공격 후 다시 추적
                        attackDone = false;
                    }
                    break;
            }
        }
        
        //2. 스테이트 초기화
        if (nextState != State.None) 
        {
            state = nextState;
            nextState = State.None;
            switch (state) 
            {
                case State.Idle:
                    animator.SetBool("isMoving", false);
                    break;
                case State.Chase:
                    animator.SetBool("isMoving", true);
                    break;
                case State.Attack:
                    animator.SetBool("isMoving", false);
                    Attack();
                    break;
            }
        }
        
        //3. 글로벌 & 스테이트 업데이트
        if (state == State.Idle)
        {
            navMeshAgent.SetDestination(patrolTarget);
        }
        else if (state == State.Chase && player != null)
        {
            navMeshAgent.SetDestination(player.position);
        }
    }
    
    private void SetNewPatrolTarget()
    {
        // 랜덤한 위치에서 패트롤 목표 설정
        Vector3 randomDirection = Random.insideUnitSphere * patrolRange;
        randomDirection += transform.position;
        
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRange, NavMesh.AllAreas))
        {
            patrolTarget = hit.position;
        }
    }
    
    private void Attack()
    {
        animator.SetTrigger("attack");
    }

    public void InstantiateFx()
    {
        Instantiate(splashFx, transform.position, Quaternion.identity);
    }
    
    public void WhenAnimationDone()
    {
        attackDone = true;
    }

    private void OnDrawGizmosSelected()
    {
        // 공격 범위
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawSphere(transform.position, attackRange);
        
        // 추적 범위
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, chaseRange);
    }
}