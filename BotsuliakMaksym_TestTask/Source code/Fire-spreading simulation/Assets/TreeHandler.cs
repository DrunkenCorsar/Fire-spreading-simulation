using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Config;

public class TreeHandler : MonoBehaviour
{
    public MainSimulationScript mainScript;

    public Material greenMaterial;
    public Material burningMaterial;
    public Material burntMaterial;

    float remainingHP; // HP are representing how much damage tree can handle before start burning
    float remainingFuel; // Fuel is representing how much time is tree fonna burn
    float burningRate; // How fast does tree burns down
    float recievingDPS; // Current DPS that is recieving from nearby trees
    TreeStatus status;

    public bool IsGreen => (status == TreeStatus.Green);
    public bool IsBurning => (status == TreeStatus.Burning);
    float RandomizationCoefficient => (1f + Random.Range(-TreeParametersRandomness, TreeParametersRandomness)); // Coefficient that helps make trees parametres a little different from tree to tree

    // Start is called before the first frame update
    void Start()
    {
        ResetToGreen();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Method that adds some more DPS from nearby burning tree
    /// </summary>
    /// <param name="additionDPS">DPS that should be added</param>
    public void AddIngnition(float additionDPS)
    {
        recievingDPS += additionDPS;
    }

    private void FixedUpdate()
    {
        if (mainScript.simulate)
        {
            if (IsGreen)
            {
                ApplyDamage(Time.fixedDeltaTime * recievingDPS);
                if (remainingHP <= 0)
                {
                    SetFire();
                    mainScript.SpreadFireFromBurningTree(this.gameObject.transform.position);
                }
            }
            else if (IsBurning)
            {
                remainingFuel -= Time.deltaTime * burningRate;
                if (remainingFuel <= 0f)
                {
                    Extinguish();
                }
            }
        }
    }

    public void StopFireIgnition()
    {
        if (IsGreen)
        {
            recievingDPS = 0f;
        }
    }

    public bool LightOrExtinguish()
    {
        if (IsGreen)
        {
            SetFire();
            return true;
        }
        if (IsBurning)
        {
            Extinguish();
            return false;
        }
        return true;
    }

    void ResetToGreen()
    {
        remainingHP = InitialTreeHP * RandomizationCoefficient;
        remainingFuel = InitialTreeFuel * RandomizationCoefficient;
        burningRate = TreeBurningRate * RandomizationCoefficient;

        status = TreeStatus.Green;
        this.gameObject.GetComponent<MeshRenderer>().material = greenMaterial;
    }

    public void ApplyDamage(float damageRecieved)
    {
        if (IsGreen)
        {
            remainingHP -= damageRecieved;
            if (remainingHP <= 0)
            {
                SetFire();
            }
        }
    }

    void SetFire()
    {
        if (IsGreen)
        {
            remainingHP = 0f;
            recievingDPS = 0f;
            status = TreeStatus.Burning;
            this.gameObject.GetComponent<MeshRenderer>().material = burningMaterial;
        }
    }

    void Extinguish()
    {
        if (IsBurning)
        {
            if (remainingFuel <= 0)
            {
                remainingFuel = 0f;
                status = TreeStatus.Burnt;
                this.gameObject.GetComponent<MeshRenderer>().material = burntMaterial;
            }
            else
            {
                remainingHP = InitialTreeHP * RandomizationCoefficient;
                status = TreeStatus.Green;
                this.gameObject.GetComponent<MeshRenderer>().material = greenMaterial;
            }
        }
    }
}
