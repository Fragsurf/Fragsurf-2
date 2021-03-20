using Fragsurf.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared
{
    public class ShotgunEquippable : GunEquippable
    {

        protected override bool CustomReload => ShotgunData.Pump;

        public ShotgunEquippableData ShotgunData => Data as ShotgunEquippableData;

        protected override void Fire()
        {
            base.FireEffects();

            if (ShotgunData.Pump)
            {
                StartCoroutine(Pump());
            }

            var hu = Equippable.Human;
            var huRay = hu.GetEyeRay();
            var right = Quaternion.Euler(hu.Angles) * Vector3.right;
            var up = Quaternion.Euler(hu.Angles) * Vector3.up;
            foreach (var pelletRay in CalculateRays(huRay.origin, huRay.direction, up, right, Equippable.Random))
            {
                if (TraceNearestHit(pelletRay, GunData.BulletRadius, GunData.BulletRange, out RaycastHit hit))
                {
                    ProcessHit(hit);
                }
            }
        }

        protected override IEnumerator ReloadOverride()
        {
            AudioSource.PlayClip(ShotgunData.BeginReloadSound);
            ViewModel.PlayAnimation("BeginReload");
            yield return new WaitForSeconds(.5f);
            while (RoundsInClip < ShotgunData.RoundsPerClip
                && ExtraRounds > 0)
            {
                AudioSource.PlayClip(ShotgunData.InsertShellSound);
                ViewModel.PlayAnimation("InsertShell", 0.01f);
                yield return new WaitForSeconds(.7f);
                RoundsInClip++;
                ExtraRounds--;
            }
            ViewModel.PlayAnimation("EndReload");
            yield return new WaitForSeconds(.3f);
            StartCoroutine(Pump());
        }

        private IEnumerator Pump()
        {
            yield return new WaitForSeconds(ShotgunData.PumpDelay);
            ViewModel.PlayAnimation("Pump");
            AudioSource.PlayClip(ShotgunData.PumpSound);
            yield return new WaitForSeconds(ShotgunData.PumpTime);
        }

        private IEnumerable<Ray> CalculateRays(Vector3 origin, Vector3 direction, Vector3 up, Vector3 right, StateRandom rnd = null)
        {
            if (rnd == null)
            {
                rnd = new StateRandom(0);
            }

            for (int i = 0; i < ShotgunData.PelletCount; i++)
            {
                var rad = 360f * rnd.Next(0, 100) / 100f;
                var spreadX = (ShotgunData.SpreadMin + (ShotgunData.SpreadMax - ShotgunData.SpreadMin) * rnd.NextDouble()) * Mathf.Cos(rad);
                var spreadY = (ShotgunData.SpreadMin + (ShotgunData.SpreadMax - ShotgunData.SpreadMin) * rnd.NextDouble()) * Mathf.Sin(rad);
                var newDirection = direction;
                newDirection += up * (float)spreadY;
                newDirection += right * (float)spreadX;
                yield return new Ray(origin, newDirection);
            }
        }

    }
}

