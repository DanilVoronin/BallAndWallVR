using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using Valve;

public class Controls : MonoBehaviour {

    SteamVR_TrackedController _Track;

    void Start()
    {
        _Track = transform.parent.GetComponent<SteamVR_TrackedController>();  
    }

    IEnumerator Vibra()
    {
        for(int q = 0; q < 5; q++)
        {
            yield return new WaitForEndOfFrame();
            _Track.Vibration(4000);
        }
    }

    //Перестраховка
    void OnCollisionEnter(Collision collision)
    {
        //-------------------------------------------
        //Проверка 3
        //-------------------------------------------
        if (GameState.GameStateObj._State != 6)
            GameState.GameStateObj._State = 2;

        if (collision.collider.name == "Sphere")
        {
            StopAllCoroutines();
            StartCoroutine(Vibra());
        }
    }

    // Update is called once per frame
    void Update ()
    {
        if (GameState.GameStateObj._State == 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 50))
            {
                if (_Track != null && _Track.triggerPressed)
                {
                    GameState.GameStateObj._State = 1;
                }
            }
        }
	}
}
