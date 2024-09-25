using System.Collections.Generic;
using UnityEngine;

public class InputUserHandler
{
    /// <summary>
    /// A reference to the script that is using the input handler.
    /// </summary>
    private readonly GameObject _obj;

    /// <summary>
    /// A collection of scripts attached to the game object that use input.
    /// </summary>
    private readonly HashSet<IUsesInput> _inputUsers;

    public InputUserHandler(GameObject obj)
    {
        // Initialize the object
        _obj = obj;

        // Initialize the input users
        _inputUsers = new HashSet<IUsesInput>();
    }

    public void UpdateInputUsers()
    {
        // Check for all scripts that use input on the game object
        var currentInputUsers = new HashSet<IUsesInput>(_obj.GetComponents<IUsesInput>());

        // Initialize the input for each user
        foreach (var user in currentInputUsers)
        {
            // If the collection does not contain the input user
            if (_inputUsers.Contains(user))
                continue;

            // Register the user with the input system
            InputManager.Instance.Register(user);

            // Add the user to the collection
            _inputUsers.Add(user);
        }

        // Remove the input for each user that is no longer on the object
        foreach (var user in _inputUsers)
        {
            // If the collection contains the input user
            if (currentInputUsers.Contains(user))
                continue;

            // unregister the user with the input system
            InputManager.Instance.Unregister(user);

            // Remove the user from the collection
            _inputUsers.Remove(user);
        }
    }

    public void RemoveAll()
    {
        // Unregister all input users
        foreach (var user in _inputUsers)
            InputManager.Instance.Unregister(user);

        // Clear the collection
        _inputUsers.Clear();
    }
}