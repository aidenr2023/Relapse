using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TemporaryApartmentLoader : MonoBehaviour
{
    [SerializeField] private PlayerInfo player;
    [SerializeField] private LevelLoaderSection[] levelLoaderSections;

    private readonly HashSet<GameObject> _loadedLevels = new HashSet<GameObject>();

    private void Awake()
    {
        // Account for the levels and add them to the hash set
        AccountForLevels();
    }

    private void AccountForLevels()
    {
        foreach (var section in levelLoaderSections)
        {
            foreach (var level in section.LevelsToLoad)
                _loadedLevels.Add(level);
        }
    }

    private void Update()
    {
        var currentSection = DetermineCurrentSection();

        if (currentSection == null)
            return;

        Debug.Log($"Current Section: {currentSection.LowerY} - {currentSection.UpperY}");

        // Load the levels
        LoadLevels(currentSection);
    }

    private LevelLoaderSection DetermineCurrentSection()
    {
        var playerY = player.transform.position.y;

        foreach (var section in levelLoaderSections)
        {
            if (playerY >= section.LowerY && playerY < section.UpperY)
                return section;
        }

        return null;
    }

    private void LoadLevels(LevelLoaderSection section)
    {
        // Unload the levels

        var levelsToUnload = _loadedLevels.Where(level => !section.LevelsToLoad.Contains(level)).ToList();

        foreach (var level in levelsToUnload)
        {
            if (level == null)
                continue;

            _loadedLevels.Remove(level);

            level.SetActive(false);
        }

        // Load the levels
        foreach (var level in section.LevelsToLoad)
        {
            if (level == null)
                continue;

            _loadedLevels.Add(level);

            level.SetActive(true);
        }
    }

    private void OnDrawGizmos()
    {
        var posVector = new Vector3(transform.position.x, 0, transform.position.z);

        const float lineSize = 200;

        foreach (var section in levelLoaderSections)
        {
            var lowerPos = posVector + new Vector3(0, section.LowerY, 0);
            var upperPos = posVector + new Vector3(0, section.UpperY, 0);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(lowerPos, lowerPos + Vector3.forward * lineSize);
            Gizmos.DrawLine(lowerPos, lowerPos - Vector3.forward * lineSize);
            Gizmos.DrawLine(lowerPos, lowerPos + Vector3.right * lineSize);
            Gizmos.DrawLine(lowerPos, lowerPos - Vector3.right * lineSize);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(upperPos, upperPos + Vector3.forward * lineSize);
            Gizmos.DrawLine(upperPos, upperPos - Vector3.forward * lineSize);
            Gizmos.DrawLine(upperPos, upperPos + Vector3.right * lineSize);
            Gizmos.DrawLine(upperPos, upperPos - Vector3.right * lineSize);
        }
    }

    [Serializable]
    private class LevelLoaderSection
    {
        [SerializeField] private float lowerY;
        [SerializeField] private float upperY;

        [SerializeField] private GameObject[] levelsToLoad;

        public float LowerY => lowerY;
        public float UpperY => upperY;
        public GameObject[] LevelsToLoad => levelsToLoad;
    }
}