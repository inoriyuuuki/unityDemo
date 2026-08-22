using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FMBG.Combat
{
    /// <summary>近战武器：OverlapBoxNonAlloc 检测 + 三阶段攻击 + 去重。</summary>
    public sealed class MeleeWeapon : Weapon
    {
        [SerializeField] private Transform hitboxOrigin;

        private MeleeWeaponConfig meleeConfig;
        private readonly Collider[] hitResults = new Collider[16];
        private readonly HashSet<IDamageable> damagedTargets = new();

        private Coroutine attackCoroutine;

        protected override void ValidateConfig(WeaponConfig config)
        {
            meleeConfig = config as MeleeWeaponConfig;
            if (meleeConfig == null)
            {
                throw new System.ArgumentException($"{name}需要MeleeWeaponConfig。");
            }
        }

        protected override void OnInitialized()
        {
            if (hitboxOrigin == null)
            {
                hitboxOrigin = transform;
            }
        }

        public override bool CanAttack()
        {
            return !IsAttacking && Owner != null && meleeConfig != null;
        }

        public override bool TryAttack(WeaponAttackContext attackContext)
        {
            if (!CanAttack())
            {
                return false;
            }

            attackCoroutine = StartCoroutine(AttackRoutine(attackContext));
            return true;
        }

        public override void CancelAttack()
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }

            damagedTargets.Clear();
            IsAttacking = false;
        }

        private IEnumerator AttackRoutine(WeaponAttackContext attackContext)
        {
            IsAttacking = true;
            damagedTargets.Clear();

            Owner.FaceTowards(attackContext.TargetPosition);

            yield return new WaitForSeconds(meleeConfig.WindupDuration);

            if (meleeConfig.DetectDuringEntireActivePhase)
            {
                // do-while：即使 ActiveDuration 为 0 也至少结算一次命中，避免"挥空"永远无伤害
                float activeTime = 0f;
                do
                {
                    PerformHitDetection();
                    activeTime += Time.deltaTime;
                    yield return null;
                }
                while (activeTime < meleeConfig.ActiveDuration);
            }
            else
            {
                PerformHitDetection();
                yield return new WaitForSeconds(meleeConfig.ActiveDuration);
            }

            yield return new WaitForSeconds(meleeConfig.RecoveryDuration);

            IsAttacking = false;
            attackCoroutine = null;
        }

        private void PerformHitDetection()
        {
            if (hitboxOrigin == null)
            {
                return;
            }

            Vector3 center = hitboxOrigin.TransformPoint(meleeConfig.HitboxOffset);
            Vector3 halfExtents = new(
                meleeConfig.HitboxSize.x * 0.5f,
                meleeConfig.HitboxSize.y * 0.5f,
                meleeConfig.HitboxSize.z * 0.5f);

            int hitCount = Physics.OverlapBoxNonAlloc(
                center,
                halfExtents,
                hitResults,
                hitboxOrigin.rotation,
                meleeConfig.TargetLayers,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                if (!hitResults[i].TryGetComponentInParent(out IDamageable target))
                {
                    continue;
                }

                if (!IsValidTarget(target))
                {
                    continue;
                }

                if (!damagedTargets.Add(target))
                {
                    continue;
                }

                Vector3 hitPoint = hitResults[i].ClosestPoint(center);
                target.TakeDamage(CreateDamageInfo(hitPoint));

                // 配置为"单目标"时，一次攻击只结算第一个命中目标
                if (!meleeConfig.CanHitMultipleTargets)
                {
                    break;
                }
            }
        }

        private bool IsValidTarget(IDamageable target)
        {
            if (target is not Component targetComponent)
            {
                return false;
            }

            if (targetComponent.gameObject == Owner.gameObject)
            {
                return false;
            }

            if (!targetComponent.TryGetComponentInParent(out FactionMember targetFaction))
            {
                return true;
            }

            return Owner.Faction != null && Owner.Faction.CanDamage(targetFaction);
        }
    }
}
