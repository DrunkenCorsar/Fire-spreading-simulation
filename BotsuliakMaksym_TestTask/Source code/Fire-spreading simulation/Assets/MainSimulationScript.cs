using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using static Config;

public partial class MainSimulationScript : MonoBehaviour
{
    public GameObject treeSample; // Template gameobject for spawning trees instanciating this object
    public Terrain terrain;
    public Camera mainCamera;

    // Objects that are helping to anderstand wind speed and direction
    public GameObject arrowShowingWindDirection; 
    public GameObject sphereShowingWindSpeed;

    public Button simulateButton;

    List<GameObject> wood;
    public bool simulate;

    Wind wind = new Wind(0f, 0f, 0f, 0f);
    ActionOnClick clickMode;

    // Start is called before the first frame update
    void Start()
    {
        wood = new List<GameObject>();
        GenerateNewWood();
        UpdateOrientirs();
        clickMode = ActionOnClick.BurnOrExtinguish;
        simulate = true;
        simulateButton.GetComponentsInChildren<Text>()[0].text = SimulateButtonStopText;
    }

    public void ChangeWindSpeed(float value)
    {
        wind.Speed = value;
        float angleInRadians = (wind.Angle / 360f) * 2f * Mathf.PI;
        wind.OffsetX = Mathf.Sin(angleInRadians) * WindBurningRadiusMultiplier * CircledBurningRadius * value;
        wind.OffsetZ = Mathf.Cos(angleInRadians) * WindBurningRadiusMultiplier * CircledBurningRadius * value;
        UpdateOrientirs();
    }

    public void ChangeWindDirection(float value)
    {
        wind.Angle = value * 360;
        float angleInRadians = value * 2f * Mathf.PI;
        wind.OffsetX = Mathf.Sin(angleInRadians) * WindBurningRadiusMultiplier * CircledBurningRadius * wind.Speed;
        wind.OffsetZ = Mathf.Cos(angleInRadians) * WindBurningRadiusMultiplier * CircledBurningRadius * wind.Speed;
        UpdateOrientirs();
    }

    public void FireRandomTrees()
    {
        int count = Random.Range(MinimumFiredTrees, MaximumFiredTrees + 1);
        for (int i = 0; i < count; i++)
        {
            int firedIndex = Random.Range(0, wood.Count);
            if (wood[firedIndex].GetComponent<TreeHandler>().LightOrExtinguish())
            {
                SpreadFireFromBurningTree(wood[firedIndex].transform.position);
            }
        }
    }

    /// <summary>
    /// Method that updates wind's speed and direction indicators. Reacts to sliders' value change
    /// </summary>
    void UpdateOrientirs()
    {
        sphereShowingWindSpeed.transform.position = new Vector3(arrowShowingWindDirection.transform.position.x + wind.OffsetX,
                                                   arrowShowingWindDirection.transform.position.y - 1,
                                                   arrowShowingWindDirection.transform.position.z + wind.OffsetZ);
        arrowShowingWindDirection.transform.rotation = Quaternion.Euler(0, wind.Angle - 90, 0);
    }

    /// <summary>
    /// Method that changes click mode (action that is done by MouseClick)
    /// </summary>
    /// <param name="value">integer interpretation of corresponding ActionOnClick enum item</param>
    public void UpdateClickMode(int value)
    {
        clickMode = (ActionOnClick)value;
    }

    /// <summary>
    /// Stops and plays simulation
    /// </summary>
    public void ChangeSimulateStatus()
    {
        if (simulate)
        {
            simulate = false;
            simulateButton.GetComponentsInChildren<Text>()[0].text = SimulateButtonStartText;
        }
        else
        {
            simulate = true;
            simulateButton.GetComponentsInChildren<Text>()[0].text = SimulateButtonStopText;
        }
    }

