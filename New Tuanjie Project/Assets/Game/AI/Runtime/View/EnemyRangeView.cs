using UnityEngine;

namespace FMBG.AI
{
    /// <summary>
    /// 运行时警戒范围可视化（Game 视图可见）：
    /// 感知半径圆环 + 视野扇形半透明面 + 目标线。
    /// 自动创建所需对象，仅需挂到敌人 GameObject 上。
    /// </summary>
    public sealed class EnemyRangeView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyActor actor;

        [Header("Circle")]
        [SerializeField] private Color circleColor = new(0f, 0.8f, 1f, 0.6f);
        [SerializeField] private float circleWidth = 0.08f;

        [Header("Cone")]
        [SerializeField] private Color coneColor = new(1f, 0.55f, 0f, 0.22f);
        [SerializeField] private int coneSegments = 24;

        [Header("Target Line")]
        [SerializeField] private Color lineVisible = new(0f, 1f, 0f, 0.8f);
        [SerializeField] private Color lineBlocked = new(1f, 0f, 0f, 0.8f);
        [SerializeField] private float lineWidth = 0.06f;

        private LineRenderer circleLine;
        private LineRenderer targetLine;
        private MeshRenderer coneRenderer;
        private float lastDistance;
        private float lastAngle;

        private void Awake()
        {
            if (actor == null) actor = GetComponent<EnemyActor>();
            SetupObjects();
            UpdateVisuals();
        }

        private void LateUpdate()
        {
            if (actor == null || actor.Config == null)
            {
                return;
            }

            float d = actor.Config.Perception.viewDistance;
            float a = actor.Config.Perception.viewAngle;

            if (Mathf.Abs(d - lastDistance) > 0.01f || Mathf.Abs(a - lastAngle) > 0.1f)
            {
                UpdateVisuals();
            }

            UpdateTargetLine();
        }

        private void SetupObjects()
        {
            // 圆环
            var circleGo = new GameObject("RangeCircle");
            circleGo.transform.SetParent(transform, false);
            circleGo.transform.localPosition = Vector3.up * 0.2f;
            circleLine = circleGo.AddComponent<LineRenderer>();
            ConfigureLine(circleLine, circleColor, circleWidth);

            // 目标线
            var lineGo = new GameObject("TargetLine");
            lineGo.transform.SetParent(transform, false);
            lineGo.transform.localPosition = Vector3.up * 0.5f;
            targetLine = lineGo.AddComponent<LineRenderer>();
            ConfigureLine(targetLine, lineVisible, lineWidth);
            targetLine.positionCount = 2;

            // 扇形 Mesh
            var coneGo = new GameObject("VisionCone");
            coneGo.transform.SetParent(transform, false);
            coneGo.transform.localPosition = Vector3.up * 0.2f;
            coneMeshFilter = coneGo.AddComponent<MeshFilter>();
            coneRenderer = coneGo.AddComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = coneColor;
            coneRenderer.sharedMaterial = mat;
            coneRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            coneRenderer.receiveShadows = false;
        }

        private MeshFilter coneMeshFilter;

        private void ConfigureLine(LineRenderer line, Color color, float width)
        {
            line.useWorldSpace = false;
            line.startColor = color;
            line.endColor = color;
            line.startWidth = width;
            line.endWidth = width;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.material.color = Color.white;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
        }

        private void UpdateVisuals()
        {
            if (actor == null || actor.Config == null)
            {
                return;
            }

            float radius = actor.Config.Perception.viewDistance;
            float angle = actor.Config.Perception.viewAngle;
            lastDistance = radius;
            lastAngle = angle;

            BuildCircle(radius);
            BuildCone(radius, angle);
        }

        private void BuildCircle(float radius)
        {
            const int segments = 64;
            circleLine.positionCount = segments + 1;
            for (int i = 0; i <= segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                circleLine.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius));
            }
        }

        private void BuildCone(float radius, float angle)
        {
            var verts = new Vector3[coneSegments + 2];
            var tris = new int[coneSegments * 3];
            var uvs = new Vector2[coneSegments + 2];

            verts[0] = Vector3.zero;
            uvs[0] = new Vector2(0.5f, 0.5f);

            float half = angle * 0.5f * Mathf.Deg2Rad;
            for (int i = 0; i <= coneSegments; i++)
            {
                float a = -half + (angle * Mathf.Deg2Rad * i / coneSegments);
                float x = Mathf.Sin(a) * radius;
                float z = Mathf.Cos(a) * radius;
                verts[i + 1] = new Vector3(x, 0f, z);
                uvs[i + 1] = new Vector2(0.5f + x / (radius * 2f), 0.5f + z / (radius * 2f));

                if (i < coneSegments)
                {
                    int tri = i * 3;
                    tris[tri] = 0;
                    tris[tri + 1] = i + 1;
                    tris[tri + 2] = i + 2;
                }
            }

            var mesh = new Mesh { name = "VisionConeMesh" };
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            coneMeshFilter.sharedMesh = mesh;
        }

        private void UpdateTargetLine()
        {
            if (actor == null || actor.Perception == null || targetLine == null)
            {
                return;
            }

            Transform target = actor.Perception.Target;
            if (target == null)
            {
                targetLine.enabled = false;
                return;
            }

            targetLine.enabled = true;
            targetLine.SetPosition(0, Vector3.zero);
            Vector3 local = transform.InverseTransformPoint(target.position);
            targetLine.SetPosition(1, new Vector3(local.x, 0f, local.z));
            targetLine.startColor = actor.Perception.CanSeeTarget ? lineVisible : lineBlocked;
            targetLine.endColor = targetLine.startColor;
        }
    }
}
