using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using MCM.Abstractions.Settings.Base.Global;
using Helpers;
using TaleWorlds.Library;
using SandBox;
using SandBox.Source.Missions.Handlers;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;

namespace MoreSettlementAction
{
    public static class MoreSettlementActionHelper
    {
        private static Random rng = new Random();

        public static int GetSkillTotalForParty(SkillObject skill)
        {
            int Sum = Hero.MainHero.GetSkillValue(skill);
            if (MobileParty.MainParty.Army != null)
            {
                foreach (MobileParty party in MobileParty.MainParty.Army.Parties)
                {
                    foreach (CharacterObject troop in party.MemberRoster.Troops)
                    {
                        if (troop.IsHero)
                        {
                            Sum += troop.HeroObject.GetSkillValue(skill);
                        }
                    }
                }
            }
            else
            {
                foreach (CharacterObject troop in MobileParty.MainParty.MemberRoster.Troops)
                {
                    if (troop.IsHero)
                    {
                        Sum += troop.HeroObject.GetSkillValue(skill);
                    }
                }
            }
            return Sum;
        }

        public static void SkillXPToAllPartyHeroes (SkillObject skill, int amount, bool IncluedMainHero)
        {
            if (MobileParty.MainParty.Army != null)
            {
                foreach (MobileParty party in MobileParty.MainParty.Army.Parties)
                {
                    foreach (CharacterObject troop in party.MemberRoster.Troops)
                    {
                        if (troop.IsHero && (troop.HeroObject != Hero.MainHero || IncluedMainHero))
                        {
                            troop.HeroObject.AddSkillXp(skill, amount);
                        }
                    }
                }
            }
            else
            {
                foreach (var troop in MobileParty.MainParty.MemberRoster.Troops)
                {
                    if (troop.IsHero && (troop.HeroObject != Hero.MainHero || IncluedMainHero))
                    {
                        troop.HeroObject.AddSkillXp(skill, amount);
                    }
                }
            }
        }

        public static void GetCreditForHelping(int relationChange)
        {
            foreach (Hero notable in Settlement.CurrentSettlement.Notables)
            {
                ChangeRelationAction.ApplyPlayerRelation(notable, relationChange);
                notable.AddPower(relationChange);
            }
        }

        public static int GetFoodAmount(PartyBase party)
        {
            int amount = 0;
            ItemRoster itemRoster = party.ItemRoster;
            for (int i = itemRoster.Count - 1; i >= 0; i--)
            {
                if (itemRoster[i].EquipmentElement.Item.IsFood)
                {
                    amount += itemRoster[i].Amount;
                }
            }
            return amount;
        }

        public static void ConsumeFood(int amount, PartyBase party)
        {
            if (amount <= 0)
            {
                return;
            }
            else
            {
                ItemRoster itemRoster = party.ItemRoster;
                int FoodAmount = MoreSettlementActionHelper.GetFoodAmount(PartyBase.MainParty);
                int number = rng.Next(1, FoodAmount);
                int count = 0;
                for (int i = itemRoster.Count - 1; i >= 0; i--)
                {
                    if (itemRoster[i].EquipmentElement.Item.IsFood)
                    {
                        count += itemRoster[i].Amount;
                        if (number <= count)
                        {
                            itemRoster.AddToCounts(itemRoster[i].EquipmentElement.Item, -1);
                            break;
                        }
                    }
                }
                ConsumeFood(amount - 1, party);
            }
        }

        public static bool TownHasWorkshop(Town town, string workshopId)
        {
            
            Workshop[] workshops = town.Workshops;
            for (int i = 0; i < workshops.Length; i++)
            {
                if (workshops[i].WorkshopType == WorkshopType.Find(workshopId))
                {
                    return true;
                }
            }
            return false;
        }

        public static int OwnsWorkshop(Town town, string workshopId)
        {
            int ret = 0;
            Workshop[] workshops = town.Workshops;
            for (int i = 0; i < workshops.Length; i++)
            {
                if (workshops[i].WorkshopType == WorkshopType.Find(workshopId) && workshops[i].Owner == Hero.MainHero)
                {
                    ret++;
                }
            }
            return ret;
        }

