using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(menuName = "Variables/Sprite Collection")]
    public class SpriteCollection : ScriptableObject
    {
        [SerializeField] private Sprite[] collection;
        private readonly Dictionary<string, Sprite> _sprites = new();

        public Sprite GetSprite(string spriteName)
        {
            var spriteExists = _sprites.TryGetValue(spriteName, out var sprite);

            return spriteExists ? sprite : null;
        }

        // FIXME: This shouldn't rattle unity warnings
        private void OnValidate()
        {
            _sprites.Clear();
            try
            {
                foreach (var sprite in collection) _sprites.Add(sprite.name, sprite);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error serializing sprite collection.\n {e}");
            }
        }
    }
}