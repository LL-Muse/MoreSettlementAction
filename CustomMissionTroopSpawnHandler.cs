using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using SandBox;
using TaleWorlds.DotNet;
using TaleWorlds.CampaignSystem.CharacterDevelopment.Managers;
using TaleWorlds.Engine;
using SandBox.TournamentMissions.Missions;
using System;
using System.Collections.Generic;
using System.Linq;


namespace MoreSettlementAction
{
    internal class CustomMissionTroopSpawnHandler : MissionLogic
    {
        private int PlayerTroopLimit;
        private int AITroopLimit;
        private bool UseHorses;
        private List<MatrixFrame> _initialSpawnFrames;

        private MissionAgentSpawnLogic _missionAgentSpawnLogic;
        private MapEvent _mapEvent;

        public CustomMissionTroopSpawnHandler(int playerTroopLimit, int aiTroopLimit, bool useHorses)
        {
            PlayerTroopLimit = playerTroopLimit;
            AITroopLimit = aiTroopLimit;
            UseHorses = useHorses;
        }

        public override void OnBehaviourInitialize()
        {
            base.OnBehaviourInitialize();
            this._missionAgentSpawnLogic = Mission.GetMissionBehaviour<MissionAgentSpawnLogic>();
            this._mapEvent = MapEvent.PlayerMapEvent;
        }


        public override void AfterStart()
        {
            int defenderTotalSpawn;
            int attackerTotalSpawn;
            if (_mapEvent.PlayerSide == BattleSideEnum.Attacker)
            {
                defenderTotalSpawn = AITroopLimit;
                attackerTotalSpawn = PlayerTroopLimit;
            }
            else
            {
                defenderTotalSpawn = PlayerTroopLimit;
                attackerTotalSpawn = AITroopLimit;
            }

            int defenderInitialSpawn = defenderTotalSpawn;
            int attackerInitialSpawn = attackerTotalSpawn;
            _missionAgentSpawnLogic.SetSpawnHorses(BattleSideEnum.Defender, UseHorses);
            _missionAgentSpawnLogic.SetSpawnHorses(BattleSideEnum.Attacker, UseHorses);
            _missionAgentSpawnLogic.InitWithSinglePhase(defenderTotalSpawn, attackerTotalSpawn, defenderInitialSpawn, attackerInitialSpawn, true, true);
        }
    }
}