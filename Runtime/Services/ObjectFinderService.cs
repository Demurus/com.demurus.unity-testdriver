using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityTestDriver.Runtime.Utils;
using Object = UnityEngine.Object;

namespace UnityTestDriver.Runtime.Services
{
    public static class ObjectFinderService
    {
        /// <summary>
        /// Searches all appearances of object(s) with <paramref name="name" /> name on the scene;
        /// Takes into consideration the <paramref name="appearanceOrder" /> appearance order.
        /// </summary>
        /// <param name="name">
        /// The exact name of the object(s) to find.
        /// </param>
        /// <param name="appearanceOrder">
        /// Zero-based index indicating which matching object to return when multiple are found;
        /// Defaults to <c>0</c> (the first match).
        /// </param>
        /// <returns>
        /// The <typeparamref name="T" /> matching the given <paramref name="name" /> at the
        /// specified <paramref name="appearanceOrder" />.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="name" /> is empty.
        /// </exception>
        /// <exception cref="MissingComponentException">
        /// Thrown if no component of type <typeparamref name="T" /> is found anywhere in the scene.
        /// </exception>
        public static T Find<T>(string name, ushort appearanceOrder = 0) where T : Object
        {
            var filteredObjects = FindFilteredObjects<T>(name);

            if (appearanceOrder >= filteredObjects.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(appearanceOrder),
                    $"Your appearance order {appearanceOrder} is out of range. There are only {filteredObjects.Count} objects typed with {typeof(T)} and named with {name}.");
            }

            var foundObject = filteredObjects[appearanceOrder];

            return foundObject;
        }

        /// <summary>
        /// Searches all appearances of object(s) with <paramref name="name" /> name on the scene;
        /// Takes into consideration the <paramref name="uniqueId" /> unique id which should be present on the object.
        /// </summary>
        /// <param name="name">
        /// The exact name of the object(s) to find.
        /// </param>
        /// <param name="uniqueId">
        /// Unique id of the search object. It should be assigned via Editor and should not be repeated elsewhere.
        /// </param>
        /// <returns>
        /// The <typeparamref name="T" /> matching the given <paramref name="name" /> with the
        /// specified <paramref name="uniqueId" />.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="name" /> is empty.
        /// </exception>
        /// <exception cref="MissingComponentException">
        /// Thrown if no component of type <typeparamref name="T" /> is found anywhere in the scene.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if there are multiple objects with <paramref name="uniqueId" /> were found in the scene.
        /// </exception>
        public static T Find<T>(string name, string uniqueId) where T : Object
        {
            var filteredObjects = FindFilteredObjects<T>(name);

            var uniqueIdComponents = new List<T>();

            foreach (var filteredObject in filteredObjects)
            {
                if (filteredObject is not GameObject filteredGameObject)
                {
                    continue;
                }

                var uniqueIdComponent = filteredGameObject.GetComponent<UniqueIdComponent>();

                if (uniqueIdComponent != null && uniqueIdComponent.UniqueId == uniqueId)
                {
                    uniqueIdComponents.Add(filteredObject);
                }
            }

            switch (uniqueIdComponents.Count)
            {
                case 0:
                    throw new MissingComponentException($"No object named {name} with unique ID {uniqueId} was not found! Make sure you add the {typeof(UniqueIdComponent)} to the desired gameobject");
                case > 1:
                    throw new ArgumentOutOfRangeException(nameof(uniqueIdComponents),
                        $"More than 1 object found with unique object ID: {uniqueId}. Please check your objects hierarchy");
            }

            var foundObject = uniqueIdComponents[0];

            return foundObject;
        }

