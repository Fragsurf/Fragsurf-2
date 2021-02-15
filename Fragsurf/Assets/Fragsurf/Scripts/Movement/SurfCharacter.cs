using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Fragsurf.Actors;

namespace Fragsurf.Movement
{
    [AddComponentMenu("Fragsurf/Surf Character")]
    public class SurfCharacter : MonoBehaviour, ISurfControllable
    {
        [Header("Physics Settings")]
        public int TickRate = 100;
        public Vector3 ColliderSize = new Vector3(1, 1.83f, 1);

        [Header("View Settings")]
        public Camera Camera;
        public Vector3 ViewOffset = new Vector3(0, 1.64f, 0);
        public Vector3 DuckedViewOffset = new Vector3(0, 1.21f, 0);
        public int FieldOfView = 75;

        [Header("Input Settings")]
        public float XSens = 25;
        public float YSens = 25;
        public KeyCode JumpButton = KeyCode.Space;
        public KeyCode DuckButton = KeyCode.LeftControl;
        public KeyCode MoveLeft = KeyCode.A;
        public KeyCode MoveRight = KeyCode.D;
        public KeyCode MoveForward = KeyCode.W;
        public KeyCode MoveBack = KeyCode.S;
        public KeyCode Noclip = KeyCode.N;
        public KeyCode Restart = KeyCode.R;
        public KeyCode YawLeft = KeyCode.Mouse4;
        public KeyCode YawRight = KeyCode.Mouse3;
        public int YawSpeed = 260;

        [Header("Movement Config")]
        [SerializeField]
        private MovementConfig _moveConfig = new MovementConfig();
        private Vector3 _startPosition;
        private SurfController _controller = new SurfController();
        public MoveType MoveType { get; set; } = MoveType.Walk;

        public MovementConfig MoveConfig => _moveConfig;
        public MoveData MoveData { get; } = new MoveData();
        public BoxCollider Collider { get; private set; }
        public GameObject GroundObject { get; set; }
        public Vector3 BaseVelocity { get; }
        public Quaternion Orientation => Quaternion.identity;
        public Vector3 Forward => transform.forward;
        public Vector3 Right => transform.right;
        public Vector3 Up => transform.up;
        public Vector3 StandingExtents => ColliderSize * 0.5f;
        private float _alpha;
        private float _accumulator;
        private float _elapsedTime;
        private bool _hasCursor = true;

        private void OnDestroy()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void Start()
        {
            Time.fixedDeltaTime = 1f / TickRate;
            var mr = GetComponent<Renderer>();
            if (mr)
            {
                mr.enabled = false;
            }

            if (Camera == null)
            {
                Camera = Camera.main;
            }

            Camera.fieldOfView = FieldOfView;
            Camera.transform.SetParent(null);

            foreach (var collider in gameObject.GetComponentsInChildren<Collider>())
            {
                GameObject.Destroy(collider);
            }

            Collider = gameObject.AddComponent<BoxCollider>();
            Collider.size = ColliderSize;
            Collider.center = new Vector3(0, ColliderSize.y * 0.5f, 0);
            Collider.isTrigger = true;

            var rbody = gameObject.GetComponent<Rigidbody>();
            if (!rbody) rbody = gameObject.AddComponent<Rigidbody>();
            rbody.isKinematic = true;

            MoveData.Origin = transform.position;
            MoveData.ViewAngles = transform.rotation.eulerAngles;
            _moveConfig.NoclipCollide = false;
            _startPosition = transform.position;

            Physics.autoSimulation = true;
            Physics.autoSyncTransforms = true;
        }

        private void Update()
        {
            if(_hasCursor)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            if(Input.GetKey(KeyCode.Escape))
            {
                _hasCursor = !_hasCursor;
            }

            UpdateTestBinds();
            UpdateRotation();
            UpdateMoveData();

            Time.fixedDeltaTime = 1f / TickRate;
            var dt = Time.fixedDeltaTime * (1f / Time.timeScale);
            var newTime = Time.realtimeSinceStartup;
            var frameTime = newTime - _elapsedTime;
            if (frameTime > Time.fixedDeltaTime)
                frameTime = Time.fixedDeltaTime;
            Time.maximumDeltaTime = Time.fixedDeltaTime * (1f / Time.timeScale);
            _elapsedTime = newTime;
            _accumulator += frameTime;

            while (_accumulator >= dt)
            {
                _accumulator -= dt;
                Tick();
            }
            _alpha = _accumulator / dt;

            if(Camera)
            {
                Camera.transform.position = Vector3.Lerp(MoveData.PreviousOrigin, MoveData.Origin, _alpha) + (MoveData.Ducked ? DuckedViewOffset : ViewOffset);
            }
        }

