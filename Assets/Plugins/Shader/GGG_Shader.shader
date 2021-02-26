Shader "Custom/Grand Geo Grass"
{
    Properties
    {
		[Header(GRAND GEO GRASS)]
		_TopColor("Top Color", Color) = (1,1,1,1)
		_BottomColor("Bottom Color", Color) = (0,0,0,1)
        _MainTex ("Texture", 2D) = "white" {}
		_Cutoff("Cutoff", Range(0,1)) = 0.25
		_MipScale("Mip Level Alpha Scale", Range(0,1)) = 0.25

		[Header(SHAPE SETTINGS)]
		_GrassHeight("Height", Float) = 1
		_GrassWidth("Width", Float) = 1

		[Header(WIND SETTINGS)]
		[Toggle(FEATURE_WIND)] _featureWind ("Wind", int) = 0
		[ShowIf(FEATURE_WIND)] _WindTex("Wind Texture", 2D) = "white" {}
		[ShowIf(FEATURE_WIND)][HDR] _WindColor("Wind Color", Color) = (0,0,0,1)

		[Header(LIGHTING)]
		_Shininess("Specular shininess", float) = 10
		_Sharpness("Translucency Sharpness", float) = 10
		[HDR]_SpecularColour("Specular Colour", Color) = (1,1,1,1)
		[HDR]_TranslucentColour("Translucent Colour", Color) = (1,1,1,1)

		[Header(INTERACTION)]
		[Toggle(FEATURE_INTERACTION)] _featureInteractive("Interactive", int) = 0
		[ShowIf(FEATURE_INTERACTION)] _InteractionStrength("Interaction Strength", Range(0,5)) = 1

		[Header(VIEW ANGLE BENDING)]
		[Toggle(FEATURE_VIEWBENDING)] _featureViewBending("View Bending", int) = 0
		[ShowIf(FEATURE_VIEWBENDING)] _ViewBendStrength("Bending Strength", Range(0,5)) = 1

		[Header(DISTANCE)]
		_MaxRenderDistance("LOD Max Render Distance", Float) = 100
		_BillboardDistance("LOD Billboard Start Distance", Float) = 50
		_LODStart("LOD Start Distance", Float) = 25
    }
    SubShader
    {
		CGINCLUDE
        #pragma vertex vert
		#pragma geometry geom
        // make fog work
        #pragma multi_compile_fog
		#pragma shader_feature_local FEATURE_WIND
		#pragma shader_feature_local FEATURE_VIEWBENDING
		#pragma shader_feature_local FEATURE_INTERACTION

				
		//Shader model 4.0 - includes geometry shader support
		#pragma target 4.0
		#include "UnityCG.cginc" 
		#include "Autolight.cginc"
		#include "GGG_HELPER.hlsl"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float4 color : COLOR;
			};

			//Vertex to geometry
			struct v2g
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float4 color : COLOR;
			};

			//Geometry to fragment
			struct g2f
			{
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldPosition : TEXCOORD4;
#if UNITY_PASS_FORWARDBASE	
				float3 normal : NORMAL;
				float4 color : COLOR;
				float3 viewDir :TEXCOORD2;
				unityShadowCoord4 _ShadowCoord : TEXCOORD3;
#endif
			};

			//Constants
			static const float SIN60 = 0.866f;

			//Texture
            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _Cutoff;

			//Grass Properties
			float _GrassHeight;
			float _GrassWidth;

			//View bending
			float _ViewBendStrength;

			//LOD distances
			float _MaxRenderDistance;
			float _BillboardDistance;
			float _LODStart;

#ifdef FEATURE_WIND
			sampler2D _WindTex;
			float4 _WindTex_ST;
			//xy - direction
			//z - strength
			//w - speed
			uniform float4 _WindData;

#endif
			
#ifdef		FEATURE_INTERACTION
			uniform sampler2D _InteractionRenderTex;
			//xy - postion
			//z - size
			uniform float3 _InteractionCameraData;
			float _InteractionStrength;