        /// <summary>
        /// Searches upward through the hierarchy from a given parent GameObject, and at each level
        /// searches **all** descendants (including inactive) for GameObjects with the specified name.
        /// </summary>
        /// <param name="name">
        /// The exact name of the GameObject(s) to find.
        /// </param>
        /// <param name="parent">
        /// The GameObject at which to begin the search. The method will inspect this object’s
        /// children, then move to its parent, and so on, until the root is reached.
        /// </param>
        /// <param name="appearanceOrder">
        /// Zero-based index indicating which matching GameObject to return when multiple are found
        /// at the same hierarchy level. Defaults to <c>0</c> (the first match).
        /// </param>
        /// <returns>
        /// The <see cref="GameObject" /> matching the given <paramref name="name" /> at the
        /// specified <paramref name="appearanceOrder" /> in the first hierarchy level where it appears.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="parent" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="name" /> is empty.
        /// </exception>
        /// <exception cref="MissingComponentException">
        /// Thrown if no component of type <see cref="GameObject" /> is found anywhere in the scene.
        /// </exception>
        public static GameObject FindInParents(string name,
            GameObject parent,
            ushort appearanceOrder = 0)
        {
            ArgumentVerifiers.VerifyName(name);
            ArgumentVerifiers.VerifyParent(parent);

            var currentScope = parent;

            while (currentScope != null)
            {
                var matches = currentScope
                    .GetComponentsInChildren<Transform>(true)
                    .Where(transform => transform.gameObject.name == name)
                    .Select(transform => transform.gameObject)
                    .ToList();

                if (matches.Count > appearanceOrder)
                {
                    return matches[appearanceOrder];
                }

                currentScope = currentScope.transform.parent?.gameObject;
            }

            throw new MissingComponentException($"{name} object was not found in the {parent.name} parent!");
        }

        /// <summary>
        /// Searches upward through the hierarchy from a given parent GameObject, and at each level
        /// searches <strong>all</strong> descendants (including inactive) for Components of the specified type
        /// whose GameObject name matches the provided <paramref name="name" />.
        /// </summary>
        /// <typeparam name="T">
        /// The type of Component to search for. Must derive from <c>UnityEngine.Component</c>.
        /// </typeparam>
        /// <param name="name">
        /// The exact name of the GameObject(s) hosting the component to find.
        /// </param>
        /// <param name="parent">
        /// The GameObject at which to begin the search. The method will inspect this object's
        /// children, then move to its parent, and so on, until the root is reached.
        /// </param>
        /// <param name="appearanceOrder">
        /// Zero-based index indicating which matching Component to return when multiple are found
        /// at the same hierarchy level. Defaults to <c>0</c> (the first match).
        /// </param>
        /// <returns>
        /// The <typeparamref name="T" /> instance whose GameObject name matches <paramref name="name" />
        /// at the specified <paramref name="appearanceOrder" /> in the first hierarchy level where it appears.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="parent" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="name" /> is empty.
        /// </exception>
        /// <exception cref="MissingComponentException">
        /// Thrown if no component of type <typeparamref name="T" /> is found anywhere in the scene.
        /// </exception>
        public static T FindInParents<T>(string name,
            GameObject parent,
            ushort appearanceOrder = 0) where T : Component
        {
            ArgumentVerifiers.VerifyName(name);
            ArgumentVerifiers.VerifyParent(parent);

            var currentScope = parent;

            while (currentScope != null)
            {
                var matches = currentScope
                    .GetComponentsInChildren<T>(true)
                    .Where(component => component.gameObject.name == name)
                    .ToList();

                if (matches.Count > appearanceOrder)
                {
                    return matches[appearanceOrder];
                }

                currentScope = currentScope.transform.parent?.gameObject;
            }

            return null;
        }

        private static List<T> FindFilteredObjects<T>(string name) where T : Object
        {
            ArgumentVerifiers.VerifyName(name);

            var allTypedObjects = Object.FindObjectsOfType<T>(true);
            var filteredObjects = allTypedObjects.Where(typedObject => typedObject.name == name).ToList();

            if (filteredObjects.Count == 0)
            {
                throw new MissingComponentException($"{name} Object was not found!");
            }

            return filteredObjects;
        }
    }
}
