using System;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Packets;
using Fragsurf.Movement;
using Fragsurf.UI;

namespace Fragsurf.Client
{
    public class ClientInput : FSComponent
    {
        private int _moveTick;
        private float _yaw;
        private bool _yawLeft;
        private bool _yawRight;
        private float _yawMulti;

        public static bool Blocked
        {
            get => Blockers.Count > 0 || UGuiManager.Instance.HasCursor();
        }

        public static List<object> Blockers = new List<object>();

        private InputActions _prevActionsDown;
        private InputActions _actionsDown;
        private Queue<InputActions> _actionsQueue = new Queue<InputActions>(128);
        private List<InputActions> _inputActionsArray = new List<InputActions>();
        private List<InputActions> _actionsRequiringRelease = new List<InputActions>(128);
        private Dictionary<InputActions, int> _actionPriority = new Dictionary<InputActions, int>(128);
        private int _currentPriority;

        private static List<InputActions> _oneshotActions = new List<InputActions>()
        {
            InputActions.Drop,
            InputActions.Interact,
            InputActions.NextItem,
            InputActions.PrevItem,
            InputActions.Slot1,
            InputActions.Slot2,
            InputActions.Slot3,
            InputActions.Slot4,
            InputActions.Slot5,
            InputActions.Reload,
            InputActions.Flashlight
        };

        protected UserCmd UserCmd { get; } = new UserCmd();
        public UserCmd.CmdFields CurrentCommand => UserCmd.Fields;

        [ConVar("input.sensitivity", "Mouse sensitivity", ConVarFlags.UserSetting)]
        public float Sensitivity { get; set; } = 1f;
        [ConVar("input.yawspeed", "Yaw speed", ConVarFlags.UserSetting)]
        public float YawSpeed { get; set; } = 260;
        [ConVar("input.mouseyaw", "Mouse yaw multiplier (default 1)", ConVarFlags.UserSetting)]
        public float MouseYaw { get; set; } = 1f;
        [ConVar("input.mousepitch", "Mouse pitch multiplier (default 1)", ConVarFlags.UserSetting)]
        public float MousePitch { get; set; } = 1f;
        [ConVar("input.confinecursor", "Confines the cursor to the game window", ConVarFlags.UserSetting)]
        public bool ConfineCursor { get; set; } = false;

        [ConCommand("+input", "", ConVarFlags.Silent)]
        private void SetInput(string actionName)
        {
            if (Enum.TryParse(actionName, true, out InputActions action))
            {
                _actionsDown |= action;
            }
        }

        [ConCommand("-input", "", ConVarFlags.Silent)]
        private void ReleaseInput(string actionName)
        {
            if (Enum.TryParse(actionName, true, out InputActions action))
            {
                _actionsDown &= ~action;
            }
        }

        [ConCommand("+yaw", flags: ConVarFlags.Silent)]
        private void Yaw(float v) => _yaw = v;
        [ConCommand("-yaw", flags: ConVarFlags.Silent)]
        private void ReleaseYaw() => _yaw = 0;
        [ConCommand("+yawmultiplier", flags: ConVarFlags.Silent)]
        private void ApplyYawMultiplier(float v) => _yawMulti = v;
        [ConCommand("-yawmultiplier", flags: ConVarFlags.Silent)]
        private void ResetYawMultiplier() => _yawMulti = 0;
        [ConCommand("+left", flags: ConVarFlags.Silent)]
        private void YawLeft() => _yawLeft = true;
        [ConCommand("+right", flags: ConVarFlags.Silent)]
        private void YawRight() => _yawRight = true;
        [ConCommand("-left", flags: ConVarFlags.Silent)]
        private void ReleaseYawLeft() => _yawLeft = false;
        [ConCommand("-right", flags: ConVarFlags.Silent)]
        private void ReleaseYawRight() => _yawRight = false;

        protected override void _Initialize()
        {
            foreach (InputActions inputButton in Enum.GetValues(typeof(InputActions)))
            {
                _inputActionsArray.Add(inputButton);
            }
        }

        protected override void _Tick()
        {
            var localPlayer = Game.PlayerManager.LocalPlayer;

            if (!Game.Live
                || Human.Local == null
                || Human.Local.Dead
                || localPlayer == null)
            {
                UserCmd.Buttons = 0;
                _actionsRequiringRelease.Clear();
                _actionsQueue.Clear();
                _prevActionsDown = InputActions.None;

                return;
            }

            UserCmd.Reset();
            UserCmd.Buttons = _actionsQueue.Count > 0 ? _actionsQueue.Dequeue() : _prevActionsDown;
            UserCmd.HostTime = ((ClientSocketManager)Game.Network).GetRemoteTime(Game.ElapsedTime);
            UserCmd.PacketId = _moveTick;
            UserCmd.Angles = Human.Local.Angles;
            _moveTick++;

            if (Blocked)
            {
                UserCmd.Buttons = 0;
                _actionsQueue.Clear();
                _prevActionsDown = InputActions.None;
            }

            TickUserCmd();

            Game.Get<UserCmdHandler>(true).HandleUserCommand(localPlayer, UserCmd, true);
        }

