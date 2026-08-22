using UnityEngine;

namespace FMBG.AI
{
    /// <summary>死亡：停止移动/攻击/感知，延迟销毁。</summary>
    public sealed class DeadState : EnemyState
    {
        public DeadState(EnemyStateNodeData data) : base(data)
        {
        }

        public override void Enter(EnemyContext context)
        {
            context.Motor.Stop();
            context.Combat.CancelAttack();

            // 禁用感知
            if (context.Perception != null)
            {
                context.Perception.enabled = false;
            }

            float delay = context.Config != null ? context.Config.Vitals.deathDestroyDelay : 0f;
            if (delay > 0f)
            {
                Object.Destroy(context.Actor.gameObject, delay);
            }
        }

        public override EnemyState EvaluateTransitions(EnemyContext context)
        {
            return null;
        }
    }
}
