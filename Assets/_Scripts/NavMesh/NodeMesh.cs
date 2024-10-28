using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class NodeMesh : MonoBehaviour
{
    private NavMeshAgent agent;

    int currentNode;
    int nextNode;
    Transform targetDestination;

    [SerializeField] Transform[] NodeList;
    void Update()
    {
        
            if(agent.remainingDistance < 0.5f)
        {
            currentNode = CalculateNextNode();
            
        }
        moveCharacter();
    }
    private void Start()
    {
        BeginNavigation();
        agent=GetComponent<NavMeshAgent>();
    }

    void BeginNavigation()
    {
        // Teleport enemy to node 0
        this.transform.position = NodeList[0].position;

        //Set current node to 0
        currentNode = 0;
        CalculateNextNode();
    }
    int CalculateNextNode()
    {
        //Go through list and find next node in the list
        nextNode = (currentNode + 1)%NodeList.Length;

        //Set target to next node
        targetDestination = NodeList[nextNode];
        return nextNode;
    }
    
    void moveCharacter()
    {
        //Set destination to next node
        agent.SetDestination(targetDestination.position);
    }
    private void OnDrawGizmos()
    {
        //Make sure node list is not empty
        if (NodeList == null || NodeList.Length == 0)
        {
            return;
        }
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
