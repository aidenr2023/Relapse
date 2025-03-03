using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ComicImpactManager : MonoBehaviour
{
    public static ComicImpactManager Instance { get; private set; }

    [SerializeField] private float impactDuration;
    [SerializeField] private Vector3 minOffset;
    [SerializeField] private Vector3 maxOffset;
    [SerializeField] private GameObject[] comicUIPrefabs;

    private void Awake()
    {
        // Destroy if the instance is already set
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    public GameObject SpawnImpact(Vector3 position)
    {
        var randomPrefab = comicUIPrefabs[Random.Range(0, comicUIPrefabs.Length)];

        var comicUI = Instantiate(randomPrefab, position, Quaternion.identity);
        Destroy(comicUI, impactDuration);

        // Get the main camera from the camera manager
        var mainCamera = CameraManager.Instance.MainCamera;

        var offset = new Vector3(
            Random.Range(minOffset.x, maxOffset.x),
            Random.Range(minOffset.y, maxOffset.y),
            Random.Range(minOffset.z, maxOffset.z)
        );

        var currentOffset = new Vector3(
            mainCamera.transform.right.x * offset.x,
            mainCamera.transform.up.y * offset.y,
            mainCamera.transform.forward.z * offset.z
        );
        
        // Apply the offset to the comic UI
        comicUI.transform.position += currentOffset;

        return comicUI;
    }
}