using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineBuilder
{
    /// <summary>
    /// Allows the use of a coroutine builder to create and manage coroutines.
    /// </summary>
    private readonly Queue<IActionWrapper> _actionWrapperQueue = new();

    private CoroutineBuilder()
    {
    }

    public static CoroutineBuilder Create()
    {
        // Create a new instance of the CoroutineBuilder
        return new CoroutineBuilder();
    }
    
    public CoroutineBuilder Enqueue(IEnumerator coroutine)
    {
        _actionWrapperQueue.Enqueue(new CoroutineWrapper(coroutine));

        return this;
    }

    public CoroutineBuilder Enqueue(Action action)
    {
        _actionWrapperQueue.Enqueue(new ActionWrapper(action));

        return this;
    }

    public CoroutineBuilder EnqueueActionAndYield(Action action)
    {
        _actionWrapperQueue.Enqueue(new CoroutineWrapper(ActionToCoroutine(action)));

        return this;
    }

    public CoroutineBuilder WaitFrames(int frameCount = 1)
    {
        // Make sure the frame count is greater than 0
        frameCount = Mathf.Max(1, frameCount);

        // Enqueue the wait for frames coroutine
        Enqueue(WaitForFrames(frameCount));

        return this;
    }

    private static IEnumerator WaitForFrames(int frameCount)
    {
        // Wait for the specified number of frames
        for (var i = 0; i < frameCount; i++)
            yield return null;
    }

    public CoroutineBuilder WaitSeconds(float time)
    {
        // Make sure the time is greater than 0
        time = Mathf.Max(0, time);

        // Enqueue the wait for time coroutine
        Enqueue(WaitForSeconds(time));

        return this;
    }

    private static IEnumerator WaitForSeconds(float time)
    {
        // Wait for the specified amount of time
        yield return new WaitForSeconds(time);
    }

    public CoroutineBuilder WaitSecondsRealtime(float time)
    {
        // Make sure the time is greater than 0
        time = Mathf.Max(0, time);

        // Enqueue the wait for real time coroutine
        Enqueue(WaitForSecondsRealtime(time));

        return this;
    }

    private static IEnumerator WaitForSecondsRealtime(float time)
    {
        // Wait for the specified amount of real time
        yield return new WaitForSecondsRealtime(time);
    }

    private CoroutineBuilder WaitUntil(Func<bool> condition)
    {
        // Enqueue the wait until coroutine
        Enqueue(WaitUntilCondition(condition));

        return this;
    }

    private static IEnumerator WaitUntilCondition(Func<bool> condition)
    {
        // Wait until the condition is met
        yield return new WaitUntil(condition);
    }

    public CoroutineBuilder WaitWhile(Func<bool> condition)
    {
        // Enqueue the wait while coroutine
        Enqueue(WaitWhileCondition(condition));

        return this;
    }

    private static IEnumerator WaitWhileCondition(Func<bool> condition)
    {
        // Wait while the condition is met
        yield return new WaitWhile(condition);
    }

    private static IEnumerator RunAllCoroutines(Queue<IActionWrapper> actionWrapperQueue)
    {
        while (actionWrapperQueue.Count > 0)
        {
            // Dequeue the next action wrapper
            var wrapper = actionWrapperQueue.Dequeue();

            switch (wrapper)
            {
                // Check if the action wrapper is a coroutine
                // yield the coroutine
                case CoroutineWrapper coroutineWrapper:
                    yield return coroutineWrapper.coroutine;
                    break;

                // Otherwise, check if it's an action
                // Execute the action
                case ActionWrapper actionWrapper:
                    actionWrapper.action();
                    break;
            }
        }
    }

    private static Coroutine RunCoroutine(MonoBehaviour coroutineRunner,
        Queue<IActionWrapper> actionWrapperQueue)
    {
        // Create the coroutine
        var coroutine = coroutineRunner.StartCoroutine(RunAllCoroutines(actionWrapperQueue));

        // Start the coroutine and return the result
        return coroutine;
    }

    public Coroutine Start(MonoBehaviour coroutineRunner)
    {
        // Get the result of the coroutine
        return RunCoroutine(coroutineRunner, _actionWrapperQueue);
    }

    private static IEnumerator ActionToCoroutine(Action action)
    {
        action();
        yield return null;
    }

    private interface IActionWrapper
    {
    }

    private struct CoroutineWrapper : IActionWrapper
    {
        public readonly IEnumerator coroutine;

        public CoroutineWrapper(IEnumerator coroutine)
        {
            this.coroutine = coroutine;
        }
    }

    private struct ActionWrapper : IActionWrapper
    {
        public readonly Action action;

        public ActionWrapper(Action action)
        {
            this.action = action;
        }
    }
}