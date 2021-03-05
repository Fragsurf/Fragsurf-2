using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.BSP
{
    public class BspEntityOutput
    {

        public string MyOutput;
        public string TargetEntity;
        public string TargetInput;
        public string Parameter;
        public float Delay;
        public bool OnlyOnce;

        public static BspEntityOutput Parse(string myOutput, string input)
        {
            try
            {
                var split = input.Split(',');
                var result = new BspEntityOutput()
                {
                    MyOutput = myOutput,
                    TargetEntity = split[0].Trim(),
                    TargetInput = split[1].Trim(),
                    Parameter = split[2].Trim()
                };
                float.TryParse(split[3], out result.Delay);
                bool.TryParse(split[4], out result.OnlyOnce);
                return result;
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                return new BspEntityOutput();
            }
        }

    }
}

