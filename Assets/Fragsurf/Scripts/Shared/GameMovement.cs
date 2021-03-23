using UnityEngine;
using Fragsurf.Movement;
using Fragsurf.Shared.Entity;

namespace Fragsurf.Shared
{
    public class GameMovement : FSComponent
    {
        public SurfController Controller { get; } = new SurfController();
        public MovementConfig Config { get; } = new MovementConfig();

        [ConVar("mv.gravity", "", ConVarFlags.Replicator)]
        public int Gravity
        {
            get => ToHammerUnits(Config.Gravity);
            set => Config.Gravity = ToUnityUnits(value);
        }

        [ConVar("mv.aircap", "", ConVarFlags.Replicator)]
        public int AirCap
        {
            get => ToHammerUnits(Config.AirCap);
            set => Config.AirCap = ToUnityUnits(value);
        }

        [ConVar("mv.accelerate", "", ConVarFlags.Replicator)]
        public float Accelerate
        {
            get => Config.Accelerate;
            set => Config.Accelerate = value;
        }

        [ConVar("mv.jumppower", "", ConVarFlags.Replicator)]
        public int JumpPower
        {
            get => ToHammerUnits(Config.JumpPower);
            set => Config.JumpPower = ToUnityUnits(value);
        }

        [ConVar("mv.maxspeed", "", ConVarFlags.Replicator)]
        public int MaxSpeed
        {
            get => ToHammerUnits(Config.MaxSpeed);
            set => Config.MaxSpeed = ToUnityUnits(value);
        }

        [ConVar("mv.maxvelocity", "", ConVarFlags.Replicator)]
        public int MaxVelocity
        {
            get => ToHammerUnits(Config.MaxVelocity);
            set => Config.MaxVelocity = ToUnityUnits(value);
        }

        [ConVar("mv.noclipspeed", "", ConVarFlags.Replicator)]
        public int NoclipSpeed
        {
            get => ToHammerUnits(Config.NoclipSpeed);
            set => Config.NoclipSpeed = ToUnityUnits(value);
        }

        [ConVar("mv.stepsize", "", ConVarFlags.Replicator)]
        public int StepSize
        {
            get => ToHammerUnits(Config.StepSize);
            set => Config.StepSize = ToUnityUnits(value);
        }

        [ConVar("mv.duckdistance", "", ConVarFlags.Replicator)]
        public int DuckDistance
        {
            get => ToHammerUnits(Config.DuckDistance);
            set => Config.DuckDistance = ToUnityUnits(value);
        }

        [ConVar("mv.maxclimbspeed", "", ConVarFlags.Replicator)]
        public int MaxClimbSpeed
        {
            get => ToHammerUnits(Config.MaxClimbSpeed);
            set => Config.MaxClimbSpeed = ToUnityUnits(value);
        }

        [ConVar("mv.forwardspeed", "", ConVarFlags.Replicator)]
        public int ForwardSpeed
        {
            get => ToHammerUnits(Config.ForwardSpeed);
            set => Config.ForwardSpeed = ToUnityUnits(value);
        }

        [ConVar("mv.sidespeed", "", ConVarFlags.Replicator)]
        public int SideSpeed
        {
            get => ToHammerUnits(Config.SideSpeed);
            set => Config.SideSpeed = ToUnityUnits(value);
        }

        [ConVar("mv.stopspeed", "", ConVarFlags.Replicator)]
        public int StopSpeed
        {
            get => ToHammerUnits(Config.StopSpeed);
            set => Config.StopSpeed = ToUnityUnits(value);
        }

        [ConVar("mv.waterswimspeed", "", ConVarFlags.Replicator)]
        public int SwimSpeed
        {
            get => ToHammerUnits(Config.WaterSwimSpeed);
            set => Config.WaterSwimSpeed = ToUnityUnits(value);
        }

        [ConVar("mv.watersinkspeed", "", ConVarFlags.Replicator)]
        public int SinkSpeed
        {
            get => ToHammerUnits(Config.WaterSinkSpeed);
            set => Config.WaterSinkSpeed = ToUnityUnits(value);
        }

        [ConVar("mv.waterjumppower", "", ConVarFlags.Replicator)]
        public int WaterJumpPower
        {
            get => ToHammerUnits(Config.WaterJumpPower);
            set => Config.WaterJumpPower = ToUnityUnits(value);
        }

        [ConVar("mv.waterjumpoutpower", "", ConVarFlags.Replicator)]
        public int WaterJumpOutPower
        {
            get => ToHammerUnits(Config.WaterJumpOutPower);
            set => Config.WaterJumpOutPower = ToUnityUnits(value);
        }

        private int ToHammerUnits(float input)
        {
            return Mathf.RoundToInt(input / SurfController.HammerScale);
        }

        private float ToUnityUnits(int input)
        {
            return input * SurfController.HammerScale;
        }

        protected override void _Initialize()
        {
            SurfPhysics.GroundLayerMask = 1 << Layers.Default;
            SurfPhysics.LadderLayerMask = 1 << Layers.Default;

            DevConsole.RegisterVariable("mv.friction", "", () => Config.Friction, v => Config.Friction = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.waterfriction", "", () => Config.WaterFriction, v => Config.WaterFriction = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.waterdepthtoswim", "", () => Config.WaterDepthToSwim, v => Config.WaterDepthToSwim = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.waterdepthtojumpout", "", () => Config.WaterDepthToJumpOut, v => Config.WaterDepthToJumpOut = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.brakespeed", "", () => Config.BrakeSpeed, v => Config.BrakeSpeed = v, this, ConVarFlags.Replicator);

            DevConsole.RegisterVariable("mv.autobhop", "", () => Config.AutoBhop, v => Config.AutoBhop = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.aircapsurfmodifier", "", () => Config.AirCapSurfModifier, v => Config.AirCapSurfModifier = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.airacceleration", "", () => Config.AirAccel, v => Config.AirAccel = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.noclipfriction", "", () => Config.NoclipFriction, v => Config.NoclipFriction = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.noclipcollide", "", () => Config.NoclipCollide, v => Config.NoclipCollide = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.duckwalkmodifier", "", () => Config.DuckWalkModifier, v => Config.DuckWalkModifier = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.falldamage", "", () => Config.FallDamage, v => Config.FallDamage = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.waterstopsfalldamage", "", () => Config.WaterStopsFallDamage, v => Config.WaterStopsFallDamage = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.solidplayers", "", () => Config.SolidPlayers, v => Config.SolidPlayers = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.movinguprapidlyfactor", "", () => Config.MovingUpRapidlyFactor, v => Config.MovingUpRapidlyFactor = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.waterdepthtoswim", "", () => Config.WaterDepthToSwim, v => Config.WaterDepthToSwim = v, this, ConVarFlags.Replicator);
            
        }

    }
}