#endif


			v2g vert(appdata v)
			{
				v2g o;
				o.vertex = v.vertex;
				o.uv = v.uv;
				o.tangent = v.tangent;
				o.normal = v.normal;
				o.color = v.color;

				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

			g2f AddVertex(float3 vertex, float2 uv, float3 normal, float4 color)
			{
				g2f o;
				o.vertex = UnityObjectToClipPos(vertex);
				o.uv = TRANSFORM_TEX(uv, _MainTex);
				o.worldPosition = mul(unity_ObjectToWorld, float4(vertex,1));
#if UNITY_PASS_FORWARDBASE

				o.normal = normal;
				o.color = color;
				o.viewDir = WorldSpaceViewDir(float4(vertex,1));
				o._ShadowCoord = ComputeScreenPos(o.vertex);
#elif UNITY_PASS_SHADOWCASTER

				o.vertex = UnityApplyLinearShadowBias(o.vertex);
#endif 
				return o;
			}

			void createBillboard(
				float3 position,
				float width,
				float height,
				float3 right,
				float3 up,
				float3 normal,
				float4 color,
				inout TriangleStream<g2f> tristream)
			{
				float4 vertPosition;

				vertPosition = float4(position + width * right, 1.0f);
				tristream.Append(AddVertex(vertPosition, float2(0, 0), normal, color));

				vertPosition = float4(position + width * right + height * up, 1.0f);
				tristream.Append(AddVertex(vertPosition, float2(0, 1), normal, color));

				vertPosition = float4(position - width * right, 1.0f);
				tristream.Append(AddVertex(vertPosition, float2(1, 0), normal, color));

				vertPosition = float4(position - width * right + height * up, 1.0f);
				tristream.Append(AddVertex(vertPosition, float2(1, 1), normal, color));
			}

			void createQuad(
				float3 vBottom, 
				float3 vTop, 
				float3 transform, 
				float3 normal,
				float4 color,
				inout TriangleStream<g2f> tristream)
			{
				tristream.Append(AddVertex(vBottom + transform, float2(1, 0), normal, color));
				tristream.Append(AddVertex(vTop +	 transform, float2(1, 1), normal, color));
				tristream.Append(AddVertex(vBottom - transform, float2(0, 0), normal, color));
				tristream.Append(AddVertex(vTop -	 transform, float2(0, 1), normal, color));
				tristream.RestartStrip();
			}

#define MAXVERTEXCOUNT 12

			[maxvertexcount(MAXVERTEXCOUNT)]
			void geom(point v2g IN[1], inout TriangleStream<g2f> tristream)
			{
				//Distance from camera to vertex
				float cameraDistance = length(ObjSpaceViewDir(IN[0].vertex));
//--------------------------------------------------------------------------
// DISTANCE CULLING
//--------------------------------------------------------------------------
				//Do not create new vertices if distance is higher than _MaxRenderDistance
				if (cameraDistance > _MaxRenderDistance)
					return;

				float random = rand(IN[0].vertex.xyz);
				
				float3 worldPosition = mul(unity_ObjectToWorld, IN[0].vertex);

				float4 color = IN[0].color;
				
				//that doesnt actually work ;-;
				//Smoothing normals, only used for lighting later
				float3 normal = IN[0].normal;// ((IN[0].normal + 1)*0.5);

#ifdef FEATURE_WIND
				float2 windTexSample = tex2Dlod(_WindTex, float4(worldPosition.xz * _WindTex_ST.xy + (_WindData.xy * _Time.y * _WindData.w), 0.0, 0.0)).xy;
				float2 windDisplacement = (windTexSample * 2.0 - 1.0) * _WindData.z;
				color.g = length(windTexSample);
#endif
//--------------------------------------------------------------------------
// BILLBOARDS
//--------------------------------------------------------------------------
				if (cameraDistance >= _BillboardDistance)
				{
					//Camera Up
					float3 up = UNITY_MATRIX_IT_MV[1].xyz;

#ifdef FEATURE_WIND
					up.xz += windDisplacement;
#endif
					//Camera Right
					float3 right = UNITY_MATRIX_IT_MV[0].xyz;

					//Randomize Direction
					right *= random > 0.5f ? 1 : -1;

					float width = (_GrassWidth * color.b) * 0.5;
					float height = _GrassHeight * color.r;

					createBillboard(IN[0].vertex.xyz, width, height, right, up, normal, color, tristream);
					
					//Do not do anything else
					return;
				}

				float3 v0 = IN[0].vertex.xyz;
				float3 v1 = v0 + IN[0].normal * (_GrassHeight * color.r);

#ifdef FEATURE_WIND
				v1.xz += windDisplacement;
#endif

#ifdef FEATURE_VIEWBENDING
				//Push the top vertex away from the camera to bend the grass clump
				float viewDot = dot(IN[0].normal, UNITY_MATRIX_IT_MV[2].xyz);
				v1.xz += UNITY_MATRIX_IT_MV[1].xz *_ViewBendStrength * saturate(viewDot);
#endif

#ifdef FEATURE_INTERACTION
				float2 cameraUV = 1 - ((worldPosition.xz - _InteractionCameraData.xy) / (_InteractionCameraData.z * 2) + 0.5f);
				float3 interactionTexSample = tex2Dlod(_InteractionRenderTex, float4(cameraUV, 0, 0)).rgb;
				float falloff = 1 - saturate((distance(_InteractionCameraData.xy, worldPosition.xz) - (_InteractionCameraData.z*0.2)) / (_InteractionCameraData.z));

				v1.xyz += (interactionTexSample.xyz * falloff * _InteractionStrength);
#endif

//--------------------------------------------------------------------------
// LOD 1
//--------------------------------------------------------------------------

				//Quads are build on it. It was (0,0,1) but in tangent space y and z are swapped.
				float3 baseLine = float3(0, 1, 0);

				float3 vBinormal = cross(IN[0].normal, IN[0].tangent.xyz) * IN[0].tangent.w;

				float3x3 tangentToLocal = float3x3(
					IN[0].tangent.x, vBinormal.x, IN[0].normal.x,
					IN[0].tangent.y, vBinormal.y, IN[0].normal.y,
					IN[0].tangent.z, vBinormal.z, IN[0].normal.z);


				//Randomly Rotate baseLine
				baseLine = mul(AngleAxis3x3(random * UNITY_TWO_PI, float3(0, 0, 1)), baseLine);

				baseLine *= 0.5 * (_GrassWidth * color.b);

				//Holds transformations
				float3 transform;

				//Quad 1
				transform = mul(tangentToLocal, baseLine);
				createQuad(v0, v1, transform, normal, color, tristream);

				if (cameraDistance > _LODStart)
				{
					//Quad 2
					float3x3 rotationMatrix = AngleAxis3x3(0.25 * UNITY_TWO_PI, float3(0, 0, 1));
					float3x3 transformMatrix = mul(tangentToLocal, rotationMatrix);
					transform = mul(transformMatrix, baseLine);
					createQuad(v0, v1, transform, normal, color, tristream);

					return;
				}

//--------------------------------------------------------------------------
// LOD 0
//--------------------------------------------------------------------------

				//Quad 2
				float3x3 rotationMatrix = AngleAxis3x3(SIN60 * UNITY_TWO_PI, float3(0, 0, 1));
				float3x3 transformMatrix = mul(tangentToLocal, rotationMatrix);
				transform = mul(transformMatrix, baseLine);
				createQuad(v0, v1, transform, normal, color, tristream);

				//Quad 3
				rotationMatrix = AngleAxis3x3(SIN60 + 1 * UNITY_TWO_PI, float3(0, 0, 1));
				transformMatrix = mul(tangentToLocal, rotationMatrix);
				transform = mul(transformMatrix, baseLine);
				createQuad(v0, v1, transform, normal, color, tristream);

			}

		ENDCG

        Pass
        {
			Tags {"RenderQueue" = "AlphaTest" "RenderType" = "TransparentCutout" "LightMode" = "ForwardBase"}
			Cull Off
			AlphaToMask On
            CGPROGRAM
            #pragma fragment frag
			#pragma multi_compile_fwdbase

			#pragma target 4.6

			#include "Lighting.cginc"

			float4 _MainTex_TexelSize;
			half _MipScale;

			float4 _TopColor;
			float4 _BottomColor;
			float4 _WindColor;

			float4 _TranslucentColour;
			float4 _SpecularColour;
			float _Sharpness;
			float _Shininess;


            fixed4 frag (g2f i) : SV_Target
            {
				float4 gradient = lerp(_BottomColor, _TopColor, i.uv.y);
				
                // sample the texture
                float4 texColor = tex2D(_MainTex, i.uv);

				float4 outColor = gradient;

#ifdef FEATURE_WIND
				outColor += (_WindColor * i.color.g * i.uv.y);
#endif
				outColor *= texColor;
				outColor.a = texColor.a;

				//Makes the edges smoother when using AlphaToCoverage
				outColor.a *= 1 + max(0, CalcMipLevel(i.uv * _MainTex_TexelSize.zw))*_MipScale;
				outColor.a = (outColor.a - _Cutoff) / max(fwidth(outColor.a), 0.0001) + 0.5;

				float3 normal = normalize(i.normal);

				
//--------------------------------------------------------------------------
// LIGHTING
//--------------------------------------------------------------------------
				float3 lightDirection = normalize(_WorldSpaceLightPos0).rgb;
				float3 viewDir = normalize(i.viewDir);
				float shadow = SHADOW_ATTENUATION(i);

				float3 ambientLighting = ShadeSH9(float4(normal, 1));
				float3 diffuseReflection = _LightColor0.rgb * max(0.0, dot(normal, lightDirection));
				float3 specularReflection = _LightColor0.rgb * _SpecularColour * pow(max(0.0, saturate(dot(reflect(-lightDirection, normal), viewDir))), _Shininess);
				float3 diffuseTranslucency = _LightColor0.rgb * _TranslucentColour * max(0.0, dot(lightDirection, -normal));
				float3 forwardTranslucency = _LightColor0.rgb * _TranslucentColour * pow(max(0.0, saturate(dot(-lightDirection, viewDir))), _Sharpness);

				outColor.rgb *= diffuseReflection + (specularReflection + diffuseTranslucency + forwardTranslucency) * i.uv.y;
				outColor.rgb *= _LightColor0 * shadow + ambientLighting;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, outColor);
                
				return outColor;
            }
            ENDCG
        }

