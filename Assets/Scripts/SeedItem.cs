using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CreateAssetMenu(fileName = "New Seed", menuName = "Items/New Seed", order = 1)]
public class SeedItem : ScriptableObject
{
    public string ID = Guid.NewGuid().ToString().ToUpper();
    public string seedName;
    public string description;
    public int growTime;
    public int cropAmount;
    public int sellPrice;
    public int hungerAmount;
}
