using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProjectKCPC
{
    public class _RampGenerator : MonoBehaviour
    {
        [HideInInspector] public float angleFrom = 10f;
        [HideInInspector] public float angleTo = 45f;
        [HideInInspector] public float angleStep = 5f;
        [HideInInspector] public float depth = 10f;
        
        [HideInInspector] public float rampWidth = 2f;
        [HideInInspector] public float spacing = 1f;

        public void GenerateRamps()
        {
            // Clear old ramps
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            if (angleStep <= 0)
            {
                Debug.LogWarning("Step must be greater than 0");
                return;
            }

            int count = 0;
            for (float angle = angleFrom; angle <= angleTo; angle += angleStep)
            {
                CreateRamp(angle, count);
                count++;
            }
        }

        private void CreateRamp(float angle, int index)
        {
            GameObject rampObj = new GameObject($"Ramp_{angle}deg");
            rampObj.transform.SetParent(transform);
            
            // Calculate position: side by side along the X axis
            float xPos = index * (rampWidth + spacing);
            rampObj.transform.localPosition = new Vector3(xPos, 0, 0);
            rampObj.transform.localRotation = Quaternion.identity;

            MeshFilter mf = rampObj.AddComponent<MeshFilter>();
            MeshRenderer mr = rampObj.AddComponent<MeshRenderer>();
            
            // Assign a default material
            GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Material defaultMat = tempCube.GetComponent<MeshRenderer>().sharedMaterial;
            DestroyImmediate(tempCube);
            mr.sharedMaterial = defaultMat;

            MeshCollider mc = rampObj.AddComponent<MeshCollider>();

            float height = depth * Mathf.Tan(angle * Mathf.Deg2Rad);

            Mesh mesh = new Mesh();
            mesh.name = $"RampMesh_{angle}";

            Vector3 p0 = new Vector3(0, 0, 0);
            Vector3 p1 = new Vector3(rampWidth, 0, 0);
            Vector3 p2 = new Vector3(0, 0, depth);
            Vector3 p3 = new Vector3(rampWidth, 0, depth);
            Vector3 p4 = new Vector3(0, height, depth);
            Vector3 p5 = new Vector3(rampWidth, height, depth);

            Vector3[] vertices = new Vector3[]
            {
                // Slope
                p0, p4, p5,
                p0, p5, p1,

                // Bottom
                p0, p1, p3,
                p0, p3, p2,

                // Back
                p2, p3, p5,
                p2, p5, p4,

                // Left
                p2, p4, p0,

                // Right
                p1, p5, p3
            };

            int[] triangles = new int[24];
            for (int i = 0; i < 24; i++)
            {
                triangles[i] = i;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            mf.sharedMesh = mesh;
            mc.sharedMesh = mesh;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(_RampGenerator))]
    public class _RampGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            _RampGenerator generator = (_RampGenerator)target;

            GUILayout.Label("Angle:");
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("From", GUILayout.Width(35));
            generator.angleFrom = EditorGUILayout.FloatField(generator.angleFrom);
            GUILayout.Label("To", GUILayout.Width(20));
            generator.angleTo = EditorGUILayout.FloatField(generator.angleTo);
            GUILayout.EndHorizontal();

            GUILayout.Label("Step:");
            generator.angleStep = EditorGUILayout.FloatField(generator.angleStep);

            GUILayout.Label("Depth:");
            generator.depth = EditorGUILayout.FloatField(generator.depth);

            EditorGUILayout.Space();
            
            if (GUILayout.Button("Generate"))
            {
                generator.GenerateRamps();
            }

            EditorGUILayout.Space();
            GUILayout.Label("Optional Settings (Not in template):", EditorStyles.miniBoldLabel);
            generator.rampWidth = EditorGUILayout.FloatField("Ramp Width", generator.rampWidth);
            generator.spacing = EditorGUILayout.FloatField("Spacing", generator.spacing);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(generator);
            }
        }
    }
#endif
}
