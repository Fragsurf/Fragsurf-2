using UnityEngine;

namespace UIForia.Rendering {

    public enum EasingFunction {

        Linear,
        QuadraticEaseIn,
        QuadraticEaseOut,
        QuadraticEaseInOut,
        CubicEaseIn,
        CubicEaseOut,
        CubicEaseInOut,
        QuarticEaseIn,
        QuarticEaseOut,
        QuarticEaseInOut,
        QuinticEaseIn,
        QuinticEaseOut,
        QuinticEaseInOut,
        SineEaseIn,
        SineEaseOut,
        SineEaseInOut,
        CircularEaseIn,
        CircularEaseOut,
        CircularEaseInOut,
        ExponentialEaseIn,
        ExponentialEaseOut,
        ExponentialEaseInOut,
        ElasticEaseIn,
        ElasticEaseOut,
        ElasticEaseInOut,
        BackEaseIn,
        BackEaseOut,
        BackEaseInOut,
        BounceEaseIn,
        BounceEaseOut,
        BounceEaseInOut

    }

    public static class Easing {

        private const float PI = Mathf.PI;
        private const float PiOverTwo = Mathf.PI / 2.0f;

        /// <summary>
        /// Interpolate using the specified function.
        /// </summary>
        public static float Interpolate(float p, EasingFunction easingFunction) {
            switch (easingFunction) {
                case EasingFunction.Linear: return Linear(p);
                case EasingFunction.QuadraticEaseOut: return QuadraticEaseOut(p);
                case EasingFunction.QuadraticEaseIn: return QuadraticEaseIn(p);
                case EasingFunction.QuadraticEaseInOut: return QuadraticEaseInOut(p);
                case EasingFunction.CubicEaseIn: return CubicEaseIn(p);
                case EasingFunction.CubicEaseOut: return CubicEaseOut(p);
                case EasingFunction.CubicEaseInOut: return CubicEaseInOut(p);
                case EasingFunction.QuarticEaseIn: return QuarticEaseIn(p);
                case EasingFunction.QuarticEaseOut: return QuarticEaseOut(p);
                case EasingFunction.QuarticEaseInOut: return QuarticEaseInOut(p);
                case EasingFunction.QuinticEaseIn: return QuinticEaseIn(p);
                case EasingFunction.QuinticEaseOut: return QuinticEaseOut(p);
                case EasingFunction.QuinticEaseInOut: return QuinticEaseInOut(p);
                case EasingFunction.SineEaseIn: return SineEaseIn(p);
                case EasingFunction.SineEaseOut: return SineEaseOut(p);
                case EasingFunction.SineEaseInOut: return SineEaseInOut(p);
                case EasingFunction.CircularEaseIn: return CircularEaseIn(p);
                case EasingFunction.CircularEaseOut: return CircularEaseOut(p);
                case EasingFunction.CircularEaseInOut: return CircularEaseInOut(p);
                case EasingFunction.ExponentialEaseIn: return ExponentialEaseIn(p);
                case EasingFunction.ExponentialEaseOut: return ExponentialEaseOut(p);
                case EasingFunction.ExponentialEaseInOut: return ExponentialEaseInOut(p);
                case EasingFunction.ElasticEaseIn: return ElasticEaseIn(p);
                case EasingFunction.ElasticEaseOut: return ElasticEaseOut(p);
                case EasingFunction.ElasticEaseInOut: return ElasticEaseInOut(p);
                case EasingFunction.BackEaseIn: return BackEaseIn(p);
                case EasingFunction.BackEaseOut: return BackEaseOut(p);
                case EasingFunction.BackEaseInOut: return BackEaseInOut(p);
                case EasingFunction.BounceEaseIn: return BounceEaseIn(p);
                case EasingFunction.BounceEaseOut: return BounceEaseOut(p);
                case EasingFunction.BounceEaseInOut: return BounceEaseInOut(p);
                default: return Linear(p);
            }
        }

        private static float Linear(float p) {
            return p;
        }

        /// <summary>
        /// Modeled after the parabola y = x^2
        /// </summary>
        public static float QuadraticEaseIn(float p) {
            return p * p;
        }

        /// <summary>
        /// Modeled after the parabola y = -x^2 + 2x
        /// </summary>
        public static float QuadraticEaseOut(float p) {
            return -(p * (p - 2));
        }

        /// <summary>
        /// Modeled after the piecewise quadratic
        /// y = (1/2)((2x)^2)             ; [0, 0.5)
        /// y = -(1/2)((2x-1)*(2x-3) - 1) ; [0.5, 1]
        /// </summary>
        public static float QuadraticEaseInOut(float p) {
            if (p < 0.5f) {
                return 2 * p * p;
            }
            else {
                return (-2 * p * p) + (4 * p) - 1;
            }
        }

