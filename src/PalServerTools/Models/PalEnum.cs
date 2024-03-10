namespace PalServerTools.Models
{
    public enum InstallState
    {
        None,
        Installing,
        Failed,
        Installed
    }

    public enum PalServerState
    {
        Running,
        Stopped
    }

    public enum PalServerUpdateState
    {
        None,
        Updating,
        Success,
        Failed
    }

    public enum EPalOptionWorldDeathPenalty
    {
        None,
        Item,
        ItemAndEquipment,
        All
    }

    public enum EPalOptionWorldDifficulty
    {
        None,
        Difficulty
    }
}
