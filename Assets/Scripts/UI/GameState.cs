public enum GameState
{
    Normal,         // 평상시
    InventoryOpen,  // 인벤토리 열림
    MergeOpen       // 머지 스테이션 열림
}

public static class GameStateManager
{
    public static GameState Current = GameState.Normal;
}
