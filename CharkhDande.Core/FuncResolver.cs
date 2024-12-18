﻿
using System;
using System.Collections.Generic;
public static class IActionRegistry
{
    private static readonly Dictionary<string, IAction> _actions = new();

    public static void Register(string key, IAction action)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null or whitespace.");
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (_actions.ContainsKey(key)) throw new InvalidOperationException($"Action with key '{key}' is already registered.");

        _actions[key] = action;
    }

    public static IAction Resolve(string key)
    {
        if (!_actions.TryGetValue(key, out var action))
            throw new KeyNotFoundException($"No action found for key '{key}'.");

        return action;
    }
}
