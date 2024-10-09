public class WeaponItem : CollectableItem
{
    void Start()
    {
        OnCollected += Test;
    }

    void Test(PlayerController player)
    {
        Destroy(this.gameObject);
        print("Item collected");
    } 
}
