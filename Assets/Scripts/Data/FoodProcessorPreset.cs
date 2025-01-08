using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FoodProcessorPreset", menuName = "Scriptable Objects/FoodProcessorPreset")]
public class FoodProcessorPreset : ScriptableObject
{
    public List<CookingAction> actions;
}
