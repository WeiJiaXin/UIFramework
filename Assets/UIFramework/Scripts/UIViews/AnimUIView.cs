using System;
using System.Collections;
using UnityEngine;

namespace Lowy.UIFramework
{
    [RequireComponent(typeof(Animation))]
    public abstract class AnimUIView : UIView
    {
        private Animation _animation;
        [SerializeField] private string displayAnimName = "Display";
        [SerializeField] private string disableAnimName = "Disable";

        private Coroutine _waitForSeconds;

        public override void Awake()
        {
            base.Awake();
            //
            _animation = GetComponent<Animation>();
        }

        protected override void PlayEnterAnim(AbsContent content, Action<AbsContent> end)
        {
            gameObject.SetActive(true);
            _animation.clip = _animation.GetClip(displayAnimName);
            _animation.Play();
            if (_waitForSeconds != null)
                StopCoroutine(_waitForSeconds);
            _waitForSeconds = StartCoroutine(WaitForSeconds(_animation.GetClip(displayAnimName).length,
                    () =>
                    {
                        _waitForSeconds = null;
                        end?.Invoke(content);
                    }
                )
            );
        }

        protected override void PlayExitAnim(AbsContent content, Action<AbsContent> end)
        {
            if (!gameObject.activeInHierarchy)
                return;
            _animation.Play(disableAnimName);
            if (_waitForSeconds != null)
                StopCoroutine(_waitForSeconds);
            _waitForSeconds = StartCoroutine(WaitForSeconds(_animation.GetClip(disableAnimName).length,
                    () =>
                    {
                        end?.Invoke(content);
                        gameObject.SetActive(false);
                        _waitForSeconds = null;
                    }
                )
            );
        }

        private IEnumerator WaitForSeconds(float time, Action action)
        {
            yield return new WaitForSeconds(time);
            action?.Invoke();
        }
    }
}