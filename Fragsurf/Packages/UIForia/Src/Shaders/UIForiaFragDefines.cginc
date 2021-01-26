#ifdef UIFORIA_USE_INSTANCING

     #define m_Size UNITY_ACCESS_INSTANCED_PROP(Props, _SizeRotationGradientStart).xy
     #define m_ClipRect UNITY_ACCESS_INSTANCED_PROP(Props, _ClipRect)

     #define m_FillRotation UNITY_ACCESS_INSTANCED_PROP(Props, _SizeRotationGradientStart).z
     #define m_GradientStart UNITY_ACCESS_INSTANCED_PROP(Props, _SizeRotationGradientStart).w
     #define m_GradientAxis 1 
     
     #define m_FillColor UNITY_ACCESS_INSTANCED_PROP(Props, _PrimaryColor)
     #define m_FillColor2 UNITY_ACCESS_INSTANCED_PROP(Props, _SecondaryColor)
     #define m_FillOffset UNITY_ACCESS_INSTANCED_PROP(Props, _FillOffsetAndScale).xy
     #define m_FillScale UNITY_ACCESS_INSTANCED_PROP(Props, _FillOffsetAndScale).zw
     
     #define m_BorderColor UNITY_ACCESS_INSTANCED_PROP(Props, _BorderColor)
     #define m_BorderRadius UNITY_ACCESS_INSTANCED_PROP(Props, _BorderRadius)
     #define m_BorderSize UNITY_ACCESS_INSTANCED_PROP(Props, _BorderSize)
     
     #define m_GridSize UNITY_ACCESS_INSTANCED_PROP(Props, _GridAndLineSize).xy
     #define m_LineSize UNITY_ACCESS_INSTANCED_PROP(Props, _GridAndLineSize).zw
     #define m_FillTexture _MainTex
     // todo change this
     #define m_ContentRect float4(0, 0, 100, 100) 
#else
     
     #define m_Size _Size
     #define m_ClipRect _ClipRect
     #define m_ContentRect _ContentRect
     #define m_FillRotation 0
     #define m_GradientStart 0
     #define m_GradientAxis 0
     
     #define m_FillColor _Color
     #define m_FillColor2 _Color
     #define m_FillOffset _Color
     #define m_FillScale _Color
     
     #define m_BorderColor _BorderColor
     #define m_BorderSize _BorderSize
     #define m_BorderRadius _BorderRadius
     
     #define m_GridSize _BorderRadius
     #define m_LineSize _BorderRadius
     #define m_FillOffset _FillOffsetScale.xy
     #define m_FillScale _FillOffsetScale.zw
     #define m_FillTexture _MainTex
     
#endif
