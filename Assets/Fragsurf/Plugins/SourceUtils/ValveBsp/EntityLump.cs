using System;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using SourceUtils.ValveBsp.Entities;

namespace SourceUtils
{
    partial class ValveBspFile
    {
        public class EntityLump : ILump, IEnumerable<Entity>
        {
            private static readonly Dictionary<string, Func<Entity>> _sEntityCtors = new Dictionary<string, Func<Entity>>();

            static EntityLump()
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!typeof(Entity).IsAssignableFrom(type)) continue;
                        var attrib = type.GetCustomAttribute<EntityClassAttribute>();
                        if (attrib == null) continue;

                        var ctor = type.GetConstructor(Type.EmptyTypes);
                        if (ctor == null) continue;

                        var action = Expression.Lambda<Func<Entity>>(Expression.New(ctor)).Compile();

                        _sEntityCtors.Add(attrib.Name, action);
                    }
                }
            }

            public LumpType LumpType { get; }

            private readonly ValveBspFile _bspFile;

            private List<Entity> _entities;

            public int Count
            {
                get
                {
                    EnsureLoaded();
                    return _entities.Count;
                }
            }

            public Entity this[ int index ]
            {
                get
                {
                    EnsureLoaded();
                    return _entities[index];
                }
            }

            public EntityLump( ValveBspFile bspFile, LumpType type )
            {
                _bspFile = bspFile;
                LumpType = type;
            }

            private void EnsureLoaded()
            {
                lock ( this )
                {
                    if ( _entities != null ) return;

                    _entities = new List<Entity>();

                    var keyValues = KeyValues.ListFromStream( _bspFile.GetLumpStream( LumpType ) );

                    foreach ( var entity in keyValues )
                    {
                        var className = entity["classname"];

                        Func<Entity> ctor;
                        var ent = _sEntityCtors.TryGetValue( className, out ctor ) ? ctor() : new Entity();
                        ent.Initialize( entity );

                        _entities.Add( ent );
                    }
                }
            }

            public T GetFirst<T>( bool inherit = true )
                where T : Entity
            {
                return this.OfType<T>().FirstOrDefault( x => inherit || x.GetType() == typeof(T) );
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<Entity> GetEnumerator()
            {
                EnsureLoaded();
                return _entities.GetEnumerator();
            }
        }
    }

    namespace ValveBsp.Entities
    {
        [AttributeUsage(AttributeTargets.Class)]
        public class EntityClassAttribute : Attribute
        {
            public string Name { get; set; }

            public EntityClassAttribute( string name )
            {
                Name = name;
            }
        }

        [AttributeUsage(AttributeTargets.Property)]
        public class EntityFieldAttribute : Attribute
        {
            public string Name { get; set; }

            public EntityFieldAttribute( string name )
            {
                Name = name;
            }
        }

        public struct EntityProperty
        {
            public static implicit operator string( EntityProperty prop )
            {
                return prop._entity.GetRawPropertyValue( prop._name );
            }

            public static implicit operator bool( EntityProperty prop )
            {
                return Entity.Converters.ToBoolean( prop._entity.GetRawPropertyValue( prop._name ) );
            }

            public static implicit operator int( EntityProperty prop )
            {
                return Entity.Converters.ToInt32( prop._entity.GetRawPropertyValue( prop._name ) );
            }

            public static implicit operator float( EntityProperty prop )
            {
                return Entity.Converters.ToSingle( prop._entity.GetRawPropertyValue( prop._name ) );
            }

            public static implicit operator Vector3( EntityProperty prop )
            {
                return Entity.Converters.ToVector3( prop._entity.GetRawPropertyValue( prop._name ) );
            }

            public static implicit operator Color32( EntityProperty prop )
            {
                return Entity.Converters.ToColor32( prop._entity.GetRawPropertyValue( prop._name ) );
            }

            private readonly Entity _entity;
            private readonly string _name;

            internal EntityProperty( Entity entity, string name )
            {
                _entity = entity;
                _name = name;
            }
        }

        public class Entity
        {
            private static readonly Dictionary<Type, Dictionary<string, Action<Entity, string>>> _sPropertyActions;

            static Entity()
            {
                _sPropertyActions = new Dictionary<Type, Dictionary<string, Action<Entity, string>>>();
                
                foreach ( var type in Assembly.GetExecutingAssembly().GetTypes() )
                {
                    if ( !typeof(Entity).IsAssignableFrom( type ) ) continue;
                    BuildPropertyActions( type );
                }
            }

            private static Dictionary<string, Action<Entity, string>> BuildPropertyActions( Type type )
            {
                Dictionary<string, Action<Entity, string>> actions;
                if ( _sPropertyActions.TryGetValue( type, out actions ) ) return actions;

                _sPropertyActions.Add( type, actions = new Dictionary<string, Action<Entity, string>>(StringComparer.InvariantCultureIgnoreCase) );
                if ( type.BaseType != null && typeof(Entity).IsAssignableFrom( type.BaseType ) )
                {
                    foreach ( var pair in BuildPropertyActions( type.BaseType ) )
                    {
                        actions.Add( pair.Key, pair.Value );
                    }
                }

                foreach ( var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public) )
                {
                    if ( property.DeclaringType != type ) continue;

                    var attrib = property.GetCustomAttribute<EntityFieldAttribute>();
                    if ( attrib == null ) continue;

                    actions.Add( attrib.Name, BuildPropertyAction( type, property ) );
                }

                return actions;
            }

            // ReSharper disable UnusedMember.Local
            // ReSharper disable MemberCanBePrivate.Local
            public static class Converters
            {
                public static string ToString( string param )
                {
                    return param;
                }

                public static int ToInt32(string param)
                {
                    return param == null ? 0
                        : int.TryParse(param, out var intVal) ? intVal
                        : float.TryParse(param, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? (int)value : 0;
                }

                public static bool ToBoolean( string param )
                {
                    return param != null && ToInt32( param ) != 0;
                }

                public static float ToSingle( string param )
                {
                    return param == null ? 0f : float.TryParse(param.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : 0f;
                }

                public static Vector3 ToVector3( string param )
                {
                    var split0 = param.IndexOf(' ');
                    var split1 = split0 == -1 ? -1 : param.IndexOf(' ', split0 + 1);

                    var x = split0 == -1 ? ToSingle(param) : ToSingle(param.Substring(0, split0));
                    var y = split0 == -1 ? 0f : split1 == -1 ? ToSingle(param.Substring(split0 + 1)) : ToSingle(param.Substring(split0 + 1, split1 - split0 - 1));
                    var z = split1 == -1 ? 0f : ToSingle(param.Substring(split1 + 1));

                    return new Vector3(x, y, z);
                }

                public static Color32 ToColor32( string param )
                {
                    if ( param == null ) return new Color32( 0x00, 0x00, 0x00 );

                    var split0 = param.IndexOf( ' ' );
                    var split1 = split0 == -1 ? -1 : param.IndexOf( ' ', split0 + 1 );
                    var split2 = split1 == -1 ? -1 : param.IndexOf( ' ', split1 + 1 );
                    
                    var r = split0 == -1 ? ToInt32( param ) : ToInt32( param.Substring( 0, split0 ) );
                    var g = split0 == -1 ? 0 : split1 == -1 ? ToInt32( param.Substring( split0 + 1 ) ) : ToInt32( param.Substring( split0 + 1, split1 - split0 - 1 ) );
                    var b = split1 == -1 ? 0 : split2 == -1 ? ToInt32( param.Substring( split1 + 1 ) ) : ToInt32( param.Substring( split1 + 1, split2 - split1 - 1 ) );
                    var a = split2 == -1 ? 255 : ToInt32( param.Substring( split2 + 1 ) );

                    return new Color32( (byte) r, (byte) g, (byte) b, (byte) a );
                }
            }
            // ReSharper restore MemberCanBePrivate.Local
            // ReSharper restore UnusedMember.Local

            private static Expression GetConversionExpression( Expression param, Type destType )
            {
                var method = typeof(Converters).GetMethods( BindingFlags.Public | BindingFlags.Static )
                    .FirstOrDefault( x =>
                        x.ReturnType == destType && x.GetParameters().Length == 1 &&
                        x.GetParameters()[0].ParameterType == typeof(string) );

                if (method == null) throw new NotImplementedException($"Unable to convert an entity field to type '{destType}'.");

                return Expression.Call( method, param );
            }

            private static Action<Entity, string> BuildPropertyAction( Type entityType, PropertyInfo property )
            {
                var entityParam = Expression.Parameter( typeof(Entity), "entity" );
                var castedEntity = Expression.Convert( entityParam, entityType );
                var valueParam = Expression.Parameter( typeof(string), "value" );
                var converted = GetConversionExpression( valueParam, property.PropertyType );
                var call = Expression.Call( castedEntity, property.SetMethod, converted );

                return Expression.Lambda<Action<Entity, string>>( call, entityParam, valueParam ).Compile();
            }

            private readonly Dictionary<string, string> _properties = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            [EntityField("classname")]
            public string ClassName { get; private set; }
            
            [EntityField("targetname")]
            public string TargetName { get; private set; }

            [EntityField("origin")]
            public Vector3 Origin { get; private set; }

            [EntityField("angles")]
            public Vector3 Angles { get; private set; }
            [EntityField("StartDisabled")]
            public int StartDisabled { get; private set; }

            [EntityField("rendermode")]
            public int RenderMode { get; private set; }
            [EntityField("spawnflags")]
            public int SpawnFlags { get; private set; }
            [EntityField("parentname")]
            public string ParentName { get; private set; }

            public IEnumerable<string> PropertyNames => _properties.Keys;

            public EntityProperty this[ string name ]
            {
                get { return new EntityProperty( this, name ); }
            }

            public string GetRawPropertyValue( string name )
            {
                return _properties.TryGetValue( name, out var value ) ? value : null;
            }

            internal int _uniqueIdx;

            internal void Initialize( KeyValues props )
            {
                foreach ( var pair in props )
                {
                    var key = _properties.ContainsKey(pair.Key)
                        ? pair.Key + "_" + _uniqueIdx
                        : pair.Key;
                    _uniqueIdx++;
                    _properties.Add(key, pair.Value);
                }

                if ( !_sPropertyActions.TryGetValue( GetType(), out var actions ) ) return;

                foreach ( var pair in props )
                {
                    if ( actions.TryGetValue( pair.Key, out var action ) ) action( this, pair.Value );
                }
            }

            public override string ToString()
            {
                return ClassName;
            }
        }

        [EntityClass("worldspawn")]
        public class Worldspawn : FuncBrush
        {
            [EntityField("skyname")]
            public string SkyName { get; private set; }

            public Worldspawn() : base(0) { }
        }
        
        [EntityClass("func_brush")]
        public class FuncBrush : Entity
        {
            [EntityField("model")]
            public string Model { get; private set; }
            [EntityField("Solidity")]
            public int Solidity { get; private set; }

            public bool Solid => Solidity != 1;

            public FuncBrush() { }

            public FuncBrush( int model )
            {
                Model = $"*{model}";
            }
        }

        [EntityClass("func_lod")]
        public class FuncLod : Entity
        {
            [EntityField("DisappearDist")]
            public int DissapearDistance { get; private set; }
            [EntityField("Solid")]
            public int Solidity { get; private set; }

            public bool Solid => Solidity == 0;

            public FuncLod() { }
        }

        [EntityClass("trigger_gravity")]
        public class TriggerGravity : Entity
        {
            [EntityField("gravity")]
            public float Gravity { get; private set; }

            public TriggerGravity() { }
        }

        [EntityClass("func_areaportal")]
        public class FuncAreaPortal : Entity
        {
            [EntityField("portalnumber")]
            public int PortalNumber { get; private set; }

            [EntityField("target")]
            public string Target { get; private set; }
        }
        
        [EntityClass("func_areaportalwindow")]
        public class FuncAreaPortalWindow : FuncAreaPortal { }

        [EntityClass("info_player_start")]
        public class InfoPlayerStart : Entity { }

        [EntityClass("info_player_terrorist")]
        public class InfoPlayerTerrorist : InfoPlayerStart { }

        [EntityClass("info_player_counterterrorist")]
        public class InfoPlayerCounterTerrorist : InfoPlayerStart { }
        
        [EntityClass("env_fog_controller")]
        public class EnvFogController : Entity
        {
            [EntityField("fogenable")]
            public bool FogEnable { get; private set; }

            [EntityField("fogstart")]
            public float FogStart {get; private set; }
            
            [EntityField("fogend")]
            public float FogEnd { get; private set; }
            
            [EntityField("fogmaxdensity")]
            public float FogMaxDensity { get; private set; }
            
            [EntityField("farz")]
            public float FarZ { get; private set; }
            
            [EntityField("fogcolor")]
            public Color32 FogColor { get; private set; }
            
            [EntityField("fogblend")]
            public bool FogBlend { get; private set; }

            [EntityField("fogcolor2")]
            public Color32 FogColor2 { get; private set; }

            [EntityField("fogdir")]
            public Vector3 FogDir { get; private set; }

            [EntityField("use_angles")]
            public bool UseAngles { get; private set; }
        }

        [EntityClass("sky_camera")]
        public class SkyCamera : EnvFogController
        {
            [EntityField("scale")]
            public int Scale { get; private set; }
        }

        [EntityClass("info_ladder")]
        public class RInfoLadder : Entity
        {
            [EntityField("mins.x")]
            public float MinX { get; private set; }
            [EntityField("mins.y")]
            public float MinY { get; private set; }
            [EntityField("mins.z")]
            public float MinZ { get; private set; }
            [EntityField("maxs.x")]
            public float MaxX { get; private set; }
            [EntityField("maxs.y")]
            public float MaxY { get; private set; }
            [EntityField("maxs.z")]
            public float MaxZ { get; private set; }

            public RInfoLadder() { }
        }

        [EntityClass("prop_dynamic")]
        public class RPropDynamic : Entity
        {
            [EntityField("model")]
            public string Model { get; private set; }
            [EntityField("solid")]
            public int Collisions { get; private set; }
            public bool Solid => Collisions > 0;
        }

        [EntityClass("prop_dynamic_override")]
        public class RPropDynamicOverride : Entity
        {
            [EntityField("model")]
            public string Model { get; private set; }
            [EntityField("solid")]
            public int Collisions { get; private set; }
            public bool Solid => Collisions > 0;
        }

        [EntityClass("light_environment")]
        public class LightEnvironment : Entity
        {
            [EntityField("_ambient")]
            public Color32 Ambient { get; private set; }
            [EntityField("_ambienthdr")]
            public Color32 AmbientHDR { get; private set; }
            [EntityField("_Ambientscalehdr")]
            public float AmbientScaleHDR { get; private set; }
            [EntityField("_light")]
            public Color32 Light { get; private set; }
            [EntityField("_lighthdr")]
            public Color32 LightHDR { get; private set; }
            [EntityField("_lightscalehdr")]
            public float LightScaleHDR { get; private set; }
            [EntityField("pitch")]
            public int Pitch { get; private set; }
        }

        [EntityClass("light")]
        public class Light : Entity
        {
            [EntityField("style")]
            public int Style { get; private set; }
            [EntityField("_quadratic_attn")]
            public float Quadratic { get; private set; }
            [EntityField("_linear_attn")]
            public float Linear { get; private set; }
            [EntityField("_constant_attn")]
            public float Constant { get; private set; }
            [EntityField("_zero_percent_distance")]
            public float ZeroPercentDistance { get; private set; }
            [EntityField("_fifty_percent_distance")]
            public float FiftyPercentDistance { get; private set; }
            [EntityField("_distance")]
            public float Distance { get; private set; }
            [EntityField("_light")]
            public Color32 Color { get; private set; }
            [EntityField("_lighthdr")]
            public Color32 ColorHDR { get; private set; }
            [EntityField("_lightscalehdr")]
            public float LightScaleHDR { get; private set; }
        }

        [EntityClass("filter_activator_name")]
        public class FilterActivatorName : Entity
        {
            [EntityField("negated")]
            public int Negated { get; private set; }
            [EntityField("filtername")]
            public string FilterName { get; private set; }

            public bool AllowMatch => Negated == 0;
        }

        [EntityClass("filter_activator_class")]
        public class FilterActivatorClass : Entity
        {
            [EntityField("negated")]
            public int Negated { get; private set; }
            [EntityField("filterclass")]
            public string FilterClass { get; private set; }

            public bool AllowMatch => Negated == 0;
        }

        [EntityClass("filter_multi")]
        public class FilterMulti : Entity
        {
            [EntityField("negated")]
            public bool Negated { get; private set; }
            /// <summary>
            /// 0 = and (all filters must pass)
            /// 1 = or (any filter must pass)
            /// </summary>
            [EntityField("filtertype")]
            public int FilterType { get; private set; }
            [EntityField("filter01")]
            public string Filter01 { get; private set; }
            [EntityField("filter02")]
            public string Filter02 { get; private set; }
            [EntityField("filter03")]
            public string Filter03 { get; private set; }
            [EntityField("filter04")]
            public string Filter04 { get; private set; }
            [EntityField("filter05")]
            public string Filter05 { get; private set; }
        }

        [EntityClass("trigger_teleport")]
        public class TriggerTeleport : Entity
        {
            [EntityField("target")]
            public string Target { get; private set; }
            [EntityField("filtername")]
            public string FilterName { get; private set; }
            [EntityField("landmark")]
            public string Landmark { get; private set; }
        }

        [EntityClass("trigger_push")]
        public class TriggerPush : Entity
        {
            [EntityField("speed")]
            public float PushSpeed { get; private set; }
            [EntityField("pushdir")]
            public Vector3 PushDirection { get; private set; }
            [EntityField("filtername")]
            public string FilterName { get; private set; }
        }

        [EntityClass("func_breakable")]
        public class FuncBreakable : Entity
        {
            [EntityField("health")]
            public int Health { get; private set; }
        }

        [EntityClass("func_rotating")]
        public class FuncRotating : Entity
        {
            [EntityField("fanfriction")]
            public float Friction { get; private set; }
            [EntityField("maxspeed")]
            public float MaxSpeed { get; private set; }

            public bool XAxis => (SpawnFlags & 4) == 4;
            public bool YAxis => (SpawnFlags & 8) == 8;
        }

        [EntityClass("func_door")]
        public class FuncDoor : Entity
        {
            [EntityField("wait")]
            public float DelayBeforeReset { get; private set; }
            [EntityField("speed")]
            public float Speed { get; private set; }
            [EntityField("movedir")]
            public Vector3 MoveDir { get; private set; }
            [EntityField("spawnpos")]
            public int SpawnPosition { get; private set; }

            public bool DefaultIsClosed => SpawnPosition == 0;
        }

        [EntityClass("func_movelinear")]
        public class FuncMoveLinear : FuncDoor
        {
            [EntityField("movedistance")]
            public float MoveDistance { get; private set; }
        }

        [EntityClass("logic_timer")]
        public class LogicTimer : Entity
        {
            [EntityField("UseRandomTime")]
            public int UseRandomTime { get; private set; }
            [EntityField("LowerRandomBound")]
            public float LowerRandomBound { get; private set; }
            [EntityField("UpperRandomBound")]
            public float UpperRandomBound { get; private set; }
            [EntityField("RefireTime")]
            public float RefireInterval { get; private set; }
        }

        [EntityClass("math_counter")]
        public class MathCounter : Entity
        {
            [EntityField("min")]
            public int Min { get; private set; }
            [EntityField("max")]
            public int Max { get; private set; }
            [EntityField("startvalue")]
            public int Start { get; private set; }
        }

        [EntityClass("func_door_rotating")]
        public class FuncDoorRotating : Entity
        {
            [EntityField("distance")]
            public float Distance { get; private set; }
            [EntityField("speed")]
            public float Speed { get; private set; }
            [EntityField("lip")]
            public float Lip { get; private set; }
            [EntityField("dmg")]
            public float BlockingDamage { get; private set; }
            [EntityField("wait")]
            public float DelayBeforeReset { get; private set; }
            [EntityField("spawnpos")]
            public int SpawnPosition { get; private set; }

            public bool XAxis => (SpawnFlags & 4) == 4;
            public bool YAxis => (SpawnFlags & 8) == 8;
            public bool DefaultIsClosed => SpawnPosition == 0;
        }

        [EntityClass("func_button")]
        public class FuncButton : Entity
        {
            [EntityField("wait")]
            public float DelayBeforeReset { get; private set; }
            [EntityField("speed")]
            public float Speed { get; private set; }
            [EntityField("movedir")]
            public Vector3 MoveDir { get; private set; }
        }

    }
}
