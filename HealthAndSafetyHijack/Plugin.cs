using CustomUI.Settings;
using IllusionPlugin;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * Moon - 4/9/2019
 * Uses *reflection* to add a new eula-style prompt at the start of the game
 * Very intentionally avoided il patching / hooks
 */

namespace HealthAndSafetyHijack
{
    public class Plugin : IPlugin
    {
        public string Name => "HealthAndSafetyHijack";
        public string Version => "0.0.1";

        private HealthWarningFlowCoordinator healthWarningFlowCoordinator;
        private HealthWarningViewController healthWarningViewController;
        private EulaViewController eulaViewController;

        public void OnApplicationStart()
        {
            Config.LoadConfig();
            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
        }

        private void SceneManagerOnActiveSceneChanged(Scene arg0, Scene arg1)
        {
            if (arg1.name == "HealthWarning" && Config.ShowDisclaimer)
            {
                SharedCoroutineStarter.instance.StartCoroutine(HijackHealthAndSafety());
            }
        }

        private IEnumerator HijackHealthAndSafety()
        {
            healthWarningFlowCoordinator = Resources.FindObjectsOfTypeAll<HealthWarningFlowCoordinator>().First();
            healthWarningViewController = healthWarningFlowCoordinator.GetField<HealthWarningViewController>("_healthWarningViewContoller");
            eulaViewController = healthWarningFlowCoordinator.GetField<EulaViewController>("_eulaViewController");

            yield return new WaitUntil(() => healthWarningViewController.isInViewControllerHierarchy);

            healthWarningViewController.didFinishEvent -= healthWarningFlowCoordinator.HandleHealtWarningViewControllerdidFinish;
            healthWarningViewController.didFinishEvent += HandleHealtWarningViewControllerdidFinish;
        }

        public void HandleHealtWarningViewControllerdidFinish()
        {
            healthWarningViewController.didFinishEvent -= HandleHealtWarningViewControllerdidFinish;
            eulaViewController.didFinishEvent -= healthWarningFlowCoordinator.HandleEulaViewControllerdidFinish;
            eulaViewController.didFinishEvent += HandleEulaViewControllerdidFinish;

            healthWarningFlowCoordinator.SetProperty("title", "The Modding Community");

            healthWarningFlowCoordinator.InvokeMethod("PresentViewController", eulaViewController, null, false);
            SharedCoroutineStarter.instance.StartCoroutine(RewriteHealthAndSafety());
        }

        private IEnumerator RewriteHealthAndSafety()
        {
            yield return new WaitUntil(() => eulaViewController.isInViewControllerHierarchy);

            var textPageScrollView = eulaViewController.GetField<TextPageScrollView>("_textPageScrollView");
            textPageScrollView.SetText("By using mods, you understand that:\n\n" +
                "1. You may experience problems that don't exist in the vanilla game. 99.9% of bugs and crashes are due to mods\n\n" +
                "2. Mods are subject to being broken by updates and that's normal. Be patient and respectful when this happens, as modders are volunteers with real lives\n\n" +
                "3. Beat Games aren't purposefully trying to break mods. This is an Early Access game and updates can happen at any time\n\n" +
                "4. Modders and devs are two separate groups; do not attack the devs for issues related to mods");
        }

        public void HandleEulaViewControllerdidFinish(bool agreed)
        {
            eulaViewController.didFinishEvent -= HandleEulaViewControllerdidFinish;

            if (agreed)
            {
                healthWarningFlowCoordinator.GetField<ScenesTransitionSetupDataSO>("_nextScenesTransitionSetupData").ReplaceScenes(0.7f, null, null);
            }
            else
            {
               healthWarningFlowCoordinator.GetField<Ease01>("_fadeInOut").FadeOutInstant();
               Application.Quit();
            }
        }

        public void OnApplicationQuit()
        {
            SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
        }

        public void OnLevelWasLoaded(int level)
        {

        }

        public void OnLevelWasInitialized(int level)
        {
        }

        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {
        }
    }
}
