#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DrawerLocator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;
    using System.Collections.Generic;
    using Utilities;
    using UnityEngine.Assertions;

    /// <summary>
    /// The custom drawer locator system is still in progress, and is kept internal for this reason. Use at your own risk.
    /// </summary>
    internal class CustomDrawerLocator
    {
        private bool inherited;
        private Func<Type, Type, DrawerInfo> singleDrawerLocator;
        private Func<Type, Type, IEnumerable<DrawerInfo>> multiDrawerLocator;

        protected CustomDrawerLocator()
        {
            this.inherited = true;
        }

        public CustomDrawerLocator(Func<Type, Type, DrawerInfo> singleDrawerLocator)
        {
            Assert.IsNotNull(singleDrawerLocator);
            this.singleDrawerLocator = singleDrawerLocator;
        }

        public CustomDrawerLocator(Func<Type, Type, IEnumerable<DrawerInfo>> multiDrawerLocator)
        {
            Assert.IsNotNull(multiDrawerLocator);
            this.multiDrawerLocator = multiDrawerLocator;
        }

        public virtual IEnumerable<DrawerInfo> GetDrawers(Type valueType, Type attributeType)
        {
            if (this.inherited)
            {
                throw new NotImplementedException("The derived custom drawer locator type '" + this.GetType().GetNiceName() + "' must override GetDrawersForType!");
            }

            if (this.singleDrawerLocator != null)
            {
                var drawerInfo = this.singleDrawerLocator(valueType, attributeType);

                if (drawerInfo != null)
                {
                    yield return drawerInfo;
                }
            }
            else
            {
                foreach (var drawerInfo in this.multiDrawerLocator(valueType, attributeType))
                {
                    if (drawerInfo != null)
                    {
                        yield return drawerInfo;
                    }
                }
            }
        }
    }
}
#endif