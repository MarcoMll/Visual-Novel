using System;
using UnityEngine;

namespace VisualNovel.Minigames.Combat
{
    public class PlayerStatsController : MonoBehaviour
    {
        private int _attackActionPoints;
        private int _defenceActionPoints;
        private int _restActionPoints;

        private int _totalLeftActionPoints;
        private FighterRuntime _playerRuntime;

        public Action onActionPointAssigned;
        public int totalLeftActionPoints => _totalLeftActionPoints;
        public int actionPointsPerRound => _playerRuntime.ActionPointsPerRound;
        
        public int BaseDamage => _playerRuntime?.BaseStats.baseDamage ?? 0;

        public void Initialize(FighterRuntime playerRuntime)
        {
            _playerRuntime = playerRuntime;
            _totalLeftActionPoints = _playerRuntime.ActionPointsPerRound;
            _attackActionPoints = 0;
            _defenceActionPoints = 0;
            _restActionPoints = 0;
        }
        
        public void AddAttackActionPoint()
        {
            if (_totalLeftActionPoints <= 0) return;
            _attackActionPoints++;
            _totalLeftActionPoints--;
            
            onActionPointAssigned?.Invoke();
        }

        public void RemoveAttackActionPoint()
        {
            if (_attackActionPoints <= 0) return;
            _attackActionPoints--;
            _totalLeftActionPoints++;
            
            onActionPointAssigned?.Invoke();
        }
        
        public void AddDefenceActionPoint()
        {
            if (_totalLeftActionPoints <= 0) return;
            _defenceActionPoints++;
            _totalLeftActionPoints--;
            
            onActionPointAssigned?.Invoke();
        }

        public void RemoveDefenceActionPoint()
        {
            if (_defenceActionPoints <= 0) return;
            _defenceActionPoints--;
            _totalLeftActionPoints++;
            
            onActionPointAssigned?.Invoke();
        }
        
        public void AddRestActionPoint()
        {
            if (_totalLeftActionPoints <= 0) return;
            _restActionPoints++;
            _totalLeftActionPoints--;
            
            onActionPointAssigned?.Invoke();
        }

        public void RemoveRestActionPoint()
        {
            if (_restActionPoints <= 0) return;
            _restActionPoints--;
            _totalLeftActionPoints++;
            
            onActionPointAssigned?.Invoke();
        }
        
        public bool AllActionPointsDistributed()
        {
            return _totalLeftActionPoints == 0;
        }

        public void ResetPoints()
        {
            _totalLeftActionPoints = _playerRuntime.ActionPointsPerRound;

            _attackActionPoints = 0;
            _defenceActionPoints = 0;
            _restActionPoints = 0;
        }
        
        public void GetDistributedActionPoints(out int attackPoints, out int defencePoints, out int restPoints)
        {
            attackPoints = _attackActionPoints;
            defencePoints = _defenceActionPoints;
            restPoints = _restActionPoints;
        }
    }
}