        /// <summary>
        /// Modeled after the cubic y = x^3
        /// </summary>
        public static float CubicEaseIn(float p) {
            return p * p * p;
        }

        /// <summary>
        /// Modeled after the cubic y = (x - 1)^3 + 1
        /// </summary>
        public static float CubicEaseOut(float p) {
            float f = (p - 1);
            return f * f * f + 1;
        }

        /// <summary>	
        /// Modeled after the piecewise cubic
        /// y = (1/2)((2x)^3)       ; [0, 0.5)
        /// y = (1/2)((2x-2)^3 + 2) ; [0.5, 1]
        /// </summary>
        public static float CubicEaseInOut(float p) {
            if (p < 0.5f) {
                return 4 * p * p * p;
            }
            else {
                float f = ((2 * p) - 2);
                return 0.5f * f * f * f + 1;
            }
        }

        /// <summary>
        /// Modeled after the quartic x^4
        /// </summary>
        public static float QuarticEaseIn(float p) {
            return p * p * p * p;
        }

        /// <summary>
        /// Modeled after the quartic y = 1 - (x - 1)^4
        /// </summary>
        public static float QuarticEaseOut(float p) {
            float f = (p - 1);
            return f * f * f * (1 - p) + 1;
        }

        /// <summary>
        /// Modeled after the piecewise quartic
        /// y = (1/2)((2x)^4)        ; [0, 0.5)
        /// y = -(1/2)((2x-2)^4 - 2) ; [0.5, 1]
        ///</summary>
        public static float QuarticEaseInOut(float p) {
            if (p < 0.5f) {
                return 8 * p * p * p * p;
            }
            else {
                float f = (p - 1);
                return -8 * f * f * f * f + 1;
            }
        }

        /// <summary>
        /// Modeled after the quintic y = x^5
        /// </summary>
        public static float QuinticEaseIn(float p) {
            return p * p * p * p * p;
        }

        /// <summary>
        /// Modeled after the quintic y = (x - 1)^5 + 1
        /// </summary>
        public static float QuinticEaseOut(float p) {
            float f = (p - 1);
            return f * f * f * f * f + 1;
        }

        /// <summary>
        /// Modeled after the piecewise quintic
        /// y = (1/2)((2x)^5)       ; [0, 0.5)
        /// y = (1/2)((2x-2)^5 + 2) ; [0.5, 1]
        /// </summary>
        public static float QuinticEaseInOut(float p) {
            if (p < 0.5f) {
                return 16 * p * p * p * p * p;
            }
            else {
                float f = ((2 * p) - 2);
                return 0.5f * f * f * f * f * f + 1;
            }
        }

        /// <summary>
        /// Modeled after quarter-cycle of sine wave
        /// </summary>
        public static float SineEaseIn(float p) {
            return Mathf.Sin((p - 1) * PiOverTwo) + 1;
        }

        /// <summary>
        /// Modeled after quarter-cycle of sine wave (different phase)
        /// </summary>
        public static float SineEaseOut(float p) {
            return Mathf.Sin(p * PiOverTwo);
        }

        /// <summary>
        /// Modeled after half sine wave
        /// </summary>
        public static float SineEaseInOut(float p) {
            return 0.5f * (1 - Mathf.Cos(p * PI));
        }

        /// <summary>
        /// Modeled after shifted quadrant IV of unit circle
        /// </summary>
        public static float CircularEaseIn(float p) {
            return 1 - Mathf.Sqrt(1 - (p * p));
        }

        /// <summary>
        /// Modeled after shifted quadrant II of unit circle
        /// </summary>
        public static float CircularEaseOut(float p) {
            return Mathf.Sqrt((2 - p) * p);
        }

        /// <summary>	
        /// Modeled after the piecewise circular function
        /// y = (1/2)(1 - Mathf.Sqrt(1 - 4x^2))           ; [0, 0.5)
        /// y = (1/2)(Mathf.Sqrt(-(2x - 3)*(2x - 1)) + 1) ; [0.5, 1]
        /// </summary>
        public static float CircularEaseInOut(float p) {
            if (p < 0.5f) {
                return 0.5f * (1 - Mathf.Sqrt(1 - 4 * (p * p)));
            }
            else {
                return 0.5f * (Mathf.Sqrt(-((2 * p) - 3) * ((2 * p) - 1)) + 1);
            }
        }

        /// <summary>
        /// Modeled after the exponential function y = 2^(10(x - 1))
        /// </summary>
        public static float ExponentialEaseIn(float p) {
            return (p == 0.0f) ? p : Mathf.Pow(2, 10 * (p - 1));
        }

