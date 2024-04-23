using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceGeneration : MonoBehaviour
{
    public GameObject treePrefab;
    private List<GameObject> trees = new List<GameObject>();

    public Transform forestPosition;
    public int forestSize;
    public int forestSpacing;
    [Range(0f, 1f)]
    public float forestDensity;
    [Range(0f, 1f)]
    public float forestVariation;

    public bool generateForest;
    public bool destroyTrees;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (generateForest)
        {
            generateForest = false;
            GenerateForest(forestPosition == null ? Vector3.zero : forestPosition.position, forestSize, forestSpacing, forestDensity, forestVariation);
        }
        if (destroyTrees)
        {
            destroyTrees = false;
            foreach (GameObject tree in trees) Destroy(tree);
            trees.Clear();
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

        // Just generate trees in a square for now
        for (int x = -size; x <= size; x += spacing)
        {
            for (int y = -size; y <= size; y += spacing)
            {
                if (density > Random.value)
                {
                    // Calculate random offset/variation, so the trees aren't just aligned on a perfect grid
                    Vector2 offset = new Vector2(Random.Range(-1f, 1f) * variation, Random.Range(-1f, 1f) * variation);
                    Vector3 treePos = position + new Vector3(x + offset.x, 0, y + offset.y);
                    // Generate new tree
                    GameObject tree = Instantiate(treePrefab, treePos, Quaternion.identity, forest.transform);
                    trees.Add(tree);
                }
            }
        }
    }
}