//--------------------------------------------------------------------------
// Additional Lights Pass
//--------------------------------------------------------------------------
		Pass
		{
			Tags {"LightMode" = "ForwardAdd"}
			Cull Off
			AlphaToMask On
			Blend OneMinusDstColor One
			ZWrite Off


			CGPROGRAM
			#pragma fragment fragAdd
			
			#include "Lighting.cginc"

			#pragma target 4.6

			#pragma multi_compile_fwdadd_fullforwardshadows

			float4 _MainTex_TexelSize;
			half _MipScale;

			float4 fragAdd(g2f i) : SV_Target
			{
				UNITY_LIGHT_ATTENUATION(atten, i, i.worldPosition);

				float3 pointlights = atten * _LightColor0.rgb;

				fixed4 col = tex2D(_MainTex, i.uv);

				//Makes the edges smoother when using AlphaToCoverage
				col.a *= 1 + max(0, CalcMipLevel(i.uv * _MainTex_TexelSize.zw))*_MipScale;
				col.a = (col.a - _Cutoff) / max(fwidth(col.a), 0.0001) + 0.5;

				return float4(pointlights, col.a);
			}

			ENDCG
		}

//--------------------------------------------------------------------------
// ShadowCaster Pass
//--------------------------------------------------------------------------
		Pass
		{
			Tags {"LightMode" = "ShadowCaster"}
			Cull Off
			//AlphaToMask On

			CGPROGRAM
			#pragma fragment fragShadow

			#pragma target 4.6
			#pragma multi_compile_shadowcaster

			float4 fragShadow(g2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				clip(col.a - _Cutoff);

				SHADOW_CASTER_FRAGMENT(i);
			}

			ENDCG
		}
    }
}
