using UnityEditor;
using UnityEngine;

namespace FMBG.EditorTools
{
    /// <summary>给武器预制体添加可见几何体（剑/枪），不依赖外部模型。通过实例化-修改-保存避免直接改 prefab 资源。</summary>
    public static class WeaponVisualBuilder
    {
        [MenuItem("Game/Tools/Build Weapon Visuals")]
        public static void BuildAllWeapons()
        {
            BuildSword();
            BuildPistol();
        }

        public static void BuildSword()
        {
            string path = "Assets/Game/Prefabs/Weapon_Sword.prefab";
            RebuildWeapon(path, BuildSwordModel);
        }

        public static void BuildPistol()
        {
            string path = "Assets/Game/Prefabs/Weapon_Pistol.prefab";
            RebuildWeapon(path, BuildPistolModel);
        }

        private static void RebuildWeapon(string path, System.Action<Transform> buildModel)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError("找不到 " + path);
                return;
            }

            // 实例化到临时场景（保留 Weapon 组件与引用）
            var instance = (GameObject)Object.Instantiate(prefab);
            instance.name = "Weapon_Temp";

            // 清空旧 Model
            var oldModel = instance.transform.Find("Model");
            if (oldModel != null) Object.DestroyImmediate(oldModel.gameObject);
            var mr = instance.GetComponent<MeshRenderer>();
            if (mr != null) Object.DestroyImmediate(mr);
            var mf = instance.GetComponent<MeshFilter>();
            if (mf != null) Object.DestroyImmediate(mf);
            var col = instance.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            var model = new GameObject("Model");
            model.transform.SetParent(instance.transform, false);
            buildModel(model.transform);

            // 保存回 prefab
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            AssetDatabase.SaveAssets();
            Debug.Log("[WeaponVisualBuilder] 可视化完成: " + path);
        }

        private static void BuildSwordModel(Transform root)
        {
            // 剑刃
            var blade = Cube(root, "Blade", new Vector3(0f, 0f, 0.7f), new Vector3(0.08f, 0.12f, 1.4f), new Color(0.8f, 0.85f, 0.95f), 0.6f);
            // 护手
            Cube(root, "Guard", new Vector3(0f, 0f, 0.08f), new Vector3(0.35f, 0.08f, 0.12f), new Color(0.7f, 0.5f, 0.2f), 0.3f);
            // 剑柄（圆柱）
            var handle = Cylinder(root, "Handle", new Vector3(0f, 0f, -0.3f), new Vector3(0.05f, 0.35f, 0.05f), new Color(0.4f, 0.3f, 0.2f), 0.2f);
            handle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private static void BuildPistolModel(Transform root)
        {
            // 枪身
            Cube(root, "Body", new Vector3(0f, 0f, 0.15f), new Vector3(0.14f, 0.16f, 0.6f), new Color(0.25f, 0.25f, 0.3f), 0.5f);
            // 枪管
            var barrel = Cylinder(root, "Barrel", new Vector3(0f, 0.06f, 0.5f), new Vector3(0.04f, 0.3f, 0.04f), new Color(0.4f, 0.4f, 0.45f), 0.5f);
            barrel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            // 握把
            var grip = Cube(root, "Grip", new Vector3(0f, -0.12f, -0.15f), new Vector3(0.12f, 0.22f, 0.2f), new Color(0.35f, 0.25f, 0.15f), 0.2f);
            grip.transform.localRotation = Quaternion.Euler(-20f, 0f, 0f);
        }

        private static Transform Cube(Transform parent, string name, Vector3 pos, Vector3 scale, Color color, float metallic)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = MakeMetal(color, metallic);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            return go.transform;
        }

        private static Transform Cylinder(Transform parent, string name, Vector3 pos, Vector3 scale, Color color, float metallic)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = MakeMetal(color, metallic);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            return go.transform;
        }

        private static Material MakeMetal(Color color, float metallic)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Glossiness", 0.4f);
            return mat;
        }
    }
}
