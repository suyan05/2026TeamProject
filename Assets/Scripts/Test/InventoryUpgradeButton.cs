using UnityEngine;

public class InventoryUpgradeButton : MonoBehaviour
{
    public InventoryGrid grid;

    public void Upgrade()
    {
        // ¿¹: °¡·Î 2Ä­, ¼¼·Î 1Ä­ È®Àå
        grid.ExpandGrid(2, 1);
    }
}
