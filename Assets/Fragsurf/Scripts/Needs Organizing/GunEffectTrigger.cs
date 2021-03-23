using System;
using UnityEngine;
using System.Collections;
using Fragsurf.Client;
using Fragsurf.Shared.Entity;

namespace Fragsurf.Misc
{
    public class GunEffectTrigger : MonoBehaviour
    {

        [SerializeField]
        private VisualEffectsSettings m_ParticleEffects = new VisualEffectsSettings();
        [SerializeField]
        private CasingEjectionSettings m_CasingEjection = null;
        private WaitForSeconds m_CasingSpawnDelay;
        private float m_LastTimeSpawningVfx = 0f;
        private float _tracerTimer;
        private ParticleSystem _muzzleFlash;

        private void Start()
        {
            m_CasingSpawnDelay = new WaitForSeconds(m_CasingEjection.SpawnDelay);
        }

        private void Update()
        {
            if (_tracerTimer > 0)
            {
                _tracerTimer -= Time.deltaTime;
            }
        }

        public void Trigger(EquippableGameObject equippable, RaycastHit hit)
        {
            if (m_ParticleEffects.Parent == null || !isActiveAndEnabled)
            {
                return;
            }

            if (!_muzzleFlash && m_ParticleEffects.MuzzleFlash)
            {
                _muzzleFlash = GameObject.Instantiate(m_ParticleEffects.MuzzleFlash).GetComponent<ParticleSystem>();
                _muzzleFlash.transform.SetParent(m_ParticleEffects.Parent, true);
                _muzzleFlash.transform.localPosition = Vector3.zero;
                _muzzleFlash.transform.localRotation = Quaternion.identity;
                _muzzleFlash.transform.localScale = Vector3.one;
                _muzzleFlash.Stop();
            }

            if (m_ParticleEffects.Tracer != null && _tracerTimer <= 0)
            {
                _tracerTimer = UnityEngine.Random.Range(.25f, 1.5f);
                var tracer = equippable.Entity.Game.Pool.Get(m_ParticleEffects.Tracer, 1.5f);
                tracer.transform.position = m_ParticleEffects.Parent.position + m_ParticleEffects.Parent.TransformVector(m_ParticleEffects.TracerOffset);
                tracer.transform.rotation = Quaternion.LookRotation(hit.point - m_ParticleEffects.Parent.transform.position);
            }

            if (Time.time < m_LastTimeSpawningVfx)
            {
                return;
            }

            if (_muzzleFlash)
            {
                _muzzleFlash.transform.localPosition = Vector3.zero;

                var randomMuzzleFlashRot = m_ParticleEffects.MuzzleFlashRandomRot;

                randomMuzzleFlashRot = new Vector3(
                    UnityEngine.Random.Range(-randomMuzzleFlashRot.x, randomMuzzleFlashRot.x),
                    UnityEngine.Random.Range(-randomMuzzleFlashRot.y, randomMuzzleFlashRot.y),
                    UnityEngine.Random.Range(-randomMuzzleFlashRot.z, randomMuzzleFlashRot.z));

                _muzzleFlash.transform.localRotation = Quaternion.Euler(randomMuzzleFlashRot);

                float randomMuzzleFlashScale = UnityEngine.Random.Range(m_ParticleEffects.MuzzleFlashRandomScale.x, m_ParticleEffects.MuzzleFlashRandomScale.y);

                _muzzleFlash.transform.localScale = Vector3.one * randomMuzzleFlashScale;
                _muzzleFlash.Play();
            }

            if (m_ParticleEffects.Light != null)
            {
                m_ParticleEffects.Light.Play(false);
            }

            // Spawn the shell if a prefab is assigned
            if (m_CasingEjection.CasingPrefab != null
                && m_CasingEjection.Parent != null)
            {
                StartCoroutine(C_SpawnCasing(equippable));
            }

            m_LastTimeSpawningVfx = Time.time + 0.05f;
        }

        private IEnumerator C_SpawnCasing(EquippableGameObject equippable)
        {
            yield return m_CasingSpawnDelay;

            var casing = equippable.Entity.Game.Pool.Get(m_CasingEjection.CasingPrefab, .5f);
            casing.transform.position = m_CasingEjection.Parent.position + /*Player.Aim.Active ? Vector3.zero : */m_CasingEjection.Parent.TransformVector(m_CasingEjection.SpawnOffset); ;
            casing.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(-30, 30), UnityEngine.Random.Range(-30, 30), UnityEngine.Random.Range(-30, 30));
            casing.transform.localScale = new Vector3(m_CasingEjection.CasingScale, m_CasingEjection.CasingScale, m_CasingEjection.CasingScale);

            if (casing.TryGetComponent(out Rigidbody rb))
            {
                rb.velocity = transform.TransformVector(new Vector3(m_CasingEjection.SpawnVelocity.x * UnityEngine.Random.Range(0.75f, 1.15f), m_CasingEjection.SpawnVelocity.y * UnityEngine.Random.Range(0.9f, 1.1f), m_CasingEjection.SpawnVelocity.z * UnityEngine.Random.Range(0.85f, 1.15f)));
                rb.maxAngularVelocity = 10000f;
                rb.angularVelocity = new Vector3(UnityEngine.Random.Range(-m_CasingEjection.Spin, m_CasingEjection.Spin), UnityEngine.Random.Range(-m_CasingEjection.Spin, m_CasingEjection.Spin), UnityEngine.Random.Range(-m_CasingEjection.Spin, m_CasingEjection.Spin));
            }
        }

        [Serializable]
        public struct VisualEffectsSettings
        {
            public Transform Parent;

            [Header("Muzzle Flash")]
            public GameObject MuzzleFlash;
            public float MuzzleFlashScale;
            public Vector2 MuzzleFlashRandomScale;
            public Vector3 MuzzleFlashHipfireOffset;
            public Vector3 MuzzleFlashRandomRot;
            [Header("Tracer")]
            public GameObject Tracer;
            public Vector3 TracerOffset;
            [Header("Light")]
            public LightEffect Light;
        }

        [Serializable]
        public class CasingEjectionSettings
        {
            [Header("Casing Ejection")]
            public GameObject CasingPrefab;
            public float SpawnDelay;
            public Transform Parent;
            public Vector3 SpawnOffset;
            public Vector3 SpawnVelocity;
            public float CasingScale = 1f;
            public float Spin;
        }

    }
}

