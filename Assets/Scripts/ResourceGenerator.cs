using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class ResourceGenerator : MonoBehaviour
{
    public TerrainGenerator terrainGenerator;
    public GameObject treePrefab;
    public GameObject berryPrefab;
    public GameObject sheepPrefab;
    private static List<Targetable> trees = new List<Targetable>();
    private static List<Targetable> berries = new List<Targetable>();
    private static List<Targetable> animals = new List<Targetable>();
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
    public bool generateBerries;
    public bool destroyBerries;

    private bool initialized;

    private void Awake()
    {
        if (!initialized) Initialize();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (generateForest)
        {
            generateForest = false;
            GenerateForest(forestPosition == null ? Vector3.zero : forestPosition.position, terrainGenerator.size, forestSpacing, forestDensity, forestVariation);
        }
    }

    private void Initialize()
    {
        terrainGenerator = GetComponent<TerrainGenerator>();

        initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (generateForest)
        {
            generateForest = false;
            foreach (Resource tree in trees) Destroy(tree.gameObject);
            trees.Clear();
            GenerateForest(forestPosition == null ? Vector3.zero : forestPosition.position, terrainGenerator.size, forestSpacing, forestDensity, forestVariation);
        }
        if (destroyTrees)
        {
            destroyTrees = false;
            foreach (Resource tree in trees) Destroy(tree.gameObject);
            trees.Clear();
        }
        if (generateBerries)
        {
            generateBerries = false;
            foreach (Resource berry in berries) Destroy(berry.gameObject);
            berries.Clear();
            GenerateBerries();
        }
        if (destroyBerries)
        {
            destroyBerries = false;
            foreach (Resource berry in berries) Destroy(berry.gameObject);
            berries.Clear();
        }
    }

    public void GenerateDefaultForest()
    {
        GenerateForest(forestPosition == null ? Vector3.zero : forestPosition.position, terrainGenerator.size, forestSpacing, forestDensity, forestVariation);
    }

    // This method generates a forest with the given parameters
    public void GenerateForest(Vector3 position, int size, int spacing, float density, float variation)
    {
        if (!initialized) Initialize();
        Debug.Log("Generating Forest");
        // Make new empty "Forest" gameobject, will parent all the trees as to not clutter the inspector
        GameObject forest = new GameObject("Forest");
        forest.transform.position = position;

        // Clamp the input values to their allowed ranges
        spacing = Mathf.Max(1, spacing);
        density = Mathf.Clamp01(density);
        variation = Mathf.Clamp01(variation) * (spacing/2f - 1f);
        size = (size / spacing) * spacing;
        Vector2 center = new Vector2(size / 2f, size / 2f);

        if (seed == 0) seed = Random.Range(int.MinValue, int.MaxValue);
        float[,] perlin = PerlinGenerator.GeneratePerlin(size, size, scale, seed);

        // Just generate trees in a square for now
        for (int x = spacing; x < size; x += spacing)
        {
            for (int y = spacing; y < size; y += spacing)
            {
                if (density > Random.value && Vector2.Distance(new Vector2(x, y), center) > 16f)
                {
                    // Calculate random offset/variation, so the trees aren't just aligned on a perfect grid
                    Vector2 offset = new Vector2(Random.Range(-1f, 1f) * variation, Random.Range(-1f, 1f) * variation);
                    Vector3 treePos = position + new Vector3(x + offset.x, 0, y + offset.y);
                    if (Mathf.Pow(perlin[x, y], 2) < Random.Range(0.1f, 0.2f))
                    {
                        // Generate new tree
                        treePos.y = terrainGenerator.GetTerrainHeight(treePos);
                        GameObject tree = Instantiate(treePrefab, treePos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), forest.transform);
                        NetworkServer.Spawn(tree);
                        trees.Add(tree.GetComponent<Resource>());
                    }
                }
            }
        }
        Debug.Log(trees.Count + " Trees Generated");
    }


    public void GenerateAnimals(Vector3 center, int count, float minRange, float maxRange)
    {
        if (!initialized) Initialize();
        for (int i = 0; i < count; i++)
        {
            if (RandomNavmeshPoint.RandomPointFromCenterCapsule(center, 0.5f, 1f, out Vector3 position, Random.Range(minRange, maxRange), 0.1f, maxRange))
            {
                GameObject animal = Instantiate(sheepPrefab, position, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
                NetworkServer.Spawn(animal);
                animals.Add(animal.GetComponent<PassiveAnimal>());
                animal.GetComponent<NavMeshAgent>().avoidancePriority = Random.Range(51, 100);
            }
        }
    }

    public void GenerateBerries()
    {
        if (!initialized) Initialize();
        // Temporary Berry bush generation
        Debug.Log("Generating Berry Bushes");
        GameObject bushes = new GameObject("Bushes");
        bushes.transform.position = Vector3.zero;
        Vector3 position = bushes.transform.position;

        for (int x = Random.Range(20, 60); x < terrainGenerator.size; x += Random.Range(30, 100))
        {
            for (int z = Random.Range(20, 60); z < terrainGenerator.size; z += Random.Range(30, 100))
            {
                position.x = x;
                position.z = z;
                position.y = terrainGenerator.GetTerrainHeight(position);
                GameObject bush = Instantiate(berryPrefab, position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), bushes.transform);
                NetworkServer.Spawn(bush);
                berries.Add(bush.GetComponent<Resource>());
            }
        }
    }

    public static List<Targetable> GetTrees()
    {
        return trees;
    }

    public static List<Targetable> GetBerries()
    {
        return berries;
    }

    public static List<Targetable> GetAnimals()
    {
        return animals;
    }

    public static void RemoveResource(Resource resource)
    {
        trees.Remove(resource);
    }
}