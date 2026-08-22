using UnityEngine;

namespace FMBG.AI
{
    /// <summary>状态转换数据：from 状态 →（条件）→ to 状态。</summary>
    [System.Serializable]
    public sealed class EnemyTransitionData
    {
        [SerializeField] private string id;
        [SerializeField] private string fromStateId;
        [SerializeField] private string toStateId;
        [SerializeField] private Vector2 conditionPosition;
        [SerializeField] private TransitionCondition condition = new();

        public string Id => id;
        public string FromStateId { get => fromStateId; set => fromStateId = value; }
        public string ToStateId { get => toStateId; set => toStateId = value; }
        public Vector2 ConditionPosition { get => conditionPosition; set => conditionPosition = value; }
        public TransitionCondition Condition => condition;

        public EnemyTransitionData()
        {
        }

        public EnemyTransitionData(
            string id,
            string fromStateId,
            string toStateId,
            TransitionCondition condition,
            Vector2 conditionPosition)
        {
            this.id = id;
            this.fromStateId = fromStateId;
            this.toStateId = toStateId;
            this.condition = condition ?? new TransitionCondition();
            this.conditionPosition = conditionPosition;
        }
    }
}
