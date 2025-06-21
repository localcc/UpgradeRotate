using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using MelonLoader;
using UnityEngine.InputSystem;

[assembly: MelonInfo(typeof(UpgradeRotate.Core), "UpgradeRotate", "1.0.1", "localcc", null)]
[assembly: MelonGame("Pigeons at Play", "Mycopunk")]

namespace UpgradeRotate
{
    public class Core : MelonMod
    {
        private InputActionMap _upgradeActionMap;
        private InputAction _rotateLeft;
        private InputAction _rotateRight;
        private InputAction _resetRotation;

        private GearDetailsWindow _gearDetailsWindow;

        public override void OnInitializeMelon()
        {
            _upgradeActionMap = new InputActionMap("upgradeRotate");

            _rotateLeft = _upgradeActionMap.AddAction("rotateUpgradeLeft");
            _rotateLeft.AddBinding("<Gamepad>/leftShoulder");
            _rotateLeft.AddBinding("<Keyboard>/q");

            _rotateRight = _upgradeActionMap.AddAction("rotateUpgradeRight");
            _rotateRight.AddBinding("<Gamepad>/rightShoulder");
            _rotateRight.AddBinding("<Keyboard>/e");

            _resetRotation = _upgradeActionMap.AddAction("rotateUpgradeReset");
            _resetRotation.AddBinding("<Gamepad>/buttonWest");
            _resetRotation.AddBinding("<Keyboard>/r");

            var leftAction = new Action(() => { Rotate(1); });
            _rotateLeft.add_performed(leftAction.FromNoParam<InputAction.CallbackContext>());
            var rightAction = new Action(() => { Rotate(-1); });
            _rotateRight.add_performed(rightAction.FromNoParam<InputAction.CallbackContext>());
            var resetAction = new Action(() => { ResetRotation(); });
            _resetRotation.add_performed(resetAction.FromNoParam<InputAction.CallbackContext>());

            LoggerInstance.Msg("Initialized!");
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            _gearDetailsWindow = UnityEngine.Object.FindObjectOfType<GearDetailsWindow>();
        }

        public void OnGearDetailsWindowOpened(GearDetailsWindow instance)
        {
            _gearDetailsWindow = instance;
            _upgradeActionMap.Enable();
        }

        public void OnGearDetailsWindowClosed(GearDetailsWindow instance)
        {
            _gearDetailsWindow = instance;
            _upgradeActionMap.Disable();
        }

        private void Rotate(int direction)
        {
            if (_gearDetailsWindow?.SelectedUpgrade == null) return;
            var newRotation = (_gearDetailsWindow.selectedUpgradeRotation + direction + 6) % 6;

            _gearDetailsWindow.SetSelectedUpgradeRotation(newRotation);
        }

        private void ResetRotation()
        {
            if (_gearDetailsWindow?.SelectedUpgrade == null) return;
            _gearDetailsWindow.SetSelectedUpgradeRotation(0);
        }
    }

    [HarmonyPatch(typeof(GearDetailsWindow))]
    class GearDetailsWindowPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GearDetailsWindow.OnOpen))]
        static void OnOpen(GearDetailsWindow __instance)
        {
            Melon<Core>.Instance.OnGearDetailsWindowOpened(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GearDetailsWindow.OnCloseCallback))]
        static void OnCloseCallback(GearDetailsWindow __instance)
        {
            Melon<Core>.Instance.OnGearDetailsWindowClosed(__instance);
        }
    }

    static class UglyActionHack
    {
        public static Il2CppSystem.Action<T> FromNoParam<T>(this Action action)
        {
            var il2cppAction = DelegateSupport.ConvertDelegate<Il2CppSystem.Action>(action);
            return System.Runtime.CompilerServices.Unsafe.As<Il2CppSystem.Action<T>>(il2cppAction);
        }
    }
}