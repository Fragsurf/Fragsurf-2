using UnityEngine;
using Fragsurf.Movement;
using Fragsurf.Shared.Entity;

namespace Fragsurf.Shared
{
    public class GameMovement : FSComponent
    {
        public SurfController Controller { get; } = new SurfController();
        public MovementConfig Config { get; } = new MovementConfig();

        protected override void _Initialize()
        {
            SurfPhysics.GroundLayerMask = 1 << Layers.Default;
            SurfPhysics.LadderLayerMask = 1 << Layers.Default;

            DevConsole.RegisterVariable("mv.gravity", "", () => Config.Gravity, v => Config.Gravity = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.autobhop", "", () => Config.AutoBhop, v => Config.AutoBhop = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.aircap", "", () => Config.AirCap, v => Config.AirCap = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.aircapsurfmodifier", "", () => Config.AirCapSurfModifier, v => Config.AirCapSurfModifier = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.airacceleration", "", () => Config.AirAccel, v => Config.AirAccel = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.acceleration", "", () => Config.Accel, v => Config.Accel = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.friction", "", () => Config.Friction, v => Config.Friction = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.stopspeed", "", () => Config.StopSpeed, v => Config.StopSpeed = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.jumppower", "", () => Config.JumpPower, v => Config.JumpPower = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.maxspeed", "", () => Config.MaxSpeed, v => Config.MaxSpeed = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.maxvelocity", "", () => Config.MaxVelocity, v => Config.MaxVelocity = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.noclipspeed", "", () => Config.NoclipSpeed, v => Config.NoclipSpeed = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.noclipfriction", "", () => Config.NoclipFriction, v => Config.NoclipFriction = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.noclipcollide", "", () => Config.NoclipCollide, v => Config.NoclipCollide = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.stepsize", "", () => Config.StepSize, v => Config.StepSize = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.duckdistance", "", () => Config.DuckDistance, v => Config.DuckDistance = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.duckwalkmodifier", "", () => Config.DuckWalkModifier, v => Config.DuckWalkModifier = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.falldamage", "", () => Config.FallDamage, v => Config.FallDamage = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.waterstopsfalldamage", "", () => Config.WaterStopsFallDamage, v => Config.WaterStopsFallDamage = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.solidplayers", "", () => Config.SolidPlayers, v => Config.SolidPlayers = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.movinguprapidlyfactor", "", () => Config.MovingUpRapidlyFactor, v => Config.MovingUpRapidlyFactor = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.maxclimbspeed", "", () => Config.MaxClimbSpeed, v => Config.MaxClimbSpeed = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.forwardspeed", "", () => Config.ForwardSpeed, v => Config.ForwardSpeed = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.sidespeed", "", () => Config.SideSpeed, v => Config.SideSpeed = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.waterdepthtoswim", "", () => Config.WaterDepthToSwim, v => Config.WaterDepthToSwim = v, this, ConVarFlags.Replicator);
            DevConsole.RegisterVariable("mv.brakespeed", "", () => Config.BrakeSpeed, v => Config.BrakeSpeed = v, this, ConVarFlags.Replicator);
        }

    }
}

