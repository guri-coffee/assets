using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Tsukikage.Game
{
    /// <summary>
    /// タイトル（TapScreen）: 画面タップでフェードアウトして次へ進む。
    /// 遷移先シーンは未実装のため、フェード完了時点ではログ出力のみ行う。
    /// </summary>
    public class TapScreenController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup fadeGroup;   // Canvas全体のCanvasGroup
        [SerializeField] private Text tapText;            // 「画面をタップ」
        [SerializeField] private float fadeDuration = 0.8f;

        private bool _tapped;

        private void Start()
        {
            // 日本語表示: OSフォントを動的ロード（TMPフォントアセット未整備のため）
            var font = Font.CreateDynamicFontFromOSFont(
                new[] { "Yu Gothic UI", "Yu Gothic", "Meiryo", "MS Gothic" }, 48);
            if (font != null && tapText != null)
            {
                tapText.font = font;
            }
        }

        private void Update()
        {
            if (_tapped) return;
            if (Input.GetMouseButtonDown(0) ||
                (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                OnTap();
            }

            // 「画面をタップ」の明滅
            if (tapText != null)
            {
                var c = tapText.color;
                c.a = 0.55f + 0.45f * Mathf.Sin(Time.time * 2.5f);
                tapText.color = c;
            }
        }

        /// <summary>タップ処理本体（テストから直接呼べるようpublic）</summary>
        public void OnTap()
        {
            if (_tapped) return;
            _tapped = true;
            Debug.Log("[TapScreen] tapped — start fade out");
            StartCoroutine(FadeOut());
        }

        private IEnumerator FadeOut()
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                if (fadeGroup != null)
                {
                    fadeGroup.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
                }
                yield return null;
            }
            Debug.Log("[TapScreen] fade complete — TODO: load Loading scene");
        }
    }
}
