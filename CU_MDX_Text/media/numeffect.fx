//--------------------------------------------------------------------------------------
// File: EnhancedMesh.fx
//
// The effect file for the EnhancedMesh sample.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------
// Global variables
//--------------------------------------------------------------------------------------
float	   g_fTime;					// App's time in seconds
float4x4 g_mWorld;					// World matrix for object
float4x4 g_mViewProjection;	// View * Projection matrix
float4   g_matDiffuse;                // Material diffuse color
float4   g_lightPos;
float4   g_light2Pos;
float4   g_lightAmbient;


/*
texture  g_txScene;


sampler g_samScene =
sampler_state
{
    Texture = <g_txScene>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};
*/

void VertScene( float3 Pos : POSITION,
                float3 Normal : NORMAL,
                out float4 oPos : POSITION,
                out float3 oWorldPos : TEXCOORD0,
                out float3 oWorldNorm : TEXCOORD1)
{
    float4 wpos = mul(float4(Pos, 1), g_mWorld);
    oPos = mul(wpos, g_mViewProjection);
    oWorldPos = wpos.xyz;
    oWorldNorm = normalize( mul( Normal, (float3x3)g_mWorld ) );
}


float4 PixScene( 
  float3 WorldPos : TEXCOORD0,
  float3 WorldNorm: TEXCOORD1 ) : COLOR0
{
  float3 invLightDir = normalize(g_lightPos.xyz - WorldPos);
  float3 invLight2Dir = normalize(g_light2Pos.xyz - WorldPos);
  
  float light = saturate( dot( WorldNorm, invLightDir ) );
  float light2 = saturate( dot( WorldNorm, invLight2Dir ) );  
  float3 col = g_matDiffuse.xyz * g_lightAmbient.x;
  col += g_matDiffuse.xyz * (light + light2);
  col += float3(1,1,1) * (pow(light, 100) + pow(light2, 100));

  return float4(col,1);
}
                

//--------------------------------------------------------------------------------------
// Techniques
//--------------------------------------------------------------------------------------
technique RenderScene
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertScene();
        PixelShader = compile ps_2_0 PixScene();
    }
}
