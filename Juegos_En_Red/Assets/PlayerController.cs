using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    PhotonView pv;
    public float speed;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    void Update()
    {
        if(pv.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.W))
                transform.position += Vector3.up * speed * Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.S))
                transform.position += Vector3.down * speed * Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.A))
                transform.position += Vector3.left * speed * Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.D))
                transform.position += Vector3.right * speed * Time.deltaTime;

        }
    }
}
