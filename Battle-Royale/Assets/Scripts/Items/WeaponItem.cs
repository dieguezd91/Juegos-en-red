public class WeaponItem : CollectableItem
{
    private WeaponInfo weaponInfo;
    void Start()
    {
        OnCollected += Test;
    }

    void Test(PlayerController player)
    {
        Photon.Pun.PhotonNetwork.Destroy(this.gameObject);
        print("Item collected");
    }

    public void SetInfo(WeaponInfo info)
    {
        weaponInfo = info;
    }
}
