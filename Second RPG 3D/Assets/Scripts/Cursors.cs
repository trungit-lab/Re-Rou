using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursors : MonoBehaviour
{
    public GameObject cursorsObject;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        cursorsObject.transform.position = Input.mousePosition;
    }
}
