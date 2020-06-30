using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MortonCodeTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        if(GUI.Button(new Rect(100, 100, 200, 150),"span Morton Code")){
            LBVH bVH = new LBVH();
            bVH.CreateAABB(3, 3, 3);
        }
    }
}
