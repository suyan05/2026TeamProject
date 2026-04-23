public class ItemInstance
{
    public ItemData data;
    public int uniqueID;

    private static int nextID = 0;

    public ItemInstance(ItemData data)
    {
        this.data = data;
        uniqueID = nextID++;
    }
}