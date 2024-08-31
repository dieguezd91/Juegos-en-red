using UnityEngine;
using Photon.Pun;
using Cinemachine;

public class GameController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    private void Start()
    {
        var player = PhotonNetwork.Instantiate(playerPrefab.name, new Vector2(Random.Range(-4, 4), Random.Range(-4, 4)), Quaternion.identity);

        if (virtualCamera != null)
        {
            virtualCamera.Follow = player.transform;
        }
    }
}
