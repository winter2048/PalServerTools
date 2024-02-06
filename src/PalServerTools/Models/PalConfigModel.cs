namespace PalServerTools.Models
{
    public class PalConfigModel
    {
        public string Difficulty { get; set; }
        public double DayTimeSpeedRate { get; set; }
        public double NightTimeSpeedRate { get; set; }
        public double ExpRate { get; set; }
        public double PalCaptureRate { get; set; }
        public double PalSpawnNumRate { get; set; }
        public double PalDamageRateAttack { get; set; }
        public double PalDamageRateDefense { get; set; }
        public double PlayerDamageRateAttack { get; set; }
        public double PlayerDamageRateDefense { get; set; }
        public double PlayerStomachDecreaseRate { get; set; }
        public double PlayerStaminaDecreaseRate { get; set; }
        public double PlayerAutoHPRegeneRate { get; set; }
        public double PlayerAutoHPRegeneRateInSleep { get; set; }
        public double PalStomachDecreaseRate { get; set; }
        public double PalStaminaDecreaseRate { get; set; }
        public double PalAutoHPRegeneRate { get; set; }
        public double PalAutoHPRegeneRateInSleep { get; set; }
        public double BuildObjectDamageRate { get; set; }
        public double BuildObjectDeteriorationDamageRate { get; set; }
        public double CollectionDropRate { get; set; }
        public double CollectionObjectHpRate { get; set; }
        public double CollectionObjectRespawnSpeedRate { get; set; }
        public double EnemyDropItemRate { get; set; }
        public string DeathPenalty { get; set; }
        public bool bEnablePlayerToPlayerDamage { get; set; }
        public bool bEnableFriendlyFire { get; set; }
        public bool bEnableInvaderEnemy { get; set; }
        public bool bActiveUNKO { get; set; }
        public bool bEnableAimAssistPad { get; set; }
        public bool bEnableAimAssistKeyboard { get; set; }
        public int DropItemMaxNum { get; set; }
        public int DropItemMaxNum_UNKO { get; set; }
        public int BaseCampMaxNum { get; set; }
        public int BaseCampWorkerMaxNum { get; set; }
        public double DropItemAliveMaxHours { get; set; }
        public bool bAutoResetGuildNoOnlinePlayers { get; set; }
        public double AutoResetGuildTimeNoOnlinePlayers { get; set; }
        public int GuildPlayerMaxNum { get; set; }
        public double PalEggDefaultHatchingTime { get; set; }
        public double WorkSpeedRate { get; set; }
        public bool bIsMultiplay { get; set; }
        public bool bIsPvP { get; set; }
        public bool bCanPickupOtherGuildDeathPenaltyDrop { get; set; }
        public bool bEnableNonLoginPenalty { get; set; }
        public bool bEnableFastTravel { get; set; }
        public bool bIsStartLocationSelectByMap { get; set; }
        public bool bExistPlayerAfterLogout { get; set; }
        public bool bEnableDefenseOtherGuildPlayer { get; set; }
        public int CoopPlayerMaxNum { get; set; }
        public int ServerPlayerMaxNum { get; set; }
        public string ServerName { get; set; }
        public string ServerDescription { get; set; }
        public string AdminPassword { get; set; }
        public string ServerPassword { get; set; }
        public int PublicPort { get; set; }
        public string PublicIP { get; set; }
        public bool RCONEnabled { get; set; }
        public int RCONPort { get; set; }
        public string Region { get; set; }
        public bool bUseAuth { get; set; }
        public string BanListURL { get; set; }
    }
}
