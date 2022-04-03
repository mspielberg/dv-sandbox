using CCL_GameScripts;
using CommandTerminal;
using DV.CabControls.Spec;
using DV.Logic.Job;
using DV.ServicePenalty;
using DV.Signs;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

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
            name = Main.mod!.Info.Id + "." + name;
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
            Register("dumpLayerNames", _ => {
                for (int i = 0 ; i < 32; i++)
                    Terminal.Log($"{i} - {LayerMask.LayerToName(i)}");
            });

            Register("dumpCar", _ =>
            {
                if (PlayerManager.Car != null)
                {
                    var dump = PlayerManager.Car.gameObject.DumpHierarchy();
                    Debug.Log(dump);
                }
            });

            Register("dumpInterior", _ =>
            {
                if (PlayerManager.Car?.interior != null)
                {
                    var dump = PlayerManager.Car.interior.gameObject.DumpHierarchy();
                    Debug.Log(dump);
                }
            });

            Register("dumpLoadedInterior", _ =>
            {
                if (PlayerManager.Car?.loadedInterior != null)
                {
                    var dump = PlayerManager.Car.loadedInterior.DumpHierarchy();
                    Debug.Log(dump);
                }
            });

            Register("ignorelayercollision", args =>
            {
                Physics.IgnoreLayerCollision(
                    LayerMask.NameToLayer(args[0].String),
                    LayerMask.NameToLayer(args[1].String));
            });

            Register("dumptractionmult", _ =>
            {
                var car = PlayerManager.Car;
                var controller = car.transform.GetComponentInChildren<LocoControllerBase>();
                Terminal.Log(controller.tractionTorqueMult.ToString());
            });

            // Register("addwheel", _ =>
            // {
            //     var wheel = PlayerManager.Car?.transform?.Find("C BrakeWheel(Clone)");
            //     if (wheel != null)
            //         GameObject.Destroy(wheel.gameObject);
            //     HandBrakeWheel.AddWheelToCar(PlayerManager.Car);
            // });

            // Register("resizewheel", args =>
            // {
            //     var wheelTransform = PlayerManager.Car?.transform?.GetComponentInChildren<Wheel>().gameObject.GetComponent<Transform>();
            //     if (wheelTransform == null)
            //     {
            //         Terminal.Log("Current car has no brake wheel");
            //         return;
            //     }
            //     wheelTransform.localScale = new Vector3(args[0].Float, 1.0f, args[0].Float);
            // });

            Register("dumpbrakesystem", _ =>
            {
                var brakeSystem = PlayerManager.Car?.brakeSystem;
                if (!brakeSystem)
                    return;
                Terminal.Log($"indep={brakeSystem.independentBrakePosition},factor={brakeSystem.brakingFactor}");
            });

            Register("movewheel", args =>
            {
                var wheelTransform = PlayerManager.Car?.interior?.GetComponentInChildren<Wheel>().gameObject.GetComponent<Transform>();
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

            Register("dumpallcolliders", _ =>
            {
                var transform = PlayerManager.Car?.transform;
                foreach (var collider in transform?.GetComponentsInChildren<Collider>())
                    Terminal.Log($"name={collider.transform.name},enabled={collider.enabled},isTrigger={collider.isTrigger},layer={LayerMask.LayerToName(collider.gameObject.layer)}");
            });

            Register("dumpwheelcolliders", _ =>
            {
                var wheelTransform = PlayerManager.Car?.transform?.Find("C BrakeWheel(Clone)");
                foreach (var collider in wheelTransform?.GetComponentsInChildren<Collider>())
                    Terminal.Log($"isTrigger={collider.isTrigger},layer={LayerMask.LayerToName(collider.gameObject.layer)}");
            });

            Register("disableobject", args =>
            {
                var transform = PlayerManager.Car.transform;
                foreach (var t in transform.GetComponentsInChildren<Transform>(includeInactive: true).Where(c => c.name.Contains(args[0].String)))
                {
                    t.gameObject.SetActive(false);
                    Terminal.Log($"Set inactive on {t.GetPath()}");
                }
            });

            Register("enableobject", args =>
            {
                var transform = PlayerManager.Car.transform;
                foreach (var t in transform.GetComponentsInChildren<Transform>(includeInactive: true).Where(c => c.name.Contains(args[0].String)))
                {
                    t.gameObject.SetActive(true);
                    Terminal.Log($"Set active on {t.GetPath()}");
                }
            });

            Register("disablecomponent", args =>
            {
                var transform = PlayerManager.Car.transform;
                foreach (var comp in transform.GetComponentsInChildren<MonoBehaviour>(includeInactive: true).Where(c => c.GetType().Name == args[0].String))
                {
                    comp.enabled = false;
                    Terminal.Log($"Disabled {comp.GetPath()} {comp.GetType()}");
                }
            });

            Register("disablecar", _ => PlayerManager.Car.gameObject.SetActive(false));

            Register("disableallcomponents", _ =>
            {
                var transform = PlayerManager.Car.transform;
                foreach (var comp in transform.GetComponentsInChildren<MonoBehaviour>(includeInactive: true))
                {
                    comp.enabled = false;
                    Terminal.Log($"Disabled {comp.GetPath()} {comp.GetType()}");
                }
            });

            Register("destroycomponent", args =>
            {
                var transform = PlayerManager.Car.transform;
                foreach (var comp in transform.GetComponentsInChildren<Component>(includeInactive: true).Where(c => c.GetType().Name == args[0].String))
                {
                    Component.Destroy(comp);
                    Terminal.Log($"Destroyed {comp.GetPath()} {comp.GetType()}");
                    return;
                }
            });

            Register("destroyallscripts", _ =>
            {
                var transform = PlayerManager.Car.transform;
                foreach (var comp in transform.GetComponentsInChildren<MonoBehaviour>(includeInactive: true))
                {
                    Terminal.Log($"Destroying {comp.GetPath()} {comp.GetType()}");
                    Component.Destroy(comp);
                }
            });

            Register("enablecomponent", args =>
            {
                var transform = PlayerManager.Car.transform;
                foreach (var comp in transform.GetComponentsInChildren<MonoBehaviour>(includeInactive: true).Where(c => c.GetType().Name == args[0].String))
                {
                    comp.enabled = true;
                    Terminal.Log($"Enabled {comp.GetPath()} {comp.GetType()}");
                }
            });

            Register("dumphj", _ =>
            {
                var hj = PlayerManager.Car.transform.GetComponentInChildren<HingeJoint>();
                Terminal.Log($"anchor={hj.anchor}, connectedAnchor={hj.connectedAnchor}, auto={hj.autoConfigureConnectedAnchor}, axis={hj.axis}");
            });

            Register("dumpcarinfo", _ =>
            {
                var car = PlayerManager.Car;
                if (car == null)
                    return;
                var damageModel = car.CarDamage;
                Terminal.Log($"Health: {damageModel.currentHealth} / {damageModel.effectiveMaxHealth} ({damageModel.EffectiveHealthPercentage}) ({damageModel.EffectiveHealthPercentage100Notation})");
                if (car.TryGetComponent<DamageController>(out var damageController))
                {
                    Terminal.Log($"Wheels: {damageController.wheels.currentHitPoints} / {damageController.wheels.fullHitPoints}");
                }
            });

            Register("dumphandbrake", _ =>
            {
                var car = PlayerManager.Car;
                if (!car)
                    return;

                var bs = car.brakeSystem;
                Terminal.Log($"BrakeSystem: independentPosition={bs.independentBrakePosition}");
                var cc = car.GetComponent<CabooseController>();
                Terminal.Log($"CabooseController: targetIndependentBrake={cc.targetIndependentBrake}");
                var cic = car.GetComponent<CabInputCaboose>();
                Terminal.Log($"CabInputCaboose: prevControllerIndepdentBrake={cic.prevControllerIndependentBrake}");
                var ckic = car.GetComponent<CarKeyboardInputCaboose>();
                Terminal.Log($"CarKeyboardInputCaboose: targetIndependentBrake={ckic.control.targetIndependentBrake}");
            });

            /*
            Register("createbell", _ =>
            {
                const float bellHeight = 0.2f;
                var rootPosition = PlayerManager.PlayerTransform.position + Vector3.up * 2;
                var pivot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pivot.transform.position = rootPosition;
                pivot.transform.Rotate(0, 0, 90f);
                pivot.transform.localScale = Vector3.one * 0.01f;
                Component.Destroy(pivot.GetComponent<Collider>());

                // bellGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bellGO = new GameObject("bell");
                bellGO.transform.position = rootPosition;
                // bellGO.transform.localScale = Vector3.one * 0.1f;
                var bellRB = bellGO.AddComponent<Rigidbody>();
                bellRB.mass = 10;

                var bellHinge = bellGO.AddComponent<HingeJoint>();
                bellHinge.anchor = Vector3.up * bellHeight / 4f;
                bellHinge.axis = Vector3.right;
                bellHinge.autoConfigureConnectedAnchor = false;
                bellHinge.connectedAnchor = bellHinge.transform.position + bellHinge.anchor;
                bellHinge.breakForce = float.PositiveInfinity;
                bellHinge.breakTorque = float.PositiveInfinity;

                var plate1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plate1.name = "plate1";
                plate1.transform.parent = bellGO.transform;
                plate1.transform.position = bellGO.transform.position;
                // plate1.transform.localPosition = Vector3.down * 0.06f;
                plate1.transform.localScale = new Vector3(0.1f, 0.1f, 0.01f);
                plate1.transform.RotateAround(rootPosition + Vector3.up * 0.1f, Vector3.right, 30f);

                var plate2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plate2.name = "plate2";
                plate2.transform.parent = bellGO.transform;
                plate2.transform.position = bellGO.transform.position;
                // plate2.transform.localPosition = Vector3.down * 0.06f;
                plate2.transform.localScale = new Vector3(0.1f, 0.1f, 0.01f);
                plate2.transform.RotateAround(rootPosition + Vector3.up * 0.1f, Vector3.right, -30f);

                clapper = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                clapper.transform.position = rootPosition + Vector3.down * bellHeight * 0.25f;
                clapper.transform.localScale = Vector3.one * 0.02f;
                var clapperRB = clapper.AddComponent<Rigidbody>();
                clapperRB.mass = 1;

                var clapperHinge = clapper.AddComponent<HingeJoint>();
                clapperHinge.anchor = Vector3.up * bellHeight * 0.25f / 0.02f;
                clapperHinge.axis = Vector3.right;
                clapperHinge.autoConfigureConnectedAnchor = false;
                clapperHinge.connectedAnchor = clapperHinge.transform.position + Vector3.up * bellHeight * 0.25f;
                clapperHinge.breakForce = float.PositiveInfinity;
                clapperHinge.breakTorque = float.PositiveInfinity;
            });


            Register("createbell2", _ =>
            {
                var rootPosition = PlayerManager.PlayerTransform.position + (Vector3.up * 2f);

                bellGO = new GameObject();
                var bellRB = bellGO.AddComponent<Rigidbody>();
                bellRB.useGravity = false;
                bellRB.mass = 5;

                var bellHinge = bellGO.AddComponent<HingeJoint>();
                bellHinge.anchor = Vector3.zero;
                bellHinge.axis = Vector3.right;
                bellHinge.autoConfigureConnectedAnchor = false;
                bellHinge.connectedAnchor = rootPosition;
                bellHinge.breakForce = float.PositiveInfinity;
                bellHinge.breakTorque = float.PositiveInfinity;
                Terminal.Log($"hinge.anchor = {bellHinge.anchor}");
                Terminal.Log($"hinge.autoConfigureConnectedAnchor = {bellHinge.autoConfigureConnectedAnchor}");
                Terminal.Log($"hinge.connectedAnchor = {bellHinge.connectedAnchor}");

                bellGO.transform.position = PlayerManager.PlayerTransform.position + (Vector3.up * 2f);
                Terminal.Log($"bellGO = {bellGO.transform.position}");

                var plate1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plate1.name = "plate1";
                plate1.transform.parent = bellGO.transform;
                plate1.transform.localPosition = Vector3.down * 0.06f;
                plate1.transform.localScale = new Vector3(0.1f, 0.1f, 0.01f);
                plate1.transform.RotateAround(bellGO.transform.position, Vector3.right, 45f);

                var plate1RB = plate1.AddComponent<Rigidbody>();
                plate1RB.mass = 0.01f;

                var plate1Joint = plate1.AddComponent<FixedJoint>();
                plate1Joint.connectedBody = bellRB;
                plate1Joint.autoConfigureConnectedAnchor = false;
                plate1Joint.anchor = Vector3.up * 0.06f;
                plate1Joint.breakForce = float.PositiveInfinity;
                plate1Joint.breakTorque = float.PositiveInfinity;

                var plate2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plate2.name = "plate2";
                plate2.transform.parent = bellGO.transform;
                plate2.transform.localPosition = Vector3.down * 0.06f;
                plate2.transform.localScale = new Vector3(0.1f, 0.1f, 0.01f);
                plate2.transform.RotateAround(bellGO.transform.position, Vector3.right, -45f);

                var plate2RB = plate2.AddComponent<Rigidbody>();
                plate2RB.mass = 0.01f;

                var plate2Joint = plate2.AddComponent<SpringJoint>();
                plate2Joint.connectedBody = bellRB;
                plate2Joint.autoConfigureConnectedAnchor = false;
                plate2Joint.anchor = Vector3.up * 0.06f;
                plate2Joint.breakForce = float.PositiveInfinity;
                plate2Joint.breakTorque = float.PositiveInfinity;
                plate2Joint.spring = 1e9f;
                plate2Joint.damper = 1;
                // plate2Joint.xMotion = plate2Joint.yMotion = plate2Joint.zMotion = ConfigurableJointMotion.Locked;
                // plate2Joint.angularXMotion = plate2Joint.angularYMotion = plate2Joint.angularZMotion = ConfigurableJointMotion.Locked;
                // plate2Joint.projectionMode = JointProjectionMode.PositionAndRotation;
            });

            Register("dumpBell", args =>
            {
                Terminal.Log(bellGO.DumpHierarchy());

                Terminal.Log(bellGO.transform.position.ToString());
                var bellHinge = bellGO.GetComponent<HingeJoint>();
                Terminal.Log($"hinge.anchor = {bellHinge.anchor}");
                Terminal.Log($"hinge.autoConfigureConnectedAnchor = {bellHinge.autoConfigureConnectedAnchor}");
                Terminal.Log($"hinge.connectedAnchor = {bellHinge.connectedAnchor}");
                Terminal.Log($"hinge.force = {bellHinge.currentForce}");
                Terminal.Log($"p2.force = {bellGO.transform.Find("plate2").GetComponent<SpringJoint>().currentForce}");
                Terminal.Log($"p2.connectedBody = {bellGO.transform.Find("plate2").GetComponent<SpringJoint>().connectedBody}");
                Terminal.Log($"p2.connectedAnchor = {bellGO.transform.Find("plate2").GetComponent<SpringJoint>().connectedAnchor}");
                Terminal.Log($"p2.anchor = {bellGO.transform.Find("plate2").GetComponent<SpringJoint>().anchor}");
            });

            Register("dumpClapper", args =>
            {
                Terminal.Log(clapper.DumpHierarchy());
            });

            Register("pushbell", args =>
            {
                bellGO.GetComponent<Rigidbody>().AddRelativeForce(0, 0, args[0].Float);
            });
            Register("pushclapper", args =>
            {
                clapper.GetComponent<Rigidbody>().AddRelativeForce(0, 0, args[0].Float);
            });

            Register("dumpffshop", _ =>
            {
                var shopController = SingletonBehaviour<GlobalShopController>.Instance;
                foreach (var shopItemData in shopController.shopItemsData)
                {
                    Terminal.Log(shopItemData.item.gameObject.name);
                    Terminal.Log(shopItemData.basePrice.ToString());
                    Terminal.Log(shopItemData.item.gameObject.layer.ToString());
                }
            });
        */
            Terminal.Shell.AddCommand("listCars", _ =>
            {
                Terminal.Log(string.Join(",",
                    SingletonBehaviour<IdGenerator>.Instance.logicCarToTrainCar
                        .Keys
                        .Select(k => k.ID)
                        .OrderBy(x => x)));
            });

            Register("teleportToCar", args =>
            {
                var id = args[0].String;
                var pair = SingletonBehaviour<IdGenerator>.Instance.logicCarToTrainCar.FirstOrDefault(pair => pair.Key.ID == id);
                if (pair.Value != null)
                    PlayerManager.TeleportPlayerToCar(pair.Value);
            });

            Register("listdebts", _ =>
            {
                var controller = SingletonBehaviour<CareerManagerDebtController>.Instance;
                foreach (var debt in controller.currentNonZeroPricedDebts)
                    Terminal.Log($"{debt.ID}: {debt.GetDebtType()} {debt.GetTotalPrice()}");
            });

            Register("cleardebt", args =>
            {
                var debtID = args[0].String;
                var controller = SingletonBehaviour<CareerManagerDebtController>.Instance;
                var debt = controller.currentNonZeroPricedDebts.Find(debt => debt.ID == debtID);
                if (debt == default)
                    return;

                if (debt is ExistingJobDebt existingJobDebt)
                    SingletonBehaviour<JobDebtController>.Instance.PayExistingJobDebt(existingJobDebt);
                else if (debt is ExistingLocoDebt existingLocoDebt)
                    SingletonBehaviour<LocoDebtController>.Instance.PayExistingLocoDebt(existingLocoDebt);
                else if (debt is StagedJobDebt stagedJobDebt)
                    SingletonBehaviour<JobDebtController>.Instance.PayStagedJobDebt(stagedJobDebt);
                else if (debt is StagedLocoDebt stagedLocoDebt)
                    SingletonBehaviour<LocoDebtController>.Instance.PayStagedLocoDebt(stagedLocoDebt);
            });

            Register("dumpcoolingaudio", _ =>
            {
                var go = PlayerManager.Car?.transform.Find("AudioShunter(Clone)/Engine/CoolingFan");
                if (go == null)
                    return;
                var source = go.GetComponent<AudioSource>();
                Terminal.Log($"volume={source.volume},pitch={source.pitch},clip={source.clip}");
            });

            Register("dumpFrictionCurve", _ =>
            {
                var force = PlayerManager.Car?.GetComponent<DrivingForce>();
                if (!force)
                    return;
                Terminal.Log(string.Join(",", force.wheelslipToFrictionModifierCurve.keys.Select(key => $"({key.time},{key.value})")));
            });

            Register("raycastInteractable", _ =>
            {
                var grabber = Component.FindObjectOfType<Grabber>();
                var collider = grabber.hit.collider;
                Terminal.Log(collider.GetPath());
            });

            Register("replaceCouplerJoints", args =>
            {
                var car = PlayerManager.Car;
                if (!car)
                    return;
                Func<Coupler, bool> replaceCouplerJoints = (Coupler coupler) =>
                {
                    if (!coupler.springyCJ)
                         return false;
                    coupler.KillJointCoroutines();

                    const float CouplerSlop = 0.5f;
                    var coupledToOffset = coupler.coupledTo.train.transform.InverseTransformPoint(coupler.coupledTo.transform.position);
                    coupledToOffset.z -= Mathf.Sign(coupledToOffset.z) * CouplerSlop / 2f;

                    var cj = car.gameObject.AddComponent<ConfigurableJoint>();
                    cj.autoConfigureConnectedAnchor = false;
                    cj.anchor = car.transform.InverseTransformPoint(coupler.transform.position);
                    cj.connectedBody = coupler.coupledTo.train.gameObject.GetComponent<Rigidbody>();
                    cj.connectedAnchor = coupledToOffset; // coupler.coupledTo.train.transform.InverseTransformPoint(coupler.coupledTo.transform.position);
                    cj.xMotion = ConfigurableJointMotion.Free;
                    cj.yMotion = ConfigurableJointMotion.Free;
                    cj.zMotion = ConfigurableJointMotion.Limited;
                    cj.angularXMotion = ConfigurableJointMotion.Free;
                    cj.angularYMotion = ConfigurableJointMotion.Free;
                    cj.angularZMotion = ConfigurableJointMotion.Free;
                    cj.linearLimit = new SoftJointLimit { limit = CouplerSlop / 2 };
                    cj.linearLimitSpring = new SoftJointLimitSpring { spring = 1e12f, damper = 1e3f };

                    cj.zDrive = new JointDrive {
                        positionSpring = args[0].Float,//1e6f,
                        positionDamper = args[1].Float,//1e4f,
                        maximumForce = 1e6f,
                    };
                    cj.targetPosition = new Vector3(0f, 0f, -CouplerSlop / 2f);

                    cj.breakForce = 1e6f;
                    cj.enableCollision = false;

                    if (coupler.springyCJ)
                        Component.Destroy(coupler.springyCJ);
                    if (coupler.rigidCJ)
                    {
                        Component.Destroy(coupler.rigidCJ);
                        coupler.rigidCJ = null;
                    }
                    coupler.springyCJ = cj;

                    Terminal.Log($"anchor={cj.anchor},connectedAnchor={cj.connectedAnchor}");
                    return true;
                };
                if (replaceCouplerJoints(car.frontCoupler))
                    Terminal.Log("Replaced front coupler joint(s)");
                if (replaceCouplerJoints(car.rearCoupler))
                    Terminal.Log("Replaced rear coupler joint(s)");
            });

            Register("dumpjoint", _ =>
            {
                var car = PlayerManager.Car;
                var coupler = car.frontCoupler;

                static void dumpJoint(ConfigurableJoint j)
                {
                    Terminal.Log($"anchor={j.anchor},anchorPosition={j.transform.TransformPoint(j.anchor)}");
                    Terminal.Log($"connectedBody={j.connectedBody}");
                    Terminal.Log($"connectedAnchor={j.connectedAnchor},connectedAnchorPosition={j.connectedBody.transform.TransformPoint(j.connectedAnchor)}");
                    var delta = j.transform.InverseTransformPoint(j.connectedBody.transform.TransformPoint(j.connectedAnchor)) - j.anchor;
                    Terminal.Log($"limitSpring={j.linearLimitSpring.spring}");
                    Terminal.Log($"couplerDelta={delta}");
                    Terminal.Log($"breakForce={j.breakForce}");
                    Terminal.Log($"jointForce={j.currentForce}");
                    Terminal.Log($"jointTorque={j.currentTorque}");
                    Terminal.Log($"targetPosition={j.targetPosition}");
                }
                if (coupler.springyCJ != null)
                {
                    Terminal.Log("Front:");
                    dumpJoint(coupler.springyCJ);
                }
                coupler = car.rearCoupler;
                if (coupler.springyCJ != null)
                {
                    Terminal.Log("Rear:");
                    dumpJoint(coupler.springyCJ);
                }
            });

            Register("dumpsigns", _ =>
            {
                foreach (var sign in Component.FindObjectsOfType<SignDebug>())
                {
                    Terminal.Log(sign.gameObject.DumpHierarchy());
                }
            });

            Register("mphsigns", _ =>
            {
                var logged = new HashSet<float>();
                foreach (var sign in Component.FindObjectsOfType<SignDebug>())
                {
                    foreach (var tmp in sign.transform.GetComponentsInChildren<TextMeshPro>())
                    {
                        var text = tmp.text;
                        if (text.Length > 0 && text.All(char.IsDigit))
                        {
                            if (float.TryParse(text, out var kmh))
                            {
                                var mph = Mathf.RoundToInt(kmh  * 10f * 0.621371f / 5) * 5;
                                tmp.text = mph.ToString();
                                if (!logged.Contains(kmh))
                                {
                                    Terminal.Log($"{kmh} -> {mph}");
                                    logged.Add(kmh);
                                }
                            }
                            else
                            {
                                Terminal.Log($"Could not parse: '{text}");
                            }
                        }
                    }
                }
            });

            Register("speedsignsize", args =>
            {
                foreach (var sign in Component.FindObjectsOfType<SignDebug>())
                {
                    foreach (var tmp in sign.transform.GetComponentsInChildren<TextMeshPro>())
                    {
                        if (tmp.text.All(char.IsDigit))
                        {
                            if (args.Length > 0)
                            {
                                    tmp.fontSize = args[0].Float; }
                            else
                            {
                                Terminal.Log($"{tmp.text} (size={tmp.fontSizeMin},{tmp.fontSize},{tmp.fontSizeMax})");
                            }
                        }
                    }
                }
            });

            Register("speedsignmargin", args =>
            {
                foreach (var sign in Component.FindObjectsOfType<SignDebug>())
                {
                    foreach (var tmp in sign.transform.GetComponentsInChildren<TextMeshPro>())
                    {
                        if (tmp.text.All(char.IsDigit))
                        {
                        if (args.Length > 0)
                            {
                                    tmp.margin = Vector4.one * args[0].Float;
                            }
                            else
                            {
                                Terminal.Log($"{tmp.text} (size={tmp.margin})");
                            }
                        }
                    }
                }
            });

            Register("dumplighting", _ =>
            {
                static void DumpLight(Light l)
                {
                    Terminal.Log(
                        $"{l.GetPath()}: " +
                        $"type={l.type}," +
                        $"color={l.color}," +
                        $"intensity={l.intensity}," +
                        $"renderMode={l.renderMode}," +
                        ""
                    );
                }
                Terminal.Log(
                    $"ambientSkyboxAmount={RenderSettings.ambientSkyboxAmount}," +
                    $"haloStrength={RenderSettings.haloStrength}," +
                    $"defaultReflectionResolution={RenderSettings.defaultReflectionResolution}," +
                    $"defaultReflectionMode={RenderSettings.defaultReflectionMode}," +
                    $"reflectionBounces={RenderSettings.reflectionBounces}," +
                    $"reflectionIntensity={RenderSettings.reflectionIntensity}," +
                    $"customReflection={RenderSettings.customReflection}," +
                    $"ambientProbe={RenderSettings.ambientProbe}," +
                    $"sun={RenderSettings.sun}," +
                    $"skybox={RenderSettings.skybox}," +
                    $"subtractiveShadowColor={RenderSettings.subtractiveShadowColor}," +
                    $"flareStrength={RenderSettings.flareStrength}," +
                    $"ambientLight={RenderSettings.ambientLight}," +
                    $"ambientGroundColor={RenderSettings.ambientGroundColor}," +
                    $"ambientEquatorColor={RenderSettings.ambientEquatorColor}," +
                    $"ambientSkyColor={RenderSettings.ambientSkyColor}," +
                    $"ambientMode={RenderSettings.ambientMode}," +
                    $"fogDensity={RenderSettings.fogDensity}," +
                    $"fogColor={RenderSettings.fogColor}," +
                    $"fogMode={RenderSettings.fogMode}," +
                    $"fogEndDistance={RenderSettings.fogEndDistance}," +
                    $"fogStartDistance={RenderSettings.fogStartDistance}," +
                    $"fog={RenderSettings.fog}," +
                    $"ambientIntensity={RenderSettings.ambientIntensity}," +
                    $"flareFadeSpeed={RenderSettings.flareFadeSpeed}," +
                    ""
                );
                Terminal.Log("Sun:");
                DumpLight(RenderSettings.sun);
                foreach (var l in Component.FindObjectsOfType<Light>())
                    DumpLight(l);
            });

            Register("dumpassetbundle", args =>
            {
                AssetBundle.UnloadAllAssetBundles(false);
                var bundle = AssetBundle.LoadFromFile(args[0].String);
                var names = bundle.GetAllAssetNames();
                foreach (var name in names)
                {
                    Terminal.Log(name);
                    var go = bundle.LoadAsset<GameObject>(names[0]);
                    var setup = go.GetComponent<TrainCarSetup>();
                    var interiorPrefab = setup?.InteriorPrefab;
                    Terminal.Log($"TrainCarSetup={setup}, InteriorPrefab={interiorPrefab}");
                }
            });

            Register("dumpcustomsetup", _ =>
            {
                var setup = PlayerManager.Car.GetComponent<TrainCarSetup>();
                Terminal.Log($"interiorPrefab={((setup.InteriorPrefab == null) ? "(null)" : setup.InteriorPrefab.ToString())}");
            });

            Register("dumpleverspec", args =>
            {
                var name = string.Join(" ", args.Select(a => a.String));
                var interior = PlayerManager.Car.loadedInterior;
                var transform = Array.Find(interior.transform.GetComponentsInChildren<Transform>(), t => t.name == name);
                if (transform == null)
                {
                    Terminal.Log("could not find transform");
                    return;
                }
                var spec = transform.GetComponent<Lever>();
                if (spec == null)
                {
                    Terminal.Log("Could not find Lever spec");
                    return;
                }
                Terminal.Log(
                    $"mass={spec.rigidbodyMass}," +
                    $"drag={spec.rigidbodyDrag}," +
                    $"angularDrag={spec.rigidbodyAngularDrag}," +
                    $"stepped={spec.useSteppedJoint}," +
                    $"notches={spec.notches}," +
                    $"invertDirection={spec.invertDirection}," +
                    $"hoverScroll={spec.scrollWheelHoverScroll}," +
                    $"wheelSpring={spec.scrollWheelSpring}," +
                    $"axis={spec.jointAxis}," +
                    $"useSpring={spec.useSpring}," +
                    $"spring={spec.jointSpring}," +
                    $"damper={spec.jointDamper}," +
                    $"useLimits={spec.useLimits}," +
                    $"min={spec.jointLimitMin}," +
                    $"max={spec.jointLimitMax}," +
                    "");
            });

            Register("setinteriorreflectionprobe", args =>
            {
                var interior = PlayerManager.Car.loadedInterior;
                foreach (var renderer in interior.GetComponentsInChildren<MeshRenderer>())
                {
                    var path = renderer.GetPath();
                    var before = renderer.reflectionProbeUsage;
                    var after = (ReflectionProbeUsage)args[0].Int;
                    renderer.reflectionProbeUsage = after;
                    Terminal.Log($"[{path}] {before} -> {after}");
                }
            });

            Register("enumerateinteriorshaders", _ =>
            {
                var interior = PlayerManager.Car.loadedInterior;
                var printed = new HashSet<string>();
                foreach (var renderer in interior.GetComponentsInChildren<MeshRenderer>())
                {
                    var name = renderer?.material?.shader?.name;
                    if (name != null && !printed.Contains(name))
                    {
                        Terminal.Log(name);
                        printed.Add(name);
                    }
                }
            });

            Register("rotatebrakes", args =>
            {
                var car = PlayerManager.Car;
                foreach (var bogie in car.Bogies)
                {
                    foreach (var pad in bogie.transform.Find("bogie_car/bogie2brakes").GetComponentsInChildren<Transform>())
                    {
                        if (pad.name.EndsWith("1") || pad.name.EndsWith("2"))
                        {
                            Terminal.Log($"old angles: {pad.localEulerAngles}");
                            pad.localEulerAngles = new Vector3(args[0].Float, args[1].Float, args[2].Float);
                        }
                    }
                }
            });

            Register("getbrakepositions", args =>
            {
                var car = PlayerManager.Car;
                foreach (var bogie in car.Bogies)
                {
                    foreach (var pad in bogie.transform.Find("bogie_car/bogie2brakes").GetComponentsInChildren<Transform>())
                    {
                        if (pad.name.EndsWith("1") || pad.name.EndsWith("2"))
                        {
                            Terminal.Log($"old position: {pad.localPosition.x},{pad.localPosition.y},{pad.localPosition.z}");
                        }
                    }
                }
            });

            Register("translatebrakes", args =>
            {
                var car = PlayerManager.Car;
                var offset = args[1].Float;
                foreach (var bogie in car.Bogies)
                {
                    foreach (var pad in bogie.transform.Find("bogie_car/bogie2brakes").GetComponentsInChildren<Transform>())
                    {
                        if (pad.name.EndsWith("1"))
                        {
                            var position = pad.localPosition;
                            Terminal.Log($"old position: {pad.localPosition}");
                            position.z = args[0].Float + offset;
                            pad.localPosition = position;
                        }
                        else if (pad.name.EndsWith("2"))
                        {
                            var position = pad.localPosition;
                            Terminal.Log($"old position: {pad.localPosition}");
                            position.z = -args[0].Float + offset;
                            pad.localPosition = position;
                        }
                    }
                }
            });
        }

        // private static GameObject? bellGO;
        // private static GameObject? clapper;
    }
}
