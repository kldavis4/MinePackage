Shader "!Debug/Vertex color" {
SubShader {
    Pass {
        Fog { Mode Off }
CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct appdata members vertex,color)
#pragma exclude_renderers d3d11 xbox360
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it does not contain a surface program or both vertex and fragment programs.
#pragma exclude_renderers gles
// Upgrade NOTE: excluded shader from Xbox360; has structs without semantics (struct appdata members vertex,color)
#pragma exclude_renderers xbox360
#pragma vertex vert

// vertex input: position, color
struct appdata {
    float4 vertex;
    float4 color;
};

struct v2f {
    float4 pos : POSITION;
    float4 color : COLOR;
};
v2f vert (appdata v) {
    v2f o;
    o.pos = mul( UNITY_MATRIX_MVP, v.vertex );
    o.color = v.color;
    return o;
}
ENDCG
    }
}
}
