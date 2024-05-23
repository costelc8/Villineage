using Mirror;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

public class ResourceGenerator : MonoBehaviour
{
    private TerrainGenerator terrainGenerator;
    public GameObject treePrefab;
    public GameObject berryPrefab;
    public GameObject sheepPrefab;
    public GameObject goatPrefab;
    public GameObject wolfPrefab;
    private GameObject treeParent;
    private GameObject berryParent;
    private GameObject animalParent;
    private static List<Targetable> trees = new List<Targetable>();
    private static List<Targetable> berries = new List<Targetable>();
    private static List<Targetable> animals = new List<Targetable>();
    public Transform forestPosition;

    public bool generateForest;
    public bool destroyTrees;
    public bool generateBerries;
    public bool destroyBerries;

    public void Initialize()
    {
        terrainGenerator = GetComponent<TerrainGenerator>();
        treeParent = new GameObject("Trees");
        berryParent = new GameObject("Berries");
        animalParent = new GameObject("Animals");
    }

    // Update is called once per frame
    void Update()
    {
        if (generateForest)
        {
            generateForest = false;
            ClearAllTrees();
            GeneratePerlinForest();
        }
        if (destroyTrees)
        {
            destroyTrees = false;
            ClearAllTrees();
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

    public void ClearAllTrees()
    {
        foreach (Resource tree in trees) Destroy(tree.gameObject);
        trees.Clear();
    }

    // This method generates a forest with the given parameters
    public void GeneratePerlinForest()
    {
        Debug.Log("Generating Forest");
        Stopwatch timer = Stopwatch.StartNew();
        // Make new empty "Forest" gameobject, will parent all the trees as to not clutter the inspector

        // Clamp the input values to their allowed ranges
        int spacing = Mathf.Max(1, SimVars.VARS.forestSpacing);
        float density = Mathf.Clamp01(SimVars.VARS.forestDensity);
        float variation = spacing/2f - 1f;
        int size = SimVars.VARS.terrainSize;
        Vector2 center = new Vector2(size / 2, size / 2);
        float[,] perlin = PerlinGenerator.GeneratePerlin(size, size, 2, SimVars.VARS.GetSeed());

        // Just generate trees in a square for now
        for (int x = spacing; x < size; x += spacing)
        {
            for (int y = spacing; y < size; y += spacing)
            {
                if (density > Random.value && Vector2.Distance(new Vector2(x, y), center) > 20f)
                {
                    // Calculate random offset/variation, so the trees aren't just aligned on a perfect grid
                    Vector3 treePos = new Vector3(x + Random.Range(-1f, 1f) * variation, 0, y + Random.Range(-1f, 1f) * variation);
                    //if (Mathf.Pow(perlin[x, y], 2) < Random.Range(0.1f, 0.2f))
                    if (perlin[x, y] < SimVars.VARS.perlinForestThreshold + Random.Range(-0.1f, 0.1f))
                    {
                        // Generate new tree
                        treePos.y = terrainGenerator.GetTerrainHeight(treePos);
                        GameObject tree = Instantiate(treePrefab, treePos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), treeParent.transform);
                        tree.GetComponent<Resource>().quantity = SimVars.VARS.woodPerTree;
                        NetworkServer.Spawn(tree);
                        trees.Add(tree.GetComponent<Resource>());
                    }
                }
            }
        }
        timer.Stop();
        Debug.Log(trees.Count + " Trees Generated in " + timer.ElapsedMilliseconds + "ms");
    }


    public void GenerateSheep(Vector3 center, int count, float minRange, float maxRange)
    {
        for (int i = 0; i < count; i++)
        {
            if (RandomNavmeshPoint.RandomPointFromCenterCapsule(center, 1.5f, 1f, out Vector3 position, Random.Range(minRange, maxRange), 0.1f, maxRange))
            {
                GameObject sheep = Instantiate(sheepPrefab, position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), animalParent.transform);
                sheep.GetComponent<Animal>().wanderOrigin = center;
                sheep.GetComponent<Resource>().quantity = SimVars.VARS.foodPerSheep;
                NetworkServer.Spawn(sheep);
                animals.Add(sheep.GetComponent<Animal>());
                sheep.GetComponent<NavMeshAgent>().avoidancePriority = Random.Range(91, 100);
            }
        }
    }

