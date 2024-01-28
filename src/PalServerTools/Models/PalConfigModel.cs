namespace PalServerTools.Models
{
    public class PalConfigModel
    {
        public string Difficulty { get; set; }
        public float DayTimeSpeedRate { get; set; }
        public float NightTimeSpeedRate { get; set; }
        public float ExpRate { get; set; }
        public float PalCaptureRate { get; set; }
        public float PalSpawnNumRate { get; set; }
        public float PalDamageRateAttack { get; set; }
        public float PalDamageRateDefense { get; set; }
        public float PlayerDamageRateAttack { get; set; }
        public float PlayerDamageRateDefense { get; set; }
        public float PlayerStomachDecreaseRate { get; set; }
        public float PlayerStaminaDecreaseRate { get; set; }
        public float PlayerAutoHPRegeneRate { get; set; }
        public float PlayerAutoHPRegeneRateInSleep { get; set; }
        public float PalStomachDecreaseRate { get; set; }
        public float PalStaminaDecreaseRate { get; set; }
        public float PalAutoHPRegeneRate { get; set; }
        public float PalAutoHPRegeneRateInSleep { get; set; }
        public float BuildObjectDamageRate { get; set; }
        public float BuildObjectDeteriorationDamageRate { get; set; }
        public float CollectionDropRate { get; set; }
        public float CollectionObjectHpRate { get; set; }
        public float CollectionObjectRespawnSpeedRate { get; set; }
        public float EnemyDropItemRate { get; set; }
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
        public float DropItemAliveMaxHours { get; set; }
        public bool bAutoResetGuildNoOnlinePlayers { get; set; }
        public float AutoResetGuildTimeNoOnlinePlayers { get; set; }
        public int GuildPlayerMaxNum { get; set; }
        public float PalEggDefaultHatchingTime { get; set; }
        public float WorkSpeedRate { get; set; }
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
