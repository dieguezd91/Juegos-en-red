using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumable", menuName = "ItemInfo/ConsumableInfo")]
public class ConsumableInfo : ItemBase
{
    [SerializeField] float recoveryAmount;

    [SerializeField] Stats affectedStat;

    private enum Stats
    {
        health,
        shield,
        stamina,
        ammo
    }

    public void Use(PlayerController user)
    {
        int i = (int)affectedStat;

        switch(i)
        {
            case 0:
                user.GetComponent<LifeController>().HealingAmount = recoveryAmount;
                user.GetComponent<LifeController>().StartHealing();
                break;

            case 1:
                break;

            case 2:
                break;

            case 3:
                break;

            default:
                break;

        }
    }

    public override void Spawn(Vector3 position, Quaternion rotation)
    {
        var temp = Photon.Pun.PhotonNetwork.Instantiate("ConsumableCollectable", position, rotation);
        temp.GetComponent<ConsumableItem>().SetInfo(this);
    }
}