    public void GenerateGoats(Vector3 center, int count, float minRange, float maxRange)
    {
        for (int i = 0; i < count; i++)
        {
            if (RandomNavmeshPoint.RandomPointFromCenterCapsule(center, 0.5f, 1f, out Vector3 position, Random.Range(minRange, maxRange), 0.1f, maxRange))
            {
                GameObject goat = Instantiate(goatPrefab, position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), animalParent.transform);
                goat.GetComponent<Animal>().wanderOrigin = center;
                goat.GetComponent<Resource>().quantity = SimVars.VARS.foodPerGoat;
                NetworkServer.Spawn(goat);
                animals.Add(goat.GetComponent<Animal>());
                goat.GetComponent<NavMeshAgent>().avoidancePriority = Random.Range(81, 90);
            }
        }
    }

    public void GenerateWolves(Vector3 center, int count, float minRange, float maxRange)
    {
        for (int i = 0; i < count; i++)
        {
            if (RandomNavmeshPoint.RandomPointFromCenterCapsule(center, 0.5f, 1f, out Vector3 position, Random.Range(minRange, maxRange), 0.1f, maxRange))
            {
                GameObject wolf = Instantiate(wolfPrefab, position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), animalParent.transform);
                wolf.GetComponent<Animal>().wanderOrigin = center;
                NetworkServer.Spawn(wolf);
                wolf.GetComponent<NavMeshAgent>().avoidancePriority = Random.Range(71, 80);
            }
        }
    }

    public void GenerateBerries()
    {
        // Temporary Berry bush generation
        //Debug.Log("Generating Berry Bushes");

        //for (int x = Random.Range(20, 40); x < SimVars.VARS.terrainSize; x += Random.Range(20, 40))
        //{
        //    for (int z = Random.Range(20, 40); z < SimVars.VARS.terrainSize; z += Random.Range(20, 40))
        //    {
        //        if (RandomNavmeshPoint.RandomPointFromCenterSphere(new Vector3(x, 10, z), 1.5f, out Vector3 position, 0, 0.1f, 1))
        //        {
        //            GameObject bush = Instantiate(berryPrefab, position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), berryParent.transform);
        //            bush.GetComponent<Resource>().quantity = SimVars.VARS.foodPerBerry;
        //            NetworkServer.Spawn(bush);
        //            berries.Add(bush.GetComponent<Resource>());
        //        }
        //    }
        //}

        Debug.Log("Generating Berry Bushes");
        Stopwatch timer = Stopwatch.StartNew();
        // Make new empty "Forest" gameobject, will parent all the trees as to not clutter the inspector

        // Clamp the input values to their allowed ranges
        int spacing = Mathf.Max(1, SimVars.VARS.forestSpacing);
        float density = Mathf.Clamp01(SimVars.VARS.forestDensity);
        float variation = spacing / 2f - 1f;
        int size = SimVars.VARS.terrainSize;
        Vector2 center = new Vector2(size / 2, size / 2);
        float[,] perlin = PerlinGenerator.GeneratePerlin(size, size, 2, SimVars.VARS.GetSeed());

        // Just generate trees in a square for now
        for (int x = spacing; x < size; x += spacing)
        {
            for (int z = spacing; z < size; z += spacing)
            {
                if (density > Random.value && Vector2.Distance(new Vector2(x, z), center) > 20f)
                {
                    // Calculate random offset/variation, so the trees aren't just aligned on a perfect grid
                    Vector3 berryPos = new Vector3(x + Random.Range(-1f, 1f) * variation, 10, z + Random.Range(-1f, 1f) * variation);
                    float lowerThreshold = SimVars.VARS.perlinForestThreshold - 0.1f;
                    float upperThreshold = SimVars.VARS.perlinForestThreshold + 0.1f;
                    if (perlin[x, z] > lowerThreshold && perlin[x, z] < upperThreshold && Random.value < 0.1f)
                    {
                        if (RandomNavmeshPoint.RandomPointFromCenterSphere(berryPos, 1.5f, out Vector3 position, 0, 0.5f, 1))
                        {
                            GameObject bush = Instantiate(berryPrefab, position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), berryParent.transform);
                            bush.GetComponent<Resource>().quantity = SimVars.VARS.foodPerBerry;
                            NetworkServer.Spawn(bush);
                            berries.Add(bush.GetComponent<Resource>());
                        }
                    }
                }
            }
        }
        timer.Stop();
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
        berries.Remove(resource);
        animals.Remove(resource);
    }

   public static void ReAddBerries(Resource resource) {
        if (resource.resourceType != ResourceType.Food) {
            Debug.Log("Wrong food!");
            return;
        }
        berries.Add(resource);
   }
}