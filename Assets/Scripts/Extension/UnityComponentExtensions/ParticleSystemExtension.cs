using System.Collections;
using UnityEngine;

namespace Game.Tools
{
    public static class ParticleSystemExtension
    {
        public static void PlayIfExisted(this ParticleSystem particleSystem)
        {
            if (particleSystem != null) particleSystem.Play();
        }
        public static void StopIfExisted(this ParticleSystem particleSystem)
        {
            if (particleSystem != null) particleSystem.Stop();
        }
        public static void PlayParticlesGroup(this ParticleSystem[] particleSystemGroup)
        {
            if (particleSystemGroup != null)
            {
                for (int i = particleSystemGroup.Length - 1; i >= 0; i--) particleSystemGroup[i].Play();
            }
        }
        public static void StopParticlesGroup(this ParticleSystem[] particleSystemGroup)
        {
            if (particleSystemGroup != null)
            {
                for (int i = particleSystemGroup.Length - 1; i >= 0; i--) particleSystemGroup[i].Stop();
            }
        }
    }
}