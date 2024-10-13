using UnityEngine;
using Photon.Pun;
using Cinemachine;

public class GameController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    private float waitTime = 0.3f;
    private float currentWaitTime;
    private bool initialized = false;

    private void Update()
    {
        if(currentWaitTime >= waitTime && initialized == false)
        {
            var player = PhotonNetwork.Instantiate(playerPrefab.name, new Vector2(Random.Range(-4, 4), Random.Range(-4, 4)), Quaternion.identity);


            player.GetComponent<PlayerController>().OnSpawn += GameManager.Instance.AddPlayer;
            player.GetComponent<LifeController>().OnDeath += GameManager.Instance.PlayerDeath;

            if (virtualCamera != null)
            {
                virtualCamera.Follow = player.transform;
            }

            initialized = true;
        }
        else
        {
            currentWaitTime += Time.deltaTime;
        }
        
    }

    private void Start()
    {
        
        
    }
}
