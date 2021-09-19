using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static class that holds all constants and enumerations of programm in one place
/// </summary>
public static class Config
{
    // UI parameters
    public const string SimulateButtonStartText = "Simulate";
    public const string SimulateButtonStopText = "Stop";

    // Trees spawning parameters
    public const float TreesMinX = -300f;
    public const float TreesMaxX = 300f;
    public const float TreesMinZ = -300f;
    public const float TreesMaxZ = 300f;
    public const int TreesMinCount = 7000;
    public const int TreesMaxCount = 9000;

    // Trees random firing parameters
    public const int MinimumFiredTrees = 3;
    public const int MaximumFiredTrees = 10;

    // Click detection parameters
    public const float RayLengthCheck = 1000f;

    // Burning tree variables
    public const float InitialTreeHP = 100f;
    public const float InitialTreeFuel = 100f;
    public const float TreeBurningRate = 4.5f;
    public const float TreeParametersRandomness = 0.1f;
    public const float PotentialIgnitionDistance = 30f;
    public const float CircledBurningRadius = 8f;
    public const float WindBurningRadiusMultiplier = 1.5f;
    public const float MaxWindBurningPower = 0.35f;

    public const float MaximumTreeDPS = 35f;


    // --------------- Enums ---------------

    public enum TreeStatus
    {
        Green,
        Burning,
        Burnt
    }

    public enum ActionOnClick
    {
        BurnOrExtinguish = 0,
        SpawnTree = 1,
        RemoveTree = 2
    }

    // -------------------------------------
}
