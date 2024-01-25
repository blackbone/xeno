#if UNITY_5_3_OR_NEWER
using UnityEngine;
using Xeno;

namespace Mediators.GameStates.Shared
{
    [ExecuteAlways]
    public class ECSInstaller : MonoBehaviour
    {
        [SerializeField] private bool createWorldInEditor;
        
        [SerializeReference] private UpdateSystem[] updateSystems;
        
        private IWorld world;
         
        // Get or create world when enter
        private void OnEnable()
        {
            Debug.LogError(nameof(OnEnable));
            if (Application.isPlaying)
            {
                world = Worlds.Create(gameObject.scene.name);
                return;
            }

#if UNITY_EDITOR
            world = Worlds.GetOrCreate($"[Editor] {gameObject.scene.name}");
#endif
        }

        private void OnDisable()
        {
            Debug.LogError(nameof(OnDisable));
            if (Application.isPlaying)
            {
                world?.Dispose();
                world = null;
                return;
            }
            
#if UNITY_EDITOR
            world?.Dispose();
            world = null;
#endif
        }
    }
}
#endif