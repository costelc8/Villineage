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
    public Vector3 previousMousePos;
    public float animalRespawnTimer;

    public bool generateForest;
    public bool destroyTrees;
    public bool generateBerries;
    public bool destroyBerries;

    private ResourceSourceType drawingResource;

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
        if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Z))
        {
            ResourceSourceType resource = ResourceSourceType.None;
            if (Input.GetKeyDown(KeyCode.V)) resource = ResourceSourceType.Tree;
            if (Input.GetKeyDown(KeyCode.B)) resource = ResourceSourceType.Berry;
            if (Input.GetKeyDown(KeyCode.C)) resource = ResourceSourceType.Sheep;
            if (Input.GetKeyDown(KeyCode.X)) resource = ResourceSourceType.Goat;
            if (Input.GetKeyDown(KeyCode.Z)) resource = ResourceSourceType.Wolf;
            if (drawingResource == resource || resource == ResourceSourceType.None)
            {
                drawingResource = ResourceSourceType.None;
                Selection.Selector.mouseMode = MouseMode.Selecting;
            }
            else
            {
                Selection.Selector.mouseMode = MouseMode.Drawing;
                drawingResource = resource;
            }
        }
        if (Input.GetMouseButton(0))
        {
            if (Vector3.Distance(Input.mousePosition, previousMousePos) > 1)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, float.PositiveInfinity, LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
                {
                    if (drawingResource == ResourceSourceType.Tree) GenerateTrees(hitInfo.point, 3);
                    if (drawingResource == ResourceSourceType.Berry) GenerateBerries(hitInfo.point, 3);
                    if (drawingResource == ResourceSourceType.Sheep) GenerateSheep(hitInfo.point, 1);
                    if (drawingResource == ResourceSourceType.Goat) GenerateGoats(hitInfo.point, 1);
                    if (drawingResource == ResourceSourceType.Wolf) GenerateWolves(hitInfo.point, 1);
                }
            }
            previousMousePos = Input.mousePosition;
        }
        if (generateForest)
        {
            generateForest = false;
            ClearAllTrees();
            GeneratePerlinTrees();
        }
        if (destroyTrees)
        {
            destroyTrees = false;
            ClearAllTrees();
        }
        if (generateBerries)
        {
            generateBerries = false;
            ClearAllBerries();
            GeneratePerlinBerries();
        }
        if (destroyBerries)
        {
            destroyBerries = false;
            ClearAllBerries();
        }
        animalRespawnTimer += Time.deltaTime;
        if (animalRespawnTimer > 20f)
        {
            animalRespawnTimer -= 20f;
            if (animals.Count < Mathf.Pow(2, SimVars.VARS.terrainScale + 1))
            {
                Vector3 position = Vector3.zero;
                switch (Random.Range(0, 4))
                {
                    case 0: position = new Vector3(8, 10, Random.Range(8, SimVars.VARS.terrainSize - 8)); break;
                    case 1: position = new Vector3(SimVars.VARS.terrainSize, 10, Random.Range(8, SimVars.VARS.terrainSize - 8)); break;
                    case 2: position = new Vector3(Random.Range(8, SimVars.VARS.terrainSize - 8), 10, 8); break;
                    case 3: position = new Vector3(Random.Range(8, SimVars.VARS.terrainSize - 8), 10, SimVars.VARS.terrainSize - 8); break;
                }
                switch (Random.Range(0, 3))
                {
                    case 0: GenerateSheep(position, Random.Range(2, 4), 0, 6, new Vector3(SimVars.VARS.terrainSize / 2, 0, SimVars.VARS.terrainSize / 2)); break;
                    case 1: GenerateGoats(position, Random.Range(2, 4), 0, 6, new Vector3(SimVars.VARS.terrainSize / 2, 0, SimVars.VARS.terrainSize / 2)); break;
                    case 2: GenerateWolves(position, Random.Range(2, 4), 0, 6, new Vector3(SimVars.VARS.terrainSize / 2, 0, SimVars.VARS.terrainSize / 2)); break;
                }
            }
        }
    }

    public void ClearAllTrees()
    {
        foreach (Targetable tree in trees) Destroy(tree.gameObject);
        trees.Clear();
    }

    public void ClearAllBerries()
    {
        foreach (Targetable berry in berries) Destroy(berry.gameObject);
        berries.Clear();
    }

    // This method generates a forest with the given parameters
    public void GeneratePerlinTrees()
    {
        Debug.Log("Generating Trees");
        Stopwatch timer = Stopwatch.StartNew();

        Random.State state = Random.state; // Save Random's state
        Random.InitState(SimVars.VARS.GetSeed()); // Seed Random's state

        // Clamp the input values to their allowed ranges
        int spacing = Mathf.Max(1, SimVars.VARS.forestSpacing);
        float density = Mathf.Clamp01(SimVars.VARS.forestDensity);
        float variation = (spacing / 2f) - 1f;
        int size = SimVars.VARS.terrainSize;
        Vector2 center = new Vector2(size / 2, size / 2);
        float[,] perlin = PerlinGenerator.GeneratePerlin(size, size, 2, SimVars.VARS.GetSeed());

        for (int x = spacing; x < size; x += spacing)
        {
            for (int y = spacing; y < size; y += spacing)
            {
                if (density > Random.value && Vector2.Distance(new Vector2(x, y), center) > 10f * SimVars.VARS.terrainScale)
                {
                    if (perlin[x, y] < SimVars.VARS.perlinForestThreshold + Random.Range(-0.1f, 0.1f))
                    {
                        // Calculate random offset/variation, so the trees aren't just aligned on a perfect grid
                        Vector3 treePos = new Vector3(x + Random.Range(-1f, 1f) * variation, 0, y + Random.Range(-1f, 1f) * variation);
                        treePos.y = terrainGenerator.GetTerrainHeight(treePos);
                        // Generate new tree
                        GameObject tree = Instantiate(treePrefab, treePos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), treeParent.transform);
                        tree.GetComponent<Resource>().quantity = SimVars.VARS.woodPerTree;
                        NetworkServer.Spawn(tree);
                        trees.Add(tree.GetComponent<Resource>());
                    }
                }
            }
        }
        Random.state = state;
        timer.Stop();
        Debug.Log(trees.Count + " Trees Generated in " + timer.ElapsedMilliseconds + "ms");
    }

    public void GeneratePerlinBerries()
    {
        Debug.Log("Generating Berry Bushes");
        Stopwatch timer = Stopwatch.StartNew();

        Random.State state = Random.state; // Save Random's state
        Random.InitState(SimVars.VARS.GetSeed()); // Seed Random's state

        // Clamp the input values to their allowed ranges
        int spacing = Mathf.Max(1, SimVars.VARS.forestSpacing);
        float density = Mathf.Clamp01(SimVars.VARS.forestDensity);
        int size = SimVars.VARS.terrainSize;
        Vector2 center = new Vector2(size / 2, size / 2);
        float[,] perlin = PerlinGenerator.GeneratePerlin(size, size, 2, SimVars.VARS.GetSeed());
        int numGroves = 0;
        for (int x = spacing; x < size; x += spacing)
        {
            for (int z = spacing; z < size; z += spacing)
            {
                if (density > Random.value && Vector2.Distance(new Vector2(x, z), center) > 5f * SimVars.VARS.terrainScale)
                {
                    float upperThreshold = SimVars.VARS.perlinForestThreshold + 0.1f;
                    if (perlin[x, z] > SimVars.VARS.perlinForestThreshold && perlin[x, z] < upperThreshold && Random.value <= 0.01f)
                    {
                        GenerateBerries(new Vector3(x, 10, z), Random.Range(4, 9)); // Generate a cluster of 4-8 berry bushes
                        numGroves++;
                    }
                }
            }
        }
        Random.state = state;
        timer.Stop();
        Debug.Log(numGroves + " Berry Bush Clusters (" + berries.Count + " Bushes) Generated in " + timer.ElapsedMilliseconds + "ms");
    }

    public void GeneratePerlinAnimals()
    {
        Debug.Log("Generating Animals");
        Stopwatch timer = Stopwatch.StartNew();

        Random.State state = Random.state; // Save Random's state
        Random.InitState(SimVars.VARS.GetSeed()); // Seed Random's state

        // Clamp the input values to their allowed ranges
        int spacing = Mathf.Max(1, SimVars.VARS.forestSpacing);
        float density = 0.5f;
        int size = SimVars.VARS.terrainSize;
        Vector2 center = new Vector2(size / 2, size / 2);
        float[,] perlin = PerlinGenerator.GeneratePerlin(size, size, 2, SimVars.VARS.GetSeed());
        int sheepPacks = 0;
        int goatPacks = 0;
        int wolfPacks = 0;
        for (int x = spacing; x < size; x += spacing)
        {
            for (int z = spacing; z < size; z += spacing)
            {
                if (density > Random.value && Vector2.Distance(new Vector2(x, z), center) > 5f * SimVars.VARS.terrainScale)
                {
                    if (perlin[x, z] > 0.6f && Random.value <= 0.01f)
                    {
                        if (perlin[x, z] < 0.7f - 0.01 * sheepPacks)
                        {
                            GenerateSheep(new Vector3(x, 10, z), Random.Range(2, 5));
                            sheepPacks++;
                        }
                        else if (perlin[x, z] < 0.8f - 0.01 * goatPacks)
                        {
                            GenerateGoats(new Vector3(x, 10, z), Random.Range(2, 5));
                            goatPacks++;
                        }
                        else if (perlin[x, z] < 0.9f - 0.01 * wolfPacks)
                        {
                            GenerateWolves(new Vector3(x, 10, z), Random.Range(2, 5));
                            wolfPacks++;
                        }
                    }
                }
            }
        }
        Random.state = state;
        timer.Stop();
        Debug.Log(sheepPacks + " Sheep Packs, " + goatPacks + " Goat Packs, and " + wolfPacks + " Wolf Packs Generated in " + timer.ElapsedMilliseconds + "ms");
    }

    public void GenerateBerries(Vector3 position, int count, float minRange = 0f, float maxRange = 6f)
    {
        for (int i = 0; i < count; i++)
        {
            if (RandomNavmeshPoint.RandomPointFromCenterSphere(position, 2.5f, out Vector3 pos, minRange, 1f, maxRange))
            {
                GameObject bush = Instantiate(berryPrefab, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), berryParent.transform);
                bush.GetComponent<Resource>().quantity = SimVars.VARS.foodPerBerry;
                NetworkServer.Spawn(bush);
                berries.Add(bush.GetComponent<Resource>());
                Physics.SyncTransforms();
            }
        }
    }

    public void GenerateTrees(Vector3 position, int count, float minRange = 0f, float maxRange = 6f)
    {
        for (int i = 0; i < count; i++)
        {
            if (RandomNavmeshPoint.RandomPointFromCenterSphere(position, 2.5f, out Vector3 pos, minRange, 1f, maxRange))
            {
                GameObject tree = Instantiate(treePrefab, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), treeParent.transform);
                tree.GetComponent<Resource>().quantity = SimVars.VARS.woodPerTree;
                NetworkServer.Spawn(tree);
                trees.Add(tree.GetComponent<Resource>());
                Physics.SyncTransforms();
            }
        }
    }


    public void GenerateSheep(Vector3 center, int count, float minRange = 0f, float maxRange = 6f, Vector3 origin = new Vector3())
    {
        for (int i = 0; i < count; i++)
        {
            if (RandomNavmeshPoint.RandomPointFromCenterCapsule(center, 2.5f, 1f, out Vector3 position, minRange, 1f, maxRange))
            {
                GameObject sheep = Instantiate(sheepPrefab, position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), animalParent.transform);
                sheep.GetComponent<Animal>().wanderOrigin = (origin == Vector3.zero) ? center : origin;
                sheep.GetComponent<Resource>().quantity = SimVars.VARS.foodPerSheep;
                NetworkServer.Spawn(sheep);
                animals.Add(sheep.GetComponent<Animal>());
                sheep.GetComponent<NavMeshAgent>().avoidancePriority = Random.Range(91, 100);
            }
        }
    }

    public void GenerateGoats(Vector3 center, int count, float minRange = 0f, float maxRange = 6f, Vector3 origin = new Vector3())
    {
        for (int i = 0; i < count; i++)
        {
            if (RandomNavmeshPoint.RandomPointFromCenterCapsule(center, 2.5f, 1f, out Vector3 position, minRange, 1f, maxRange))
            {
                GameObject goat = Instantiate(goatPrefab, position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), animalParent.transform);
                goat.GetComponent<Animal>().wanderOrigin = (origin == Vector3.zero) ? center : origin;
                goat.GetComponent<Resource>().quantity = SimVars.VARS.foodPerGoat;
                NetworkServer.Spawn(goat);
                animals.Add(goat.GetComponent<Animal>());
                goat.GetComponent<NavMeshAgent>().avoidancePriority = Random.Range(81, 90);
            }
        }
    }

    public void GenerateWolves(Vector3 center, int count, float minRange = 0f, float maxRange = 6f, Vector3 origin = new Vector3())
    {
        for (int i = 0; i < count; i++)
        {
            if (RandomNavmeshPoint.RandomPointFromCenterCapsule(center, 2.5f, 1f, out Vector3 position, minRange, 1f, maxRange))
            {
                GameObject wolf = Instantiate(wolfPrefab, position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), animalParent.transform);
                wolf.GetComponent<Animal>().wanderOrigin = (origin == Vector3.zero) ? center : origin;
                NetworkServer.Spawn(wolf);
                animals.Add(wolf.GetComponent<Animal>());
                wolf.GetComponent<NavMeshAgent>().avoidancePriority = Random.Range(71, 80);
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
        berries.Remove(resource);
        animals.Remove(resource);
    }

   public static void ReAddBerries(Resource resource) {
        if (resource.resourceType != ResourceType.Food)
        {
            Debug.Log("Wrong food!");
            return;
        }
        berries.Add(resource);
   }
}