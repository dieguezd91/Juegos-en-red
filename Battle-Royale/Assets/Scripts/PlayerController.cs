using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    PhotonView pv;
    Rigidbody2D rb;
    [SerializeField] float speedMovement;
    Vector2 inputMovement;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (pv.IsMine)
        {
            HandleInput();
        }
    }

    void FixedUpdate()
    {
        if (pv.IsMine)
        {
            Move();
        }
    }

    private void HandleInput()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        inputMovement = new Vector2(moveX, moveY).normalized;
    }

    private void Move()
    {
        rb.velocity = inputMovement * speedMovement;
    }
}