        /// <summary>
        /// Modeled after the exponential function y = -2^(-10x) + 1
        /// </summary>
        public static float ExponentialEaseOut(float p) {
            return (p == 1.0f) ? p : 1 - Mathf.Pow(2, -10 * p);
        }

        /// <summary>
        /// Modeled after the piecewise exponential
        /// y = (1/2)2^(10(2x - 1))         ; [0,0.5)
        /// y = -(1/2)*2^(-10(2x - 1))) + 1 ; [0.5,1]
        /// </summary>
        public static float ExponentialEaseInOut(float p) {
            if (p == 0.0 || p == 1.0) return p;

            if (p < 0.5f) {
                return 0.5f * Mathf.Pow(2, (20 * p) - 10);
            }
            else {
                return -0.5f * Mathf.Pow(2, (-20 * p) + 10) + 1;
            }
        }

        /// <summary>
        /// Modeled after the damped sine wave y = sin(13pi/2*x)*Mathf.Pow(2, 10 * (x - 1))
        /// </summary>
        public static float ElasticEaseIn(float p) {
            return Mathf.Sin(13 * PiOverTwo * p) * Mathf.Pow(2, 10 * (p - 1));
        }

        /// <summary>
        /// Modeled after the damped sine wave y = sin(-13pi/2*(x + 1))*Mathf.Pow(2, -10x) + 1
        /// </summary>
        public static float ElasticEaseOut(float p) {
            return Mathf.Sin(-13 * PiOverTwo * (p + 1)) * Mathf.Pow(2, -10 * p) + 1;
        }

        /// <summary>
        /// Modeled after the piecewise exponentially-damped sine wave:
        /// y = (1/2)*sin(13pi/2*(2*x))*Mathf.Pow(2, 10 * ((2*x) - 1))      ; [0,0.5)
        /// y = (1/2)*(sin(-13pi/2*((2x-1)+1))*Mathf.Pow(2,-10(2*x-1)) + 2) ; [0.5, 1]
        /// </summary>
        public static float ElasticEaseInOut(float p) {
            if (p < 0.5f) {
                return 0.5f * Mathf.Sin(13 * PiOverTwo * (2 * p)) * Mathf.Pow(2, 10 * ((2 * p) - 1));
            }
            else {
                return 0.5f * (Mathf.Sin(-13 * PiOverTwo * ((2 * p - 1) + 1)) * Mathf.Pow(2, -10 * (2 * p - 1)) + 2);
            }
        }

        /// <summary>
        /// Modeled after the overshooting cubic y = x^3-x*sin(x*pi)
        /// </summary>
        public static float BackEaseIn(float p) {
            return p * p * p - p * Mathf.Sin(p * PI);
        }

        /// <summary>
        /// Modeled after overshooting cubic y = 1-((1-x)^3-(1-x)*sin((1-x)*pi))
        /// </summary>	
        public static float BackEaseOut(float p) {
            float f = (1 - p);
            return 1 - (f * f * f - f * Mathf.Sin(f * PI));
        }

        /// <summary>
        /// Modeled after the piecewise overshooting cubic function:
        /// y = (1/2)*((2x)^3-(2x)*sin(2*x*pi))           ; [0, 0.5)
        /// y = (1/2)*(1-((1-x)^3-(1-x)*sin((1-x)*pi))+1) ; [0.5, 1]
        /// </summary>
        public static float BackEaseInOut(float p) {
            if (p < 0.5f) {
                float f = 2 * p;
                return 0.5f * (f * f * f - f * Mathf.Sin(f * PI));
            }
            else {
                float f = (1 - (2 * p - 1));
                return 0.5f * (1 - (f * f * f - f * Mathf.Sin(f * PI))) + 0.5f;
            }
        }

        /// <summary>
        /// </summary>
        public static float BounceEaseIn(float p) {
            return 1 - BounceEaseOut(1 - p);
        }

        /// <summary>
        /// </summary>
        public static float BounceEaseOut(float p) {
            if (p < 4 / 11.0f) {
                return (121 * p * p) / 16.0f;
            }
            else if (p < 8 / 11.0f) {
                return (363 / 40.0f * p * p) - (99 / 10.0f * p) + 17 / 5.0f;
            }
            else if (p < 9 / 10.0f) {
                return (4356 / 361.0f * p * p) - (35442 / 1805.0f * p) + 16061 / 1805.0f;
            }
            else {
                return (54 / 5.0f * p * p) - (513 / 25.0f * p) + 268 / 25.0f;
            }
        }

        /// <summary>
        /// </summary>
        public static float BounceEaseInOut(float p) {
            if (p < 0.5f) {
                return 0.5f * BounceEaseIn(p * 2);
            }
            else {
                return 0.5f * BounceEaseOut(p * 2 - 1) + 0.5f;
            }
        }

    }

}