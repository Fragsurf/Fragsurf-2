using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Tricksurf
{

    public class TrickTreeNode
    {
        public List<TrickTreeNode> Children = new List<TrickTreeNode>();
        public readonly int TriggerId;
        public Trick Trick;
        public TrickTreeNode(int triggerId, Trick trick = null)
        {
            TriggerId = triggerId;
            Trick = trick;
        }
    }

    public class Trick
    {
        public int id;
        public List<int> path;
        public HashSet<int> pass;
        public int points;
        public string name;
        public bool prespeed;
        public bool custom;

        public override string ToString()
        {
            return name;
        }
    }

    public class TrickData 
    {
        public string whop_velocity = "0,0,0";
        public string whop_origin = "0,0,0";
        public string tune;
        public Dictionary<int, string> triggers;
        public List<Trick> tricks;
        public Dictionary<int, int> chains;
        public TrickTreeNode TrickTree;
        public List<int> lateload;

        private Dictionary<string, int> _triggerIds;
        private Dictionary<string, Trick> _trickMap;

        public int GetTriggerId(string triggerName)
        {
            if(_triggerIds.ContainsKey(triggerName))
            {
                return _triggerIds[triggerName];
            }
            return -1;
        }

        public Trick GetTrick(int trickId)
        {
            return tricks.Find(x => x.id == trickId);
        }

        public Trick GetTrick(string name)
        {
            if(_trickMap.ContainsKey(name))
            {
                return _trickMap[name];
            }
            return null;
        }

        public void BuildTree()
        {
            triggers = triggers.ToDictionary(kv => kv.Key, kv => (kv.Value).ToLower());

            _triggerIds = new Dictionary<string, int>();
            foreach(var kvp in triggers)
            {
                if (_triggerIds.ContainsKey(kvp.Value))
                {
                    continue;
                }
                _triggerIds.Add(kvp.Value, kvp.Key);
            }

            _trickMap = new Dictionary<string, Trick>();
            foreach(var trick in tricks)
            {
                _trickMap[trick.name] = trick;
            }

            TrickTree = new TrickTreeNode(-1, null);

            foreach(var trick in tricks)
            {
                if(chains != null && chains.Count > 0)
                {
                    for (int i = trick.path.Count - 2; i >= 0; i--)
                    {
                        if (chains.ContainsKey(trick.path[i])
                            && trick.path[i + 1] != chains[trick.path[i]])
                        {
                            trick.path.Insert(i + 1, chains[trick.path[i]]);
                        }
                    }
                }

                AddNode(TrickTree, trick);
            }

            //var str = PrintTree(TrickTree);
            //System.IO.File.WriteAllText(@"C:\Users\Jake\Documents\WriteLines.txt", str);
        }

        private void AddNode(TrickTreeNode parent, Trick trick)
        {
            for(int i = 0; i < trick.path.Count; i++)
            {
                var newParent = parent.Children.Find(x => x.TriggerId == trick.path[i]);
                if(newParent == null)
                {
                    newParent = new TrickTreeNode(trick.path[i]);
                    parent.Children.Add(newParent);
                }
                if(i == trick.path.Count - 1)
                {
                    newParent.Trick = trick;
                }
                parent = newParent;
            }
        }

        private string PrintTree(TrickTreeNode node, int level = 0)
        {
            var tab = string.Concat(Enumerable.Repeat("--", level + 1));
            var result = tab + ":" + node.TriggerId + ":" + (node.Trick != null ? node.Trick.name : string.Empty);
            foreach(var child in node.Children)
            {
                result += "\n" + PrintTree(child, level + 1);
            }
            return result;
        }

    }

}
