using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Settings.Base.Global;

namespace MoreSettlementAction
{
    public class MoreSettlementActionSettings : AttributeGlobalSettings<MoreSettlementActionSettings>
    {
        public override string Id => nameof(MoreSettlementActionSettings);

        public override string DisplayName => "More Settlement Action Settings";

        public override string FolderName => nameof(MoreSettlementActionSettings);

        public override string Format => "json";

        [SettingPropertyGroup("Village Housing", GroupOrder = 0)]
        [SettingPropertyInteger("HardWood Cost", 1, 1000, "0", HintText = "HardWood cost for building village housing", Order = 0, RequireRestart = false)]
        public int VillageHousingHardwoodCost { get; set; } = 50;

        [SettingPropertyGroup("Village Housing", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Base Build Time", 1, 10000, "0.00", HintText = "Time in hours to complete with party size 1 and engineering skill 0", Order = 0, RequireRestart = false)]
        public float VillageHousingBaseBuildTime { get; set; } = 1000;

        [SettingPropertyGroup("Village Housing", GroupOrder = 0)]
        [SettingPropertyInteger("Engineering XP Gain", 0, 1000, "0", HintText = "Engineering Skill XP gained upon completion", Order = 0, RequireRestart = false)]
        public int VillageHousingEngineeringXp { get; set; } = 100;

        [SettingPropertyGroup("Village Housing", GroupOrder = 0)]
        [SettingPropertyInteger("Gold Gain", 0, 10000, "0", HintText = "Gold gained upon completion", Order = 0, RequireRestart = false)]
        public int VillageHousingGold { get; set; } = 400;

        [SettingPropertyGroup("Village Housing", GroupOrder = 0)]
        [SettingPropertyInteger("Hearth Gain", 0, 100, "0", HintText = "Village hearth gained upon completion", Order = 0, RequireRestart = false)]
        public int VillageHousingHearth { get; set; } = 10;

        [SettingPropertyGroup("Village Housing", GroupOrder = 0)]
        [SettingPropertyInteger("Notable Relations Gain", 0, 100, "0", HintText = "Relations with all notables in village gained upon completion", Order = 0, RequireRestart = false)]
        public int VillageHousingRelations { get; set; } = 3;

        [SettingPropertyGroup("Village Clearing", GroupOrder = 0)]
        [SettingPropertyInteger("Tool Cost", 0, 100, "0", HintText = "Tools required for clearing village land", Order = 0, RequireRestart = false)]
        public int VillageClearingToolCost { get; set; } = 10;

        [SettingPropertyGroup("Village Clearing", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Base Build Time", 1, 10000, "0.00", HintText = "Time in hours to complete with party size 1 and athletics skill 0", Order = 0, RequireRestart = false)]
        public float VillageClearingTime { get; set; } = 500;

        [SettingPropertyGroup("Village Clearing", GroupOrder = 0)]
        [SettingPropertyInteger("Athletic XP Gain", 0, 1000, "0", HintText = "Athletics Skill XP gained upon completion", Order = 0, RequireRestart = false)]
        public int VillageClearingAthleticXp { get; set; } = 100;

        [SettingPropertyGroup("Village Clearing", GroupOrder = 0)]
        [SettingPropertyInteger("Gold Gain", 0, 10000, "0", HintText = "Gold gained upon completion", Order = 0, RequireRestart = false)]
        public int VillageClearingGold { get; set; } = 200;

        [SettingPropertyGroup("Village Clearing", GroupOrder = 0)]
        [SettingPropertyInteger("Hearth Gain", 0, 100, "0", HintText = "Village hearth gained upon completion", Order = 0, RequireRestart = false)]
        public int VillageClearingHearth { get; set; } = 5;

        [SettingPropertyGroup("Village Clearing", GroupOrder = 0)]
        [SettingPropertyInteger("Notable Relations Gain", 0, 100, "0", HintText = "Relations with all notables in village gained upon completion", Order = 0, RequireRestart = false)]
        public int VillageClearingRelations { get; set; } = 2;

        [SettingPropertyGroup("Town Housing", GroupOrder = 0)]
        [SettingPropertyInteger("HardWood Cost", 1, 1000, "0", HintText = "HardWood cost for building town housing", Order = 0, RequireRestart = false)]
        public int TownHousingHardwoodCost { get; set; } = 200;

        [SettingPropertyGroup("Town Housing", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Base Build Time", 1, 10000, "0.00", HintText = "Time in hours to complete with party size 1 and engineering skill 0", Order = 0, RequireRestart = false)]
        public float TownHousingBaseBuildTime { get; set; } = 2500;

        [SettingPropertyGroup("Town Housing", GroupOrder = 0)]
        [SettingPropertyInteger("Engineering XP Gain", 0, 1000, "0", HintText = "Engineering Skill XP gained upon completion", Order = 0, RequireRestart = false)]
        public int TownHousingEngineeringXp { get; set; } = 500;

        [SettingPropertyGroup("Town Housing", GroupOrder = 0)]
        [SettingPropertyInteger("Gold Gain", 0, 10000, "0", HintText = "Gold gained upon completion", Order = 0, RequireRestart = false)]
        public int TownHousingGold { get; set; } = 3500;

        [SettingPropertyGroup("Town Housing", GroupOrder = 0)]
        [SettingPropertyInteger("Prosperity Gain", 0, 100, "0", HintText = "Town prosperity gained upon completion", Order = 0, RequireRestart = false)]
        public int TownHousingProsperity { get; set; } = 25;

        [SettingPropertyGroup("Town Housing", GroupOrder = 0)]
        [SettingPropertyInteger("Notable Relations Gain", 0, 100, "0", HintText = "Relations with all notables in town gained upon completion", Order = 0, RequireRestart = false)]
        public int TownHousingRelations { get; set; } = 2;

        [SettingPropertyGroup("Militia", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Base Trainging Time", 1, 100, "0.00", HintText = "Time in hours to complete with leadership skill 100", Order = 0, RequireRestart = false)]
        public float MilitiaBaseTime { get; set; } = 6;

        [SettingPropertyGroup("Militia", GroupOrder = 0)]
        [SettingPropertyInteger("Leadership XP Gain", 0, 1000, "0", HintText = "Leadership Skill XP gained upon completion", Order = 0, RequireRestart = false)]
        public int MilitiaLeadershipXp { get; set; } = 100;

        [SettingPropertyGroup("Militia", GroupOrder = 0)]
        [SettingPropertyInteger("Gold Gain", 0, 5000, "0", HintText = "Gold gained upon completion", Order = 0, RequireRestart = false)]
        public int MilitiaGold { get; set; } = 50;

        [SettingPropertyGroup("Militia", GroupOrder = 0)]
        [SettingPropertyInteger("Notable Relations Gain", 0, 100, "0", HintText = "Relations with all notables in town gained upon completion", Order = 0, RequireRestart = false)]
        public int MilitiaRelations { get; set; } = 0;

        [SettingPropertyGroup("Town Festival", GroupOrder = 0)]
        [SettingPropertyInteger("Town Festival Time", 0, 100, "0", HintText = "Time in hours to organize a festival in town", Order = 0, RequireRestart = false)]
        public int TownFestivalBaseTime { get; set; } = 8;

        [SettingPropertyGroup("Town Festival", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Town Festival Loyalty", 0f, 100f, "0.00", HintText = "Loyalty gained from town festival", Order = 0, RequireRestart = false)]
        public int TownFestivalLoyalty { get; set; } = 5;

        [SettingPropertyGroup("Town Festival", GroupOrder = 0)]
        [SettingPropertyInteger("Town Festival Food", 0, 1000, "0", HintText = "Amount of food from inventory consumed by Festival", Order = 0, RequireRestart = false)]
        public int TownFestivalFood { get; set; } = 50;

        [SettingPropertyGroup("Town Festival", GroupOrder = 0)]
        [SettingPropertyInteger("Town Festival Charm XP", 0, 5000, "0", HintText = "Amount of charm xp gain organizing a festival", Order = 0, RequireRestart = false)]
        public int TownFestivalCharmXp { get; set; } = 500;

        [SettingPropertyGroup("Town Patrol", GroupOrder = 0)]
        [SettingPropertyInteger("Town Patrol Min Gang Member", 1, 100, "0", HintText = "Minimal party size for gang encounted while patroling", Order = 0, RequireRestart = false)]
        public int TownPatrolMinGangSize { get; set; } = 8;

        [SettingPropertyGroup("Town Patrol", GroupOrder = 0)]
        [SettingPropertyInteger("Town Patrol Max Gang Member", 1, 100, "0", HintText = "Max party size for gang encounted while patroling (must be greater or equal to min)", Order = 0, RequireRestart = false)]
        public int TownPatrolMaxGangSize { get; set; } = 20;

        [SettingPropertyGroup("Town Patrol", GroupOrder = 0)]
        [SettingPropertyInteger("Fight Security Change", 1, 100, "0", HintText = "Secutity change from winning or lossing fight with town gangs", Order = 0, RequireRestart = false)]
        public int TownPatrolFightSecurity { get; set; } = 10;

        [SettingPropertyGroup("Town Patrol", GroupOrder = 0)]
        [SettingPropertyInteger("Fight Relation", 1, 100, "0", HintText = "Amount of relation lost with the leader of the gang you defeated but gained with all other notable in the settlement", Order = 0, RequireRestart = false)]
        public int TownPatrolFightRelation { get; set; } = 5;

        [SettingPropertyGroup("Town Patrol", GroupOrder = 0)]
        [SettingPropertyInteger("Fight Gold", 1, 10000, "0", HintText = "Gold looted by defeating town gangs while on patrol", Order = 0, RequireRestart = false)]
        public int TownPatrolFightGold { get; set; } = 1000;

        [SettingPropertyGroup("Town Patrol", GroupOrder = 0)]
        [SettingPropertyInteger("Fight Leadership XP", 1, 10000, "0", HintText = "Leadership gained from defeating gangs while patroling", Order = 0, RequireRestart = false)]
        public int TownPatrolFightLeadership { get; set; } = 1000;

        [SettingPropertyGroup("Town Patrol", GroupOrder = 0)]
        [SettingPropertyInteger("Patrol Time", 1, 100, "0", HintText = "Time need foor patrol action", Order = 0, RequireRestart = false)]
        public int TownPatrolTime { get; set; } = 12;

        [SettingPropertyGroup("Town Patrol", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Gang Encounter Chance", 0, 100, "0", HintText = "Percent chance to encounter gangs while patroling", Order = 0, RequireRestart = false)]
        public float TownPatrolGangEncounterPercentage { get; set; } = 33;

        [SettingPropertyGroup("Town Patrol", GroupOrder = 0)]
        [SettingPropertyInteger("Patrol Gold", 0, 10000, "0", HintText = "Gold gained from patroling town (no gang encounter)", Order = 0, RequireRestart = false)]
        public int TownPatrolGold { get; set; } = 500;

        [SettingPropertyGroup("Town Patrol", GroupOrder = 0)]
        [SettingPropertyInteger("Patrol Leadership XP", 1, 4000, "0", HintText = "Leadership gained from patroling town (no gang encounter)", Order = 0, RequireRestart = false)]
        public int TownPatrolLeadershipXP { get; set; } = 200;

        [SettingPropertyGroup("Town Patrol", GroupOrder = 0)]
        [SettingPropertyInteger("Patrol Relations", 0, 100, "0", HintText = "Relation with notables gained gained from patroling town (no gang encounter)", Order = 0, RequireRestart = false)]
        public int TownPatrolRelations { get; set; } = 1;

        [SettingPropertyGroup("Town Patrol", GroupOrder = 0)]
        [SettingPropertyInteger("Succesful Lure Tactics XP", 0, 10000, "0", HintText = "Tactics exp gain if gang is succefully lured out of the city", Order = 0, RequireRestart = false)]
        public int TownPatrolLureTacticsXP { get; set; } = 1000;

        [SettingPropertyGroup("Village Production Rates", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Grain base rate", 0, 100, "0", HintText = "base time in hours to produce 1 grain", Order = 0, RequireRestart = false)]
        public float VillageGrainProductionTime { get; set; } = 24;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Grain Athletics XP", 0, 5000, "0", HintText = "Athletics XP from producing grain", Order = 0, RequireRestart = false)]
        public int VillageGrainAthleticsXP { get; set; } = 50;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Flax Base Rate", 0, 100, "0", HintText = "base time in hours to produce 1 flax", Order = 0, RequireRestart = false)]
        public float VillageFlaxProductionTime { get; set; } = 24;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Flax Athletics XP", 0, 5000, "0", HintText = "Athletics XP from producing Flax", Order = 0, RequireRestart = false)]
        public int VillageFlaxAthleticsXP { get; set; } = 50;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Fish Base Rate", 0, 100, "0", HintText = "base time in hours to produce 1 fish", Order = 0, RequireRestart = false)]
        public float VillageFishProductionTime { get; set; } = 24;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Fish Athletics XP", 0, 5000, "0", HintText = "Athletics XP from producing fish", Order = 0, RequireRestart = false)]
        public int VillageFishAthleticsXP { get; set; } = 50;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Dates Base Rate", 0, 100, "0", HintText = "base time in hours to produce 1 dates", Order = 0, RequireRestart = false)]
        public float VillageDatesProductionTime { get; set; } = 24;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Dates Athletics XP", 0, 5000, "0", HintText = "Athletics XP from producing dates", Order = 0, RequireRestart = false)]
        public int VillageDatesAthleticsXP { get; set; } = 50;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Olives Base Rate", 0, 100, "0", HintText = "base time in hours to produce 1 olive", Order = 0, RequireRestart = false)]
        public float VillageOlivesProductionTime { get; set; } = 24;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Olives Athletics XP", 0, 5000, "0", HintText = "Athletics XP from producing olives", Order = 0, RequireRestart = false)]
        public int VillageOlivesAthleticsXP { get; set; } = 50;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Cotton Base Rate", 0, 100, "0", HintText = "base time in hours to produce 1 cotton", Order = 0, RequireRestart = false)]
        public float VillageCottonProductionTime { get; set; } = 24;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Cotton Athletics XP", 0, 5000, "0", HintText = "Athletics XP from producing cotton", Order = 0, RequireRestart = false)]
        public int VillageCottonAthleticsXP { get; set; } = 50;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Fur Base Rate", 0, 100, "0", HintText = "base time in hours to produce 1 fur", Order = 0, RequireRestart = false)]
        public float VillageFurProductionTime { get; set; } = 24;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Fur Athletics XP", 0, 5000, "0", HintText = "Athletics XP from producing fur", Order = 0, RequireRestart = false)]
        public int VillageFurAthleticsXP { get; set; } = 50;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Wood Base Rate", 0, 100, "0", HintText = "base time in hours to produce 1 hardwood", Order = 0, RequireRestart = false)]
        public float VillageWoodProductionTime { get; set; } = 24;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Wood Athletics XP", 0, 5000, "0", HintText = "Athletics XP from producing hardwood", Order = 0, RequireRestart = false)]
        public int VillageWoodAthleticsXP { get; set; } = 50;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Clay Base Rate", 0, 100, "0", HintText = "base time in hours to produce 1 clay", Order = 0, RequireRestart = false)]
        public float VillageClayProductionTime { get; set; } = 24;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Clay Athletics XP", 0, 5000, "0", HintText = "Athletics XP from producing clay", Order = 0, RequireRestart = false)]
        public int VillageClayAthleticsXP { get; set; } = 50;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Salt Base Rate", 0, 100, "0", HintText = "base time in hours to produce 1 salt", Order = 0, RequireRestart = false)]
        public float VillageSaltProductionTime { get; set; } = 24;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Salt Athletics XP", 0, 5000, "0", HintText = "Athletics XP from producing salt", Order = 0, RequireRestart = false)]
        public int VillageSaltAthleticsXP { get; set; } = 50;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Iron Base Rate", 0, 100, "0", HintText = "base time in hours to produce 1 iron", Order = 0, RequireRestart = false)]
        public float VillageIronProductionTime { get; set; } = 24;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Iron Athletics XP", 0, 5000, "0", HintText = "Athletics XP from producing iron", Order = 0, RequireRestart = false)]
        public int VillageIronAthleticsXP { get; set; } = 50;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Grape Base Rate", 0, 100, "0", HintText = "base time in hours to produce 1 grape", Order = 0, RequireRestart = false)]
        public float VillageGrapeProductionTime { get; set; } = 24;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Grape Athletics XP", 0, 5000, "0", HintText = "Athletics XP from producing grape", Order = 0, RequireRestart = false)]
        public int VillageGrapeAthleticsXP { get; set; } = 50;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyInteger("Tool Break Chance", 0, 100, "0", HintText = "Chance in percent to break a tool while working in a village that produces raw materials", Order = 0, RequireRestart = false)]
        public int ToolBreakChance { get; set; } = 10;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Notable Power Gain Per Unit Produced", 0f, 1f, "0.000", HintText = "power gained by notable from player producing goods in that village", Order = 0, RequireRestart = false)]
        public float VillageNotablePowerGainPerGoodProduced { get; set; } = 0.01f;

        [SettingPropertyGroup("Village Production", GroupOrder = 0)]
        [SettingPropertyInteger("Notable Power Gain Per Unit Produced", 0, 100, "0", HintText = "gold paid per unit of goods produced", Order = 0, RequireRestart = false)]
        public int VillageGoldPerUnitPayment { get; set; } = 5;

        [SettingPropertyGroup("Workship Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Beer Trade XP", 0, 5000, "0", HintText = "Trade XP from producing beer", Order = 0, RequireRestart = false)]
        public int WorkshopBeerTradeXP { get; set; } = 50;

        [SettingPropertyGroup("Workship Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Beer Base Rate", 0, 1000, "0", HintText = "Base time in hours to produce 1 beer", Order = 0, RequireRestart = false)]
        public int WorkshopBeerBaseTime { get; set; } = 24;

        [SettingPropertyGroup("Workship Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Velvet Trade XP", 0, 5000, "0", HintText = "Trade XP from producing velvet", Order = 0, RequireRestart = false)]
        public int WorkshopVelvetTradeXP { get; set; } = 200;

        [SettingPropertyGroup("Workship Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Velvet Base Rate", 0, 1000, "0", HintText = "Base time in hours to produce 1 velvet", Order = 0, RequireRestart = false)]
        public int WorkshopVelvetBaseTime { get; set; } = 96;

        [SettingPropertyGroup("Workship Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Pottery Trade XP", 0, 5000, "0", HintText = "Trade XP from producing pottery", Order = 0, RequireRestart = false)]
        public int WorkshopPotteryTradeXP { get; set; } = 100;

        [SettingPropertyGroup("Workship Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Pottery Base Rate", 0, 1000, "0", HintText = "Base time in hours to produce 1 pottery", Order = 0, RequireRestart = false)]
        public int WorkshopPotteryBaseTime { get; set; } = 48;

        [SettingPropertyGroup("Workship Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Leather Trade XP", 0, 5000, "0", HintText = "Trade XP from producing leather", Order = 0, RequireRestart = false)]
        public int WorkshopLeatherTradeXP { get; set; } = 200;

        [SettingPropertyGroup("Workship Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Leather Base Rate", 0, 1000, "0", HintText = "Base time in hours to produce 1 leather", Order = 0, RequireRestart = false)]
        public int WorkshopLeatherBaseTime { get; set; } = 96;

        [SettingPropertyGroup("Workship Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Linen Trade XP", 0, 5000, "0", HintText = "Trade XP from producing linen", Order = 0, RequireRestart = false)]
        public int WorkshopLinenTradeXP { get; set; } = 100;

        [SettingPropertyGroup("Workship Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Linen Base Rate", 0, 1000, "0", HintText = "Base time in hours to produce 1 linen", Order = 0, RequireRestart = false)]
        public int WorkshopLinenBaseTime { get; set; } = 48;

        [SettingPropertyGroup("Workship Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Wine Trade XP", 0, 5000, "0", HintText = "Trade XP from producing wine", Order = 0, RequireRestart = false)]
        public int WorkshopWineTradeXP { get; set; } = 100;

        [SettingPropertyGroup("Workship Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Wine Base Rate", 0, 1000, "0", HintText = "Base time in hours to produce 1 wine", Order = 0, RequireRestart = false)]
        public int WorkshopWineBaseTime { get; set; } = 48;

        [SettingPropertyGroup("Workship Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Oil Trade XP", 0, 5000, "0", HintText = "Trade XP from producing oil", Order = 0, RequireRestart = false)]
        public int WorkshopOilTradeXP { get; set; } = 100;

        [SettingPropertyGroup("Workship Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Oil Base Rate", 0, 1000, "0", HintText = "Base time in hours to produce 1 oil", Order = 0, RequireRestart = false)]
        public int WorkshopOilBaseTime { get; set; } = 48;

        [SettingPropertyGroup("Workship Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Jewelry Trade XP", 0, 5000, "0", HintText = "Trade XP from producing jewelry", Order = 0, RequireRestart = false)]
        public int WorkshopJewelryTradeXP { get; set; } = 400;

        [SettingPropertyGroup("Workship Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Jewelry Base Rate", 0, 1000, "0", HintText = "Base time in hours to produce 1 jewelry", Order = 0, RequireRestart = false)]
        public int WorkshopJewelryBaseTime { get; set; } = 192;

        [SettingPropertyGroup("Workship Production", GroupOrder = 0)]
        [SettingPropertyInteger("Produce Tool Trade XP", 0, 5000, "0", HintText = "Trade XP from producing tool", Order = 0, RequireRestart = false)]
        public int WorkshopToolTradeXP { get; set; } = 67;

        [SettingPropertyGroup("Workship Production", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Produce Tool Base Rate", 0, 1000, "0", HintText = "Base time in hours to produce 1 tool", Order = 0, RequireRestart = false)]
        public int WorkshopToolBaseTime { get; set; } = 32;

        [SettingPropertyGroup("Actions", GroupOrder = 0)]
        [SettingPropertyFloatingInteger("Minimal Build time", 0, 50, "0.00", HintText = "Lower bound in hours for how fast a building project can be completed", Order = 0, RequireRestart = false)]
        public float BuildProjectMinTime { get; set; } = 1;

        [SettingPropertyGroup("Actions", GroupOrder = 0)]
        [SettingPropertyInteger("Max Gold Lose From Scripted Battles", 1, 50000, "0", HintText = "Most gold that can be lost from lossing a battle relating to this mod", Order = 0, RequireRestart = false)]
        public int MaxGoldFromLosingScriptBattle { get; set; } = 6;

        [SettingPropertyBool("Slavery", HintText = "Allows player to own slave estates", Order = 1, RequireRestart = true)]
        [SettingPropertyGroup("Slavery", IsMainToggle = true)]
        public bool SlaveEstates { get; set; } = true;

        [SettingPropertyGroup("Slavery", GroupOrder = 0)]
        [SettingPropertyInteger("Slave Daily Death Rate", 0, 100, "0", HintText = "Chance for slaves working your estate to be worked to death each day", Order = 0, RequireRestart = false)]
        public int SlaveDailyDeathRate { get; set; } = 8;

        [SettingPropertyGroup("Slavery", GroupOrder = 0)]
        [SettingPropertyInteger("Slave Estate Buy Cost", 0, 100000, "0", HintText = "", Order = 0, RequireRestart = false)]
        public int SlaveEstateCost { get; set; } = 10000;

        [SettingPropertyGroup("Slavery", GroupOrder = 0)]
        [SettingPropertyInteger("Slave Estate Trade XP", 0, 100000, "0", HintText = "Trade XP gain per unit of good produced by your slave estates", Order = 0, RequireRestart = false)]
        public int SlaveEstateTradeXP { get; set; } = 10;

        [SettingPropertyBool("Form Parties", HintText = "Idle heroes in your clan will automatically form parties to lead.  The party will be formed in your clan's home town", Order = 1, RequireRestart = false)]
        public bool FormParties { get; set; } = false;

        [SettingPropertyBool("Dynamic Bandit Density", HintText = "Settlement with low security will increase the global looter party cap.  Largers Towns (higher prosperity) will provide a bigger increase to the cap than smaller towns", Order = 1, RequireRestart = false)]
        public bool DyanmicBanditDensity { get; set; } = true;
    }
}