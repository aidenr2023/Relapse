using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerSelect : MonoBehaviour
{
    //Select the Animation layer by index
    public int layerIndex = 0;

    enum MyEnum
    {
        Fallen, 
        Idle,
        interactive
        
    }
    // Start is called before the first frame update
    void Start()
    {
        //Set the layer to the selected index
        GetComponent<Animator>().SetLayerWeight(layerIndex, 1);
        //for each layer in the animator that not the selected layer
        for (int i = 0; i < GetComponent<Animator>().layerCount; i++)
        {
            //if the layer is not the selected layer
            if (i != layerIndex)
            {
                //Set the weight of the layer to 0
                GetComponent<Animator>().SetLayerWeight(i, 0);
            }
        }
    }
    
}