    public void ExitFromApplication()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        HandleMouseOnClickActions();
    }

    /// <summary>
    /// Method that performs actions on mouse click. Depens on current click mode
    /// </summary>
    void HandleMouseOnClickActions()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, RayLengthCheck))
            {
                if (hit.transform != null)
                {
                    switch (clickMode)
                    {
                        case ActionOnClick.BurnOrExtinguish:
                            if (hit.transform.gameObject.tag == treeSample.tag)
                            {
                                if (hit.transform.gameObject.GetComponent<TreeHandler>().LightOrExtinguish())
                                {
                                    SpreadFireFromBurningTree(hit.transform.gameObject.transform.position);
                                }
                                else
                                {
                                    StopIgnitionAroundTree(hit.transform.gameObject.transform.position);
                                }
                            }
                            break;
                        case ActionOnClick.SpawnTree:
                            if (hit.transform.gameObject.tag == terrain.tag)
                            {
                                wood.Add(Instantiate(treeSample));
                                wood[wood.Count - 1].transform.position = hit.point + new Vector3(0, treeSample.transform.localScale.y, 0);
                                wood[wood.Count - 1].SetActive(true);
                            }
                            break;
                        case ActionOnClick.RemoveTree:
                            if (hit.transform.gameObject.tag == treeSample.tag)
                            {
                                int id = hit.transform.gameObject.GetInstanceID();
                                for (int i = 0; i < wood.Count; i++)
                                {
                                    if (wood[i].GetInstanceID() == id)
                                    {
                                        Destroy(wood[i]);
                                        wood.RemoveAt(i);
                                        break;
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Clears old wood and generates a new one
    /// </summary>
    public void GenerateNewWood()
    {
        ClearWood();

        int treeCount = Random.Range(TreesMinCount, TreesMaxCount);
        for (int i = 0; i < treeCount; i++)
        {
            float treeHeightOffset = treeSample.transform.localScale.y;
            float treeX = Random.Range(TreesMinX, TreesMaxX);
            float treeZ = Random.Range(TreesMinZ, TreesMaxZ);
            float treeY = terrain.SampleHeight(new Vector3(treeX, 0f, treeZ)) + treeHeightOffset;
            GameObject newTree = Instantiate(treeSample);
            newTree.transform.position = new Vector3(treeX, treeY, treeZ);
            newTree.SetActive(true);
            wood.Add(newTree);
        }
    }

    public void ClearWood()
    {
        while(wood.Count > 0)
        {
            Destroy(wood[0]);
            wood.RemoveAt(0);
        }
    }

    /// <summary>
    /// Method that calculates list of trees, on which fire can be spread from current tree.
    /// </summary>
    /// <param name="treePosition">Current burning tree</param>
    public void SpreadFireFromBurningTree(Vector3 treePosition)
    {
        float minX = treePosition.x - PotentialIgnitionDistance;
        float maxX = treePosition.x + PotentialIgnitionDistance;
        float minZ = treePosition.z - PotentialIgnitionDistance;
        float maxZ = treePosition.z + PotentialIgnitionDistance;

        // Going through all green trees that can be in the range of fire
        for (int i = 0; i < wood.Count; i++)
        {
            if (wood[i].transform.position.x >= minX && wood[i].transform.position.x <= maxX &&
                wood[i].transform.position.z >= minZ && wood[i].transform.position.z <= maxZ &&
                wood[i].GetComponent<TreeHandler>().IsGreen)
            {
                // Calculating if this nearby tree can be damaged
                float damagePerSecond;
                if (CalculateCircledFireDamage(out damagePerSecond, new Vector2(treePosition.x, treePosition.z),
                    new Vector2(wood[i].transform.position.x, wood[i].transform.position.z)))
                {

                    // Applying damage to tree
                    wood[i].GetComponent<TreeHandler>().AddIngnition(damagePerSecond);
                }
            }
        }
    }

    /// <summary>
    /// Method that stops first stage of fire starting process on nearby trees.
    /// Used when player is trying to extinguish tree so that nearby trees won't fire without reason after some time.
    /// </summary>
    /// <param name="treePosition"></param>
    void StopIgnitionAroundTree(Vector3 treePosition)
    {
        float minX = treePosition.x - PotentialIgnitionDistance;
        float maxX = treePosition.x + PotentialIgnitionDistance;
        float minZ = treePosition.z - PotentialIgnitionDistance;
        float maxZ = treePosition.z + PotentialIgnitionDistance;

        // Going through all green trees that can be in the range of fire
        for (int i = 0; i < wood.Count; i++)
        {
            if (wood[i].transform.position.x >= minX && wood[i].transform.position.x <= maxX &&
                wood[i].transform.position.z >= minZ && wood[i].transform.position.z <= maxZ &&
                wood[i].GetComponent<TreeHandler>().IsGreen)
            {
                // Calculating if this nearby tree can be damaged
                float damagePerSecond;
                if (CalculateCircledFireDamage(out damagePerSecond, new Vector2(treePosition.x, treePosition.z),
                    new Vector2(wood[i].transform.position.x, wood[i].transform.position.z)))
                {

                    // Applying damage to tree
                    wood[i].GetComponent<TreeHandler>().StopFireIgnition();
                }
            }
        }
    }

    /// <summary>
    /// Calculating fire damage by theoretical system version 1.3 (Doubbled sircle). 
    /// Fire depens on trees position and on wind.
    /// </summary>
    /// <param name="damagePerSecond">Output parameter that shows estimated damage per second on tree that is potentially can be ignited.</param>
    /// <param name="burningTreePosition">Position of tree that is spreading fire</param>
    /// <param name="greenTreePosition">Position of tree that is potencially can be ignited</param>
    /// <returns></returns>
    bool CalculateCircledFireDamage(out float damagePerSecond, Vector2 burningTreePosition, Vector2 greenTreePosition)
    {
        float distanceBetweenTrees = Vector2.Distance(burningTreePosition, greenTreePosition);

        if (distanceBetweenTrees > CircledBurningRadius)
        {
            damagePerSecond = 0f;
        }
        else
        {
            damagePerSecond = ((CircledBurningRadius - distanceBetweenTrees) / CircledBurningRadius) * MaximumTreeDPS;
        }

        if (wind.Speed > 0)
        {
            Vector2 burningWindPosition = new Vector2(burningTreePosition.x + wind.OffsetX, burningTreePosition.y + wind.OffsetZ);
            if (Vector2.Distance(burningWindPosition, greenTreePosition) <= CircledBurningRadius * WindBurningRadiusMultiplier)
            {
                damagePerSecond += MaxWindBurningPower * wind.Speed * MaximumTreeDPS;
            }
        }

        if (damagePerSecond == 0f)
        {
            return false;
        }

        return true;
    }

    struct Wind
    {
        public Wind(float OffsetX, float OffsetZ, float Speed, float Angle)
        {
            this.OffsetX = OffsetX;
            this.OffsetZ = OffsetZ;
            this.Speed = Speed;
            this.Angle = Angle;
        }

        public float OffsetX; // Offset (by X axis) of second circle, relative to burning tree coordinates (1.3 theoretical model)
        public float OffsetZ; // Offset (by X axis) of second circle, relative to burning tree coordinates (1.3 theoretical model)
        public float Speed;
        public float Angle;
    }
}
