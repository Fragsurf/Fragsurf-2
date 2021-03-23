using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Lidgren.Network;
using Fragsurf;
using Fragsurf.Shared.Player;
using Fragsurf.Actors;
using Fragsurf.Movement;
using MessagePack;
using Fragsurf.Shared.Entity;

namespace Game.Tricksurf
{

    [MessagePackObject]
    public struct TouchInfo
    {
        [IgnoreMember] public int Tick;
        [IgnoreMember] public FSMTrigger Trigger;
        [Key(0)] public int TriggerId;
        [IgnoreMember] public bool Admissible;
        [Key(1)] public float Time;
        [Key(2)] public float ZoneOffset;
        [Key(3)] public Vector3 Velocity;
        [Key(4)] public MoveStyle Style;
        [Key(5)] public bool InvalidStart;

        [IgnoreMember]
        public int Speed
        {
            get
            {
                var vel = Velocity;
                vel.y = 0;
                return (int)(vel.magnitude / SurfController.HammerScale);
            }
        }
    }

    public class TrickPosition
    {
        public TrickTreeNode Node;
        public TrickPosition Parent;
        public TouchInfo Info;
        public bool DeleteMe;
    }

    public class TrickTrack
    {

        public bool Invalidated;
        private TrickData _data;

        private TrickPosition[] _trickCache = new TrickPosition[256];
        private TrickTreeNode[] _nodeCache = new TrickTreeNode[1024];
        private List<TrickPosition> _positions = new List<TrickPosition>();
        private bool _detectionEnabled = true;

        public bool Debug;

        public TrickTrack(TrickData data)
        {
            _data = data;
        }

        public void Invalidate()
        {
            if(Debug)
            {
                DevConsole.WriteLine("\n[Reset]\n");
            }

            _positions.Clear();
            Invalidated = true;
        }

        public void UpdateTrickData(TrickData newData)
        {
            Invalidate();
            _data = newData;
        }

        public void EnableDetection(bool enabled)
        {
            _detectionEnabled = enabled;
        }

        private TrickPosition UpdatePositions(TouchInfo info)
        {
            var count = UpdatePositions(info, _trickCache);
            var maxLen = 0;
            TrickPosition result = null;
            for(int i = 0; i < count; i++)
            {
                if (_trickCache[i].Node.Trick.path.Count > maxLen)
                {
                    maxLen = _trickCache[i].Node.Trick.path.Count;
                    result = _trickCache[i];
                }
                else if(_trickCache[i].Node.Trick.path.Count == maxLen && result != null)
                {
                    if(result.Node.Trick.points < _trickCache[i].Node.Trick.points)
                    {
                        result = _trickCache[i];
                    }
                }
            }
            return result;
        }

        private int UpdatePositions(TouchInfo info, TrickPosition[] result)
        {
            var count = 0;
            var finalIndex = _positions.Count - 1;
            for (int i = finalIndex; i >= 0; i--)
            {
                if (info.TriggerId == _positions[i].Node.TriggerId)
                {
                    continue;
                }

                var walkCount = WalkTreeAll(info.TriggerId, _positions[i].Node, _nodeCache);

                for(int j = 0; j < walkCount; j++)
                {
                    var node = _nodeCache[j];
                    var stepPosition = new TrickPosition()
                    {
                        Node = node,
                        Parent = _positions[i],
                        Info = info
                    };
                    if (node.Trick != null)
                    {
                        result[count] = stepPosition;
                        count++;
                    }
                    _positions.Add(stepPosition);
                }

                if (!info.Admissible || count > 0)
                {
                    _positions.RemoveAt(i);
                    //var parent = _positions[i];
                    //while(parent != null)
                    //{
                    //    parent.DeleteMe = true;
                    //    parent = parent.Parent;
                    //}
                }
            }

            //for(int i = _positions.Count - 1; i >= 0; i--)
            //{
            //    if(_positions[i].DeleteMe)
            //    {
            //        _positions.RemoveAt(i);
            //        // todo: object pool
            //    }
            //}

            var newNode = _data.TrickTree.Children.FirstOrDefault(x => x.TriggerId == info.TriggerId);
            if (newNode != null)
            {
                var newPos = new TrickPosition()
                {
                    Node = newNode,
                    Info = info
                };
                _positions.Add(newPos);
            }

            return count;
        }

        private int WalkTreeAll(int triggerId, TrickTreeNode node, TrickTreeNode[] result)
        {
            int count = 0;

            foreach(var child in node.Children)
            {
                if(child.TriggerId == triggerId)
                {
                    result[count] = child;
                    count++;
                }
            }

            return count;
        }

        public int Test(List<TouchInfo> frames, Trick[] result)
        {
            _positions.Clear();

            var count = 0;

            for(int i = 0; i < frames.Count; i++)
            {
                var trick = UpdatePositions(frames[i]);
                if(trick != null)
                {
                    result[count] = trick.Node.Trick;
                    count++;
                }
            }

            return count;
        }

        public bool RegisterTouch(string triggerName, TouchInfo info, out TrickCompletion completion)
        {
            completion = default;

            if (Debug)
            {
                DevConsole.WriteLine($"[Touch] {triggerName}:{info.Admissible}");
            }

            var trickPos = UpdatePositions(info);
            if(trickPos != null && _detectionEnabled)
            {
                return GetTrickCompletion(trickPos, out completion);
            }
            else
            {
                return false;
            }
        }

        public void UpdateTouchSpeedAndTime(string triggerName, Vector3 velocity, bool invalidStart, float time, float zoneOffset)
        {
            var triggerId = _data.GetTriggerId(triggerName);
            if(triggerId == -1)
            {
                return;
            }
            for(int i = _positions.Count - 1; i >= 0; i--)
            {
                if (_positions[i].Info.TriggerId == triggerId)
                {
                    if (_positions[i].Info.TriggerId == triggerId)
                    {
                        var info = _positions[i].Info;
                        info.Time = time;
                        info.Velocity = velocity;
                        info.ZoneOffset = zoneOffset;
                        info.InvalidStart = invalidStart;
                        _positions[i].Info = info;
                    }
                    break;
                }
            }
        }

        private bool GetTrickCompletion(TrickPosition position, out TrickCompletion completion)
        {
            completion = new TrickCompletion();
            completion.Style = position.Info.Style;
            completion.Touches = new List<TouchInfo>(position.Node.Trick.path.Count);
            completion.EndTick = position.Info.Tick;
            completion.TrickName = position.Node.Trick.name;
            completion.TrickId = position.Node.Trick.id;
            var canPrespeed = position.Node.Trick.prespeed;

            var endTime = position.Info.Time;
            var count = 0;
            TrickPosition firstPosition = null;

            while(position != null)
            {
                completion.Touches.Add(position.Info);
                if(position.Parent != null && position.Info.Style != completion.Style)
                {
                    completion.Style = MoveStyle.FW;
                }

                completion.AverageVelocity += position.Info.Speed;
                firstPosition = position.Parent ?? position;
                position = position.Parent;
                count++;
            }

            if(firstPosition.Info.InvalidStart
                && !canPrespeed)
            {
                return false;
            }

            completion.Style = MoveStyle.FW;
            completion.Touches.Reverse();
            completion.CompletionTime = endTime - firstPosition.Info.Time;
            completion.AverageVelocity = completion.AverageVelocity / count;
            completion.StartTick = firstPosition.Info.Tick;

            return true;
        }

    }
}
