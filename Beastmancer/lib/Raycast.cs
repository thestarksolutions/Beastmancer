using RDR2;
using RDR2.Native;
using RDR2.UI;
using RDR2.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beastmancer
{
    class Raycast
    {
        /*
         * Overriding this from scripthook for now.
         * Was crashing if the hit distance was too far away.
         */
        public static Vector3 GetAimCoordinates(float max_distance = 600f)
        {
            var source = GameplayCamera.Position;
            var rotation = (float)(System.Math.PI / 180.0) * GameplayCamera.Rotation;
            var forward = Vector3.Normalize(new Vector3(
                (float)-System.Math.Sin(rotation.Z) * (float)System.Math.Abs(System.Math.Cos(rotation.X)),
                (float)System.Math.Cos(rotation.Z) * (float)System.Math.Abs(System.Math.Cos(rotation.X)),
                (float)System.Math.Sin(rotation.X)));
            var target = source + forward * max_distance;
            return RaycastHit(source, target, IntersectOptions.Everything, max_distance);
        }

        public static Vector3 RaycastHit(Vector3 source, Vector3 target, IntersectOptions options, float max_distance)
        {
            int t = Function.Call<int>(Hash._START_SHAPE_TEST_RAY, source.X, source.Y, source.Z,
                target.X, target.Y, target.Z, (int)options, 0, 7);

            var hitPos = new OutputArgument();
            var ditHit = new OutputArgument();
            var entity = new OutputArgument();
            var normal = new OutputArgument();

            int result = Function.Call<int>(Hash.GET_SHAPE_TEST_RESULT, t, ditHit, hitPos, normal, entity);
            Vector3 hit_position = hitPos.GetResult<Vector3>();
            if (source.DistanceTo(hit_position) > max_distance)
            {
                return Vector3.Zero;
            }

            return hit_position;
        }
    }
}
