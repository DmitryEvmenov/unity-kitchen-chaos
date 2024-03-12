using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlatesCounterVisual : MonoBehaviour
{
    [SerializeField] private Transform counterTopPoint;
    [SerializeField] private Transform plateVisualPrefab;
    [SerializeField] private PlatesCounter platesCounter;
    [SerializeField] private float plateOffsetY;

    private List<GameObject> plateVisualGameObjectList;

    private void Awake()
    {
        plateVisualGameObjectList = new List<GameObject>();
    }

    public void Start()
    {
        platesCounter.OnPlateSpawned += PlatesCounter_OnPlateSpawned;
        platesCounter.OnPlateRemoved += PlatesCounter_OnPlateRemoved;
    }

    private void PlatesCounter_OnPlateRemoved(object sender, System.EventArgs e)
    {
        var lastPlate = plateVisualGameObjectList.TakeLast(1).First();
        Destroy(lastPlate);

        plateVisualGameObjectList.Remove(lastPlate);
    }

    private void PlatesCounter_OnPlateSpawned(object sender, System.EventArgs e)
    {
        var plateTransform = Instantiate(plateVisualPrefab, counterTopPoint);
        plateTransform.localPosition = new Vector3(0, plateOffsetY * plateVisualGameObjectList.Count, 0);

        plateVisualGameObjectList.Add(plateTransform.gameObject);
    }
}
