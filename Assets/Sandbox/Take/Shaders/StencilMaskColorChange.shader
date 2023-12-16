Shader "Unlit/StencilMaskColorChange"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }
        
        Pass
        {
            ColorMask 0
            Stencil
            {
                Ref 2
                Comp Equal
                Pass IncrSat
            }
        }
    }
}
