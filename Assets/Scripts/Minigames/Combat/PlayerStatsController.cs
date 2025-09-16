using System;
using UnityEngine;

namespace VisualNovel.Minigames.Combat
{
    public enum ActionPointType
    {
        Attack,
        Defence,
        Rest
    }

    public class PlayerStatsController : MonoBehaviour
    {
        private int _attackActionPoints;
        private int _defenceActionPoints;
        private int _restActionPoints;

        private int _totalLeftActionPoints;
        private FighterRuntime _playerRuntime;

        public Action onActionPointAssigned;
        public Action onStatsChanged;
        public int totalLeftActionPoints => _totalLeftActionPoints;
        public int actionPointsPerRound => _playerRuntime?.ActionPointsPerRound ?? 0;

        public int BaseDamage => _playerRuntime?.BaseDamage ?? 0;

        public void Initialize(FighterRuntime playerRuntime)
        {
            if (_playerRuntime != null)
            {
                _playerRuntime.OnCombatModifierChanged -= HandleRuntimeModifierChanged;
            }

            _playerRuntime = playerRuntime;
            if (_playerRuntime != null)
            {
                _playerRuntime.OnCombatModifierChanged += HandleRuntimeModifierChanged;
                _totalLeftActionPoints = _playerRuntime.ActionPointsPerRound;
            }
            else
            {
                _totalLeftActionPoints = 0;
            }
            _attackActionPoints = 0;
            _defenceActionPoints = 0;
            _restActionPoints = 0;

            onStatsChanged?.Invoke();
        }
        
        public void ModifyActionPoint(ActionPointType type, int delta)
        {
            if (delta == 0) return;

            ref int points = ref _attackActionPoints;
            switch (type)
            {
                case ActionPointType.Attack:
                    points = ref _attackActionPoints;
                    break;
                case ActionPointType.Defence:
                    points = ref _defenceActionPoints;
                    break;
                case ActionPointType.Rest:
                    points = ref _restActionPoints;
                    break;
                default:
                    return;
            }

            if (delta > 0)
            {
                if (_totalLeftActionPoints < delta) return;
                points += delta;
                _totalLeftActionPoints -= delta;
            }
            else // delta < 0
            {
                if (points < -delta) return;
                points += delta;
                _totalLeftActionPoints -= delta; // delta is negative
            }

            onActionPointAssigned?.Invoke();
        }
        
        public bool AllActionPointsDistributed()
        {
            return _totalLeftActionPoints == 0;
        }

        public void ResetPoints()
        {
            _totalLeftActionPoints = _playerRuntime?.ActionPointsPerRound ?? 0;

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

        private void HandleRuntimeModifierChanged(CombatModifierType type)
        {
            onStatsChanged?.Invoke();
        }

        private void OnDestroy()
        {
            if (_playerRuntime != null)
            {
                _playerRuntime.OnCombatModifierChanged -= HandleRuntimeModifierChanged;
            }
        }
    }
}