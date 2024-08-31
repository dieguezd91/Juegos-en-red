using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public PhotonView pv;
    private Rigidbody2D _rb;
    [SerializeField] private float speedMovement;
    private Vector2 _inputMovement;

    public static event System.Action<PlayerController> OnPlayerControllerInstantiated;

    private void Start()
    {
        pv = GetComponent<PhotonView>();
        _rb = GetComponent<Rigidbody2D>();

        if (pv.IsMine)
        {
            OnPlayerControllerInstantiated?.Invoke(this);
        }
    }

    private void Update()
    {
        if (pv.IsMine)
        {
            HandleInput();
        }
    }

    private void FixedUpdate()
    {
        if (pv.IsMine)
        {
            Move();
        }
    }

    private void HandleInput()
    {
        var moveX = Input.GetAxis("Horizontal");
        var moveY = Input.GetAxis("Vertical");
        _inputMovement = new Vector2(moveX, moveY).normalized;
    }

    private void Move()
    {
        _rb.velocity = _inputMovement * speedMovement;
    }
}

