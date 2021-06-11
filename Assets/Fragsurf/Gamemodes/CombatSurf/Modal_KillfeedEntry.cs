using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using Fragsurf.UI;
using Fragsurf.Utility;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fragsurf.Gamemodes.CombatSurf
{
    public class Modal_KillfeedEntry : EntryElement<Modal_KillfeedEntry.Data>
    {

        [SerializeField]
        private TMP_Text _text;
        [SerializeField]
        private float _expiresIn = 8f;

        private float _expireTimer;

        public class Data
        {
            public DamageInfo DamageInfo;
        }

        public override void LoadData(Data data)
        {
            _expireTimer = _expiresIn;

            var cl = FSGameLoop.GetGameInstance(false);
            if (!cl)
            {
                return;
            }

            var killerHu = cl.EntityManager.FindEntity<Human>(data.DamageInfo.AttackerEntityId);
            var victimHu = cl.EntityManager.FindEntity<Human>(data.DamageInfo.VictimEntityId);
            var wpn = cl.EntityManager.FindEntity<Equippable>(data.DamageInfo.WeaponId);

            var killerName = string.Empty;
            var victimName = string.Empty;
            var wpnName = string.Empty;

            if(killerHu != null)
            {
                var player = cl.PlayerManager.FindPlayer(killerHu.OwnerId);
                if(player != null)
                {
                    killerName = $"<color={PlayerManager.GetTeamColor(player.Team).HashRGBA()}>{player.DisplayName}</color>";
                }
            }

            if (victimHu != null)
            {
                var player = cl.PlayerManager.FindPlayer(victimHu.OwnerId);
                if (player != null)
                {
                    victimName = $"<color={PlayerManager.GetTeamColor(player.Team).HashRGBA()}>{player.DisplayName}</color>";
                }
            }

            if(wpn != null && wpn.EquippableGameObject)
            {
                wpnName = wpn.EquippableGameObject.Data.Name;
            }

            _text.text = $"{killerName} killed {victimName}";
            //_text.text = $"<size=12>(<color=yellow>{wpnName} -> {data.DamageInfo.HitArea}</color>)</size> {killerName} killed {victimName}";
        }

        private void Update()
        {
            _expireTimer -= Time.deltaTime;
            if(_expireTimer <= 0)
            {
                _parent.Remove(this);
            }
        }

    }
}

