using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    public GameObject grass;
    public ParticleSystem fxHit;
    private bool isCut;
    // Start is called before the first frame update
    void Start()
    {
        isCut = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void GetHit(int n)
    {
        if (isCut)
        {
            grass.transform.localScale = Vector3.one * 2f;
            fxHit.Emit(100);
            isCut = false;
        }
        
    }
}
