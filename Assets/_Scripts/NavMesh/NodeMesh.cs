using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class NodeMesh : MonoBehaviour
{
    private NavMeshAgent _agent;

    private int _currentNode;
    private int _nextNode;
    private Transform _targetDestination;

    [SerializeField] private Transform[] NodeList;

    private void Awake()
    {
        // Get the NavMeshAgent component
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        BeginNavigation();
    }

    private void BeginNavigation()
    {
        // Teleport enemy to node 0
        this.transform.position = NodeList[0].position;

        //Set current node to 0
        _currentNode = 0;
        CalculateNextNode();
    }

    private void Update()
    {
        // Determine if the agent has reached the destination
        if (_agent.remainingDistance < 0.5f)
            _currentNode = CalculateNextNode();

        // Move the agent
        MoveCharacter();
    }

    private int CalculateNextNode()
    {
        //Go through list and find next node in the list
        _nextNode = (_currentNode + 1) % NodeList.Length;

        //Set target to next node
        _targetDestination = NodeList[_nextNode];
        return _nextNode;
    }

    private void MoveCharacter()
    {
        //Set destination to next node
        _agent.SetDestination(_targetDestination.position);
    }

    private void OnDrawGizmos()
    {
        //Make sure node list is not empty
        if (NodeList == null || NodeList.Length == 0)
            return;

        //Draw gizmos between each node in the sequence
        for (int i = 0; i < NodeList.Length; i++)
        {
            var nextIndex = (i + 1) % NodeList.Length;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(NodeList[i].position, NodeList[nextIndex].position);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(NodeList[i].position, 0.1f);
        }
    }
}