using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ArenaGenerator : EditorWindow
{
    [Header("Prefabs")]
    public GameObject[] floorPrefabs;   // SM_Env_Ground_Dirt_Flat_*
    public GameObject[] rockPrefabs;    // SM_Env_Rock_Cliff_*

    // Configurações da arena
    int gridX = 8;              // nº de placas de chão na largura
    int gridZ = 8;              // nº de placas na profundidade
    float tileSize = 10f;       // tamanho de cada placa (medir no prefab)
    int rockCount = 25;         // quantos rochedos espalhar
    float minDistance = 4f;     // distância mínima entre rochedos
    int seed = 0;               // muda o seed = layout diferente
    Transform parent;           // arraste o objeto "Ground" aqui

    SerializedObject so;

    [MenuItem("Tools/Arena Generator")]
    static void Open() => GetWindow<ArenaGenerator>("Arena Generator");

    void OnEnable() => so = new SerializedObject(this);

    void OnGUI()
    {
        so.Update();
        EditorGUILayout.PropertyField(so.FindProperty("floorPrefabs"), true);
        EditorGUILayout.PropertyField(so.FindProperty("rockPrefabs"), true);
        so.ApplyModifiedProperties();

        gridX = EditorGUILayout.IntField("Grid X (placas)", gridX);
        gridZ = EditorGUILayout.IntField("Grid Z (placas)", gridZ);
        tileSize = EditorGUILayout.FloatField("Tamanho da placa", tileSize);
        rockCount = EditorGUILayout.IntField("Qtd rochedos", rockCount);
        minDistance = EditorGUILayout.FloatField("Dist. min rochedos", minDistance);
        seed = EditorGUILayout.IntField("Seed (mude p/ variar)", seed);
        parent = (Transform)EditorGUILayout.ObjectField("Parent (Ground)", parent, typeof(Transform), true);

        EditorGUILayout.Space();
        if (GUILayout.Button("Gerar Arena"))
            Generate();
        if (GUILayout.Button("Limpar filhos do Parent"))
            ClearChildren();
    }

    void Generate()
    {
        if (parent == null) { Debug.LogError("Defina o Parent (Ground)."); return; }
        if (floorPrefabs == null || floorPrefabs.Length == 0) { Debug.LogError("Sem floorPrefabs."); return; }

        ClearChildren();
        Random.InitState(seed);

        // 1) Piso em grid
        Vector3 origin = parent.position;
        for (int x = 0; x < gridX; x++)
        for (int z = 0; z < gridZ; z++)
        {
            var prefab = floorPrefabs[Random.Range(0, floorPrefabs.Length)];
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.position = origin + new Vector3(x * tileSize, 0, z * tileSize);
            // rotação em passos de 90° pra variar sem criar buracos
            go.transform.rotation = Quaternion.Euler(0, 90 * Random.Range(0, 4), 0);
            Undo.RegisterCreatedObjectUndo(go, "Gerar Arena");
        }

        // 2) Rochedos espalhados com distância mínima (Poisson simplificado)
        if (rockPrefabs != null && rockPrefabs.Length > 0)
        {
            float w = gridX * tileSize;
            float d = gridZ * tileSize;
            var placed = new List<Vector3>();
            int tries = 0, maxTries = rockCount * 30;

            while (placed.Count < rockCount && tries < maxTries)
            {
                tries++;
                Vector3 p = origin + new Vector3(Random.Range(0, w), 100f, Random.Range(0, d));

                bool tooClose = false;
                foreach (var q in placed)
                    if (Vector3.Distance(new Vector3(p.x,0,p.z), new Vector3(q.x,0,q.z)) < minDistance)
                    { tooClose = true; break; }
                if (tooClose) continue;

                // assenta no chão via raycast
                float groundY = origin.y;
                if (Physics.Raycast(p, Vector3.down, out RaycastHit hit, 200f))
                    groundY = hit.point.y;

                var prefab = rockPrefabs[Random.Range(0, rockPrefabs.Length)];
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                Vector3 finalPos = new Vector3(p.x, groundY, p.z);
                go.transform.position = finalPos;
                go.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                float s = Random.Range(0.85f, 1.3f);
                go.transform.localScale = Vector3.one * s;
                Undo.RegisterCreatedObjectUndo(go, "Gerar Arena");
                placed.Add(finalPos);
            }
        }

        Debug.Log("Arena gerada. Lembre de fazer Bake das NavMeshSurface!");
    }

    void ClearChildren()
    {
        if (parent == null) return;
        for (int i = parent.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);
    }
}