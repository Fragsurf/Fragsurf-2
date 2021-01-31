using Fragsurf.Shared.Entity;
using System;
using UnityEngine;

namespace Fragsurf.Misc
{
	[RequireComponent(typeof(Light))]
	public class LightEffect : MonoBehaviour, IClientComponent
	{
		public enum PlayMode { Once, Loop }

		[Serializable]
		public class PulseSettings
		{
			public bool Enabled;

			public PlayMode Mode;

			[Space]

			[Range(0f, 3f)]
			public float Duration;
			public Color Color;

			[Space]

			[Range(0f, 1f)]
			public float IntensityAmplitude;

			[Range(0f, 1f)]
			public float RangeAmplitude;

			[Range(0f, 1f)]
			public float ColorWeight;
		}

		[Serializable]
		public class NoiseSettings
		{
			public bool Enabled;

			[Range(0f, 1f)]
			public float Intensity = 0.05f;

			[Range(0f, 10f)]
			public float Speed = 1f;
		}

		public bool IsPlaying { get => m_IsPlaying; }
		public float IntensityMultiplier { get; set; }

		[SerializeField]
		private bool m_PlayOnAwake = false;

		[SerializeField]
		[Range(0f, 5f)]
		private float m_Intensity = 1f;

		[SerializeField]
		[Range(0f, 10f)]
		private float m_Range = 1f;

		[SerializeField]
		private Color m_Color = Color.yellow;

		[Space]

		[SerializeField]
		[Range(0f, 2f)]
		private float m_FadeInTime = 0.5f;

		[SerializeField]
		[Range(0f, 2f)]
		private float m_FadeOutTime = 0.5f;

		[Header("Effects")]

		[SerializeField]
		private PulseSettings m_Pulse = null;

		[SerializeField]
		private NoiseSettings m_Noise = null;

		private bool m_IsPlaying;
		private bool m_LightsEnabled;

		private float m_Weight;
		private bool m_FadingIn;
		private bool m_FadingOut;

		private Light[] m_Lights;

		private bool m_PulseActive;
		private float m_PulseTimer;


		public void Play(bool fadeIn)
		{
			if(m_IsPlaying)
				return;

			m_IsPlaying = true;
			m_PulseActive = true;
			m_FadingIn = fadeIn;
			m_Weight = m_FadingIn ? 0f : 1f;
			m_PulseTimer = 0f;
		}

		public void Stop(bool fadeOut)
		{
			m_IsPlaying = false;
			m_FadingOut = fadeOut;

			m_PulseActive = false;

			if(!m_FadingOut)
				m_Weight = 0f;
		}

		private void Awake()
		{
			m_Lights = GetComponentsInChildren<Light>(true);
			EnableLights(false);
			IntensityMultiplier = 1f;

			if (m_PlayOnAwake)
				Play(true);
		}

		private void Update()
		{
			float intensity = m_Intensity;
			float range = m_Range;
			Color color = m_Color;

			if(m_IsPlaying)
			{
				// Pulse handling
				if (m_Pulse.Enabled && m_PulseActive)
				{
					float time = m_PulseTimer / Mathf.Max(m_Pulse.Duration, 0.001f);
					float param = (Mathf.Sin(Mathf.PI * (2f * time - 0.5f)) + 1f) * 0.5f;

					intensity += m_Intensity * param * m_Pulse.IntensityAmplitude;
					range += m_Range * param * m_Pulse.RangeAmplitude;
					color = Color.Lerp(color, m_Pulse.Color, param * m_Pulse.ColorWeight);

					m_PulseTimer += Time.deltaTime;

					if(m_PulseTimer > m_Pulse.Duration)
					{
						if(m_Pulse.Mode == PlayMode.Once)
							m_PulseActive = false;

						m_PulseTimer = 0f;
					}
				}

				// Auto stop when all effects finished playing
				if(!m_PulseActive)
				{
					m_IsPlaying = false;
					m_FadingOut = true;
				}
			}

			// Noise
			if(m_LightsEnabled && m_Noise.Enabled)
			{
				float noise = Mathf.PerlinNoise(Time.time * m_Noise.Speed, 0f) * m_Noise.Intensity;
				intensity += m_Intensity * noise;
				range += m_Range * noise;
			}

			// Fade in & out
			if(m_FadingIn)
			{
				m_Weight = Mathf.MoveTowards(m_Weight, 1f, Time.deltaTime * (1f / m_FadeInTime));

				if(m_Weight == 1f)
					m_FadingIn = false;
			}
			else if(m_FadingOut)
			{
				m_Weight = Mathf.MoveTowards(m_Weight, 0f, Time.deltaTime * (1f / m_FadeOutTime));

				if(m_Weight == 0f)
				{
					EnableLights(false);
					SetLightsIntensity(0f);
					m_FadingOut = false;
				}
			}

			// Enable the light if effects started playing
			if(!m_LightsEnabled && m_IsPlaying)
				EnableLights(true);

			// Apply effects to the light
			if(m_LightsEnabled)
				SetLightsParameters(intensity * IntensityMultiplier * m_Weight, range, color);
		}

		private void EnableLights(bool enable)
		{
			for(int i = 0;i < m_Lights.Length;i++)
				m_Lights[i].enabled = enable;

			m_LightsEnabled = enable;
		}

		private void SetLightsIntensity(float intensity)
		{
			for(int i = 0;i < m_Lights.Length;i++)
				m_Lights[i].intensity = intensity;
		}

		private void SetLightsParameters(float intensity, float range, Color color)
		{
			for(int i = 0;i < m_Lights.Length;i++)
			{
				m_Lights[i].intensity = intensity;
				m_Lights[i].range = range;
				m_Lights[i].color = color;
			}
		}
	}
}
