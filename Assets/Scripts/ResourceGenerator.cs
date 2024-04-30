using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceGenerator : MonoBehaviour
{
    public TerrainGenerator terrainGenerator;
    public GameObject treePrefab;
    public static List<GameObject> Trees = new List<GameObject>();
    public Transform forestPosition;
    public int forestSpacing;
    [Range(0f, 1f)]
    public float forestDensity;
    [Range(0f, 1f)]
    public float forestVariation;

    public int seed;
    public float scale;

    public bool generateForest;
    public bool destroyTrees;

    // Start is called before the first frame update
    void Start()
    {
        terrainGenerator = GetComponent<TerrainGenerator>();
        if (generateForest)
        {
            generateForest = false;
            GenerateForest(forestPosition == null ? Vector3.zero : forestPosition.position, terrainGenerator.size, forestSpacing, forestDensity, forestVariation);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (generateForest)
        {
            generateForest = false;
            foreach (GameObject tree in Trees) Destroy(tree);
            Trees.Clear();
            GenerateForest(forestPosition == null ? Vector3.zero : forestPosition.position, terrainGenerator.size, forestSpacing, forestDensity, forestVariation);
        }
        if (destroyTrees)
        {
            destroyTrees = false;
            foreach (GameObject tree in Trees) Destroy(tree);
            Trees.Clear();
        }
    }

    // This method generates a forest with the given parameters
    public void GenerateForest(Vector3 position, int size, int spacing, float density, float variation)
    {
        // Make new empty "Forest" gameobject, will parent all the trees as to not clutter the inspector
        GameObject forest = new GameObject("Forest");
        forest.transform.position = position;

        // Clamp the input values to their allowed ranges
        spacing = Mathf.Max(1, spacing);
        density = Mathf.Clamp01(density);
        variation = Mathf.Clamp01(variation) * (spacing/2f - 1f);
        size = (size / spacing) * spacing;

        float[,] perlin = PerlinGenerator.GeneratePerlin(size, size, scale, seed);

        // Just generate trees in a square for now
        for (int x = spacing; x < size; x += spacing)
        {
            for (int y = spacing; y < size; y += spacing)
            {
                if (density > Random.value)
                {
                    // Calculate random offset/variation, so the trees aren't just aligned on a perfect grid
                    Vector2 offset = new Vector2(Random.Range(-1f, 1f) * variation, Random.Range(-1f, 1f) * variation);
                    Vector3 treePos = position + new Vector3(x + offset.x, 0, y + offset.y);
                    if (Mathf.Pow(perlin[x, y], 2) < Random.Range(0.1f, 0.2f))
                    {
                        // Generate new tree
                        treePos.y = terrainGenerator.GetTerrainHeight(treePos);
                        GameObject tree = Instantiate(treePrefab, treePos, Quaternion.identity, forest.transform);
                        Trees.Add(tree);
                    }
                }
            }
        }
        Debug.Log(Trees.Count + " Trees Generated");
    }

    public static void UpdateList(GameObject tree) {
        Trees.Remove(tree);
    }
}
