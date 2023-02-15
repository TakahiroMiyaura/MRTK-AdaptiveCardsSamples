// Copyright (c) 2023 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using Microsoft.MixedReality.AdaptiveCards.Core;
using Microsoft.MixedReality.Toolkit.Data;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;

public class DataConsumerAdaptiveCard : DataConsumerText
{
    /* Used to find all keypaths that influence a specific component to make sure all variable data is updated when any one element changes */
    protected Dictionary<Component, ComponentInformation2> _componentInfoLookup = new();

    [SerializeField]
    [Tooltip("The AdaptiveCard JSON payload.")]
    private TextAsset CardJson;

    /// </inheritdoc/>
    protected override Type[] GetComponentTypes()
    {
        Type[] types = { typeof(AdaptiveCardUI) };
        return types;
    }

    /// </inheritdoc/>
    public override void DataChangeSetEnd(IDataSource dataSource)
    {
        foreach (var componentInfo in _componentInfoLookup.Values) componentInfo.ApplyAllChanges();
    }

    /// </inheritdoc/>
    protected override void ProcessDataChanged(IDataSource dataSource, string resolvedKeyPath, string localKeyPath,
        object value, DataChangeType dataChangeType)
    {
        foreach (var componentInfo in _componentInfoLookup.Values)
            componentInfo.ProcessDataChanged(dataSource, resolvedKeyPath, localKeyPath, value, dataChangeType);
    }

    /// </inheritdoc/>
    protected override void DetachDataConsumer()
    {
        foreach (var ci in _componentInfoLookup.Values) ci.Detach();

        if (!IsFixedHierarchyWillUseCachedValues) _componentInfoLookup.Clear();
    }

    /// </inheritdoc/>
    protected override void AddVariableKeyPathsForComponent(Component component)
    {
        if (_componentInfoLookup.TryGetValue(component, out var componentInfo))
        {
            var dataSource = GetBestDataSource();

            foreach (var varName in componentInfo.TemplateVars)
            {
                var resolvedKeyPath = dataSource.ResolveKeyPath(ResolvedKeyPathPrefix, varName);

                componentInfo.AddKeyPathListener(resolvedKeyPath, varName);

                AddKeyPathListener(varName);
            }
        }
    }

    private void Awake()
    {
        foreach (var managedComponent in FindComponentsToManage())
        {
            var o = new GameObject("_dummy");
            var addComponent = o.AddComponent<TextMeshPro>();
            o.transform.parent = managedComponent.transform;
            addComponent.text = CardJson.text;
            var componentInfo =
                new ComponentInformation2(managedComponent, false, 25, IsFixedHierarchyWillUseCachedValues);
            // Make sure this DataConsumerText ONLY manages Text values which have a template, indicated by {{DataValue}}.
            if (componentInfo.IsTemplatedString)
            {
                _componentInfoLookup[managedComponent] = componentInfo;
                // Clear out the values on the Text components, as we don't want the template visible during the time Databinding is Attach()ing
                componentInfo.SetValue(string.Empty);
            }
        }
    }

    protected class ComponentInformation2 : ComponentInformation
    {
        private readonly AdaptiveCardUI _generateAdaptiveCardUI;
        private readonly TextMeshPro _TextUI;

        public ComponentInformation2(Component theComponent, bool truncate, int maxChars, bool isParentFixedHierarchy) :
            base(tet(theComponent), truncate, maxChars, isParentFixedHierarchy)
        {
            _generateAdaptiveCardUI = theComponent as AdaptiveCardUI;
            _TextUI = theComponent.gameObject.GetNamedChild("_dummy").GetComponent<TextMeshPro>();
            _TextUI.OnPreRenderText += _TextUI_OnPreRenderText;
            ;
        }

        private void _TextUI_OnPreRenderText(TMP_TextInfo obj)
        {
            if (_generateAdaptiveCardUI != null && _TextUI != null && _TextUI.text.Length > 0)
                _generateAdaptiveCardUI.GenerateFromJson(_TextUI.text);
        }

        private static Component tet(Component theComponent)
        {
            return theComponent.GetComponentInChildren<TextMeshPro>();
        }


        /// <summary>
        ///     Utility function to get the string contained within a Component of type TMP_Text or Unity text.
        /// </summary>
        public new string GetValue()
        {
            return base.GetValue();
        }

        /// <summary>
        ///     Utility function to set the string of a Component of type TMP_Text or Unity text.
        /// </summary>
        public new void SetValue(string newValue)
        {
            base.SetValue(newValue);
            if (_generateAdaptiveCardUI != null && _TextUI != null && _TextUI.text.Length > 0)
                _generateAdaptiveCardUI.GenerateFromJson(_TextUI.text);
        }
    } /* End of protected class ComponentInformation */
}