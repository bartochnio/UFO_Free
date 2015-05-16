using UnityEngine;
using System;
using System.Collections.Generic;

public delegate void Callback();
public delegate void Callback<T>(T arg);

static public class Messenger
{
    private static Dictionary<UFOEvents, Callback> eventTable = new Dictionary<UFOEvents, Callback>();

    static public void AddListener(UFOEvents eventType, Callback handler)
    {
        if (!eventTable.ContainsKey(eventType))
        {
            eventTable.Add(eventType, null);
        }

        eventTable[eventType] += handler;
    }

    static public void RemoveListener(UFOEvents eventType, Callback handler)
    {
        if (eventTable.ContainsKey(eventType))
        {
            eventTable[eventType] -= handler;

            if (eventTable[eventType] == null)
            {
                eventTable.Remove(eventType);
            }
        }
    }

    static public void Invoke(UFOEvents eventType)
    {
        if (eventTable.ContainsKey(eventType))
        {
            Callback func = eventTable[eventType];

            if (func != null)
            {
                func();
            }
        }
    }
}

static public class Messenger<T>
{
    private static Dictionary<UFOEvents, Callback<T>> eventTable = new Dictionary<UFOEvents, Callback<T>>();

    static public void AddListener(UFOEvents eventType, Callback<T> handler)
    {
        if (!eventTable.ContainsKey(eventType))
        {
            eventTable.Add(eventType, null);
        }

        eventTable[eventType] += handler;
    }

    static public void RemoveListener(UFOEvents eventType, Callback<T> handler)
    {
        if (eventTable.ContainsKey(eventType))
        {
            eventTable[eventType] -= handler;

            if (eventTable[eventType] == null)
            {
                eventTable.Remove(eventType);
            }
        }
    }

    static public void Invoke(UFOEvents eventType, T arg)
    {
        if (eventTable.ContainsKey(eventType))
        {
            Callback<T> func = eventTable[eventType];

            if (func != null)
            {
                func(arg);
            }
        }
    }
}