        protected override void _Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _yawLeft = false;
                _yawRight = false;
                _yaw = 0;
                _yawMulti = 0;
            }

            if (CursorIsVisible())
            {
                Cursor.visible = true;
                Cursor.lockState = ConfineCursor ? CursorLockMode.Confined : CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            UpdateUserCmd();
        }

        protected virtual void UpdateUserCmd()
        {
            var processPlayerInput = Game.Live
                && Human.Local != null
                && Human.Local.MovementController != null
                && Human.Local.IsFirstPerson
                && !Blocked;

            if (processPlayerInput)
            {
                var delta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

                if (Human.Local.MovementController.MouseControlsRotation)
                {
                    Human.Local.Angles = ProcessMouseLook(Human.Local.Angles, delta.x, delta.y);

                    var yaw = _yaw != 0 ? _yaw : (_yawLeft ? -YawSpeed : (_yawRight) ? YawSpeed : 0);

                    if (yaw != 0)
                    {
                        if (_yawMulti != 0)
                        {
                            yaw *= _yawMulti;
                        }
                        var rot = Human.Local.Angles;
                        rot.y += yaw * Time.deltaTime;
                        if (rot.y > 360) rot.y -= 360;
                        else if (rot.y < -360) rot.y += 360;
                        Human.Local.Angles = rot;
                    }
                }

                var newAction = _actionsDown;
                if (newAction != _prevActionsDown)
                {
                    _actionsQueue.Enqueue(newAction);
                    _prevActionsDown = newAction;
                }
            }
        }

        private InputActions _prevButtons;
        // todo: build usercmds in a way that is modular and configurable
        protected virtual void TickUserCmd()
        {
            foreach (var btn in _inputActionsArray)
            {
                if (_oneshotActions.Contains(btn))
                {
                    if (UserCmd.Buttons.HasFlag(btn))
                    {
                        if (_actionsRequiringRelease.Contains(btn))
                        {
                            UserCmd.Buttons &= ~btn;
                        }
                        else
                        {
                            _actionsRequiringRelease.Add(btn);
                        }
                    }
                    else if (_actionsRequiringRelease.Contains(btn))
                    {
                        _actionsRequiringRelease.Remove(btn);
                    }
                }
                var isDown = UserCmd.Buttons.HasFlag(btn);
                var wasDown = _prevButtons.HasFlag(btn);
                if (isDown && !wasDown)
                {
                    _currentPriority++;
                    _actionPriority[btn] = _currentPriority;
                }
                else if (!isDown)
                {
                    _actionPriority[btn] = 0;
                }
            }

            _prevButtons = UserCmd.Buttons;

            var lp = _actionPriority[InputActions.MoveLeft];
            var rp = _actionPriority[InputActions.MoveRight];
            var fwp = _actionPriority[InputActions.MoveForward];
            var bwp = _actionPriority[InputActions.MoveBack];
            if (lp > 0 && lp > rp)
            {
                UserCmd.Buttons &= ~InputActions.MoveRight;
            }
            else if (rp > 0 && rp > lp)
            {
                UserCmd.Buttons &= ~InputActions.MoveLeft;
            }
            if (fwp > 0 && fwp > bwp)
            {
                UserCmd.Buttons &= ~InputActions.MoveBack;
            }
            else if (bwp > 0 && bwp > fwp)
            {
                UserCmd.Buttons &= ~InputActions.MoveForward;
            }

            
        }

        private Vector3 ProcessMouseLook(Vector3 start, float x, float y)
        {
            var cameraSensModifier = Human.Local.CameraController.SensitivityModifier;
            var mx = x * Sensitivity * MouseYaw * cameraSensModifier;
            var my = y * Sensitivity * MousePitch * cameraSensModifier;
            var rotDelta = new Vector3(-my, mx, 0f);
            var finalRot = start + rotDelta;
            finalRot.x = ClampAngle(finalRot.x, -89f, 89f);
            if (finalRot.y > 360) finalRot.y -= 360;
            else if (finalRot.y < -360) finalRot.y += 360;
            return finalRot;
        }

        private float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }

        protected virtual bool CursorIsVisible()
        {
            var cursorIsVisible = !Game.Live
                || Input.GetKey(KeyCode.Period)
                || Blocked
                || (Human.Local != null && Human.Local.MovementController != null && Human.Local.MovementController.ShowsCursor);

            return cursorIsVisible;
        }

    }
}

