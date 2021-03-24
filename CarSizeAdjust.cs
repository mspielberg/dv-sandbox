using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
namespace DvMod.Sandbox
{
    public static class PassengerCarSizeAdjust
    {
        private const float SizeMultiplier = 0.8f;

        private static bool IsPassengerCar(TrainCar car)
        {
            return car.carType == TrainCarType.PassengerBlue || car.carType == TrainCarType.PassengerGreen || car.carType == TrainCarType.PassengerRed;
        }

        public static void AdjustTransforms(TrainCar car)
        {
            car.transform.localScale = new Vector3(1, 1, SizeMultiplier);
            // Debug.Log($"start AdjustTransforms: {car.gameObject.DumpHierarchy()}");

            // DFS
            var stack = new List<Transform>();
            stack.AddRange(car.transform.OfType<Transform>());
            while (stack.Count > 0)
            {
                var transform = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
                switch (transform.name)
                {
                    case "car_passenger_lod":
                    case "[colliders]":
                        continue;
                }
                if (transform.localPosition == Vector3.zero)
                {
                    // Debug.Log($"treating {transform.GetPath()} as parent: sqrMagnitude = {transform.localPosition.sqrMagnitude}");
                    stack.AddRange(transform.OfType<Transform>());
                }
                else
                {
                    // Debug.Log($"Adjusting transform {transform.GetPath()}: sqrMagnitude = {transform.localPosition.sqrMagnitude}");
                    transform.localScale = new Vector3(1, 1, 1f / SizeMultiplier);
                }
            }
        }

        [HarmonyPatch(typeof(TrainCar), nameof(TrainCar.Awake))]
        public static class AwakePatch
        {
            public static void Prefix(TrainCar __instance)
            {
                if (IsPassengerCar(__instance))
                    AdjustTransforms(__instance);
            }
        }

        [HarmonyPatch(typeof(TrainCar), nameof(TrainCar.AwakeForPooledCar))]
        public static class AwakeForPooledCarPatch
        {
            public static void Prefix(TrainCar __instance)
            {
                if (IsPassengerCar(__instance))
                    AdjustTransforms(__instance);
            }
        }

        [HarmonyPatch(typeof(CarTypes), nameof(CarTypes.GetCarPrefab))]
        public static class GetCarPrefabPatch
        {
            public static void Postfix(ref GameObject __result)
            {
                if (__result == null)
                    return;
                var car = __result.GetComponent<TrainCar>();
                if (IsPassengerCar(car))
                {
                    AdjustTransforms(car);
                    // Debug.Log($"From GetCarPrefab: {__result.DumpHierarchy()}");
                }
            }
        }
    }

    [HarmonyPatch(typeof(TrainCar), nameof(TrainCar.Bounds), MethodType.Getter)]
    public static class BoundsPatch
    {
        public static bool Prefix(TrainCar __instance, ref Bounds __result, ref Bounds ____bounds)
        {
            if (____bounds.size.z == 0.0f || !Application.isPlaying)
            {
                ____bounds = TrainCarColliders.GetCollisionBounds(__instance);
                // Debug.Log($"Computed collision bounds for {__instance.carType}: {____bounds}");
                ____bounds.Encapsulate(Vector3.Scale(__instance.FrontCouplerAnchor.localPosition, __instance.FrontCouplerAnchor.parent.lossyScale));
                ____bounds.Encapsulate(Vector3.Scale(__instance.RearCouplerAnchor.localPosition, __instance.RearCouplerAnchor.parent.lossyScale));
                // Debug.Log($"Computed bounds for {__instance.carType}: {____bounds}");
            }
            __result = ____bounds;
            return false;
        }
    }
}
*/