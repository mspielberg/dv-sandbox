using CommandTerminal;
using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace DvMod.Sandbox
{
    public static class Commands
    {
        [HarmonyPatch(typeof(Terminal), nameof(Terminal.Start))]
        public static class RegisterCommandsPatch
        {
            public static void Postfix()
            {
                Register();
            }
        }

        private static void Register(string name, Action<CommandArg[]> proc)
        {
            if (Terminal.Shell == null)
                return;
            if (Terminal.Shell.Commands.Remove(name.ToUpper()))
                Main.DebugLog($"replacing existing command {name}");
            else
                Terminal.Autocomplete.Register(name);
            Terminal.Shell.AddCommand(name, proc);
        }

        public static void Register()
        {
            Register("sandbox.dumpCar", _ =>
            {
                if (PlayerManager.Car != null)
                {
                    var dump = PlayerManager.Car.gameObject.DumpHierarchy();
                    Debug.Log(dump);
                }
            });

            Register("sandbox.dumpInterior", _ =>
            {
                if (PlayerManager.Car?.loadedInterior != null)
                {
                    var dump = PlayerManager.Car.loadedInterior.DumpHierarchy();
                    Debug.Log(dump);
                }
            });

            Register("sandbox.movewheel", args =>
            {
                var wheelTransform = PlayerManager.Car?.transform?.Find("C BrakeWheel(Clone)");
                if (wheelTransform == null)
                {
                    Terminal.Log("Current car has no brake wheel");
                    return;
                }
                var hj = wheelTransform.GetComponent<HingeJoint>();
                hj.autoConfigureConnectedAnchor = false;
                hj.connectedAnchor = new Vector3(args[0].Float, args[1].Float, args[2].Float);
                wheelTransform.localPosition = new Vector3(args[0].Float, args[1].Float, args[2].Float);
            });

            Register("sandbox.disableobject", args =>
            {
                var transform = PlayerManager.Car.transform;
                foreach (var t in transform.GetComponentsInChildren<Transform>(includeInactive: true).Where(c => c.name.Contains(args[0].String)))
                {
                    t.gameObject.SetActive(false);
                    Terminal.Log($"Set inactive on {t.GetPath()}");
                }
            });

            Register("sandbox.enableobject", args =>
            {
                var transform = PlayerManager.Car.transform;
                foreach (var t in transform.GetComponentsInChildren<Transform>(includeInactive: true).Where(c => c.name.Contains(args[0].String)))
                {
                    t.gameObject.SetActive(true);
                    Terminal.Log($"Set active on {t.GetPath()}");
                }
            });

            Register("sandbox.disablecomponent", args =>
            {
                var transform = PlayerManager.Car.transform;
                foreach (var comp in transform.GetComponentsInChildren<MonoBehaviour>(includeInactive: true).Where(c => c.GetType().Name == args[0].String))
                {
                    comp.enabled = false;
                    Terminal.Log($"Disabled {comp.GetPath()} {comp.GetType()}");
                }
            });

            Register("sandbox.disablecar", _ => PlayerManager.Car.gameObject.SetActive(false));

            Register("sandbox.disableallcomponents", _ =>
            {
                var transform = PlayerManager.Car.transform;
                foreach (var comp in transform.GetComponentsInChildren<MonoBehaviour>(includeInactive: true))
                {
                    comp.enabled = false;
                    Terminal.Log($"Disabled {comp.GetPath()} {comp.GetType()}");
                }
            });

            Register("sandbox.destroycomponent", args =>
            {
                var transform = PlayerManager.Car.transform;
                foreach (var comp in transform.GetComponentsInChildren<Component>(includeInactive: true).Where(c => c.GetType().Name == args[0].String))
                {
                    Component.Destroy(comp);
                    Terminal.Log($"Destroyed {comp.GetPath()} {comp.GetType()}");
                    return;
                }
            });

            Register("sandbox.destroyallscripts", _ =>
            {
                var transform = PlayerManager.Car.transform;
                foreach (var comp in transform.GetComponentsInChildren<MonoBehaviour>(includeInactive: true))
                {
                    Terminal.Log($"Destroying {comp.GetPath()} {comp.GetType()}");
                    Component.Destroy(comp);
                }
            });

            Register("sandbox.enablecomponent", args =>
            {
                var transform = PlayerManager.Car.transform;
                foreach (var comp in transform.GetComponentsInChildren<MonoBehaviour>(includeInactive: true).Where(c => c.GetType().Name == args[0].String))
                {
                    comp.enabled = true;
                    Terminal.Log($"Enabled {comp.GetPath()} {comp.GetType()}");
                }
            });

            Register("sandbox.dumphj", _ =>
            {
                var hj = PlayerManager.Car.transform.GetComponentInChildren<HingeJoint>();
                Terminal.Log($"anchor={hj.anchor}, connectedAnchor={hj.connectedAnchor}, auto={hj.autoConfigureConnectedAnchor}, axis={hj.axis}");
            });
        }
    }
}