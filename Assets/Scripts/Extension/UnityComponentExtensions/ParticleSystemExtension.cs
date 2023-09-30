using System.Collections;
using UnityEngine;

namespace DaiVQScript.Utilities
{
    public static class ParticleSystemExtension
    {
        public static void PlayIfExisted(this ParticleSystem particleSystem)
        {
            if (particleSystem != null) particleSystem.Play();
        }
        public static void PlayIfExisted(this ParticleSystem particleSystem, Vector3 position)
        {
            if (particleSystem != null)
            {
                particleSystem.transform.position = position;
                particleSystem.Play();
            }
        }
        public static void PlayIfExisted(this ParticleSystem particleSystem, Vector3 position, Vector2 lookDirection)
        {
            if (particleSystem != null)
            {
                particleSystem.transform.SetPositionAndRotation(position, Quaternion.LookRotation(Vector3.forward, lookDirection));
                particleSystem.Play();
            }
        }
        public static void PlayIfExisted(this ParticleSystem particleSystem, Vector3 position, float scale)
        {
            if (particleSystem != null)
            {
                particleSystem.transform.position = position;
                particleSystem.transform.localScale = new Vector3(scale, scale, scale);
                particleSystem.Play();
            }
        }
        public static void PlayIfExisted(this ParticleSystem particleSystem, Vector3 position, Vector2 lookDirection, float scale)
        {
            if (particleSystem != null)
            {
                particleSystem.transform.SetPositionAndRotation(position, Quaternion.LookRotation(Vector3.forward, lookDirection));
                particleSystem.transform.localScale = new Vector3(scale, scale, scale);
                particleSystem.Play();
            }
        }
        public static void PlayIfExisted(this ParticleSystem particleSystem, Vector3 position, Quaternion rotation, float scale)
        {
            if (particleSystem != null)
            {
                particleSystem.transform.SetPositionAndRotation(position, rotation);
                particleSystem.transform.localScale = new Vector3(scale, scale, scale);
                particleSystem.Play();
            }
        }
        public static void StopIfExisted(this ParticleSystem particleSystem)
        {
            if (particleSystem != null) particleSystem.Stop();
        }
        public static void PlayParticlesGroup(this ParticleSystem[] particleSystemGroup)
        {
            if (particleSystemGroup != null)
            {
                for (int i = particleSystemGroup.Length - 1; i >= 0; i--)
                    if (particleSystemGroup[i] != null) particleSystemGroup[i].Play();
            }
        }
        public static void StopParticlesGroup(this ParticleSystem[] particleSystemGroup)
        {
            if (particleSystemGroup != null)
            {
                for (int i = particleSystemGroup.Length - 1; i >= 0; i--) 
                    if (particleSystemGroup[i] != null) particleSystemGroup[i].Stop();
            }
        }

        //public static float GetMaxDuration(this ParticleSystem particleSystem)
        //{
        //    var main = particleSystem.main;
        //    return main.startDelayMultiplier + Mathf.Max(main.duration, main.startLifetimeMultiplier);
        //}
    }
}