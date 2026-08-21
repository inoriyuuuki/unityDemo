using System.Collections;
using UnityEngine;

namespace FMBG.Combat
{
    /// <summary>远程武器：前摇后生成投射物。</summary>
    public sealed class RangedWeapon : Weapon
    {
        [SerializeField] private Transform muzzlePoint;

        private RangedWeaponConfig rangedConfig;
        private Coroutine attackCoroutine;

        protected override void ValidateConfig(WeaponConfig config)
        {
            rangedConfig = config as RangedWeaponConfig;
            if (rangedConfig == null)
            {
                throw new System.ArgumentException($"{name}需要RangedWeaponConfig。");
            }
        }

        protected override void OnInitialized()
        {
            if (muzzlePoint == null)
            {
                muzzlePoint = transform;
            }
        }

        public override bool CanAttack()
        {
            return !IsAttacking && Owner != null && rangedConfig != null;
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

            IsAttacking = false;
        }

        private IEnumerator AttackRoutine(WeaponAttackContext attackContext)
        {
            IsAttacking = true;

            Owner.FaceTowards(attackContext.TargetPosition);

            yield return new WaitForSeconds(rangedConfig.WindupDuration);

            Fire(attackContext);

            yield return new WaitForSeconds(rangedConfig.ActiveDuration + rangedConfig.RecoveryDuration);

            IsAttacking = false;
            attackCoroutine = null;
        }

        private void Fire(WeaponAttackContext attackContext)
        {
            if (rangedConfig.ProjectilePrefab == null)
            {
                return;
            }

            Vector3 aimDirection = attackContext.TargetPosition - transform.position;
            aimDirection.y = 0f;
            aimDirection.Normalize();

            Vector3 baseDirection = aimDirection.sqrMagnitude > 0.001f
                ? aimDirection
                : transform.forward;

            int count = Mathf.Max(1, rangedConfig.ProjectileCount);
            for (int i = 0; i < count; i++)
            {
                Quaternion spread = Quaternion.Euler(
                    0f,
                    Random.Range(-rangedConfig.SpreadAngle, rangedConfig.SpreadAngle),
                    0f);

                Vector3 fireDirection = spread * baseDirection;

                Projectile projectile = Instantiate(
                    rangedConfig.ProjectilePrefab,
                    muzzlePoint != null ? muzzlePoint.position : transform.position,
                    Quaternion.LookRotation(fireDirection, Vector3.up));

                projectile.Initialize(
                    fireDirection,
                    rangedConfig.ProjectileSpeed,
                    rangedConfig.ProjectileLifetime,
                    rangedConfig.Damage,
                    Owner != null ? Owner.Faction : null,
                    Owner != null ? Owner.gameObject : gameObject,
                    rangedConfig.TargetLayers);
            }
        }
    }
}
