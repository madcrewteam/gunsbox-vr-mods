using UnityEngine;

public class GunsBoxHitSurface : MonoBehaviour
{
    public SurfaceType surfaceType = SurfaceType.Wood;

    public enum SurfaceType
    {
        Wood,
        Brick,
        Concrete,
        MetalThin,
        MetalThick,
        Cardboard,
        Glass,
        Skin
    }
}