        public static int OwnsAnyWorkshop(Town town)
        {
            int ret = 0;
            Workshop[] workshops = town.Workshops;
            for (int i = 0; i < workshops.Length; i++)
            {
                if (workshops[i].Owner == Hero.MainHero)
                {
                    ret++;
                }
            }
            return ret;
        }

        public static Mission OpenCustomBattleMission(string scene, int PlayerTroopLimit, int AITroopLimit)
        {
            bool isPlayerSergeant = MobileParty.MainParty.MapEvent.IsPlayerSergeant();
            bool isPlayerInArmy = MobileParty.MainParty.Army != null;
            List<string> heroesOnPlayerSideByPriority = HeroHelper.OrderHeroesOnPlayerSideByPriority();
            return MissionState.OpenNew("Battle", SandBoxMissions.CreateSandBoxMissionInitializerRecord(scene), (InitializeMissionBehvaioursDelegate)(mission => (IEnumerable<MissionBehaviour>)new MissionBehaviour[26]
           {
        (MissionBehaviour) new MissionOptionsComponent(),
        (MissionBehaviour) new CampaignMissionComponent(),
        (MissionBehaviour) new BattleEndLogic(),
        (MissionBehaviour) new MissionCombatantsLogic((IEnumerable<IBattleCombatant>) MobileParty.MainParty.MapEvent.InvolvedParties, (IBattleCombatant) PartyBase.MainParty, (IBattleCombatant) MobileParty.MainParty.MapEvent.GetLeaderParty(BattleSideEnum.Defender), (IBattleCombatant) MobileParty.MainParty.MapEvent.GetLeaderParty(BattleSideEnum.Attacker), Mission.MissionTeamAITypeEnum.FieldBattle, false),
        (MissionBehaviour) new MissionDefaultCaptainAssignmentLogic(),
        (MissionBehaviour) new BattleMissionStarterLogic(),
        (MissionBehaviour) new BattleSpawnLogic("battle_set"),
        (MissionBehaviour) new AgentBattleAILogic(),
        (MissionBehaviour) new MissionAgentSpawnLogic(new IMissionTroopSupplier[2]
    {
      (IMissionTroopSupplier) new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, BattleSideEnum.Defender),
      (IMissionTroopSupplier) new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, BattleSideEnum.Attacker)
    }, PartyBase.MainParty.Side),
        (MissionBehaviour) new CustomMissionTroopSpawnHandler(PlayerTroopLimit, AITroopLimit, false),
        (MissionBehaviour) new AgentFadeOutLogic(),
        (MissionBehaviour) new BattleObserverMissionLogic(),
        (MissionBehaviour) new BattleAgentLogic(),
        (MissionBehaviour) new AgentVictoryLogic(),
        (MissionBehaviour) new MissionDebugHandler(),
        (MissionBehaviour) new MissionAgentPanicHandler(),
        (MissionBehaviour) new MissionHardBorderPlacer(),
        (MissionBehaviour) new MissionBoundaryPlacer(),
        (MissionBehaviour) new MissionBoundaryCrossingHandler(),
        (MissionBehaviour) new BattleMissionAgentInteractionLogic(),
        (MissionBehaviour) new FieldBattleController(),
        (MissionBehaviour) new AgentMoraleInteractionLogic(),
        (MissionBehaviour) new HighlightsController(),
        (MissionBehaviour) new BattleHighlightsController(),
        (MissionBehaviour) new AssignPlayerRoleInTeamMissionController(!isPlayerSergeant, isPlayerSergeant, isPlayerInArmy, heroesOnPlayerSideByPriority),
        (MissionBehaviour) new CreateBodyguardMissionBehavior(MapEvent.PlayerMapEvent.AttackerSide.LeaderParty.LeaderHero?.Name.ToString(), MapEvent.PlayerMapEvent.DefenderSide.LeaderParty.LeaderHero?.Name.ToString())
           }));
        }
    }
}
