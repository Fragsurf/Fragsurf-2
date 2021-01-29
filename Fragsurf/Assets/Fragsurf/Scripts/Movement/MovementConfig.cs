using System;

namespace Fragsurf.Movement
{
    [Serializable]
    public class MovementConfig
    {

        public const float WORLD_SCALE = 0.0253999862840074f;

        public float Gravity = 800 * WORLD_SCALE;
        public bool AutoBhop = true;
        public float AirCap = 30 * WORLD_SCALE;
        public float AirCapSurfModifier = 1f;
        public float AirAccel = 1500;
        public float Accel = 8.2f;
        public float Friction = 4f;
        public float BrakeSpeed = 1f;
        public float StopSpeed = 4.2f;
        public float JumpPower = 7.15f;
        public float MaxSpeed = 320 * WORLD_SCALE;
        public float MaxVelocity = 3500 * WORLD_SCALE;
        public float NoclipSpeed = 6.5f;
        public float NoclipFriction = 12f;
        public bool NoclipCollide = false;
        public float StepSize = 0.48f;
        public float DuckDistance = 0.35f;
        public float DuckWalkModifier = 0.4f;
        public bool FallDamage = true;
        public bool WaterStopsFallDamage = true;
        public float LadderDistance = 0.1f;
        public float MaxClimbSpeed = 6.9f;
        public float JumpOffLadderSpeed = 7.02f;
        public float LadderAngle = -0.707f;
        public float LadderDampen = 0.2f;
        public bool SolidPlayers;
        public float MovingUpRapidlyFactor = 0.85f;
        public float SlideFactor = 0.75f;
        public float SlideDot = 0.08f;
        public float ForwardSpeed = 400 * WORLD_SCALE;
        public float SideSpeed = 400 * WORLD_SCALE;
        public float WaterDepthToSwim = 0.6f;
    }
}
