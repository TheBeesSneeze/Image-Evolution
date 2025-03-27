using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    private float x_gap => CandidateManager.x_gap;
    private float y_gap => CandidateManager.y_gap;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        float x = ( Time.time % (x_gap * 2) ) - x_gap;
        float y = ( Time.time % (y_gap * 2) ) - y_gap;
        Vector2 pos = new Vector2( x, y );  
        transform.position = pos;
    }
}
