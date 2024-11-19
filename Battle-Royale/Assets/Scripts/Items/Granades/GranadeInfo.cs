using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGranade", menuName = "ItemInfo/GranadeInfo")]
public class GranadeInfo : ItemBase
{
    [SerializeField] private float bounciness;

    [SerializeField] private float damage;

    [SerializeField] private float range;

    [SerializeField] private GameObject prefab;

    public GameObject Prefab { get { return prefab; } }

    public override void Spawn(Vector3 position, Quaternion rotation)
    {
        var temp = Photon.Pun.PhotonNetwork.Instantiate("GranadeCollectable", position, rotation);
        temp.GetComponent<GranadeItem>().SetInfo(this);
    }

    public void Throw()
    {
        // instantiate explosive granade prefab
    }
}
