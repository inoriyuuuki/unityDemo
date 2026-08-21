using UnityEngine;
using XNode;

namespace FMBG.AI
{
    [CreateNodeMenu("Enemy AI/States/Dead")]
    public sealed class DeadStateNode : EnemyStateNode
    {
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

        public override EnemyStateNode EvaluateTransition(EnemyContext context)
        {
            return null;
        }
    }
}
