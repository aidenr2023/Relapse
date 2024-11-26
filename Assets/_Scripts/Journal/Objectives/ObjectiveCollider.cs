using System;
using UnityEngine;

public class ObjectiveCollider : MonoBehaviour
{
    [SerializeField] private JournalObjectiveMode objectiveMode;
    [SerializeField] private JournalObjective objective;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        switch (objectiveMode)
        {
            case JournalObjectiveMode.Add:
                JournalObjectiveManager.Instance.AddObjective(objective);
                break;

            case JournalObjectiveMode.Complete:
                JournalObjectiveManager.Instance.CompleteObjective(objective);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}