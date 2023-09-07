public enum ActionCmd
{
    /// <summary>
    /// 主模块
    /// </summary>
    cmd = ModuleCmd.player_action,
    
    /// <summary>
    /// 进入游戏
    /// </summary>
    enterGame = 0,

    /**
     * 离开游戏
     */
    leaveGame = 1,

    /**
     * 进入地图
     */
    enterMap = 2,

    /**
     * 离开地图
     */
    leaveMap = 3,

    /**
     * 移动
     */
    move = 4,

    /**
     * 同步玩家
     */
    syncPlayer = 5,
}