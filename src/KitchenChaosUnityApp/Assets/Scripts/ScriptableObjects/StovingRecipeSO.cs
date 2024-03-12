using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class StovingRecipeSO : ScriptableObject
{
    public KitchenObjectSO input;
    public KitchenObjectSO outputWhenCooked;
    public KitchenObjectSO outputWhenBurned;
    public int secondsToCook;
    public int secondsToBurnAfterCooked;
}
