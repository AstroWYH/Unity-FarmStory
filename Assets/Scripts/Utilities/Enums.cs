public enum ItemType
{
    Seed, Commodity, Furniture,///种子，商品，家具///
    HoeTool, ChopTool, BreakTool,ReapTool, WaterTool,CollectTool,///锄头，砍树，砸石头，割草，浇水，收割
    ReapableScenery///杂草
}


public enum SlotType
{
    Bag, Box, Shop
}

public enum InventoryLocation
{
    Player, Box
}
public enum PartType 
{
    None, Carry, Hoe, Break, Water, Chop, Collect, Reap///空，举起，工具
}

public enum PartName
{
    Body, Hair, Arm, Tool
}

public enum Season
{
    春天, 夏天, 秋天, 冬天
}

public enum GridType//网格类型
{
    Diggable, DropItem, PlaceFurniture, NPCObstacle
}

public enum ParticleEffectType
{
    None, LeavesFalling01, LeavesFalling02, Rock, ReapableScenery
}

public enum GameState
{
    Gameplay, Pause
}

public enum LightShift
{
    Morning, Night
}

public enum SoundName
{
    none, FootStepSoft, FootStepHard,
    Axe, Pickaxe, Hoe, Reap, Water, Basket,
    Pickup, Plant, TreeFalling, Rustle,
    AmbientCountryside1, AmbientCountryside2, MusicCalm1, MusicCalm2, MusicCalm3, MusicCalm4, MusicCalm5, MusicCalm6, AmbientIndoor1
}