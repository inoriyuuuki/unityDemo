using FMBG.Visual;
using UnityEditor;
using UnityEngine;

namespace FMBG.EditorTools
{
    /// <summary>一键为角色构建程序化视觉（身体/头/四肢/朝向箭头），并移除旧 Capsule。</summary>
    public static class CharacterVisualBuilder
    {
        [MenuItem("Game/Tools/Build Character Visual")]
        public static void BuildSelected()
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                BuildCharacter(go);
            }
        }

        [MenuItem("Game/Tools/Build All Character Visuals")]
        public static void BuildAll()
        {
            BuildCharacter(GameObject.Find("Player"));
            BuildCharacter(GameObject.Find("Enemy_MeleeGrunt"));
        }

        public static CharacterVisual BuildCharacter(GameObject root)
        {
            if (root == null)
            {
                return null;
            }

            // 移除旧的 Capsule 模型（保留组件）
            var oldRenderer = root.GetComponent<MeshRenderer>();
            var oldFilter = root.GetComponent<MeshFilter>();
            if (oldRenderer != null) Object.DestroyImmediate(oldRenderer);
            if (oldFilter != null) Object.DestroyImmediate(oldFilter);
            var oldCollider = root.GetComponent<CapsuleCollider>();
            if (oldCollider != null) Object.DestroyImmediate(oldCollider);

            // 角色视觉根（实际渲染的对象，避免与逻辑根组件冲突）
            var visRoot = CreateChild(root.transform, "VisualRoot", Vector3.zero);
            var visual = visRoot.AddComponent<CharacterVisual>();

            // 身体（胶囊）
            var body = CreatePart(visRoot.transform, "Body", PrimitiveType.Capsule, new Vector3(0f, 0.9f, 0f), new Vector3(0.5f, 0.6f, 0.35f));
            // 头（球）
            var head = CreatePart(visRoot.transform, "Head", PrimitiveType.Sphere, new Vector3(0f, 1.75f, 0f), Vector3.one * 0.45f);
            // 手臂（胶囊）
            var armL = CreatePart(visRoot.transform, "Arm_L", PrimitiveType.Capsule, new Vector3(-0.5f, 1.35f, 0f), new Vector3(0.16f, 0.35f, 0.16f));
            var armR = CreatePart(visRoot.transform, "Arm_R", PrimitiveType.Capsule, new Vector3(0.5f, 1.35f, 0f), new Vector3(0.16f, 0.35f, 0.16f));
            // 腿（胶囊）
            var legL = CreatePart(visRoot.transform, "Leg_L", PrimitiveType.Capsule, new Vector3(-0.2f, 0.45f, 0f), new Vector3(0.2f, 0.45f, 0.2f));
            var legR = CreatePart(visRoot.transform, "Leg_R", PrimitiveType.Capsule, new Vector3(0.2f, 0.45f, 0f), new Vector3(0.2f, 0.45f, 0.2f));

            // 武器挂点（右手前方）
            var weaponPivot = CreateChild(armR.transform, "WeaponPivot", new Vector3(0f, -0.4f, 0.15f));

            // 朝向指示箭头（脚下前方）
            var arrow = CreateArrow(visRoot.transform);

            // 配色
            var bodyRenderer = body.GetComponent<MeshRenderer>();
            bodyRenderer.sharedMaterial = MakeMat(visual.bodyColor);
            head.GetComponent<MeshRenderer>().sharedMaterial = MakeMat(visual.headColor);
            armL.GetComponent<MeshRenderer>().sharedMaterial = MakeMat(visual.limbColor);
            armR.GetComponent<MeshRenderer>().sharedMaterial = MakeMat(visual.limbColor);
            legL.GetComponent<MeshRenderer>().sharedMaterial = MakeMat(visual.limbColor);
            legR.GetComponent<MeshRenderer>().sharedMaterial = MakeMat(visual.limbColor);

            visual.AssignParts(body.transform, head.transform, armL.transform, armR.transform, legL.transform, legR.transform, weaponPivot.transform, arrow);

            // 把已有 WeaponHolder 挂到右手武器挂点下（若存在）
            var weaponHolder = root.transform.Find("WeaponHolder");
            if (weaponHolder != null && root.GetComponent<FMBG.Combat.CharacterCombat>() != null)
            {
                weaponHolder.SetParent(weaponPivot.transform, false);
                weaponHolder.localPosition = Vector3.zero;
                weaponHolder.localRotation = Quaternion.identity;
            }

            EditorUtility.SetDirty(root);
            return visual;
        }

        private static GameObject CreateChild(Transform parent, string name, Vector3 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            return go;
        }

        private static GameObject CreatePart(Transform parent, string name, PrimitiveType type, Vector3 localPos, Vector3 localScale)
        {
            var prim = GameObject.CreatePrimitive(type);
            prim.name = name;
            prim.transform.SetParent(parent, false);
            prim.transform.localPosition = localPos;
            prim.transform.localScale = localScale;

            // 移除碰撞体（视觉部件不需要）
            var col = prim.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            return prim;
        }

        private static Transform CreateArrow(Transform parent)
        {
            // 箭头：三角锥 + 杆
            var arrowRoot = new GameObject("AimArrow");
            arrowRoot.transform.SetParent(parent, false);
            arrowRoot.transform.localPosition = new Vector3(0f, 0.02f, 0.45f);

            var cone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cone.name = "ArrowHead";
            cone.transform.SetParent(arrowRoot.transform, false);
            cone.transform.localPosition = new Vector3(0f, 0f, 0.18f);
            cone.transform.localScale = new Vector3(0.25f, 0.05f, 0.4f);
            cone.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            cone.GetComponent<MeshRenderer>().sharedMaterial = MakeMat(new Color(1f, 0.2f, 0.2f, 0.9f));
            Object.DestroyImmediate(cone.GetComponent<Collider>());

            var stem = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stem.name = "ArrowStem";
            stem.transform.SetParent(arrowRoot.transform, false);
            stem.transform.localPosition = new Vector3(0f, 0f, -0.15f);
            stem.transform.localScale = new Vector3(0.08f, 0.02f, 0.3f);
            stem.GetComponent<MeshRenderer>().sharedMaterial = MakeMat(new Color(1f, 0.2f, 0.2f, 0.6f));
            Object.DestroyImmediate(stem.GetComponent<Collider>());

            return arrowRoot.transform;
        }

        private static Material MakeMat(Color color)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            return mat;
        }
    }
}
