using UnityEngine.Pool;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Effects/Outline12", 81)]
    public class Outline12 : Shadow
    {
        protected Outline12()
        {}

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;

            var verts = ListPool<UIVertex>.Get();
            vh.GetUIVertexStream(verts);

            var neededCapacity = verts.Count * 13;
            if (verts.Capacity < neededCapacity)
                verts.Capacity = neededCapacity;

            var start = 0;
            var end = verts.Count;

            float x = effectDistance.x;
            float y = effectDistance.y;
            float hx = x * 0.5f;
            float hy = y * 0.5f;

            // 4 стороны
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, x, 0);
            start = end; end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -x, 0);
            start = end; end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, 0, y);
            start = end; end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, 0, -y);

            // 4 диагонали
            start = end; end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, x, y);
            start = end; end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, x, -y);
            start = end; end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -x, y);
            start = end; end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -x, -y);

            // 4 полудиагонали
            start = end; end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, hx, y);
            start = end; end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, hx, -y);
            start = end; end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -hx, y);
            start = end; end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -hx, -y);

            vh.Clear();
            vh.AddUIVertexTriangleStream(verts);
            ListPool<UIVertex>.Release(verts);
        }
    }
}