        private void UpdateTestBinds()
        {
            if(Input.GetKeyDown(Restart))
            {
                MoveData.Velocity = Vector3.zero;
                MoveData.Origin = _startPosition;
            }
            if(Input.GetKeyDown(Noclip))
            {
                MoveType = MoveType == MoveType.Noclip ? MoveType.Walk : MoveType.Noclip;
            }
        }

        private List<GameObject> _touchingLastFrame = new List<GameObject>();
        private void Tick()
        {
            _controller.CalculateMovement(this, _moveConfig, Time.fixedDeltaTime);
            transform.position = MoveData.Origin;

            var prevOrigin = MoveData.PreviousOrigin;
            var newOrigin = MoveData.Origin;
            var center = prevOrigin;
            center.y += Collider.bounds.extents.y;
            var dir = (newOrigin - prevOrigin).normalized;
            var currentDistance = Vector3.Distance(prevOrigin, newOrigin);

            var touchedThisFrame = Physics.BoxCastAll(center: center,
                halfExtents: Collider.bounds.extents,
                direction: dir,
                orientation: Quaternion.identity,
                maxDistance: currentDistance,
                layerMask: SurfPhysics.GroundLayerMask,
                queryTriggerInteraction: QueryTriggerInteraction.Collide).ToList();
            
            for(int i = _touchingLastFrame.Count - 1; i >= 0; i--)
            {
                foreach(var hit in touchedThisFrame)
                {
                    if(hit.transform.gameObject == _touchingLastFrame[i].gameObject)
                    {
                        _touchingLastFrame[i].GetComponent<FSMTrigger>().OnEndTouch(0, true);
                        _touchingLastFrame.RemoveAt(i);
                        break;
                    }
                }
            }

            foreach(var raycastHit in touchedThisFrame)
            {
                if(!raycastHit.collider.isTrigger)
                {
                    continue;
                }
                var fsmTrigger = raycastHit.transform.GetComponent<FSMTrigger>();
                if (fsmTrigger != null)
                {
                    if (!_touchingLastFrame.Contains(raycastHit.transform.gameObject))
                    {
                        fsmTrigger.OnStartTouch(0, true);
                        _touchingLastFrame.Add(raycastHit.transform.gameObject);
                    }
                    else
                    {
                        fsmTrigger.OnTouch(0, true);
                    }
                }
            }
        }

        private void UpdateMoveData()
        {
            var moveLeft = Input.GetKey(MoveLeft);
            var moveRight = Input.GetKey(MoveRight);
            var moveFwd = Input.GetKey(MoveForward);
            var moveBack = Input.GetKey(MoveBack);
            var jump = Input.GetKey(JumpButton);
            var duck = Input.GetKey(DuckButton);

            if (!moveLeft && !moveRight)
                MoveData.SideMove = 0;
            else if (moveLeft)
                MoveData.SideMove = -MoveConfig.Accelerate;
            else if (moveRight)
                MoveData.SideMove = MoveConfig.Accelerate;

            if (!moveFwd && !moveBack)
                MoveData.ForwardMove = 0;
            else if (moveFwd)
                MoveData.ForwardMove = MoveConfig.Accelerate;
            else if (moveBack)
                MoveData.ForwardMove = -MoveConfig.Accelerate;

            if (jump)
                MoveData.Buttons |= InputActions.Jump;
            else
                MoveData.Buttons &= ~InputActions.Jump;

            if (duck)
                MoveData.Buttons |= InputActions.Duck;
            else
                MoveData.Buttons &= ~InputActions.Duck;

            MoveData.OldButtons = MoveData.Buttons;
            MoveData.ViewAngles = Camera.transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0, MoveData.ViewAngles.y, MoveData.ViewAngles.z);
        }

        private void UpdateRotation()
        {
            var angles = MoveData.ViewAngles;
            float mx = (Input.GetAxis("Mouse X") * XSens * .02200f);
            float my = Input.GetAxis("Mouse Y") * YSens * .02200f;
            var rot = angles + new Vector3(-my, mx, 0f);
            rot.x = SurfPhysics.ClampAngle(rot.x, -89f, 89f);

            var yaw = 0;
            if(Input.GetKey(YawLeft))
            {
                yaw = -YawSpeed;
            }
            else if(Input.GetKey(YawRight))
            {
                yaw = YawSpeed;
            }

            rot.y += yaw * Time.deltaTime;
            Camera.transform.rotation = Quaternion.Euler(rot);
        }

    }
}

