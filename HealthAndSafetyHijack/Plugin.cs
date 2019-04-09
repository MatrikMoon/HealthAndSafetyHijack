using IllusionPlugin;
using System.Collections;
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
            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
        }

        private void SceneManagerOnActiveSceneChanged(Scene arg0, Scene arg1)
        {
            if (arg1.name == "HealthWarning")
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
            textPageScrollView.SetText("Don't be dumb, yo.");
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
