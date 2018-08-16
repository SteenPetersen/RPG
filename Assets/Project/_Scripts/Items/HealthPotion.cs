using UnityEngine;

[CreateAssetMenu(fileName = "Health Potion", menuName ="Items/Potion", order = 1)]
public class HealthPotion : Item, IUseable {

    [SerializeField]
    private int health = 0;

    public override void Use()
    {
        if (PlayerStats.instance.currentHealth < PlayerStats.instance.maxHealth)
        {
            SoundManager.instance.PlayInventorySound("gulp");

            Remove();

            PlayerStats.instance.Heal(health);
        }
    }

    public override string GetDescription(bool showSaleValue = true)
    {
        return base.GetDescription() + string.Format("Use: Restores <color=#00ff00ff>{0}</color> health", health);
    }
}
