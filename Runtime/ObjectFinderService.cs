using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityTestDriver.Runtime.Utils;
using Object = UnityEngine.Object;

namespace UnityTestDriver.Runtime
{
    public static class ObjectFinderService
    {
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

        public static GameObject FindInParents(string name,
            GameObject parent,
            ushort appearanceOrder = 0)
        {
            VerifyName(name);
            VerifyParent(parent);

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

            return null;
        }

        public static T FindInParents<T>(string name,
            GameObject parent,
            ushort appearanceOrder = 0) where T : Component
        {
            VerifyName(name);
            VerifyParent(parent);
            
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
            VerifyName(name);

            var allTypedObjects = Object.FindObjectsOfType<T>(true);
            var filteredObjects = allTypedObjects.Where(typedObject => typedObject.name == name).ToList();

            if (filteredObjects.Count == 0)
            {
                throw new MissingComponentException($"{name} Object was not found!");
            }

            return filteredObjects;
        }

        private static void VerifyName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Object name cannot be empty.");
            }
        }
        
        private static void VerifyParent(Object parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent), "Parent GameObject cannot be null.");
            }
        }
    }
}
