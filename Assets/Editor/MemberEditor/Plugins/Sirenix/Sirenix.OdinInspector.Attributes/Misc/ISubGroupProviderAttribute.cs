//-----------------------------------------------------------------------
// <copyright file="ISubGroupProviderAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Internal
{
    using System.Collections.Generic;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public interface ISubGroupProviderAttribute
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        IList<PropertyGroupAttribute> GetSubGroupAttributes();

        /// <summary>
        /// Not yet documented.
        /// </summary>
        string RepathMemberAttribute(PropertyGroupAttribute attr);
    }
}