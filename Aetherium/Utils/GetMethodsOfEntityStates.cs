using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aetherium.Utils
{
    class GetMethodsOfEntityStates
    {
        public void Run()
        {
            //THE HOOK TO END ALL HOOKS
            Debug.Log("STARTING BLASTER SWORD INITIALIZATION. PLEASE WAIT.");

            var handler = typeof(BlasterSword).GetMethod("SwordCheckerIGuess");

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(EntityState).IsAssignableFrom(type))
                    {
                        var t1 = typeof(Action<>).MakeGenericType(type);
                        var t2 = typeof(Action<,>).MakeGenericType(t1, type);
                        var genericMethod = handler.MakeGenericMethod(type);
                        var handlerDelegate = Delegate.CreateDelegate(t2, genericMethod);

                        var onUpdate = type.GetMethod("FixedUpdate");
                        if (onUpdate != null)
                        {
                            new Hook(onUpdate, handlerDelegate);
                        }

                        var onEnter = type.GetMethod("OnEnter");
                        if (onEnter != null)
                        {
                            new Hook(onEnter, handlerDelegate);
                        }

                        var onExit = type.GetMethod("OnExit");
                        if (onExit != null)
                        {
                            new Hook(onExit, handlerDelegate);
                        }
                    }
                }
            }
            Debug.Log("BLASTER SWORD INITIALIZATION DONE. SORRY ABOUT THE WAIT.");

        }

        public static void SwordCheckerIGuess<T>(Action<T> orig, T self)
        {

            try
            {
                ShouldWeFireSword++;
                orig(self);
            }
            finally
            {
                ShouldWeFireSword--;
            }
        }
    }
}
