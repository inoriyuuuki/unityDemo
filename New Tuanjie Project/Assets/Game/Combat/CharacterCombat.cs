using UnityEngine;

namespace FMBG.Combat
{
    /// <summary>玩家与敌人的统一战斗入口：武器装备、攻击请求、范围查询。</summary>
    public sealed class CharacterCombat : MonoBehaviour
    {
        [SerializeField] private Transform weaponHolder;
        [SerializeField] private WeaponConfig startingWeapon;
        [SerializeField] private FactionMember faction;

        public Weapon CurrentWeapon { get; private set; }
        public WeaponConfig CurrentWeaponConfig =>
            CurrentWeapon != null ? CurrentWeapon.Config : null;
        public FactionMember Faction => faction;

        public bool IsAttacking =>
            CurrentWeapon != null && CurrentWeapon.IsAttacking;

        private void Awake()
        {
            if (startingWeapon != null)
            {
                Equip(startingWeapon);
            }
        }

        public void Equip(WeaponConfig config)
        {
            if (config == null || config.WeaponPrefab == null)
            {
                Debug.LogError("武器配置或武器Prefab为空。", this);
                return;
            }

            if (CurrentWeapon != null)
            {
                CurrentWeapon.CancelAttack();
                Destroy(CurrentWeapon.gameObject);
            }

            Transform parent = weaponHolder != null ? weaponHolder : transform;
            CurrentWeapon = Instantiate(config.WeaponPrefab, parent);
            CurrentWeapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            CurrentWeapon.Initialize(this, config);
        }

        public bool TryAttack(Vector3 targetPosition, Transform target = null)
        {
            if (CurrentWeapon == null)
            {
                return false;
            }

            return CurrentWeapon.TryAttack(
                new WeaponAttackContext(targetPosition, target));
        }

        public bool IsTargetInAttackRange(Transform target, float tolerance = 0f)
        {
            if (CurrentWeapon == null || target == null)
            {
                return false;
            }

            float distance = Vector3.Distance(transform.position, target.position);
            return distance >= CurrentWeapon.MinAttackRange &&
                   distance <= CurrentWeapon.MaxAttackRange + tolerance;
        }

        public void CancelAttack()
        {
            CurrentWeapon?.CancelAttack();
        }

        public void FaceTowards(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }
    }
}
