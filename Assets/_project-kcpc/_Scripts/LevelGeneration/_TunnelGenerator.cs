using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProjectKCPC
{
    public class _TunnelGenerator : MonoBehaviour
    {
        public float floorHeightFrom = 0f;
        public float floorHeightTo = 5f;
        public float floorHeightStep = 1f;

        public float wallSizeSide = 1f;
        public float wallSizeRoof = 1f;

        public float tunnelSizeWidth = 3f;
        public float tunnelSizeHeight = 3f;
        public float tunnelSizeDepth = 3f;

        public float spacing = 0f;

        public void GenerateBlocks()
        {
            // Очистка старых блоков
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child.name.StartsWith("TunnelBlock"))
                {
#if UNITY_EDITOR
                    Undo.DestroyObjectImmediate(child.gameObject);
#else
                    DestroyImmediate(child.gameObject);
#endif
                }
            }

            float currentH = floorHeightFrom;
            int count = 0;

            if (floorHeightStep > 0)
            {
                while (currentH <= floorHeightTo + 0.0001f)
                {
                    CreateBlock(currentH, count);
                    currentH += floorHeightStep;
                    count++;
                    if (count > 1000) break; // Защита от бесконечного цикла
                }
            }
            else if (floorHeightStep < 0)
            {
                while (currentH >= floorHeightTo - 0.0001f)
                {
                    CreateBlock(currentH, count);
                    currentH += floorHeightStep;
                    count++;
                    if (count > 1000) break;
                }
            }
            else
            {
                // Если шаг 0, генерируем только один блок
                CreateBlock(currentH, count);
            }
        }

        private void CreateBlock(float H, int index)
        {
            GameObject block = new GameObject($"TunnelBlock_{index}");
            block.transform.SetParent(this.transform);
            
            // Сдвиг по оси X (бок к боку) с учетом полной ширины блока и отступов
            float totalBlockWidth = tunnelSizeWidth + 2 * wallSizeSide;
            float xPos = index * (totalBlockWidth + spacing);
            block.transform.localPosition = new Vector3(xPos, 0, 0);
            block.transform.localRotation = Quaternion.identity;

#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(block, "Create Tunnel Block");
#endif

            // Нижняя часть (пол)
            // Идет от 0 до H. Если H < 0, то обрезается на уровне 0 (высота пола становится 0).
            float floorTop = Mathf.Max(0, H);
            float floorBottom = 0f; // Строго в нулевой координате
            float currentFloorHeight = floorTop - floorBottom;

            if (currentFloorHeight > 0)
            {
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.name = "Floor";
                floor.transform.SetParent(block.transform);
                float width = tunnelSizeWidth + 2 * wallSizeSide;
                floor.transform.localScale = new Vector3(width, currentFloorHeight, tunnelSizeDepth);
                floor.transform.localPosition = new Vector3(0, floorBottom + currentFloorHeight / 2f, tunnelSizeDepth / 2f);
            }

            // Боковые стены
            // Если H < 0, тоннель опускается и обрезается. Стены идут до верха потолка, обрезаясь снизу на 0.
            float wallTop = Mathf.Max(0, H + tunnelSizeHeight + wallSizeRoof);
            float wallBottom = Mathf.Max(0, H);
            float currentWallHeight = wallTop - wallBottom;

            if (currentWallHeight > 0)
            {
                GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leftWall.name = "LeftWall";
                leftWall.transform.SetParent(block.transform);
                leftWall.transform.localScale = new Vector3(wallSizeSide, currentWallHeight, tunnelSizeDepth);
                leftWall.transform.localPosition = new Vector3(-(tunnelSizeWidth + wallSizeSide) / 2f, wallBottom + currentWallHeight / 2f, tunnelSizeDepth / 2f);

                GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rightWall.name = "RightWall";
                rightWall.transform.SetParent(block.transform);
                rightWall.transform.localScale = new Vector3(wallSizeSide, currentWallHeight, tunnelSizeDepth);
                rightWall.transform.localPosition = new Vector3((tunnelSizeWidth + wallSizeSide) / 2f, wallBottom + currentWallHeight / 2f, tunnelSizeDepth / 2f);
            }

            // Потолок
            float roofTop = Mathf.Max(0, H + tunnelSizeHeight + wallSizeRoof);
            float roofBottom = Mathf.Max(0, H + tunnelSizeHeight);
            float currentRoofHeight = roofTop - roofBottom;

            if (currentRoofHeight > 0)
            {
                GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
                roof.name = "Roof";
                roof.transform.SetParent(block.transform);
                float width = tunnelSizeWidth + 2 * wallSizeSide;
                roof.transform.localScale = new Vector3(width, currentRoofHeight, tunnelSizeDepth);
                roof.transform.localPosition = new Vector3(0, roofBottom + currentRoofHeight / 2f, tunnelSizeDepth / 2f);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(_TunnelGenerator))]
    public class _TunnelGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            _TunnelGenerator generator = (_TunnelGenerator)target;
            Undo.RecordObject(generator, "Change Tunnel Generator Parameters");

            float originalLabelWidth = EditorGUIUtility.labelWidth;

            // Floor Height:
            // From: <field> To: <field> Step: <field>
            GUILayout.Label("Floor Height", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 40;
            generator.floorHeightFrom = EditorGUILayout.FloatField("From:", generator.floorHeightFrom);
            EditorGUIUtility.labelWidth = 25;
            generator.floorHeightTo = EditorGUILayout.FloatField("To:", generator.floorHeightTo);
            EditorGUIUtility.labelWidth = 35;
            generator.floorHeightStep = EditorGUILayout.FloatField("Step:", generator.floorHeightStep);
            GUILayout.EndHorizontal();

            // Walls Size:
            // Side: <field> Roof: <field>
            GUILayout.Space(5);
            GUILayout.Label("Walls Size", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 35;
            generator.wallSizeSide = EditorGUILayout.FloatField("Side:", generator.wallSizeSide);
            EditorGUIUtility.labelWidth = 35;
            generator.wallSizeRoof = EditorGUILayout.FloatField("Roof:", generator.wallSizeRoof);
            GUILayout.EndHorizontal();

            // Tunnel Size:
            // Width: <field> Height: <field> Depth: <field>
            GUILayout.Space(5);
            GUILayout.Label("Tunnel Size", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 45;
            generator.tunnelSizeWidth = EditorGUILayout.FloatField("Width:", generator.tunnelSizeWidth);
            EditorGUIUtility.labelWidth = 45;
            generator.tunnelSizeHeight = EditorGUILayout.FloatField("Height:", generator.tunnelSizeHeight);
            EditorGUIUtility.labelWidth = 45;
            generator.tunnelSizeDepth = EditorGUILayout.FloatField("Depth:", generator.tunnelSizeDepth);
            GUILayout.EndHorizontal();

            // Spacing:
            // <field>
            GUILayout.Space(5);
            GUILayout.Label("Spacing", EditorStyles.boldLabel);
            EditorGUIUtility.labelWidth = originalLabelWidth;
            generator.spacing = EditorGUILayout.FloatField(generator.spacing);

            GUILayout.Space(15);
            if (GUILayout.Button("Generate", GUILayout.Height(30)))
            {
                generator.GenerateBlocks();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(generator);
            }
        }
    }
#endif
}
