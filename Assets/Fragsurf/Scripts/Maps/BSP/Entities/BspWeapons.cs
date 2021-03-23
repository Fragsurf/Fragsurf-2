using Fragsurf.Actors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.BSP
{
    [EntityComponent("weapon_*")]
    public class BspWeapons : BspEntityMonoBehaviour
    {

        protected override void OnStart()
        {
            base.OnStart();

            var wpnName = Entity.ClassName.Replace("weapon_", "").ToLower();
            if(GetItem(wpnName, out FSMPickup.ItemNames item))
            {
                var pickup = gameObject.AddComponent<FSMPickup>();
                pickup.Quantity = 1;
                pickup.GiveOnTouch = true;
                pickup.Item = item;
            }
        }

        private bool GetItem(string csWeaponName, out FSMPickup.ItemNames item)
        {
            item = FSMPickup.ItemNames.R870;

            switch (csWeaponName)
            {
                // nades
                case "hegrenade":
                case "smokegrenade":
                case "flashbang":
                    return false;


                // items
                case "c4":
                    return false;


                // pistol
                case "deagle":
                    item = FSMPickup.ItemNames.Revolver;
                    break;

                case "elite":
                case "fiveseven":
                case "glock":
                case "p228":
                case "usp":
                    item = FSMPickup.ItemNames.M1911;
                    break;


                // sniper
                case "scout":
                case "sg550":
                case "sg552":
                case "awp":
                    item = FSMPickup.ItemNames.Bolty;
                    break;


                // rifle
                case "ak47":
                case "g3sg1":
                case "m249":
                case "m4a1":
                case "aug":
                    item = FSMPickup.ItemNames.AK47;
                    break;


                case "mac10":
                case "mp5navy":
                case "p90":
                case "ump45":
                case "tmp":
                    item = FSMPickup.ItemNames.MP5;
                    break;


                case "galil":
                case "famas":
                    item = FSMPickup.ItemNames.Famas;
                    break;


                // shotgun
                case "m3":
                case "nova":
                case "xm1014":
                    item = FSMPickup.ItemNames.R870;
                    break;


                // melee
                case "knife":
                    item = FSMPickup.ItemNames.Knife;
                    break;
            }

            return true;
        }

    }
}

