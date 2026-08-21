using UnityEngine;
using UnityEngine.AI;

namespace FMBG.AI
{
    /// <summary>敌人移动：NavMeshAgent 封装。</summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class EnemyMotor : MonoBehaviour
    {
        private NavMeshAgent agent;
        private EnemyMovementSettings settings;
        private bool initialized;

        public NavMeshAgent Agent => agent;
        public bool IsMoving => agent != null && agent.hasPath && agent.remainingDistance > agent.stoppingDistance;

        public void Initialize(EnemyMovementSettings movementSettings)
        {
            settings = movementSettings;
            agent = GetComponent<NavMeshAgent>();
            initialized = true;

            if (agent != null)
            {
                agent.speed = settings.patrolSpeed;
                agent.acceleration = Mathf.Max(1f, settings.acceleration);
                agent.angularSpeed = settings.angularSpeed > 0f ? settings.angularSpeed : 360f;
                agent.stoppingDistance = Mathf.Max(0.1f, settings.stoppingDistanceTolerance);
            }
        }

        public void MoveTo(Vector3 destination)
        {
            if (!initialized || agent == null || !agent.isOnNavMesh)
            {
                return;
            }

            agent.isStopped = false;
            agent.SetDestination(destination);
        }

        public void SetPatrolSpeed()
        {
            if (agent != null)
            {
                agent.speed = settings.patrolSpeed;
            }
        }

        public void SetChaseSpeed()
        {
            if (agent != null)
            {
                agent.speed = settings.chaseSpeed;
            }
        }

        public void SetStoppingDistance(float distance)
        {
            if (agent != null)
            {
                agent.stoppingDistance = Mathf.Max(0.1f, distance);
            }
        }

        public void FaceTowards(Vector3 targetPosition, float deltaTime)
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            float angularSpeed = settings.angularSpeed > 0f ? settings.angularSpeed : 360f;
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                angularSpeed * deltaTime);
        }

        public void Stop()
        {
            if (agent != null)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }

        public void Resume()
        {
            if (agent != null)
            {
                agent.isStopped = false;
            }
        }

        public bool ReachedDestination()
        {
            return agent != null &&
                   (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + 0.05f);
        }
    }
}
