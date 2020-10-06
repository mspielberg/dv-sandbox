using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace DvMod.Sandbox
{
    public static class HandBrakeWheel
    {
        private readonly struct WheelPosition
        {
            public readonly Vector3 position;
            public readonly bool needsLod;

            public WheelPosition(Vector3 position, bool needsLod)
            {
                this.position = position;
                this.needsLod = needsLod;
            }
        }

        private static readonly Dictionary<TrainCarType, WheelPosition> wheelPositions = new Dictionary<TrainCarType, WheelPosition>()
        {
            [TrainCarType.FlatbedEmpty] = new WheelPosition(new Vector3(-1.37f, 0.81f, 1f), true),
            [TrainCarType.FlatbedMilitary] = new WheelPosition(new Vector3(-1.37f, 0.81f, 1f), true),
            [TrainCarType.FlatbedStakes] = new WheelPosition(new Vector3(-1.37f, 0.81f, 1f), true),
        };

        private static GameObject? lodWheel;
        private static GameObject? wheelInteractionArea;
        private static GameObject? wheelControl;

        private static void GetCabooseAssets()
        {
            if (lodWheel != null && wheelInteractionArea != null && wheelControl != null)
                return;
            var caboosePrefab = CarTypes.GetCarPrefab(TrainCarType.CabooseRed);
            lodWheel = caboosePrefab.transform.Find("[interior LOD]/BrakeWheel").gameObject;
            var cabooseInteriorPrefab = caboosePrefab.GetComponent<TrainCar>().interiorPrefab;
            wheelInteractionArea = cabooseInteriorPrefab.transform.Find("IA BrakeWheel").gameObject;
            wheelControl = cabooseInteriorPrefab.transform.Find("C BrakeWheel").gameObject;
        }

        public static void AddWheelToCar(TrainCar car)
        {
            if (!wheelPositions.TryGetValue(car.carType, out var wheelPosition))
                return;
            GetCabooseAssets();
            var cabInput = car.gameObject.AddComponent<CabInputCaboose>();

            var interactionArea = Object.Instantiate(wheelInteractionArea, car.transform)!;
            interactionArea.transform.localPosition = wheelPosition.position;
            // interactionArea.SetLayersRecursive("Interactable_In_Cab");
            interactionArea.transform.Find("IA_collider").GetComponent<SphereCollider>().isTrigger = true;

            var control = Object.Instantiate(
                wheelControl,
                car.transform.TransformPoint(wheelPosition.position),
                Quaternion.LookRotation(car.transform.up, car.transform.right),
                car.transform)!;
            // control.transform.localScale = Vector3.one * 0.7f;
            // control.transform.localPosition = wheelPosition.position;
            // control.GetComponent<HingeJoint>().connectedAnchor = wheelPosition.position;
            control.SetLayersRecursive("Interactable_In_Cab");
            foreach (var collider in control.transform.GetComponentsInChildren<Collider>())
                collider.isTrigger = true;
            // control.SetActive(false);
            cabInput.independentBrake = control;
            Debug.Log($"control position: {control.transform.localPosition}");
            var hj = control.GetComponent<HingeJoint>();
            Debug.Log($"hinge joint: anchor={hj.anchor}, body={hj.connectedBody}, connectedAnchor={hj.connectedAnchor}");
        }

        [HarmonyPatch(typeof(TrainCar), nameof(TrainCar.Awake))]
        public static class AwakePatch
        {
            public static void Postfix(TrainCar __instance)
            {
            }
        }
    }
}