using UnityEngine;
using UnityEngine.AddressableAssets;
namespace SpaceTrader.Game.Data
{

    [CreateAssetMenu(menuName = "Game/Core/GameDef", fileName = "def_")]
    public class GameDef : ScriptableObject
    {
        [Tooltip("Benzersiz kimlik (dosya adıyla aynı tutmak pratik olur)")]
        public string id;

        [Header("Addressables")]
        public AssetReferenceGameObject prefab;  // zorunlu: sahneye spawn için
        public AssetReferenceSprite icon;        // opsiyonel: UI görseli

        [Header("Genel Etiketler")]
        public string category; // örn: "ship","enemy","weapon","fx","prop","ui"...
        public string[] tags;   // arama/filtre için

        [Header("Basit Stat Alanları (istersen kullan)")]
        public int maxHP = 0;
        public float speed = 0f;
        public int cost = 0;
    }
}
