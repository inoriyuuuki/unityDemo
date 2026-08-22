using System;
using FMBG.Combat;
using UnityEngine;

namespace FMBG.AI
{
    /// <summary>敌人感知：距离 + 视野角 + 障碍检测 + 警觉值。</summary>
    public sealed class EnemyPerception : MonoBehaviour
    {
        [SerializeField] private Transform self;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private LayerMask obstacleLayers;

        private EnemyPerceptionSettings settings;
        private bool initialized;

        private float scanTimer;
        private float alertValue;
        private float lastSeenTime;
        private bool canSeeTarget;

        private readonly Collider[] overlapResults = new Collider[32];

        public Transform Target { get; private set; }
        public bool CanSeeTarget => canSeeTarget;
        public float AlertValue => alertValue;
        public Vector3 LastKnownPosition { get; private set; }
        public bool HasLastKnownPosition { get; private set; }

        public event Action<Transform> TargetAcquired;
        public event Action TargetLost;

        public void Initialize(EnemyPerceptionSettings perceptionSettings)
        {
            settings = perceptionSettings;
            targetLayers = perceptionSettings.targetLayers;
            obstacleLayers = perceptionSettings.obstacleLayers;
            initialized = true;
            scanTimer = 0f;
        }

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            scanTimer -= Time.deltaTime;
            if (scanTimer > 0f)
            {
                return;
            }

            scanTimer = settings.scanInterval;
            Scan();
        }

        private void Scan()
        {
            if (self == null)
            {
                self = transform;
            }

            Transform found = FindVisibleTarget();
            bool nowVisible = found != null;

            // 当前目标已死亡：立即丢失目标
            if (canSeeTarget && Target != null &&
                Target.TryGetComponentInParent(out IDamageable tracked) &&
                !tracked.IsAlive)
            {
                canSeeTarget = false;
                Target = null;
                TargetLost?.Invoke();
                return;
            }

            if (nowVisible)
            {
                alertValue += settings.alertDuration * settings.scanInterval;
                lastSeenTime = Time.time;
                LastKnownPosition = found.position;
                HasLastKnownPosition = true;
            }
            else
            {
                alertValue = Mathf.Max(0f, alertValue - settings.forgetDuration * settings.scanInterval);
            }

            alertValue = Mathf.Clamp01(alertValue);

            bool thresholdReached = alertValue >= 1f;

            if (!canSeeTarget && thresholdReached)
            {
                canSeeTarget = true;
                Target = found;
                TargetAcquired?.Invoke(found);
            }
            else if (canSeeTarget)
            {
                // 保持目标：只要目标仍可见（或刚丢失但警觉未完全消退）
                if (nowVisible)
                {
                    Target = found;
                    LastKnownPosition = found.position;
                }

                // 警戒值归零：立即丢失目标（不再继续追击）
                if (alertValue <= 0f)
                {
                    canSeeTarget = false;
                    Target = null;
                    TargetLost?.Invoke();
                }
                else if (Time.time - lastSeenTime > settings.forgetDuration)
                {
                    canSeeTarget = false;
                    Target = null;
                    TargetLost?.Invoke();
                }
            }
            else if (alertValue <= 0f)
            {
                Target = null;
            }
        }

        private Transform FindVisibleTarget()
        {
            int hitCount = Physics.OverlapSphereNonAlloc(
                self.position, settings.viewDistance, overlapResults, targetLayers);
            Transform best = null;
            float bestScore = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                Transform candidate = overlapResults[i].transform;

                // 跳过已死亡目标（玩家死亡后不再被敌人锁定）
                if (candidate.TryGetComponentInParent(out IDamageable damageable) &&
                    !damageable.IsAlive)
                {
                    continue;
                }

                Vector3 toTarget = candidate.position - self.position;
                float distance = toTarget.magnitude;
                if (distance > settings.viewDistance)
                {
                    continue;
                }

                Vector3 dir = toTarget.normalized;
                float angle = Vector3.Angle(self.forward, dir);
                if (angle > settings.viewAngle * 0.5f)
                {
                    continue;
                }

                if (HasObstacleBetween(self.position + Vector3.up * 0.5f, candidate.position + Vector3.up * 0.5f, distance))
                {
                    continue;
                }

                // 选择距离最近的目标
                if (distance < bestScore)
                {
                    bestScore = distance;
                    best = candidate;
                }
            }

            return best;
        }

        private bool HasObstacleBetween(Vector3 from, Vector3 to, float distance)
        {
            if (obstacleLayers.value == 0)
            {
                return false;
            }

            return Physics.Raycast(
                from,
                (to - from).normalized,
                distance,
                obstacleLayers);
        }
    }
}
