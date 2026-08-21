using UnityEngine;

namespace FMBG.SlateClips
{
    /// <summary>创建技能特效（瞬时事件）。</summary>
    public sealed class SpawnEffectClip : SkillClipBase
    {
        public GameObject effectPrefab;
        public Vector3 offset = Vector3.zero;
        public float destroyDelay = 2f;

        [SerializeField, HideInInspector] private float _length = 0.01f;
        [SerializeField, HideInInspector] private float _blendIn = 0f;
        [SerializeField, HideInInspector] private float _blendOut = 0f;

        public override float length
        {
            get { return _length; }
            set { _length = Mathf.Max(0f, value); }
        }

        public override float blendIn
        {
            get { return _blendIn; }
            set { _blendIn = value; }
        }

        public override float blendOut
        {
            get { return _blendOut; }
            set { _blendOut = value; }
        }

        public override string info => effectPrefab != null ? "Spawn Effect: " + effectPrefab.name : "Spawn Effect";

        protected override void OnEnter()
        {
            if (!CanResolve || effectPrefab == null || actor == null)
            {
                return;
            }

            GameObject effect = Instantiate(
                effectPrefab,
                actor.transform.position + actor.transform.TransformVector(offset),
                actor.transform.rotation);

            if (destroyDelay > 0f)
            {
                Destroy(effect, destroyDelay);
            }
        }
    }
}
